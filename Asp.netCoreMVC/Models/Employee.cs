using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Asp.netCoreMVC.Models
{
    public class Employee
    {

        public int ID { get; set; }
        [Required, MaxLength(10)]
        public string Name { get; set; } = string.Empty;
        public int Age { get; set; }
    }

    public class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
    {
        public void Configure(EntityTypeBuilder<Employee> builder)
        {
            builder.ToTable("Employees");
            builder.HasKey(h => h.ID);
            builder.Property(m => m.Age).HasColumnType("int");
        }
    }

}
