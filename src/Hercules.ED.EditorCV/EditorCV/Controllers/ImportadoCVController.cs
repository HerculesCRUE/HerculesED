using Microsoft.AspNetCore.Http;
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
using EditorCV.Models.PreimportModels;
using System.Text.Json;
using System.Xml.Serialization;
using System.IO;
using System.Text;

namespace EditorCV.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [EnableCors("_myAllowSpecificOrigins")]
    public class ImportadoCVController : Controller
    {
        readonly ConfigService _Configuracion;

        public ImportadoCVController(ConfigService pConfig)
        {
            _Configuracion = pConfig;
        }

        /// <summary>
        /// Servicio de Preimportación del CV
        /// </summary>
        /// <param name="userID"></param>
        /// <param name="File"></param>
        /// <returns></returns>
        [HttpPost("PreimportarCV")]
        public IActionResult PreimportarCV([Required][FromForm] string userID, [Required] IFormFile File)
        {
            try
            {
                string pCVId = UtilityCV.GetCVFromUser(userID);
                if (string.IsNullOrEmpty(pCVId))
                {
                    throw new Exception("Usuario no encontrado " + userID);
                }

                AccionesImportacion accionesImportacion = new AccionesImportacion();
                Preimport preimport = accionesImportacion.PreimportarCV(_Configuracion, pCVId, File);

                ConcurrentBag<Models.API.Templates.Tab> tabTemplatesAux = UtilityCV.TabTemplates;
                ConcurrentDictionary<int, Models.API.Response.Tab> respuesta =  accionesImportacion.GetListTabs(tabTemplatesAux, preimport);

                //Añado el archivoXML en la posicion 99 de la respuesta.
                Models.API.Response.Tab tab = new Models.API.Response.Tab();
                tab.title = preimport.cvn_xml;
                respuesta.TryAdd(99, tab);

                //Añado XML de preimportacion en la posicion 100 de la respuesta.
                Models.API.Response.Tab tabPreimportar = new Models.API.Response.Tab();
                tabPreimportar.title = preimport.cvn_preimportar;
                respuesta.TryAdd(100, tabPreimportar);

                return Ok(respuesta);
            }
            catch (Exception ex)
            {
                return Ok(new Models.API.Response.JsonResult() { error = ex.Message + " " + ex.StackTrace });
            }
        }

        /// <summary>
        /// Servicio de Postimportación del CV
        /// </summary>
        /// <param name="userID"></param>
        /// <param name="fileData"></param>
        /// <param name="filePreimport"></param>
        /// <param name="listaId"></param>
        /// <param name="listaOpcionSeleccionados"></param>
        /// <returns></returns>
        [HttpPost("PostimportarCV")]
        public IActionResult PostimportarCV([Required][FromForm] string userID, [FromForm] string fileData, [FromForm] string filePreimport, [FromForm] string listaId, [FromForm] string listaOpcionSeleccionados)
        {
            try
            {
                List<string> listadoId = new List<string>();
                List<string> listadoOpciones = new List<string>();
                Dictionary<string, string> dicOpciones = new Dictionary<string, string>();

                string pCVId = UtilityCV.GetCVFromUser(userID);
                if (string.IsNullOrEmpty(pCVId))
                {
                    throw new Exception("Usuario no encontrado " + userID);
                }

                if (!string.IsNullOrEmpty(listaId))
                {
                    listadoId = listaId.Split("@@@").ToList();
                }

                if (!string.IsNullOrEmpty(listaOpcionSeleccionados))
                {
                    string idOpcion;
                    string valueOpcion;
                    listadoOpciones = listaOpcionSeleccionados.Split("@@@").ToList();
                    foreach (string opcion in listadoOpciones)
                    {
                        idOpcion = opcion.Split("|||").First();
                        valueOpcion = opcion.Split("|||").Last();
                        dicOpciones.Add(idOpcion, valueOpcion);
                    }
                }

                byte[] file = Encoding.UTF8.GetBytes(fileData);
                byte[] filePreimportByte = Encoding.UTF8.GetBytes(filePreimport);

                AccionesImportacion accionesImportacion = new AccionesImportacion();                
                accionesImportacion.PostimportarCV(_Configuracion, pCVId, file, filePreimportByte, listadoId, dicOpciones);

                return Ok();
            }
            catch (Exception ex)
            {
                return Ok(new Models.API.Response.JsonResult() { error = ex.Message + " " + ex.StackTrace });
            }
        }

    }
}
