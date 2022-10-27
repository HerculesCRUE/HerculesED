using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Utils;
using static Gnoss.ApiWrapper.ApiModel.SparqlObject;

namespace ImportadorWebCV.Exporta.Secciones.ActividadDocenteSubclases
{
    public class FormacionAcademicaSubclase : SeccionBase
    {
        private readonly List<string> propiedadesItem = new List<string>() { "http://w3id.org/roh/teachingExperience",
            "http://w3id.org/roh/impartedAcademicTrainings", "http://vivoweb.org/ontology/core#relatedBy" };
        private readonly string graph = "impartedacademictraining";

        public FormacionAcademicaSubclase(cvnRootResultBean cvn, string cvID) : base(cvn, cvID)
        {

        }

        /// <summary>
        /// Exporta los datos de la sección "030.010.000.000" a cvn.cvnRootResultBean
        /// </summary>
        /// <param name="MultilangProp"></param>
        /// <param name="listaId"></param>
        public void ExportaFormacionAcademica(Dictionary<string, List<Dictionary<string, Data>>> MultilangProp, [Optional] List<string> listaId)
        {
            List<CvnItemBean> listado = new List<CvnItemBean>();

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
                    Code = "030.010.000.000",
                    Items = new List<CVNObject>()
                };

                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadDocente.formacionAcademicaTipoDocenciaOficialidad),
                    "030.010.000.010", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadDocente.formacionAcademicaPaisEntidadRealizacion),
                    "030.010.000.040", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadDocente.formacionAcademicaCCAAEntidadRealizacion),
                    "030.010.000.050", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadDocente.formacionAcademicaCiudadEntidadRealizacion),
                    "030.010.000.070", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadDocente.formacionAcademicaDepartamento),
                    "030.010.000.130", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadDocente.formacionAcademicaTipoPrograma),
                    "030.010.000.140", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadDocente.formacionAcademicaTipoProgramaOtros),
                    "030.010.000.150", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadDocente.formacionAcademicaNombreAsignatura),
                    "030.010.000.160", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadDocente.formacionAcademicaTipoDocenciaModalidad),
                    "030.010.000.170", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadDocente.formacionAcademicaTipoDocenciaModalidadOtros),
                    "030.010.000.180", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadDocente.formacionAcademicaTipoAsignatura),
                    "030.010.000.190", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadDocente.formacionAcademicaTipoAsignaturaOtros),
                    "030.010.000.430", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadDocente.formacionAcademicaCursoTitulacion),
                    "030.010.000.200", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadDocente.formacionAcademicaTipoECTS),
                    "030.010.000.210", keyValue.Value);

                string NumeroECTS = UtilityExportar.Comprobar(keyValue.Value.properties.Where(x => x.prop.Equals(Variables.ActividadDocente.formacionAcademicaNumeroECTS))) ?
                    keyValue.Value.properties.Where(x => x.prop.Equals(Variables.ActividadDocente.formacionAcademicaNumeroECTS)).Select(x => x.values).FirstOrDefault().FirstOrDefault()
                    : null;
                if (!string.IsNullOrEmpty(NumeroECTS))
                {
                    UtilityExportar.AddCvnItemBeanCvnDouble(itemBean, "030.010.000.220", NumeroECTS);
                }

                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadDocente.formacionAcademicaIdiomaAsignatura),
                    "030.010.000.230", keyValue.Value);

                string frecuenciaAsignatura = UtilityExportar.Comprobar(keyValue.Value.properties.Where(x => x.prop.Equals(Variables.ActividadDocente.formacionAcademicaFrecuenciaAsignatura))) ?
                    keyValue.Value.properties.Where(x => x.prop.Equals(Variables.ActividadDocente.formacionAcademicaFrecuenciaAsignatura)).Select(x => x.values)?.FirstOrDefault().FirstOrDefault()
                    : null;
                if (!string.IsNullOrEmpty(frecuenciaAsignatura))
                {
                    UtilityExportar.AddCvnItemBeanCvnDouble(itemBean, "030.010.000.240", frecuenciaAsignatura);
                }

                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadDocente.formacionAcademicaCompetenciasRelacionadas),
                    "030.010.000.260", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadDocente.formacionAcademicaCategoriaProfesional),
                    "030.010.000.270", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadDocente.formacionAcademicaCalificacionObtenida),
                    "030.010.000.280", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadDocente.formacionAcademicaCalificacionMax),
                    "030.010.000.290", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadDocente.formacionAcademicaPaisEntidadEvaluacion),
                    "030.010.000.440", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadDocente.formacionAcademicaCCAAEntidadEvaluacion),
                    "030.010.000.450", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadDocente.formacionAcademicaCiudadEntidadEvaluacion),
                    "030.010.000.470", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadDocente.formacionAcademicaTipoEvaluacion),
                    "030.010.000.320", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadDocente.formacionAcademicaTipoEvaluacionOtros),
                    "030.010.000.330", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadDocente.formacionAcademicaPaisEntidadFinanciadora),
                    "030.010.000.480", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadDocente.formacionAcademicaCCAAEntidadFinanciadora),
                    "030.010.000.500", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadDocente.formacionAcademicaCiudadEntidadFinanciadora),
                    "030.010.000.510", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadDocente.formacionAcademicaEntFinanTipoConvocatoria),
                    "030.010.000.390", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadDocente.formacionAcademicaEntFinanTipoConvocatoriaOtros),
                    "030.010.000.400", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadDocente.formacionAcademicaEntFinanAmbitoGeo),
                    "030.010.000.410", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadDocente.formacionAcademicaEntFinanAmbitoGeoOtros),
                    "030.010.000.420", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnEntityBean(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadDocente.formacionAcademicaFacultadEscuela),
                    "030.010.000.540", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnDateDayMonthYear(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadDocente.formacionAcademicaFechaInicio),
                    "030.010.000.550", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnDateDayMonthYear(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadDocente.formacionAcademicaFechaFinalizacion),
                    "030.010.000.610", keyValue.Value);

                //Titulacion universitaria
                UtilityExportar.AddCvnItemBeanCvnTitleBean(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadDocente.formacionAcademicaTitulacionUniversitaria),
                     UtilityExportar.EliminarRDF(Variables.ActividadDocente.formacionAcademicaTitulacionUniversitariaNombre),
                    "030.010.000.020", keyValue.Value);

                //Entidad realizacion
                UtilityExportar.AddCvnItemBeanCvnEntityBean(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadDocente.formacionAcademicaEntidadRealizacionNombre),
                    "030.010.000.080", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadDocente.formacionAcademicaTipoEntidadRealizacion),
                    "030.010.000.110", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadDocente.formacionAcademicaTipoEntidadRealizacionOtros),
                    "030.010.000.120", keyValue.Value);

                //Entidad financiadora
                UtilityExportar.AddCvnItemBeanCvnEntityBean(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadDocente.formacionAcademicaEntidadFinanciadoraNombre),
                    "030.010.000.350", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadDocente.formacionAcademicaTipoEntidadFinanciadora),
                    "030.010.000.370", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadDocente.formacionAcademicaTipoEntidadFinanciadoraOtros),
                    "030.010.000.380", keyValue.Value);

                //Entidad evaluacion
                UtilityExportar.AddCvnItemBeanCvnEntityBean(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadDocente.formacionAcademicaEntidadEvaluacionNombre),
                    "030.010.000.300", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadDocente.formacionAcademicaTipoEntidadEvaluacion),
                    "030.010.000.520", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadDocente.formacionAcademicaTipoEntidadEvaluacionOtros),
                    "030.010.000.530", keyValue.Value);

                listado.Add(itemBean);
            }

            //Añado en el cvnRootResultBean los items que forman parte del listado
            UtilityExportar.AniadirItems(mCvn, listado);
        }
    }
}
