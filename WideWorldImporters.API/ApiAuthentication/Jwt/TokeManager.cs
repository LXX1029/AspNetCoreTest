using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;

namespace WideWorldImporters.API.ApiAuthentication.Jwt
{
    public static class HttpContextEx
    {
        public static string RetrieveJwtToke(this HttpContext context, string userName)
        {
            if (context == null) return string.Empty;
            string token;
            if (context.Items.TryGetValue(TokeManager.JwtToken, out object token1))
            {
                token = token1.ToString();
            }
            else
            {
                token = TokeManager.GenerateToken(userName);
                context.Items.Add(TokeManager.JwtToken, token);
            }
            return token;
        }
    }

    public static class TokeManager
    {

        public const string JwtToken = "Jwt.Token";
        // 定义Secret
        private static string Secret = "ERMN05OPLoDvbTTa/QkqLNMI7cPLguaRyHzyg7n5qNBVjQmtBhz4SzYh4NBVCXi3KJHlSXKP+oi2+bXr6CUYTR==";
        public static string GenerateToken(string userName)
        {
            byte[] key = Convert.FromBase64String(Secret);
            SymmetricSecurityKey securityKey = new SymmetricSecurityKey(key);
            // 同AuthenticationProperty
            SecurityTokenDescriptor descriptor = new SecurityTokenDescriptor
            {
                Subject = new System.Security.Claims.ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, userName) }), // ClaimsIdentity
                Expires = DateTime.UtcNow.AddSeconds(5),
                SigningCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature)
            };
            JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
            JwtSecurityToken token = handler.CreateJwtSecurityToken(descriptor);
            return handler.WriteToken(token);
        }

        public static ClaimsPrincipal GetPrincipal(string token)
        {
            try
            {
                JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
                JwtSecurityToken jwtToken = (JwtSecurityToken)handler.ReadToken(token);
                if (jwtToken == null) return null;
                var key = Convert.FromBase64String(Secret);
                // 设置验证参数
                var parameters = new TokenValidationParameters
                {
                    RequireAudience = false,
                    RequireExpirationTime = true,
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    IssuerSigningKey = new SymmetricSecurityKey(key)
                };
                var principal = handler.ValidateToken(token, parameters, out SecurityToken validatedToke);
                return principal;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static string ValidateToken(string token)
        {
            var principal = GetPrincipal(token);
            if (principal == null) return string.Empty;
            ClaimsIdentity identity;
            try
            {
                identity = (ClaimsIdentity)principal.Identity;
            }
            catch (Exception)
            {

                throw;
            }
            var claim = identity.FindFirst(ClaimTypes.Name);
            return claim.Value;
        }
    }
}
