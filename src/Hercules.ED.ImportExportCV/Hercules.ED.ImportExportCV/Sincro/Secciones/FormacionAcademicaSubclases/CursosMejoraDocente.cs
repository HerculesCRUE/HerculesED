using Gnoss.ApiWrapper;
using Gnoss.ApiWrapper.ApiModel;
using Hercules.CommonsEDMA.DisambiguationEngine.Models;
using System.Collections.Generic;
using System.Linq;
using Utils;
using static Gnoss.ApiWrapper.ApiModel.SparqlObject;
namespace ImportadorWebCV.Sincro.Secciones.FormacionAcademicaSubclases
{
    class CursosMejoraDocente : DisambiguableEntity
    {
        public string Descripcion { get; set; }
        public string Fecha { get; set; }
        public string EntidadOrganizadora { get; set; }

        private static readonly DisambiguationDataConfig configDescripcionCurMejDoc = new(DisambiguationDataConfigType.equalsTitle, 0.8f);
        private static readonly DisambiguationDataConfig configFechaCurMejDoc = new(DisambiguationDataConfigType.equalsItem, 0.5f, 0.5f);
        private static readonly DisambiguationDataConfig configEntOrgCurMejDoc = new(DisambiguationDataConfigType.equalsItem, 0.5f, 0.5f);

        public override List<DisambiguationData> GetDisambiguationData()
        {
            List<DisambiguationData> data = new()
            {
                new DisambiguationData(configDescripcionCurMejDoc, "descripcion", Descripcion),
                new DisambiguationData(configFechaCurMejDoc, "fecha", Fecha),
                new DisambiguationData(configEntOrgCurMejDoc, "entidadOrganizadora", EntidadOrganizadora)
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
        public static Dictionary<string, DisambiguableEntity> GetBBDDCurMejDoc(ResourceApi pResourceApi, string pCVID, string graph, List<string> propiedadesItem)
        {
            //Obtenemos IDS
            HashSet<string> ids = UtilitySecciones.GetIDS(pResourceApi, pCVID, propiedadesItem);

            Dictionary<string, DisambiguableEntity> resultadosCurMejDoc = new();

            //Divido la lista en listas de elementos
            List<List<string>> listaListas = UtilitySecciones.SplitList(ids.ToList(), Utility.splitListNum).ToList();

            foreach (List<string> lista in listaListas)
            {
                string select = $@"SELECT distinct ?item ?itemTitle ?itemDate ?itemEO ";
                string where = $@"where {{
                                        ?item <{Variables.FormacionAcademica.cursosSeminariosTitulo}> ?itemTitle . 
                                        OPTIONAL{{?item <{Variables.FormacionAcademica.cursosSeminariosFechaInicio}> ?itemDate }}.
                                        OPTIONAL{{?item <{Variables.FormacionAcademica.cursosSeminariosEntidadOrganizadoraNombre}> ?itemEO }}.
                                        FILTER(?item in (<{string.Join(">,<", lista)}>))
                                    }}";
                SparqlObject resultData = pResourceApi.VirtuosoQuery(select, where, graph);
                foreach (Dictionary<string, Data> fila in resultData.results.bindings)
                {
                    CursosMejoraDocente cursosMejoraDocente = new()
                    {
                        ID = fila["item"].value,
                        Descripcion = fila["itemTitle"].value,
                        Fecha = fila.ContainsKey("itemDate") ? fila["itemDate"].value : "",
                        EntidadOrganizadora = fila.ContainsKey("itemEO") ? fila["itemEO"].value : ""
                    };

                    resultadosCurMejDoc.Add(pResourceApi.GetShortGuid(fila["item"].value).ToString(), cursosMejoraDocente);
                }
            }

            return resultadosCurMejDoc;
        }
    }
}
