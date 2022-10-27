using Gnoss.ApiWrapper;
using Gnoss.ApiWrapper.ApiModel;
using Hercules.CommonsEDMA.DisambiguationEngine.Models;
using System.Collections.Generic;
using System.Linq;
using Utils;
using static Gnoss.ApiWrapper.ApiModel.SparqlObject;

namespace ImportadorWebCV.Sincro.Secciones.ExperienciaCientificaSubclases
{
    class ObrasArtisticas : DisambiguableEntity
    {
        public string descripcion { get; set; }
        public string nombre { get; set; }
        public string fecha { get; set; }

        private static readonly DisambiguationDataConfig configDescripcion = new(DisambiguationDataConfigType.equalsTitle, 0.8f);
        private static readonly DisambiguationDataConfig configNombre = new(DisambiguationDataConfigType.equalsItem, 0.5f, 0.5f);
        private static readonly DisambiguationDataConfig configFecha = new(DisambiguationDataConfigType.equalsItem, 0.5f, 0.5f);

        public override List<DisambiguationData> GetDisambiguationData()
        {
            List<DisambiguationData> data = new()
            {
                new DisambiguationData(configDescripcion, "descripcion", descripcion),
                new DisambiguationData(configNombre,"nombre",nombre),
                new DisambiguationData(configFecha,"fecha",fecha)
            };
            return data;
        }

        public static Dictionary<string, DisambiguableEntity> GetBBDD(ResourceApi pResourceApi, string pCVID, string graph, List<string> propiedadesItem)
        {
            //Obtenemos IDS
            HashSet<string> ids = UtilitySecciones.GetIDS(pResourceApi, pCVID, propiedadesItem);

            Dictionary<string, DisambiguableEntity> resultados = new ();

            //Divido la lista en listas de elementos
            List<List<string>> listaListas = UtilitySecciones.SplitList(ids.ToList(), Utility.splitListNum).ToList();

            foreach (List<string> lista in listaListas)
            {
                string select = $@"SELECT distinct ?item ?itemTitle ?itemName ?itemDate ";
                string where = $@"where {{
                                        ?item <{Variables.ExperienciaCientificaTecnologica.obrasArtisticasDescripcion}> ?itemTitle . 
                                        OPTIONAL{{?item <{Variables.ExperienciaCientificaTecnologica.obrasArtisticasNombreExpo}> ?itemName }} .
                                        OPTIONAL{{?item <{Variables.ExperienciaCientificaTecnologica.obrasArtisticasFechaInicio}> ?itemDate }} .
                                        FILTER(?item in (<{string.Join(">,<", lista)}>))
                                    }}";

                SparqlObject resultData = pResourceApi.VirtuosoQuery(select, where, graph);
                foreach (Dictionary<string, Data> fila in resultData.results.bindings)
                {
                    ObrasArtisticas obrasArtisticas = new ObrasArtisticas
                    {
                        ID = fila["item"].value,
                        descripcion = fila["itemTitle"].value,
                        nombre = fila.ContainsKey("itemName") ? fila["itemName"].value : "",
                        fecha = fila.ContainsKey("itemDate") ? fila["itemDate"].value : ""
                    };

                    resultados.Add(pResourceApi.GetShortGuid(fila["item"].value).ToString(), obrasArtisticas);
                }
            }

            return resultados;
        }
    }
}
