using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using WideWorldImporters.API.ApiAuthentication;
using WideWorldImporters.API.DealException;
using WideWorldImporters.API.Filters;
using WideWorldImporters.API.Models;

namespace WideWorldImporters.API.Controllers
{
    [Produces("application/json")]
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class WarehouseController : Controller
    {
        private const string API_SERVERERROR = "{0}发生错误:{1}。";
        private const string API_DEBUGMSG = "{0}方法被调用。";
        private const string API_DELETE_SUCCESS = "删除成功";
        private const string API_ADD_SUCCESS = "新增成功";
        private const string API_UPDATE_SUCCESS = "更新成功";
        private const string API_NORECORD = "检索数据失败";

        private readonly ILogger Logger;
        private readonly IWarehouseRepository Repository;
        private readonly IMemoryCache cache;
        private readonly IOptions<MyTestOptions> myTestOptions;
        private readonly IDistributedCache redisCache;
        private readonly IDataProtectionProvider dataProtectionProvider;
        private readonly IDistributedCache sqlserverCache;
        public WarehouseController(ILogger<WarehouseController> logger, IWarehouseRepository repository, IMemoryCache cache, IOptions<MyTestOptions> options
            , IDistributedCache redisCache, IDataProtectionProvider dataProtectionProvider, IDistributedCache sqlserverCache)
        {
            this.Logger = logger;
            this.Repository = repository;
            this.cache = cache;
            this.myTestOptions = options;
            this.redisCache = redisCache;
            this.sqlserverCache = sqlserverCache;
            this.dataProtectionProvider = dataProtectionProvider;
        }
        [CustomResultFilter]
        [CustomActionFilter]
        [HttpGet]
        public IActionResult GetImageResult()
        {
            string filePath = Directory.GetCurrentDirectory() + @"\MyStaticFiles\images\1.jpg";
            byte[] imageBytes = System.IO.File.ReadAllBytes(filePath);
            if (imageBytes == null || imageBytes.Length == 0) return new NoContentResult();
            var ms = new MemoryStream(imageBytes);
            ms.Seek(0, SeekOrigin.Begin);
            //return new FileStreamResult(ms, "");
            return File(ms, "image/jpeg");
        }


        #region ResponseCache

        [ResponseCache(CacheProfileName = "Default")]
        [HttpGet]
        public string GetResponseCahce()
        {
            string str = Guid.NewGuid().ToString();
            return str;
        }
        #endregion


        #region Nlog
        [HttpGet]
        public void SetLog()
        {
            this.Logger.LogInformation("information........");
            this.Logger.LogError("error", null);
            this.Logger.LogDebug("debug", null);
            this.Logger.LogCritical("Critical", null);

        }
        #endregion

        #region Model绑定参数
        /// <summary>
        /// 从请求服务中绑定请求参数
        /// </summary>
        /// <param name="logger">logger 对象</param>
        /// <returns>object</returns>
        [HttpGet]
        public object GetDataFromServices([FromServices]ILogger<WarehouseController> logger) => $"FromServices-{logger.GetType().FullName}";

        [HttpGet]
        public object GetDataFromHeader([FromHeader]int id) => $"FromHeader-{ id}";

        [HttpGet]
        public object GetDataFromQuery([FromQuery]int id) => $"FromQuery-{ id}";

        [HttpGet]
        public object GetDataFromRoute([FromRoute]int id) => $"FromRoute-{ id}";

        [HttpGet]
        public object GetDataFromBody([FromBody]StockItem item) => item;

        [HttpGet]
        public object GetDataFromBodyText([FromBody]string text) => $"FromBodyText-{text}";

        [HttpPost]
        public object GetDataFromForm([FromForm]StockItem item) => $"FromForm-{ item.StockItemName}";

        [HttpGet]
        public object GetDataFromForm([FromForm]string item) => $"FromForm-{ item}";

        [HttpGet]
        public object GetBadRequest([FromQuery]string param) => $"param:{param}";
        #endregion


        [HttpGet]
        public object GetCurentHttpContext()
        {
            return HttpContext.Request.Headers.Keys.Count;
        }

        [ProducesResponseType(404)]
        [HttpGet]
        public ActionResult GetNotFound()
        {
            return NotFound();
        }


        /// <summary>
        /// Index 页
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public object Index()
        {
            return new { Id = 100, count = 102402 };
        }
        [HttpGet]
        public ActionResult login()
        {
            return new ContentResult() { Content = "login" };
        }


        #region Session操作
        [HttpPost]
        public NoContentResult SetSession()
        {
            HttpContext.Session.Set("sessionId", System.Text.Encoding.UTF8.GetBytes("admin"));
            return NoContent();
        }

        [HttpGet]
        public object GetSession()
        {
            if (HttpContext.Session.TryGetValue("sessionId", out byte[] bytes))
            {
                return Encoding.UTF8.GetString(bytes);
            }
            return "";
        }
        #endregion


        #region IOptions
        [HttpGet]
        public object GetOptionsValue()
        {
            return this.myTestOptions.Value;
        }
        #endregion

        #region SqlCache
        [HttpGet]
        public string GetSqlCache()
        {
            byte[] bytes = this.sqlserverCache.Get("item1");
            if (bytes != null)
                return System.Text.Encoding.UTF8.GetString(this.sqlserverCache.Get("item1"));
            return "Expiration time";
        }

        [HttpGet]
        public string SetSqlCache()
        {
            this.sqlserverCache.SetString("item1", "123456");
            return "success";
        }
        #endregion

        #region MemoryCache

        [HttpGet]
        public async void SetCache()
        {
            StockItem item = new StockItem { StockItemID = 100, StockItemName = "获取缓存-测试" };
            // TimeSpan.FromSeconds(10)  相对于此刻的过期时间
            //this.cache.Set<StockItem>("cache1", item, TimeSpan.FromSeconds(10));
            MemoryCacheEntryOptions options = new MemoryCacheEntryOptions
            {
                AbsoluteExpiration = new DateTimeOffset(DateTime.Now.AddSeconds(5)),  // 绝对过期时间，1分钟之后过期。
                                                                                      //AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(10),         // 设置相对于当前时间之后的时间段
                                                                                      //Priority = CacheItemPriority.NeverRemove, // 永不过期
                                                                                      //Size = 20,
            };

            var number = 0;
            int GetNumber()
            {
                return this.cache.GetOrCreate<int>("cache1", entry =>
                {
                    entry.SetAbsoluteExpiration(TimeSpan.FromSeconds(2));   // 绝对到期时间,相对于Now
                    //entry.SetAbsoluteExpiration(DateTime.Now.AddSeconds(15));   // 绝对到期时间,相对UTC时间
                    //entry.SetSlidingExpiration(TimeSpan.FromSeconds(3)); // 滑动过期时间，在时间内再次请求会延续过期时间。
                    entry.RegisterPostEvictionCallback((key, value, reason, state) =>
                    {
                        Console.WriteLine($"key:{key}   value:{value}    reason:{reason}  state:{state}");
                    }, null);
                    //entry.Value = item;
                    return Interlocked.Increment(ref number);
                });
            }

            for (int i = 0; i < 6; i++)
            {
                Console.WriteLine(GetNumber());
                await Task.Delay(1000);
            }
            /*
             1
             1
             2
             2
             3
             3
             */
        }

        [HttpGet]
        public object GetCache()
        {
            if (this.cache.TryGetValue<StockItem>("cache1", out StockItem item))
            {
                return item;
            }
            return "缓存数据已过期";
        }

        /// <summary>
        /// 自定义缓存过期策略-监控文件.
        /// </summary>
        public async void WatchFile()
        {
            string path = @"D:\测试\WatchFile.txt";
            if (!System.IO.File.Exists(path))
            {
                System.IO.File.Create(path).Dispose();
            }
            var fileProvider = new PhysicalFileProvider(@"D:\测试");
            Task<string> GetFileContnt()
            {
                return this.cache.GetOrCreate("fileToke", entry =>
                {
                    // 文件更新
                    entry.AddExpirationToken(fileProvider.Watch("WatchFile.txt"));
                    entry.RegisterPostEvictionCallback((key, value, reason, state) =>
                    {
                        Console.WriteLine($"key:{key}   value:{((Task<string>)value).Result}    reason:{reason}  state:{state}");
                    }, null);

                    return System.IO.File.ReadAllTextAsync(path);
                });
            };
            Console.WriteLine($"content:{await GetFileContnt()}");
            await System.IO.File.WriteAllTextAsync(path, "11");
            await Task.Delay(500);
            Console.WriteLine($"content:{await GetFileContnt()}");
            await System.IO.File.WriteAllTextAsync(path, "22");
            await Task.Delay(500);
            Console.WriteLine($"content:{await GetFileContnt()}");
            await System.IO.File.WriteAllTextAsync(path, "33");
            await Task.Delay(500);
            Console.WriteLine($"content:{await GetFileContnt()}");

            await Task.Delay(500);
            //System.IO.File.Delete(path);
            this.cache.Remove("fileToke");
            Console.WriteLine($"content:{await GetFileContnt()}");
        }

        CancellationTokenSource cts = null;
        public void CancellationChangedToke()
        {
            var number = 0;
            int getNumber()
            {
                return this.cache.GetOrCreate("CancelToke", entry =>
                {
                    if (cts == null || cts.IsCancellationRequested)
                        cts = new CancellationTokenSource();
                    entry.AddExpirationToken(new CancellationChangeToken(cts.Token));
                    return Interlocked.Increment(ref number);
                });
            }
            Console.WriteLine(getNumber());
            Console.WriteLine(getNumber());
            cts.Cancel();
            Console.WriteLine(getNumber());
            Console.WriteLine(getNumber());
        }

        #endregion

        #region RedisCache
        [HttpGet]
        public byte[] GetRedisValue()
        {
            var entity = new { id = 100, name = "name" };
            redisCache.Set("key3", System.Text.Encoding.UTF8.GetBytes("Key3"));
            return redisCache.Get("key3");
        }
        #endregion

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetStockItems(int pageSize = 10, int pageNumber = 1, int? lastEditedBy = null, int? colorID = null, int? outerPackageID = null, int? supplierID = null, int? unitPackageID = null)
        {
            string obj = this.HttpContext.HttpApiUser()?.Identity?.Name;
            Logger?.LogDebug(API_DEBUGMSG);
            var response = new PagedResponse<StockItem>();
            try
            {
                // 获取数据
                var query = Repository.GetStockItems();
                // 设置分页
                response.PageSize = pageSize;
                response.PageNumber = pageNumber;
                // 获取总条目
                response.ItemsCount = await query.CountAsync().ConfigureAwait(false);
                // 获取指定页数据
                response.Model = await query.Paging(pageSize, pageNumber).ToListAsync().ConfigureAwait(false);
                response.Message = $"第{pageNumber}/{response.PageCount},共{response.ItemsCount}";
                Logger?.LogInformation("数据检索成功!");
            }
            catch (Exception ex)
            {
                response.DidError = true;
                response.ErrorMessage = ex.Message;
                Logger?.LogCritical(API_SERVERERROR, "", ex);
            }
            return response.ToHttpResponse();
        }


        /// <summary>
        /// 获取单个StockItem
        /// </summary>
        /// <param name="id"></param>
        //GET: api/Warehouse/GetStockItemAsync/1
        [HttpGet("GetStockItemAsync")]
        public async Task<IActionResult> GetStockItemAsync(int id)
        {
            Logger?.LogDebug(API_DEBUGMSG, nameof(GetStockItemAsync));
            var response = new SingleResponse<StockItem>();
            try
            {
                response.Model = await Repository.GetStockItemAsync(new StockItem(id)).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                response.DidError = true;
                response.ErrorMessage = ex.Message;
                Logger?.LogCritical(API_SERVERERROR, nameof(GetStockItemAsync), ex);
            }
            return response.ToHttpResponse();
        }

        [HttpPost]
        public async Task<IActionResult> PostStockItemAsync([FromBody]StockItem item)
        {

            var response = new SingleResponse<StockItem>();
            if (item == null) return response.ToHttpResponse();
            Logger?.LogDebug(API_DEBUGMSG, nameof(PostStockItemAsync));

            try
            {
                item.ValidFrom = DateTime.Now;
                item.ValidTo = DateTime.Now;
                var existingEntity = await Repository.GetStockItemsByStockItemNameAsync(item).ConfigureAwait(false);
                if (existingEntity != null)
                {
                    response.DidError = true;
                    response.ErrorMessage = "名称已存在";
                }
                else
                {
                    var entity = item;
                    Repository.Add<StockItem>(item);
                    await Repository.CommitChangesAsync().ConfigureAwait(false);
                    response.Model = entity;
                    response.Message = API_ADD_SUCCESS;
                }
            }
            catch (Exception ex)
            {
                response.DidError = true;
                response.ErrorMessage = ex.Message;
                Logger?.LogCritical(API_SERVERERROR, nameof(GetStockItemAsync), ex);
            }
            return response.ToHttpResponse();
        }


        // PUT  api/v1/Warehouse/StockItem/5
        [HttpPut]
        public async Task<IActionResult> PutStockItemAsync(StockItem item)
        {
            var response = new Response();
            if (item == null) return response.ToHttpResponse();
            Logger?.LogDebug(API_DEBUGMSG, nameof(PutStockItemAsync));
            try
            {
                var entity = await Repository.GetStockItemAsync(item).ConfigureAwait(false);
                if (entity == null)
                {
                    response.DidError = true;
                    response.Message = API_NORECORD;
                }
                else
                {
                    entity.StockItemName = item.StockItemName;
                    entity.TaxRate = item.TaxRate;
                    //entity.ValidFrom = item.ValidFrom;
                    //entity.ValidTo = item.ValidTo;
                    // 更新方法
                    Repository.Update(entity);
                    await Repository.CommitChangesAsync().ConfigureAwait(false);
                    response.Message = API_UPDATE_SUCCESS;
                }
            }
            catch (Exception ex)
            {
                response.DidError = true;
                response.ErrorMessage = ex.Message;
                Logger?.LogCritical(API_SERVERERROR, nameof(GetStockItemAsync), ex);
            }
            return response.ToHttpResponse();
        }
        //[HttpDelete("StockItem/{id}")]
        [HttpPost]
        public async Task<IActionResult> DeleteStockItemAsync(StockItem item)
        {
            Logger?.LogDebug(API_DEBUGMSG, nameof(DeleteStockItemAsync));
            var response = new Response();
            try
            {
                var entity = await Repository.GetStockItemAsync(item);
                if (entity == null)
                {
                    response.DidError = true;
                    response.Message = API_NORECORD;
                }
                else
                {
                    //Repository.Remove(entity);
                    await Repository.CommitChangesAsync();
                    response.Message = API_DELETE_SUCCESS;
                }
            }
            catch (Exception ex)
            {
                response.DidError = true;
                response.ErrorMessage = ex.Message;
                Logger?.LogCritical(API_SERVERERROR, nameof(DeleteStockItemAsync), ex);
            }
            return response.ToHttpResponse();
        }


    }
};