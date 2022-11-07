using Gnoss.ApiWrapper;
using Gnoss.ApiWrapper.ApiModel;
using Hercules.CommonsEDMA.DisambiguationEngine.Models;
using System.Collections.Generic;
using System.Linq;
using Utils;
using static Gnoss.ApiWrapper.ApiModel.SparqlObject;

namespace ImportadorWebCV.Sincro.Secciones.ActividadCientificaSubclases
{
    class PeriodosActividad : DisambiguableEntity
    {
        public string NumTramos { get; set; }
        public string Fecha { get; set; }
        public string EntidadAcreditante { get; set; }

        private static readonly DisambiguationDataConfig configDescripcionPerAct = new(DisambiguationDataConfigType.equalsTitle, 0.8f);
        private static readonly DisambiguationDataConfig configFechaPerAct = new(DisambiguationDataConfigType.equalsItem, 0.5f, 0.5f);
        private static readonly DisambiguationDataConfig configEntActPerAct = new DisambiguationDataConfig(DisambiguationDataConfigType.equalsItem, 0.5f, 0.5f);

        public override List<DisambiguationData> GetDisambiguationData()
        {
            List<DisambiguationData> data = new()
            {
                new DisambiguationData(configDescripcionPerAct, "numTramos", NumTramos),
                new DisambiguationData(configFechaPerAct, "fecha", Fecha),
                new DisambiguationData(configEntActPerAct, "entidadAcreditacion", EntidadAcreditante)
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
        public static Dictionary<string, DisambiguableEntity> GetBBDDPerAct(ResourceApi pResourceApi, string pCVID, string graph, List<string> propiedadesItem)
        {
            //Obtenemos IDS
            HashSet<string> ids = UtilitySecciones.GetIDS(pResourceApi, pCVID, propiedadesItem);

            Dictionary<string, DisambiguableEntity> resultadosPerAct = new();

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
                        NumTramos = fila["itemTitle"].value,
                        Fecha = fila.ContainsKey("itemDate") ? fila["itemDate"].value : "",
                        EntidadAcreditante = fila.ContainsKey("itemEA") ? fila["itemEA"].value : ""
                    };

                    resultadosPerAct.Add(pResourceApi.GetShortGuid(fila["item"].value).ToString(), periodosActividad);
                }
            }

            return resultadosPerAct;
        }
    }
}
