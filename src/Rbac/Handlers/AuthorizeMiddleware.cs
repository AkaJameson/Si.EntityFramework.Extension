using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using Si.EntityFramework.Extension.DataBase.Abstraction;
using Si.EntityFramework.Extension.Rbac.Kits;
using System.Reflection;
using System.Security.Claims;

namespace Si.EntityFramework.Extension.Rbac.Handlers
{
    public class AuthorizationMiddleware
    {
        private readonly RequestDelegate _next;
        public AuthorizationMiddleware(RequestDelegate next)
        {
            _next = next;
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
            var sessions = (IUserInfo)context?.RequestServices.GetService(typeof(IUserInfo));
            // 获取方法上的 PermissionAttribute
            var anonymousAttribute = actionDescriptor.MethodInfo.GetCustomAttribute<AllowAnonymousAttribute>()
                ?? actionDescriptor.ControllerTypeInfo.GetCustomAttribute<AllowAnonymousAttribute>();
            if (anonymousAttribute != null)
            {
                await _next(context);
                return;
            }
            var PermissionAttribute = actionDescriptor.MethodInfo.GetCustomAttribute<PermissionAttribute>()
                ?? actionDescriptor.ControllerTypeInfo.GetCustomAttribute<PermissionAttribute>();
            if (PermissionAttribute == null)
            {
                await Response.ReturnForbidden(context);
                return;
            }
            var hasPermission = await CheckPermissionAsync(context, sessions, PermissionAttribute);
            if (hasPermission)
                await _next(context);
        }

        public async Task<bool> CheckPermissionAsync(HttpContext context, IUserInfo userSessions, PermissionAttribute permissionAttribute)
        {

            if (PermCache.HasPermission(userSessions.Roles, permissionAttribute?.PermissionName))
            {
                return true;
            }
            await Response.ReturnForbidden(context);
            return false;
        }
    }
}
