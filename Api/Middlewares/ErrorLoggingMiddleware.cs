using System;
using System.IO;
using System.Net;
using System.Security.Authentication;
using System.Threading.Tasks;
using Api.Models.Internal;
using Core;
using Microsoft.AspNetCore.Http;

namespace Api.Middlewares
{
    /// <summary>
    /// Error logging middleware
    /// </summary>
    public class ErrorLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ITechnicalLogger _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        public ErrorLoggingMiddleware(RequestDelegate next, ITechnicalLogger logger)
        {
            _next = next;
            _logger = logger;
        }

        /// <summary>
        /// Invoke
        /// </summary>
        /// <param name="context">HTTP context</param>
        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (FileNotFoundException exception)
            {
                await HandleExceptionAsync(context ?? new DefaultHttpContext(), exception);
            }
            catch (WebException exception)
            {
                await HandleExceptionAsync(context ?? new DefaultHttpContext(), exception);
            }
            catch (AuthenticationException exception)
            {
                await HandleExceptionAsync(context ?? new DefaultHttpContext(), exception);
            }
            catch (Exception exception)
            {
                await HandleExceptionAsync(context ?? new DefaultHttpContext(), exception);
            }
        }

        /// <summary>
        /// handle exception
        /// </summary>
        /// <param name="context">HTTP context</param>
        /// <param name="exception">Exception</param>
        private Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            _logger.Error($"{exception.GetType().FullName} has occurred", exception);

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            return context.Response.WriteAsync(new ErrorDetails
            {
                StatusCode = context.Response.StatusCode,
                Message = "Internal Server Error"
            }.ToString());
        }
    }
}
