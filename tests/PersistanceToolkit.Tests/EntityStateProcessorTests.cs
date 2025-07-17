using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PersistanceToolkit.Domain;
using PersistanceToolkit.Tests.Initializers;
using PersistanceToolKit.Persistence.Persistance;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace PersistanceToolkit.Tests
{
    public class EntityStateProcessorTests : IDisposable
    {
        private readonly ServiceProvider _serviceProvider;
        private readonly BaseContext _dbContext;
        private readonly EntityStateProcessor _processor;

        public EntityStateProcessorTests()
        {
            _serviceProvider = DependencyInjectionSetup.InitializeServiceProvider();
            _dbContext = _serviceProvider.GetService<BaseContext>();
            _processor = new EntityStateProcessor(_dbContext);
        }

        public void Dispose()
        {
            _serviceProvider?.Dispose();
        }

        /// <summary>
        /// Verifies that new entities (Id = 0) are set to Added state.
        /// </summary>
        [Fact]
        public void SetState_With_New_Entity_Should_Set_Added_State()
        {
            // Arrange
            var entity = new ParentTable { Id = 0, Title = "New Entity" };

            // Act
            _processor.SetState(entity);

            // Assert
            var entry = _dbContext.Entry(entity);
            Assert.Equal(EntityState.Added, entry.State);
        }

        /// <summary>
        /// Verifies that existing entities with changes are set to Modified state.
        /// </summary>
        [Fact]
        public void SetState_With_Existing_Entity_With_Changes_Should_Set_Modified_State()
        {
            // Arrange
            var entity = new ParentTable { Id = 1, Title = "Existing Entity" };
            entity.CaptureLoadTimeSnapshot(); // Capture initial state
            entity.Title = "Modified Entity"; // Make a change

            // Act
            _processor.SetState(entity);

            // Assert
            var entry = _dbContext.Entry(entity);
            Assert.Equal(EntityState.Modified, entry.State);
        }

        /// <summary>
        /// Verifies that existing entities without changes are not set to Modified state.
        /// </summary>
        [Fact]
        public void SetState_With_Existing_Entity_Without_Changes_Should_Not_Set_Modified_State()
        {
            // Arrange
            var entity = new ParentTable { Id = 1, Title = "Existing Entity" };
            entity.CaptureLoadTimeSnapshot(); // Capture initial state
            // No changes made

            // Act
            _processor.SetState(entity);

            // Assert
            var entry = _dbContext.Entry(entity);
            Assert.Equal(EntityState.Detached, entry.State); // Should remain detached
        }



        /// <summary>
        /// Verifies that child entities in navigation properties are processed.
        /// </summary>
        [Fact]
        public void SetState_With_Child_Navigation_Should_Process_Child()
        {
            // Arrange
            var child = new ChildTable { Id = 0, Title = "Child" };
            var parent = new ParentTable { Id = 0, Title = "Parent", IgnoredChild = child };

            // Act
            _processor.SetState(parent);

            // Assert
            var parentEntry = _dbContext.Entry(parent);
            var childEntry = _dbContext.Entry(child);
            
            Assert.Equal(EntityState.Added, parentEntry.State);
            Assert.Equal(EntityState.Added, childEntry.State);
        }

        /// <summary>
        /// Verifies that collections of entities in navigation properties are processed.
        /// </summary>
        [Fact]
        public void SetState_With_Child_Collection_Should_Process_All_Children()
        {
            // Arrange
            var children = new List<ChildTable>
            {
                new ChildTable { Id = 0, Title = "Child1" },
                new ChildTable { Id = 0, Title = "Child2" },
                new ChildTable { Id = 0, Title = "Child3" }
            };
            var parent = new ParentTable { Id = 0, Title = "Parent", ChildTables = children };

            // Act
            _processor.SetState(parent);

            // Assert
            var parentEntry = _dbContext.Entry(parent);
            Assert.Equal(EntityState.Added, parentEntry.State);
            
            Assert.All(children, child =>
            {
                var childEntry = _dbContext.Entry(child);
                Assert.Equal(EntityState.Added, childEntry.State);
            });
        }

        /// <summary>
        /// Verifies that ignored navigation properties are not processed.
        /// </summary>
        [Fact]
        public void SetState_With_Ignored_Navigation_Should_Skip_Processing()
        {
            // Arrange
            var ignoredChild = new ChildTable { Id = 0, Title = "Ignored Child" };
            var parent = new ParentTable { Id = 0, Title = "Parent", IgnoredChild = ignoredChild };
            
            // Act
            _processor.SetState(parent);

            // Assert
            var parentEntry = _dbContext.Entry(parent);
            var ignoredChildEntry = _dbContext.Entry(ignoredChild);
            
            Assert.Equal(EntityState.Added, parentEntry.State);
            Assert.Equal(EntityState.Detached, ignoredChildEntry.State); // Should not be processed
        }

        /// <summary>
        /// Verifies that null navigation properties are handled gracefully.
        /// </summary>
        [Fact]
        public void SetState_With_Null_Navigation_Should_Not_Throw()
        {
            // Arrange
            var parent = new ParentTable { Id = 0, Title = "Parent", IgnoredChild = null };

            // Act & Assert
            var exception = Record.Exception(() => _processor.SetState(parent));
            
            Assert.Null(exception);
            var parentEntry = _dbContext.Entry(parent);
            Assert.Equal(EntityState.Added, parentEntry.State);
        }

        /// <summary>
        /// Verifies that null collections in navigation properties are handled gracefully.
        /// </summary>
        [Fact]
        public void SetState_With_Null_Collection_Navigation_Should_Not_Throw()
        {
            // Arrange
            var parent = new ParentTable { Id = 0, Title = "Parent", ChildTables = null };

            // Act & Assert
            var exception = Record.Exception(() => _processor.SetState(parent));
            
            Assert.Null(exception);
            var parentEntry = _dbContext.Entry(parent);
            Assert.Equal(EntityState.Added, parentEntry.State);
        }

        /// <summary>
        /// Verifies that DetachedAllTrackedEntries clears the ChangeTracker.
        /// </summary>
        [Fact]
        public void DetachedAllTrackedEntries_Should_Clear_ChangeTracker()
        {
            // Arrange
            var entity1 = new ParentTable { Id = 0, Title = "Entity1" };
            var entity2 = new ChildTable { Id = 0, Title = "Entity2" };
            
            _processor.SetState(entity1);
            _processor.SetState(entity2);
            
            // Verify entities are tracked
            Assert.Equal(2, _dbContext.ChangeTracker.Entries().Count());

            // Act
            _processor.DetachedAllTrackedEntries();

            // Assert
            Assert.Empty(_dbContext.ChangeTracker.Entries());
        }

        /// <summary>
        /// Verifies that complex nested structures are processed correctly.
        /// </summary>
        [Fact]
        public void SetState_With_Complex_Nested_Structure_Should_Process_All_Levels()
        {
            // Arrange
            var grandChild1 = new GrandChildTable { Id = 0, Title = "GrandChild1" };
            var grandChild2 = new GrandChildTable { Id = 0, Title = "GrandChild2" };
            
            var child1 = new ChildTable { Id = 0, Title = "Child1", GrandChildTables = new List<GrandChildTable> { grandChild1 } };
            var child2 = new ChildTable { Id = 0, Title = "Child2", GrandChildTables = new List<GrandChildTable> { grandChild2 } };
            
            var parent = new ParentTable { Id = 0, Title = "Parent", IgnoredChild = child1, ChildTables = new List<ChildTable> { child2 } };

            // Act
            _processor.SetState(parent);

            // Assert
            var parentEntry = _dbContext.Entry(parent);
            var child1Entry = _dbContext.Entry(child1);
            var child2Entry = _dbContext.Entry(child2);
            var grandChild1Entry = _dbContext.Entry(grandChild1);
            var grandChild2Entry = _dbContext.Entry(grandChild2);
            
            Assert.Equal(EntityState.Added, parentEntry.State);
            Assert.Equal(EntityState.Added, child1Entry.State);
            Assert.Equal(EntityState.Added, child2Entry.State);
            Assert.Equal(EntityState.Added, grandChild1Entry.State);
            Assert.Equal(EntityState.Added, grandChild2Entry.State);
        }

        /// <summary>
        /// Verifies that mixed navigation types (single and collection) are processed.
        /// </summary>
        [Fact]
        public void SetState_With_Mixed_Navigation_Types_Should_Process_All()
        {
            // Arrange
            var singleChild = new ChildTable { Id = 0, Title = "Single Child" };
            var collectionChildren = new List<ChildTable>
            {
                new ChildTable { Id = 0, Title = "Collection Child 1" },
                new ChildTable { Id = 0, Title = "Collection Child 2" }
            };
            var parent = new ParentTable 
            { 
                Id = 0, 
                Title = "Parent", 
                IgnoredChild = singleChild,
                ChildTables = collectionChildren
            };

            // Act
            _processor.SetState(parent);

            // Assert
            var parentEntry = _dbContext.Entry(parent);
            var singleChildEntry = _dbContext.Entry(singleChild);
            
            Assert.Equal(EntityState.Added, parentEntry.State);
            Assert.Equal(EntityState.Added, singleChildEntry.State);
            
            Assert.All(collectionChildren, child =>
            {
                var childEntry = _dbContext.Entry(child);
                Assert.Equal(EntityState.Added, childEntry.State);
            });
        }

        /// <summary>
        /// Verifies that entities with existing IDs but no changes are handled correctly.
        /// </summary>
        [Fact]
        public void SetState_With_Existing_Entity_No_Changes_Should_Not_Modify_State()
        {
            // Arrange
            var entity = new ParentTable { Id = 1, Title = "Existing Entity" };
            entity.CaptureLoadTimeSnapshot(); // Capture snapshot to indicate no changes

            // Act
            _processor.SetState(entity);

            // Assert
            var entry = _dbContext.Entry(entity);
            Assert.Equal(EntityState.Detached, entry.State); // Should remain detached
        }


    }
} 