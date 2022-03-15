using Gnoss.ApiWrapper;
using Gnoss.ApiWrapper.ApiModel;
using Hercules.ED.DisambiguationEngine.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utils;
using static Gnoss.ApiWrapper.ApiModel.SparqlObject;

namespace ImportadorWebCV.Sincro.Secciones.FormacionAcademicaSubclases
{
    class ConocimientoIdiomas : DisambiguableEntity
    {
        public string idioma { get; set; }

        private static readonly DisambiguationDataConfig configIdioma = new DisambiguationDataConfig()
        {
            type = DisambiguationDataConfigType.equalsTitle,
            score = 0.8f
        };

        public override List<DisambiguationData> GetDisambiguationData()
        {
            List<DisambiguationData> data = new List<DisambiguationData>
            {
                new DisambiguationData()
                {
                    property = "idioma",
                    config = configIdioma,
                    value = idioma
                }
            };

            return data;
        }

        public static Dictionary<string, DisambiguableEntity> GetBBDD(ResourceApi pResourceApi, string pCVID, string graph, List<string> propiedadesItem)
        {
            //Obtenemos IDS
            HashSet<string> ids = UtilitySecciones.GetIDS(pResourceApi, pCVID, propiedadesItem);

            Dictionary<string, DisambiguableEntity> resultados = new Dictionary<string, DisambiguableEntity>();

            //Divido la lista en listas de 1.000 elementos
            List<List<string>> listaListas = UtilitySecciones.SplitList(ids.ToList(), 1000).ToList();

            foreach (List<string> lista in listaListas)
            {
                string select = $@"SELECT distinct ?item ?itemTitle ";
                string where = $@"where {{
                                        ?item <{Variables.FormacionAcademica.conocimientoIdiomasIdioma}> ?itemTitle . 
                                        FILTER(?item in (<{string.Join(">,<", lista)}>))
                                    }}";
                //TODO check where valores
                SparqlObject resultData = pResourceApi.VirtuosoQuery(select, where, graph);
                foreach (Dictionary<string, Data> fila in resultData.results.bindings)
                {
                    ConocimientoIdiomas conocimientoIdiomas = new ConocimientoIdiomas
                    {
                        ID = fila["item"].value,
                        idioma = fila["itemTitle"].value
                    };
                    resultados.Add(pResourceApi.GetShortGuid(fila["item"].value).ToString(), conocimientoIdiomas);
                }
            }

            return resultados;
        }
    }
}
