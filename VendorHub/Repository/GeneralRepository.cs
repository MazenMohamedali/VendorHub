using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Threading.Tasks;
using VendorHub.Models;

namespace VendorHub.Repository
{
    public class GeneralRepository<T> : IGeneralRepository<T> where T : class
    {
        protected readonly VendorHubDbContext _context;
        private readonly DbSet<T> _dbSet;
        public GeneralRepository(VendorHubDbContext context)
        {
            _context = context;
            _dbSet = _context.Set<T>();
        }

        public async Task AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
        }

        public async Task UpdateAsync(T entity)
        {
            _context.Attach(entity);
            _context.Entry(entity).State = EntityState.Modified;
            await SaveAsync();
        }

        public async Task DeleteByIdAsync(int id)
        {
            await _dbSet
                 .Where(e => EF.Property<int>(e, "Id") == id)
                 .ExecuteDeleteAsync();
        }

        public async Task DeleteAsync(T entity)
        {
            if(_context.Entry(entity).State == EntityState.Detached)
                _dbSet.Attach(entity);
            
            _dbSet.Remove(entity);
        }

        public async Task<T?> GetByIdAsync(int id)
        {
            return await _dbSet.FindAsync(id);
        }

        public async Task<T?> GetByAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.FirstOrDefaultAsync(predicate);
        }

        public IQueryable<T> GetBy(Expression<Func<T, bool>> predicate)
        {
            return _dbSet.Where(predicate);
        }

        public IQueryable<T> GetAll()
        {
            return _dbSet;
        }

        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
