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
        public string Descripcion { get; set; }
        public string NombreActividad { get; set; }
        public string EntidadRealizacion { get; set; }
        public string FechaInicio { get; set; }

        private static readonly DisambiguationDataConfig configDescripcionEvalRev = new(DisambiguationDataConfigType.equalsTitle, 0.8f);
        private static readonly DisambiguationDataConfig configNomActEvalRev = new(DisambiguationDataConfigType.equalsItem, 0.5f, 0.5f);
        private static readonly DisambiguationDataConfig configFechaIniEvalRev = new(DisambiguationDataConfigType.equalsItem, 0.5f, 0.5f);
        private static readonly DisambiguationDataConfig configEntReaEvalRev = new(DisambiguationDataConfigType.equalsItem, 0.5f, 0.5f);

        public override List<DisambiguationData> GetDisambiguationData()
        {
            List<DisambiguationData> data = new()
            {
                new DisambiguationData(configDescripcionEvalRev, "descripcion", Descripcion),
                new DisambiguationData(configNomActEvalRev, "nombreActividad", NombreActividad),
                new DisambiguationData(configFechaIniEvalRev, "fechaInicio", FechaInicio),
                new DisambiguationData(configEntReaEvalRev, "entidadRealizacion", EntidadRealizacion)
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
        public static Dictionary<string, DisambiguableEntity> GetBBDDEvalRev(ResourceApi pResourceApi, string pCVID, string graph, List<string> propiedadesItem)
        {
            //Obtenemos IDS
            HashSet<string> ids = UtilitySecciones.GetIDS(pResourceApi, pCVID, propiedadesItem);

            Dictionary<string, DisambiguableEntity> resultadosEvalRev = new();

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
                        Descripcion = fila["itemTitle"].value,
                        NombreActividad = fila.ContainsKey("itemNombre") ? fila["itemNombre"].value : "",
                        FechaInicio = fila.ContainsKey("itemFecha") ? fila["itemFecha"].value : "",
                        EntidadRealizacion = fila.ContainsKey("itemER") ? fila["itemER"].value : ""
                    };

                    resultadosEvalRev.Add(pResourceApi.GetShortGuid(fila["item"].value).ToString(), evalRevIDI);
                }
            }

            return resultadosEvalRev;
        }
    }
}
