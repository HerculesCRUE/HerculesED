using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Utils;

namespace ImportadorWebCV.Exporta.Secciones.DatosIdentificacion
{
    public class DatosIdentificacion : SeccionBase
    {
        private readonly List<string> propiedadesItem = new List<string>() { "http://w3id.org/roh/personalData" };

        public DatosIdentificacion(cvnRootResultBean mCvn, string cvID) : base(mCvn, cvID)
        {

        }

        /// <summary>
        /// Exporta los datos de la sección "000.010.000.000" a cvn.cvnRootResultBean.
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="seccion"></param>
        /// <param name="listaId"></param>
        public void ExportaDatosIdentificacion(Entity entity, string seccion, [Optional] List<string> listaId)
        {
            List<CvnItemBean> listado = new List<CvnItemBean>();

            // Selecciono los identificadores de las entidades de la seccion
            List<Tuple<string, string>> listadoIdentificadores = UtilityExportar.GetListadoEntidades(mResourceApi, propiedadesItem, mCvID);
            if (!UtilityExportar.Iniciar(mResourceApi, propiedadesItem, mCvID, listadoIdentificadores, listaId))
            {
                return;
            }

            CvnItemBean itemBean = new CvnItemBean()
            {
                Code = "000.010.000.000",
                Items = new List<CVNObject>()
            };

            UtilityExportar.AddCvnItemBeanCvnString(itemBean, seccion, UtilityExportar.EliminarRDF(Variables.DatosIdentificacion.nombre), "000.010.000.020", entity);
            UtilityExportar.AddCvnItemBeanCvnFamilyNameBean(itemBean, seccion,
                new List<string>() { UtilityExportar.EliminarRDF(Variables.DatosIdentificacion.primerApellido),
                    UtilityExportar.EliminarRDF(Variables.DatosIdentificacion.segundoApellido) }, "000.010.000.010", entity);
            UtilityExportar.AddCvnItemBeanNumericValue(itemBean, seccion, UtilityExportar.EliminarRDF(Variables.DatosIdentificacion.genero), "000.010.000.030", entity);
            UtilityExportar.AddCvnItemBeanNumericValue(itemBean, seccion, UtilityExportar.EliminarRDF(Variables.DatosIdentificacion.nacionalidad), "000.010.000.040", entity);
            UtilityExportar.AddCvnItemBeanCvnDateDayMonthYear(itemBean, seccion, UtilityExportar.EliminarRDF(Variables.DatosIdentificacion.fechaNacimiento), "000.010.000.050", entity);
            UtilityExportar.AddCvnItemBeanCvnString(itemBean, seccion, UtilityExportar.EliminarRDF(Variables.DatosIdentificacion.dni), "000.010.000.100", entity);

            //Si no he insertado el DNI busco NIE
            if (!itemBean.Items.Any(x => x.Code.Equals("000.010.000.100"))) {
                UtilityExportar.AddCvnItemBeanCvnString(itemBean, seccion, UtilityExportar.EliminarRDF(Variables.DatosIdentificacion.nie), "000.010.000.110", entity);

                //Si no he insertado DNI o NIE busco el pasaporte
                if (!itemBean.Items.Any(x => x.Code.Equals("000.010.000.110"))) {
                    UtilityExportar.AddCvnItemBeanCvnString(itemBean, seccion, UtilityExportar.EliminarRDF(Variables.DatosIdentificacion.pasaporte), "000.010.000.120", entity);
                }
            }
            UtilityExportar.AddCvnItemBeanCvnPhotoBean(itemBean, seccion, UtilityExportar.EliminarRDF(Variables.DatosIdentificacion.imagenDigital), "000.010.000.130", entity);
            UtilityExportar.AddCvnItemBeanCvnString(itemBean, seccion, UtilityExportar.EliminarRDF(Variables.DatosIdentificacion.email), "000.010.000.230", entity);
            UtilityExportar.AddCvnItemBeanCvnString(itemBean, seccion, UtilityExportar.EliminarRDF(Variables.DatosIdentificacion.paginaWeb), "000.010.000.250", entity);
            UtilityExportar.AddCvnItemBeanCvnExternalPKBean(itemBean, seccion, UtilityExportar.EliminarRDF(Variables.DatosIdentificacion.ORCID), "000.010.000.260", entity);
            UtilityExportar.AddCvnItemBeanCvnExternalPKBean(itemBean, seccion, UtilityExportar.EliminarRDF(Variables.DatosIdentificacion.scopus), "000.010.000.260", entity);
            UtilityExportar.AddCvnItemBeanCvnExternalPKBean(itemBean, seccion, UtilityExportar.EliminarRDF(Variables.DatosIdentificacion.researcherId), "000.010.000.260", entity);

            //Direccion Nacimiento
            UtilityExportar.AddDireccion(itemBean, seccion, UtilityExportar.EliminarRDF(Variables.DatosIdentificacion.direccionNacimientoPais), "000.010.000.060", entity);
            UtilityExportar.AddDireccion(itemBean, seccion, UtilityExportar.EliminarRDF(Variables.DatosIdentificacion.direccionNacimientoRegion), "000.010.000.070", entity);
            UtilityExportar.AddDireccion(itemBean, seccion, UtilityExportar.EliminarRDF(Variables.DatosIdentificacion.direccionNacimientoCiudad), "000.010.000.090", entity);

            //Direccion Contacto 
            UtilityExportar.AddDireccion(itemBean, seccion, UtilityExportar.EliminarRDF(Variables.DatosIdentificacion.direccionContactoPais), "000.010.000.180", entity);
            UtilityExportar.AddDireccion(itemBean, seccion, UtilityExportar.EliminarRDF(Variables.DatosIdentificacion.direccionContactoRegion), "000.010.000.190", entity);
            UtilityExportar.AddDireccion(itemBean, seccion, UtilityExportar.EliminarRDF(Variables.DatosIdentificacion.direccionContactoProvincia), "000.010.000.200", entity);
            UtilityExportar.AddDireccion(itemBean, seccion, UtilityExportar.EliminarRDF(Variables.DatosIdentificacion.direccionContactoCiudad), "000.010.000.170", entity);
            UtilityExportar.AddDireccion(itemBean, seccion, UtilityExportar.EliminarRDF(Variables.DatosIdentificacion.direccionContactoCodPostal), "000.010.000.160", entity);
            UtilityExportar.AddDireccion(itemBean, seccion, UtilityExportar.EliminarRDF(Variables.DatosIdentificacion.direccionContactoResto), "000.010.000.150", entity);
            UtilityExportar.AddDireccion(itemBean, seccion, UtilityExportar.EliminarRDF(Variables.DatosIdentificacion.direccionContacto), "000.010.000.140", entity);

            //Movil            
            UtilityExportar.AddCvnItemBeanCvnPhoneBean(itemBean, UtilityExportar.EliminarRDF(Variables.DatosIdentificacion.movilCodInternacional).Split("|").FirstOrDefault(), "000.010.000.240", entity);

            //Telefono
            UtilityExportar.AddCvnItemBeanCvnPhoneBean(itemBean, UtilityExportar.EliminarRDF(Variables.DatosIdentificacion.telefonoCodInternacional).Split("|").FirstOrDefault(), "000.010.000.210", entity);

            //Fax
            UtilityExportar.AddCvnItemBeanCvnPhoneBean(itemBean, UtilityExportar.EliminarRDF(Variables.DatosIdentificacion.faxCodInternacional).Split("|").FirstOrDefault(), "000.010.000.220", entity);

            //Otros identificadores
            UtilityExportar.AddCvnItemBeanCvnExternalPKBean(itemBean, seccion, UtilityExportar.EliminarRDF(Variables.DatosIdentificacion.otroIdentificador), "000.010.000.260", entity);

            //Añado en el cvnRootResultBean los items que forman parte del listado
            UtilityExportar.AniadirItems(mCvn, new List<CvnItemBean>() { itemBean });
        }
    }
}
