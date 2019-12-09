using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Asp.netCoreMVC.ApiHandler;
using Asp.netCoreMVC.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Asp.netCoreMVC.Controllers
{
    public class AccountController : Controller
    {

        private SignInManager<User> _signInManager;
        private UserManager<User> _userManager;
        private IConfiguration _configuration;
        private IOptions<UserLoginSettingOption> _userLoginSettingOption;
        public AccountController(SignInManager<User> signInManager, UserManager<User> userManager, IConfiguration configuration, IOptions<UserLoginSettingOption> option)
        {
            this._signInManager = signInManager;
            this._userManager = userManager;
            this._configuration = configuration;
            this._userLoginSettingOption = option;
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View("Signup");
        }

        [HttpPost]
        public async Task<IActionResult> Register([FromForm]Account account)
        {
            if (ModelState.IsValid)
            {
                // 创建User
                var user = new User()
                {
                    UserName = account.UserName,
                };
                var result = await _userManager.CreateAsync(user, account.PassWord);
                if (result.Succeeded)
                {
                    await _signInManager.SignInAsync(user, false);
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    result.Errors.ToList().ForEach(f =>
                    {
                        ModelState.AddModelError("", f.Description);
                    });
                }
            }
            return View("Signup");
        }

        [HttpPost]
        public async Task<IActionResult> Signout()
        {
            await this._signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult Login(string returnUrl = "")
        {
            var account = new Account() { ReturnUrl = returnUrl };
            return View(account);

        }

        [HttpPost]
        public async Task<IActionResult> Login([FromForm]Account account)
        {
            if (ModelState.IsValid)
            {
                var result = await this._signInManager.PasswordSignInAsync(account.UserName, account.PassWord, false, false);
                if (result.Succeeded)
                {
                    if (!string.IsNullOrEmpty(account.ReturnUrl) && Url.IsLocalUrl(account.ReturnUrl))
                    {
                        return Redirect(account.ReturnUrl);
                    }
                    else
                    {
                        return RedirectToAction("Index", "Home");
                    }
                }
            }
            if (string.IsNullOrEmpty(account.ReturnUrl))
            {
                account.ReturnUrl = "";
            }
            ModelState.AddModelError("", "Invalid login attempt");
            return View(account);
        }



        [HttpPost]
        public async Task<object> ApiLogin([FromForm]string UserName, [FromForm]string PassWord)
        {
            // 登陆失败计数
            var maxCount = this._userLoginSettingOption.Value.LoginFailMaxCount;
            var lockOutData = this._userLoginSettingOption.Value.LockoutEndDate;
            try
            {
                if (_userManager == null)
                {
                    return new { code = -1, message = "_userManager is null" };
                }
                if (string.IsNullOrEmpty(UserName))
                {
                    return new { code = -1, message = "user is null" };
                }
                var userEntity = await _userManager.FindByNameAsync(UserName);
                if (userEntity == null)
                {
                    return new { code = -1, message = "user is not exist" };
                }
                var isLockedout = await _userManager.IsLockedOutAsync(userEntity);
                if (isLockedout)
                {
                    return new { code = -1, message = $"the { UserName} is lockout,please try again after {lockOutData} minutes." };
                }

                if (string.IsNullOrEmpty(PassWord))
                {
                    return new { code = -1, message = "pwd is null" };
                }

                //登陆
                if (await _userManager.CheckPasswordAsync(userEntity, PassWord))
                {
                    await _userManager.ResetAccessFailedCountAsync(userEntity);
                    var identity = new ClaimsIdentity(MyHandler.ApiScheme, ClaimsIdentity.DefaultNameClaimType, ClaimsIdentity.DefaultRoleClaimType);

                    var roles = await _userManager.GetRolesAsync(userEntity);
                    if (roles != null && roles.Count > 0)
                    {
                        roles.ToList().ForEach(f =>
                        {
                            identity.AddClaim(new Claim(ClaimsIdentity.DefaultRoleClaimType, f));
                        });
                    }

                    identity.AddClaim(new Claim(ClaimsIdentity.DefaultNameClaimType, userEntity.UserName));

                    var principal = new ClaimsPrincipal(identity);
                    //var properties = new AuthenticationProperties();
                    await this.HttpContext.SignInAsync(MyHandler.ApiScheme, principal);
                    var token = this.HttpContext.HttpApiToken();
                    if (!string.IsNullOrEmpty(token))
                    {
                        return new { code = 0, msg = $"login successful", token };
                    }

                }

                var count = await _userManager.GetAccessFailedCountAsync(userEntity);
                ++count;
                if (count < maxCount)
                {
                    await _userManager.AccessFailedAsync(userEntity);
                    return new { code = -1, message = $"password is not correct,residue time {maxCount - count}" };
                }
                if (count == maxCount)
                {
                    await _userManager.SetLockoutEnabledAsync(userEntity, true);
                    var timeOffset = new DateTimeOffset(DateTime.Now.AddSeconds(lockOutData));
                    await _userManager.SetLockoutEndDateAsync(userEntity, timeOffset);
                    await _userManager.ResetAccessFailedCountAsync(userEntity);
                    return new { code = -1, message = $"the { UserName} is lockout,please try again after {lockOutData} minutes." };
                }
                return new { code = -1, message = $"password is not correct" };
            }
            catch (Exception ex)
            {
                return new { code = -1, message = ex.Message };
            }
        }


        [HttpPost]
        [Authorize(AuthenticationSchemes = MyHandler.ApiScheme)]
        public async Task<object> ApiLogout()
        {
            try
            {
                await this.HttpContext.SignOutAsync(MyHandler.ApiScheme);
                return new { code = 0, msg = "logout successfull" };
            }
            catch (Exception ex)
            {
                return new { code = 0, msg = ex.Message };
            }
        }

        [HttpPost]
        public async Task<object> ApiRegister([FromForm]string user, [FromForm] string pwd, [FromForm]int roletype)
        {
            try
            {
                if (string.IsNullOrEmpty(user))
                {
                    return new { msg = "user is null" };
                }
                if (string.IsNullOrEmpty(pwd))
                {
                    return new { msg = "pwd is null" };
                }
                var userEntity = await _userManager.FindByNameAsync(user);

                if (userEntity != null)
                {
                    return new { msg = $"user {user} is existed" };
                }
                userEntity = new User()
                {
                    UserName = user
                };
                var result = await _userManager.CreateAsync(userEntity, pwd);
                if (result.Succeeded)
                {
                    if (roletype == 0)
                    {
                        await _userManager.AddToRoleAsync(userEntity, Roles.SuperAdmin);
                        return new { msg = $"user {user} created successful,role:{Roles.SuperAdmin}" };
                    }
                    if (roletype == 1)
                    {
                        await _userManager.AddToRoleAsync(userEntity, Roles.Admin);
                        return new { msg = $"user {user} created successful,role:{Roles.Admin}" };
                    }
                    if (roletype == 2)
                    {
                        await _userManager.AddToRoleAsync(userEntity, Roles.User);
                        return new { msg = $"user {user} created successful,role:{Roles.User}" };
                    }
                }
                StringBuilder builder = new StringBuilder();
                if (result.Errors.Any())
                {
                    result.Errors.ToList().ForEach(f =>
                    {
                        builder.AppendLine($"{f.Code} {f.Description}");
                    });
                    return new { msg = $"error {builder.ToString()}" };
                }
                return new { msg = $"user {user} created successful,role:{Roles.User}" };
            }
            catch (Exception ex)
            {
                return new { msg = $"error {ex.Message}" };
            }
        }

    }
}