using Ardalis.Specification;
using PersistanceToolkit.Domain;

namespace PersistanceToolkit.Abstractions
{
    public interface IGenericReadBaseRepository<T> where T : class
    {
        Task<T?> FirstOrDefaultAsync(ISpecification<T> specification, CancellationToken cancellationToken = default);
        Task<TResult?> FirstOrDefaultAsync<TResult>(ISpecification<T, TResult> specification, CancellationToken cancellationToken = default);
        Task<T?> SingleOrDefaultAsync(ISpecification<T> specification, CancellationToken cancellationToken = default);
        Task<TResult?> SingleOrDefaultAsync<TResult>(ISpecification<T, TResult> specification, CancellationToken cancellationToken = default);
        Task<List<T>> ListAsync(CancellationToken cancellationToken = default);
        Task<List<T>> ListAsync(ISpecification<T> specification, CancellationToken cancellationToken = default);
        Task<List<TResult>> ListAsync<TResult>(ISpecification<T, TResult> specification, CancellationToken cancellationToken = default);
        Task<List<T>> PaginatedListAsync(ISpecification<T> specification, CancellationToken cancellationToken = default);
        Task<List<T>> PaginatedListAsync(ISpecification<T> specification, int skip, int take, CancellationToken cancellationToken = default);
        Task<List<TResult>> PaginatedListAsync<TResult>(ISpecification<T, TResult> specification, CancellationToken cancellationToken = default);
        Task<List<TResult>> PaginatedListAsync<TResult>(ISpecification<T, TResult> specification, int skip, int take, CancellationToken cancellationToken = default);
        Task<int> CountAsync(ISpecification<T> specification, CancellationToken cancellationToken = default);
        Task<int> CountAsync(CancellationToken cancellationToken = default);
        Task<bool> AnyAsync(ISpecification<T> specification, CancellationToken cancellationToken = default);
        Task<bool> AnyAsync(CancellationToken cancellationToken = default);
    }
    public interface IGenericRepository<T> : IGenericReadBaseRepository<T> where T : class
    {
        Task<bool> Save(T entity, CancellationToken cancellationToken = default);
        Task<bool> SaveRange(IEnumerable<T> entities, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(T entity, CancellationToken cancellationToken = default);
        Task<bool> DeleteRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);
        Task<bool> DeleteRangeAsync(ISpecification<T> specification, CancellationToken cancellationToken = default);
    }
    public interface IEntityRepository<T> : IGenericRepository<T> where T : Entity
    {
        Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    }
    public interface IAggregateRepository<T> : IEntityRepository<T> where T : Entity, IAggregateRoot
    {
    }
}
