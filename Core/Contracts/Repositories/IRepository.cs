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
    Task<int> InsertOrUpdateAsync(T entity);
    Task<int> BatchInsertOrUpdateAsync(IReadOnlyList<T> entities);
    Task<int> RemoveAsync(int id);
}
