using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Utils;
using static Gnoss.ApiWrapper.ApiModel.SparqlObject;

namespace ImportadorWebCV.Exporta.Secciones.ActividadCientificaSubclases
{
    public class SociedadesAsociaciones:SeccionBase
    {
        private readonly List<string> propiedadesItem = new () { "http://w3id.org/roh/scientificActivity", 
            "http://w3id.org/roh/societies", "http://vivoweb.org/ontology/core#relatedBy" };
        private readonly string graph = "society";
        public SociedadesAsociaciones(cvnRootResultBean cvn, string cvID) : base(cvn, cvID)
        {
        }

        /// <summary>
        /// Exporta los datos de la sección "060.030.020.000" a cvn.cvnRootResultBean
        /// </summary>
        /// <param name="MultilangProp"></param>
        /// <param name="listaId"></param>
        public void ExportaSociedadesAsociaciones(Dictionary<string, List<Dictionary<string, Data>>> MultilangProp, [Optional] List<string> listaId)
        {
            List<CvnItemBean> listado = new ();

            // Selecciono los identificadores de las entidades de la seccion
            List<Tuple<string, string>> listadoIdentificadoresSocAso = UtilityExportar.GetListadoEntidades(mResourceApi, propiedadesItem, mCvID);
            if (!UtilityExportar.Iniciar(mResourceApi, propiedadesItem, mCvID, listadoIdentificadoresSocAso, listaId))
            {
                return;
            }

            Dictionary<string, Entity> listaEntidadesSocAso = GetListLoadedEntity(listadoIdentificadoresSocAso, graph, MultilangProp);
            foreach (KeyValuePair<string, Entity> keyValue in listaEntidadesSocAso)
            {
                CvnItemBean itemBean = new ();
                itemBean.Code = "060.030.020.000";
                if (itemBean.Items == null)
                {
                    itemBean.Items = new List<CVNObject>();
                }
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.sociedadesNombre),
                    "060.030.020.010", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.sociedadesPaisRadicacion),
                    "060.030.020.020", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.sociedadesCCAARadicacion),
                    "060.030.020.030", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.sociedadesCiudadRadicacion),
                    "060.030.020.180", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.sociedadesPaisEntidadAfiliacion),
                    "060.030.020.150", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.sociedadesCCAAEntidadAfiliacion),
                    "060.030.020.160", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.sociedadesCiudadEntidadAfiliacion),
                    "060.030.020.170", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.sociedadesCategoriaProfesional),
                    "060.030.020.100", keyValue.Value);
                string tamanioSociedad = UtilityExportar.Comprobar(keyValue.Value.properties.Where(x => x.prop.Equals(Variables.ActividadCientificaTecnologica.sociedadesTamanio))) ?
                    keyValue.Value.properties.Where(x => x.prop.Equals(Variables.ActividadCientificaTecnologica.sociedadesTamanio)).Select(x => x.values)?.FirstOrDefault().FirstOrDefault()
                    : null;
                if (!string.IsNullOrEmpty(tamanioSociedad))
                {
                    UtilityExportar.AddCvnItemBeanCvnDouble(itemBean, "060.030.020.110", tamanioSociedad);
                }
                UtilityExportar.AddCvnItemBeanCvnDateDayMonthYear(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.sociedadesFechaInicio),
                    "060.030.020.120", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnDateDayMonthYear(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.sociedadesFechaFinalizacion),
                    "060.030.020.130", keyValue.Value);
                // Entidad afiliación
                UtilityExportar.AddCvnItemBeanCvnEntityBean(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.sociedadesEntidadAfiliacionNombre),
                    "060.030.020.050", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.sociedadesTipoEntidadAfiliacion),
                    "060.030.020.070", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.sociedadesTipoEntidadAfiliacionOtros),
                    "060.030.020.080", keyValue.Value);
                // Palabras clave
                UtilityExportar.AddCvnItemBeanCvnKeyword(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.sociedadesPalabrasClave),
                    "060.030.020.090", keyValue.Value);
                listado.Add(itemBean);
            }

            //Añado en el cvnRootResultBean los items que forman parte del listado
            UtilityExportar.AniadirItems(mCvn, listado);
        }
    }
}
