using Gnoss.ApiWrapper;
using Gnoss.ApiWrapper.ApiModel;
using Hercules.CommonsEDMA.DisambiguationEngine.Models;
using System.Collections.Generic;
using System.Linq;
using Utils;
using static Gnoss.ApiWrapper.ApiModel.SparqlObject;

namespace ImportadorWebCV.Sincro.Secciones.ActividadCientificaSubclases
{
    class OrganizacionesIDI : DisambiguableEntity
    {
        public string descripcion { get; set; }
        public string fecha { get; set; }
        public string tipoActividad { get; set; }
        public string entidadConvocante { get; set; }

        private static readonly DisambiguationDataConfig configDescripcion = new(DisambiguationDataConfigType.equalsTitle, 0.8f);
        private static readonly DisambiguationDataConfig configFecha = new(DisambiguationDataConfigType.equalsItem, 0.5f, 0.5f);
        private static readonly DisambiguationDataConfig configTipoActividad = new(DisambiguationDataConfigType.equalsItem, 0.5f, 0.5f);
        private static readonly DisambiguationDataConfig configEC = new(DisambiguationDataConfigType.equalsItem, 0.5f, 0.5f);

        public override List<DisambiguationData> GetDisambiguationData()
        {
            List<DisambiguationData> data = new()
            {
                new DisambiguationData(configDescripcion, "descripcion",descripcion),
                new DisambiguationData(configFecha,"fecha",fecha),
                new DisambiguationData(configTipoActividad,"tipoActividad",tipoActividad),
                new DisambiguationData(configEC,"entidadConvocante",entidadConvocante)
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
                string select = $@"SELECT distinct ?item ?itemTitle ?itemTipo ?itemEC";
                string where = $@"where {{
                                        ?item <{Variables.ActividadCientificaTecnologica.orgIDITituloActividad}> ?itemTitle . 
                                        OPTIONAL{{ ?item <{Variables.ActividadCientificaTecnologica.orgIDIFechaInicio}> ?itemDate }} .
                                        OPTIONAL{{ ?item <{Variables.ActividadCientificaTecnologica.orgIDITipoActividad}> ?itemTipo }} .
                                        OPTIONAL{{ ?item <{Variables.ActividadCientificaTecnologica.orgIDIEntidadConvocanteNombre}> ?itemEC }} .
                                        FILTER(?item in (<{string.Join(">,<", lista)}>))
                                    }}";

                SparqlObject resultData = pResourceApi.VirtuosoQuery(select, where, graph);
                foreach (Dictionary<string, Data> fila in resultData.results.bindings)
                {
                    OrganizacionesIDI organizacionesIDI = new OrganizacionesIDI
                    {
                        ID = fila["item"].value,
                        descripcion = fila["itemTitle"].value,
                        fecha = fila.ContainsKey("itemDate") ? fila["itemDate"].value : "",
                        tipoActividad = fila.ContainsKey("itemTipo") ? fila["itemTipo"].value : "",
                        entidadConvocante = fila.ContainsKey("itemEC") ? fila["itemEC"].value : ""
                    };

                    resultados.Add(pResourceApi.GetShortGuid(fila["item"].value).ToString(), organizacionesIDI);
                }
            }

            return resultados;
        }
    }
}
