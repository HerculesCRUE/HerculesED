using ExportadorWebCV.Utils;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ImportadorWebCV.Exporta.Secciones
{
    public class DatosIdentificacion : SeccionBase
    {
        public DatosIdentificacion(cvnRootResultBean mCvn, string cvID) : base(mCvn, cvID)
        {

        }

        public void ExportaDatosIdentificacion(Entity entity, string seccion)
        {

            CvnItemBean itemBean = new CvnItemBean()
            {
                Code = "000.010.000.000"
            };

            if (itemBean.Items == null)
            {
                itemBean.Items = new List<CVNObject>();
            }

            UtilityExportar.AddCvnItemBeanCvnString(itemBean, seccion, UtilityExportar.EliminarRDF(Variables.DatosIdentificacion.nombre), "000.010.000.020", entity);
            UtilityExportar.AddCvnItemBeanCvnFamilyNameBean(itemBean, seccion,
                new List<string>() { UtilityExportar.EliminarRDF(Variables.DatosIdentificacion.primerApellido),
                    UtilityExportar.EliminarRDF(Variables.DatosIdentificacion.segundoApellido) }, "000.010.000.010", entity);
            UtilityExportar.AddCvnItemBeanNumericValue(itemBean, seccion, UtilityExportar.EliminarRDF(Variables.DatosIdentificacion.genero), "000.010.000.030", entity);
            UtilityExportar.AddCvnItemBeanNumericValue(itemBean, seccion, UtilityExportar.EliminarRDF(Variables.DatosIdentificacion.nacionalidad), "000.010.000.040", entity);
            UtilityExportar.AddCvnItemBeanCvnDateDayMonthYear(itemBean, seccion, UtilityExportar.EliminarRDF(Variables.DatosIdentificacion.fechaNacimiento), "000.010.000.050", entity);
            UtilityExportar.AddCvnItemBeanCvnString(itemBean, seccion, UtilityExportar.EliminarRDF(Variables.DatosIdentificacion.dni), "000.010.000.100", entity);
            UtilityExportar.AddCvnItemBeanCvnString(itemBean, seccion, UtilityExportar.EliminarRDF(Variables.DatosIdentificacion.nie), "000.010.000.110", entity);
            UtilityExportar.AddCvnItemBeanCvnString(itemBean, seccion, UtilityExportar.EliminarRDF(Variables.DatosIdentificacion.pasaporte), "000.010.000.120", entity);
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
            UtilityExportar.AddCvnItemBeanCvnPhoneBean(itemBean, UtilityExportar.EliminarRDF(Variables.DatosIdentificacion.telefonoCodInternacional.Split("@@@")[0]), "000.010.000.140", entity);

            //Telefono
            UtilityExportar.AddCvnItemBeanCvnPhoneBean(itemBean, UtilityExportar.EliminarRDF(Variables.DatosIdentificacion.telefonoCodInternacional.Split("@@@")[0]), "000.010.000.140", entity);

            //Fax
            UtilityExportar.AddCvnItemBeanCvnPhoneBean(itemBean, UtilityExportar.EliminarRDF(Variables.DatosIdentificacion.telefonoCodInternacional.Split("@@@")[0]), "000.010.000.140", entity);

            //Otros identificadores
            UtilityExportar.AddCvnItemBeanCvnExternalPKBean(itemBean, seccion, UtilityExportar.EliminarRDF(Variables.DatosIdentificacion.otroIdentificador), "000.010.000.260", entity);

            //Añado en el cvnRootResultBean los items que forman parte del listado
            UtilityExportar.AniadirItems(mCvn, new List<CvnItemBean>() { itemBean });
        }
    }
}
