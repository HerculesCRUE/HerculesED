using Gnoss.ApiWrapper;
using Gnoss.ApiWrapper.ApiModel;
using Hercules.CommonsEDMA.DisambiguationEngine.Models;
using System.Collections.Generic;
using System.Linq;
using Utils;
using static Gnoss.ApiWrapper.ApiModel.SparqlObject;

namespace ImportadorWebCV.Sincro.Secciones.FormacionAcademicaSubclases
{
    class ConocimientoIdiomas : DisambiguableEntity
    {
        public string Idioma { get; set; }

        private static readonly DisambiguationDataConfig configIdiomaConIdi = new(DisambiguationDataConfigType.equalsTitle, 0.8f);

        public override List<DisambiguationData> GetDisambiguationData()
        {
            List<DisambiguationData> data = new()
            {
                new DisambiguationData(configIdiomaConIdi, "idioma", Idioma)
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
        public static Dictionary<string, DisambiguableEntity> GetBBDDConIdi(ResourceApi pResourceApi, string pCVID, string graph, List<string> propiedadesItem)
        {
            //Obtenemos IDS
            HashSet<string> ids = UtilitySecciones.GetIDS(pResourceApi, pCVID, propiedadesItem);

            Dictionary<string, DisambiguableEntity> resultadosConIdi = new();

            //Divido la lista en listas de elementos
            List<List<string>> listaListas = UtilitySecciones.SplitList(ids.ToList(), Utility.splitListNum).ToList();

            foreach (List<string> lista in listaListas)
            {
                string select = $@"SELECT distinct ?item ?itemTitle ";
                string where = $@"where {{
                                        ?item <{Variables.FormacionAcademica.conocimientoIdiomasIdioma.Split("@@@").First()}> ?itemTitle . 
                                        FILTER(?item in (<{string.Join(">,<", lista)}>))
                                    }}";
                SparqlObject resultData = pResourceApi.VirtuosoQuery(select, where, graph);
                foreach (Dictionary<string, Data> fila in resultData.results.bindings)
                {
                    ConocimientoIdiomas conocimientoIdiomas = new()
                    {
                        ID = fila["item"].value,
                        Idioma = fila["itemTitle"].value
                    };
                    resultadosConIdi.Add(pResourceApi.GetShortGuid(fila["item"].value).ToString(), conocimientoIdiomas);
                }
            }

            return resultadosConIdi;
        }
    }
}
