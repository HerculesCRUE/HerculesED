using Gnoss.ApiWrapper;
using Gnoss.ApiWrapper.Model;
using Harvester;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace OAI_PMH.Models.SGI
{
    public abstract class SGI_Base
    {
        internal string gnossId;

        /// <summary>
        /// Transforma un objeto a un recurso a cargar.
        /// </summary>
        /// <param name="pHarvesterServices"></param>
        /// <param name="pConfig"></param>
        /// <param name="pResourceApi"></param>
        /// <param name="pDicIdentificadores"></param>
        /// <param name="pDicRutas"></param>
        /// <returns></returns>
        public abstract ComplexOntologyResource ToRecurso(IHarvesterServices pHarvesterServices, ReadConfig pConfig, ResourceApi pResourceApi, Dictionary<string, HashSet<string>> pDicIdentificadores, Dictionary<string, Dictionary<string, string>> pDicRutas, bool pFusionarPersona = false, string pIdPersona = null);

        /// <summary>
        /// Obtiene el ID en BBDD preguntando por el crisidentifier.
        /// </summary>
        /// <param name="pResourceApi"></param>
        /// <returns></returns>
        public abstract string ObtenerIDBBDD(ResourceApi pResourceApi);

        /// <summary>
        /// Carga/Modifica el recurso.
        /// </summary>
        /// <param name="pHarvesterServices"></param>
        /// <param name="pConfig"></param>
        /// <param name="pResourceApi"></param>
        /// <param name="pIdGrafo"></param>
        /// <param name="pDicIdentificadores"></param>
        /// <param name="pDicRutas"></param>
        /// <returns></returns>
        public string Cargar(IHarvesterServices pHarvesterServices, ReadConfig pConfig, ResourceApi pResourceApi, string pIdGrafo, Dictionary<string, HashSet<string>> pDicIdentificadores, Dictionary<string, Dictionary<string, string>> pDicRutas, bool pPersona = false)
        {
            gnossId = ObtenerIDBBDD(pResourceApi);
            pResourceApi.ChangeOntoly(pIdGrafo);            

            if (!string.IsNullOrEmpty(gnossId))
            {
                // Modificación.                
                ComplexOntologyResource resource;
                
                if(pPersona)
                {
                    resource = ToRecurso(pHarvesterServices, pConfig, pResourceApi, pDicIdentificadores, pDicRutas, true);
                }
                else
                {
                    resource = ToRecurso(pHarvesterServices, pConfig, pResourceApi, pDicIdentificadores, pDicRutas);
                }
                
                resource.GnossId = gnossId;
                pResourceApi.ModifyComplexOntologyResource(resource, false, false);
                return resource.GnossId;
            }
            else
            {
                // Carga.
                ComplexOntologyResource resource = ToRecurso(pHarvesterServices, pConfig, pResourceApi, pDicIdentificadores, pDicRutas);
                pResourceApi.LoadComplexSemanticResource(resource, false, false);
                return resource.GnossId;
            }
        }        

        /// <summary>
        /// Divide una lista.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="pItems"></param>
        /// <param name="pSize"></param>
        /// <returns></returns>
        public static IEnumerable<List<T>> SplitList<T>(List<T> pItems, int pSize)
        {
            for (int i = 0; i < pItems.Count; i += pSize)
            {
                yield return pItems.GetRange(i, Math.Min(pSize, pItems.Count - i));
            }
        }

        /// <summary>
        /// Resta dos fechas devolviendo los días, meses y años entre ellas.
        /// </summary>
        /// <param name="fechaInicio"></param>
        /// <param name="fechaFin"></param>
        /// <returns></returns>
        public static Tuple<string, string, string> RestarFechas(DateTime fechaInicio, DateTime fechaFin)
        {
            int total = (fechaFin - fechaInicio).Days;
            int anios = total / 365;
            int meses = (total - (365 * anios)) / 30;
            int dias = total - (365 * anios) - (30 * meses);
            return new Tuple<string, string, string>(anios.ToString(), meses.ToString(), dias.ToString());
        }
    }
}
