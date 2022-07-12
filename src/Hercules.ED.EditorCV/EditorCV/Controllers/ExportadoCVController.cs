using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using Microsoft.AspNetCore.Cors;
using EditorCV.Models;
using EditorCV.Models.API.Input;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using EditorCV.Models.Utils;
using EditorCV.Models.API.Templates;
using EditorCV.Models.API.Response;
using System.Net.Http;
using System.ComponentModel.DataAnnotations;
using System.Runtime.InteropServices;

namespace EditorCV.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [EnableCors("_myAllowSpecificOrigins")]
    public class ExportadoCVController : ControllerBase
    {
        readonly ConfigService _Configuracion;

        public ExportadoCVController(ConfigService pConfig)
        {
            _Configuracion = pConfig;
        }

        /// <summary>
        /// Añade un archivo descargable al CV
        /// </summary>
        /// <param name="userID"></param>
        /// <param name="lang"></param>
        /// <param name="listaId">listado de Identificadores concatenados por "@@@"</param>
        [HttpPost("GetCV")]
        public IActionResult GetCV([Required][FromForm] string userID, [Required][FromForm] string lang, [Required][FromForm] string nombreCV, [Optional][FromForm] string listaId)
        {
            try
            {
                List<string> listadoId = null;
                if (listaId != null)
                {
                    listadoId = new List<string>();
                    listadoId = listaId.Split("@@@", StringSplitOptions.RemoveEmptyEntries).ToList();
                }

                string pCVId = UtilityCV.GetCVFromUser(userID);
                if (string.IsNullOrEmpty(pCVId))
                {
                    throw new Exception("Usuario no encontrado " + userID);
                }

                //Añado el archivo
                AccionesExportacion accionesExportacion = new AccionesExportacion();
                accionesExportacion.AddFile(_Configuracion, pCVId, nombreCV, lang, listadoId);
                return Ok(new Models.API.Response.JsonResult() { ok = true });
            }
            catch (Exception ex)
            {
                return Ok(new Models.API.Response.JsonResult() { error = ex.Message + " " + ex.StackTrace });
            }

        }

        /// <summary>
        /// Obtiene los datos de todas las pestaña dentro del editor
        /// </summary>
        /// <param name="userID">Identificador del usuario</param>
        /// <param name="pLang">lenguaje </param>
        /// <returns></returns>
        [HttpGet("GetAllTabs")]
        public IActionResult GetAllTabs([Required] string userID, [Required] string pLang)
        {
            try
            {
                string pCVId = UtilityCV.GetCVFromUser(userID);
                if (string.IsNullOrEmpty(pCVId))
                {
                    throw new Exception("Usuario no encontrado " + userID);
                }
                AccionesExportacion accionesExportacion = new AccionesExportacion();
                ConcurrentDictionary<string, string> pListId = accionesExportacion.GetAllTabs(pCVId);

                AccionesEdicion accionesEdicion = new AccionesEdicion();
                ConcurrentDictionary<int, AuxTab> listTabs = new ConcurrentDictionary<int, AuxTab>();

                ConcurrentBag<Models.API.Templates.Tab> tabTemplatesAux = UtilityCV.TabTemplates;

                Parallel.ForEach(pListId, new ParallelOptions { MaxDegreeOfParallelism = 6 }, keyValue =>
                {
                    int index = tabTemplatesAux.ToList().IndexOf(UtilityCV.TabTemplates.ToList().First(x => x.rdftype == keyValue.Key));
                    listTabs.TryAdd(index, accionesEdicion.GetTab(_Configuracion, pCVId, keyValue.Value, keyValue.Key, pLang));
                });

                return Ok(listTabs.OrderBy(x => x.Key).Select(x => (object)x.Value));
            }
            catch (Exception ex)
            {
                return Ok(new Models.API.Response.JsonResult() { error = ex.Message + " " + ex.StackTrace });
            }
        }

        [HttpGet("GetListadoCV")]
        public IActionResult GetListadoCV([Required] string userID, [Required] string baseUrl, [Required] int timezoneOffset)
        {
            try
            {
                string pCVId = UtilityCV.GetCVFromUser(userID);
                if (string.IsNullOrEmpty(pCVId))
                {
                    throw new Exception("Usuario no encontrado " + userID);
                }
                List<FilePDF> pListId = AccionesExportacion.GetListPDFFile(pCVId, baseUrl, timezoneOffset);
                return Ok(pListId);
            }
            catch (Exception ex)
            {
                return Ok(new Models.API.Response.JsonResult() { error = ex.Message + " " + ex.StackTrace });
            }
        }

        [HttpGet("GetPerfilExportacion")]
        public IActionResult GetPerfilExportacion([Required] string userID)
        {
            try
            {
                string pCVId = UtilityCV.GetCVFromUser(userID);
                if (string.IsNullOrEmpty(pCVId))
                {
                    throw new Exception("Usuario no encontrado " + userID);
                }
                AccionesExportacion accionesExportacion = new AccionesExportacion();
                Dictionary<string, List<string>> resultado = accionesExportacion.GetPerfilExportacion(pCVId);
                return Ok(resultado);
            }
            catch (Exception ex)
            {
                return Ok(new Models.API.Response.JsonResult() { error = ex.Message + " " + ex.StackTrace });
            }
        }

        [HttpPost("AddPerfilExportacion")]
        public IActionResult AddPerfilExportacion([Required][FromForm] string userID, [Required][FromForm] string title, [Required][FromForm] List<string> checks)
        {
            try
            {
                string pCVId = UtilityCV.GetCVFromUser(userID);
                if (string.IsNullOrEmpty(pCVId))
                {
                    throw new Exception("Usuario no encontrado " + userID);
                }

                AccionesExportacion accionesExportacion = new AccionesExportacion();
                bool resultado = accionesExportacion.AddPerfilExportacion(_Configuracion, pCVId, title, checks);
                return Ok(resultado);
            }
            catch (Exception ex)
            {
                return Ok(new Models.API.Response.JsonResult() { error = ex.Message + " " + ex.StackTrace });
            }
        }

        [HttpDelete("DeletePerfilExportacion")]
        public IActionResult DeletePerfilExportacion([Required][FromForm] string userID, [Required][FromForm] string title)
        {
            try
            {
                string pCVId = UtilityCV.GetCVFromUser(userID);
                if (string.IsNullOrEmpty(pCVId))
                {
                    throw new Exception("Usuario no encontrado " + userID);
                }
                return Ok();
            }
            catch (Exception ex)
            {
                return Ok(new Models.API.Response.JsonResult() { error = ex.Message + " " + ex.StackTrace });
            }
        }

    }
}
