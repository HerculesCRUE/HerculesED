using System;
using System.Collections.Generic;
using System.Linq;
using static Models.Entity;
using Utils;
using Models;
using System.Runtime.InteropServices;
using Hercules.ED.ImportExportCV.Models;
using Hercules.CommonsEDMA.DisambiguationEngine.Models;
using ImportadorWebCV.Sincro.Secciones.ActividadDocenteSubclases;
using Hercules.ED.ImportExportCV.Controllers;

namespace ImportadorWebCV.Sincro.Secciones
{
    class ActividadDocente : SeccionBase
    {
        private List<CvnItemBean> listadoDatos = new List<CvnItemBean>();
        private List<CvnItemBean> listadoPremios = new List<CvnItemBean>();
        private List<CvnItemBean> listadoCvn = new List<CvnItemBean>();
        private readonly string RdfTypeTab = "http://w3id.org/roh/TeachingExperience";

        public ActividadDocente(cvnRootResultBean cvn, string cvID, string personID, ConfigService configuracion) : base(cvn, cvID, personID, configuracion)
        {
            listadoDatos = mCvn.GetListadoBloque("030");
            listadoPremios = mCvn.GetListadoBloque("060.030.080"); 
            listadoCvn = mCvn.cvnRootBean.ToList();
        }

        /// <summary>
        /// Metodo para sincronizar los datos pertenecientes al subapartado 
        /// "Formación académica impartida".
        /// Con el codigo identificativo 030.010.000.000
        /// </summary>
        public List<SubseccionItem> SincroFormacionAcademica(bool procesar, [Optional] bool preimportar, [Optional] List<string> listadoIdBBDD, [Optional] PetitionStatus petitionStatus)
        {
            //Si procesar es false, no hago nada.
            if (!procesar)
            {
                return null;
            }

            List<string> propiedadesItem = new List<string>() { "http://w3id.org/roh/teachingExperience", "http://w3id.org/roh/impartedAcademicTrainings", "http://vivoweb.org/ontology/core#relatedBy" };
            string graph = "impartedacademictraining";
            string propTitle = "http://w3id.org/roh/title";
            string rdfType = "http://w3id.org/roh/ImpartedAcademicTraining";
            string rdfTypePrefix = "RelatedImpartedAcademicTrainings";

            Dictionary<string, DisambiguableEntity> entidadesXML = new Dictionary<string, DisambiguableEntity>();
            Dictionary<string, string> equivalencias = new Dictionary<string, string>();
            List<bool> listadoBloqueados = new List<bool>();

            //1º Obtenemos la entidad del XML.
            List<Entity> listadoAux = GetFormacionAcademica(listadoDatos, petitionStatus);

            if (listadoIdBBDD == null)
            {
                foreach (Entity entityXML in listadoAux)
                {
                    ActividadDocenteSubclases.FormacionAcademica formacionAcademica = new ActividadDocenteSubclases.FormacionAcademica();
                    formacionAcademica.titulo = entityXML.properties.FirstOrDefault(x => x.prop == Variables.ActividadDocente.formacionAcademicaTitulacionUniversitariaNombre)?.values.FirstOrDefault();
                    formacionAcademica.nombreAsignatura = entityXML.properties.FirstOrDefault(x => x.prop == Variables.ActividadDocente.formacionAcademicaNombreAsignatura)?.values.FirstOrDefault();
                    formacionAcademica.fecha = entityXML.properties.FirstOrDefault(x => x.prop == Variables.ActividadDocente.formacionAcademicaFechaInicio)?.values.FirstOrDefault();
                    formacionAcademica.entidadRealizacion = entityXML.properties.FirstOrDefault(x => x.prop == Variables.ActividadDocente.formacionAcademicaEntidadRealizacionNombre)?.values.FirstOrDefault();
                    formacionAcademica.ID = Guid.NewGuid().ToString();
                    entidadesXML.Add(formacionAcademica.ID, formacionAcademica);
                }

                //2º Obtenemos las entidades de la BBDD.
                Dictionary<string, DisambiguableEntity> entidadesBBDD = ActividadDocenteSubclases.FormacionAcademica.GetBBDD(mResourceApi, mCvID, graph, propiedadesItem);
                var entidadesBBDDOpciones = entidadesBBDD.Select(x => new { x.Value.ID, x.Value.block }).ToList();

                //3º Comparamos las equivalentes
                equivalencias = Disambiguation.SimilarityBBDD(entidadesXML.Values.ToList(), entidadesBBDD.Values.ToList());
              
                foreach (var item in equivalencias.Values)
                {
                    listadoBloqueados.Add(entidadesBBDDOpciones.Where(x => x.ID.Equals(item)).Select(x => x.block).FirstOrDefault());
                }
            }

            //Comparamos si queremos Preimportar o actualizar las entidades.
            return CheckPreimportar(preimportar, listadoAux, entidadesXML, equivalencias, propTitle, graph, rdfType, rdfTypePrefix, propiedadesItem, RdfTypeTab, listadoBloqueados, listadoIdBBDD: listadoIdBBDD);
        }

        /// <summary>
        /// Metodo para sincronizar los datos pertenecientes al subapartado 
        /// "Dirección de tesis doctorales y/o proyectos fin de carrera".
        /// Con el codigo identificativo 030.040.000.000
        /// </summary>
        public List<SubseccionItem> SincroDireccionTesis(bool procesar, [Optional] bool preimportar, [Optional] List<string> listadoIdBBDD, [Optional] PetitionStatus petitionStatus)
        {
            //Si procesar es false, no hago nada.
            if (!procesar)
            {
                return null;
            }

            List<string> propiedadesItem = new List<string>() { "http://w3id.org/roh/teachingExperience", "http://w3id.org/roh/thesisSupervisions", "http://vivoweb.org/ontology/core#relatedBy" };
            string graph = "thesissupervision";
            string propTitle = "http://w3id.org/roh/title";
            string rdfType = "http://w3id.org/roh/ThesisSupervision";
            string rdfTypePrefix = "RelatedThesisSupervisions";

            Dictionary<string, DisambiguableEntity> entidadesXML = new Dictionary<string, DisambiguableEntity>();
            Dictionary<string, string> equivalencias = new Dictionary<string, string>();
            List<bool> listadoBloqueados = new List<bool>();

            //1º Obtenemos la entidad del XML.
            List<Entity> listadoAux = GetDireccionTesis(listadoDatos, petitionStatus);

            if (listadoIdBBDD == null)
            {
                foreach (Entity entityXML in listadoAux)
                {
                    DireccionTesis direccionTesis = new DireccionTesis();
                    direccionTesis.descripcion = entityXML.properties.FirstOrDefault(x => x.prop == Variables.ActividadDocente.direccionTesisTituloTrabajo)?.values.FirstOrDefault();
                    direccionTesis.fecha = entityXML.properties.FirstOrDefault(x => x.prop == Variables.ActividadDocente.direccionTesisFechaDefensa)?.values.FirstOrDefault();
                    direccionTesis.entidadRealizacion = entityXML.properties.FirstOrDefault(x => x.prop == Variables.ActividadDocente.direccionTesisEntidadRealizacionNombre)?.values.FirstOrDefault();
                    direccionTesis.ID = Guid.NewGuid().ToString();
                    entidadesXML.Add(direccionTesis.ID, direccionTesis);
                }

                //2º Obtenemos las entidades de la BBDD.
                Dictionary<string, DisambiguableEntity> entidadesBBDD = DireccionTesis.GetBBDD(mResourceApi, mCvID, graph, propiedadesItem);
                var entidadesBBDDOpciones = entidadesBBDD.Select(x => new { x.Value.ID, x.Value.block }).ToList();

                //3º Comparamos las equivalentes
                equivalencias = Disambiguation.SimilarityBBDD(entidadesXML.Values.ToList(), entidadesBBDD.Values.ToList());
              
                foreach (var item in equivalencias.Values)
                {
                    listadoBloqueados.Add(entidadesBBDDOpciones.Where(x => x.ID.Equals(item)).Select(x => x.block).FirstOrDefault());
                }
            }

            //Comparamos si queremos Preimportar o actualizar las entidades.
            return CheckPreimportar(preimportar, listadoAux, entidadesXML, equivalencias, propTitle, graph, rdfType, rdfTypePrefix, propiedadesItem, RdfTypeTab, listadoBloqueados, listadoIdBBDD: listadoIdBBDD);
        }

        /// <summary>
        /// Metodo para sincronizar los datos pertenecientes al subapartado 
        /// "Tutorías académicas de estudiantes".
        /// Con el codigo identificativo 030.050.000.000
        /// </summary>
        public List<SubseccionItem> SincroTutoriasAcademicas(bool procesar, [Optional] bool preimportar, [Optional] List<string> listadoIdBBDD, [Optional] PetitionStatus petitionStatus)
        {
            //Si procesar es false, no hago nada.
            if (!procesar)
            {
                return null;
            }

            List<string> propiedadesItem = new List<string>() { "http://w3id.org/roh/teachingExperience", "http://w3id.org/roh/academicTutorials", "http://vivoweb.org/ontology/core#relatedBy" };
            string graph = "tutorship";
            string propTitle = "http://w3id.org/roh/tutorshipProgramType";
            string rdfType = "http://w3id.org/roh/Tutorship";
            string rdfTypePrefix = "RelatedAcademicTutorials";

            Dictionary<string, DisambiguableEntity> entidadesXML = new Dictionary<string, DisambiguableEntity>();
            Dictionary<string, string> equivalencias = new Dictionary<string, string>();
            List<bool> listadoBloqueados = new List<bool>();

            //1º Obtenemos la entidad del XML.
            List<Entity> listadoAux = GetTutoriasAcademicas(listadoDatos, petitionStatus);

            if (listadoIdBBDD == null)
            {
                foreach (Entity entityXML in listadoAux)
                {
                    TutoriasAcademicas tutoriasAcademicas = new TutoriasAcademicas();
                    tutoriasAcademicas.nombre = entityXML.properties.FirstOrDefault(x => x.prop == Variables.ActividadDocente.tutoAcademicaNombrePrograma)?.values.FirstOrDefault();
                    tutoriasAcademicas.nombreOtros = entityXML.properties.FirstOrDefault(x => x.prop == Variables.ActividadDocente.tutoAcademicaNombreProgramaOtros)?.values.FirstOrDefault();
                    tutoriasAcademicas.entidadRealizacion = entityXML.properties.FirstOrDefault(x => x.prop == Variables.ActividadDocente.tutoAcademicaEntidadRealizacionNombre)?.values.FirstOrDefault();
                    tutoriasAcademicas.ID = Guid.NewGuid().ToString();
                    entidadesXML.Add(tutoriasAcademicas.ID, tutoriasAcademicas);
                }

                //2º Obtenemos las entidades de la BBDD
                Dictionary<string, DisambiguableEntity> entidadesBBDD = TutoriasAcademicas.GetBBDD(mResourceApi, mCvID, graph, propiedadesItem);
                var entidadesBBDDOpciones = entidadesBBDD.Select(x => new { x.Value.ID, x.Value.block }).ToList();

                //3º Comparamos las equivalentes
                equivalencias = Disambiguation.SimilarityBBDD(entidadesXML.Values.ToList(), entidadesBBDD.Values.ToList());
              
                foreach (var item in equivalencias.Values)
                {
                    listadoBloqueados.Add(entidadesBBDDOpciones.Where(x => x.ID.Equals(item)).Select(x => x.block).FirstOrDefault());
                }
            }

            //Comparamos si queremos Preimportar o actualizar las entidades
            return CheckPreimportar(preimportar, listadoAux, entidadesXML, equivalencias, propTitle, graph, rdfType, rdfTypePrefix, propiedadesItem, RdfTypeTab, listadoBloqueados, listadoIdBBDD: listadoIdBBDD);
        }

        /// <summary>
        /// Metodo para sincronizar los datos pertenecientes al subapartado 
        /// "Cursos y seminarios impartidos orientados a la formación docente universitaria".
        /// Con el codigo identificativo 030.060.000.000
        /// </summary>
        public List<SubseccionItem> SincroCursosSeminarios(bool procesar, [Optional] bool preimportar, [Optional] List<string> listadoIdBBDD, [Optional] PetitionStatus petitionStatus)
        {
            //Si procesar es false, no hago nada.
            if (!procesar)
            {
                return null;
            }

            List<string> propiedadesItem = new List<string>() { "http://w3id.org/roh/teachingExperience", "http://w3id.org/roh/impartedCoursesSeminars", "http://vivoweb.org/ontology/core#relatedBy" };
            string graph = "impartedcoursesseminars";
            string propTitle = "http://w3id.org/roh/title";
            string rdfType = "http://w3id.org/roh/ImpartedCoursesSeminars";
            string rdfTypePrefix = "RelatedImpartedCoursesSeminars";

            Dictionary<string, DisambiguableEntity> entidadesXML = new Dictionary<string, DisambiguableEntity>();
            Dictionary<string, string> equivalencias = new Dictionary<string, string>();
            List<bool> listadoBloqueados = new List<bool>();

            //1º Obtenemos la entidad del XML.
            List<Entity> listadoAux = GetCursosSeminarios(listadoDatos, petitionStatus);

            if (listadoIdBBDD == null)
            {
                foreach (Entity entityXML in listadoAux)
                {
                    CursosSeminarios cursosSeminarios = new CursosSeminarios();
                    cursosSeminarios.nombreEvento = entityXML.properties.FirstOrDefault(x => x.prop == Variables.ActividadDocente.cursosSeminariosNombreEvento)?.values.FirstOrDefault();
                    cursosSeminarios.fecha = entityXML.properties.FirstOrDefault(x => x.prop == Variables.ActividadDocente.cursosSeminariosFechaImparticion)?.values.FirstOrDefault();
                    cursosSeminarios.entidadOrganizadora = entityXML.properties.FirstOrDefault(x => x.prop == Variables.ActividadDocente.cursosSeminariosEntidadOrganizadoraNombre)?.values.FirstOrDefault();
                    cursosSeminarios.ID = Guid.NewGuid().ToString();
                    entidadesXML.Add(cursosSeminarios.ID, cursosSeminarios);
                }

                //2º Obtenemos las entidades de la BBDD.
                Dictionary<string, DisambiguableEntity> entidadesBBDD = CursosSeminarios.GetBBDD(mResourceApi, mCvID, graph, propiedadesItem);
                var entidadesBBDDOpciones = entidadesBBDD.Select(x => new { x.Value.ID, x.Value.block }).ToList();

                //3º Comparamos las equivalentes
                equivalencias = Disambiguation.SimilarityBBDD(entidadesXML.Values.ToList(), entidadesBBDD.Values.ToList());
              
                foreach (var item in equivalencias.Values)
                {
                    listadoBloqueados.Add(entidadesBBDDOpciones.Where(x => x.ID.Equals(item)).Select(x => x.block).FirstOrDefault());
                }
            }

            //Comparamos si queremos Preimportar o actualizar las entidades.
            return CheckPreimportar(preimportar, listadoAux, entidadesXML, equivalencias, propTitle, graph, rdfType, rdfTypePrefix, propiedadesItem, RdfTypeTab, listadoBloqueados, listadoIdBBDD: listadoIdBBDD);
        }

        /// <summary>
        /// Metodo para sincronizar los datos pertenecientes al subapartado 
        /// "Publicaciones docentes o de carácter pedagógico, libros, artículos, etc.".
        /// Con el codigo identificativo 030.070.000.000
        /// </summary>
        public List<SubseccionItem> SincroPublicacionDocentes(bool procesar, [Optional] bool preimportar, [Optional] List<string> listadoIdBBDD, [Optional] PetitionStatus petitionStatus)
        {
            //Si procesar es false, no hago nada.
            if (!procesar)
            {
                return null;
            }

            List<string> propiedadesItem = new List<string>() { "http://w3id.org/roh/teachingExperience", "http://w3id.org/roh/teachingPublications", "http://vivoweb.org/ontology/core#relatedBy" };
            string graph = "teachingpublication";
            string propTitle = "http://w3id.org/roh/title";
            string rdfType = "http://w3id.org/roh/TeachingPublication";
            string rdfTypePrefix = "RelatedTeachingPublications";

            Dictionary<string, DisambiguableEntity> entidadesXML = new Dictionary<string, DisambiguableEntity>();
            Dictionary<string, string> equivalencias = new Dictionary<string, string>();
            List<bool> listadoBloqueados = new List<bool>();

            //1º Obtenemos la entidad del XML.
            List<Entity> listadoAux = GetPublicacionDocentes(listadoDatos, petitionStatus);

            if (listadoIdBBDD == null)
            {
                foreach (Entity entityXML in listadoAux)
                {
                    PublicacionDocentes publicacionDocentes = new PublicacionDocentes();
                    publicacionDocentes.descripcion = entityXML.properties.FirstOrDefault(x => x.prop == Variables.ActividadDocente.publicacionDocenteNombre)?.values.FirstOrDefault();
                    publicacionDocentes.fecha = entityXML.properties.FirstOrDefault(x => x.prop == Variables.ActividadDocente.publicacionDocenteFechaElaboracion)?.values.FirstOrDefault();
                    publicacionDocentes.tituloPublicacion = entityXML.properties.FirstOrDefault(x => x.prop == Variables.ActividadDocente.publicacionDocenteTituloPublicacion)?.values.FirstOrDefault();
                    publicacionDocentes.ID = Guid.NewGuid().ToString();
                    entidadesXML.Add(publicacionDocentes.ID, publicacionDocentes);
                }

                //2º Obtenemos las entidades de la BBDD.
                Dictionary<string, DisambiguableEntity> entidadesBBDD = PublicacionDocentes.GetBBDD(mResourceApi, mCvID, graph, propiedadesItem);
                var entidadesBBDDOpciones = entidadesBBDD.Select(x => new { x.Value.ID, x.Value.block }).ToList();

                //3º Comparamos las equivalentes
                equivalencias = Disambiguation.SimilarityBBDD(entidadesXML.Values.ToList(), entidadesBBDD.Values.ToList());
              
                foreach (var item in equivalencias.Values)
                {
                    listadoBloqueados.Add(entidadesBBDDOpciones.Where(x => x.ID.Equals(item)).Select(x => x.block).FirstOrDefault());
                }
            }

            //Comparamos si queremos Preimportar o actualizar las entidades.
            return CheckPreimportar(preimportar, listadoAux, entidadesXML, equivalencias, propTitle, graph, rdfType, rdfTypePrefix, propiedadesItem, RdfTypeTab, listadoBloqueados, listadoIdBBDD: listadoIdBBDD);
        }

        /// <summary>
        /// Metodo para sincronizar los datos pertenecientes al subapartado 
        /// "Participación en proyectos de innovación docente".
        /// Con el codigo identificativo 030.080.000.000
        /// </summary>
        public List<SubseccionItem> SincroParticipacionProyectosInnovacionDocente(bool procesar, [Optional] bool preimportar, [Optional] List<string> listadoIdBBDD, [Optional] PetitionStatus petitionStatus)
        {
            //Si procesar es false, no hago nada.
            if (!procesar)
            {
                return null;
            }

            List<string> propiedadesItem = new List<string>() { "http://w3id.org/roh/teachingExperience", "http://w3id.org/roh/teachingProjects", "http://vivoweb.org/ontology/core#relatedBy" };
            string graph = "teachingproject";
            string propTitle = "http://w3id.org/roh/title";
            string rdfType = "http://w3id.org/roh/TeachingProject";
            string rdfTypePrefix = "RelatedTeachingProjects";

            Dictionary<string, DisambiguableEntity> entidadesXML = new Dictionary<string, DisambiguableEntity>();
            Dictionary<string, string> equivalencias = new Dictionary<string, string>();
            List<bool> listadoBloqueados = new List<bool>();

            //1º Obtenemos la entidad del XML.
            List<Entity> listadoAux = GetParticipacionProyectosInnovacionDocente(listadoDatos, petitionStatus);

            if (listadoIdBBDD == null)
            {
                foreach (Entity entityXML in listadoAux)
                {
                    ParticipacionProyectosInnovacionDocente participacionProyectosInnovacion = new ParticipacionProyectosInnovacionDocente();
                    participacionProyectosInnovacion.titulo = entityXML.properties.FirstOrDefault(x => x.prop == Variables.ActividadDocente.participacionInnovaTitulo)?.values.FirstOrDefault();
                    participacionProyectosInnovacion.fecha = entityXML.properties.FirstOrDefault(x => x.prop == Variables.ActividadDocente.participacionInnovaFechaInicio)?.values.FirstOrDefault();
                    participacionProyectosInnovacion.entidadFinanciadora = entityXML.properties.FirstOrDefault(x => x.prop == Variables.ActividadDocente.participacionInnovaEntidadFinanciadoraNombre)?.values.FirstOrDefault();
                    participacionProyectosInnovacion.ID = Guid.NewGuid().ToString();
                    entidadesXML.Add(participacionProyectosInnovacion.ID, participacionProyectosInnovacion);
                }

                //2º Obtenemos las entidades de la BBDD.
                Dictionary<string, DisambiguableEntity> entidadesBBDD = ParticipacionProyectosInnovacionDocente.GetBBDD(mResourceApi, mCvID, graph, propiedadesItem);
                var entidadesBBDDOpciones = entidadesBBDD.Select(x => new { x.Value.ID, x.Value.block }).ToList();

                //3º Comparamos las equivalentes
                equivalencias = Disambiguation.SimilarityBBDD(entidadesXML.Values.ToList(), entidadesBBDD.Values.ToList());
              
                foreach (var item in equivalencias.Values)
                {
                    listadoBloqueados.Add(entidadesBBDDOpciones.Where(x => x.ID.Equals(item)).Select(x => x.block).FirstOrDefault());
                }
            }

            //Comparamos si queremos Preimportar o actualizar las entidades.
            return CheckPreimportar(preimportar, listadoAux, entidadesXML, equivalencias, propTitle, graph, rdfType, rdfTypePrefix, propiedadesItem, RdfTypeTab, listadoBloqueados, listadoIdBBDD: listadoIdBBDD);
        }

        /// <summary>
        /// Metodo para sincronizar los datos pertenecientes al subapartado 
        /// "Participación en congresos con ponencias orientadas a la formación docente".
        /// Con el codigo identificativo 030.090.000.000
        /// </summary>
        public List<SubseccionItem> SincroParticipacionCongresosFormacionDocente(bool procesar, [Optional] bool preimportar, [Optional] List<string> listadoIdBBDD, [Optional] PetitionStatus petitionStatus)
        {
            //Si procesar es false, no hago nada.
            if (!procesar)
            {
                return null;
            }

            List<string> propiedadesItem = new List<string>() { "http://w3id.org/roh/teachingExperience", "http://w3id.org/roh/teachingCongress", "http://vivoweb.org/ontology/core#relatedBy" };
            string graph = "teachingcongress";
            string propTitle = "http://w3id.org/roh/title";
            string rdfType = "http://w3id.org/roh/TeachingCongress";
            string rdfTypePrefix = "RelatedTeachingCongress";

            Dictionary<string, DisambiguableEntity> entidadesXML = new Dictionary<string, DisambiguableEntity>();
            Dictionary<string, string> equivalencias = new Dictionary<string, string>();
            List<bool> listadoBloqueados = new List<bool>();

            //1º Obtenemos la entidad del XML.
            List<Entity> listadoAux = GetParticipacionCongresosFormacionDocente(listadoDatos, petitionStatus);

            if (listadoIdBBDD == null)
            {
                foreach (Entity entityXML in listadoAux)
                {
                    ParticipacionCongresosFormacionDocente participacionCongresos = new ParticipacionCongresosFormacionDocente();
                    participacionCongresos.descripcion = entityXML.properties.FirstOrDefault(x => x.prop == Variables.ActividadDocente.participaCongresosNombreEvento)?.values.FirstOrDefault();
                    participacionCongresos.fecha = entityXML.properties.FirstOrDefault(x => x.prop == Variables.ActividadDocente.participaCongresosFechaPresentacion)?.values.FirstOrDefault();
                    participacionCongresos.entidadOrganizadora = entityXML.properties.FirstOrDefault(x => x.prop == Variables.ActividadDocente.participaCongresosEntidadOrganizadoraNombre)?.values.FirstOrDefault();
                    participacionCongresos.ID = Guid.NewGuid().ToString();
                    entidadesXML.Add(participacionCongresos.ID, participacionCongresos);
                }

                //2º Obtenemos las entidades de la BBDD.
                Dictionary<string, DisambiguableEntity> entidadesBBDD = ParticipacionCongresosFormacionDocente.GetBBDD(mResourceApi, mCvID, graph, propiedadesItem);
                var entidadesBBDDOpciones = entidadesBBDD.Select(x => new { x.Value.ID, x.Value.block }).ToList();

                //3º Comparamos las equivalentes
                equivalencias = Disambiguation.SimilarityBBDD(entidadesXML.Values.ToList(), entidadesBBDD.Values.ToList());

                foreach (var item in equivalencias.Values)
                {
                    listadoBloqueados.Add(entidadesBBDDOpciones.Where(x => x.ID.Equals(item)).Select(x => x.block).FirstOrDefault());
                }
            }

            //Comparamos si queremos Preimportar o actualizar las entidades.
            return CheckPreimportar(preimportar, listadoAux, entidadesXML, equivalencias, propTitle, graph, rdfType, rdfTypePrefix, propiedadesItem, RdfTypeTab, listadoBloqueados, listadoIdBBDD: listadoIdBBDD);
        }

        /// <summary>
        /// Metodo para sincronizar los datos pertenecientes al bloque 
        /// "Premios de innovación docente recibidos".
        /// Con el codigo identificativo 060.030.080.000
        /// </summary>
        public List<SubseccionItem> SincroPremiosInovacionDocente(bool procesar, [Optional] bool preimportar, [Optional] List<string> listadoIdBBDD, [Optional] PetitionStatus petitionStatus)
        {
            //Si procesar es false, no hago nada.
            if (!procesar)
            {
                return null;
            }

            List<string> propiedadesItem = new List<string>() { "http://w3id.org/roh/teachingExperience", "http://w3id.org/roh/teachingInnovationAwardsReceived", "http://vivoweb.org/ontology/core#relatedBy" };
            string graph = "accreditation";
            string propTitle = "http://w3id.org/roh/title";
            string rdfType = "http://w3id.org/roh/Accreditation";
            string rdfTypePrefix = "RelatedTeachingInnovationAwardsReceived";

            Dictionary<string, DisambiguableEntity> entidadesXML = new Dictionary<string, DisambiguableEntity>();
            Dictionary<string, string> equivalencias = new Dictionary<string, string>();
            List<bool> listadoBloqueados = new List<bool>();

            //1º Obtenemos la entidad del XML.
            List<Entity> listadoAux = GetPremiosInovacionDocente(listadoPremios, petitionStatus);

            if (listadoIdBBDD == null)
            {
                foreach (Entity entityXML in listadoAux)
                {
                    PremiosInnovacionDocente premiosInnovacion = new PremiosInnovacionDocente();
                    premiosInnovacion.nombre = entityXML.properties.FirstOrDefault(x => x.prop == Variables.ActividadDocente.premiosInnovaNombre)?.values.FirstOrDefault();
                    premiosInnovacion.entidadConcesion = entityXML.properties.FirstOrDefault(x => x.prop == Variables.ActividadDocente.premiosInnovaEntidadConcesionariaNombre)?.values.FirstOrDefault();
                    premiosInnovacion.fecha = entityXML.properties.FirstOrDefault(x => x.prop == Variables.ActividadDocente.premiosInnovaFechaConcesion)?.values.FirstOrDefault();
                    premiosInnovacion.ID = Guid.NewGuid().ToString();
                    entidadesXML.Add(premiosInnovacion.ID, premiosInnovacion);
                }

                //2º Obtenemos las entidades de la BBDD
                Dictionary<string, DisambiguableEntity> entidadesBBDD = PremiosInnovacionDocente.GetBBDD(mResourceApi, mCvID, graph, propiedadesItem);
                var entidadesBBDDOpciones = entidadesBBDD.Select(x => new { x.Value.ID, x.Value.block }).ToList();

                //3º Comparamos las equivalentes
                equivalencias = Disambiguation.SimilarityBBDD(entidadesXML.Values.ToList(), entidadesBBDD.Values.ToList());

                foreach (var item in equivalencias.Values)
                {
                    listadoBloqueados.Add(entidadesBBDDOpciones.Where(x => x.ID.Equals(item)).Select(x => x.block).FirstOrDefault());
                }
            }

            //Comparamos si queremos Preimportar o actualizar las entidades
            return CheckPreimportar(preimportar, listadoAux, entidadesXML, equivalencias, propTitle, graph, rdfType, rdfTypePrefix, propiedadesItem, RdfTypeTab, listadoBloqueados, listadoIdBBDD: listadoIdBBDD);
        }

        /// <summary>
        /// Metodo para sincronizar los datos pertenecientes al subapartado 
        /// "Otras actividades/méritos no incluidos en la relación anterior".
        /// Con el codigo identificativo 030.100.000.000
        /// </summary>
        public List<SubseccionItem> SincroOtrasActividades(bool procesar, [Optional] bool preimportar, [Optional] List<string> listadoIdBBDD, [Optional] PetitionStatus petitionStatus)
        {
            //Si procesar es false, no hago nada.
            if (!procesar)
            {
                return null;
            }

            List<string> propiedadesItem = new List<string>() { "http://w3id.org/roh/teachingExperience", "http://w3id.org/roh/otherActivities", "http://vivoweb.org/ontology/core#relatedBy" };
            string graph = "activity";
            string propTitle = "http://w3id.org/roh/title";
            string rdfType = "http://w3id.org/roh/Activity";
            string rdfTypePrefix = "RelatedOtherActivities";

            Dictionary<string, DisambiguableEntity> entidadesXML = new Dictionary<string, DisambiguableEntity>();
            Dictionary<string, string> equivalencias = new Dictionary<string, string>();
            List<bool> listadoBloqueados = new List<bool>();

            //1º Obtenemos la entidad del XML.
            List<Entity> listadoAux = GetOtrasActividades(listadoDatos, petitionStatus);

            if (listadoIdBBDD == null)
            {
                foreach (Entity entityXML in listadoAux)
                {
                    OtrasActividades otrasActividades = new OtrasActividades();
                    otrasActividades.descripcion = entityXML.properties.FirstOrDefault(x => x.prop == Variables.ActividadDocente.otrasActividadesDescripcion)?.values.FirstOrDefault();
                    otrasActividades.fecha = entityXML.properties.FirstOrDefault(x => x.prop == Variables.ActividadDocente.otrasActividadesFechaFinalizacion)?.values.FirstOrDefault();
                    otrasActividades.entidadOrganizadora = entityXML.properties.FirstOrDefault(x => x.prop == Variables.ActividadDocente.otrasActividadesEntidadOrganizadoraNombre)?.values.FirstOrDefault();
                    otrasActividades.ID = Guid.NewGuid().ToString();
                    entidadesXML.Add(otrasActividades.ID, otrasActividades);
                }

                //2º Obtenemos las entidades de la BBDD
                Dictionary<string, DisambiguableEntity> entidadesBBDD = OtrasActividades.GetBBDD(mResourceApi, mCvID, graph, propiedadesItem);
                var entidadesBBDDOpciones = entidadesBBDD.Select(x => new { x.Value.ID, x.Value.block }).ToList();

                //3º Comparamos las equivalentes
                equivalencias = Disambiguation.SimilarityBBDD(entidadesXML.Values.ToList(), entidadesBBDD.Values.ToList());

                foreach (var item in equivalencias.Values)
                {
                    listadoBloqueados.Add(entidadesBBDDOpciones.Where(x => x.ID.Equals(item)).Select(x => x.block).FirstOrDefault());
                }
            }

            //Comparamos si queremos Preimportar o actualizar las entidades
            return CheckPreimportar(preimportar, listadoAux, entidadesXML, equivalencias, propTitle, graph, rdfType, rdfTypePrefix, propiedadesItem, RdfTypeTab, listadoBloqueados, listadoIdBBDD: listadoIdBBDD);
        }

        /// <summary>
        /// Metodo para sincronizar los datos pertenecientes al subapartado 
        /// "Aportaciones más relevantes de su CV de docencia".
        /// Con el codigo identificativo 030.110.000.000
        /// </summary>
        public List<SubseccionItem> SincroAportacionesRelevantes(bool procesar, [Optional] bool preimportar, [Optional] List<string> listadoIdBBDD, [Optional] PetitionStatus petitionStatus)
        {
            //Si procesar es false, no hago nada.
            if (!procesar)
            {
                return null;
            }

            List<string> propiedadesItem = new List<string>() { "http://w3id.org/roh/teachingExperience", "http://w3id.org/roh/mostRelevantContributions", "http://vivoweb.org/ontology/core#relatedBy" };
            string graph = "activity";
            string propTitle = "http://w3id.org/roh/title";
            string rdfType = "http://w3id.org/roh/Activity";
            string rdfTypePrefix = "RelatedMostRelevantContributions";

            Dictionary<string, DisambiguableEntity> entidadesXML = new Dictionary<string, DisambiguableEntity>();
            Dictionary<string, string> equivalencias = new Dictionary<string, string>();
            List<bool> listadoBloqueados = new List<bool>();

            //1º Obtenemos la entidad del XML.
            List<Entity> listadoAux = GetAportacionesRelevantes(listadoDatos, petitionStatus);

            if (listadoIdBBDD == null)
            {
                foreach (Entity entityXML in listadoAux)
                {
                    AportacionesRelevantes aportacionesRelevantes = new AportacionesRelevantes();
                    aportacionesRelevantes.descripcion = entityXML.properties.FirstOrDefault(x => x.prop == Variables.ActividadDocente.aportacionesCVDescripcion)?.values.FirstOrDefault();
                    aportacionesRelevantes.fecha = entityXML.properties.FirstOrDefault(x => x.prop == Variables.ActividadDocente.aportacionesCVFechaFinalizacion)?.values.FirstOrDefault();
                    aportacionesRelevantes.entidadOrganizadora = entityXML.properties.FirstOrDefault(x => x.prop == Variables.ActividadDocente.aportacionesCVEntidadOrganizadoraNombre)?.values.FirstOrDefault();
                    aportacionesRelevantes.ID = Guid.NewGuid().ToString();
                    entidadesXML.Add(aportacionesRelevantes.ID, aportacionesRelevantes);
                }

                //2º Obtenemos las entidades de la BBDD
                Dictionary<string, DisambiguableEntity> entidadesBBDD = AportacionesRelevantes.GetBBDD(mResourceApi, mCvID, graph, propiedadesItem);
                var entidadesBBDDOpciones = entidadesBBDD.Select(x => new { x.Value.ID, x.Value.block }).ToList();

                //3º Comparamos las equivalentes
                equivalencias = Disambiguation.SimilarityBBDD(entidadesXML.Values.ToList(), entidadesBBDD.Values.ToList());

                foreach (var item in equivalencias.Values)
                {
                    listadoBloqueados.Add(entidadesBBDDOpciones.Where(x => x.ID.Equals(item)).Select(x => x.block).FirstOrDefault());
                }
            }

            //Comparamos si queremos Preimportar o actualizar las entidades
            return CheckPreimportar(preimportar, listadoAux, entidadesXML, equivalencias, propTitle, graph, rdfType, rdfTypePrefix, propiedadesItem, RdfTypeTab, listadoBloqueados, listadoIdBBDD: listadoIdBBDD);
        }





        /// <summary>
        /// 030.040.000.000
        /// </summary>
        private List<Entity> GetDireccionTesis(List<CvnItemBean> listadoDatos, [Optional] PetitionStatus petitionStatus)
        {
            List<Entity> listado = new List<Entity>();

            List<CvnItemBean> listadoDireccionTesis = listadoDatos.Where(x => x.Code.Equals("030.040.000.000")).ToList();
            if (listadoDireccionTesis.Count > 0)
            {
                foreach (CvnItemBean item in listadoDireccionTesis)
                {
                    //Actualizo el estado de los recursos tratados
                    if (petitionStatus != null)
                    {
                        petitionStatus.actualWork++;
                        petitionStatus.actualWorkSubtitle = "Dirección de tesis doctorales y/o proyectos fin de carrera";
                    }

                    Entity entidadAux = new Entity();
                    entidadAux.properties = new List<Property>();
                    if (!string.IsNullOrEmpty(item.GetStringPorIDCampo("030.040.000.030")))
                    {
                        entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                            new Property(Variables.Generico.codigoCVN, "030.040.000.000"),
                            new Property(Variables.Generico.personaCVN, mPersonID)
                        ));
                        entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                            new Property(Variables.ActividadDocente.direccionTesisTipoProyecto, item.GetTipoProyectoCharacterPorIDCampo("030.040.000.010")),
                            new Property(Variables.ActividadDocente.direccionTesisTipoProyectoOtros, item.GetStringPorIDCampo("030.040.000.020")),
                            new Property(Variables.ActividadDocente.direccionTesisTituloTrabajo, item.GetStringPorIDCampo("030.040.000.030")),
                            new Property(Variables.ActividadDocente.direccionTesisPaisEntidadRealizacion, item.GetPaisPorIDCampo("030.040.000.040")),
                            new Property(Variables.ActividadDocente.direccionTesisCCAAEntidadRealizacion, item.GetRegionPorIDCampo("030.040.000.050")),
                            new Property(Variables.ActividadDocente.direccionTesisCiudadEntidadRealizacion, item.GetStringPorIDCampo("030.040.000.070")),
                            new Property(Variables.ActividadDocente.direccionTesisFechaDefensa, item.GetStringDatetimePorIDCampo("030.040.000.140")),
                            new Property(Variables.ActividadDocente.direccionTesisCalificacionObtenida, item.GetStringPorIDCampo("030.040.000.150")),
                            new Property(Variables.ActividadDocente.direccionTesisFechaMencionDoctUE, item.GetStringDatetimePorIDCampo("030.040.000.160")),
                            new Property(Variables.ActividadDocente.direccionTesisMencionCalidad, item.GetStringBooleanPorIDCampo("030.040.000.170")),
                            new Property(Variables.ActividadDocente.direccionTesisDoctoradoUE, item.GetStringBooleanPorIDCampo("030.040.000.190")),
                            new Property(Variables.ActividadDocente.direccionTesisFechaMencionCalidad, item.GetStringDatetimePorIDCampo("030.040.000.200"))
                        ));
                        DireccionTesisPalabarasClave(item, entidadAux);
                        DireccionTesisEntidadRealizacion(item, entidadAux);
                        DireccionTesisAlumno(item, entidadAux);
                        DireccionTesisCodirectores(item, entidadAux);

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
        /// pertenecientes al Alumno.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="entidadAux"></param>
        private void DireccionTesisAlumno(CvnItemBean item, Entity entidadAux)
        {
            CvnItemBeanCvnAuthorBean alumno = item.GetElementoPorIDCampo<CvnItemBeanCvnAuthorBean>("030.040.000.120");

            //Compruebo que no es nulo.
            if (alumno == null) { return; }

            //Añado el director de la tesis.
            entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                new Property(Variables.ActividadDocente.direccionTesisAlumnoFirma, alumno.GetFirmaAutor()),
                new Property(Variables.ActividadDocente.direccionTesisAlumnoNombre, alumno.GetNombreAutor()),
                new Property(Variables.ActividadDocente.direccionTesisAlumnoPrimerApellido, alumno.GetPrimerApellidoAutor()),
                new Property(Variables.ActividadDocente.direccionTesisAlumnoSegundoApellido, alumno.GetSegundoApellidoAutor())
            ));
        }

        /// <summary>
        /// Inserta en <paramref name="entidadAux"/> los valores de <paramref name="item"/>,
        /// pertenecientes a los codirectores de la tesis.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="entidadAux"></param>
        private void DireccionTesisCodirectores(CvnItemBean item, Entity entidadAux)
        {
            List<CvnItemBeanCvnAuthorBean> listadoCodirectores = item.GetListaElementosPorIDCampo<CvnItemBeanCvnAuthorBean>("030.040.000.180");

            string propiedadFirma = Variables.ActividadDocente.direccionTesisCodirectorTesisFirma;
            string propiedadOrden = Variables.ActividadDocente.direccionTesisCodirectorTesisOrden;
            string propiedadNombre = Variables.ActividadDocente.direccionTesisCodirectorTesisNombre;
            string propiedadPrimerApellido = Variables.ActividadDocente.direccionTesisCodirectorTesisPrimerApellido;
            string propiedadSegundoApellido = Variables.ActividadDocente.direccionTesisCodirectorTesisSegundoApellido;

            UtilitySecciones.InsertaAutorProperties(listadoCodirectores, entidadAux, propiedadFirma, propiedadOrden,
                propiedadNombre, propiedadPrimerApellido, propiedadSegundoApellido);
        }

        /// <summary>
        /// Inserta en <paramref name="entidadAux"/> los valores de <paramref name="item"/>,
        /// pertenecientes a la Entidad de realización.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="entidadAux"></param>
        private void DireccionTesisEntidadRealizacion(CvnItemBean item, Entity entidadAux)
        {
            //Añado la referencia si existe Entidad
            UtilitySecciones.AniadirEntidadOrganizacion(mResourceApi, item.GetNameEntityBeanPorIDCampo("030.040.000.080"),
                Variables.ActividadDocente.direccionTesisEntidadRealizacionNombre,
                Variables.ActividadDocente.direccionTesisEntidadRealizacion, entidadAux);

            //Añado otros, o el ID de una preseleccion
            string valorTipo = !string.IsNullOrEmpty(item.GetStringPorIDCampo("030.040.000.110")) ? mResourceApi.GraphsUrl + "items/organizationtype_OTHERS" : item.GetOrganizacionPorIDCampo("030.040.000.100");

            entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                new Property(Variables.ActividadDocente.direccionTesisTipoEntidadRealizacion, valorTipo),
                new Property(Variables.ActividadDocente.direccionTesisTipoEntidadRealizacionOtros, item.GetStringPorIDCampo("030.040.000.110"))
            ));
        }

        /// <summary>
        /// Inserta en <paramref name="entidadAux"/> los valores de <paramref name="item"/>,
        /// pertenecientes a las palabras clave.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="entidadAux"></param>
        private void DireccionTesisPalabarasClave(CvnItemBean item, Entity entidadAux)
        {
            List<CvnItemBeanCvnString> listadoPalabrasClave = item.GetListaElementosPorIDCampo<CvnItemBeanCvnString>("030.040.000.130");

            string propiedadPalabrasClave = Variables.ActividadDocente.direccionTesisPalabrasClave;

            foreach (CvnItemBeanCvnString palabraClave in listadoPalabrasClave)
            {
                string entityPartAux = Guid.NewGuid().ToString() + "@@@";

                List<string> listadoPalabras = Utility.GetPadresPalabrasClave(palabraClave);
                foreach (string palabra in listadoPalabras)
                {
                    Property propertyPalabrasClave = entidadAux.properties.FirstOrDefault(x => x.prop == Variables.ActividadDocente.direccionTesisPalabrasClave);
                    UtilitySecciones.CheckProperty(propertyPalabrasClave, entidadAux,
                        UtilitySecciones.StringGNOSSID(entityPartAux, Utility.ObtenerPalabraClave(mResourceApi, palabra)), propiedadPalabrasClave);
                }
            }
        }

        /// <summary>
        /// 030.010.000.000
        /// </summary>
        private List<Entity> GetFormacionAcademica(List<CvnItemBean> listadoDatos, [Optional] PetitionStatus petitionStatus)
        {
            List<Entity> listado = new List<Entity>();

            List<CvnItemBean> listadoFormacionAcademica = listadoDatos.Where(x => x.Code.Equals("030.010.000.000")).ToList();
            if (listadoFormacionAcademica.Count > 0)
            {
                foreach (CvnItemBean item in listadoFormacionAcademica)
                {
                    //Actualizo el estado de los recursos tratados
                    if (petitionStatus != null)
                    {
                        petitionStatus.actualWork++;
                        petitionStatus.actualWorkSubtitle = "Formación académica impartida";
                    }

                    Entity entidadAux = new Entity();
                    entidadAux.properties = new List<Property>();
                    if (!string.IsNullOrEmpty(item.GetNameTitleBeanPorIDCampo("030.010.000.020")))
                    {
                        entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                            new Property(Variables.Generico.codigoCVN, "030.010.000.000"),
                            new Property(Variables.Generico.personaCVN, mPersonID)
                        ));
                        entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                            new Property(Variables.ActividadDocente.formacionAcademicaTipoDocenciaOficialidad, item.GetTipoDocenciaOficialidadPorIDCampo("030.010.000.010")),
                            new Property(Variables.ActividadDocente.formacionAcademicaPaisEntidadRealizacion, item.GetPaisPorIDCampo("030.010.000.040")),
                            new Property(Variables.ActividadDocente.formacionAcademicaCCAAEntidadRealizacion, item.GetRegionPorIDCampo("030.010.000.050")),
                            new Property(Variables.ActividadDocente.formacionAcademicaCiudadEntidadRealizacion, item.GetStringPorIDCampo("030.010.000.070")),
                            new Property(Variables.ActividadDocente.formacionAcademicaDepartamento, item.GetStringPorIDCampo("030.010.000.130")),
                            new Property(Variables.ActividadDocente.formacionAcademicaTipoPrograma, item.GetTipoProgramaPorIDCampo("030.010.000.140")),
                            new Property(Variables.ActividadDocente.formacionAcademicaTipoProgramaOtros, item.GetStringPorIDCampo("030.010.000.150")),
                            new Property(Variables.ActividadDocente.formacionAcademicaNombreAsignatura, item.GetStringPorIDCampo("030.010.000.160")),
                            new Property(Variables.ActividadDocente.formacionAcademicaTipoDocenciaModalidad, item.GetTipoDocenciaModalidadPorIDCampo("030.010.000.170")),
                            new Property(Variables.ActividadDocente.formacionAcademicaTipoDocenciaModalidadOtros, item.GetStringPorIDCampo("030.010.000.180")),
                            new Property(Variables.ActividadDocente.formacionAcademicaTipoAsignatura, item.GetTipoCursoPorIDCampo("030.010.000.190")),
                            new Property(Variables.ActividadDocente.formacionAcademicaTipoAsignaturaOtros, item.GetStringPorIDCampo("030.010.000.430")),
                            new Property(Variables.ActividadDocente.formacionAcademicaCursoTitulacion, item.GetStringPorIDCampo("030.010.000.200")),
                            new Property(Variables.ActividadDocente.formacionAcademicaTipoECTS, item.GetHorasCreditosECTSPorIDCampo("030.010.000.210")),
                            new Property(Variables.ActividadDocente.formacionAcademicaNumeroECTS, item.GetStringDoublePorIDCampo("030.010.000.220")),
                            new Property(Variables.ActividadDocente.formacionAcademicaIdiomaAsignatura, item.GetTraduccion("030.010.000.230")),
                            new Property(Variables.ActividadDocente.formacionAcademicaFrecuenciaAsignatura, item.GetStringDoublePorIDCampo("030.010.000.240")),
                            new Property(Variables.ActividadDocente.formacionAcademicaCompetenciasRelacionadas, item.GetStringPorIDCampo("030.010.000.260")),
                            new Property(Variables.ActividadDocente.formacionAcademicaCategoriaProfesional, item.GetStringPorIDCampo("030.010.000.270")),
                            new Property(Variables.ActividadDocente.formacionAcademicaCalificacionObtenida, item.GetStringPorIDCampo("030.010.000.280")),
                            new Property(Variables.ActividadDocente.formacionAcademicaCalificacionMax, item.GetStringPorIDCampo("030.010.000.290")),
                            new Property(Variables.ActividadDocente.formacionAcademicaPaisEntidadEvaluacion, item.GetPaisPorIDCampo("030.010.000.440")),
                            new Property(Variables.ActividadDocente.formacionAcademicaCCAAEntidadEvaluacion, item.GetRegionPorIDCampo("030.010.000.450")),
                            new Property(Variables.ActividadDocente.formacionAcademicaCiudadEntidadEvaluacion, item.GetStringPorIDCampo("030.010.000.470")),
                            new Property(Variables.ActividadDocente.formacionAcademicaTipoEvaluacion, item.GetTipoEvaluacionPorIDCampo("030.010.000.320")),
                            new Property(Variables.ActividadDocente.formacionAcademicaTipoEvaluacionOtros, item.GetStringPorIDCampo("030.010.000.330")),
                            new Property(Variables.ActividadDocente.formacionAcademicaPaisEntidadFinanciadora, item.GetPaisPorIDCampo("030.010.000.480")),
                            new Property(Variables.ActividadDocente.formacionAcademicaCCAAEntidadFinanciadora, item.GetRegionPorIDCampo("030.010.000.500")),
                            new Property(Variables.ActividadDocente.formacionAcademicaCiudadEntidadFinanciadora, item.GetStringPorIDCampo("030.010.000.510")),
                            new Property(Variables.ActividadDocente.formacionAcademicaEntFinanTipoConvocatoria, item.GetTipoConvocatoriaPorIDCampo("030.010.000.390")),
                            new Property(Variables.ActividadDocente.formacionAcademicaEntFinanTipoConvocatoriaOtros, item.GetStringPorIDCampo("030.010.000.400")),
                            new Property(Variables.ActividadDocente.formacionAcademicaEntFinanAmbitoGeo, item.GetGeographicRegionPorIDCampo("030.010.000.410")),
                            new Property(Variables.ActividadDocente.formacionAcademicaEntFinanAmbitoGeoOtros, item.GetStringPorIDCampo("030.010.000.420")),
                            new Property(Variables.ActividadDocente.formacionAcademicaFacultadEscuela, item.GetNameEntityBeanPorIDCampo("030.010.000.540")),
                            new Property(Variables.ActividadDocente.formacionAcademicaFechaInicio, item.GetStringDatetimePorIDCampo("030.010.000.550")),
                            new Property(Variables.ActividadDocente.formacionAcademicaFechaFinalizacion, item.GetStringDatetimePorIDCampo("030.010.000.610"))
                        ));
                        FormacionAcademicaTitulacionUniversitaria(item, entidadAux);
                        FormacionAcademicaEntidadRealizacion(item, entidadAux);
                        FormacionAcademicaEntidadFinanciadora(item, entidadAux);
                        FormacionAcademicaEntidadEvaluacion(item, entidadAux);

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
        /// pertenecientes a la Titulación universitaria.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="entidadAux"></param>
        private void FormacionAcademicaTitulacionUniversitaria(CvnItemBean item, Entity entidadAux)
        {
            //Añado la referencia si existiese.
            string valorTitulacionUniversitaria = !string.IsNullOrEmpty(item.GetIdentificationTitleBeanPorIDCampo("030.010.000.020")) ?
                mResourceApi.GraphsUrl + "items/degreetype_" + item.GetIdentificationTitleBeanPorIDCampo("030.010.000.020") : "";

            entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                new Property(Variables.ActividadDocente.formacionAcademicaTitulacionUniversitariaNombre, item.GetNameTitleBeanPorIDCampo("030.010.000.020")),
                new Property(Variables.ActividadDocente.formacionAcademicaTitulacionUniversitaria, valorTitulacionUniversitaria)
            ));
        }

        /// <summary>
        /// Inserta en <paramref name="entidadAux"/> los valores de <paramref name="item"/>,
        /// pertenecientes a la Entidad de evaluación.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="entidadAux"></param>
        private void FormacionAcademicaEntidadEvaluacion(CvnItemBean item, Entity entidadAux)
        {
            //Añado la referencia si existe Entidad
            UtilitySecciones.AniadirEntidadOrganizacion(mResourceApi, item.GetNameEntityBeanPorIDCampo("030.010.000.300"),
                Variables.ActividadDocente.formacionAcademicaEntidadEvaluacionNombre,
                Variables.ActividadDocente.formacionAcademicaEntidadEvaluacion, entidadAux);

            //Añado otros, o el ID de una preseleccion
            string valorTipo = !string.IsNullOrEmpty(item.GetStringPorIDCampo("030.010.000.530")) ? mResourceApi.GraphsUrl + "items/organizationtype_OTHERS" : item.GetOrganizacionPorIDCampo("030.010.000.520");

            entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                new Property(Variables.ActividadDocente.formacionAcademicaTipoEntidadEvaluacion, valorTipo),
                new Property(Variables.ActividadDocente.formacionAcademicaTipoEntidadEvaluacionOtros, item.GetStringPorIDCampo("030.010.000.530"))
            ));
        }

        /// <summary>
        /// Inserta en <paramref name="entidadAux"/> los valores de <paramref name="item"/>,
        /// pertenecientes a la Entidad financiadora.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="entidadAux"></param>
        private void FormacionAcademicaEntidadFinanciadora(CvnItemBean item, Entity entidadAux)
        {
            //Añado la referencia si existe Entidad
            UtilitySecciones.AniadirEntidadOrganizacion(mResourceApi, item.GetNameEntityBeanPorIDCampo("030.010.000.350"),
                Variables.ActividadDocente.formacionAcademicaEntidadFinanciadoraNombre,
                Variables.ActividadDocente.formacionAcademicaEntidadFinanciadora, entidadAux);

            //Añado otros, o el ID de una preseleccion
            string valorTipo = !string.IsNullOrEmpty(item.GetStringPorIDCampo("030.010.000.380")) ? mResourceApi.GraphsUrl + "items/organizationtype_OTHERS" : item.GetOrganizacionPorIDCampo("030.010.000.370");

            entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                new Property(Variables.ActividadDocente.formacionAcademicaTipoEntidadFinanciadora, valorTipo),
                new Property(Variables.ActividadDocente.formacionAcademicaTipoEntidadFinanciadoraOtros, item.GetStringPorIDCampo("030.010.000.380"))
            ));
        }

        /// <summary>
        /// Inserta en <paramref name="entidadAux"/> los valores de <paramref name="item"/>,
        /// pertenecientes a la Entidad de realización.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="entidadAux"></param>
        private void FormacionAcademicaEntidadRealizacion(CvnItemBean item, Entity entidadAux)
        {
            //Añado la referencia si existe Entidad
            UtilitySecciones.AniadirEntidadOrganizacion(mResourceApi, item.GetNameEntityBeanPorIDCampo("030.010.000.080"),
                Variables.ActividadDocente.formacionAcademicaEntidadRealizacionNombre,
                Variables.ActividadDocente.formacionAcademicaEntidadRealizacion, entidadAux);

            //Añado otros, o el ID de una preseleccion
            string valorTipo = !string.IsNullOrEmpty(item.GetStringPorIDCampo("030.010.000.120")) ? mResourceApi.GraphsUrl + "items/organizationtype_OTHERS" : item.GetOrganizacionPorIDCampo("030.010.000.110");

            entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                new Property(Variables.ActividadDocente.formacionAcademicaTipoEntidadRealizacion, valorTipo),
                new Property(Variables.ActividadDocente.formacionAcademicaTipoEntidadRealizacionOtros, item.GetStringPorIDCampo("030.010.000.120"))
            ));
        }

        /// <summary>
        /// 030.050.000.000
        /// </summary>
        private List<Entity> GetTutoriasAcademicas(List<CvnItemBean> listadoDatos, [Optional] PetitionStatus petitionStatus)
        {
            List<Entity> listado = new List<Entity>();

            List<CvnItemBean> listadoTutoriasAcademicas = listadoDatos.Where(x => x.Code.Equals("030.050.000.000")).ToList();
            if (listadoTutoriasAcademicas.Count > 0)
            {
                foreach (CvnItemBean item in listadoTutoriasAcademicas)
                {
                    //Actualizo el estado de los recursos tratados
                    if (petitionStatus != null)
                    {
                        petitionStatus.actualWork++;
                        petitionStatus.actualWorkSubtitle = "Tutorías académicas de estudiantes";
                    }

                    Entity entidadAux = new Entity();
                    entidadAux.properties = new List<Property>();
                    if (!string.IsNullOrEmpty(item.GetStringPorIDCampo("030.050.000.010")))
                    {
                        entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                            new Property(Variables.Generico.codigoCVN, "030.050.000.000"),
                            new Property(Variables.Generico.personaCVN, mPersonID)
                        ));
                        entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                            new Property(Variables.ActividadDocente.tutoAcademicaPaisEntidadRealizacion, item.GetPaisPorIDCampo("030.050.000.030")),
                            new Property(Variables.ActividadDocente.tutoAcademicaCCAAEntidadRealizacion, item.GetRegionPorIDCampo("030.050.000.040")),
                            new Property(Variables.ActividadDocente.tutoAcademicaCiudadEntidadRealizacion, item.GetStringPorIDCampo("030.050.000.060")),
                            new Property(Variables.ActividadDocente.tutoAcademicaNumAlumnosTutelados, item.GetStringDoublePorIDCampo("030.050.000.110")),
                            new Property(Variables.ActividadDocente.tutoAcademicaFrecuenciaActividad, item.GetStringDoublePorIDCampo("030.050.000.120")),
                            new Property(Variables.ActividadDocente.tutoAcademicaNumHorasECTS, item.GetStringDoublePorIDCampo("030.050.000.130"))
                        ));
                        TutoriasAcademicasNombrePrograma(item, entidadAux);
                        TutoriasAcademicasEntidadRealizacion(item, entidadAux);

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
        /// pertenecientes al Nombre del programa y el campo otros del mismo.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="entidadAux"></param>
        private void TutoriasAcademicasNombrePrograma(CvnItemBean item, Entity entidadAux)
        {
            if (item.GetStringPorIDCampo("030.050.000.010").Equals("OTHERS"))
            {
                entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                    new Property(Variables.ActividadDocente.tutoAcademicaNombrePrograma, item.GetTipoProgramaTutorizacionPorIDCampo("030.050.000.010")),
                    new Property(Variables.ActividadDocente.tutoAcademicaNombreProgramaOtros, item.GetStringPorIDCampo("030.050.000.020"))
                ));
            }
            else
            {
                entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                    new Property(Variables.ActividadDocente.tutoAcademicaNombrePrograma, item.GetTipoProgramaTutorizacionPorIDCampo("030.050.000.010"))
                ));
            }
        }

        /// <summary>
        /// Inserta en <paramref name="entidadAux"/> los valores de <paramref name="item"/>,
        /// pertenecientes a la Entidad de Realización.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="entidadAux"></param>
        private void TutoriasAcademicasEntidadRealizacion(CvnItemBean item, Entity entidadAux)
        {
            //Añado la referencia si existe Entidad
            UtilitySecciones.AniadirEntidadOrganizacion(mResourceApi, item.GetNameEntityBeanPorIDCampo("030.050.000.070"),
                Variables.ActividadDocente.tutoAcademicaEntidadRealizacionNombre,
                Variables.ActividadDocente.tutoAcademicaEntidadRealizacion, entidadAux);

            //Añado otros, o el ID de una preseleccion
            string valorTipo = !string.IsNullOrEmpty(item.GetStringPorIDCampo("030.050.000.100")) ? mResourceApi.GraphsUrl + "items/organizationtype_OTHERS" : item.GetOrganizacionPorIDCampo("030.050.000.090");

            entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                new Property(Variables.ActividadDocente.tutoAcademicaTipoEntidadRealizacion, valorTipo),
                new Property(Variables.ActividadDocente.tutoAcademicaTipoEntidadRealizacionOtros, item.GetStringPorIDCampo("030.050.000.100"))
            ));
        }

        /// <summary>
        /// 030.060.000.000
        /// </summary>
        private List<Entity> GetCursosSeminarios(List<CvnItemBean> listadoDatos, [Optional] PetitionStatus petitionStatus)
        {
            List<Entity> listado = new List<Entity>();

            List<CvnItemBean> listadoCursosSeminarios = listadoDatos.Where(x => x.Code.Equals("030.060.000.000")).ToList();
            if (listadoCursosSeminarios.Count > 0)
            {
                foreach (CvnItemBean item in listadoCursosSeminarios)
                {
                    //Actualizo el estado de los recursos tratados
                    if (petitionStatus != null)
                    {
                        petitionStatus.actualWork++;
                        petitionStatus.actualWorkSubtitle = "Cursos y seminarios impartidos orientados a la formación docente universitaria";
                    }

                    Entity entidadAux = new Entity();
                    entidadAux.properties = new List<Property>();
                    if (!string.IsNullOrEmpty(item.GetStringPorIDCampo("030.060.000.030")))
                    {
                        entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                            new Property(Variables.Generico.codigoCVN, "030.060.000.000"),
                            new Property(Variables.Generico.personaCVN, mPersonID)
                        ));
                        entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                            new Property(Variables.ActividadDocente.cursosSeminariosTipoEvento, item.GetTipoEventoPorIDCampo("030.060.000.010")),
                            new Property(Variables.ActividadDocente.cursosSeminariosTipoEventoOtros, item.GetStringPorIDCampo("030.060.000.020")),
                            new Property(Variables.ActividadDocente.cursosSeminariosNombreEvento, item.GetStringPorIDCampo("030.060.000.030")),
                            new Property(Variables.ActividadDocente.cursosSeminariosPaisEntidadOrganizadora, item.GetPaisPorIDCampo("030.060.000.040")),
                            new Property(Variables.ActividadDocente.cursosSeminariosCCAAEntidadOrganizadora, item.GetRegionPorIDCampo("030.060.000.050")),
                            new Property(Variables.ActividadDocente.cursosSeminariosCiudadEntidadOrganizadora, item.GetStringPorIDCampo("030.060.000.070")),
                            new Property(Variables.ActividadDocente.cursosSeminariosObjetivosCurso, item.GetStringPorIDCampo("030.060.000.120")),
                            new Property(Variables.ActividadDocente.cursosSeminariosPerfilDestinatarios, item.GetStringPorIDCampo("030.060.000.130")),
                            new Property(Variables.ActividadDocente.cursosSeminariosIdiomaImpartio, item.GetTraduccion("030.060.000.140")),
                            new Property(Variables.ActividadDocente.cursosSeminariosFechaImparticion, item.GetStringDatetimePorIDCampo("030.060.000.150")),
                            new Property(Variables.ActividadDocente.cursosSeminariosHorasImpartidas, item.GetStringDoublePorIDCampo("030.060.000.160")),
                            new Property(Variables.ActividadDocente.cursosSeminariosTipoParticipacion, item.GetTipoParticipacionDocumentoPorIDCampo("030.060.000.170")),
                            new Property(Variables.ActividadDocente.cursosSeminariosTipoParticipacionOtros, item.GetStringPorIDCampo("030.060.000.180")),
                            new Property(Variables.ActividadDocente.cursosSeminariosAutorCorrespondencia, item.GetStringBooleanPorIDCampo("030.060.000.200"))
                        ));
                        CursosSeminariosEntidadOrganizadora(item, entidadAux);
                        CursosSeminariosISBN(item, entidadAux);
                        CursosSeminariosISSN(item, entidadAux);
                        CursosSeminariosIDPublicacion(item, entidadAux);

                        listado.Add(entidadAux);
                    }
                    else
                    {
                        listadoDatos.Remove(item);
                    }
                }
            }
            return listado;
        }

        /// <summary>
        /// Inserta en <paramref name="entidadAux"/> los valores de <paramref name="item"/>,
        /// pertenecientes a los identificadores de publicación (Handle, DOI, PMID u otros).
        /// </summary>
        /// <param name="item"></param>
        /// <param name="entidadAux"></param>
        private void CursosSeminariosIDPublicacion(CvnItemBean item, Entity entidadAux)
        {
            List<CvnItemBeanCvnExternalPKBean> listadoIDs = item.GetListaElementosPorIDCampo<CvnItemBeanCvnExternalPKBean>("030.060.000.210");
            string propIdHandle = Variables.ActividadDocente.cursosSeminariosIDPubDigitalHandle;
            string propIdDOI = Variables.ActividadDocente.cursosSeminariosIDPubDigitalDOI;
            string propIdPMID = Variables.ActividadDocente.cursosSeminariosIDPubDigitalPMID;
            string propIdOtroPub = Variables.ActividadDocente.cursosSeminariosIDOtroPubDigital;
            string nombreOtroPub = Variables.ActividadDocente.cursosSeminariosNombreOtroIDPubDigital;

            UtilitySecciones.InsertaTiposIDPublicacion(listadoIDs, entidadAux, propIdHandle, propIdDOI, propIdPMID, propIdOtroPub, nombreOtroPub);
        }

        /// <summary>
        /// Inserta en <paramref name="entidadAux"/> los valores de <paramref name="item"/>,
        /// pertenecientes a la Entidad Organizadora.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="entidadAux"></param>
        private void CursosSeminariosEntidadOrganizadora(CvnItemBean item, Entity entidadAux)
        {
            //Añado la referencia si existe Entidad
            UtilitySecciones.AniadirEntidadOrganizacion(mResourceApi, item.GetNameEntityBeanPorIDCampo("030.060.000.080"),
                Variables.ActividadDocente.cursosSeminariosEntidadOrganizadoraNombre,
                Variables.ActividadDocente.cursosSeminariosEntidadOrganizadora, entidadAux);

            //Añado otros, o el ID de una preseleccion
            string valorTipo = !string.IsNullOrEmpty(item.GetStringPorIDCampo("030.060.000.110")) ? mResourceApi.GraphsUrl + "items/organizationtype_OTHERS" : item.GetOrganizacionPorIDCampo("030.060.000.100");

            entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                new Property(Variables.ActividadDocente.cursosSeminariosTipoEntidadOrganizadora, valorTipo),
                new Property(Variables.ActividadDocente.cursosSeminariosTipoEntidadOrganizadoraOtros, item.GetStringPorIDCampo("030.060.000.110"))
            ));
        }

        /// <summary>
        /// Inserta en <paramref name="entidadAux"/> los valores de <paramref name="item"/>,
        /// pertenecientes al ISBN.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="entidadAux"></param>
        private void CursosSeminariosISBN(CvnItemBean item, Entity entidadAux)
        {
            List<CvnItemBeanCvnExternalPKBean> listadoISBN = item.GetListaElementosPorIDCampo<CvnItemBeanCvnExternalPKBean>("030.060.000.190");
            string propiedadISBN = Variables.ActividadDocente.cursosSeminariosISBN;

            UtilitySecciones.InsertaISBN(listadoISBN, entidadAux, propiedadISBN);
        }

        /// <summary>
        /// Inserta en <paramref name="entidadAux"/> los valores de <paramref name="item"/>,
        /// pertenecientes al ISSN.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="entidadAux"></param>
        private void CursosSeminariosISSN(CvnItemBean item, Entity entidadAux)
        {
            List<CvnItemBeanCvnExternalPKBean> listadoISSN = item.GetListaElementosPorIDCampo<CvnItemBeanCvnExternalPKBean>("030.060.000.190");
            string propiedadISSN = Variables.ActividadDocente.cursosSeminariosISSN;

            UtilitySecciones.InsertaISSN(listadoISSN, entidadAux, propiedadISSN);
        }

        /// <summary>
        /// 030.070.000.000
        /// </summary>
        private List<Entity> GetPublicacionDocentes(List<CvnItemBean> listadoDatos, [Optional] PetitionStatus petitionStatus)
        {
            List<Entity> listado = new List<Entity>();

            List<CvnItemBean> listadoPublicacionDocentes = listadoDatos.Where(x => x.Code.Equals("030.070.000.000")).ToList();
            if (listadoPublicacionDocentes.Count > 0)
            {
                foreach (CvnItemBean item in listadoPublicacionDocentes)
                {
                    //Actualizo el estado de los recursos tratados
                    if (petitionStatus != null)
                    {
                        petitionStatus.actualWork++;
                        petitionStatus.actualWorkSubtitle = "Publicaciones docentes o de carácter pedagógico, libros, artículos, etc.";
                    }

                    Entity entidadAux = new Entity();
                    entidadAux.properties = new List<Property>();
                    if (!string.IsNullOrEmpty(item.GetStringPorIDCampo("030.070.000.010")))
                    {
                        entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                            new Property(Variables.Generico.codigoCVN, "030.070.000.000"),
                            new Property(Variables.Generico.personaCVN, mPersonID)
                        ));
                        entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                            new Property(Variables.ActividadDocente.publicacionDocenteNombre, item.GetStringPorIDCampo("030.070.000.010")),
                            new Property(Variables.ActividadDocente.publicacionDocentePerfilDestinatario, item.GetStringPorIDCampo("030.070.000.020")),
                            new Property(Variables.ActividadDocente.publicacionDocentePosicionFirma, item.GetStringDoublePorIDCampo("030.070.000.040")),
                            new Property(Variables.ActividadDocente.publicacionDocenteFechaElaboracion, item.GetStringDatetimePorIDCampo("030.070.000.050")),
                            new Property(Variables.ActividadDocente.publicacionDocenteTipologiaSoporte, item.GetTipoSoportePorIDCampo("030.070.000.060")),
                            new Property(Variables.ActividadDocente.publicacionDocenteTipologiaSoporteOtros, item.GetStringPorIDCampo("030.070.000.070")),
                            new Property(Variables.ActividadDocente.publicacionDocenteTituloPublicacion, item.GetStringPorIDCampo("030.070.000.080")),
                            new Property(Variables.ActividadDocente.publicacionDocenteNombrePublicacion, item.GetStringPorIDCampo("030.070.000.190")),
                            new Property(Variables.ActividadDocente.publicacionDocenteVolumenPublicacion, item.GetVolumenPorIDCampo("030.070.000.090")),
                            new Property(Variables.ActividadDocente.publicacionDocenteNumeroPublicacion, item.GetNumeroVolumenPorIDCampo("030.070.000.090")),
                            new Property(Variables.ActividadDocente.publicacionDocentePagIniPublicacion, item.GetPaginaInicialPorIDCampo("030.070.000.100")),
                            new Property(Variables.ActividadDocente.publicacionDocentePagFinalPublicacion, item.GetPaginaFinalPorIDCampo("030.070.000.100")),
                            new Property(Variables.ActividadDocente.publicacionDocenteEditorialPublicacion, item.GetStringPorIDCampo("030.070.000.110")),
                            new Property(Variables.ActividadDocente.publicacionDocentePaisPublicacion, item.GetPaisPorIDCampo("030.070.000.120")),
                            new Property(Variables.ActividadDocente.publicacionDocenteCCAAPublicacion, item.GetRegionPorIDCampo("030.070.000.130")),
                            new Property(Variables.ActividadDocente.publicacionDocenteFechaPublicacion, item.GetStringDatetimePorIDCampo("030.070.000.150")),
                            new Property(Variables.ActividadDocente.publicacionDocenteURLPublicacion, item.GetStringPorIDCampo("030.070.000.160")),
                            new Property(Variables.ActividadDocente.publicacionDocenteDepositoLegal, item.GetValueCvnExternalPKBean("030.070.000.180")),
                            new Property(Variables.ActividadDocente.publicacionDocenteJustificacionMaterial, item.GetStringPorIDCampo("030.070.000.200")),
                            new Property(Variables.ActividadDocente.publicacionDocenteGradoContribucion, item.GetGradoContribucionDocumentoPorIDCampo("030.070.000.210")),
                            new Property(Variables.ActividadDocente.publicacionDocenteAutorCorrespondencia, item.GetStringBooleanPorIDCampo("030.070.000.220"))
                        ));
                        PublicacionDocentesISBN(item, entidadAux);
                        PublicacionDocentesISSN(item, entidadAux);
                        PublicacionDocentesIDPublicacion(item, entidadAux);
                        PublicacionDocentesAutores(item, entidadAux);

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
        /// pertenecientes a los Autores.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="entidadAux"></param>
        private void PublicacionDocentesAutores(CvnItemBean item, Entity entidadAux)
        {
            List<CvnItemBeanCvnAuthorBean> listadoAutores = item.GetListaElementosPorIDCampo<CvnItemBeanCvnAuthorBean>("030.070.000.030");

            string propiedadAutorFirma = Variables.ActividadDocente.publicacionDocenteAutorFirma;
            string propiedadAutorOrden = Variables.ActividadDocente.publicacionDocenteAutorOrden;
            string propiedadAutorNombre = Variables.ActividadDocente.publicacionDocenteAutorNombre;
            string propiedadAutorPrimerApellido = Variables.ActividadDocente.publicacionDocenteAutorPrimerApellido;
            string propiedadAutorSegundoApellido = Variables.ActividadDocente.publicacionDocenteAutorSegundoApellido;

            UtilitySecciones.InsertaAutorProperties(listadoAutores, entidadAux, propiedadAutorFirma, propiedadAutorOrden,
                propiedadAutorNombre, propiedadAutorPrimerApellido, propiedadAutorSegundoApellido);
        }

        /// <summary>
        /// Inserta en <paramref name="entidadAux"/> los valores de <paramref name="item"/>,
        /// pertenecientes a los identificadores de publicación (Handle, DOI, PMID u otros).
        /// </summary>
        /// <param name="item"></param>
        /// <param name="entidadAux"></param>
        private void PublicacionDocentesIDPublicacion(CvnItemBean item, Entity entidadAux)
        {
            List<CvnItemBeanCvnExternalPKBean> listadoIDs = item.GetListaElementosPorIDCampo<CvnItemBeanCvnExternalPKBean>("030.070.000.230");
            string propIdHandle = Variables.ActividadDocente.publicacionDocenteIDPubDigitalHandle;
            string propIdDOI = Variables.ActividadDocente.publicacionDocenteIDPubDigitalDOI;
            string propIdPMID = Variables.ActividadDocente.publicacionDocenteIDPubDigitalPMID;
            string propIdOtroPub = Variables.ActividadDocente.publicacionDocenteIDOtroPubDigital;
            string nombreOtroPub = Variables.ActividadDocente.publicacionDocenteNombreOtroIDPubDigital;

            UtilitySecciones.InsertaTiposIDPublicacion(listadoIDs, entidadAux, propIdHandle, propIdDOI, propIdPMID, propIdOtroPub, nombreOtroPub);
        }

        /// <summary>
        /// Inserta en <paramref name="entidadAux"/> los valores de <paramref name="item"/>,
        /// pertenecientes al ISBN.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="entidadAux"></param>
        private void PublicacionDocentesISBN(CvnItemBean item, Entity entidadAux)
        {
            List<CvnItemBeanCvnExternalPKBean> listadoISBN = item.GetListaElementosPorIDCampo<CvnItemBeanCvnExternalPKBean>("030.070.000.170");
            string propiedadISBN = Variables.ActividadDocente.publicacionDocenteISBNPublicacion;

            UtilitySecciones.InsertaISBN(listadoISBN, entidadAux, propiedadISBN);
        }

        /// <summary>
        /// Inserta en <paramref name="entidadAux"/> los valores de <paramref name="item"/>,
        /// pertenecientes al ISSN.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="entidadAux"></param>
        private void PublicacionDocentesISSN(CvnItemBean item, Entity entidadAux)
        {
            List<CvnItemBeanCvnExternalPKBean> listadoISSN = item.GetListaElementosPorIDCampo<CvnItemBeanCvnExternalPKBean>("030.070.000.170");
            string propiedadISSN = Variables.ActividadDocente.publicacionDocenteISSNPublicacion;

            UtilitySecciones.InsertaISSN(listadoISSN, entidadAux, propiedadISSN);
        }

        /// <summary>
        /// 030.080.000.000
        /// </summary>
        private List<Entity> GetParticipacionProyectosInnovacionDocente(List<CvnItemBean> listadoDatos, [Optional] PetitionStatus petitionStatus)
        {
            List<Entity> listado = new List<Entity>();

            List<CvnItemBean> listadoParticipacionProyectosInnovacionDocente = listadoDatos.Where(x => x.Code.Equals("030.080.000.000")).ToList();
            if (listadoParticipacionProyectosInnovacionDocente.Count > 0)
            {
                foreach (CvnItemBean item in listadoParticipacionProyectosInnovacionDocente)
                {
                    //Actualizo el estado de los recursos tratados
                    if (petitionStatus != null)
                    {
                        petitionStatus.actualWork++;
                        petitionStatus.actualWorkSubtitle = "Participación en proyectos de innovación docente";
                    }

                    Entity entidadAux = new Entity();
                    entidadAux.properties = new List<Property>();
                    if (!string.IsNullOrEmpty(item.GetStringPorIDCampo("030.080.000.010")))
                    {
                        entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                            new Property(Variables.Generico.codigoCVN, "030.080.000.000"),
                            new Property(Variables.Generico.personaCVN, mPersonID)
                        ));
                        entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                            new Property(Variables.ActividadDocente.participacionInnovaTitulo, item.GetStringPorIDCampo("030.080.000.010")),
                            new Property(Variables.ActividadDocente.participacionInnovaPaisEntidadRealizacion, item.GetPaisPorIDCampo("030.080.000.020")),
                            new Property(Variables.ActividadDocente.participacionInnovaCCAAEntidadRealizacion, item.GetRegionPorIDCampo("030.080.000.030")),
                            new Property(Variables.ActividadDocente.participacionInnovaCiudadEntidadRealizacion, item.GetStringPorIDCampo("030.080.000.250")),
                            new Property(Variables.ActividadDocente.participacionInnovaTipoParticipacion, item.GetTipoParticipacionProyectoPorIDCampo("030.080.000.050")),
                            new Property(Variables.ActividadDocente.participacionInnovaTipoParticipacionOtros, item.GetStringPorIDCampo("030.080.000.060")),
                            new Property(Variables.ActividadDocente.participacionInnovaAportacionProyecto, item.GetStringPorIDCampo("030.080.000.070")),
                            new Property(Variables.ActividadDocente.participacionInnovaRegimenDedicacion, item.GetRegimenDedicacion("030.080.000.080")),
                            new Property(Variables.ActividadDocente.participacionInnovaTipoConvocatoria, item.GetTipoConvocatoriaPorIDCampo("030.080.000.130")),
                            new Property(Variables.ActividadDocente.participacionInnovaTipoConvocatoriaOtros, item.GetStringPorIDCampo("030.080.000.140")),
                            new Property(Variables.ActividadDocente.participacionInnovaTipoDuracionRelacionLaboral, item.GetTipoDuracionLaboralPorIDCampo("030.080.000.190")),
                            new Property(Variables.ActividadDocente.participacionInnovaDuracionParticipacionAnio, item.GetDurationAnioPorIDCampo("030.080.000.200")),
                            new Property(Variables.ActividadDocente.participacionInnovaDuracionParticipacionMes, item.GetDurationMesPorIDCampo("030.080.000.200")),
                            new Property(Variables.ActividadDocente.participacionInnovaDuracionParticipacionDia, item.GetDurationDiaPorIDCampo("030.080.000.200")),
                            new Property(Variables.ActividadDocente.participacionInnovaFechaFinalizacionParticipacion, item.GetStringDatetimePorIDCampo("030.080.000.210")),
                            new Property(Variables.ActividadDocente.participacionInnovaNumParticipantes, item.GetStringDoublePorIDCampo("030.080.000.230")),
                            new Property(Variables.ActividadDocente.participacionInnovaImporteConcedido, item.GetStringDoublePorIDCampo("030.080.000.240")),
                            new Property(Variables.ActividadDocente.participacionInnovaAmbitoProyecto, item.GetGeographicRegionPorIDCampo("030.080.000.260")),
                            new Property(Variables.ActividadDocente.participacionInnovaAmbitoProyectoOtros, item.GetStringPorIDCampo("030.080.000.270")),
                            new Property(Variables.ActividadDocente.participacionInnovaFechaInicio, item.GetStringDatetimePorIDCampo("030.080.000.280"))
                        ));
                        ParticipacionProyectosInnovacionDocenteEntidadFinanciadora(item, entidadAux);
                        ParticipacionProyectosInnovacionDocenteEntidadParticipante(item, entidadAux);
                        ParticipacionProyectosInnovacionDocenteInvestigadorPrincipal(item, entidadAux);

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
        /// pertenecientes al investigador principal (IP).
        /// </summary>
        /// <param name="item"></param>
        /// <param name="entidadAux"></param>
        private void ParticipacionProyectosInnovacionDocenteInvestigadorPrincipal(CvnItemBean item, Entity entidadAux)
        {
            CvnItemBeanCvnAuthorBean ip = item.GetElementoPorIDCampo<CvnItemBeanCvnAuthorBean>("030.080.000.220");
            if (ip == null) { return; }
            if (string.IsNullOrEmpty(ip.GetNombreAutor())) { return; }
            entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                new Property(Variables.ActividadDocente.participacionInnovaNombreIP, ip.GetNombreAutor()),
                new Property(Variables.ActividadDocente.participacionInnovaFirmaIP, ip.GetFirmaAutor()),
                new Property(Variables.ActividadDocente.participacionInnovaPrimerApellidoIP, ip.GetPrimerApellidoAutor()),
                new Property(Variables.ActividadDocente.participacionInnovaSegundoApellidoIP, ip.GetSegundoApellidoAutor())
            ));
        }

        /// <summary>
        /// Inserta en <paramref name="entidadAux"/> los valores de <paramref name="item"/>,
        /// pertenecientes a la Entidad financiadora.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="entidadAux"></param>
        private void ParticipacionProyectosInnovacionDocenteEntidadFinanciadora(CvnItemBean item, Entity entidadAux)
        {
            //Añado la referencia si existe Entidad
            UtilitySecciones.AniadirEntidadOrganizacion(mResourceApi, item.GetNameEntityBeanPorIDCampo("030.080.000.090"),
                Variables.ActividadDocente.participacionInnovaEntidadFinanciadoraNombre,
                Variables.ActividadDocente.participacionInnovaEntidadFinanciadora, entidadAux);

            //Añado otros, o el ID de una preseleccion
            string valorTipo = !string.IsNullOrEmpty(item.GetStringPorIDCampo("030.080.000.120")) ? mResourceApi.GraphsUrl + "items/organizationtype_OTHERS" : item.GetOrganizacionPorIDCampo("030.080.000.110");

            entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                new Property(Variables.ActividadDocente.participacionInnovaTipoEntidadFinanciadora, valorTipo),
                new Property(Variables.ActividadDocente.participacionInnovaTipoEntidadFinanciadoraOtros, item.GetStringPorIDCampo("030.080.000.120"))
            ));
        }

        /// <summary>
        /// Inserta en <paramref name="entidadAux"/> los valores de <paramref name="item"/>,
        /// pertenecientes a la Entidad participante.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="entidadAux"></param>
        private void ParticipacionProyectosInnovacionDocenteEntidadParticipante(CvnItemBean item, Entity entidadAux)
        {
            List<CvnItemBeanCvnCodeGroup> listadoEntidadParticipante = item.GetListaElementosPorIDCampo<CvnItemBeanCvnCodeGroup>("030.080.000.150");

            string propiedadNombre = Variables.ActividadDocente.participacionInnovaEntidadParticipanteNombre;
            string propiedadEP = Variables.ActividadDocente.participacionInnovaEntidadParticipante;
            string propiedadTipo = Variables.ActividadDocente.participacionInnovaTipoEntidadParticipante;
            string propiedadTipoOtros = Variables.ActividadDocente.participacionInnovaTipoEntidadParticipanteOtros;

            foreach (CvnItemBeanCvnCodeGroup entidad in listadoEntidadParticipante)
            {
                string entityPartAux = Guid.NewGuid().ToString() + "@@@";
                Property propertyNombre = entidadAux.properties.FirstOrDefault(x => x.prop == propiedadNombre);
                Property propertyEP = entidadAux.properties.FirstOrDefault(x => x.prop == propiedadEP);
                Property propertyTipo = entidadAux.properties.FirstOrDefault(x => x.prop == propiedadTipo);
                Property propertyTipoOtros = entidadAux.properties.FirstOrDefault(x => x.prop == propiedadTipoOtros);

                //Añado la referencia si existe Entidad
                string nombreEntidad = entidad.GetNameEntityBeanCvnCodeGroup("030.080.000.150");
                string entidadN = UtilitySecciones.GetOrganizacionPorNombre(mResourceApi, nombreEntidad);

                UtilitySecciones.CheckProperty(propertyNombre, entidadAux, UtilitySecciones.StringGNOSSID(entityPartAux, nombreEntidad), propiedadNombre);
                UtilitySecciones.CheckProperty(propertyEP, entidadAux, UtilitySecciones.StringGNOSSID(entityPartAux, entidadN), propiedadEP);

                //Añado otros, o el ID de una preseleccion
                if (!string.IsNullOrEmpty(entidad.GetStringCvnCodeGroup("030.080.000.180")))
                {
                    string valorTipo = mResourceApi.GraphsUrl + "items/organizationtype_OTHERS";
                    string valorTipoOtros = entidad.GetStringCvnCodeGroup("030.080.000.180");

                    UtilitySecciones.CheckProperty(propertyTipo, entidadAux, UtilitySecciones.StringGNOSSID(entityPartAux, valorTipo), propiedadTipo);
                    UtilitySecciones.CheckProperty(propertyTipoOtros, entidadAux, UtilitySecciones.StringGNOSSID(entityPartAux, valorTipoOtros), propiedadTipoOtros);
                }
                else if (!string.IsNullOrEmpty(entidad.GetStringCvnCodeGroup("030.080.000.170")))
                {
                    string valorTipo = mResourceApi.GraphsUrl + "items/organizationtype_" + entidad.GetStringCvnCodeGroup("030.080.000.170");

                    UtilitySecciones.CheckProperty(propertyTipo, entidadAux, UtilitySecciones.StringGNOSSID(entityPartAux, valorTipo), propiedadTipo);
                }
            }
        }

        /// <summary>
        /// 030.090.000.000
        /// </summary>
        private List<Entity> GetParticipacionCongresosFormacionDocente(List<CvnItemBean> listadoDatos, [Optional] PetitionStatus petitionStatus)
        {
            List<Entity> listado = new List<Entity>();

            List<CvnItemBean> listadoParticipacionCongresosFormacionDocente = listadoDatos.Where(x => x.Code.Equals("030.090.000.000")).ToList();
            if (listadoParticipacionCongresosFormacionDocente.Count > 0)
            {
                foreach (CvnItemBean item in listadoParticipacionCongresosFormacionDocente)
                {
                    //Actualizo el estado de los recursos tratados
                    if (petitionStatus != null)
                    {
                        petitionStatus.actualWork++;
                        petitionStatus.actualWorkSubtitle = "Participación en congresos con ponencias orientadas a la formación docente";
                    }

                    Entity entidadAux = new Entity();
                    entidadAux.properties = new List<Property>();
                    if (!string.IsNullOrEmpty(item.GetStringPorIDCampo("030.090.000.030")))
                    {
                        entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                            new Property(Variables.Generico.codigoCVN, "030.090.000.000"),
                            new Property(Variables.Generico.personaCVN, mPersonID)
                        ));
                        entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                            new Property(Variables.ActividadDocente.participaCongresosTipoEvento, item.GetTipoEventoPorIDCampo("030.090.000.010")),
                            new Property(Variables.ActividadDocente.participaCongresosTipoEventoOtros, item.GetStringPorIDCampo("030.090.000.020")),
                            new Property(Variables.ActividadDocente.participaCongresosNombreEvento, item.GetStringPorIDCampo("030.090.000.030")),
                            new Property(Variables.ActividadDocente.participaCongresosPaisEvento, item.GetPaisPorIDCampo("030.090.000.040")),
                            new Property(Variables.ActividadDocente.participaCongresosCCAAEvento, item.GetRegionPorIDCampo("030.090.000.050")),
                            new Property(Variables.ActividadDocente.participaCongresosCiudadEvento, item.GetStringPorIDCampo("030.090.000.070")),
                            new Property(Variables.ActividadDocente.participaCongresosPaisEntidadOrganizadora, item.GetPaisPorIDCampo("030.090.000.190")),
                            new Property(Variables.ActividadDocente.participaCongresosCCAAEntidadOrganizadora, item.GetRegionPorIDCampo("030.090.000.200")),
                            new Property(Variables.ActividadDocente.participaCongresosCiudadEntidadOrganizadora, item.GetStringPorIDCampo("030.090.000.210")),
                            new Property(Variables.ActividadDocente.participaCongresosObjetivosEvento, item.GetStringPorIDCampo("030.090.000.120")),
                            new Property(Variables.ActividadDocente.participaCongresosPerfilDestinatarios, item.GetStringPorIDCampo("030.090.000.130")),
                            new Property(Variables.ActividadDocente.participaCongresosIdiomaPresentacion, item.GetTraduccion("030.090.000.140")),
                            new Property(Variables.ActividadDocente.participaCongresosFechaPresentacion, item.GetStringDatetimePorIDCampo("030.090.000.150")),
                            new Property(Variables.ActividadDocente.participaCongresosTipoParticipacion, item.GetTipoParticipacionDocumentoPorIDCampo("030.090.000.160")),
                            new Property(Variables.ActividadDocente.participaCongresosTipoParticipacionOtros, item.GetStringPorIDCampo("030.090.000.170")),
                            new Property(Variables.ActividadDocente.participaCongresosTipoPublicacion, item.GetTipoPublicacionPorIDCampo("030.090.000.220")),
                            new Property(Variables.ActividadDocente.participaCongresosTituloPublicacion, item.GetStringPorIDCampo("030.090.000.230")),
                            new Property(Variables.ActividadDocente.participaCongresosNombrePublicacion, item.GetStringPorIDCampo("030.090.000.330")),
                            new Property(Variables.ActividadDocente.participaCongresosVolumenPublicacion, item.GetVolumenPorIDCampo("030.090.000.240")),
                            new Property(Variables.ActividadDocente.participaCongresosNumeroPublicacion, item.GetNumeroVolumenPorIDCampo("030.090.000.240")),
                            new Property(Variables.ActividadDocente.participaCongresosPagIniPublicacion, item.GetPaginaInicialPorIDCampo("030.090.000.250")),
                            new Property(Variables.ActividadDocente.participaCongresosPagFinalPublicacion, item.GetPaginaFinalPorIDCampo("030.090.000.250")),
                            new Property(Variables.ActividadDocente.participaCongresosEditorialPublicacion, item.GetStringPorIDCampo("030.090.000.260")),
                            new Property(Variables.ActividadDocente.participaCongresosPaisPublicacion, item.GetPaisPorIDCampo("030.090.000.270")),
                            new Property(Variables.ActividadDocente.participaCongresosCCAAPublicacion, item.GetRegionPorIDCampo("030.090.000.280")),
                            new Property(Variables.ActividadDocente.participaCongresosFechaPublicacion, item.GetStringDatetimePorIDCampo("030.090.000.300")),
                            new Property(Variables.ActividadDocente.participaCongresosURLPublicacion, item.GetStringPorIDCampo("030.090.000.310")),
                            new Property(Variables.ActividadDocente.participaCongresosDepositoLegalPublicacion, item.GetValueCvnExternalPKBean("030.090.000.320")),
                            new Property(Variables.ActividadDocente.participaCongresosNumHorasPublicacion, item.GetDurationHorasPorIDCampo("030.090.000.340")),
                            new Property(Variables.ActividadDocente.participaCongresosFechaInicioPublicacion, item.GetStringDatetimePorIDCampo("030.090.000.350")),
                            new Property(Variables.ActividadDocente.participaCongresosFechaFinalPublicacion, item.GetStringDatetimePorIDCampo("030.090.000.360")),
                            new Property(Variables.ActividadDocente.participaCongresosAutorCorrespondencia, item.GetStringBooleanPorIDCampo("030.090.000.400"))
                        ));
                        ParticipacionCongresosFormacionDocenteEntidadOrganizadora(item, entidadAux);
                        ParticipacionCongresosFormacionDocenteISBN(item, entidadAux);
                        ParticipacionCongresosFormacionDocenteISSN(item, entidadAux);
                        ParticipacionCongresosFormacionDocenteIDPublicacion(item, entidadAux);

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
        /// pertenecientes a los identificadores de publicación (Handle, DOI, PMID u otros).
        /// </summary>
        /// <param name="item"></param>
        /// <param name="entidadAux"></param>
        private void ParticipacionCongresosFormacionDocenteIDPublicacion(CvnItemBean item, Entity entidadAux)
        {
            List<CvnItemBeanCvnExternalPKBean> listadoIDs = item.GetListaElementosPorIDCampo<CvnItemBeanCvnExternalPKBean>("030.090.000.370");
            string propIdHandle = Variables.ActividadDocente.participaCongresosIDPubDigitalHandle;
            string propIdDOI = Variables.ActividadDocente.participaCongresosIDPubDigitalDOI;
            string propIdPMID = Variables.ActividadDocente.participaCongresosIDPubDigitalPMID;
            string propIdOtroPub = Variables.ActividadDocente.participaCongresosIDOtroPubDigital;
            string nombreOtroPub = Variables.ActividadDocente.participaCongresosNombreOtroIDPubDigital;

            UtilitySecciones.InsertaTiposIDPublicacion(listadoIDs, entidadAux, propIdHandle, propIdDOI, propIdPMID, propIdOtroPub, nombreOtroPub);
        }

        /// <summary>
        /// Inserta en <paramref name="entidadAux"/> los valores de <paramref name="item"/>,
        /// pertenecientes al ISBN.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="entidadAux"></param>
        private void ParticipacionCongresosFormacionDocenteISBN(CvnItemBean item, Entity entidadAux)
        {
            List<CvnItemBeanCvnExternalPKBean> listadoISBN = item.GetListaElementosPorIDCampo<CvnItemBeanCvnExternalPKBean>("030.090.000.180");
            string propiedadISBN = Variables.ActividadDocente.participaCongresosISBNPublicacion;

            UtilitySecciones.InsertaISBN(listadoISBN, entidadAux, propiedadISBN);
        }

        /// <summary>
        /// Inserta en <paramref name="entidadAux"/> los valores de <paramref name="item"/>,
        /// pertenecientes al ISSN.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="entidadAux"></param>
        private void ParticipacionCongresosFormacionDocenteISSN(CvnItemBean item, Entity entidadAux)
        {
            List<CvnItemBeanCvnExternalPKBean> listadoISSN = item.GetListaElementosPorIDCampo<CvnItemBeanCvnExternalPKBean>("030.090.000.180");
            string propiedadISSN = Variables.ActividadDocente.participaCongresosISSNPublicacion;

            UtilitySecciones.InsertaISSN(listadoISSN, entidadAux, propiedadISSN);
        }

        /// <summary>
        /// Inserta en <paramref name="entidadAux"/> los valores de <paramref name="item"/>,
        /// pertenecientes a la Entidad Organizadora.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="entidadAux"></param>
        private void ParticipacionCongresosFormacionDocenteEntidadOrganizadora(CvnItemBean item, Entity entidadAux)
        {
            //Añado la referencia si existe Entidad
            UtilitySecciones.AniadirEntidadOrganizacion(mResourceApi, item.GetNameEntityBeanPorIDCampo("030.090.000.080"),
                Variables.ActividadDocente.participaCongresosEntidadOrganizadoraNombre,
                Variables.ActividadDocente.participaCongresosEntidadOrganizadora, entidadAux);

            //Añado otros, o el ID de una preseleccion
            string valorTipo = !string.IsNullOrEmpty(item.GetStringPorIDCampo("030.090.000.110")) ? mResourceApi.GraphsUrl + "items/organizationtype_OTHERS" : item.GetOrganizacionPorIDCampo("030.090.000.100");

            entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                new Property(Variables.ActividadDocente.participaCongresosTipoEntidadOrganizadora, valorTipo),
                new Property(Variables.ActividadDocente.participaCongresosTipoEntidadOrganizadoraOtros, item.GetStringPorIDCampo("030.090.000.110"))
            ));
        }

        /// <summary>
        /// 060.030.080.000
        /// </summary>
        private List<Entity> GetPremiosInovacionDocente(List<CvnItemBean> listadoDatos, [Optional] PetitionStatus petitionStatus)
        {
            List<Entity> listado = new List<Entity>();

            List<CvnItemBean> listadoPremiosInovacionDocente = listadoDatos.Where(x => x.Code.Equals("060.030.080.000")).ToList();
            if (listadoPremiosInovacionDocente.Count > 0)
            {
                foreach (CvnItemBean item in listadoPremiosInovacionDocente)
                {
                    //Actualizo el estado de los recursos tratados
                    if (petitionStatus != null)
                    {
                        petitionStatus.actualWork++;
                        petitionStatus.actualWorkSubtitle = "Premios de innovación docente recibidos";
                    }

                    Entity entidadAux = new Entity();
                    entidadAux.properties = new List<Property>();
                    if (!string.IsNullOrEmpty(item.GetStringPorIDCampo("060.030.080.010")))
                    {
                        entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                            new Property(Variables.Generico.codigoCVN, "060.030.080.000"),
                            new Property(Variables.Generico.personaCVN, mPersonID)
                        ));
                        entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                            new Property(Variables.ActividadDocente.premiosInnovaNombre, item.GetStringPorIDCampo("060.030.080.010")),
                            new Property(Variables.ActividadDocente.premiosInnovaPaisEntidadConcesionaria, item.GetPaisPorIDCampo("060.030.080.020")),
                            new Property(Variables.ActividadDocente.premiosInnovaCCAAEntidadConcesionaria, item.GetRegionPorIDCampo("060.030.080.030")),
                            new Property(Variables.ActividadDocente.premiosInnovaCiudadEntidadConcesionaria, item.GetStringPorIDCampo("060.030.080.110")),
                            new Property(Variables.ActividadDocente.premiosInnovaPropuestaDe, item.GetStringPorIDCampo("060.030.080.090")),
                            new Property(Variables.ActividadDocente.premiosInnovaFechaConcesion, item.GetStringDatetimePorIDCampo("060.030.080.100"))
                        ));
                        PremiosInnovacionDocenteEntidadConcesionaria(item, entidadAux);

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
        /// pertenecientes a la Entidad concesionaria.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="entidadAux"></param>
        private void PremiosInnovacionDocenteEntidadConcesionaria(CvnItemBean item, Entity entidadAux)
        {
            //Añado la referencia si existe Entidad
            UtilitySecciones.AniadirEntidadOrganizacion(mResourceApi, item.GetNameEntityBeanPorIDCampo("060.030.080.050"),
                Variables.ActividadDocente.premiosInnovaEntidadConcesionariaNombre,
                Variables.ActividadDocente.premiosInnovaEntidadConcesionaria, entidadAux);

            //Añado otros, o el ID de una preseleccion
            string valorTipo = !string.IsNullOrEmpty(item.GetStringPorIDCampo("060.030.080.080")) ? mResourceApi.GraphsUrl + "items/organizationtype_OTHERS" : item.GetOrganizacionPorIDCampo("060.030.080.070");

            entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                new Property(Variables.ActividadDocente.premiosInnovaTipoEntidadConcesionaria, valorTipo),
                new Property(Variables.ActividadDocente.premiosInnovaTipoEntidadConcesionariaOtros, item.GetStringPorIDCampo("060.030.080.080"))
            ));
        }

        /// <summary>
        /// 030.100.000.000
        /// </summary>
        private List<Entity> GetOtrasActividades(List<CvnItemBean> listadoDatos, [Optional] PetitionStatus petitionStatus)
        {
            List<Entity> listado = new List<Entity>();

            List<CvnItemBean> listadoOtrasActividades = listadoDatos.Where(x => x.Code.Equals("030.100.000.000")).ToList();
            if (listadoOtrasActividades.Count > 0)
            {
                foreach (CvnItemBean item in listadoOtrasActividades)
                {
                    //Actualizo el estado de los recursos tratados
                    if (petitionStatus != null)
                    {
                        petitionStatus.actualWork++;
                        petitionStatus.actualWorkSubtitle = "Otras actividades/méritos no incluidos en la relación anterior";
                    }

                    Entity entidadAux = new Entity();
                    entidadAux.properties = new List<Property>();
                    if (!string.IsNullOrEmpty(item.GetStringPorIDCampo("030.100.000.010")))
                    {
                        entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                            new Property(Variables.Generico.codigoCVN, "030.100.000.000"),
                            new Property(Variables.Generico.personaCVN, mPersonID)
                        ));
                        entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                            new Property(Variables.ActividadDocente.otrasActividadesDescripcion, item.GetStringPorIDCampo("030.100.000.010")),
                            new Property(Variables.ActividadDocente.otrasActividadesPaisRealizacion, item.GetPaisPorIDCampo("030.100.000.030")),
                            new Property(Variables.ActividadDocente.otrasActividadesCCAARealizacion, item.GetRegionPorIDCampo("030.100.000.040")),
                            new Property(Variables.ActividadDocente.otrasActividadesCiudadRealizacion, item.GetStringPorIDCampo("030.100.000.060")),
                            new Property(Variables.ActividadDocente.otrasActividadesFechaFinalizacion, item.GetStringDatetimePorIDCampo("030.100.000.110"))
                        ));
                        OtrasActividadesPalabrasClave(item, entidadAux);
                        OtrasActividadesEntidadOrganizadora(item, entidadAux);

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
        /// pertenecientes a las Palabras clave.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="entidadAux"></param>
        private void OtrasActividadesPalabrasClave(CvnItemBean item, Entity entidadAux)
        {
            List<CvnItemBeanCvnString> listadoPalabrasClave = item.GetListaElementosPorIDCampo<CvnItemBeanCvnString>("030.100.000.020");

            string propiedadPalabrasClave = Variables.ActividadDocente.otrasActividadesPalabrasClave;

            foreach (CvnItemBeanCvnString palabraClave in listadoPalabrasClave)
            {
                string entityPartAux = Guid.NewGuid().ToString() + "@@@";

                List<string> listadoPalabras = Utility.GetPadresPalabrasClave(palabraClave);
                foreach (string palabra in listadoPalabras)
                {
                    Property propertyPalabrasClave = entidadAux.properties.FirstOrDefault(x => x.prop == Variables.ActividadDocente.otrasActividadesPalabrasClave);
                    UtilitySecciones.CheckProperty(propertyPalabrasClave, entidadAux,
                        UtilitySecciones.StringGNOSSID(entityPartAux, Utility.ObtenerPalabraClave(mResourceApi, palabra)), propiedadPalabrasClave);
                }
            }
        }

        /// <summary>
        /// Inserta en <paramref name="entidadAux"/> los valores de <paramref name="item"/>,
        /// pertenecientes al tipo de Entidad Organizadora.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="entidadAux"></param>
        private void OtrasActividadesEntidadOrganizadora(CvnItemBean item, Entity entidadAux)
        {
            //Añado la referencia si existe Entidad
            UtilitySecciones.AniadirEntidadOrganizacion(mResourceApi, item.GetNameEntityBeanPorIDCampo("030.100.000.070"),
                Variables.ActividadDocente.otrasActividadesEntidadOrganizadoraNombre,
                Variables.ActividadDocente.otrasActividadesEntidadOrganizadora, entidadAux);

            //Añado otros, o el ID de una preseleccion
            string valorTipo = !string.IsNullOrEmpty(item.GetStringPorIDCampo("030.100.000.100")) ? mResourceApi.GraphsUrl + "items/organizationtype_OTHERS" : item.GetOrganizacionPorIDCampo("030.100.000.090");

            entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                new Property(Variables.ActividadDocente.otrasActividadesTipoEntidadOrganizadora, valorTipo),
                new Property(Variables.ActividadDocente.otrasActividadesTipoEntidadOrganizadoraOtros, item.GetStringPorIDCampo("030.100.000.100"))
            ));
        }

        /// <summary>
        /// 030.110.000.000
        /// </summary>
        private List<Entity> GetAportacionesRelevantes(List<CvnItemBean> listadoDatos, [Optional] PetitionStatus petitionStatus)
        {
            List<Entity> listado = new List<Entity>();

            List<CvnItemBean> listadoAportacionesRelevantes = listadoDatos.Where(x => x.Code.Equals("030.110.000.000")).ToList();
            if (listadoAportacionesRelevantes.Count > 0)
            {
                foreach (CvnItemBean item in listadoAportacionesRelevantes)
                {
                    //Actualizo el estado de los recursos tratados
                    if (petitionStatus != null)
                    {
                        petitionStatus.actualWork++;
                        petitionStatus.actualWorkSubtitle = "Aportaciones más relevantes de su CV de docencia";
                    }

                    Entity entidadAux = new Entity();
                    entidadAux.properties = new List<Property>();
                    if (!string.IsNullOrEmpty(item.GetStringPorIDCampo("030.110.000.010")))
                    {
                        entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                               new Property(Variables.Generico.codigoCVN, "030.110.000.000"),
                               new Property(Variables.Generico.personaCVN, mPersonID)
                           ));
                        entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                            new Property(Variables.ActividadDocente.aportacionesCVDescripcion, item.GetStringPorIDCampo("030.110.000.010")),
                            new Property(Variables.ActividadDocente.aportacionesCVPaisRealizacion, item.GetPaisPorIDCampo("030.110.000.030")),
                            new Property(Variables.ActividadDocente.aportacionesCVCCAARealizacion, item.GetRegionPorIDCampo("030.110.000.040")),
                            new Property(Variables.ActividadDocente.aportacionesCVCiudadRealizacion, item.GetStringPorIDCampo("030.110.000.060")),
                            new Property(Variables.ActividadDocente.aportacionesCVFechaFinalizacion, item.GetStringDatetimePorIDCampo("030.110.000.110"))
                        ));
                        AportacionesRelevantesPalabrasClave(item, entidadAux);
                        AportacionesRelevantesEntidadOrganizadora(item, entidadAux);

                        listado.Add(entidadAux);
                    }
                    else
                    {
                        listadoDatos.Remove(item);
                    }
                }
            }
            return listado;
        }

        /// <summary>
        /// Inserta en <paramref name="entidadAux"/> los valores de <paramref name="item"/>,
        /// pertenecientes al tipo de Entidad Organizadora.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="entidadAux"></param>
        private void AportacionesRelevantesEntidadOrganizadora(CvnItemBean item, Entity entidadAux)
        {
            //Añado la referencia si existe Entidad
            UtilitySecciones.AniadirEntidadOrganizacion(mResourceApi, item.GetNameEntityBeanPorIDCampo("030.110.000.070"),
                Variables.ActividadDocente.aportacionesCVEntidadOrganizadoraNombre,
                Variables.ActividadDocente.aportacionesCVEntidadOrganizadora, entidadAux);

            //Añado otros, o el ID de una preseleccion
            string valorTipo = !string.IsNullOrEmpty(item.GetStringPorIDCampo("030.110.000.100")) ? mResourceApi.GraphsUrl + "items/organizationtype_OTHERS" : item.GetOrganizacionPorIDCampo("030.110.000.090");

            entidadAux.properties.AddRange(UtilitySecciones.AddProperty(
                new Property(Variables.ActividadDocente.aportacionesCVTipoEntidadOrganizadora, valorTipo),
                new Property(Variables.ActividadDocente.aportacionesCVTipoEntidadOrganizadoraOtros, item.GetStringPorIDCampo("030.110.000.100"))
            ));
        }

        /// <summary>
        /// Inserta en <paramref name="entidadAux"/> los valores de <paramref name="item"/>,
        /// pertenecientes a las Palabras clave.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="entidadAux"></param>
        private void AportacionesRelevantesPalabrasClave(CvnItemBean item, Entity entidadAux)
        {
            List<CvnItemBeanCvnString> listadoPalabrasClave = item.GetListaElementosPorIDCampo<CvnItemBeanCvnString>("030.110.000.020");

            string propiedadPalabrasClave = Variables.ActividadDocente.aportacionesCVPalabrasClave;

            foreach (CvnItemBeanCvnString palabraClave in listadoPalabrasClave)
            {
                string entityPartAux = Guid.NewGuid().ToString() + "@@@";

                List<string> listadoPalabras = Utility.GetPadresPalabrasClave(palabraClave);
                foreach (string palabra in listadoPalabras)
                {
                    Property propertyPalabrasClave = entidadAux.properties.FirstOrDefault(x => x.prop == Variables.ActividadDocente.aportacionesCVPalabrasClave);
                    UtilitySecciones.CheckProperty(propertyPalabrasClave, entidadAux,
                        UtilitySecciones.StringGNOSSID(entityPartAux, Utility.ObtenerPalabraClave(mResourceApi, palabra)), propiedadPalabrasClave);
                }
            }
        }
    }
}
