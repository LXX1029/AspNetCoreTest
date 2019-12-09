using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WideWorldImporters.API.Models
{
    public interface IUnitOfWork
    {
        bool Commit();
    }

    public class UnitOfWork : IUnitOfWork, IDisposable
    {
        WideWorldImportersDbContext Dbcontext;
        public UnitOfWork(WideWorldImportersDbContext dbContext)
        {
            this.Dbcontext = dbContext;
        }

        public bool Commit()
        {
            return this.Dbcontext.SaveChanges() > 0;
        }

        public void Dispose()
        {
            if (this.Dbcontext != null)
            {
                this.Dbcontext.Dispose();
            }

        }
    }
}
