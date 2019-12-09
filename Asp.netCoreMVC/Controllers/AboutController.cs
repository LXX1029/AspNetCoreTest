using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Asp.netCoreMVC.ApiHandler;
using Asp.netCoreMVC.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Asp.netCoreMVC.Controllers
{
    public class AboutController : Controller
    {

        public AboutController()
        {

        }


        [Authorize(AuthenticationSchemes = MyHandler.ApiScheme, Roles = Roles.Admin)]
        public IActionResult Index()
        {
            return Content("About-index....");
        }

        [Authorize(AuthenticationSchemes = MyHandler.ApiScheme, Roles = Roles.SuperAdmin + "," + Roles.Admin)]
        public IActionResult Index1()
        {
            return Content($"current user:{this.HttpContext.User.Identity.Name}");
        }
    }
}