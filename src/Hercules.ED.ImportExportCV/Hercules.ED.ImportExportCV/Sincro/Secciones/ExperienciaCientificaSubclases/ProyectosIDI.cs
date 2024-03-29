﻿using Gnoss.ApiWrapper;
using Gnoss.ApiWrapper.ApiModel;
using Hercules.CommonsEDMA.DisambiguationEngine.Models;
using System.Collections.Generic;
using System.Linq;
using Utils;
using static Gnoss.ApiWrapper.ApiModel.SparqlObject;

namespace ImportadorWebCV.Sincro.Secciones.ExperienciaCientificaSubclases
{
    class ProyectosIDI : DisambiguableEntity
    {
        public string Nombre { get; set; }

        private static readonly DisambiguationDataConfig configNombreProyIdi = new(DisambiguationDataConfigType.equalsTitle, 1f);

        public override List<DisambiguationData> GetDisambiguationData()
        {
            List<DisambiguationData> data = new()
            {
                new DisambiguationData(configNombreProyIdi, "nombre", Nombre)
            };
            return data;
        }

        public static Dictionary<string, DisambiguableEntity> GetBBDDProyIdi(ResourceApi pResourceApi, string pCVID, string graph, List<string> propiedadesItem)
        {
            //Obtenemos IDS
            HashSet<string> ids = UtilitySecciones.GetIDS(pResourceApi, pCVID, propiedadesItem);

            Dictionary<string, DisambiguableEntity> resultadosProyIdi = new();

            //Divido la lista en listas de elementos
            List<List<string>> listaListas = UtilitySecciones.SplitList(ids.ToList(), Utility.splitListNum).ToList();

            foreach (List<string> lista in listaListas)
            {
                string select = $@"SELECT distinct ?item ?itemTitle ?isValidated ";
                string where = $@"where {{
                                        ?item <{Variables.ExperienciaCientificaTecnologica.proyectosIDINombre}> ?itemTitle . 
                                        ?item <http://w3id.org/roh/isValidated> ?isValidated .
                                        FILTER(?item in (<{string.Join(">,<", lista)}>))
                                    }}";
                SparqlObject resultData = pResourceApi.VirtuosoQuery(select, where, graph);
                foreach (Dictionary<string, Data> fila in resultData.results.bindings)
                {
                    ProyectosIDI proyectosIDI = new ProyectosIDI
                    {
                        ID = fila["item"].value,
                        Nombre = fila["itemTitle"].value,
                        block = fila["isValidated"].value.Equals("true")
                    };

                    resultadosProyIdi.Add(pResourceApi.GetShortGuid(fila["item"].value).ToString(), proyectosIDI);
                }
            }

            return resultadosProyIdi;
        }
    }
}
