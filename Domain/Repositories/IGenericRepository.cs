using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Domain.Entities;

namespace Domain.Repositories
{
    public interface IGenericRepository<TEntity, TKey> where TEntity : BaseEntity<TKey>
    {
        // ==================== BASIC CRUD OPERATIONS ====================

        Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<TEntity> GetAsync(TKey id, CancellationToken cancellationToken = default);
        Task<IEnumerable<TEntity>> GetAllAsNoTrackingAsync(CancellationToken cancellationToken = default);
        Task<TEntity> GetAsNoTrackingAsync(TKey id, CancellationToken cancellationToken = default);
        Task AddAsync(TEntity entity, CancellationToken cancellationToken = default);
        void Update(TEntity entity);
        void Delete(TEntity entity);
        Task<TEntity?> DeleteAsync(TEntity entity, CancellationToken cancellationToken = default);
        Task<TEntity?> GetByIdAsync(TKey id, CancellationToken cancellationToken = default);

        // ==================== QUERY OPERATIONS ====================
        Task<IEnumerable<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);

        Task<TEntity?> FindFirstAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);
        Task<TEntity?> FindOneAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);
        Task<int> DeleteRangeAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);
        Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);

        // ==================== QUERYABLE OPERATIONS ====================
        IQueryable<TEntity> GetIQueryable();

        // ==================== SPECIFICATION PATTERN OPERATIONS ====================


        Task<int> GetCountAsync(ISpecification<TEntity, TKey> spec, CancellationToken cancellationToken = default);
        Task<IEnumerable<TEntity>> GetAllWithSpecificationAsync(ISpecification<TEntity, TKey> spec, CancellationToken cancellationToken = default);
        Task<TEntity> GetWithSpecificationAsync(ISpecification<TEntity, TKey> spec, CancellationToken cancellationToken = default);
        Task<IEnumerable<TEntity>> FindWithSpecificationAsync(ISpecification<TEntity, TKey> spec, CancellationToken cancellationToken = default);

    }
}
