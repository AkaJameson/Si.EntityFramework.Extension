using Microsoft.AspNetCore.Http;
using Si.EntityFramework.Extension.Abstraction;
using Si.EntityFramework.Extension.Entitys;
using Si.EntityFramework.Extension.Kits;

namespace Si.EntityFramework.Extension.Rbac.Handlers
{
    internal class UserInfoMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly TokenManager _jwtManager;
        public UserInfoMiddleware(RequestDelegate next, TokenManager jwtManager)
        {
            _next = next;
            _jwtManager = jwtManager;
        }
        public async Task InvokeAsync(HttpContext context)
        {
            var authHeader = context.Request.Headers["Authorization"];
            if (string.IsNullOrEmpty(authHeader.ToString()))
            {
                await _next(context);
                return;
            }
            var authHeaderStr = authHeader.ToString();
            // 如果 token 格式不正确
            if (!authHeaderStr.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                await Response.ReturnBadRequestResponse(context, "Invalid Authorization format.");
                return;
            }
            // 获取 token 并验证
            var token = authHeaderStr.Substring(7);
            if (!_jwtManager.ValidateToken(token, out var claims))
            {
                await Response.ReturnUnauthorizedResponse(context, "Unauthorized.");
                return;
            }
            var userIdstr = claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
            if (string.IsNullOrEmpty(userIdstr) || !long.TryParse(userIdstr, out var userId))
            {
                await Response.ReturnUnauthorizedResponse(context, "Unauthorized.");
                return;
            }
            var tentantId = claims.FirstOrDefault(c => c.Type == "TentantId")?.Value;
            var userName = claims.FirstOrDefault(c => c.Type == "UserName")?.Value;
            var roles = claims.Where(c => c.Type == "RoleName")?.Select(r => r.Value)?.ToList();
            var session = (IUserInfo)context.RequestServices.GetService(typeof(IUserInfo));
            if (session == null)
            {
                throw new ArgumentNullException("haven't register IUserService");
            }
            session.UserId = userId;
            session.UserName = userName ?? string.Empty;
            session.Roles = roles ?? new List<string>();
            session.TenantId = tentantId ?? string.Empty;
            await _next(context);
        }
    }
}
