using Gnoss.ApiWrapper;
using Gnoss.ApiWrapper.ApiModel;
using Hercules.CommonsEDMA.DisambiguationEngine.Models;
using System.Collections.Generic;
using System.Linq;
using Utils;
using static Gnoss.ApiWrapper.ApiModel.SparqlObject;

namespace ImportadorWebCV.Sincro.Secciones.ActividadDocenteSubclases
{
    class PublicacionDocentes : DisambiguableEntity
    {
        public string Descripcion { get; set; }
        public string Fecha { get; set; }
        public string TituloPublicacion { get; set; }

        private static readonly DisambiguationDataConfig configDescripcionPubDoc = new(DisambiguationDataConfigType.equalsTitle, 0.8f);
        private static readonly DisambiguationDataConfig configFechaPubDoc = new(DisambiguationDataConfigType.equalsItem, 0.5f, 0.5f);
        private static readonly DisambiguationDataConfig configTituloPubDoc = new(DisambiguationDataConfigType.equalsItem, 0.5f, 0.5f);

        public override List<DisambiguationData> GetDisambiguationData()
        {
            List<DisambiguationData> data = new()
            {
                new DisambiguationData(configDescripcionPubDoc, "descripcion", Descripcion),
                new DisambiguationData(configFechaPubDoc, "fecha", Fecha),
                new DisambiguationData(configTituloPubDoc, "tituloPublicacion", TituloPublicacion)
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
        public static Dictionary<string, DisambiguableEntity> GetBBDDPubDoc(ResourceApi pResourceApi, string pCVID, string graph, List<string> propiedadesItem)
        {
            //Obtenemos IDS
            HashSet<string> ids = UtilitySecciones.GetIDS(pResourceApi, pCVID, propiedadesItem);

            Dictionary<string, DisambiguableEntity> resultadosPubDoc = new();

            //Divido la lista en listas de elementos
            List<List<string>> listaListas = UtilitySecciones.SplitList(ids.ToList(), Utility.splitListNum).ToList();

            foreach (List<string> lista in listaListas)
            {
                string select = $@"SELECT distinct ?item ?itemTitle ?itemDate ?itemTitulo ";
                string where = $@"where {{
                                        ?item <{Variables.ActividadDocente.publicacionDocenteNombre}> ?itemTitle . 
                                        OPTIONAL{{?item <{Variables.ActividadDocente.publicacionDocenteFechaElaboracion}> ?itemDate }} . 
                                        OPTIONAL{{?item <{Variables.ActividadDocente.publicacionDocenteTituloPublicacion}> ?itemTitulo }} .
                                        FILTER(?item in (<{string.Join(">,<", lista)}>))
                                    }}";
                SparqlObject resultData = pResourceApi.VirtuosoQuery(select, where, graph);
                foreach (Dictionary<string, Data> fila in resultData.results.bindings)
                {
                    PublicacionDocentes publicacionDocentes = new()
                    {
                        ID = fila["item"].value,
                        Descripcion = fila["itemTitle"].value,
                        Fecha = fila.ContainsKey("itemDate") ? fila["itemDate"].value : "",
                        TituloPublicacion = fila.ContainsKey("itemTitulo") ? fila["itemTitulo"].value : ""
                    };

                    resultadosPubDoc.Add(pResourceApi.GetShortGuid(fila["item"].value).ToString(), publicacionDocentes);
                }
            }

            return resultadosPubDoc;
        }
    }
}
