using Ardalis.Specification;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersistanceToolkit.Contract
{
    public interface IReadBaseRepository<T> where T : BaseEntity
    {
        Task<T?> GetByIdAsync<TId>(TId id, CancellationToken cancellationToken = default) where TId : notnull;
        Task<T?> FirstOrDefaultAsync(ISpecification<T> specification, CancellationToken cancellationToken = default);
        Task<TResult?> FirstOrDefaultAsync<TResult>(ISpecification<T, TResult> specification, CancellationToken cancellationToken = default);
        Task<T?> SingleOrDefaultAsync(ISingleResultSpecification<T> specification, CancellationToken cancellationToken = default);
        Task<TResult?> SingleOrDefaultAsync<TResult>(ISingleResultSpecification<T, TResult> specification, CancellationToken cancellationToken = default);
        Task<List<T>> ListAsync(CancellationToken cancellationToken = default);
        Task<List<T>> ListAsync(ISpecification<T> specification, CancellationToken cancellationToken = default);
        Task<List<TResult>> ListAsync<TResult>(ISpecification<T, TResult> specification, CancellationToken cancellationToken = default);
        Task<int> CountAsync(ISpecification<T> specification, CancellationToken cancellationToken = default);
        Task<int> CountAsync(CancellationToken cancellationToken = default);
        Task<bool> AnyAsync(ISpecification<T> specification, CancellationToken cancellationToken = default);
        Task<bool> AnyAsync(CancellationToken cancellationToken = default);
    }
    public interface IBaseRepository<T> : IReadBaseRepository<T> where T : BaseEntity
    {
    }
}
