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

namespace HerculesAplicacionConsola.Sincro.Secciones.ActividadCientifica
{
    class EvalRevIDI : DisambiguableEntity
    {
        public string descripcion { get; set; }
        public string nombreActividad { get; set; }
        public string entidadRealizacion { get; set; }
        public string fechaInicio { get; set; }

        private static DisambiguationDataConfig configDescripcion = new DisambiguationDataConfig()
        {
            type = DisambiguationDataConfigType.equalsTitle,
            score = 0.8f
        };

        private static DisambiguationDataConfig configNomAct = new DisambiguationDataConfig()
        {
            type = DisambiguationDataConfigType.equalsItem,
            score = 0.5f,
            scoreMinus = 0.5f
        };

        private static DisambiguationDataConfig configFechaIni = new DisambiguationDataConfig()
        {
            type = DisambiguationDataConfigType.equalsItem,
            score = 0.5f,
            scoreMinus = 0.5f
        };
        private static DisambiguationDataConfig configEntidadRealizacion = new DisambiguationDataConfig()
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
                property = "nombreActividad",
                config = configNomAct,
                value = nombreActividad
            });

            data.Add(new DisambiguationData()
            {
                property = "fechaInicio",
                config = configFechaIni,
                value = fechaInicio
            });

            data.Add(new DisambiguationData()
            {
                property = "entidadRealizacion",
                config = configEntidadRealizacion,
                value = entidadRealizacion
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
                    EvalRevIDI evalRevIDI = new EvalRevIDI();
                    evalRevIDI.ID = fila["item"].value;
                    evalRevIDI.descripcion = fila["itemTitle"].value;
                    evalRevIDI.nombreActividad = "";
                    if (fila.ContainsKey("itemNombre"))
                    {
                        evalRevIDI.nombreActividad = fila["itemNombre"].value;
                    }
                    evalRevIDI.fechaInicio = "";
                    if (fila.ContainsKey("itemFecha"))
                    {
                        evalRevIDI.fechaInicio = fila["itemFecha"].value;
                    }
                    evalRevIDI.entidadRealizacion = "";
                    if (fila.ContainsKey("itemER"))
                    {
                        evalRevIDI.entidadRealizacion = fila["itemER"].value;
                    }
                    resultados.Add(pResourceApi.GetShortGuid(fila["item"].value).ToString(), evalRevIDI);
                }
            }

            return resultados;
        }
    }
}
