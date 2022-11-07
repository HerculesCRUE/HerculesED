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
        public string Descripcion { get; set; }
        public string Fecha { get; set; }
        public string TipoActividad { get; set; }
        public string EntidadConvocante { get; set; }

        private static readonly DisambiguationDataConfig configDescripcionOrgIdi = new(DisambiguationDataConfigType.equalsTitle, 0.8f);
        private static readonly DisambiguationDataConfig configFechaOrgIdi = new(DisambiguationDataConfigType.equalsItem, 0.5f, 0.5f);
        private static readonly DisambiguationDataConfig configTipoActividadOrgIdi = new(DisambiguationDataConfigType.equalsItem, 0.5f, 0.5f);
        private static readonly DisambiguationDataConfig configEntConOrgIdi = new(DisambiguationDataConfigType.equalsItem, 0.5f, 0.5f);

        public override List<DisambiguationData> GetDisambiguationData()
        {
            List<DisambiguationData> data = new()
            {
                new DisambiguationData(configDescripcionOrgIdi, "descripcion", Descripcion),
                new DisambiguationData(configFechaOrgIdi, "fecha", Fecha),
                new DisambiguationData(configTipoActividadOrgIdi, "tipoActividad", TipoActividad),
                new DisambiguationData(configEntConOrgIdi, "entidadConvocante", EntidadConvocante)
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
        public static Dictionary<string, DisambiguableEntity> GetBBDDOrgIdi(ResourceApi pResourceApi, string pCVID, string graph, List<string> propiedadesItem)
        {
            //Obtenemos IDS
            HashSet<string> ids = UtilitySecciones.GetIDS(pResourceApi, pCVID, propiedadesItem);

            Dictionary<string, DisambiguableEntity> resultadosOrgIdi = new Dictionary<string, DisambiguableEntity>();

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
                        Descripcion = fila["itemTitle"].value,
                        Fecha = fila.ContainsKey("itemDate") ? fila["itemDate"].value : "",
                        TipoActividad = fila.ContainsKey("itemTipo") ? fila["itemTipo"].value : "",
                        EntidadConvocante = fila.ContainsKey("itemEC") ? fila["itemEC"].value : ""
                    };

                    resultadosOrgIdi.Add(pResourceApi.GetShortGuid(fila["item"].value).ToString(), organizacionesIDI);
                }
            }

            return resultadosOrgIdi;
        }
    }
}
