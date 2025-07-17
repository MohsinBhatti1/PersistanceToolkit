using Microsoft.Extensions.DependencyInjection;
using PersistanceToolkit.Abstractions.Repositories;
using PersistanceToolkit.Abstractions.Specifications;
using PersistanceToolkit.Tests.Initializers;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using System;
using Ardalis.Specification;

namespace PersistanceToolkit.Tests
{
    public class SpecificationPostProcessingTests : IDisposable
    {
        private readonly IAggregateRepository<ParentTable> _parentTableRepository;
        private readonly ServiceProvider _serviceProvider;

        public SpecificationPostProcessingTests()
        {
            _serviceProvider = DependencyInjectionSetup.InitializeServiceProvider();
            _parentTableRepository = _serviceProvider.GetService<IAggregateRepository<ParentTable>>();
        }

        public void Dispose()
        {
            _serviceProvider?.Dispose();
        }

        /// <summary>
        /// Verifies that FirstOrDefaultAsync with specification calls PostProcessingAction.
        /// </summary>
        [Fact]
        public async Task FirstOrDefaultAsync_With_Specification_Should_Call_PostProcessingAction()
        {
            // Arrange
            var entity = new ParentTable { Title = "FirstOrDefaultTest" };
            await _parentTableRepository.Save(entity);

            // Act
            var spec = new ParentTableSpec();
            var result = await _parentTableRepository.FirstOrDefaultAsync(spec);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.HasChange()); // PostProcessingAction should capture snapshot
        }

        /// <summary>
        /// Verifies that SingleOrDefaultAsync with specification calls PostProcessingAction.
        /// </summary>
        [Fact]
        public async Task SingleOrDefaultAsync_With_Specification_Should_Call_PostProcessingAction()
        {
            // Arrange
            var entity = new ParentTable { Title = "SingleOrDefaultTest" };
            await _parentTableRepository.Save(entity);

            // Act
            var spec = new ParentTableSpec();
            var result = await _parentTableRepository.SingleOrDefaultAsync(spec);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.HasChange()); // PostProcessingAction should capture snapshot
        }

        /// <summary>
        /// Verifies that ListAsync with specification calls PostProcessingAction.
        /// </summary>
        [Fact]
        public async Task ListAsync_With_Specification_Should_Call_PostProcessingAction()
        {
            // Arrange
            var entities = new List<ParentTable>
            {
                new ParentTable { Title = "ListTest1" },
                new ParentTable { Title = "ListTest2" },
                new ParentTable { Title = "ListTest3" }
            };
            await _parentTableRepository.SaveRange(entities);

            // Act
            var spec = new ParentTableSpec();
            var result = await _parentTableRepository.ListAsync(spec);

            // Assert
            Assert.Equal(3, result.Count);
            Assert.All(result, entity => Assert.False(entity.HasChange())); // All should have snapshots captured
        }

        /// <summary>
        /// Verifies that PaginatedListAsync with specification calls PostProcessingAction.
        /// </summary>
        [Fact]
        public async Task PaginatedListAsync_With_Specification_Should_Call_PostProcessingAction()
        {
            // Arrange
            var entities = new List<ParentTable>();
            for (int i = 0; i < 10; i++)
            {
                entities.Add(new ParentTable { Title = $"PaginatedTest{i}" });
            }
            await _parentTableRepository.SaveRange(entities);

            // Act
            var spec = new ParentTableSpec();
            var result = await _parentTableRepository.PaginatedListAsync(spec, 0, 5);

            // Assert
            Assert.Equal(5, result.Items.Count);
            Assert.Equal(10, result.TotalCount);
            Assert.All(result.Items, entity => Assert.False(entity.HasChange())); // All should have snapshots captured
        }

        /// <summary>
        /// Verifies that CountAsync with specification calls PostProcessingAction.
        /// </summary>
        [Fact]
        public async Task CountAsync_With_Specification_Should_Call_PostProcessingAction()
        {
            // Arrange
            var entities = new List<ParentTable>
            {
                new ParentTable { Title = "CountTest1" },
                new ParentTable { Title = "CountTest2" },
                new ParentTable { Title = "CountTest3" },
                new ParentTable { Title = "CountTest4" }
            };
            await _parentTableRepository.SaveRange(entities);

            // Act
            var spec = new ParentTableSpec();
            var result = await _parentTableRepository.CountAsync(spec);

            // Assert
            Assert.Equal(4, result);
        }

        /// <summary>
        /// Verifies that AnyAsync with specification calls PostProcessingAction.
        /// </summary>
        [Fact]
        public async Task AnyAsync_With_Specification_Should_Call_PostProcessingAction()
        {
            // Arrange
            var entity = new ParentTable { Title = "AnyTest" };
            await _parentTableRepository.Save(entity);

            // Act
            var spec = new ParentTableSpec();
            var result = await _parentTableRepository.AnyAsync(spec);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Verifies that PostProcessingAction removes deleted entries from aggregates.
        /// </summary>
        [Fact]
        public async Task PostProcessingAction_Should_Remove_Deleted_Entries_From_Aggregates()
        {
            // Arrange
            var parent = new ParentTable { Title = "DeletedEntriesTest" };
            var activeChild = new ChildTable { Title = "ActiveChild" };
            var deletedChild = new ChildTable { Title = "DeletedChild" };
            deletedChild.MarkAsDeleted(1, DateTime.Now);
            
            parent.ChildTables = new List<ChildTable> { activeChild, deletedChild };
            await _parentTableRepository.Save(parent);

            // Act
            var spec = new ParentTableSpec();
            var result = await _parentTableRepository.FirstOrDefaultAsync(spec);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.ChildTables);
            Assert.Single(result.ChildTables); // Only active child should remain
            Assert.Equal("ActiveChild", result.ChildTables.First().Title);
            Assert.DoesNotContain(result.ChildTables, c => c.IsDeleted);
        }

        /// <summary>
        /// Verifies that PostProcessingAction removes deleted entries from nested grandchildren.
        /// </summary>
        [Fact]
        public async Task PostProcessingAction_Should_Remove_Deleted_Grandchildren_From_Aggregates()
        {
            // Arrange
            var parent = new ParentTable { Title = "DeletedGrandchildrenTest" };
            var child = new ChildTable { Title = "Child" };
            var activeGrandChild = new GrandChildTable { Title = "ActiveGrandChild" };
            var deletedGrandChild = new GrandChildTable { Title = "DeletedGrandChild" };
            deletedGrandChild.MarkAsDeleted(1, DateTime.Now);
            
            child.GrandChildTables = new List<GrandChildTable> { activeGrandChild, deletedGrandChild };
            parent.ChildTables = new List<ChildTable> { child };
            await _parentTableRepository.Save(parent);

            // Act
            var spec = new ParentTableSpec();
            var result = await _parentTableRepository.FirstOrDefaultAsync(spec);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.ChildTables);
            Assert.Single(result.ChildTables);
            Assert.NotNull(result.ChildTables.First().GrandChildTables);
            Assert.Single(result.ChildTables.First().GrandChildTables); // Only active grandchild should remain
            Assert.Equal("ActiveGrandChild", result.ChildTables.First().GrandChildTables.First().Title);
            Assert.DoesNotContain(result.ChildTables.First().GrandChildTables, gc => gc.IsDeleted);
        }

        /// <summary>
        /// Verifies that PostProcessingAction captures snapshots for all entities in result.
        /// </summary>
        [Fact]
        public async Task PostProcessingAction_Should_Capture_Snapshots_For_All_Entities()
        {
            // Arrange
            var entities = new List<ParentTable>
            {
                new ParentTable { Title = "SnapshotTest1" },
                new ParentTable { Title = "SnapshotTest2" },
                new ParentTable { Title = "SnapshotTest3" }
            };
            await _parentTableRepository.SaveRange(entities);

            // Act
            var spec = new ParentTableSpec();
            var result = await _parentTableRepository.ListAsync(spec);

            // Assert
            Assert.Equal(3, result.Count);
            Assert.All(result, entity => Assert.False(entity.HasChange())); // All should have snapshots
        }

        /// <summary>
        /// Verifies that PostProcessingAction works with nested deleted entities.
        /// </summary>
        [Fact]
        public async Task PostProcessingAction_Should_Handle_Nested_Deleted_Entities()
        {
            // Arrange
            var parent = new ParentTable { Title = "NestedDeletedTest" };
            var child1 = new ChildTable { Title = "Child1" };
            var child2 = new ChildTable { Title = "Child2" };
            var child3 = new ChildTable { Title = "Child3" };
            
            child2.MarkAsDeleted(1, DateTime.Now);
            
            parent.ChildTables = new List<ChildTable> { child1, child2, child3 };
            await _parentTableRepository.Save(parent);

            // Act
            var spec = new ParentTableSpec();
            var result = await _parentTableRepository.FirstOrDefaultAsync(spec);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.ChildTables);
            Assert.Equal(2, result.ChildTables.Count); // Only active children should remain
            Assert.Contains(result.ChildTables, c => c.Title == "Child1");
            Assert.Contains(result.ChildTables, c => c.Title == "Child3");
            Assert.DoesNotContain(result.ChildTables, c => c.Title == "Child2");
        }

        /// <summary>
        /// Verifies that PostProcessingAction works with complex nested deleted entities including grandchildren.
        /// </summary>
        [Fact]
        public async Task PostProcessingAction_Should_Handle_Complex_Nested_Deleted_Entities()
        {
            // Arrange
            var parent = new ParentTable { Title = "ComplexNestedDeletedTest" };
            
            var child1 = new ChildTable { Title = "Child1" };
            var grandChild1 = new GrandChildTable { Title = "GrandChild1" };
            var grandChild2 = new GrandChildTable { Title = "GrandChild2" };
            grandChild2.MarkAsDeleted(1, DateTime.Now);
            child1.GrandChildTables = new List<GrandChildTable> { grandChild1, grandChild2 };
            
            var child2 = new ChildTable { Title = "Child2" };
            child2.MarkAsDeleted(1, DateTime.Now);
            
            var child3 = new ChildTable { Title = "Child3" };
            var grandChild3 = new GrandChildTable { Title = "GrandChild3" };
            child3.GrandChildTables = new List<GrandChildTable> { grandChild3 };
            
            parent.ChildTables = new List<ChildTable> { child1, child2, child3 };
            await _parentTableRepository.Save(parent);

            // Act
            var spec = new ParentTableSpec();
            var result = await _parentTableRepository.FirstOrDefaultAsync(spec);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.ChildTables);
            Assert.Equal(2, result.ChildTables.Count); // Only active children should remain
            
            var activeChild1 = result.ChildTables.First(c => c.Title == "Child1");
            var activeChild3 = result.ChildTables.First(c => c.Title == "Child3");
            
            Assert.Single(activeChild1.GrandChildTables); // Only active grandchild should remain
            Assert.Equal("GrandChild1", activeChild1.GrandChildTables.First().Title);
            
            Assert.Single(activeChild3.GrandChildTables);
            Assert.Equal("GrandChild3", activeChild3.GrandChildTables.First().Title);
            
            Assert.DoesNotContain(result.ChildTables, c => c.Title == "Child2");
        }

        /// <summary>
        /// Verifies that PostProcessingAction works when IncludeDeletedRecords is true.
        /// </summary>
        [Fact]
        public async Task PostProcessingAction_Should_Include_Deleted_Records_When_Requested()
        {
            // Arrange
            var parent = new ParentTable { Title = "IncludeDeletedTest" };
            var activeChild = new ChildTable { Title = "ActiveChild" };
            var deletedChild = new ChildTable { Title = "DeletedChild" };
            deletedChild.MarkAsDeleted(1, DateTime.Now);
            
            parent.ChildTables = new List<ChildTable> { activeChild, deletedChild };
            await _parentTableRepository.Save(parent);

            // Act
            var spec = new ParentTableSpec { IncludeDeletedRecords = true };
            var result = await _parentTableRepository.FirstOrDefaultAsync(spec);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.ChildTables);
            Assert.Equal(2, result.ChildTables.Count); // Both should remain
            Assert.Contains(result.ChildTables, c => c.Title == "ActiveChild");
            Assert.Contains(result.ChildTables, c => c.Title == "DeletedChild" && c.IsDeleted);
        }

        // Custom specifications for testing
        public class ParentTableSpec : BaseSpecification<ParentTable>
        {
            public ParentTableSpec()
            {
                Query.Include(c => c.ChildTables)
                     .ThenInclude(child => child.GrandChildTables);
            }
        }
    }
} 