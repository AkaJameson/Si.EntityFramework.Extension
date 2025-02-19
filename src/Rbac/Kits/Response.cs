using Microsoft.AspNetCore.Http;

namespace Si.EntityFramework.Extension.Rbac.Kits
{
    public class Response
    {
        /// <summary>
        /// 返回 403 无权限的响应
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static async Task ReturnForbidden(HttpContext context)
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
        public static async Task ReturnUnauthorized(HttpContext context, string message)
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
        public static async Task ReturnBadRequest(HttpContext context, string message)
        {
            context.Response.StatusCode = 400;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync($"{{\"message\":\"{message}\"}}");
        }
    }
}
