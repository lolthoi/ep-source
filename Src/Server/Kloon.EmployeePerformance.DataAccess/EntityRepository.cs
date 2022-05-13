using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Kloon.EmployeePerformance.DataAccess
{
    public class EntityRepository<T> : IDisposable, IEntityRepository<T>
           where T : class, new()
    {
        protected readonly DbContext _entitiesContext;

        public EntityRepository(DbContext entitiesContext)
        {
            _entitiesContext = entitiesContext ?? throw new ArgumentNullException("entitiesContext");
        }

        private DbSet<T> _entities;

        public DbSet<T> Entities
        {
            get
            {
                if (_entities == null)
                {
                    _entities = _entitiesContext.Set<T>();
                }
                return _entities;
            }
        }

        public virtual IQueryable<T> AllIncluding(
            params Expression<Func<T, object>>[] includeProperties)
        {
            IQueryable<T> query = Query();
            foreach (var includeProperty in includeProperties)
            {
                query = query.Include(includeProperty);
            }

            return query;
        }

        public virtual IQueryable<T> Query()
        {
            return _entitiesContext.Set<T>();
        }

        public virtual IQueryable<T> Query(Expression<Func<T, bool>> predicate)
        {
            return Query().Where(predicate);
        }

        public virtual void Add(T entity)
        {
            Entities.Add(entity);
        }

        public virtual void Edit(T entity)
        {
            var dbEntityEntry = _entitiesContext.Entry(entity);
            dbEntityEntry.State = EntityState.Modified;
        }

        public virtual void Delete(T entity)
        {
            var dbEntityEntry = _entitiesContext.Entry(entity);
            dbEntityEntry.State = EntityState.Deleted;
        }

        public virtual void UpdateRange(List<T> entities)
        {
            if (entities == null)
            {
                throw new ArgumentNullException("entities");
            }

            entities.ForEach(entity =>
            {
                if (_entitiesContext.Entry(entity).State == EntityState.Detached)
                {
                    _entitiesContext.Entry(entity).State = EntityState.Modified;
                }
            });
            _entitiesContext.SaveChanges();
        }

        public virtual void InsertRange(List<T> entities, int batchSize = 100)
        {
            if (entities == null)
            {
                throw new ArgumentNullException("entities");
            }

            if (entities.Any())
            {
                if (batchSize <= 0)
                {
                    entities.ForEach(x => Entities.Add(x));
                    _entitiesContext.SaveChanges();
                }
                else
                {
                    var i = 1;
                    var saved = false;
                    foreach (var entity in entities)
                    {
                        Entities.Add(entity);
                        saved = false;
                        if (i % batchSize == 0)
                        {
                            _entitiesContext.SaveChanges();
                            i = 0;
                            saved = true;
                        }
                        i++;
                    }

                    if (!saved)
                    {
                        _entitiesContext.SaveChanges();
                    }
                }
            }

        }

        public virtual void DeleteRange(List<T> entities)
        {
            if (entities == null)
            {
                throw new ArgumentNullException("entities");
            }

            entities.ForEach(entity =>
            {
                if (_entitiesContext.Entry(entity).State == EntityState.Detached)
                {
                    _entitiesContext.Entry(entity).State = EntityState.Modified;
                }
            });
            _entitiesContext.SaveChanges();
        }

        private bool disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    _entitiesContext.Dispose();
                }
            }
            disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
