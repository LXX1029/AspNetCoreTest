using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Asp.netCoreMVC.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Asp.netCoreMVC.ApiHandler
{

    public static class HttpContextEx
    {
        public static string HttpApiToken(this HttpContext context)
        {
            if (context == null) return null;
            if (context.Items.TryGetValue(MyHandler.ApiTokenKey, out object token))
            {
                return token.ToString();
            }
            return context.Session.Id;
        }
    }

    public class MyHandler : AuthenticationHandler<ApiAuthenticationSchemeOption>, IAuthenticationSignInHandler
    {
        public string myCookie = "myCookie";

        public const string ApiScheme = "Api.Scheme";
        public const string ApiTokenKey = "Api.Token";

        private TicketDataFormat _dataformat;
        private ILogger _logger = null;
        public MyHandler(IOptionsMonitor<ApiAuthenticationSchemeOption> options, IDataProtectionProvider dp, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock)
            : base(options, logger, encoder, clock)
        {
            _logger = logger.CreateLogger("MyHandler");
            if (dp == null) return;
            this._dataformat = new TicketDataFormat(dp.CreateProtector("APIAuthentication"));
        }

        public async Task SignInAsync(ClaimsPrincipal user, AuthenticationProperties properties)
        {
            if (user == null || this.Context == null) return;
            properties ??= new AuthenticationProperties();
            properties.ExpiresUtc = this.Clock.UtcNow + this.Options.ExpireTime; // 过期时间
            properties.IssuedUtc = DateTime.Now; // 发行时间

            var name = user.Identity.Name;
            if (!string.IsNullOrEmpty(name))
            {
                // 创建Ticket
                var ticket = new AuthenticationTicket(user, properties, this.Scheme.Name);
                var token = _dataformat.Protect(ticket);
                this.Context.Items.Add(ApiTokenKey, token);
            }
            await Task.FromResult(true);
        }

        public async Task SignOutAsync(AuthenticationProperties properties)
        {
            await Task.FromResult(0);
        }

        protected override Task HandleForbiddenAsync(AuthenticationProperties properties)
        {
            this.Context.Response.WriteAsync("403 forbidden......");

            return Task.CompletedTask;
            //return base.HandleForbiddenAsync(properties);
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {

            if (this.Context == null) return AuthenticateResult.Fail("context is null");
            // 获取token
            var token = this.Context.Request.Query[Options.TokenQueryKey].FirstOrDefault();
            if (string.IsNullOrEmpty(token))
            {
                await this.Context.Response.WriteAsync("toke is null");
                return AuthenticateResult.NoResult();
            }

            var unprotectToke = _dataformat.Unprotect(token);
            if (unprotectToke == null)
            {
                await this.Context.Response.WriteAsync("toke data is null");
                return AuthenticateResult.NoResult();
            }

            if (unprotectToke.Properties?.ExpiresUtc.HasValue == true)
            {
                if (unprotectToke.Properties.ExpiresUtc < base.Clock.UtcNow)
                {
                    await this.Context.Response.WriteAsync("Ticket Expire");
                    return AuthenticateResult.Fail("Ticket Expire");
                }
            }

            if (unprotectToke.Principal == null)
                return AuthenticateResult.Fail("no principal");
            await Task.FromResult(true);
            return AuthenticateResult.Success(unprotectToke);


        }
    }
    public class ApiAuthenticationSchemeOption : AuthenticationSchemeOptions
    {
        public string TokenQueryKey { get; set; }
        public TimeSpan ExpireTime { get; set; }
        public ApiAuthenticationSchemeOption()
        {
            this.TokenQueryKey = "token";
            this.ExpireTime = TimeSpan.FromMinutes(5);

        }
    }
}
