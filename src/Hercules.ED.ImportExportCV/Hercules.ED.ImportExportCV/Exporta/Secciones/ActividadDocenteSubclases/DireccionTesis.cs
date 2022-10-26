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
    public class DireccionTesis : SeccionBase
    {
        private readonly List<string> propiedadesItem = new List<string>() { "http://w3id.org/roh/teachingExperience",
            "http://w3id.org/roh/thesisSupervisions", "http://vivoweb.org/ontology/core#relatedBy" };
        private readonly string graph = "thesissupervision";

        public DireccionTesis(cvnRootResultBean cvn, string cvID) : base(cvn, cvID)
        {

        }

        /// <summary>
        /// Exporta los datos de la sección "030.040.000.000" a cvn.cvnRootResultBean
        /// </summary>
        /// <param name="MultilangProp"></param>
        /// <param name="listaId"></param>
        public void ExportaDireccionTesis(Dictionary<string, List<Dictionary<string, Data>>> MultilangProp, [Optional] List<string> listaId)
        {
            List<CvnItemBean> listado = new List<CvnItemBean>();
            //Selecciono los identificadores de las entidades de la seccion, en caso de que se pase un listado de exportación se comprueba que el 
            // identificador esté en el listado. Si tras comprobarlo el listado es vacio salgo del metodo
            List<Tuple<string, string>> listadoIdentificadores = UtilityExportar.GetListadoEntidades(mResourceApi, propiedadesItem, mCvID);
            if (listaId != null && listaId.Count != 0 && listadoIdentificadores != null)
            {
                listadoIdentificadores = listadoIdentificadores.Where(x => listaId.Contains(x.Item2)).ToList();
                if (listadoIdentificadores.Count == 0)
                {
                    return;
                }
            }

            Dictionary<string, Entity> listaEntidadesSP = GetListLoadedEntity(listadoIdentificadores, graph, MultilangProp);
            foreach (KeyValuePair<string, Entity> keyValue in listaEntidadesSP)
            {
                CvnItemBean itemBean = new CvnItemBean()
                {
                    Code = "030.040.000.000",
                    Items = new List<CVNObject>()
                };

                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadDocente.direccionTesisTipoProyecto),
                    "030.040.000.010", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadDocente.direccionTesisTipoProyectoOtros),
                    "030.040.000.020", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadDocente.direccionTesisTituloTrabajo),
                    "030.040.000.030", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadDocente.direccionTesisPaisEntidadRealizacion),
                    "030.040.000.040", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadDocente.direccionTesisCCAAEntidadRealizacion),
                    "030.040.000.050", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadDocente.direccionTesisCiudadEntidadRealizacion),
                    "030.040.000.070", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnDateDayMonthYear(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadDocente.direccionTesisFechaDefensa),
                    "030.040.000.140", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadDocente.direccionTesisCalificacionObtenida),
                    "030.040.000.150", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnDateDayMonthYear(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadDocente.direccionTesisFechaMencionDoctUE),
                    "030.040.000.160", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnBoolean(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadDocente.direccionTesisMencionCalidad),
                    "030.040.000.170", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnBoolean(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadDocente.direccionTesisDoctoradoUE),
                    "030.040.000.190", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnDateDayMonthYear(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadDocente.direccionTesisFechaMencionCalidad),
                    "030.040.000.200", keyValue.Value);

                //Palabras clave
                UtilityExportar.AddCvnItemBeanCvnKeyword(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadDocente.direccionTesisPalabrasClave),
                    "030.040.000.130", keyValue.Value);

                //Entidad realizacion
                UtilityExportar.AddCvnItemBeanCvnEntityBean(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadDocente.direccionTesisEntidadRealizacionNombre),
                    "030.040.000.080", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadDocente.direccionTesisTipoEntidadRealizacion),
                    "030.040.000.100", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.ActividadDocente.direccionTesisTipoEntidadRealizacionOtros),
                    "030.040.000.110", keyValue.Value);

                //Alumno
                Dictionary<string, string> listadoPropiedadesAlumno = new Dictionary<string, string>();
                listadoPropiedadesAlumno.Add("Firma", UtilityExportar.EliminarRDF(Variables.ActividadDocente.direccionTesisAlumnoFirma));
                listadoPropiedadesAlumno.Add("Nombre", UtilityExportar.EliminarRDF(Variables.ActividadDocente.direccionTesisAlumnoNombre));
                listadoPropiedadesAlumno.Add("PrimerApellido", UtilityExportar.EliminarRDF(Variables.ActividadDocente.direccionTesisAlumnoPrimerApellido));
                listadoPropiedadesAlumno.Add("SegundoApellido", UtilityExportar.EliminarRDF(Variables.ActividadDocente.direccionTesisAlumnoSegundoApellido));

                UtilityExportar.AddCvnItemBeanCvnAuthorBean(itemBean, listadoPropiedadesAlumno, "030.040.000.120", keyValue.Value);

                //Codirectores
                Dictionary<string, string> listadoPropiedadesCodirector = new Dictionary<string, string>();
                listadoPropiedadesCodirector.Add("Orden", UtilityExportar.EliminarRDF(Variables.ActividadDocente.direccionTesisCodirectorTesisOrden));
                listadoPropiedadesCodirector.Add("Firma", UtilityExportar.EliminarRDF(Variables.ActividadDocente.direccionTesisCodirectorTesisFirma));
                listadoPropiedadesCodirector.Add("Nombre", UtilityExportar.EliminarRDF(Variables.ActividadDocente.direccionTesisCodirectorTesisNombre));
                listadoPropiedadesCodirector.Add("PrimerApellido", UtilityExportar.EliminarRDF(Variables.ActividadDocente.direccionTesisCodirectorTesisPrimerApellido));
                listadoPropiedadesCodirector.Add("SegundoApellido", UtilityExportar.EliminarRDF(Variables.ActividadDocente.direccionTesisCodirectorTesisSegundoApellido));

                UtilityExportar.AddCvnItemBeanCvnAuthorBeanList(itemBean, listadoPropiedadesCodirector, "030.040.000.180", keyValue.Value);

                listado.Add(itemBean);
            }

            //Añado en el cvnRootResultBean los items que forman parte del listado
            UtilityExportar.AniadirItems(mCvn, listado);
        }
    }
}
