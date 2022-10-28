using Gnoss.ApiWrapper;
using Gnoss.ApiWrapper.ApiModel;
using Hercules.CommonsEDMA.DisambiguationEngine.Models;
using Utils;
using System.Collections.Generic;
using System.Linq;
using static Gnoss.ApiWrapper.ApiModel.SparqlObject;

namespace ImportadorWebCV.Sincro.Secciones.ActividadDocenteSubclases
{
    class ParticipacionProyectosInnovacionDocente : DisambiguableEntity
    {
        public string Titulo { get; set; }
        public string Fecha { get; set; }
        public string EntidadFinanciadora { get; set; }

        private static readonly DisambiguationDataConfig configTituloPaPrInnDo = new(DisambiguationDataConfigType.equalsTitle, 0.8f);

        private static readonly DisambiguationDataConfig configFechaPaPrInnDo = new(DisambiguationDataConfigType.equalsItem, 0.5f, 0.5f);

        private static readonly DisambiguationDataConfig configEntFinPaPrInnDo = new(DisambiguationDataConfigType.equalsItem, 0.5f, 0.5f);

        public override List<DisambiguationData> GetDisambiguationData()
        {
            List<DisambiguationData> data = new()
            {
                new DisambiguationData(configTituloPaPrInnDo,"titulo",Titulo),
                new DisambiguationData(configFechaPaPrInnDo,"fecha",Fecha),
                new DisambiguationData(configEntFinPaPrInnDo,"entidadFinanciadora",EntidadFinanciadora)
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
        public static Dictionary<string, DisambiguableEntity> GetBBDDPaPrInnDo(ResourceApi pResourceApi, string pCVID, string graph, List<string> propiedadesItem)
        {
            //Obtenemos IDS
            HashSet<string> ids = UtilitySecciones.GetIDS(pResourceApi, pCVID, propiedadesItem);

            Dictionary<string, DisambiguableEntity> resultadosPaPrInnDo = new ();

            //Divido la lista en listas de elementos
            List<List<string>> listaListas = UtilitySecciones.SplitList(ids.ToList(), Utility.splitListNum).ToList();

            foreach (List<string> lista in listaListas)
            {
                string select = $@"SELECT distinct ?item ?itemTitle ?itemDate ?itemEF ";
                string where = $@"where {{
                                        ?item <{Variables.ActividadDocente.participacionInnovaTitulo}> ?itemTitle . 
                                        OPTIONAL{{?item <{Variables.ActividadDocente.participacionInnovaFechaInicio}> ?itemDate }}.
                                        OPTIONAL{{?item <{Variables.ActividadDocente.participacionInnovaEntidadFinanciadoraNombre}> ?itemEF }}.
                                        FILTER(?item in (<{string.Join(">,<", lista)}>))
                                    }}";
                SparqlObject resultData = pResourceApi.VirtuosoQuery(select, where, graph);
                foreach (Dictionary<string, Data> fila in resultData.results.bindings)
                {
                    ParticipacionProyectosInnovacionDocente participacionProyectos = new ParticipacionProyectosInnovacionDocente
                    {
                        ID = fila["item"].value,
                        Titulo = fila["itemTitle"].value,
                        Fecha = fila.ContainsKey("itemDate") ? fila["itemDate"].value : "",
                        EntidadFinanciadora = fila.ContainsKey("itemEF") ? fila["itemEF"].value : ""
                    };

                    resultadosPaPrInnDo.Add(pResourceApi.GetShortGuid(fila["item"].value).ToString(), participacionProyectos);
                }
            }

            return resultadosPaPrInnDo;
        }
    }
}
