using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodingExercisePDE.Services
{
    public class Repository<TModel> : IRepository<TModel> where TModel : class
    {
        protected readonly DbContext DatabaseContext;

        public Repository(DbContext context)
        {
            this.DatabaseContext = context;
        }

        public void Add(TModel entity)
        {
            DatabaseContext.Set<TModel>().Add(entity);
        }

        public TModel Get(int id)
        {
            return DatabaseContext.Set<TModel>().Find(id);
        }

        public List<TModel> GetAll()
        {
            return DatabaseContext.Set<TModel>().ToList();
        }

        public Task<List<TModel>> GetAllAsync()
        {
            return DatabaseContext.Set<TModel>().ToListAsync();
        }

        public async Task<TModel> GetAsync(int id)
        {
            return await DatabaseContext.Set<TModel>().FindAsync(id);
        }

        public void Remove(TModel entity)
        {
            DatabaseContext.Entry(entity).State = EntityState.Deleted;
            DatabaseContext.Set<TModel>().Remove(entity);
        }

        public async Task<bool> SaveChangesAsync()
        {
            return await DatabaseContext.SaveChangesAsync() >= 0;
        }

        public void UpdateGeneric<T>(T entity) where T : class
        {
            DatabaseContext.Entry(entity).State = EntityState.Modified;
        }
    }
}
