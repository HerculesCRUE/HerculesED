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
        public string Descripcion { get; set; }
        public string Funciones { get; set; }
        public string EntidadRealizacion { get; set; }
        public string Fecha { get; set; }

        private static readonly DisambiguationDataConfig configDescripcionGestIdi = new(DisambiguationDataConfigType.equalsTitle, 0.8f);
        private static readonly DisambiguationDataConfig configEntReaGestIdi = new(DisambiguationDataConfigType.equalsItem, 0.5f, 0.5f);
        private static readonly DisambiguationDataConfig configFuncionesGestIdi = new(DisambiguationDataConfigType.equalsItem, 0.5f, 0.5f);
        private static readonly DisambiguationDataConfig configFechaGestIdi = new(DisambiguationDataConfigType.equalsItem, 0.5f, 0.5f);

        public override List<DisambiguationData> GetDisambiguationData()
        {
            List<DisambiguationData> data = new()
            {
                new DisambiguationData(configDescripcionGestIdi, "descripcion", Descripcion),
                new DisambiguationData(configEntReaGestIdi, "entidadRealizacion", EntidadRealizacion),
                new DisambiguationData(configFuncionesGestIdi, "funciones", Funciones),
                new DisambiguationData(configFechaGestIdi, "fecha", Fecha)
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
        public static Dictionary<string, DisambiguableEntity> GetBBDDGestIdi(ResourceApi pResourceApi, string pCVID, string graph, List<string> propiedadesItem)
        {
            //Obtenemos IDS
            HashSet<string> ids = UtilitySecciones.GetIDS(pResourceApi, pCVID, propiedadesItem);

            Dictionary<string, DisambiguableEntity> resultadosGestIdi = new();

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
                    GestionIDI gestionIDI = new()
                    {
                        ID = fila["item"].value,
                        Descripcion = fila["itemTitle"].value,
                        Funciones = fila.ContainsKey("itemFunciones") ? fila["itemFunciones"].value : "",
                        EntidadRealizacion = fila.ContainsKey("itemER") ? fila["itemER"].value : "",
                        Fecha = fila.ContainsKey("itemDate") ? fila["itemDate"].value : ""
                    };

                    resultadosGestIdi.Add(pResourceApi.GetShortGuid(fila["item"].value).ToString(), gestionIDI);
                }
            }

            return resultadosGestIdi;
        }
    }
}
