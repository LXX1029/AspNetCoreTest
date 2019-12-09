using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace WideWorldImporters.API.Models
{



    /// <summary>
    /// 数据类
    /// </summary>

    public partial class StockItem
    {
        public StockItem()
        {
        }
        public StockItem(int? stockItemID)
        {
            StockItemID = stockItemID;
        }

        /// <summary>
        /// 标识列
        /// </summary>
        public int? StockItemID { get; set; }
        /// <summary>
        /// 名称
        /// </summary>
        public string StockItemName { get; set; }
        public decimal? TaxRate { get; set; } = 0;
        public DateTime? ValidFrom { get; set; }
        public DateTime? ValidTo { get; set; }
        public decimal? Price { get; set; }
        public string Addresses { get; set; }
        public int Counts { get; set; }
        public string Desc { get; set; }
    }
    /// <summary>
    /// StockItem 类映射
    /// </summary>
    public class StockItemsConfiguration : IEntityTypeConfiguration<StockItem>
    {
        public void Configure(EntityTypeBuilder<StockItem> builder)
        {
            // 配置表
            builder.ToTable("StockItems", "Warehouse");
            builder.HasKey(k => k.StockItemID);  // 设置主键
            //builder.HasKey(k => new { k.StockItemID, k.StockItemName });
            builder.Property(p => p.StockItemID).HasColumnType("int").IsRequired();
            builder.Property(p => p.StockItemName).HasColumnType("nvarchar(200)").IsRequired();
            builder.Property(p => p.TaxRate).HasColumnType("decimal(18, 3)").IsRequired();
            builder.Property(p => p.Price).HasColumnType("decimal(18, 3)");
            builder.Property(p => p.ValidFrom).HasColumnType("datetime2").HasDefaultValue(DateTime.Now).ValueGeneratedOnAddOrUpdate();
            builder.Property(p => p.ValidTo).HasColumnType("datetime2").HasDefaultValue(DateTime.Now).ValueGeneratedOnAddOrUpdate();
            builder.Property(p => p.Addresses).HasColumnType("nvarchar(150)");
            builder.Property(p => p.Counts).HasColumnType("int");
            builder.Property(p => p.Desc).HasColumnType("nvarchar(200)");

        }
    }

    /// <summary>
    /// DbContext
    /// </summary>
    //public class WideWorldImportersDbContext : DbContext
    public class WideWorldImportersDbContext : IdentityDbContext<User>
    {
        private string _dbString;
        public WideWorldImportersDbContext(string dbString)
        {
            _dbString = dbString;
        }

        public WideWorldImportersDbContext(DbContextOptions<WideWorldImportersDbContext> options)
          : base(options)
        {

        }
        /// <summary>
        /// 配置数据库
        /// </summary>
        /// <param name="optionsBuilder"></param>
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            //if (_factory != null)
            //{
            //    optionsBuilder.UseLoggerFactory(_factory);
            //}
            if (!optionsBuilder.IsConfigured)
                optionsBuilder.UseSqlServer(_dbString, (mysql) => mysql.CommandTimeout(60 * 5)).ConfigureWarnings(warnings => warnings.Throw(RelationalEventId.CommandError));
        }


        /// <summary>
        /// 根据Model创建数据库表
        /// </summary>
        /// <param name="modelBuilder"></param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new StockItemsConfiguration());
            //modelBuilder.Entity<StockItem>().toda

            base.OnModelCreating(modelBuilder);
        }
        public DbSet<StockItem> StockItems { get; set; }
    }


}
