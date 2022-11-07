using Gnoss.ApiWrapper;
using Gnoss.ApiWrapper.ApiModel;
using Hercules.CommonsEDMA.DisambiguationEngine.Models;
using System.Collections.Generic;
using System.Linq;
using Utils;
using static Gnoss.ApiWrapper.ApiModel.SparqlObject;


namespace ImportadorWebCV.Sincro.Secciones.ActividadDocenteSubclases
{
    class AportacionesRelevantes : DisambiguableEntity
    {
        public string Descripcion { get; set; }
        public string EntidadOrganizadora { get; set; }
        public string Fecha { get; set; }

        private static readonly DisambiguationDataConfig configDescripcionApoRel = new(DisambiguationDataConfigType.equalsTitle, 0.8f);
        private static readonly DisambiguationDataConfig configFechaApoRel = new(DisambiguationDataConfigType.equalsItem, 0.5f, 0.5f);
        private static readonly DisambiguationDataConfig configEntOrgApoRel = new(DisambiguationDataConfigType.equalsItem, 0.5f, 0.5f);

        public override List<DisambiguationData> GetDisambiguationData()
        {
            List<DisambiguationData> data = new()
            {
                new DisambiguationData(configDescripcionApoRel, "descripcion", Descripcion),
                new DisambiguationData(configFechaApoRel, "fecha", Fecha),
                new DisambiguationData(configEntOrgApoRel, "entidadOrganizadora", EntidadOrganizadora)
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
        public static Dictionary<string, DisambiguableEntity> GetBBDDApoRel(ResourceApi pResourceApi, string pCVID, string graph, List<string> propiedadesItem)
        {
            //Obtenemos IDS
            HashSet<string> ids = UtilitySecciones.GetIDS(pResourceApi, pCVID, propiedadesItem);

            Dictionary<string, DisambiguableEntity> resultadosApoRel = new();

            //Divido la lista en listas de elementos
            List<List<string>> listaListas = UtilitySecciones.SplitList(ids.ToList(), Utility.splitListNum).ToList();

            foreach (List<string> lista in listaListas)
            {
                string select = $@"SELECT distinct ?item ?itemTitle ?itemDate  ?itemEO ";
                string where = $@"where {{
                                        ?item <{Variables.ActividadDocente.aportacionesCVDescripcion}> ?itemTitle . 
                                        OPTIONAL{{?item <{Variables.ActividadDocente.aportacionesCVFechaFinalizacion}> ?itemDate }}.
                                        OPTIONAL{{?item <{Variables.ActividadDocente.aportacionesCVEntidadOrganizadoraNombre}> ?itemEO }}.
                                        FILTER(?item in (<{string.Join(">,<", lista)}>))
                                    }}";
                SparqlObject resultData = pResourceApi.VirtuosoQuery(select, where, graph);
                foreach (Dictionary<string, Data> fila in resultData.results.bindings)
                {
                    AportacionesRelevantes aportacionesRelevantes = new()
                    {
                        ID = fila["item"].value,
                        Descripcion = fila["itemTitle"].value,
                        Fecha = fila.ContainsKey("itemDate") ? fila["itemDate"].value : "",
                        EntidadOrganizadora = fila.ContainsKey("itemEO") ? fila["itemEO"].value : ""
                    };

                    resultadosApoRel.Add(pResourceApi.GetShortGuid(fila["item"].value).ToString(), aportacionesRelevantes);
                }
            }

            return resultadosApoRel;
        }
    }
}
