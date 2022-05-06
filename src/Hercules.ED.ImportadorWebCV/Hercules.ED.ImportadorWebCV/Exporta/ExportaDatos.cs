using ExportadorWebCV.Utils;
using Hercules.ED.ImportadorWebCV.Exporta.Secciones;
using Hercules.ED.ImportadorWebCV.Exporta.Secciones.SituacionProfesionalSubclases;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace ImportadorWebCV.Exporta
{
    public class ExportaDatos : SeccionBase
    {
        private string cvID;
        private cvnRootResultBean cvn;

        public ExportaDatos(cvnRootResultBean cvn, string cvID) : base(cvn, cvID)
        {
            this.cvID = cvID;
            this.cvn = cvn;
        }

        public void ExportaDatosIdentificacion(Entity entity, [Optional] List<string> secciones, [Optional] bool preexportar)
        {
            string seccion = "http://w3id.org/roh/personalData";
            List<CvnItemBean> listado = new List<CvnItemBean>();
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

            //Añado el item al listado
            listado.Add(itemBean);

            //Añado en el cvnRootResultBean los items que forman parte del listado
            UtilityExportar.AniadirItems(cvn, listado);
        }

        public void ExportaSituacionProfesional(Entity entity, [Optional] List<string> secciones, [Optional] bool preexportar)
        {
            string seccion = "http://w3id.org/roh/professionalSituation";
            //List<CvnItemBean> listado = new List<CvnItemBean>();

            SituacionProfesionalActual situacionProfesional = new SituacionProfesionalActual(cvn,cvID);
            situacionProfesional.ExportaSituacionProfesional(entity, seccion);

            CargosActividades cargosActividades = new CargosActividades(cvn,cvID);
            cargosActividades.ExportaCargosActividades(entity);

            ////Añado en el cvnRootResultBean los items que forman parte del listado
            //UtilityExportar.AniadirItems(cvn, listado);
        }

        public void ExportaFormacionAcademica(Entity entity, [Optional] List<string> secciones, [Optional] bool preexportar)
        {
            string seccion = "http://w3id.org/roh/qualifications";
            List<CvnItemBean> listado = new List<CvnItemBean>();



            //Añado en el cvnRootResultBean los items que forman parte del listado
            UtilityExportar.AniadirItems(cvn, listado);
        }
        public void ExportaActividadDocente(Entity entity, [Optional] List<string> secciones, [Optional] bool preexportar)
        {
            string seccion = "http://w3id.org/roh/teachingExperience";
            List<CvnItemBean> listado = new List<CvnItemBean>();




            //Añado en el cvnRootResultBean los items que forman parte del listado
            UtilityExportar.AniadirItems(cvn, listado);
        }
        public void ExportaExperienciaCientificaTecnologica(Entity entity, [Optional] List<string> secciones, [Optional] bool preexportar)
        {
            string seccion = "http://w3id.org/roh/scientificExperience";
            List<CvnItemBean> listado = new List<CvnItemBean>();


            //Añado en el cvnRootResultBean los items que forman parte del listado
            UtilityExportar.AniadirItems(cvn, listado);
        }
        public void ExportaActividadCientificaTecnologica(Entity entity, [Optional] List<string> secciones, [Optional] bool preexportar)
        {
            string seccion = "http://w3id.org/roh/scientificActivity";

            List<CvnItemBean> listado = new List<CvnItemBean>();


            //Añado en el cvnRootResultBean los items que forman parte del listado
            UtilityExportar.AniadirItems(cvn, listado);
        }

        public void ExportaTextoLibre(Entity entity, [Optional] List<string> secciones, [Optional] bool preexportar)
        {
            string propResumenLibre = UtilityExportar.EliminarRDF(entity.properties.Where(x => x.prop.EndsWith(Variables.TextoLibre.resumenLibre)).Select(x => x.prop).FirstOrDefault());
            string propResumenTFG = UtilityExportar.EliminarRDF(entity.properties.Where(x => x.prop.EndsWith(Variables.TextoLibre.b1DescripcionTFG)).Select(x => x.prop).FirstOrDefault());
            string propResumenTFM = UtilityExportar.EliminarRDF(entity.properties.Where(x => x.prop.EndsWith(Variables.TextoLibre.b2DescripcionTFM)).Select(x => x.prop).FirstOrDefault());

            List<CvnItemBean> listado = new List<CvnItemBean>();
            CvnItemBean itemBean = new CvnItemBean()
            {
                Code = "070.000.000.000"
            };

            if (itemBean.Items == null)
            {
                itemBean.Items = new List<CVNObject>();
            }

            //Selecciono el ultimo valor que se corresponde a la propiedad en caso de que esta exista.
            string resumenLibre = UtilityExportar.Comprobar(entity.properties.Where(x => UtilityExportar.EliminarRDF(x.prop).EndsWith(propResumenLibre))) && !string.IsNullOrEmpty(propResumenLibre) ?
                entity.properties.Where(x => UtilityExportar.EliminarRDF(x.prop).EndsWith(propResumenLibre)).Select(x => x.values).FirstOrDefault().FirstOrDefault().Split("@@@").Last()
                : null;
            string resumenTFG = UtilityExportar.Comprobar(entity.properties.Where(x => UtilityExportar.EliminarRDF(x.prop).EndsWith(propResumenTFG))) && !string.IsNullOrEmpty(propResumenTFG) ?
                entity.properties.Where(x => UtilityExportar.EliminarRDF(x.prop).EndsWith(propResumenTFG)).Select(x => x.values).FirstOrDefault().FirstOrDefault().Split("@@@").Last()
                : null;
            string resumenTFM = UtilityExportar.Comprobar(entity.properties.Where(x => UtilityExportar.EliminarRDF(x.prop).EndsWith(propResumenTFM))) && !string.IsNullOrEmpty(propResumenTFM) ?
                entity.properties.Where(x => UtilityExportar.EliminarRDF(x.prop).EndsWith(propResumenTFM)).Select(x => x.values).FirstOrDefault().FirstOrDefault().Split("@@@").Last()
                : null;

            //Separación de los diferentes apartados por los titulos del FECYT. 
            string resumen = resumenLibre + " B.1. Breve descripción del Trabajo de Fin de Grado (TFG) y puntuación obtenida"
                + resumenTFG + " B.2. Breve descripción del Trabajo de Fin de Máster (TFM) y puntuación obtenida" + resumenTFM;

            UtilityExportar.AddCvnItemBeanCvnRichText(itemBean, resumen, "070.010.000.000");

            //Añado el item al listado
            listado.Add(itemBean);

            //Añado en el cvnRootResultBean los items que forman parte del listado
            UtilityExportar.AniadirItems(cvn, listado);
        }
    }
}
