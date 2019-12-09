using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Asp.netCoreMVC.Repositorys;

namespace Asp.netCoreMVC.Models
{
    public interface IEmployeeService : IRepository<Employee>
    {

    }


    public class EmployeeService : Repository<Employee>, IEmployeeService
    {
        public EmployeeService(FlowerDbContext dbContext)
            : base(dbContext)
        {

        }
    }
}
