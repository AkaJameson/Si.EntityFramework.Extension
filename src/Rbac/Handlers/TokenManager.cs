using Microsoft.IdentityModel.Tokens;
using Si.EntityFramework.Extension.Rbac.Entitys;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Si.EntityFramework.Extension.Rbac.Handlers
{
    public class TokenManager
    {
        private RbacOptions _rbacOptions;
        public TokenManager(RbacOptions rbacOptions)
        {
            _rbacOptions = rbacOptions;
        }
        public bool ValidateToken(string token, out List<Claim> claims)
        {
            claims = new List<Claim>();
            if (string.IsNullOrEmpty(token))
                return false;
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(_rbacOptions.SecrectKey);
                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = _rbacOptions.Issuer,
                    ValidateAudience = true,
                    ValidAudience = _rbacOptions.Audience,
                    ValidateLifetime = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key)
                };
                var principal = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);
                var userClaims = principal.Claims;
                claims.AddRange(userClaims);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        public string GenerateToken(long userId, string userName, List<string> roleName, string TentantId = null)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_rbacOptions.SecrectKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var claims = new List<Claim>
            {
                new Claim("UserId", userId.ToString()),
                new Claim("UserName", userName)
            };
            if (TentantId != null)
            {
                claims.Add(new Claim("TentantId", TentantId));
            }
            claims.AddRange(roleName.Select(r => new Claim("RoleName", r)));
            var token = new JwtSecurityToken(
                _rbacOptions.Issuer,
                _rbacOptions.Audience,
                claims,
                expires: DateTime.Now.AddHours(1),
                signingCredentials: credentials
            );
            var tokenHandler = new JwtSecurityTokenHandler();
            return tokenHandler.WriteToken(token);
        }
    }
}
