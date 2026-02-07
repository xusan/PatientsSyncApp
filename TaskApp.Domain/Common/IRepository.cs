using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskApp.Domain.Common
{
    public interface IRepository<T> where T : class, IDomainEntity
    {
        Task<T?> FindById(int id);
        Task<List<T>> GetList(int count = -1, int skip = 0);
        Task<int> AddAsync(T entity);
        Task<int> UpdateAsync(T entity);
        Task<int> RemoveAsync(int id);
        Task<int> AddAllAsync(List<T> entities);
        Task<int> UpdateAllAsync(List<T> entities);
        Task UpsertBatchAsync(IEnumerable<T> entities);
        //Task<int> ClearAsync(string reason);
    }
}
