using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace WideWorldImporters.API.Models
{
    /// <summary>
    /// 新增时传递的实体
    /// </summary>
    public class PostStockItemsRequest
    {
        [Key]
        public int? StockItemID { get; set; }

        [Required]
        [StringLength(200)]
        public string StockItemName { get; set; }



        [Required]
        public decimal? TaxRate { get; set; }

        public DateTime? ValidFrom { get; set; }

        public DateTime? ValidTo { get; set; }
    }
    /// <summary>
    /// 修改时传递的实体
    /// </summary>
    public class PutStockItemsRequest
    {
        [Required]
        [StringLength(200)]
        public string StockItemName { get; set; }

        [Required]
        public int? TaxRate { get; set; }

        public DateTime? ValidFrom { get; set; }

        [Required]
        public DateTime? ValidTo { get; set; }
    }

    public static class Extensions
    {
        public static StockItem ToEntity(this PostStockItemsRequest request)
            => new StockItem
            {
                StockItemID = request.StockItemID,
                StockItemName = request.StockItemName,
                TaxRate = request.TaxRate,
                ValidFrom = request.ValidFrom,
                ValidTo = request.ValidTo
            };
    }
}
