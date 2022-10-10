using Gnoss.ApiWrapper;
using Gnoss.ApiWrapper.ApiModel;
using Hercules.CommonsEDMA.DisambiguationEngine.Models;
using Utils;
using System.Collections.Generic;
using System.Linq;
using static Gnoss.ApiWrapper.ApiModel.SparqlObject;

namespace ImportadorWebCV.Sincro.Secciones.ActividadDocenteSubclases
{
    class TutoriasAcademicas : DisambiguableEntity
    {
        public string nombre { get; set; }
        public string nombreOtros { get; set; }
        public string entidadRealizacion { get; set; }

        private static readonly DisambiguationDataConfig configNombre = new DisambiguationDataConfig()
        {
            type = DisambiguationDataConfigType.equalsTitle,
            score = 0.8f
        };

        private static readonly DisambiguationDataConfig configNombreOtros = new DisambiguationDataConfig()
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
                    property = "nombre",
                    config = configNombre,
                    value = nombre
                },

                new DisambiguationData()
                {
                    property = "nombreOtros",
                    config = configNombreOtros,
                    value = nombreOtros
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
                string select = $@"SELECT distinct ?item ?itemTitle ?itemOther ?itemER ";
                string where = $@"where {{
                                        ?item <{Variables.ActividadDocente.tutoAcademicaNombrePrograma}> ?itemTitle . 
                                        OPTIONAL{{?item <{Variables.ActividadDocente.tutoAcademicaNombreProgramaOtros}> ?itemOther }}.
                                        OPTIONAL{{?item <{Variables.ActividadDocente.tutoAcademicaEntidadRealizacionNombre}> ?itemER }}.
                                        FILTER(?item in (<{string.Join(">,<", lista)}>))
                                    }}";
                SparqlObject resultData = pResourceApi.VirtuosoQuery(select, where, graph);
                foreach (Dictionary<string, Data> fila in resultData.results.bindings)
                {
                    TutoriasAcademicas tutoriasAcademicas = new TutoriasAcademicas
                    {
                        ID = fila["item"].value,
                        nombre = fila["itemTitle"].value,
                        nombreOtros = fila.ContainsKey("itemOther") ? fila["itemOther"].value : "",
                        entidadRealizacion = fila.ContainsKey("itemER") ? fila["itemER"].value : "",
                    };

                    resultados.Add(pResourceApi.GetShortGuid(fila["item"].value).ToString(), tutoriasAcademicas);
                }
            }

            return resultados;
        }
    }
}
