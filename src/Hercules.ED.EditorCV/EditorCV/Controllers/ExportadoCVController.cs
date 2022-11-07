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
using Models;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Http;

namespace EditorCV.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [EnableCors("_myAllowSpecificOrigins")]
    public class ExportadoCVController : ControllerBase
    {
        readonly ConfigService _Configuracion;

        private static ConcurrentDictionary<string, PetitionStatus> petitionStatus = new ConcurrentDictionary<string, PetitionStatus>();

        public ExportadoCVController(ConfigService pConfig)
        {
            _Configuracion = pConfig;
        }

        /// <summary>
        /// Añade un archivo descargable al CV
        /// </summary>
        /// <param name="userID">Identificador del usuario</param>
        /// <param name="lang">Lenguaje</param>
        /// <param name="listaId">listado de Identificadores concatenados por "@@@"</param>
        [HttpPost("GetCV")]
        public IActionResult GetCV([Required][FromForm] string userID, [Required][FromForm] string lang, [Required][FromForm] string nombreCV,
            [Required][FromForm] string tipoCVNExportacion, [FromForm][Required] string versionExportacion, [Optional][FromForm] string listaId)
        {
            try
            {
                //Solo puede obtenerlo el usuario pasado por parámetro
                if (!Security.CheckUser(new Guid(userID), Request))
                {
                    return StatusCode(StatusCodes.Status401Unauthorized);
                }
                List<string> listadoId = null;
                if (listaId != null)
                {
                    listadoId = listaId.Split("@@@", StringSplitOptions.RemoveEmptyEntries).ToList();
                }

                string pCVId = UtilityCV.GetCVFromUser(userID);
                if (string.IsNullOrEmpty(pCVId))
                {
                    throw new ArgumentException("Usuario no encontrado " + userID);
                }

                //Añado el archivo
                AccionesExportacion accionesExportacion = new AccionesExportacion();
                accionesExportacion.AddFile(_Configuracion, pCVId, nombreCV, lang, listadoId, tipoCVNExportacion, versionExportacion);
                return Ok(new Models.API.Response.JsonResult() { ok = true });
            }
            catch (Exception ex)
            {
                return Ok(new Models.API.Response.JsonResult() { error = ex.Message + " " + ex.StackTrace });
            }

        }

        /// <summary>
        /// Devuelve el estado actual de la peticion con identificador <paramref name="petitionID"/>
        /// </summary>
        /// <param name="petitionID">Identificador de la petición</param>
        /// <param name="accion">Accion desde donde se lanza la petición</param>
        /// <returns>Estado de la petición</returns>
        [HttpGet("ExportarCVStatus")]
        public IActionResult ExportarCVStatus([Required] string petitionID)
        {
            try
            {
                if (petitionStatus.ContainsKey(petitionID))
                {
                    return Ok(petitionStatus[petitionID]);
                }
                return Ok();
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
        /// <param name="pLang">Lenguaje </param>
        /// <returns></returns>
        [HttpGet("GetAllTabs")]
        public IActionResult GetAllTabs([Required] string userID, [Required] string petitionID, [Required] string pLang)
        {
            try
            {
                //Solo puede obtenerlo el usuario pasado por parámetro
                if (!Security.CheckUser(new Guid(userID), Request))
                {
                    return StatusCode(StatusCodes.Status401Unauthorized);
                }
                string pCVId = UtilityCV.GetCVFromUser(userID);
                if (string.IsNullOrEmpty(pCVId))
                {
                    throw new ArgumentException("Usuario no encontrado " + userID);
                }

                petitionStatus.TryAdd(petitionID, new PetitionStatus(1, 3, "LEYENDO_DATOS_CV"));
                AccionesExportacion accionesExportacion = new AccionesExportacion();
                ConcurrentDictionary<string, string> pListId = accionesExportacion.GetAllTabs(pCVId);

                AccionesEdicion accionesEdicion = new AccionesEdicion();
                ConcurrentDictionary<int, AuxTab> listTabs = new ConcurrentDictionary<int, AuxTab>();

                ConcurrentBag<Models.API.Templates.Tab> tabTemplatesAux = UtilityCV.TabTemplates;

                PetitionStatus peticionCarga = new PetitionStatus(2, 3, "CARGANDO_DATOS_CV");
                peticionCarga.subActualWork = 0;
                peticionCarga.subTotalWorks = tabTemplatesAux.Count;
                petitionStatus[petitionID] = peticionCarga;
                Parallel.ForEach(pListId, new ParallelOptions { MaxDegreeOfParallelism = 5 }, keyValue =>
                {
                    int index = tabTemplatesAux.ToList().IndexOf(UtilityCV.TabTemplates.First(x => x.rdftype == keyValue.Key));
                    listTabs.TryAdd(index, accionesEdicion.GetTab(_Configuracion, pCVId, keyValue.Value, keyValue.Key, pLang));
                    petitionStatus[petitionID].subActualWork++;
                });
                petitionStatus[petitionID] = new PetitionStatus(3, 3, "PINTANDO_DATOS_CV");

                return Ok(listTabs.OrderBy(x => x.Key).Select(x => (object)x.Value));
            }
            catch (Exception ex)
            {
                return Ok(new Models.API.Response.JsonResult() { error = ex.Message + " " + ex.StackTrace });
            }
        }

        /// <summary>
        /// Devuelve el listado de ficheros PDF guardados
        /// </summary>
        /// <param name="userID">Identificador del usuario</param>
        /// <param name="baseUrl"></param>
        /// <param name="timezoneOffset"></param>
        /// <returns>Listado de PDF guardados</returns>
        [HttpGet("GetListadoCV")]
        public IActionResult GetListadoCV([Required] string userID, [Required] string baseUrl, [Required] int timezoneOffset)
        {
            try
            {
                //Solo puede obtenerlo el usuario pasado por parámetro
                if (!Security.CheckUser(new Guid(userID), Request))
                {
                    return StatusCode(StatusCodes.Status401Unauthorized);
                }
                string pCVId = UtilityCV.GetCVFromUser(userID);
                if (string.IsNullOrEmpty(pCVId))
                {
                    throw new ArgumentException("Usuario no encontrado " + userID);
                }
                List<FilePdf> pListId = AccionesExportacion.GetListPDFFile(pCVId, baseUrl, timezoneOffset);
                return Ok(pListId);
            }
            catch (Exception ex)
            {
                return Ok(new Models.API.Response.JsonResult() { error = ex.Message + " " + ex.StackTrace });
            }
        }

        /// <summary>
        /// Devuelve un diccionario con todos los perfiles de informacion del usuario con identificador <paramref name="userID"/>
        /// </summary>
        /// <param name="userID">Identificador del usuario</param>
        /// <returns>Diccionario con los perfiles de exportación</returns>
        [HttpGet("GetPerfilExportacion")]
        public IActionResult GetPerfilExportacion([Required] string userID)
        {
            try
            {
                //Solo puede obtenerlo el usuario pasado por parámetro
                if (!Security.CheckUser(new Guid(userID), Request))
                {
                    return StatusCode(StatusCodes.Status401Unauthorized);
                }
                string pCVId = UtilityCV.GetCVFromUser(userID);
                if (string.IsNullOrEmpty(pCVId))
                {
                    throw new ArgumentException("Usuario no encontrado " + userID);
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

        /// <summary>
        /// Añade el perfil de exportación con titulo <paramref name="title"/> y valores checkeados <paramref name="checks"/> al usuario <paramref name="userID"/>.
        /// </summary>
        /// <param name="userID">Identificador del usuario</param>
        /// <param name="title">Titulo del perfil de exportación</param>
        /// <param name="checks">Listado de elementos marcados</param>
        /// <returns>True si es añadido en base de datos</returns>
        [HttpPost("AddPerfilExportacion")]
        public IActionResult AddPerfilExportacion([Required][FromForm] string userID, [Required][FromForm] string title, [Required][FromForm] List<string> checks)
        {
            try
            {
                //Solo puede obtenerlo el usuario pasado por parámetro
                if (!Security.CheckUser(new Guid(userID), Request))
                {
                    return StatusCode(StatusCodes.Status401Unauthorized);
                }
                string pCVId = UtilityCV.GetCVFromUser(userID);
                if (string.IsNullOrEmpty(pCVId))
                {
                    throw new ArgumentException("Usuario no encontrado " + userID);
                }

                AccionesExportacion accionesExportacion = new AccionesExportacion();
                bool resultado = accionesExportacion.AddPerfilExportacion(pCVId, title, checks);
                return Ok(resultado);
            }
            catch (Exception ex)
            {
                return Ok(new Models.API.Response.JsonResult() { error = ex.Message + " " + ex.StackTrace });
            }
        }

        /// <summary>
        /// Elimina el perfil de exportación con titulo <paramref name="title"/> del usuario <paramref name="userID"/>
        /// </summary>
        /// <param name="userID">Identificador del usuario</param>
        /// <param name="title">Titulo del perfil de exportación</param>
        /// <returns>True si se elimina de base de datos</returns>
        [HttpDelete("DeletePerfilExportacion")]
        public IActionResult DeletePerfilExportacion([Required][FromForm] string userID, [Required][FromForm] string title)
        {
            try
            {
                //Solo puede obtenerlo el usuario pasado por parámetro
                if (!Security.CheckUser(new Guid(userID), Request))
                {
                    return StatusCode(StatusCodes.Status401Unauthorized);
                }
                string pCVId = UtilityCV.GetCVFromUser(userID);
                if (string.IsNullOrEmpty(pCVId))
                {
                    throw new ArgumentException("Usuario no encontrado " + userID);
                }
                AccionesExportacion accionesExportacion = new AccionesExportacion();
                bool resultado = accionesExportacion.DeletePerfilExportacion(pCVId, title);
                return Ok(resultado);
            }
            catch (Exception ex)
            {
                return Ok(new Models.API.Response.JsonResult() { error = ex.Message + " " + ex.StackTrace });
            }
        }

    }
}
