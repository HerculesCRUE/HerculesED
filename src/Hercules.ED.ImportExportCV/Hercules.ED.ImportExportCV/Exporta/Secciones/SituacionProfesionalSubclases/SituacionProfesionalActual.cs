using ImportadorWebCV;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Utils;
using static Gnoss.ApiWrapper.ApiModel.SparqlObject;

namespace ImportadorWebCV.Exporta.Secciones.SituacionProfesionalSubclases
{
    public class SituacionProfesionalActual : SeccionBase
    {
        private readonly List<string> propiedadesItem = new List<string>() { "http://w3id.org/roh/professionalSituation",
                "http://w3id.org/roh/currentProfessionalSituation", "http://vivoweb.org/ontology/core#relatedBy" };
        private readonly string graph = "position";

        public SituacionProfesionalActual(cvnRootResultBean cvn, string cvID) : base(cvn, cvID)
        {
        }

        /// <summary>
        /// Exporta los datos de la sección "010.010.000.000" a cvn.cvnRootResultBean
        /// </summary>
        /// <param name="MultilangProp"></param>
        /// <param name="listaId"></param>
        public void ExportaSituacionProfesional(Dictionary<string, List<Dictionary<string, Data>>> MultilangProp, [Optional] List<string> listaId)
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
                CvnItemBean itemBean = new CvnItemBean();
                itemBean.Code = "010.010.000.000";
                if (itemBean.Items == null)
                {
                    itemBean.Items = new List<CVNObject>();
                }

                UtilityExportar.AddCvnItemBeanCvnBoolean(itemBean, UtilityExportar.EliminarRDF(Variables.SituacionProfesional.situacionProfesionalGestionDocente),
                    "010.010.000.010", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnEntityBean(itemBean, UtilityExportar.EliminarRDF(Variables.SituacionProfesional.situacionProfesionalFacultadEscuela),
                    "010.010.000.060", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnEntityBean(itemBean, UtilityExportar.EliminarRDF(Variables.SituacionProfesional.situacionProfesionalDepartamento),
                   "010.010.000.080", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.SituacionProfesional.situacionProfesionalCiudadEntidadEmpleadora),
                    "010.010.000.100", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.SituacionProfesional.situacionProfesionalPaisEntidadEmpleadora),
                    "010.010.000.110", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.SituacionProfesional.situacionProfesionalCCAAEntidadEmpleadora),
                   "010.010.000.120", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.SituacionProfesional.situacionProfesionalCategoriaProfesional),
                    "010.010.000.170", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnDateDayMonthYear(itemBean, UtilityExportar.EliminarRDF(Variables.SituacionProfesional.situacionProfesionalFechaInicio),
                    "010.010.000.180", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.SituacionProfesional.situacionProfesionalModalidadContrato),
                    "010.010.000.190", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.SituacionProfesional.situacionProfesionalModalidadContratoOtros),
                    "010.010.000.200", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.SituacionProfesional.situacionProfesionalRegimenDedicacion),
                    "010.010.000.210", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.SituacionProfesional.situacionProfesionalFuncionesDesempeñadas),
                    "010.010.000.250", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.SituacionProfesional.situacionProfesionalInteresDocencia),
                    "010.010.000.280", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.SituacionProfesional.situacionProfesionalAmbitoActividadGestion),
                    "010.010.000.290", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.SituacionProfesional.situacionProfesionalAmbitoActividadGestionOtros),
                    "010.010.000.300", keyValue.Value);

                // Palabras clave
                UtilityExportar.AddCvnItemBeanCvnKeyword(itemBean, UtilityExportar.EliminarRDF(Variables.SituacionProfesional.situacionProfesionalPalabrasClave),
                    "010.010.000.260", keyValue.Value);

                // Entidad empleadora
                UtilityExportar.AddCvnItemBeanCvnEntityBean(itemBean, UtilityExportar.EliminarRDF(Variables.SituacionProfesional.situacionProfesionalEntidadEmpleadoraNombre),
                    "010.010.000.020", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.SituacionProfesional.situacionProfesionalTipoEntidadEmpleadora),
                    "010.010.000.040", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.SituacionProfesional.situacionProfesionalTipoEntidadEmpleadoraOtros),
                    "010.010.000.050", keyValue.Value);

                // Telefono
                string propTelefono = Variables.SituacionProfesional.situacionProfesionalFijoNumero.Split("@@@").FirstOrDefault();
                if (!string.IsNullOrEmpty(propTelefono))
                {
                    UtilityExportar.AddCvnItemBeanCvnPhoneBean(itemBean, propTelefono, "010.010.000.140", keyValue.Value);
                }

                // Fax
                string propFax = Variables.SituacionProfesional.situacionProfesionalFaxNumero.Split("@@@").FirstOrDefault();
                if (!string.IsNullOrEmpty(propFax))
                {
                    UtilityExportar.AddCvnItemBeanCvnPhoneBean(itemBean, propFax, "010.010.000.150", keyValue.Value);
                }

                // Cod Unesco
                UtilityExportar.AddCvnItemBeanCvnKeyword(itemBean, UtilityExportar.EliminarRDF(Variables.SituacionProfesional.situacionProfesionalCodUnescoPrimaria),
                    "010.010.000.220", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnKeyword(itemBean, UtilityExportar.EliminarRDF(Variables.SituacionProfesional.situacionProfesionalCodUnescoSecundaria),
                    "010.010.000.230", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnKeyword(itemBean, UtilityExportar.EliminarRDF(Variables.SituacionProfesional.situacionProfesionalCodUnescoTerciaria),
                    "010.010.000.240", keyValue.Value);

                // Correo electronico
                List<string> listadoCorreos = UtilityExportar.Comprobar(keyValue.Value.properties.Where(x => x.prop.Equals(Variables.SituacionProfesional.situacionProfesionalCorreoElectronico))) ?
                    keyValue.Value.properties.Where(x => x.prop.Equals(Variables.SituacionProfesional.situacionProfesionalCorreoElectronico)).Select(x => x.values).FirstOrDefault()
                    : null;

                //Compruebo que los correo tienen el formato correcto
                UtilityExportar.ComprobarCorreos(listadoCorreos);

                // Si hay algún correo, guardo los correos concatenados con ';' en un string. En caso contrario guardo null.
                string correos = (listadoCorreos != null && listadoCorreos.Any()) ? string.Join(";", listadoCorreos) : null;
                if (!string.IsNullOrEmpty(correos))
                {
                    UtilityExportar.AddCvnItemBeanCvnStringSimple(itemBean, "010.010.000.160", correos);
                }
                listado.Add(itemBean);
            }

            //Añado en el cvnRootResultBean los items que forman parte del listado
            UtilityExportar.AniadirItems(mCvn, listado);
        }
    }
}
