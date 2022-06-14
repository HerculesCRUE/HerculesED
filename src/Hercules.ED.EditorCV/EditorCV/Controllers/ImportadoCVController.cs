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

                //Añado el archivo en la posicion 99 de la respuesta.
                Models.API.Response.Tab tab = new Models.API.Response.Tab();
                tab.title = preimport.cvn_xml;
                respuesta.TryAdd(99, tab);

                return Ok(respuesta);
            }
            catch (Exception ex)
            {
                return Ok(new Models.API.Response.JsonResult() { error = ex.Message + " " + ex.StackTrace });
            }
        }

        [HttpPost("PostimportarCV")]
        public IActionResult PostimportarCV([Required][FromForm] string userID, [FromForm] string fileData, [FromForm] string listaId, [FromForm] string listaOpcionSeleccionados)
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

                //Si la opcion es "ig"-"ignorar" elimino ese Identificador de los listados
                foreach(KeyValuePair<string, string> valuePair in dicOpciones)
                {
                    if (valuePair.Value.Equals("ig") && listadoId.Contains(valuePair.Key))
                    {
                        listadoId.Remove(valuePair.Key);
                        dicOpciones.Remove(valuePair.Key);
                    }
                }

                XmlSerializer ser = new XmlSerializer(typeof(Preimport));
                Preimport preimport = new Preimport();
                using (TextReader reader = new StringReader(fileData))
                {
                    // Call the Deserialize method to restore the object's state.
                    preimport = (Preimport)ser.Deserialize(reader);
                }

                AccionesImportacion accionesImportacion = new AccionesImportacion();                
                accionesImportacion.PostimportarCV(_Configuracion, pCVId, preimport, listadoId, dicOpciones);

                return Ok();
            }
            catch (Exception ex)
            {
                return Ok(new Models.API.Response.JsonResult() { error = ex.Message + " " + ex.StackTrace });
            }
        }

    }
}
