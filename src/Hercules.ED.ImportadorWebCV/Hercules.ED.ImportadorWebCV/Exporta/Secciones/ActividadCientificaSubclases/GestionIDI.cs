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
    public class GestionIDI:SeccionBase
    {
        List<string> propiedadesItem = new List<string>() { "http://w3id.org/roh/scientificActivity", 
            "http://w3id.org/roh/activitiesManagement", "http://vivoweb.org/ontology/core#relatedBy" };
        string graph = "activity";
        public GestionIDI(cvnRootResultBean cvn, string cvID) : base(cvn, cvID)
        {
        }
        public void ExportaGestionIDI(Entity entity, string seccion, [Optional] List<string> secciones, [Optional] bool preimportar)
        {
            List<CvnItemBean> listado = new List<CvnItemBean>();
            List<string> listadoIdentificadores = UtilityExportar.GetListadoEntidades(mResourceApi, propiedadesItem, mCvID);
            Dictionary<string, Entity> listaEntidadesSP = GetListLoadedEntity(listadoIdentificadores, graph);
            foreach (KeyValuePair<string, Entity> keyValue in listaEntidadesSP)
            {
                CvnItemBean itemBean = new CvnItemBean();
                itemBean.Code = "060.020.040.000";
                if (itemBean.Items == null)
                {
                    itemBean.Items = new List<CVNObject>();
                }
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.gestionIDIFunciones),
                    "060.020.040.010", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.gestionIDIPaisEntidadRealizacion),
                    "060.020.040.020", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.gestionIDICCAAEntidadRealizacion),
                    "060.020.040.030", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.gestionIDICiudadEntidadRealizacion),
                    "060.020.040.050", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.gestionIDINombreActividad),
                    "060.020.040.060", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.gestionIDITipologiaGestion),
                    "060.020.040.070", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.gestionIDITipologiaGestionOtros),
                    "060.020.040.080", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnDateDayMonthYear(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.gestionIDIEntornoFechaInicio),
                    "060.020.040.130", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnDuration(itemBean, "060.020.040.140", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.gestionIDISistemaAcceso),
                    "060.020.040.150", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.gestionIDISistemaAccesoOtros),
                    "060.020.040.160", keyValue.Value);
                string promedioPropuesto = UtilityExportar.Comprobar(keyValue.Value.properties.Where(x => x.prop.Equals(Variables.ActividadCientificaTecnologica.gestionIDIPromedioPresupuesto))) ?
                    keyValue.Value.properties.Where(x => x.prop.Equals(Variables.ActividadCientificaTecnologica.gestionIDIPromedioPresupuesto)).Select(x => x.values).FirstOrDefault().FirstOrDefault()
                    : null;
                if (!string.IsNullOrEmpty(promedioPropuesto))
                {
                    UtilityExportar.AddCvnItemBeanCvnDouble(itemBean, "060.020.040.170", promedioPropuesto);
                }
                string numPersonas = UtilityExportar.Comprobar(keyValue.Value.properties.Where(x => x.prop.Equals(Variables.ActividadCientificaTecnologica.gestionIDINumPersonas))) ?
                       keyValue.Value.properties.Where(x => x.prop.Equals(Variables.ActividadCientificaTecnologica.gestionIDINumPersonas)).Select(x => x.values).FirstOrDefault().FirstOrDefault()
                       : null;
                if (!string.IsNullOrEmpty(numPersonas))
                {
                    UtilityExportar.AddCvnItemBeanCvnDouble(itemBean, "060.020.040.180", numPersonas);
                }
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.gestionIDIObjetivosEvento),
                    "060.020.040.240", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.gestionIDIPerfilGrupo),
                    "060.020.040.190", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.gestionIDIAmbitoTerritorial),
                    "060.020.040.200", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.gestionIDIAmbitoTerritorialOtros),
                    "060.020.040.210", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.gestionIDITareasConcretas),
                    "060.020.040.220", keyValue.Value);

                // Entidad realización
                UtilityExportar.AddCvnItemBeanCvnEntityBean(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.gestionIDIEntornoEntidadRealizacionNombre),
                    "060.020.040.090", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.gestionIDIEntornoTipoEntidadRealizacion),
                    "060.020.040.110", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.gestionIDIEntornoTipoEntidadRealizacionOtros),
                    "060.020.040.120", keyValue.Value);

                // Palabras clave
                UtilityExportar.AddCvnItemBeanCvnKeyword(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.gestionIDIPalabrasClave),
                    "060.020.040.230", keyValue.Value);

                listado.Add(itemBean);
            }

            //Añado en el cvnRootResultBean los items que forman parte del listado
            UtilityExportar.AniadirItems(mCvn, listado);
        }

    }
}
