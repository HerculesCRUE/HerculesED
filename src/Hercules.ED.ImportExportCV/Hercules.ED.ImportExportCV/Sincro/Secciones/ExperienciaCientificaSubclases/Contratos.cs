﻿using Gnoss.ApiWrapper;
using Gnoss.ApiWrapper.ApiModel;
using Hercules.CommonsEDMA.DisambiguationEngine.Models;
using System.Collections.Generic;
using System.Linq;
using Utils;
using static Gnoss.ApiWrapper.ApiModel.SparqlObject;

namespace ImportadorWebCV.Sincro.Secciones.ExperienciaCientificaSubclases
{
    class Contratos : DisambiguableEntity
    {
        public string Nombre { get; set; }

        private static readonly DisambiguationDataConfig configNombreContr = new(DisambiguationDataConfigType.equalsTitle, 1f);

        public override List<DisambiguationData> GetDisambiguationData()
        {
            List<DisambiguationData> data = new()
            {
                new DisambiguationData(configNombreContr, "nombre", Nombre)
            };
            return data;
        }

        public static Dictionary<string, DisambiguableEntity> GetBBDDContr(ResourceApi pResourceApi, string pCVID, string graph, List<string> propiedadesItem)
        {
            //Obtenemos IDS
            HashSet<string> ids = UtilitySecciones.GetIDS(pResourceApi, pCVID, propiedadesItem);

            Dictionary<string, DisambiguableEntity> resultadosContr = new();

            //Divido la lista en listas de elementos
            List<List<string>> listaListas = UtilitySecciones.SplitList(ids.ToList(), Utility.splitListNum).ToList();

            foreach (List<string> lista in listaListas)
            {
                string select = $@"SELECT distinct ?item ?itemTitle ?isValidated ";
                string where = $@"where {{
                                        ?item <{Variables.ExperienciaCientificaTecnologica.contratosNombreProyecto}> ?itemTitle . 
                                        ?item <http://w3id.org/roh/isValidated> ?isValidated .
                                        FILTER(?item in (<{string.Join(">,<", lista)}>))
                                    }}";
                SparqlObject resultData = pResourceApi.VirtuosoQuery(select, where, graph);
                foreach (Dictionary<string, Data> fila in resultData.results.bindings)
                {
                    Contratos contratos = new Contratos
                    {
                        ID = fila["item"].value,
                        Nombre = fila["itemTitle"].value,
                        block = fila["isValidated"].value.Equals("true")
                    };

                    resultadosContr.Add(pResourceApi.GetShortGuid(fila["item"].value).ToString(), contratos);
                }
            }

            return resultadosContr;
        }
    }
}
