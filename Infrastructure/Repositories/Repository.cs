using System.Collections.Generic;
using System.Threading.Tasks;
using Core;
using Infrastructure.Context;
using Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class Repository<TEntity> : IRepository<TEntity> where TEntity : class
    {
        private readonly ApplicationContext _applicationContext;
        protected readonly ITechnicalLogger _technicalLogger;
        protected readonly DbSet<TEntity> DbSet;

        public Repository(ApplicationContext context, ITechnicalLogger technicalLogger)
        {
            _applicationContext = context;
            _technicalLogger = technicalLogger;

            DbSet = _applicationContext.Set<TEntity>();
        }

        public async Task<TEntity> GetByIdAsync(int id)
        {
            _technicalLogger.Information("Get entity by ID", new KeyValuePair<string, object>(nameof(id), id), new KeyValuePair<string, object>(nameof(TEntity), typeof(TEntity)));

            var entity = await DbSet.FindAsync(id);
            if (entity != null)
            {
                _applicationContext.Entry(entity).State = EntityState.Detached;
            }

            return entity;
        }

        public async Task<IEnumerable<TEntity>> GetAllAsync()
        {
            _technicalLogger.Information("Get all entities", new KeyValuePair<string, object>(nameof(TEntity), typeof(TEntity)));

            return await DbSet.ToListAsync();
        }

        public async Task AddAsync(TEntity entity)
        {
            _technicalLogger.Information("Add entity", new KeyValuePair<string, object>(nameof(TEntity), typeof(TEntity)));

            await _applicationContext.Set<TEntity>().AddAsync(entity);
            await _applicationContext.SaveChangesAsync();
        }

        public async Task<TEntity> AddWithReturnAsync(TEntity entity)
        {
            _technicalLogger.Information("Add entity with return", new KeyValuePair<string, object>(nameof(TEntity), typeof(TEntity)));

            await _applicationContext.Set<TEntity>().AddAsync(entity);
            await _applicationContext.SaveChangesAsync();

            return entity;
        }

        public Task UpdateAsync(TEntity entity)
        {
            _technicalLogger.Information("Update entity", new KeyValuePair<string, object>(nameof(TEntity), typeof(TEntity)));
            _applicationContext.Entry(entity).State = EntityState.Modified;
            return _applicationContext.SaveChangesAsync();
        }

        public async Task<TEntity> UpdateWithReturnAsync(TEntity entity)
        {
            _technicalLogger.Information("Update entity with return", new KeyValuePair<string, object>(nameof(TEntity), typeof(TEntity)));

            _applicationContext.Entry(entity).State = EntityState.Modified;
            await _applicationContext.SaveChangesAsync();

            return entity;
        }

        public async Task RemoveAsync(int id)
        {
            _technicalLogger.Information("Delete entity by ID", new KeyValuePair<string, object>(nameof(id), id), new KeyValuePair<string, object>(nameof(TEntity), typeof(TEntity)));

            var entity = await DbSet.FindAsync(id);
            if (entity != null)
            {
                _applicationContext.Entry(entity).State = EntityState.Deleted;
                await _applicationContext.SaveChangesAsync();
            }
        }
    }
}