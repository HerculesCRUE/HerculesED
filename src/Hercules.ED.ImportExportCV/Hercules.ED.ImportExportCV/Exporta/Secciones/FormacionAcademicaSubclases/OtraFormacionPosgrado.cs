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
    public class OtraFormacionPosgrado : SeccionBase
    {
        private readonly List<string> propiedadesItem = new List<string>() { "http://w3id.org/roh/qualifications",
            "http://w3id.org/roh/postgraduates", "http://vivoweb.org/ontology/core#relatedBy" };
        private readonly string graph = "academicdegree";

        public OtraFormacionPosgrado(cvnRootResultBean cvn, string cvID) : base(cvn, cvID)
        {

        }

        /// <summary>
        /// Exporta los datos de la sección "020.010.030.000" a cvn.cvnRootResultBean
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="MultilangProp"></param>
        /// <param name="listaId"></param>
        public void ExportaOtraFormacionPosgrado(Entity entity, Dictionary<string, List<Dictionary<string, Data>>> MultilangProp, [Optional] List<string> listaId)
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
                CvnItemBean itemBean = new CvnItemBean()
                {
                    Code = "020.010.030.000",
                    Items = new List<CVNObject>()
                };

                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.FormacionAcademica.otraFormacionTipoFormacion),
                    "020.010.030.010", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.FormacionAcademica.otraFormacionPaisEntidadTitulacion),
                    "020.010.030.040", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.FormacionAcademica.otraFormacionCCAAEntidadTitulacion),
                    "020.010.030.050", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.FormacionAcademica.otraFormacionCiudadEntidadTitulacion),
                    "020.010.030.070", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnDateDayMonthYear(itemBean, UtilityExportar.EliminarRDF(Variables.FormacionAcademica.otraFormacionFechaTitulacion),
                    "020.010.030.120", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.FormacionAcademica.otraFormacionCalificacionObtenida),
                    "020.010.030.130", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnBoolean(itemBean, UtilityExportar.EliminarRDF(Variables.FormacionAcademica.otraFormacionTituloHomologado),
                    "020.010.030.150", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnDateDayMonthYear(itemBean, UtilityExportar.EliminarRDF(Variables.FormacionAcademica.otraFormacionFechaHomologacion),
                    "020.010.030.160", keyValue.Value);

                //Titulacion posgrado
                UtilityExportar.AddCvnItemBeanCvnTitleBean(itemBean, UtilityExportar.EliminarRDF(Variables.FormacionAcademica.otraFormacionTituloPosgrado),
                     UtilityExportar.EliminarRDF(Variables.FormacionAcademica.otraFormacionTituloPosgradoNombre),
                     "020.010.030.020", keyValue.Value);

                //Entidad titulacion
                UtilityExportar.AddCvnItemBeanCvnEntityBean(itemBean, UtilityExportar.EliminarRDF(Variables.FormacionAcademica.otraFormacionEntidadTitulacionNombre),
                    "020.010.030.080", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnEntityBean(itemBean, UtilityExportar.EliminarRDF(Variables.FormacionAcademica.otraFormacionFacultadEscuela),
                    "020.010.030.140", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.FormacionAcademica.otraFormacionTipoEntidadTitulacion),
                    "020.010.030.100", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.FormacionAcademica.otraFormacionTipoEntidadTitulacionOtros),
                    "020.010.030.110", keyValue.Value);

                listado.Add(itemBean);
            }

            //Añado en el cvnRootResultBean los items que forman parte del listado
            UtilityExportar.AniadirItems(mCvn, listado);
        }

    }
}
