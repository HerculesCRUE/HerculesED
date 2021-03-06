using Gnoss.ApiWrapper;
using Gnoss.ApiWrapper.ApiModel;
using Hercules.ED.DisambiguationEngine.Models;
using System.Collections.Generic;
using System.Linq;
using Utils;
using static Gnoss.ApiWrapper.ApiModel.SparqlObject;

namespace ImportadorWebCV.Sincro.Secciones.ActividadCientificaSubclases
{
    class PeriodosActividad : DisambiguableEntity
    {
        public string numTramos { get; set; }
        public string fecha { get; set; }
        public string entidadAcreditante { get; set; }

        private static readonly DisambiguationDataConfig configDescripcion = new DisambiguationDataConfig()
        {
            type = DisambiguationDataConfigType.equalsTitle,
            score = 0.8f
        };

        private static readonly DisambiguationDataConfig configFecha = new DisambiguationDataConfig()
        {
            type = DisambiguationDataConfigType.equalsItem,
            score = 0.5f,
            scoreMinus = 0.5f
        };

        private static readonly DisambiguationDataConfig configEA = new DisambiguationDataConfig()
        {
            type = DisambiguationDataConfigType.equalsItem,
            score = 0.5f,
            scoreMinus = 0.5f
        };

        public override List<DisambiguationData> GetDisambiguationData()
        {
            List<DisambiguationData> data = new List<DisambiguationData>
            {
                new DisambiguationData()
                {
                    property = "numTramos",
                    config = configDescripcion,
                    value = numTramos
                },

                new DisambiguationData()
                {
                    property = "fecha",
                    config = configFecha,
                    value = fecha
                },

                new DisambiguationData()
                {
                    property = "entidadAcreditacion",
                    config = configEA,
                    value = entidadAcreditante
                }
            };

            return data;
        }

        /// <summary>
        /// Devuelve las entidades de BBDD del <paramref name="pCVID"/> de con las propiedades de <paramref name="propiedadesItem"/>
        /// </summary>
        /// <param name="pResourceApi">pResourceApi</param>
        /// <param name="pCVID">pCVID</param>
        /// <param name="graph">graph</param>
        /// <param name="propiedadesItem">propiedadesItem</param>
        /// <returns></returns>
        public static Dictionary<string, DisambiguableEntity> GetBBDD(ResourceApi pResourceApi, string pCVID, string graph, List<string> propiedadesItem)
        {
            //Obtenemos IDS
            HashSet<string> ids = UtilitySecciones.GetIDS(pResourceApi, pCVID, propiedadesItem);

            Dictionary<string, DisambiguableEntity> resultados = new Dictionary<string, DisambiguableEntity>();

            //Divido la lista en listas de elementos
            List<List<string>> listaListas = UtilitySecciones.SplitList(ids.ToList(), Utility.splitListNum).ToList();

            foreach (List<string> lista in listaListas)
            {
                string select = $@"SELECT distinct ?item ?itemTitle ?itemDate ?itemEA ";
                string where = $@"where {{
                                        ?item <{Variables.ActividadCientificaTecnologica.actividadInvestigadoraNumeroTramos}> ?itemTitle . 
                                        OPTIONAL{{?item <{Variables.ActividadCientificaTecnologica.actividadInvestigadoraFechaObtencion}> ?itemDate }} .
                                        OPTIONAL{{?item <{Variables.ActividadCientificaTecnologica.actividadInvestigadoraEntidadNombre}> ?itemEA }} .
                                        FILTER(?item in (<{string.Join(">,<", lista)}>))
                                    }}";

                SparqlObject resultData = pResourceApi.VirtuosoQuery(select, where, graph);
                foreach (Dictionary<string, Data> fila in resultData.results.bindings)
                {
                    PeriodosActividad periodosActividad = new PeriodosActividad
                    {
                        ID = fila["item"].value,
                        numTramos = fila["itemTitle"].value,
                        fecha = fila.ContainsKey("itemDate") ? fila["itemDate"].value : "",
                        entidadAcreditante = fila.ContainsKey("itemEA") ? fila["itemEA"].value : ""
                    };

                    resultados.Add(pResourceApi.GetShortGuid(fila["item"].value).ToString(), periodosActividad);
                }
            }

            return resultados;
        }
    }
}
