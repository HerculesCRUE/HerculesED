using Gnoss.ApiWrapper;
using Gnoss.ApiWrapper.ApiModel;
using Hercules.CommonsEDMA.DisambiguationEngine.Models;
using Utils;
using System.Collections.Generic;
using System.Linq;
using static Gnoss.ApiWrapper.ApiModel.SparqlObject;

namespace ImportadorWebCV.Sincro.Secciones.ActividadDocenteSubclases
{
    class PremiosInnovacionDocente : DisambiguableEntity
    {
        public string Nombre { get; set; }
        public string Fecha { get; set; }
        public string EntidadConcesion { get; set; }

        private static readonly DisambiguationDataConfig configNombrePreInnDoc = new(DisambiguationDataConfigType.equalsTitle, 0.8f);
        private static readonly DisambiguationDataConfig configFechaPreInnDoc = new(DisambiguationDataConfigType.equalsItem, 0.5f, 0.5f);
        private static readonly DisambiguationDataConfig configEntConPreInnDoc = new(DisambiguationDataConfigType.equalsItem, 0.5f, 0.5f);

        public override List<DisambiguationData> GetDisambiguationData()
        {
            List<DisambiguationData> data = new()
            {
                new DisambiguationData(configNombrePreInnDoc,"descripcion",Nombre),
                new DisambiguationData(configFechaPreInnDoc,"fecha",Fecha),
                new DisambiguationData(configEntConPreInnDoc, "entidadConcesion",EntidadConcesion)
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
        public static Dictionary<string, DisambiguableEntity> GetBBDDPreInnDoc(ResourceApi pResourceApi, string pCVID, string graph, List<string> propiedadesItem)
        {
            //Obtenemos IDS
            HashSet<string> ids = UtilitySecciones.GetIDS(pResourceApi, pCVID, propiedadesItem);

            Dictionary<string, DisambiguableEntity> resultadosPreInnDoc = new ();

            //Divido la lista en listas de elementos
            List<List<string>> listaListas = UtilitySecciones.SplitList(ids.ToList(), Utility.splitListNum).ToList();

            foreach (List<string> lista in listaListas)
            {
                string select = $@"SELECT distinct ?item ?itemTitle ?itemDate ?itemEC ";
                string where = $@"where {{
                                        ?item <{Variables.ActividadDocente.premiosInnovaNombre}> ?itemTitle . 
                                        OPTIONAL{{?item <{Variables.ActividadDocente.premiosInnovaFechaConcesion}> ?itemDate }}.
                                        OPTIONAL{{?item <{Variables.ActividadDocente.premiosInnovaEntidadConcesionariaNombre}> ?itemEC }}.
                                        FILTER(?item in (<{string.Join(">,<", lista)}>))
                                    }}";
                SparqlObject resultData = pResourceApi.VirtuosoQuery(select, where, graph);
                foreach (Dictionary<string, Data> fila in resultData.results.bindings)
                {
                    PremiosInnovacionDocente premiosInnovacion = new ()
                    {
                        ID = fila["item"].value,
                        Nombre = fila["itemTitle"].value,
                        Fecha = fila.ContainsKey("itemDate") ? fila["itemDate"].value : "",
                        EntidadConcesion = fila.ContainsKey("itemEC") ? fila["itemEC"].value : ""
                    };

                    resultadosPreInnDoc.Add(pResourceApi.GetShortGuid(fila["item"].value).ToString(), premiosInnovacion);
                }
            }

            return resultadosPreInnDoc;
        }
    }
}
