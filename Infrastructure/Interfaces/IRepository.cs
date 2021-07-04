using System.Collections.Generic;
using System.Threading.Tasks;

namespace Infrastructure.Interfaces
{
    public interface IRepository<TEntity> where TEntity : class
    {
        Task<TEntity> GetByIdAsync(int id);

        Task<IEnumerable<TEntity>> GetAllAsync();

        Task AddAsync(TEntity entity);

        Task<TEntity> AddWithReturnAsync(TEntity entity);

        Task UpdateAsync(TEntity entity);

        Task<TEntity> UpdateWithReturnAsync(TEntity entity);

        Task RemoveAsync(int id);
    }
}
