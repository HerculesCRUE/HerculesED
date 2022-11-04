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
        public string Descripcion { get; set; }
        public string Fecha { get; set; }
        public string EntidadTitular { get; set; }

        private static readonly DisambiguationDataConfig configDescripcionPropIndInt = new(DisambiguationDataConfigType.equalsTitle, 0.8f);
        private static readonly DisambiguationDataConfig configFechaPropIndInt = new(DisambiguationDataConfigType.equalsItem, 0.5f);
        private static readonly DisambiguationDataConfig configEntTitPropIndInt = new(DisambiguationDataConfigType.equalsItem, 0.5f, 0.5f);

        public override List<DisambiguationData> GetDisambiguationData()
        {
            List<DisambiguationData> data = new()
            {
                new DisambiguationData(configDescripcionPropIndInt, "descripcion", Descripcion),
                new DisambiguationData(configFechaPropIndInt, "fecha", Fecha),
                new DisambiguationData(configEntTitPropIndInt, "entidadTitular", EntidadTitular)
            };
            return data;
        }

        public static Dictionary<string, DisambiguableEntity> GetBBDDPropIndInt(ResourceApi pResourceApi, string pCVID, string graph, List<string> propiedadesItem)
        {
            //Obtenemos IDS
            HashSet<string> ids = UtilitySecciones.GetIDS(pResourceApi, pCVID, propiedadesItem);

            Dictionary<string, DisambiguableEntity> resultadosPropIndInt = new();

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
                    PropiedadIndustrialIntelectual propII = new()
                    {
                        ID = fila["item"].value,
                        Descripcion = fila["itemTitle"].value,
                        Fecha = fila.ContainsKey("itemDate") ? fila["itemDate"].value : "",
                        EntidadTitular = fila.ContainsKey("itemET") ? fila["itemET"].value : ""
                    };

                    resultadosPropIndInt.Add(pResourceApi.GetShortGuid(fila["item"].value).ToString(), propII);
                }
            }

            return resultadosPropIndInt;
        }
    }
}
