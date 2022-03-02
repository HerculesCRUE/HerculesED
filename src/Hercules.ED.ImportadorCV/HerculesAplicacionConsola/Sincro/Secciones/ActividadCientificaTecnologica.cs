using Gnoss.ApiWrapper.ApiModel;
using Gnoss.ApiWrapper.Model;
using Hercules.ED.DisambiguationEngine.Models;
using HerculesAplicacionConsola.Sincro.Secciones.ActividadCientifica;
using HerculesAplicacionConsola.Utils;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Utils;
using static Gnoss.ApiWrapper.ApiModel.SparqlObject;
using static Models.Entity;

namespace HerculesAplicacionConsola.Sincro.Secciones
{
    class ActividadCientificaTecnologica : SeccionBase
    {


        private List<CvnItemBean> listadoDatos = new List<CvnItemBean>();
        private string RdfTypeTab = "http://w3id.org/roh/ScientificActivity";
        public ActividadCientificaTecnologica(cvnRootResultBean cvn, string cvID) : base(cvn, cvID)
        {
            listadoDatos = mCvn.GetListadoBloque("060");
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
        /// Añade o modifica las propiedades de las entidades en BBDD comparandolas con las leidas del XML.
        /// </summary>
        private void AniadirModificarActividadCientifica(List<Entity> listadoAux, Dictionary<string, DisambiguableEntity> entidadesXML,
            Dictionary<string, string> equivalencias, string propTitle, string graph, string rdfType, string rdfTypePrefix,
            List<string> propiedadesItem, [Optional] string pPropertyCV, [Optional] string pRdfTypeCV)
        {
            for (int i = 0; i < listadoAux.Count; i++)
            {
                Entity entityXML = listadoAux[i];
                string idXML = entidadesXML.Keys.ToList()[i];
                string idNewEntity = "";

                if (string.IsNullOrEmpty(equivalencias[idXML]))
                {
                    //Añadir
                    entityXML.propTitle = propTitle;
                    entityXML.ontology = graph;
                    entityXML.rdfType = rdfType;
                    idNewEntity = CreateListEntityAux(mCvID, RdfTypeTab, rdfTypePrefix, propiedadesItem, entityXML);
                }
                else
                {
                    //Modificar
                    ModificarExistentes(equivalencias, idXML, graph, propTitle, entityXML);
                }

                if (string.IsNullOrEmpty(idNewEntity))
                {
                    idNewEntity = "";
                }

                if (!string.IsNullOrEmpty(pPropertyCV) && !string.IsNullOrEmpty(pRdfTypeCV))
                {
                    valoresPropertiesCV(entityXML, equivalencias, propiedadesItem, pPropertyCV, pRdfTypeCV, idXML, idNewEntity);
                }
            }
        }

        private void valoresPropertiesCV(Entity entityXML, Dictionary<string, string> equivalencias, List<string> propiedadesItem,
            string pPropertyCV, string pRdfTypeCV, string pIdXML, string idNewEntity)
        {
            string rdfTypePrefix = "";
            string entityCVID = "";
            //Si es nueva tendra valor sino traera una cadena vacia
            string idEntity = idNewEntity;

            if (entityXML.properties_cv != null && entityXML.properties_cv.Count() > 0 &&
                propiedadesItem.Count > 2 && equivalencias.ContainsKey(pIdXML))
            {

                //Obtenemos la auxiliar en la que cargar la entidad
                SparqlObject tab = mResourceApi.VirtuosoQuery("select *", "where{<" + mCvID + "> ?s ?o. ?o a <" + RdfTypeTab + "> }", "curriculumvitae");
                string idTab = tab.results.bindings[0]["o"].value;

                //Query para obtener entityID
                string select = "select distinct ?related ?relatedCV ";
                string where = $@"where {{
                                        ?s <{propiedadesItem[0]}> ?category . 
                                        ?category <{propiedadesItem[propiedadesItem.Count - 2] }> ?related . 
                                        ?related <{pPropertyCV}> ?relatedCV.
                                        ?related <http://vivoweb.org/ontology/core#relatedBy> ?item
                                        FILTER(?s=<{mCvID}>)
                                        FILTER(?item=<{equivalencias[pIdXML]}>)
                                    }}";
                SparqlObject entityIDCV = mResourceApi.VirtuosoQuery(select, where, "curriculumvitae");


                //Si no es una nueva entidad añado la referencia de la clase intermedia y de claseCV
                string idEntityCV = "";
                if (entityIDCV.results.bindings.Count > 0)
                {
                    idEntity = entityIDCV.results.bindings[0]["related"].value;
                    idEntityCV = entityIDCV.results.bindings[0]["relatedCV"].value;
                }

                //Si es nulo, genero una nueva referencia de claseCV
                if (string.IsNullOrEmpty(idEntityCV))
                {
                    rdfTypePrefix = GuardadoCV.Models.Utils.UtilityCV.AniadirPrefijo(pRdfTypeCV);
                    rdfTypePrefix = rdfTypePrefix.Substring(rdfTypePrefix.IndexOf(":") + 1);
                    entityCVID = $"{mResourceApi.GraphsUrl}items/" + rdfTypePrefix + "_" + mResourceApi.GetShortGuid(mCvID) + "_" + Guid.NewGuid();
                }
                else
                {
                    entityCVID = idEntityCV;
                }

                List<string> propertyIDs = new List<string>(propiedadesItem);
                propertyIDs.RemoveAt(propertyIDs.Count - 1);
                propertyIDs.Add(pPropertyCV);
                List<string> entityIDs = new List<string>()
                    {
                        idTab,
                        idEntity,
                        entityCVID
                    };

                Entity entityToLoad = new Entity();
                entityToLoad.id = entityCVID;
                entityToLoad.ontology = "curriculumvitae";
                entityToLoad.properties = entityXML.properties_cv;
                entityToLoad.rdfType = pRdfTypeCV;
                Entity entityBBDD = GetLoadedEntity(entityCVID, "curriculumvitae");
                UpdateEntityAux(mResourceApi.GetShortGuid(mCvID), propertyIDs, entityIDs, entityBBDD, entityToLoad);
            }
        }

        /// <summary>
        /// Metodo para sincronizar los datos pertenecientes al código de campo
        /// "Indicadores generales de calidad de la producción científica".
        /// Con el codigo identificativo 060.010.060.010
        /// </summary>
        public void SincroIndicadoresGenerales()
        {
            List<string> propiedadesItem = new List<string>() { "http://w3id.org/roh/scientificActivity", "http://w3id.org/roh/generalQualityIndicators", "http://w3id.org/roh/generalQualityIndicatorCV" };
            List<string> rdfTypeItem = new List<string>() { "http://w3id.org/roh/GeneralQualityIndicator", "http://w3id.org/roh/GeneralQualityIndicatorCV" };

            //1º Obtenemos la entidad de BBDD.
            Tuple<string, string, string> identificadores = GetIdentificadoresItemPresentation(mCvID, propiedadesItem);

            Entity entityBBDD = null;
            GetEntidadesSecundarias(ref entityBBDD, identificadores, rdfTypeItem, "curriculumvitae");

            Entity entityXML = GetIndicadoresGenerales(listadoDatos);
            UpdateEntityAux(mResourceApi.GetShortGuid(mCvID), propiedadesItem, new List<string>() { identificadores.Item1, identificadores.Item2, identificadores.Item3 }, entityBBDD, entityXML);
        }

        /// <summary>
        /// Metodo para sincronizar los datos pertenecientes al 
        /// bloque "Publicaciones, documentos científicos y técnicos".
        /// Con el codigo identificativo 060.010.010.000
        /// </summary>
        public void SincroPublicacionesDocumentos()
        {
            List<string> propiedadesItem = new List<string>() { "http://w3id.org/roh/scientificActivity", "http://w3id.org/roh/scientificPublications", "http://vivoweb.org/ontology/core#relatedBy" };
            string graph = "document";
            string propTitle = "http://w3id.org/roh/title";
            string rdfType = "http://purl.org/ontology/bibo/Document";
            string rdfTypePrefix = "RelatedScientificPublication";

            //1º Obtenemos la entidad del XML.
            List<Entity> listadoAux = GetPublicacionesDocumentos(listadoDatos);

            Dictionary<string, DisambiguableEntity> entidadesXML = new Dictionary<string, DisambiguableEntity>();
            foreach (Entity entityXML in listadoAux)
            {
                PublicacionesDocumentos publicacionesDocumentos = new PublicacionesDocumentos();
                publicacionesDocumentos.descripcion = entityXML.properties.FirstOrDefault(x => x.prop == Variables.ActividadCientificaTecnologica.pubDocumentosPubTitulo)?.values.FirstOrDefault();
                publicacionesDocumentos.fecha = entityXML.properties.FirstOrDefault(x => x.prop == Variables.ActividadCientificaTecnologica.pubDocumentosPubFecha)?.values.FirstOrDefault();
                publicacionesDocumentos.ID = Guid.NewGuid().ToString();
                entidadesXML.Add(publicacionesDocumentos.ID, publicacionesDocumentos);
            }

            //2º Obtenemos las entidades de la BBDD
            Dictionary<string, DisambiguableEntity> entidadesBBDD = PublicacionesDocumentos.GetBBDD(mResourceApi, mCvID, graph, propiedadesItem);

            //3º Comparamos las equivalentes
            Dictionary<string, string> equivalencias = Disambiguation.SimilarityBBDD(entidadesXML.Values.ToList(), entidadesBBDD.Values.ToList());

            //4º Añadimos o modificamos las entidades
            AniadirModificarActividadCientifica(listadoAux, entidadesXML, equivalencias, propTitle,
                graph, rdfType, rdfTypePrefix, propiedadesItem, "http://w3id.org/roh/relatedScientificPublicationCV", "http://w3id.org/roh/RelatedScientificPublicationCV");
        }

        /// <summary>
        /// Metodo para sincronizar los datos pertenecientes al 
        /// bloque "Trabajos presentados en congresos nacionales o internacionales".
        /// Con el codigo identificativo 060.010.020.000
        /// </summary>
        public void SincroTrabajosCongresos()
        {
            List<string> propiedadesItem = new List<string>() { "http://w3id.org/roh/scientificActivity", "http://w3id.org/roh/worksSubmittedConferences", "http://vivoweb.org/ontology/core#relatedBy" };
            string graph = "document";
            string propTitle = "http://w3id.org/roh/title";
            string rdfType = "http://purl.org/ontology/bibo/Document";
            string rdfTypePrefix = "RelatedWorkSubmittedConferences";

            //1º Obtenemos la entidad del XML.
            List<Entity> listadoAux = GetTrabajosCongresos(listadoDatos);

            Dictionary<string, DisambiguableEntity> entidadesXML = new Dictionary<string, DisambiguableEntity>();
            foreach (Entity entityXML in listadoAux)
            {
                TrabajosCongresos trabajosCongresos = new TrabajosCongresos();
                trabajosCongresos.descripcion = entityXML.properties.FirstOrDefault(x => x.prop == Variables.ActividadCientificaTecnologica.trabajosCongresosTitulo)?.values.FirstOrDefault();
                trabajosCongresos.ID = Guid.NewGuid().ToString();
                entidadesXML.Add(trabajosCongresos.ID, trabajosCongresos);
            }

            //2º Obtenemos las entidades de la BBDD
            Dictionary<string, DisambiguableEntity> entidadesBBDD = TrabajosCongresos.GetBBDD(mResourceApi, mCvID, graph, propiedadesItem);

            //3º Comparamos las equivalentes
            Dictionary<string, string> equivalencias = Disambiguation.SimilarityBBDD(entidadesXML.Values.ToList(), entidadesBBDD.Values.ToList());

            //4º Añadimos o modificamos las entidades
            AniadirModificarActividadCientifica(listadoAux, entidadesXML, equivalencias, propTitle, graph, rdfType, rdfTypePrefix, propiedadesItem);
        }

        /// <summary>
        /// Metodo para sincronizar los datos pertenecientes al 
        /// bloque "Trabajos presentados en jornadas, seminarios,
        /// talleres de trabajo y/o cursos nacionales o internacionales".
        /// Con el codigo identificativo 060.010.030.000
        /// </summary>
        public void SincroTrabajosJornadasSeminarios()
        {
            List<string> propiedadesItem = new List<string>() { "http://w3id.org/roh/scientificActivity", "http://w3id.org/roh/worksSubmittedSeminars", "http://vivoweb.org/ontology/core#relatedBy" };
            string graph = "document";
            string propTitle = "http://w3id.org/roh/title";
            string rdfType = "http://purl.org/ontology/bibo/Document";
            string rdfTypePrefix = "RelatedWorkSubmittedSeminars";

            //1º Obtenemos la entidad del XML.
            List<Entity> listadoAux = GetTrabajosJornadasSeminarios(listadoDatos);

            Dictionary<string, DisambiguableEntity> entidadesXML = new Dictionary<string, DisambiguableEntity>();
            foreach (Entity entityXML in listadoAux)
            {
                TrabajosJornadasSeminarios trabajosJornadas = new TrabajosJornadasSeminarios();
                trabajosJornadas.descripcion = entityXML.properties.FirstOrDefault(x => x.prop == Variables.ActividadCientificaTecnologica.trabajosJornSemTituloTrabajo)?.values.FirstOrDefault();
                trabajosJornadas.ID = Guid.NewGuid().ToString();
                entidadesXML.Add(trabajosJornadas.ID, trabajosJornadas);
            }

            //2º Obtenemos las entidades de la BBDD
            Dictionary<string, DisambiguableEntity> entidadesBBDD = TrabajosJornadasSeminarios.GetBBDD(mResourceApi, mCvID, graph, propiedadesItem);

            //3º Comparamos las equivalentes
            Dictionary<string, string> equivalencias = Disambiguation.SimilarityBBDD(entidadesXML.Values.ToList(), entidadesBBDD.Values.ToList());

            //4º Añadimos o modificamos las entidades
            AniadirModificarActividadCientifica(listadoAux, entidadesXML, equivalencias, propTitle, graph, rdfType, rdfTypePrefix, propiedadesItem);
        }

        /// <summary>
        /// Metodo para sincronizar los datos pertenecientes al 
        /// bloque "Otras actividades de divulgación".
        /// Con el codigo identificativo 060.010.040.000
        /// </summary>
        public void SincroOtrasActividadesDivulgacion()
        {
            List<string> propiedadesItem = new List<string>() { "http://w3id.org/roh/scientificActivity", "http://w3id.org/roh/otherDisseminationActivities", "http://vivoweb.org/ontology/core#relatedBy" };
            string graph = "document";
            string propTitle = "http://w3id.org/roh/title";
            string rdfType = "http://purl.org/ontology/bibo/Document";
            string rdfTypePrefix = "RelatedOtherDisseminationActivity";

            //1º Obtenemos la entidad del XML.
            List<Entity> listadoAux = GetOtrasActividadesDivulgacion(listadoDatos);

            Dictionary<string, DisambiguableEntity> entidadesXML = new Dictionary<string, DisambiguableEntity>();
            foreach (Entity entityXML in listadoAux)
            {
                OtrasActividadesDivulgacion otrasActividadesDivulgacion = new OtrasActividadesDivulgacion();
                otrasActividadesDivulgacion.descripcion = entityXML.properties.FirstOrDefault(x => x.prop == Variables.ActividadCientificaTecnologica.otrasActDivulTitulo)?.values.FirstOrDefault();
                otrasActividadesDivulgacion.fecha = entityXML.properties.FirstOrDefault(x => x.prop == Variables.ActividadCientificaTecnologica.otrasActDivulPubFecha)?.values.FirstOrDefault();
                otrasActividadesDivulgacion.ID = Guid.NewGuid().ToString();
                entidadesXML.Add(otrasActividadesDivulgacion.ID, otrasActividadesDivulgacion);
            }

            //2º Obtenemos las entidades de la BBDD
            Dictionary<string, DisambiguableEntity> entidadesBBDD = OtrasActividadesDivulgacion.GetBBDD(mResourceApi, mCvID, graph, propiedadesItem);

            //3º Comparamos las equivalentes
            Dictionary<string, string> equivalencias = Disambiguation.SimilarityBBDD(entidadesXML.Values.ToList(), entidadesBBDD.Values.ToList());

            //4º Añadimos o modificamos las entidades
            AniadirModificarActividadCientifica(listadoAux, entidadesXML, equivalencias, propTitle, graph, rdfType, rdfTypePrefix, propiedadesItem);
        }

        /// <summary>
        /// Metodo para sincronizar los datos pertenecientes al 
        /// bloque "Comités científicos, técnicos y/o asesores".
        /// Con el codigo identificativo 060.020.010.000
        /// </summary>
        public void SincroComitesCTA()
        {
            List<string> propiedadesItem = new List<string>() { "http://w3id.org/roh/scientificActivity", "http://w3id.org/roh/committees", "http://vivoweb.org/ontology/core#relatedBy" };
            string graph = "committee";
            string propTitle = "http://w3id.org/roh/title";
            string rdfType = "http://w3id.org/roh/Committee";
            string rdfTypePrefix = "RelatedCommittee";

            //1º Obtenemos la entidad del XML.
            List<Entity> listadoAux = GetComitesCTA(listadoDatos);

            Dictionary<string, DisambiguableEntity> entidadesXML = new Dictionary<string, DisambiguableEntity>();
            foreach (Entity entityXML in listadoAux)
            {
                ComitesCTA comitesCTA = new ComitesCTA();
                comitesCTA.descripcion = entityXML.properties.FirstOrDefault(x => x.prop == Variables.ActividadCientificaTecnologica.comitesCTATitulo)?.values.FirstOrDefault();
                comitesCTA.fecha = entityXML.properties.FirstOrDefault(x => x.prop == Variables.ActividadCientificaTecnologica.comitesCTAFechaInicio)?.values.FirstOrDefault();
                comitesCTA.ID = Guid.NewGuid().ToString();
                entidadesXML.Add(comitesCTA.ID, comitesCTA);
            }

            //2º Obtenemos las entidades de la BBDD
            Dictionary<string, DisambiguableEntity> entidadesBBDD = ComitesCTA.GetBBDD(mResourceApi, mCvID, graph, propiedadesItem);

            //3º Comparamos las equivalentes
            Dictionary<string, string> equivalencias = Disambiguation.SimilarityBBDD(entidadesXML.Values.ToList(), entidadesBBDD.Values.ToList());

            //4º Añadimos o modificamos las entidades
            AniadirModificarActividadCientifica(listadoAux, entidadesXML, equivalencias, propTitle, graph, rdfType, rdfTypePrefix, propiedadesItem);
        }

        /// <summary>
        /// Metodo para sincronizar los datos pertenecientes al 
        /// bloque "Organización de actividades de I+D+i".
        /// Con el codigo identificativo 060.020.030.000
        /// </summary>
        public void SincroOrganizacionIDI()
        {
            List<string> propiedadesItem = new List<string>() { "http://w3id.org/roh/scientificActivity", "http://w3id.org/roh/activitiesOrganization", "http://vivoweb.org/ontology/core#relatedBy" };
            string graph = "activity";
            string propTitle = "http://w3id.org/roh/title";
            string rdfType = "http://w3id.org/roh/Activity";
            string rdfTypePrefix = "RelatedActivityOrganization";

            //1º Obtenemos la entidad del XML.
            List<Entity> listadoAux = GetOrganizacionIDI(listadoDatos);

            Dictionary<string, DisambiguableEntity> entidadesXML = new Dictionary<string, DisambiguableEntity>();
            foreach (Entity entityXML in listadoAux)
            {
                OrganizacionesIDI organizacionesIDI = new OrganizacionesIDI();
                organizacionesIDI.descripcion = entityXML.properties.FirstOrDefault(x => x.prop == Variables.ActividadCientificaTecnologica.orgIDITituloActividad)?.values.FirstOrDefault();
                organizacionesIDI.tipoActividad = entityXML.properties.FirstOrDefault(x => x.prop == Variables.ActividadCientificaTecnologica.orgIDITipoActividad)?.values.FirstOrDefault();
                organizacionesIDI.entidadConvocante = entityXML.properties.FirstOrDefault(x => x.prop == Variables.ActividadCientificaTecnologica.orgIDIEntidadConvocanteNombre)?.values.FirstOrDefault();
                organizacionesIDI.fecha = entityXML.properties.FirstOrDefault(x => x.prop == Variables.ActividadCientificaTecnologica.orgIDIFechaInicio)?.values.FirstOrDefault();
                organizacionesIDI.ID = Guid.NewGuid().ToString();
                entidadesXML.Add(organizacionesIDI.ID, organizacionesIDI);
            }

            //2º Obtenemos las entidades de la BBDD
            Dictionary<string, DisambiguableEntity> entidadesBBDD = OrganizacionesIDI.GetBBDD(mResourceApi, mCvID, graph, propiedadesItem);

            //3º Comparamos las equivalentes
            Dictionary<string, string> equivalencias = Disambiguation.SimilarityBBDD(entidadesXML.Values.ToList(), entidadesBBDD.Values.ToList());

            //4º Añadimos o modificamos las entidades
            AniadirModificarActividadCientifica(listadoAux, entidadesXML, equivalencias, propTitle, graph, rdfType, rdfTypePrefix, propiedadesItem);
        }

        /// <summary>
        /// Metodo para sincronizar los datos pertenecientes al 
        /// bloque "Gestión de I+D+i".
        /// Con el codigo identificativo 060.020.040.000
        /// </summary>
        public void SincroGestionIDI()
        {
            List<string> propiedadesItem = new List<string>() { "http://w3id.org/roh/scientificActivity", "http://w3id.org/roh/activitiesManagement", "http://vivoweb.org/ontology/core#relatedBy" };
            string graph = "activity";
            string propTitle = "http://w3id.org/roh/title";
            string rdfType = "http://w3id.org/roh/Activity";
            string rdfTypePrefix = "RelatedActivityManagement";

            //1º Obtenemos la entidad del XML.
            List<Entity> listadoAux = GetGestionIDI(listadoDatos);

            Dictionary<string, DisambiguableEntity> entidadesXML = new Dictionary<string, DisambiguableEntity>();
            foreach (Entity entityXML in listadoAux)
            {
                GestionIDI gestionIDI = new GestionIDI();
                gestionIDI.descripcion = entityXML.properties.FirstOrDefault(x => x.prop == Variables.ActividadCientificaTecnologica.gestionIDINombreActividad)?.values.FirstOrDefault();
                gestionIDI.entidadRealizacion = entityXML.properties.FirstOrDefault(x => x.prop == Variables.ActividadCientificaTecnologica.gestionIDIEntornoEntidadRealizacionNombre)?.values.FirstOrDefault();
                gestionIDI.funciones = entityXML.properties.FirstOrDefault(x => x.prop == Variables.ActividadCientificaTecnologica.gestionIDIFunciones)?.values.FirstOrDefault();
                gestionIDI.fecha = entityXML.properties.FirstOrDefault(x => x.prop == Variables.ActividadCientificaTecnologica.gestionIDIEntornoFechaInicio)?.values.FirstOrDefault();
                gestionIDI.ID = Guid.NewGuid().ToString();
                entidadesXML.Add(gestionIDI.ID, gestionIDI);
            }

            //2º Obtenemos las entidades de la BBDD
            Dictionary<string, DisambiguableEntity> entidadesBBDD = GestionIDI.GetBBDD(mResourceApi, mCvID, graph, propiedadesItem);

            //3º Comparamos las equivalentes
            Dictionary<string, string> equivalencias = Disambiguation.SimilarityBBDD(entidadesXML.Values.ToList(), entidadesBBDD.Values.ToList());

            //4º Añadimos o modificamos las entidades
            AniadirModificarActividadCientifica(listadoAux, entidadesXML, equivalencias, propTitle, graph, rdfType, rdfTypePrefix, propiedadesItem);
        }

        /// <summary>
        /// Metodo para sincronizar los datos pertenecientes al 
        /// bloque "Foros y comités nacionales e internacionales".
        /// Con el codigo identificativo 060.020.050.000
        /// </summary>
        public void SincroForosComites()
        {
            List<string> propiedadesItem = new List<string>() { "http://w3id.org/roh/scientificActivity", "http://w3id.org/roh/forums", "http://vivoweb.org/ontology/core#relatedBy" };
            string graph = "activity";
            string propTitle = "http://w3id.org/roh/title";
            string rdfType = "http://w3id.org/roh/Activity";
            string rdfTypePrefix = "RelatedForum";

            //1º Obtenemos la entidad del XML.
            List<Entity> listadoAux = GetForosComites(listadoDatos);

            Dictionary<string, DisambiguableEntity> entidadesXML = new Dictionary<string, DisambiguableEntity>();
            foreach (Entity entityXML in listadoAux)
            {
                ForosComites forosComites = new ForosComites();
                forosComites.descripcion = entityXML.properties.FirstOrDefault(x => x.prop == Variables.ActividadCientificaTecnologica.forosComitesNombre)?.values.FirstOrDefault();
                forosComites.categoriaProfesional = entityXML.properties.FirstOrDefault(x => x.prop == Variables.ActividadCientificaTecnologica.forosComitesCategoriaProfesional)?.values.FirstOrDefault();
                forosComites.ID = Guid.NewGuid().ToString();
                entidadesXML.Add(forosComites.ID, forosComites);
            }

            //2º Obtenemos las entidades de la BBDD
            Dictionary<string, DisambiguableEntity> entidadesBBDD = ForosComites.GetBBDD(mResourceApi, mCvID, graph, propiedadesItem);

            //3º Comparamos las equivalentes
            Dictionary<string, string> equivalencias = Disambiguation.SimilarityBBDD(entidadesXML.Values.ToList(), entidadesBBDD.Values.ToList());

            //4º Añadimos o modificamos las entidades
            AniadirModificarActividadCientifica(listadoAux, entidadesXML, equivalencias, propTitle, graph, rdfType, rdfTypePrefix, propiedadesItem);
        }

        /// <summary>
        /// Metodo para sincronizar los datos pertenecientes al 
        /// bloque "Evaluación y revisión de proyectos y artículos de I+D+i".
        /// Con el codigo identificativo 060.020.060.000
        /// </summary>
        public void SincroEvalRevIDI()
        {
            List<string> propiedadesItem = new List<string>() { "http://w3id.org/roh/scientificActivity", "http://w3id.org/roh/researchEvaluations", "http://vivoweb.org/ontology/core#relatedBy" };
            string graph = "activity";
            string propTitle = "http://w3id.org/roh/functions";
            string rdfType = "http://w3id.org/roh/Activity";
            string rdfTypePrefix = "RelatedResearchEvaluation";

            //1º Obtenemos la entidad del XML.
            List<Entity> listadoAux = GetEvalRevIDI(listadoDatos);

            Dictionary<string, DisambiguableEntity> entidadesXML = new Dictionary<string, DisambiguableEntity>();
            foreach (Entity entityXML in listadoAux)
            {
                EvalRevIDI evalRevIDI = new EvalRevIDI();
                evalRevIDI.descripcion = entityXML.properties.FirstOrDefault(x => x.prop == Variables.ActividadCientificaTecnologica.evalRevIDIFunciones)?.values.FirstOrDefault();
                evalRevIDI.nombreActividad = entityXML.properties.FirstOrDefault(x => x.prop == Variables.ActividadCientificaTecnologica.evalRevIDINombre)?.values.FirstOrDefault();
                evalRevIDI.entidadRealizacion = entityXML.properties.FirstOrDefault(x => x.prop == Variables.ActividadCientificaTecnologica.evalRevIDIEntidadNombre)?.values.FirstOrDefault();
                evalRevIDI.fechaInicio = entityXML.properties.FirstOrDefault(x => x.prop == Variables.ActividadCientificaTecnologica.evalRevIDIFechaInicio)?.values.FirstOrDefault();
                evalRevIDI.ID = Guid.NewGuid().ToString();
                entidadesXML.Add(evalRevIDI.ID, evalRevIDI);
            }

            //2º Obtenemos las entidades de la BBDD
            Dictionary<string, DisambiguableEntity> entidadesBBDD = EvalRevIDI.GetBBDD(mResourceApi, mCvID, graph, propiedadesItem);

            //3º Comparamos las equivalentes
            Dictionary<string, string> equivalencias = Disambiguation.SimilarityBBDD(entidadesXML.Values.ToList(), entidadesBBDD.Values.ToList());

            //4º Añadimos o modificamos las entidades
            AniadirModificarActividadCientifica(listadoAux, entidadesXML, equivalencias, propTitle, graph, rdfType, rdfTypePrefix, propiedadesItem);
        }

        /// <summary>
        /// Metodo para sincronizar los datos pertenecientes al 
        /// bloque "Estancias en centros de I+D+i públicos o privados".
        /// Con el codigo identificativo 060.010.050.000
        /// </summary>
        public void SincroEstanciasIDI()
        {
            List<string> propiedadesItem = new List<string>() { "http://w3id.org/roh/scientificActivity", "http://w3id.org/roh/stays", "http://vivoweb.org/ontology/core#relatedBy" };
            string graph = "stay";
            string propTitle = "http://w3id.org/roh/performedTasks";
            string rdfType = "http://w3id.org/roh/Stay";
            string rdfTypePrefix = "RelatedStay";

            //1º Obtenemos la entidad del XML.
            List<Entity> listadoAux = GetEstanciasIDI(listadoDatos);

            Dictionary<string, DisambiguableEntity> entidadesXML = new Dictionary<string, DisambiguableEntity>();
            foreach (Entity entityXML in listadoAux)
            {
                EstanciasIDI estanciasIDI = new EstanciasIDI();
                estanciasIDI.descripcion = entityXML.properties.FirstOrDefault(x => x.prop == Variables.ActividadCientificaTecnologica.estanciasIDITareasContrastables)?.values.FirstOrDefault();
                estanciasIDI.fecha = entityXML.properties.FirstOrDefault(x => x.prop == Variables.ActividadCientificaTecnologica.estanciasIDIFechaInicioEntidadRealizacion)?.values.FirstOrDefault();
                estanciasIDI.entidadRealizacion = entityXML.properties.FirstOrDefault(x => x.prop == Variables.ActividadCientificaTecnologica.estanciasIDIEntidadRealizacionNombre)?.values.FirstOrDefault();
                estanciasIDI.ID = Guid.NewGuid().ToString();
                entidadesXML.Add(estanciasIDI.ID, estanciasIDI);
            }

            //2º Obtenemos las entidades de la BBDD
            Dictionary<string, DisambiguableEntity> entidadesBBDD = EstanciasIDI.GetBBDD(mResourceApi, mCvID, graph, propiedadesItem);

            //3º Comparamos las equivalentes
            Dictionary<string, string> equivalencias = Disambiguation.SimilarityBBDD(entidadesXML.Values.ToList(), entidadesBBDD.Values.ToList());

            //4º Añadimos o modificamos las entidades
            AniadirModificarActividadCientifica(listadoAux, entidadesXML, equivalencias, propTitle, graph, rdfType, rdfTypePrefix, propiedadesItem);
        }

        /// <summary>
        /// Metodo para sincronizar los datos pertenecientes al 
        /// bloque "Ayudas y becas obtenidas".
        /// Con el codigo identificativo 060.030.010.000
        /// </summary>
        public void SincroAyudasBecas()
        {
            List<string> propiedadesItem = new List<string>() { "http://w3id.org/roh/scientificActivity", "http://w3id.org/roh/grants", "http://vivoweb.org/ontology/core#relatedBy" };
            string graph = "grant";
            string propTitle = "http://w3id.org/roh/title";
            string rdfType = "http://vivoweb.org/ontology/core#Grant";
            string rdfTypePrefix = "RelatedGrant";

            //1º Obtenemos la entidad del XML.
            List<Entity> listadoAux = GetAyudasBecas(listadoDatos);

            Dictionary<string, DisambiguableEntity> entidadesXML = new Dictionary<string, DisambiguableEntity>();
            foreach (Entity entityXML in listadoAux)
            {
                AyudaBecas ayudaBecas = new AyudaBecas();
                ayudaBecas.descripcion = entityXML.properties.FirstOrDefault(x => x.prop == Variables.ActividadCientificaTecnologica.ayudasBecasNombre)?.values.FirstOrDefault();
                ayudaBecas.fecha = entityXML.properties.FirstOrDefault(x => x.prop == Variables.ActividadCientificaTecnologica.ayudasBecasFechaConcesion)?.values.FirstOrDefault();
                ayudaBecas.entidadConcesionaria = entityXML.properties.FirstOrDefault(x => x.prop == Variables.ActividadCientificaTecnologica.ayudasBecasEntidadConcedeNombre)?.values.FirstOrDefault();
                ayudaBecas.ID = Guid.NewGuid().ToString();
                entidadesXML.Add(ayudaBecas.ID, ayudaBecas);
            }

            //2º Obtenemos las entidades de la BBDD
            Dictionary<string, DisambiguableEntity> entidadesBBDD = AyudaBecas.GetBBDD(mResourceApi, mCvID, graph, propiedadesItem);

            //3º Comparamos las equivalentes
            Dictionary<string, string> equivalencias = Disambiguation.SimilarityBBDD(entidadesXML.Values.ToList(), entidadesBBDD.Values.ToList());

            //4º Añadimos o modificamos las entidades
            AniadirModificarActividadCientifica(listadoAux, entidadesXML, equivalencias, propTitle, graph, rdfType, rdfTypePrefix, propiedadesItem);
        }

        /// <summary>
        /// Metodo para sincronizar los datos pertenecientes al 
        /// bloque "Otros modos de colaboración con investigadores/as o tecnólogos/as".
        /// Con el codigo identificativo 060.020.020.000
        /// </summary>
        public void SincroOtrosModosColaboracion()
        {
            List<string> propiedadesItem = new List<string>() { "http://w3id.org/roh/scientificActivity", "http://w3id.org/roh/otherCollaborations", "http://vivoweb.org/ontology/core#relatedBy" };
            string graph = "collaboration";
            string propTitle = "http://w3id.org/roh/title";
            string rdfType = "http://w3id.org/roh/Collaboration";
            string rdfTypePrefix = "RelatedOtherCollaboration";

            //1º Obtenemos la entidad del XML.
            List<Entity> listadoAux = GetOtrosModosColaboracion(listadoDatos);

            Dictionary<string, DisambiguableEntity> entidadesXML = new Dictionary<string, DisambiguableEntity>();
            foreach (Entity entityXML in listadoAux)
            {
                OtrosModosColaboracion otrosModosColaboracion = new OtrosModosColaboracion();
                otrosModosColaboracion.descripcion = entityXML.properties.FirstOrDefault(x => x.prop == Variables.ActividadCientificaTecnologica.otrasColabDescripcionColaboracion)?.values.FirstOrDefault();
                otrosModosColaboracion.fecha = entityXML.properties.FirstOrDefault(x => x.prop == Variables.ActividadCientificaTecnologica.otrasColabFechaInicio)?.values.FirstOrDefault();
                otrosModosColaboracion.ID = Guid.NewGuid().ToString();
                entidadesXML.Add(otrosModosColaboracion.ID, otrosModosColaboracion);
            }

            //2º Obtenemos las entidades de la BBDD
            Dictionary<string, DisambiguableEntity> entidadesBBDD = OtrosModosColaboracion.GetBBDD(mResourceApi, mCvID, graph, propiedadesItem);

            //3º Comparamos las equivalentes
            Dictionary<string, string> equivalencias = Disambiguation.SimilarityBBDD(entidadesXML.Values.ToList(), entidadesBBDD.Values.ToList());

            //4º Añadimos o modificamos las entidades
            AniadirModificarActividadCientifica(listadoAux, entidadesXML, equivalencias, propTitle, graph, rdfType, rdfTypePrefix, propiedadesItem);
        }

        /// <summary>
        /// Metodo para sincronizar los datos pertenecientes al 
        /// bloque "Sociedades científicas y asociaciones profesionales".
        /// Con el codigo identificativo 060.030.020.000
        /// </summary>
        public void SincroSociedadesAsociaciones()
        {
            List<string> propiedadesItem = new List<string>() { "http://w3id.org/roh/scientificActivity", "http://w3id.org/roh/societies", "http://vivoweb.org/ontology/core#relatedBy" };
            string graph = "society";
            string propTitle = "http://w3id.org/roh/title";
            string rdfType = "http://w3id.org/roh/Society";
            string rdfTypePrefix = "RelatedSociety";

            //1º Obtenemos la entidad del XML.
            List<Entity> listadoAux = GetSociedadesAsociaciones(listadoDatos);

            Dictionary<string, DisambiguableEntity> entidadesXML = new Dictionary<string, DisambiguableEntity>();
            foreach (Entity entityXML in listadoAux)
            {
                SociedadesAsociaciones sociedadesAsociaciones = new SociedadesAsociaciones();
                sociedadesAsociaciones.descripcion = entityXML.properties.FirstOrDefault(x => x.prop == Variables.ActividadCientificaTecnologica.sociedadesNombre)?.values.FirstOrDefault();
                sociedadesAsociaciones.fecha = entityXML.properties.FirstOrDefault(x => x.prop == Variables.ActividadCientificaTecnologica.sociedadesFechaInicio)?.values.FirstOrDefault();
                sociedadesAsociaciones.entidadAfiliacion = entityXML.properties.FirstOrDefault(x => x.prop == Variables.ActividadCientificaTecnologica.sociedadesEntidadAfiliacionNombre)?.values.FirstOrDefault();
                sociedadesAsociaciones.ID = Guid.NewGuid().ToString();
                entidadesXML.Add(sociedadesAsociaciones.ID, sociedadesAsociaciones);
            }

            //2º Obtenemos las entidades de la BBDD
            Dictionary<string, DisambiguableEntity> entidadesBBDD = SociedadesAsociaciones.GetBBDD(mResourceApi, mCvID, graph, propiedadesItem);

            //3º Comparamos las equivalentes
            Dictionary<string, string> equivalencias = Disambiguation.SimilarityBBDD(entidadesXML.Values.ToList(), entidadesBBDD.Values.ToList());

            //4º Añadimos o modificamos las entidades
            AniadirModificarActividadCientifica(listadoAux, entidadesXML, equivalencias, propTitle, graph, rdfType, rdfTypePrefix, propiedadesItem);
        }

        /// <summary>
        /// Metodo para sincronizar los datos pertenecientes al 
        /// bloque "Consejos editoriales".
        /// Con el codigo identificativo 060.030.030.000
        /// </summary>
        public void SincroConsejos()
        {
            List<string> propiedadesItem = new List<string>() { "http://w3id.org/roh/scientificActivity", "http://w3id.org/roh/councils", "http://vivoweb.org/ontology/core#relatedBy" };
            string graph = "council";
            string propTitle = "http://w3id.org/roh/title";
            string rdfType = "http://w3id.org/roh/Council";
            string rdfTypePrefix = "RelatedCouncil";

            //1º Obtenemos la entidad del XML.
            List<Entity> listadoAux = GetConsejos(listadoDatos);

            Dictionary<string, DisambiguableEntity> entidadesXML = new Dictionary<string, DisambiguableEntity>();
            foreach (Entity entityXML in listadoAux)
            {
                Consejos consejos = new Consejos();
                consejos.descripcion = entityXML.properties.FirstOrDefault(x => x.prop == Variables.ActividadCientificaTecnologica.consejosNombre)?.values.FirstOrDefault();
                consejos.EntAfi = entityXML.properties.FirstOrDefault(x => x.prop == Variables.ActividadCientificaTecnologica.consejosEntidadAfiliacionNombre)?.values.FirstOrDefault();
                consejos.ID = Guid.NewGuid().ToString();
                entidadesXML.Add(consejos.ID, consejos);
            }

            //2º Obtenemos las entidades de la BBDD
            Dictionary<string, DisambiguableEntity> entidadesBBDD = Consejos.GetBBDD(mResourceApi, mCvID, graph, propiedadesItem);

            //3º Comparamos las equivalentes
            Dictionary<string, string> equivalencias = Disambiguation.SimilarityBBDD(entidadesXML.Values.ToList(), entidadesBBDD.Values.ToList());

            //4º Añadimos o modificamos las entidades
            AniadirModificarActividadCientifica(listadoAux, entidadesXML, equivalencias, propTitle, graph, rdfType, rdfTypePrefix, propiedadesItem);
        }

        /// <summary>
        /// Metodo para sincronizar los datos pertenecientes al 
        /// bloque "Redes de cooperación".
        /// Con el codigo identificativo 060.030.040.000
        /// </summary>
        public void SincroRedesCooperacion()
        {
            List<string> propiedadesItem = new List<string>() { "http://w3id.org/roh/scientificActivity", "http://w3id.org/roh/networks", "http://vivoweb.org/ontology/core#relatedBy" };
            string graph = "network";
            string propTitle = "http://w3id.org/roh/title";
            string rdfType = "http://w3id.org/roh/Network";
            string rdfTypePrefix = "RelatedNetwork";

            //1º Obtenemos la entidad del XML.
            List<Entity> listadoAux = GetRedesCooperacion(listadoDatos);

            Dictionary<string, DisambiguableEntity> entidadesXML = new Dictionary<string, DisambiguableEntity>();
            foreach (Entity entityXML in listadoAux)
            {
                RedesCooperacion redesCooperacion = new RedesCooperacion();
                redesCooperacion.descripcion = entityXML.properties.FirstOrDefault(x => x.prop == Variables.ActividadCientificaTecnologica.redesCoopNombre)?.values.FirstOrDefault();
                redesCooperacion.IdRed = entityXML.properties.FirstOrDefault(x => x.prop == Variables.ActividadCientificaTecnologica.redesCoopIdentificacion)?.values.FirstOrDefault();
                redesCooperacion.ID = Guid.NewGuid().ToString();
                entidadesXML.Add(redesCooperacion.ID, redesCooperacion);
            }

            //2º Obtenemos las entidades de la BBDD
            Dictionary<string, DisambiguableEntity> entidadesBBDD = RedesCooperacion.GetBBDD(mResourceApi, mCvID, graph, propiedadesItem);

            //3º Comparamos las equivalentes
            Dictionary<string, string> equivalencias = Disambiguation.SimilarityBBDD(entidadesXML.Values.ToList(), entidadesBBDD.Values.ToList());

            //4º Añadimos o modificamos las entidades
            AniadirModificarActividadCientifica(listadoAux, entidadesXML, equivalencias, propTitle, graph, rdfType, rdfTypePrefix, propiedadesItem);
        }

        /// <summary>
        /// Metodo para sincronizar los datos pertenecientes al 
        /// bloque "Premios, menciones y distinciones".
        /// Con el codigo identificativo 060.030.050.000
        /// </summary>
        public void SincroPremiosMenciones()
        {
            List<string> propiedadesItem = new List<string>() { "http://w3id.org/roh/scientificActivity", "http://w3id.org/roh/prizes", "http://vivoweb.org/ontology/core#relatedBy" };
            string graph = "accreditation";
            string propTitle = "http://w3id.org/roh/title";
            string rdfType = "http://w3id.org/roh/Accreditation";
            string rdfTypePrefix = "RelatedPrize";

            //1º Obtenemos la entidad del XML.
            List<Entity> listadoAux = GetPremiosMenciones(listadoDatos);

            Dictionary<string, DisambiguableEntity> entidadesXML = new Dictionary<string, DisambiguableEntity>();
            foreach (Entity entityXML in listadoAux)
            {
                PremiosMenciones premiosMenciones = new PremiosMenciones();
                premiosMenciones.descripcion = entityXML.properties.FirstOrDefault(x => x.prop == Variables.ActividadCientificaTecnologica.premiosMencionesDescripcion)?.values.FirstOrDefault();
                premiosMenciones.fecha = entityXML.properties.FirstOrDefault(x => x.prop == Variables.ActividadCientificaTecnologica.premiosMencionesFechaConcesion)?.values.FirstOrDefault();
                premiosMenciones.ID = Guid.NewGuid().ToString();
                entidadesXML.Add(premiosMenciones.ID, premiosMenciones);
            }

            //2º Obtenemos las entidades de la BBDD
            Dictionary<string, DisambiguableEntity> entidadesBBDD = PremiosMenciones.GetBBDD(mResourceApi, mCvID, graph, propiedadesItem);

            //3º Comparamos las equivalentes
            Dictionary<string, string> equivalencias = Disambiguation.SimilarityBBDD(entidadesXML.Values.ToList(), entidadesBBDD.Values.ToList());

            //4º Añadimos o modificamos las entidades
            AniadirModificarActividadCientifica(listadoAux, entidadesXML, equivalencias, propTitle, graph, rdfType, rdfTypePrefix, propiedadesItem);
        }

        /// <summary>
        /// Metodo para sincronizar los datos pertenecientes al 
        /// bloque "Otras distinciones (carrera profesional y/o empresarial)".
        /// Con codigo identificativo 060.030.060.000
        /// </summary>
        public void SincroOtrasDistinciones()
        {
            List<string> propiedadesItem = new List<string>() { "http://w3id.org/roh/scientificActivity", "http://w3id.org/roh/otherDistinctions", "http://vivoweb.org/ontology/core#relatedBy" };
            string graph = "accreditation";
            string propTitle = "http://w3id.org/roh/title";
            string rdfType = "http://w3id.org/roh/Accreditation";
            string rdfTypePrefix = "RelatedOtherDistinction";

            //1º Obtenemos la entidad del XML.
            List<Entity> listadoAux = GetOtrasDistinciones(listadoDatos);

            Dictionary<string, DisambiguableEntity> entidadesXML = new Dictionary<string, DisambiguableEntity>();
            foreach (Entity entityXML in listadoAux)
            {
                OtrasDistinciones otrasDistinciones = new OtrasDistinciones();
                otrasDistinciones.descripcion = entityXML.properties.FirstOrDefault(x => x.prop == Variables.ActividadCientificaTecnologica.otrasDistincionesDescripcion)?.values.FirstOrDefault();
                otrasDistinciones.fecha = entityXML.properties.FirstOrDefault(x => x.prop == Variables.ActividadCientificaTecnologica.otrasDistincionesFechaConcesion)?.values.FirstOrDefault();
                otrasDistinciones.ID = Guid.NewGuid().ToString();
                entidadesXML.Add(otrasDistinciones.ID, otrasDistinciones);
            }

            //2º Obtenemos las entidades de la BBDD
            Dictionary<string, DisambiguableEntity> entidadesBBDD = OtrasDistinciones.GetBBDD(mResourceApi, mCvID, graph, propiedadesItem);

            //3º Comparamos las equivalentes
            Dictionary<string, string> equivalencias = Disambiguation.SimilarityBBDD(entidadesXML.Values.ToList(), entidadesBBDD.Values.ToList());

            //4º Añadimos o modificamos las entidades
            AniadirModificarActividadCientifica(listadoAux, entidadesXML, equivalencias, propTitle, graph, rdfType, rdfTypePrefix, propiedadesItem);
        }

        /// <summary>
        /// Metodo para sincronizar los datos pertenecientes al 
        /// bloque "Períodos de actividad investigadora".
        /// Con codigo identificativo 060.030.070.000
        /// </summary>
        public void SincroPeriodosActividad()
        {
            List<string> propiedadesItem = new List<string>() { "http://w3id.org/roh/scientificActivity", "http://w3id.org/roh/researchActivityPeriods", "http://vivoweb.org/ontology/core#relatedBy" };
            string graph = "accreditation";
            string propTitle = "http://w3id.org/roh/recognizedPeriods";
            string rdfType = "http://w3id.org/roh/Accreditation";
            string rdfTypePrefix = "RelatedResearchActivityPeriod";

            //1º Obtenemos la entidad de los datos del XML.
            List<Entity> listadoAux = GetPeriodosActividadInvestigadora(listadoDatos);


            Dictionary<string, DisambiguableEntity> entidadesXML = new Dictionary<string, DisambiguableEntity>();
            foreach (Entity entityXML in listadoAux)
            {
                PeriodosActividad periodosActividad = new PeriodosActividad();
                periodosActividad.numTramos = entityXML.properties.FirstOrDefault(x => x.prop == Variables.ActividadCientificaTecnologica.actividadInvestigadoraNumeroTramos)?.values.FirstOrDefault();
                periodosActividad.fecha = entityXML.properties.FirstOrDefault(x => x.prop == Variables.ActividadCientificaTecnologica.actividadInvestigadoraFechaObtencion)?.values.FirstOrDefault();
                periodosActividad.entidadAcreditante = entityXML.properties.FirstOrDefault(x => x.prop == Variables.ActividadCientificaTecnologica.actividadInvestigadoraEntidadNombre)?.values.FirstOrDefault();
                periodosActividad.ID = Guid.NewGuid().ToString();
                entidadesXML.Add(periodosActividad.ID, periodosActividad);
            }

            //2º Obtenemos las entidades de la BBDD
            Dictionary<string, DisambiguableEntity> entidadesBBDD = PeriodosActividad.GetBBDD(mResourceApi, mCvID, graph, propiedadesItem);

            //3º Comparamos las equivalentes
            Dictionary<string, string> equivalencias = Disambiguation.SimilarityBBDD(entidadesXML.Values.ToList(), entidadesBBDD.Values.ToList());

            //4º Añadimos o modificamos las entidades
            AniadirModificarActividadCientifica(listadoAux, entidadesXML, equivalencias, propTitle, graph, rdfType, rdfTypePrefix, propiedadesItem);
        }

        /// <summary>
        /// Metodo para sincronizar los datos pertenecientes al 
        /// bloque "Acreditaciones/reconocimientos obtenidos".
        /// Con codigo identificativo 060.030.090.000
        /// </summary>
        public void SincroAcreditacionesObtenidas()
        {
            List<string> propiedadesItem = new List<string>() { "http://w3id.org/roh/scientificActivity", "http://w3id.org/roh/obtainedRecognitions", "http://vivoweb.org/ontology/core#relatedBy" };
            string graph = "accreditation";
            string propTitle = "http://w3id.org/roh/title";
            string rdfType = "http://w3id.org/roh/Accreditation";
            string rdfTypePrefix = "RelatedObtainedRecognition";

            //1º Obtenemos la entidad del XML.
            List<Entity> listadoAux = GetAcreditacionesObtenidas(listadoDatos);

            Dictionary<string, DisambiguableEntity> entidadesXML = new Dictionary<string, DisambiguableEntity>();
            foreach (Entity entityXML in listadoAux)
            {
                AcreditacionesReconocimientos acreditacionesReconocimientos = new AcreditacionesReconocimientos();
                acreditacionesReconocimientos.descripcion = entityXML.properties.FirstOrDefault(x => x.prop == Variables.ActividadCientificaTecnologica.acreditacionesDescripcion)?.values.FirstOrDefault();
                acreditacionesReconocimientos.fecha = entityXML.properties.FirstOrDefault(x => x.prop == Variables.ActividadCientificaTecnologica.acreditacionesFechaObtencion)?.values.FirstOrDefault();
                acreditacionesReconocimientos.ID = Guid.NewGuid().ToString();
                entidadesXML.Add(acreditacionesReconocimientos.ID, acreditacionesReconocimientos);
            }

            //2º Obtenemos las entidades de la BBDD
            Dictionary<string, DisambiguableEntity> entidadesBBDD = AcreditacionesReconocimientos.GetBBDD(mResourceApi, mCvID, graph, propiedadesItem);

            //3º Comparamos las equivalentes
            Dictionary<string, string> equivalencias = Disambiguation.SimilarityBBDD(entidadesXML.Values.ToList(), entidadesBBDD.Values.ToList());

            //4º Añadimos o modificamos las entidades
            AniadirModificarActividadCientifica(listadoAux, entidadesXML, equivalencias, propTitle, graph, rdfType, rdfTypePrefix, propiedadesItem);
        }

        /// <summary>
        /// Metodo para sincronizar los datos pertenecientes al 
        /// bloque "Resumen de otros méritos".
        /// Con codigo identificativo 060.030.100.000
        /// </summary>
        public void SincroResumenOtrosMeritos()
        {
            List<string> propiedadesItem = new List<string>() { "http://w3id.org/roh/scientificActivity", "http://w3id.org/roh/otherAchievements", "http://vivoweb.org/ontology/core#relatedBy" };
            string graph = "accreditation";
            string propTitle = "http://w3id.org/roh/title";
            string rdfType = "http://w3id.org/roh/Accreditation";
            string rdfTypePrefix = "RelatedOtherAchievement";

            //1º Obtenemos la entidad del XML.
            List<Entity> listadoAux = GetOtrosMeritos(listadoDatos);

            Dictionary<string, DisambiguableEntity> entidadesXML = new Dictionary<string, DisambiguableEntity>();
            foreach (Entity entityXML in listadoAux)
            {
                OtrosMeritos otrosMeritos = new OtrosMeritos();
                otrosMeritos.descripcion = entityXML.properties.FirstOrDefault(x => x.prop == Variables.ActividadCientificaTecnologica.otrosMeritosTextoLibre)?.values.FirstOrDefault();
                otrosMeritos.fecha = entityXML.properties.FirstOrDefault(x => x.prop == Variables.ActividadCientificaTecnologica.otrosMeritosFechaConcesion)?.values.FirstOrDefault();
                otrosMeritos.ID = Guid.NewGuid().ToString();
                entidadesXML.Add(otrosMeritos.ID, otrosMeritos);
            }

            //2º Obtenemos las entidades de la BBDD
            Dictionary<string, DisambiguableEntity> entidadesBBDD = OtrosMeritos.GetBBDD(mResourceApi, mCvID, graph, propiedadesItem);

            //3º Comparamos las equivalentes
            Dictionary<string, string> equivalencias = Disambiguation.SimilarityBBDD(entidadesXML.Values.ToList(), entidadesBBDD.Values.ToList());

            //4º Añadimos o modificamos las entidades
            AniadirModificarActividadCientifica(listadoAux, entidadesXML, equivalencias, propTitle, graph, rdfType, rdfTypePrefix, propiedadesItem);
        }

        private void GetEntidadesSecundarias(ref Entity entityBBDD, Tuple<string, string, string> identificadores, List<string> rdfTypeItem, string graph)
        {
            if (!string.IsNullOrEmpty(identificadores.Item3))
            {
                entityBBDD = GetLoadedEntity(identificadores.Item3, graph);
            }
            else
            {
                string item1 = identificadores.Item1;
                string item2 = identificadores.Item2;
                string item3 = identificadores.Item3;
                if (string.IsNullOrEmpty(item2))
                {
                    string nombreEntidad = rdfTypeItem[0];
                    if (nombreEntidad.Contains("#"))
                    {
                        nombreEntidad = nombreEntidad.Substring(nombreEntidad.LastIndexOf("#") + 1);
                    }
                    if (nombreEntidad.Contains("/"))
                    {
                        nombreEntidad = nombreEntidad.Substring(nombreEntidad.LastIndexOf("/") + 1);
                    }
                    item2 = mResourceApi.GraphsUrl + "items/" + nombreEntidad + "_" + mResourceApi.GetShortGuid(mCvID).ToString().ToLower() + "_" + Guid.NewGuid().ToString().ToLower();
                }
                if (string.IsNullOrEmpty(item3))
                {
                    string nombreEntidad = rdfTypeItem[1];
                    if (nombreEntidad.Contains("#"))
                    {
                        nombreEntidad = nombreEntidad.Substring(nombreEntidad.LastIndexOf("#") + 1);
                    }
                    if (nombreEntidad.Contains("/"))
                    {
                        nombreEntidad = nombreEntidad.Substring(nombreEntidad.LastIndexOf("/") + 1);
                    }
                    item3 = mResourceApi.GraphsUrl + "items/" + nombreEntidad + "_" + mResourceApi.GetShortGuid(mCvID).ToString().ToLower() + "_" + Guid.NewGuid().ToString().ToLower();
                }
                identificadores = new Tuple<string, string, string>(item1, item2, item3);
            }
        }


        /// <summary>
        /// 060.010.060.010
        /// </summary>
        /// <param name="listadoDatos"></param>
        /// <returns></returns>
        private Entity GetIndicadoresGenerales(List<CvnItemBean> listadoDatos)
        {
            try
            {
                Entity entity = new Entity();
                entity.auxEntityRemove = new List<string>();

                entity.properties = UtilitySecciones.AddProperty(
                    new Property(Variables.ActividadCientificaTecnologica.indicadoresGeneralesCalidad, listadoDatos.GetElementoPorIDCampo<CvnItemBeanCvnRichText>("060.010.060.010")?.Value)
                );

                return entity;
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e);
                return null;
            }
        }

        /// <summary>
        /// 060.010.010.000
        /// </summary>
        /// <param name="listadoDatos"></param>
        /// <returns></returns>
        public List<Entity> GetPublicacionesDocumentos(List<CvnItemBean> listadoDatos)
        {
            List<Entity> listado = new List<Entity>();

            List<CvnItemBean> listadoPublicacionesDocumentos = listadoDatos.Where(x => x.Code.Equals("060.010.010.000")).ToList();
            if (listadoPublicacionesDocumentos.Count > 0)
            {
                foreach (CvnItemBean item in listadoPublicacionesDocumentos)
                {
                    Entity entidadAux = new Entity();
                    entidadAux.properties = new List<Property>();
                    entidadAux.properties_cv = new List<Property>();
                    //TODO
                    if (!string.IsNullOrEmpty(item.GetStringPorIDCampo("060.010.010.010")))
                    {
                        entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                            new Property(Variables.ActividadCientificaTecnologica.pubDocumentosTipoProd, item.GetTipoPublicacionPorIDCampo("060.010.010.010")),
                            new Property(Variables.ActividadCientificaTecnologica.pubDocumentosTipoProdOtros, item.GetStringPorIDCampo("060.010.010.020")),
                            //new Property(Variables.ActividadCientificaTecnologica.pubDocumentosPosicion, item.GetStringPorIDCampo("060.010.010.050")),
                            //new Property(Variables.ActividadCientificaTecnologica.pubDocumentosNumAutores, item.GetStringPorIDCampo("060.010.010.380")),
                            new Property(Variables.ActividadCientificaTecnologica.pubDocumentosPubTitulo, item.GetStringPorIDCampo("060.010.010.030")),
                            new Property(Variables.ActividadCientificaTecnologica.pubDocumentosPubVolumen, item.GetVolumenPorIDCampo("060.010.010.080")),
                            new Property(Variables.ActividadCientificaTecnologica.pubDocumentosPubNumero, item.GetNumeroVolumenPorIDCampo("060.010.010.080")),
                            new Property(Variables.ActividadCientificaTecnologica.pubDocumentosPubPagIni, item.GetPaginaInicialPorIDCampo("060.010.010.090")),
                            new Property(Variables.ActividadCientificaTecnologica.pubDocumentosPubPagFin, item.GetPaginaFinalPorIDCampo("060.010.010.090")),
                            //new Property(Variables.ActividadCientificaTecnologica.pubDocumentosPubEditorial, item.GetStringPorIDCampo("060.010.010.100")),//TODO - ¿?
                            new Property(Variables.ActividadCientificaTecnologica.pubDocumentosPubPais, item.GetPaisPorIDCampo("060.010.010.110")),
                            new Property(Variables.ActividadCientificaTecnologica.pubDocumentosPubCCAA, item.GetRegionPorIDCampo("060.010.010.120")),
                            new Property(Variables.ActividadCientificaTecnologica.pubDocumentosPubFecha, item.GetStringDatetimePorIDCampo("060.010.010.140")),
                            new Property(Variables.ActividadCientificaTecnologica.pubDocumentosPubURL, item.GetStringPorIDCampo("060.010.010.150")),
                            new Property(Variables.ActividadCientificaTecnologica.pubDocumentosPubDepositoLegal, item.GetElementoPorIDCampo<CvnItemBeanCvnExternalPKBean>("060.010.010.170")?.Value),
                            new Property(Variables.ActividadCientificaTecnologica.pubDocumentosPubCiudad, item.GetStringPorIDCampo("060.010.010.220")),
                            new Property(Variables.ActividadCientificaTecnologica.pubDocumentosColeccion, item.GetStringPorIDCampo("060.010.010.270")),
                            new Property(Variables.ActividadCientificaTecnologica.pubDocumentosReseniaRevista, item.GetStringDoublePorIDCampo("060.010.010.340"))
                        //,
                        //new Property(Variables.ActividadCientificaTecnologica.pubDocumentosAutorCorrespondencia, item.GetStringBooleanPorIDCampo("060.010.010.390"))
                        ));
                        entidadAux.properties_cv.AddRange(UtilitySecciones.AddProperty(
                            new Property(Variables.ActividadCientificaTecnologica.pubDocumentosGradoContribucion, item.GetGradoContribucionPorIDCampo("060.010.010.060")),//properties_cv
                            new Property(Variables.ActividadCientificaTecnologica.pubDocumentosResultadosDestacados, item.GetStringPorIDCampo("060.010.010.290")),//properties_cv
                            new Property(Variables.ActividadCientificaTecnologica.pubDocumentosPubRelevante, item.GetStringBooleanPorIDCampo("060.010.010.300"))//properties_cv
                        ));
                        PublicacionesDocumentosSoporte(item, entidadAux);
                        //PublicacionesDocumentosAutores(item, entidadAux);
                        PublicacionesDocumentosTraducciones(item, entidadAux);
                        PublicacionesDocumentosIDPublicacion(item, entidadAux);
                        PublicacionesDocumentosISBN(item, entidadAux);

                        listado.Add(entidadAux);
                    }
                }
            }
            return listado;
        }

        /// <summary>
        /// Inserta en <paramref name="entidadAux"/> los valores de <paramref name="item"/>,
        /// pertenecientes al Soporte.
        /// </summary>
        /// <param name="item">item</param>
        /// <param name="entidadAux">entidadAux</param>
        private void PublicacionesDocumentosSoporte(CvnItemBean item, Entity entidadAux)
        {
            //Si es nulo no hago nada
            if (string.IsNullOrEmpty(item.GetStringPorIDCampo("060.010.010.070"))) { return; }

            //Si es revista(057)
            if (item.GetStringPorIDCampo("060.010.010.070").Equals("057"))
            {
                string revista = UtilitySecciones.GetNombreRevista(mResourceApi, item.GetStringPorIDCampo("060.010.010.210"));
                //asignar el documento
                if (!string.IsNullOrEmpty(revista))
                {
                    entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                        new Property(Variables.ActividadCientificaTecnologica.pubDocumentosTipoSoporte, item.GetTipoSoportePorIDCampo("060.010.010.070")),
                        new Property(Variables.ActividadCientificaTecnologica.pubDocumentosPubMainDoc, revista)
                    ));
                }
            }

            //Si es Libro(032), Documento o informe cientifico-tecnico(018) o catalogo de obra artistica(006)
            else
            {
                entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                    new Property(Variables.ActividadCientificaTecnologica.pubDocumentosTipoSoporte, item.GetTipoSoportePorIDCampo("060.010.010.070")),
                    new Property(Variables.ActividadCientificaTecnologica.pubDocumentosPubNombre, item.GetStringPorIDCampo("060.010.010.210"))
                ));
            }
        }

        /// <summary>
        /// Inserta en <paramref name="entidadAux"/> los valores de <paramref name="item"/>,
        /// pertenecientes a los Autores/as.
        /// </summary>
        /// <param name="item">item</param>
        /// <param name="entidadAux">entidadAux</param>
        private void PublicacionesDocumentosAutores(CvnItemBean item, Entity entidadAux)//TODO
        {
            List<CvnItemBeanCvnAuthorBean> listadoAutores = item.GetListaElementosPorIDCampo<CvnItemBeanCvnAuthorBean>("060.010.010.040");
            List<string> busqueda = UtilitySecciones.GetBusquedaAutor(mResourceApi, "skarmeta");

            string entityPartAux = Guid.NewGuid().ToString() + "@@@";
            if (busqueda.Count > 0)
            {

            }
            foreach (CvnItemBeanCvnAuthorBean autor in listadoAutores)
            {
                entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                //new Property(Variables.ActividadCientificaTecnologica.pubDocumentosAutorNombre, autor.GivenName),
                //new Property(Variables.ActividadCientificaTecnologica.pubDocumentosAutorPrimerApellido, autor.CvnFamilyNameBean?.FirstFamilyName),
                //new Property(Variables.ActividadCientificaTecnologica.pubDocumentosAutorSegundoApellido, autor.CvnFamilyNameBean?.SecondFamilyName),
                //new Property(Variables.ActividadCientificaTecnologica.pubDocumentosAutorFirma, autor.Signature.ToString())
                ));
            }
        }

        /// <summary>
        /// Inserta en <paramref name="entidadAux"/> los valores de <paramref name="item"/>,
        /// pertenecientes a las Traducciones.
        /// </summary>
        /// <param name="item">item</param>
        /// <param name="entidadAux">entidadAux</param>
        private void PublicacionesDocumentosTraducciones(CvnItemBean item, Entity entidadAux)
        {
            List<CvnItemBeanCvnTitleBean> listadoTraducciones = item.GetListaElementosPorIDCampo<CvnItemBeanCvnTitleBean>("060.010.010.350");
            foreach (CvnItemBeanCvnTitleBean isbn in listadoTraducciones)
            {
                Property IDOtro = entidadAux.properties.FirstOrDefault(x => x.prop == Variables.ActividadCientificaTecnologica.pubDocumentosTraduccion);
                string valor = isbn.GetTraduccion();
                string propiedad = Variables.ActividadCientificaTecnologica.pubDocumentosTraduccion;
                UtilitySecciones.CheckProperty(IDOtro, entidadAux, valor, propiedad);
            }
        }

        /// <summary>
        /// Inserta en <paramref name="entidadAux"/> los valores de <paramref name="item"/>,
        /// pertenecientes a los Identificadores de publicación digital.
        /// </summary>
        /// <param name="item">item</param>
        /// <param name="entidadAux">entidadAux</param>
        private void PublicacionesDocumentosIDPublicacion(CvnItemBean item, Entity entidadAux)
        {
            List<CvnItemBeanCvnExternalPKBean> listadoIDs = item.GetListaElementosPorIDCampo<CvnItemBeanCvnExternalPKBean>("060.010.010.400");
            foreach (CvnItemBeanCvnExternalPKBean identificador in listadoIDs)
            {
                switch (identificador.Type)
                {
                    case "120":
                        entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                            new Property(Variables.ActividadCientificaTecnologica.pubDocumentosIDPubDigitalHandle, identificador.Value)
                        ));
                        break;
                    case "040":
                        entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                            new Property(Variables.ActividadCientificaTecnologica.pubDocumentosIDPubDigitalDOI, identificador.Value)
                        ));
                        break;
                    case "130":
                        entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                            new Property(Variables.ActividadCientificaTecnologica.pubDocumentosIDPubDigitalPMID, identificador.Value)
                        ));
                        break;
                    case "OTHERS"://TODO - check
                        Property IDOtro = entidadAux.properties.FirstOrDefault(x => x.prop == Variables.ActividadCientificaTecnologica.pubDocumentosIDOtroPubDigital);
                        Property NombreOtro = entidadAux.properties.FirstOrDefault(x => x.prop == Variables.ActividadCientificaTecnologica.pubDocumentosNombreOtroPubDigital);

                        string entityPartAux = Guid.NewGuid().ToString() + "@@@";
                        string valorID = StringGNOSSID(entityPartAux, identificador.Value);
                        string propiedadID = Variables.ActividadCientificaTecnologica.pubDocumentosIDOtroPubDigital;
                        UtilitySecciones.CheckProperty(IDOtro, entidadAux, valorID, propiedadID);

                        string valorNombre = StringGNOSSID(entityPartAux, identificador.Others);
                        string propiedadNombre = Variables.ActividadCientificaTecnologica.pubDocumentosNombreOtroPubDigital;
                        UtilitySecciones.CheckProperty(IDOtro, entidadAux, valorNombre, propiedadNombre);
                        break;
                }
            }
        }

        /// <summary>
        /// Inserta en <paramref name="entidadAux"/> los valores de <paramref name="item"/>,
        /// pertenecientes al ISBN/ISSN.
        /// </summary>
        /// <param name="item">item</param>
        /// <param name="entidadAux">entidadAux</param>
        private void PublicacionesDocumentosISBN(CvnItemBean item, Entity entidadAux)
        {
            List<CvnItemBeanCvnExternalPKBean> listadoISBN = item.GetListaElementosPorIDCampo<CvnItemBeanCvnExternalPKBean>("060.010.010.160");
            foreach (CvnItemBeanCvnExternalPKBean isbn in listadoISBN)
            {
                //Si no hay type, ignoro el valor
                if (string.IsNullOrEmpty(isbn.Type)) { continue; }

                //Si es ISBN (020)
                if (isbn.Type.Equals("020"))
                {
                    entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                        new Property(Variables.ActividadCientificaTecnologica.pubDocumentosPubISBN, isbn.Value)
                    ));
                }
            }
        }

        /// <summary>
        /// 060.010.020.000
        /// </summary>
        /// <param name="listadoDatos"></param>
        /// <returns></returns>
        public List<Entity> GetTrabajosCongresos(List<CvnItemBean> listadoDatos)
        {
            List<Entity> listado = new List<Entity>();

            List<CvnItemBean> listadoTrabajosCongresos = listadoDatos.Where(x => x.Code.Equals("060.010.020.000")).ToList();
            if (listadoTrabajosCongresos.Count > 0)
            {
                foreach (CvnItemBean item in listadoTrabajosCongresos)
                {
                    Entity entidadAux = new Entity();
                    entidadAux.properties = new List<Property>();
                    if (!string.IsNullOrEmpty(item.GetStringPorIDCampo("060.010.020.010")))
                    {
                        //TODO
                        entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                            new Property(Variables.ActividadCientificaTecnologica.trabajosCongresosTitulo, item.GetStringPorIDCampo("060.010.020.030")),
                            //new Property(Variables.ActividadCientificaTecnologica.trabajosCongresosTipoParticipacion, item.GetStringPorIDCampo("060.010.020.050")),
                            //new Property(Variables.ActividadCientificaTecnologica.trabajosCongresosIntervencion, item.GetStringPorIDCampo("060.010.020.060")),
                            //new Property(Variables.ActividadCientificaTecnologica.trabajosCongresosIntervencionOtros, item.GetStringPorIDCampo("060.010.020.070")),                          
                            new Property(Variables.ActividadCientificaTecnologica.trabajosCongresosPubActa, item.GetStringPorIDCampo("060.010.020.200")),
                            //new Property(Variables.ActividadCientificaTecnologica.trabajosCongresosFormaContribucion, item.GetStringPorIDCampo("060.010.020.220")),
                            new Property(Variables.ActividadCientificaTecnologica.trabajosCongresosPubTitulo, item.GetStringPorIDCampo("060.010.020.230")),
                            //new Property(Variables.ActividadCientificaTecnologica.trabajosCongresosPubNombre, item.GetStringPorIDCampo("060.010.020.370")),
                            new Property(Variables.ActividadCientificaTecnologica.trabajosCongresosPubVolumen, item.GetVolumenPorIDCampo("060.010.020.240")),
                            new Property(Variables.ActividadCientificaTecnologica.trabajosCongresosPubNumero, item.GetNumeroVolumenPorIDCampo("060.010.020.240")),
                            new Property(Variables.ActividadCientificaTecnologica.trabajosCongresosPubPagIni, item.GetPaginaInicialPorIDCampo("060.010.020.250")),
                            new Property(Variables.ActividadCientificaTecnologica.trabajosCongresosPubPagFin, item.GetPaginaFinalPorIDCampo("060.010.020.250")),
                            new Property(Variables.ActividadCientificaTecnologica.trabajosCongresosPubPais, item.GetPaisPorIDCampo("060.010.020.270")),
                            //new Property(Variables.ActividadCientificaTecnologica.trabajosCongresosPubEditorial, item.GetStringPorIDCampo("060.010.020.260")),
                            new Property(Variables.ActividadCientificaTecnologica.trabajosCongresosPubCCAA, item.GetRegionPorIDCampo("060.010.020.280")),
                            //new Property(Variables.ActividadCientificaTecnologica.trabajosCongresosPubFecha, item.GetStringDatetimePorIDCampo("060.010.020.300")),
                            new Property(Variables.ActividadCientificaTecnologica.trabajosCongresosPubURL, item.GetStringPorIDCampo("060.010.020.310")),
                            new Property(Variables.ActividadCientificaTecnologica.trabajosCongresosPubDepositoLegal, item.GetStringPorIDCampo("060.010.020.330"))
                        //,
                        //new Property(Variables.ActividadCientificaTecnologica.trabajosCongresosAutoCorrespondencia, item.GetStringPorIDCampo("060.010.020.390"))
                        ));
                        TrabajosCongresosEvento(item, entidadAux);
                        //TrabajosCongresosAutores(item, entidadAux);
                        TrabajosCongresosIDPublicacion(item, entidadAux);
                        //TrabajosCongresosISBN(item, entidadAux);

                        listado.Add(entidadAux);
                    }
                }
            }
            return listado;
        }

        /// <summary>
        /// Inserta en <paramref name="entidadAux"/> los valores de <paramref name="item"/>
        /// pertenecientes a los Eventos
        /// </summary>
        /// <param name="item">item</param>
        /// <param name="entidadAux">entidadAux</param>
        private void TrabajosCongresosEvento(CvnItemBean item, Entity entidadAux)
        {
            //new Property(Variables.ActividadCientificaTecnologica.trabajosCongresosPaisCongreso, item.GetPaisPorIDCampo("060.010.020.340")),
            //new Property(Variables.ActividadCientificaTecnologica.trabajosCongresosCCAACongreso, item.GetRegionPorIDCampo("060.010.020.350")),
            //new Property(Variables.ActividadCientificaTecnologica.trabajosCongresosCiudadCongreso, item.GetStringPorIDCampo("060.010.020.360"))

            string entityPartAux = Guid.NewGuid().ToString() + "@@@";
            entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                new Property(Variables.ActividadCientificaTecnologica.trabajosCongresosNombreCongreso, StringGNOSSID(entityPartAux, item.GetStringPorIDCampo("060.010.020.100"))),
                new Property(Variables.ActividadCientificaTecnologica.trabajosCongresosFechaCelebracion, StringGNOSSID(entityPartAux, item.GetStringPorIDCampo("060.010.020.190"))),
                new Property(Variables.ActividadCientificaTecnologica.trabajosCongresosFechaFin, StringGNOSSID(entityPartAux, item.GetStringDatetimePorIDCampo("060.010.020.380"))),
                new Property(Variables.ActividadCientificaTecnologica.trabajosCongresosCiudadCelebracion, StringGNOSSID(entityPartAux, item.GetStringPorIDCampo("060.010.020.180"))),
                new Property(Variables.ActividadCientificaTecnologica.trabajosCongresosPaisCelebracion, StringGNOSSID(entityPartAux, item.GetPaisPorIDCampo("060.010.020.150"))),
                new Property(Variables.ActividadCientificaTecnologica.trabajosCongresosCCAACelebracion, StringGNOSSID(entityPartAux, item.GetRegionPorIDCampo("060.010.020.160"))),
                //new Property(Variables.ActividadCientificaTecnologica.trabajosCongresosEntidadOrganizadora, StringGNOSSID(entityPartAux, item.GetStringPorIDCampo("060.010.020.110"))),
                new Property(Variables.ActividadCientificaTecnologica.trabajosCongresosTipoEvento, StringGNOSSID(entityPartAux, item.GetTipoEventoPorIDCampo("060.010.020.010"))),
                //new Property(Variables.ActividadCientificaTecnologica.trabajosCongresosTipoEventoOtros, StringGNOSSID(entityPartAux, item.GetStringPorIDCampo("060.010.020.020"))),
                new Property(Variables.ActividadCientificaTecnologica.trabajosCongresosComiteExterno, StringGNOSSID(entityPartAux, item.GetStringPorIDCampo("060.010.020.210"))),
                new Property(Variables.ActividadCientificaTecnologica.trabajosCongresosAmbitoGeo, StringGNOSSID(entityPartAux, item.GetGeographicRegionPorIDCampo("060.010.020.080"))),
                new Property(Variables.ActividadCientificaTecnologica.trabajosCongresosAmbitoGeoOtros, StringGNOSSID(entityPartAux, item.GetStringPorIDCampo("060.010.020.090")))
            ));
        }

        /// <summary>
        /// Inserta en <paramref name="entidadAux"/> los valores de <paramref name="item"/>,
        /// pertenecientes a los Autores/as.
        /// </summary>
        /// <param name="item">item</param>
        /// <param name="entidadAux">entidadAux</param>
        private void TrabajosCongresosAutores(CvnItemBean item, Entity entidadAux)//TODO
        {
            List<CvnItemBeanCvnAuthorBean> listadoAutores = item.GetListaElementosPorIDCampo<CvnItemBeanCvnAuthorBean>("060.010.020.040");
            foreach (CvnItemBeanCvnAuthorBean autor in listadoAutores)
            {
                //Property autorNombre = entidadAux.properties.FirstOrDefault(x => x.prop == Variables.ActividadCientificaTecnologica.trabajosCongresosAutorNombre);
                //Property autorPrimerApellido = entidadAux.properties.FirstOrDefault(x => x.prop == Variables.ActividadCientificaTecnologica.trabajosCongresosAutorPrimerApellido);
                //Property autorSegundoApellido = entidadAux.properties.FirstOrDefault(x => x.prop == Variables.ActividadCientificaTecnologica.trabajosCongresosAutorSegundoApellido);
                //Property autorFirma = entidadAux.properties.FirstOrDefault(x => x.prop == Variables.ActividadCientificaTecnologica.trabajosCongresosAutorFirma);

                entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                //new Property(Variables.ActividadCientificaTecnologica.trabajosCongresosAutorNombre, autor.GivenName),
                //new Property(Variables.ActividadCientificaTecnologica.trabajosCongresosAutorPrimerApellido, autor.CvnFamilyNameBean?.FirstFamilyName),
                //new Property(Variables.ActividadCientificaTecnologica.trabajosCongresosAutorSegundoApellido, autor.CvnFamilyNameBean?.SecondFamilyName),
                //new Property(Variables.ActividadCientificaTecnologica.trabajosCongresosAutorFirma, autor.Signature.ToString())
                ));
            }
        }

        /// <summary>
        /// Inserta en <paramref name="entidadAux"/> los valores de <paramref name="item"/>,
        /// pertenecientes a los Identificadores de publicación digital.
        /// </summary>
        /// <param name="item">item</param>
        /// <param name="entidadAux">entidadAux</param>
        private void TrabajosCongresosIDPublicacion(CvnItemBean item, Entity entidadAux)
        {
            List<CvnItemBeanCvnExternalPKBean> listadoIDs = item.GetListaElementosPorIDCampo<CvnItemBeanCvnExternalPKBean>("060.010.020.400");
            foreach (CvnItemBeanCvnExternalPKBean identificador in listadoIDs)
            {
                switch (identificador.Type)
                {
                    case "120":
                        entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                            new Property(Variables.ActividadCientificaTecnologica.trabajosCongresosIDPubDigitalHandle, identificador.Value)
                        ));
                        break;
                    case "040":
                        entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                            new Property(Variables.ActividadCientificaTecnologica.trabajosCongresosIDPubDigitalDOI, identificador.Value)
                        ));
                        break;
                    case "130":
                        entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                            new Property(Variables.ActividadCientificaTecnologica.trabajosCongresosIDPubDigitalPMID, identificador.Value)
                        ));
                        break;
                    case "OTHERS":
                        Property IDOtro = entidadAux.properties.FirstOrDefault(x => x.prop == Variables.ActividadCientificaTecnologica.trabajosCongresosIDOtroPubDigital);
                        Property NombreOtro = entidadAux.properties.FirstOrDefault(x => x.prop == Variables.ActividadCientificaTecnologica.trabajosCongresosNombreOtroPubDigital);

                        string entityPartAux = Guid.NewGuid().ToString() + "@@@";
                        string valorID = StringGNOSSID(entityPartAux, identificador.Value);
                        string propiedadID = Variables.ActividadCientificaTecnologica.trabajosCongresosIDOtroPubDigital;
                        UtilitySecciones.CheckProperty(IDOtro, entidadAux, valorID, propiedadID);

                        string valorNombre = StringGNOSSID(entityPartAux, identificador.Others);
                        string propiedadNombre = Variables.ActividadCientificaTecnologica.trabajosCongresosNombreOtroPubDigital;
                        UtilitySecciones.CheckProperty(IDOtro, entidadAux, valorNombre, propiedadNombre);
                        break;
                }
            }
        }

        /// <summary>
        /// Inserta en <paramref name="entidadAux"/> los valores de <paramref name="item"/>,
        /// pertenecientes al ISBN/ISSN.
        /// </summary>
        /// <param name="item">item</param>
        /// <param name="entidadAux">entidadAux</param>
        private void TrabajosCongresosISBN(CvnItemBean item, Entity entidadAux)//TODO - check
        {
            List<CvnItemBeanCvnExternalPKBean> listadoISBN = item.GetListaElementosPorIDCampo<CvnItemBeanCvnExternalPKBean>("060.010.020.320");
            Property isbnProperty = entidadAux.properties.FirstOrDefault(x => x.prop == Variables.ActividadCientificaTecnologica.trabajosCongresosPubISBN);
            //Property issnProperty = entidadAux.properties.FirstOrDefault(x => x.prop == Variables.ActividadCientificaTecnologica.trabajosCongresosPubISSN);

            foreach (CvnItemBeanCvnExternalPKBean isbn in listadoISBN)
            {
                //Si no hay type, ignoro el valor
                if (string.IsNullOrEmpty(isbn.Type)) { continue; }

                //Si es ISBN (020)
                if (isbn.Type.Equals("020"))
                {
                    entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                        new Property(Variables.ActividadCientificaTecnologica.trabajosCongresosPubISBN, isbn.Value)
                    ));
                }

            }
        }

        /// <summary>
        /// Inserta en <paramref name="entidadAux"/> los valores de <paramref name="item"/>,
        /// pertenecientes al tipo de Entidad Organizadora.
        /// </summary>
        /// <param name="item">item</param>
        /// <param name="entidadAux">entidadAux</param>
        private void TrabajosCongresosTipoEntidad(CvnItemBean item, Entity entidadAux)
        {
            //Añado otros, o el ID de una preseleccion
            string valorTipo = !string.IsNullOrEmpty(item.GetStringPorIDCampo("060.010.020.140")) ? mResourceApi.GraphsUrl + "items/organizationtype_OTHERS" : item.GetOrganizacionPorIDCampo("060.010.020.130");

            entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                new Property(Variables.ActividadCientificaTecnologica.trabajosCongresosTipoEntidadOrganizadora, valorTipo),
                new Property(Variables.ActividadCientificaTecnologica.trabajosCongresosTipoEntidadOrganizadoraOtros, item.GetStringPorIDCampo("060.010.020.140"))
            ));
        }

        /// <summary>
        /// 060.010.030.000
        /// </summary>
        /// <param name="listadoDatos"></param>
        /// <returns></returns>
        public List<Entity> GetTrabajosJornadasSeminarios(List<CvnItemBean> listadoDatos)
        {
            List<Entity> listado = new List<Entity>();

            List<CvnItemBean> listadoComitesTrabajosJornadasSeminarios = listadoDatos.Where(x => x.Code.Equals("060.010.030.000")).ToList();
            if (listadoComitesTrabajosJornadasSeminarios.Count > 0)
            {
                foreach (CvnItemBean item in listadoComitesTrabajosJornadasSeminarios)
                {
                    Entity entidadAux = new Entity();
                    entidadAux.properties = new List<Property>();
                    if (!string.IsNullOrEmpty(item.GetStringPorIDCampo("060.010.030.010")))
                    {
                        //TODO
                        entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                            new Property(Variables.ActividadCientificaTecnologica.trabajosJornSemTituloTrabajo, item.GetStringPorIDCampo("060.010.030.010")),
                            //new Property(Variables.ActividadCientificaTecnologica.trabajosJornSemIntervencion, item.GetStringPorIDCampo("060.010.030.040")),
                            new Property(Variables.ActividadCientificaTecnologica.trabajosJornSemPubActaCongreso, item.GetStringBooleanPorIDCampo("060.010.030.170")),
                            //new Property(Variables.ActividadCientificaTecnologica.trabajosJornSemPubTipo, item.GetStringPorIDCampo("060.010.030.190")),//TODO - value others
                            new Property(Variables.ActividadCientificaTecnologica.trabajosJornSemPubTitulo, item.GetStringPorIDCampo("060.010.030.200")),
                            //new Property(Variables.ActividadCientificaTecnologica.trabajosJornSemPubNombre, item.GetStringPorIDCampo("060.010.030.350")),
                            new Property(Variables.ActividadCientificaTecnologica.trabajosJornSemPubVolumen, item.GetVolumenPorIDCampo("060.010.030.210")),
                            new Property(Variables.ActividadCientificaTecnologica.trabajosJornSemPubNumero, item.GetNumeroVolumenPorIDCampo("060.010.030.210")),
                            new Property(Variables.ActividadCientificaTecnologica.trabajosJornSemPubPagIni, item.GetPaginaInicialPorIDCampo("060.010.030.220")),
                            new Property(Variables.ActividadCientificaTecnologica.trabajosJornSemPubPagFin, item.GetPaginaFinalPorIDCampo("060.010.030.220")),
                            new Property(Variables.ActividadCientificaTecnologica.trabajosJornSemPubPais, item.GetPaisPorIDCampo("060.010.030.240")),
                            //new Property(Variables.ActividadCientificaTecnologica.trabajosJornSemPubEditorial, item.GetStringPorIDCampo("060.010.030.230")),
                            new Property(Variables.ActividadCientificaTecnologica.trabajosJornSemPubCCAA, item.GetRegionPorIDCampo("060.010.030.250")),
                            new Property(Variables.ActividadCientificaTecnologica.trabajosJornSemPubFecha, item.GetStringDatetimePorIDCampo("060.010.030.270")),
                            new Property(Variables.ActividadCientificaTecnologica.trabajosJornSemPubURL, item.GetStringPorIDCampo("060.010.030.280")),
                            new Property(Variables.ActividadCientificaTecnologica.trabajosJornSemPubDepositoLegal, item.GetStringPorIDCampo("060.010.030.300"))
                        //,
                        //new Property(Variables.ActividadCientificaTecnologica.trabajosJornSemAutorCorrespondencia, item.GetStringPorIDCampo("060.010.030.390"))
                        ));
                        TrabajosJornadasSeminariosEvento(item, entidadAux);
                        //TrabajosJornadasSeminariosAutores(item, entidadAux);
                        TrabajosJornadasSeminariosIDPublicacion(item, entidadAux);
                        //TrabajosJornadasSeminariosISBN(item, entidadAux);
                        ////TrabajosJornadasSeminariosTipoEntidad(item, entidadAux);

                        listado.Add(entidadAux);
                    }
                }
            }
            return listado;
        }

        /// <summary>
        /// Inserta en <paramref name="entidadAux"/> los valores de <paramref name="item"/>,
        /// pertenecientes a los Eventos.
        /// </summary>
        /// <param name="item">item</param>
        /// <param name="entidadAux">entidadAux</param>
        private void TrabajosJornadasSeminariosEvento(CvnItemBean item, Entity entidadAux)
        {
            //new Property(Variables.ActividadCientificaTecnologica.trabajosJornSemPaisEntidadOrganizadora, item.GetPaisPorIDCampo("060.010.030.320")),
            //new Property(Variables.ActividadCientificaTecnologica.trabajosJornSemCCAAEntidadOrganizadora, item.GetRegionPorIDCampo("060.010.030.330")),
            //new Property(Variables.ActividadCientificaTecnologica.trabajosJornSemCiudadEntidadOrganizadora, item.GetStringPorIDCampo("060.010.030.340"))

            string entityPartAux = Guid.NewGuid().ToString() + "@@@";
            entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                new Property(Variables.ActividadCientificaTecnologica.trabajosJornSemNombreEvento, StringGNOSSID(entityPartAux, item.GetStringPorIDCampo("060.010.030.070"))),
                new Property(Variables.ActividadCientificaTecnologica.trabajosJornSemFechaCelebracion, StringGNOSSID(entityPartAux, item.GetStringDatetimePorIDCampo("060.010.030.160"))),
                new Property(Variables.ActividadCientificaTecnologica.trabajosJornSemPubFechaFinCelebracion, StringGNOSSID(entityPartAux, item.GetStringDatetimePorIDCampo("060.010.030.370"))),
                new Property(Variables.ActividadCientificaTecnologica.trabajosJornSemCiudadCelebracion, StringGNOSSID(entityPartAux, item.GetStringPorIDCampo("060.010.030.150"))),
                new Property(Variables.ActividadCientificaTecnologica.trabajosJornSemPaisCelebracion, StringGNOSSID(entityPartAux, item.GetPaisPorIDCampo("060.010.030.120"))),
                new Property(Variables.ActividadCientificaTecnologica.trabajosJornSemCCAACelebracion, StringGNOSSID(entityPartAux, item.GetRegionPorIDCampo("060.010.030.130"))),
                ////new Property(Variables.ActividadCientificaTecnologica.trabajosJornSemEntidadOrganizadora, StringGNOSSID(entityPartAux, item.GetNameEntityBeanPorIDCampo("060.010.030.080"))),
                new Property(Variables.ActividadCientificaTecnologica.trabajosJornSemTipoEvento, StringGNOSSID(entityPartAux, item.GetTipoEventoPorIDCampo("060.010.030.020"))),
                //new Property(Variables.ActividadCientificaTecnologica.trabajosJornSemTipoEventoOtros, StringGNOSSID(entityPartAux, item.GetStringPorIDCampo("060.010.030.030")),
                new Property(Variables.ActividadCientificaTecnologica.trabajosJornSemPubActaCongresoExterno, StringGNOSSID(entityPartAux, item.GetStringBooleanPorIDCampo("060.010.030.180"))),
                new Property(Variables.ActividadCientificaTecnologica.trabajosJornSemAmbitoGeo, StringGNOSSID(entityPartAux, item.GetGeographicRegionPorIDCampo("060.010.030.050"))),
                new Property(Variables.ActividadCientificaTecnologica.trabajosJornSemAmbitoGeoOtros, StringGNOSSID(entityPartAux, item.GetStringPorIDCampo("060.010.030.060")))
            ));
        }

        /// <summary>
        /// Inserta en <paramref name="entidadAux"/> los valores de <paramref name="item"/>,
        /// pertenecientes a los Autores/as.
        /// </summary>
        /// <param name="item">item</param>
        /// <param name="entidadAux">entidadAux</param>
        private void TrabajosJornadasSeminariosAutores(CvnItemBean item, Entity entidadAux)//TODO
        {
            List<CvnItemBeanCvnAuthorBean> listadoAutores = item.GetListaElementosPorIDCampo<CvnItemBeanCvnAuthorBean>("060.010.030.310");
            foreach (CvnItemBeanCvnAuthorBean autor in listadoAutores)
            {
                entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                //new Property(Variables.ActividadCientificaTecnologica.trabajosJornSemAutorNombre, autor.GivenName),
                //new Property(Variables.ActividadCientificaTecnologica.trabajosJornSemAutorPrimerApellido, autor.CvnFamilyNameBean?.FirstFamilyName),
                //new Property(Variables.ActividadCientificaTecnologica.trabajosJornSemAutorSegundoApellido, autor.CvnFamilyNameBean?.SecondFamilyName),
                //new Property(Variables.ActividadCientificaTecnologica.trabajosJornSemAutorFirma, autor.Signature.ToString())
                ));
            }
        }

        /// <summary>
        /// /// <summary>
        /// Inserta en <paramref name="entidadAux"/> los valores de <paramref name="item"/>,
        /// pertenecientes a los Identificadores de publicación digital.
        /// </summary>
        /// <param name="item">item</param>
        /// <param name="entidadAux">entidadAux</param>
        private void TrabajosJornadasSeminariosIDPublicacion(CvnItemBean item, Entity entidadAux)
        {
            List<CvnItemBeanCvnExternalPKBean> listadoIDs = item.GetListaElementosPorIDCampo<CvnItemBeanCvnExternalPKBean>("060.010.030.400");

            foreach (CvnItemBeanCvnExternalPKBean identificador in listadoIDs)
            {
                switch (identificador.Type)
                {
                    case "120":
                        entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                            new Property(Variables.ActividadCientificaTecnologica.trabajosJornSemIDPubDigitalHandle, identificador.Value)
                        ));
                        break;
                    case "040":
                        entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                            new Property(Variables.ActividadCientificaTecnologica.trabajosJornSemIDPubDigitalDOI, identificador.Value)
                        ));
                        break;
                    case "130":
                        entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                            new Property(Variables.ActividadCientificaTecnologica.trabajosJornSemIDPubDigitalPMID, identificador.Value)
                        ));
                        break;
                    case "OTHERS"://TODO - check
                        Property IDOtro = entidadAux.properties.FirstOrDefault(x => x.prop == Variables.ActividadCientificaTecnologica.trabajosJornSemIDOtroPubDigital);
                        Property NombreOtro = entidadAux.properties.FirstOrDefault(x => x.prop == Variables.ActividadCientificaTecnologica.trabajosJornSemNombreOtroPubDigital);

                        string entityPartAux = Guid.NewGuid().ToString() + "@@@";
                        string valorID = StringGNOSSID(entityPartAux, identificador.Value);
                        string propiedadID = Variables.ActividadCientificaTecnologica.trabajosJornSemIDOtroPubDigital;
                        UtilitySecciones.CheckProperty(IDOtro, entidadAux, valorID, propiedadID);

                        string valorNombre = StringGNOSSID(entityPartAux, identificador.Others);
                        string propiedadNombre = Variables.ActividadCientificaTecnologica.trabajosJornSemNombreOtroPubDigital;
                        UtilitySecciones.CheckProperty(IDOtro, entidadAux, valorNombre, propiedadNombre);
                        break;
                }
            }
        }

        /// <summary>
        /// Inserta en <paramref name="entidadAux"/> los valores de <paramref name="item"/>,
        /// pertenecientes al ISBN/ISSN.
        /// </summary>
        /// <param name="item">item</param>
        /// <param name="entidadAux">entidadAux</param>
        private void TrabajosJornadasSeminariosISBN(CvnItemBean item, Entity entidadAux)
        {
            List<CvnItemBeanCvnExternalPKBean> listadoISBN = item.GetListaElementosPorIDCampo<CvnItemBeanCvnExternalPKBean>("060.010.030.290");
            foreach (CvnItemBeanCvnExternalPKBean isbn in listadoISBN)
            {
                //Si no hay type, ignoro el valor
                if (string.IsNullOrEmpty(isbn.Type)) { continue; }

                //Si es ISBN (020)
                if (isbn.Type.Equals("020"))
                {
                    entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                        new Property(Variables.ActividadCientificaTecnologica.trabajosJornSemPubISBN, isbn.Value)//TODO - añadir valor propiedad
                    ));
                }
            }
        }

        /// <summary>
        /// Inserta en <paramref name="entidadAux"/> los valores de <paramref name="item"/>,
        /// pertenecientes al tipo de Entidad Organizadora.
        /// </summary>
        /// <param name="item">item</param>
        /// <param name="entidadAux">entidadAux</param>
        private void TrabajosJornadasSeminariosTipoEntidad(CvnItemBean item, Entity entidadAux)
        {
            //Añado otros, o el ID de una preseleccion
            string valorTipo = !string.IsNullOrEmpty(item.GetStringPorIDCampo("060.010.030.110")) ? mResourceApi.GraphsUrl + "items/organizationtype_OTHERS" : item.GetOrganizacionPorIDCampo("060.010.030.100");

            entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                new Property(Variables.ActividadCientificaTecnologica.trabajosJornSemTipoEntidadOrganizadora, valorTipo),
                new Property(Variables.ActividadCientificaTecnologica.trabajosJornSemTipoEntidadOrganizadoraOtros, item.GetStringPorIDCampo("060.010.030.110"))
            ));
        }

        /// <summary>
        /// 060.010.040.000
        /// </summary>
        /// <param name="listadoDatos"></param>
        /// <returns></returns>
        public List<Entity> GetOtrasActividadesDivulgacion(List<CvnItemBean> listadoDatos)
        {
            List<Entity> listado = new List<Entity>();

            List<CvnItemBean> listadoOtrasActividadesDivulgacion = listadoDatos.Where(x => x.Code.Equals("060.010.040.000")).ToList();
            if (listadoOtrasActividadesDivulgacion.Count > 0)
            {
                foreach (CvnItemBean item in listadoOtrasActividadesDivulgacion)
                {
                    Entity entidadAux = new Entity();
                    entidadAux.properties = new List<Property>();
                    if (!string.IsNullOrEmpty(item.GetStringPorIDCampo("060.010.040.010")))
                    {
                        entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                            new Property(Variables.ActividadCientificaTecnologica.otrasActDivulTitulo, item.GetStringPorIDCampo("060.010.040.010")),
                            //new Property(Variables.ActividadCientificaTecnologica.otrasActDivulIntervencion, item.GetStringPorIDCampo("060.010.040.040")),//TODO
                            //new Property(Variables.ActividadCientificaTecnologica.otrasActDivulIntervencionIndicar, item.GetStringPorIDCampo("060.010.040.050")),
                            //new Property(Variables.ActividadCientificaTecnologica.otrasActDivulPaisEntidadOrg, item.GetPaisPorIDCampo("060.010.040.320")),
                            //new Property(Variables.ActividadCientificaTecnologica.otrasActDivulCCAAEntidadOrg, item.GetRegionPorIDCampo("060.010.040.330")),
                            //new Property(Variables.ActividadCientificaTecnologica.otrasActDivulCiudadEntidadOrg, item.GetStringPorIDCampo("060.010.040.340")),
                            new Property(Variables.ActividadCientificaTecnologica.otrasActDivulPubActaCongreso, item.GetStringBooleanPorIDCampo("060.010.040.180")),
                            //new Property(Variables.ActividadCientificaTecnologica.otrasActDivulPubTipo, item.GetStringPorIDCampo("060.010.040.200")),
                            new Property(Variables.ActividadCientificaTecnologica.otrasActDivulPubTitulo, item.GetStringPorIDCampo("060.010.040.210")),
                            //new Property(Variables.ActividadCientificaTecnologica.otrasActDivulPubNombre, item.GetStringPorIDCampo("060.010.040.360")),
                            new Property(Variables.ActividadCientificaTecnologica.otrasActDivulPubVolumen, item.GetVolumenPorIDCampo("060.010.040.220")),
                            new Property(Variables.ActividadCientificaTecnologica.otrasActDivulPubNumero, item.GetNumeroVolumenPorIDCampo("060.010.040.220")),
                            new Property(Variables.ActividadCientificaTecnologica.otrasActDivulPubPagIni, item.GetPaginaInicialPorIDCampo("060.010.040.230")),
                            new Property(Variables.ActividadCientificaTecnologica.otrasActDivulPubPagFin, item.GetPaginaFinalPorIDCampo("060.010.040.230")),
                            //new Property(Variables.ActividadCientificaTecnologica.otrasActDivulPubEditorial, item.GetStringPorIDCampo("060.010.040.240")),
                            new Property(Variables.ActividadCientificaTecnologica.otrasActDivulPubPais, item.GetPaisPorIDCampo("060.010.040.250")),
                            new Property(Variables.ActividadCientificaTecnologica.otrasActDivulPubCCAA, item.GetRegionPorIDCampo("060.010.040.260")),
                            new Property(Variables.ActividadCientificaTecnologica.otrasActDivulPubFecha, item.GetStringPorIDCampo("060.010.040.280")),
                            new Property(Variables.ActividadCientificaTecnologica.otrasActDivulPubURL, item.GetStringPorIDCampo("060.010.040.290")),
                            new Property(Variables.ActividadCientificaTecnologica.otrasActDivulPubDepositoLegal, item.GetStringPorIDCampo("060.010.040.310"))
                        //,
                        //new Property(Variables.ActividadCientificaTecnologica.otrasActDivulAutorCorrespondencia, item.GetStringPorIDCampo("060.010.040.390"))
                        ));
                        OtrasActividadesDivulgacionEvento(item, entidadAux);
                        //OtrasActividadesDivulgacionAutores(item,entidadAux);
                        OtrasActividadesDivulgacionIDPublicacion(item, entidadAux);
                        //OtrasActividadesDivulgacionISBN(item, entidadAux);
                        //OtrasActividadesDivulgacionTipoEntidad(item, entidadAux);

                        listado.Add(entidadAux);
                    }
                }
            }
            return listado;
        }

        /// <summary>
        /// Inserta en <paramref name="entidadAux"/> los valores de <paramref name="item"/>
        /// pertenecientes a los Eventos
        /// </summary>
        /// <param name="item">item</param>
        /// <param name="entidadAux">entidadAux</param>
        private void OtrasActividadesDivulgacionEvento(CvnItemBean item, Entity entidadAux)
        {
            string entityPartAux = Guid.NewGuid().ToString() + "@@@";
            entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                new Property(Variables.ActividadCientificaTecnologica.otrasActDivulNombreEvento, StringGNOSSID(entityPartAux, item.GetStringPorIDCampo("060.010.040.080"))),
                new Property(Variables.ActividadCientificaTecnologica.otrasActDivulFechaCelebracion, StringGNOSSID(entityPartAux, item.GetStringDatetimePorIDCampo("060.010.040.170"))),
                new Property(Variables.ActividadCientificaTecnologica.otrasActDivulCiudadCelebracion, StringGNOSSID(entityPartAux, item.GetStringPorIDCampo("060.010.040.160"))),
                new Property(Variables.ActividadCientificaTecnologica.otrasActDivulPaisCelebracion, StringGNOSSID(entityPartAux, item.GetPaisPorIDCampo("060.010.040.130"))),
                new Property(Variables.ActividadCientificaTecnologica.otrasActDivulCCAACelebracion, StringGNOSSID(entityPartAux, item.GetRegionPorIDCampo("060.010.040.140"))),
                //new Property(Variables.ActividadCientificaTecnologica.otrasActDivulEntidadOrg,StringGNOSSID(entityPartAux,  item.GetNameEntityBeanPorIDCampo("060.010.040.090"))),
                new Property(Variables.ActividadCientificaTecnologica.otrasActDivulTipoEvento, StringGNOSSID(entityPartAux, item.GetTipoEventoPorIDCampo("060.010.040.020"))),
                //new Property(Variables.ActividadCientificaTecnologica.otrasActDivulTipoEventoOtros, StringGNOSSID(entityPartAux, item.GetStringPorIDCampo("060.010.040.030"))),
                new Property(Variables.ActividadCientificaTecnologica.otrasActDivulPubActaAdmisionExt, StringGNOSSID(entityPartAux, item.GetStringBooleanPorIDCampo("060.010.040.190"))),
                new Property(Variables.ActividadCientificaTecnologica.otrasActDivulAmbitoEvento, StringGNOSSID(entityPartAux, item.GetGeographicRegionPorIDCampo("060.010.040.060"))),
                new Property(Variables.ActividadCientificaTecnologica.otrasActDivulAmbitoEventoOtros, StringGNOSSID(entityPartAux, item.GetStringPorIDCampo("060.010.040.070")))
            ));

        }

        /// <summary>
        /// Inserta en <paramref name="entidadAux"/> los valores de <paramref name="item"/>,
        /// pertenecientes a los Autores/as.
        /// </summary>
        /// <param name="item">item</param>
        /// <param name="entidadAux">entidadAux</param>
        private void OtrasActividadesDivulgacionAutores(CvnItemBean item, Entity entidadAux)//TODO
        {
            List<CvnItemBeanCvnAuthorBean> listadoAutores = item.GetListaElementosPorIDCampo<CvnItemBeanCvnAuthorBean>("060.010.040.350");
            foreach (CvnItemBeanCvnAuthorBean autor in listadoAutores)
            {
                entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                //new Property(Variables.ActividadCientificaTecnologica.otrasActDivulAutorNombre, autor.GivenName),
                //new Property(Variables.ActividadCientificaTecnologica.otrasActDivulAutorPrimerApellido, autor.CvnFamilyNameBean?.FirstFamilyName),
                //new Property(Variables.ActividadCientificaTecnologica.otrasActDivulAutorSegundoApellido, autor.CvnFamilyNameBean?.SecondFamilyName),
                //new Property(Variables.ActividadCientificaTecnologica.otrasActDivulAutorFirma, autor.Signature.ToString())
                ));
            }
        }

        /// <summary>
        /// Inserta en <paramref name="entidadAux"/> los valores de <paramref name="item"/>,
        /// pertenecientes a los Identificadores de publicación digital.
        /// </summary>
        /// <param name="item">item</param>
        /// <param name="entidadAux">entidadAux</param>
        private void OtrasActividadesDivulgacionIDPublicacion(CvnItemBean item, Entity entidadAux)
        {
            List<CvnItemBeanCvnExternalPKBean> listadoIDs = item.GetListaElementosPorIDCampo<CvnItemBeanCvnExternalPKBean>("060.010.040.400");
            foreach (CvnItemBeanCvnExternalPKBean identificador in listadoIDs)
            {
                switch (identificador.Type)
                {
                    case "120":
                        entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                            new Property(Variables.ActividadCientificaTecnologica.otrasActDivulIDPubDigitalHandle, identificador.Value)
                        ));
                        break;
                    case "040":
                        entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                            new Property(Variables.ActividadCientificaTecnologica.otrasActDivulIDPubDigitalDOI, identificador.Value)
                        ));
                        break;
                    case "130":
                        entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                            new Property(Variables.ActividadCientificaTecnologica.otrasActDivulIDPubDigitalPMID, identificador.Value)
                        ));
                        break;
                    case "OTHERS":
                        Property IDOtro = entidadAux.properties.FirstOrDefault(x => x.prop == Variables.ActividadCientificaTecnologica.otrasActDivulIDOtroPubDigital);
                        Property NombreOtro = entidadAux.properties.FirstOrDefault(x => x.prop == Variables.ActividadCientificaTecnologica.otrasActDivulNombreOtroIDPubDigital);

                        string entityPartAux = Guid.NewGuid().ToString() + "@@@";
                        string valorID = StringGNOSSID(entityPartAux, identificador.Value);
                        string propiedadID = Variables.ActividadCientificaTecnologica.otrasActDivulIDOtroPubDigital;
                        UtilitySecciones.CheckProperty(IDOtro, entidadAux, valorID, propiedadID);

                        string valorNombre = StringGNOSSID(entityPartAux, identificador.Others);
                        string propiedadNombre = Variables.ActividadCientificaTecnologica.otrasActDivulNombreOtroIDPubDigital;
                        UtilitySecciones.CheckProperty(IDOtro, entidadAux, valorNombre, propiedadNombre);
                        break;
                }
            }
        }

        /// <summary>
        /// Inserta en <paramref name="entidadAux"/> los valores de <paramref name="item"/>,
        /// pertenecientes al ISBN/ISSN.
        /// </summary>
        /// <param name="item">item</param>
        /// <param name="entidadAux">entidadAux</param>
        private void OtrasActividadesDivulgacionISBN(CvnItemBean item, Entity entidadAux)
        {
            List<CvnItemBeanCvnExternalPKBean> listadoISBN = item.GetListaElementosPorIDCampo<CvnItemBeanCvnExternalPKBean>("060.010.040.300");
            foreach (CvnItemBeanCvnExternalPKBean isbn in listadoISBN)
            {
                //Si no hay type, ignoro el valor
                if (string.IsNullOrEmpty(isbn.Type)) { continue; }

                //Si es ISBN (020)
                if (isbn.Type.Equals("020"))
                {
                    entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                        new Property(Variables.ActividadCientificaTecnologica.otrasActDivulPubISBN, isbn.Value)//TODO - añadir valor de la propiedad
                    ));
                }
            }
        }

        /// <summary>
        /// Inserta en <paramref name="entidadAux"/> los valores de <paramref name="item"/>,
        /// pertenecientes al tipo de Entidad Organizadora.
        /// </summary>
        /// <param name="item">item</param>
        /// <param name="entidadAux">entidadAux</param>
        private void OtrasActividadesDivulgacionTipoEntidad(CvnItemBean item, Entity entidadAux)
        {
            //Añado otros, o el ID de una preseleccion
            string valorTipo = !string.IsNullOrEmpty(item.GetStringPorIDCampo("060.010.040.120")) ? mResourceApi.GraphsUrl + "items/organizationtype_OTHERS" : item.GetOrganizacionPorIDCampo("060.010.040.110");

            entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                new Property(Variables.ActividadCientificaTecnologica.otrasActDivulTipoEntidadOrg, valorTipo),
                new Property(Variables.ActividadCientificaTecnologica.otrasActDivulTipoEntidadOrgOtros, item.GetStringPorIDCampo("060.010.040.120"))
            ));
        }

        /// <summary>
        /// Comités científicos, técnicos y/o asesores - 060.020.010.000
        /// </summary>
        /// <param name="listadoDatos"></param>
        /// <returns></returns>
        public List<Entity> GetComitesCTA(List<CvnItemBean> listadoDatos)
        {
            List<Entity> listado = new List<Entity>();

            List<CvnItemBean> listadoComitesCTA = listadoDatos.Where(x => x.Code.Equals("060.020.010.000")).ToList();
            if (listadoComitesCTA.Count > 0)
            {
                foreach (CvnItemBean item in listadoComitesCTA)
                {
                    Entity entidadAux = new Entity();
                    entidadAux.properties = new List<Property>();
                    if (!string.IsNullOrEmpty(item.GetStringPorIDCampo("060.020.010.010")))
                    {
                        entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                            new Property(Variables.ActividadCientificaTecnologica.comitesCTATitulo, item.GetStringPorIDCampo("060.020.010.010")),
                            new Property(Variables.ActividadCientificaTecnologica.comitesCTAPaisRadicacion, item.GetPaisPorIDCampo("060.020.010.020")),
                            new Property(Variables.ActividadCientificaTecnologica.comitesCTACCAARadicacion, item.GetRegionPorIDCampo("060.020.010.030")),
                            new Property(Variables.ActividadCientificaTecnologica.comitesCTACiudadRadicacion, item.GetStringPorIDCampo("060.020.010.050")),
                            new Property(Variables.ActividadCientificaTecnologica.comitesCTAPaisEntidadAfiliacion, item.GetPaisPorIDCampo("060.020.010.190")),
                            new Property(Variables.ActividadCientificaTecnologica.comitesCTACCAAEntidadAfiliacion, item.GetRegionPorIDCampo("060.020.010.180")),
                            new Property(Variables.ActividadCientificaTecnologica.comitesCTACiudadEntidadAfiliacion, item.GetStringPorIDCampo("060.020.010.170")),
                            new Property(Variables.ActividadCientificaTecnologica.comitesCTAAmbitoActividad, item.GetGeographicRegionPorIDCampo("060.020.010.100")),
                            new Property(Variables.ActividadCientificaTecnologica.comitesCTAAmbitoActividadOtros, item.GetStringPorIDCampo("060.020.010.110")),
                            new Property(Variables.ActividadCientificaTecnologica.comitesCTAFechaInicio, item.GetStringDatetimePorIDCampo("060.020.010.150")),
                            new Property(Variables.ActividadCientificaTecnologica.comitesCTAFechaFinalizacion, item.GetStringDatetimePorIDCampo("060.020.010.160"))
                        ));
                        ComitesCTAEntidadAfiliacion(item, entidadAux);
                        ComitesCTACodigosUnesco(item, entidadAux);

                        listado.Add(entidadAux);
                    }
                }
            }
            return listado;
        }

        /// <summary>
        /// Inserta en <paramref name="entidadAux"/> los valores de <paramref name="item"/>,
        /// pertenecientes a la Entidad de Afiliación.
        /// </summary>
        /// <param name="item">item</param>
        /// <param name="entidadAux">entidadAux</param>
        private void ComitesCTAEntidadAfiliacion(CvnItemBean item, Entity entidadAux)
        {
            //Añado la referencia si existe Entidad Realizacion
            UtilitySecciones.AniadirEntidad(mResourceApi, item.GetNameEntityBeanPorIDCampo("060.020.010.060"),
                Variables.ActividadCientificaTecnologica.comitesCTAEntidadAfiliacionNombre,
                Variables.ActividadCientificaTecnologica.comitesCTAEntidadAfiliacion, entidadAux);

            //Añado otros, o el ID de una preseleccion
            string valorTipo = !string.IsNullOrEmpty(item.GetStringPorIDCampo("060.020.010.090")) ? mResourceApi.GraphsUrl + "items/organizationtype_OTHERS" : item.GetOrganizacionPorIDCampo("060.020.010.080");

            entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                new Property(Variables.ActividadCientificaTecnologica.comitesCTATipoEntidadAfiliacion, valorTipo),
                new Property(Variables.ActividadCientificaTecnologica.comitesCTATipoEntidadAfiliacionOtros, item.GetStringPorIDCampo("060.020.010.090"))
            ));
        }

        /// <summary>
        /// Inserta en <paramref name="entidadAux"/> los valores de <paramref name="item"/>,
        /// pertenecientes a los Códigos UNESCO de especialización primaria,
        /// secundaria y terciaria.
        /// </summary>
        /// <param name="item">item</param>
        /// <param name="entidadAux">entidadAux</param>
        private void ComitesCTACodigosUnesco(CvnItemBean item, Entity entidadAux)
        {
            List<CvnItemBeanCvnString> listadoCodUnescoPrimaria = item.GetListaElementosPorIDCampo<CvnItemBeanCvnString>("060.020.010.120");
            List<CvnItemBeanCvnString> listadoCodUnescoSecundaria = item.GetListaElementosPorIDCampo<CvnItemBeanCvnString>("060.020.010.130");
            List<CvnItemBeanCvnString> listadoCodUnescoTerciaria = item.GetListaElementosPorIDCampo<CvnItemBeanCvnString>("060.020.010.140");

            foreach (CvnItemBeanCvnString codigo in listadoCodUnescoPrimaria)
            {
                string entityPartAux = Guid.NewGuid().ToString() + "@@@";
                List<string> listadoCodigos = Utility.GetPadresCodUnesco(codigo);
                //Añado Codigo UNESCO
                foreach (string codigolista in listadoCodigos)
                {
                    Property propertyCodUnescoPrimaria = entidadAux.properties.FirstOrDefault(x => x.prop == Variables.ActividadCientificaTecnologica.comitesCTACodUnescoPrimaria);

                    string valorCodigo = StringGNOSSID(entityPartAux, Utility.GetCodUnescoIDCampo(codigolista));
                    string propiedadCodigo = Variables.ActividadCientificaTecnologica.comitesCTACodUnescoPrimaria;
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
                    Property propertyCodUnescoSecundaria = entidadAux.properties.FirstOrDefault(x => x.prop == Variables.ActividadCientificaTecnologica.comitesCTACodUnescoSecundaria);

                    string valorCodigo = StringGNOSSID(entityPartAux, Utility.GetCodUnescoIDCampo(codigolista));
                    string propiedadCodigo = Variables.ActividadCientificaTecnologica.comitesCTACodUnescoSecundaria;
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
                    Property propertyCodUnescoTerciaria = entidadAux.properties.FirstOrDefault(x => x.prop == Variables.ActividadCientificaTecnologica.comitesCTACodUnescoTerciaria);

                    string valorCodigo = StringGNOSSID(entityPartAux, Utility.GetCodUnescoIDCampo(codigolista));
                    string propiedadCodigo = Variables.ActividadCientificaTecnologica.comitesCTACodUnescoTerciaria;
                    UtilitySecciones.CheckProperty(propertyCodUnescoTerciaria, entidadAux, valorCodigo, propiedadCodigo);
                }
            }
        }



        /// <summary>
        /// Inserta en <paramref name="entidadAux"/> los valores de <paramref name="item"/>,
        /// pertenecientes al tipo de Entidad de Afiliación.
        /// </summary>
        /// <param name="item">item</param>
        /// <param name="entidadAux">entidadAux</param>
        private void ComitesCTATipoEntidad(CvnItemBean item, Entity entidadAux)
        {
            //Añado otros, o el ID de una preseleccion
            string valorTipo = !string.IsNullOrEmpty(item.GetStringPorIDCampo("060.020.010.090")) ? mResourceApi.GraphsUrl + "items/organizationtype_OTHERS" : item.GetOrganizacionPorIDCampo("060.020.010.080");

            entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                new Property(Variables.ActividadCientificaTecnologica.comitesCTATipoEntidadAfiliacion, valorTipo),
                new Property(Variables.ActividadCientificaTecnologica.comitesCTATipoEntidadAfiliacionOtros, item.GetStringPorIDCampo("060.020.010.090"))
            ));
        }

        /// <summary>
        /// 060.020.030.000
        /// </summary>
        /// <param name="listadoDatos"></param>
        /// <returns></returns>
        public List<Entity> GetOrganizacionIDI(List<CvnItemBean> listadoDatos)
        {
            List<Entity> listado = new List<Entity>();

            List<CvnItemBean> listadoOrganizacionIDI = listadoDatos.Where(x => x.Code.Equals("060.020.030.000")).ToList();
            if (listadoOrganizacionIDI.Count > 0)
            {
                foreach (CvnItemBean item in listadoOrganizacionIDI)
                {
                    Entity entidadAux = new Entity();
                    entidadAux.properties = new List<Property>();
                    if (!string.IsNullOrEmpty(item.GetStringPorIDCampo("060.020.030.010")))
                    {
                        entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                            new Property(Variables.ActividadCientificaTecnologica.orgIDITituloActividad, item.GetStringPorIDCampo("060.020.030.010")),
                            new Property(Variables.ActividadCientificaTecnologica.orgIDITipoActividad, item.GetStringPorIDCampo("060.020.030.020")),
                            new Property(Variables.ActividadCientificaTecnologica.orgIDIPaisActividad, item.GetPaisPorIDCampo("060.020.030.030")),
                            new Property(Variables.ActividadCientificaTecnologica.orgIDICCAAActividad, item.GetRegionPorIDCampo("060.020.030.040")),
                            new Property(Variables.ActividadCientificaTecnologica.orgIDICiudadActividad, item.GetStringPorIDCampo("060.020.030.060")),
                            new Property(Variables.ActividadCientificaTecnologica.orgIDIPaisEntidadConvocante, item.GetPaisPorIDCampo("060.020.030.180")),
                            new Property(Variables.ActividadCientificaTecnologica.orgIDICCAAEntidadConvocante, item.GetRegionPorIDCampo("060.020.030.190")),
                            new Property(Variables.ActividadCientificaTecnologica.orgIDICiudadEntidadConvocante, item.GetStringPorIDCampo("060.020.030.200")),
                            new Property(Variables.ActividadCientificaTecnologica.orgIDIModoParticipacion, item.GetTipoParticipacionActividadPorIDCampo("060.020.030.110")),
                            new Property(Variables.ActividadCientificaTecnologica.orgIDIModoParticipacionOtros, item.GetStringPorIDCampo("060.020.030.120")),
                            new Property(Variables.ActividadCientificaTecnologica.orgIDIAmbitoReunion, item.GetGeographicRegionPorIDCampo("060.020.030.130")),
                            new Property(Variables.ActividadCientificaTecnologica.orgIDIAmbitoReunionOtros, item.GetStringPorIDCampo("060.020.030.140")),
                            new Property(Variables.ActividadCientificaTecnologica.orgIDIFechaInicio, item.GetStringDatetimePorIDCampo("060.020.030.160")),
                            new Property(Variables.ActividadCientificaTecnologica.orgIDIDuracionAnio, item.GetDurationAnioPorIDCampo("060.020.030.170")),
                            new Property(Variables.ActividadCientificaTecnologica.orgIDIDuracionMes, item.GetDurationMesPorIDCampo("060.020.030.170")),
                            new Property(Variables.ActividadCientificaTecnologica.orgIDIDuracionDia, item.GetDurationDiaPorIDCampo("060.020.030.170")),
                            new Property(Variables.ActividadCientificaTecnologica.orgIDIFechaFinalizacion, item.GetStringDatetimePorIDCampo("060.020.030.220")),
                            new Property(Variables.ActividadCientificaTecnologica.orgIDINumeroAsistentes, item.GetStringDoublePorIDCampo("060.020.030.150"))
                        ));
                        OrganizacionIDIEntidadConvocante(item, entidadAux);
                        listado.Add(entidadAux);
                    }
                }
            }
            return listado;
        }

        /// <summary>
        /// Inserta en <paramref name="entidadAux"/> los valores de <paramref name="item"/>,
        /// pertenecientes al tipo de Entidad Convocante.
        /// </summary>
        /// <param name="item">item</param>
        /// <param name="entidadAux">entidadAux</param>
        private void OrganizacionIDIEntidadConvocante(CvnItemBean item, Entity entidadAux)
        {
            //Añado la referencia si existe Entidad Realizacion
            UtilitySecciones.AniadirEntidad(mResourceApi, item.GetNameEntityBeanPorIDCampo("060.020.030.070"),
                Variables.ActividadCientificaTecnologica.orgIDIEntidadConvocanteNombre,
                Variables.ActividadCientificaTecnologica.orgIDIEntidadConvocante, entidadAux);

            //Añado otros, o el ID de una preseleccion
            string valorTipo = !string.IsNullOrEmpty(item.GetStringPorIDCampo("060.020.030.100")) ? mResourceApi.GraphsUrl + "items/organizationtype_OTHERS" : item.GetOrganizacionPorIDCampo("060.020.030.090");

            entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                new Property(Variables.ActividadCientificaTecnologica.orgIDITipoEntidadConvocante, valorTipo),
                new Property(Variables.ActividadCientificaTecnologica.orgIDITipoEntidadConvocanteOtros, item.GetStringPorIDCampo("060.020.030.100"))
            ));
        }

        /// <summary>
        /// 060.020.040.000
        /// </summary>
        /// <param name="listadoDatos"></param>
        /// <returns></returns>
        public List<Entity> GetGestionIDI(List<CvnItemBean> listadoDatos)
        {
            List<Entity> listado = new List<Entity>();

            List<CvnItemBean> listadoGestionIDI = listadoDatos.Where(x => x.Code.Equals("060.020.040.000")).ToList();
            if (listadoGestionIDI.Count > 0)
            {
                foreach (CvnItemBean item in listadoGestionIDI)
                {
                    Entity entidadAux = new Entity();
                    entidadAux.properties = new List<Property>();
                    if (!string.IsNullOrEmpty(item.GetStringPorIDCampo("060.020.040.060")))
                    {
                        entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                            new Property(Variables.ActividadCientificaTecnologica.gestionIDIFunciones, item.GetStringPorIDCampo("060.020.040.010")),
                            new Property(Variables.ActividadCientificaTecnologica.gestionIDIPaisEntidadRealizacion, item.GetPaisPorIDCampo("060.020.040.020")),
                            new Property(Variables.ActividadCientificaTecnologica.gestionIDICCAAEntidadRealizacion, item.GetRegionPorIDCampo("060.020.040.030")),
                            new Property(Variables.ActividadCientificaTecnologica.gestionIDICiudadEntidadRealizacion, item.GetStringPorIDCampo("060.020.040.050")),
                            new Property(Variables.ActividadCientificaTecnologica.gestionIDINombreActividad, item.GetStringPorIDCampo("060.020.040.060")),
                            new Property(Variables.ActividadCientificaTecnologica.gestionIDITipologiaGestion, item.GetTipoTipologiaGestionPorIDCampo("060.020.040.070")),
                            new Property(Variables.ActividadCientificaTecnologica.gestionIDITipologiaGestionOtros, item.GetStringPorIDCampo("060.020.040.080")),
                            new Property(Variables.ActividadCientificaTecnologica.gestionIDIEntornoFechaInicio, item.GetStringDatetimePorIDCampo("060.020.040.130")),
                            new Property(Variables.ActividadCientificaTecnologica.gestionIDIEntornoDuracionAnio, item.GetDurationAnioPorIDCampo("060.020.040.140")),
                            new Property(Variables.ActividadCientificaTecnologica.gestionIDIEntornoDuracionMes, item.GetDurationMesPorIDCampo("060.020.040.140")),
                            new Property(Variables.ActividadCientificaTecnologica.gestionIDIEntornoDuracionDia, item.GetDurationDiaPorIDCampo("060.020.040.140")),
                            new Property(Variables.ActividadCientificaTecnologica.gestionIDISistemaAcceso, item.GetSistemaActividadPorIDCampo("060.020.040.150")),
                            new Property(Variables.ActividadCientificaTecnologica.gestionIDISistemaAccesoOtros, item.GetStringPorIDCampo("060.020.040.160")),
                            new Property(Variables.ActividadCientificaTecnologica.gestionIDIPromedioPresupuesto, item.GetStringDoublePorIDCampo("060.020.040.170")),
                            new Property(Variables.ActividadCientificaTecnologica.gestionIDINumPersonas, item.GetStringDoublePorIDCampo("060.020.040.180")),
                            new Property(Variables.ActividadCientificaTecnologica.gestionIDIObjetivosEvento, item.GetStringPorIDCampo("060.020.040.240")),
                            new Property(Variables.ActividadCientificaTecnologica.gestionIDIPerfilGrupo, item.GetTipoPerfilGrupoPorIDCampo("060.020.040.190")),
                            new Property(Variables.ActividadCientificaTecnologica.gestionIDIAmbitoTerritorial, item.GetGeographicRegionPorIDCampo("060.020.040.200")),
                            new Property(Variables.ActividadCientificaTecnologica.gestionIDIAmbitoTerritorialOtros, item.GetStringPorIDCampo("060.020.040.210")),
                            new Property(Variables.ActividadCientificaTecnologica.gestionIDITareasConcretas, item.GetStringPorIDCampo("060.020.040.220"))
                        ));
                        GestionIDIEntidadRealizacion(item, entidadAux);
                        //GestionIDIPalabrasClave(item, entidadAux);

                        listado.Add(entidadAux);
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
        private void GestionIDIPalabrasClave(CvnItemBean item, Entity entidadAux)
        {
            List<CvnItemBeanCvnString> listadoPalabrasClave = item.GetListaElementosPorIDCampo<CvnItemBeanCvnString>("060.020.040.230");

            foreach (CvnItemBeanCvnString palabra in listadoPalabrasClave)
            {
                entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                    new Property(Variables.ActividadCientificaTecnologica.gestionIDIPalabrasClave, palabra.Value)//TODO cambio valores (falta tesauro)
                ));
            }
        }

        /// <summary>
        /// Inserta en <paramref name="entidadAux"/> los valores de <paramref name="item"/>,
        /// pertenecientes al tipo de Entidad de Realización.
        /// </summary>
        /// <param name="item">item</param>
        /// <param name="entidadAux">entidadAux</param>
        private void GestionIDIEntidadRealizacion(CvnItemBean item, Entity entidadAux)
        {
            //Añado la referencia si existe Entidad Realizacion
            UtilitySecciones.AniadirEntidad(mResourceApi, item.GetNameEntityBeanPorIDCampo("060.020.040.090"),
                Variables.ActividadCientificaTecnologica.gestionIDIEntornoEntidadRealizacionNombre,
                Variables.ActividadCientificaTecnologica.gestionIDIEntornoEntidadRealizacion, entidadAux);

            //Añado otros, o el ID de una preseleccion
            string valorTipo = !string.IsNullOrEmpty(item.GetStringPorIDCampo("060.020.040.120")) ? mResourceApi.GraphsUrl + "items/organizationtype_OTHERS" : item.GetOrganizacionPorIDCampo("060.020.040.110");

            entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                new Property(Variables.ActividadCientificaTecnologica.gestionIDIEntornoTipoEntidadRealizacion, valorTipo),
                new Property(Variables.ActividadCientificaTecnologica.gestionIDIEntornoTipoEntidadRealizacionOtros, item.GetStringPorIDCampo("060.020.040.120"))
            ));
        }


        /// <summary>
        /// 060.020.050.000
        /// </summary>
        /// <param name="listadoDatos"></param>
        /// <returns></returns>
        private List<Entity> GetForosComites(List<CvnItemBean> listadoDatos)
        {
            List<Entity> listado = new List<Entity>();

            List<CvnItemBean> listadoForosComites = listadoDatos.Where(x => x.Code.Equals("060.020.050.000")).ToList();
            if (listadoForosComites.Count > 0)
            {
                foreach (CvnItemBean item in listadoForosComites)
                {
                    Entity entidadAux = new Entity();
                    entidadAux.properties = new List<Property>();
                    if (!string.IsNullOrEmpty(item.GetStringPorIDCampo("060.020.050.010")))
                    {
                        entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                            new Property(Variables.ActividadCientificaTecnologica.forosComitesNombre, item.GetStringPorIDCampo("060.020.050.010")),
                            new Property(Variables.ActividadCientificaTecnologica.forosComitesPaisEntidadRealizacion, item.GetPaisPorIDCampo("060.020.050.020")),
                            new Property(Variables.ActividadCientificaTecnologica.forosComitesCCAAEntidadRealizacion, item.GetRegionPorIDCampo("060.020.050.030")),
                            new Property(Variables.ActividadCientificaTecnologica.forosComitesCiudadEntidadRealizacion, item.GetStringPorIDCampo("060.020.050.050")),
                            new Property(Variables.ActividadCientificaTecnologica.forosComitesPaisEntidadOrganizadora, item.GetPaisPorIDCampo("060.020.050.170")),
                            new Property(Variables.ActividadCientificaTecnologica.forosComitesCCAAEntidadOrganizadora, item.GetRegionPorIDCampo("060.020.050.180")),
                            new Property(Variables.ActividadCientificaTecnologica.forosComitesCiudadEntidadOrganizadora, item.GetStringPorIDCampo("060.020.050.190")),
                            new Property(Variables.ActividadCientificaTecnologica.forosComitesCategoriaProfesional, item.GetStringPorIDCampo("060.020.050.100")),
                            new Property(Variables.ActividadCientificaTecnologica.forosComitesPaisEntidadRepresentada, item.GetPaisPorIDCampo("060.020.050.200")),
                            new Property(Variables.ActividadCientificaTecnologica.forosComitesCCAAEntidadRepresentada, item.GetRegionPorIDCampo("060.020.050.210")),
                            new Property(Variables.ActividadCientificaTecnologica.forosComitesCiudadEntidadRepresentada, item.GetStringPorIDCampo("060.020.050.220")),
                            new Property(Variables.ActividadCientificaTecnologica.forosComitesFechaInicio, item.GetStringDatetimePorIDCampo("060.020.050.150")),
                            new Property(Variables.ActividadCientificaTecnologica.forosComitesFechaFinalizacion, item.GetStringDatetimePorIDCampo("060.020.050.160"))
                        ));
                        ForosComitesEntidadOrganizadoraORepresentada(item, entidadAux);

                        listado.Add(entidadAux);
                    }
                }
            }
            return listado;
        }

        /// <summary>
        /// Inserta en <paramref name="entidadAux"/> los valores de <paramref name="item"/>,
        /// pertenecientes al tipo de Entidad Organizadora.
        /// </summary>
        /// <param name="item">item</param>
        /// <param name="entidadAux">entidadAux</param>
        private void ForosComitesEntidadOrganizadoraORepresentada(CvnItemBean item, Entity entidadAux)
        {
            //Añado la referencia si existe Entidad Organizadora
            UtilitySecciones.AniadirEntidad(mResourceApi, item.GetNameEntityBeanPorIDCampo("060.020.050.060"),
                Variables.ActividadCientificaTecnologica.forosComitesEntidadOrganizadoraNombre,
                Variables.ActividadCientificaTecnologica.forosComitesEntidadOrganizadora, entidadAux);

            //Añado otros Entidad Organizadora, o el ID de una preseleccion
            string valorTipoEO = !string.IsNullOrEmpty(item.GetStringPorIDCampo("060.020.050.090")) ? mResourceApi.GraphsUrl + "items/organizationtype_OTHERS" : item.GetOrganizacionPorIDCampo("060.020.050.080");

            entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                new Property(Variables.ActividadCientificaTecnologica.forosComitesTipoEntidadOrganizadora, valorTipoEO),
                new Property(Variables.ActividadCientificaTecnologica.forosComitesTipoEntidadOrganizadoraOtros, item.GetStringPorIDCampo("060.020.050.090"))
            ));


            //Añado la referencia si existe Entidad Representada
            UtilitySecciones.AniadirEntidad(mResourceApi, item.GetNameEntityBeanPorIDCampo("060.020.050.110"),
                Variables.ActividadCientificaTecnologica.forosComitesOrganismoRepresentadoNombre,
                Variables.ActividadCientificaTecnologica.forosComitesOrganismoRepresentado, entidadAux);

            //Añado otros Entidad Representada, o el ID de una preseleccion
            string valorTipoER = !string.IsNullOrEmpty(item.GetStringPorIDCampo("060.020.050.140")) ? mResourceApi.GraphsUrl + "items/organizationtype_OTHERS" : item.GetOrganizacionPorIDCampo("060.020.050.130");

            entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                new Property(Variables.ActividadCientificaTecnologica.forosComitesTipoOrganismoRepresentado, valorTipoER),
                new Property(Variables.ActividadCientificaTecnologica.forosComitesTipoOrganismoRepresentadoOtros, item.GetStringPorIDCampo("060.020.050.140"))
            ));
        }

        /// <summary>
        /// 060.020.060.000
        /// </summary>
        /// <param name="listadoDatos"></param>
        /// <returns></returns>
        private List<Entity> GetEvalRevIDI(List<CvnItemBean> listadoDatos)
        {
            List<Entity> listado = new List<Entity>();

            List<CvnItemBean> listadoEvalRevIDI = listadoDatos.Where(x => x.Code.Equals("060.020.060.000")).ToList();
            if (listadoEvalRevIDI.Count > 0)
            {
                foreach (CvnItemBean item in listadoEvalRevIDI)
                {
                    Entity entidadAux = new Entity();
                    entidadAux.properties = new List<Property>();
                    if (!string.IsNullOrEmpty(item.GetStringPorIDCampo("060.020.060.010")))
                    {
                        entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                            new Property(Variables.ActividadCientificaTecnologica.evalRevIDIFunciones, item.GetStringPorIDCampo("060.020.060.010")),
                            new Property(Variables.ActividadCientificaTecnologica.evalRevIDINombre, item.GetStringPorIDCampo("060.020.060.020")),
                            new Property(Variables.ActividadCientificaTecnologica.evalRevIDIPaisEntidadRealizacion, item.GetPaisPorIDCampo("060.020.060.030")),
                            new Property(Variables.ActividadCientificaTecnologica.evalRevIDICCAAEntidadRealizacion, item.GetRegionPorIDCampo("060.020.060.040")),
                            new Property(Variables.ActividadCientificaTecnologica.evalRevIDICiudadEntidadRealizacion, item.GetStringPorIDCampo("060.020.060.190")),
                            new Property(Variables.ActividadCientificaTecnologica.evalRevIDIModalidadActividad, item.GetModalidadActividadPorIDCampo("060.020.060.060")),
                            new Property(Variables.ActividadCientificaTecnologica.evalRevIDIModalidadActividadOtros, item.GetStringPorIDCampo("060.020.060.070")),
                            new Property(Variables.ActividadCientificaTecnologica.evalRevIDIFechaInicio, item.GetStringDatetimePorIDCampo("060.020.060.120")),
                            new Property(Variables.ActividadCientificaTecnologica.evalRevIDIFechaFinalizacion, item.GetStringDatetimePorIDCampo("060.020.060.130")),
                            new Property(Variables.ActividadCientificaTecnologica.evalRevIDIFrecuenciaActividad, item.GetStringDoublePorIDCampo("060.020.060.140")),
                            new Property(Variables.ActividadCientificaTecnologica.evalRevIDISistemaAcceso, item.GetSistemaActividadPorIDCampo("060.020.060.150")),
                            new Property(Variables.ActividadCientificaTecnologica.evalRevIDISistemaAccesoOtros, item.GetStringPorIDCampo("060.020.060.160")),
                            new Property(Variables.ActividadCientificaTecnologica.evalRevIDIAmbito, item.GetGeographicRegionPorIDCampo("060.020.060.170")),
                            new Property(Variables.ActividadCientificaTecnologica.evalRevIDIAmbitoOtros, item.GetStringPorIDCampo("060.020.060.180"))
                        ));
                        EvalRevIDIEntidadRealizacion(item, entidadAux);

                        listado.Add(entidadAux);
                    }
                }
            }
            return listado;
        }

        /// <summary>
        /// Inserta en <paramref name="entidadAux"/> los valores de <paramref name="item"/>,
        /// pertenecientes al tipo de Entidad de Realización.
        /// </summary>
        /// <param name="item">item</param>
        /// <param name="entidadAux">entidadAux</param>
        private void EvalRevIDIEntidadRealizacion(CvnItemBean item, Entity entidadAux)
        {

            //Añado la referencia si existe
            UtilitySecciones.AniadirEntidad(mResourceApi, item.GetNameEntityBeanPorIDCampo("060.020.060.080"),
                Variables.ActividadCientificaTecnologica.evalRevIDIEntidadNombre,
                Variables.ActividadCientificaTecnologica.evalRevIDIEntidad, entidadAux);

            //Añado otros, o el ID de una preseleccion
            string valorTipoER = !string.IsNullOrEmpty(item.GetStringPorIDCampo("060.020.060.110")) ? mResourceApi.GraphsUrl + "items/organizationtype_OTHERS" : item.GetOrganizacionPorIDCampo("060.020.060.100");

            entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                new Property(Variables.ActividadCientificaTecnologica.evalRevIDITipoEntidad, valorTipoER),
                new Property(Variables.ActividadCientificaTecnologica.evalRevIDITipoEntidadOtros, item.GetStringPorIDCampo("060.020.060.110"))
            ));
        }

        /// <summary>
        /// 060.010.050.000
        /// </summary>
        /// <param name="listadoDatos"></param>
        /// <returns></returns>
        private List<Entity> GetEstanciasIDI(List<CvnItemBean> listadoDatos)
        {
            List<Entity> listado = new List<Entity>();

            List<CvnItemBean> listadoEstanciasIDI = listadoDatos.Where(x => x.Code.Equals("060.010.050.000")).ToList();
            if (listadoEstanciasIDI.Count > 0)
            {
                foreach (CvnItemBean item in listadoEstanciasIDI)
                {
                    Entity entidadAux = new Entity();
                    entidadAux.properties = new List<Property>();
                    if (!string.IsNullOrEmpty(item.GetStringPorIDCampo("060.010.050.210")))
                    {
                        entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                            new Property(Variables.ActividadCientificaTecnologica.estanciasIDIPaisEntidadRealizacion, item.GetPaisPorIDCampo("060.010.050.050")),
                            new Property(Variables.ActividadCientificaTecnologica.estanciasIDICCAAEntidadRealizacion, item.GetRegionPorIDCampo("060.010.050.060")),
                            new Property(Variables.ActividadCientificaTecnologica.estanciasIDICiudadEntidadRealizacion, item.GetStringPorIDCampo("060.010.050.080")),
                            new Property(Variables.ActividadCientificaTecnologica.estanciasIDIFechaInicioEntidadRealizacion, item.GetStringDatetimePorIDCampo("060.010.050.090")),
                            new Property(Variables.ActividadCientificaTecnologica.estanciasIDIDuracionAnioEntidadRealizacion, item.GetDurationAnioPorIDCampo("060.010.050.100")),
                            new Property(Variables.ActividadCientificaTecnologica.estanciasIDIDuracionMesEntidadRealizacion, item.GetDurationMesPorIDCampo("060.010.050.100")),
                            new Property(Variables.ActividadCientificaTecnologica.estanciasIDIDuracionDiaEntidadRealizacion, item.GetDurationDiaPorIDCampo("060.010.050.100")),
                            new Property(Variables.ActividadCientificaTecnologica.estanciasIDIObjetivoEstancia, item.GetObjetivoPorIDCampo("060.010.050.110")),
                            new Property(Variables.ActividadCientificaTecnologica.estanciasIDIObjetivoEstanciaOtros, item.GetStringPorIDCampo("060.010.050.120")),
                            new Property(Variables.ActividadCientificaTecnologica.estanciasIDIPaisEntidadFinanciadora, item.GetPaisPorIDCampo("060.010.050.250")),
                            new Property(Variables.ActividadCientificaTecnologica.estanciasIDICCAAEntidadFinanciadora, item.GetRegionPorIDCampo("060.010.050.260")),
                            new Property(Variables.ActividadCientificaTecnologica.estanciasIDICiudadEntidadFinanciadora, item.GetStringPorIDCampo("060.010.050.270")),
                            new Property(Variables.ActividadCientificaTecnologica.estanciasIDINombrePrograma, item.GetStringPorIDCampo("060.010.050.200")),
                            new Property(Variables.ActividadCientificaTecnologica.estanciasIDITareasContrastables, item.GetStringPorIDCampo("060.010.050.210")),
                            new Property(Variables.ActividadCientificaTecnologica.estanciasIDICapacidadesAdquiridas, item.GetStringPorIDCampo("060.010.050.220")),
                            new Property(Variables.ActividadCientificaTecnologica.estanciasIDIResultadosRelevantes, item.GetStringPorIDCampo("060.010.050.230")),
                            new Property(Variables.ActividadCientificaTecnologica.estanciasIDIFacultadEscuela, item.GetNameEntityBeanPorIDCampo("060.010.050.280")),
                            new Property(Variables.ActividadCientificaTecnologica.estanciasIDIFechaFinalizacion, item.GetStringDatetimePorIDCampo("060.010.050.290"))
                        ));
                        EstanciasIDICodigosUnesco(item, entidadAux);
                        EstanciasIDIEntidadRealizacion(item, entidadAux);
                        //EstanciasIDIPalabrasClave(item, entidadAux);

                        listado.Add(entidadAux);
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
        private void EstanciasIDIPalabrasClave(CvnItemBean item, Entity entidadAux)
        {
            List<CvnItemBeanCvnString> listadoPalabrasClave = item.GetListaElementosPorIDCampo<CvnItemBeanCvnString>("060.010.050.240");

            foreach (CvnItemBeanCvnString palabra in listadoPalabrasClave)
            {
                entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                    new Property(Variables.ActividadCientificaTecnologica.estanciasIDIPalabrasClave, palabra.Value)//TODO cambio valores (falta tesauro)
                ));
            }
        }

        /// <summary>
        /// Inserta en <paramref name="entidadAux"/> los valores de <paramref name="item"/>,
        /// pertenecientes a los Códigos UNESCO.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="entidadAux"></param>
        private void EstanciasIDICodigosUnesco(CvnItemBean item, Entity entidadAux)
        {
            List<CvnItemBeanCvnString> listadoCodUnescoPrimaria = item.GetListaElementosPorIDCampo<CvnItemBeanCvnString>("060.010.050.130");
            List<CvnItemBeanCvnString> listadoCodUnescoSecundaria = item.GetListaElementosPorIDCampo<CvnItemBeanCvnString>("060.010.050.140");
            List<CvnItemBeanCvnString> listadoCodUnescoTerciaria = item.GetListaElementosPorIDCampo<CvnItemBeanCvnString>("060.010.050.150");

            foreach (CvnItemBeanCvnString codigo in listadoCodUnescoPrimaria)
            {
                string entityPartAux = Guid.NewGuid().ToString() + "@@@";
                List<string> listadoCodigos = Utility.GetPadresCodUnesco(codigo);
                //Añado Codigo UNESCO
                foreach (string codigolista in listadoCodigos)
                {
                    Property propertyCodUnescoPrimaria = entidadAux.properties.FirstOrDefault(x => x.prop == Variables.ActividadCientificaTecnologica.estanciasIDICodUnescoPrimaria);

                    string valorCodigo = StringGNOSSID(entityPartAux, Utility.GetCodUnescoIDCampo(codigolista));
                    string propiedadCodigo = Variables.ActividadCientificaTecnologica.estanciasIDICodUnescoPrimaria;
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
                    Property propertyCodUnescoSecundaria = entidadAux.properties.FirstOrDefault(x => x.prop == Variables.ActividadCientificaTecnologica.estanciasIDICodUnescoSecundaria);

                    string valorCodigo = StringGNOSSID(entityPartAux, Utility.GetCodUnescoIDCampo(codigolista));
                    string propiedadCodigo = Variables.ActividadCientificaTecnologica.estanciasIDICodUnescoSecundaria;
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
                    Property propertyCodUnescoTerciaria = entidadAux.properties.FirstOrDefault(x => x.prop == Variables.ActividadCientificaTecnologica.estanciasIDICodUnescoTerciaria);

                    string valorCodigo = StringGNOSSID(entityPartAux, Utility.GetCodUnescoIDCampo(codigolista));
                    string propiedadCodigo = Variables.ActividadCientificaTecnologica.estanciasIDICodUnescoTerciaria;
                    UtilitySecciones.CheckProperty(propertyCodUnescoTerciaria, entidadAux, valorCodigo, propiedadCodigo);
                }
            }
        }

        /// <summary>
        /// Inserta en <paramref name="entidadAux"/> los valores de <paramref name="item"/>,
        /// pertenecientes al tipo de Entidad de Realización.
        /// </summary>
        /// <param name="item">item</param>
        /// <param name="entidadAux">entidadAux</param>
        private void EstanciasIDIEntidadRealizacion(CvnItemBean item, Entity entidadAux)
        {
            //Añado la referencia si existe
            UtilitySecciones.AniadirEntidad(mResourceApi, item.GetNameEntityBeanPorIDCampo("060.010.050.010"),
                Variables.ActividadCientificaTecnologica.estanciasIDIEntidadRealizacionNombre,
                Variables.ActividadCientificaTecnologica.estanciasIDIEntidadRealizacion, entidadAux);

            //Añado otros Entidad Realizacion, o el ID de una preseleccion
            string valorTipoER = !string.IsNullOrEmpty(item.GetStringPorIDCampo("060.010.050.040")) ? mResourceApi.GraphsUrl + "items/organizationtype_OTHERS" : item.GetOrganizacionPorIDCampo("060.010.050.030");

            entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                new Property(Variables.ActividadCientificaTecnologica.estanciasIDITipoEntidadRealizacion, valorTipoER),
                new Property(Variables.ActividadCientificaTecnologica.estanciasIDITipoEntidadRealizacionOtros, item.GetStringPorIDCampo("060.010.050.040"))
            ));

            //Añado otros Entidad Financiacion, o el ID de una preseleccion
            string valorTipoEF = !string.IsNullOrEmpty(item.GetStringPorIDCampo("060.010.050.190")) ? mResourceApi.GraphsUrl + "items/organizationtype_OTHERS" : item.GetOrganizacionPorIDCampo("060.010.050.180");

            //Añado la referencia si existe
            UtilitySecciones.AniadirEntidad(mResourceApi, item.GetNameEntityBeanPorIDCampo("060.010.050.160"),
                Variables.ActividadCientificaTecnologica.estanciasIDIEntidadFinanciadoraNombre,
                Variables.ActividadCientificaTecnologica.estanciasIDIEntidadFinanciadora, entidadAux);

            //Añado tipo entidad
            entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                new Property(Variables.ActividadCientificaTecnologica.estanciasIDITipoEntidadFinanciadora, valorTipoEF),
                new Property(Variables.ActividadCientificaTecnologica.estanciasIDITipoEntidadFinanciadoraOtros, item.GetStringPorIDCampo("060.010.050.190"))
            ));
        }

        /// <summary>
        /// 060.030.010.000
        /// </summary>
        /// <param name="listadoDatos"></param>
        /// <returns></returns>
        private List<Entity> GetAyudasBecas(List<CvnItemBean> listadoDatos)
        {
            List<Entity> listado = new List<Entity>();

            List<CvnItemBean> listadoAyudasBecas = listadoDatos.Where(x => x.Code.Equals("060.030.010.000")).ToList();
            if (listadoAyudasBecas.Count > 0)
            {
                foreach (CvnItemBean item in listadoAyudasBecas)
                {
                    Entity entidadAux = new Entity();
                    entidadAux.properties = new List<Property>();
                    if (!string.IsNullOrEmpty(item.GetStringPorIDCampo("060.030.010.010")))
                    {
                        entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                            new Property(Variables.ActividadCientificaTecnologica.ayudasBecasNombre, item.GetStringPorIDCampo("060.030.010.010")),
                            new Property(Variables.ActividadCientificaTecnologica.ayudasBecasPaisConcede, item.GetPaisPorIDCampo("060.030.010.020")),
                            new Property(Variables.ActividadCientificaTecnologica.ayudasBecasCCAAConcede, item.GetRegionPorIDCampo("060.030.010.030")),
                            new Property(Variables.ActividadCientificaTecnologica.ayudasBecasCiudadConcede, item.GetStringPorIDCampo("060.030.010.150")),
                            new Property(Variables.ActividadCientificaTecnologica.ayudasBecasFinalidad, item.GetFinalidadPorIDCampo("060.030.010.060")),
                            new Property(Variables.ActividadCientificaTecnologica.ayudasBecasFinalidadOtros, item.GetStringPorIDCampo("060.030.010.070")),
                            new Property(Variables.ActividadCientificaTecnologica.ayudasBecasImporte, item.GetStringDoublePorIDCampo("060.030.010.120")),
                            new Property(Variables.ActividadCientificaTecnologica.ayudasBecasFechaConcesion, item.GetStringDatetimePorIDCampo("060.030.010.130")),
                            new Property(Variables.ActividadCientificaTecnologica.ayudasBecasDuracionAnio, item.GetDurationAnioPorIDCampo("060.030.010.140")),
                            new Property(Variables.ActividadCientificaTecnologica.ayudasBecasDuracionMes, item.GetDurationMesPorIDCampo("060.030.010.140")),
                            new Property(Variables.ActividadCientificaTecnologica.ayudasBecasDuracionDia, item.GetDurationDiaPorIDCampo("060.030.010.140")),
                            new Property(Variables.ActividadCientificaTecnologica.ayudasBecasFechaFinalizacion, item.GetStringDatetimePorIDCampo("060.030.010.160")),
                            new Property(Variables.ActividadCientificaTecnologica.ayudasBecasFacultadEscuela, item.GetNameEntityBeanPorIDCampo("060.030.010.170"))
                         ));
                        AyudasBecasEntidadConcede(item, entidadAux);
                        AyudasBecasEntidadRealizacion(item, entidadAux);
                        //AyudaBecasPalabrasClave(item, entidadAux);

                        listado.Add(entidadAux);
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
        private void AyudaBecasPalabrasClave(CvnItemBean item, Entity entidadAux)
        {                
            List<CvnItemBeanCvnString> listadoPalabrasClave = item.GetListaElementosPorIDCampo<CvnItemBeanCvnString>("060.030.010.050");

            foreach (CvnItemBeanCvnString palabra in listadoPalabrasClave)
            {
                entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                    new Property(Variables.ActividadCientificaTecnologica.ayudasBecasPalabrasClave, palabra.Value)//TODO cambio valores (falta tesauro)
                ));
            }
        }

        /// <summary>
        /// Inserta en <paramref name="entidadAux"/> los valores de <paramref name="item"/>,
        /// pertenecientes al tipo de Entidad concesionaria.
        /// </summary>
        /// <param name="item">item</param>
        /// <param name="entidadAux">entidadAux</param>
        private void AyudasBecasEntidadRealizacion(CvnItemBean item, Entity entidadAux)
        {
            //Si no esta Entidad no añado datos
            if (string.IsNullOrEmpty(item.GetNameEntityBeanPorIDCampo("060.030.010.180"))) { return; }

            //Añado la referencia si existe
            UtilitySecciones.AniadirEntidad(mResourceApi, item.GetNameEntityBeanPorIDCampo("060.030.010.180"),
                Variables.ActividadCientificaTecnologica.ayudasBecasEntidadRealizacionNombre,
                Variables.ActividadCientificaTecnologica.ayudasBecasEntidadRealizacion, entidadAux);
        }
        
        /// <summary>
        /// Inserta en <paramref name="entidadAux"/> los valores de <paramref name="item"/>,
        /// pertenecientes al tipo de Entidad concesionaria.
        /// </summary>
        /// <param name="item">item</param>
        /// <param name="entidadAux">entidadAux</param>
        private void AyudasBecasEntidadConcede(CvnItemBean item, Entity entidadAux)
        {
            //Si no esta Entidad no añado datos
            if (string.IsNullOrEmpty(item.GetNameEntityBeanPorIDCampo("060.030.010.080"))) { return; }

            //Añado la referencia si existe
            UtilitySecciones.AniadirEntidad(mResourceApi, item.GetNameEntityBeanPorIDCampo("060.030.010.080"),
                Variables.ActividadCientificaTecnologica.ayudasBecasEntidadConcedeNombre,
                Variables.ActividadCientificaTecnologica.ayudasBecasEntidadConcede, entidadAux);

            //Añado otros, o el ID de una preseleccion
            string valorTipoEntidad = !string.IsNullOrEmpty(item.GetStringPorIDCampo("060.030.010.110")) ? mResourceApi.GraphsUrl + "items/organizationtype_OTHERS" : item.GetOrganizacionPorIDCampo("060.030.010.100");

            entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                new Property(Variables.ActividadCientificaTecnologica.ayudasBecasTipoEntidadConcede, valorTipoEntidad),
                new Property(Variables.ActividadCientificaTecnologica.ayudasBecasTipoEntidadConcedeOtros, item.GetStringPorIDCampo("060.030.010.110"))
            ));
        }

        /// <summary>
        /// 060.020.020.000
        /// </summary>
        /// <param name="listadoDatos"></param>
        private List<Entity> GetOtrosModosColaboracion(List<CvnItemBean> listadoDatos)
        {
            List<Entity> listado = new List<Entity>();

            List<CvnItemBean> listadoOtrosModosColaboracion = listadoDatos.Where(x => x.Code.Equals("060.020.020.000")).ToList();
            if (listadoOtrosModosColaboracion.Count > 0)
            {
                foreach (CvnItemBean item in listadoOtrosModosColaboracion)
                {
                    Entity entidadAux = new Entity();
                    entidadAux.properties = new List<Property>();
                    if (!string.IsNullOrEmpty(item.GetStringPorIDCampo("060.020.020.140")))
                    {
                        entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                            new Property(Variables.ActividadCientificaTecnologica.otrasColabModoRelacion, item.GetRelacionPorIDCampo("060.020.020.010")),
                            new Property(Variables.ActividadCientificaTecnologica.otrasColabModoRelacionOtros, item.GetStringPorIDCampo("060.020.020.020")),
                            new Property(Variables.ActividadCientificaTecnologica.otrasColabPaisRadicacion, item.GetPaisPorIDCampo("060.020.020.030")),
                            new Property(Variables.ActividadCientificaTecnologica.otrasColabCCAARadicacion, item.GetRegionPorIDCampo("060.020.020.040")),
                            new Property(Variables.ActividadCientificaTecnologica.otrasColabCiudadRadicacion, item.GetStringPorIDCampo("060.020.020.060")),
                            new Property(Variables.ActividadCientificaTecnologica.otrasColabFechaInicio, item.GetStringDatetimePorIDCampo("060.020.020.120")),
                            new Property(Variables.ActividadCientificaTecnologica.otrasColabDuracionAnio, item.GetDurationAnioPorIDCampo("060.020.020.130")),
                            new Property(Variables.ActividadCientificaTecnologica.otrasColabDuracionMes, item.GetDurationMesPorIDCampo("060.020.020.130")),
                            new Property(Variables.ActividadCientificaTecnologica.otrasColabDuracionDia, item.GetDurationDiaPorIDCampo("060.020.020.130")),
                            new Property(Variables.ActividadCientificaTecnologica.otrasColabDescripcionColaboracion, item.GetStringPorIDCampo("060.020.020.140")),
                            new Property(Variables.ActividadCientificaTecnologica.otrasColabResultadosRelevantes, item.GetStringPorIDCampo("060.020.020.150"))
                         ));
                        //OtrosModosColaboracionAutores(item, entidadAux);
                        OtrosModosColaboracionEntidadesParticipantes(item, entidadAux);
                        //OtrosModosColaboracionPalabrasClave(item, entidadAux);

                        listado.Add(entidadAux);
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
        private void OtrosModosColaboracionPalabrasClave(CvnItemBean item, Entity entidadAux)
        {                            
            List<CvnItemBeanCvnString> listadoPalabrasClave = item.GetListaElementosPorIDCampo<CvnItemBeanCvnString>("060.020.020.160");

            foreach (CvnItemBeanCvnString palabra in listadoPalabrasClave)
            {
                entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                    new Property(Variables.ActividadCientificaTecnologica.otrasColabPalabrasClave, palabra.Value)//TODO cambio valores (falta tesauro)
                    ));
            }
        }

        private void OtrosModosColaboracionAutores(CvnItemBean item, Entity entidadAux)
        {
            List<CvnItemBeanCvnAuthorBean> listadoAutores = item.GetListaElementosPorIDCampo<CvnItemBeanCvnAuthorBean>("060.020.020.070");

            foreach (CvnItemBeanCvnAuthorBean autor in listadoAutores)
            {
                if (!string.IsNullOrEmpty(autor.GivenName))
                {
                    string entityPartAux = Guid.NewGuid().ToString() + "@@@";
                    entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                        new Property(Variables.ActividadCientificaTecnologica.otrasColabFirmaInvestigador, StringGNOSSID(entityPartAux, autor.Signature.ToString())),
                        new Property(Variables.ActividadCientificaTecnologica.otrasColabNombreInvestigador, StringGNOSSID(entityPartAux, autor.GivenName)),
                        new Property(Variables.ActividadCientificaTecnologica.otrasColabPrimApellInvestigador, StringGNOSSID(entityPartAux, autor.CvnFamilyNameBean?.FirstFamilyName)),
                        new Property(Variables.ActividadCientificaTecnologica.otrasColabSegApellInvestigador, StringGNOSSID(entityPartAux, autor.CvnFamilyNameBean?.SecondFamilyName))                        
                    ));
                }
            }
        }

        /// <summary>
        /// Inserta en <paramref name="entidadAux"/> los valores de las Entidades Participantes,
        /// pertenecientes al listado <paramref name="listadoEntidadParticipante"/>.
        /// </summary>
        /// <param name="listadoEntidadParticipante">listadoEntidadParticipante</param>
        /// <param name="entidadAux">entidadAux</param>
        private void OtrosModosColaboracionEntidadesParticipantes(CvnItemBean item, Entity entidadAux)
        {
            List<CvnItemBeanCvnCodeGroup> listadoEntidadParticipante = item.GetListaElementosPorIDCampo<CvnItemBeanCvnCodeGroup>("060.020.020.080");

            foreach (CvnItemBeanCvnCodeGroup entidadParticipante in listadoEntidadParticipante)
            {
                if (!string.IsNullOrEmpty(entidadParticipante.GetNameEntityBeanCvnCodeGroup("060.020.020.080")))
                {
                    string entityPartAux = Guid.NewGuid().ToString() + "@@@";
                    Property propertyEntidadParticipante = entidadAux.properties.FirstOrDefault(x => x.prop == Variables.ActividadCientificaTecnologica.otrasColabEntidadesParticipantes);
                    Property propertyEntidadParticipanteNombre = entidadAux.properties.FirstOrDefault(x => x.prop == Variables.ActividadCientificaTecnologica.otrasColabEntidadesParticipantesNombre);
                    Property propertyPaisEntidadParticipante = entidadAux.properties.FirstOrDefault(x => x.prop == Variables.ActividadCientificaTecnologica.otrasColabPaisEntidadParticipante);
                    Property propertyRegionEntidadParticipante = entidadAux.properties.FirstOrDefault(x => x.prop == Variables.ActividadCientificaTecnologica.otrasColabCCAAEntidadParticipante);
                    Property propertyCiudadEntidadParticipante = entidadAux.properties.FirstOrDefault(x => x.prop == Variables.ActividadCientificaTecnologica.otrasColabCiudadEntidadParticipante);

                    //Añado titulo entidad participante
                    string valorTituloEP = StringGNOSSID(entityPartAux, entidadParticipante.GetNameEntityBeanCvnCodeGroup("060.020.020.080"));
                    string propiedadTituloEP = Variables.ActividadCientificaTecnologica.otrasColabEntidadesParticipantesNombre;
                    UtilitySecciones.CheckProperty(propertyEntidadParticipanteNombre, entidadAux, valorTituloEP, propiedadTituloEP);

                    //Añado referencia entidad participante
                    string nombreEP = entidadParticipante.GetNameEntityBeanCvnCodeGroup("060.020.020.080");
                    if (string.IsNullOrEmpty(nombreEP))
                    {
                        string referenciaEP = UtilitySecciones.GetOrganizacionPorNombre(mResourceApi, nombreEP);
                        string valorEP = StringGNOSSID(entityPartAux, referenciaEP);
                        string propiedadEP = Variables.ActividadCientificaTecnologica.otrasColabEntidadesParticipantes;
                        UtilitySecciones.CheckProperty(propertyEntidadParticipante, entidadAux, valorEP, propiedadEP);
                    }

                    //Añado pais entidad participante
                    string valorPaisEP = StringGNOSSID(entityPartAux, entidadParticipante.GetPaisPorIDCampo("060.020.020.170"));
                    string propiedadPaisEP = Variables.ActividadCientificaTecnologica.otrasColabPaisEntidadParticipante;
                    UtilitySecciones.CheckProperty(propertyPaisEntidadParticipante, entidadAux, valorPaisEP, propiedadPaisEP);

                    //Añado region entidad participante
                    string valorRegionEP = StringGNOSSID(entityPartAux, entidadParticipante.GetRegionPorIDCampo("060.020.020.180"));
                    string propiedadRegionEP = Variables.ActividadCientificaTecnologica.otrasColabCCAAEntidadParticipante;
                    UtilitySecciones.CheckProperty(propertyRegionEntidadParticipante, entidadAux, valorRegionEP, propiedadRegionEP);

                    //Añado ciudad entidad participante
                    string valorCiudadEP = StringGNOSSID(entityPartAux, entidadParticipante.GetStringCvnCodeGroup("060.020.020.190"));
                    string propiedadCiudadEP = Variables.ActividadCientificaTecnologica.otrasColabCiudadEntidadParticipante;
                    UtilitySecciones.CheckProperty(propertyCiudadEntidadParticipante, entidadAux, valorCiudadEP, propiedadCiudadEP);


                    Property propertyTipoEntidadParticipante = entidadAux.properties.FirstOrDefault(x => x.prop == Variables.ActividadCientificaTecnologica.redesCoopTipoEntidadParticipante);
                    Property propertyTipoEntidadParticipanteOtros = entidadAux.properties.FirstOrDefault(x => x.prop == Variables.ActividadCientificaTecnologica.redesCoopTipoEntidadParticipanteOtros);

                    //Añado tipo entidad participante
                    string valorOtroTipoEP = StringGNOSSID(entityPartAux, entidadParticipante.GetStringCvnCodeGroup("060.020.020.110"));
                    string propiedadOtroTipoEP = Variables.ActividadCientificaTecnologica.otrasColabTipoEntidadOtros;

                    string valorTipoEP = !string.IsNullOrEmpty(valorOtroTipoEP) ? StringGNOSSID(entityPartAux, mResourceApi.GraphsUrl + "items/organizationtype_OTHERS") : StringGNOSSID(entityPartAux, entidadParticipante.GetOrganizationCvnCodeGroup("060.020.020.100"));
                    string propiedadTipoEP = Variables.ActividadCientificaTecnologica.otrasColabTipoEntidad;

                    UtilitySecciones.CheckProperty(propertyTipoEntidadParticipante, entidadAux, valorTipoEP, propiedadTipoEP);
                    UtilitySecciones.CheckProperty(propertyTipoEntidadParticipanteOtros, entidadAux, valorOtroTipoEP, propiedadOtroTipoEP);

                }
            }
        }

        /// <summary>
        /// 060.030.020.000
        /// </summary>
        /// <param name="listadoDatos"></param>
        private List<Entity> GetSociedadesAsociaciones(List<CvnItemBean> listadoDatos)
        {
            List<Entity> listado = new List<Entity>();

            List<CvnItemBean> listadoSociedadesAsociaciones = listadoDatos.Where(x => x.Code.Equals("060.030.020.000")).ToList();
            if (listadoSociedadesAsociaciones.Count > 0)
            {
                foreach (CvnItemBean item in listadoSociedadesAsociaciones)
                {
                    Entity entidadAux = new Entity();
                    entidadAux.properties = new List<Property>();
                    if (!string.IsNullOrEmpty(item.GetStringPorIDCampo("060.030.020.010")))
                    {
                        entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                            new Property(Variables.ActividadCientificaTecnologica.sociedadesNombre, item.GetStringPorIDCampo("060.030.020.010")),
                            new Property(Variables.ActividadCientificaTecnologica.sociedadesPaisRadicacion, item.GetPaisPorIDCampo("060.030.020.020")),
                            new Property(Variables.ActividadCientificaTecnologica.sociedadesCCAARadicacion, item.GetRegionPorIDCampo("060.030.020.030")),
                            new Property(Variables.ActividadCientificaTecnologica.sociedadesCiudadRadicacion, item.GetStringPorIDCampo("060.030.020.180")),
                            new Property(Variables.ActividadCientificaTecnologica.sociedadesPaisEntidadAfiliacion, item.GetPaisPorIDCampo("060.030.020.150")),
                            new Property(Variables.ActividadCientificaTecnologica.sociedadesCCAAEntidadAfiliacion, item.GetRegionPorIDCampo("060.030.020.160")),
                            new Property(Variables.ActividadCientificaTecnologica.sociedadesCiudadEntidadAfiliacion, item.GetStringPorIDCampo("060.030.020.170")),
                            new Property(Variables.ActividadCientificaTecnologica.sociedadesCategoriaProfesional, item.GetStringPorIDCampo("060.030.020.100")),
                            new Property(Variables.ActividadCientificaTecnologica.sociedadesTamanio, item.GetStringDoublePorIDCampo("060.030.020.110")),
                            new Property(Variables.ActividadCientificaTecnologica.sociedadesFechaInicio, item.GetStringDatetimePorIDCampo("060.030.020.120")),
                            new Property(Variables.ActividadCientificaTecnologica.sociedadesFechaFinalizacion, item.GetStringDatetimePorIDCampo("060.030.020.130"))
                         ));
                        SociedadesAsociacionesEntidadAfiliacion(item, entidadAux);
                        //SociedadesAsociacionesPalabrasClave(item, entidadAux);

                        listado.Add(entidadAux);
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
        private void SociedadesAsociacionesPalabrasClave(CvnItemBean item, Entity entidadAux)
        {
            List<CvnItemBeanCvnString> listadoPalabrasClave = item.GetListaElementosPorIDCampo<CvnItemBeanCvnString>("060.030.020.090");

            foreach(CvnItemBeanCvnString palabra in listadoPalabrasClave)
            {
                entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                    new Property(Variables.ActividadCientificaTecnologica.sociedadesPalabrasClave, palabra.Value)//TODO cambio valores (falta tesauro)
                    ));
            }
        }

        /// <summary>
        /// Inserta en <paramref name="entidadAux"/> los valores de <paramref name="item"/>,
        /// pertenecientes al tipo de Entidad de Afiliación.
        /// </summary>
        /// <param name="item">item</param>
        /// <param name="entidadAux">entidadAux</param>
        private void SociedadesAsociacionesEntidadAfiliacion(CvnItemBean item, Entity entidadAux)
        {
            //Si no esta Entidad de Afiliacion no añado datos
            if (string.IsNullOrEmpty(item.GetNameEntityBeanPorIDCampo("060.030.020.050"))) { return; }

            //Añado la referencia si existe
            UtilitySecciones.AniadirEntidad(mResourceApi, item.GetNameEntityBeanPorIDCampo("060.030.020.050"),
                Variables.ActividadCientificaTecnologica.sociedadesEntidadAfiliacionNombre,
                Variables.ActividadCientificaTecnologica.sociedadesEntidadAfiliacion, entidadAux);

            //Añado otros, o el ID de una preseleccion
            string valorTipoEntidad = !string.IsNullOrEmpty(item.GetStringPorIDCampo("060.030.020.080")) ? mResourceApi.GraphsUrl + "items/organizationtype_OTHERS" : item.GetOrganizacionPorIDCampo("060.030.020.070");

            entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                new Property(Variables.ActividadCientificaTecnologica.sociedadesTipoEntidadAfiliacion, valorTipoEntidad),
                new Property(Variables.ActividadCientificaTecnologica.sociedadesTipoEntidadAfiliacionOtros, item.GetStringPorIDCampo("060.030.020.080"))
            ));
        }

        /// <summary>
        /// 060.030.030.000
        /// </summary>
        /// <param name="listadoDatos"></param>
        private List<Entity> GetConsejos(List<CvnItemBean> listadoDatos)
        {
            List<Entity> listado = new List<Entity>();

            List<CvnItemBean> listadoConsejos = listadoDatos.Where(x => x.Code.Equals("060.030.030.000")).ToList();
            if (listadoConsejos.Count > 0)
            {
                foreach (CvnItemBean item in listadoConsejos)
                {
                    Entity entidadAux = new Entity();
                    entidadAux.properties = new List<Property>();
                    if (!string.IsNullOrEmpty(item.GetStringPorIDCampo("060.030.030.010")))
                    {
                        entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                            new Property(Variables.ActividadCientificaTecnologica.consejosNombre, item.GetStringPorIDCampo("060.030.030.010")),
                            new Property(Variables.ActividadCientificaTecnologica.consejosPaisRadicacion, item.GetPaisPorIDCampo("060.030.030.020")),
                            new Property(Variables.ActividadCientificaTecnologica.consejosCCAARadicacion, item.GetRegionPorIDCampo("060.030.030.030")),
                            new Property(Variables.ActividadCientificaTecnologica.consejosCiudadRadicacion, item.GetStringPorIDCampo("060.030.030.160")),
                            new Property(Variables.ActividadCientificaTecnologica.consejosPaisEntidadAfiliacion, item.GetPaisPorIDCampo("060.030.030.170")),
                            new Property(Variables.ActividadCientificaTecnologica.consejosCCAAEntidadAfiliacion, item.GetRegionPorIDCampo("060.030.030.180")),
                            new Property(Variables.ActividadCientificaTecnologica.consejosCiudadEntidadAfiliacion, item.GetStringPorIDCampo("060.030.030.190")),
                            new Property(Variables.ActividadCientificaTecnologica.consejosTareasDesarrolladas, item.GetStringPorIDCampo("060.030.030.090")),
                            new Property(Variables.ActividadCientificaTecnologica.consejosCategoriaProfesional, item.GetStringPorIDCampo("060.030.030.100")),
                            new Property(Variables.ActividadCientificaTecnologica.consejosTamanioSociedad, item.GetStringDoublePorIDCampo("060.030.030.110")),
                            new Property(Variables.ActividadCientificaTecnologica.consejosAmbito, item.GetGeographicRegionPorIDCampo("060.030.030.120")),
                            new Property(Variables.ActividadCientificaTecnologica.consejosAmbitoOtros, item.GetStringPorIDCampo("060.030.030.130")),
                            new Property(Variables.ActividadCientificaTecnologica.consejosFechaInicio, item.GetStringDatetimePorIDCampo("060.030.030.140")),
                            new Property(Variables.ActividadCientificaTecnologica.redesCoopDuracionAnio, item.GetDurationAnioPorIDCampo("060.030.030.150")),
                            new Property(Variables.ActividadCientificaTecnologica.redesCoopDuracionMes, item.GetDurationMesPorIDCampo("060.030.030.150")),
                            new Property(Variables.ActividadCientificaTecnologica.redesCoopDuracionDias, item.GetDurationDiaPorIDCampo("060.030.030.150"))
                        ));
                        ConsejosEntidadAfiliacion(item, entidadAux);

                        listado.Add(entidadAux);
                    }
                }
            }
            return listado;
        }

        /// <summary>
        /// Inserta en <paramref name="entidadAux"/> los valores de <paramref name="item"/>,
        /// pertenecientes al tipo de Entidad de Afiliación.
        /// </summary>
        /// <param name="item">item</param>
        /// <param name="entidadAux">entidadAux</param>
        private void ConsejosEntidadAfiliacion(CvnItemBean item, Entity entidadAux)
        {
            //Si no esta Entidad no añado datos
            if (string.IsNullOrEmpty(item.GetNameEntityBeanPorIDCampo("060.030.030.050"))) { return; }

            //Añado la referencia si existe
            UtilitySecciones.AniadirEntidad(mResourceApi, item.GetNameEntityBeanPorIDCampo("060.030.030.050"),
                Variables.ActividadCientificaTecnologica.consejosEntidadAfiliacionNombre,
                Variables.ActividadCientificaTecnologica.consejosEntidadAfiliacion, entidadAux);

            //Añado otros, o el ID de una preseleccion
            string valorTipoEntidad = !string.IsNullOrEmpty(item.GetStringPorIDCampo("060.030.030.080")) ? mResourceApi.GraphsUrl + "items/organizationtype_OTHERS" : item.GetOrganizacionPorIDCampo("060.030.030.070");

            entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                new Property(Variables.ActividadCientificaTecnologica.consejosTipoEntidadAfiliacion, valorTipoEntidad),
                new Property(Variables.ActividadCientificaTecnologica.consejosTipoEntidadAfiliacionOtros, item.GetStringPorIDCampo("060.030.030.080"))
            ));
        }

        /// <summary>
        /// 060.030.040.000
        /// </summary>
        /// <param name="listadoDatos"></param>
        private List<Entity> GetRedesCooperacion(List<CvnItemBean> listadoDatos)
        {
            List<Entity> listado = new List<Entity>();

            List<CvnItemBean> listadoRedesCooperacion = listadoDatos.Where(x => x.Code.Equals("060.030.040.000")).ToList();
            if (listadoRedesCooperacion.Count > 0)
            {
                foreach (CvnItemBean item in listadoRedesCooperacion)
                {
                    Entity entidadAux = new Entity();
                    entidadAux.properties = new List<Property>();
                    if (!string.IsNullOrEmpty(item.GetStringPorIDCampo("060.030.040.010")))
                    {
                        entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                            new Property(Variables.ActividadCientificaTecnologica.redesCoopNombre, item.GetStringPorIDCampo("060.030.040.010")),
                            new Property(Variables.ActividadCientificaTecnologica.redesCoopIdentificacion, item.GetStringPorIDCampo("060.030.040.020")),
                            new Property(Variables.ActividadCientificaTecnologica.redesCoopNumInvestigadores, item.GetStringDoublePorIDCampo("060.030.040.030")),
                            new Property(Variables.ActividadCientificaTecnologica.redesCoopPaisRadicacion, item.GetPaisPorIDCampo("060.030.040.040")),
                            new Property(Variables.ActividadCientificaTecnologica.redesCoopCCAARadicacion, item.GetRegionPorIDCampo("060.030.040.050")),
                            new Property(Variables.ActividadCientificaTecnologica.redesCoopCiudadRadicacion, item.GetStringPorIDCampo("060.030.040.180")),
                            new Property(Variables.ActividadCientificaTecnologica.redesCoopPaisEntidadSeleccion, item.GetPaisPorIDCampo("060.030.040.190")),
                            new Property(Variables.ActividadCientificaTecnologica.redesCoopCCAAEntidadSeleccion, item.GetRegionPorIDCampo("060.030.040.200")),
                            new Property(Variables.ActividadCientificaTecnologica.redesCoopCiudadEntidadSeleccion, item.GetStringPorIDCampo("060.030.040.210")),
                            new Property(Variables.ActividadCientificaTecnologica.redesCoopTareas, item.GetStringPorIDCampo("060.030.040.150")),
                            new Property(Variables.ActividadCientificaTecnologica.redesCoopFechaInicio, item.GetStringDatetimePorIDCampo("060.030.040.160")),
                            new Property(Variables.ActividadCientificaTecnologica.redesCoopDuracionAnio, item.GetDurationAnioPorIDCampo("060.030.040.170")),
                            new Property(Variables.ActividadCientificaTecnologica.redesCoopDuracionMes, item.GetDurationMesPorIDCampo("060.030.040.170")),
                            new Property(Variables.ActividadCientificaTecnologica.redesCoopDuracionDias, item.GetDurationDiaPorIDCampo("060.030.040.170"))
                        ));
                        RedesCooperacionEntidadSeleccion(item, entidadAux);
                        RedesCooperacionEntidadesParticipantes(item, entidadAux);

                        listado.Add(entidadAux);
                    }
                }
            }
            return listado;
        }

        /// <summary>
        /// Inserta en <paramref name="entidadAux"/> los valores de <paramref name="item"/>,
        /// pertenecientes al tipo de Entidad de Selección.
        /// </summary>
        /// <param name="item">item</param>
        /// <param name="entidadAux">entidadAux</param>        
        private void RedesCooperacionEntidadSeleccion(CvnItemBean item, Entity entidadAux)
        {
            //Si no esta Entidad no añado datos
            if (string.IsNullOrEmpty(item.GetNameEntityBeanPorIDCampo("060.030.040.110"))) { return; }

            //Añado la referencia si existe
            UtilitySecciones.AniadirEntidad(mResourceApi, item.GetNameEntityBeanPorIDCampo("060.030.040.110"),
                Variables.ActividadCientificaTecnologica.redesCoopEntidadSeleccionNombre,
                Variables.ActividadCientificaTecnologica.redesCoopEntidadSeleccion, entidadAux);

            //Añado otros, o el ID de una preseleccion
            string valorTipoEntidad = !string.IsNullOrEmpty(item.GetStringPorIDCampo("060.030.040.140")) ? mResourceApi.GraphsUrl + "items/organizationtype_OTHERS" : item.GetOrganizacionPorIDCampo("060.030.040.130");

            entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                new Property(Variables.ActividadCientificaTecnologica.redesCoopTipoEntidadSeleccion, valorTipoEntidad),
                new Property(Variables.ActividadCientificaTecnologica.redesCoopTipoEntidadSeleccionOtros, item.GetStringPorIDCampo("060.030.040.140"))
            ));
        }

        /// <summary>
        /// Inserta en <paramref name="entidadAux"/> los valores de las Entidades Participantes,
        /// pertenecientes al listado <paramref name="listadoEntidadParticipante"/>.
        /// </summary>
        /// <param name="listadoEntidadParticipante">listadoEntidadParticipante</param>
        /// <param name="entidadAux">entidadAux</param>
        private void RedesCooperacionEntidadesParticipantes(CvnItemBean item, Entity entidadAux)
        {
            List<CvnItemBeanCvnCodeGroup> listadoEntidadParticipante = item.GetListaElementosPorIDCampo<CvnItemBeanCvnCodeGroup>("060.030.040.070");

            foreach (CvnItemBeanCvnCodeGroup entidadParticipante in listadoEntidadParticipante)
            {
                if (!string.IsNullOrEmpty(entidadParticipante.GetNameEntityBeanCvnCodeGroup("060.030.040.070")))
                {
                    string entityPartAux = Guid.NewGuid().ToString() + "@@@";

                    //Añado Nombre Entidad Participante
                    Property propertyEntidadParticipanteNombre = entidadAux.properties.FirstOrDefault(x => x.prop == Variables.ActividadCientificaTecnologica.redesCoopEntidadParticipanteNombre);

                    string valorNombreEP = StringGNOSSID(entityPartAux, entidadParticipante.GetNameEntityBeanCvnCodeGroup("060.030.040.070"));
                    string propiedadNombreEP = Variables.ActividadCientificaTecnologica.redesCoopEntidadParticipanteNombre;

                    UtilitySecciones.CheckProperty(propertyEntidadParticipanteNombre, entidadAux, valorNombreEP, propiedadNombreEP);

                    //Añado referencia Entidad Participante
                    Property propertyEntidadParticipante = entidadAux.properties.FirstOrDefault(x => x.prop == Variables.ActividadCientificaTecnologica.redesCoopEntidadParticipante);
                    string nombre = entidadParticipante.CvnEntityBean.Name;
                    if (!string.IsNullOrEmpty(nombre))
                    {
                        string referenciaEP = UtilitySecciones.GetOrganizacionPorNombre(mResourceApi, nombre);
                        string valorEP = (referenciaEP != null) ? StringGNOSSID(entityPartAux, referenciaEP) : entityPartAux + "";
                        string propiedadEP = Variables.ActividadCientificaTecnologica.redesCoopEntidadParticipante;

                        UtilitySecciones.CheckProperty(propertyEntidadParticipanteNombre, entidadAux, valorEP, propiedadEP);
                    }

                    //Añado Tipo Entidad Participante
                    Property propertyTipoEntidadParticipante = entidadAux.properties.FirstOrDefault(x => x.prop == Variables.ActividadCientificaTecnologica.redesCoopTipoEntidadParticipante);
                    Property propertyTipoEntidadParticipanteOtros = entidadAux.properties.FirstOrDefault(x => x.prop == Variables.ActividadCientificaTecnologica.redesCoopTipoEntidadParticipanteOtros);

                    string valorOtroTipoEP = StringGNOSSID(entityPartAux, entidadParticipante.GetStringCvnCodeGroup("060.030.040.100"));
                    string propiedadOtroTipoEP = Variables.ActividadCientificaTecnologica.otrasColabTipoEntidadOtros;

                    string valorTipoEP = !string.IsNullOrEmpty(valorOtroTipoEP) ? StringGNOSSID(entityPartAux, mResourceApi.GraphsUrl + "items/organizationtype_OTHERS") : StringGNOSSID(entityPartAux, entidadParticipante.GetOrganizationCvnCodeGroup("060.030.040.090"));
                    string propiedadTipoEP = Variables.ActividadCientificaTecnologica.otrasColabTipoEntidad;

                    UtilitySecciones.CheckProperty(propertyTipoEntidadParticipante, entidadAux, valorTipoEP, propiedadTipoEP);
                    UtilitySecciones.CheckProperty(propertyTipoEntidadParticipanteOtros, entidadAux, valorOtroTipoEP, propiedadOtroTipoEP);

                }
            }
        }

        /// <summary>
        /// 060.030.050.000
        /// </summary>
        /// <param name="listadoDatos"></param>
        private List<Entity> GetPremiosMenciones(List<CvnItemBean> listadoDatos)
        {
            List<Entity> listado = new List<Entity>();

            List<CvnItemBean> listadoPremios = listadoDatos.Where(x => x.Code.Equals("060.030.050.000")).ToList();
            if (listadoPremios.Count > 0)
            {
                foreach (CvnItemBean item in listadoPremios)
                {
                    Entity entidadAux = new Entity();
                    entidadAux.properties = new List<Property>();
                    if (!string.IsNullOrEmpty(item.GetStringPorIDCampo("060.030.050.010")))
                    {
                        entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                            new Property(Variables.ActividadCientificaTecnologica.premiosMencionesDescripcion, item.GetStringPorIDCampo("060.030.050.010")),
                            new Property(Variables.ActividadCientificaTecnologica.premiosMencionesPais, item.GetPaisPorIDCampo("060.030.050.020")),
                            new Property(Variables.ActividadCientificaTecnologica.premiosMencionesCCAA, item.GetRegionPorIDCampo("060.030.050.030")),
                            new Property(Variables.ActividadCientificaTecnologica.premiosMencionesCiudad, item.GetStringPorIDCampo("060.030.050.110")),
                            new Property(Variables.ActividadCientificaTecnologica.premiosMencionesReconocimientosLigados, item.GetStringPorIDCampo("060.030.050.090")),
                            new Property(Variables.ActividadCientificaTecnologica.premiosMencionesFechaConcesion, item.GetStringDatetimePorIDCampo("060.030.050.100"))
                        ));
                        PremiosMencionesEntidad(item, entidadAux);

                        listado.Add(entidadAux);
                    }
                }
            }
            return listado;
        }

        /// <summary>
        /// Inserta en <paramref name="entidadAux"/> los valores de <paramref name="item"/>,
        /// pertenecientes al tipo de Entidad Concesionaria.
        /// </summary>
        /// <param name="item">item</param>
        /// <param name="entidadAux">entidadAux</param>
        private void PremiosMencionesEntidad(CvnItemBean item, Entity entidadAux)
        {
            //Si no esta Entidad no añado datos
            if (string.IsNullOrEmpty(item.GetNameEntityBeanPorIDCampo("060.030.050.050"))) { return; }

            //Añado la referencia si existe
            UtilitySecciones.AniadirEntidad(mResourceApi, item.GetNameEntityBeanPorIDCampo("060.030.050.050"),
                Variables.ActividadCientificaTecnologica.premiosMencionesEntidadNombre,
                Variables.ActividadCientificaTecnologica.premiosMencionesEntidad, entidadAux);

            //Añado otros, o el ID de una preseleccion
            string valorTipoEntidad = !string.IsNullOrEmpty(item.GetStringPorIDCampo("060.030.050.080")) ? mResourceApi.GraphsUrl + "items/organizationtype_OTHERS" : item.GetOrganizacionPorIDCampo("060.030.050.070");

            entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                new Property(Variables.ActividadCientificaTecnologica.premiosMencionesTipoEntidad, valorTipoEntidad),
                new Property(Variables.ActividadCientificaTecnologica.premiosMencionesTipoEntidadOtros, item.GetStringPorIDCampo("060.030.050.080"))
            ));
        }

        /// <summary>
        /// 060.030.060.000
        /// </summary>
        /// <param name="listadoDatos"></param>
        private List<Entity> GetOtrasDistinciones(List<CvnItemBean> listadoDatos)
        {
            List<Entity> listado = new List<Entity>();

            List<CvnItemBean> listadoDistinciones = listadoDatos.Where(x => x.Code.Equals("060.030.060.000")).ToList();
            if (listadoDistinciones.Count > 0)
            {
                foreach (CvnItemBean item in listadoDistinciones)
                {
                    Entity entidadAux = new Entity();
                    entidadAux.properties = new List<Property>();
                    if (!string.IsNullOrEmpty(item.GetStringPorIDCampo("060.030.060.010")))
                    {
                        entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                            new Property(Variables.ActividadCientificaTecnologica.otrasDistincionesDescripcion, item.GetStringPorIDCampo("060.030.060.010")),
                            new Property(Variables.ActividadCientificaTecnologica.otrasDistincionesPais, item.GetPaisPorIDCampo("060.030.060.020")),
                            new Property(Variables.ActividadCientificaTecnologica.otrasDistincionesCCAA, item.GetRegionPorIDCampo("060.030.060.030")),
                            new Property(Variables.ActividadCientificaTecnologica.otrasDistincionesCiudad, item.GetStringPorIDCampo("060.030.060.120")),
                            new Property(Variables.ActividadCientificaTecnologica.otrasDistincionesAmbito, item.GetGeographicRegionPorIDCampo("060.030.060.090")),
                            new Property(Variables.ActividadCientificaTecnologica.otrasDistincionesAmbitoOtros, item.GetStringPorIDCampo("060.030.060.100")),
                            new Property(Variables.ActividadCientificaTecnologica.otrasDistincionesFechaConcesion, item.GetStringDatetimePorIDCampo("060.030.060.110"))
                        ));
                        OtrasDistincionesEntidad(item, entidadAux);

                        listado.Add(entidadAux);
                    }
                }
            }
            return listado;
        }

        /// <summary>
        /// Inserta en <paramref name="entidadAux"/> los valores de <paramref name="item"/>,
        /// pertenecientes al tipo de Entidad Concesionaria.
        /// </summary>
        /// <param name="item">item</param>
        /// <param name="entidadAux">entidadAux</param>
        private void OtrasDistincionesEntidad(CvnItemBean item, Entity entidadAux)
        {
            //Si no esta Entidad no añado datos
            if (string.IsNullOrEmpty(item.GetNameEntityBeanPorIDCampo("060.030.060.050"))) { return; }

            //Añado la referencia si existe
            UtilitySecciones.AniadirEntidad(mResourceApi, item.GetNameEntityBeanPorIDCampo("060.030.060.050"),
                Variables.ActividadCientificaTecnologica.otrasDistincionesEntidadNombre,
                Variables.ActividadCientificaTecnologica.otrasDistincionesEntidad, entidadAux);

            //Añado otros, o el ID de una preseleccion
            string valorTipoEntidad = !string.IsNullOrEmpty(item.GetStringPorIDCampo("060.030.060.080")) ? mResourceApi.GraphsUrl + "items/organizationtype_OTHERS" : item.GetOrganizacionPorIDCampo("060.030.060.070");

            entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                new Property(Variables.ActividadCientificaTecnologica.otrasDistincionesTipoEntidad, valorTipoEntidad),
                new Property(Variables.ActividadCientificaTecnologica.otrasDistincionesTipoEntidadOtros, item.GetStringPorIDCampo("060.030.060.080"))
            ));
        }

        /// <summary>
        /// 060.030.070.000
        /// </summary>
        /// <param name="listadoDatos"></param>
        private List<Entity> GetPeriodosActividadInvestigadora(List<CvnItemBean> listadoDatos)
        {
            List<Entity> listado = new List<Entity>();

            List<CvnItemBean> listadoAcreditaciones = listadoDatos.Where(x => x.Code.Equals("060.030.070.000")).ToList();
            if (listadoAcreditaciones.Count > 0)
            {
                foreach (CvnItemBean item in listadoAcreditaciones)
                {
                    Entity entidadAux = new Entity();
                    entidadAux.properties = new List<Property>();
                    if (!string.IsNullOrEmpty(item.GetStringDoublePorIDCampo("060.030.070.010")))
                    {
                        entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                            new Property(Variables.ActividadCientificaTecnologica.actividadInvestigadoraNumeroTramos, item.GetStringDoublePorIDCampo("060.030.070.010")),
                            new Property(Variables.ActividadCientificaTecnologica.actividadInvestigadoraPaisEntidad, item.GetPaisPorIDCampo("060.030.070.020")),
                            new Property(Variables.ActividadCientificaTecnologica.actividadInvestigadoraCCAAEntidad, item.GetRegionPorIDCampo("060.030.070.030")),
                            new Property(Variables.ActividadCientificaTecnologica.actividadInvestigadoraCiudadEntidad, item.GetStringPorIDCampo("060.030.070.120")),
                            new Property(Variables.ActividadCientificaTecnologica.actividadInvestigadoraAmbito, item.GetGeographicRegionPorIDCampo("060.030.070.090")),
                            new Property(Variables.ActividadCientificaTecnologica.actividadInvestigadoraAmbitoOtros, item.GetStringPorIDCampo("060.030.070.100")),
                            new Property(Variables.ActividadCientificaTecnologica.actividadInvestigadoraFechaObtencion, item.GetStringDatetimePorIDCampo("060.030.070.110"))
                        ));
                        PeriodosActividadInvestigadoraEntidadAfiliacion(item, entidadAux);

                        listado.Add(entidadAux);
                    }
                }
            }
            return listado;
        }

        /// <summary>
        /// Inserta en <paramref name="entidadAux"/> los valores de <paramref name="item"/>,
        /// pertenecientes al tipo de Entidad Acreditante.
        /// </summary>
        /// <param name="item">item</param>
        /// <param name="entidadAux">entidadAux</param>
        private void PeriodosActividadInvestigadoraEntidadAfiliacion(CvnItemBean item, Entity entidadAux)
        {
            //Si no esta Entidad no añado datos
            if (string.IsNullOrEmpty(item.GetNameEntityBeanPorIDCampo("060.030.070.050"))) { return; }

            //Añado la referencia si existe
            UtilitySecciones.AniadirEntidad(mResourceApi, item.GetNameEntityBeanPorIDCampo("060.030.070.050"),
                Variables.ActividadCientificaTecnologica.actividadInvestigadoraEntidadNombre,
                Variables.ActividadCientificaTecnologica.actividadInvestigadoraEntidad, entidadAux);

            //Añado otros, o el ID de una preseleccion
            string valorTipoEntidad = !string.IsNullOrEmpty(item.GetStringPorIDCampo("060.030.070.080")) ? mResourceApi.GraphsUrl + "items/organizationtype_OTHERS" : item.GetOrganizacionPorIDCampo("060.030.070.070");

            entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                new Property(Variables.ActividadCientificaTecnologica.actividadInvestigadoraTipoEntidad, valorTipoEntidad),
                new Property(Variables.ActividadCientificaTecnologica.actividadInvestigadoraTipoEntidadOtros, item.GetStringPorIDCampo("060.030.070.080"))
            ));
        }

        /// <summary>
        /// 060.030.090.000
        /// </summary>
        /// <param name="listadoDatos"></param>
        private List<Entity> GetAcreditacionesObtenidas(List<CvnItemBean> listadoDatos)
        {
            List<Entity> listado = new List<Entity>();

            List<CvnItemBean> listadoAcreditaciones = listadoDatos.Where(x => x.Code.Equals("060.030.090.000")).ToList();
            if (listadoAcreditaciones.Count > 0)
            {
                foreach (CvnItemBean item in listadoAcreditaciones)
                {
                    Entity entidadAux = new Entity();
                    entidadAux.properties = new List<Property>();
                    if (!string.IsNullOrEmpty(item.GetStringPorIDCampo("060.030.090.010")))
                    {
                        entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                            new Property(Variables.ActividadCientificaTecnologica.acreditacionesDescripcion, item.GetStringPorIDCampo("060.030.090.010")),
                            new Property(Variables.ActividadCientificaTecnologica.acreditacionesPaisEntidad, item.GetPaisPorIDCampo("060.030.090.020")),
                            new Property(Variables.ActividadCientificaTecnologica.acreditacionesCCAAEntidad, item.GetRegionPorIDCampo("060.030.090.030")),
                            new Property(Variables.ActividadCientificaTecnologica.acreditacionesCiudadEntidad, item.GetStringPorIDCampo("060.030.090.120")),
                            new Property(Variables.ActividadCientificaTecnologica.acreditacionesFechaObtencion, item.GetStringDatetimePorIDCampo("060.030.090.050")),
                            new Property(Variables.ActividadCientificaTecnologica.acreditacionesNumeroTramos, item.GetStringDoublePorIDCampo("060.030.090.100")),
                            new Property(Variables.ActividadCientificaTecnologica.acreditacionesFechaReconocimiento, item.GetStringDatetimePorIDCampo("060.030.090.110"))
                        ));
                        AcreditacionesObtenidasEntidad(item, entidadAux);

                        listado.Add(entidadAux);
                    }
                }
            }
            return listado;
        }

        /// <summary>
        /// Compruebo si existe la entidad en BBDD y en caso de que exista añado la referencia a la misma
        /// </summary>
        /// <param name="item"></param>
        /// <param name="entidadAux"></param>
        private void AcreditacionesObtenidasEntidad(CvnItemBean item, Entity entidadAux)
        {
            //Si no esta Entidad no añado datos
            if (string.IsNullOrEmpty(item.GetNameEntityBeanPorIDCampo("060.030.090.060"))) { return; }

            //Añado la referencia si existe
            UtilitySecciones.AniadirEntidad(mResourceApi, item.GetNameEntityBeanPorIDCampo("060.030.090.060"),
                Variables.ActividadCientificaTecnologica.acreditacionesEntidadNombre,
                Variables.ActividadCientificaTecnologica.acreditacionesEntidad, entidadAux);

            //Añado otros, o el ID de una preseleccion
            string valorTipoEntidad = !string.IsNullOrEmpty(item.GetStringPorIDCampo("060.030.090.090")) ? mResourceApi.GraphsUrl + "items/organizationtype_OTHERS" : item.GetOrganizacionPorIDCampo("060.030.090.080");

            entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                new Property(Variables.ActividadCientificaTecnologica.acreditacionesTipoEntidad, valorTipoEntidad),
                new Property(Variables.ActividadCientificaTecnologica.acreditacionesTipoEntidadOtros, item.GetStringPorIDCampo("060.030.090.090"))
            ));
        }


        /// <summary>
        /// 060.030.100.000
        /// </summary>
        /// <param name="listadoDatos"></param>
        private List<Entity> GetOtrosMeritos(List<CvnItemBean> listadoDatos)
        {
            List<Entity> listado = new List<Entity>();

            List<CvnItemBean> listadoOtros = listadoDatos.Where(x => x.Code.Equals("060.030.100.000")).ToList();
            if (listadoOtros.Count > 0)
            {
                foreach (CvnItemBean item in listadoOtros)
                {
                    Entity entidadAux = new Entity();
                    entidadAux.properties = new List<Property>();

                    if (!string.IsNullOrEmpty(item.GetStringPorIDCampo("060.030.100.010")))
                    {
                        entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                            new Property(Variables.ActividadCientificaTecnologica.otrosMeritosTextoLibre, item.GetStringPorIDCampo("060.030.100.010")),
                            new Property(Variables.ActividadCientificaTecnologica.otrosMeritosPaisEntidad, item.GetPaisPorIDCampo("060.030.100.070")),
                            new Property(Variables.ActividadCientificaTecnologica.otrosMeritosCCAAEntidad, item.GetRegionPorIDCampo("060.030.100.090")),
                            new Property(Variables.ActividadCientificaTecnologica.otrosMeritosCiudadEntidad, item.GetStringPorIDCampo("060.030.100.080")),
                            new Property(Variables.ActividadCientificaTecnologica.otrosMeritosFechaConcesion, item.GetStringDatetimePorIDCampo("060.030.100.040"))
                        ));
                        OtrosMeritosEntidad(item, entidadAux);

                        listado.Add(entidadAux);
                    }
                }
            }
            return listado;
        }

        /// <summary>
        /// Compruebo si existe la entidad en BBDD y en caso de que exista añado la referencia a la misma
        /// </summary>
        /// <param name="item"></param>
        /// <param name="entidadAux"></param>
        private void OtrosMeritosEntidad(CvnItemBean item, Entity entidadAux)
        {
            //Si no esta Entidad no añado datos
            if (string.IsNullOrEmpty(item.GetNameEntityBeanPorIDCampo("060.030.100.020"))) { return; }

            //Añado la referencia si existe
            UtilitySecciones.AniadirEntidad(mResourceApi, item.GetNameEntityBeanPorIDCampo("060.030.100.020"),
                Variables.ActividadCientificaTecnologica.otrosMeritosEntidadNombre,
                Variables.ActividadCientificaTecnologica.otrosMeritosEntidad, entidadAux);

            //Añado otros, o el ID de una preseleccion
            string valorTipoEntidad = !string.IsNullOrEmpty(item.GetStringPorIDCampo("060.030.100.050")) ? mResourceApi.GraphsUrl + "items/organizationtype_OTHERS" : item.GetOrganizacionPorIDCampo("060.030.100.060");

            entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                new Property(Variables.ActividadCientificaTecnologica.otrosMeritosTipoEntidad, valorTipoEntidad),
                new Property(Variables.ActividadCientificaTecnologica.otrosMeritosTipoEntidadOtros, item.GetStringPorIDCampo("060.030.100.050"))
            ));
        }
    }
}
