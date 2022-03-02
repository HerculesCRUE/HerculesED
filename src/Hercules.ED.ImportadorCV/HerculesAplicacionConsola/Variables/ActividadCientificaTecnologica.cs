using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HerculesAplicacionConsola.Variables
{
    class ActividadCientificaTecnologica
    {
        /// <summary>
        /// Indicadores generales de calidad de la producción científica - 060.010.060.000
        /// </summary>
        public const string indicadoresGeneralesCalidad = "http://w3id.org/roh/generalQualityIndicator";

        /// <summary>
        /// Publicaciones, documentos científicos y técnicos - 060.010.010.000
        /// <summary>
        public const string pubDocumentosTipoProd = "http://purl.org/dc/elements/1.1/type";
        public const string pubDocumentosTipoProdOtros = "http://w3id.org/roh/typeOthers";
        //public const string pubDocumentosAutorNombre = "";
        //public const string pubDocumentosAutorPrimerApellido = "";
        //public const string pubDocumentosAutorSegundoApellido = "";
        public const string pubDocumentosAutorFirma = "http://purl.org/ontology/bibo/authorList@@@http://purl.obolibrary.org/obo/BFO_0000023|http://xmlns.com/foaf/0.1/nick";
        public const string pubDocumentosPosicion = "";
        public const string pubDocumentosNumAutores = "";
        public const string pubDocumentosGradoContribucion = "http://w3id.org/roh/contributionGrade";
        public const string pubDocumentosTipoSoporte = "http://w3id.org/roh/supportType";
        public const string pubDocumentosPubTitulo = "http://w3id.org/roh/title";
        public const string pubDocumentosPubMainDoc = "http://vivoweb.org/ontology/core#hasPublicationVenue";
        public const string pubDocumentosPubNombre = "http://w3id.org/roh/hasPublicationVenueText";
        public const string pubDocumentosPubVolumen = "http://purl.org/ontology/bibo/volume";
        public const string pubDocumentosPubNumero = "http://purl.org/ontology/bibo/issue";
        public const string pubDocumentosPubPagIni = "http://purl.org/ontology/bibo/pageStart";
        public const string pubDocumentosPubPagFin = "http://purl.org/ontology/bibo/pageEnd";
        //public const string pubDocumentosPubEditorial = ""; //TODO ¿?
        public const string pubDocumentosPubPais = "https://www.w3.org/2006/vcard/ns#hasCountryName";
        public const string pubDocumentosPubCCAA = "https://www.w3.org/2006/vcard/ns#hasRegion";
        public const string pubDocumentosPubFecha = "http://purl.org/dc/terms/issued";
        public const string pubDocumentosPubURL = "https://www.w3.org/2006/vcard/ns#url";
        public const string pubDocumentosPubISBN = "http://w3id.org/roh/isbn";
        public const string pubDocumentosPubDepositoLegal = "http://w3id.org/roh/legalDeposit";        
        public const string pubDocumentosPubCiudad = "https://www.w3.org/2006/vcard/ns#locality";
        //public const string pubDocumentosCategoria = "";
        public const string pubDocumentosColeccion = "http://w3id.org/roh/collection";
        public const string pubDocumentosResultadosDestacados = "http://w3id.org/roh/relevantResults";
        public const string pubDocumentosPubRelevante = "http://w3id.org/roh/relevantPublication";
        public const string pubDocumentosReseniaRevista = "http://w3id.org/roh/reviewsNumber";
        public const string pubDocumentosTraduccion = "https://www.w3.org/2006/vcard/ns#hasLanguage";
        public const string pubDocumentosAutorCorrespondencia = "";
        public const string pubDocumentosIDPubDigitalDOI = "http://purl.org/ontology/bibo/doi";
        public const string pubDocumentosIDPubDigitalHandle = "http://purl.org/ontology/bibo/handle";
        public const string pubDocumentosIDPubDigitalPMID = "http://purl.org/ontology/bibo/pmid";
        public const string pubDocumentosIDOtroPubDigital = "http://purl.org/ontology/bibo/identifier@@@http://xmlns.com/foaf/0.1/Document|http://purl.org/dc/elements/1.1/title";
        public const string pubDocumentosNombreOtroPubDigital = "http://purl.org/ontology/bibo/identifier@@@http://xmlns.com/foaf/0.1/Document|http://xmlns.com/foaf/0.1/topic";
        
        /// <summary>
        /// Trabajos presentados en congresos nacionales o internacionales - 060.010.020.000
        /// <summary>
        public const string trabajosCongresosTipoEvento = "http://purl.org/ontology/bibo/presentedAt@@@http://purl.org/ontology/bibo/Event|http://purl.org/dc/elements/1.1/type";
        public const string trabajosCongresosTipoEventoOtros = "";//TODO
        public const string trabajosCongresosTitulo = "http://w3id.org/roh/title";
        //public const string trabajosCongresosAutores = "http://purl.org/ontology/bibo/authorList@@@http://purl.obolibrary.org/obo/BFO_0000023|http://xmlns.com/foaf/0.1/nick";
        //public const string trabajosCongresosAutorNombre = "";
        //public const string trabajosCongresosAutorPrimerApellido = "";
        //public const string trabajosCongresosAutorSegundoApellido = "";
        public const string trabajosCongresosAutorFirma = "";
        public const string trabajosCongresosTipoParticipacion = "";
        public const string trabajosCongresosIntervencion = "";
        public const string trabajosCongresosIntervencionOtros = "";
        public const string trabajosCongresosAmbitoGeo = "http://purl.org/ontology/bibo/presentedAt@@@http://purl.org/ontology/bibo/Event|http://vivoweb.org/ontology/core#geographicFocus";
        public const string trabajosCongresosAmbitoGeoOtros = "http://purl.org/ontology/bibo/presentedAt@@@http://purl.org/ontology/bibo/Event|http://vivoweb.org/ontology/core#geographicFocusOther";
        public const string trabajosCongresosNombreCongreso = "http://purl.org/ontology/bibo/presentedAt@@@http://purl.org/ontology/bibo/Event|http://purl.org/dc/elements/1.1/title";
        public const string trabajosCongresosPaisCongreso = "";
        public const string trabajosCongresosCCAACongreso = "";
        public const string trabajosCongresosCiudadCongreso = "";
        public const string trabajosCongresosEntidadOrganizadora = "http://purl.org/ontology/bibo/presentedAt@@@http://purl.org/ontology/bibo/Event|http://purl.org/ontology/bibo/organizer";
        public const string trabajosCongresosTipoEntidadOrganizadora = "";
        public const string trabajosCongresosTipoEntidadOrganizadoraOtros = "";
        public const string trabajosCongresosPaisCelebracion = "http://purl.org/ontology/bibo/presentedAt@@@http://purl.org/ontology/bibo/Event|https://www.w3.org/2006/vcard/ns#hasCountryName";
        public const string trabajosCongresosCCAACelebracion = "http://purl.org/ontology/bibo/presentedAt@@@http://purl.org/ontology/bibo/Event|https://www.w3.org/2006/vcard/ns#hasRegion";
        public const string trabajosCongresosCiudadCelebracion = "http://purl.org/ontology/bibo/presentedAt@@@http://purl.org/ontology/bibo/Event|https://www.w3.org/2006/vcard/ns#locality";
        public const string trabajosCongresosFechaCelebracion = "http://purl.org/ontology/bibo/presentedAt@@@http://purl.org/ontology/bibo/Event|http://vivoweb.org/ontology/core#start";
        public const string trabajosCongresosPubActa = "http://w3id.org/roh/congressProceedingsPublication";
        public const string trabajosCongresosComiteExterno = "http://purl.org/ontology/bibo/presentedAt@@@http://purl.org/ontology/bibo/Event|http://w3id.org/roh/withExternalAdmissionsCommittee";
        public const string trabajosCongresosFormaContribucion = "";
        public const string trabajosCongresosPubTitulo = "http://w3id.org/roh/publicationTitle";
        public const string trabajosCongresosPubNombre = "http://w3id.org/roh/hasPublicationVenueText";
        public const string trabajosCongresosPubVolumen = "http://purl.org/ontology/bibo/volume";
        public const string trabajosCongresosPubNumero = "http://purl.org/ontology/bibo/issue";
        public const string trabajosCongresosPubPagIni = "http://purl.org/ontology/bibo/pageStart";
        public const string trabajosCongresosPubPagFin = "http://purl.org/ontology/bibo/pageEnd";
        //public const string trabajosCongresosPubEditorial = ""; //TODO ¿?
        public const string trabajosCongresosPubPais = "https://www.w3.org/2006/vcard/ns#hasCountryName";
        public const string trabajosCongresosPubCCAA = "https://www.w3.org/2006/vcard/ns#hasRegion";
        public const string trabajosCongresosPubFecha = "http://purl.org/dc/terms/issued";
        public const string trabajosCongresosPubURL = "https://www.w3.org/2006/vcard/ns#url";
        public const string trabajosCongresosPubISBN = "http://w3id.org/roh/isbn";
        public const string trabajosCongresosPubDepositoLegal = "http://w3id.org/roh/legalDeposit";
        public const string trabajosCongresosFechaFin = "http://purl.org/ontology/bibo/presentedAt@@@http://purl.org/ontology/bibo/Event|http://vivoweb.org/ontology/core#end";
        public const string trabajosCongresosAutoCorrespondencia = "";
        public const string trabajosCongresosIDPubDigitalDOI = "http://purl.org/ontology/bibo/doi";
        public const string trabajosCongresosIDPubDigitalHandle = "http://purl.org/ontology/bibo/handle";
        public const string trabajosCongresosIDPubDigitalPMID = "http://purl.org/ontology/bibo/pmid";
        public const string trabajosCongresosIDOtroPubDigital = "http://purl.org/ontology/bibo/identifier@@@http://xmlns.com/foaf/0.1/Document|http://purl.org/dc/elements/1.1/title";
        public const string trabajosCongresosNombreOtroPubDigital = "http://purl.org/ontology/bibo/identifier@@@http://xmlns.com/foaf/0.1/Document|http://xmlns.com/foaf/0.1/topic";
        
        
        /// <summary>
        /// Trabajos presentados en jornadas, seminarios, talleres de trabajo y/o cursos nacionales o internacionales - 060.010.030.000
        /// <summary>
        public const string trabajosJornSemTituloTrabajo = "http://w3id.org/roh/title";
        public const string trabajosJornSemTipoEvento = "http://purl.org/ontology/bibo/presentedAt@@@http://purl.org/ontology/bibo/Event|http://purl.org/dc/elements/1.1/type";
        public const string trabajosJornSemTipoEventoOtros = ""; //TODO
        //public const string trabajosJornSemAutorNombre = "";
        //public const string trabajosJornSemAutorPrimerApellido = "";
        //public const string trabajosJornSemAutorSegundoApellido = "";
        public const string trabajosJornSemAutorFirma = "";
        public const string trabajosJornSemIntervencion = "";
        public const string trabajosJornSemAmbitoGeo = "http://purl.org/ontology/bibo/presentedAt@@@http://purl.org/ontology/bibo/Event|http://vivoweb.org/ontology/core#geographicFocus";
        public const string trabajosJornSemAmbitoGeoOtros = "http://purl.org/ontology/bibo/presentedAt@@@http://purl.org/ontology/bibo/Event|http://w3id.org/roh/geographicFocusOther";
        public const string trabajosJornSemNombreEvento = "http://purl.org/ontology/bibo/presentedAt@@@http://purl.org/ontology/bibo/Event|http://purl.org/dc/elements/1.1/title";
        public const string trabajosJornSemPaisEntidadOrganizadora = "";
        public const string trabajosJornSemCCAAEntidadOrganizadora = "";
        public const string trabajosJornSemCiudadEntidadOrganizadora = "";
        public const string trabajosJornSemEntidadOrganizadora = "http://purl.org/ontology/bibo/presentedAt@@@http://purl.org/ontology/bibo/Event|http://purl.org/ontology/bibo/organizer";
        public const string trabajosJornSemTipoEntidadOrganizadora = "";
        public const string trabajosJornSemTipoEntidadOrganizadoraOtros = "";
        public const string trabajosJornSemPaisCelebracion = "http://purl.org/ontology/bibo/presentedAt@@@http://purl.org/ontology/bibo/Event|https://www.w3.org/2006/vcard/ns#hasCountryName";
        public const string trabajosJornSemCCAACelebracion = "http://purl.org/ontology/bibo/presentedAt@@@http://purl.org/ontology/bibo/Event|https://www.w3.org/2006/vcard/ns#hasRegion";
        public const string trabajosJornSemCiudadCelebracion = "http://purl.org/ontology/bibo/presentedAt@@@http://purl.org/ontology/bibo/Event|https://www.w3.org/2006/vcard/ns#locality";
        public const string trabajosJornSemFechaCelebracion = "http://purl.org/ontology/bibo/presentedAt@@@http://purl.org/ontology/bibo/Event|http://vivoweb.org/ontology/core#start";
        public const string trabajosJornSemPubActaCongreso = "http://w3id.org/roh/congressProceedingsPublication";
        public const string trabajosJornSemPubActaCongresoExterno = "http://purl.org/ontology/bibo/presentedAt@@@http://purl.org/ontology/bibo/Event|http://w3id.org/roh/withExternalAdmissionsCommittee";
        public const string trabajosJornSemPubTipo = "";
        public const string trabajosJornSemPubTitulo = "http://w3id.org/roh/publicationTitle";
        public const string trabajosJornSemPubNombre = "";
        public const string trabajosJornSemPubVolumen = "http://purl.org/ontology/bibo/volume";
        public const string trabajosJornSemPubNumero = "http://purl.org/ontology/bibo/issue";
        public const string trabajosJornSemPubPagIni = "http://purl.org/ontology/bibo/pageStart";
        public const string trabajosJornSemPubPagFin = "http://purl.org/ontology/bibo/pageEnd";
        //public const string trabajosJornSemPubEditorial = ""; //TODO ¿?
        public const string trabajosJornSemPubPais = "https://www.w3.org/2006/vcard/ns#hasCountryName";
        public const string trabajosJornSemPubCCAA = "https://www.w3.org/2006/vcard/ns#hasRegion";
        public const string trabajosJornSemPubFecha = "http://purl.org/dc/terms/issued";
        public const string trabajosJornSemPubURL = "https://www.w3.org/2006/vcard/ns#url";
        public const string trabajosJornSemPubISBN = "";
        public const string trabajosJornSemPubDepositoLegal = "http://w3id.org/roh/legalDeposit";
        public const string trabajosJornSemPubFechaFinCelebracion = "http://purl.org/ontology/bibo/presentedAt@@@http://purl.org/ontology/bibo/Event|http://vivoweb.org/ontology/core#end";
        public const string trabajosJornSemAutorCorrespondencia = "";
        public const string trabajosJornSemIDPubDigitalDOI = "http://purl.org/ontology/bibo/doi";
        public const string trabajosJornSemIDPubDigitalHandle = "http://purl.org/ontology/bibo/handle";
        public const string trabajosJornSemIDPubDigitalPMID = "http://purl.org/ontology/bibo/pmid";
        public const string trabajosJornSemNombreOtroPubDigital = "http://purl.org/ontology/bibo/identifier@@@http://xmlns.com/foaf/0.1/Document|http://xmlns.com/foaf/0.1/topic";
        public const string trabajosJornSemIDOtroPubDigital = "http://purl.org/ontology/bibo/identifier@@@http://xmlns.com/foaf/0.1/Document|http://purl.org/dc/elements/1.1/title";
        
        /// <summary>
        /// Otras actividades de divulgación - 060.010.040.000
        /// <summary>
        public const string otrasActDivulTitulo = "http://w3id.org/roh/title";
        public const string otrasActDivulTipoEvento = "http://purl.org/ontology/bibo/presentedAt@@@http://purl.org/ontology/bibo/Event|http://purl.org/dc/elements/1.1/type";
        public const string otrasActDivulTipoEventoOtros = "";//TODO
        //public const string otrasActDivulAutorNombre = "";
        //public const string otrasActDivulAutorPrimerApellido = "";
        //public const string otrasActDivulAutorSegundoApellido = "";
        public const string otrasActDivulAutorFirma = "";
        public const string otrasActDivulIntervencion = "";
        public const string otrasActDivulIntervencionIndicar = "";
        public const string otrasActDivulAmbitoEvento = "http://purl.org/ontology/bibo/presentedAt@@@http://purl.org/ontology/bibo/Event|http://vivoweb.org/ontology/core#geographicFocus";
        public const string otrasActDivulAmbitoEventoOtros = "http://purl.org/ontology/bibo/presentedAt@@@http://purl.org/ontology/bibo/Event|http://w3id.org/roh/geographicFocusOther";
        public const string otrasActDivulNombreEvento = "http://purl.org/ontology/bibo/presentedAt@@@http://purl.org/ontology/bibo/Event|http://purl.org/dc/elements/1.1/title";
        public const string otrasActDivulPaisEntidadOrg = "";
        public const string otrasActDivulCCAAEntidadOrg = "";
        public const string otrasActDivulCiudadEntidadOrg = "";
        public const string otrasActDivulEntidadOrg = "http://purl.org/ontology/bibo/presentedAt@@@http://purl.org/ontology/bibo/Event|http://purl.org/ontology/bibo/organizer";
        public const string otrasActDivulTipoEntidadOrg = "";
        public const string otrasActDivulTipoEntidadOrgOtros = "";
        public const string otrasActDivulPaisCelebracion = "http://purl.org/ontology/bibo/presentedAt@@@http://purl.org/ontology/bibo/Event|https://www.w3.org/2006/vcard/ns#hasCountryName";
        public const string otrasActDivulCCAACelebracion = "http://purl.org/ontology/bibo/presentedAt@@@http://purl.org/ontology/bibo/Event|https://www.w3.org/2006/vcard/ns#hasRegion";
        public const string otrasActDivulCiudadCelebracion = "http://purl.org/ontology/bibo/presentedAt@@@http://purl.org/ontology/bibo/Event|https://www.w3.org/2006/vcard/ns#locality";
        public const string otrasActDivulFechaCelebracion = "http://purl.org/ontology/bibo/presentedAt@@@http://purl.org/ontology/bibo/Event|http://vivoweb.org/ontology/core#start";
        public const string otrasActDivulPubActaCongreso = "http://w3id.org/roh/congressProceedingsPublication";
        public const string otrasActDivulPubActaAdmisionExt = "http://w3id.org/roh/withExternalAdmissionsCommittee";
        public const string otrasActDivulPubTipo = "";
        public const string otrasActDivulPubTitulo = "http://w3id.org/roh/publicationTitle";
        public const string otrasActDivulPubNombre = "";
        public const string otrasActDivulPubVolumen = "http://purl.org/ontology/bibo/volume";
        public const string otrasActDivulPubNumero = "http://purl.org/ontology/bibo/issue";
        public const string otrasActDivulPubPagIni = "http://purl.org/ontology/bibo/pageStart";
        public const string otrasActDivulPubPagFin = "http://purl.org/ontology/bibo/pageEnd";
        //public const string otrasActDivulPubEditorial = "";//TODO ¿?
        public const string otrasActDivulPubPais = "https://www.w3.org/2006/vcard/ns#hasCountryName";
        public const string otrasActDivulPubCCAA = "https://www.w3.org/2006/vcard/ns#hasRegion";
        public const string otrasActDivulPubFecha = "http://purl.org/dc/terms/issued";
        public const string otrasActDivulPubURL = "https://www.w3.org/2006/vcard/ns#url";
        public const string otrasActDivulPubISBN = "";
        public const string otrasActDivulPubDepositoLegal = "http://w3id.org/roh/legalDeposit";
        public const string otrasActDivulAutorCorrespondencia = "";
        public const string otrasActDivulIDPubDigitalDOI = "http://purl.org/ontology/bibo/doi";
        public const string otrasActDivulIDPubDigitalHandle = "http://purl.org/ontology/bibo/handle";
        public const string otrasActDivulIDPubDigitalPMID = "http://purl.org/ontology/bibo/pmid";
        public const string otrasActDivulNombreOtroIDPubDigital = "http://purl.org/ontology/bibo/identifier@@@http://xmlns.com/foaf/0.1/Document|http://xmlns.com/foaf/0.1/topic";
        public const string otrasActDivulIDOtroPubDigital = "http://purl.org/ontology/bibo/identifier@@@http://xmlns.com/foaf/0.1/Document|http://purl.org/dc/elements/1.1/title";
        
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
        public const string comitesCTAEntidadAfiliacion = "";
        public const string comitesCTAEntidadAfiliacionNombre = "";
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
        public const string gestionIDIPalabrasClave = "http://w3id.org/roh/hasKnowledgeArea@@@http://w3id.org/roh/CategoryPath|http://w3id.org/roh/categoryNode";//TODO - revisar

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
        public const string estanciasIDIPalabrasClave = ""; // TODO
        public const string estanciasIDIFacultadEscuela = "http://w3id.org/roh/center";
        public const string estanciasIDIFechaFinalizacion = "http://vivoweb.org/ontology/core#end";

        /// <summary>
        /// Ayudas y becas obtenidas - 060.030.010.000
        /// </summary>
        public const string ayudasBecasNombre = "http://w3id.org/roh/title";
        public const string ayudasBecasPaisConcede = "https://www.w3.org/2006/vcard/ns#hasCountryName";
        public const string ayudasBecasCCAAConcede = "https://www.w3.org/2006/vcard/ns#hasRegion";
        public const string ayudasBecasCiudadConcede = "https://www.w3.org/2006/vcard/ns#locality";
        public const string ayudasBecasPalabrasClave = "";// --- TODO --- 
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
        public const string ayudasBecasEntidadRealizacion = "http://w3id.org/roh/entityTitle";

        /// <summary>
        /// Otros modos de colaboración con investigadores/as o tecnólogos/as - 060.020.020.000
        /// </summary>
        public const string otrasColabModoRelacion = "http://w3id.org/roh/relationshipType";
        public const string otrasColabModoRelacionOtros = "http://w3id.org/roh/relationshipTypeOther";
        public const string otrasColabPaisRadicacion = "https://www.w3.org/2006/vcard/ns#hasCountryName";
        public const string otrasColabCCAARadicacion = "https://www.w3.org/2006/vcard/ns#hasRegion";
        public const string otrasColabCiudadRadicacion = "https://www.w3.org/2006/vcard/ns#locality";
        public const string otrasColabNombreInvestigador = "";// --- TODO --- ?
        public const string otrasColabPrimApellInvestigador = "";// --- TODO --- ?
        public const string otrasColabSegApellInvestigador = "";// --- TODO --- ?
        public const string otrasColabFirmaInvestigador = "http://w3id.org/roh/researchers@@@http://purl.obolibrary.org/obo/BFO_0000023|http://xmlns.com/foaf/0.1/nick";// --- TODO --- ?
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
        public const string otrasColabPalabrasClave = "http://vivoweb.org/ontology/core#freeTextKeyword";// ?

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
        public const string sociedadesPalabrasClave = "";// --- TODO --- 
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
