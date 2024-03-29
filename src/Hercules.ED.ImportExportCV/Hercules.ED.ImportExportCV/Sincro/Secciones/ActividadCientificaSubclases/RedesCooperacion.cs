﻿using Gnoss.ApiWrapper;
using Gnoss.ApiWrapper.ApiModel;
using Hercules.CommonsEDMA.DisambiguationEngine.Models;
using System.Collections.Generic;
using System.Linq;
using Utils;
using static Gnoss.ApiWrapper.ApiModel.SparqlObject;

namespace ImportadorWebCV.Sincro.Secciones.ActividadCientificaSubclases
{
    class RedesCooperacion : DisambiguableEntity
    {
        public string Descripcion { get; set; }
        public string IdRed { get; set; }

        private static readonly DisambiguationDataConfig configDescripcionRedCoop = new(DisambiguationDataConfigType.equalsTitle, 0.8f);
        private static readonly DisambiguationDataConfig configIdRedRedCoop = new(DisambiguationDataConfigType.equalsItem, 0.5f, 0.5f);

        public override List<DisambiguationData> GetDisambiguationData()
        {
            List<DisambiguationData> data = new()
            {
                new DisambiguationData(configDescripcionRedCoop, "descripcion", Descripcion),
                new DisambiguationData(configIdRedRedCoop, "idRed", IdRed)
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
        public static Dictionary<string, DisambiguableEntity> GetBBDDRedCoop(ResourceApi pResourceApi, string pCVID, string graph, List<string> propiedadesItem)
        {
            //Obtenemos IDS
            HashSet<string> ids = UtilitySecciones.GetIDS(pResourceApi, pCVID, propiedadesItem);

            Dictionary<string, DisambiguableEntity> resultadosRedCoop = new();

            //Divido la lista en listas de elementos
            List<List<string>> listaListas = UtilitySecciones.SplitList(ids.ToList(), Utility.splitListNum).ToList();

            foreach (List<string> lista in listaListas)
            {
                string select = $@"SELECT distinct ?item ?itemTitle ?itemID ";
                string where = $@"where {{
                                        ?item <{Variables.ActividadCientificaTecnologica.redesCoopNombre}> ?itemTitle . 
                                        OPTIONAL{{?item <{Variables.ActividadCientificaTecnologica.redesCoopIdentificacion}> ?itemID }}.
                                        FILTER(?item in (<{string.Join(">,<", lista)}>))
                                    }}";

                SparqlObject resultData = pResourceApi.VirtuosoQuery(select, where, graph);
                foreach (Dictionary<string, Data> fila in resultData.results.bindings)
                {
                    RedesCooperacion redesCooperacion = new RedesCooperacion
                    {
                        ID = fila["item"].value,
                        Descripcion = fila["itemTitle"].value,
                        IdRed = fila.ContainsKey("itemID") ? fila["itemID"].value : ""
                    };

                    resultadosRedCoop.Add(pResourceApi.GetShortGuid(fila["item"].value).ToString(), redesCooperacion);
                }
            }

            return resultadosRedCoop;
        }
    }
}
