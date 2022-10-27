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
    public class OrganizacionesIDI:SeccionBase
    {
        private readonly List<string> propiedadesItem = new () { "http://w3id.org/roh/scientificActivity", 
            "http://w3id.org/roh/activitiesOrganization", "http://vivoweb.org/ontology/core#relatedBy" };
        private readonly string graph = "activity";
        public OrganizacionesIDI(cvnRootResultBean cvn, string cvID) : base(cvn, cvID)
        {
        }

        /// <summary>
        /// Exporta los datos de la sección "060.020.030.000" a cvn.cvnRootResultBean
        /// </summary>
        /// <param name="MultilangProp"></param>
        /// <param name="listaId"></param>
        public void ExportaOrganizacionesIDI(Dictionary<string, List<Dictionary<string, Data>>> MultilangProp, [Optional] List<string> listaId)
        {
            List<CvnItemBean> listado = new ();

            // Selecciono los identificadores de las entidades de la seccion
            List<Tuple<string, string>> listadoIdentificadores = UtilityExportar.GetListadoEntidades(mResourceApi, propiedadesItem, mCvID);
            if (!UtilityExportar.Iniciar(mResourceApi, propiedadesItem, mCvID, listadoIdentificadores, listaId))
            {
                return;
            }

            Dictionary<string, Entity> listaEntidadesSP = GetListLoadedEntity(listadoIdentificadores, graph, MultilangProp);
            foreach (KeyValuePair<string, Entity> keyValue in listaEntidadesSP)
            {
                CvnItemBean itemBean = new ()
                {
                    Code = "060.020.030.000",
                    Items = new List<CVNObject>()
                };

                 UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.orgIDITituloActividad),
                     "060.020.030.010", keyValue.Value);
                 UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.orgIDITipoActividad), 
                     "060.020.030.020", keyValue.Value);
                 UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.orgIDIPaisActividad),
                     "060.020.030.030", keyValue.Value);
                 UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.orgIDICCAAActividad),
                     "060.020.030.040", keyValue.Value);
                 UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.orgIDICiudadActividad),
                     "060.020.030.060", keyValue.Value);
                 UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.orgIDIPaisEntidadConvocante),
                     "060.020.030.180", keyValue.Value);
                 UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.orgIDICCAAEntidadConvocante),
                     "060.020.030.190", keyValue.Value);
                 UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.orgIDICiudadEntidadConvocante),
                     "060.020.030.200", keyValue.Value);
                 UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.orgIDIModoParticipacion),
                     "060.020.030.110", keyValue.Value);
                 UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.orgIDIModoParticipacionOtros),
                     "060.020.030.120", keyValue.Value);
                 UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.orgIDIAmbitoReunion),
                     "060.020.030.130", keyValue.Value);
                 UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.orgIDIAmbitoReunionOtros),
                     "060.020.030.140", keyValue.Value);
                 UtilityExportar.AddCvnItemBeanCvnDateDayMonthYear(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.orgIDIFechaInicio),
                     "060.020.030.160", keyValue.Value);
                 UtilityExportar.AddCvnItemBeanCvnDuration(itemBean, "060.020.030.170", keyValue.Value);
                 UtilityExportar.AddCvnItemBeanCvnDateDayMonthYear(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.orgIDIFechaFinalizacion),
                     "060.020.030.220", keyValue.Value);

                string numeroAsistentes = UtilityExportar.Comprobar(keyValue.Value.properties.Where(x => x.prop.Equals(Variables.ActividadCientificaTecnologica.orgIDINumeroAsistentes))) ?
                    keyValue.Value.properties.Where(x => x.prop.Equals(Variables.ActividadCientificaTecnologica.orgIDINumeroAsistentes)).Select(x => x.values)?.FirstOrDefault().FirstOrDefault()
                    : null;
                if (!string.IsNullOrEmpty(numeroAsistentes))
                {
                    UtilityExportar.AddCvnItemBeanCvnDouble(itemBean, "060.020.030.150", numeroAsistentes);
                }
                // Entidad Convocante
                UtilityExportar.AddCvnItemBeanCvnEntityBean(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.orgIDIEntidadConvocanteNombre),
                    "060.020.030.070", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.orgIDITipoEntidadConvocante),
                    "060.020.030.090", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadCientificaTecnologica.orgIDITipoEntidadConvocanteOtros),
                    "060.020.030.100", keyValue.Value);

                listado.Add(itemBean);
            }

            //Añado en el cvnRootResultBean los items que forman parte del listado
            UtilityExportar.AniadirItems(mCvn, listado);
        }
    }
}
