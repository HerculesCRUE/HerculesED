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
        public string Nombre { get; set; }
        public string NombreOtros { get; set; }
        public string EntidadRealizacion { get; set; }

        private static readonly DisambiguationDataConfig configNombreTutAca = new(DisambiguationDataConfigType.equalsTitle, 0.8f);
        private static readonly DisambiguationDataConfig configNombreOtrosTutAca = new(DisambiguationDataConfigType.equalsItem, 0.5f, 0.5f);
        private static readonly DisambiguationDataConfig configEntReaTutAca = new(DisambiguationDataConfigType.equalsItem, 0.5f, 0.5f);

        public override List<DisambiguationData> GetDisambiguationData()
        {
            List<DisambiguationData> data = new()
            {
                new DisambiguationData(configNombreTutAca, "nombre", Nombre),
                new DisambiguationData(configNombreOtrosTutAca, "nombreOtros", NombreOtros),
                new DisambiguationData(configEntReaTutAca, "entidadRealizacion", EntidadRealizacion)
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
        public static Dictionary<string, DisambiguableEntity> GetBBDDTutAca(ResourceApi pResourceApi, string pCVID, string graph, List<string> propiedadesItem)
        {
            //Obtenemos IDS
            HashSet<string> ids = UtilitySecciones.GetIDS(pResourceApi, pCVID, propiedadesItem);

            Dictionary<string, DisambiguableEntity> resultadosTutAca = new ();

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
                        Nombre = fila["itemTitle"].value,
                        NombreOtros = fila.ContainsKey("itemOther") ? fila["itemOther"].value : "",
                        EntidadRealizacion = fila.ContainsKey("itemER") ? fila["itemER"].value : "",
                    };

                    resultadosTutAca.Add(pResourceApi.GetShortGuid(fila["item"].value).ToString(), tutoriasAcademicas);
                }
            }

            return resultadosTutAca;
        }
    }
}
