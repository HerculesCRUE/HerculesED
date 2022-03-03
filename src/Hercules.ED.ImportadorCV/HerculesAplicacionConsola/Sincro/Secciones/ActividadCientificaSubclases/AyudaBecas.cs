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

namespace HerculesAplicacionConsola.Sincro.Secciones.ActividadCientificaSubclases
{
    class AyudaBecas : DisambiguableEntity
    {
        public string descripcion { get; set; }
        public string fecha { get; set; }
        public string entidadConcesionaria { get; set; }

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
        
        private static DisambiguationDataConfig configEC = new DisambiguationDataConfig()
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

            data.Add(new DisambiguationData()
            {
                property = "entidadConcesionaria",
                config = configEC,
                value = entidadConcesionaria
            });
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

            //Divido la lista en listas de 1.000 elementos
            List<List<string>> listaListas = UtilitySecciones.SplitList(ids.ToList(), 1000).ToList();

            foreach (List<string> lista in listaListas)
            {
                string select = $@"SELECT distinct ?item ?itemTitle ?itemDate ?itemEC";
                string where = $@"where {{
                                        ?item <{Variables.ActividadCientificaTecnologica.ayudasBecasNombre}> ?itemTitle . 
                                        OPTIONAL{{?item <{Variables.ActividadCientificaTecnologica.ayudasBecasFechaConcesion}> ?itemDate }}.
                                        OPTIONAL{{?item <{Variables.ActividadCientificaTecnologica.ayudasBecasEntidadConcedeNombre}> ?itemEC }}.
                                        FILTER(?item in (<{string.Join(">,<", lista)}>))
                                    }}";

                SparqlObject resultData = pResourceApi.VirtuosoQuery(select, where, graph);
                foreach (Dictionary<string, Data> fila in resultData.results.bindings)
                {
                    AyudaBecas ayudaBecas = new AyudaBecas();
                    ayudaBecas.ID = fila["item"].value;
                    ayudaBecas.descripcion = fila["itemTitle"].value;
                    ayudaBecas.fecha = "";
                    if (fila.ContainsKey("itemDate"))
                    {
                        ayudaBecas.fecha = fila["itemDate"].value;
                    }
                    ayudaBecas.entidadConcesionaria = "";
                    if (fila.ContainsKey("itemEC"))
                    {
                        ayudaBecas.entidadConcesionaria = fila["itemEC"].value;
                    }
                    resultados.Add(pResourceApi.GetShortGuid(fila["item"].value).ToString(), ayudaBecas);
                }
            }

            return resultados;
        }
    }
}
