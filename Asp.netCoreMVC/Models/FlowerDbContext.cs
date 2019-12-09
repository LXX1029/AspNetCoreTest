using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Asp.netCoreMVC.Models
{
    public class FlowerDbContext : IdentityDbContext<User>// DbContext
    {
        public FlowerDbContext()
        {

        }
        private readonly IConfiguration Configuration;
        public FlowerDbContext(DbContextOptions<FlowerDbContext> options, IConfiguration configuration)
            : base(options)
        {
            this.Configuration = configuration;
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite(this.Configuration["connectionString"]);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new EmployeeConfiguration());
            base.OnModelCreating(modelBuilder);
        }

        public DbSet<Employee> Employees { get; set; }
    }
}
