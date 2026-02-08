using Core.Contracts;
using Core.Contracts.Repositories;
using Microsoft.EntityFrameworkCore;

namespace EfDataStorage.Repositories;

public abstract class Repository<T> : IRepository<T> where T : class, IEntity
{
    private readonly DbContext context;
    private readonly DbSet<T> table;

    public Repository(AppDbContext db)
    {
        context = db;
        table = db.Set<T>();
    }

    public async Task<T> FindById(int id)
    {
        return await table.FindAsync(id);
    }

    public async Task<IReadOnlyList<T>> GetAllAsync()
    {
        var entities = await table
                               .AsNoTracking()
                               .OrderBy(p => p.Id)                           
                               .ToListAsync();

        return entities;
    }

    public async Task<IReadOnlyList<T>> GetPagedListAsync(int skip, int take)
    {
        var entities = await table
                               .AsNoTracking()
                               .OrderBy(p => p.Id)
                               .Skip(skip)
                               .Take(take)
                               .ToListAsync();

        return entities;
    }
    
    public async Task<int> InsertOrUpdateAsync(T entity)
    {        
        var existingEntity = await table.FindAsync(entity.Id);

        if (existingEntity != null)
        {            
            // EF Core will automatically track which properties actually changed
            context.Entry(existingEntity).CurrentValues.SetValues(entity);
        }
        else
        {         
            await table.AddAsync(entity);
        }
        return await context.SaveChangesAsync();
    }

    public async Task<int> BatchInsertOrUpdateAsync(IReadOnlyList<T> entities)
    {
        int totalRowsAffected = 0;
        int batchSize = 100; // Adjust batch size as needed

        for (int i = 0; i < entities.Count; i += batchSize)
        {
            // 1. Grab the current "chunk"
            var batch = entities.Skip(i).Take(batchSize).ToList();
            var incomingIds = batch.Select(p => p.Id).ToList();

            // 2. Fetch only existing records for this specific batch
            var existingEntities = await table
                .Where(p => incomingIds.Contains(p.Id))
                .ToDictionaryAsync(p => p.Id);

            foreach (var incomingEntity in batch)
            {
                if (existingEntities.TryGetValue(incomingEntity.Id, out var existingEntity))
                {
                    // Update: SetValues only updates properties that actually changed
                    context.Entry(existingEntity).CurrentValues.SetValues(incomingEntity);
                }
                else
                {                    
                    table.Add(incomingEntity);
                }
            }

            // 3. Save this batch and clear tracking to keep memory low
            totalRowsAffected += await context.SaveChangesAsync();
            context.ChangeTracker.Clear();
        }

        return totalRowsAffected;
    }

    public async Task<int> RemoveAsync(int id)
    {
        return await table.Where(p => p.Id == id).ExecuteDeleteAsync();
    }   
}
