using Gnoss.ApiWrapper;
using Gnoss.ApiWrapper.ApiModel;
using Hercules.ED.DisambiguationEngine.Models;
using Utils;
using System.Collections.Generic;
using System.Linq;
using static Gnoss.ApiWrapper.ApiModel.SparqlObject;


namespace ImportadorWebCV.Sincro.Secciones.ActividadDocenteSubclases
{
    class FormacionAcademica : DisambiguableEntity
    {
        public string titulo { get; set; }
        public string nombreAsignatura { get; set; }
        public string fecha { get; set; }
        public string entidadRealizacion { get; set; }

        private static readonly DisambiguationDataConfig configTitulo = new DisambiguationDataConfig()
        {
            type = DisambiguationDataConfigType.equalsTitle,
            score = 0.8f
        };

        private static readonly DisambiguationDataConfig configFecha = new DisambiguationDataConfig()
        {
            type = DisambiguationDataConfigType.equalsItem,
            score = 0.5f,
            scoreMinus = 0.5f
        };

        private static readonly DisambiguationDataConfig configNombre = new DisambiguationDataConfig()
        {
            type = DisambiguationDataConfigType.equalsItem,
            score = 0.5f,
            scoreMinus = 0.5f
        };

        private static readonly DisambiguationDataConfig configER = new DisambiguationDataConfig()
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
                    property = "titulo",
                    config = configTitulo,
                    value = titulo
                },

                new DisambiguationData()
                {
                    property = "nombreAsignatura",
                    config = configNombre,
                    value = nombreAsignatura
                },

                new DisambiguationData()
                {
                    property = "fecha",
                    config = configFecha,
                    value = fecha
                },

                new DisambiguationData()
                {
                    property = "entidadRealizacion",
                    config = configER,
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
                string select = $@"SELECT distinct ?item ?itemTitle ?itemName ?itemDate ?itemER ";
                string where = $@"where {{
                                        ?item <{Variables.ActividadDocente.formacionAcademicaTitulacionUniversitariaNombre}> ?itemTitle . 
                                        OPTIONAL{{?item <{Variables.ActividadDocente.formacionAcademicaNombreAsignatura}> ?itemName }}.
                                        OPTIONAL{{?item <{Variables.ActividadDocente.formacionAcademicaFechaInicio}> ?itemDate }}.
                                        OPTIONAL{{?item <{Variables.ActividadDocente.formacionAcademicaEntidadRealizacionNombre}> ?itemER }}.
                                        FILTER(?item in (<{string.Join(">,<", lista)}>))
                                    }}";
                SparqlObject resultData = pResourceApi.VirtuosoQuery(select, where, graph);
                foreach (Dictionary<string, Data> fila in resultData.results.bindings)
                {
                    FormacionAcademica formacionAcademica = new FormacionAcademica
                    {
                        ID = fila["item"].value,
                        titulo = fila["itemTitle"].value,
                        nombreAsignatura = fila.ContainsKey("itemName") ? fila["itemName"].value : "",
                        fecha = fila.ContainsKey("itemDate") ? fila["itemDate"].value : "",
                        entidadRealizacion = fila.ContainsKey("itemER") ? fila["itemER"].value : ""
                    };

                    resultados.Add(pResourceApi.GetShortGuid(fila["item"].value).ToString(), formacionAcademica);
                }
            }

            return resultados;
        }
    }
}
