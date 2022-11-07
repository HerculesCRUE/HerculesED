using Gnoss.ApiWrapper;
using Gnoss.ApiWrapper.ApiModel;
using Hercules.CommonsEDMA.DisambiguationEngine.Models;
using Utils;
using System.Collections.Generic;
using System.Linq;
using static Gnoss.ApiWrapper.ApiModel.SparqlObject;

namespace ImportadorWebCV.Sincro.Secciones.ActividadDocenteSubclases
{
    class OtrasActividades : DisambiguableEntity
    {
        public string Descripcion { get; set; }
        public string Fecha { get; set; }
        public string EntidadOrganizadora { get; set; }

        private static readonly DisambiguationDataConfig configDescripcionOtrAct = new(DisambiguationDataConfigType.equalsTitle, 0.8f);
        private static readonly DisambiguationDataConfig configFechaOtrAct = new(DisambiguationDataConfigType.equalsItem, 0.5f, 0.5f);
        private static readonly DisambiguationDataConfig configEntOrgOtrAct = new(DisambiguationDataConfigType.equalsItem, 0.5f, 0.5f);

        public override List<DisambiguationData> GetDisambiguationData()
        {
            List<DisambiguationData> data = new()
            {
                new DisambiguationData(configDescripcionOtrAct, "descripcion", Descripcion),
                new DisambiguationData(configFechaOtrAct, "fecha", Fecha),
                new DisambiguationData(configEntOrgOtrAct, "entidadOrganizadora", EntidadOrganizadora)
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
        public static Dictionary<string, DisambiguableEntity> GetBBDDOtrAct(ResourceApi pResourceApi, string pCVID, string graph, List<string> propiedadesItem)
        {
            //Obtenemos IDS
            HashSet<string> ids = UtilitySecciones.GetIDS(pResourceApi, pCVID, propiedadesItem);

            Dictionary<string, DisambiguableEntity> resultadosOtrAct = new();

            //Divido la lista en listas de elementos
            List<List<string>> listaListas = UtilitySecciones.SplitList(ids.ToList(), Utility.splitListNum).ToList();

            foreach (List<string> lista in listaListas)
            {
                string select = $@"SELECT distinct ?item ?itemTitle ?itemDate ?itemEO ";
                string where = $@"where {{
                                        ?item <{Variables.ActividadDocente.otrasActividadesDescripcion}> ?itemTitle . 
                                        OPTIONAL{{?item <{Variables.ActividadDocente.otrasActividadesFechaFinalizacion}> ?itemDate }}.
                                        OPTIONAL{{?item <{Variables.ActividadDocente.otrasActividadesEntidadOrganizadoraNombre}> ?itemEO }}.
                                        FILTER(?item in (<{string.Join(">,<", lista)}>))
                                    }}";
                SparqlObject resultData = pResourceApi.VirtuosoQuery(select, where, graph);
                foreach (Dictionary<string, Data> fila in resultData.results.bindings)
                {
                    OtrasActividades otrasActividades = new OtrasActividades
                    {
                        ID = fila["item"].value,
                        Descripcion = fila["itemTitle"].value,
                        Fecha = fila.ContainsKey("itemDate") ? fila["itemDate"].value : "",
                        EntidadOrganizadora = fila.ContainsKey("itemEO") ? fila["itemEO"].value : ""
                    };

                    resultadosOtrAct.Add(pResourceApi.GetShortGuid(fila["item"].value).ToString(), otrasActividades);
                }
            }

            return resultadosOtrAct;
        }
    }
}
