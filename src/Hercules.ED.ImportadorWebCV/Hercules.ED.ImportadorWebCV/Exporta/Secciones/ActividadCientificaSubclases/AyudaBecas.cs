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
    public class AyudaBecas : SeccionBase
    {
        List<string> propiedadesItem = new List<string>() { "http://w3id.org/roh/scientificActivity", "http://w3id.org/roh/grants", "http://vivoweb.org/ontology/core#relatedBy" };
        string graph = "grant";
        public AyudaBecas(cvnRootResultBean cvn, string cvID) : base(cvn, cvID)
        {




        }
        public void ExportaAyudaBecas(Entity entity, string seccion, [Optional] List<string> secciones, [Optional] bool preimportar)
        {
            List<CvnItemBean> listado = new List<CvnItemBean>();
            List<string> listadoIdentificadores = UtilityExportar.GetListadoEntidades(mResourceApi, propiedadesItem, mCvID);
            Dictionary<string, Entity> listaEntidadesSP = GetListLoadedEntity(listadoIdentificadores, graph);
            foreach (KeyValuePair<string, Entity> keyValue in listaEntidadesSP)
            {
                CvnItemBean itemBean = new CvnItemBean()
                {
                    Code = "060.030.010.000",
                    Items = new List<CVNObject>()
                };



                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.ayudasBecasNombre),
                    "060.030.010.010", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.ayudasBecasPaisConcede),
                    "060.030.010.020", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.ayudasBecasCCAAConcede),
                    "060.030.010.030", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.ayudasBecasCiudadConcede),
                    "060.030.010.150", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.ayudasBecasFinalidad),
                    "060.030.010.060", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.ayudasBecasFinalidadOtros),
                    "060.030.010.070", keyValue.Value);

                string importeBecas = UtilityExportar.Comprobar(keyValue.Value.properties.Where(x => x.prop.Equals(Variables.ActividadCientificaTecnologica.ayudasBecasImporte))) ?
                keyValue.Value.properties.Where(x => x.prop.Equals(Variables.ActividadCientificaTecnologica.ayudasBecasImporte)).Select(x => x.values).FirstOrDefault().FirstOrDefault()
                : null;
                if (!string.IsNullOrEmpty(importeBecas))
                {
                    UtilityExportar.AddCvnItemBeanCvnDouble(itemBean, "060.030.010.120", importeBecas);

                }
                
                UtilityExportar.AddCvnItemBeanCvnDateDayMonthYear(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.ayudasBecasFechaConcesion),
                    "060.030.010.130", keyValue.Value);

                //TODO test
                UtilityExportar.AddCvnItemBeanCvnDuration(itemBean,"060.030.010.140", keyValue.Value); 

                UtilityExportar.AddCvnItemBeanCvnDateDayMonthYear(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.ayudasBecasFechaFinalizacion),
                    "060.030.010.160", keyValue.Value);

                // Entidad Concede
                UtilityExportar.AddCvnItemBeanCvnEntityBean(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.ayudasBecasEntidadConcedeNombre),
                   "060.030.010.080", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.ayudasBecasTipoEntidadConcede),
                    "060.030.010.100", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.ayudasBecasTipoEntidadConcedeOtros),
                    "060.030.010.110", keyValue.Value);

                // Entidad Realizacion
                UtilityExportar.AddCvnItemBeanCvnEntityBean(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.ayudasBecasEntidadRealizacionNombre),
                   "060.030.010.180", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.ayudasBecasFacultadEscuela),
                    "060.030.010.170", keyValue.Value);

                listado.Add(itemBean);


            }

            //Añado en el cvnRootResultBean los items que forman parte del listado
            UtilityExportar.AniadirItems(mCvn, listado);
        }
    }
}
