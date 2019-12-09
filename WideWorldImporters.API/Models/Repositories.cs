using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace WideWorldImporters.API.Models
{
    /// <summary>
    /// 公共操作接口
    /// </summary>
    public interface IRepository : IDisposable
    {
        void Add<TEntity>(TEntity entity) where TEntity : class;

        void Update<TEntity>(TEntity entity) where TEntity : class;

        void Remove<TEntity>(TEntity entity) where TEntity : class;

        int CommitChanges();

        Task<int> CommitChangesAsync();
    }

    /// <summary>
    /// 仓储类，提供虚方法
    /// </summary>
    public class Repository
    {
        protected bool Disposed;
        protected WideWorldImportersDbContext DbContext;
        public Repository(WideWorldImportersDbContext dbContext)
        {
            DbContext = dbContext;
        }

        public void Dispose()
        {
            if (!Disposed)
            {
                DbContext?.Dispose();
                Disposed = true;
            }
        }

        public virtual void Add<TEntity>(TEntity entity) where TEntity : class
        {
            DbContext.Add(entity);
        }

        public virtual void Update<TEntity>(TEntity entity) where TEntity : class
        {
            DbContext.Update(entity);
        }
        public virtual void Remove<TEntity>(TEntity entity) where TEntity : class
        {
            DbContext.Remove(entity);
        }

        public int CommitChanges() => DbContext.SaveChanges();

        public Task<int> CommitChangesAsync() => DbContext.SaveChangesAsync();
    }

    /// <summary>
    /// StockItem 操作接口
    /// </summary>
    public interface IWarehouseRepository : IRepository
    {
        IQueryable<StockItem> GetStockItems(int pageSize = 10, int pageNumber = 1
            , string stockItemName = "");
        Task<StockItem> GetStockItemAsync(StockItem entity);
        Task<StockItem> GetStockItemsByStockItemNameAsync(StockItem entity);
    }


    /// <summary>
    /// StockItem 实现类
    /// </summary>
    public class WarehouseRepository : Repository, IWarehouseRepository
    {
        public WarehouseRepository(WideWorldImportersDbContext dbContext)
            : base(dbContext)
        {

        }



        public IQueryable<StockItem> GetStockItems(int pageSize = 10, int pageNumber = 1, string stockItemName = "")
        {

            var query = DbContext.StockItems.AsQueryable();

            if (!string.IsNullOrEmpty(stockItemName))
                query = query.Where(item => item.StockItemName == stockItemName);
            return query;
        }

        public async Task<StockItem> GetStockItemAsync(StockItem entity)
        {

            return await DbContext.StockItems.FirstOrDefaultAsync(item => item.StockItemID == entity.StockItemID);
        }

        public async Task<StockItem> GetStockItemsByStockItemNameAsync(StockItem entity)
        {
            return await DbContext.StockItems.Where(item => item.StockItemName == entity.StockItemName).SingleOrDefaultAsync();
        }

    }
    /// <summary>
    /// 分页扩展
    /// </summary>
    public static class RepositoryExtensions
    {
        public static IQueryable<TModel> Paging<TModel>(this IQueryable<TModel> query, int pageSize = 0, int pageNumber = 0) where TModel : class
            => pageSize > 0 && pageNumber > 0 ? query.Skip((pageNumber - 1) * pageSize).Take(pageSize) : query;
    }
}
