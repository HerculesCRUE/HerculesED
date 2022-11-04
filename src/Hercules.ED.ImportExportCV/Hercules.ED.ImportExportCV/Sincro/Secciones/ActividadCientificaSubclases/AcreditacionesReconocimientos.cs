using Gnoss.ApiWrapper;
using Gnoss.ApiWrapper.ApiModel;
using Hercules.CommonsEDMA.DisambiguationEngine.Models;
using System.Collections.Generic;
using System.Linq;
using Utils;
using static Gnoss.ApiWrapper.ApiModel.SparqlObject;

namespace ImportadorWebCV.Sincro.Secciones.ActividadCientificaSubclases
{
    class AcreditacionesReconocimientos : DisambiguableEntity
    {
        public string Descripcion { get; set; }
        public string Fecha { get; set; }
        public string NombreEntAcreditante { get; set; }

        private static readonly DisambiguationDataConfig configDescripcionAcRe = new(DisambiguationDataConfigType.equalsTitle, 0.8f);
        private static readonly DisambiguationDataConfig configFechaAcRe = new(DisambiguationDataConfigType.equalsItem, 0.5f, 0.5f);
        private static readonly DisambiguationDataConfig configEAAcRe = new(DisambiguationDataConfigType.equalsItem, 0.5f, 0.5f);

        public override List<DisambiguationData> GetDisambiguationData()
        {
            List<DisambiguationData> data = new()
            {
                new DisambiguationData(configDescripcionAcRe, "descripcion", Descripcion),
                new DisambiguationData(configFechaAcRe, "fecha", Fecha),
                new DisambiguationData(configEAAcRe, "entidadAcreditante", NombreEntAcreditante)
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
        public static Dictionary<string, DisambiguableEntity> GetBBDDAcRe(ResourceApi pResourceApi, string pCVID, string graph, List<string> propiedadesItem)
        {
            //Obtenemos IDS
            HashSet<string> ids = UtilitySecciones.GetIDS(pResourceApi, pCVID, propiedadesItem);

            Dictionary<string, DisambiguableEntity> resultadosAcRe = new();

            //Divido la lista en listas de elementos
            List<List<string>> listaListas = UtilitySecciones.SplitList(ids.ToList(), Utility.splitListNum).ToList();

            foreach (List<string> lista in listaListas)
            {
                string select = $@"SELECT distinct ?item ?itemTitle ?itemDate ?itemEA ";
                string where = $@"where {{
                                        ?item <{Variables.ActividadCientificaTecnologica.acreditacionesDescripcion}> ?itemTitle . 
                                        OPTIONAL{{?item <{Variables.ActividadCientificaTecnologica.acreditacionesFechaObtencion}> ?itemDate }}.
                                        OPTIONAL{{?item <{Variables.ActividadCientificaTecnologica.acreditacionesEntidadNombre}> ?itemEA }}.
                                        FILTER(?item in (<{string.Join(">,<", lista)}>))
                                    }}";

                SparqlObject resultData = pResourceApi.VirtuosoQuery(select, where, graph);
                foreach (Dictionary<string, Data> fila in resultData.results.bindings)
                {
                    AcreditacionesReconocimientos acreditacionesReconocimientos = new AcreditacionesReconocimientos
                    {
                        ID = fila["item"].value,
                        Descripcion = fila["itemTitle"].value,
                        Fecha = fila.ContainsKey("itemDate") ? fila["itemDate"].value : "",
                        NombreEntAcreditante = fila.ContainsKey("itemEA") ? fila["itemEA"].value : ""
                    };

                    resultadosAcRe.Add(pResourceApi.GetShortGuid(fila["item"].value).ToString(), acreditacionesReconocimientos);
                }
            }

            return resultadosAcRe;
        }
    }
}
