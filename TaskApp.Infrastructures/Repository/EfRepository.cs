using Microsoft.EntityFrameworkCore;
using TaskApp.Domain.Common;

namespace TaskApp.Infrastructures.Repository
{
    public class EfRepository<T> : IRepository<T>  where T : class, IDomainEntity
    {
        private readonly DbContext _db;
        private readonly DbSet<T> _set;

        public EfRepository(AppDbContext db)
        {
            _db = db;
            _set = db.Set<T>();
        }

        public async Task<T?> FindById(int id)
        {
            return await _set.FindAsync(id);
        }

        public async Task<List<T>> GetList(int count = -1, int skip = 0)
        {
            IQueryable<T> query = _set.AsNoTracking()
                .OrderBy(p => p.Id) // Required for Skip/Take
                .Skip(skip)
                .Take(count);

            return await query.ToListAsync();
        }

        public async Task<int> AddAsync(T entity)
        {
            await _set.AddAsync(entity);
            return await _db.SaveChangesAsync();
        }

        public async Task<int> AddAllAsync(List<T> entities)
        {
            await _set.AddRangeAsync(entities);
            return await _db.SaveChangesAsync();
        }

        public async Task<int> UpdateAsync(T entity)
        {
            // handles detached entities safely
            _set.Attach(entity);
            _db.Entry(entity).State = EntityState.Modified;

            return await _db.SaveChangesAsync();
        }

        public async Task<int> UpdateAllAsync(List<T> entities)
        {
            foreach (var entity in entities)
            {
                _set.Attach(entity);
                _db.Entry(entity).State = EntityState.Modified;
            }

            return await _db.SaveChangesAsync();
        }

        public async Task UpsertBatchAsync(IEnumerable<T> entities)
        {
            // 1. Map Models to Entities internally            
            var incomingIds = entities.Select(p => p.Id).ToList();

            // 2. Fetch existing records to check for Updates vs Inserts
            var existingEntities = await _set
                .Where(p => incomingIds.Contains(p.Id))
                .ToDictionaryAsync(p => p.Id);

            foreach (var incomingEntity in entities)
            {
                if (existingEntities.TryGetValue(incomingEntity.Id, out var existingEntity))
                {
                    // Intelligent update (only changes what is different)
                    _set.Entry(existingEntity).CurrentValues.SetValues(incomingEntity);
                }
                else
                {
                    await _set.AddAsync(incomingEntity);
                }
            }

            await _db.SaveChangesAsync();
        }

        public async Task<int> RemoveAsync(int id)
        {
            var entity = await FindById(id);

            if (entity == null)
                return 0;

            _set.Remove(entity);

            return await _db.SaveChangesAsync();
        }
    }
}
