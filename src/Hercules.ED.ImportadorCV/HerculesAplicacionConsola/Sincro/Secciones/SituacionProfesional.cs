using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Models.Entity;
using Utils;
using Models;
using Hercules.ED.DisambiguationEngine.Models;
using System.Runtime.InteropServices;
using HerculesAplicacionConsola.Sincro.Secciones.SituacionProfesionalSubclases;
using Gnoss.ApiWrapper.ApiModel;
using HerculesAplicacionConsola.Utils;

namespace HerculesAplicacionConsola.Sincro.Secciones
{
    class SituacionProfesional : SeccionBase
    {
        /// <summary>
        /// Añade en la lista de propiedades de la entidad las propiedades en las 
        /// que los valores no son nulos, en caso de que los valores sean nulos se omite
        /// dicha propiedad.
        /// </summary>
        /// <param name="list"></param>
        private List<Property> AddProperty(params Property[] list)
        {
            List<Property> listado = new List<Property>();
            for (int i = 0; i < list.Length; i++)
            {
                if (!string.IsNullOrEmpty(list[i].values[0]))
                {
                    listado.Add(list[i]);
                }
            }
            return listado;
        }

        private List<CvnItemBean> listadoDatos = new List<CvnItemBean>();
        private string RdfTypeTab = "http://w3id.org/roh/ProfessionalSituation";
        public SituacionProfesional(cvnRootResultBean cvn, string cvID) : base(cvn, cvID)
        {
            listadoDatos = mCvn.GetListadoBloque("010");
        }

        /// <summary>
        /// Dada una cadena de GUID concatenados y finalizando en "|" y un string en caso de que 
        /// el string no sea nulo los concatena, sino devuelve null.
        /// </summary>
        /// <param name="entityAux">GUID concatenado con "|"</param>
        /// <param name="valor">Valor del parametro</param>
        /// <returns>String de concatenar los parametros, o nulo si el valor es vacio</returns>
        private string StringGNOSSID(string entityAux, string valor)
        {
            if (!string.IsNullOrEmpty(valor))
            {
                return entityAux + valor;
            }
            return null;
        }


        /// <summary>
        /// Función para sincronizar los datos pertenecientes al apartado 
        /// "Cargos y actividades desempeñados con anterioridad", con codigo identificativo 
        /// "010.010.000.000".
        /// </summary>
        public void SincroSituacionProfesionalActual()
        {
            List<string> propiedadesItem = new List<string>() { "http://w3id.org/roh/professionalSituation", "http://w3id.org/roh/currentProfessionalSituation", "http://vivoweb.org/ontology/core#relatedBy" };
            string graph = "position";
            string propTitle = "http://w3id.org/roh/employerOrganizationTitle";
            string rdfType = "http://vivoweb.org/ontology/core#Position";
            string rdfTypePrefix = "RelatedCurrentProfessionalSituation";


            //1º Obtenemos la entidad del XML.
            List<Entity> listadoAux = GetSituacionProfesionalActual(listadoDatos);

            Dictionary<string, DisambiguableEntity> entidadesXML = new Dictionary<string, DisambiguableEntity>();
            foreach (Entity entityXML in listadoAux)
            {
                SituacionProfesionalActual situacionProfesional = new SituacionProfesionalActual();
                situacionProfesional.nombre = entityXML.properties.FirstOrDefault(x => x.prop == Variables.SituacionProfesional.situacionProfesionalEntidadEmpleadoraNombre)?.values.FirstOrDefault();
                situacionProfesional.categoria = entityXML.properties.FirstOrDefault(x => x.prop == Variables.SituacionProfesional.situacionProfesionalCategoriaProfesional)?.values.FirstOrDefault();
                situacionProfesional.ID = Guid.NewGuid().ToString();
                entidadesXML.Add(situacionProfesional.ID, situacionProfesional);
            }

            //2º Obtenemos las entidades de la BBDD
            Dictionary<string, DisambiguableEntity> entidadesBBDD = SituacionProfesionalActual.GetBBDD(mResourceApi, mCvID, graph, propiedadesItem);

            //3º Comparamos las equivalentes
            Dictionary<string, string> equivalencias = Disambiguation.SimilarityBBDD(entidadesXML.Values.ToList(), entidadesBBDD.Values.ToList());

            //4º Añadimos o modificamos las entidades
            AniadirModificarSituacionProfesional(listadoAux, entidadesXML, equivalencias, propTitle, graph, rdfType, rdfTypePrefix, propiedadesItem);
        }

        /// <summary>
        /// Función para sincronizar los datos pertenecientes al apartado 
        /// "Situación profesional actual", con codigo identificativo 
        /// "010.020.000.000".
        /// </summary>
        public void SincroCargosActividades()
        {
            List<string> propiedadesItem = new List<string>() { "http://w3id.org/roh/professionalSituation", "http://w3id.org/roh/previousPositions", "http://vivoweb.org/ontology/core#relatedBy" };
            string graph = "position";
            string propTitle = "http://w3id.org/roh/employerOrganizationTitle";
            string rdfType = "http://vivoweb.org/ontology/core#Position";
            string rdfTypePrefix = "RelatedPreviousPositions";


            //1º Obtenemos la entidad del XML.
            List<Entity> listadoAux = GetCargosActividades(listadoDatos);

            Dictionary<string, DisambiguableEntity> entidadesXML = new Dictionary<string, DisambiguableEntity>();
            foreach (Entity entityXML in listadoAux)
            {
                CargosActividades cargosActividades = new CargosActividades();
                cargosActividades.nombre = entityXML.properties.FirstOrDefault(x => x.prop == Variables.SituacionProfesional.cargosActividadesEntidadEmpleadoraNombre)?.values.FirstOrDefault();
                cargosActividades.categoria = entityXML.properties.FirstOrDefault(x => x.prop == Variables.SituacionProfesional.cargosActividadesCategoriaProfesional)?.values.FirstOrDefault();
                cargosActividades.fechaIni = entityXML.properties.FirstOrDefault(x => x.prop == Variables.SituacionProfesional.cargosActividadesFechaInicio)?.values.FirstOrDefault();
                cargosActividades.ID = Guid.NewGuid().ToString();
                entidadesXML.Add(cargosActividades.ID, cargosActividades);
            }

            //2º Obtenemos las entidades de la BBDD
            Dictionary<string, DisambiguableEntity> entidadesBBDD = CargosActividades.GetBBDD(mResourceApi, mCvID, graph, propiedadesItem);

            //3º Comparamos las equivalentes
            Dictionary<string, string> equivalencias = Disambiguation.SimilarityBBDD(entidadesXML.Values.ToList(), entidadesBBDD.Values.ToList());

            //4º Añadimos o modificamos las entidades
            AniadirModificarSituacionProfesional(listadoAux, entidadesXML, equivalencias, propTitle, graph, rdfType, rdfTypePrefix, propiedadesItem);
        }

        /// <summary>
        /// Añade o modifica las propiedades de las entidades en BBDD comparandolas con las leidas del XML.
        /// </summary>
        private void AniadirModificarSituacionProfesional(List<Entity> listadoAux, Dictionary<string, DisambiguableEntity> entidadesXML,
            Dictionary<string, string> equivalencias, string propTitle, string graph, string rdfType, string rdfTypePrefix,
            List<string> propiedadesItem)
        {
            for (int i = 0; i < listadoAux.Count; i++)
            {
                Entity entityXML = listadoAux[i];
                string idXML = entidadesXML.Keys.ToList()[i];

                if (string.IsNullOrEmpty(equivalencias[idXML]))
                {
                    //Añadir
                    entityXML.propTitle = propTitle;
                    entityXML.ontology = graph;
                    entityXML.rdfType = rdfType;
                    CreateListEntityAux(mCvID, RdfTypeTab, rdfTypePrefix, propiedadesItem, entityXML);
                }
                else
                {
                    //Modificar
                    ModificarExistentes(equivalencias, idXML, graph, propTitle, entityXML);
                }
            }
        }

        /// <summary>
        /// 010.010.000.000
        /// </summary>
        /// <param name="listadoDatos"></param>
        /// <returns></returns>
        private List<Entity> GetSituacionProfesionalActual(List<CvnItemBean> listadoDatos)
        {
            List<Entity> listado = new List<Entity>();

            List<CvnItemBean> listadoSituacionProfesionalActual = listadoDatos.Where(x => x.Code.Equals("010.010.000.000")).ToList();
            if (listadoSituacionProfesionalActual.Count > 0)
            {
                foreach (CvnItemBean item in listadoSituacionProfesionalActual)
                {
                    Entity entidadAux = new Entity();
                    entidadAux.properties = new List<Property>();
                    if (!string.IsNullOrEmpty(item.GetNameEntityBeanPorIDCampo("010.010.000.020")))//TODO -check
                    {
                        entidadAux.properties.AddRange(AddProperty(
                            new Property(Variables.SituacionProfesional.situacionProfesionalGestionDocente, item.GetStringBooleanPorIDCampo("010.010.000.010")),
                            new Property(Variables.SituacionProfesional.situacionProfesionalFacultadEscuela, item.GetNameEntityBeanPorIDCampo("010.010.000.060")),
                            new Property(Variables.SituacionProfesional.situacionProfesionalDepartamento, item.GetNameEntityBeanPorIDCampo("010.010.000.080")),
                            new Property(Variables.SituacionProfesional.situacionProfesionalCiudadEntidadEmpleadora, item.GetStringPorIDCampo("010.010.000.100")),
                            new Property(Variables.SituacionProfesional.situacionProfesionalPaisEntidadEmpleadora, item.GetPaisPorIDCampo("010.010.000.110")),
                            new Property(Variables.SituacionProfesional.situacionProfesionalCCAAEntidadEmpleadora, item.GetRegionPorIDCampo("010.010.000.120")),
                            new Property(Variables.SituacionProfesional.situacionProfesionalCategoriaProfesional, item.GetStringPorIDCampo("010.010.000.170")),
                            new Property(Variables.SituacionProfesional.situacionProfesionalFechaInicio, item.GetStringDatetimePorIDCampo("010.010.000.180")),
                            new Property(Variables.SituacionProfesional.situacionProfesionalModalidadContrato, item.GetModalidadContrato("010.010.000.190")),
                            new Property(Variables.SituacionProfesional.situacionProfesionalModalidadContratoOtros, item.GetStringPorIDCampo("010.010.000.200")),
                            new Property(Variables.SituacionProfesional.situacionProfesionalRegimenDedicacion, item.GetRegimenDedicacion("010.010.000.210")),
                            new Property(Variables.SituacionProfesional.situacionProfesionalFuncionesDesempeñadas, item.GetStringPorIDCampo("010.010.000.250")),
                            //new Property(Variables.SituacionProfesional.situacionProfesionalPalabrasClave, item.GetStringPorIDCampo("010.010.000.260")),//rep//TODO
                            new Property(Variables.SituacionProfesional.situacionProfesionalInteresDocencia, item.GetStringPorIDCampo("010.010.000.280")),
                            new Property(Variables.SituacionProfesional.situacionProfesionalAmbitoActividadGestion, item.GetAmbitoGestion("010.010.000.290")),
                            new Property(Variables.SituacionProfesional.situacionProfesionalAmbitoActividadGestionOtros, item.GetStringPorIDCampo("010.010.000.300"))
                        ));
                        SituacionProfesionalEntidadEmpleadora(item, entidadAux);
                        SituacionProfesionalTelefono(item, entidadAux);
                        SituacionProfesionalFax(item, entidadAux);
                        SituacionProfesionalCodUnesco(item, entidadAux);
                        SituacionProfesionalCorreoElectronico(item, entidadAux);

                        listado.Add(entidadAux);
                    }
                }
            }
            return listado;
        }

        /// <summary>
        /// Inserta en <paramref name="entidadAux"/> los valores de <paramref name="item"/>,
        /// pertenecientes a la Entidad Empleadora.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="entidadAux"></param>
        private void SituacionProfesionalEntidadEmpleadora(CvnItemBean item, Entity entidadAux)
        {
            //Si no esta Entidad no añado datos
            if (string.IsNullOrEmpty(item.GetNameEntityBeanPorIDCampo("010.010.000.020"))) { return; }

            //Añado la referencia si existe
            UtilitySecciones.AniadirEntidad(mResourceApi, item.GetNameEntityBeanPorIDCampo("010.010.000.020"),
                Variables.SituacionProfesional.situacionProfesionalEntidadEmpleadoraNombre,
                Variables.SituacionProfesional.situacionProfesionalEntidadEmpleadora, entidadAux);

            //Añado otros, o el ID de una preseleccion
            string valorEE = !string.IsNullOrEmpty(item.GetStringPorIDCampo("010.010.000.050")) ? mResourceApi.GraphsUrl + "items/organizationtype_OTHERS" : item.GetOrganizacionPorIDCampo("010.010.000.040");

            entidadAux.properties.AddRange(AddProperty(
                new Property(Variables.SituacionProfesional.situacionProfesionalTipoEntidadEmpleadora, valorEE),
                new Property(Variables.SituacionProfesional.situacionProfesionalTipoEntidadEmpleadoraOtros, item.GetStringPorIDCampo("010.010.000.050"))
            ));
        }

        /// <summary>
        /// Inserta en <paramref name="entidadAux"/> los valores de <paramref name="item"/>,
        /// pertenecientes a los correos electrónicos.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="entidadAux"></param>
        private void SituacionProfesionalCorreoElectronico(CvnItemBean item, Entity entidadAux)
        {
            if (string.IsNullOrEmpty(item.GetStringPorIDCampo("010.010.000.160"))) { return; }

            List<string> listado = item.GetStringPorIDCampo("010.010.000.160").Split(";")?.ToList();
            foreach (string correo in listado)
            {
                entidadAux.properties.AddRange(AddProperty(
                    new Property(Variables.SituacionProfesional.situacionProfesionalCorreoElectronico, correo)
                ));
            }
        }

        /// <summary>
        /// Inserta en <paramref name="entidadAux"/> los valores de <paramref name="item"/>,
        /// pertenecientes al teléfono.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="entidadAux"></param>
        private void SituacionProfesionalTelefono(CvnItemBean item, Entity entidadAux)
        {
            List<CvnItemBeanCvnPhoneBean> listado = item.GetListaElementosPorIDCampo<CvnItemBeanCvnPhoneBean>("010.010.000.140");
            foreach (CvnItemBeanCvnPhoneBean telefono in listado)
            {
                if (string.IsNullOrEmpty(telefono.Number)) { continue; }

                string entityPartAux = Guid.NewGuid().ToString() + "@@@";

                //Añado Numero
                Property propertyNumero = entidadAux.properties.FirstOrDefault(x => x.prop == Variables.SituacionProfesional.situacionProfesionalFijoNumero);

                string valorNumero = StringGNOSSID(entityPartAux, telefono.Number);
                string propiedadNumero = Variables.SituacionProfesional.situacionProfesionalFijoNumero;

                UtilitySecciones.CheckProperty(propertyNumero, entidadAux, valorNumero, propiedadNumero);

                //Añado Codigo Internacional
                Property propertyCodInternacional = entidadAux.properties.FirstOrDefault(x => x.prop == Variables.SituacionProfesional.situacionProfesionalFijoCodInternacional);

                string valorCodInternacional = StringGNOSSID(entityPartAux, telefono.InternationalCode);
                string propiedadCodInternacional = Variables.SituacionProfesional.situacionProfesionalFijoCodInternacional;

                UtilitySecciones.CheckProperty(propertyCodInternacional, entidadAux, valorCodInternacional, propiedadCodInternacional);

                //Añado Extension
                Property propertyExtension = entidadAux.properties.FirstOrDefault(x => x.prop == Variables.SituacionProfesional.situacionProfesionalFijoExtension);

                string valorExtension = StringGNOSSID(entityPartAux, telefono.Extension);
                string propiedadExtension = Variables.SituacionProfesional.situacionProfesionalFijoExtension;

                UtilitySecciones.CheckProperty(propertyExtension, entidadAux, valorExtension, propiedadExtension);
            }
        }

        /// <summary>
        /// Inserta en <paramref name="entidadAux"/> los valores de <paramref name="item"/>,
        /// pertenecientes al Fax.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="entidadAux"></param>
        private void SituacionProfesionalFax(CvnItemBean item, Entity entidadAux)
        {

            List<CvnItemBeanCvnPhoneBean> listado = item.GetListaElementosPorIDCampo<CvnItemBeanCvnPhoneBean>("010.010.000.150");
            foreach (CvnItemBeanCvnPhoneBean telefono in listado)
            {
                if (string.IsNullOrEmpty(telefono.Number)) { continue; }

                string entityPartAux = Guid.NewGuid().ToString() + "@@@";
                //Añado Numero
                Property propertyNumero = entidadAux.properties.FirstOrDefault(x => x.prop == Variables.SituacionProfesional.situacionProfesionalFaxNumero);

                string valorNumero = StringGNOSSID(entityPartAux, telefono.Number);
                string propiedadNumero = Variables.SituacionProfesional.situacionProfesionalFaxNumero;

                UtilitySecciones.CheckProperty(propertyNumero, entidadAux, valorNumero, propiedadNumero);

                //Añado Codigo Internacional
                Property propertyCodInternacional = entidadAux.properties.FirstOrDefault(x => x.prop == Variables.SituacionProfesional.situacionProfesionalFaxCodInternacional);

                string valorCodInternacional = StringGNOSSID(entityPartAux, telefono.InternationalCode);
                string propiedadCodInternacional = Variables.SituacionProfesional.situacionProfesionalFaxCodInternacional;

                UtilitySecciones.CheckProperty(propertyCodInternacional, entidadAux, valorCodInternacional, propiedadCodInternacional);

                //Añado Extension
                Property propertyExtension = entidadAux.properties.FirstOrDefault(x => x.prop == Variables.SituacionProfesional.situacionProfesionalFaxExtension);

                string valorExtension = StringGNOSSID(entityPartAux, telefono.Extension);
                string propiedadExtension = Variables.SituacionProfesional.situacionProfesionalFaxExtension;

                UtilitySecciones.CheckProperty(propertyExtension, entidadAux, valorExtension, propiedadExtension);
            }
        }

        /// <summary>
        /// Inserta en <paramref name="entidadAux"/> los valores de <paramref name="item"/>,
        /// pertenecientes a los códigos UNESCO.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="entidadAux"></param>
        private void SituacionProfesionalCodUnesco(CvnItemBean item, Entity entidadAux)
        {
            List<CvnItemBeanCvnString> listadoCodUnescoPrimaria = item.GetListaElementosPorIDCampo<CvnItemBeanCvnString>("010.010.000.220");
            List<CvnItemBeanCvnString> listadoCodUnescoSecundaria = item.GetListaElementosPorIDCampo<CvnItemBeanCvnString>("010.010.000.230");
            List<CvnItemBeanCvnString> listadoCodUnescoTerciaria = item.GetListaElementosPorIDCampo<CvnItemBeanCvnString>("010.010.000.240");

            foreach (CvnItemBeanCvnString codigo in listadoCodUnescoPrimaria)
            {
                string entityPartAux = Guid.NewGuid().ToString() + "@@@";
                List<string> listadoCodigos = Utility.GetPadresCodUnesco(codigo);
                //Añado Codigo UNESCO
                foreach (string codigolista in listadoCodigos)
                {
                    Property propertyCodUnescoPrimaria = entidadAux.properties.FirstOrDefault(x => x.prop == Variables.SituacionProfesional.situacionProfesionalCodUnescoPrimaria);

                    string valorCodigo = StringGNOSSID(entityPartAux, Utility.GetCodUnescoIDCampo(codigolista));
                    string propiedadCodigo = Variables.SituacionProfesional.situacionProfesionalCodUnescoPrimaria;
                    UtilitySecciones.CheckProperty(propertyCodUnescoPrimaria, entidadAux, valorCodigo, propiedadCodigo);
                }
            }

            foreach (CvnItemBeanCvnString codigo in listadoCodUnescoSecundaria)
            {
                string entityPartAux = Guid.NewGuid().ToString() + "@@@";
                List<string> listadoCodigos = Utility.GetPadresCodUnesco(codigo);
                //Añado Codigo UNESCO
                foreach (string codigolista in listadoCodigos)
                {
                    Property propertyCodUnescoSecundaria = entidadAux.properties.FirstOrDefault(x => x.prop == Variables.SituacionProfesional.situacionProfesionalCodUnescoSecundaria);

                    string valorCodigo = StringGNOSSID(entityPartAux, Utility.GetCodUnescoIDCampo(codigolista));
                    string propiedadCodigo = Variables.SituacionProfesional.situacionProfesionalCodUnescoSecundaria;
                    UtilitySecciones.CheckProperty(propertyCodUnescoSecundaria, entidadAux, valorCodigo, propiedadCodigo);
                }
            }

            foreach (CvnItemBeanCvnString codigo in listadoCodUnescoTerciaria)
            {
                string entityPartAux = Guid.NewGuid().ToString() + "@@@";
                List<string> listadoCodigos = Utility.GetPadresCodUnesco(codigo);
                //Añado Codigo UNESCO
                foreach (string codigolista in listadoCodigos)
                {
                    Property propertyCodUnescoTerciaria = entidadAux.properties.FirstOrDefault(x => x.prop == Variables.SituacionProfesional.situacionProfesionalCodUnescoTerciaria);

                    string valorCodigo = StringGNOSSID(entityPartAux, Utility.GetCodUnescoIDCampo(codigolista));
                    string propiedadCodigo = Variables.SituacionProfesional.situacionProfesionalCodUnescoTerciaria;
                    UtilitySecciones.CheckProperty(propertyCodUnescoTerciaria, entidadAux, valorCodigo, propiedadCodigo);
                }
            }
        }

        /// <summary>
        /// 010.020.000.000
        /// </summary>
        /// <param name="listadoDatos"></param>
        /// <returns></returns>
        private List<Entity> GetCargosActividades(List<CvnItemBean> listadoDatos)
        {
            List<Entity> listado = new List<Entity>();

            List<CvnItemBean> listadoCargosActividades = listadoDatos.Where(x => x.Code.Equals("010.020.000.000")).ToList();
            if (listadoCargosActividades.Count > 0)
            {
                foreach (CvnItemBean item in listadoCargosActividades)
                {
                    Entity entidadAux = new Entity();
                    entidadAux.properties = new List<Property>();
                    if (!string.IsNullOrEmpty(item.GetNameEntityBeanPorIDCampo("010.020.000.020")))
                    {
                        entidadAux.properties.AddRange(AddProperty(
                            new Property(Variables.SituacionProfesional.cargosActividadesGestionDocente, item.GetStringBooleanPorIDCampo("010.020.000.010")),
                            new Property(Variables.SituacionProfesional.cargosActividadesFacultadEscuela, item.GetNameEntityBeanPorIDCampo("010.020.000.060")),
                            new Property(Variables.SituacionProfesional.cargosActividadesDepartamento, item.GetNameEntityBeanPorIDCampo("010.020.000.080")),
                            new Property(Variables.SituacionProfesional.cargosActividadesCiudadEntidadEmpleadora, item.GetStringPorIDCampo("010.020.000.100")),
                            new Property(Variables.SituacionProfesional.cargosActividadesPaisEntidadEmpleadora, item.GetPaisPorIDCampo("010.020.000.110")),
                            new Property(Variables.SituacionProfesional.cargosActividadesCCAAEntidadEmpleadora, item.GetRegionPorIDCampo("010.020.000.120")),
                            new Property(Variables.SituacionProfesional.cargosActividadesCategoriaProfesional, item.GetStringPorIDCampo("010.020.000.170")),
                            new Property(Variables.SituacionProfesional.cargosActividadesFechaInicio, item.GetStringDatetimePorIDCampo("010.020.000.180")),
                            new Property(Variables.SituacionProfesional.cargosActividadesDuracionAnio, item.GetDurationAnioPorIDCampo("010.020.000.190")),
                            new Property(Variables.SituacionProfesional.cargosActividadesDuracionMes, item.GetDurationMesPorIDCampo("010.020.000.190")),
                            new Property(Variables.SituacionProfesional.cargosActividadesDuracionDia, item.GetDurationDiaPorIDCampo("010.020.000.190")),
                            new Property(Variables.SituacionProfesional.cargosActividadesModalidadContrato, item.GetModalidadContrato("010.020.000.200")),
                            new Property(Variables.SituacionProfesional.cargosActividadesModalidadContratoOtros, item.GetStringPorIDCampo("010.020.000.210")),
                            new Property(Variables.SituacionProfesional.cargosActividadesRegimenDedicacion, item.GetRegimenDedicacion("010.020.000.220")),
                            new Property(Variables.SituacionProfesional.cargosActividadesFuncionesDesempeñadas, item.GetStringPorIDCampo("010.020.000.260")),
                            //new Property(Variables.SituacionProfesional.cargosActividadesPalabrasClave, item.GetStringPorIDCampo("010.020.000.270")),//TODO
                            new Property(Variables.SituacionProfesional.cargosActividadesInteresDocencia, item.GetStringPorIDCampo("010.020.000.280")),
                            new Property(Variables.SituacionProfesional.cargosActividadesAmbitoActividadGestion, item.GetAmbitoGestion("010.020.000.290")),
                            new Property(Variables.SituacionProfesional.cargosActividadesAmbitoActividadGestionOtros, item.GetStringPorIDCampo("010.020.000.300")),
                            new Property(Variables.SituacionProfesional.cargosActividadesFechaFinalizacion, item.GetStringDatetimePorIDCampo("010.020.000.310"))
                        ));
                        CargosActividadesEntidadEmpleadora(item, entidadAux);
                        CargosActividadesTelefono(item, entidadAux);
                        CargosActividadesFax(item, entidadAux);
                        CargosActividadesCodUnesco(item, entidadAux);
                        CargosActividadesCorreoElectronico(item, entidadAux);

                        listado.Add(entidadAux);
                    }
                }
            }
            return listado;
        }

        /// <summary>
        /// Inserta en <paramref name="entidadAux"/> los valores de <paramref name="item"/>,
        /// pertenecientes a la Entidad Empleadora.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="entidadAux"></param>
        private void CargosActividadesEntidadEmpleadora(CvnItemBean item, Entity entidadAux)
        {
            //Si no esta Entidad no añado datos
            if (string.IsNullOrEmpty(item.GetNameEntityBeanPorIDCampo("010.020.000.020"))) { return; }

            //Añado la referencia si existe
            UtilitySecciones.AniadirEntidad(mResourceApi, item.GetNameEntityBeanPorIDCampo("010.020.000.020"),
                Variables.SituacionProfesional.cargosActividadesEntidadEmpleadoraNombre,
                Variables.SituacionProfesional.cargosActividadesEntidadEmpleadora, entidadAux);

            //Añado otros, o el ID de una preseleccion
            string valorEE = !string.IsNullOrEmpty(item.GetStringPorIDCampo("010.020.000.050")) ? mResourceApi.GraphsUrl + "items/organizationtype_OTHERS" : item.GetOrganizacionPorIDCampo("010.020.000.040");

            entidadAux.properties.AddRange(AddProperty(
                new Property(Variables.SituacionProfesional.cargosActividadesTipoEntidadEmpleadora, valorEE),
                new Property(Variables.SituacionProfesional.cargosActividadesTipoEntidadEmpleadoraOtros, item.GetStringPorIDCampo("010.020.000.050"))
            ));
        }

        /// <summary>
        /// Inserta en <paramref name="entidadAux"/> los valores de <paramref name="item"/>,
        /// pertenecientes a los correos electrónicos.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="entidadAux"></param>
        private void CargosActividadesCorreoElectronico(CvnItemBean item, Entity entidadAux)
        {
            if (string.IsNullOrEmpty(item.GetStringPorIDCampo("010.020.000.160"))) { return; }

            List<string> listado = item.GetStringPorIDCampo("010.020.000.160").Split(";")?.ToList();
            foreach (string correo in listado)
            {
                entidadAux.properties.AddRange(AddProperty(
                    new Property(Variables.SituacionProfesional.cargosActividadesCorreoElectronico, correo)
                ));
            }
        }

        /// <summary>
        /// Inserta en <paramref name="entidadAux"/> los valores de <paramref name="item"/>,
        /// pertenecientes al teléfono.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="entidadAux"></param>
        private void CargosActividadesTelefono(CvnItemBean item, Entity entidadAux)
        {
            List<CvnItemBeanCvnPhoneBean> listado = item.GetListaElementosPorIDCampo<CvnItemBeanCvnPhoneBean>("010.020.000.140");
            foreach (CvnItemBeanCvnPhoneBean telefono in listado)
            {
                if (string.IsNullOrEmpty(telefono.Number)) { continue; }

                string entityPartAux = Guid.NewGuid().ToString() + "@@@";

                //Añado Numero
                Property propertyNumero = entidadAux.properties.FirstOrDefault(x => x.prop == Variables.SituacionProfesional.cargosActividadesFijoNumero);

                string valorNumero = StringGNOSSID(entityPartAux, telefono.Number);
                string propiedadNumero = Variables.SituacionProfesional.cargosActividadesFijoNumero;

                UtilitySecciones.CheckProperty(propertyNumero, entidadAux, valorNumero, propiedadNumero);

                //Añado Codigo Internacional
                Property propertyCodInternacional = entidadAux.properties.FirstOrDefault(x => x.prop == Variables.SituacionProfesional.cargosActividadesFijoCodInternacional);

                string valorCodInternacional = StringGNOSSID(entityPartAux, telefono.InternationalCode);
                string propiedadCodInternacional = Variables.SituacionProfesional.cargosActividadesFijoCodInternacional;

                UtilitySecciones.CheckProperty(propertyCodInternacional, entidadAux, valorCodInternacional, propiedadCodInternacional);

                //Añado Extension
                Property propertyExtension = entidadAux.properties.FirstOrDefault(x => x.prop == Variables.SituacionProfesional.cargosActividadesFijoExtension);

                string valorExtension = StringGNOSSID(entityPartAux, telefono.Extension);
                string propiedadExtension = Variables.SituacionProfesional.cargosActividadesFijoExtension;

                UtilitySecciones.CheckProperty(propertyExtension, entidadAux, valorExtension, propiedadExtension);
            }
        }

        /// <summary>
        /// Inserta en <paramref name="entidadAux"/> los valores de <paramref name="item"/>,
        /// pertenecientes al Fax.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="entidadAux"></param>
        private void CargosActividadesFax(CvnItemBean item, Entity entidadAux)
        {
            List<CvnItemBeanCvnPhoneBean> listado = item.GetListaElementosPorIDCampo<CvnItemBeanCvnPhoneBean>("010.020.000.150");
            foreach (CvnItemBeanCvnPhoneBean telefono in listado)
            {
                if (string.IsNullOrEmpty(telefono.Number)) { continue; }

                string entityPartAux = Guid.NewGuid().ToString() + "@@@";
                //Añado Numero
                Property propertyNumero = entidadAux.properties.FirstOrDefault(x => x.prop == Variables.SituacionProfesional.cargosActividadesFaxNumero);

                string valorNumero = StringGNOSSID(entityPartAux, telefono.Number);
                string propiedadNumero = Variables.SituacionProfesional.cargosActividadesFaxNumero;

                UtilitySecciones.CheckProperty(propertyNumero, entidadAux, valorNumero, propiedadNumero);

                //Añado Codigo Internacional
                Property propertyCodInternacional = entidadAux.properties.FirstOrDefault(x => x.prop == Variables.SituacionProfesional.cargosActividadesFaxCodInternacional);

                string valorCodInternacional = StringGNOSSID(entityPartAux, telefono.InternationalCode);
                string propiedadCodInternacional = Variables.SituacionProfesional.cargosActividadesFaxCodInternacional;

                UtilitySecciones.CheckProperty(propertyCodInternacional, entidadAux, valorCodInternacional, propiedadCodInternacional);

                //Añado Extension
                Property propertyExtension = entidadAux.properties.FirstOrDefault(x => x.prop == Variables.SituacionProfesional.cargosActividadesFaxExtension);

                string valorExtension = StringGNOSSID(entityPartAux, telefono.Extension);
                string propiedadExtension = Variables.SituacionProfesional.cargosActividadesFaxExtension;

                UtilitySecciones.CheckProperty(propertyExtension, entidadAux, valorExtension, propiedadExtension);
            }
        }

        /// <summary>
        /// Inserta en <paramref name="entidadAux"/> los valores de <paramref name="item"/>,
        /// pertenecientes a los códigos UNESCO.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="entidadAux"></param>
        private void CargosActividadesCodUnesco(CvnItemBean item, Entity entidadAux)
        {
            List<CvnItemBeanCvnString> listadoCodUnescoPrimaria = item.GetListaElementosPorIDCampo<CvnItemBeanCvnString>("010.020.000.230");
            List<CvnItemBeanCvnString> listadoCodUnescoSecundaria = item.GetListaElementosPorIDCampo<CvnItemBeanCvnString>("010.020.000.240");
            List<CvnItemBeanCvnString> listadoCodUnescoTerciaria = item.GetListaElementosPorIDCampo<CvnItemBeanCvnString>("010.020.000.250");

            foreach (CvnItemBeanCvnString codigo in listadoCodUnescoPrimaria)
            {
                string entityPartAux = Guid.NewGuid().ToString() + "@@@";
                List<string> listadoCodigos = Utility.GetPadresCodUnesco(codigo);
                //Añado Codigo UNESCO
                foreach (string codigolista in listadoCodigos)
                {
                    Property propertyCodUnescoPrimaria = entidadAux.properties.FirstOrDefault(x => x.prop == Variables.SituacionProfesional.cargosActividadesCodUnescoPrimaria);

                    string valorCodigo = StringGNOSSID(entityPartAux, Utility.GetCodUnescoIDCampo(codigolista));
                    string propiedadCodigo = Variables.SituacionProfesional.cargosActividadesCodUnescoPrimaria;
                    UtilitySecciones.CheckProperty(propertyCodUnescoPrimaria, entidadAux, valorCodigo, propiedadCodigo);
                }
            }

            foreach (CvnItemBeanCvnString codigo in listadoCodUnescoSecundaria)
            {
                string entityPartAux = Guid.NewGuid().ToString() + "@@@";
                List<string> listadoCodigos = Utility.GetPadresCodUnesco(codigo);
                //Añado Codigo UNESCO
                foreach (string codigolista in listadoCodigos)
                {
                    Property propertyCodUnescoSecundaria = entidadAux.properties.FirstOrDefault(x => x.prop == Variables.SituacionProfesional.cargosActividadesCodUnescoSecundaria);

                    string valorCodigo = StringGNOSSID(entityPartAux, Utility.GetCodUnescoIDCampo(codigolista));
                    string propiedadCodigo = Variables.SituacionProfesional.cargosActividadesCodUnescoSecundaria;
                    UtilitySecciones.CheckProperty(propertyCodUnescoSecundaria, entidadAux, valorCodigo, propiedadCodigo);
                }
            }

            foreach (CvnItemBeanCvnString codigo in listadoCodUnescoTerciaria)
            {
                string entityPartAux = Guid.NewGuid().ToString() + "@@@";
                List<string> listadoCodigos = Utility.GetPadresCodUnesco(codigo);
                //Añado Codigo UNESCO
                foreach (string codigolista in listadoCodigos)
                {
                    Property propertyCodUnescoTerciaria = entidadAux.properties.FirstOrDefault(x => x.prop == Variables.SituacionProfesional.cargosActividadesCodUnescoTerciaria);

                    string valorCodigo = StringGNOSSID(entityPartAux, Utility.GetCodUnescoIDCampo(codigolista));
                    string propiedadCodigo = Variables.SituacionProfesional.cargosActividadesCodUnescoTerciaria;
                    UtilitySecciones.CheckProperty(propertyCodUnescoTerciaria, entidadAux, valorCodigo, propiedadCodigo);
                }
            }
        }


    }
}
