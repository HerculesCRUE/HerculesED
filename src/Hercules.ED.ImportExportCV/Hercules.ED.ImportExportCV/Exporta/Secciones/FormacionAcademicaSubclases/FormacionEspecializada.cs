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
    public class FormacionEspecializada : SeccionBase
    {
        private readonly List<string> propiedadesItem = new List<string>() { "http://w3id.org/roh/qualifications",
            "http://w3id.org/roh/specialisedTraining", "http://vivoweb.org/ontology/core#relatedBy" };
        private readonly string graph = "academicdegree";

        public FormacionEspecializada(cvnRootResultBean cvn, string cvID) : base(cvn, cvID)
        {

        }

        /// <summary>
        /// Exporta los datos de la sección "020.020.000.000" a cvn.cvnRootResultBean
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="MultilangProp"></param>
        /// <param name="listaId"></param>
        public void ExportaFormacionEspecializada(Entity entity, Dictionary<string, List<Dictionary<string, Data>>> MultilangProp, [Optional] List<string> listaId)
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
                    Code = "020.020.000.000",
                    Items = new List<CVNObject>()
                };

                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.FormacionAcademica.formacionEspeTipoFormacion),
                    "020.020.000.010", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.FormacionAcademica.formacionEspeTipoFormacionOtros),
                    "020.020.000.020", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.FormacionAcademica.formacionEspeTituloFormacion),
                    "020.020.000.030", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.FormacionAcademica.formacionEspePaisEntidadTitulacion),
                    "020.020.000.040", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.FormacionAcademica.formacionEspeCCAAEntidadTitulacion),
                    "020.020.000.050", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.FormacionAcademica.formacionEspeCiudadEntidadTitulacion),
                    "020.020.000.070", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.FormacionAcademica.formacionEspeObjetivosEntidad),
                    "020.020.000.120", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnDurationHours(itemBean,"020.020.000.140",keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnDateDayMonthYear(itemBean, UtilityExportar.EliminarRDF(Variables.FormacionAcademica.formacionEspeFechaFinalizacion),
                    "020.020.000.150", keyValue.Value);

                //Entidad titulacion
                UtilityExportar.AddCvnItemBeanCvnEntityBean(itemBean, UtilityExportar.EliminarRDF(Variables.FormacionAcademica.formacionEspeEntidadTitulacionNombre),
                    "020.020.000.080", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.FormacionAcademica.formacionEspeTipoEntidadTitulacion),
                    "020.020.000.100", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.FormacionAcademica.formacionEspeTipoEntidadTitulacionOtros),
                    "020.020.000.110", keyValue.Value);

                //Responsable
                Dictionary<string, string> listadoPropiedades = new Dictionary<string, string>();
                listadoPropiedades.Add("Firma",Variables.FormacionAcademica.formacionEspeResponsableFirma);
                listadoPropiedades.Add("Nombre",Variables.FormacionAcademica.formacionEspeResponsableNombre);
                listadoPropiedades.Add("PrimerApellido",Variables.FormacionAcademica.formacionEspeResponsablePrimerApellido);
                listadoPropiedades.Add("SegundoApellido",Variables.FormacionAcademica.formacionEspeResponsableSegundoApellido);

                UtilityExportar.AddCvnItemBeanCvnAuthorBean(itemBean, listadoPropiedades, "020.020.000.130", keyValue.Value);

                listado.Add(itemBean);
            }

            //Añado en el cvnRootResultBean los items que forman parte del listado
            UtilityExportar.AniadirItems(mCvn, listado);
        }
    }
}
