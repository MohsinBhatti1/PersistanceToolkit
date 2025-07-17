using Ardalis.Specification;
using Microsoft.Extensions.DependencyInjection;
using PersistanceToolkit.Abstractions.Repositories;
using PersistanceToolkit.Abstractions.Specifications;
using PersistanceToolkit.Tests.Initializers;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using System;

namespace PersistanceToolkit.Tests
{
    public class SpecificationAdvancedTests : IDisposable
    {
        private readonly IAggregateRepository<ParentTable> _parentTableRepository;
        private readonly ServiceProvider _serviceProvider;

        public SpecificationAdvancedTests()
        {
            // Create unique database name for each test class instance
            _serviceProvider = DependencyInjectionSetup.InitializeServiceProvider();
            _parentTableRepository = _serviceProvider.GetService<IAggregateRepository<ParentTable>>();
        }

        public void Dispose()
        {
            _serviceProvider?.Dispose();
        }

        /// <summary>
        /// Verifies that new entities automatically get the current user's tenant ID when saved.
        /// This tests the automatic tenant assignment behavior in the repository.
        /// </summary>
        [Fact]
        public async Task Specification_NewEntities_Should_Get_CurrentUser_TenantId()
        {
            // Arrange
            var entities = new List<ParentTable>();
            for (int i = 0; i < 5; i++)
            {
                entities.Add(new ParentTable { Title = $"Entity-{i}" });
            }

            // Act
            await _parentTableRepository.SaveRange(entities);

            // Assert
            Assert.All(entities, x => Assert.Equal(1, x.TenantId)); // All should have current user's tenant ID
            Assert.All(entities, x => Assert.True(x.Id > 0)); // All should have been assigned IDs
        }

        /// <summary>
        /// Verifies that new entities with nested grandchildren automatically get the current user's tenant ID when saved.
        /// This tests the automatic tenant assignment behavior in the repository for complex hierarchies.
        /// </summary>
        [Fact]
        public async Task Specification_NewEntities_With_Grandchildren_Should_Get_CurrentUser_TenantId()
        {
            // Arrange
            var parent = new ParentTable { Title = "ParentWithGrandchildren" };
            var child = new ChildTable { Title = "Child" };
            var grandChild = new GrandChildTable { Title = "GrandChild" };
            
            child.GrandChildTables = new List<GrandChildTable> { grandChild };
            parent.ChildTables = new List<ChildTable> { child };

            // Act
            await _parentTableRepository.Save(parent);

            // Assert
            Assert.Equal(1, parent.TenantId);
            Assert.Equal(1, child.TenantId);
            Assert.Equal(1, grandChild.TenantId);
            Assert.True(parent.Id > 0);
            Assert.True(child.Id > 0);
            Assert.True(grandChild.Id > 0);
        }



        [Fact]
        public async Task Specification_Should_Exclude_SoftDeleted_Entities_By_Default()
        {
            // Arrange
            var entities = new List<ParentTable>();
            for (int i = 0; i < 5; i++)
            {
                var entity = new ParentTable { Title = $"Active-{i}" };
                entities.Add(entity);
            }
            for (int i = 0; i < 2; i++)
            {
                var entity = new ParentTable { Title = $"Deleted-{i}" };
                entity.MarkAsDeleted(1, System.DateTime.Now);
                entities.Add(entity);
            }
            await _parentTableRepository.SaveRange(entities);

            // Act
            var spec = new ParentTableSpec();
            var result = await _parentTableRepository.ListAsync(spec);

            // Assert
            Assert.DoesNotContain(result, x => x.IsDeleted);
            Assert.Equal(5, result.Count);
        }

        [Fact]
        public async Task Specification_Should_Include_SoftDeleted_Entities_When_Requested()
        {
            // Arrange
            var entities = new List<ParentTable>();
            for (int i = 0; i < 3; i++)
            {
                var entity = new ParentTable { Title = $"Active-{i}" };
                entities.Add(entity);
            }
            for (int i = 0; i < 2; i++)
            {
                var entity = new ParentTable { Title = $"Deleted-{i}" };
                entity.MarkAsDeleted(1, System.DateTime.Now);
                entities.Add(entity);
            }
            await _parentTableRepository.SaveRange(entities);

            // Act
            var spec = new ParentTableSpec { IncludeDeletedRecords = true };
            var result = await _parentTableRepository.ListAsync(spec);

            // Assert
            Assert.Contains(result, x => x.IsDeleted);
            Assert.Equal(5, result.Count);
        }

        [Fact]
        public async Task Specification_PostProcessing_Should_Remove_Deleted_Entries_From_Aggregates()
        {
            // Arrange
            var parent = new ParentTable
            {
                Title = "Parent"
            };
            var activeChild = new ChildTable { Title = "ActiveChild" };
            var deletedChild = new ChildTable { Title = "DeletedChild" };
            deletedChild.MarkAsDeleted(1, System.DateTime.Now);
            parent.ChildTables = new List<ChildTable> { activeChild, deletedChild };
            await _parentTableRepository.Save(parent);

            // Act
            var spec = new ParentTableSpec();
            var result = await _parentTableRepository.ListAsync(spec);
            var loadedParent = result.FirstOrDefault();

            // Assert
            Assert.NotNull(loadedParent);
            Assert.DoesNotContain(loadedParent.ChildTables, c => c.IsDeleted);
        }

        [Fact]
        public async Task Specification_PostProcessing_Should_Capture_LoadTimeSnapshot()
        {
            // Arrange
            var parent = new ParentTable { Title = "SnapshotTest" };
            await _parentTableRepository.Save(parent);

            // Act
            var spec = new ParentTableSpec();
            var result = await _parentTableRepository.ListAsync(spec);
            var loadedParent = result.FirstOrDefault(x => x.Title == "SnapshotTest");

            // Assert
            Assert.NotNull(loadedParent);
            Assert.False(loadedParent.HasChange());
        }

        [Fact]
        public async Task CustomSpecification_Should_Apply_Custom_Filtering()
        {
            // Arrange
            var entities = new List<ParentTable>();
            for (int i = 0; i < 5; i++)
            {
                entities.Add(new ParentTable { Title = i % 2 == 0 ? "Match" : "NoMatch" });
            }
            await _parentTableRepository.SaveRange(entities);

            // Act
            var spec = new CustomTitleSpec("Match");
            var result = await _parentTableRepository.ListAsync(spec);

            // Assert
            Assert.All(result, x => Assert.Equal("Match", x.Title));
        }

        [Fact]
        public async Task CustomSpecification_Should_Include_Navigation_Properties()
        {
            // Arrange
            var parent = new ParentTable
            {
                Title = "ParentWithChildren",
                ChildTables = new List<ChildTable>
                {
                    new ChildTable { Title = "Child1" },
                    new ChildTable { Title = "Child2" }
                }
            };
            await _parentTableRepository.Save(parent);

            // Act
            var spec = new ParentTableSpec();
            var result = await _parentTableRepository.ListAsync(spec);
            var loadedParent = result.FirstOrDefault(x => x.Title == "ParentWithChildren");

            // Assert
            Assert.NotNull(loadedParent);
            Assert.NotNull(loadedParent.ChildTables);
            Assert.Equal(2, loadedParent.ChildTables.Count);
        }

        [Fact]
        public async Task CustomSpecification_Should_Include_Nested_Grandchildren()
        {
            // Arrange
            var parent = new ParentTable
            {
                Title = "ParentWithGrandchildren",
                ChildTables = new List<ChildTable>
                {
                    new ChildTable 
                    { 
                        Title = "Child1",
                        GrandChildTables = new List<GrandChildTable>
                        {
                            new GrandChildTable { Title = "GrandChild1" },
                            new GrandChildTable { Title = "GrandChild2" }
                        }
                    },
                    new ChildTable 
                    { 
                        Title = "Child2",
                        GrandChildTables = new List<GrandChildTable>
                        {
                            new GrandChildTable { Title = "GrandChild3" }
                        }
                    }
                }
            };
            await _parentTableRepository.Save(parent);

            // Act
            var spec = new ParentTableSpec();
            var result = await _parentTableRepository.ListAsync(spec);
            var loadedParent = result.FirstOrDefault(x => x.Title == "ParentWithGrandchildren");

            // Assert
            Assert.NotNull(loadedParent);
            Assert.NotNull(loadedParent.ChildTables);
            Assert.Equal(2, loadedParent.ChildTables.Count);
            
            var child1 = loadedParent.ChildTables.First(c => c.Title == "Child1");
            var child2 = loadedParent.ChildTables.First(c => c.Title == "Child2");
            
            Assert.NotNull(child1.GrandChildTables);
            Assert.Equal(2, child1.GrandChildTables.Count);
            Assert.Contains(child1.GrandChildTables, gc => gc.Title == "GrandChild1");
            Assert.Contains(child1.GrandChildTables, gc => gc.Title == "GrandChild2");
            
            Assert.NotNull(child2.GrandChildTables);
            Assert.Single(child2.GrandChildTables);
            Assert.Equal("GrandChild3", child2.GrandChildTables.First().Title);
        }

        /// <summary>
        /// On the first page (skip == 0), total count is calculated and returned.
        /// This is for UI caching and performance: subsequent pages do not recalculate count.
        /// </summary>
        [Fact]
        public async Task Pagination_FirstPage_Should_Return_TotalCount()
        {
            // Arrange
            var entities = Enumerable.Range(0, 25)
                .Select(i => new ParentTable { Title = $"Parent-{i}" }).ToList();
            await _parentTableRepository.SaveRange(entities);

            // Act
            var spec = new ParentTableSpec();
            var paginated = await _parentTableRepository.PaginatedListAsync(spec, 0, 10);

            // Assert
            Assert.Equal(10, paginated.Items.Count);
            Assert.Equal(25, paginated.TotalCount); // Count is correct on first page
        }

        /// <summary>
        /// On non-first pages (skip > 0), total count is NOT recalculated (should be 0).
        /// This avoids unnecessary count queries for performance.
        /// </summary>
        [Fact]
        public async Task Pagination_NonFirstPage_Should_Not_Return_TotalCount()
        {
            // Arrange
            var entities = Enumerable.Range(0, 25)
                .Select(i => new ParentTable { Title = $"Parent-{i}" }).ToList();
            await _parentTableRepository.SaveRange(entities);

            // Act
            var spec = new ParentTableSpec();
            var paginated = await _parentTableRepository.PaginatedListAsync(spec, 10, 10);

            // Assert
            Assert.Equal(10, paginated.Items.Count);
            Assert.Equal(0, paginated.TotalCount); // Count is not recalculated on non-first page
        }

        /// <summary>
        /// Simulate UI caching: UI should cache total count from first page and use it for subsequent pages.
        /// </summary>
        [Fact]
        public async Task Pagination_UI_Should_Cache_TotalCount_From_FirstPage()
        {
            // Arrange
            var entities = Enumerable.Range(0, 25)
                .Select(i => new ParentTable { Title = $"Parent-{i}" }).ToList();
            await _parentTableRepository.SaveRange(entities);

            // Act
            var spec = new ParentTableSpec();
            var firstPage = await _parentTableRepository.PaginatedListAsync(spec, 0, 10);
            var cachedCount = firstPage.TotalCount;

            var secondPage = await _parentTableRepository.PaginatedListAsync(spec, 10, 10);

            // Assert
            Assert.Equal(25, cachedCount); // UI caches this
            Assert.Equal(0, secondPage.TotalCount); // Not recalculated
            Assert.Equal(10, secondPage.Items.Count);
        }

        /// <summary>
        /// When a new filter is applied, the first page should return the new total count.
        /// </summary>
        [Fact]
        public async Task Pagination_Changing_Filter_Should_Reset_TotalCount_On_FirstPage()
        {
            // Arrange
            var entities = new List<ParentTable>();
            for (int i = 0; i < 10; i++)
                entities.Add(new ParentTable { Title = "A" });
            for (int i = 0; i < 15; i++)
                entities.Add(new ParentTable { Title = "B" });
            await _parentTableRepository.SaveRange(entities);

            // Act
            var specA = new CustomTitleSpec("A");
            var firstPageA = await _parentTableRepository.PaginatedListAsync(specA, 0, 5);

            var specB = new CustomTitleSpec("B");
            var firstPageB = await _parentTableRepository.PaginatedListAsync(specB, 0, 5);

            // Assert
            Assert.Equal(10, firstPageA.TotalCount);
            Assert.Equal(15, firstPageB.TotalCount);
        }

        /// <summary>
        /// TenantId should only be set for new entities (Id == 0), not overwritten for existing entities.
        /// This prevents tenant ID conflicts when updating entities from different tenants.
        /// </summary>
        [Fact]
        public async Task Save_NewEntity_Should_Set_TenantId()
        {
            // Arrange
            var newEntity = new ParentTable { Title = "NewEntity" };
            Assert.Equal(0, newEntity.Id); // Verify it's a new entity
            Assert.Equal(0, newEntity.TenantId); // Verify no tenant set initially

            // Act
            var result = await _parentTableRepository.Save(newEntity);

            // Assert
            Assert.True(result);
            Assert.Equal(1, newEntity.TenantId); // Should be set to current user's tenant
            Assert.True(newEntity.Id > 0); // Should have been assigned an ID
        }

        /// <summary>
        /// When updating an existing entity, TenantId should not be overwritten.
        /// This ensures entities maintain their original tenant assignment.
        /// </summary>
        [Fact]
        public async Task Save_ExistingEntity_Should_Not_Overwrite_TenantId()
        {
            // Arrange
            var originalEntity = new ParentTable { Title = "OriginalEntity" };
            await _parentTableRepository.Save(originalEntity);

            var originalTenantId = originalEntity.TenantId;
            var originalId = originalEntity.Id;

            // Modify the entity
            originalEntity.Title = "UpdatedEntity";

            // Act
            var result = await _parentTableRepository.Save(originalEntity);

            // Assert
            Assert.True(result);
            Assert.Equal(originalTenantId, originalEntity.TenantId); // TenantId should remain unchanged
            Assert.Equal(originalId, originalEntity.Id); // ID should remain unchanged
            Assert.Equal("UpdatedEntity", originalEntity.Title); // Title should be updated
        }

        /// <summary>
        /// When saving a collection, only new entities should have TenantId set.
        /// Existing entities should retain their original TenantId.
        /// </summary>
        [Fact]
        public async Task SaveRange_MixedEntities_Should_Only_Set_TenantId_For_New_Ones()
        {
            // Arrange
            var existingEntity = new ParentTable { Title = "ExistingEntity" };
            await _parentTableRepository.Save(existingEntity);
            var originalTenantId = existingEntity.TenantId;

            var newEntity = new ParentTable { Title = "NewEntity" };
            Assert.Equal(0, newEntity.Id);
            Assert.Equal(0, newEntity.TenantId);

            var entitiesToSave = new List<ParentTable> { existingEntity, newEntity };

            // Act
            var result = await _parentTableRepository.SaveRange(entitiesToSave);

            // Assert
            Assert.True(result);
            Assert.Equal(originalTenantId, existingEntity.TenantId); // Existing entity unchanged
            Assert.Equal(1, newEntity.TenantId); // New entity gets tenant set
            Assert.True(newEntity.Id > 0); // New entity gets ID
        }



        [Fact]
        public async Task Specification_CountAsync_Should_Return_Correct_Count()
        {
            // Arrange
            var entities = new List<ParentTable>();
            for (int i = 0; i < 10; i++)
            {
                entities.Add(new ParentTable { Title = i % 2 == 0 ? "Even" : "Odd" });
            }
            await _parentTableRepository.SaveRange(entities);

            // Act
            var spec = new CustomTitleSpec("Even");
            var count = await _parentTableRepository.CountAsync(spec);

            // Assert
            Assert.Equal(5, count);
        }

        [Fact]
        public async Task Specification_AnyAsync_Should_Return_True_If_Exists()
        {
            // Arrange
            var entity = new ParentTable { Title = "UniqueTitle" };
            await _parentTableRepository.Save(entity);

            // Act
            var spec = new CustomTitleSpec("UniqueTitle");
            var exists = await _parentTableRepository.AnyAsync(spec);

            // Assert
            Assert.True(exists);
        }

        [Fact]
        public async Task Specification_SingleOrDefaultAsync_Should_Return_Null_If_Multiple()
        {
            // Arrange
            var entities = new List<ParentTable>
            {
                new ParentTable { Title = "Duplicate" },
                new ParentTable { Title = "Duplicate" }
            };
            await _parentTableRepository.SaveRange(entities);

            // Act & Assert
            var spec = new CustomTitleSpec("Duplicate");
            await Assert.ThrowsAsync<System.InvalidOperationException>(async () =>
            {
                await _parentTableRepository.SingleOrDefaultAsync(spec);
            });
        }

        // Custom specification for filtering by title
        public class CustomTitleSpec : BaseSpecification<ParentTable>
        {
            public CustomTitleSpec(string title)
            {
                Query.Where(x => x.Title == title);
            }
        }
    }
}