using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Hercules.ED.UMLS.Models.Data;
using System.Collections.Generic;

namespace Hercules.ED.UMLS.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UmlsController : ControllerBase
    {
        readonly ConfigService _Configuracion;
        private Models.UMLS _UMLS;

        /// <summary>
        /// Contructor.
        /// </summary>
        /// <param name="pConfig">Configuración.</param>
        public UmlsController(ConfigService pConfig)
        {
            _Configuracion = pConfig;
            _UMLS = new Models.UMLS(_Configuracion);
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public Data GetData(string pName, string pMeshId)
        {
            // Objeto a devolver.
            Data data = new Data();

            // Obtención del TGT.
            string tgt = _UMLS.GetTGT();

            // Obtención del ID de SNOMED.
            string idSnomed = null;

            // Obtención del ID SNOMED
            int contador = 0;
            while (idSnomed == null)
            {
                if(contador == 5)
                {
                    return data;
                }
                else
                {
                    contador++;
                }

                // Obtención del ST.
                string st = _UMLS.GetTicket(tgt);

                // Petición al servicio de obtención de ID.
                idSnomed = _UMLS.GetSnomedId(pName, pMeshId.Trim(), st, data);                
            }

            // Obtención de la información de las relaciones.
            data.relations = _UMLS.GetRelaciones(idSnomed, tgt);

            return data;
        }
    }
}
