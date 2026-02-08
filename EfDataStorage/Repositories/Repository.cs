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

    public async Task<int> AddAsync(T entity)
    {
        table.Add(entity);
        return await context.SaveChangesAsync();
    }

    public async Task<int> AddAllAsync(IReadOnlyList<T> entities)
    {
        table.AddRange(entities);
        return await context.SaveChangesAsync();
    }

    public async Task<int> UpdateAsync(T entity)
    {
        // handles detached entities safely
        table.Attach(entity);
        context.Entry(entity).State = EntityState.Modified;

        return await context.SaveChangesAsync();
    }

    public async Task<int> UpdateAllAsync(IReadOnlyList<T> entities)
    {
        foreach (var entity in entities)
        {
            table.Attach(entity);
            context.Entry(entity).State = EntityState.Modified;
        }

        return await context.SaveChangesAsync();
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
        var incomingIds = entities.Select(p => p.Id).ToList();
        
        var existingEntities = await table
            .Where(p => incomingIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id);

        foreach (var incomingEntity in entities)
        {
            if (existingEntities.TryGetValue(incomingEntity.Id, out var existingEntity))
            {
                // Intelligent update (only changes what is different)
                table.Entry(existingEntity).CurrentValues.SetValues(incomingEntity);
            }
            else
            {
                await table.AddAsync(incomingEntity);
            }
        }

        return await context.SaveChangesAsync();
    }

    public async Task<int> RemoveAsync(int id)
    {
        return await table.Where(p => p.Id == id).ExecuteDeleteAsync();
    }   
}
