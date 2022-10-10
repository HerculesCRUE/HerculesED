using Gnoss.ApiWrapper;
using Gnoss.ApiWrapper.ApiModel;
using Hercules.CommonsEDMA.DisambiguationEngine.Models;
using System.Collections.Generic;
using System.Linq;
using Utils;
using static Gnoss.ApiWrapper.ApiModel.SparqlObject;

namespace ImportadorWebCV.Sincro.Secciones.SituacionProfesionalSubclases
{
    class SituacionProfesionalActual : DisambiguableEntity
    {
        public string nombre { get; set; }
        public string categoria { get; set; }

        private static readonly DisambiguationDataConfig configNombre = new DisambiguationDataConfig()
        {
            type = DisambiguationDataConfigType.equalsTitle,
            score = 0.8f
        };

        private static readonly DisambiguationDataConfig configCategoria = new DisambiguationDataConfig()
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
                    property = "nombre",
                    config = configNombre,
                    value = nombre
                },

                new DisambiguationData()
                {
                    property = "categoria",
                    config = configCategoria,
                    value = categoria
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
                string select = $@"SELECT distinct ?item ?itemTitle ?itemCategoria";
                string where = $@"where {{
                                        ?item <{Variables.SituacionProfesional.situacionProfesionalEntidadEmpleadoraNombre}> ?itemTitle . 
                                        OPTIONAL{{?item <{Variables.SituacionProfesional.situacionProfesionalCategoriaProfesional}> ?itemCategoria }} .
                                        FILTER(?item in (<{string.Join(">,<", lista)}>))
                                    }}";

                SparqlObject resultData = pResourceApi.VirtuosoQuery(select, where, graph);
                foreach (Dictionary<string, Data> fila in resultData.results.bindings)
                {
                    SituacionProfesionalActual situacion = new SituacionProfesionalActual
                    {
                        ID = fila["item"].value,
                        nombre = fila["itemTitle"].value,
                        categoria = fila.ContainsKey("itemCategoria") ? fila["itemCategoria"].value : ""
                    };

                    resultados.Add(pResourceApi.GetShortGuid(fila["item"].value).ToString(), situacion);
                }
            }

            return resultados;
        }
    }
}
