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
    class SociedadesAsociaciones : DisambiguableEntity
    {
        public string descripcion { get; set; }
        public string fecha { get; set; }
        public string entidadAfiliacion { get; set; }

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

        private static DisambiguationDataConfig configEA = new DisambiguationDataConfig()
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
                property = "entidadAfiliacion",
                config = configEA,
                value = entidadAfiliacion
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
                string select = $@"SELECT distinct ?item ?itemTitle ?itemDate ?itemEA";
                string where = $@"where {{
                                        ?item <{Variables.ActividadCientificaTecnologica.sociedadesNombre}> ?itemTitle . 
                                        OPTIONAL{{?item <{Variables.ActividadCientificaTecnologica.sociedadesFechaInicio}> ?itemDate }}.
                                        OPTIONAL{{?item <{Variables.ActividadCientificaTecnologica.sociedadesEntidadAfiliacionNombre}> ?itemEA }}.
                                        FILTER(?item in (<{string.Join(">,<", lista)}>))
                                    }}";

                SparqlObject resultData = pResourceApi.VirtuosoQuery(select, where, graph);
                foreach (Dictionary<string, Data> fila in resultData.results.bindings)
                {
                    SociedadesAsociaciones sociedadesAsociaciones = new SociedadesAsociaciones();
                    sociedadesAsociaciones.ID = fila["item"].value;
                    sociedadesAsociaciones.descripcion = fila["itemTitle"].value;
                    sociedadesAsociaciones.fecha = "";
                    if (fila.ContainsKey("itemDate"))
                    {
                        sociedadesAsociaciones.fecha = fila["itemDate"].value;
                    }
                    sociedadesAsociaciones.entidadAfiliacion = "";
                    if (fila.ContainsKey("itemNombre"))
                    {
                        sociedadesAsociaciones.entidadAfiliacion = fila["itemEA"].value;
                    }
                    resultados.Add(pResourceApi.GetShortGuid(fila["item"].value).ToString(), sociedadesAsociaciones);
                }
            }

            return resultados;
        }
    }
}
