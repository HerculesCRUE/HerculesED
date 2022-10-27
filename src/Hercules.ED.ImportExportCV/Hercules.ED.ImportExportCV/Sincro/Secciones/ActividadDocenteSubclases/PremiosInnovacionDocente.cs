using Gnoss.ApiWrapper;
using Gnoss.ApiWrapper.ApiModel;
using Hercules.CommonsEDMA.DisambiguationEngine.Models;
using Utils;
using System.Collections.Generic;
using System.Linq;
using static Gnoss.ApiWrapper.ApiModel.SparqlObject;

namespace ImportadorWebCV.Sincro.Secciones.ActividadDocenteSubclases
{
    class PremiosInnovacionDocente : DisambiguableEntity
    {
        public string nombre { get; set; }
        public string fecha { get; set; }
        public string entidadConcesion { get; set; }

        private static readonly DisambiguationDataConfig configNombre = new(DisambiguationDataConfigType.equalsTitle, 0.8f);
        private static readonly DisambiguationDataConfig configFecha = new(DisambiguationDataConfigType.equalsItem, 0.5f, 0.5f);
        private static readonly DisambiguationDataConfig configEC = new(DisambiguationDataConfigType.equalsItem, 0.5f, 0.5f);

        public override List<DisambiguationData> GetDisambiguationData()
        {
            List<DisambiguationData> data = new()
            {
                new DisambiguationData(configNombre,"descripcion",nombre),
                new DisambiguationData(configFecha,"fecha",fecha),
                new DisambiguationData(configEC, "entidadConcesion",entidadConcesion)
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
                string select = $@"SELECT distinct ?item ?itemTitle ?itemDate ?itemEC ";
                string where = $@"where {{
                                        ?item <{Variables.ActividadDocente.premiosInnovaNombre}> ?itemTitle . 
                                        OPTIONAL{{?item <{Variables.ActividadDocente.premiosInnovaFechaConcesion}> ?itemDate }}.
                                        OPTIONAL{{?item <{Variables.ActividadDocente.premiosInnovaEntidadConcesionariaNombre}> ?itemEC }}.
                                        FILTER(?item in (<{string.Join(">,<", lista)}>))
                                    }}";
                SparqlObject resultData = pResourceApi.VirtuosoQuery(select, where, graph);
                foreach (Dictionary<string, Data> fila in resultData.results.bindings)
                {
                    PremiosInnovacionDocente premiosInnovacion = new PremiosInnovacionDocente
                    {
                        ID = fila["item"].value,
                        nombre = fila["itemTitle"].value,
                        fecha = fila.ContainsKey("itemDate") ? fila["itemDate"].value : "",
                        entidadConcesion = fila.ContainsKey("itemEC") ? fila["itemEC"].value : ""
                    };

                    resultados.Add(pResourceApi.GetShortGuid(fila["item"].value).ToString(), premiosInnovacion);
                }
            }

            return resultados;
        }
    }
}
