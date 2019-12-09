using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Asp.netCoreMVC.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Asp.netCoreMVC.Repositorys
{

    public interface IRepository<T> : IDisposable
        where T : class
    {
        T AddEntity(T t, out Exception ex);
        bool RemoveEntity(T t);

        T UpdateEntity(T t, out Exception ex);
        List<T> GetEntities();

        T GetById(int id);
    }



    public class Repository<T> : IRepository<T>
        where T : class
    {
        private bool Disposed;
        protected readonly FlowerDbContext DbContext;
        public Repository() { }
        public Repository(FlowerDbContext context)
        {
            this.DbContext = context;
        }

        public void Dispose()
        {
            if (!Disposed)
            {
                DbContext?.Dispose();
                Disposed = true;
            }
        }


        public virtual T AddEntity(T t, out Exception ex)
        {
            ex = null;
            try
            {
                DbContext.Add(t);
                DbContext.SaveChanges();
            }
            catch (Exception ex1)
            {
                ex = ex1;
            }
            return t;
        }

        public virtual T GetById(int id)
        {
            var model = DbContext.Set<T>().Find(id);
            if (model == null)
                return default;
            return model;
        }

        public virtual List<T> GetEntities()
        {
            return DbContext.Set<T>().ToListAsync().Result;
        }

        public virtual bool RemoveEntity(T t)
        {
            if (DbContext.Entry(t).State != EntityState.Deleted)
                DbContext.Entry(t).State = EntityState.Deleted;
            return DbContext.SaveChangesAsync().Result > 0 ? true : false;
        }

        public virtual T UpdateEntity(T t, out Exception ex)
        {
            ex = null;
            if (t == null)
            {
                ex = new Exception("对象为空");
                return null;
            }
            using var transaction = DbContext.Database.BeginTransaction();
            try
            {
                if (DbContext.Entry(t).State != EntityState.Modified)
                    DbContext.Entry(t).State = EntityState.Modified;
                _ = DbContext.SaveChangesAsync().Result;
                transaction.Commit();
            }
            catch (Exception ex1)
            {
                ex = ex1;
                transaction.Rollback();
            }
            return t;
        }
    }
}
