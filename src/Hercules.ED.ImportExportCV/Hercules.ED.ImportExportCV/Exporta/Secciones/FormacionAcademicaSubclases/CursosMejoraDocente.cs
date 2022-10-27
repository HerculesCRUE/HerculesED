using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Utils;
using static Gnoss.ApiWrapper.ApiModel.SparqlObject;

namespace ImportadorWebCV.Exporta.Secciones.FormacionAcademicaSubclases
{
    public class CursosMejoraDocente : SeccionBase
    {
        private readonly List<string> propiedadesItem = new () { "http://w3id.org/roh/qualifications",
            "http://w3id.org/roh/coursesAndSeminars", "http://vivoweb.org/ontology/core#relatedBy" };
        private readonly string graph = "academicdegree";

        public CursosMejoraDocente(cvnRootResultBean cvn, string cvID) : base(cvn, cvID)
        {

        }

        /// <summary>
        /// Exporta los datos de la sección "020.050.000.000" a cvn.cvnRootResultBean
        /// </summary>
        /// <param name="MultilangProp"></param>
        /// <param name="listaId"></param>
        public void ExportaCursosMejoraDocente(Dictionary<string, List<Dictionary<string, Data>>> MultilangProp, [Optional] List<string> listaId)
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
