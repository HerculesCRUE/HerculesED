using Gnoss.ApiWrapper;
using Gnoss.ApiWrapper.ApiModel;
using Hercules.CommonsEDMA.DisambiguationEngine.Models;
using System.Collections.Generic;
using System.Linq;
using Utils;
using static Gnoss.ApiWrapper.ApiModel.SparqlObject;

namespace ImportadorWebCV.Sincro.Secciones.ExperienciaCientificaSubclases
{
    class PropiedadIndustrialIntelectual : DisambiguableEntity
    {
        public string descripcion { get; set; }
        public string fecha { get; set; }
        public string entidadTitular { get; set; }

        private static readonly DisambiguationDataConfig configDescripcion = new(DisambiguationDataConfigType.equalsTitle, 0.8f);
        private static readonly DisambiguationDataConfig configFecha = new(DisambiguationDataConfigType.equalsItem, 0.5f);
        private static readonly DisambiguationDataConfig configET = new(DisambiguationDataConfigType.equalsItem, 0.5f, 0.5f);

        public override List<DisambiguationData> GetDisambiguationData()
        {
            List<DisambiguationData> data = new()
            {
                new DisambiguationData(configDescripcion,"descripcion",descripcion),
                new DisambiguationData(configFecha,"fecha",fecha),
                new DisambiguationData(configET,"entidadTitular",entidadTitular)
            };
            return data;
        }

        public static Dictionary<string, DisambiguableEntity> GetBBDD(ResourceApi pResourceApi, string pCVID, string graph, List<string> propiedadesItem)
        {
            //Obtenemos IDS
            HashSet<string> ids = UtilitySecciones.GetIDS(pResourceApi, pCVID, propiedadesItem);

            Dictionary<string, DisambiguableEntity> resultados = new Dictionary<string, DisambiguableEntity>();

            //Divido la lista en listas de elementos
            List<List<string>> listaListas = UtilitySecciones.SplitList(ids.ToList(), Utility.splitListNum).ToList();

            foreach (List<string> lista in listaListas)
            {
                string select = $@"SELECT distinct ?item ?itemTitle ?itemDate ?itemET ";
                string where = $@"where {{
                                        ?item <{Variables.ExperienciaCientificaTecnologica.propIITituloPropIndus}> ?itemTitle . 
                                        OPTIONAL{{?item <{Variables.ExperienciaCientificaTecnologica.propIIFechaConcesion}> ?itemDate }} .
                                        OPTIONAL{{?item <{Variables.ExperienciaCientificaTecnologica.propIIEntidadTitularDerechosNombre}> ?itemET }} .
                                        FILTER(?item in (<{string.Join(">,<", lista)}>))
                                    }}";
                SparqlObject resultData = pResourceApi.VirtuosoQuery(select, where, graph);
                foreach (Dictionary<string, Data> fila in resultData.results.bindings)
                {
                    PropiedadIndustrialIntelectual propII = new ()
                    {
                        ID = fila["item"].value,
                        descripcion = fila["itemTitle"].value,
                        fecha = fila.ContainsKey("itemDate") ? fila["itemDate"].value : "",
                        entidadTitular = fila.ContainsKey("itemET") ? fila["itemET"].value : ""
                    };

                    resultados.Add(pResourceApi.GetShortGuid(fila["item"].value).ToString(), propII);
                }
            }

            return resultados;
        }
    }
}
