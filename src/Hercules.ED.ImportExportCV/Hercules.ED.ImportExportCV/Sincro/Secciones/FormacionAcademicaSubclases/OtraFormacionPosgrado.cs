using Gnoss.ApiWrapper;
using Gnoss.ApiWrapper.ApiModel;
using Hercules.CommonsEDMA.DisambiguationEngine.Models;
using Utils;
using System.Collections.Generic;
using System.Linq;
using static Gnoss.ApiWrapper.ApiModel.SparqlObject;

namespace ImportadorWebCV.Sincro.Secciones.FormacionAcademicaSubclases
{
    class OtraFormacionPosgrado : DisambiguableEntity
    {
        public string Descripcion { get; set; }
        public string EntidadTitulacion { get; set; }
        public string Fecha { get; set; }

        private static readonly DisambiguationDataConfig configDescripcionOtrFormPos = new(DisambiguationDataConfigType.equalsTitle, 0.8f);
        private static readonly DisambiguationDataConfig configFechaOtrFormPos = new(DisambiguationDataConfigType.equalsItem, 0.5f, 0.5f);
        private static readonly DisambiguationDataConfig configEntTitOtrFormPos = new(DisambiguationDataConfigType.equalsItem, 0.5f, 0.5f);

        public override List<DisambiguationData> GetDisambiguationData()
        {
            List<DisambiguationData> data = new()
            {
                new DisambiguationData(configDescripcionOtrFormPos, "descripcion", Descripcion),
                new DisambiguationData(configFechaOtrFormPos, "fecha", Fecha),
                new DisambiguationData(configEntTitOtrFormPos, "entidadTitulacion", EntidadTitulacion)
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
        public static Dictionary<string, DisambiguableEntity> GetBBDDOtrFormPos(ResourceApi pResourceApi, string pCVID, string graph, List<string> propiedadesItem)
        {
            //Obtenemos IDS
            HashSet<string> ids = UtilitySecciones.GetIDS(pResourceApi, pCVID, propiedadesItem);

            Dictionary<string, DisambiguableEntity> resultadosOtrFormPos = new();

            //Divido la lista en listas de elementos
            List<List<string>> listaListas = UtilitySecciones.SplitList(ids.ToList(), Utility.splitListNum).ToList();

            foreach (List<string> lista in listaListas)
            {
                string select = $@"SELECT distinct ?item ?itemTitle ?itemDate ?itemET ";
                string where = $@"where {{
                                        ?item <{Variables.FormacionAcademica.otraFormacionTipoFormacion}> ?itemTitle . 
                                        OPTIONAL{{?item <{Variables.FormacionAcademica.otraFormacionFechaTitulacion}> ?itemDate }}.
                                        OPTIONAL{{?item <{Variables.FormacionAcademica.otraFormacionEntidadTitulacionNombre}> ?itemET }}.
                                        FILTER(?item in (<{string.Join(">,<", lista)}>))
                                    }}";

                SparqlObject resultData = pResourceApi.VirtuosoQuery(select, where, graph);
                foreach (Dictionary<string, Data> fila in resultData.results.bindings)
                {
                    OtraFormacionPosgrado otraFormacion = new()
                    {
                        ID = fila["item"].value,
                        Descripcion = fila["itemTitle"].value,
                        Fecha = fila.ContainsKey("itemDate") ? fila["itemDate"].value : "",
                        EntidadTitulacion = fila.ContainsKey("itemET") ? fila["itemET"].value : ""
                    };
                    resultadosOtrFormPos.Add(pResourceApi.GetShortGuid(fila["item"].value).ToString(), otraFormacion);
                }
            }

            return resultadosOtrFormPos;
        }
    }
}
