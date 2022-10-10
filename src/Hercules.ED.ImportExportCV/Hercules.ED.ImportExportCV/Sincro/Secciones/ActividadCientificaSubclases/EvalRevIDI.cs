using Gnoss.ApiWrapper;
using Gnoss.ApiWrapper.ApiModel;
using Hercules.CommonsEDMA.DisambiguationEngine.Models;
using System.Collections.Generic;
using System.Linq;
using Utils;
using static Gnoss.ApiWrapper.ApiModel.SparqlObject;

namespace ImportadorWebCV.Sincro.Secciones.ActividadCientificaSubclases
{
    class EvalRevIDI : DisambiguableEntity
    {
        public string descripcion { get; set; }
        public string nombreActividad { get; set; }
        public string entidadRealizacion { get; set; }
        public string fechaInicio { get; set; }

        private static readonly DisambiguationDataConfig configDescripcion = new DisambiguationDataConfig()
        {
            type = DisambiguationDataConfigType.equalsTitle,
            score = 0.8f
        };

        private static readonly DisambiguationDataConfig configNomAct = new DisambiguationDataConfig()
        {
            type = DisambiguationDataConfigType.equalsItem,
            score = 0.5f,
            scoreMinus = 0.5f
        };

        private static readonly DisambiguationDataConfig configFechaIni = new DisambiguationDataConfig()
        {
            type = DisambiguationDataConfigType.equalsItem,
            score = 0.5f,
            scoreMinus = 0.5f
        };
        private static readonly DisambiguationDataConfig configEntidadRealizacion = new DisambiguationDataConfig()
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
                    property = "nombreActividad",
                    config = configNomAct,
                    value = nombreActividad
                },

                new DisambiguationData()
                {
                    property = "fechaInicio",
                    config = configFechaIni,
                    value = fechaInicio
                },

                new DisambiguationData()
                {
                    property = "entidadRealizacion",
                    config = configEntidadRealizacion,
                    value = entidadRealizacion
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
                string select = $@"SELECT distinct ?item ?itemTitle ?itemNombre ?itemFecha ?itemER ";
                string where = $@"where {{
                                        ?item <{Variables.ActividadCientificaTecnologica.evalRevIDIFunciones}> ?itemTitle . 
                                        OPTIONAL{{ ?item <{Variables.ActividadCientificaTecnologica.evalRevIDINombre}> ?itemNombre }} .
                                        OPTIONAL{{ ?item <{Variables.ActividadCientificaTecnologica.evalRevIDIFechaInicio}> ?itemFecha }} .
                                        OPTIONAL{{ ?item <{Variables.ActividadCientificaTecnologica.evalRevIDIEntidadNombre}> ?itemER }} .
                                        FILTER(?item in (<{string.Join(">,<", lista)}>))
                                    }}";

                SparqlObject resultData = pResourceApi.VirtuosoQuery(select, where, graph);
                foreach (Dictionary<string, Data> fila in resultData.results.bindings)
                {
                    EvalRevIDI evalRevIDI = new EvalRevIDI
                    {
                        ID = fila["item"].value,
                        descripcion = fila["itemTitle"].value,
                        nombreActividad = fila.ContainsKey("itemNombre") ? fila["itemNombre"].value : "",
                        fechaInicio = fila.ContainsKey("itemFecha") ? fila["itemFecha"].value : "",
                        entidadRealizacion = fila.ContainsKey("itemER") ? fila["itemER"].value : ""
                    };

                    resultados.Add(pResourceApi.GetShortGuid(fila["item"].value).ToString(), evalRevIDI);
                }
            }

            return resultados;
        }
    }
}
