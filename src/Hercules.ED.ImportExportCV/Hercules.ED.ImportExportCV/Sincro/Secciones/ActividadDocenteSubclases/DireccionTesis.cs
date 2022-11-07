using Gnoss.ApiWrapper;
using Gnoss.ApiWrapper.ApiModel;
using Hercules.CommonsEDMA.DisambiguationEngine.Models;
using Utils;
using System.Collections.Generic;
using System.Linq;
using static Gnoss.ApiWrapper.ApiModel.SparqlObject;

namespace ImportadorWebCV.Sincro.Secciones.ActividadDocenteSubclases
{
    class DireccionTesis : DisambiguableEntity
    {
        public string Descripcion { get; set; }
        public string Fecha { get; set; }
        public string EntidadRealizacion { get; set; }

        private static readonly DisambiguationDataConfig configDescripcionDirTes = new(DisambiguationDataConfigType.equalsTitle, 0.8f);
        private static readonly DisambiguationDataConfig configFechaDirTes = new(DisambiguationDataConfigType.equalsItem, 0.5f, 0.5f);
        private static readonly DisambiguationDataConfig configEntReaDirTes = new(DisambiguationDataConfigType.equalsItem, 0.5f, 0.5f);

        public override List<DisambiguationData> GetDisambiguationData()
        {
            List<DisambiguationData> data = new()
            {
                new DisambiguationData(configDescripcionDirTes, "descripcion", Descripcion),
                new DisambiguationData(configFechaDirTes, "fecha", Fecha),
                new DisambiguationData(configEntReaDirTes, "entidadRealizacion", EntidadRealizacion)
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
        public static Dictionary<string, DisambiguableEntity> GetBBDDDirTes(ResourceApi pResourceApi, string pCVID, string graph, List<string> propiedadesItem)
        {
            //Obtenemos IDS
            HashSet<string> ids = UtilitySecciones.GetIDS(pResourceApi, pCVID, propiedadesItem);

            Dictionary<string, DisambiguableEntity> resultadosDirTes = new();

            //Divido la lista en listas de elementos
            List<List<string>> listaListas = UtilitySecciones.SplitList(ids.ToList(), Utility.splitListNum).ToList();

            foreach (List<string> lista in listaListas)
            {
                string select = $@"SELECT distinct ?item ?itemTitle ?itemDate ?itemER ";
                string where = $@"where {{
                                        ?item <{Variables.ActividadDocente.direccionTesisTituloTrabajo}> ?itemTitle . 
                                        OPTIONAL{{?item <{Variables.ActividadDocente.direccionTesisFechaDefensa}> ?itemDate }}.
                                        OPTIONAL{{?item <{Variables.ActividadDocente.direccionTesisEntidadRealizacionNombre}> ?itemER }}.
                                        FILTER(?item in (<{string.Join(">,<", lista)}>))
                                    }}";
                SparqlObject resultData = pResourceApi.VirtuosoQuery(select, where, graph);
                foreach (Dictionary<string, Data> fila in resultData.results.bindings)
                {
                    DireccionTesis direccionTesis = new()
                    {
                        ID = fila["item"].value,
                        Descripcion = fila["itemTitle"].value,
                        Fecha = fila.ContainsKey("itemDate") ? fila["itemDate"].value : "",
                        EntidadRealizacion = fila.ContainsKey("itemER") ? fila["itemER"].value : ""
                    };

                    resultadosDirTes.Add(pResourceApi.GetShortGuid(fila["item"].value).ToString(), direccionTesis);
                }
            }

            return resultadosDirTes;
        }
    }
}
