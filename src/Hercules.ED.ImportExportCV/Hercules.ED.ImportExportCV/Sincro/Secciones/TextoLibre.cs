using Hercules.ED.ImportExportCV.Controllers;
using Hercules.ED.ImportExportCV.Models;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Utils;
using static Models.Entity;
namespace ImportadorWebCV.Sincro.Secciones
{
    class TextoLibre : SeccionBase
    {
        private readonly List<CvnItemBean> listadoDatos;

        public TextoLibre(cvnRootResultBean cvn, string cvID, ConfigService configuracion) : base(cvn, cvID, configuracion)
        {
            listadoDatos = mCvn.GetListadoBloque("070");
        }

        /// <summary>
        /// Función para sincronizar los datos pertenecientes al 
        /// subapartado "Texto libre"
        /// Con código identificativo "070.010.000.000".
        /// </summary>
        public List<SubseccionItem> SincroTextoLibre(bool procesar, [Optional] bool preimportar, [Optional] List<string> listadoIdBBDD)
        {
            //Si procesar es false, no hago nada.
            if (!procesar)
            {
                return null;
            }

            List<string> propiedadesItem = new List<string>() { "http://w3id.org/roh/freeTextSummary", "http://w3id.org/roh/freeTextSummaryValues", "http://w3id.org/roh/freeTextSummaryValuesCV" };
            List<string> rdfTypeItem = new List<string>() { "http://w3id.org/roh/FreeTextSummaryValues", "http://w3id.org/roh/FreeTextSummaryValuesCV" };

            //1º Obtenemos la entidad de BBDD.
            Tuple<string, string, string> identificadores = GetIdentificadoresItemPresentation(mCvID, propiedadesItem, rdfTypeItem);

            //2º Obtenemos la entidad de BBDD.
            Entity entityBBDD = null;
            GetEntidadesSecundarias(ref entityBBDD, identificadores, rdfTypeItem, "curriculumvitae");

            //3º Obtenemos la entidad de los datos del XML.
            Entity entityXML = ObtenerTextoLibre(listadoDatos);

            if (preimportar)
            {
                List<SubseccionItem> listaAux = new List<SubseccionItem>();
                if (entityBBDD == null)
                {
                    listaAux.Add(new SubseccionItem(0, null, entityXML.properties));
                }
                else
                {
                    listaAux.Add(new SubseccionItem(0, entityBBDD.id, entityXML.properties));
                }
                return listaAux;
            }
            else
            {
                if (listadoIdBBDD != null && listadoIdBBDD.Count > 0 && listadoIdBBDD.Any(x => x.StartsWith("http://gnoss.com/items/FreeTextSummaryValuesCV_")))
                {
                    List<string> listadoEliminacion = new List<string>() { "0", "1", "2" };
                    string numero;
                    for (int i = 0; i < listadoIdBBDD.Count; i++)
                    {
                        numero = listadoIdBBDD.ElementAt(i).Split("@@@").Last();
                        if (!string.IsNullOrEmpty(numero))
                        {
                            listadoEliminacion.Remove(numero);
                        }
                    }
                    foreach (string eliminar in listadoEliminacion)
                    {
                        if (eliminar.Equals("0") && entityXML.properties.Any(x => x.prop.Equals(Variables.TextoLibre.resumenLibre)))
                        {
                            entityXML.properties.Remove(entityXML.properties.First(x => x.prop.Equals(Variables.TextoLibre.resumenLibre)));
                        }
                        if (eliminar.Equals("1") && entityXML.properties.Any(x => x.prop.Equals(Variables.TextoLibre.b1DescripcionTFG)))
                        {
                            entityXML.properties.Remove(entityXML.properties.First(x => x.prop.Equals(Variables.TextoLibre.b1DescripcionTFG)));
                        }
                        if (eliminar.Equals("2") && entityXML.properties.Any(x => x.prop.Equals(Variables.TextoLibre.b2DescripcionTFM)))
                        {
                            entityXML.properties.Remove(entityXML.properties.First(x => x.prop.Equals(Variables.TextoLibre.b2DescripcionTFM)));
                        }
                    }
                }
                UpdateEntityAux(mResourceApi.GetShortGuid(mCvID), propiedadesItem, new List<string>() { identificadores.Item1, identificadores.Item2, identificadores.Item3 }, entityBBDD, entityXML);
                if (listadoIdBBDD != null && listadoIdBBDD.Count > 0 && listadoIdBBDD.Any(x => x.StartsWith("http://gnoss.com/items/FreeTextSummaryValuesCV_")))
                {
                    listadoIdBBDD.RemoveAll(x => x.StartsWith("http://gnoss.com/items/FreeTextSummaryValuesCV_"));
                }
                return null;
            }
        }

        private Entity ObtenerTextoLibre(List<CvnItemBean> listadoDatosIdentificacion)
        {
            try
            {
                Entity entity = new Entity();
                entity.properties = new List<Property>();
                List<(string, string)> resumen = GetResumen(listadoDatosIdentificacion.GetElementoPorIDCampo<CvnItemBeanCvnRichText>("070.010.000.010"));
                if (resumen == null) { return entity; }

                if (resumen.Count == 3)
                {
                    entity.properties = UtilitySecciones.AddProperty(
                        new Property(Variables.TextoLibre.resumenLibre, resumen.FirstOrDefault(x => x.Item1.Equals("resumenLibre")).Item2),
                        new Property(Variables.TextoLibre.b1DescripcionTFG, resumen.FirstOrDefault(x => x.Item1.Equals("TFG")).Item2),
                        new Property(Variables.TextoLibre.b2DescripcionTFM, resumen.FirstOrDefault(x => x.Item1.Equals("TFM")).Item2)
                    );
                }
                else
                {
                    entity.properties = new List<Property>();
                }

                return entity;
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e);
                return null;
            }
        }

        private List<(string, string)> GetResumen(CvnItemBeanCvnRichText item)
        {
            if (item == null) { return null; }

            string b1 = "B.1. Breve descripción del Trabajo de Fin de Grado (TFG) y puntuación obtenida";
            string b2 = "B.2. Breve descripción del Trabajo de Fin de Máster (TFM) y puntuación obtenida";

            string resumenLibre = "";
            string TFG = "";
            string TFM = "";

            string resumen = item.Value;
            if (resumen.Contains(b1) && resumen.Contains(b2))
            {
                resumenLibre = resumen.Split(b1, 2).First();
                TFG = resumen.Split(b1, 2).Last().Split(b2, 2).First();
                TFM = resumen.Split(b2, 2).Last();
            }
            else if (resumen.Contains(b1))
            {
                resumenLibre = resumen.Split(b1, 2).First();
                TFG = resumen.Split(b1, 2).Last();
            }
            else if (resumen.Contains(b2))
            {
                resumenLibre = resumen.Split(b2, 2).First();
                TFM = resumen.Split(b2, 2).Last();
            }
            else
            {
                resumenLibre = resumen;
            }

            List<(string, string)> listadoResumen = new List<(string, string)> { ("resumenLibre", resumenLibre.Trim()), ("TFG", TFG.Trim()), ("TFM", TFM.Trim()) };

            return listadoResumen;
        }

        private void GetEntidadesSecundarias(ref Entity entityBBDD, Tuple<string, string, string> identificadores, List<string> rdfTypeItem, string graph)
        {
            if (!string.IsNullOrEmpty(identificadores.Item3))
            {
                entityBBDD = GetLoadedEntity(identificadores.Item3, graph);
            }
            else
            {
                string item1 = identificadores.Item1;
                string item2 = identificadores.Item2;
                string item3 = identificadores.Item3;
                if (string.IsNullOrEmpty(item2))
                {
                    string nombreEntidad = rdfTypeItem.First();
                    if (nombreEntidad.Contains('#'))
                    {
                        nombreEntidad = nombreEntidad.Substring(nombreEntidad.LastIndexOf("#") + 1);
                    }
                    if (nombreEntidad.Contains('/'))
                    {
                        nombreEntidad = nombreEntidad.Substring(nombreEntidad.LastIndexOf("/") + 1);
                    }
                    item2 = mResourceApi.GraphsUrl + "items/" + nombreEntidad + "_" + mResourceApi.GetShortGuid(mCvID).ToString().ToLower() + "_" + Guid.NewGuid().ToString().ToLower();
                }
                if (string.IsNullOrEmpty(item3))
                {
                    string nombreEntidad = rdfTypeItem[1];
                    if (nombreEntidad.Contains('#'))
                    {
                        nombreEntidad = nombreEntidad.Substring(nombreEntidad.LastIndexOf("#") + 1);
                    }
                    if (nombreEntidad.Contains('/'))
                    {
                        nombreEntidad = nombreEntidad.Substring(nombreEntidad.LastIndexOf("/") + 1);
                    }
                    item3 = mResourceApi.GraphsUrl + "items/" + nombreEntidad + "_" + mResourceApi.GetShortGuid(mCvID).ToString().ToLower() + "_" + Guid.NewGuid().ToString().ToLower();
                }
                identificadores = new Tuple<string, string, string>(item1, item2, item3);
            }
        }
    }
}
