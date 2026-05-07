using System.Linq.Expressions;

namespace VendorHub.Repository
{
    public interface IGeneralRepository<T> where T : class
    {
        public Task AddAsync(T entity);
        public Task UpdateAsync(T entity);
        public Task<T?> GetByIdAsync(int id);
        public Task DeleteAsync(T entity);
        public Task SaveAsync();
        public Task<T?> GetByAsync(Expression<Func<T, bool>> predicate);
        public IQueryable<T> GetBy(Expression<Func<T, bool>> predicate);
        public IQueryable<T> GetAll();
    }
}
