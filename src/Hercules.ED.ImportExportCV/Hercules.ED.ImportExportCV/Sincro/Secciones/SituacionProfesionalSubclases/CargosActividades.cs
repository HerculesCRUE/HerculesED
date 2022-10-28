using Gnoss.ApiWrapper;
using Gnoss.ApiWrapper.ApiModel;
using Hercules.CommonsEDMA.DisambiguationEngine.Models;
using System.Collections.Generic;
using System.Linq;
using Utils;
using static Gnoss.ApiWrapper.ApiModel.SparqlObject;
namespace ImportadorWebCV.Sincro.Secciones.SituacionProfesionalSubclases
{
    class CargosActividades : DisambiguableEntity
    {
        public string Nombre { get; set; }
        public string Categoria { get; set; }
        public string FechaIni { get; set; }

        private static readonly DisambiguationDataConfig configNombreCarAct = new (DisambiguationDataConfigType.equalsTitle, 0.8f);
        private static readonly DisambiguationDataConfig configCategoriaCarAct = new (DisambiguationDataConfigType.equalsItem, 0.5f, 0.5f);
        private static readonly DisambiguationDataConfig configFechaIniCarAct = new (DisambiguationDataConfigType.equalsItem, 0.5f, 0.5f);

        public override List<DisambiguationData> GetDisambiguationData()
        {
            List<DisambiguationData> data = new ()
            {
                new DisambiguationData(configNombreCarAct,"nombre",Nombre),
                new DisambiguationData(configCategoriaCarAct,"categoria",Categoria),
                new DisambiguationData(configFechaIniCarAct,"fechaIni",FechaIni)
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
        public static Dictionary<string, DisambiguableEntity> GetBBDDCarAct(ResourceApi pResourceApi, string pCVID, string graph, List<string> propiedadesItem)
        {
            //Obtenemos IDS
            HashSet<string> ids = UtilitySecciones.GetIDS(pResourceApi, pCVID, propiedadesItem);

            Dictionary<string, DisambiguableEntity> resultadosCarAct = new ();

            //Divido la lista en listas de elementos
            List<List<string>> listaListas = UtilitySecciones.SplitList(ids.ToList(), Utility.splitListNum).ToList();

            foreach (List<string> lista in listaListas)
            {
                string select = $@"SELECT distinct ?item ?itemTitle ?itemCategoria ?itemFechaIni";
                string where = $@"where {{
                                        ?item <{Variables.SituacionProfesional.cargosActividadesEntidadEmpleadoraNombre}> ?itemTitle . 
                                        OPTIONAL{{?item <{Variables.SituacionProfesional.cargosActividadesCategoriaProfesional}> ?itemCategoria }}.
                                        OPTIONAL{{?item <{Variables.SituacionProfesional.cargosActividadesFechaInicio}> ?itemFechaIni }}.
                                        FILTER(?item in (<{string.Join(">,<", lista)}>))
                                    }}";

                SparqlObject resultData = pResourceApi.VirtuosoQuery(select, where, graph);
                foreach (Dictionary<string, Data> fila in resultData.results.bindings)
                {
                    CargosActividades cargosActividades = new CargosActividades
                    {
                        ID = fila["item"].value,
                        Nombre = fila["itemTitle"].value,
                        Categoria = fila.ContainsKey("itemCategoria") ? fila["itemCategoria"].value : "",
                        FechaIni = fila.ContainsKey("itemFechaIni") ? fila["itemFechaIni"].value : ""
                    };

                    resultadosCarAct.Add(pResourceApi.GetShortGuid(fila["item"].value).ToString(), cargosActividades);
                }
            }

            return resultadosCarAct;
        }
    }
}
