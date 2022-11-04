using Gnoss.ApiWrapper;
using Gnoss.ApiWrapper.ApiModel;
using Hercules.CommonsEDMA.DisambiguationEngine.Models;
using System.Collections.Generic;
using System.Linq;
using Utils;
using static Gnoss.ApiWrapper.ApiModel.SparqlObject;


namespace ImportadorWebCV.Sincro.Secciones.ExperienciaCientificaSubclases
{
    class GrupoIDI : DisambiguableEntity
    {
        public string Descripcion { get; set; }
        public string Fecha { get; set; }

        private static readonly DisambiguationDataConfig configDescripcionGruIdi = new(DisambiguationDataConfigType.equalsTitle, 0.8f);
        private static readonly DisambiguationDataConfig configFechaGruIdi = new(DisambiguationDataConfigType.equalsItem, 0.5f);

        public override List<DisambiguationData> GetDisambiguationData()
        {
            List<DisambiguationData> data = new()
            {
                new DisambiguationData(configDescripcionGruIdi, "descripcion", Descripcion),
                new DisambiguationData(configFechaGruIdi, "fecha", Fecha)
            };
            return data;
        }

        public static Dictionary<string, DisambiguableEntity> GetBBDDGruIdi(ResourceApi pResourceApi, string pCVID, string graph, List<string> propiedadesItem)
        {
            //Obtenemos IDS
            HashSet<string> ids = UtilitySecciones.GetIDS(pResourceApi, pCVID, propiedadesItem);

            Dictionary<string, DisambiguableEntity> resultadosGruIdi = new();

            //Divido la lista en listas de elementos
            List<List<string>> listaListas = UtilitySecciones.SplitList(ids.ToList(), Utility.splitListNum).ToList();

            foreach (List<string> lista in listaListas)
            {
                string select = $@"SELECT distinct ?item ?itemTitle ?itemDate ?isValidated";
                string where = $@"where {{
                                        ?item <{Variables.ExperienciaCientificaTecnologica.grupoIDINombreGrupo}> ?itemTitle . 
                                        ?item <http://w3id.org/roh/isValidated> ?isValidated . 
                                        OPTIONAL{{?item <{Variables.ExperienciaCientificaTecnologica.grupoIDIFechaInicio}> ?itemDate }}.
                                        FILTER(?item in (<{string.Join(">,<", lista)}>))
                                    }}";

                SparqlObject resultData = pResourceApi.VirtuosoQuery(select, where, graph);
                foreach (Dictionary<string, Data> fila in resultData.results.bindings)
                {
                    GrupoIDI grupoIDI = new GrupoIDI
                    {
                        ID = fila["item"].value,
                        Descripcion = fila["itemTitle"].value,
                        Fecha = fila.ContainsKey("itemDate") ? fila["itemDate"].value : "",
                        block = fila["isValidated"].value.Equals("true")
                    };

                    resultadosGruIdi.Add(pResourceApi.GetShortGuid(fila["item"].value).ToString(), grupoIDI);
                }
            }

            return resultadosGruIdi;
        }
    }
}
