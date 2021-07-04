using Microsoft.AspNetCore.Http;

namespace Api.Helpers
{
    /// <summary>
    /// HTTP context extension
    /// </summary>
    public static class HttpContextAccessorExtension
    {
        /// <summary>
        /// Gets the id of the current user
        /// </summary>
        public static int CurrentUserId(this IHttpContextAccessor httpContextAccessor)
        {
            var currentUserId = httpContextAccessor?.HttpContext?.User?.Identity?.Name;

            return int.TryParse(currentUserId, out var userId) ? userId : 0;
        }

        /// <summary>
        /// Gets the IP address of the current user
        /// </summary>
        public static string? CurrentIpAddress(this IHttpContextAccessor httpContextAccessor)
        {
            return httpContextAccessor?.HttpContext?.Connection?.RemoteIpAddress?.MapToIPv4().ToString();
        }
    }
}
