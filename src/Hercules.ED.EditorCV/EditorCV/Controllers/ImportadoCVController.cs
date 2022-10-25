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

using System.Text;
using Models;
using Newtonsoft.Json;

namespace EditorCV.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [EnableCors("_myAllowSpecificOrigins")]
    public class ImportadoCVController : Controller
    {
        readonly ConfigService _Configuracion;

        private static ConcurrentDictionary<string, PetitionStatus> petitionStatus = new ConcurrentDictionary<string, PetitionStatus>();


        public ImportadoCVController(ConfigService pConfig)
        {
            _Configuracion = pConfig;
        }

        /// <summary>
        /// Servicio de Preimportación del CV
        /// </summary>
        /// <param name="userID"></param>
        /// <param name="File"></param>
        /// <param name="petitionID">ID de la petición</param>
        /// <returns></returns>
        [HttpPost("FechaCheck")]
        public IActionResult FechaCheck([FromForm][Required] string pCVID)
        {
            string url = _Configuracion.GetUrlImportador()+"/fechaCheck";
            HttpClient client = new HttpClient();
            FormUrlEncodedContent form = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string,string>("pCVID",pCVID)
            });

            HttpResponseMessage response = client.PostAsync(url, form).Result;
            if (response.IsSuccessStatusCode)
            {
                return Ok(response.Content.ReadAsStringAsync().Result);
            }
            else
            {
                return Ok();
            }
        }


       /// <summary>
       /// Servicio de Preimportación del CV
       /// </summary>
       /// <param name="userID"></param>
       /// <param name="File"></param>
       /// <param name="petitionID">ID de la petición</param>
       /// <returns></returns>
       [HttpPost("PreimportarCV")]
        public IActionResult PreimportarCV([Required][FromForm] string userID, [Required][FromForm] string petitionID, [Required] IFormFile File)
        {
            try
            {
                //Estado de la petición
                PetitionStatus estadoPreimport = new PetitionStatus(1, 4, "ESTADO_PREIMPORTAR_INICIO");
                petitionStatus[petitionID] = estadoPreimport;

                string pCVId = UtilityCV.GetCVFromUser(userID);
                if (string.IsNullOrEmpty(pCVId))
                {
                    throw new Exception("Usuario no encontrado " + userID);
                }


                AccionesImportacion accionesImportacion = new AccionesImportacion();
                Preimport preimport = accionesImportacion.PreimportarCV(_Configuracion, pCVId, File, petitionID);

                //Cambio el estado de la petición
                petitionStatus[petitionID].actualWork = 4;
                petitionStatus[petitionID].subActualWork = 0;
                petitionStatus[petitionID].subTotalWorks = 0;
                petitionStatus[petitionID].actualWorkTitle = "ESTADO_PREIMPORTAR_FINLECTURA";

                ConcurrentBag<Models.API.Templates.Tab> tabTemplatesAux = UtilityCV.TabTemplates;
                ConcurrentDictionary<int, Models.API.Response.Tab> respuesta = accionesImportacion.GetListTabs(tabTemplatesAux, preimport);

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
        /// Devuelve el estado actual de la peticion con identificador <paramref name="petitionID"/>
        /// </summary>
        /// <param name="petitionID">Identificador de la petición</param>
        /// <param name="accion">Accion desde donde se lanza la petición</param>
        /// <returns>Estado de la petición</returns>
        [HttpGet("ImportarCVStatus")]
        public IActionResult ImportarCVStatus([Required] string petitionID, [Required] string accion)
        {
            try
            {
                if (petitionStatus.ContainsKey(petitionID))
                {
                    if (petitionStatus[petitionID].actualWorkTitle != "ESTADO_PREIMPORTAR_FINLECTURA")
                    {
                        //Petición de estado
                        try
                        {
                            string urlEstado = _Configuracion.GetUrlImportador() + "/PetitionCVStatus?petitionID=" + petitionID;
                            HttpClient httpClientEstado = new HttpClient();
                            HttpResponseMessage responseEstado = httpClientEstado.GetAsync($"{ urlEstado }").Result;
                            PetitionStatus estadoRespuesta = JsonConvert.DeserializeObject<PetitionStatus>(responseEstado.Content.ReadAsStringAsync().Result);
                                                        
                            if (estadoRespuesta != null && accion == "PREIMPORTAR")
                            {
                                if (estadoRespuesta.actualWorkTitle == "ESTADO_PREIMPORTAR_LECTURA")
                                {
                                    petitionStatus[petitionID].actualWork = 2;
                                    petitionStatus[petitionID].actualWorkTitle = "ESTADO_PREIMPORTAR_LECTURA";
                                    petitionStatus[petitionID].actualWorkSubtitle = estadoRespuesta.actualWorkSubtitle;
                                    petitionStatus[petitionID].actualSubWorks = estadoRespuesta.actualSubWorks;
                                    petitionStatus[petitionID].actualSubTotalWorks = estadoRespuesta.actualSubTotalWorks;
                                }
                                else if (estadoRespuesta.actualWorkTitle == "ESTADO_PREIMPORTAR_PROCESARDATOS")
                                {
                                    petitionStatus[petitionID].actualWork = 3;
                                    petitionStatus[petitionID].subActualWork = estadoRespuesta.actualWork;
                                    petitionStatus[petitionID].subTotalWorks = estadoRespuesta.totalWorks;
                                    petitionStatus[petitionID].actualWorkTitle = "ESTADO_PREIMPORTAR_PROCESARDATOS";
                                    petitionStatus[petitionID].actualWorkSubtitle = estadoRespuesta.actualWorkSubtitle;
                                    petitionStatus[petitionID].actualSubWorks = estadoRespuesta.actualSubWorks;
                                    petitionStatus[petitionID].actualSubTotalWorks = estadoRespuesta.actualSubTotalWorks;
                                }
                            }
                            else if (estadoRespuesta != null && accion == "POSTIMPORTAR")
                            {
                                if (estadoRespuesta.actualWorkTitle == "ESTADO_POSTIMPORTAR_LECTURA")
                                {
                                    petitionStatus[petitionID].actualWork = 2;
                                    petitionStatus[petitionID].subActualWork = estadoRespuesta.actualWork;
                                    petitionStatus[petitionID].subTotalWorks = estadoRespuesta.totalWorks;
                                    petitionStatus[petitionID].actualWorkTitle = estadoRespuesta.actualWorkTitle;
                                    petitionStatus[petitionID].actualWorkSubtitle = estadoRespuesta.actualWorkSubtitle;
                                    petitionStatus[petitionID].actualSubWorks = estadoRespuesta.actualSubWorks;
                                    petitionStatus[petitionID].actualSubTotalWorks = estadoRespuesta.actualSubTotalWorks;
                                }
                                else if (estadoRespuesta.actualWorkTitle == "ESTADO_POSTIMPORTAR_DUPLICAR")
                                {
                                    petitionStatus[petitionID].actualWork = 3;
                                    petitionStatus[petitionID].subActualWork = estadoRespuesta.actualWork;
                                    petitionStatus[petitionID].subTotalWorks = estadoRespuesta.totalWorks;
                                    petitionStatus[petitionID].actualWorkTitle = estadoRespuesta.actualWorkTitle;
                                    petitionStatus[petitionID].actualWorkSubtitle = estadoRespuesta.actualWorkSubtitle;
                                    petitionStatus[petitionID].actualSubWorks = estadoRespuesta.actualSubWorks;
                                    petitionStatus[petitionID].actualSubTotalWorks = estadoRespuesta.actualSubTotalWorks;
                                }
                                else if (estadoRespuesta.actualWorkTitle == "ESTADO_POSTIMPORTAR_FUSIONAR")
                                {
                                    petitionStatus[petitionID].actualWork = 4;
                                    petitionStatus[petitionID].subActualWork = estadoRespuesta.actualWork;
                                    petitionStatus[petitionID].subTotalWorks = estadoRespuesta.totalWorks;
                                    petitionStatus[petitionID].actualWorkTitle = estadoRespuesta.actualWorkTitle;
                                    petitionStatus[petitionID].actualWorkSubtitle = estadoRespuesta.actualWorkSubtitle;
                                    petitionStatus[petitionID].actualSubWorks = estadoRespuesta.actualSubWorks;
                                    petitionStatus[petitionID].actualSubTotalWorks = estadoRespuesta.actualSubTotalWorks;
                                }
                                else if (estadoRespuesta.actualWorkTitle == "ESTADO_POSTIMPORTAR_SOBRESCRIBIR")
                                {
                                    petitionStatus[petitionID].actualWork = 5;
                                    petitionStatus[petitionID].subActualWork = estadoRespuesta.actualWork;
                                    petitionStatus[petitionID].subTotalWorks = estadoRespuesta.totalWorks;
                                    petitionStatus[petitionID].actualWorkTitle = estadoRespuesta.actualWorkTitle;
                                    petitionStatus[petitionID].actualWorkSubtitle = estadoRespuesta.actualWorkSubtitle;
                                    petitionStatus[petitionID].actualSubWorks = estadoRespuesta.actualSubWorks;
                                    petitionStatus[petitionID].actualSubTotalWorks = estadoRespuesta.actualSubTotalWorks;
                                }
                            }

                            if (petitionStatus[petitionID].subActualWork > petitionStatus[petitionID].subTotalWorks)
                            {
                                petitionStatus[petitionID].subActualWork = petitionStatus[petitionID].subTotalWorks;
                            }
                        }
                        catch (Exception)
                        {

                        }
                    }
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
        /// Servicio de Postimportación del CV
        /// </summary>
        /// <param name="userID"></param>
        /// <param name="fileData"></param>
        /// <param name="filePreimport"></param>
        /// <param name="listaId"></param>
        /// <param name="listaOpcionSeleccionados"></param>
        /// <returns></returns>
        [HttpPost("PostimportarCV")]
        public IActionResult PostimportarCV([Required][FromForm] string userID, [Required][FromForm] string petitionID,
            [FromForm] string fileData, [FromForm] string filePreimport, [FromForm] string listaId, [FromForm] string listaOpcionSeleccionados)
        {
            try
            {
                //Estado de la petición
                PetitionStatus estadoPostimport = new PetitionStatus(1, 5, "ESTADO_POSTIMPORTAR_INICIO");
                petitionStatus[petitionID] = estadoPostimport;

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

                AccionesImportacion accionesImportacion = new AccionesImportacion();
                accionesImportacion.PostimportarCV(_Configuracion, pCVId, petitionID, petitionStatus, file, filePreimport, listadoId, dicOpciones);

                return Ok();
            }
            catch (Exception ex)
            {
                return Ok(new Models.API.Response.JsonResult() { error = ex.Message + " " + ex.StackTrace });
            }
        }

    }
}
