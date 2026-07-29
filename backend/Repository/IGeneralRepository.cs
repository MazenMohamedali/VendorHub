using Microsoft.EntityFrameworkCore.Storage;
using System.Linq.Expressions;

namespace VendorHub.Repository
{
    public interface IGeneralRepository<T> where T : class
    {
        Task AddAsync(T entity, CancellationToken cancellationToken = default);
        Task AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);

        void Update(T entity);
        void UpdateRange(IEnumerable<T> entities);

        void Delete(T entity);
        Task DeleteByIdAsync(int id, CancellationToken cancellationToken = default);
        void DeleteRange(IEnumerable<T> entities);

        IQueryable<T> GetAll();
        IQueryable<T> GetAllAsNoTracking();
        IQueryable<T> GetBy(Expression<Func<T, bool>> predicate);
        IQueryable<T> GetByAsNoTracking(Expression<Func<T, bool>> predicate);
        IQueryable<T> GetAllIgnoreFilters();

        Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<T?> GetByAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
        Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);

        Task SaveAsync(CancellationToken cancellationToken = default);

        Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);
    }
}
