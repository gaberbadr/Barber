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
    public interface IGenaricRepository<TEntity, TKey> where TEntity : BaseEntity<TKey>
    {
        // ==================== BASIC CRUD OPERATIONS ====================

        Task<IEnumerable<TEntity>> GetAllAsync();
        Task<TEntity> GetAsync(TKey id);
        Task<IEnumerable<TEntity>> GetAllAsNoTrackingAsync();
        Task<TEntity> GetAsNoTrackingAsync(TKey id);
        Task AddAsync(TEntity entity);
        void Update(TEntity entity);
        void Delete(TEntity entity);
        Task<TEntity?> DeleteAsync(TEntity entity);
        Task<TEntity?> GetByIdAsync(TKey id);

        // ==================== QUERY OPERATIONS ====================
        Task<IEnumerable<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate);

        Task<TEntity?> FindFirstAsync(Expression<Func<TEntity, bool>> predicate);
        Task<TEntity?> FindOneAsync(Expression<Func<TEntity, bool>> predicate);
        Task<int> DeleteRangeAsync(Expression<Func<TEntity, bool>> predicate);
        Task AddRangeAsync(IEnumerable<TEntity> entities);

        // ==================== QUERYABLE OPERATIONS ====================
        IQueryable<TEntity> GetIQueryable();

        // ==================== SPECIFICATION PATTERN OPERATIONS ====================


        Task<int> GetCountAsync(ISpecifications<TEntity, TKey> spec);
        Task<IEnumerable<TEntity>> GetAllWithSpecficationAsync(ISpecifications<TEntity, TKey> spec);
        Task<TEntity> GetWithSpecficationAsync(ISpecifications<TEntity, TKey> spec);
        Task<IEnumerable<TEntity>> FindWithSpecificationAsync(ISpecifications<TEntity, TKey> spec);

    }
}
