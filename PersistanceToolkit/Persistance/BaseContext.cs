using Microsoft.EntityFrameworkCore;
namespace PersistanceToolkit.Persistance
{

    public abstract class BaseContext : DbContext
    {
        private readonly Dictionary<Type, List<string>> _ignoredNavigationsOnUpdate = new();
        protected readonly string _connectionString;
        public BaseContext(DbContextOptions<BaseContext> options) : base(options)
        {
        }
        public BaseContext(string connectionString)
        {
            _connectionString = connectionString;
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                var connectionString = _connectionString;
                optionsBuilder.UseSqlServer(connectionString);
            }
        }
        protected sealed override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            ApplyConfiguration(modelBuilder);
            DefineManualConfiguration(modelBuilder);

            var collected = NavigationIgnoreTracker.CollectAndReset();
            foreach (var kvp in collected)
            {
                foreach (var prop in kvp.Value.Distinct())
                {
                    AddIgnoredNavigationOnUpdate(kvp.Key, prop);
                }
            }
        }
        protected abstract void ApplyConfiguration(ModelBuilder modelBuilder);
        protected abstract void DefineManualConfiguration(ModelBuilder modelBuilder);

        #region Private & Internal Methods
        private void AddIgnoredNavigationOnUpdate(Type entityType, string propertyName)
        {
            if (!_ignoredNavigationsOnUpdate.ContainsKey(entityType))
            {
                _ignoredNavigationsOnUpdate[entityType] = new List<string>();
            }
            _ignoredNavigationsOnUpdate[entityType].Add(propertyName);
        }
        internal bool IsNavigationIgnoredOnUpdate(Type entityType, string propertyName)
        {
            return _ignoredNavigationsOnUpdate.TryGetValue(entityType, out var properties) && properties.Contains(propertyName);
        }
        internal bool IsPropertyIgnored<T>(string propertyName) where T : class
        {
            var entityType = Model.FindEntityType(typeof(T));

            if (entityType == null)
            {
                throw new InvalidOperationException($"Entity type {typeof(T).Name} not found in the model.");
            }

            return !entityType.GetProperties().Any(p => p.Name == propertyName);
        }
        #endregion
    }
}
