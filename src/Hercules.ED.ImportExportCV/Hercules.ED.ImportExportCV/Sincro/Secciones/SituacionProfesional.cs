using System;
using System.Collections.Generic;
using System.Linq;
using static Models.Entity;
using Utils;
using Models;
using Hercules.CommonsEDMA.DisambiguationEngine.Models;
using System.Runtime.InteropServices;
using ImportadorWebCV.Sincro.Secciones.SituacionProfesionalSubclases;
using Hercules.ED.ImportExportCV.Models;
using Hercules.ED.ImportExportCV.Controllers;

namespace ImportadorWebCV.Sincro.Secciones
{
    class SituacionProfesional : SeccionBase
    {
        private readonly List<CvnItemBean> listadoDatos;
        private readonly List<CvnItemBean> listadoCvn;
        private readonly string RdfTypeTab = "http://w3id.org/roh/ProfessionalSituation";
        public SituacionProfesional(cvnRootResultBean cvn, string cvID, string personID, ConfigService configuracion) : base(cvn, cvID, personID, configuracion)
        {
            listadoDatos = mCvn.GetListadoBloque("010");
            listadoCvn = mCvn.cvnRootBean.ToList();
        }

        /// <summary>
        /// Función para sincronizar los datos pertenecientes al apartado 
        /// "Situación profesional actual", con codigo identificativo 
        /// "010.010.000.000".
        /// </summary>
        public List<SubseccionItem> SincroSituacionProfesionalActual(bool procesar, [Optional] bool preimportar, [Optional] List<string> listadoIdBBDD, [Optional] PetitionStatus petitionStatus)
        {
            //Si procesar es false, no hago nada.
            if (!procesar)
            {
                return new List<SubseccionItem>();
            }

            List<string> propiedadesItem = new() { "http://w3id.org/roh/professionalSituation", "http://w3id.org/roh/currentProfessionalSituation", "http://vivoweb.org/ontology/core#relatedBy" };
            string graph = "position";
            string propTitle = "http://w3id.org/roh/employerOrganizationTitle";
            string rdfType = "http://vivoweb.org/ontology/core#Position";
            string rdfTypePrefix = "RelatedCurrentProfessionalSituation";

            Dictionary<string, DisambiguableEntity> entidadesXML = new();
            Dictionary<string, string> equivalencias = new();
            List<bool> listadoBloqueados = new();

            if (petitionStatus != null)
            {
                petitionStatus.actualWorkSubtitle = "IMPORTACION_SITUACION_PROFESIONAL";
            }

            //1º Obtenemos la entidad del XML.
            List<Entity> listadoAux = GetSituacionProfesionalActual(listadoDatos, petitionStatus);

            if (listadoIdBBDD == null)
            {
                foreach (Entity entityXML in listadoAux)
                {
                    SituacionProfesionalActual situacionProfesional = new();
                    situacionProfesional.Nombre = entityXML.properties.FirstOrDefault(x => x.prop == Variables.SituacionProfesional.situacionProfesionalEntidadEmpleadoraNombre)?.values.FirstOrDefault();
                    situacionProfesional.Categoria = entityXML.properties.FirstOrDefault(x => x.prop == Variables.SituacionProfesional.situacionProfesionalCategoriaProfesional)?.values.FirstOrDefault();
                    situacionProfesional.ID = Guid.NewGuid().ToString();
                    entidadesXML.Add(situacionProfesional.ID, situacionProfesional);
                }

                //2º Obtenemos las entidades de la BBDD
                Dictionary<string, DisambiguableEntity> entidadesBBDD = SituacionProfesionalActual.GetBBDDSitProf(mResourceApi, mCvID, graph, propiedadesItem);
                var entidadesBBDDOpciones = entidadesBBDD.Select(x => new { x.Value.ID, x.Value.block }).ToList();

                //3º Comparamos las equivalentes
                equivalencias = Disambiguation.SimilarityBBDD(entidadesXML.Values.ToList(), entidadesBBDD.Values.ToList());

                foreach (var item in equivalencias.Values)
                {
                    listadoBloqueados.Add(entidadesBBDDOpciones.Where(x => x.ID.Equals(item)).Select(x => x.block).FirstOrDefault());
                }
            }

            return CheckPreimportar(preimportar, listadoAux, entidadesXML, equivalencias, propTitle, graph, rdfType, rdfTypePrefix,
                propiedadesItem, RdfTypeTab, listadoBloqueados, listadoIdBBDD: listadoIdBBDD, petitionStatus: petitionStatus);
        }

        /// <summary>
        /// Función para sincronizar los datos pertenecientes al apartado 
        /// "Cargos y actividades desempeñados con anterioridad", con codigo identificativo 
        /// "010.020.000.000".
        /// </summary>
        public List<SubseccionItem> SincroCargosActividades(bool procesar, [Optional] bool preimportar, [Optional] List<string> listadoIdBBDD, [Optional] PetitionStatus petitionStatus)
        {
            //Si procesar es false, no hago nada.
            if (!procesar)
            {
                return new List<SubseccionItem>();
            }

            List<string> propiedadesItem = new() { "http://w3id.org/roh/professionalSituation", "http://w3id.org/roh/previousPositions", "http://vivoweb.org/ontology/core#relatedBy" };
            string graph = "position";
            string propTitle = "http://w3id.org/roh/employerOrganizationTitle";
            string rdfType = "http://vivoweb.org/ontology/core#Position";
            string rdfTypePrefix = "RelatedPreviousPositions";

            Dictionary<string, DisambiguableEntity> entidadesXML = new();
            Dictionary<string, string> equivalencias = new();
            List<bool> listadoBloqueados = new();

            if (petitionStatus != null)
            {
                petitionStatus.actualWorkSubtitle = "IMPORTACION_CARGOS_ACTIVIDADES";
            }

            //1º Obtenemos la entidad del XML.
            List<Entity> listadoAux = GetCargosActividades(listadoDatos, petitionStatus);

            if (listadoIdBBDD == null)
            {
                foreach (Entity entityXML in listadoAux)
                {
                    CargosActividades cargosActividades = new();
                    cargosActividades.Nombre = entityXML.properties.FirstOrDefault(x => x.prop == Variables.SituacionProfesional.cargosActividadesEntidadEmpleadoraNombre)?.values.FirstOrDefault();
                    cargosActividades.Categoria = entityXML.properties.FirstOrDefault(x => x.prop == Variables.SituacionProfesional.cargosActividadesCategoriaProfesional)?.values.FirstOrDefault();
                    cargosActividades.FechaIni = entityXML.properties.FirstOrDefault(x => x.prop == Variables.SituacionProfesional.cargosActividadesFechaInicio)?.values.FirstOrDefault();
                    cargosActividades.ID = Guid.NewGuid().ToString();
                    entidadesXML.Add(cargosActividades.ID, cargosActividades);
                }

                //2º Obtenemos las entidades de la BBDD
                Dictionary<string, DisambiguableEntity> entidadesBBDD = CargosActividades.GetBBDDCarAct(mResourceApi, mCvID, graph, propiedadesItem);
                var entidadesBBDDOpciones = entidadesBBDD.Select(x => new { x.Value.ID, x.Value.block }).ToList();

                //3º Comparamos las equivalentes
                equivalencias = Disambiguation.SimilarityBBDD(entidadesXML.Values.ToList(), entidadesBBDD.Values.ToList());

                foreach (var item in equivalencias.Values)
                {
                    listadoBloqueados.Add(entidadesBBDDOpciones.Where(x => x.ID.Equals(item)).Select(x => x.block).FirstOrDefault());
                }
            }

            return CheckPreimportar(preimportar, listadoAux, entidadesXML, equivalencias, propTitle, graph, rdfType, rdfTypePrefix,
                propiedadesItem, RdfTypeTab, listadoBloqueados, listadoIdBBDD: listadoIdBBDD, petitionStatus: petitionStatus);
        }


        /// <summary>
        /// 010.010.000.000
        /// </summary>
        /// <param name="listadoDatos"></param>
        /// <returns></returns>
        private List<Entity> GetSituacionProfesionalActual(List<CvnItemBean> listadoDatos, [Optional] PetitionStatus petitionStatus)
        {
            List<Entity> listado = new();

            List<CvnItemBean> listadoSituacionProfesionalActual = listadoDatos.Where(x => x.Code.Equals("010.010.000.000")).ToList();
            if (listadoSituacionProfesionalActual.Count > 0)
            {
                if (petitionStatus != null)
                {
                    petitionStatus.actualSubWorks = 1;
                    petitionStatus.actualSubTotalWorks = listadoSituacionProfesionalActual.Count;
                }

                foreach (CvnItemBean item in listadoSituacionProfesionalActual)
                {
                    //Actualizo el estado de los recursos tratados
                    if (petitionStatus != null)
                    {
                        petitionStatus.actualWork++;
                        petitionStatus.actualSubWorks++;
                    }

                    Entity entidadAux = new();
                    entidadAux.properties = new();
                    if (!string.IsNullOrEmpty(item.GetNameEntityBeanPorIDCampo("010.010.000.020")) && !string.IsNullOrEmpty(item.GetStringPorIDCampo("010.010.000.170")))
                    {
                        entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                            new Property(Variables.Generico.codigoCVN, "010.010.000.000"),
                            new Property(Variables.Generico.personaCVN, mPersonID)
                        ));
                        entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
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
                            new Property(Variables.SituacionProfesional.situacionProfesionalInteresDocencia, item.GetStringPorIDCampo("010.010.000.280")),
                            new Property(Variables.SituacionProfesional.situacionProfesionalAmbitoActividadGestion, item.GetAmbitoGestion("010.010.000.290")),
                            new Property(Variables.SituacionProfesional.situacionProfesionalAmbitoActividadGestionOtros, item.GetStringPorIDCampo("010.010.000.300"))
                        ));
                        SituacionProfesionalPalabrasClave(item, entidadAux);
                        SituacionProfesionalEntidadEmpleadora(item, entidadAux);
                        SituacionProfesionalTelefono(item, entidadAux);
                        SituacionProfesionalFax(item, entidadAux);
                        SituacionProfesionalCodUnesco(item, entidadAux);
                        SituacionProfesionalCorreoElectronico(item, entidadAux);

                        listado.Add(entidadAux);
                    }
                    else
                    {
                        listadoCvn.Remove(item);
                        mCvn.cvnRootBean = listadoCvn.ToArray();
                    }
                }
            }
            return listado;
        }

        /// <summary>
        /// Inserta en <paramref name="entidadAux"/> los valores de <paramref name="item"/>,
        /// pertenecientes a las palabras clave.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="entidadAux"></param>
        private static void SituacionProfesionalPalabrasClave(CvnItemBean item, Entity entidadAux)
        {
            List<CvnItemBeanCvnString> listadoPalabrasClave = item.GetListaElementosPorIDCampo<CvnItemBeanCvnString>("010.010.000.260");

            string propiedadPalabrasClave = Variables.SituacionProfesional.situacionProfesionalPalabrasClave;

            foreach (CvnItemBeanCvnString palabraClave in listadoPalabrasClave)
            {
                string entityPartAux = Guid.NewGuid().ToString() + "@@@";

                List<string> listadoPalabras = Utility.GetPadresPalabrasClave(palabraClave);
                foreach (string palabra in listadoPalabras)
                {
                    Property propertyPalabrasClave = entidadAux.properties.FirstOrDefault(x => x.prop == Variables.SituacionProfesional.situacionProfesionalPalabrasClave);
                    UtilitySecciones.CheckProperty(propertyPalabrasClave, entidadAux,
                        UtilitySecciones.StringGNOSSID(entityPartAux, Utility.ObtenerPalabraClave(mResourceApi, palabra)), propiedadPalabrasClave);
                }
            }
        }

        /// <summary>
        /// Inserta en <paramref name="entidadAux"/> los valores de <paramref name="item"/>,
        /// pertenecientes a la Entidad Empleadora.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="entidadAux"></param>
        private static void SituacionProfesionalEntidadEmpleadora(CvnItemBean item, Entity entidadAux)
        {
            //Si no esta Entidad no añado datos
            if (string.IsNullOrEmpty(item.GetNameEntityBeanPorIDCampo("010.010.000.020"))) { return; }

            //Añado la referencia si existe
            UtilitySecciones.AniadirEntidadOrganizacion(mResourceApi, item.GetNameEntityBeanPorIDCampo("010.010.000.020"),
                Variables.SituacionProfesional.situacionProfesionalEntidadEmpleadoraNombre,
                Variables.SituacionProfesional.situacionProfesionalEntidadEmpleadora, entidadAux);

            //Añado otros, o el ID de una preseleccion
            string valorEE = !string.IsNullOrEmpty(item.GetStringPorIDCampo("010.010.000.050")) ? mResourceApi.GraphsUrl + "items/organizationtype_OTHERS" : item.GetOrganizacionPorIDCampo("010.010.000.040");

            entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
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
        private static void SituacionProfesionalCorreoElectronico(CvnItemBean item, Entity entidadAux)
        {
            if (string.IsNullOrEmpty(item.GetStringPorIDCampo("010.010.000.160"))) { return; }

            List<string> listado = item.GetStringPorIDCampo("010.010.000.160").Split(";")?.ToList();
            if (listado == null || !listado.Any())
            {
                return;
            }

            foreach (string correo in listado)
            {
                entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
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
        private static void SituacionProfesionalTelefono(CvnItemBean item, Entity entidadAux)
        {
            List<CvnItemBeanCvnPhoneBean> listado = item.GetListaElementosPorIDCampo<CvnItemBeanCvnPhoneBean>("010.010.000.140");

            string propiedadNumero = Variables.SituacionProfesional.situacionProfesionalFijoNumero;
            string propiedadCodInternacional = Variables.SituacionProfesional.situacionProfesionalFijoCodInternacional;
            string propiedadExtension = Variables.SituacionProfesional.situacionProfesionalFijoExtension;

            UtilitySecciones.InsertarListadoTelefonos(listado, entidadAux, propiedadNumero, propiedadCodInternacional, propiedadExtension);
        }

        /// <summary>
        /// Inserta en <paramref name="entidadAux"/> los valores de <paramref name="item"/>,
        /// pertenecientes al Fax.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="entidadAux"></param>
        private static void SituacionProfesionalFax(CvnItemBean item, Entity entidadAux)
        {
            List<CvnItemBeanCvnPhoneBean> listado = item.GetListaElementosPorIDCampo<CvnItemBeanCvnPhoneBean>("010.010.000.150");

            string propiedadNumero = Variables.SituacionProfesional.situacionProfesionalFaxNumero;
            string propiedadCodInternacional = Variables.SituacionProfesional.situacionProfesionalFaxCodInternacional;
            string propiedadExtension = Variables.SituacionProfesional.situacionProfesionalFaxExtension;

            UtilitySecciones.InsertarListadoTelefonos(listado, entidadAux, propiedadNumero, propiedadCodInternacional, propiedadExtension);
        }

        /// <summary>
        /// Inserta en <paramref name="entidadAux"/> los valores de <paramref name="item"/>,
        /// pertenecientes a los Códigos UNESCO de especialización primaria, secundaria y terciaria.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="entidadAux"></param>
        private static void SituacionProfesionalCodUnesco(CvnItemBean item, Entity entidadAux)
        {
            //Añado los códigos UNESCO de especialización primaria
            List<CvnItemBeanCvnString> listadoCodUnescoPrimaria = item.GetListaElementosPorIDCampo<CvnItemBeanCvnString>("010.010.000.220");
            UtilitySecciones.CodigosUnesco(listadoCodUnescoPrimaria, entidadAux, Variables.SituacionProfesional.situacionProfesionalCodUnescoPrimaria);

            //Añado los códigos UNESCO de especialización secundaria
            List<CvnItemBeanCvnString> listadoCodUnescoSecundaria = item.GetListaElementosPorIDCampo<CvnItemBeanCvnString>("010.010.000.230");
            UtilitySecciones.CodigosUnesco(listadoCodUnescoSecundaria, entidadAux, Variables.SituacionProfesional.situacionProfesionalCodUnescoSecundaria);

            //Añado los códigos UNESCO de especialización terciaria
            List<CvnItemBeanCvnString> listadoCodUnescoTerciaria = item.GetListaElementosPorIDCampo<CvnItemBeanCvnString>("010.010.000.240");
            UtilitySecciones.CodigosUnesco(listadoCodUnescoTerciaria, entidadAux, Variables.SituacionProfesional.situacionProfesionalCodUnescoTerciaria);

        }

        /// <summary>
        /// 010.020.000.000
        /// </summary>
        /// <param name="listadoDatos"></param>
        /// <returns></returns>
        private List<Entity> GetCargosActividades(List<CvnItemBean> listadoDatos, [Optional] PetitionStatus petitionStatus)
        {
            List<Entity> listado = new();

            List<CvnItemBean> listadoCargosActividades = listadoDatos.Where(x => x.Code.Equals("010.020.000.000")).ToList();
            if (listadoCargosActividades.Count > 0)
            {
                if (petitionStatus != null)
                {
                    petitionStatus.actualSubWorks = 1;
                    petitionStatus.actualSubTotalWorks = listadoCargosActividades.Count;
                }

                foreach (CvnItemBean item in listadoCargosActividades)
                {
                    //Actualizo el estado de los recursos tratados
                    if (petitionStatus != null)
                    {
                        petitionStatus.actualWork++;
                        petitionStatus.actualSubWorks++;
                    }

                    Entity entidadAux = new();
                    entidadAux.properties = new();
                    if (!string.IsNullOrEmpty(item.GetNameEntityBeanPorIDCampo("010.020.000.020")) && !string.IsNullOrEmpty(item.GetStringPorIDCampo("010.020.000.170")))
                    {
                        entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                            new Property(Variables.Generico.codigoCVN, "010.020.000.000"),
                            new Property(Variables.Generico.personaCVN, mPersonID)
                        ));
                        entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
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
                            new Property(Variables.SituacionProfesional.cargosActividadesInteresDocencia, item.GetStringPorIDCampo("010.020.000.280")),
                            new Property(Variables.SituacionProfesional.cargosActividadesAmbitoActividadGestion, item.GetAmbitoGestion("010.020.000.290")),
                            new Property(Variables.SituacionProfesional.cargosActividadesAmbitoActividadGestionOtros, item.GetStringPorIDCampo("010.020.000.300")),
                            new Property(Variables.SituacionProfesional.cargosActividadesFechaFinalizacion, item.GetStringDatetimePorIDCampo("010.020.000.310"))
                        ));
                        CargosActividadesPalabrasClave(item, entidadAux);
                        CargosActividadesEntidadEmpleadora(item, entidadAux);
                        CargosActividadesTelefono(item, entidadAux);
                        CargosActividadesFax(item, entidadAux);
                        CargosActividadesCodUnesco(item, entidadAux);
                        CargosActividadesCorreoElectronico(item, entidadAux);

                        listado.Add(entidadAux);
                    }
                    else
                    {
                        listadoCvn.Remove(item);
                        mCvn.cvnRootBean = listadoCvn.ToArray();
                    }
                }
            }
            return listado;
        }

        /// <summary>
        /// Inserta en <paramref name="entidadAux"/> los valores de <paramref name="item"/>,
        /// pertenecientes a las palabras clave.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="entidadAux"></param>
        private static void CargosActividadesPalabrasClave(CvnItemBean item, Entity entidadAux)
        {
            List<CvnItemBeanCvnString> listadoPalabrasClave = item.GetListaElementosPorIDCampo<CvnItemBeanCvnString>("010.020.000.270");

            string propiedadPalabrasClave = Variables.SituacionProfesional.cargosActividadesPalabrasClave;

            foreach (CvnItemBeanCvnString palabraClave in listadoPalabrasClave)
            {
                string entityPartAux = Guid.NewGuid().ToString() + "@@@";

                List<string> listadoPalabras = Utility.GetPadresPalabrasClave(palabraClave);
                foreach (string palabra in listadoPalabras)
                {
                    Property propertyPalabrasClave = entidadAux.properties.FirstOrDefault(x => x.prop == Variables.SituacionProfesional.cargosActividadesPalabrasClave);
                    UtilitySecciones.CheckProperty(propertyPalabrasClave, entidadAux,
                        UtilitySecciones.StringGNOSSID(entityPartAux, Utility.ObtenerPalabraClave(mResourceApi, palabra)), propiedadPalabrasClave);
                }
            }
        }

        /// <summary>
        /// Inserta en <paramref name="entidadAux"/> los valores de <paramref name="item"/>,
        /// pertenecientes a la Entidad Empleadora.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="entidadAux"></param>
        private static void CargosActividadesEntidadEmpleadora(CvnItemBean item, Entity entidadAux)
        {
            //Si no esta Entidad no añado datos
            if (string.IsNullOrEmpty(item.GetNameEntityBeanPorIDCampo("010.020.000.020"))) { return; }

            //Añado la referencia si existe
            UtilitySecciones.AniadirEntidadOrganizacion(mResourceApi, item.GetNameEntityBeanPorIDCampo("010.020.000.020"),
                Variables.SituacionProfesional.cargosActividadesEntidadEmpleadoraNombre,
                Variables.SituacionProfesional.cargosActividadesEntidadEmpleadora, entidadAux);

            //Añado otros, o el ID de una preseleccion
            string valorEE = !string.IsNullOrEmpty(item.GetStringPorIDCampo("010.020.000.050")) ? mResourceApi.GraphsUrl + "items/organizationtype_OTHERS" : item.GetOrganizacionPorIDCampo("010.020.000.040");

            entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
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
        private static void CargosActividadesCorreoElectronico(CvnItemBean item, Entity entidadAux)
        {
            if (string.IsNullOrEmpty(item.GetStringPorIDCampo("010.020.000.160"))) { return; }

            List<string> listado = item.GetStringPorIDCampo("010.020.000.160").Split(";")?.ToList();
            if (listado == null || !listado.Any())
            {
                return;
            }

            foreach (string correo in listado)
            {
                entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
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
        private static void CargosActividadesTelefono(CvnItemBean item, Entity entidadAux)
        {
            List<CvnItemBeanCvnPhoneBean> listado = item.GetListaElementosPorIDCampo<CvnItemBeanCvnPhoneBean>("010.020.000.140");

            string propiedadNumero = Variables.SituacionProfesional.cargosActividadesFijoNumero;
            string propiedadCodInternacional = Variables.SituacionProfesional.cargosActividadesFijoCodInternacional;
            string propiedadExtension = Variables.SituacionProfesional.cargosActividadesFijoExtension;

            UtilitySecciones.InsertarListadoTelefonos(listado, entidadAux, propiedadNumero, propiedadCodInternacional, propiedadExtension);
        }

        /// <summary>
        /// Inserta en <paramref name="entidadAux"/> los valores de <paramref name="item"/>,
        /// pertenecientes al Fax.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="entidadAux"></param>
        private static void CargosActividadesFax(CvnItemBean item, Entity entidadAux)
        {
            List<CvnItemBeanCvnPhoneBean> listado = item.GetListaElementosPorIDCampo<CvnItemBeanCvnPhoneBean>("010.020.000.150");

            string propiedadNumero = Variables.SituacionProfesional.cargosActividadesFaxNumero;
            string propiedadCodInternacional = Variables.SituacionProfesional.cargosActividadesFaxCodInternacional;
            string propiedadExtension = Variables.SituacionProfesional.cargosActividadesFaxExtension;

            UtilitySecciones.InsertarListadoTelefonos(listado, entidadAux, propiedadNumero, propiedadCodInternacional, propiedadExtension);
        }

        /// <summary>
        /// Inserta en <paramref name="entidadAux"/> los valores de <paramref name="item"/>,
        /// pertenecientes a los Códigos UNESCO de especialización primaria, secundaria y terciaria.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="entidadAux"></param>
        private static void CargosActividadesCodUnesco(CvnItemBean item, Entity entidadAux)
        {
            //Añado los códigos UNESCO de especialización primaria
            List<CvnItemBeanCvnString> listadoCodUnescoPrimaria = item.GetListaElementosPorIDCampo<CvnItemBeanCvnString>("010.020.000.230");
            UtilitySecciones.CodigosUnesco(listadoCodUnescoPrimaria, entidadAux, Variables.SituacionProfesional.cargosActividadesCodUnescoPrimaria);

            //Añado los códigos UNESCO de especialización secundaria
            List<CvnItemBeanCvnString> listadoCodUnescoSecundaria = item.GetListaElementosPorIDCampo<CvnItemBeanCvnString>("010.020.000.240");
            UtilitySecciones.CodigosUnesco(listadoCodUnescoSecundaria, entidadAux, Variables.SituacionProfesional.cargosActividadesCodUnescoSecundaria);

            //Añado los códigos UNESCO de especialización terciaria
            List<CvnItemBeanCvnString> listadoCodUnescoTerciaria = item.GetListaElementosPorIDCampo<CvnItemBeanCvnString>("010.020.000.250");
            UtilitySecciones.CodigosUnesco(listadoCodUnescoTerciaria, entidadAux, Variables.SituacionProfesional.cargosActividadesCodUnescoTerciaria);
        }
    }
}
