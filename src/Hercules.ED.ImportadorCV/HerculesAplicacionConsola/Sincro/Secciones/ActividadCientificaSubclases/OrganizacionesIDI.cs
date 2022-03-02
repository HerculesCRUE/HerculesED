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
    class OrganizacionesIDI : DisambiguableEntity
    {
        public string descripcion { get; set; }
        public string fecha { get; set; }
        public string tipoActividad { get; set; }
        public string entidadConvocante { get; set; }

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
        
        private static DisambiguationDataConfig configTipoActividad = new DisambiguationDataConfig()
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
                property = "tipoActividad",
                config = configTipoActividad,
                value = tipoActividad
            });

            data.Add(new DisambiguationData()
            {
                property = "entidadConvocante",
                config = configEC,
                value = entidadConvocante
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
                string select = $@"SELECT distinct ?item ?itemTitle ?itemTipo ?itemEC";
                string where = $@"where {{
                                        ?item <{Variables.ActividadCientificaTecnologica.orgIDITituloActividad}> ?itemTitle . 
                                        OPTIONAL{{ ?item <{Variables.ActividadCientificaTecnologica.orgIDIFechaInicio}> ?itemDate }} .
                                        OPTIONAL{{ ?item <{Variables.ActividadCientificaTecnologica.orgIDITipoActividad}> ?itemTipo }} .
                                        OPTIONAL{{ ?item <{Variables.ActividadCientificaTecnologica.orgIDIEntidadConvocanteNombre}> ?itemEC }} .
                                        FILTER(?item in (<{string.Join(">,<", lista)}>))
                                    }}";

                SparqlObject resultData = pResourceApi.VirtuosoQuery(select, where, graph);
                foreach (Dictionary<string, Data> fila in resultData.results.bindings)
                {
                    OrganizacionesIDI organizacionesIDI = new OrganizacionesIDI();
                    organizacionesIDI.ID = fila["item"].value;
                    organizacionesIDI.descripcion = fila["itemTitle"].value;
                    organizacionesIDI.fecha = "";
                    if (fila.ContainsKey("itemDate"))
                    {
                        organizacionesIDI.fecha = fila["itemDate"].value;
                    }
                    organizacionesIDI.tipoActividad = "";
                    if (fila.ContainsKey("itemTipo"))
                    {
                        organizacionesIDI.tipoActividad = fila["itemTipo"].value;
                    }
                    organizacionesIDI.entidadConvocante = "";
                    if (fila.ContainsKey("itemEC"))
                    {
                        organizacionesIDI.entidadConvocante = fila["itemEC"].value;
                    }
                    resultados.Add(pResourceApi.GetShortGuid(fila["item"].value).ToString(), organizacionesIDI);
                }
            }

            return resultados;
        }
    }
}
