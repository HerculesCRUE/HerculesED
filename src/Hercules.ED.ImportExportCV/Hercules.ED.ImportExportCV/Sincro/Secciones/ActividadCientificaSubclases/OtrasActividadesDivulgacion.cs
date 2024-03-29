﻿using Gnoss.ApiWrapper;
using Gnoss.ApiWrapper.ApiModel;
using Hercules.CommonsEDMA.DisambiguationEngine.Models;
using System.Collections.Generic;
using System.Linq;
using Utils;
using static Gnoss.ApiWrapper.ApiModel.SparqlObject;

namespace ImportadorWebCV.Sincro.Secciones.ActividadCientificaSubclases
{
    class OtrasActividadesDivulgacion : DisambiguableEntity
    {
        public string TituloTrabajo { get; set; }
        public string Fecha { get; set; }
        public string NombreEvento { get; set; }

        private static readonly DisambiguationDataConfig configTituloOtAcDiv = new(DisambiguationDataConfigType.equalsTitle, 0.8f);
        private static readonly DisambiguationDataConfig configFechaOtAcDiv = new(DisambiguationDataConfigType.equalsItem, 0.5f, 0.5f);
        private static readonly DisambiguationDataConfig configNombreEventoOtAcDiv = new(DisambiguationDataConfigType.equalsItem, 0.5f, 0.5f);

        public override List<DisambiguationData> GetDisambiguationData()
        {
            List<DisambiguationData> data = new()
            {
                new DisambiguationData(configTituloOtAcDiv, "tituloTrabajo", TituloTrabajo),
                new DisambiguationData(configFechaOtAcDiv, "fecha", Fecha),
                new DisambiguationData(configNombreEventoOtAcDiv, "nombreEvento", NombreEvento)
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
        public static Dictionary<string, DisambiguableEntity> GetBBDDOtAcDiv(ResourceApi pResourceApi, string pCVID, string graph, List<string> propiedadesItem)
        {
            //Obtenemos IDS
            HashSet<string> ids = UtilitySecciones.GetIDS(pResourceApi, pCVID, propiedadesItem);

            Dictionary<string, DisambiguableEntity> resultadosOtAcDiv = new();

            //Divido la lista en listas de elementos
            List<List<string>> listaListas = UtilitySecciones.SplitList(ids.ToList(), Utility.splitListNum).ToList();

            foreach (List<string> lista in listaListas)
            {
                string select = $@"SELECT distinct ?item ?itemTitle ?itemDate ?itemNombre ";
                string where = $@"where {{
                                        ?item <{Variables.ActividadCientificaTecnologica.otrasActDivulTitulo}> ?itemTitle . 
                                        OPTIONAL{{ ?item <{Variables.ActividadCientificaTecnologica.otrasActDivulPubFecha}> ?itemDate }} .
                                        OPTIONAL{{ ?item <{Variables.ActividadCientificaTecnologica.otrasActDivulNombreEvento}> ?itemNombre }} .
                                        FILTER(?item in (<{string.Join(">,<", lista)}>))
                                    }}";

                SparqlObject resultData = pResourceApi.VirtuosoQuery(select, where, graph);
                foreach (Dictionary<string, Data> fila in resultData.results.bindings)
                {
                    OtrasActividadesDivulgacion otrasActividades = new OtrasActividadesDivulgacion
                    {
                        ID = fila["item"].value,
                        TituloTrabajo = fila["itemTitle"].value,
                        Fecha = fila.ContainsKey("itemDate") ? fila["itemDate"].value : "",
                        NombreEvento = fila.ContainsKey("itemNombre") ? fila["itemNombre"].value : ""
                    };

                    resultadosOtAcDiv.Add(pResourceApi.GetShortGuid(fila["item"].value).ToString(), otrasActividades);
                }
            }

            return resultadosOtAcDiv;
        }
    }
}
