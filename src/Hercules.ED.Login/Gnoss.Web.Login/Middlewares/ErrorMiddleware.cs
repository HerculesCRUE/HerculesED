using Es.Riam.Gnoss.AD.EntityModel;
using Es.Riam.Gnoss.Elementos.Notificacion;
using Es.Riam.Gnoss.Recursos;
using Es.Riam.Gnoss.Servicios.ControladoresServiciosWeb;
using Es.Riam.Gnoss.Util.Configuracion;
using Es.Riam.Gnoss.Util.General;
using Es.Riam.Gnoss.Web.MVC.Models.Administracion;
using Es.Riam.Gnoss.Web.MVC.Models.Routes;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using System.Web;

namespace Gnoss.Web.Login
{
    public class ErrorMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IHostingEnvironment _env;
        private ConfigService _configService;
        public ErrorMiddleware(RequestDelegate next, IHostingEnvironment env, ConfigService configService)
        { 
            _next = next;
            _env = env;
            _configService = configService;
        }

        public async Task Invoke(HttpContext context, LoggingService loggingService, EntityContext entityContext)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                HandleExceptionAsync(context, ex, loggingService, entityContext);
                await _next(context);
            }
        }

        private void HandleExceptionAsync(HttpContext context, Exception ex, LoggingService loggingService, EntityContext entityContext)
        {
            var code = HttpStatusCode.InternalServerError;
            context.Response.StatusCode = (int)code;
            string mensajeError = $" ERROR:  {ex.Message}\r\nStackTrace: {ex.StackTrace}";
            loggingService.GuardarLogError(ex); //Enviar Excepción

        }

    }

}
