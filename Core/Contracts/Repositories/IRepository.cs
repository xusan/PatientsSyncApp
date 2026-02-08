using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Contracts.Repositories;

public interface IRepository<T> where T : class, IEntity
{
    Task<T> FindById(int id);
    Task<IReadOnlyList<T>> GetAllAsync();
    Task<IReadOnlyList<T>> GetPagedListAsync(int skip, int take);
    Task<int> AddAsync(T entity);
    Task<int> UpdateAsync(T entity);
    Task<int> RemoveAsync(int id);
    Task<int> AddAllAsync(IReadOnlyList<T> entities);
    Task<int> UpdateAllAsync(IReadOnlyList<T> entities);
    Task<int> InsertOrUpdateAsync(T entity);
    Task<int> BatchInsertOrUpdateAsync(IReadOnlyList<T> entities);    
}
