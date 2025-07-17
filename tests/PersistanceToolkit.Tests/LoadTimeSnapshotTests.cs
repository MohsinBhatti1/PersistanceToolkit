using Microsoft.Extensions.DependencyInjection;
using PersistanceToolkit.Abstractions.Repositories;
using PersistanceToolkit.Abstractions.Specifications;
using PersistanceToolkit.Domain;
using PersistanceToolkit.Tests.Initializers;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using System;
using Ardalis.Specification;

namespace PersistanceToolkit.Tests
{
    public class LoadTimeSnapshotTests : IDisposable
    {
        private readonly IAggregateRepository<ParentTable> _parentTableRepository;
        private readonly ServiceProvider _serviceProvider;

        public LoadTimeSnapshotTests()
        {
            _serviceProvider = DependencyInjectionSetup.InitializeServiceProvider();
            _parentTableRepository = _serviceProvider.GetService<IAggregateRepository<ParentTable>>();
        }

        public void Dispose()
        {
            _serviceProvider?.Dispose();
        }



        /// <summary>
        /// Verifies that CaptureLoadTimeSnapshot sets the snapshot and HasChange returns false.
        /// </summary>
        [Fact]
        public async Task CaptureLoadTimeSnapshot_Should_Set_Snapshot_And_Reset_Changes()
        {
            // Arrange
            var entity = new ParentTable { Title = "SnapshotTest" };

            // Act
            entity.CaptureLoadTimeSnapshot();

            // Assert
            Assert.False(entity.HasChange());
        }

        /// <summary>
        /// Verifies that modifying an entity after capturing snapshot detects changes.
        /// </summary>
        [Fact]
        public async Task Modifying_Entity_After_Snapshot_Should_Detect_Changes()
        {
            // Arrange
            var entity = new ParentTable { Title = "OriginalTitle" };
            entity.CaptureLoadTimeSnapshot();

            // Act
            entity.Title = "ModifiedTitle";

            // Assert
            Assert.True(entity.HasChange());
        }

        /// <summary>
        /// Verifies that saving an entity automatically captures the snapshot.
        /// </summary>
        [Fact]
        public async Task Save_Should_Automatically_Capture_Snapshot()
        {
            // Arrange
            var entity = new ParentTable { Title = "AutoSnapshot" };

            // Act
            await _parentTableRepository.Save(entity);

            // Assert
            Assert.False(entity.HasChange());
        }

        /// <summary>
        /// Verifies that loading an entity from database captures the snapshot.
        /// </summary>
        [Fact]
        public async Task Load_From_Database_Should_Capture_Snapshot()
        {
            // Arrange
            var entity = new ParentTable { Title = "LoadSnapshot" };
            await _parentTableRepository.Save(entity);

            // Act
            var spec = new ParentTableSpec();
            var loadedEntity = await _parentTableRepository.FirstOrDefaultAsync(spec);

            // Assert
            Assert.NotNull(loadedEntity);
            Assert.False(loadedEntity.HasChange());
        }

        /// <summary>
        /// Verifies that modifying a loaded entity detects changes.
        /// </summary>
        [Fact]
        public async Task Modifying_Loaded_Entity_Should_Detect_Changes()
        {
            // Arrange
            var entity = new ParentTable { Title = "LoadAndModify" };
            await _parentTableRepository.Save(entity);

            var spec = new ParentTableSpec();
            var loadedEntity = await _parentTableRepository.FirstOrDefaultAsync(spec);

            // Act
            loadedEntity.Title = "ModifiedAfterLoad";

            // Assert
            Assert.True(loadedEntity.HasChange());
        }

        /// <summary>
        /// Verifies that saving a modified entity resets the change detection.
        /// </summary>
        [Fact]
        public async Task Save_Modified_Entity_Should_Reset_Change_Detection()
        {
            // Arrange
            var entity = new ParentTable { Title = "OriginalTitle" };
            await _parentTableRepository.Save(entity);

            var spec = new ParentTableSpec();
            var loadedEntity = await _parentTableRepository.FirstOrDefaultAsync(spec);
            loadedEntity.Title = "ModifiedTitle";

            // Act
            await _parentTableRepository.Save(loadedEntity);

            // Assert
            Assert.False(loadedEntity.HasChange());
        }

        /// <summary>
        /// Verifies that multiple property changes are detected.
        /// </summary>
        [Fact]
        public async Task Multiple_Property_Changes_Should_Be_Detected()
        {
            // Arrange
            var entity = new ParentTable { Title = "MultipleChanges" };
            entity.CaptureLoadTimeSnapshot();

            // Act
            entity.Title = "ChangedTitle";
            entity.ChildTables = new List<ChildTable> { new ChildTable { Title = "Child" } };

            // Assert
            Assert.True(entity.HasChange());
        }

        /// <summary>
        /// Verifies that nested entity changes are detected.
        /// </summary>
        [Fact]
        public async Task Nested_Entity_Changes_Should_Be_Detected()
        {
            // Arrange
            var parent = new ParentTable { Title = "Parent" };
            var child = new ChildTable { Title = "Child" };
            parent.ChildTables = new List<ChildTable> { child };

            await _parentTableRepository.Save(parent);

            var spec = new ParentTableSpec();
            var loadedParent = await _parentTableRepository.FirstOrDefaultAsync(spec);

            // Act
            loadedParent.ChildTables.First().Title = "ModifiedChild";

            // Assert
            Assert.True(loadedParent.HasChange());
        }

        /// <summary>
        /// Verifies that deeply nested grandchild changes are detected.
        /// </summary>
        [Fact]
        public async Task Deeply_Nested_Grandchild_Changes_Should_Be_Detected()
        {
            // Arrange
            var parent = new ParentTable { Title = "Parent" };
            var child = new ChildTable { Title = "Child" };
            var grandChild = new GrandChildTable { Title = "GrandChild" };
            
            child.GrandChildTables = new List<GrandChildTable> { grandChild };
            parent.ChildTables = new List<ChildTable> { child };

            await _parentTableRepository.Save(parent);

            var spec = new ParentTableSpec();
            var loadedParent = await _parentTableRepository.FirstOrDefaultAsync(spec);

            // Act
            loadedParent.ChildTables.First().GrandChildTables.First().Title = "ModifiedGrandChild";

            // Assert
            Assert.True(loadedParent.HasChange());
        }

        /// <summary>
        /// Verifies that snapshot captures the exact state at the time of capture.
        /// </summary>
        [Fact]
        public async Task Snapshot_Should_Capture_Exact_State()
        {
            // Arrange
            var entity = new ParentTable { Title = "ExactState" };
            entity.CaptureLoadTimeSnapshot();

            // Act
            entity.Title = "ModifiedState";
            var hasChangeAfterModify = entity.HasChange();
            
            entity.Title = "ExactState"; // Revert to original state

            // Assert
            Assert.True(hasChangeAfterModify);
            Assert.False(entity.HasChange()); // Should be false after reverting to original state
        }

        /// <summary>
        /// Verifies that snapshot works with complex objects (navigation properties).
        /// </summary>
        [Fact]
        public async Task Snapshot_Should_Work_With_Complex_Objects()
        {
            // Arrange
            var parent = new ParentTable
            {
                Title = "ComplexParent",
                ChildTables = new List<ChildTable>
                {
                    new ChildTable { Title = "Child1" },
                    new ChildTable { Title = "Child2" }
                }
            };

            // Act
            parent.CaptureLoadTimeSnapshot();
            var hasChangeInitially = parent.HasChange();

            parent.ChildTables.Add(new ChildTable { Title = "Child3" });

            // Assert
            Assert.False(hasChangeInitially);
            Assert.True(parent.HasChange());
        }

        /// <summary>
        /// Verifies that snapshot is JSON-based and handles special characters.
        /// </summary>
        [Fact]
        public async Task Snapshot_Should_Handle_Special_Characters()
        {
            // Arrange
            var entity = new ParentTable { Title = "Special\"Chars\n\t\r" };
            entity.CaptureLoadTimeSnapshot();

            // Act
            entity.Title = "Different\"Chars\n\t\r";
            var hasChange = entity.HasChange();

            // Assert
            Assert.True(hasChange);
        }

        /// <summary>
        /// Verifies that snapshot works with null values.
        /// </summary>
        [Fact]
        public async Task Snapshot_Should_Work_With_Null_Values()
        {
            // Arrange
            var entity = new ParentTable { Title = null };
            entity.CaptureLoadTimeSnapshot();

            // Act
            entity.Title = "NotNull";
            var hasChange = entity.HasChange();

            // Assert
            Assert.True(hasChange);
        }

        /// <summary>
        /// Verifies that snapshot works with empty collections.
        /// </summary>
        [Fact]
        public async Task Snapshot_Should_Work_With_Empty_Collections()
        {
            // Arrange
            var entity = new ParentTable
            {
                Title = "EmptyCollection",
                ChildTables = new List<ChildTable>()
            };
            entity.CaptureLoadTimeSnapshot();

            // Act
            entity.ChildTables.Add(new ChildTable { Title = "NewChild" });
            var hasChange = entity.HasChange();

            // Assert
            Assert.True(hasChange);
        }

        /// <summary>
        /// Verifies that snapshot is captured for all entities in a collection.
        /// </summary>
        [Fact]
        public async Task Snapshot_Should_Be_Captured_For_All_Entities_In_Collection()
        {
            // Arrange
            var entities = new List<ParentTable>
            {
                new ParentTable { Title = "Entity1" },
                new ParentTable { Title = "Entity2" },
                new ParentTable { Title = "Entity3" }
            };

            // Act
            await _parentTableRepository.SaveRange(entities);

            // Assert
            foreach (var entity in entities)
            {
                Assert.False(entity.HasChange());
            }
        }

        /// <summary>
        /// Verifies that snapshot is captured during post-processing in specifications.
        /// </summary>
        [Fact]
        public async Task Specification_PostProcessing_Should_Capture_Snapshot()
        {
            // Arrange
            var entity = new ParentTable { Title = "PostProcessing" };
            await _parentTableRepository.Save(entity);

            // Act
            var spec = new ParentTableSpec();
            var result = await _parentTableRepository.ListAsync(spec);
            var loadedEntity = result.FirstOrDefault();

            // Assert
            Assert.NotNull(loadedEntity);
            Assert.False(loadedEntity.HasChange());
        }

        /// <summary>
        /// Verifies that snapshot comparison is case-sensitive.
        /// </summary>
        [Fact]
        public async Task Snapshot_Comparison_Should_Be_Case_Sensitive()
        {
            // Arrange
            var entity = new ParentTable { Title = "CaseSensitive" };
            entity.CaptureLoadTimeSnapshot();

            // Act
            entity.Title = "casesensitive"; // Different case
            var hasChange = entity.HasChange();

            // Assert
            Assert.True(hasChange);
        }



        // Custom specification for testing
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