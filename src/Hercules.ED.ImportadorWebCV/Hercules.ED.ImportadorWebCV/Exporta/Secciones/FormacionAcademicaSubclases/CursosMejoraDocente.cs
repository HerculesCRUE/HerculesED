using ExportadorWebCV.Utils;
using ImportadorWebCV;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace ImportadorWebCV.Exporta.Secciones.FormacionAcademicaSubclases
{
    public class CursosMejoraDocente : SeccionBase
    {
        private List<string> propiedadesItem = new List<string>() { "http://w3id.org/roh/qualifications",
            "http://w3id.org/roh/coursesAndSeminars", "http://vivoweb.org/ontology/core#relatedBy" };
        private string graph = "academicdegree";

        public CursosMejoraDocente(cvnRootResultBean cvn, string cvID) : base(cvn, cvID)
        {

        }
        /// <summary>
        /// Exporta los datos de la sección "020.050.000.000" a cvn.cvnRootResultBean
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="seccion"></param>
        /// <param name="secciones"></param>
        /// <param name="preimportar"></param>
        public void ExportaCursosMejoraDocente(Entity entity, string seccion, [Optional] List<string> secciones, [Optional] bool preimportar)
        {
            List<CvnItemBean> listado = new List<CvnItemBean>();
            List<string> listadoIdentificadores = UtilityExportar.GetListadoEntidades(mResourceApi, propiedadesItem, mCvID);
            Dictionary<string, Entity> listaEntidadesSP = GetListLoadedEntity(listadoIdentificadores, graph);
            foreach (KeyValuePair<string, Entity> keyValue in listaEntidadesSP)
            {
                CvnItemBean itemBean = new CvnItemBean()
                {
                    Code = "020.050.000.000",
                    Items = new List<CVNObject>()
                };

                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.FormacionAcademica.cursosSeminariosTitulo),
                    "020.050.000.010", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.FormacionAcademica.cursosSeminariosObjetivos),
                    "020.050.000.020", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.FormacionAcademica.cursosSeminariosPaisEntidadOrganizadora),
                    "020.050.000.030", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.FormacionAcademica.cursosSeminariosCCAAEntidadOrganizadora),
                    "020.050.000.040", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.FormacionAcademica.cursosSeminariosCiudadEntidadOrganizadora),
                    "020.050.000.060", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnDurationHours(itemBean, "020.050.000.110", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnDateDayMonthYear(itemBean, UtilityExportar.EliminarRDF(Variables.FormacionAcademica.cursosSeminariosFechaFinal),
                    "020.050.000.120",keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.FormacionAcademica.cursosSeminariosPerfilDestinatarios),
                    "020.050.000.130", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnDateDayMonthYear(itemBean, UtilityExportar.EliminarRDF(Variables.FormacionAcademica.cursosSeminariosFechaInicio),
                    "020.050.000.140", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnDuration(itemBean,"020.050.000.170",keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.FormacionAcademica.cursosSeminariosProgramaFinanciacion),
                    "020.050.000.180", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.FormacionAcademica.cursosSeminariosTareasContrastables),
                    "020.050.000.190", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.FormacionAcademica.cursosSeminariosObjetivoEstancia),
                    "020.050.000.200", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.FormacionAcademica.cursosSeminariosObjetivoEstanciaOtros),
                    "020.050.000.210", keyValue.Value);

                //Entidad organizadora
                UtilityExportar.AddCvnItemBeanCvnEntityBean(itemBean, UtilityExportar.EliminarRDF(Variables.FormacionAcademica.cursosSeminariosEntidadOrganizadoraNombre),
                    "020.050.000.070",keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnEntityBean(itemBean, UtilityExportar.EliminarRDF(Variables.FormacionAcademica.cursosSeminariosFacultadEscuela),
                    "020.050.000.150",keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.FormacionAcademica.cursosSeminariosTipoEntidadOrganizadora),
                    "020.050.000.090", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.FormacionAcademica.cursosSeminariosTipoEntidadOrganizadoraOtros),
                    "020.050.000.100", keyValue.Value);

                listado.Add(itemBean);
            }

            //Añado en el cvnRootResultBean los items que forman parte del listado
            UtilityExportar.AniadirItems(mCvn, listado);
        }
    }
}
