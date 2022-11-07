namespace ImportadorWebCV.Variables
{
    class ActividadCientificaTecnologica
    {
        /// <summary>
        /// Producción cientifica - 060.010.000.000
        /// </summary>
        public const string prodCientificaIndiceH = "http://w3id.org/roh/h-index";
        public const string prodCientificaFechaAplicacion = "http://purl.org/dc/terms/issued";
        public const string prodCientificaFuenteIndiceH = "http://w3id.org/roh/h-indexSource";
        public const string prodCientificaFuenteIndiceHOtros = "http://w3id.org/roh/h-indexSourceOther";

        /// <summary>
        /// Indicadores generales de calidad de la producción científica - 060.010.060.000
        /// </summary>
        public const string indicadoresGeneralesCalidad = "http://w3id.org/roh/generalQualityIndicator";

        /// <summary>
        /// Publicaciones, documentos científicos y técnicos - 060.010.010.000
        /// <summary>
        public const string pubDocumentosTipoProd = "http://purl.org/dc/elements/1.1/type";
        public const string pubDocumentosTipoProdOtros = "http://w3id.org/roh/typeOthers";
        public const string pubDocumentosGradoContribucion = "http://w3id.org/roh/contributionGrade";
        public const string pubDocumentosTipoSoporte = "http://w3id.org/roh/supportType";
        public const string pubDocumentosPubTitulo = "http://w3id.org/roh/title";
        public const string pubDocumentosPubMainDoc = "http://vivoweb.org/ontology/core#hasPublicationVenue";
        public const string pubDocumentosPubNombre = "http://w3id.org/roh/hasPublicationVenueText";
        public const string pubDocumentosPubVolumen = "http://purl.org/ontology/bibo/volume";
        public const string pubDocumentosPubNumero = "http://purl.org/ontology/bibo/issue";
        public const string pubDocumentosPubPagIni = "http://purl.org/ontology/bibo/pageStart";
        public const string pubDocumentosPubPagFin = "http://purl.org/ontology/bibo/pageEnd";
        public const string pubDocumentosPubPais = "https://www.w3.org/2006/vcard/ns#hasCountryName";
        public const string pubDocumentosPubCCAA = "https://www.w3.org/2006/vcard/ns#hasRegion";
        public const string pubDocumentosPubFecha = "http://purl.org/dc/terms/issued";
        public const string pubDocumentosPubURL = "https://www.w3.org/2006/vcard/ns#url";
        public const string pubDocumentosPubISBN = "http://w3id.org/roh/isbn";
        public const string pubDocumentosPubDepositoLegal = "http://w3id.org/roh/legalDeposit";
        public const string pubDocumentosPubCiudad = "https://www.w3.org/2006/vcard/ns#locality";
        public const string pubDocumentosColeccion = "http://w3id.org/roh/collection";
        public const string pubDocumentosResultadosDestacados = "http://w3id.org/roh/relevantResults";
        public const string pubDocumentosPubRelevante = "http://w3id.org/roh/relevantPublication";
        public const string pubDocumentosReseniaRevista = "http://w3id.org/roh/reviewsNumber";
        public const string pubDocumentosTraduccion = "https://www.w3.org/2006/vcard/ns#hasLanguage";
        public const string pubDocumentosAutorCorrespondencia = "http://w3id.org/roh/correspondingAuthor";
        public const string pubDocumentosIDPubDigitalDOI = "http://purl.org/ontology/bibo/doi";
        public const string pubDocumentosIDPubDigitalHandle = "http://purl.org/ontology/bibo/handle";
        public const string pubDocumentosIDPubDigitalPMID = "http://purl.org/ontology/bibo/pmid";
        public const string pubDocumentosIDOtroPubDigital = "http://purl.org/ontology/bibo/identifier@@@http://xmlns.com/foaf/0.1/Document|http://purl.org/dc/elements/1.1/title";
        public const string pubDocumentosNombreOtroPubDigital = "http://purl.org/ontology/bibo/identifier@@@http://xmlns.com/foaf/0.1/Document|http://xmlns.com/foaf/0.1/topic";
        public const string pubDocumentosPubEditorial = "http://purl.org/ontology/bibo/publisher";
        public const string pubDocumentosIndiceImpactoSource = "http://w3id.org/roh/impactIndex@@@http://w3id.org/roh/ImpactIndex|http://w3id.org/roh/impactSource";
        public const string pubDocumentosIndiceImpactoSourceOther = "http://w3id.org/roh/impactIndex@@@http://w3id.org/roh/ImpactIndex|http://w3id.org/roh/impactSourceOther";
        public const string pubDocumentosIndiceImpactoCategoria = "http://w3id.org/roh/impactIndex@@@http://w3id.org/roh/ImpactIndex|http://w3id.org/roh/impactIndexCategory";
        public const string pubDocumentosIndiceImpactoIndexInYear = "http://w3id.org/roh/impactIndex@@@http://w3id.org/roh/ImpactIndex|http://w3id.org/roh/impactIndexInYear";
        public const string pubDocumentosIndiceImpactoPublicationPosition = "http://w3id.org/roh/impactIndex@@@http://w3id.org/roh/ImpactIndex|http://w3id.org/roh/publicationPosition";
        public const string pubDocumentosIndiceImpactoJournalNumberInCat = "http://w3id.org/roh/impactIndex@@@http://w3id.org/roh/ImpactIndex|http://w3id.org/roh/journalNumberInCat";
        public const string pubDocumentosIndiceImpactoCuartil = "http://w3id.org/roh/impactIndex@@@http://w3id.org/roh/ImpactIndex|http://w3id.org/roh/quartile";
        public const string pubDocumentosCitasInrecs = "http://w3id.org/roh/inrecsCitationCount";
        public const string pubDocumentosCitasGoogleScholar = "http://w3id.org/roh/googleScholarCitationCount";
        public const string pubDocumentosCitasScopus = "http://w3id.org/roh/scopusCitationCount";
        public const string pubDocumentosCitasWOS = "http://w3id.org/roh/wosCitationCount";
        public const string pubDocumentosCitasScholar = "http://w3id.org/roh/semanticScholarCitationCount";
        public const string pubDocumentosAutores = "http://purl.org/ontology/bibo/authorList@@@http://purl.org/ontology/bibo/Document|http://www.w3.org/1999/02/22-rdf-syntax-ns#member";
        public const string pubDocumentosAutoresFirma = "http://purl.org/ontology/bibo/authorList@@@http://purl.org/ontology/bibo/Document|http://xmlns.com/foaf/0.1/nick";
        public const string pubDocumentosNombreRevista = "http://w3id.org/roh/hasPublicationVenueJournalText";
        public const string pubDocumentosAreasTematicasEnriquecidas = "http://w3id.org/roh/enrichedKnowledgeArea@@@http://w3id.org/roh/CategoryPath|http://w3id.org/roh/categoryNode";
        public const string pubDocumentosTextosEnriquecidosTitulo = "http://w3id.org/roh/enrichedKeywords@@@http://w3id.org/roh/EnrichedKeyWord|http://w3id.org/roh/title";
        public const string pubDocumentosTextosEnriquecidosScore = "http://w3id.org/roh/enrichedKeywords@@@http://w3id.org/roh/EnrichedKeyWord|http://w3id.org/roh/score";
        public const string pubDocumentosDescripcion = "http://purl.org/ontology/bibo/abstract";
        public const string pubDocumentosURLDocumento = "http://w3id.org/roh/hasFile";
        public const string pubDocumentosOpenAccess = "http://w3id.org/roh/openAccess";
        public const string pubDocumentosAreasTematicasExternas = "http://w3id.org/roh/externalKnowledgeArea@@@http://w3id.org/roh/CategoryPath|http://w3id.org/roh/categoryNode";
        public const string pubDocumentosTextosExternosTitulo = "http://w3id.org/roh/externalKeywords";
        public const string pubDocumentosBiblioDOI = "http://w3id.org/roh/references@@@http://w3id.org/roh/Reference|http://purl.org/ontology/bibo/doi";
        public const string pubDocumentosBiblioURL = "http://w3id.org/roh/references@@@http://w3id.org/roh/Reference|https://www.w3.org/2006/vcard/ns#url";
        public const string pubDocumentosBiblioAnioPub = "http://w3id.org/roh/references@@@http://w3id.org/roh/Reference|http://purl.org/dc/terms/issued";
        public const string pubDocumentosBiblioTitulo = "http://w3id.org/roh/references@@@http://w3id.org/roh/Reference|http://w3id.org/roh/title";
        public const string pubDocumentosBiblioRevista = "http://w3id.org/roh/references@@@http://w3id.org/roh/Reference|http://w3id.org/roh/hasPublicationVenueText";
        public const string pubDocumentosBiblioAutoresNombre = "http://w3id.org/roh/references@@@http://w3id.org/roh/Reference|http://w3id.org/roh/authorList@@@http://w3id.org/roh/ReferenceAuthor|http://xmlns.com/foaf/0.1/name";
        public const string pubDocumentosBiblioAutoresScholarID = "http://w3id.org/roh/references@@@http://w3id.org/roh/Reference|http://w3id.org/roh/authorList@@@http://w3id.org/roh/ReferenceAuthor|http://w3id.org/roh/semanticScholarId";
        public const string pubDocumentosOrigenFuentes = "http://w3id.org/roh/dataOrigin";
        public const string pubDocumentosIDNombre = "http://purl.org/ontology/bibo/identifier@@@http://xmlns.com/foaf/0.1/Document|http://xmlns.com/foaf/0.1/topic";
        public const string pubDocumentosIDValor = "http://purl.org/ontology/bibo/identifier@@@http://xmlns.com/foaf/0.1/Document|http://purl.org/dc/elements/1.1/title";

        /// <summary>
        /// Trabajos presentados en congresos nacionales o internacionales - 060.010.020.000
        /// <summary>
        public const string trabajosCongresosTipoEvento = "http://w3id.org/roh/presentedAtType";
        public const string trabajosCongresosTipoEventoOtros = "http://w3id.org/roh/presentedAtTypeOther";
        public const string trabajosCongresosTitulo = "http://w3id.org/roh/title";
        public const string trabajosCongresosTipoParticipacion = "http://w3id.org/roh/participationType";
        public const string trabajosCongresosIntervencion = "http://w3id.org/roh/inscriptionType";
        public const string trabajosCongresosIntervencionOtros = "http://w3id.org/roh/inscriptionTypeOther";
        public const string trabajosCongresosAmbitoGeo = "http://w3id.org/roh/presentedAtGeographicFocus";
        public const string trabajosCongresosAmbitoGeoOtros = "http://w3id.org/roh/presentedAtGeographicFocusOther";
        public const string trabajosCongresosNombreCongreso = "http://purl.org/ontology/bibo/presentedAt";
        public const string trabajosCongresosEntidadOrganizadora = "http://w3id.org/roh/presentedAtOrganizer";
        public const string trabajosCongresosEntidadOrganizadoraNombre = "http://w3id.org/roh/presentedAtOrganizerTitle";
        public const string trabajosCongresosTipoEntidadOrganizadora = "http://w3id.org/roh/presentedAtOrganizerType";
        public const string trabajosCongresosTipoEntidadOrganizadoraOtros = "http://w3id.org/roh/presentedAtOrganizerTypeOther";
        public const string trabajosCongresosCiudadEntidadOrganizadora = "http://w3id.org/roh/presentedAtOrganizerLocality";
        public const string trabajosCongresosCCAAEntidadOrganizadora = "http://w3id.org/roh/presentedAtOrganizerHasRegion";
        public const string trabajosCongresosPaisEntidadOrganizadora = "http://w3id.org/roh/presentedAtOrganizerHasCountryName";
        public const string trabajosCongresosPaisCelebracion = "http://w3id.org/roh/presentedAtHasCountryName";
        public const string trabajosCongresosCCAACelebracion = "http://w3id.org/roh/presentedAtHasRegion";
        public const string trabajosCongresosCiudadCelebracion = "http://w3id.org/roh/presentedAtLocality";
        public const string trabajosCongresosFechaCelebracion = "http://w3id.org/roh/presentedAtStart";
        public const string trabajosCongresosPubActa = "http://w3id.org/roh/congressProceedingsPublication";
        public const string trabajosCongresosComiteExterno = "http://w3id.org/roh/presentedAtWithExternalAdmissionsCommittee";
        public const string trabajosCongresosFormaContribucion = "http://purl.org/dc/elements/1.1/type";
        public const string trabajosCongresosPubTitulo = "http://w3id.org/roh/publicationTitle";
        public const string trabajosCongresosPubNombre = "http://w3id.org/roh/hasPublicationVenueText";
        public const string trabajosCongresosPubVolumen = "http://purl.org/ontology/bibo/volume";
        public const string trabajosCongresosPubNumero = "http://purl.org/ontology/bibo/issue";
        public const string trabajosCongresosPubPagIni = "http://purl.org/ontology/bibo/pageStart";
        public const string trabajosCongresosPubPagFin = "http://purl.org/ontology/bibo/pageEnd";
        public const string trabajosCongresosPubEditorial = "http://purl.org/ontology/bibo/publisher";
        public const string trabajosCongresosPubPais = "https://www.w3.org/2006/vcard/ns#hasCountryName";
        public const string trabajosCongresosPubCCAA = "https://www.w3.org/2006/vcard/ns#hasRegion";
        public const string trabajosCongresosPubFecha = "http://purl.org/dc/terms/issued";
        public const string trabajosCongresosPubURL = "https://www.w3.org/2006/vcard/ns#url";
        public const string trabajosCongresosPubISBN = "http://w3id.org/roh/isbn";
        public const string trabajosCongresosPubISSN = "http://purl.org/ontology/bibo/issn";
        public const string trabajosCongresosPubDepositoLegal = "http://w3id.org/roh/legalDeposit";
        public const string trabajosCongresosFechaFin = "http://w3id.org/roh/presentedAtEnd";
        public const string trabajosCongresosAutorCorrespondencia = "http://w3id.org/roh/correspondingAuthor";
        public const string trabajosCongresosIDPubDigitalDOI = "http://purl.org/ontology/bibo/doi";
        public const string trabajosCongresosIDPubDigitalHandle = "http://purl.org/ontology/bibo/handle";
        public const string trabajosCongresosIDPubDigitalPMID = "http://purl.org/ontology/bibo/pmid";
        public const string trabajosCongresosIDOtroPubDigital = "http://purl.org/ontology/bibo/identifier@@@http://xmlns.com/foaf/0.1/Document|http://purl.org/dc/elements/1.1/title";
        public const string trabajosCongresosNombreOtroPubDigital = "http://purl.org/ontology/bibo/identifier@@@http://xmlns.com/foaf/0.1/Document|http://xmlns.com/foaf/0.1/topic";
        public const string trabajosCongresosCitasInrecs = "http://w3id.org/roh/inrecsCitationCount";
        public const string trabajosCongresosCitasGoogleScholar = "http://w3id.org/roh/googleScholarCitationCount";
        public const string trabajosCongresosCitasScopus = "http://w3id.org/roh/scopusCitationCount";
        public const string trabajosCongresosCitasWOS = "http://w3id.org/roh/wosCitationCount";
        public const string trabajosCongresosCitasScholar = "http://w3id.org/roh/semanticScholarCitationCount";
        public const string trabajosCongresosMiembrosAutores = "http://purl.org/ontology/bibo/authorList@@@http://purl.org/ontology/bibo/Document|http://www.w3.org/1999/02/22-rdf-syntax-ns#member";
        public const string trabajosCongresosMiembrosAutorFirma = "http://purl.org/ontology/bibo/authorList@@@http://purl.org/ontology/bibo/Document|http://xmlns.com/foaf/0.1/nick";
        public const string trabajosCongresosAreasTematicasEnriquecidas = "http://w3id.org/roh/enrichedKnowledgeArea@@@http://w3id.org/roh/CategoryPath|http://w3id.org/roh/categoryNode";
        public const string trabajosCongresosTextosEnriquecidosTitulo = "http://w3id.org/roh/enrichedKeywords@@@http://w3id.org/roh/EnrichedKeyWord|http://w3id.org/roh/title";
        public const string trabajosCongresosTextosEnriquecidosScore = "http://w3id.org/roh/enrichedKeywords@@@http://w3id.org/roh/EnrichedKeyWord|http://w3id.org/roh/score";
        public const string trabajosCongresosDescripcion = "http://purl.org/ontology/bibo/abstract";
        public const string trabajosCongresosURLDocumento = "http://w3id.org/roh/hasFile";
        public const string trabajosCongresosOpenAccess = "http://w3id.org/roh/openAccess";
        public const string trabajosCongresosBiblioDOI = "http://w3id.org/roh/references@@@http://w3id.org/roh/Reference|http://purl.org/ontology/bibo/doi";
        public const string trabajosCongresosBiblioURL = "http://w3id.org/roh/references@@@http://w3id.org/roh/Reference|https://www.w3.org/2006/vcard/ns#url";
        public const string trabajosCongresosBiblioAnioPub = "http://w3id.org/roh/references@@@http://w3id.org/roh/Reference|http://purl.org/dc/terms/issued";
        public const string trabajosCongresosBiblioTitulo = "http://w3id.org/roh/references@@@http://w3id.org/roh/Reference|http://w3id.org/roh/title";
        public const string trabajosCongresosBiblioRevista = "http://w3id.org/roh/references@@@http://w3id.org/roh/Reference|http://w3id.org/roh/hasPublicationVenueText";
        public const string trabajosCongresosBiblioAutoresNombre = "http://w3id.org/roh/references@@@http://w3id.org/roh/Reference|http://w3id.org/roh/authorList@@@http://w3id.org/roh/ReferenceAuthor|http://xmlns.com/foaf/0.1/name";
        public const string trabajosCongresosBiblioAutoresScholarID = "http://w3id.org/roh/references@@@http://w3id.org/roh/Reference|http://w3id.org/roh/authorList@@@http://w3id.org/roh/ReferenceAuthor|http://w3id.org/roh/semanticScholarId";
        public const string trabajosCongresosIDNombre = "http://purl.org/ontology/bibo/identifier@@@http://xmlns.com/foaf/0.1/Document|http://xmlns.com/foaf/0.1/topic";
        public const string trabajosCongresosIDValor = "http://purl.org/ontology/bibo/identifier@@@http://xmlns.com/foaf/0.1/Document|http://purl.org/dc/elements/1.1/title";
        public const string trabajosCongresosOrigenFuentes = "http://w3id.org/roh/dataOrigin";
        public const string trabajosCongresosAreasTematicasExternas = "http://w3id.org/roh/externalKnowledgeArea@@@http://w3id.org/roh/CategoryPath|http://w3id.org/roh/categoryNode";
        public const string trabajosCongresosTextosExternosTitulo = "http://w3id.org/roh/enrichedKeywords@@@http://w3id.org/roh/EnrichedKeyWord|http://w3id.org/roh/title";

        /// <summary>
        /// Trabajos presentados en jornadas, seminarios, talleres de trabajo y/o cursos nacionales o internacionales - 060.010.030.000
        /// <summary>
        public const string trabajosJornSemTituloTrabajo = "http://w3id.org/roh/title";
        public const string trabajosJornSemTipoEvento = "http://w3id.org/roh/presentedAtSeminarType";
        public const string trabajosJornSemTipoEventoOtros = "http://w3id.org/roh/presentedAtSeminarTypeOther";
        public const string trabajosJornSemIntervencion = "http://w3id.org/roh/inscriptionType";
        public const string trabajosJornSemAmbitoGeo = "http://w3id.org/roh/presentedAtGeographicFocus";
        public const string trabajosJornSemAmbitoGeoOtros = "http://w3id.org/roh/presentedAtGeographicFocusOther";
        public const string trabajosJornSemNombreEvento = "http://purl.org/ontology/bibo/presentedAt";
        public const string trabajosJornSemPaisEntidadOrganizadora = "http://w3id.org/roh/presentedAtOrganizerHasCountryName";
        public const string trabajosJornSemCCAAEntidadOrganizadora = "http://w3id.org/roh/presentedAtOrganizerHasRegion";
        public const string trabajosJornSemCiudadEntidadOrganizadora = "http://w3id.org/roh/presentedAtOrganizerLocality";
        public const string trabajosJornSemEntidadOrganizadora = "http://w3id.org/roh/presentedAtOrganizer";
        public const string trabajosJornSemEntidadOrganizadoraNombre = "http://w3id.org/roh/presentedAtOrganizerTitle";
        public const string trabajosJornSemTipoEntidadOrganizadora = "http://w3id.org/roh/presentedAtOrganizerType";
        public const string trabajosJornSemTipoEntidadOrganizadoraOtros = "http://w3id.org/roh/presentedAtOrganizerTypeOther";
        public const string trabajosJornSemPaisCelebracion = "http://w3id.org/roh/presentedAtHasCountryName";
        public const string trabajosJornSemCCAACelebracion = "http://w3id.org/roh/presentedAtHasRegion";
        public const string trabajosJornSemCiudadCelebracion = "http://w3id.org/roh/presentedAtLocality";
        public const string trabajosJornSemFechaCelebracion = "http://w3id.org/roh/presentedAtStart";
        public const string trabajosJornSemPubActaCongreso = "http://w3id.org/roh/congressProceedingsPublication";
        public const string trabajosJornSemPubActaCongresoExterno = "http://w3id.org/roh/presentedAtWithExternalAdmissionsCommittee";
        public const string trabajosJornSemPubTipo = "http://purl.org/dc/elements/1.1/type";
        public const string trabajosJornSemPubTitulo = "http://w3id.org/roh/publicationTitle";
        public const string trabajosJornSemPubNombre = "http://w3id.org/roh/hasPublicationVenueText";
        public const string trabajosJornSemPubVolumen = "http://purl.org/ontology/bibo/volume";
        public const string trabajosJornSemPubNumero = "http://purl.org/ontology/bibo/issue";
        public const string trabajosJornSemPubPagIni = "http://purl.org/ontology/bibo/pageStart";
        public const string trabajosJornSemPubPagFin = "http://purl.org/ontology/bibo/pageEnd";
        public const string trabajosJornSemPubEditorial = "http://purl.org/ontology/bibo/publisher";
        public const string trabajosJornSemPubPais = "https://www.w3.org/2006/vcard/ns#hasCountryName";
        public const string trabajosJornSemPubCCAA = "https://www.w3.org/2006/vcard/ns#hasRegion";
        public const string trabajosJornSemPubFecha = "http://purl.org/dc/terms/issued";
        public const string trabajosJornSemPubURL = "https://www.w3.org/2006/vcard/ns#url";
        public const string trabajosJornSemPubISBN = "http://w3id.org/roh/isbn";
        public const string trabajosJornSemPubISSN = "http://purl.org/ontology/bibo/issn";
        public const string trabajosJornSemPubDepositoLegal = "http://w3id.org/roh/legalDeposit";
        public const string trabajosJornSemPubFechaFinCelebracion = "http://w3id.org/roh/presentedAtEnd";
        public const string trabajosJornSemAutorCorrespondencia = "http://w3id.org/roh/correspondingAuthor";
        public const string trabajosJornSemIDPubDigitalDOI = "http://purl.org/ontology/bibo/doi";
        public const string trabajosJornSemIDPubDigitalHandle = "http://purl.org/ontology/bibo/handle";
        public const string trabajosJornSemIDPubDigitalPMID = "http://purl.org/ontology/bibo/pmid";
        public const string trabajosJornSemNombreOtroPubDigital = "http://purl.org/ontology/bibo/identifier@@@http://xmlns.com/foaf/0.1/Document|http://xmlns.com/foaf/0.1/topic";
        public const string trabajosJornSemIDOtroPubDigital = "http://purl.org/ontology/bibo/identifier@@@http://xmlns.com/foaf/0.1/Document|http://purl.org/dc/elements/1.1/title";
        public const string trabajosJornSemAutores = "http://purl.org/ontology/bibo/authorList@@@http://purl.org/ontology/bibo/Document|http://www.w3.org/1999/02/22-rdf-syntax-ns#member";
        public const string trabajosJornSemAutoresFirma = "http://purl.org/ontology/bibo/authorList@@@http://purl.org/ontology/bibo/Document|http://xmlns.com/foaf/0.1/nick";
        public const string trabajosJornSemAreasTematicasEnriquecidas = "http://w3id.org/roh/enrichedKnowledgeArea@@@http://w3id.org/roh/CategoryPath|http://w3id.org/roh/categoryNode";
        public const string trabajosJornSemTextosEnriquecidosTitulo = "http://w3id.org/roh/enrichedKeywords@@@http://w3id.org/roh/EnrichedKeyWord|http://w3id.org/roh/title";
        public const string trabajosJornSemTextosEnriquecidosScore = "http://w3id.org/roh/enrichedKeywords@@@http://w3id.org/roh/EnrichedKeyWord|http://w3id.org/roh/score";

        /// <summary>
        /// Otras actividades de divulgación - 060.010.040.000
        /// <summary>
        public const string otrasActDivulTitulo = "http://w3id.org/roh/title";
        public const string otrasActDivulTipoEvento = "http://w3id.org/roh/eventType";
        public const string otrasActDivulTipoEventoOtros = "http://w3id.org/roh/eventTypeOther";
        public const string otrasActDivulAutorNombre = "http://purl.org/ontology/bibo/authorList@@@http://purl.obolibrary.org/obo/BFO_0000023|http://xmlns.com/foaf/0.1/firstName";
        public const string otrasActDivulAutorPrimerApellido = "http://purl.org/ontology/bibo/authorList@@@http://purl.obolibrary.org/obo/BFO_0000023|http://xmlns.com/foaf/0.1/familyName";
        public const string otrasActDivulAutorSegundoApellido = "http://purl.org/ontology/bibo/authorList@@@http://purl.obolibrary.org/obo/BFO_0000023|http://w3id.org/roh/secondFamilyName";
        public const string otrasActDivulAutorFirma = "http://purl.org/ontology/bibo/authorList@@@http://purl.obolibrary.org/obo/BFO_0000023|http://xmlns.com/foaf/0.1/nick";
        public const string otrasActDivulAutorOrden = "http://purl.org/ontology/bibo/authorList@@@http://purl.obolibrary.org/obo/BFO_0000023|http://www.w3.org/1999/02/22-rdf-syntax-ns#comment";
        public const string otrasActDivulIntervencion = "http://w3id.org/roh/eventInscriptionType";
        public const string otrasActDivulIntervencionOtros = "http://w3id.org/roh/eventInscriptionTypeOther";
        public const string otrasActDivulAmbitoEvento = "http://vivoweb.org/ontology/core#geographicFocus";
        public const string otrasActDivulAmbitoEventoOtros = "http://w3id.org/roh/geographicFocusOther";
        public const string otrasActDivulNombreEvento = "http://purl.org/ontology/bibo/presentedAt";
        public const string otrasActDivulPaisEntidadOrg = "http://w3id.org/roh/promotedByHasCountryName";
        public const string otrasActDivulCCAAEntidadOrg = "http://w3id.org/roh/promotedByHasRegion";
        public const string otrasActDivulCiudadEntidadOrg = "http://w3id.org/roh/promotedByLocality";
        public const string otrasActDivulEntidadOrgNombre = "http://w3id.org/roh/promotedByTitle";
        public const string otrasActDivulEntidadOrg = "http://w3id.org/roh/promotedBy";
        public const string otrasActDivulTipoEntidadOrg = "http://w3id.org/roh/promotedByType";
        public const string otrasActDivulTipoEntidadOrgOtros = "http://w3id.org/roh/promotedByTypeOther";
        public const string otrasActDivulPaisCelebracion = "https://www.w3.org/2006/vcard/ns#hasCountryName";
        public const string otrasActDivulCCAACelebracion = "https://www.w3.org/2006/vcard/ns#hasRegion";
        public const string otrasActDivulCiudadCelebracion = "https://www.w3.org/2006/vcard/ns#locality";
        public const string otrasActDivulFechaCelebracion = "http://vivoweb.org/ontology/core#start";
        public const string otrasActDivulPubActaCongreso = "http://w3id.org/roh/congressProceedingsPublication";
        public const string otrasActDivulPubActaAdmisionExt = "http://w3id.org/roh/withExternalAdmissionsCommittee";
        public const string otrasActDivulPubTipo = "http://w3id.org/roh/publicationType";
        public const string otrasActDivulPubTitulo = "http://w3id.org/roh/publicationTitle";
        public const string otrasActDivulPubNombre = "http://w3id.org/roh/publicationName";
        public const string otrasActDivulPubVolumen = "http://purl.org/ontology/bibo/volume";
        public const string otrasActDivulPubNumero = "http://purl.org/ontology/bibo/issue";
        public const string otrasActDivulPubPagIni = "http://purl.org/ontology/bibo/pageStart";
        public const string otrasActDivulPubPagFin = "http://purl.org/ontology/bibo/pageEnd";
        public const string otrasActDivulPubPais = "http://w3id.org/roh/publicationHasCountryName";
        public const string otrasActDivulPubCCAA = "http://w3id.org/roh/publicationHasRegion";
        public const string otrasActDivulPubFecha = "http://purl.org/dc/terms/issued";
        public const string otrasActDivulPubURL = "https://www.w3.org/2006/vcard/ns#url";
        public const string otrasActDivulPubISBN = "http://w3id.org/roh/isbn";
        public const string otrasActDivulPubISSN = "http://purl.org/ontology/bibo/issn";
        public const string otrasActDivulPubDepositoLegal = "http://w3id.org/roh/legalDeposit";
        public const string otrasActDivulAutorCorrespondencia = "http://w3id.org/roh/correspondingAuthor";
        public const string otrasActDivulIDPubDigitalDOI = "http://purl.org/ontology/bibo/doi";
        public const string otrasActDivulIDPubDigitalHandle = "http://purl.org/ontology/bibo/handle";
        public const string otrasActDivulIDPubDigitalPMID = "http://purl.org/ontology/bibo/pmid";
        public const string otrasActDivulNombreOtroIDPubDigital = "http://purl.org/ontology/bibo/identifier@@@http://xmlns.com/foaf/0.1/Document|http://xmlns.com/foaf/0.1/topic";
        public const string otrasActDivulIDOtroPubDigital = "http://purl.org/ontology/bibo/identifier@@@http://xmlns.com/foaf/0.1/Document|http://purl.org/dc/elements/1.1/title";
        public const string otrasActDivulResponsableEditorial = "http://purl.org/ontology/bibo/publisher";

        /// <summary>
        /// Comités científicos, técnicos y/o asesores - 060.020.010.000
        /// <summary>
        public const string comitesCTATitulo = "http://w3id.org/roh/title";
        public const string comitesCTAPaisRadicacion = "https://www.w3.org/2006/vcard/ns#hasCountryName";
        public const string comitesCTACCAARadicacion = "https://www.w3.org/2006/vcard/ns#hasRegion";
        public const string comitesCTACiudadRadicacion = "https://www.w3.org/2006/vcard/ns#locality";
        public const string comitesCTAPaisEntidadAfiliacion = "http://w3id.org/roh/affiliatedOrganizationHasCountryName";
        public const string comitesCTACCAAEntidadAfiliacion = "http://w3id.org/roh/affiliatedOrganizationHasRegion";
        public const string comitesCTACiudadEntidadAfiliacion = "http://w3id.org/roh/affiliatedOrganizationLocality";
        public const string comitesCTAEntidadAfiliacion = "http://vivoweb.org/ontology/core#affiliatedOrganization";
        public const string comitesCTAEntidadAfiliacionNombre = "http://w3id.org/roh/affiliatedOrganizationTitle";
        public const string comitesCTATipoEntidadAfiliacion = "http://w3id.org/roh/affiliatedOrganizationType";
        public const string comitesCTATipoEntidadAfiliacionOtros = "http://w3id.org/roh/affiliatedOrganizationTypeOther";
        public const string comitesCTAAmbitoActividad = "http://vivoweb.org/ontology/core#geographicFocus";
        public const string comitesCTAAmbitoActividadOtros = "http://w3id.org/roh/geographicFocusOther";
        public const string comitesCTACodUnescoPrimaria = "http://w3id.org/roh/unescoPrimary@@@http://w3id.org/roh/CategoryPath|http://w3id.org/roh/categoryNode";
        public const string comitesCTACodUnescoSecundaria = "http://w3id.org/roh/unescoSecondary@@@http://w3id.org/roh/CategoryPath|http://w3id.org/roh/categoryNode";
        public const string comitesCTACodUnescoTerciaria = "http://w3id.org/roh/unescoTertiary@@@http://w3id.org/roh/CategoryPath|http://w3id.org/roh/categoryNode";
        public const string comitesCTAFechaInicio = "http://vivoweb.org/ontology/core#start";
        public const string comitesCTAFechaFinalizacion = "http://vivoweb.org/ontology/core#end";

        /// <summary>
        /// Organización de actividades de I+D+i - 060.020.030.000
        /// <summary>
        public const string orgIDITituloActividad = "http://w3id.org/roh/title";
        public const string orgIDITipoActividad = "http://w3id.org/roh/concreteFunctions";
        public const string orgIDIPaisActividad = "https://www.w3.org/2006/vcard/ns#hasCountryName";
        public const string orgIDICCAAActividad = "https://www.w3.org/2006/vcard/ns#hasRegion";
        public const string orgIDICiudadActividad = "https://www.w3.org/2006/vcard/ns#locality";
        public const string orgIDIPaisEntidadConvocante = "http://w3id.org/roh/promotedByHasCountryName";
        public const string orgIDICCAAEntidadConvocante = "http://w3id.org/roh/promotedByHasRegion";
        public const string orgIDICiudadEntidadConvocante = "http://w3id.org/roh/promotedByLocality";
        public const string orgIDIEntidadConvocante = "http://w3id.org/roh/promotedBy";
        public const string orgIDIEntidadConvocanteNombre = "http://w3id.org/roh/promotedByTitle";
        public const string orgIDITipoEntidadConvocante = "http://w3id.org/roh/promotedByType";
        public const string orgIDITipoEntidadConvocanteOtros = "http://w3id.org/roh/promotedByTypeOther";
        public const string orgIDIModoParticipacion = "http://w3id.org/roh/participationType";
        public const string orgIDIModoParticipacionOtros = "http://w3id.org/roh/participationTypeOther";
        public const string orgIDIAmbitoReunion = "http://vivoweb.org/ontology/core#geographicFocus";
        public const string orgIDIAmbitoReunionOtros = "http://w3id.org/roh/geographicFocusOther";
        public const string orgIDINumeroAsistentes = "http://w3id.org/roh/attendants";
        public const string orgIDIFechaInicio = "http://vivoweb.org/ontology/core#start";
        public const string orgIDIDuracionAnio = "http://w3id.org/roh/durationYears";
        public const string orgIDIDuracionMes = "http://w3id.org/roh/durationMonths";
        public const string orgIDIDuracionDia = "http://w3id.org/roh/durationDays";
        public const string orgIDIFechaFinalizacion = "http://vivoweb.org/ontology/core#end";

        /// <summary>
        /// Gestión de actividades de I+D+i - 060.020.040.000
        /// <summary>
        public const string gestionIDIFunciones = "http://w3id.org/roh/functions";
        public const string gestionIDIPaisEntidadRealizacion = "https://www.w3.org/2006/vcard/ns#hasCountryName";
        public const string gestionIDICCAAEntidadRealizacion = "https://www.w3.org/2006/vcard/ns#hasRegion";
        public const string gestionIDICiudadEntidadRealizacion = "https://www.w3.org/2006/vcard/ns#locality";
        public const string gestionIDINombreActividad = "http://w3id.org/roh/title";
        public const string gestionIDITipologiaGestion = "http://w3id.org/roh/managementType";
        public const string gestionIDITipologiaGestionOtros = "http://w3id.org/roh/managementTypeOther";
        public const string gestionIDIEntornoEntidadRealizacion = "http://w3id.org/roh/promotedBy";
        public const string gestionIDIEntornoEntidadRealizacionNombre = "http://w3id.org/roh/promotedByTitle";
        public const string gestionIDIEntornoTipoEntidadRealizacion = "http://w3id.org/roh/promotedByType";
        public const string gestionIDIEntornoTipoEntidadRealizacionOtros = "http://w3id.org/roh/promotedByTypeOther";
        public const string gestionIDIEntornoFechaInicio = "http://vivoweb.org/ontology/core#start";
        public const string gestionIDIEntornoDuracionAnio = "http://w3id.org/roh/durationYears";
        public const string gestionIDIEntornoDuracionMes = "http://w3id.org/roh/durationMonths";
        public const string gestionIDIEntornoDuracionDia = "http://w3id.org/roh/durationDays";
        public const string gestionIDISistemaAcceso = "http://w3id.org/roh/accessSystemActivity";
        public const string gestionIDISistemaAccesoOtros = "http://w3id.org/roh/accessSystemActivityOther";
        public const string gestionIDIPromedioPresupuesto = "http://w3id.org/roh/averageAnnualBudget";
        public const string gestionIDINumPersonas = "http://w3id.org/roh/attendants";
        public const string gestionIDIObjetivosEvento = "http://w3id.org/roh/goals";
        public const string gestionIDIPerfilGrupo = "http://w3id.org/roh/targetGroupProfile";
        public const string gestionIDIAmbitoTerritorial = "http://vivoweb.org/ontology/core#geographicFocus";
        public const string gestionIDIAmbitoTerritorialOtros = "http://w3id.org/roh/geographicFocusOther";
        public const string gestionIDITareasConcretas = "http://w3id.org/roh/concreteFunctions";
        public const string gestionIDIPalabrasClave = "http://vivoweb.org/ontology/core#freeTextKeyword@@@http://w3id.org/roh/CategoryPath|http://w3id.org/roh/categoryNode";

        /// <summary>
        /// Foros y comités nacionales e internacionales - 060.020.050.000
        /// <summary>
        public const string forosComitesNombre = "http://w3id.org/roh/title";
        public const string forosComitesPaisEntidadRealizacion = "https://www.w3.org/2006/vcard/ns#hasCountryName";
        public const string forosComitesCCAAEntidadRealizacion = "https://www.w3.org/2006/vcard/ns#hasRegion";
        public const string forosComitesCiudadEntidadRealizacion = "https://www.w3.org/2006/vcard/ns#locality";
        public const string forosComitesPaisEntidadOrganizadora = "http://w3id.org/roh/promotedByHasCountryName";
        public const string forosComitesCCAAEntidadOrganizadora = "http://w3id.org/roh/promotedByHasRegion";
        public const string forosComitesCiudadEntidadOrganizadora = "http://w3id.org/roh/promotedByLocality";
        public const string forosComitesEntidadOrganizadora = "http://w3id.org/roh/promotedBy";
        public const string forosComitesEntidadOrganizadoraNombre = "http://w3id.org/roh/promotedByTitle";
        public const string forosComitesTipoEntidadOrganizadora = "http://w3id.org/roh/promotedByType";
        public const string forosComitesTipoEntidadOrganizadoraOtros = "http://w3id.org/roh/promotedByTypeOther";
        public const string forosComitesCategoriaProfesional = "http://w3id.org/roh/professionalCategory";
        public const string forosComitesPaisEntidadRepresentada = "http://w3id.org/roh/representedEntityHasCountryName";
        public const string forosComitesCCAAEntidadRepresentada = "http://w3id.org/roh/representedEntityHasRegion";
        public const string forosComitesCiudadEntidadRepresentada = "http://w3id.org/roh/representedEntityLocality";
        public const string forosComitesOrganismoRepresentado = "http://w3id.org/roh/representedEntity";
        public const string forosComitesOrganismoRepresentadoNombre = "http://w3id.org/roh/representedEntityTitle";
        public const string forosComitesTipoOrganismoRepresentado = "http://w3id.org/roh/representedEntityType";
        public const string forosComitesTipoOrganismoRepresentadoOtros = "http://w3id.org/roh/representedEntityTypeOther";
        public const string forosComitesFechaInicio = "http://vivoweb.org/ontology/core#start";
        public const string forosComitesFechaFinalizacion = "http://vivoweb.org/ontology/core#end";


        /// <summary>
        /// Evaluación y revisión de proyectos y artículos de I+D+i - 060.020.060.000
        /// <summary>
        public const string evalRevIDIFunciones = "http://w3id.org/roh/functions";
        public const string evalRevIDINombre = "http://w3id.org/roh/title";
        public const string evalRevIDIEntidad = "http://w3id.org/roh/conductedBy";
        public const string evalRevIDIEntidadNombre = "http://w3id.org/roh/conductedByTitle";
        public const string evalRevIDIPaisEntidadRealizacion = "https://www.w3.org/2006/vcard/ns#hasCountryName";
        public const string evalRevIDICCAAEntidadRealizacion = "https://www.w3.org/2006/vcard/ns#hasRegion";
        public const string evalRevIDICiudadEntidadRealizacion = "https://www.w3.org/2006/vcard/ns#locality";
        public const string evalRevIDIModalidadActividad = "http://w3id.org/roh/activityModality";
        public const string evalRevIDIModalidadActividadOtros = "http://w3id.org/roh/activityModalityOther";
        public const string evalRevIDITipoEntidad = "http://w3id.org/roh/conductedByType";
        public const string evalRevIDITipoEntidadOtros = "http://w3id.org/roh/conductedByTypeOther";
        public const string evalRevIDIFechaInicio = "http://vivoweb.org/ontology/core#start";
        public const string evalRevIDIFechaFinalizacion = "http://vivoweb.org/ontology/core#end";
        public const string evalRevIDIFrecuenciaActividad = "http://w3id.org/roh/frequency";
        public const string evalRevIDISistemaAcceso = "http://w3id.org/roh/accessSystemActivity";
        public const string evalRevIDISistemaAccesoOtros = "http://w3id.org/roh/accessSystemActivityOther";
        public const string evalRevIDIAmbito = "http://vivoweb.org/ontology/core#geographicFocus";
        public const string evalRevIDIAmbitoOtros = "http://w3id.org/roh/geographicFocusOther";

        /// <summary>
        /// Estancias en centros de I+D+i públicos o privados - 060.010.050.000
        /// </summary>
        public const string estanciasIDIEntidadRealizacion = "http://w3id.org/roh/entity";
        public const string estanciasIDIEntidadRealizacionNombre = "http://w3id.org/roh/entityTitle";
        public const string estanciasIDITipoEntidadRealizacion = "http://w3id.org/roh/entityType";
        public const string estanciasIDITipoEntidadRealizacionOtros = "http://w3id.org/roh/entityTypeOther";
        public const string estanciasIDIPaisEntidadRealizacion = "https://www.w3.org/2006/vcard/ns#hasCountryName";
        public const string estanciasIDICCAAEntidadRealizacion = "https://www.w3.org/2006/vcard/ns#hasRegion";
        public const string estanciasIDICiudadEntidadRealizacion = "https://www.w3.org/2006/vcard/ns#locality";
        public const string estanciasIDIFechaInicioEntidadRealizacion = "http://vivoweb.org/ontology/core#start";
        public const string estanciasIDIDuracionAnioEntidadRealizacion = "http://w3id.org/roh/durationYears";
        public const string estanciasIDIDuracionMesEntidadRealizacion = "http://w3id.org/roh/durationMonths";
        public const string estanciasIDIDuracionDiaEntidadRealizacion = "http://w3id.org/roh/durationDays";
        public const string estanciasIDIObjetivoEstancia = "http://w3id.org/roh/goals";
        public const string estanciasIDIObjetivoEstanciaOtros = "http://w3id.org/roh/goalsOther";
        public const string estanciasIDICodUnescoPrimaria = "http://w3id.org/roh/unescoPrimary@@@http://w3id.org/roh/CategoryPath|http://w3id.org/roh/categoryNode";
        public const string estanciasIDICodUnescoSecundaria = "http://w3id.org/roh/unescoSecondary@@@http://w3id.org/roh/CategoryPath|http://w3id.org/roh/categoryNode";
        public const string estanciasIDICodUnescoTerciaria = "http://w3id.org/roh/unescoTertiary@@@http://w3id.org/roh/CategoryPath|http://w3id.org/roh/categoryNode";
        public const string estanciasIDIEntidadFinanciadora = "http://w3id.org/roh/fundedBy";
        public const string estanciasIDIEntidadFinanciadoraNombre = "http://w3id.org/roh/fundedByTitle";
        public const string estanciasIDIPaisEntidadFinanciadora = "http://w3id.org/roh/fundedByHasCountryName";
        public const string estanciasIDICCAAEntidadFinanciadora = "http://w3id.org/roh/fundedByHasRegion";
        public const string estanciasIDICiudadEntidadFinanciadora = "http://w3id.org/roh/fundedByLocality";
        public const string estanciasIDITipoEntidadFinanciadora = "http://w3id.org/roh/fundedByType";
        public const string estanciasIDITipoEntidadFinanciadoraOtros = "http://w3id.org/roh/fundedByTypeOther";
        public const string estanciasIDINombrePrograma = "http://w3id.org/roh/programme";
        public const string estanciasIDITareasContrastables = "http://w3id.org/roh/performedTasks";
        public const string estanciasIDICapacidadesAdquiridas = "http://w3id.org/roh/skillsDeveloped";
        public const string estanciasIDIResultadosRelevantes = "http://w3id.org/roh/relevantResults";
        public const string estanciasIDIPalabrasClave = "http://vivoweb.org/ontology/core#freeTextKeyword@@@http://w3id.org/roh/CategoryPath|http://w3id.org/roh/categoryNode";
        public const string estanciasIDIFacultadEscuela = "http://w3id.org/roh/center";
        public const string estanciasIDIFechaFinalizacion = "http://vivoweb.org/ontology/core#end";

        /// <summary>
        /// Ayudas y becas obtenidas - 060.030.010.000
        /// </summary>
        public const string ayudasBecasNombre = "http://w3id.org/roh/title";
        public const string ayudasBecasPaisConcede = "https://www.w3.org/2006/vcard/ns#hasCountryName";
        public const string ayudasBecasCCAAConcede = "https://www.w3.org/2006/vcard/ns#hasRegion";
        public const string ayudasBecasCiudadConcede = "https://www.w3.org/2006/vcard/ns#locality";
        public const string ayudasBecasPalabrasClave = "http://vivoweb.org/ontology/core#freeTextKeyword@@@http://w3id.org/roh/CategoryPath|http://w3id.org/roh/categoryNode";
        public const string ayudasBecasFinalidad = "http://w3id.org/roh/aims";
        public const string ayudasBecasFinalidadOtros = "http://w3id.org/roh/aimsOther";
        public const string ayudasBecasEntidadConcede = "http://w3id.org/roh/awardingEntity";
        public const string ayudasBecasEntidadConcedeNombre = "http://w3id.org/roh/awardingEntityTitle";
        public const string ayudasBecasTipoEntidadConcede = "http://w3id.org/roh/awardingEntityType";
        public const string ayudasBecasTipoEntidadConcedeOtros = "http://w3id.org/roh/awardingEntityTypeOther";
        public const string ayudasBecasImporte = "http://w3id.org/roh/monetaryAmount";
        public const string ayudasBecasFechaConcesion = "http://vivoweb.org/ontology/core#start";
        public const string ayudasBecasDuracionAnio = "http://w3id.org/roh/durationYears";
        public const string ayudasBecasDuracionMes = "http://w3id.org/roh/durationMonths";
        public const string ayudasBecasDuracionDia = "http://w3id.org/roh/durationDays";
        public const string ayudasBecasFechaFinalizacion = "http://vivoweb.org/ontology/core#end";
        public const string ayudasBecasFacultadEscuela = "http://w3id.org/roh/center";
        public const string ayudasBecasEntidadRealizacionNombre = "http://w3id.org/roh/entityTitle";
        public const string ayudasBecasEntidadRealizacion = "http://w3id.org/roh/entity";

        /// <summary>
        /// Otros modos de colaboración con investigadores/as o tecnólogos/as - 060.020.020.000
        /// </summary>
        public const string otrasColabModoRelacion = "http://w3id.org/roh/relationshipType";
        public const string otrasColabModoRelacionOtros = "http://w3id.org/roh/relationshipTypeOther";
        public const string otrasColabPaisRadicacion = "https://www.w3.org/2006/vcard/ns#hasCountryName";
        public const string otrasColabCCAARadicacion = "https://www.w3.org/2006/vcard/ns#hasRegion";
        public const string otrasColabCiudadRadicacion = "https://www.w3.org/2006/vcard/ns#locality";
        public const string otrasColabNombreInvestigador = "http://w3id.org/roh/researchers@@@http://purl.obolibrary.org/obo/BFO_0000023|http://xmlns.com/foaf/0.1/firstName";
        public const string otrasColabPrimApellInvestigador = "http://w3id.org/roh/researchers@@@http://purl.obolibrary.org/obo/BFO_0000023|http://xmlns.com/foaf/0.1/familyName";
        public const string otrasColabSegApellInvestigador = "http://w3id.org/roh/researchers@@@http://purl.obolibrary.org/obo/BFO_0000023|http://w3id.org/roh/secondFamilyName";
        public const string otrasColabFirmaInvestigador = "http://w3id.org/roh/researchers@@@http://purl.obolibrary.org/obo/BFO_0000023|http://xmlns.com/foaf/0.1/nick";
        public const string otrasColabOrdenInvestigador = "http://w3id.org/roh/researchers@@@http://purl.obolibrary.org/obo/BFO_0000023|http://www.w3.org/1999/02/22-rdf-syntax-ns#comment";
        public const string otrasColabPaisEntidadParticipante = "http://w3id.org/roh/participates@@@http://w3id.org/roh/Organization|https://www.w3.org/2006/vcard/ns#hasCountryName";
        public const string otrasColabCCAAEntidadParticipante = "http://w3id.org/roh/participates@@@http://w3id.org/roh/Organization|https://www.w3.org/2006/vcard/ns#hasRegion";
        public const string otrasColabCiudadEntidadParticipante = "http://w3id.org/roh/participates@@@http://w3id.org/roh/Organization|https://www.w3.org/2006/vcard/ns#locality";
        public const string otrasColabEntidadesParticipantesNombre = "http://w3id.org/roh/participates@@@http://w3id.org/roh/Organization|http://w3id.org/roh/organizationTitle";
        public const string otrasColabEntidadesParticipantes = "http://w3id.org/roh/participates@@@http://w3id.org/roh/Organization|http://w3id.org/roh/organization";
        public const string otrasColabTipoEntidad = "http://w3id.org/roh/participates@@@http://w3id.org/roh/Organization|http://w3id.org/roh/organizationType";
        public const string otrasColabTipoEntidadOtros = "http://w3id.org/roh/participates@@@http://w3id.org/roh/Organization|http://w3id.org/roh/organizationTypeOther";
        public const string otrasColabFechaInicio = "http://vivoweb.org/ontology/core#start";
        public const string otrasColabDuracionAnio = "http://w3id.org/roh/durationYears";
        public const string otrasColabDuracionMes = "http://w3id.org/roh/durationMonths";
        public const string otrasColabDuracionDia = "http://w3id.org/roh/durationDays";
        public const string otrasColabDescripcionColaboracion = "http://w3id.org/roh/title";
        public const string otrasColabResultadosRelevantes = "http://w3id.org/roh/relevantResults";
        public const string otrasColabPalabrasClave = "http://vivoweb.org/ontology/core#freeTextKeyword@@@http://w3id.org/roh/CategoryPath|http://w3id.org/roh/categoryNode";

        /// <summary>
        /// Sociedades científicas y asociaciones profesionales - 060.030.020.000
        /// </summary>
        public const string sociedadesNombre = "http://w3id.org/roh/title";
        public const string sociedadesPaisRadicacion = "https://www.w3.org/2006/vcard/ns#hasCountryName";
        public const string sociedadesCCAARadicacion = "https://www.w3.org/2006/vcard/ns#hasRegion";
        public const string sociedadesCiudadRadicacion = "https://www.w3.org/2006/vcard/ns#locality";
        public const string sociedadesPaisEntidadAfiliacion = "http://w3id.org/roh/affiliatedOrganizationHasCountryName";
        public const string sociedadesCCAAEntidadAfiliacion = "http://w3id.org/roh/affiliatedOrganizationHasRegion";
        public const string sociedadesCiudadEntidadAfiliacion = "http://w3id.org/roh/affiliatedOrganizationLocality";
        public const string sociedadesEntidadAfiliacion = "http://vivoweb.org/ontology/core#affiliatedOrganization";
        public const string sociedadesEntidadAfiliacionNombre = "http://w3id.org/roh/affiliatedOrganizationTitle";
        public const string sociedadesTipoEntidadAfiliacion = "http://w3id.org/roh/affiliatedOrganizationType";
        public const string sociedadesTipoEntidadAfiliacionOtros = "http://w3id.org/roh/affiliatedOrganizationTypeOther";
        public const string sociedadesPalabrasClave = "http://vivoweb.org/ontology/core#freeTextKeyword@@@http://w3id.org/roh/CategoryPath|http://w3id.org/roh/categoryNode";
        public const string sociedadesCategoriaProfesional = "http://w3id.org/roh/professionalCategory";
        public const string sociedadesTamanio = "http://w3id.org/roh/members";
        public const string sociedadesFechaInicio = "http://vivoweb.org/ontology/core#start";
        public const string sociedadesFechaFinalizacion = "http://vivoweb.org/ontology/core#end";

        /// <summary>
        /// Consejos editoriales - 060.030.030.000
        /// </summary>
        public const string consejosNombre = "http://w3id.org/roh/title";
        public const string consejosPaisRadicacion = "https://www.w3.org/2006/vcard/ns#hasCountryName";
        public const string consejosCCAARadicacion = "https://www.w3.org/2006/vcard/ns#hasRegion";
        public const string consejosCiudadRadicacion = "https://www.w3.org/2006/vcard/ns#locality";
        public const string consejosPaisEntidadAfiliacion = "http://w3id.org/roh/affiliatedOrganizationHasCountryName";
        public const string consejosEntidadAfiliacion = "http://vivoweb.org/ontology/core#affiliatedOrganization";
        public const string consejosEntidadAfiliacionNombre = "http://w3id.org/roh/affiliatedOrganizationTitle";
        public const string consejosCCAAEntidadAfiliacion = "http://w3id.org/roh/affiliatedOrganizationHasRegion";
        public const string consejosCiudadEntidadAfiliacion = "http://w3id.org/roh/affiliatedOrganizationLocality";
        public const string consejosTipoEntidadAfiliacion = "http://w3id.org/roh/affiliatedOrganizationType";
        public const string consejosTipoEntidadAfiliacionOtros = "http://w3id.org/roh/affiliatedOrganizationTypeOther";
        public const string consejosTareasDesarrolladas = "http://w3id.org/roh/performedTasks";
        public const string consejosCategoriaProfesional = "http://w3id.org/roh/professionalCategory";
        public const string consejosTamanioSociedad = "http://w3id.org/roh/members";
        public const string consejosAmbito = "http://vivoweb.org/ontology/core#geographicFocus";
        public const string consejosAmbitoOtros = "http://w3id.org/roh/geographicFocusOther";
        public const string consejosFechaInicio = "http://vivoweb.org/ontology/core#start";
        public const string consejosFechaInicioDuracionAnio = "http://w3id.org/roh/durationYears";
        public const string consejosFechaInicioDuracionMes = "http://w3id.org/roh/durationMonths";
        public const string consejosFechaInicioDuracionDia = "http://w3id.org/roh/durationDays";

        /// <summary>
        /// Redes de cooperación - 060.030.040.000
        /// </summary>
        public const string redesCoopNombre = "http://w3id.org/roh/title";
        public const string redesCoopIdentificacion = "http://w3id.org/roh/identification";
        public const string redesCoopNumInvestigadores = "http://w3id.org/roh/members";
        public const string redesCoopPaisRadicacion = "https://www.w3.org/2006/vcard/ns#hasCountryName";
        public const string redesCoopCCAARadicacion = "https://www.w3.org/2006/vcard/ns#hasRegion";
        public const string redesCoopCiudadRadicacion = "https://www.w3.org/2006/vcard/ns#locality";
        public const string redesCoopEntidadSeleccionNombre = "http://w3id.org/roh/selectionEntityTitle";
        public const string redesCoopEntidadSeleccion = "http://w3id.org/roh/selectionEntity";
        public const string redesCoopTipoEntidadSeleccion = "http://w3id.org/roh/selectionEntityType";
        public const string redesCoopTipoEntidadSeleccionOtros = "http://w3id.org/roh/selectionEntityTypeOther";
        public const string redesCoopPaisEntidadSeleccion = "http://w3id.org/roh/selectionEntityHasCountryName";
        public const string redesCoopCCAAEntidadSeleccion = "http://w3id.org/roh/selectionEntityHasRegion";
        public const string redesCoopCiudadEntidadSeleccion = "http://w3id.org/roh/selectionEntityLocality";
        public const string redesCoopEntidadParticipante = "http://w3id.org/roh/participates@@@http://w3id.org/roh/Organization|http://w3id.org/roh/organization";
        public const string redesCoopEntidadParticipanteNombre = "http://w3id.org/roh/participates@@@http://w3id.org/roh/Organization|http://w3id.org/roh/organizationTitle";
        public const string redesCoopTipoEntidadParticipante = "http://w3id.org/roh/participates@@@http://w3id.org/roh/Organization|http://w3id.org/roh/organizationType";
        public const string redesCoopTipoEntidadParticipanteOtros = "http://w3id.org/roh/participates@@@http://w3id.org/roh/Organization|http://w3id.org/roh/organizationTypeOther";
        public const string redesCoopTareas = "http://w3id.org/roh/performedTasks";
        public const string redesCoopFechaInicio = "http://vivoweb.org/ontology/core#start";
        public const string redesCoopDuracionDias = "http://w3id.org/roh/durationDays";
        public const string redesCoopDuracionMes = "http://w3id.org/roh/durationMonths";
        public const string redesCoopDuracionAnio = "http://w3id.org/roh/durationYears";

        /// <summary>
        /// Premios, menciones y distinciones - 060.030.050.000
        /// </summary>
        public const string premiosMencionesDescripcion = "http://w3id.org/roh/title";
        public const string premiosMencionesPais = "https://www.w3.org/2006/vcard/ns#hasCountryName";
        public const string premiosMencionesCCAA = "https://www.w3.org/2006/vcard/ns#hasRegion";
        public const string premiosMencionesCiudad = "https://www.w3.org/2006/vcard/ns#locality";
        public const string premiosMencionesEntidadNombre = "http://w3id.org/roh/accreditationIssuedByTitle";
        public const string premiosMencionesEntidad = "http://w3id.org/roh/accreditationIssuedBy";
        public const string premiosMencionesTipoEntidad = "http://w3id.org/roh/organizationType";
        public const string premiosMencionesTipoEntidadOtros = "http://w3id.org/roh/organizationTypeOther";
        public const string premiosMencionesReconocimientosLigados = "http://w3id.org/roh/recognitionLinked";
        public const string premiosMencionesFechaConcesion = "http://w3id.org/roh/dateIssued";

        /// <summary>
        /// Otras distinciones (carrera profesional y/o empresarial) - 060.030.060.000
        /// </summary>
        public const string otrasDistincionesDescripcion = "http://w3id.org/roh/title";
        public const string otrasDistincionesPais = "https://www.w3.org/2006/vcard/ns#hasCountryName";
        public const string otrasDistincionesCCAA = "https://www.w3.org/2006/vcard/ns#hasRegion";
        public const string otrasDistincionesCiudad = "https://www.w3.org/2006/vcard/ns#locality";
        public const string otrasDistincionesEntidadNombre = "http://w3id.org/roh/accreditationIssuedByTitle";
        public const string otrasDistincionesEntidad = "http://w3id.org/roh/accreditationIssuedBy";
        public const string otrasDistincionesTipoEntidad = "http://w3id.org/roh/organizationType";
        public const string otrasDistincionesTipoEntidadOtros = "http://w3id.org/roh/organizationTypeOther";
        public const string otrasDistincionesAmbito = "http://vivoweb.org/ontology/core#geographicFocus";
        public const string otrasDistincionesAmbitoOtros = "http://w3id.org/roh/geographicFocusOther";
        public const string otrasDistincionesFechaConcesion = "http://w3id.org/roh/dateIssued";

        /// <summary>
        /// Periodos de actividad investigadora - 060.030.070.000
        /// </summary>
        public const string actividadInvestigadoraNumeroTramos = "http://w3id.org/roh/recognizedPeriods";
        public const string actividadInvestigadoraPaisEntidad = "https://www.w3.org/2006/vcard/ns#hasCountryName";
        public const string actividadInvestigadoraCCAAEntidad = "https://www.w3.org/2006/vcard/ns#hasRegion";
        public const string actividadInvestigadoraCiudadEntidad = "https://www.w3.org/2006/vcard/ns#locality";
        public const string actividadInvestigadoraEntidadNombre = "http://w3id.org/roh/accreditationIssuedByTitle";
        public const string actividadInvestigadoraEntidad = "http://w3id.org/roh/accreditationIssuedBy";
        public const string actividadInvestigadoraTipoEntidad = "http://w3id.org/roh/organizationType";
        public const string actividadInvestigadoraTipoEntidadOtros = "http://w3id.org/roh/organizationTypeOther";
        public const string actividadInvestigadoraAmbito = "http://vivoweb.org/ontology/core#geographicFocus";
        public const string actividadInvestigadoraAmbitoOtros = "http://w3id.org/roh/geographicFocusOther";
        public const string actividadInvestigadoraFechaObtencion = "http://w3id.org/roh/dateIssued";

        /// <summary>
        /// Acreditaciones/reconocimientos obtenidos - 060.030.090.000
        /// </summary>
        public const string acreditacionesDescripcion = "http://w3id.org/roh/title";
        public const string acreditacionesPaisEntidad = "https://www.w3.org/2006/vcard/ns#hasCountryName";
        public const string acreditacionesCCAAEntidad = "https://www.w3.org/2006/vcard/ns#hasRegion";
        public const string acreditacionesCiudadEntidad = "https://www.w3.org/2006/vcard/ns#locality";
        public const string acreditacionesFechaObtencion = "http://w3id.org/roh/dateIssued";
        public const string acreditacionesEntidadNombre = "http://w3id.org/roh/accreditationIssuedByTitle";
        public const string acreditacionesEntidad = "http://w3id.org/roh/accreditationIssuedBy";
        public const string acreditacionesTipoEntidad = "http://w3id.org/roh/organizationType";
        public const string acreditacionesTipoEntidadOtros = "http://w3id.org/roh/organizationTypeOther";
        public const string acreditacionesNumeroTramos = "http://w3id.org/roh/recognizedPeriods";
        public const string acreditacionesFechaReconocimiento = "http://w3id.org/roh/receptionDate";

        /// <summary>
        /// Resumen de otros méritos - 060.030.100.000
        /// </summary>
        public const string otrosMeritosTextoLibre = "http://w3id.org/roh/title";
        public const string otrosMeritosPaisEntidad = "https://www.w3.org/2006/vcard/ns#hasCountryName";
        public const string otrosMeritosCCAAEntidad = "https://www.w3.org/2006/vcard/ns#hasRegion";
        public const string otrosMeritosCiudadEntidad = "https://www.w3.org/2006/vcard/ns#locality";
        public const string otrosMeritosEntidadNombre = "http://w3id.org/roh/accreditationIssuedByTitle";
        public const string otrosMeritosEntidad = "http://w3id.org/roh/accreditationIssuedBy";
        public const string otrosMeritosTipoEntidad = "http://w3id.org/roh/organizationType";
        public const string otrosMeritosTipoEntidadOtros = "http://w3id.org/roh/organizationTypeOther";
        public const string otrosMeritosFechaConcesion = "http://w3id.org/roh/dateIssued";

    }
}
