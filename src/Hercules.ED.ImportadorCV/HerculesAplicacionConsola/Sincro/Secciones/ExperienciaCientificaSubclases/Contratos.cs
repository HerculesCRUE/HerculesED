using Gnoss.ApiWrapper;
using Gnoss.ApiWrapper.ApiModel;
using Hercules.ED.DisambiguationEngine.Models;
using HerculesAplicacionConsola.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Gnoss.ApiWrapper.ApiModel.SparqlObject;

namespace HerculesAplicacionConsola.Sincro.Secciones.ExperienciaCientificaSubclases
{
    class Contratos : DisambiguableEntity
    {
        public string descripcion { get; set; }
        public string fecha { get; set; }

        private static DisambiguationDataConfig configDescripcion = new DisambiguationDataConfig()
        {
            type = DisambiguationDataConfigType.equalsTitle,
            score = 0.8f
        };

        private static DisambiguationDataConfig configFecha = new DisambiguationDataConfig()
        {
            type = DisambiguationDataConfigType.equalsItem,
            score = 0.5f,
            scoreMinus = 0.5f
        };

        public override List<DisambiguationData> GetDisambiguationData()
        {
            List<DisambiguationData> data = new List<DisambiguationData>();

            data.Add(new DisambiguationData()
            {
                property = "descripcion",
                config = configDescripcion,
                value = descripcion
            });

            data.Add(new DisambiguationData()
            {
                property = "fecha",
                config = configFecha,
                value = fecha
            });
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
                string select = $@"SELECT distinct ?item ?itemTitle ?itemDate ";
                string where = $@"where {{
                                        ?item <{Variables.ExperienciaCientificaTecnologica.contratosNombrePrograma}> ?itemTitle . 
                                        OPTIONAL{{?item <{Variables.ExperienciaCientificaTecnologica.contratosFechaInicio}> ?itemDate }}.
                                        FILTER(?item in (<{string.Join(">,<", lista)}>))
                                    }}";
                //TODO check where valores
                SparqlObject resultData = pResourceApi.VirtuosoQuery(select, where, graph);
                foreach (Dictionary<string, Data> fila in resultData.results.bindings)
                {
                    Contratos contratos = new Contratos();
                    contratos.ID = fila["item"].value;
                    contratos.descripcion = fila["itemTitle"].value;
                    contratos.fecha = "";
                    if (fila.ContainsKey("itemDate"))
                    {
                        contratos.fecha = fila["itemDate"].value;
                    }
                    resultados.Add(pResourceApi.GetShortGuid(fila["item"].value).ToString(), contratos);
                }
            }

            return resultados;
        }
    }
}
