using ExportadorWebCV.Utils;
using ImportadorWebCV.Sincro.Secciones;
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

        private void TryAddCvnItemBeanCvnString(CvnItemBean itemBean, string section, string property, string code, Entity entity)
        {
            if (entity.properties.Where(x => x.prop.StartsWith(section)).Count() > 0 &&
                entity.properties.Where(x => x.prop.EndsWith(property)).Count() > 0)
            {
                itemBean.Items.Add(new CvnItemBeanCvnString()
                {
                    Code = code,
                    Value = entity.properties.Where(x => x.prop.EndsWith(property)).Select(x => x.values).FirstOrDefault().FirstOrDefault().Split("@@@")[1]
                });
            }
        }

        private void TryAddDireccion(CvnItemBean itemBean, string section, string property, string code, Entity entity)
        {
            if (entity.properties.Where(x => x.prop.StartsWith(section)).Count() > 0 &&
                entity.properties.Where(x => x.prop.EndsWith(property)).Count() > 0)
            {
                itemBean.Items.Add(new CvnItemBeanCvnString()
                {
                    Code = code,
                    Value = entity.properties.Where(x => x.prop.EndsWith(property)).Select(x => x.values).FirstOrDefault().FirstOrDefault().Split("@@@").Last().Split("_").Last()
                });
            }
        }

        private void TryAddCvnItemBeanCvnFamilyNameBean(CvnItemBean itemBean, string section, List<string> property, string code, Entity entity)
        {
            if (property.Count != 2)
            {
                return;
            }
            CvnItemBeanCvnFamilyNameBean cvnFamilyNameBean = new CvnItemBeanCvnFamilyNameBean();
            cvnFamilyNameBean.Code = code;

            if (entity.properties.Where(x => x.prop.StartsWith(section)).Count() == 0)
            {
                return;
            }
            TryAddCvnItemBeanCvnFamilyNameBeanFirstFamilyName(cvnFamilyNameBean, property.ElementAt(0), entity);
            TryAddCvnItemBeanCvnFamilyNameBeanSecondFamilyName(cvnFamilyNameBean, property.ElementAt(1), entity);

            itemBean.Items.Add(cvnFamilyNameBean);
        }

        private void TryAddCvnItemBeanCvnFamilyNameBeanFirstFamilyName(CvnItemBeanCvnFamilyNameBean familyNameBean, string prop, Entity entity)
        {
            if (entity.properties.Where(x => x.prop.EndsWith(prop)).Count() == 0)
            {
                return;
            }
            familyNameBean.FirstFamilyName = entity.properties.Where(x => x.prop.EndsWith(prop)).Select(x => x.values).FirstOrDefault().FirstOrDefault().Split("@@@")[1];
        }

        private void TryAddCvnItemBeanCvnFamilyNameBeanSecondFamilyName(CvnItemBeanCvnFamilyNameBean familyNameBean, string prop, Entity entity)
        {
            if (entity.properties.Where(x => x.prop.EndsWith(prop)).Count() == 0)
            {
                return;
            }
            familyNameBean.SecondFamilyName = entity.properties.Where(x => x.prop.EndsWith(prop)).Select(x => x.values).FirstOrDefault().FirstOrDefault().Split("@@@")[1];
        }

        /// <summary>
        /// Inserta en <paramref name="entity"/> el con propiedad <paramref name="property"/> de <paramref name="itemBean"/>,
        /// Debe estar Concatenado por "_", y se seleccionará el ultimo valor de la concatenación de "_"
        /// </summary>
        /// <param name="itemBean"></param>
        /// <param name="section"></param>
        /// <param name="property"></param>
        /// <param name="code"></param>
        /// <param name="entity"></param>
        private void TryAddCvnItemBeanNumericValue(CvnItemBean itemBean, string section, string property, string code, Entity entity)
        {
            if (entity.properties.Where(x => x.prop.StartsWith(section)).Count() > 0 &&
                entity.properties.Where(x => x.prop.EndsWith(property)).Count() > 0)
            {
                itemBean.Items.Add(new CvnItemBeanCvnString()
                {
                    Code = code,
                    Value = entity.properties.Where(x => x.prop.EndsWith(property)).Select(x => x.values).FirstOrDefault().FirstOrDefault().Split("@@@")[1].Split("_").Last()
                });
            }
        }

        /// <summary>
        /// Inserta en <paramref name="entity"/> el con propiedad <paramref name="property"/> de <paramref name="itemBean"/>,
        /// Debe tener formato de fecha GNOSS "yyyMMddHHmmSS" y estar concatenado por "@@@"
        /// </summary>
        /// <param name="itemBean"></param>
        /// <param name="section"></param>
        /// <param name="property"></param>
        /// <param name="code"></param>
        /// <param name="entity"></param>
        private void TryAddCvnItemBeanCvnDateDayMonthYear(CvnItemBean itemBean, string section, string property, string code, Entity entity)
        {
            if (entity.properties.Where(x => x.prop.StartsWith(section)).Count() > 0 &&
                entity.properties.Where(x => x.prop.EndsWith(property)).Count() > 0)
            {
                string gnossDate = entity.properties.Where(x => x.prop.EndsWith(property)).Select(x => x.values).FirstOrDefault().FirstOrDefault().Split("@@@")[1];
                string anio = gnossDate.Substring(0, 4);
                string mes = gnossDate.Substring(4, 2);
                string dia = gnossDate.Substring(6, 2);
                string hora = gnossDate.Substring(8, 2);
                string minuto = gnossDate.Substring(10, 2);
                string segundos = gnossDate.Substring(12, 2);
                string fecha = anio + "-" + mes + "-" + dia + "T" + hora + ":" + minuto + ":" + segundos + "+01:00";
                DateTime fechaDateTime = DateTime.Parse(fecha);
                itemBean.Items.Add(new CvnItemBeanCvnDateDayMonthYear()
                {
                    Code = code,
                    Value = fechaDateTime
                });
            }
        }

        /// <summary>
        /// Inserta en <paramref name="entity"/> el con propiedad <paramref name="property"/> de <paramref name="itemBean"/>,
        /// Debe estar Concatenado por @@@, el valor del tipo debe encontrarse entre "/" y ";", los bytes de la imagen 
        /// deben estar despues de la primera ",".
        /// </summary>
        /// <param name="itemBean"></param>
        /// <param name="section"></param>
        /// <param name="property"></param>
        /// <param name="code"></param>
        /// <param name="entity"></param>
        private void TryAddCvnItemBeanCvnPhotoBean(CvnItemBean itemBean, string section, string property, string code, Entity entity)
        {
            if (entity.properties.Where(x => x.prop.StartsWith(section)).Count() > 0 &&
                entity.properties.Where(x => x.prop.EndsWith(property)).Count() > 0)
            {
                string datos = entity.properties.Where(x => x.prop.EndsWith(property)).Select(x => x.values).FirstOrDefault().FirstOrDefault().Split("@@@")[1];
                string tipoImagen = datos.Split(";")[0].Split("/")[1];
                string bytesImagen = datos.Split(";")[1].Split(",")[1];
                itemBean.Items.Add(new CvnItemBeanCvnPhotoBean()
                {
                    Code = code,
                    BytesInBase64 = bytesImagen,
                    MimeType = tipoImagen
                });
            }
        }

        private void TryAddCvnItemBeanCvnExternalPKBean(CvnItemBean itemBean, string section, string property, string code, Entity entity)
        {
            if (entity.properties.Where(x => x.prop.StartsWith(section)).Count() > 0 &&
                entity.properties.Where(x => x.prop.EndsWith(property)).Count() > 0)
            {
                CvnItemBeanCvnExternalPKBean externalPKBean = new CvnItemBeanCvnExternalPKBean();
                externalPKBean.Code = code;

                if (property.Contains("http://w3id.org/roh/otherIds"))
                {
                    externalPKBean.Type = "OTHERS";
                    externalPKBean.Value = entity.properties.Where(x => x.prop.EndsWith(property)).Select(x => x.values).FirstOrDefault().FirstOrDefault().Split("@@@")[2];
                    externalPKBean.Others = entity.properties.Where(x => x.prop.EndsWith("http://purl.org/dc/elements/1.1/title")).Select(x => x.values).FirstOrDefault().FirstOrDefault().Split("@@@")[2];
                    itemBean.Items.Add(externalPKBean);
                    return;
                }

                switch (property)
                {
                    case "http://w3id.org/roh/ORCID":
                        externalPKBean.Type = "140";
                        break;
                    case "http://vivoweb.org/ontology/core#scopusId":
                        externalPKBean.Type = "150";
                        break;
                    case "http://vivoweb.org/ontology/core#researcherId":
                        externalPKBean.Type = "160";
                        break;
                    default:
                        break;
                }

                externalPKBean.Value = entity.properties.Where(x => x.prop.EndsWith(property)).Select(x => x.values).FirstOrDefault().FirstOrDefault().Split("@@@")[1];

                itemBean.Items.Add(externalPKBean);
            }
        }

        private void TryAddCvnItemBeanCvnPhoneBean(CvnItemBean itemBean, string section, string property, string code, Entity entity)
        {           
                CvnItemBeanCvnPhoneBean phone = new CvnItemBeanCvnPhoneBean();
                phone.Code = code;

                phone.Extension = Comprobar(entity.properties.Where(x => UtilityExportar.EliminarRDF(x.prop).EndsWith(property + "|http://w3id.org/roh/hasExtension"))) ?
                    entity.properties.Where(x => UtilityExportar.EliminarRDF(x.prop).EndsWith(property + "|http://w3id.org/roh/hasExtension"))
                    .Select(x => x.values).FirstOrDefault().FirstOrDefault().Split("@@@")[2]
                    : null;
                phone.InternationalCode = Comprobar(entity.properties.Where(x => UtilityExportar.EliminarRDF(x.prop).EndsWith(property + "|http://w3id.org/roh/hasInternationalCode"))) ?
                    entity.properties.Where(x => UtilityExportar.EliminarRDF(x.prop).EndsWith(property + "|http://w3id.org/roh/hasInternationalCode"))
                    .Select(x => x.values).FirstOrDefault().FirstOrDefault().Split("@@@")[2]
                    : null;
                phone.Number = Comprobar(entity.properties.Where(x => UtilityExportar.EliminarRDF(x.prop).EndsWith(property + "|https://www.w3.org/2006/vcard/ns#hasValue"))) ?
                    entity.properties.Where(x => UtilityExportar.EliminarRDF(x.prop).EndsWith(property + "|https://www.w3.org/2006/vcard/ns#hasValue"))
                    .Select(x => x.values).FirstOrDefault().FirstOrDefault().Split("@@@")[2]
                    : null;

                itemBean.Items.Add(phone);            
        }

        private void TryAddCvnItemBeanCvnRichText(CvnItemBean itemBean, string value, string code, Entity entity)
        {
            CvnItemBeanCvnRichText richText = new CvnItemBeanCvnRichText();
            richText.Code = code;
            richText.Value = value;

            itemBean.Items.Add(richText);
        }

        private bool Comprobar<T>(IEnumerable<T> enumeracion)
        {
            return enumeracion.Any();
        }

        public void ExportaDatosIdentificacion(Entity entity, [Optional] List<string> secciones, [Optional] bool preimportar)
        {
            string seccion = "http://w3id.org/roh/personalData";
            List<CvnItemBean> listado = new List<CvnItemBean>();
            CvnItemBean itemBean = new CvnItemBean() {
                Code = "000.010.000.000"
            };

            if (itemBean.Items == null)
            {
                itemBean.Items = new List<CVNObject>();
            }
            TryAddCvnItemBeanCvnString(itemBean, seccion, Variables.DatosIdentificacion.nombre, "000.010.000.020", entity);
            TryAddCvnItemBeanCvnFamilyNameBean(itemBean, seccion,
                new List<string>() { Variables.DatosIdentificacion.primerApellido, Variables.DatosIdentificacion.segundoApellido }, "000.010.000.010", entity);
            TryAddCvnItemBeanNumericValue(itemBean, seccion, Variables.DatosIdentificacion.genero, "000.010.000.030", entity);
            TryAddCvnItemBeanNumericValue(itemBean, seccion, Variables.DatosIdentificacion.nacionalidad, "000.010.000.040", entity);
            TryAddCvnItemBeanCvnDateDayMonthYear(itemBean, seccion, Variables.DatosIdentificacion.fechaNacimiento, "000.010.000.050", entity);
            TryAddCvnItemBeanCvnString(itemBean, seccion, Variables.DatosIdentificacion.dni, "000.010.000.100", entity);
            TryAddCvnItemBeanCvnString(itemBean, seccion, Variables.DatosIdentificacion.nie, "000.010.000.110", entity);
            TryAddCvnItemBeanCvnString(itemBean, seccion, Variables.DatosIdentificacion.pasaporte, "000.010.000.120", entity);
            TryAddCvnItemBeanCvnPhotoBean(itemBean, seccion, Variables.DatosIdentificacion.imagenDigital, "000.010.000.130", entity);
            TryAddCvnItemBeanCvnString(itemBean, seccion, Variables.DatosIdentificacion.email, "000.010.000.230", entity);
            TryAddCvnItemBeanCvnString(itemBean, seccion, Variables.DatosIdentificacion.paginaWeb, "000.010.000.250", entity);
            TryAddCvnItemBeanCvnExternalPKBean(itemBean, seccion, Variables.DatosIdentificacion.ORCID, "000.010.000.260", entity);
            TryAddCvnItemBeanCvnExternalPKBean(itemBean, seccion, Variables.DatosIdentificacion.scopus, "000.010.000.260", entity);
            TryAddCvnItemBeanCvnExternalPKBean(itemBean, seccion, Variables.DatosIdentificacion.researcherId, "000.010.000.260", entity);

            //Direccion Nacimiento
            TryAddDireccion(itemBean, seccion, UtilityExportar.EliminarRDF(Variables.DatosIdentificacion.direccionNacimientoPais), "000.010.000.060", entity);
            TryAddDireccion(itemBean, seccion, UtilityExportar.EliminarRDF(Variables.DatosIdentificacion.direccionNacimientoRegion), "000.010.000.070", entity);
            TryAddDireccion(itemBean, seccion, UtilityExportar.EliminarRDF(Variables.DatosIdentificacion.direccionNacimientoCiudad), "000.010.000.090", entity);

            //Direccion Contacto 
            TryAddDireccion(itemBean, seccion, UtilityExportar.EliminarRDF(Variables.DatosIdentificacion.direccionContactoPais), "000.010.000.180", entity);
            TryAddDireccion(itemBean, seccion, UtilityExportar.EliminarRDF(Variables.DatosIdentificacion.direccionContactoRegion), "000.010.000.190", entity);
            TryAddDireccion(itemBean, seccion, UtilityExportar.EliminarRDF(Variables.DatosIdentificacion.direccionContactoProvincia), "000.010.000.200", entity);
            TryAddDireccion(itemBean, seccion, UtilityExportar.EliminarRDF(Variables.DatosIdentificacion.direccionContactoCiudad), "000.010.000.170", entity);
            TryAddDireccion(itemBean, seccion, UtilityExportar.EliminarRDF(Variables.DatosIdentificacion.direccionContactoCodPostal), "000.010.000.160", entity);
            TryAddDireccion(itemBean, seccion, UtilityExportar.EliminarRDF(Variables.DatosIdentificacion.direccionContactoResto), "000.010.000.150", entity);
            TryAddDireccion(itemBean, seccion, UtilityExportar.EliminarRDF(Variables.DatosIdentificacion.direccionContacto), "000.010.000.140", entity);

            //Movil
            TryAddCvnItemBeanCvnPhoneBean(itemBean, seccion, UtilityExportar.EliminarRDF(Variables.DatosIdentificacion.telefonoCodInternacional.Split("@@@")[0]), "000.010.000.140", entity);

            //Telefono
            TryAddCvnItemBeanCvnPhoneBean(itemBean, seccion, UtilityExportar.EliminarRDF(Variables.DatosIdentificacion.telefonoCodInternacional.Split("@@@")[0]), "000.010.000.140", entity);

            //Fax
            TryAddCvnItemBeanCvnPhoneBean(itemBean, seccion, UtilityExportar.EliminarRDF(Variables.DatosIdentificacion.telefonoCodInternacional.Split("@@@")[0]), "000.010.000.140", entity);

            //Otros identificadores
            TryAddCvnItemBeanCvnExternalPKBean(itemBean, seccion, UtilityExportar.EliminarRDF(Variables.DatosIdentificacion.otroIdentificador), "000.010.000.260", entity);

            //Añado el item al listado
            listado.Add(itemBean);

            //Añado en el cvnRootResultBean los items que forman parte del listado
            UtilityExportar.AniadirItems(cvn, listado);
        }

        public void ExportaFormacionAcademica(Entity entity, [Optional] List<string> secciones, [Optional] bool preimportar) { }
        public void ExportaActividadDocente(Entity entity, [Optional] List<string> secciones, [Optional] bool preimportar) { }
        public void ExportaExperienciaCientificaTecnologica(Entity entity, [Optional] List<string> secciones, [Optional] bool preimportar) { }
        public void ExportaActividadCientificaTecnologiaca(Entity entity, [Optional] List<string> secciones, [Optional] bool preimportar) { }
        
        public void ExportaTextoLibre(Entity entity, [Optional] List<string> secciones, [Optional] bool preimportar)
        {
            string propResumenLibre = UtilityExportar.EliminarRDF(entity.properties.Where(x => x.prop.EndsWith(Variables.TextoLibre.resumenLibre)).Select(x => x.prop).FirstOrDefault());
            string propResumenTFG = UtilityExportar.EliminarRDF(entity.properties.Where(x => x.prop.EndsWith(Variables.TextoLibre.b1DescripcionTFG)).Select(x => x.prop).FirstOrDefault());
            string propResumenTFM = UtilityExportar.EliminarRDF(entity.properties.Where(x => x.prop.EndsWith(Variables.TextoLibre.b2DescripcionTFM)).Select(x => x.prop).FirstOrDefault());

            List<CvnItemBean> listado = new List<CvnItemBean>();
            CvnItemBean itemBean = new CvnItemBean() {
                Code = "070.000.000.000" 
            };

            if (itemBean.Items == null)
            {
                itemBean.Items = new List<CVNObject>();
            }

            //Selecciono el ultimo valor que se corresponde a la propiedad en caso de que esta exista.
            string resumenLibre = Comprobar(entity.properties.Where(x => UtilityExportar.EliminarRDF(x.prop).EndsWith(propResumenLibre))) && !string.IsNullOrEmpty(propResumenLibre) ?
                entity.properties.Where(x => UtilityExportar.EliminarRDF(x.prop).EndsWith(propResumenLibre)).Select(x => x.values).FirstOrDefault().FirstOrDefault().Split("@@@").Last()
                : null;
            string resumenTFG = Comprobar(entity.properties.Where(x => UtilityExportar.EliminarRDF(x.prop).EndsWith(propResumenTFG))) && !string.IsNullOrEmpty(propResumenTFG) ?
                entity.properties.Where(x => UtilityExportar.EliminarRDF(x.prop).EndsWith(propResumenTFG)).Select(x => x.values).FirstOrDefault().FirstOrDefault().Split("@@@").Last()
                : null;
            string resumenTFM = Comprobar(entity.properties.Where(x => UtilityExportar.EliminarRDF(x.prop).EndsWith(propResumenTFM))) && !string.IsNullOrEmpty(propResumenTFM) ?
                entity.properties.Where(x => UtilityExportar.EliminarRDF(x.prop).EndsWith(propResumenTFM)).Select(x => x.values).FirstOrDefault().FirstOrDefault().Split("@@@").Last()
                : null;

            //Separación de los diferentes apartados por los titulos del FECYT. 
            string resumen = resumenLibre + " B.1. Breve descripción del Trabajo de Fin de Grado (TFG) y puntuación obtenida"
                + resumenTFG + " B.2. Breve descripción del Trabajo de Fin de Máster (TFM) y puntuación obtenida" + resumenTFM;

            TryAddCvnItemBeanCvnRichText(itemBean, resumen, "070.010.000.000", entity);

            //Añado el item al listado
            listado.Add(itemBean);

            //Añado en el cvnRootResultBean los items que forman parte del listado
            UtilityExportar.AniadirItems(cvn, listado);
        }
    }
}
