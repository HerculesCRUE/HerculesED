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
    public class Doctorados : SeccionBase
    {
        private readonly List<string> propiedadesItem = new () { "http://w3id.org/roh/qualifications",
            "http://w3id.org/roh/doctorates", "http://vivoweb.org/ontology/core#relatedBy" };
        private readonly string graph = "academicdegree";

        public Doctorados(cvnRootResultBean cvn, string cvID) : base(cvn, cvID)
        {

        }

        /// <summary>
        /// Exporta los datos de la sección "020.010.020.000" a cvn.cvnRootResultBean
        /// </summary>
        /// <param name="MultilangProp"></param>
        /// <param name="listaId"></param>
        public void ExportaDoctorados( Dictionary<string, List<Dictionary<string, Data>>> MultilangProp, [Optional] List<string> listaId)
        {
            List<CvnItemBean> listado = new();

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
                    Code = "020.010.020.000",
                    Items = new List<CVNObject>()
                };

                UtilityExportar.AddCvnItemBeanCvnDateDayMonthYear(itemBean, UtilityExportar.EliminarRDF(Variables.FormacionAcademica.doctoradosFechaObtencionDEA),
                    "020.010.020.050", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.FormacionAcademica.doctoradosPaisEntidadTitulacion),
                    "020.010.020.060", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.FormacionAcademica.doctoradosCCAAEntidadTitulacion),
                    "020.010.020.070", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.FormacionAcademica.doctoradosCiudadEntidadTitulacion),
                    "020.010.020.090", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnDateDayMonthYear(itemBean, UtilityExportar.EliminarRDF(Variables.FormacionAcademica.doctoradosFechaTitulacion),
                    "020.010.020.140", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.FormacionAcademica.doctoradosTituloTesis),
                    "020.010.020.160", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.FormacionAcademica.doctoradosCalificacionObtenida),
                    "020.010.020.190", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnBoolean(itemBean, UtilityExportar.EliminarRDF(Variables.FormacionAcademica.doctoradosMencionCalidad),
                    "020.010.020.210", keyValue.Value);

                //Programa doctorado
                UtilityExportar.AddCvnItemBeanCvnTitleBean(itemBean, UtilityExportar.EliminarRDF(Variables.FormacionAcademica.doctoradosProgramaDoctorado),
                     UtilityExportar.EliminarRDF(Variables.FormacionAcademica.doctoradosProgramaDoctoradoNombre),
                     "020.010.020.010", keyValue.Value);

                //Entidad titulación
                UtilityExportar.AddCvnItemBeanCvnEntityBean(itemBean, UtilityExportar.EliminarRDF(Variables.FormacionAcademica.doctoradosEntidadTitulacionNombre),
                    "020.010.020.100", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.FormacionAcademica.doctoradosTipoEntidadTitulacion),
                    "020.010.020.120", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.FormacionAcademica.doctoradosTipoEntidadTitulacionOtros),
                    "020.010.020.130", keyValue.Value);

                //Entidad titulación DEA
                UtilityExportar.AddCvnItemBeanCvnEntityBean(itemBean, UtilityExportar.EliminarRDF(Variables.FormacionAcademica.doctoradosEntidadTitulacionDEANombre),
                    "020.010.020.040", keyValue.Value);

                //Director tesis
                Dictionary<string, string> listadoPropiedadesDirector = new Dictionary<string, string>();
                listadoPropiedadesDirector.Add("Firma", UtilityExportar.EliminarRDF(Variables.FormacionAcademica.doctoradosDirectorTesisFirma));
                listadoPropiedadesDirector.Add("Nombre", UtilityExportar.EliminarRDF(Variables.FormacionAcademica.doctoradosDirectorTesisNombre));
                listadoPropiedadesDirector.Add("PrimerApellido", UtilityExportar.EliminarRDF(Variables.FormacionAcademica.doctoradosDirectorTesisPrimerApellido));
                listadoPropiedadesDirector.Add("SegundoApellido", UtilityExportar.EliminarRDF(Variables.FormacionAcademica.doctoradosDirectorTesisSegundoApellido));

                UtilityExportar.AddCvnItemBeanCvnAuthorBean(itemBean, listadoPropiedadesDirector, "020.010.020.170",keyValue.Value);

                //Codirectores tesis
                Dictionary<string, string> listadoPropiedadesCodirector = new Dictionary<string, string>();
                listadoPropiedadesCodirector.Add("Orden", UtilityExportar.EliminarRDF(Variables.FormacionAcademica.doctoradosCodirectorTesisOrden));
                listadoPropiedadesCodirector.Add("Firma", UtilityExportar.EliminarRDF(Variables.FormacionAcademica.doctoradosCodirectorTesisFirma));
                listadoPropiedadesCodirector.Add("Nombre", UtilityExportar.EliminarRDF(Variables.FormacionAcademica.doctoradosCodirectorTesisNombre));
                listadoPropiedadesCodirector.Add("PrimerApellido", UtilityExportar.EliminarRDF(Variables.FormacionAcademica.doctoradosCodirectorTesisPrimerApellido));
                listadoPropiedadesCodirector.Add("SegundoApellido", UtilityExportar.EliminarRDF(Variables.FormacionAcademica.doctoradosCodirectorTesisSegundoApellido));

                UtilityExportar.AddCvnItemBeanCvnAuthorBeanList(itemBean, listadoPropiedadesCodirector, "020.010.020.180", keyValue.Value);

                //Doctorado UE
                UtilityExportar.AddCvnItemBeanCvnBoolean(itemBean, UtilityExportar.EliminarRDF(Variables.FormacionAcademica.doctoradosDoctoradoUE),
                    "020.010.020.200", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnDateDayMonthYear(itemBean, UtilityExportar.EliminarRDF(Variables.FormacionAcademica.doctoradosFechaMencionDocUE),
                    "020.010.020.150", keyValue.Value);

                //Premio extraordinario
                UtilityExportar.AddCvnItemBeanCvnBoolean(itemBean, UtilityExportar.EliminarRDF(Variables.FormacionAcademica.doctoradosPremioExtraordinario),
                    "020.010.020.220", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnDateDayMonthYear(itemBean, UtilityExportar.EliminarRDF(Variables.FormacionAcademica.doctoradosFechaObtencion),
                    "020.010.020.230", keyValue.Value);

                //Titulo homologado
                UtilityExportar.AddCvnItemBeanCvnBoolean(itemBean, UtilityExportar.EliminarRDF(Variables.FormacionAcademica.doctoradosTituloHomologado),
                    "020.010.020.240", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnDateDayMonthYear(itemBean, UtilityExportar.EliminarRDF(Variables.FormacionAcademica.doctoradosFechaHomologacion),
                    "020.010.020.250", keyValue.Value);

                listado.Add(itemBean);
            }

            //Añado en el cvnRootResultBean los items que forman parte del listado
            UtilityExportar.AniadirItems(mCvn, listado);
        }
    }
}
