using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Asp.netCoreMVC.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Asp.netCoreMVC.Controllers
{
    //[Route("[controller]/[action]")]
    [Authorize]
    public class HomeController : Controller
    {
        private readonly IEmployeeService Service;
        public HomeController(IEmployeeService service)
        {
            this.Service = service;
        }
        [AllowAnonymous]
        [HttpGet]
        public IActionResult Index()
        {
            //var model = new Employee() { ID = 100, Name = "DW" };
            //return new ObjectResult(model);
            //return Content("HomeController--Index");
            //return View("Index");
            //return View(model);
            //return View("Index1");
            var list = this.Service.GetEntities();
            HomeViewModel viewModel = new HomeViewModel();
            viewModel.Employees = list;
            return View(viewModel);
        }

        [HttpGet]
        public IActionResult Detail(int? id)
        {
            if (id.HasValue)
            {
                var model = this.Service.GetById((int)id);
                return View(model);
            }
            return Content("id=null");
        }

        [HttpGet]
        public IActionResult Edit(int? id)
        {
            if (id.HasValue)
            {
                var model = this.Service.GetById(id.Value);
                return View(model);
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Edit(int? id, [FromForm] Employee employee)
        {
            if (employee.ID != 0 && ModelState.IsValid)
            {
                this.Service.UpdateEntity(employee, out Exception ex);
                if (ex == null)
                {
                    return RedirectToAction("Detail", new { id = employee.ID });
                }
            }
            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult Add()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Add([FromForm]Employee employee)
        {
            if (ModelState.IsValid)
            {
                this.Service.AddEntity(employee, out Exception ex);
                if (ex != null)
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    return RedirectToAction("Detail", new { id = employee.ID });
                }
            }
            return View();
        }

        [HttpGet]
        public IActionResult Delete(int? id)
        {
            if (id.HasValue)
            {
                var entity = this.Service.GetById((int)id.Value);
                var result = this.Service.RemoveEntity(entity);
            }
            return RedirectToAction("Index");
        }
    }

    public class HomeViewModel
    {
        public IEnumerable<Employee> Employees { get; set; }
    }
}
