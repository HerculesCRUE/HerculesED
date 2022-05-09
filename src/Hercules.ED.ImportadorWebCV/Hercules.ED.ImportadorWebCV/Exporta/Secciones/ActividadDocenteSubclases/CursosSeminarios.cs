using ExportadorWebCV.Utils;
using ImportadorWebCV;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace ImportadorWebCV.Exporta.Secciones.ActividadDocenteSubclases
{
    public class CursosSeminarios : SeccionBase
    {
        private List<string> propiedadesItem = new List<string>() { "http://w3id.org/roh/teachingExperience",
            "http://w3id.org/roh/impartedCoursesSeminars", "http://vivoweb.org/ontology/core#relatedBy" };
        private string graph = "impartedcoursesseminars";

        public CursosSeminarios(cvnRootResultBean cvn, string cvID) : base(cvn, cvID)
        {

        }

        public void ExportaCursosSeminarios(Entity entity, string seccion, [Optional] List<string> secciones, [Optional] bool preimportar)
        {
            List<CvnItemBean> listado = new List<CvnItemBean>();
            List<string> listadoIdentificadores = UtilityExportar.GetListadoEntidades(mResourceApi, propiedadesItem, mCvID);
            Dictionary<string, Entity> listaEntidadesSP = GetListLoadedEntity(listadoIdentificadores, graph);
            foreach (KeyValuePair<string, Entity> keyValue in listaEntidadesSP)
            {
                CvnItemBean itemBean = new CvnItemBean()
                {
                    Code = "030.060.000.000",
                    Items = new List<CVNObject>()
                };

                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadDocente.cursosSeminariosTipoEvento),
                    "030.060.000.010", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadDocente.cursosSeminariosTipoEventoOtros),
                    "030.060.000.020", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadDocente.cursosSeminariosNombreEvento),
                    "030.060.000.030", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadDocente.cursosSeminariosPaisEntidadOrganizadora),
                    "030.060.000.040", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadDocente.cursosSeminariosCCAAEntidadOrganizadora),
                    "030.060.000.050", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadDocente.cursosSeminariosCiudadEntidadOrganizadora),
                    "030.060.000.070", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadDocente.cursosSeminariosObjetivosCurso),
                    "030.060.000.120", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadDocente.cursosSeminariosPerfilDestinatarios),
                    "030.060.000.130", keyValue.Value);
                UtilityExportar.AddLanguage(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadDocente.cursosSeminariosIdiomaImpartio),
                    "030.060.000.140", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnDateDayMonthYear(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadDocente.cursosSeminariosFechaImparticion),
                    "030.060.000.150", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnDuration(itemBean, "030.060.000.160", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadDocente.cursosSeminariosTipoParticipacion),
                    "030.060.000.170", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadDocente.cursosSeminariosTipoParticipacionOtros),
                    "030.060.000.180", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnBoolean(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadDocente.cursosSeminariosAutorCorrespondencia),
                    "030.060.000.200", keyValue.Value);

                //Entidad organizadora
                UtilityExportar.AddCvnItemBeanCvnEntityBean(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadDocente.cursosSeminariosEntidadOrganizadoraNombre),
                    "030.060.000.080", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadDocente.cursosSeminariosTipoEntidadOrganizadora),
                    "030.060.000.100", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadDocente.cursosSeminariosTipoEntidadOrganizadoraOtros),
                    "030.060.000.110", keyValue.Value);

                //ISBN

                //ISSN

                //ID publicacion

                listado.Add(itemBean);
            }

            //Añado en el cvnRootResultBean los items que forman parte del listado
            UtilityExportar.AniadirItems(mCvn, listado);
        }
    }
}
