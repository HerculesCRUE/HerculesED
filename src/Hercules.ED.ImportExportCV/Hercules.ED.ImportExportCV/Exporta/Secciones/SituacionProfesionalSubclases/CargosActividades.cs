using Gnoss.ApiWrapper;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Utils;
using Gnoss.ApiWrapper.ApiModel;
using static Gnoss.ApiWrapper.ApiModel.SparqlObject;
using ImportadorWebCV;
using System.Runtime.InteropServices;

namespace ImportadorWebCV.Exporta.Secciones.SituacionProfesionalSubclases
{
    public class CargosActividades : SeccionBase
    {
        private readonly List<string> propiedadesItem = new () { "http://w3id.org/roh/professionalSituation",
                "http://w3id.org/roh/previousPositions", "http://vivoweb.org/ontology/core#relatedBy" };
        private readonly string graph = "position";

        public CargosActividades(cvnRootResultBean cvn, string cvID) : base(cvn, cvID)
        {
        }

        /// <summary>
        /// Exporta los datos de la sección "010.020.000.000" a cvn.cvnRootResultBean
        /// </summary>
        /// <param name="MultilangProp"></param>
        /// <param name="listaId"></param>
        public void ExportaCargosActividades(Dictionary<string, List<Dictionary<string, Data>>> MultilangProp, [Optional] List<string> listaId)
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
                    Code = "010.020.000.000",
                    Items = new List<CVNObject>()
                };

                UtilityExportar.AddCvnItemBeanCvnBoolean(itemBean, UtilityExportar.EliminarRDF(Variables.SituacionProfesional.cargosActividadesGestionDocente),
                    "010.020.000.010", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnEntityBean(itemBean, UtilityExportar.EliminarRDF(Variables.SituacionProfesional.cargosActividadesFacultadEscuela),
                    "010.020.000.060", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnEntityBean(itemBean, UtilityExportar.EliminarRDF(Variables.SituacionProfesional.cargosActividadesDepartamento),
                    "010.020.000.080", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.SituacionProfesional.cargosActividadesCiudadEntidadEmpleadora),
                    "010.020.000.100", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.SituacionProfesional.cargosActividadesPaisEntidadEmpleadora),
                    "010.020.000.110", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.SituacionProfesional.cargosActividadesCCAAEntidadEmpleadora),
                    "010.020.000.120", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.SituacionProfesional.cargosActividadesCategoriaProfesional),
                    "010.020.000.170", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnDateDayMonthYear(itemBean, UtilityExportar.EliminarRDF(Variables.SituacionProfesional.cargosActividadesFechaInicio),
                    "010.020.000.180", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnDuration(itemBean, 
                    "010.020.000.190", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.SituacionProfesional.cargosActividadesModalidadContrato),
                    "010.020.000.200", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.SituacionProfesional.cargosActividadesModalidadContratoOtros),
                    "010.020.000.210", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.SituacionProfesional.cargosActividadesRegimenDedicacion),
                    "010.020.000.220", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.SituacionProfesional.cargosActividadesFuncionesDesempeñadas),
                    "010.020.000.260", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.SituacionProfesional.cargosActividadesInteresDocencia),
                    "010.020.000.280", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.SituacionProfesional.cargosActividadesAmbitoActividadGestion),
                    "010.020.000.290", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.SituacionProfesional.cargosActividadesAmbitoActividadGestionOtros),
                    "010.020.000.300", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnDateDayMonthYear(itemBean, UtilityExportar.EliminarRDF(Variables.SituacionProfesional.cargosActividadesFechaFinalizacion),
                    "010.020.000.310", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnKeyword(itemBean, UtilityExportar.EliminarRDF(Variables.SituacionProfesional.cargosActividadesPalabrasClave),
                    "010.020.000.270",keyValue.Value);

                //Entidad empleadora
                UtilityExportar.AddCvnItemBeanCvnEntityBean(itemBean, UtilityExportar.EliminarRDF(Variables.SituacionProfesional.cargosActividadesEntidadEmpleadoraNombre),
                    "010.020.000.020", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.SituacionProfesional.cargosActividadesTipoEntidadEmpleadora),
                    "010.020.000.040", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, UtilityExportar.EliminarRDF(Variables.SituacionProfesional.cargosActividadesTipoEntidadEmpleadoraOtros),
                    "010.020.000.050", keyValue.Value);

                //Telefono
                string propTelefono = Variables.SituacionProfesional.cargosActividadesFijoNumero.Split("@@@").FirstOrDefault();
                if (!string.IsNullOrEmpty(propTelefono))
                {
                    UtilityExportar.AddCvnItemBeanCvnPhoneBean(itemBean, propTelefono, 
                        "010.020.000.140", keyValue.Value);
                }

                //Fax
                string propFax = Variables.SituacionProfesional.cargosActividadesFaxNumero.Split("@@@").FirstOrDefault();
                if (!string.IsNullOrEmpty(propFax))
                {
                    UtilityExportar.AddCvnItemBeanCvnPhoneBean(itemBean, propFax, 
                        "010.020.000.150", keyValue.Value);
                }

                // Codigo Unesco
                UtilityExportar.AddCvnItemBeanCvnKeyword(itemBean, UtilityExportar.EliminarRDF(Variables.SituacionProfesional.cargosActividadesCodUnescoPrimaria),
                    "010.020.000.230", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnKeyword(itemBean, UtilityExportar.EliminarRDF(Variables.SituacionProfesional.cargosActividadesCodUnescoSecundaria),
                    "010.020.000.240", keyValue.Value);
                UtilityExportar.AddCvnItemBeanCvnKeyword(itemBean, UtilityExportar.EliminarRDF(Variables.SituacionProfesional.cargosActividadesCodUnescoTerciaria),
                    "010.020.000.250", keyValue.Value);

                //Correo Electronico
                List<string> listadoCorreos = UtilityExportar.Comprobar(keyValue.Value.properties.Where(x => x.prop.Equals(Variables.SituacionProfesional.cargosActividadesCorreoElectronico))) ?
                keyValue.Value.properties.Where(x => x.prop.Equals(Variables.SituacionProfesional.cargosActividadesCorreoElectronico)).Select(x => x.values).FirstOrDefault() 
                : null;

                //Compruebo que los correo tienen el formato correcto
                UtilityExportar.ComprobarCorreos(listadoCorreos);

                // Si hay algún correo, guardo los correos concatenados con ';' en un string. En caso contrario guardo null.
                string correos = (listadoCorreos != null && listadoCorreos.Any()) ? string.Join(";", listadoCorreos) : null;
                if (!string.IsNullOrEmpty(correos))
                {
                    UtilityExportar.AddCvnItemBeanCvnStringSimple(itemBean, "010.020.000.160", correos);
                }

                listado.Add(itemBean);
            }

            //Añado en el cvnRootResultBean los items que forman parte del listado
            UtilityExportar.AniadirItems(mCvn, listado);
        }
    }
}
