﻿using Gnoss.ApiWrapper;
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
    class OtrasActividadesDivulgacion : DisambiguableEntity
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
                string select = $@"SELECT distinct ?item ?itemTitle ?itemDate ";
                string where = $@"where {{
                                        ?item <{Variables.ActividadCientificaTecnologica.otrasActDivulTitulo}> ?itemTitle . 
                                        OPTIONAL{{ ?item <{Variables.ActividadCientificaTecnologica.otrasActDivulPubFecha}> ?itemDate }} .
                                        FILTER(?item in (<{string.Join(">,<", lista)}>))
                                    }}";

                SparqlObject resultData = pResourceApi.VirtuosoQuery(select, where, graph);
                foreach (Dictionary<string, Data> fila in resultData.results.bindings)
                {
                    OtrasActividadesDivulgacion otrasActividades = new OtrasActividadesDivulgacion();
                    otrasActividades.ID = fila["item"].value;
                    otrasActividades.descripcion = fila["itemTitle"].value;
                    otrasActividades.fecha = "";
                    if (fila.ContainsKey("itemDate"))
                    {
                        otrasActividades.fecha = fila["itemDate"].value;
                    }
                    resultados.Add(pResourceApi.GetShortGuid(fila["item"].value).ToString(), otrasActividades);
                }
            }

            return resultados;
        }
    }
}
