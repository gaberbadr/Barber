using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using API.Infrastructure.Persistence.Context;
using Domain.Entities;
using Domain.Repositories;
using Google;
using Microsoft.EntityFrameworkCore;


namespace Infrastructure.Persistence.Repositories
{
    public class UnitOfWork : IUnitOfWork, IDisposable
    {
        private readonly ApplicationDbContext _storeDbContext;
        private readonly Hashtable _hashtableRepos;
        private bool _disposed = false;

        public UnitOfWork(ApplicationDbContext storeDbContext)
        {
            _storeDbContext = storeDbContext;
            _hashtableRepos = new Hashtable();
        }

        // Commits all pending changes to the database
        public async Task<int> CompleteAsync()
        {
            try
            {
                return await _storeDbContext.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                throw new InvalidOperationException(
                    "A concurrency conflict occurred. The data may have been modified by another process.", ex);
            }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException(
                    "SaveChangesAsync failed. Please check your data and try again.", ex);
            }
            catch (OperationCanceledException ex)
            {
                throw new InvalidOperationException("The operation was cancelled.", ex);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"An unexpected error occurred: {ex.GetType().Name}. {ex.Message}", ex);
            }
        }

        // Returns or creates a generic repository for the specified entity type
        public IGenaricRepository<TEntity, Tkey> Repository<TEntity, Tkey>() where TEntity : BaseEntity<Tkey>
        {
            var type = typeof(TEntity).Name;

            if (!_hashtableRepos.ContainsKey(type))
            {
                var repo = new GenaricRepository<TEntity, Tkey>(_storeDbContext);
                _hashtableRepos.Add(type, repo);
            }

            return _hashtableRepos[type] as IGenaricRepository<TEntity, Tkey>;
        }

        // Disposes the DbContext
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                _storeDbContext?.Dispose();
                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
