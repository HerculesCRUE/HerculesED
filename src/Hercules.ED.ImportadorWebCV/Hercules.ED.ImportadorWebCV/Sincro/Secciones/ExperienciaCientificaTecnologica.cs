using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using static Models.Entity;
using Utils;
using Hercules.ED.DisambiguationEngine.Models;
using ImportadorWebCV.Sincro.Secciones.ExperienciaCientificaSubclases;
using System.Runtime.InteropServices;
using Hercules.ED.ImportadorWebCV.Models;

namespace ImportadorWebCV.Sincro.Secciones
{
    class ExperienciaCientificaTecnologica : SeccionBase
    {
        private List<CvnItemBean> listadoDatos = new List<CvnItemBean>();
        private readonly string RdfTypeTab = "http://w3id.org/roh/ScientificExperience";
        public ExperienciaCientificaTecnologica(cvnRootResultBean cvn, string cvID) : base(cvn, cvID)
        {
            listadoDatos = mCvn.GetListadoBloque("050");
        }

        /// <summary>
        /// Metodo para sincronizar los datos pertenecientes al bloque 
        /// "Proyectos de I+D+i financiados en convocatorias competitivas de Administraciones o entidades públicas y privadas".
        /// Con el codigo identificativo 050.020.010.000
        /// </summary>
        public List<SubseccionItem> SincroProyectosIDI([Optional] bool preimportar)
        {
            List<string> propiedadesItem = new List<string>() { "http://w3id.org/roh/scientificExperience", "http://w3id.org/roh/competitiveProjects", "http://vivoweb.org/ontology/core#relatedBy" };
            string graph = "project";
            string propTitle = "http://w3id.org/roh/title";
            string rdfType = "http://vivoweb.org/ontology/core#Project";
            string rdfTypePrefix = "RelatedCompetitiveProyect";

            //1º Obtenemos la entidad del XML.
            List<Entity> listadoAux = GetProyectosIDI(listadoDatos);

            Dictionary<string, DisambiguableEntity> entidadesXML = new Dictionary<string, DisambiguableEntity>();
            foreach (Entity entityXML in listadoAux)//TODO -check
            {
                ProyectosIDI proyectosIDI = new ProyectosIDI();
                proyectosIDI.descripcion = entityXML.properties.FirstOrDefault(x => x.prop == Variables.ExperienciaCientificaTecnologica.proyectosIDINombre)?.values.FirstOrDefault();
                proyectosIDI.fecha = entityXML.properties.FirstOrDefault(x => x.prop == Variables.ExperienciaCientificaTecnologica.proyectosIDIFechaInicio)?.values.FirstOrDefault();
                proyectosIDI.ID = Guid.NewGuid().ToString();
                entidadesXML.Add(proyectosIDI.ID, proyectosIDI);
            }

            //2º Obtenemos las entidades de la BBDD
            Dictionary<string, DisambiguableEntity> entidadesBBDD = ProyectosIDI.GetBBDD(mResourceApi, mCvID, graph, propiedadesItem);

            //3º Comparamos las equivalentes
            Dictionary<string, string> equivalencias = Disambiguation.SimilarityBBDD(entidadesXML.Values.ToList(), entidadesBBDD.Values.ToList());

            //Comparamos si queremos Preimportar o actualizar las entidades
            return CheckPreimportar(preimportar, listadoAux, entidadesXML, equivalencias, propTitle, graph, rdfType, rdfTypePrefix, propiedadesItem, RdfTypeTab);
        }

        /// <summary>
        /// Metodo para sincronizar los datos pertenecientes al bloque 
        /// "Contratos, convenios o proyectos de I+D+i no competitivos con Administraciones o entidades públicas o privadas".
        /// Con el codigo identificativo 050.020.020.000
        /// </summary>
        public List<SubseccionItem> SincroContratos([Optional] bool preimportar)
        {
            List<string> propiedadesItem = new List<string>() { "http://w3id.org/roh/scientificExperience", "http://w3id.org/roh/nonCompetitiveProjects", "http://vivoweb.org/ontology/core#relatedBy" };
            string graph = "project";
            string propTitle = "http://w3id.org/roh/title";
            string rdfType = "http://vivoweb.org/ontology/core#Project";
            string rdfTypePrefix = "RelatedNonCompetitiveProyect";

            //1º Obtenemos la entidad del XML.
            List<Entity> listadoAux = GetContratos(listadoDatos);

            Dictionary<string, DisambiguableEntity> entidadesXML = new Dictionary<string, DisambiguableEntity>();
            foreach (Entity entityXML in listadoAux)//TODO -check
            {
                Contratos contratos = new Contratos();
                contratos.descripcion = entityXML.properties.FirstOrDefault(x => x.prop == Variables.ExperienciaCientificaTecnologica.contratosNombreProyecto)?.values.FirstOrDefault();
                contratos.fecha = entityXML.properties.FirstOrDefault(x => x.prop == Variables.ExperienciaCientificaTecnologica.contratosFechaInicio)?.values.FirstOrDefault();
                contratos.ID = Guid.NewGuid().ToString();
                entidadesXML.Add(contratos.ID, contratos);
            }

            //2º Obtenemos las entidades de la BBDD
            Dictionary<string, DisambiguableEntity> entidadesBBDD = Contratos.GetBBDD(mResourceApi, mCvID, graph, propiedadesItem);

            //3º Comparamos las equivalentes
            Dictionary<string, string> equivalencias = Disambiguation.SimilarityBBDD(entidadesXML.Values.ToList(), entidadesBBDD.Values.ToList());

            //Comparamos si queremos Preimportar o actualizar las entidades
            return CheckPreimportar(preimportar, listadoAux, entidadesXML, equivalencias, propTitle, graph, rdfType, rdfTypePrefix, propiedadesItem, RdfTypeTab);
        }

        /// <summary>
        /// Metodo para sincronizar los datos pertenecientes al bloque 
        /// "Propiedad industrial e intelectual".
        /// Con el codigo identificativo 050.030.010.000
        /// </summary>
        public List<SubseccionItem> SincroPropiedadIndustrialIntelectual([Optional] bool preimportar)
        {
            List<string> propiedadesItem = new List<string>() { "http://w3id.org/roh/scientificExperience", "http://w3id.org/roh/patents", "http://vivoweb.org/ontology/core#relatedBy" };
            string graph = "patent";
            string propTitle = "http://w3id.org/roh/title";
            string rdfType = "http://purl.org/ontology/bibo/Patent";
            string rdfTypePrefix = "RelatedPatent";

            //1º Obtenemos la entidad del XML.
            List<Entity> listadoAux = GetPropiedadIndustrialIntelectual(listadoDatos);

            Dictionary<string, DisambiguableEntity> entidadesXML = new Dictionary<string, DisambiguableEntity>();
            foreach (Entity entityXML in listadoAux)
            {
                PropiedadIndustrialIntelectual propiedadIndustrialIntelectual = new PropiedadIndustrialIntelectual();
                propiedadIndustrialIntelectual.descripcion = entityXML.properties.FirstOrDefault(x => x.prop == Variables.ExperienciaCientificaTecnologica.propIITituloPropIndus)?.values.FirstOrDefault();
                propiedadIndustrialIntelectual.fecha = entityXML.properties.FirstOrDefault(x => x.prop == Variables.ExperienciaCientificaTecnologica.propIIFechaConcesion)?.values.FirstOrDefault();
                propiedadIndustrialIntelectual.entidadTitular = entityXML.properties.FirstOrDefault(x => x.prop == Variables.ExperienciaCientificaTecnologica.propIIEntidadTitularDerechosNombre)?.values.FirstOrDefault();
                propiedadIndustrialIntelectual.ID = Guid.NewGuid().ToString();
                entidadesXML.Add(propiedadIndustrialIntelectual.ID, propiedadIndustrialIntelectual);
            }

            //2º Obtenemos las entidades de la BBDD
            Dictionary<string, DisambiguableEntity> entidadesBBDD = PropiedadIndustrialIntelectual.GetBBDD(mResourceApi, mCvID, graph, propiedadesItem);

            //3º Comparamos las equivalentes
            Dictionary<string, string> equivalencias = Disambiguation.SimilarityBBDD(entidadesXML.Values.ToList(), entidadesBBDD.Values.ToList());

            //Comparamos si queremos Preimportar o actualizar las entidades
            return CheckPreimportar(preimportar, listadoAux, entidadesXML, equivalencias, propTitle, graph, rdfType, rdfTypePrefix, propiedadesItem, RdfTypeTab);
        }

        /// <summary>
        /// Metodo para sincronizar los datos pertenecientes al subapartado 
        /// "Grupos/equipos de investigación, desarrollo o innovación".
        /// Con el codigo identificativo 050.010.000.000
        /// </summary>
        public List<SubseccionItem> SincroGrupoIDI([Optional] bool preimportar)
        {
            List<string> propiedadesItem = new List<string>() { "http://w3id.org/roh/scientificExperience", "http://w3id.org/roh/groups", "http://vivoweb.org/ontology/core#relatedBy" };
            string graph = "group";
            string propTitle = "http://w3id.org/roh/title";
            string rdfType = "http://xmlns.com/foaf/0.1/Group";
            string rdfTypePrefix = "RelatedGroup";

            //1º Obtenemos la entidad del XML.
            List<Entity> listadoAux = GetGrupoIDI(listadoDatos);

            Dictionary<string, DisambiguableEntity> entidadesXML = new Dictionary<string, DisambiguableEntity>();
            foreach (Entity entityXML in listadoAux)//TODO -check
            {
                GrupoIDI grupoIDI = new GrupoIDI();
                grupoIDI.descripcion = entityXML.properties.FirstOrDefault(x => x.prop == Variables.ExperienciaCientificaTecnologica.grupoIDINombreGrupo)?.values.FirstOrDefault();
                grupoIDI.fecha = entityXML.properties.FirstOrDefault(x => x.prop == Variables.ExperienciaCientificaTecnologica.grupoIDIFechaInicio)?.values.FirstOrDefault();
                grupoIDI.ID = Guid.NewGuid().ToString();
                entidadesXML.Add(grupoIDI.ID, grupoIDI);
            }

            //2º Obtenemos las entidades de la BBDD
            Dictionary<string, DisambiguableEntity> entidadesBBDD = GrupoIDI.GetBBDD(mResourceApi, mCvID, graph, propiedadesItem);

            //3º Comparamos las equivalentes
            Dictionary<string, string> equivalencias = Disambiguation.SimilarityBBDD(entidadesXML.Values.ToList(), entidadesBBDD.Values.ToList());

            //Comparamos si queremos Preimportar o actualizar las entidades
            return CheckPreimportar(preimportar, listadoAux, entidadesXML, equivalencias, propTitle, graph, rdfType, rdfTypePrefix, propiedadesItem, RdfTypeTab);
        }

        /// <summary>
        /// Metodo para sincronizar los datos pertenecientes al bloque 
        /// "Obras artísticas dirigidas".
        /// Con el codigo identificativo 050.020.030.000
        /// </summary>
        public List<SubseccionItem> SincroObrasArtisticas([Optional] bool preimportar)
        {
            List<string> propiedadesItem = new List<string>() { "http://w3id.org/roh/scientificExperience", "http://w3id.org/roh/supervisedArtisticProjects", "http://vivoweb.org/ontology/core#relatedBy" };
            string graph = "supervisedartisticproject";
            string propTitle = "http://w3id.org/roh/title";
            string rdfType = "http://w3id.org/roh/SupervisedArtisticProject";
            string rdfTypePrefix = "RelatedSupervisedArtisticProject";

            //1º Obtenemos la entidad del XML.
            List<Entity> listadoAux = GetObrasArtisticas(listadoDatos);
            Dictionary<string, DisambiguableEntity> entidadesXML = new Dictionary<string, DisambiguableEntity>();
            foreach (Entity entityXML in listadoAux)
            {
                ObrasArtisticas obrasArtisticas = new ObrasArtisticas();
                obrasArtisticas.descripcion = entityXML.properties.FirstOrDefault(x => x.prop == Variables.ExperienciaCientificaTecnologica.obrasArtisticasDescripcion)?.values.FirstOrDefault();
                obrasArtisticas.nombre = entityXML.properties.FirstOrDefault(x => x.prop == Variables.ExperienciaCientificaTecnologica.obrasArtisticasNombreExpo)?.values.FirstOrDefault();
                obrasArtisticas.fecha = entityXML.properties.FirstOrDefault(x => x.prop == Variables.ExperienciaCientificaTecnologica.obrasArtisticasFechaInicio)?.values.FirstOrDefault();
                obrasArtisticas.ID = Guid.NewGuid().ToString();
                entidadesXML.Add(obrasArtisticas.ID, obrasArtisticas);
            }

            //2º Obtenemos las entidades de la BBDD
            Dictionary<string, DisambiguableEntity> entidadesBBDD = ObrasArtisticas.GetBBDD(mResourceApi, mCvID, graph, propiedadesItem);

            //3º Comparamos las equivalentes
            Dictionary<string, string> equivalencias = Disambiguation.SimilarityBBDD(entidadesXML.Values.ToList(), entidadesBBDD.Values.ToList());

            //Comparamos si queremos Preimportar o actualizar las entidades
            return CheckPreimportar(preimportar, listadoAux, entidadesXML, equivalencias, propTitle, graph, rdfType, rdfTypePrefix, propiedadesItem, RdfTypeTab);
        }

        /// <summary>
        /// Metodo para sincronizar los datos pertenecientes al bloque 
        /// "Resultados tecnológicos derivados de actividades especializadas y de transferencia no incluidos en apartados anteriores".
        /// Con el codigo identificativo 050.030.020.000
        /// </summary>
        public List<SubseccionItem> SincroResultadosTecnologicos([Optional] bool preimportar)
        {
            List<string> propiedadesItem = new List<string>() { "http://w3id.org/roh/scientificExperience", "http://w3id.org/roh/technologicalResults", "http://vivoweb.org/ontology/core#relatedBy" };
            string graph = "technologicalresult";
            string propTitle = "http://vivoweb.org/ontology/core#description";
            string rdfType = "http://w3id.org/roh/TechnologicalResult";
            string rdfTypePrefix = "RelatedTechnologicalResult";

            //1º Obtenemos la entidad del XML.
            List<Entity> listadoAux = GetResultadosTecnologicos(listadoDatos);
            Dictionary<string, DisambiguableEntity> entidadesXML = new Dictionary<string, DisambiguableEntity>();
            foreach (Entity entityXML in listadoAux)//TODO -check
            {
                ResultadosTecnologicos resultadosTecnologicos = new ResultadosTecnologicos();
                resultadosTecnologicos.descripcion = entityXML.properties.FirstOrDefault(x => x.prop == Variables.ExperienciaCientificaTecnologica.resultadosTecnologicosDescripcion)?.values.FirstOrDefault();
                resultadosTecnologicos.fecha = entityXML.properties.FirstOrDefault(x => x.prop == Variables.ExperienciaCientificaTecnologica.resultadosTecnologicosFechaInicio)?.values.FirstOrDefault();
                resultadosTecnologicos.ID = Guid.NewGuid().ToString();
                entidadesXML.Add(resultadosTecnologicos.ID, resultadosTecnologicos);
            }

            //2º Obtenemos las entidades de la BBDD
            Dictionary<string, DisambiguableEntity> entidadesBBDD = ResultadosTecnologicos.GetBBDD(mResourceApi, mCvID, graph, propiedadesItem);

            //3º Comparamos las equivalentes
            Dictionary<string, string> equivalencias = Disambiguation.SimilarityBBDD(entidadesXML.Values.ToList(), entidadesBBDD.Values.ToList());

            //Comparamos si queremos Preimportar o actualizar las entidades
            return CheckPreimportar(preimportar, listadoAux, entidadesXML, equivalencias, propTitle, graph, rdfType, rdfTypePrefix, propiedadesItem, RdfTypeTab);
        }

        /// <summary>
        /// 050.020.010.000
        /// </summary>
        /// <param name="listadoDatos"></param>
        /// <returns></returns>
        private List<Entity> GetProyectosIDI(List<CvnItemBean> listadoDatos)
        {
            List<Entity> listado = new List<Entity>();

            List<CvnItemBean> listadoProyectosIDI = listadoDatos.Where(x => x.Code.Equals("050.020.010.000")).ToList();
            if (listadoProyectosIDI.Count > 0)
            {
                foreach (CvnItemBean item in listadoProyectosIDI)
                {
                    Entity entidadAux = new Entity();
                    entidadAux.properties = new List<Property>();
                    if (!string.IsNullOrEmpty(item.GetStringPorIDCampo("050.020.010.010")))
                    {
                        entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                            new Property(Variables.ExperienciaCientificaTecnologica.proyectosIDINombre, item.GetStringPorIDCampo("050.020.010.010")),
                            new Property(Variables.ExperienciaCientificaTecnologica.proyectosIDIModalidadProyecto, item.GetModalidadProyectoPorIDCampo("050.020.010.030")),//TODO-check
                            new Property(Variables.ExperienciaCientificaTecnologica.proyectosIDIAmbitoProyecto, item.GetGeographicRegionPorIDCampo("050.020.010.040")),//TODO-check
                            new Property(Variables.ExperienciaCientificaTecnologica.proyectosIDIAmbitoProyectoOtros, item.GetStringPorIDCampo("050.020.010.050")),
                            new Property(Variables.ExperienciaCientificaTecnologica.proyectosIDINumInvestigadores, item.GetStringDoublePorIDCampo("050.020.010.150")),
                            new Property(Variables.ExperienciaCientificaTecnologica.proyectosIDINumPersonasAnio, item.GetStringDoublePorIDCampo("050.020.010.160")),
                            new Property(Variables.ExperienciaCientificaTecnologica.proyectosIDIGradoContribucion, item.GetGradoContribucionPorIDCampo("050.020.010.170")),//TODO-check
                            new Property(Variables.ExperienciaCientificaTecnologica.proyectosIDIGradoContribucionOtros, item.GetStringPorIDCampo("050.020.010.180")),
                            new Property(Variables.ExperienciaCientificaTecnologica.proyectosIDITipoParticipacion, item.GetTipoParticipacionActividadPorIDCampo("050.020.010.230")),//TODO- check 
                            new Property(Variables.ExperienciaCientificaTecnologica.proyectosIDITipoParticipacionOtros, item.GetStringPorIDCampo("050.020.010.240")),//TODO - revisar 
                            new Property(Variables.ExperienciaCientificaTecnologica.proyectosIDINombreProgramaFinanciacion, item.GetStringPorIDCampo("050.020.010.250")),
                            new Property(Variables.ExperienciaCientificaTecnologica.proyectosIDICodEntidadFinanciacion, item.GetValueCvnExternalPKBean("050.020.010.260")),//TODO - check
                            new Property(Variables.ExperienciaCientificaTecnologica.proyectosIDIFechaInicio, item.GetStringDatetimePorIDCampo("050.020.010.270")),
                            new Property(Variables.ExperienciaCientificaTecnologica.proyectosIDIDuracionAnio, item.GetDurationAnioPorIDCampo("050.020.010.280")),
                            new Property(Variables.ExperienciaCientificaTecnologica.proyectosIDIDuracionMes, item.GetDurationMesPorIDCampo("050.020.010.280")),
                            new Property(Variables.ExperienciaCientificaTecnologica.proyectosIDIDuracionDia, item.GetDurationDiaPorIDCampo("050.020.010.280")),
                            new Property(Variables.ExperienciaCientificaTecnologica.proyectosIDICuantiaTotal, item.GetStringDoublePorIDCampo("050.020.010.290")),
                            new Property(Variables.ExperienciaCientificaTecnologica.proyectosIDIResultadosRelevantes, item.GetStringPorIDCampo("050.020.010.340")),
                            new Property(Variables.ExperienciaCientificaTecnologica.proyectosIDIFechaFinalizacion, item.GetStringDatetimePorIDCampo("050.020.010.410")),
                        new Property(Variables.ExperienciaCientificaTecnologica.proyectosIDIAportacionSolicitante, item.GetStringPorIDCampo("050.020.010.420")),
                        new Property(Variables.ExperienciaCientificaTecnologica.proyectosIDIRegimenDedicacion, item.GetStringPorIDCampo("050.020.010.430"))//TODO
                        ));
                        ProyectosIDIEntidadRealizacion(item, entidadAux);
                        ProyectosIDIResultadosRelevantesPalabrasClave(item, entidadAux);
                        ProyectosIDIPalabrasClave(item, entidadAux);
                        ProyectosIDIFinanciacion(item, entidadAux);
                        ProyectosIDIEntidadesParticipantes(item, entidadAux);
                        ProyectosIDIAutores(item, entidadAux);
                        ProyectosIDIEntidadFinanciadora(item, entidadAux);

                        listado.Add(entidadAux);
                    }
                }
            }
            return listado;
        }

        /// <summary>
        /// Inserta en <paramref name="entidadAux"/> los valores de <paramref name="item"/>,
        /// pertenecientes a las palabaras clave.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="entidadAux"></param>
        private void ProyectosIDIPalabrasClave(CvnItemBean item, Entity entidadAux)
        {
            List<CvnItemBeanCvnString> listadoPalabrasClave = item.GetListaElementosPorIDCampo<CvnItemBeanCvnString>("050.020.010.020");

            string propiedadPalabrasClave = Variables.ExperienciaCientificaTecnologica.proyectosIDIPalabrasClave;

            foreach (CvnItemBeanCvnString palabraClave in listadoPalabrasClave)
            {
                string entityPartAux = Guid.NewGuid().ToString() + "@@@";

                List<string> listadoPalabras = Utility.GetPadresPalabrasClave(palabraClave);
                foreach (string palabra in listadoPalabras)
                {
                    Property propertyPalabrasClave = entidadAux.properties.FirstOrDefault(x => x.prop == Variables.ExperienciaCientificaTecnologica.proyectosIDIPalabrasClave);
                    UtilitySecciones.CheckProperty(propertyPalabrasClave, entidadAux,
                        UtilitySecciones.StringGNOSSID(entityPartAux, Utility.ObtenerPalabraClave(mResourceApi, palabra)), propiedadPalabrasClave);
                }
            }
        }

        /// <summary>
        /// Inserta en <paramref name="entidadAux"/> los valores de <paramref name="item"/>,
        /// pertenecientes a las palabaras clave pertenecientes a los resultados relevantes.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="entidadAux"></param>
        private void ProyectosIDIResultadosRelevantesPalabrasClave(CvnItemBean item, Entity entidadAux)
        {
            List<CvnItemBeanCvnString> listadoPalabrasClave = item.GetListaElementosPorIDCampo<CvnItemBeanCvnString>("050.020.010.350");

            string propiedadPalabrasClave = Variables.ExperienciaCientificaTecnologica.proyectosIDIResultadosRelevantesPalabrasClave;

            foreach (CvnItemBeanCvnString palabraClave in listadoPalabrasClave)
            {
                string entityPartAux = Guid.NewGuid().ToString() + "@@@";

                List<string> listadoPalabras = Utility.GetPadresPalabrasClave(palabraClave);
                foreach (string palabra in listadoPalabras)
                {
                    Property propertyPalabrasClave = entidadAux.properties.FirstOrDefault(x => x.prop == Variables.ExperienciaCientificaTecnologica.proyectosIDIResultadosRelevantesPalabrasClave);
                    UtilitySecciones.CheckProperty(propertyPalabrasClave, entidadAux,
                        UtilitySecciones.StringGNOSSID(entityPartAux, Utility.ObtenerPalabraClave(mResourceApi, palabra)), propiedadPalabrasClave);
                }
            }
        }

        /// <summary>
        /// Inserta en <paramref name="entidadAux"/> los valores de <paramref name="item"/>,
        /// pertenecientes a la Financiación del proyecto (cuantía del subproyecto,
        /// porcentaje en subvencion, porcentaje en crédito y porcentaje mixto).
        /// </summary>
        /// <param name="item">item</param>
        /// <param name="entidadAux">entidadAux</param>
        private void ProyectosIDIFinanciacion(CvnItemBean item, Entity entidadAux)
        {
            /*
            new Property(Variables.ExperienciaCientificaTecnologica.proyectosIDICuantiaSubproyecto, item.GetStringDoublePorIDCampo("050.020.010.300")),
            new Property(Variables.ExperienciaCientificaTecnologica.proyectosIDIPorcentajeSubvencion, item.GetStringDoublePorIDCampo("050.020.010.310")),
            new Property(Variables.ExperienciaCientificaTecnologica.proyectosIDIPorcentajeCredito, item.GetStringDoublePorIDCampo("050.020.010.320")),
            new Property(Variables.ExperienciaCientificaTecnologica.proyectosIDIPorcentajeMixto, item.GetStringDoublePorIDCampo("050.020.010.330")),
             */

            string entityPartAux = Guid.NewGuid().ToString() + "@@@";
            entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                new Property(Variables.ExperienciaCientificaTecnologica.proyectosIDICuantiaSubproyecto, UtilitySecciones.StringGNOSSID(entityPartAux, item.GetStringDoublePorIDCampo("050.020.010.300"))),
                new Property(Variables.ExperienciaCientificaTecnologica.proyectosIDIPorcentajeSubvencion, UtilitySecciones.StringGNOSSID(entityPartAux, item.GetStringDoublePorIDCampo("050.020.010.310"))),
                new Property(Variables.ExperienciaCientificaTecnologica.proyectosIDIPorcentajeCredito, UtilitySecciones.StringGNOSSID(entityPartAux, item.GetStringDoublePorIDCampo("050.020.010.320"))),
                new Property(Variables.ExperienciaCientificaTecnologica.proyectosIDIPorcentajeMixto, UtilitySecciones.StringGNOSSID(entityPartAux, item.GetStringDoublePorIDCampo("050.020.010.330")))
            ));
        }

        /// <summary>
        /// Inserta en <paramref name="entidadAux"/> los valores de <paramref name="item"/>,
        /// pertenecientes a la Entidad de Realización
        /// </summary>
        /// <param name="item">item</param>
        /// <param name="entidadAux">entidadAux</param>
        private void ProyectosIDIEntidadRealizacion(CvnItemBean item, Entity entidadAux)
        {
            //Añado la referencia si existe Entidad
            UtilitySecciones.AniadirEntidad(mResourceApi, item.GetNameEntityBeanPorIDCampo("050.020.010.100"),
                Variables.ExperienciaCientificaTecnologica.proyectosIDIEntidadRealizacionNombre,
                Variables.ExperienciaCientificaTecnologica.proyectosIDIEntidadRealizacion, entidadAux);

            //new Property(Variables.ExperienciaCientificaTecnologica.proyectosIDIEntidadRealizacion, item.GetNameEntityBeanPorIDCampo("050.020.010.100"))

            //new Property(Variables.ExperienciaCientificaTecnologica.proyectosIDIPaisEntidadRealizacion, item.GetPaisPorIDCampo("050.020.010.060")),
            //new Property(Variables.ExperienciaCientificaTecnologica.proyectosIDICCAAEntidadRealizacion, item.GetRegionPorIDCampo("050.020.010.070")),
            //new Property(Variables.ExperienciaCientificaTecnologica.proyectosIDICiudadEntidadRealizacion, item.GetStringPorIDCampo("050.020.010.090")),
            //new Property(Variables.ExperienciaCientificaTecnologica.proyectosIDITipoEntidadRealizacion, item.GetStringPorIDCampo("050.020.010.120")),
            //new Property(Variables.ExperienciaCientificaTecnologica.proyectosIDITipoEntidadRealizacionOtros, item.GetStringPorIDCampo("050.020.010.130")),

        }

        /// <summary>
        /// Inserta en <paramref name="entidadAux"/> los valores de <paramref name="item"/>,
        /// pertenecientes a las Entidades Participantes.
        /// </summary>
        /// <param name="item">item</param>
        /// <param name="entidadAux">entidadAux</param>
        private void ProyectosIDIEntidadesParticipantes(CvnItemBean item, Entity entidadAux)
        {

            //Añado el listado de entidades participantes
            List<CvnItemBeanCvnEntityBean> listadoEntidadesParticipantes = item.GetListaElementosPorIDCampo<CvnItemBeanCvnEntityBean>("050.020.010.400");
            foreach (CvnItemBeanCvnEntityBean entidad in listadoEntidadesParticipantes)
            {
                //Añado la referencia si existe Entidad
                UtilitySecciones.AniadirEntidad(mResourceApi, entidad.Name,
                    Variables.ExperienciaCientificaTecnologica.proyectosIDIEntidadParticipanteNombre,
                    Variables.ExperienciaCientificaTecnologica.proyectosIDIEntidadParticipante, entidadAux);
            }
        }

        /// <summary>
        /// Inserta en <paramref name="entidadAux"/> los valores de <paramref name="item"/>,
        /// pertenecientes a los Autores/as.
        /// </summary>
        /// <param name="item">item</param>
        /// <param name="entidadAux">entidadAux</param>
        private void ProyectosIDIAutores(CvnItemBean item, Entity entidadAux)
        {
            //Añado el listado de autores principales
            List<CvnItemBeanCvnAuthorBean> listadoAutores = item.GetListaElementosPorIDCampo<CvnItemBeanCvnAuthorBean>("050.020.010.140");
            foreach (CvnItemBeanCvnAuthorBean autor in listadoAutores)
            {
                //Si no tiene nombre no lo inserto
                if (string.IsNullOrEmpty(autor.GetNombreAutor())) { continue; }

                Property propertyNombre = entidadAux.properties.FirstOrDefault(x => x.prop == Variables.ExperienciaCientificaTecnologica.proyectosIDINombreIP);
                Property propertyPrimerApellido = entidadAux.properties.FirstOrDefault(x => x.prop == Variables.ExperienciaCientificaTecnologica.proyectosIDIPrimerApellidoIP);
                Property propertySegundoApellido = entidadAux.properties.FirstOrDefault(x => x.prop == Variables.ExperienciaCientificaTecnologica.proyectosIDISegundoApellidoIP);
                Property propertyFirma = entidadAux.properties.FirstOrDefault(x => x.prop == Variables.ExperienciaCientificaTecnologica.proyectosIDIFirmaIP);

                string valorNombre = autor.GetNombreAutor();
                string valorPrimerApellido = autor.GetPrimerApellidoAutor();
                string valorSegundoApellido = autor.GetSegundoApellidoAutor();
                string valorFirma = autor.GetFirmaAutor();

                string propiedadNombre = Variables.ExperienciaCientificaTecnologica.proyectosIDINombreIP;
                string propiedadPrimerApellido = Variables.ExperienciaCientificaTecnologica.proyectosIDIPrimerApellidoIP;
                string propiedadSegundoApellido = Variables.ExperienciaCientificaTecnologica.proyectosIDISegundoApellidoIP;
                string propiedadFirma = Variables.ExperienciaCientificaTecnologica.proyectosIDIFirmaIP;

                UtilitySecciones.CheckProperty(propertyNombre, entidadAux, valorNombre, propiedadNombre);
                UtilitySecciones.CheckProperty(propertyPrimerApellido, entidadAux, valorPrimerApellido, propiedadPrimerApellido);
                UtilitySecciones.CheckProperty(propertySegundoApellido, entidadAux, valorSegundoApellido, propiedadSegundoApellido);
                UtilitySecciones.CheckProperty(propertyFirma, entidadAux, valorFirma, propiedadFirma);
            }
        }

        /// <summary>
        /// Inserta en <paramref name="entidadAux"/> los valores de <paramref name="item"/>,
        /// pertenecientes a la Entidad Financiadora.
        /// </summary>
        /// <param name="item">item</param>
        /// <param name="entidadAux">entidadAux</param>
        private void ProyectosIDIEntidadFinanciadora(CvnItemBean item, Entity entidadAux)//TODO 
        {
            List<CvnItemBeanCvnCodeGroup> listadoEntidadFinanciadora = item.GetListaElementosPorIDCampo<CvnItemBeanCvnCodeGroup>("050.020.010.190");
            foreach (CvnItemBeanCvnCodeGroup entidadFinanciadora in listadoEntidadFinanciadora)
            {
                ////Añado la referencia si existe Entidad    //TODO - sustituir por el metodo actual
                //UtilitySecciones.AniadirEntidad(mResourceApi, item.GetNameEntityBeanPorIDCampo("050.020.010.100"),
                //    Variables.ExperienciaCientificaTecnologica.proyectosIDIEntidadRealizacionNombre,
                //    Variables.ExperienciaCientificaTecnologica.proyectosIDIEntidadRealizacion, entidadAux);

                //Añado pais, CCAA y ciudad
                entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                    new Property(Variables.ExperienciaCientificaTecnologica.proyectosIDIPaisEntidadFinancidora, entidadFinanciadora.GetPaisPorIDCampo("050.020.010.360")),
                    new Property(Variables.ExperienciaCientificaTecnologica.proyectosIDICCAAEntidadFinancidora, entidadFinanciadora.GetRegionPorIDCampo("050.020.010.370")),
                    new Property(Variables.ExperienciaCientificaTecnologica.proyectosIDICiudadEntidadFinancidora, entidadFinanciadora.GetStringCvnCodeGroup("050.020.010.390"))
                ));
                //Añado los datos asociados a la entidad
                if (!string.IsNullOrEmpty(entidadFinanciadora.GetNameEntityBeanCvnCodeGroup("060.010.010.190")))
                {
                    entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                        new Property(Variables.ExperienciaCientificaTecnologica.proyectosIDIEntidadFinancidora, entidadFinanciadora.GetNameEntityBeanCvnCodeGroup("050.020.010.190"))
                    ));

                    //Añado otros, o el ID de una preseleccion
                    string valorTipo = !string.IsNullOrEmpty(entidadFinanciadora.GetStringCvnCodeGroup("050.020.010.220")) ?
                        "http://gnoss.com/items/organizationtype_OTHERS" : entidadFinanciadora.GetOrganizationCvnCodeGroup("050.020.010.210");

                    entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                        new Property(Variables.ExperienciaCientificaTecnologica.proyectosIDITipoEntidadFinancidora, valorTipo),
                        new Property(Variables.ExperienciaCientificaTecnologica.proyectosIDITipoEntidadFinancidoraOtros, entidadFinanciadora.GetStringCvnCodeGroup("050.020.010.220"))
                    ));
                }
            }
        }

        /// <summary>
        /// 050.020.020.000
        /// </summary>
        /// <param name="listadoDatos"></param>
        /// <returns></returns>
        private List<Entity> GetContratos(List<CvnItemBean> listadoDatos)
        {
            List<Entity> listado = new List<Entity>();

            List<CvnItemBean> listadoContratos = listadoDatos.Where(x => x.Code.Equals("050.020.020.000")).ToList();
            if (listadoContratos.Count > 0)
            {
                foreach (CvnItemBean item in listadoContratos)
                {
                    Entity entidadAux = new Entity();
                    entidadAux.properties = new List<Property>();
                    if (!string.IsNullOrEmpty(item.GetStringPorIDCampo("050.020.020.010")))
                    {
                        entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                            new Property(Variables.ExperienciaCientificaTecnologica.contratosNombreProyecto, item.GetStringPorIDCampo("050.020.020.010")),
                            new Property(Variables.ExperienciaCientificaTecnologica.contratosModalidadProyecto, item.GetModalidadProyectoPorIDCampo("050.020.020.030")),//TODO-check
                            new Property(Variables.ExperienciaCientificaTecnologica.contratosAmbitoProyecto, item.GetGeographicRegionPorIDCampo("050.020.020.040")),//TODO-check
                            new Property(Variables.ExperienciaCientificaTecnologica.contratosAmbitoProyectoOtros, item.GetStringPorIDCampo("050.020.020.050")),
                            new Property(Variables.ExperienciaCientificaTecnologica.contratosPaisEntidadRealizacion, item.GetPaisPorIDCampo("050.020.020.060")),
                            new Property(Variables.ExperienciaCientificaTecnologica.contratosCCAAEntidadRealizacion, item.GetRegionPorIDCampo("050.020.020.070")),
                            new Property(Variables.ExperienciaCientificaTecnologica.contratosCiudadEntidadRealizacion, item.GetStringPorIDCampo("050.020.020.090")),
                            new Property(Variables.ExperienciaCientificaTecnologica.contratosEntidadRealizacion, item.GetStringPorIDCampo("050.020.020.370")),
                            new Property(Variables.ExperienciaCientificaTecnologica.contratosCodEntidadFinanciadora, item.GetNameEntityBeanPorIDCampo("050.020.020.110")),
                            new Property(Variables.ExperienciaCientificaTecnologica.contratosTipoProyecto, item.GetTipoProyectoPorIDCampo("050.020.020.160")),//TODO-check
                            new Property(Variables.ExperienciaCientificaTecnologica.contratosNombrePrograma, item.GetStringPorIDCampo("050.020.020.170")),
                            new Property(Variables.ExperienciaCientificaTecnologica.contratosFechaInicio, item.GetStringDatetimePorIDCampo("050.020.020.180")),
                            new Property(Variables.ExperienciaCientificaTecnologica.contratosDuracionAnio, item.GetDurationAnioPorIDCampo("050.020.020.190")),
                            new Property(Variables.ExperienciaCientificaTecnologica.contratosDuracionMes, item.GetDurationMesPorIDCampo("050.020.020.190")),
                            new Property(Variables.ExperienciaCientificaTecnologica.contratosDuracionDia, item.GetDurationDiaPorIDCampo("050.020.020.190")),
                            new Property(Variables.ExperienciaCientificaTecnologica.contratosCuantiaTotal, item.GetStringDoublePorIDCampo("050.020.020.200")),
                            new Property(Variables.ExperienciaCientificaTecnologica.contratosNumInvestigadores, item.GetStringDoublePorIDCampo("050.020.020.260")),
                            new Property(Variables.ExperienciaCientificaTecnologica.contratosNumPersonasAnio, item.GetStringDoublePorIDCampo("050.020.020.270")),
                            new Property(Variables.ExperienciaCientificaTecnologica.contratosGradoContribucion, item.GetGradoContribucionPorIDCampo("050.020.020.280")),//TODO -check
                            new Property(Variables.ExperienciaCientificaTecnologica.contratosGradoContribucionOtros, item.GetStringPorIDCampo("050.020.020.290")),
                            new Property(Variables.ExperienciaCientificaTecnologica.contratosResultadosRelevantes, item.GetStringPorIDCampo("050.020.020.300"))
                        ));
                        ContratosFinanciacion(item, entidadAux);
                        ContratosEntidadRealizacion(item, entidadAux);
                        //ContratosTipoEntidadRealizacion(item, entidadAux);
                        ContratosEntidadesParticipantes(item, entidadAux);
                        ContratosAutores(item, entidadAux);
                        ContratosEntidadFinanciadora(item, entidadAux);
                        ContratosResultadosRelevantesPalabrasClave(item, entidadAux);
                        ContratosPalabrasClave(item, entidadAux);

                        listado.Add(entidadAux);
                    }
                }
            }
            return listado;
        }

        /// <summary>
        /// Inserta en <paramref name="entidadAux"/> los valores de <paramref name="item"/>,
        /// pertenecientes a las palabaras clave.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="entidadAux"></param>
        private void ContratosPalabrasClave(CvnItemBean item, Entity entidadAux)
        {
            List<CvnItemBeanCvnString> listadoPalabrasClave = item.GetListaElementosPorIDCampo<CvnItemBeanCvnString>("050.020.020.020");

            string propiedadPalabrasClave = Variables.ExperienciaCientificaTecnologica.contratosPalabrasClave;

            foreach (CvnItemBeanCvnString palabraClave in listadoPalabrasClave)
            {
                string entityPartAux = Guid.NewGuid().ToString() + "@@@";

                List<string> listadoPalabras = Utility.GetPadresPalabrasClave(palabraClave);
                foreach (string palabra in listadoPalabras)
                {
                    Property propertyPalabrasClave = entidadAux.properties.FirstOrDefault(x => x.prop == Variables.ExperienciaCientificaTecnologica.contratosPalabrasClave);
                    UtilitySecciones.CheckProperty(propertyPalabrasClave, entidadAux,
                        UtilitySecciones.StringGNOSSID(entityPartAux, Utility.ObtenerPalabraClave(mResourceApi, palabra)), propiedadPalabrasClave);
                }
            }
        }

        /// <summary>
        /// Inserta en <paramref name="entidadAux"/> los valores de <paramref name="item"/>,
        /// pertenecientes a las palabaras clave pertenecientes a los resultados relevantes.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="entidadAux"></param>
        private void ContratosResultadosRelevantesPalabrasClave(CvnItemBean item, Entity entidadAux)
        {
            List<CvnItemBeanCvnString> listadoPalabrasClave = item.GetListaElementosPorIDCampo<CvnItemBeanCvnString>("050.020.020.310");

            string propiedadPalabrasClave = Variables.ExperienciaCientificaTecnologica.contratosResultadosRelevantesPalabrasClave;

            foreach (CvnItemBeanCvnString palabraClave in listadoPalabrasClave)
            {
                string entityPartAux = Guid.NewGuid().ToString() + "@@@";

                List<string> listadoPalabras = Utility.GetPadresPalabrasClave(palabraClave);
                foreach (string palabra in listadoPalabras)
                {
                    Property propertyPalabrasClave = entidadAux.properties.FirstOrDefault(x => x.prop == Variables.ExperienciaCientificaTecnologica.contratosResultadosRelevantesPalabrasClave);
                    UtilitySecciones.CheckProperty(propertyPalabrasClave, entidadAux,
                        UtilitySecciones.StringGNOSSID(entityPartAux, Utility.ObtenerPalabraClave(mResourceApi, palabra)), propiedadPalabrasClave);
                }
            }
        }

        /// <summary>
        /// Inserta en <paramref name="entidadAux"/> los valores de <paramref name="item"/>,
        /// pertenecientes a la Financiación del proyecto (cuantía del subproyecto,
        /// porcentaje en subvencion, porcentaje en crédito y porcentaje mixto).
        /// </summary>
        /// <param name="item">item</param>
        /// <param name="entidadAux">entidadAux</param>
        private void ContratosFinanciacion(CvnItemBean item, Entity entidadAux)
        {
            string entityPartAux = Guid.NewGuid().ToString() + "@@@";
            entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                new Property(Variables.ExperienciaCientificaTecnologica.contratosCuantiaSubproyecto, UtilitySecciones.StringGNOSSID(entityPartAux, item.GetStringDoublePorIDCampo("050.020.020.210"))),
                new Property(Variables.ExperienciaCientificaTecnologica.contratosPorcentajeSubvencion, UtilitySecciones.StringGNOSSID(entityPartAux, item.GetStringDoublePorIDCampo("050.020.020.220"))),
                new Property(Variables.ExperienciaCientificaTecnologica.contratosPorcentajeCredito, UtilitySecciones.StringGNOSSID(entityPartAux, item.GetStringDoublePorIDCampo("050.020.020.230"))),
                new Property(Variables.ExperienciaCientificaTecnologica.contratosPorcentajeMixto, UtilitySecciones.StringGNOSSID(entityPartAux, item.GetStringDoublePorIDCampo("050.020.020.240")))
            ));
        }

        /// <summary>
        /// Inserta en <paramref name="entidadAux"/> los valores de <paramref name="item"/>,
        /// pertenecientes a la Entidad de Realización.
        /// </summary>
        /// <param name="item">item</param>
        /// <param name="entidadAux">entidadAux</param>
        private void ContratosEntidadRealizacion(CvnItemBean item, Entity entidadAux)//TODO
        {
            //Añado la referencia si existe Entidad
            UtilitySecciones.AniadirEntidad(mResourceApi, item.GetNameEntityBeanPorIDCampo("050.020.020.370"),
                Variables.ExperienciaCientificaTecnologica.proyectosIDIEntidadRealizacionNombre,
                Variables.ExperienciaCientificaTecnologica.proyectosIDIEntidadRealizacion, entidadAux);

            //if (!string.IsNullOrEmpty(item.GetNameEntityBeanPorIDCampo("050.020.020.370")))
            //{
            //    string organizacion = UtilitySecciones.GetOrganizacionPorNombre(mResourceApi, item.GetNameEntityBeanPorIDCampo("050.020.020.370"));
            //    entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
            //        new Property(Variables.ExperienciaCientificaTecnologica.contratosEntidadRealizacion, organizacion)
            //    ));
            //}
        }

        //private void ContratosTipoEntidadRealizacion(CvnItemBean item, Entity entidadAux)
        //{

        //    //Añado otros, o el ID de una preseleccion
        //    if (!string.IsNullOrEmpty(item.GetStringPorIDCampo("050.020.020.380")))
        //    {
        //        entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
        //            new Property(Variables.ExperienciaCientificaTecnologica.contratosTipoEntidadRealizacion, "http://gnoss.com/items/organizationtype_OTHERS"),//TODO revisar
        //            new Property(Variables.ExperienciaCientificaTecnologica.contratosTipoEntidadRealizacionOtros, item.GetStringPorIDCampo("050.020.020.380"))
        //        ));
        //    }
        //    else
        //    {
        //        entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
        //            new Property(Variables.ExperienciaCientificaTecnologica.contratosTipoEntidadRealizacion, item.GetStringPorIDCampo("050.020.020.330"))
        //        ));
        //    }
        //}

        /// <summary>
        /// Inserta en <paramref name="entidadAux"/> los valores de <paramref name="item"/>,
        /// pertenecientes a las Entidades Participantes.
        /// </summary>
        /// <param name="item">item</param>
        /// <param name="entidadAux">entidadAux</param>
        private void ContratosEntidadesParticipantes(CvnItemBean item, Entity entidadAux)//TODO
        {
            //Añado el listado de entidades participantes
            List<CvnItemBeanCvnEntityBean> listadoEntidades = item.GetListaElementosPorIDCampo<CvnItemBeanCvnEntityBean>("050.020.020.320");
            foreach (CvnItemBeanCvnEntityBean entidad in listadoEntidades)
            {
                //Añado la referencia si existe Entidad
                UtilitySecciones.AniadirEntidad(mResourceApi, entidad.Name,
                    Variables.ExperienciaCientificaTecnologica.proyectosIDIEntidadRealizacionNombre,
                    Variables.ExperienciaCientificaTecnologica.proyectosIDIEntidadRealizacion, entidadAux);

                //entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                //    new Property(Variables.ExperienciaCientificaTecnologica.contratosEntidadParticipante, entidad.Name)
                //));
            }
        }

        /// <summary>
        /// Inserta en <paramref name="entidadAux"/> los valores de <paramref name="item"/>,
        /// pertenecientes a los Autores/as.
        /// </summary>
        /// <param name="item">item</param>
        /// <param name="entidadAux">entidadAux</param>
        private void ContratosAutores(CvnItemBean item, Entity entidadAux)
        {
            //Añado el listado de autores principales
            List<CvnItemBeanCvnAuthorBean> listadoAutores = item.GetListaElementosPorIDCampo<CvnItemBeanCvnAuthorBean>("050.020.020.250");
            foreach (CvnItemBeanCvnAuthorBean autor in listadoAutores)
            {
                //Si no tiene nombre no lo inserto
                if (string.IsNullOrEmpty(autor.GetNombreAutor())) { continue; }

                Property propertyNombre = entidadAux.properties.FirstOrDefault(x => x.prop == Variables.ExperienciaCientificaTecnologica.contratosIPNombre);
                Property propertyPrimerApellido = entidadAux.properties.FirstOrDefault(x => x.prop == Variables.ExperienciaCientificaTecnologica.contratosIPPrimerApellido);
                Property propertySegundoApellido = entidadAux.properties.FirstOrDefault(x => x.prop == Variables.ExperienciaCientificaTecnologica.contratosIPSegundoApellido);
                Property propertyFirma = entidadAux.properties.FirstOrDefault(x => x.prop == Variables.ExperienciaCientificaTecnologica.contratosIPFirma);

                string valorNombre = autor.GetNombreAutor();
                string valorPrimerApellido = autor.GetPrimerApellidoAutor();
                string valorSegundoApellido = autor.GetSegundoApellidoAutor();
                string valorFirma = autor.GetFirmaAutor();

                string propiedadNombre = Variables.ExperienciaCientificaTecnologica.contratosIPNombre;
                string propiedadPrimerApellido = Variables.ExperienciaCientificaTecnologica.contratosIPPrimerApellido;
                string propiedadSegundoApellido = Variables.ExperienciaCientificaTecnologica.contratosIPSegundoApellido;
                string propiedadFirma = Variables.ExperienciaCientificaTecnologica.contratosIPFirma;

                UtilitySecciones.CheckProperty(propertyNombre, entidadAux, valorNombre, propiedadNombre);
                UtilitySecciones.CheckProperty(propertyPrimerApellido, entidadAux, valorPrimerApellido, propiedadPrimerApellido);
                UtilitySecciones.CheckProperty(propertySegundoApellido, entidadAux, valorSegundoApellido, propiedadSegundoApellido);
                UtilitySecciones.CheckProperty(propertyFirma, entidadAux, valorFirma, propiedadFirma);
            }
        }

        /// <summary>
        /// Inserta en <paramref name="entidadAux"/> los valores de <paramref name="item"/>,
        /// pertenecientes a la Entidad Financiadora.
        /// </summary>
        /// <param name="item">item</param>
        /// <param name="entidadAux">entidadAux</param>
        private void ContratosEntidadFinanciadora(CvnItemBean item, Entity entidadAux)//TODO
        {
            List<CvnItemBeanCvnCodeGroup> listadoEntidadFinanciadora = item.GetListaElementosPorIDCampo<CvnItemBeanCvnCodeGroup>("050.020.020.120");
            foreach (CvnItemBeanCvnCodeGroup entidadFinanciadora in listadoEntidadFinanciadora)
            {
                //Añado la referencia si existe Entidad
                if(entidadFinanciadora.CvnEntityBean == null) { continue; }
                UtilitySecciones.AniadirEntidad(mResourceApi, entidadFinanciadora.CvnEntityBean.Name,
                    Variables.ExperienciaCientificaTecnologica.proyectosIDIEntidadRealizacionNombre,
                    Variables.ExperienciaCientificaTecnologica.proyectosIDIEntidadRealizacion, entidadAux);

                //Property entidad = entidadAux.properties.FirstOrDefault(x => x.prop == Variables.ExperienciaCientificaTecnologica.contratosEntidadFinanciadora);
                //string valor = UtilitySecciones.GetOrganizacionPorNombre(mResourceApi, entidadFinanciadora.GetNameEntityBeanCvnCodeGroup("050.020.020.120"));
                //string propiedad = Variables.ExperienciaCientificaTecnologica.contratosEntidadFinanciadora;
                //UtilitySecciones.CheckProperty(entidad, entidadAux, valor, propiedad);
                /*
                //Añado pais, CCAA y ciudad
                entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                    new Property(Variables.ExperienciaCientificaTecnologica.contratosPaisEntidadFinanciadora, entidadFinanciadora.GetPaisPorIDCampo("050.020.020.350")),
                    new Property(Variables.ExperienciaCientificaTecnologica.contratosCCAAEntidadFinanciadora, entidadFinanciadora.GetRegionPorIDCampo("050.020.020.360")),
                    new Property(Variables.ExperienciaCientificaTecnologica.contratosCiudadEntidadFinanciadora, entidadFinanciadora.GetStringCvnCodeGroup("050.020.020.340"))
                ));
                //Añado los datos asociados a la entidad
                if (!string.IsNullOrEmpty(entidadFinanciadora.GetNameEntityBeanCvnCodeGroup("050.020.020.120")))
                {
                    entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                        new Property(Variables.ExperienciaCientificaTecnologica.contratosEntidadFinanciadora, entidadFinanciadora.GetNameEntityBeanCvnCodeGroup("050.020.020.120"))
                    ));

                    //Añado otros, o el ID de una preseleccion
                    if (!string.IsNullOrEmpty(entidadFinanciadora.GetStringCvnCodeGroup("050.020.020.150")))
                    {
                        entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                            new Property(Variables.ExperienciaCientificaTecnologica.contratosTipoEntidadFinanciadora, "http://gnoss.com/items/organizationtype_OTHERS"),//TODO revisar
                            new Property(Variables.ExperienciaCientificaTecnologica.contratosTipoEntidadFinanciadoraOtros, entidadFinanciadora.GetStringCvnCodeGroup("050.020.020.150"))
                        ));
                    }
                    else
                    {
                        entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                            new Property(Variables.ExperienciaCientificaTecnologica.contratosTipoEntidadFinanciadora, entidadFinanciadora.GetOrganizationCvnCodeGroup("050.020.020.140"))
                        ));
                    }
                }
                */
            }
        }

        /// <summary>
        /// 050.030.010.000
        /// </summary>
        /// <param name="listadoDatos"></param>
        /// <returns></returns>
        private List<Entity> GetPropiedadIndustrialIntelectual(List<CvnItemBean> listadoDatos)
        {
            List<Entity> listado = new List<Entity>();

            List<CvnItemBean> listadoPropiedadIndustrialIntelectual = listadoDatos.Where(x => x.Code.Equals("050.030.010.000")).ToList();
            if (listadoPropiedadIndustrialIntelectual.Count > 0)
            {
                foreach (CvnItemBean item in listadoPropiedadIndustrialIntelectual)
                {
                    Entity entidadAux = new Entity();
                    entidadAux.properties = new List<Property>();
                    if (!string.IsNullOrEmpty(item.GetStringPorIDCampo("050.030.010.020")))
                    {
                        entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                            new Property(Variables.ExperienciaCientificaTecnologica.propIIDescripcion, item.GetStringPorIDCampo("050.030.010.010")),
                            new Property(Variables.ExperienciaCientificaTecnologica.propIITituloPropIndus, item.GetStringPorIDCampo("050.030.010.020")),
                            new Property(Variables.ExperienciaCientificaTecnologica.propIITipoPropIndus, item.GetTipoPropiedadIndustrialPorIDCampo("050.030.010.030")),
                            new Property(Variables.ExperienciaCientificaTecnologica.propIITipoPropIndusOtros, item.GetStringPorIDCampo("050.030.010.040")),
                            new Property(Variables.ExperienciaCientificaTecnologica.propIIDerechosAutor, item.GetStringBooleanPorIDCampo("050.030.010.050")),
                            new Property(Variables.ExperienciaCientificaTecnologica.propIIDerechosConexos, item.GetStringBooleanPorIDCampo("050.030.010.060")),
                            new Property(Variables.ExperienciaCientificaTecnologica.propIISecretoEmpresarial, item.GetStringBooleanPorIDCampo("050.030.010.070")),
                            new Property(Variables.ExperienciaCientificaTecnologica.propIIModalidadKnowHow, item.GetStringBooleanPorIDCampo("050.030.010.080")),
                            new Property(Variables.ExperienciaCientificaTecnologica.propIICodReferencia, item.GetValueCvnExternalPKBean("050.030.010.100")),
                            new Property(Variables.ExperienciaCientificaTecnologica.propIINumSolicitud, item.GetValueCvnExternalPKBean("050.030.010.110")),
                            new Property(Variables.ExperienciaCientificaTecnologica.propIIPaisInscripcion, item.GetPaisPorIDCampo("050.030.010.120")),
                            new Property(Variables.ExperienciaCientificaTecnologica.propIICCAAInscripcion, item.GetRegionPorIDCampo("050.030.010.130")),
                            new Property(Variables.ExperienciaCientificaTecnologica.propIIFechaRegistro, item.GetStringDatetimePorIDCampo("050.030.010.150")),
                            new Property(Variables.ExperienciaCientificaTecnologica.propIIPatenteEsp, item.GetStringBooleanPorIDCampo("050.030.010.160")),
                            new Property(Variables.ExperienciaCientificaTecnologica.propIIPatenteUE, item.GetStringBooleanPorIDCampo("050.030.010.170")),
                            new Property(Variables.ExperienciaCientificaTecnologica.propIIPatenteNoUE, item.GetStringBooleanPorIDCampo("050.030.010.180")),
                            new Property(Variables.ExperienciaCientificaTecnologica.propIIResultadosRelevantes, item.GetStringPorIDCampo("050.030.010.190")),
                            new Property(Variables.ExperienciaCientificaTecnologica.propIILicencias, item.GetStringBooleanPorIDCampo("050.030.010.210")),
                            new Property(Variables.ExperienciaCientificaTecnologica.propIIExplotacionExclusiva, item.GetStringBooleanPorIDCampo("050.030.010.260")),
                            new Property(Variables.ExperienciaCientificaTecnologica.propIIGeneradaEmpresaInnov, item.GetStringBooleanPorIDCampo("050.030.010.270")),
                            new Property(Variables.ExperienciaCientificaTecnologica.propIIResultadoEmpresaInnov, item.GetTipoResultadoIDCampo("050.030.010.280")),
                            new Property(Variables.ExperienciaCientificaTecnologica.propIINumPatente, item.GetValueCvnExternalPKBean("050.030.010.310")),
                            new Property(Variables.ExperienciaCientificaTecnologica.propIIFechaConcesion, item.GetStringDatetimePorIDCampo("050.030.010.320")),
                            new Property(Variables.ExperienciaCientificaTecnologica.propIIPatentePCT, item.GetStringBooleanPorIDCampo("050.030.010.330"))
                        ));
                        PropiedadIndustrialIntelectualEntidadTitularDerechos(item, entidadAux);
                        PropiedadIndustrialIntelectualEmpresas(item, entidadAux);
                        PropiedadIndustrialIntelectualProductos(item, entidadAux);
                        PropiedadIndustrialIntelectualAutores(item, entidadAux);
                        PropiedadIndustrialIntelectualPaisExplotacion(item, entidadAux);
                        PropiedadIndustrialIntelectualPalabrasClave(item, entidadAux);

                        listado.Add(entidadAux);
                    }
                }
            }
            return listado;
        }

        /// <summary>
        /// Inserta en <paramref name="entidadAux"/> los valores de <paramref name="item"/>,
        /// pertenecientes a las palabaras clave.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="entidadAux"></param>
        private void PropiedadIndustrialIntelectualPalabrasClave(CvnItemBean item, Entity entidadAux)
        {
            List<CvnItemBeanCvnString> listadoPalabrasClave = item.GetListaElementosPorIDCampo<CvnItemBeanCvnString>("050.030.010.200");

            string propiedadPalabrasClave = Variables.ExperienciaCientificaTecnologica.propIIPalabrasClave;

            foreach (CvnItemBeanCvnString palabraClave in listadoPalabrasClave)
            {
                string entityPartAux = Guid.NewGuid().ToString() + "@@@";

                List<string> listadoPalabras = Utility.GetPadresPalabrasClave(palabraClave);
                foreach (string palabra in listadoPalabras)
                {
                    Property propertyPalabrasClave = entidadAux.properties.FirstOrDefault(x => x.prop == Variables.ExperienciaCientificaTecnologica.propIIPalabrasClave);
                    UtilitySecciones.CheckProperty(propertyPalabrasClave, entidadAux,
                        UtilitySecciones.StringGNOSSID(entityPartAux, Utility.ObtenerPalabraClave(mResourceApi, palabra)), propiedadPalabrasClave);
                }
            }
        }

        /// <summary>
        /// Inserta en <paramref name="entidadAux"/> los valores de <paramref name="item"/>,
        /// pertenecientes a la Entidad titular de derechos.
        /// </summary>
        /// <param name="item">item</param>
        /// <param name="entidadAux">entidadAux</param>
        private void PropiedadIndustrialIntelectualEntidadTitularDerechos(CvnItemBean item, Entity entidadAux)
        {
            //Añado la referencia si existe Entidad
            UtilitySecciones.AniadirEntidad(mResourceApi, item.GetNameEntityBeanPorIDCampo("050.030.010.300"),
                Variables.ExperienciaCientificaTecnologica.propIIEntidadTitularDerechosNombre,
                Variables.ExperienciaCientificaTecnologica.propIIEntidadTitularDerechos, entidadAux);
        }

        /// <summary>
        /// Inserta en <paramref name="entidadAux"/> los valores de <paramref name="item"/>,
        /// pertenecientes a las Empresas.
        /// </summary>
        /// <param name="item">item</param>
        /// <param name="entidadAux">entidadAux</param>
        private void PropiedadIndustrialIntelectualEmpresas(CvnItemBean item, Entity entidadAux)
        {
            //Añado el listado de empresas
            List<CvnItemBeanCvnEntityBean> listadoEmpresas = item.GetListaElementosPorIDCampo<CvnItemBeanCvnEntityBean>("050.030.010.250");

            foreach (CvnItemBeanCvnEntityBean empresa in listadoEmpresas)
            {
                string entityPartAux = Guid.NewGuid().ToString() + "@@@";

                Property referenciaPropII = entidadAux.properties.FirstOrDefault(x => x.prop == Variables.ExperienciaCientificaTecnologica.propIIEmpresasExplotacion);
                Property nombrePropII = entidadAux.properties.FirstOrDefault(x => x.prop == Variables.ExperienciaCientificaTecnologica.propIIEmpresasExplotacionNombre);

                string valorReferencia = UtilitySecciones.StringGNOSSID(entityPartAux, UtilitySecciones.GetOrganizacionPorNombre(mResourceApi, empresa.Name));
                string valorNombre = UtilitySecciones.StringGNOSSID(entityPartAux, empresa.Name);
                string propiedadReferencia = Variables.ExperienciaCientificaTecnologica.propIIEmpresasExplotacion;
                string propiedadNombre = Variables.ExperienciaCientificaTecnologica.propIIEmpresasExplotacionNombre;

                UtilitySecciones.CheckProperty(referenciaPropII, entidadAux, valorReferencia, propiedadReferencia);
                UtilitySecciones.CheckProperty(nombrePropII, entidadAux, valorNombre, propiedadNombre);
            }
        }

        /// <summary>
        /// Inserta en <paramref name="entidadAux"/> los valores de <paramref name="item"/>,
        /// pertenecientes a los Productos.
        /// </summary>
        /// <param name="item">item</param>
        /// <param name="entidadAux">entidadAux</param>
        private void PropiedadIndustrialIntelectualProductos(CvnItemBean item, Entity entidadAux)
        {
            //Añado el listado de productos
            List<CvnItemBeanCvnString> listadoProductos = item.GetListaElementosPorIDCampo<CvnItemBeanCvnString>("050.030.010.290");

            foreach (CvnItemBeanCvnString producto in listadoProductos)
            {
                Property nombrePropII = entidadAux.properties.FirstOrDefault(x => x.prop == Variables.ExperienciaCientificaTecnologica.propIINombreProductos);
                string valor = producto.Value;
                string propiedad = Variables.ExperienciaCientificaTecnologica.propIINombreProductos;
                UtilitySecciones.CheckProperty(nombrePropII, entidadAux, valor, propiedad);
            }
        }

        /// <summary>
        /// Inserta en <paramref name="entidadAux"/> los valores de <paramref name="item"/>,
        /// pertenecientes a los Autores/as.
        /// </summary>
        /// <param name="item">item</param>
        /// <param name="entidadAux">entidadAux</param>
        private void PropiedadIndustrialIntelectualAutores(CvnItemBean item, Entity entidadAux)
        {
            //Añado el listado de autores principales
            List<CvnItemBeanCvnAuthorBean> listadoAutores = item.GetListaElementosPorIDCampo<CvnItemBeanCvnAuthorBean>("050.030.010.090");
            foreach (CvnItemBeanCvnAuthorBean autor in listadoAutores)
            {
                //Si no tiene nombre no lo inserto
                if (string.IsNullOrEmpty(autor.GetNombreAutor())) { continue; }
                string entityPartAux = Guid.NewGuid().ToString() + "@@@";

                Property propertyNombre = entidadAux.properties.FirstOrDefault(x => x.prop == Variables.ExperienciaCientificaTecnologica.propIIInventoresAutoresNombre);
                Property propertyPrimerApellido = entidadAux.properties.FirstOrDefault(x => x.prop == Variables.ExperienciaCientificaTecnologica.propIIInventoresAutoresPrimerApellido);
                Property propertySegundoApellido = entidadAux.properties.FirstOrDefault(x => x.prop == Variables.ExperienciaCientificaTecnologica.propIIInventoresAutoresSegundoApellido);
                Property propertyFirma = entidadAux.properties.FirstOrDefault(x => x.prop == Variables.ExperienciaCientificaTecnologica.propIIInventoresAutoresFirma);

                string valorNombre = UtilitySecciones.StringGNOSSID(entityPartAux, autor.GetNombreAutor());
                string valorPrimerApellido = UtilitySecciones.StringGNOSSID(entityPartAux, autor.GetPrimerApellidoAutor());
                string valorSegundoApellido = UtilitySecciones.StringGNOSSID(entityPartAux, autor.GetSegundoApellidoAutor());
                string valorFirma = UtilitySecciones.StringGNOSSID(entityPartAux, autor.GetFirmaAutor());

                string propiedadNombre = Variables.ExperienciaCientificaTecnologica.propIIInventoresAutoresNombre;
                string propiedadPrimerApellido = Variables.ExperienciaCientificaTecnologica.propIIInventoresAutoresPrimerApellido;
                string propiedadSegundoApellido = Variables.ExperienciaCientificaTecnologica.propIIInventoresAutoresSegundoApellido;
                string propiedadFirma = Variables.ExperienciaCientificaTecnologica.propIIInventoresAutoresFirma;

                UtilitySecciones.CheckProperty(propertyNombre, entidadAux, valorNombre, propiedadNombre);
                UtilitySecciones.CheckProperty(propertyPrimerApellido, entidadAux, valorPrimerApellido, propiedadPrimerApellido);
                UtilitySecciones.CheckProperty(propertySegundoApellido, entidadAux, valorSegundoApellido, propiedadSegundoApellido);
                UtilitySecciones.CheckProperty(propertyFirma, entidadAux, valorFirma, propiedadFirma);
            }
        }

        /// <summary>
        /// Inserta en <paramref name="entidadAux"/> los valores de <paramref name="item"/>,
        /// pertenecientes al Pais y CCAA/Región de explotación.
        /// </summary>
        /// <param name="item">item</param>
        /// <param name="entidadAux">entidadAux</param>
        private void PropiedadIndustrialIntelectualPaisExplotacion(CvnItemBean item, Entity entidadAux)
        {
            List<CvnItemBeanCvnCodeGroup> listadoPais = item.GetListaElementosPorIDCampo<CvnItemBeanCvnCodeGroup>("050.030.010.220");

            foreach (CvnItemBeanCvnCodeGroup entidadPais in listadoPais)
            {
                string entityPartAux = Guid.NewGuid().ToString() + "@@@";
                Property propertyPais = entidadAux.properties.FirstOrDefault(x => x.prop == Variables.ExperienciaCientificaTecnologica.propIIPaisExplotacion);
                Property propertyRegion = entidadAux.properties.FirstOrDefault(x => x.prop == Variables.ExperienciaCientificaTecnologica.propIICCAAExplotacion);

                string valorPais = entidadPais.GetPaisPorIDCampo("050.030.010.220");
                valorPais = !string.IsNullOrEmpty(valorPais) ? UtilitySecciones.StringGNOSSID(entityPartAux, valorPais) : null;

                string valorRegion = entidadPais.GetPaisPorIDCampo("050.030.010.230");
                valorRegion = !string.IsNullOrEmpty(valorRegion) ? UtilitySecciones.StringGNOSSID(entityPartAux, valorRegion) : null;

                string propiedadPais = Variables.ExperienciaCientificaTecnologica.propIIPaisExplotacion;
                string propiedadRegion = Variables.ExperienciaCientificaTecnologica.propIICCAAExplotacion;

                UtilitySecciones.CheckProperty(propertyPais, entidadAux, valorPais, propiedadPais);
                UtilitySecciones.CheckProperty(propertyRegion, entidadAux, valorRegion, propiedadRegion);
            }
        }

        /// <summary>
        /// 050.010.000.000
        /// </summary>
        /// <param name="listadoDatos"></param>
        /// <returns></returns>
        private List<Entity> GetGrupoIDI(List<CvnItemBean> listadoDatos)
        {
            List<Entity> listado = new List<Entity>();

            List<CvnItemBean> listadoGrupoIDI = listadoDatos.Where(x => x.Code.Equals("050.010.000.000")).ToList();
            if (listadoGrupoIDI.Count > 0)
            {
                foreach (CvnItemBean item in listadoGrupoIDI)
                {
                    Entity entidadAux = new Entity();
                    entidadAux.properties = new List<Property>();
                    if (!string.IsNullOrEmpty(item.GetStringPorIDCampo("050.010.000.020")))
                    {
                        entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                            new Property(Variables.ExperienciaCientificaTecnologica.grupoIDIObjetoGrupo, item.GetStringPorIDCampo("050.010.000.010")),
                            new Property(Variables.ExperienciaCientificaTecnologica.grupoIDINombreGrupo, item.GetStringPorIDCampo("050.010.000.020")),
                            new Property(Variables.ExperienciaCientificaTecnologica.grupoIDICodNormalizado, item.GetValueCvnExternalPKBean("050.010.000.030")),
                            new Property(Variables.ExperienciaCientificaTecnologica.grupoIDIPaisRadicacion, item.GetPaisPorIDCampo("050.010.000.040")),
                            new Property(Variables.ExperienciaCientificaTecnologica.grupoIDICCAARadicacion, item.GetRegionPorIDCampo("050.010.000.050")),
                            new Property(Variables.ExperienciaCientificaTecnologica.grupoIDICiudadRadicacion, item.GetStringPorIDCampo("050.010.000.070")),
                            new Property(Variables.ExperienciaCientificaTecnologica.grupoIDINumComponentes, item.GetStringDoublePorIDCampo("050.010.000.130")),
                            new Property(Variables.ExperienciaCientificaTecnologica.grupoIDIFechaInicio, item.GetStringDatetimePorIDCampo("050.010.000.140")),
                            new Property(Variables.ExperienciaCientificaTecnologica.grupoIDIDuracionAnio, item.GetDurationAnioPorIDCampo("050.010.000.150")),
                            new Property(Variables.ExperienciaCientificaTecnologica.grupoIDIDuracionMes, item.GetDurationMesPorIDCampo("050.010.000.150")),
                            new Property(Variables.ExperienciaCientificaTecnologica.grupoIDIDuracionDia, item.GetDurationDiaPorIDCampo("050.010.000.150")),
                            new Property(Variables.ExperienciaCientificaTecnologica.grupoIDIClaseColaboracion, item.GetTipoColaboracionPorIDCampo("050.010.000.160")),//TODO - check
                            new Property(Variables.ExperienciaCientificaTecnologica.grupoIDINumTesisDirigidas, item.GetStringDoublePorIDCampo("050.010.000.170")),
                            new Property(Variables.ExperienciaCientificaTecnologica.grupoIDINumPosDocDirigidos, item.GetStringDoublePorIDCampo("050.010.000.180")),
                            new Property(Variables.ExperienciaCientificaTecnologica.grupoIDIResultadosOtros, item.GetStringPorIDCampo("050.010.000.190")),
                            new Property(Variables.ExperienciaCientificaTecnologica.grupoIDIResultadosMasRelevantes, item.GetStringPorIDCampo("050.010.000.200"))
                        ));
                        GrupoIDIAutores(item, entidadAux);
                        GrupoIDIEntidadAfiliacion(item, entidadAux);
                        GrupoIDIPalabrasClave(item, entidadAux);

                        listado.Add(entidadAux);
                    }
                }
            }
            return listado;
        }

        /// <summary>
        /// Inserta en <paramref name="entidadAux"/> los valores de <paramref name="item"/>,
        /// pertenecientes a las palabaras clave.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="entidadAux"></param>
        private void GrupoIDIPalabrasClave(CvnItemBean item, Entity entidadAux)
        {
            List<CvnItemBeanCvnString> listadoPalabrasClave = item.GetListaElementosPorIDCampo<CvnItemBeanCvnString>("050.010.000.210");

            string propiedadPalabrasClave = Variables.ExperienciaCientificaTecnologica.grupoIDIPalabrasClave;

            foreach (CvnItemBeanCvnString palabraClave in listadoPalabrasClave)
            {
                string entityPartAux = Guid.NewGuid().ToString() + "@@@";

                List<string> listadoPalabras = Utility.GetPadresPalabrasClave(palabraClave);
                foreach (string palabra in listadoPalabras)
                {
                    Property propertyPalabrasClave = entidadAux.properties.FirstOrDefault(x => x.prop == Variables.ExperienciaCientificaTecnologica.grupoIDIPalabrasClave);
                    UtilitySecciones.CheckProperty(propertyPalabrasClave, entidadAux,
                        UtilitySecciones.StringGNOSSID(entityPartAux, Utility.ObtenerPalabraClave(mResourceApi, palabra)), propiedadPalabrasClave);
                }
            }
        }

        /// <summary>
        /// Inserta en <paramref name="entidadAux"/> los valores de <paramref name="item"/>,
        /// pertenecientes a la Entidad de Afiliación.
        /// </summary>
        /// <param name="item">item</param>
        /// <param name="entidadAux">entidadAux</param>
        private void GrupoIDIEntidadAfiliacion(CvnItemBean item, Entity entidadAux)
        {
            //Añado la referencia si existe Entidad
            UtilitySecciones.AniadirEntidad(mResourceApi, item.GetNameEntityBeanPorIDCampo("050.010.000.090"),
                Variables.ExperienciaCientificaTecnologica.grupoIDIEntidadAfiliacionNombre,
                Variables.ExperienciaCientificaTecnologica.grupoIDIEntidadAfiliacion, entidadAux);
        }

        /// <summary>
        /// Inserta en <paramref name="entidadAux"/> los valores de <paramref name="item"/>,
        /// pertenecientes a los Autores/as.
        /// </summary>
        /// <param name="item">item</param>
        /// <param name="entidadAux">entidadAux</param>
        private void GrupoIDIAutores(CvnItemBean item, Entity entidadAux)
        {
            string propiedadAutorFirma = Variables.ExperienciaCientificaTecnologica.grupoIDIFirmaIP;
            string propiedadAutorNombre = Variables.ExperienciaCientificaTecnologica.grupoIDINombreIP;
            string propiedadAutorPrimerApellido = Variables.ExperienciaCientificaTecnologica.grupoIDIPrimerApellidoIP;
            string propiedadAutorSegundoApellido = Variables.ExperienciaCientificaTecnologica.grupoIDISegundoapellidoIP;

            //Añado el listado de autores principales
            List<CvnItemBeanCvnAuthorBean> listadoAutores = item.GetListaElementosPorIDCampo<CvnItemBeanCvnAuthorBean>("050.010.000.080");
            foreach (CvnItemBeanCvnAuthorBean autor in listadoAutores)
            {
                //Si no tiene nombre continuo con el siguiente
                if (string.IsNullOrEmpty(autor.GetNombreAutor())) { continue; }

                string entityPartAux = Guid.NewGuid().ToString() + "@@@";
                Property propertyAutorFirma = entidadAux.properties.FirstOrDefault(x => x.prop == propiedadAutorFirma);
                Property propertyAutorNombre = entidadAux.properties.FirstOrDefault(x => x.prop == propiedadAutorNombre);
                Property propertyAutorPrimerApellido = entidadAux.properties.FirstOrDefault(x => x.prop == propiedadAutorPrimerApellido);
                Property propertyAutorSegundoApellido = entidadAux.properties.FirstOrDefault(x => x.prop == propiedadAutorSegundoApellido);

                UtilitySecciones.CheckProperty(propertyAutorFirma, entidadAux,
                    UtilitySecciones.StringGNOSSID(entityPartAux, autor.GetFirmaAutor()), propiedadAutorFirma);
                UtilitySecciones.CheckProperty(propertyAutorNombre, entidadAux,
                    UtilitySecciones.StringGNOSSID(entityPartAux, autor.GetNombreAutor()), propiedadAutorNombre);
                UtilitySecciones.CheckProperty(propertyAutorPrimerApellido, entidadAux,
                    UtilitySecciones.StringGNOSSID(entityPartAux, autor.GetPrimerApellidoAutor()), propiedadAutorPrimerApellido);
                UtilitySecciones.CheckProperty(propertyAutorSegundoApellido, entidadAux,
                    UtilitySecciones.StringGNOSSID(entityPartAux, autor.GetSegundoApellidoAutor()), propiedadAutorSegundoApellido);
            }
        }

        /// <summary>
        /// 050.020.030.000
        /// </summary>
        /// <param name="listadoDatos"></param>
        /// <returns></returns>
        private List<Entity> GetObrasArtisticas(List<CvnItemBean> listadoDatos)
        {
            List<Entity> listado = new List<Entity>();

            List<CvnItemBean> listadoObrasArtisticas = listadoDatos.Where(x => x.Code.Equals("050.020.030.000")).ToList();
            if (listadoObrasArtisticas.Count > 0)
            {
                foreach (CvnItemBean item in listadoObrasArtisticas)
                {
                    Entity entidadAux = new Entity();
                    entidadAux.properties = new List<Property>();
                    if (!string.IsNullOrEmpty(item.GetStringPorIDCampo("050.020.030.010")))
                    {
                        entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                            new Property(Variables.ExperienciaCientificaTecnologica.obrasArtisticasDescripcion, item.GetStringPorIDCampo("050.020.030.010")),
                            new Property(Variables.ExperienciaCientificaTecnologica.obrasArtisticasNombreExpo, item.GetStringPorIDCampo("050.020.030.020")),
                            new Property(Variables.ExperienciaCientificaTecnologica.obrasArtisticasPaisExpo, item.GetPaisPorIDCampo("050.020.030.040")),
                            new Property(Variables.ExperienciaCientificaTecnologica.obrasArtisticasCCAAExpo, item.GetRegionPorIDCampo("050.020.030.050")),
                            new Property(Variables.ExperienciaCientificaTecnologica.obrasArtisticasCiudadExpo, item.GetStringPorIDCampo("050.020.030.070")),
                            new Property(Variables.ExperienciaCientificaTecnologica.obrasArtisticasForoExpo, item.GetStringPorIDCampo("050.020.030.080")),
                            new Property(Variables.ExperienciaCientificaTecnologica.obrasArtisticasMonografica, item.GetStringBooleanPorIDCampo("050.020.030.090")),
                            new Property(Variables.ExperienciaCientificaTecnologica.obrasArtisticasCatalogo, item.GetStringBooleanPorIDCampo("050.020.030.100")),
                            new Property(Variables.ExperienciaCientificaTecnologica.obrasArtisticasComisario, item.GetStringBooleanPorIDCampo("050.020.030.110")),
                            new Property(Variables.ExperienciaCientificaTecnologica.obrasArtisticasFechaInicio, item.GetStringDatetimePorIDCampo("050.020.030.120")),
                            new Property(Variables.ExperienciaCientificaTecnologica.obrasArtisticasCatalogacion, item.GetStringPorIDCampo("050.020.030.130")),
                            new Property(Variables.ExperienciaCientificaTecnologica.obrasArtisticasPremio, item.GetStringPorIDCampo("050.020.030.140")),
                            new Property(Variables.ExperienciaCientificaTecnologica.obrasArtisticasTituloPublicacion, item.GetStringPorIDCampo("050.020.030.150")),
                            new Property(Variables.ExperienciaCientificaTecnologica.obrasArtisticasOtros, item.GetStringPorIDCampo("050.020.030.160"))
                        ));
                        ObrasArtisticasAutores(item, entidadAux);

                        listado.Add(entidadAux);
                    }
                }
            }
            return listado;
        }

        /// <summary>
        /// Inserta en <paramref name="entidadAux"/> los valores de <paramref name="item"/>,
        /// pertenecientes a los Autores/as.
        /// </summary>
        /// <param name="item">item</param>
        /// <param name="entidadAux">entidadAux</param>
        private void ObrasArtisticasAutores(CvnItemBean item, Entity entidadAux)
        {
            string propiedadAutorFirma = Variables.ExperienciaCientificaTecnologica.obrasArtisticasAutoresFirma;
            string propiedadAutorNombre = Variables.ExperienciaCientificaTecnologica.obrasArtisticasAutoresNombre;
            string propiedadAutorPrimerApellido = Variables.ExperienciaCientificaTecnologica.obrasArtisticasAutoresPrimerApellido;
            string propiedadAutorSegundoApellido = Variables.ExperienciaCientificaTecnologica.obrasArtisticasAutoresSegundoApellido;

            //Añado el listado de autores principales
            List<CvnItemBeanCvnAuthorBean> listadoAutores = item.GetListaElementosPorIDCampo<CvnItemBeanCvnAuthorBean>("050.020.030.030");
            foreach (CvnItemBeanCvnAuthorBean autor in listadoAutores)
            {
                //Si no tiene nombre continuo con el siguiente
                if (string.IsNullOrEmpty(autor.GetNombreAutor())) { continue; }

                string entityPartAux = Guid.NewGuid().ToString() + "@@@";
                Property propertyAutorFirma = entidadAux.properties.FirstOrDefault(x => x.prop == propiedadAutorFirma);
                Property propertyAutorNombre = entidadAux.properties.FirstOrDefault(x => x.prop == propiedadAutorNombre);
                Property propertyAutorPrimerApellido = entidadAux.properties.FirstOrDefault(x => x.prop == propiedadAutorPrimerApellido);
                Property propertyAutorSegundoApellido = entidadAux.properties.FirstOrDefault(x => x.prop == propiedadAutorSegundoApellido);

                UtilitySecciones.CheckProperty(propertyAutorFirma, entidadAux,
                    UtilitySecciones.StringGNOSSID(entityPartAux, autor.GetFirmaAutor()), propiedadAutorFirma);
                UtilitySecciones.CheckProperty(propertyAutorNombre, entidadAux,
                    UtilitySecciones.StringGNOSSID(entityPartAux, autor.GetNombreAutor()), propiedadAutorNombre);
                UtilitySecciones.CheckProperty(propertyAutorPrimerApellido, entidadAux,
                    UtilitySecciones.StringGNOSSID(entityPartAux, autor.GetPrimerApellidoAutor()), propiedadAutorPrimerApellido);
                UtilitySecciones.CheckProperty(propertyAutorSegundoApellido, entidadAux,
                    UtilitySecciones.StringGNOSSID(entityPartAux, autor.GetSegundoApellidoAutor()), propiedadAutorSegundoApellido);
            }
        }

        /// <summary>
        /// 050.030.020.000
        /// </summary>
        /// <param name="listadoDatos"></param>
        /// <returns></returns>
        private List<Entity> GetResultadosTecnologicos(List<CvnItemBean> listadoDatos)
        {
            List<Entity> listado = new List<Entity>();

            List<CvnItemBean> listadoResultadosTecnologicos = listadoDatos.Where(x => x.Code.Equals("050.030.020.000")).ToList();
            if (listadoResultadosTecnologicos.Count > 0)
            {
                foreach (CvnItemBean item in listadoResultadosTecnologicos)
                {
                    Entity entidadAux = new Entity();
                    entidadAux.properties = new List<Property>();
                    if (!string.IsNullOrEmpty(item.GetStringPorIDCampo("050.030.020.010")))
                    {
                        entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                            new Property(Variables.ExperienciaCientificaTecnologica.resultadosTecnologicosDescripcion, item.GetStringPorIDCampo("050.030.020.010")),
                            new Property(Variables.ExperienciaCientificaTecnologica.resultadosTecnologicosGradoContribucion, item.GetGradoContribucionPorIDCampo("050.030.020.070")),//TODO -check
                            new Property(Variables.ExperienciaCientificaTecnologica.resultadosTecnologicosGradoContribucionOtros, item.GetStringPorIDCampo("050.030.020.080")),
                            new Property(Variables.ExperienciaCientificaTecnologica.resultadosTecnologicosNuevasTecnicasEquip, item.GetStringBooleanPorIDCampo("050.030.020.090")),
                            new Property(Variables.ExperienciaCientificaTecnologica.resultadosTecnologicosEmpresasSpinOff, item.GetStringBooleanPorIDCampo("050.030.020.100")),
                            new Property(Variables.ExperienciaCientificaTecnologica.resultadosTecnologicosResultadosMejoraProd, item.GetStringBooleanPorIDCampo("050.030.020.110")),
                            new Property(Variables.ExperienciaCientificaTecnologica.resultadosTecnologicosHomologos, item.GetStringBooleanPorIDCampo("050.030.020.120")),
                            new Property(Variables.ExperienciaCientificaTecnologica.resultadosTecnologicosExpertoTecnologico, item.GetStringBooleanPorIDCampo("050.030.020.130")),
                            new Property(Variables.ExperienciaCientificaTecnologica.resultadosTecnologicosConveniosColab, item.GetStringBooleanPorIDCampo("050.030.020.140")),
                            new Property(Variables.ExperienciaCientificaTecnologica.resultadosTecnologicosAmbitoActividad, item.GetGeographicRegionPorIDCampo("050.030.020.150")),//TODO - check
                            new Property(Variables.ExperienciaCientificaTecnologica.resultadosTecnologicosAmbitoActividadOtros, item.GetStringPorIDCampo("050.030.020.160")),
                            new Property(Variables.ExperienciaCientificaTecnologica.resultadosTecnologicosFechaInicio, item.GetStringDatetimePorIDCampo("050.030.020.250")),
                            new Property(Variables.ExperienciaCientificaTecnologica.resultadosTecnologicosDuracionAnio, item.GetDurationAnioPorIDCampo("050.030.020.260")),
                            new Property(Variables.ExperienciaCientificaTecnologica.resultadosTecnologicosDuracionMes, item.GetDurationMesPorIDCampo("050.030.020.260")),
                            new Property(Variables.ExperienciaCientificaTecnologica.resultadosTecnologicosDuracionDia, item.GetDurationDiaPorIDCampo("050.030.020.260")),
                            new Property(Variables.ExperienciaCientificaTecnologica.resultadosTecnologicosResultadosRelevantes, item.GetStringPorIDCampo("050.030.020.270"))
                        ));
                        ResultadosTecnologicosAutoresYCorresponsables(item, entidadAux);
                        ResultadosTecnologicosEntidadColaboradora(item, entidadAux);
                        ResultadosTecnologicosEntidadDestinataria(item, entidadAux);
                        ResultadosTecnologicosCodigosUnesco(item, entidadAux);
                        ResultadosTecnologicosPalabrasClave(item, entidadAux);

                        listado.Add(entidadAux);
                    }
                }
            }
            return listado;
        }

        /// <summary>
        /// Inserta en <paramref name="entidadAux"/> los valores de <paramref name="item"/>,
        /// pertenecientes a los Códigos UNESCO.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="entidadAux"></param>
        private void ResultadosTecnologicosCodigosUnesco(CvnItemBean item, Entity entidadAux)
        {
            //Añado los códigos UNESCO de especialización primaria
            List<CvnItemBeanCvnString> listadoCodUnescoPrimaria = item.GetListaElementosPorIDCampo<CvnItemBeanCvnString>("050.030.020.020");
            UtilitySecciones.CodigosUnesco(listadoCodUnescoPrimaria, entidadAux, Variables.ExperienciaCientificaTecnologica.resultadosTecnologicosCodUnescoPrimaria);

            //Añado los códigos UNESCO de especialización secundaria
            List<CvnItemBeanCvnString> listadoCodUnescoSecundaria = item.GetListaElementosPorIDCampo<CvnItemBeanCvnString>("050.030.020.030");
            UtilitySecciones.CodigosUnesco(listadoCodUnescoSecundaria, entidadAux, Variables.ExperienciaCientificaTecnologica.resultadosTecnologicosCodUnescoSecundaria);

            //Añado los códigos UNESCO de especialización terciaria
            List<CvnItemBeanCvnString> listadoCodUnescoTerciaria = item.GetListaElementosPorIDCampo<CvnItemBeanCvnString>("050.030.020.040");
            UtilitySecciones.CodigosUnesco(listadoCodUnescoTerciaria, entidadAux, Variables.ExperienciaCientificaTecnologica.resultadosTecnologicosCodUnescoTerciaria);
        }

        /// <summary>
        /// Inserta en <paramref name="entidadAux"/> los valores de <paramref name="item"/>,
        /// pertenecientes a las palabras clave.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="entidadAux"></param>
        private void ResultadosTecnologicosPalabrasClave(CvnItemBean item, Entity entidadAux)
        {
            List<CvnItemBeanCvnString> listadoPalabrasClave = item.GetListaElementosPorIDCampo<CvnItemBeanCvnString>("050.030.020.280");

            string propiedadPalabrasClave = Variables.ExperienciaCientificaTecnologica.resultadosTecnologicosPalabrasClave;

            foreach (CvnItemBeanCvnString palabraClave in listadoPalabrasClave)
            {
                string entityPartAux = Guid.NewGuid().ToString() + "@@@";

                List<string> listadoPalabras = Utility.GetPadresPalabrasClave(palabraClave);
                foreach (string palabra in listadoPalabras)
                {
                    Property propertyPalabrasClave = entidadAux.properties.FirstOrDefault(x => x.prop == Variables.ExperienciaCientificaTecnologica.resultadosTecnologicosPalabrasClave);
                    UtilitySecciones.CheckProperty(propertyPalabrasClave, entidadAux,
                        UtilitySecciones.StringGNOSSID(entityPartAux, Utility.ObtenerPalabraClave(mResourceApi, palabra)), propiedadPalabrasClave);
                }
            }
        }

        /// <summary>
        /// Inserta en <paramref name="entidadAux"/> los valores de <paramref name="item"/>,
        /// pertenecientes a los Autor/a y Corresponsable.
        /// </summary>
        /// <param name="item">item</param>
        /// <param name="entidadAux">entidadAux</param>
        private void ResultadosTecnologicosAutoresYCorresponsables(CvnItemBean item, Entity entidadAux)
        {
            //Añado el autor principal
            CvnItemBeanCvnAuthorBean autor = item.GetElementoPorIDCampo<CvnItemBeanCvnAuthorBean>("050.030.020.050");

            entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                 new Property(Variables.ExperienciaCientificaTecnologica.resultadosTecnologicosIPNombre, autor.GivenName),
                 new Property(Variables.ExperienciaCientificaTecnologica.resultadosTecnologicosIPPrimerApellido, autor.CvnFamilyNameBean?.FirstFamilyName),
                 new Property(Variables.ExperienciaCientificaTecnologica.resultadosTecnologicosIPSegundoApellido, autor.CvnFamilyNameBean?.SecondFamilyName),
                 new Property(Variables.ExperienciaCientificaTecnologica.resultadosTecnologicosIPFirma, autor.Signature.ToString())
             ));

            //Añado el corresponsable
            CvnItemBeanCvnAuthorBean corresponsable = item.GetElementoPorIDCampo<CvnItemBeanCvnAuthorBean>("050.030.020.060");

            entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                new Property(Variables.ExperienciaCientificaTecnologica.resultadosTecnologicosCoIPNombre, corresponsable.GivenName),
                new Property(Variables.ExperienciaCientificaTecnologica.resultadosTecnologicosCoIPPrimerApellido, corresponsable.CvnFamilyNameBean?.FirstFamilyName),
                new Property(Variables.ExperienciaCientificaTecnologica.resultadosTecnologicosCoIPSegundoApellido, corresponsable.CvnFamilyNameBean?.SecondFamilyName),
                new Property(Variables.ExperienciaCientificaTecnologica.resultadosTecnologicosCoIPFirma, corresponsable.Signature.ToString())
            ));

        }

        /// <summary>
        /// Inserta en <paramref name="entidadAux"/> los valores de <paramref name="item"/>,
        /// pertenecientes a las Entidades Colaboradoras.
        /// </summary>
        /// <param name="item">item</param>
        /// <param name="entidadAux">entidadAux</param>
        private void ResultadosTecnologicosEntidadColaboradora(CvnItemBean item, Entity entidadAux)//TODO
        {
            List<CvnItemBeanCvnCodeGroup> listadoEntidadColaboradora = item.GetListaElementosPorIDCampo<CvnItemBeanCvnCodeGroup>("050.030.020.170");
            foreach (CvnItemBeanCvnCodeGroup entidadColaboradora in listadoEntidadColaboradora)
            {
                Property entidad = entidadAux.properties.FirstOrDefault(x => x.prop == Variables.ExperienciaCientificaTecnologica.resultadosTecnologicosEntidadColaboradora);
                string valor = UtilitySecciones.GetOrganizacionPorNombre(mResourceApi, entidadColaboradora.GetNameEntityBeanCvnCodeGroup("050.030.020.170"));
                string propiedad = Variables.ExperienciaCientificaTecnologica.resultadosTecnologicosEntidadColaboradora;
                UtilitySecciones.CheckProperty(entidad, entidadAux, valor, propiedad);
            }
        }

        /// <summary>
        /// Inserta en <paramref name="entidadAux"/> los valores de <paramref name="item"/>,
        /// pertenecientes a las Entidades Destinatarias.
        /// </summary>
        /// <param name="item">item</param>
        /// <param name="entidadAux">entidadAux</param>
        private void ResultadosTecnologicosEntidadDestinataria(CvnItemBean item, Entity entidadAux)//TODO
        {
            List<CvnItemBeanCvnCodeGroup> listadoEntidadDestinataria = item.GetListaElementosPorIDCampo<CvnItemBeanCvnCodeGroup>("050.030.020.210");
            foreach (CvnItemBeanCvnCodeGroup entidadDestinataria in listadoEntidadDestinataria)
            {
                Property entidad = entidadAux.properties.FirstOrDefault(x => x.prop == Variables.ExperienciaCientificaTecnologica.resultadosTecnologicosEntidadDestinataria);
                string valor = UtilitySecciones.GetOrganizacionPorNombre(mResourceApi, entidadDestinataria.GetNameEntityBeanCvnCodeGroup("050.030.020.210"));
                string propiedad = Variables.ExperienciaCientificaTecnologica.resultadosTecnologicosEntidadDestinataria;
                UtilitySecciones.CheckProperty(entidad, entidadAux, valor, propiedad);
            }
        }
    }
}
