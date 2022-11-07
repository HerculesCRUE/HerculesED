namespace ImportadorWebCV.Variables
{
    class FormacionAcademica
    {
        /// <summary>
        /// Estudios de 1º y 2º ciclo, y antiguos ciclos (Licenciados, Diplomados, Ingenieros Superiores, Ingenieros Técnicos, Arquitectos) - 020.010.010.000
        /// </summary>
        public const string estudiosCicloTipoTitulacion = "http://w3id.org/roh/universityDegreeType";
        public const string estudiosCicloTipoTitulacionOtros = "http://w3id.org/roh/universityDegreeTypeOther";
        public const string estudiosCicloNombreTitulo = "http://w3id.org/roh/title";
        public const string estudiosCicloTitulo = "http://w3id.org/roh/degreeType";
        public const string estudiosCicloPaisEntidadTitulacion = "https://www.w3.org/2006/vcard/ns#hasCountryName";
        public const string estudiosCicloCCAAEntidadTitulacion = "https://www.w3.org/2006/vcard/ns#hasRegion";
        public const string estudiosCicloCiudadEntidadTitulacion = "https://www.w3.org/2006/vcard/ns#locality";
        public const string estudiosCicloEntidadTitulacion = "http://w3id.org/roh/conductedBy";
        public const string estudiosCicloEntidadTitulacionNombre = "http://w3id.org/roh/conductedByTitle";
        public const string estudiosCicloTipoEntidadTitulacion = "http://w3id.org/roh/conductedByType";
        public const string estudiosCicloTipoEntidadTitulacionOtros = "http://w3id.org/roh/conductedByTypeOther";
        public const string estudiosCicloFechaTitulacion = "http://purl.org/dc/terms/issued";
        public const string estudiosCicloNotaMedia = "http://w3id.org/roh/mark";
        public const string estudiosCicloTituloExtranjero = "http://w3id.org/roh/foreignDegreeType";
        public const string estudiosCicloTituloExtranjeroNombre = "http://w3id.org/roh/foreignTitle";
        public const string estudiosCicloFechaHomologacion = "http://w3id.org/roh/approvedDate";
        public const string estudiosCicloTituloHomologado = "http://w3id.org/roh/approvedDegree";
        public const string estudiosCicloPremio = "http://w3id.org/roh/prize";
        public const string estudiosCicloPremioOtros = "http://w3id.org/roh/prizeOther";

        /// <summary>
        /// Doctorados - 020.010.020.000
        /// </summary>
        public const string doctoradosProgramaDoctorado = "http://w3id.org/roh/doctoralProgram";
        public const string doctoradosProgramaDoctoradoNombre = "http://w3id.org/roh/title";
        public const string doctoradosEntidadTitulacionDEA = "http://w3id.org/roh/deaEntity";
        public const string doctoradosEntidadTitulacionDEANombre = "http://w3id.org/roh/deaEntityTitle";
        public const string doctoradosFechaObtencionDEA = "http://w3id.org/roh/deaDate";
        public const string doctoradosPaisEntidadTitulacion = "https://www.w3.org/2006/vcard/ns#hasCountryName";
        public const string doctoradosCCAAEntidadTitulacion = "https://www.w3.org/2006/vcard/ns#hasRegion";
        public const string doctoradosCiudadEntidadTitulacion = "https://www.w3.org/2006/vcard/ns#locality";
        public const string doctoradosEntidadTitulacion = "http://w3id.org/roh/conductedBy";
        public const string doctoradosEntidadTitulacionNombre = "http://w3id.org/roh/conductedByTitle";
        public const string doctoradosTipoEntidadTitulacion = "http://w3id.org/roh/conductedByType";
        public const string doctoradosTipoEntidadTitulacionOtros = "http://w3id.org/roh/conductedByTypeOther";
        public const string doctoradosFechaTitulacion = "http://purl.org/dc/terms/issued";
        public const string doctoradosFechaMencionDocUE = "http://w3id.org/roh/europeanDoctorateDate";
        public const string doctoradosTituloTesis = "http://w3id.org/roh/thesisTitle";
        public const string doctoradosDirectorTesisFirma = "http://w3id.org/roh/directorNick";
        public const string doctoradosDirectorTesisNombre = "http://w3id.org/roh/directorName";
        public const string doctoradosDirectorTesisPrimerApellido = "http://w3id.org/roh/directorFirstSurname";
        public const string doctoradosDirectorTesisSegundoApellido = "http://w3id.org/roh/directorSecondSurname";
        public const string doctoradosCodirectorTesisOrden = "http://w3id.org/roh/codirector@@@http://w3id.org/roh/PersonAux|http://www.w3.org/1999/02/22-rdf-syntax-ns#comment";
        public const string doctoradosCodirectorTesisFirma = "http://w3id.org/roh/codirector@@@http://w3id.org/roh/PersonAux|http://xmlns.com/foaf/0.1/nick";
        public const string doctoradosCodirectorTesisNombre = "http://w3id.org/roh/codirector@@@http://w3id.org/roh/PersonAux|http://xmlns.com/foaf/0.1/firstName";
        public const string doctoradosCodirectorTesisPrimerApellido = "http://w3id.org/roh/codirector@@@http://w3id.org/roh/PersonAux|http://xmlns.com/foaf/0.1/familyName";
        public const string doctoradosCodirectorTesisSegundoApellido = "http://w3id.org/roh/codirector@@@http://w3id.org/roh/PersonAux|http://w3id.org/roh/secondFamilyName";
        public const string doctoradosCalificacionObtenida = "http://w3id.org/roh/qualification";
        public const string doctoradosDoctoradoUE = "http://w3id.org/roh/europeanDoctorate";
        public const string doctoradosMencionCalidad = "http://w3id.org/roh/qualityMention";
        public const string doctoradosPremioExtraordinario = "http://w3id.org/roh/doctorExtraordinaryAward";
        public const string doctoradosFechaObtencion = "http://w3id.org/roh/doctorExtraordinaryAwardDate";
        public const string doctoradosTituloHomologado = "http://w3id.org/roh/approvedDegree";
        public const string doctoradosFechaHomologacion = "http://w3id.org/roh/approvedDate";

        /// <summary>
        /// Otra formación de posgrado - 020.010.030.000
        /// </summary>
        public const string otraFormacionTipoFormacion = "http://w3id.org/roh/formationType";
        public const string otraFormacionTituloPosgrado = "http://w3id.org/roh/postgradeDegree";
        public const string otraFormacionTituloPosgradoNombre = "http://w3id.org/roh/title";
        public const string otraFormacionPaisEntidadTitulacion = "https://www.w3.org/2006/vcard/ns#hasCountryName";
        public const string otraFormacionCCAAEntidadTitulacion = "https://www.w3.org/2006/vcard/ns#hasRegion";
        public const string otraFormacionCiudadEntidadTitulacion = "https://www.w3.org/2006/vcard/ns#locality";
        public const string otraFormacionEntidadTitulacion = "http://w3id.org/roh/conductedBy";
        public const string otraFormacionEntidadTitulacionNombre = "http://w3id.org/roh/conductedByTitle";
        public const string otraFormacionTipoEntidadTitulacion = "http://w3id.org/roh/conductedByType";
        public const string otraFormacionTipoEntidadTitulacionOtros = "http://w3id.org/roh/conductedByTypeOther";
        public const string otraFormacionFechaTitulacion = "http://purl.org/dc/terms/issued";
        public const string otraFormacionCalificacionObtenida = "http://w3id.org/roh/qualification";
        public const string otraFormacionFacultadEscuela = "http://w3id.org/roh/center";
        public const string otraFormacionTituloHomologado = "http://w3id.org/roh/approvedDegree";
        public const string otraFormacionFechaHomologacion = "http://w3id.org/roh/approvedDate";

        /// <summary>
        /// Formación especializada, continuada, técnica, profesionalizada, de reciclaje y actualización - 020.020.000.000
        /// </summary>
        public const string formacionEspeTipoFormacion = "http://w3id.org/roh/formationActivityType";
        public const string formacionEspeTipoFormacionOtros = "http://w3id.org/roh/formationActivityTypeOther";
        public const string formacionEspeTituloFormacion = "http://w3id.org/roh/title";
        public const string formacionEspePaisEntidadTitulacion = "https://www.w3.org/2006/vcard/ns#hasCountryName";
        public const string formacionEspeCCAAEntidadTitulacion = "https://www.w3.org/2006/vcard/ns#hasRegion";
        public const string formacionEspeCiudadEntidadTitulacion = "https://www.w3.org/2006/vcard/ns#locality";
        public const string formacionEspeEntidadTitulacion = "http://w3id.org/roh/conductedBy";
        public const string formacionEspeEntidadTitulacionNombre = "http://w3id.org/roh/conductedByTitle";
        public const string formacionEspeTipoEntidadTitulacion = "http://w3id.org/roh/conductedByType";
        public const string formacionEspeTipoEntidadTitulacionOtros = "http://w3id.org/roh/conductedByTypeOther";
        public const string formacionEspeObjetivosEntidad = "http://w3id.org/roh/goals";
        public const string formacionEspeResponsableFirma = "http://w3id.org/roh/trainerNick";
        public const string formacionEspeResponsableNombre = "http://w3id.org/roh/trainerName";
        public const string formacionEspeResponsablePrimerApellido = "http://w3id.org/roh/trainerFirstSurname";
        public const string formacionEspeResponsableSegundoApellido = "http://w3id.org/roh/trainerSecondSurname";
        public const string formacionEspeDuracionHoras = "http://w3id.org/roh/durationHours";
        public const string formacionEspeFechaFinalizacion = "http://vivoweb.org/ontology/core#end";

        /// <summary>
        /// Cursos y seminarios recibidos de perfeccionamiento, innovación y mejora 
        /// docente, nuevas tecnologías, etc., cuyo objetivo sea la mejora de la docencia - 020.050.000.000
        /// </summary>
        public const string cursosSeminariosTitulo = "http://w3id.org/roh/title";
        public const string cursosSeminariosObjetivos = "http://w3id.org/roh/goals";
        public const string cursosSeminariosPaisEntidadOrganizadora = "https://www.w3.org/2006/vcard/ns#hasCountryName";
        public const string cursosSeminariosCCAAEntidadOrganizadora = "https://www.w3.org/2006/vcard/ns#hasRegion";
        public const string cursosSeminariosCiudadEntidadOrganizadora = "https://www.w3.org/2006/vcard/ns#locality";
        public const string cursosSeminariosEntidadOrganizadora = "http://w3id.org/roh/conductedBy";
        public const string cursosSeminariosEntidadOrganizadoraNombre = "http://w3id.org/roh/conductedByTitle";
        public const string cursosSeminariosTipoEntidadOrganizadora = "http://w3id.org/roh/conductedByType";
        public const string cursosSeminariosTipoEntidadOrganizadoraOtros = "http://w3id.org/roh/conductedByTypeOther";
        public const string cursosSeminariosDuracionHoras = "http://w3id.org/roh/durationHours";
        public const string cursosSeminariosFechaFinal = "http://vivoweb.org/ontology/core#end";
        public const string cursosSeminariosPerfilDestinatarios = "http://w3id.org/roh/targetProfile";
        public const string cursosSeminariosFechaInicio = "http://purl.org/dc/terms/issued";
        public const string cursosSeminariosFacultadEscuela = "http://w3id.org/roh/center";
        public const string cursosSeminariosMesesAnio = "http://w3id.org/roh/durationYears";
        public const string cursosSeminariosMesesMes = "http://w3id.org/roh/durationMonths";
        public const string cursosSeminariosMesesDia = "http://w3id.org/roh/durationDays";
        public const string cursosSeminariosProgramaFinanciacion = "http://w3id.org/roh/fundingProgram";
        public const string cursosSeminariosTareasContrastables = "http://w3id.org/roh/performedTasks";
        public const string cursosSeminariosObjetivoEstancia = "http://w3id.org/roh/stayGoal";
        public const string cursosSeminariosObjetivoEstanciaOtros = "http://w3id.org/roh/stayGoalOther";

        /// <summary>
        /// Conocimiento de idiomas - 020.060.000.000
        /// </summary>
        public const string conocimientoIdiomasIdioma = "http://w3id.org/roh/languageOfTheCertificate@@@http://purl.org/dc/elements/1.1/title";
        public const string conocimientoIdiomasComprensionAuditiva = "http://w3id.org/roh/listeningSkill@@@http://purl.org/dc/elements/1.1/title";
        public const string conocimientoIdiomasComprensionLectura = "http://w3id.org/roh/readingSkill@@@http://purl.org/dc/elements/1.1/title";
        public const string conocimientoIdiomasInteraccionOral = "http://w3id.org/roh/spokingInteractionSkill@@@http://purl.org/dc/elements/1.1/title";
        public const string conocimientoIdiomasExpresionOral = "http://w3id.org/roh/speakingSkill@@@http://purl.org/dc/elements/1.1/title";
        public const string conocimientoIdiomasExpresionEscrita = "http://w3id.org/roh/writingSkill@@@http://purl.org/dc/elements/1.1/title";
    }
}
