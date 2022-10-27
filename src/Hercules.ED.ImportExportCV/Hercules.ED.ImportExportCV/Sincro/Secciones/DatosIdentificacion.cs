using Gnoss.ApiWrapper.ApiModel;
using Hercules.ED.ImportExportCV.Controllers;
using Hercules.ED.ImportExportCV.Models;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Runtime.InteropServices;
using Utils;
using static Gnoss.ApiWrapper.ApiModel.SparqlObject;
using static Models.Entity;

namespace ImportadorWebCV.Sincro.Secciones
{
    public class DatosIdentificacion : SeccionBase
    {
        public DatosIdentificacion(cvnRootResultBean cvn, string cvID, ConfigService configuracion) : base(cvn, cvID, configuracion)
        {
        }

        /// <summary>
        /// Función para sincronizar los datos pertenecientes al apartado 
        /// "Datos de identificación y contacto", con codigo identificativo 
        /// "000.000.000.000".
        /// El cual comprende los subapartados "Identificación CVN" (000.010.000.000)
        /// e Identificación de currículo (000.020.000.000).
        /// </summary>
        public List<SubseccionItem> SincroDatosIdentificacion(bool procesar, [Optional] bool preimportar, [Optional] List<string> listadoIdBBDD, [Optional] PetitionStatus petitionStatus)
        {
            //Si procesar es false, no hago nada.
            if (!procesar)
            {
                return new List<SubseccionItem>();
            }

            //Actualizo el estado de los recursos tratados
            if (petitionStatus != null)
            {
                petitionStatus.actualWork++;
                petitionStatus.actualSubWorks = 1;
                petitionStatus.actualSubTotalWorks = 1;
                petitionStatus.actualWorkSubtitle = "IMPORTACION_DATOS_IDENTIFICACION";
            }

            //1º Recuperamos los elementos necesarios del cv, del archivo xml.
            List<CvnItemBean> listadoDatosIdentificacion = mCvn.GetListadoBloque("000");

            //2º Obtenemos la entidad de BBDD.
            Entity entityBBDD = GetLoadedEntity(GetIdentificadorDatosPersonales(mCvID), "curriculumvitae");

            //3º Obtenemos la entidad de los datos del XML.
            Entity entityXML = ObtenerDatosPersonales(entityBBDD, listadoDatosIdentificacion);

            if (preimportar)
            {
                List<SubseccionItem> listaAux = new List<SubseccionItem>();
                listaAux.Add(new SubseccionItem(0, entityBBDD.id, entityXML.properties));

                return listaAux;
            }
            else
            {
                if (listadoIdBBDD != null && listadoIdBBDD.Count > 0 && listadoIdBBDD.ElementAt(0).StartsWith("http://gnoss.com/items/PersonalData_"))
                {
                    //4º Actualizamos la entidad.
                    UpdateEntityAux(mResourceApi.GetShortGuid(mCvID), new List<string>() { "http://w3id.org/roh/personalData" }, new List<string>() { entityBBDD.id }, entityBBDD, entityXML);

                    listadoIdBBDD.RemoveAt(0);
                }
                return null;
            }
        }

        /// <summary>
        /// Dado el ID del curriculum devuelve el GNOSSID de PersonalData
        /// </summary>
        /// <param name="pId">Id curriculum</param>
        /// <returns>GNOSSID de PersonalData en caso de existir</returns>
        public static string GetIdentificadorDatosPersonales(string pId)
        {
            try
            {
                string selectID = "select ?o";
                string whereID = $@"where{{
                                    <{pId}> <http://w3id.org/roh/personalData> ?o .                
                                }}";

                SparqlObject resultData = mResourceApi.VirtuosoQuery(selectID, whereID, "curriculumvitae");
                if (resultData == null || resultData.results == null || resultData.results.bindings == null || !resultData.results.bindings.Any())
                {
                    throw new KeyNotFoundException("No existe la entidad http://w3id.org/roh/personalData");
                }
                Dictionary<string, Data> fila = resultData.results.bindings.First();

                return fila["o"].value;

            }
            catch (NullReferenceException e)
            {
                Console.Error.WriteLine("Errores al cargar mResourceApi " + e.Message);
                throw new ArgumentException("Errores al cargar mResourceApi");
            }
        }

        /// <summary>
        /// Crea y puebla un Entity con los datos del subapartado 000.010.000.000
        /// que formen parte del listado.
        /// </summary>
        /// <param name="entityBBDD">Entity del objeto de BBDD</param>
        /// <param name="listadoDatosIdentificacion">Lista de datos leidos del XML</param>
        /// <returns>Entity poblado con los datos extraidos del XML</returns>
        private Entity ObtenerDatosPersonales(Entity entityBBDD, List<CvnItemBean> listadoDatosIdentificacion)
        {
            try
            {
                Entity entity = new ();
                entity.auxEntityRemove = new ();
                entity.properties = UtilitySecciones.AddProperty(
                    new Property(Variables.DatosIdentificacion.nombre, listadoDatosIdentificacion.GetStringPorIDCampo("000.010.000.020")),
                    new Property(Variables.DatosIdentificacion.primerApellido, listadoDatosIdentificacion.GetElementoPorIDCampo<CvnItemBeanCvnFamilyNameBean>("000.010.000.010")?.FirstFamilyName),
                    new Property(Variables.DatosIdentificacion.segundoApellido, listadoDatosIdentificacion.GetElementoPorIDCampo<CvnItemBeanCvnFamilyNameBean>("000.010.000.010")?.SecondFamilyName),
                    new Property(Variables.DatosIdentificacion.genero, listadoDatosIdentificacion.GetGeneroPorIDCampo("000.010.000.030")),
                    new Property(Variables.DatosIdentificacion.nacionalidad, listadoDatosIdentificacion.GetPaisPorIDCampo("000.010.000.040")),
                    new Property(Variables.DatosIdentificacion.fechaNacimiento, listadoDatosIdentificacion.GetStringDatetimePorIDCampo("000.010.000.050")),
                    new Property(Variables.DatosIdentificacion.dni, listadoDatosIdentificacion.GetStringPorIDCampo("000.010.000.100")),
                    new Property(Variables.DatosIdentificacion.nie, listadoDatosIdentificacion.GetStringPorIDCampo("000.010.000.110")),
                    new Property(Variables.DatosIdentificacion.pasaporte, listadoDatosIdentificacion.GetStringPorIDCampo("000.010.000.120")),
                    new Property(Variables.DatosIdentificacion.imagenDigital, listadoDatosIdentificacion.GetImagenPorIDCampo("000.010.000.130")),
                    new Property(Variables.DatosIdentificacion.email, listadoDatosIdentificacion.GetStringPorIDCampo("000.010.000.230")),
                    new Property(Variables.DatosIdentificacion.paginaWeb, listadoDatosIdentificacion.GetStringPorIDCampo("000.010.000.250")),
                    new Property(Variables.DatosIdentificacion.ORCID, listadoDatosIdentificacion.GetListaElementosPorIDCampo<CvnItemBeanCvnExternalPKBean>("000.010.000.260").GetORCID()),
                    new Property(Variables.DatosIdentificacion.scopus, listadoDatosIdentificacion.GetListaElementosPorIDCampo<CvnItemBeanCvnExternalPKBean>("000.010.000.260").GetScopus()),
                    new Property(Variables.DatosIdentificacion.researcherId, listadoDatosIdentificacion.GetListaElementosPorIDCampo<CvnItemBeanCvnExternalPKBean>("000.010.000.260").GetResearcherID())
                );
                GetDireccionNacimiento(listadoDatosIdentificacion, entity, entityBBDD);
                GetDireccionContacto(listadoDatosIdentificacion, entity, entityBBDD);
                GetMovil(listadoDatosIdentificacion, entity, entityBBDD);
                GetTelefono(listadoDatosIdentificacion, entity, entityBBDD);
                GetFax(listadoDatosIdentificacion, entity, entityBBDD);
                GetOtrosIdentificadores(listadoDatosIdentificacion, entity, entityBBDD);

                return entity;
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e);
                return null;
            }
        }

        /// <summary>
        ///  Lee del listado los Otros identificadores,
        ///  en caso de existir los añade en entity, 
        ///  y los marca para eliminar de entityBBDD
        /// </summary>
        /// <param name="listadoDatosIdentificacion">Listado de datos leidos del XML</param>
        /// <param name="entity">Entity a poblar</param>
        /// <param name="entityBBDD">Entity con los datos de BBDD</param>
        private void GetOtrosIdentificadores(List<CvnItemBean> listadoDatosIdentificacion, Entity entity, Entity entityBBDD)
        {
            if (listadoDatosIdentificacion.GetListaElementosPorIDCampo<CvnItemBeanCvnExternalPKBean>("000.010.000.260") == null)
            {
                return;
            }
            List<CvnItemBeanCvnExternalPKBean> listadoOtros = listadoDatosIdentificacion.GetListaElementosPorIDCampo<CvnItemBeanCvnExternalPKBean>("000.010.000.260").Where(x => x.Type.Equals("OTHERS")).ToList();
            if (listadoOtros.Count > 0)
            {
                foreach (CvnItemBeanCvnExternalPKBean item in listadoOtros)
                {
                    if (string.IsNullOrEmpty(item.Others) || string.IsNullOrEmpty(item.Value)) { return; }

                    string entityAux = Guid.NewGuid().ToString() + "|";
                    entity.properties.AddRange(UtilitySecciones.AddProperty(
                        new Property(Variables.DatosIdentificacion.otroIdentificadorTitulo, UtilitySecciones.StringGNOSSID(entityAux, item.Value)),
                        new Property(Variables.DatosIdentificacion.otroIdentificador, UtilitySecciones.StringGNOSSID(entityAux, item.Others))
                    ));
                    entity.auxEntityRemove.AddRange(entityBBDD.properties.Where(x => x.prop.Contains("http://w3id.org/roh/otherIds")).SelectMany(x => x.values).Select(x => x.Substring(0, x.IndexOf("@@@"))));
                }
            }
        }

        /// <summary>
        ///  Lee del listado los datos de la direccion de contacto,
        ///  en caso de existir los añade en entity, 
        ///  y los marca para eliminar de entityBBDD 
        /// </summary>
        /// <param name="listadoDatosIdentificacion">Listado de datos leidos del XML</param>
        /// <param name="entity">Entity a poblar</param>
        /// <param name="entityBBDD">Entity con los datos de BBDD</param>
        private void GetDireccionContacto(List<CvnItemBean> listadoDatosIdentificacion, Entity entity, Entity entityBBDD)
        {
            if (string.IsNullOrEmpty(listadoDatosIdentificacion.GetStringPorIDCampo("000.010.000.170")))
            {
                return;
            }

            string entityAux = Guid.NewGuid().ToString() + "|";
            entity.properties.AddRange(UtilitySecciones.AddProperty(
                new Property(Variables.DatosIdentificacion.direccionContactoCiudad, UtilitySecciones.StringGNOSSID(entityAux, listadoDatosIdentificacion.GetStringPorIDCampo("000.010.000.170"))),
                new Property(Variables.DatosIdentificacion.direccionContacto, UtilitySecciones.StringGNOSSID(entityAux, listadoDatosIdentificacion.GetStringPorIDCampo("000.010.000.140"))),
                new Property(Variables.DatosIdentificacion.direccionContactoResto, UtilitySecciones.StringGNOSSID(entityAux, listadoDatosIdentificacion.GetStringPorIDCampo("000.010.000.150"))),
                new Property(Variables.DatosIdentificacion.direccionContactoCodPostal, UtilitySecciones.StringGNOSSID(entityAux, listadoDatosIdentificacion.GetStringPorIDCampo("000.010.000.160"))),
                new Property(Variables.DatosIdentificacion.direccionContactoPais, UtilitySecciones.StringGNOSSID(entityAux, listadoDatosIdentificacion.GetPaisPorIDCampo("000.010.000.180"))),
                new Property(Variables.DatosIdentificacion.direccionContactoRegion, UtilitySecciones.StringGNOSSID(entityAux, listadoDatosIdentificacion.GetRegionPorIDCampo("000.010.000.190"))),
                new Property(Variables.DatosIdentificacion.direccionContactoProvincia, UtilitySecciones.StringGNOSSID(entityAux, listadoDatosIdentificacion.GetProvinciaPorIDCampo("000.010.000.200")))
            ));
            entity.auxEntityRemove.AddRange(entityBBDD.properties.Where(x => x.prop.Contains("https://www.w3.org/2006/vcard/ns#address")).SelectMany(x => x.values).Select(x => x.Substring(0, x.IndexOf("@@@"))));
        }

        /// <summary>
        ///  Lee del listado los datos de la direccion de nacimiento,
        ///  en caso de existir los añade en entity, 
        ///  y los marca para eliminar de entityBBDD 
        /// </summary>
        /// <param name="listadoDatosIdentificacion">Listado de datos leidos del XML</param>
        /// <param name="entity">Entity a poblar</param>
        /// <param name="entityBBDD">Entity con los datos de BBDD</param>
        private void GetDireccionNacimiento(List<CvnItemBean> listadoDatosIdentificacion, Entity entity, Entity entityBBDD)
        {
            if (string.IsNullOrEmpty(listadoDatosIdentificacion.GetStringPorIDCampo("000.010.000.090")))
            {
                return;
            }
            string entityAux = Guid.NewGuid().ToString() + "|";
            entity.properties.AddRange(UtilitySecciones.AddProperty(
                new Property(Variables.DatosIdentificacion.direccionNacimientoCiudad, UtilitySecciones.StringGNOSSID(entityAux, listadoDatosIdentificacion.GetStringPorIDCampo("000.010.000.090"))),
                new Property(Variables.DatosIdentificacion.direccionNacimientoPais, UtilitySecciones.StringGNOSSID(entityAux, listadoDatosIdentificacion.GetPaisPorIDCampo("000.010.000.060"))),
                new Property(Variables.DatosIdentificacion.direccionNacimientoRegion, UtilitySecciones.StringGNOSSID(entityAux, listadoDatosIdentificacion.GetRegionPorIDCampo("000.010.000.070")))
            ));
            entity.auxEntityRemove.AddRange(entityBBDD.properties.Where(x => x.prop.Contains("http://w3id.org/roh/birthplace")).SelectMany(x => x.values).Select(x => x.Substring(0, x.IndexOf("@@@"))));
        }

        /// <summary>
        ///  Lee del listado los datos del telefono,
        ///  en caso de existir los añade en entity, 
        ///  y los marca para eliminar de entityBBDD 
        /// </summary>
        /// <param name="listadoDatosIdentificacion">Listado de datos leidos del XML</param>
        /// <param name="entity">Entity a poblar</param>
        /// <param name="entityBBDD">Entity con los datos de BBDD</param>
        private void GetTelefono(List<CvnItemBean> listadoDatosIdentificacion, Entity entity, Entity entityBBDD)
        {
            if (listadoDatosIdentificacion.GetElementoPorIDCampo<CvnItemBeanCvnPhoneBean>("000.010.000.210") == null)
            {
                return;
            }
            string entityAux = Guid.NewGuid().ToString() + "|";
            entity.properties.AddRange(UtilitySecciones.AddProperty(
                new Property(Variables.DatosIdentificacion.telefonoNumero, UtilitySecciones.StringGNOSSID(entityAux, listadoDatosIdentificacion.GetElementoPorIDCampo<CvnItemBeanCvnPhoneBean>("000.010.000.210").Number.ToString())),
                new Property(Variables.DatosIdentificacion.telefonoCodInternacional, UtilitySecciones.StringGNOSSID(entityAux, listadoDatosIdentificacion.GetElementoPorIDCampo<CvnItemBeanCvnPhoneBean>("000.010.000.210").InternationalCode?.ToString())),
                new Property(Variables.DatosIdentificacion.telefonoExtension, UtilitySecciones.StringGNOSSID(entityAux, listadoDatosIdentificacion.GetElementoPorIDCampo<CvnItemBeanCvnPhoneBean>("000.010.000.210").Extension?.ToString()))
            ));
            entity.auxEntityRemove.AddRange(entityBBDD.properties.Where(x => x.prop.Contains("https://www.w3.org/2006/vcard/ns#hasTelephone")).SelectMany(x => x.values).Select(x => x.Substring(0, x.IndexOf("@@@"))));
        }

        /// <summary>
        ///  Lee del listado los datos del fax,
        ///  en caso de existir los añade en entity, 
        ///  y los marca para eliminar de entityBBDD 
        /// </summary>
        /// <param name="listadoDatosIdentificacion">Listado de datos leidos del XML</param>
        /// <param name="entity">Entity a poblar</param>
        /// <param name="entityBBDD">Entity con los datos de BBDD</param>
        private void GetFax(List<CvnItemBean> listadoDatosIdentificacion, Entity entity, Entity entityBBDD)
        {
            if (listadoDatosIdentificacion.GetElementoPorIDCampo<CvnItemBeanCvnPhoneBean>("000.010.000.220") == null)
            {
                return;
            }
            string entityAux = Guid.NewGuid().ToString() + "|";
            entity.properties.AddRange(UtilitySecciones.AddProperty(
                new Property(Variables.DatosIdentificacion.faxNumero, UtilitySecciones.StringGNOSSID(entityAux, listadoDatosIdentificacion.GetElementoPorIDCampo<CvnItemBeanCvnPhoneBean>("000.010.000.220").Number.ToString())),
                new Property(Variables.DatosIdentificacion.faxCodInternacional, UtilitySecciones.StringGNOSSID(entityAux, listadoDatosIdentificacion.GetElementoPorIDCampo<CvnItemBeanCvnPhoneBean>("000.010.000.220").InternationalCode?.ToString())),
                new Property(Variables.DatosIdentificacion.faxExtension, UtilitySecciones.StringGNOSSID(entityAux, listadoDatosIdentificacion.GetElementoPorIDCampo<CvnItemBeanCvnPhoneBean>("000.010.000.220").Extension?.ToString()))
            ));
            entity.auxEntityRemove.AddRange(entityBBDD.properties.Where(x => x.prop.Contains("http://w3id.org/roh/hasFax")).SelectMany(x => x.values).Select(x => x.Substring(0, x.IndexOf("@@@"))));
        }

        /// <summary>
        ///  Lee del listado los datos del telefono movil,
        ///  en caso de existir los añade en entity, 
        ///  y los marca para eliminar de entityBBDD 
        /// </summary>
        /// <param name="listadoDatosIdentificacion">Listado de datos leidos del XML</param>
        /// <param name="entity">Entity a poblar</param>
        /// <param name="entityBBDD">Entity con los datos de BBDD</param>
        private void GetMovil(List<CvnItemBean> listadoDatosIdentificacion, Entity entity, Entity entityBBDD)
        {
            if (listadoDatosIdentificacion.GetElementoPorIDCampo<CvnItemBeanCvnPhoneBean>("000.010.000.240") == null)
            {
                return;
            }
            string entityAux = Guid.NewGuid().ToString() + "|";
            entity.properties.AddRange(UtilitySecciones.AddProperty(
                new Property(Variables.DatosIdentificacion.movilNumero, UtilitySecciones.StringGNOSSID(entityAux, listadoDatosIdentificacion.GetElementoPorIDCampo<CvnItemBeanCvnPhoneBean>("000.010.000.240").Number.ToString())),
                new Property(Variables.DatosIdentificacion.movilCodInternacional, UtilitySecciones.StringGNOSSID(entityAux, listadoDatosIdentificacion.GetElementoPorIDCampo<CvnItemBeanCvnPhoneBean>("000.010.000.240").InternationalCode?.ToString())),
                new Property(Variables.DatosIdentificacion.movilExtension, UtilitySecciones.StringGNOSSID(entityAux, listadoDatosIdentificacion.GetElementoPorIDCampo<CvnItemBeanCvnPhoneBean>("000.010.000.240").Extension?.ToString()))
            ));
            entity.auxEntityRemove.AddRange(entityBBDD.properties.Where(x => x.prop.Contains("http://w3id.org/roh/hasMobilePhone")).SelectMany(x => x.values).Select(x => x.Substring(0, x.IndexOf("@@@"))));
        }
    }
}
