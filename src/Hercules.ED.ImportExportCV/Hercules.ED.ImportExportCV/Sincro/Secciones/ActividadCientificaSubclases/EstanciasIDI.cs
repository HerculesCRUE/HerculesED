﻿using Gnoss.ApiWrapper;
using Gnoss.ApiWrapper.ApiModel;
using Hercules.CommonsEDMA.DisambiguationEngine.Models;
using System.Collections.Generic;
using System.Linq;
using Utils;
using static Gnoss.ApiWrapper.ApiModel.SparqlObject;

namespace ImportadorWebCV.Sincro.Secciones.ActividadCientificaSubclases
{
    class EstanciasIDI : DisambiguableEntity
    {
        public string Descripcion { get; set; }
        public string Fecha { get; set; }
        public string EntidadRealizacion { get; set; }

        private static readonly DisambiguationDataConfig configDescripcionEstIdi = new(DisambiguationDataConfigType.equalsTitle, 0.8f);
        private static readonly DisambiguationDataConfig configFechaEstIdi = new(DisambiguationDataConfigType.equalsItem, 0.5f, 0.5f);
        private static readonly DisambiguationDataConfig configEntReaEstIdi = new(DisambiguationDataConfigType.equalsItem, 0.5f, 0.5f);

        public override List<DisambiguationData> GetDisambiguationData()
        {
            List<DisambiguationData> data = new()
            {
                new DisambiguationData(configDescripcionEstIdi, "descripcion", Descripcion),
                new DisambiguationData(configFechaEstIdi, "fecha", Fecha),
                new DisambiguationData(configEntReaEstIdi, "entidadRealizacion", EntidadRealizacion)
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
        public static Dictionary<string, DisambiguableEntity> GetBBDDEstIdi(ResourceApi pResourceApi, string pCVID, string graph, List<string> propiedadesItem)
        {
            //Obtenemos IDS
            HashSet<string> ids = UtilitySecciones.GetIDS(pResourceApi, pCVID, propiedadesItem);

            Dictionary<string, DisambiguableEntity> resultadosEstIdi = new();

            //Divido la lista en listas de elementos
            List<List<string>> listaListas = UtilitySecciones.SplitList(ids.ToList(), Utility.splitListNum).ToList();

            foreach (List<string> lista in listaListas)
            {
                string select = $@"SELECT distinct ?item ?itemTitle ?itemDate ?itemER ";
                string where = $@"where {{
                                        ?item <{Variables.ActividadCientificaTecnologica.estanciasIDITareasContrastables}> ?itemTitle . 
                                        OPTIONAL{{ ?item <{Variables.ActividadCientificaTecnologica.estanciasIDIFechaInicioEntidadRealizacion}> ?itemDate }} .
                                        OPTIONAL{{ ?item <{Variables.ActividadCientificaTecnologica.estanciasIDIEntidadRealizacionNombre}> ?itemER }} .
                                        FILTER(?item in (<{string.Join(">,<", lista)}>))
                                    }}";

                SparqlObject resultData = pResourceApi.VirtuosoQuery(select, where, graph);
                foreach (Dictionary<string, Data> fila in resultData.results.bindings)
                {
                    EstanciasIDI estanciasIDI = new EstanciasIDI
                    {
                        ID = fila["item"].value,
                        Descripcion = fila["itemTitle"].value,
                        Fecha = fila.ContainsKey("itemDate") ? fila["itemDate"].value : "",
                        EntidadRealizacion = fila.ContainsKey("itemER") ? fila["itemER"].value : ""
                    };

                    resultadosEstIdi.Add(pResourceApi.GetShortGuid(fila["item"].value).ToString(), estanciasIDI);
                }
            }

            return resultadosEstIdi;
        }
    }
}
