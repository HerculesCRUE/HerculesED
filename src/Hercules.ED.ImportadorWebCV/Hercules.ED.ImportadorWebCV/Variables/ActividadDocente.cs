namespace ImportadorWebCV.Variables
{
    class ActividadDocente
    {
        /// <summary>
        /// Dirección de tesis doctorales y/o proyectos fin de carrera - 030.040.000.000
        /// </summary>
        public const string direccionTesisTipoProyecto = "http://w3id.org/roh/projectCharacterType";
        public const string direccionTesisTipoProyectoOtros = "http://w3id.org/roh/projectCharacterTypeOther";
        public const string direccionTesisTituloTrabajo = "http://w3id.org/roh/title";
        public const string direccionTesisCodirectorTesisOrden = "http://w3id.org/roh/codirector@@@http://xmlns.com/foaf/0.1/Person|http://www.w3.org/1999/02/22-rdf-syntax-ns#comment";
        public const string direccionTesisCodirectorTesisFirma = "http://w3id.org/roh/codirector@@@http://xmlns.com/foaf/0.1/Person|http://xmlns.com/foaf/0.1/nick";
        public const string direccionTesisCodirectorTesisNombre = "http://w3id.org/roh/codirector@@@http://xmlns.com/foaf/0.1/Person|http://xmlns.com/foaf/0.1/firstName";
        public const string direccionTesisCodirectorTesisPrimerApellido = "http://w3id.org/roh/codirector@@@http://xmlns.com/foaf/0.1/Person|http://xmlns.com/foaf/0.1/familyName";
        public const string direccionTesisCodirectorTesisSegundoApellido = "http://w3id.org/roh/codirector@@@http://xmlns.com/foaf/0.1/Person|http://w3id.org/roh/secondFamilyName";
        public const string direccionTesisPaisEntidadRealizacion = "https://www.w3.org/2006/vcard/ns#hasCountryName";
        public const string direccionTesisCCAAEntidadRealizacion = "https://www.w3.org/2006/vcard/ns#hasRegion";
        public const string direccionTesisCiudadEntidadRealizacion = "https://www.w3.org/2006/vcard/ns#locality";
        public const string direccionTesisEntidadRealizacion = "http://w3id.org/roh/promotedBy";
        public const string direccionTesisEntidadRealizacionNombre = "http://w3id.org/roh/promotedByTitle";
        public const string direccionTesisTipoEntidadRealizacion = "http://w3id.org/roh/promotedByType";
        public const string direccionTesisTipoEntidadRealizacionOtros = "http://w3id.org/roh/promotedByTypeOther";
        public const string direccionTesisAlumnoFirma = "http://w3id.org/roh/studentNick";
        public const string direccionTesisAlumnoNombre = "http://w3id.org/roh/studentName";
        public const string direccionTesisAlumnoPrimerApellido = "http://w3id.org/roh/studentFirstSurname";
        public const string direccionTesisAlumnoSegundoApellido = "http://w3id.org/roh/studentSecondSurname";
        public const string direccionTesisPalabrasClave = "http://vivoweb.org/ontology/core#freeTextKeyword@@@http://w3id.org/roh/CategoryPath|http://w3id.org/roh/categoryNode";
        public const string direccionTesisFechaDefensa = "http://purl.org/dc/terms/issued";
        public const string direccionTesisCalificacionObtenida = "http://w3id.org/roh/qualification";
        public const string direccionTesisFechaMencionDoctUE = "http://w3id.org/roh/europeanDoctorateDate";
        public const string direccionTesisMencionCalidad = "http://w3id.org/roh/qualityMention";
        public const string direccionTesisDoctoradoUE = "http://w3id.org/roh/europeanDoctorate";
        public const string direccionTesisFechaMencionCalidad = "http://w3id.org/roh/qualityMentionDate";

        /// <summary>
        /// Formación académica impartida - 030.010.000.000
        /// </summary>
        public const string formacionAcademicaTipoDocenciaOficialidad = "http://w3id.org/roh/teachingType";
        public const string formacionAcademicaTitulacionUniversitaria = "http://w3id.org/roh/degreeType";
        public const string formacionAcademicaTitulacionUniversitariaNombre = "http://w3id.org/roh/title";
        public const string formacionAcademicaPaisEntidadRealizacion = "https://www.w3.org/2006/vcard/ns#hasCountryName";
        public const string formacionAcademicaCCAAEntidadRealizacion = "https://www.w3.org/2006/vcard/ns#hasRegion";
        public const string formacionAcademicaCiudadEntidadRealizacion = "https://www.w3.org/2006/vcard/ns#locality";
        public const string formacionAcademicaEntidadRealizacion = "http://w3id.org/roh/promotedBy";
        public const string formacionAcademicaEntidadRealizacionNombre = "http://w3id.org/roh/promotedByTitle";
        public const string formacionAcademicaTipoEntidadRealizacion = "http://w3id.org/roh/promotedByType";
        public const string formacionAcademicaTipoEntidadRealizacionOtros = "http://w3id.org/roh/promotedByTypeOther";
        public const string formacionAcademicaDepartamento = "http://w3id.org/roh/department";
        public const string formacionAcademicaTipoPrograma = "http://w3id.org/roh/programType";
        public const string formacionAcademicaTipoProgramaOtros = "http://w3id.org/roh/programTypeOther";
        public const string formacionAcademicaNombreAsignatura = "http://w3id.org/roh/teaches";
        public const string formacionAcademicaTipoDocenciaModalidad = "http://w3id.org/roh/modalityTeachingType";
        public const string formacionAcademicaTipoDocenciaModalidadOtros = "http://w3id.org/roh/modalityTeachingTypeOther";
        public const string formacionAcademicaTipoAsignatura = "http://w3id.org/roh/courseType";
        public const string formacionAcademicaTipoAsignaturaOtros = "http://w3id.org/roh/courseTypeOther";
        public const string formacionAcademicaCursoTitulacion = "http://w3id.org/roh/course";
        public const string formacionAcademicaTipoECTS = "http://w3id.org/roh/hoursCreditsECTSType";
        public const string formacionAcademicaNumeroECTS = "http://w3id.org/roh/numberECTSHours";
        public const string formacionAcademicaIdiomaAsignatura = "https://www.w3.org/2006/vcard/ns#hasLanguage";
        public const string formacionAcademicaFrecuenciaAsignatura = "http://w3id.org/roh/frequency";
        public const string formacionAcademicaCompetenciasRelacionadas = "http://w3id.org/roh/competencies";
        public const string formacionAcademicaCategoriaProfesional = "http://w3id.org/roh/professionalCategory";
        public const string formacionAcademicaCalificacionObtenida = "http://w3id.org/roh/qualification";
        public const string formacionAcademicaCalificacionMax = "http://w3id.org/roh/maxQualification";
        public const string formacionAcademicaPaisEntidadEvaluacion = "http://w3id.org/roh/evaluatedByHasCountryName";
        public const string formacionAcademicaCCAAEntidadEvaluacion = "http://w3id.org/roh/evaluatedByHasRegion";
        public const string formacionAcademicaCiudadEntidadEvaluacion = "http://w3id.org/roh/evaluatedByLocality";
        public const string formacionAcademicaEntidadEvaluacion = "http://w3id.org/roh/evaluatedBy";
        public const string formacionAcademicaEntidadEvaluacionNombre = "http://w3id.org/roh/evaluatedByTitle";
        public const string formacionAcademicaTipoEntidadEvaluacion = "http://w3id.org/roh/evaluatedByType";
        public const string formacionAcademicaTipoEntidadEvaluacionOtros = "http://w3id.org/roh/evaluatedByTypeOther";
        public const string formacionAcademicaTipoEvaluacion = "http://w3id.org/roh/evaluationType";
        public const string formacionAcademicaTipoEvaluacionOtros = "http://w3id.org/roh/evaluationTypeOther";
        public const string formacionAcademicaPaisEntidadFinanciadora = "http://w3id.org/roh/financedByHasCountryName";
        public const string formacionAcademicaCCAAEntidadFinanciadora = "http://w3id.org/roh/financedByHasRegion";
        public const string formacionAcademicaCiudadEntidadFinanciadora = "http://w3id.org/roh/financedByLocality";
        public const string formacionAcademicaEntidadFinanciadora = "http://w3id.org/roh/financedBy";
        public const string formacionAcademicaEntidadFinanciadoraNombre = "http://w3id.org/roh/financedByTitle";
        public const string formacionAcademicaTipoEntidadFinanciadora = "http://w3id.org/roh/financedByType";
        public const string formacionAcademicaTipoEntidadFinanciadoraOtros = "http://w3id.org/roh/financedByTypeOther";
        public const string formacionAcademicaEntFinanTipoConvocatoria = "http://w3id.org/roh/callType";
        public const string formacionAcademicaEntFinanTipoConvocatoriaOtros = "http://w3id.org/roh/callTypeOther";
        public const string formacionAcademicaEntFinanAmbitoGeo = "http://vivoweb.org/ontology/core#geographicFocus";
        public const string formacionAcademicaEntFinanAmbitoGeoOtros = "http://w3id.org/roh/geographicFocusOther";
        public const string formacionAcademicaFacultadEscuela = "http://w3id.org/roh/center";
        public const string formacionAcademicaFechaInicio = "http://vivoweb.org/ontology/core#start";
        public const string formacionAcademicaFechaFinalizacion = "http://vivoweb.org/ontology/core#end";
      
        /// <summary>
        /// Tutorias académicas de estudiantes - 030.050.000.000
        /// </summary>
        public const string tutoAcademicaNombrePrograma = "http://w3id.org/roh/tutorshipProgramType";
        public const string tutoAcademicaNombreProgramaOtros = "http://w3id.org/roh/tutorshipProgramTypeOther";
        public const string tutoAcademicaPaisEntidadRealizacion = "https://www.w3.org/2006/vcard/ns#hasCountryName";
        public const string tutoAcademicaCCAAEntidadRealizacion = "https://www.w3.org/2006/vcard/ns#hasRegion";
        public const string tutoAcademicaCiudadEntidadRealizacion = "https://www.w3.org/2006/vcard/ns#locality";
        public const string tutoAcademicaEntidadRealizacion = "http://w3id.org/roh/conductedBy";
        public const string tutoAcademicaEntidadRealizacionNombre = "http://w3id.org/roh/conductedByTitle";
        public const string tutoAcademicaTipoEntidadRealizacion = "http://w3id.org/roh/conductedByType";
        public const string tutoAcademicaTipoEntidadRealizacionOtros = "http://w3id.org/roh/conductedByTypeOther";
        public const string tutoAcademicaNumAlumnosTutelados = "http://w3id.org/roh/numberSupervisedStudents";
        public const string tutoAcademicaFrecuenciaActividad = "http://w3id.org/roh/frequency";
        public const string tutoAcademicaNumHorasECTS = "http://w3id.org/roh/numberECTSHoursRecognized";

        /// <summary>
        /// Cursos y seminarios impartidos orientados a la formación docente universitaria - 030.060.000.000
        /// </summary>
        public const string cursosSeminariosTipoEvento = "http://w3id.org/roh/eventType";
        public const string cursosSeminariosTipoEventoOtros = "http://w3id.org/roh/eventTypeOther";
        public const string cursosSeminariosNombreEvento = "http://w3id.org/roh/title";
        public const string cursosSeminariosPaisEntidadOrganizadora = "https://www.w3.org/2006/vcard/ns#hasCountryName";
        public const string cursosSeminariosCCAAEntidadOrganizadora = "https://www.w3.org/2006/vcard/ns#hasRegion";
        public const string cursosSeminariosCiudadEntidadOrganizadora = "https://www.w3.org/2006/vcard/ns#locality";
        public const string cursosSeminariosEntidadOrganizadoraNombre = "http://w3id.org/roh/promotedByTitle";
        public const string cursosSeminariosEntidadOrganizadora = "http://w3id.org/roh/promotedBy";
        public const string cursosSeminariosTipoEntidadOrganizadora = "http://w3id.org/roh/promotedByType";
        public const string cursosSeminariosTipoEntidadOrganizadoraOtros = "http://w3id.org/roh/promotedByTypeOther";
        public const string cursosSeminariosObjetivosCurso = "http://w3id.org/roh/goals";
        public const string cursosSeminariosPerfilDestinatarios = "http://w3id.org/roh/targetProfile";
        public const string cursosSeminariosIdiomaImpartio = "https://www.w3.org/2006/vcard/ns#hasLanguage";
        public const string cursosSeminariosFechaImparticion = "http://vivoweb.org/ontology/core#start";
        public const string cursosSeminariosHorasImpartidas = "http://w3id.org/roh/durationHours";
        public const string cursosSeminariosTipoParticipacion = "http://w3id.org/roh/participationType";
        public const string cursosSeminariosTipoParticipacionOtros = "http://w3id.org/roh/participationTypeOther";
        public const string cursosSeminariosISBN = "http://w3id.org/roh/isbn";
        public const string cursosSeminariosISSN = "http://purl.org/ontology/bibo/issn";
        public const string cursosSeminariosAutorCorrespondencia = "http://w3id.org/roh/correspondingAuthor";
        public const string cursosSeminariosIDPubDigitalDOI = "http://purl.org/ontology/bibo/doi";
        public const string cursosSeminariosIDPubDigitalHandle = "http://purl.org/ontology/bibo/handle";
        public const string cursosSeminariosIDPubDigitalPMID = "http://purl.org/ontology/bibo/pmid";
        public const string cursosSeminariosNombreOtroIDPubDigital = "http://purl.org/ontology/bibo/identifier@@@http://xmlns.com/foaf/0.1/Document|http://xmlns.com/foaf/0.1/topic";
        public const string cursosSeminariosIDOtroPubDigital = "http://purl.org/ontology/bibo/identifier@@@http://xmlns.com/foaf/0.1/Document|http://purl.org/dc/elements/1.1/title";


        /// <summary>
        /// Publicaciones docentes o de carácter pedagógico, libros, articulos, etc. - 030.070.000.000
        /// </summary>
        public const string publicacionDocenteNombre = "http://w3id.org/roh/title";
        public const string publicacionDocentePerfilDestinatario = "http://w3id.org/roh/targetProfile";
        public const string publicacionDocenteAutorFirma = "http://purl.org/ontology/bibo/authorList@@@http://xmlns.com/foaf/0.1/Person|http://xmlns.com/foaf/0.1/nick";
        public const string publicacionDocenteAutorNombre = "http://purl.org/ontology/bibo/authorList@@@http://xmlns.com/foaf/0.1/Person|http://xmlns.com/foaf/0.1/firstName";
        public const string publicacionDocenteAutorOrden = "http://purl.org/ontology/bibo/authorList@@@http://xmlns.com/foaf/0.1/Person|http://www.w3.org/1999/02/22-rdf-syntax-ns#comment";
        public const string publicacionDocenteAutorPrimerApellido = "http://purl.org/ontology/bibo/authorList@@@http://xmlns.com/foaf/0.1/Person|http://xmlns.com/foaf/0.1/familyName";
        public const string publicacionDocenteAutorSegundoApellido = "http://purl.org/ontology/bibo/authorList@@@http://xmlns.com/foaf/0.1/Person|http://w3id.org/roh/secondFamilyName";
        public const string publicacionDocentePosicionFirma = "http://w3id.org/roh/signaturePosition";
        public const string publicacionDocenteFechaElaboracion = "http://purl.org/dc/terms/issued";
        public const string publicacionDocenteTipologiaSoporte = "http://w3id.org/roh/supportType";
        public const string publicacionDocenteTipologiaSoporteOtros = "http://w3id.org/roh/supportTypeOther";
        public const string publicacionDocenteTituloPublicacion = "http://w3id.org/roh/publicationTitle";
        public const string publicacionDocenteNombrePublicacion = "http://w3id.org/roh/publicationName";
        public const string publicacionDocenteVolumenPublicacion = "http://purl.org/ontology/bibo/volume";
        public const string publicacionDocenteNumeroPublicacion = "http://purl.org/ontology/bibo/issue";
        public const string publicacionDocentePagIniPublicacion = "http://purl.org/ontology/bibo/pageStart";
        public const string publicacionDocentePagFinalPublicacion = "http://purl.org/ontology/bibo/pageEnd";
        public const string publicacionDocenteEditorialPublicacion = "http://vivoweb.org/ontology/core#publisher";
        public const string publicacionDocentePaisPublicacion = "https://www.w3.org/2006/vcard/ns#hasCountryName";
        public const string publicacionDocenteCCAAPublicacion = "https://www.w3.org/2006/vcard/ns#hasRegion";
        public const string publicacionDocenteFechaPublicacion = "http://w3id.org/roh/publishDate";
        public const string publicacionDocenteURLPublicacion = "https://www.w3.org/2006/vcard/ns#url";
        public const string publicacionDocenteISBNPublicacion = "http://w3id.org/roh/isbn";
        public const string publicacionDocenteISSNPublicacion = "http://purl.org/ontology/bibo/issn";
        public const string publicacionDocenteDepositoLegal = "http://w3id.org/roh/legalDeposit";
        public const string publicacionDocenteJustificacionMaterial = "http://w3id.org/roh/materialJustification";
        public const string publicacionDocenteGradoContribucion = "http://w3id.org/roh/contributionGrade";
        public const string publicacionDocenteAutorCorrespondencia = "http://w3id.org/roh/correspondingAuthor";
        public const string publicacionDocenteIDPubDigitalDOI = "http://purl.org/ontology/bibo/doi";
        public const string publicacionDocenteIDPubDigitalHandle = "http://purl.org/ontology/bibo/handle";
        public const string publicacionDocenteIDPubDigitalPMID = "http://purl.org/ontology/bibo/pmid";
        public const string publicacionDocenteNombreOtroIDPubDigital = "http://purl.org/ontology/bibo/identifier@@@http://xmlns.com/foaf/0.1/Document|http://xmlns.com/foaf/0.1/topic";
        public const string publicacionDocenteIDOtroPubDigital = "http://purl.org/ontology/bibo/identifier@@@http://xmlns.com/foaf/0.1/Document|http://purl.org/dc/elements/1.1/title";


        /// <summary>
        /// Participación en proyectos de innovacion docente - 030.080.000.000
        /// </summary>
        public const string participacionInnovaTitulo = "";
        public const string participacionInnovaPaisEntidadRealizacion = "";
        public const string participacionInnovaCCAAEntidadRealizacion = "";
        public const string participacionInnovaCiudadEntidadRealizacion = "";
        public const string participacionInnovaTipoParticipacion = "";
        public const string participacionInnovaTipoParticipacionOtros = "";
        public const string participacionInnovaAportacionProyecto = "";
        public const string participacionInnovaRegimenDedicacion = "";
        public const string participacionInnovaEntidadFinanciadora = "";
        public const string participacionInnovaEntidadFinanciadoraNombre = "";
        public const string participacionInnovaTipoEntidadFinanciadora = "";
        public const string participacionInnovaTipoEntidadFinanciadoraOtros = "";
        public const string participacionInnovaTipoConvocatoria = "";
        public const string participacionInnovaTipoConvocatoriaOtros = "";
        public const string participacionInnovaEntidadParticipante = "";
        public const string participacionInnovaEntidadParticipanteNombre = "";
        public const string participacionInnovaTipoEntidadParticipante = "";
        public const string participacionInnovaTipoEntidadParticipanteOtros = "";
        public const string participacionInnovaTipoDuracionRelacionLaboral = "";
        public const string participacionInnovaDuracionParticipacionAnio = "";
        public const string participacionInnovaDuracionParticipacionMes = "";
        public const string participacionInnovaDuracionParticipacionDia = "";
        public const string participacionInnovaFechaFinalizacionParticipacion = "";
        public const string participacionInnovaFirmaIP = "";
        public const string participacionInnovaNombreIP = "";
        public const string participacionInnovaPrimerApellidoIP = "";
        public const string participacionInnovaSegundoApellidoIP = "";
        public const string participacionInnovaNumParticipantes = "";
        public const string participacionInnovaImporteConcedido = "";
        public const string participacionInnovaAmbitoProyecto = "";
        public const string participacionInnovaAmbitoProyectoOtros = "";
        public const string participacionInnovaFechaInicio = "";

        /// <summary>
        /// Participación en congresos con ponencias orientadas a la formación docente - 030.090.000.000
        /// </summary>
        public const string participaCongresosTipoEvento = "";
        public const string participaCongresosTipoEventoOtros = "";
        public const string participaCongresosNombreEvento = "";
        public const string participaCongresosPaisEvento = "";
        public const string participaCongresosCCAAEvento = "";
        public const string participaCongresosCiudadEvento = "";
        public const string participaCongresosEntidadOrganizadora = "";
        public const string participaCongresosEntidadOrganizadoraNombre = "";
        public const string participaCongresosTipoEntidadOrganizadora = "";
        public const string participaCongresosTipoEntidadOrganizadoraOtros = "";
        public const string participaCongresosPaisEntidadOrganizadora = "";
        public const string participaCongresosCCAAEntidadOrganizadora = "";
        public const string participaCongresosCiudadEntidadOrganizadora = "";
        public const string participaCongresosObjetivosEvento = "";
        public const string participaCongresosPerfilDestinatarios = "";
        public const string participaCongresosIdiomaPresentacion = "";
        public const string participaCongresosFechaPresentacion = "";
        public const string participaCongresosTipoParticipacion = "";
        public const string participaCongresosTipoParticipacionOtros = "";
        public const string participaCongresosTipoPublicacion = "";
        public const string participaCongresosTituloPublicacion = "";
        public const string participaCongresosNombrePublicacion = "";
        public const string participaCongresosVolumenPublicacion = "";
        public const string participaCongresosNumeroPublicacion = "";
        public const string participaCongresosPagIniPublicacion = "";
        public const string participaCongresosPagFinalPublicacion = "";
        public const string participaCongresosEditorialPublicacion = "";
        public const string participaCongresosPaisPublicacion = "";
        public const string participaCongresosCCAAPublicacion = "";
        public const string participaCongresosFechaPublicacion = "";
        public const string participaCongresosURLPublicacion = "";
        public const string participaCongresosISBNPublicacion = "";
        public const string participaCongresosDepositoLegalPublicacion = "";
        public const string participaCongresosNumHorasPublicacion = "";
        public const string participaCongresosFechaInicioPublicacion = "";
        public const string participaCongresosFechaFinalPublicacion = "";
        public const string participaCongresosAutorCorrespondencia = "";
        public const string participaCongresosIDPubDigitalDOI = "";
        public const string participaCongresosIDPubDigitalHandle = "";
        public const string participaCongresosIDPubDigitalPMID = "";
        public const string participaCongresosNombreOtroIDPubDigital = "";
        public const string participaCongresosIDOtroPubDigital = "";


        /// <summary>
        /// Premios de innovación docente recibidos - 060.030.080.000
        /// </summary>
        public const string premiosInnovaNombre = "http://w3id.org/roh/title";
        public const string premiosInnovaPaisEntidadConcesionaria = "https://www.w3.org/2006/vcard/ns#hasCountryName";
        public const string premiosInnovaCCAAEntidadConcesionaria = "https://www.w3.org/2006/vcard/ns#hasRegion";
        public const string premiosInnovaCiudadEntidadConcesionaria = "https://www.w3.org/2006/vcard/ns#locality";
        public const string premiosInnovaEntidadConcesionaria = "http://w3id.org/roh/accreditationIssuedBy";
        public const string premiosInnovaEntidadConcesionariaNombre = "http://w3id.org/roh/accreditationIssuedByTitle";
        public const string premiosInnovaTipoEntidadConcesionaria = "http://w3id.org/roh/organizationType";
        public const string premiosInnovaTipoEntidadConcesionariaOtros = "http://w3id.org/roh/organizationTypeOther";
        public const string premiosInnovaPropuestaDe = "http://w3id.org/roh/proposedBy";
        public const string premiosInnovaFechaConcesion = "http://w3id.org/roh/receptionDate";


        /// <summary>
        /// Otras actividades/méritos no incluidos en la relación anterior - 030.100.000.000
        /// </summary>
        public const string otrasActividadesDescripcion = "http://w3id.org/roh/title";
        public const string otrasActividadesPalabrasClave = "http://vivoweb.org/ontology/core#freeTextKeyword@@@http://w3id.org/roh/CategoryPath|http://w3id.org/roh/categoryNode";
        public const string otrasActividadesPaisRealizacion = "https://www.w3.org/2006/vcard/ns#hasCountryName";
        public const string otrasActividadesCCAARealizacion = "https://www.w3.org/2006/vcard/ns#hasRegion";
        public const string otrasActividadesCiudadRealizacion = "https://www.w3.org/2006/vcard/ns#locality";
        public const string otrasActividadesEntidadOrganizadora = "http://w3id.org/roh/promotedBy";
        public const string otrasActividadesEntidadOrganizadoraNombre = "http://w3id.org/roh/promotedByTitle";
        public const string otrasActividadesTipoEntidadOrganizadora = "http://w3id.org/roh/promotedByType";
        public const string otrasActividadesTipoEntidadOrganizadoraOtros = "http://w3id.org/roh/promotedByTypeOther";
        public const string otrasActividadesFechaFinalizacion = "http://vivoweb.org/ontology/core#end";


        /// <summary>
        /// Aportaciones más relevantes de su CV de docencia - 030.110.000.000
        /// </summary>
        public const string aportacionesCVDescripcion = "http://w3id.org/roh/title";
        public const string aportacionesCVPalabrasClave = "http://vivoweb.org/ontology/core#freeTextKeyword@@@http://w3id.org/roh/CategoryPath|http://w3id.org/roh/categoryNode";
        public const string aportacionesCVPaisRealizacion = "https://www.w3.org/2006/vcard/ns#hasCountryName";
        public const string aportacionesCVCCAARealizacion = "https://www.w3.org/2006/vcard/ns#hasRegion";
        public const string aportacionesCVCiudadRealizacion = "https://www.w3.org/2006/vcard/ns#locality";
        public const string aportacionesCVEntidadOrganizadora = "http://w3id.org/roh/promotedBy";
        public const string aportacionesCVEntidadOrganizadoraNombre = "http://w3id.org/roh/promotedByTitle";
        public const string aportacionesCVTipoEntidadOrganizadora = "http://w3id.org/roh/promotedByType";
        public const string aportacionesCVTipoEntidadOrganizadoraOtros = "http://w3id.org/roh/promotedByTypeOther";
        public const string aportacionesCVFechaFinalizacion = "http://vivoweb.org/ontology/core#end";

    }
}
