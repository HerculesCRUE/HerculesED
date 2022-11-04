using Hercules.ED.ImportExportCV.Controllers;
using ImportadorWebCV;
using ImportadorWebCV.Exporta;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.InteropServices;
using System.Text;

namespace Hercules.ED.ExportadorWebCV.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ExportadorCVController : ControllerBase
    {
        private readonly ILogger<ExportadorCVController> _logger;
        private readonly cvnRootResultBean _cvn;
        readonly ConfigService _Configuracion;

        public ExportadorCVController(ILogger<ExportadorCVController> logger, ConfigService pConfig)
        {
            System.Globalization.CultureInfo.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
            _logger = logger;
            _Configuracion = pConfig;
            _cvn = new cvnRootResultBean();
        }

        /// <summary>
        /// Devuelve un fichero PDF con los datos del CV
        /// </summary>
        /// <param name="pCVID">ID curriculum</param>
        /// <returns>Archivo pdf con los datos del CV</returns>
        [HttpPost("Exportar")]
        public ActionResult Exportar([FromForm][Required] string pCVID, [FromForm][Required] string lang, [FromForm][Required] string tipoCVNExportacion, [FromForm][Required] string versionExportacion)
        {
            if (!Utils.UtilityExportar.EsMultiidioma(lang))
            {
                throw new FormatException("El lenguaje de exportación es incorrecto");
            }

            ExportaDatos exporta = new(_cvn, pCVID, lang);
            Entity entity = exporta.GetLoadedEntity(pCVID, "curriculumvitae");

            if (entity == null)
            {
                return Content("El CV no se ha encontrado");
            }

            exporta.ExportaDatosIdentificacion(entity, versionExportacion);
            exporta.ExportaSituacionProfesional(entity);
            exporta.ExportaFormacionAcademica(entity);
            exporta.ExportaActividadDocente(entity);
            exporta.ExportaExperienciaCientificaTecnologica(entity);
            exporta.ExportaActividadCientificaTecnologica(entity, versionExportacion);
            exporta.ExportaTextoLibre(entity);

            try
            {
                if (versionExportacion.Equals("1_4_0"))
                {
                    var resp = exporta.ExportarVersion140(_Configuracion, _cvn, tipoCVNExportacion, lang);

                    if (resp.returnCode != "00")
                    {
                        _logger.LogError("Error code " + resp.returnCode);
                        return NotFound();
                    }

                    return File(resp.dataHandler, "application/pdf", resp.filename);
                }
                else if (versionExportacion.Equals("1_4_3"))
                {
                    var resp = exporta.ExportarVersion143(_Configuracion, _cvn, tipoCVNExportacion, lang);

                    if (resp.returnCode != "00")
                    {
                        _logger.LogError("Error code " + resp.returnCode);
                        return NotFound();
                    }

                    return File(resp.dataHandler, "application/pdf", resp.filename);
                }
                else
                {
                    throw new FormatException("La versión de exportación no es correcta");
                }

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
        /// <returns>Archivo pdf con los datos del CV</returns>
        [HttpPost("ExportarLimitado")]
        public ActionResult ExportarLimitado([FromForm][Required] string pCVID, [FromForm][Required] string lang, [FromForm][Required] string tipoCVNExportacion,
            [FromForm][Required] string versionExportacion, [FromForm][Optional] List<string> listaId)
        {
            if (!Utils.UtilityExportar.EsMultiidioma(lang))
            {
                throw new FormatException("El lenguaje de exportación es incorrecto");
            }

            ExportaDatos exporta = new(_cvn, pCVID, lang);
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
            exporta.ExportaActividadCientificaTecnologica(entity, versionExportacion, listaId);
            exporta.ExportaTextoLibre(entity, listaId);

            try
            {
                if (versionExportacion.Equals("1_4_0"))
                {
                    var resp = exporta.ExportarVersion140(_Configuracion, _cvn, tipoCVNExportacion, lang);

                    if (resp.returnCode != "00")
                    {
                        _logger.LogError("Error code " + resp.returnCode);
                        _logger.LogError("Error info " + Encoding.UTF8.GetString(resp.dataHandler));
                        return NotFound();
                    }

                    return File(resp.dataHandler, "application/pdf", resp.filename);
                }
                else if (versionExportacion.Equals("1_4_3"))
                {
                    var resp = exporta.ExportarVersion143(_Configuracion, _cvn, tipoCVNExportacion, lang);

                    if (resp.returnCode != "00")
                    {
                        _logger.LogError("Error code " + resp.returnCode);
                        _logger.LogError("Error info " + Encoding.UTF8.GetString(resp.dataHandler));
                        return NotFound();
                    }

                    return File(resp.dataHandler, "application/pdf", resp.filename);
                }
                else
                {
                    throw new FormatException("La versión de exportación no es correcta");
                }

            }
            catch (Exception e)
            {
                _logger.LogError("Exception: " + e.Message + ", stacktrace: " + e.StackTrace);
                return NotFound();
            }

        }
    }
}
