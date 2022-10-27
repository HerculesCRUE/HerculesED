using Gnoss.ApiWrapper;
using Gnoss.ApiWrapper.ApiModel;
using Hercules.CommonsEDMA.DisambiguationEngine.Models;
using System.Collections.Generic;
using System.Linq;
using Utils;
using static Gnoss.ApiWrapper.ApiModel.SparqlObject;

namespace ImportadorWebCV.Sincro.Secciones.ActividadCientificaSubclases
{
    class OtrasActividadesDivulgacion : DisambiguableEntity
    {
        public string tituloTrabajo { get; set; }
        public string fecha { get; set; }
        public string nombreEvento { get; set; }

        private static readonly DisambiguationDataConfig configTitulo = new(DisambiguationDataConfigType.equalsTitle, 0.8f);
        private static readonly DisambiguationDataConfig configFecha = new(DisambiguationDataConfigType.equalsItem, 0.5f, 0.5f);
        private static readonly DisambiguationDataConfig configNombreEvento = new(DisambiguationDataConfigType.equalsItem, 0.5f, 0.5f);

        public override List<DisambiguationData> GetDisambiguationData()
        {
            List<DisambiguationData> data = new()
            {
                new DisambiguationData(configTitulo,"tituloTrabajo",tituloTrabajo),
                new DisambiguationData(configFecha,"fecha",fecha),
                new DisambiguationData(configNombreEvento,"nombreEvento",nombreEvento)
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
                string select = $@"SELECT distinct ?item ?itemTitle ?itemDate ?itemNombre ";
                string where = $@"where {{
                                        ?item <{Variables.ActividadCientificaTecnologica.otrasActDivulTitulo}> ?itemTitle . 
                                        OPTIONAL{{ ?item <{Variables.ActividadCientificaTecnologica.otrasActDivulPubFecha}> ?itemDate }} .
                                        OPTIONAL{{ ?item <{Variables.ActividadCientificaTecnologica.otrasActDivulNombreEvento}> ?itemNombre }} .
                                        FILTER(?item in (<{string.Join(">,<", lista)}>))
                                    }}";

                SparqlObject resultData = pResourceApi.VirtuosoQuery(select, where, graph);
                foreach (Dictionary<string, Data> fila in resultData.results.bindings)
                {
                    OtrasActividadesDivulgacion otrasActividades = new OtrasActividadesDivulgacion
                    {
                        ID = fila["item"].value,
                        tituloTrabajo = fila["itemTitle"].value,
                        fecha = fila.ContainsKey("itemDate") ? fila["itemDate"].value : "",
                        nombreEvento = fila.ContainsKey("itemNombre") ? fila["itemNombre"].value : ""
                    };

                    resultados.Add(pResourceApi.GetShortGuid(fila["item"].value).ToString(), otrasActividades);
                }
            }

            return resultados;
        }
    }
}
