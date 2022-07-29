using Gnoss.ApiWrapper.Model;
using Hercules.ED.ImportExportCV.Controllers;
using ImportadorWebCV;
using ImportadorWebCV.Exporta;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Models;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace Hercules.ED.ExportadorWebCV.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ExportadorCVController : ControllerBase
    {
        private readonly ILogger<ExportadorCVController> _logger;
        private cvnRootResultBean _cvn;
        readonly ConfigService _Configuracion;

        public ExportadorCVController(ILogger<ExportadorCVController> logger, ConfigService pConfig)
        {
            System.Globalization.CultureInfo.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
            _logger = logger;
            _Configuracion = pConfig;
            _cvn = new cvnRootResultBean();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCVID">ID curriculum</param>
        /// <returns></returns>
        [HttpPost("Exportar")]
        public ActionResult Exportar([FromForm][Required] string pCVID, [FromForm][Required] string lang, [FromForm][Required] string tipoCVNExportacion)
        {
            if (!Utils.UtilityExportar.EsMultiidioma(lang))
            {
                throw new Exception("El lenguaje de exportación es incorrecto");
            }

            ExportaDatos exporta = new ExportaDatos(_cvn, pCVID, lang);
            Entity entity = exporta.GetLoadedEntity(pCVID, "curriculumvitae");

            if (entity == null)
            {
                return Content("El CV no se ha encontrado");
            }

            exporta.ExportaDatosIdentificacion(entity, _Configuracion.GetVersion());
            exporta.ExportaSituacionProfesional(entity);
            exporta.ExportaFormacionAcademica(entity);
            exporta.ExportaActividadDocente(entity);
            exporta.ExportaExperienciaCientificaTecnologica(entity);
            exporta.ExportaActividadCientificaTecnologica(entity);
            exporta.ExportaTextoLibre(entity);

            try
            {
                Export.GenerarPDFWSClient client = new Export.GenerarPDFWSClient();

                //Aumento el tiempo de espera a 2 hora como maximo
                client.Endpoint.Binding.CloseTimeout = new TimeSpan(2, 0, 0);
                client.Endpoint.Binding.SendTimeout = new TimeSpan(2, 0, 0);

                var peticion = client.crearPDFBeanCvnRootBeanAsync(_Configuracion.GetUsuarioPDF(), _Configuracion.GetContraseñaPDF(), "CVN", _cvn.cvnRootBean, tipoCVNExportacion, Utils.UtilityExportar.CvnLangCode(lang));
                var resp = peticion.Result.@return;
                client.Close();

                if (resp.returnCode != "00")
                {
                    _logger.LogError("Error code " + resp.returnCode);
                    return NotFound();
                }

                return File(resp.dataHandler, "application/pdf", resp.filename);
            }
            catch (Exception e)
            {
                _logger.LogError("Exception: " + e.Message + ", stacktrace: " + e.StackTrace);
                return NotFound();
            }
        }

        /// <summary>
        /// Devuelve un fichero PDF con los datos pasados en el listado.
        /// </summary>
        /// <param name="pCVID">ID curriculum</param>
        /// <param name="lang">Lenguaje del CV</param>
        /// <param name="listaId">Listado de identificadores de los recursos a devolver</param>
        /// <returns></returns>
        [HttpPost("ExportarLimitado")]
        public ActionResult Exportar([FromForm][Required] string pCVID, [FromForm][Required] string lang, [FromForm][Required] string tipoCVNExportacion, [FromForm][Optional] List<string> listaId)
        {
            if (!Utils.UtilityExportar.EsMultiidioma(lang))
            {
                throw new Exception("El lenguaje de exportación es incorrecto");
            }

            ExportaDatos exporta = new ExportaDatos(_cvn, pCVID, lang);
            Entity entity = exporta.GetLoadedEntity(pCVID, "curriculumvitae");

            if (entity == null)
            {
                return Content("El CV no se ha encontrado");
            }
            if (listaId == null || listaId.Count == 0)
            {
                return Content("No hay elementos en el listado");
            }

            exporta.ExportaDatosIdentificacion(entity, _Configuracion.GetVersion(), listaId);
            exporta.ExportaSituacionProfesional(entity, listaId);
            exporta.ExportaFormacionAcademica(entity, listaId);
            exporta.ExportaActividadDocente(entity, listaId);
            exporta.ExportaExperienciaCientificaTecnologica(entity, listaId);
            exporta.ExportaActividadCientificaTecnologica(entity, listaId);
            exporta.ExportaTextoLibre(entity, listaId);

            try
            {
                Export.GenerarPDFWSClient client = new Export.GenerarPDFWSClient();

                //Aumento el tiempo de espera a 2 hora como máximo
                client.Endpoint.Binding.CloseTimeout = new TimeSpan(2, 0, 0);
                client.Endpoint.Binding.SendTimeout = new TimeSpan(2, 0, 0);

                var peticion = client.crearPDFBeanCvnRootBeanAsync(_Configuracion.GetUsuarioPDF(), _Configuracion.GetContraseñaPDF(), "CVN", _cvn.cvnRootBean, tipoCVNExportacion, Utils.UtilityExportar.CvnLangCode(lang));
                var resp = peticion.Result.@return;
                client.Close();


                if (resp.returnCode != "00")
                {
                    _logger.LogError("Error code " + resp.returnCode);
                    return NotFound();
                }

                return File(resp.dataHandler, "application/pdf", resp.filename);
            }
            catch (Exception e)
            {
                _logger.LogError("Exception: " + e.Message + ", stacktrace: " + e.StackTrace);
                return NotFound();
            }

        }
    }
}
