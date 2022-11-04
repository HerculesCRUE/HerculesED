using Gnoss.ApiWrapper;
using Gnoss.ApiWrapper.ApiModel;
using Hercules.CommonsEDMA.DisambiguationEngine.Models;
using System.Collections.Generic;
using System.Linq;
using Utils;
using static Gnoss.ApiWrapper.ApiModel.SparqlObject;

namespace ImportadorWebCV.Sincro.Secciones.SituacionProfesionalSubclases
{
    class SituacionProfesionalActual : DisambiguableEntity
    {
        public string Nombre { get; set; }
        public string Categoria { get; set; }

        private static readonly DisambiguationDataConfig configNombreSitProf = new(DisambiguationDataConfigType.equalsTitle, 0.8f);

        private static readonly DisambiguationDataConfig configCategoriaSitProf = new(DisambiguationDataConfigType.equalsItem, 0.5f, 0.5f);

        public override List<DisambiguationData> GetDisambiguationData()
        {
            List<DisambiguationData> data = new()
            {
                new DisambiguationData(configNombreSitProf, "nombre", Nombre),
                new DisambiguationData(configCategoriaSitProf, "categoria", Categoria)
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
        public static Dictionary<string, DisambiguableEntity> GetBBDDSitProf(ResourceApi pResourceApi, string pCVID, string graph, List<string> propiedadesItem)
        {
            //Obtenemos IDS
            HashSet<string> ids = UtilitySecciones.GetIDS(pResourceApi, pCVID, propiedadesItem);

            Dictionary<string, DisambiguableEntity> resultadosSitProf = new();

            //Divido la lista en listas de elementos
            List<List<string>> listaListas = UtilitySecciones.SplitList(ids.ToList(), Utility.splitListNum).ToList();

            foreach (List<string> lista in listaListas)
            {
                string select = $@"SELECT distinct ?item ?itemTitle ?itemCategoria";
                string where = $@"where {{
                                        ?item <{Variables.SituacionProfesional.situacionProfesionalEntidadEmpleadoraNombre}> ?itemTitle . 
                                        OPTIONAL{{?item <{Variables.SituacionProfesional.situacionProfesionalCategoriaProfesional}> ?itemCategoria }} .
                                        FILTER(?item in (<{string.Join(">,<", lista)}>))
                                    }}";

                SparqlObject resultData = pResourceApi.VirtuosoQuery(select, where, graph);
                foreach (Dictionary<string, Data> fila in resultData.results.bindings)
                {
                    SituacionProfesionalActual situacion = new()
                    {
                        ID = fila["item"].value,
                        Nombre = fila["itemTitle"].value,
                        Categoria = fila.ContainsKey("itemCategoria") ? fila["itemCategoria"].value : ""
                    };

                    resultadosSitProf.Add(pResourceApi.GetShortGuid(fila["item"].value).ToString(), situacion);
                }
            }

            return resultadosSitProf;
        }
    }
}
