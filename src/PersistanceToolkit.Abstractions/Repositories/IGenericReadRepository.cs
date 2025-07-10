using Ardalis.Specification;

namespace PersistanceToolkit.Abstractions.Repositories
{
    public interface IGenericReadRepository<T> where T : class
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
}
