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
        public string Descripcion { get; set; }
        public string Nombre { get; set; }
        public string Fecha { get; set; }

        private static readonly DisambiguationDataConfig configDescripcionObraArt = new(DisambiguationDataConfigType.equalsTitle, 0.8f);
        private static readonly DisambiguationDataConfig configNombreObraArt = new(DisambiguationDataConfigType.equalsItem, 0.5f, 0.5f);
        private static readonly DisambiguationDataConfig configFechaObraArt = new(DisambiguationDataConfigType.equalsItem, 0.5f, 0.5f);

        public override List<DisambiguationData> GetDisambiguationData()
        {
            List<DisambiguationData> data = new()
            {
                new DisambiguationData(configDescripcionObraArt, "descripcion", Descripcion),
                new DisambiguationData(configNombreObraArt,"nombre",Nombre),
                new DisambiguationData(configFechaObraArt,"fecha",Fecha)
            };
            return data;
        }

        public static Dictionary<string, DisambiguableEntity> GetBBDDObraArt(ResourceApi pResourceApi, string pCVID, string graph, List<string> propiedadesItem)
        {
            //Obtenemos IDS
            HashSet<string> ids = UtilitySecciones.GetIDS(pResourceApi, pCVID, propiedadesItem);

            Dictionary<string, DisambiguableEntity> resultadosObraArt = new ();

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
                        Descripcion = fila["itemTitle"].value,
                        Nombre = fila.ContainsKey("itemName") ? fila["itemName"].value : "",
                        Fecha = fila.ContainsKey("itemDate") ? fila["itemDate"].value : ""
                    };

                    resultadosObraArt.Add(pResourceApi.GetShortGuid(fila["item"].value).ToString(), obrasArtisticas);
                }
            }

            return resultadosObraArt;
        }
    }
}
