using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using Si.EntityFramework.PermGuard.Kits;
using System.Reflection;
using System.Security.Claims;

namespace Si.EntityFramework.PermGuard.Handlers
{
    public class AuthorizationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly TokenManager _jwtManager;
        public AuthorizationMiddleware(RequestDelegate next, TokenManager jwtManager)
        {
            _next = next;
            _jwtManager = jwtManager;
        }
        public async Task InvokeAsync(HttpContext context)
        {
            var endpoint = context?.GetEndpoint();
            var actionDescriptor = endpoint?.Metadata.GetMetadata<ControllerActionDescriptor>();
            if (actionDescriptor == null)
            {
                await _next(context); // 如果没有 ActionDescriptor，直接跳过
                return;
            }
            // 获取方法上的 PermissionAttribute
            var anonymousAttribute = actionDescriptor.MethodInfo.GetCustomAttribute<AllowAnonymousAttribute>() 
                ?? actionDescriptor.ControllerTypeInfo.GetCustomAttribute<AllowAnonymousAttribute>(); ;
            if (anonymousAttribute != null)
            {
                await _next(context);
                return;
            }
            var PermissionAttribute = actionDescriptor.MethodInfo.GetCustomAttribute<PermissionAttribute>()
                ?? actionDescriptor.ControllerTypeInfo.GetCustomAttribute<PermissionAttribute>();
            if (PermissionAttribute == null)
            {
                await ReturnForbiddenResponse(context);
                return;
            }
            var hasPermission = await CheckPermissionAsync(context, PermissionAttribute);
            if (hasPermission)
                await _next(context);
        }
        /// <summary>
        /// 返回 403 无权限的响应
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private async Task ReturnForbiddenResponse(HttpContext context)
        {
            context.Response.StatusCode = 403;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync("{\"message\":\"无权限\"}");
        }

        /// <summary>
        /// 返回 401 未授权的响应
        /// </summary>
        /// <param name="context"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        private async Task ReturnUnauthorizedResponse(HttpContext context, string message)
        {
            context.Response.StatusCode = 401;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync($"{{\"message\":\"{message}\"}}");
        }
        /// <summary>
        /// 返回400 错误请求的响应
        /// </summary>
        /// <param name="context"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        private async Task ReturnBadRequestResponse(HttpContext context, string message)
        {
            context.Response.StatusCode = 400;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync($"{{\"message\":\"{message}\"}}");
        }
        public async Task<bool> CheckPermissionAsync(HttpContext context, PermissionAttribute permissionAttribute)
        {
            var authHeader = context.Request.Headers["Authorization"];
            if (authHeader.ToString() == null)
            {
                await ReturnUnauthorizedResponse(context, "Unauthorized.");
                return false;
            }
            var authHeaderStr = authHeader.ToString();
            // 如果 token 格式不正确
            if (!authHeaderStr.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                await ReturnBadRequestResponse(context, "Invalid Authorization format.");
                return false;
            }
            // 获取 token 并验证
            var token = authHeaderStr.Substring(7);
            if (!_jwtManager.ValidateToken(token, out var claims))
            {
                await ReturnUnauthorizedResponse(context, "Unauthorized.");
                return false;
            }

            var userId = claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
            if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out var userIdInt))
            {
                await ReturnUnauthorizedResponse(context, "Unauthorized.");
                return false;
            }
            context.Items["UserId"] = userIdInt;
            var tentantId = claims.FirstOrDefault(c => c.Type == "TentantId")?.Value;
            if (tentantId != null)
            {
                context.Items["TentantId"] = tentantId;
            }
            // 获取用户角色并验证权限
            var roles = claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).ToList();
            if (PermCache.HasPermission(roles, permissionAttribute?.PermissionName))
            {
                return true;
            }
            await ReturnForbiddenResponse(context);
            return false;
        }
    }
}
