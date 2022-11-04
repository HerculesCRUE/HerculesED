using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Utils;
using static Gnoss.ApiWrapper.ApiModel.SparqlObject;

namespace ImportadorWebCV.Exporta.Secciones.ActividadCientificaSubclases
{
    public class ProduccionCientifica : SeccionBase
    {
        private readonly List<string> propiedadesItem = new()
        {
            "http://w3id.org/roh/scientificActivity",
            "http://w3id.org/roh/scientificProduction",
            "http://vivoweb.org/ontology/core#relatedBy"
        };
        private readonly string graph = "scientificproduction";
        public ProduccionCientifica(cvnRootResultBean cvn, string cvID) : base(cvn, cvID)
        {
        }

        /// <summary>
        /// Exporta los datos de la sección "060.010.000.000" a cvn.cvnRootResultBean
        /// </summary>
        /// <param name="MultilangProp"></param>
        /// <param name="listaId"></param>
        public void ExportaProduccionCientifica(Dictionary<string, List<Dictionary<string, Data>>> MultilangProp, string versionExportacion, [Optional] List<string> listaId)
        {
            int contador = 0;

            List<CvnItemBean> listado = new();

            // Selecciono los identificadores de las entidades de la seccion
            List<Tuple<string, string>> listadoIdentificadoresProdCie = UtilityExportar.GetListadoEntidades(mResourceApi, propiedadesItem, mCvID);
            if (!UtilityExportar.Iniciar(mResourceApi, propiedadesItem, mCvID, listadoIdentificadoresProdCie, listaId))
            {
                return;
            }

            Dictionary<string, Entity> listaEntidadesProdCie = GetListLoadedEntity(listadoIdentificadoresProdCie, graph, MultilangProp);
            foreach (KeyValuePair<string, Entity> keyValue in listaEntidadesProdCie)
            {
                //solo se permite 1 valor en la exportación de la version 1.4.0
                if (versionExportacion.Equals("1_4_0") && contador > 0)
                {
                    break;
                }

                CvnItemBean itemBean = new();
                itemBean.Code = "060.010.000.000";
                if (itemBean.Items == null)
                {
                    itemBean.Items = new List<CVNObject>();
                }

                //En la versión 1.4 no se envía, en la 1.4.3 sí.
                if (!versionExportacion.Equals("1_4_0"))
                {
                    UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.prodCientificaFuenteIndiceH),
                        "060.010.000.030", keyValue.Value);
                    UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.prodCientificaFuenteIndiceHOtros),
                        "060.010.000.040", keyValue.Value);
                }

                string indiceH = UtilityExportar.Comprobar(keyValue.Value.properties.Where(x => x.prop.Equals(Variables.ActividadCientificaTecnologica.prodCientificaIndiceH))) ?
                    keyValue.Value.properties.Where(x => x.prop.Equals(Variables.ActividadCientificaTecnologica.prodCientificaIndiceH)).Select(x => x.values)?.FirstOrDefault().FirstOrDefault()
                    : null;
                if (!string.IsNullOrEmpty(indiceH))
                {
                    UtilityExportar.AddCvnItemBeanCvnDouble(itemBean, "060.010.000.010", indiceH);
                }
                UtilityExportar.AddCvnItemBeanCvnDateDayMonthYear(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.prodCientificaFechaAplicacion),
                    "060.010.000.020", keyValue.Value);

                listado.Add(itemBean);

                //Aumento el contador para solo insertar 1 valor para la version 1.4.0
                contador++;
            }

            //Añado en el cvnRootResultBean los items que forman parte del listado
            UtilityExportar.AniadirItems(mCvn, listado);
        }

    }
}
