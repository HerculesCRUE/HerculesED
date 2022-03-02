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
namespace HerculesAplicacionConsola.Sincro.Secciones.SituacionProfesionalSubclases
{
    class CargosActividades : DisambiguableEntity
    {
        public string nombre { get; set; }
        public string categoria { get; set; }
        public string fechaIni { get; set; }

        private static DisambiguationDataConfig configNombre = new DisambiguationDataConfig()
        {
            type = DisambiguationDataConfigType.equalsTitle,
            score = 0.8f
        };

        private static DisambiguationDataConfig configCategoria = new DisambiguationDataConfig()
        {
            type = DisambiguationDataConfigType.equalsItem,
            score = 0.5f,
            scoreMinus = 0.5f
        };

        private static DisambiguationDataConfig configFechaIni= new DisambiguationDataConfig()
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
                property = "nombre",
                config = configNombre,
                value = nombre
            });

            data.Add(new DisambiguationData()
            {
                property = "categoria",
                config = configCategoria,
                value = categoria
            });
            
            data.Add(new DisambiguationData()
            {
                property = "fechaIni",
                config = configFechaIni,
                value = fechaIni
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
                    CargosActividades cargosActividades = new CargosActividades();
                    cargosActividades.ID = fila["item"].value;
                    cargosActividades.nombre = fila["itemTitle"].value;
                    cargosActividades.categoria = "";
                    if (fila.ContainsKey("itemCategoria"))
                    {
                        cargosActividades.categoria = fila["itemCategoria"].value;
                    }
                    cargosActividades.fechaIni = "";
                    if (fila.ContainsKey("itemCategoria"))
                    {
                        cargosActividades.fechaIni = fila["itemFechaIni"].value;
                    }
                    resultados.Add(pResourceApi.GetShortGuid(fila["item"].value).ToString(), cargosActividades);
                }
            }

            return resultados;
        }
    }
}
