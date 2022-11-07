using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Serilog;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace PublicationAPI.Middlewares
{
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private IConfiguration _configuration { get; set; }
        private string _timeStamp;

        public ErrorHandlingMiddleware(RequestDelegate next, IConfiguration configuration)
        {
            _next = next;
            _configuration = configuration;

        }

        public async Task Invoke(HttpContext context /* other dependencies */)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private Task HandleExceptionAsync(HttpContext context, Exception ex)
        {
            var code = HttpStatusCode.InternalServerError;

            var result = JsonConvert.SerializeObject(new { error = "Internal server error" });
            if (code != HttpStatusCode.InternalServerError)
            {
                result = JsonConvert.SerializeObject(new { error = ex.Message });
            }
            else
            {
                Log.Error($"{ex.Message}\n{ex.StackTrace}\n");
            }

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)code;

            return context.Response.WriteAsync(result);
        }

    }
}
