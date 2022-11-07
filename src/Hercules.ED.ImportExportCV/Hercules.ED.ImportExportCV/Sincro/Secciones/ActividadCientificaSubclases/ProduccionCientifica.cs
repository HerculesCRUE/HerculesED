using Gnoss.ApiWrapper;
using Gnoss.ApiWrapper.ApiModel;
using Hercules.CommonsEDMA.DisambiguationEngine.Models;
using System.Collections.Generic;
using System.Linq;
using Utils;
using static Gnoss.ApiWrapper.ApiModel.SparqlObject;

namespace ImportadorWebCV.Sincro.Secciones.ActividadCientificaSubclases
{
    class ProduccionCientifica : DisambiguableEntity
    {
        public string FuenteH { get; set; }
        public string FuenteHOtros { get; set; }

        private static readonly DisambiguationDataConfig configFuenteHProCie = new(DisambiguationDataConfigType.equalsTitle, 0.8f);
        private static readonly DisambiguationDataConfig configFuenteHOtrosProCie = new(DisambiguationDataConfigType.equalsItem, 0.5f, 0.5f);

        public override List<DisambiguationData> GetDisambiguationData()
        {
            List<DisambiguationData> data = new()
            {
                new DisambiguationData(configFuenteHProCie, "fuenteIndiceH", FuenteH),
                new DisambiguationData(configFuenteHOtrosProCie, "fuenteIndiceHOtros", FuenteHOtros)
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
        public static Dictionary<string, DisambiguableEntity> GetBBDDProCie(ResourceApi pResourceApi, string pCVID, string graph, List<string> propiedadesItem)
        {
            //Obtenemos IDS
            HashSet<string> ids = UtilitySecciones.GetIDS(pResourceApi, pCVID, propiedadesItem);

            Dictionary<string, DisambiguableEntity> resultadosProCie = new();

            //Divido la lista en listas de elementos
            List<List<string>> listaListas = UtilitySecciones.SplitList(ids.ToList(), Utility.splitListNum).ToList();

            foreach (List<string> lista in listaListas)
            {
                string select = $@"SELECT distinct ?item ?itemTitle ?itemFuenteH ?itemFuenteHOtros";
                string where = $@"where {{
                                        ?item <{Variables.ActividadCientificaTecnologica.prodCientificaIndiceH}> ?itemTitle . 
                                        OPTIONAL{{?item <{Variables.ActividadCientificaTecnologica.prodCientificaFuenteIndiceH}> ?itemFuenteH }}. 
                                        OPTIONAL{{?item <{Variables.ActividadCientificaTecnologica.prodCientificaFuenteIndiceHOtros}> ?itemFuenteHOtros }}. 
                                        FILTER(?item in (<{string.Join(">,<", lista)}>))
                                    }}";

                SparqlObject resultData = pResourceApi.VirtuosoQuery(select, where, graph);
                foreach (Dictionary<string, Data> fila in resultData.results.bindings)
                {
                    ProduccionCientifica produccionCientifica = new ProduccionCientifica
                    {
                        ID = fila["item"].value,
                        FuenteH = fila.ContainsKey("itemFuenteH") ? fila["itemFuenteH"].value : "",
                        FuenteHOtros = fila.ContainsKey("itemFuenteHOtros") ? fila["itemFuenteHOtros"].value : ""
                    };

                    resultadosProCie.Add(pResourceApi.GetShortGuid(fila["item"].value).ToString(), produccionCientifica);
                }
            }

            return resultadosProCie;
        }
    }
}
