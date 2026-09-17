using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using API.Infrastructure.Persistence.Context;
using Domain;
using Domain.Entities;
using Domain.Repositories;
using Google;
using Infrastructure.Specification;
using Microsoft.EntityFrameworkCore;


namespace Infrastructure.Persistence.Repositories
{
    public class GenericRepository<TEntity, TKey> : IGenericRepository<TEntity, TKey>
     where TEntity : BaseEntity<TKey>
    {
        private readonly ApplicationDbContext _dbContext;

        public GenericRepository(ApplicationDbContext dBContext)
        {
            _dbContext = dBContext;
        }

        public async Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _dbContext.Set<TEntity>().ToListAsync(cancellationToken);
        }

        public async Task<TEntity> GetAsync(TKey id, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Set<TEntity>().FindAsync([id], cancellationToken);
        }

        public async Task<TEntity?> GetByIdAsync(TKey id, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Set<TEntity>().FindAsync([id], cancellationToken);
        }

        public async Task<IEnumerable<TEntity>> GetAllAsNoTrackingAsync(CancellationToken cancellationToken = default)
        {
            return await _dbContext.Set<TEntity>().AsNoTracking().ToListAsync(cancellationToken);
        }

        public async Task<TEntity> GetAsNoTrackingAsync(TKey id, CancellationToken cancellationToken = default)
        {
            var entity = await _dbContext.Set<TEntity>().FindAsync([id], cancellationToken);
            if (entity != null)
            {
                _dbContext.Entry(entity).State = EntityState.Detached;
            }
            return entity;
        }

        public async Task AddAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            await _dbContext.Set<TEntity>().AddAsync(entity, cancellationToken);
        }

        public void Update(TEntity entity)
        {
            _dbContext.Set<TEntity>().Update(entity);
        }

        public void Delete(TEntity entity)
        {
            _dbContext.Set<TEntity>().Remove(entity);
        }

        public Task<TEntity?> DeleteAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            _dbContext.Set<TEntity>().Remove(entity);
            return Task.FromResult<TEntity?>(entity);
        }

        public async Task<IEnumerable<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Set<TEntity>().Where(predicate).ToListAsync(cancellationToken);
        }

        public async Task<TEntity?> FindFirstAsync(
        Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Set<TEntity>()
                .FirstOrDefaultAsync(predicate, cancellationToken);
        }

        public async Task<TEntity?> FindOneAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Set<TEntity>()
                .FirstOrDefaultAsync(predicate, cancellationToken);
        }

        public async Task<int> DeleteRangeAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
        {
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            return await _dbContext.Set<TEntity>().Where(predicate).ExecuteDeleteAsync(cancellationToken);
        }

        public async Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
        {
            await _dbContext.Set<TEntity>().AddRangeAsync(entities, cancellationToken);
        }

        public IQueryable<TEntity> GetIQueryable()
        {
            return _dbContext.Set<TEntity>();
        }

        //refactor function specifications pattern
        private IQueryable<TEntity> ApplySpecifications(ISpecification<TEntity, TKey> spec)
        {
            return SpecificationsEvaluator<TEntity, TKey>.GetQuery(_dbContext.Set<TEntity>(), spec);
        }

        public async Task<int> GetCountAsync(ISpecification<TEntity, TKey> spec, CancellationToken cancellationToken = default)
        {
            return await ApplySpecifications(spec).CountAsync(cancellationToken);
        }

        public async Task<IEnumerable<TEntity>> GetAllWithSpecificationAsync(ISpecification<TEntity, TKey> spec, CancellationToken cancellationToken = default)
        {
            return await ApplySpecifications(spec).ToListAsync(cancellationToken);
        }

        public async Task<TEntity> GetWithSpecificationAsync(ISpecification<TEntity, TKey> spec, CancellationToken cancellationToken = default)
        {
            return await ApplySpecifications(spec).FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<IEnumerable<TEntity>> FindWithSpecificationAsync(ISpecification<TEntity, TKey> spec, CancellationToken cancellationToken = default)
        {
            if (spec == null)
                throw new ArgumentNullException(nameof(spec));

            return await ApplySpecifications(spec).ToListAsync(cancellationToken);
        }
    }
}
