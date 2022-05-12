using ExportadorWebCV.Utils;
using ImportadorWebCV;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace ImportadorWebCV.Exporta.Secciones.ActividadCientificaSubclases
{
    public class ProduccionCientifica:SeccionBase
    {
        List<string> propiedadesItem = new List<string>() { "http://w3id.org/roh/scientificActivity", 
            "http://w3id.org/roh/scientificProduction", "http://vivoweb.org/ontology/core#relatedBy" };
        string graph = "scientificproduction";
        public ProduccionCientifica(cvnRootResultBean cvn, string cvID) : base(cvn, cvID)
        {
        }
        /// <summary>
        /// Exporta los datos de la sección "060.010.000.000" a cvn.cvnRootResultBean
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="seccion"></param>
        /// <param name="secciones"></param>
        /// <param name="preimportar"></param>
        public void ExportaProduccionCientifica(Entity entity, string seccion, [Optional] List<string> secciones, [Optional] bool preimportar)
        {
            List<CvnItemBean> listado = new List<CvnItemBean>();
            List<string> listadoIdentificadores = UtilityExportar.GetListadoEntidades(mResourceApi, propiedadesItem, mCvID);
            Dictionary<string, Entity> listaEntidadesSP = GetListLoadedEntity(listadoIdentificadores, graph);
            foreach (KeyValuePair<string, Entity> keyValue in listaEntidadesSP)
            {
                CvnItemBean itemBean = new CvnItemBean();
                itemBean.Code = "060.010.000.000";
                if (itemBean.Items == null)
                {
                    itemBean.Items = new List<CVNObject>();
                }
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.prodCientificaFuenteIndiceH),
                    "060.010.000.030", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.prodCientificaFuenteIndiceHOtros),
                    "060.010.000.040", keyValue.Value);
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
            }

            //Añado en el cvnRootResultBean los items que forman parte del listado
            UtilityExportar.AniadirItems(mCvn, listado);
        }

    }
}
