using PersistanceToolkit.Domain;

namespace PersistanceToolkit.Abstractions.Repositories
{
    public interface IEntityReadRepository<T> : IGenericReadRepository<T> where T : Entity
    {
        Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    }
}
