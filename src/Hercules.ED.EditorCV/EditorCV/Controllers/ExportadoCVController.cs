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

        [HttpGet("GetCV")]
        public void GetCV(string userID, string lang)
        {
            string pCVId = UtilityCV.GetCVFromUser(userID);
            if (string.IsNullOrEmpty(pCVId))
            {
                throw new Exception("Usuario no encontrado " + userID);
            }

            //Añado el archivo
            //AccionesExportacion.AddFile(_Configuracion, pCVId, lang);
        }

        /// <summary>
        /// Obtiene los datos de todas las pestaña dentro del editor
        /// </summary>
        /// <param name="userID"></param>
        /// <param name="pLang"></param>
        /// <returns></returns>
        [HttpGet("GetAllTabs")]
        public IActionResult GetAllTabs(string userID, string pLang)
        {
            try
            {
                string pCVId = UtilityCV.GetCVFromUser(userID);
                if (string.IsNullOrEmpty(pCVId))
                {
                    throw new Exception("Usuario no encontrado " + userID);
                }
                ConcurrentDictionary<string, string> pListId = AccionesExportacion.GetAllTabs(pCVId);

                AccionesEdicion accionesEdicion = new AccionesEdicion();
                ConcurrentDictionary<int,AuxTab> listTabs = new ConcurrentDictionary<int,AuxTab>();

                ConcurrentBag<Models.API.Templates.Tab> tabTemplatesAux = UtilityCV.TabTemplates;

                Parallel.ForEach(pListId, new ParallelOptions { MaxDegreeOfParallelism = 6 }, keyValue =>
                {
                    int index= tabTemplatesAux.ToList().IndexOf(UtilityCV.TabTemplates.ToList().First(x => x.rdftype == keyValue.Key));
                    listTabs.TryAdd(index,accionesEdicion.GetTab(pCVId, keyValue.Value, keyValue.Key, pLang));
                });

                return Ok(listTabs.OrderBy(x => x.Key).Select(x => (object)x.Value));
            }
            catch (Exception ex)
            {
                return Ok(new Models.API.Response.JsonResult() { error = ex.Message + " " + ex.StackTrace });
            }
        }
    }
}
