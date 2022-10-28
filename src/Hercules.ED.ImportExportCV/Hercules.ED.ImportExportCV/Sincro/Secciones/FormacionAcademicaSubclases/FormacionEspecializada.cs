using Gnoss.ApiWrapper;
using Gnoss.ApiWrapper.ApiModel;
using Hercules.CommonsEDMA.DisambiguationEngine.Models;
using System.Collections.Generic;
using System.Linq;
using Utils;
using static Gnoss.ApiWrapper.ApiModel.SparqlObject;

namespace ImportadorWebCV.Sincro.Secciones.FormacionAcademicaSubclases
{
    class FormacionEspecializada : DisambiguableEntity
    {
        public string Descripcion { get; set; }
        public string EntidadTitulacion { get; set; }
        public string Fecha { get; set; }

        private static readonly DisambiguationDataConfig configDescripcionFormEsp = new(DisambiguationDataConfigType.equalsTitle, 0.8f);
        private static readonly DisambiguationDataConfig configEntTitFormEsp = new(DisambiguationDataConfigType.equalsItem, 0.5f, 0.5f);
        private static readonly DisambiguationDataConfig configFechaFormEsp = new(DisambiguationDataConfigType.equalsItem, 0.5f, 0.5f);

        public override List<DisambiguationData> GetDisambiguationData()
        {
            List<DisambiguationData> data = new()
            {
                new DisambiguationData(configDescripcionFormEsp,"descripcion",Descripcion),
                new DisambiguationData(configEntTitFormEsp,"entidadTitulacion",EntidadTitulacion),
                new DisambiguationData(configFechaFormEsp,"fecha",Fecha)
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
        public static Dictionary<string, DisambiguableEntity> GetBBDDFormEsp(ResourceApi pResourceApi, string pCVID, string graph, List<string> propiedadesItem)
        {
            //Obtenemos IDS
            HashSet<string> ids = UtilitySecciones.GetIDS(pResourceApi, pCVID, propiedadesItem);

            Dictionary<string, DisambiguableEntity> resultadosFormEsp = new ();

            //Divido la lista en listas de elementos
            List<List<string>> listaListas = UtilitySecciones.SplitList(ids.ToList(), Utility.splitListNum).ToList();

            foreach (List<string> lista in listaListas)
            {
                string select = $@"SELECT distinct ?item ?itemTitle ?itemDate  ?itemET ";
                string where = $@"where {{
                                        ?item <{Variables.FormacionAcademica.formacionEspeTituloFormacion}> ?itemTitle . 
                                        OPTIONAL{{?item <{Variables.FormacionAcademica.formacionEspeFechaFinalizacion}> ?itemDate }}.
                                        OPTIONAL{{?item <{Variables.FormacionAcademica.formacionEspeEntidadTitulacionNombre}> ?itemET }}.
                                        FILTER(?item in (<{string.Join(">,<", lista)}>))
                                    }}";
                SparqlObject resultData = pResourceApi.VirtuosoQuery(select, where, graph);
                foreach (Dictionary<string, Data> fila in resultData.results.bindings)
                {
                    FormacionEspecializada formacionEspecializada = new ()
                    {
                        ID = fila["item"].value,
                        Descripcion = fila["itemTitle"].value,
                        Fecha = fila.ContainsKey("itemDate") ? fila["itemDate"].value : "",
                        EntidadTitulacion = fila.ContainsKey("itemET") ? fila["itemET"].value : ""
                    };

                    resultadosFormEsp.Add(pResourceApi.GetShortGuid(fila["item"].value).ToString(), formacionEspecializada);
                }
            }

            return resultadosFormEsp;
        }
    }
}
