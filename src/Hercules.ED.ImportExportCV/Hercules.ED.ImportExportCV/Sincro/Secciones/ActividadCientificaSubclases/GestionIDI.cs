using Gnoss.ApiWrapper;
using Gnoss.ApiWrapper.ApiModel;
using Hercules.CommonsEDMA.DisambiguationEngine.Models;
using System.Collections.Generic;
using System.Linq;
using Utils;
using static Gnoss.ApiWrapper.ApiModel.SparqlObject;

namespace ImportadorWebCV.Sincro.Secciones.ActividadCientificaSubclases
{
    class GestionIDI : DisambiguableEntity
    {
        public string descripcion { get; set; }
        public string funciones { get; set; }
        public string entidadRealizacion { get; set; }
        public string fecha { get; set; }

        private static readonly DisambiguationDataConfig configDescripcion = new DisambiguationDataConfig()
        {
            type = DisambiguationDataConfigType.equalsTitle,
            score = 0.8f
        };

        private static readonly DisambiguationDataConfig configER = new DisambiguationDataConfig()
        {
            type = DisambiguationDataConfigType.equalsItem,
            score = 0.5f,
            scoreMinus = 0.5f
        };

        private static readonly DisambiguationDataConfig configFunciones = new DisambiguationDataConfig()
        {
            type = DisambiguationDataConfigType.equalsItem,
            score = 0.5f,
            scoreMinus = 0.5f
        };

        private static readonly DisambiguationDataConfig configFecha = new DisambiguationDataConfig()
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
                    property = "descripcion",
                    config = configDescripcion,
                    value = descripcion
                },

                new DisambiguationData()
                {
                    property = "entidadRealizacion",
                    config = configER,
                    value = entidadRealizacion
                },

                new DisambiguationData()
                {
                    property = "funciones",
                    config = configFunciones,
                    value = funciones
                },

                new DisambiguationData()
                {
                    property = "fecha",
                    config = configFecha,
                    value = fecha
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
                string select = $@"SELECT distinct ?item ?itemTitle ?itemFunciones ?itemER ?itemDate ";
                string where = $@"where {{
                                        ?item <{Variables.ActividadCientificaTecnologica.gestionIDINombreActividad}> ?itemTitle . 
                                        OPTIONAL{{ ?item <{Variables.ActividadCientificaTecnologica.gestionIDIFunciones}> ?itemFunciones }} .
                                        OPTIONAL{{ ?item <{Variables.ActividadCientificaTecnologica.gestionIDIEntornoEntidadRealizacionNombre}> ?itemER}} .
                                        OPTIONAL{{ ?item <{Variables.ActividadCientificaTecnologica.gestionIDIEntornoFechaInicio}> ?itemDate }} .
                                        FILTER(?item in (<{string.Join(">,<", lista)}>))
                                    }}";

                SparqlObject resultData = pResourceApi.VirtuosoQuery(select, where, graph);
                foreach (Dictionary<string, Data> fila in resultData.results.bindings)
                {
                    GestionIDI gestionIDI = new GestionIDI
                    {
                        ID = fila["item"].value,
                        descripcion = fila["itemTitle"].value,
                        funciones = fila.ContainsKey("itemFunciones") ? fila["itemFunciones"].value : "",
                        entidadRealizacion = fila.ContainsKey("itemER") ? fila["itemER"].value : "",
                        fecha = fila.ContainsKey("itemDate") ? fila["itemDate"].value : ""
                };

                resultados.Add(pResourceApi.GetShortGuid(fila["item"].value).ToString(), gestionIDI);
            }
        }

            return resultados;
        }
}
}
