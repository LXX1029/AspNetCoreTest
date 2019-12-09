using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace Asp.netCoreMVC.Models
{

    public class Account
    {
        [Required, MaxLength(20)]
        public string UserName { get; set; }
        [Required, MaxLength(10)]
        public string PassWord { get; set; }

        [Display(Description = "记住")]
        public bool Remember { get; set; }
        public string ReturnUrl { get; set; }

    }

    public class User : IdentityUser
    {
        public DateTime? CreatedTime { get; set; }
        public User()
        {

        }
    }

    public class UserService
    {

    }

    public class Roles
    {
        public const string User = "user";
        public const string Admin = "admin";
        public const string SuperAdmin = "superadmin";
    }

    public class UserLoginSettingOption
    {
        public UserLoginSettingOption()
        {
        }

        public int LoginFailMaxCount { get; set; }
        public int LockoutEndDate { get; set; } = 1; // 单位：分钟

        public int ExpireDate { get; set; } = 10;//分钟
    }
}