using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImportadorWebCV.Variables
{
    class ExperienciaCientificaTecnologica
    {
        /// <summary>
        /// Proyectos de I+D+i financiados en convocatorias competitivas de Administraciones o entidades públicas y privadas - 050.020.010.000
        /// </summary>
        public const string proyectosIDINombre = "http://w3id.org/roh/title";
        public const string proyectosIDIPalabrasClave = "";
        public const string proyectosIDIModalidadProyecto = "http://w3id.org/roh/modality";
        public const string proyectosIDIAmbitoProyecto = "http://vivoweb.org/ontology/core#geographicFocus";
        public const string proyectosIDIAmbitoProyectoOtros = "http://w3id.org/roh/geographicFocusOther";
        public const string proyectosIDIPaisEntidadRealizacion = "";
        public const string proyectosIDICCAAEntidadRealizacion = "";
        public const string proyectosIDICiudadEntidadRealizacion = "";
        public const string proyectosIDIEntidadRealizacion = "http://w3id.org/roh/conductedBy";
        public const string proyectosIDITipoEntidadRealizacion = "";
        public const string proyectosIDITipoEntidadRealizacionOtros = "";
        public const string proyectosIDINombreIP = "";  
        public const string proyectosIDIPrimerApellidoIP = "";
        public const string proyectosIDISegundoApellidoIP = "";
        public const string proyectosIDIFirmaIP = "";
        public const string proyectosIDINumInvestigadores = "http://w3id.org/roh/researchersNumber";
        public const string proyectosIDINumPersonasAnio = "http://w3id.org/roh/researchersNumber";
        public const string proyectosIDIGradoContribucion = "";
        public const string proyectosIDIGradoContribucionOtros = "";
        public const string proyectosIDIPaisEntidadFinancidora = "";
        public const string proyectosIDICCAAEntidadFinancidora = "";
        public const string proyectosIDICiudadEntidadFinancidora = "";
        public const string proyectosIDIEntidadFinancidora = "";
        public const string proyectosIDITipoEntidadFinancidora = "";
        public const string proyectosIDITipoEntidadFinancidoraOtros = "";
        public const string proyectosIDITipoParticipacion = "";
        public const string proyectosIDITipoParticipacionOtros = "";
        public const string proyectosIDINombreProgramaFinanciacion = "";
        public const string proyectosIDICodEntidadFinanciacion = "";
        public const string proyectosIDIFechaInicio = "http://vivoweb.org/ontology/core#start";
        public const string proyectosIDIDuracionAnio = "http://w3id.org/roh/durationYears";
        public const string proyectosIDIDuracionMes = "http://w3id.org/roh/durationMonths";
        public const string proyectosIDIDuracionDia = "http://w3id.org/roh/durationDays";
        public const string proyectosIDICuantiaTotal = "http://w3id.org/roh/monetaryAmount";
        public const string proyectosIDICuantiaSubproyecto = "http://w3id.org/roh/isSupportedBy@@@http://w3id.org/roh/Funding|http://w3id.org/roh/monetaryAmount";
        public const string proyectosIDIPorcentajeSubvencion = "http://w3id.org/roh/isSupportedBy@@@http://w3id.org/roh/Funding|http://w3id.org/roh/grantsPercentage";
        public const string proyectosIDIPorcentajeCredito = "http://w3id.org/roh/isSupportedBy@@@http://w3id.org/roh/Funding|http://w3id.org/roh/creditPercentage";
        public const string proyectosIDIPorcentajeMixto = "http://w3id.org/roh/isSupportedBy@@@http://w3id.org/roh/Funding|http://w3id.org/roh/mixedPercentage";
        public const string proyectosIDIResultadosRelevantes = "http://w3id.org/roh/relevantResults";
        public const string proyectosIDIResultadosRelevantesPalabrasClave = "http://w3id.org/roh/hasProjectClassification@@@http://w3id.org/roh/CategoryPath|http://w3id.org/roh/categoryNode";
        public const string proyectosIDIEntidadesParticipantes = "";
        public const string proyectosIDIFechaFinalizacion = "http://vivoweb.org/ontology/core#end";
        public const string proyectosIDIAportacionSolicitante = "";
        public const string proyectosIDIRegimenDedicacion = "";

        /// <summary>
        /// Contratos, convenios o proyectos de I+D+i no competitivos con Administraciones o entidades públicas o privadas - 050.020.020.000
        /// </summary>
        public const string contratosNombreProyecto = "http://w3id.org/roh/title";
        public const string contratosPalabrasClave = "http://vivoweb.org/ontology/core#freeTextKeyword";
        public const string contratosModalidadProyecto = "http://w3id.org/roh/modality";
        public const string contratosAmbitoProyecto = "http://vivoweb.org/ontology/core#geographicFocus";
        public const string contratosAmbitoProyectoOtros = "http://w3id.org/roh/geographicFocusOther";
        //public const string contratosPaisEntidadRealizacion = "";
        //public const string contratosCCAAEntidadRealizacion = "";
        //public const string contratosCiudadEntidadRealizacion = "";
        public const string contratosEntidadRealizacion = "http://w3id.org/roh/conductedBy";
        //public const string contratosTipoEntidadRealizacion = "";
        //public const string contratosTipoEntidadRealizacionOtros = "";
        //public const string contratosCodEntidadFinanciadora = "";
        //public const string contratosPaisEntidadFinanciadora = "";
        //public const string contratosCCAAEntidadFinanciadora = "";
        //public const string contratosCiudadEntidadFinanciadora = "";
        public const string contratosEntidadFinanciadora = "http://w3id.org/roh/grantedBy";
        //public const string contratosTipoEntidadFinanciadora = "";
        //public const string contratosTipoEntidadFinanciadoraOtros = "";
        public const string contratosTipoProyecto = "http://w3id.org/roh/projectType";
        public const string contratosNombrePrograma = "";
        public const string contratosFechaInicio = "http://vivoweb.org/ontology/core#start";
        public const string contratosDuracionAnio = "http://w3id.org/roh/durationYears";
        public const string contratosDuracionMes = "http://w3id.org/roh/durationMonths";
        public const string contratosDuracionDia = "http://w3id.org/roh/durationDays";
        public const string contratosCuantiaTotal = "http://w3id.org/roh/monetaryAmount";
        public const string contratosCuantiaSubproyecto = "http://w3id.org/roh/isSupportedBy@@@http://w3id.org/roh/Funding|http://w3id.org/roh/monetaryAmount";
        public const string contratosPorcentajeSubvencion = "http://w3id.org/roh/isSupportedBy@@@http://w3id.org/roh/Funding|http://w3id.org/roh/grantsPercentage";
        public const string contratosPorcentajeCredito = "http://w3id.org/roh/isSupportedBy@@@http://w3id.org/roh/Funding|http://w3id.org/roh/creditPercentage";
        public const string contratosPorcentajeMixto = "http://w3id.org/roh/isSupportedBy@@@http://w3id.org/roh/Funding|http://w3id.org/roh/mixedPercentage";
        public const string contratosIPNombre = "";
        public const string contratosIPPrimerApellido = "";
        public const string contratosIPSegundoApellido = "";
        public const string contratosIPFirma = "";//TODO
        public const string contratosNumInvestigadores = "http://w3id.org/roh/researchersNumber";
        public const string contratosNumPersonasAnio = "http://w3id.org/roh/researchersNumber";
        public const string contratosGradoContribucion = "";
        public const string contratosGradoContribucionOtros = "";
        public const string contratosResultadosRelevantes = "http://w3id.org/roh/relevantResults";
        public const string contratosResultadosRelevantesPalabrasClave = "";
        public const string contratosEntidadParticipante = "";

        /// <summary>
        /// Propiedad industrial e intelectual - 050.030.010.000
        /// </summary>
        public const string propIIDescripcion = "http://vivoweb.org/ontology/core#description";
        public const string propIITituloPropIndus = "http://w3id.org/roh/title";
        public const string propIITipoPropIndus = "http://w3id.org/roh/industrialPropertyType";
        public const string propIITipoPropIndusOtros = "http://w3id.org/roh/industrialPropertyTypeOther";
        public const string propIIDerechosAutor = "http://w3id.org/roh/authorsRights";
        public const string propIIDerechosConexos = "http://w3id.org/roh/relatedRights";
        public const string propIISecretoEmpresarial = "http://w3id.org/roh/tradeSecret";
        public const string propIIModalidadKnowHow = "http://w3id.org/roh/knowHow";
        public const string propIIInventoresAutoresNombre = "";
        public const string propIIInventoresAutoresPrimerApellido = "";
        public const string propIIInventoresAutoresSegundoApellido = "";
        public const string propIIInventoresAutoresFirma = "";
        public const string propIICodReferencia = "http://w3id.org/roh/referenceCode";
        public const string propIINumSolicitud = "http://w3id.org/roh/applicationNumber";
        public const string propIIPaisInscripcion = "https://www.w3.org/2006/vcard/ns#hasCountryName";
        public const string propIICCAAInscripcion = "https://www.w3.org/2006/vcard/ns#hasRegion";
        public const string propIIFechaRegistro = "http://vivoweb.org/ontology/core#dateFiled";
        public const string propIIPatenteEsp = "http://w3id.org/roh/spanishPatent";
        public const string propIIPatenteUE = "http://w3id.org/roh/europeanPatent";
        public const string propIIPatenteNoUE = "http://w3id.org/roh/internationalPatent";
        public const string propIIResultadosRelevantes = "http://w3id.org/roh/relevantResults";
        public const string propIIPalabrasClave = "";
        public const string propIILicencias = "http://w3id.org/roh/licenses";
        public const string propIIPaisExplotacion = "";
        public const string propIICCAAExplotacion = "";
        public const string propIIEmpresasExplotacion = "http://w3id.org/roh/ownerOrganization";
        public const string propIIExplotacionExclusiva = "http://w3id.org/roh/exclusiveUse";
        public const string propIIGeneradaEmpresaInnov = "http://w3id.org/roh/innovativeEnterprise";
        public const string propIIResultadoEmpresaInnov = "http://w3id.org/roh/results";
        public const string propIINombreProductos = "http://w3id.org/roh/products";
        public const string propIIEntidadTitularDerechos = "http://w3id.org/roh/ownerOrganization";
        public const string propIINumPatente = "http://w3id.org/roh/patentNumber";
        public const string propIIFechaConcesion = "http://vivoweb.org/ontology/core#dateIssued";
        public const string propIIPatentePCT = "http://w3id.org/roh/pctPatent";

        /// <summary>
        /// Grupos/equipos de investigación, desarrollo o innovación - 050.010.000.000
        /// </summary>
        public const string grupoIDIObjetoGrupo = "http://vivoweb.org/ontology/core#description";
        public const string grupoIDINombreGrupo = "http://w3id.org/roh/title";
        public const string grupoIDICodNormalizado = "http://w3id.org/roh/normalizedCode";
        public const string grupoIDIPaisRadicacion = "";
        public const string grupoIDICCAARadicacion = "";
        public const string grupoIDICiudadRadicacion = "";
        public const string grupoIDINombreIP = "http://w3id.org/roh/mainResearcher";//TODO
        public const string grupoIDIPrimerApellidoIP = "";//TODO
        public const string grupoIDISegundoapellidoIP = "";//TODO
        public const string grupoIDIFirmaIP = "";//TODO
        public const string grupoIDIEntidadAfiliacion = "http://vivoweb.org/ontology/core#affiliatedOrganization";
        public const string grupoIDITipoEntidadAfiliacion = "";
        public const string grupoIDITipoEntidadAfiliacionOtros = "";
        public const string grupoIDINumComponentes = "http://w3id.org/roh/researchersNumber";
        public const string grupoIDIFechaInicio = "";
        public const string grupoIDIDuracionAnio = "";
        public const string grupoIDIDuracionMes = "";
        public const string grupoIDIDuracionDia = "";
        public const string grupoIDIClaseColaboracion = "http://w3id.org/roh/colaborationType";
        public const string grupoIDINumTesisDirigidas = "http://w3id.org/roh/directedThesisNumber";
        public const string grupoIDINumPosDocDirigidos = "http://w3id.org/roh/directedPostdocsNumber";
        public const string grupoIDIResultadosOtros = "http://w3id.org/roh/otherRelevantResults";
        public const string grupoIDIResultadosMasRelevantes = "http://w3id.org/roh/relevantResults";
        public const string grupoIDIPalabrasClave = "http://vivoweb.org/ontology/core#freeTextKeyword";
                
        /// <summary>
        /// Obras artísticas dirigidas - 050.020.030.000
        /// </summary>
        public const string obrasArtisticasDescripcion = "http://vivoweb.org/ontology/core#description";
        public const string obrasArtisticasNombreExpo = "http://w3id.org/roh/title";
        public const string obrasArtisticasAutoresNombre = "";//TODO
        public const string obrasArtisticasAutoresPrimerApellido = "";
        public const string obrasArtisticasAutoresSegundoApellido = "";
        public const string obrasArtisticasAutoresFirma = "";
        public const string obrasArtisticasPaisExpo = "https://www.w3.org/2006/vcard/ns#hasCountryName";
        public const string obrasArtisticasCCAAExpo = "https://www.w3.org/2006/vcard/ns#hasRegion";
        public const string obrasArtisticasCiudadExpo = "https://www.w3.org/2006/vcard/ns#locality";
        public const string obrasArtisticasForoExpo = "http://w3id.org/roh/exhibitingForum";
        public const string obrasArtisticasMonografica = "http://w3id.org/roh/monographic";
        public const string obrasArtisticasCatalogo = "http://w3id.org/roh/catalogue";
        public const string obrasArtisticasComisario = "";
        public const string obrasArtisticasFechaInicio = "http://vivoweb.org/ontology/core#start";
        public const string obrasArtisticasCatalogacion = "http://w3id.org/roh/cataloguing";
        public const string obrasArtisticasPremio = "http://w3id.org/roh/award";
        public const string obrasArtisticasTituloPublicacion = "http://w3id.org/roh/publicationTitle";
        public const string obrasArtisticasTituloPublicacionOtros = "http://w3id.org/roh/others";

        /// <summary>
        /// Resultados tecnológicos derivados de actividades especializadas y de transferencia no incluidos en apartados anteriores - 050.030.020.000
        /// </summary>
        public const string resultadosTecnologicosDescripcion = "http://vivoweb.org/ontology/core#description";
        public const string resultadosTecnologicosCodUnescoPrimaria = "";
        public const string resultadosTecnologicosCodUnescoSecundaria = "";
        public const string resultadosTecnologicosCodUnescoTerciaria = "";
        public const string resultadosTecnologicosIPNombre = "";//TODO
        public const string resultadosTecnologicosIPPrimerApellido = "";
        public const string resultadosTecnologicosIPSegundoApellido = "";
        public const string resultadosTecnologicosIPFirma = "";
        public const string resultadosTecnologicosCoIPNombre = "";
        public const string resultadosTecnologicosCoIPPrimerApellido = "";
        public const string resultadosTecnologicosCoIPSegundoApellido = "";
        public const string resultadosTecnologicosCoIPFirma = "";
        public const string resultadosTecnologicosGradoContribucion = "";
        public const string resultadosTecnologicosGradoContribucionOtros = "";
        public const string resultadosTecnologicosNuevasTecnicasEquip = "";
        public const string resultadosTecnologicosEmpresasSpinOff = "";
        public const string resultadosTecnologicosResultadosMejoraProd = "";
        public const string resultadosTecnologicosHomologos = "";
        public const string resultadosTecnologicosExpertoTecnologico = "";
        public const string resultadosTecnologicosConveniosColab = "";
        public const string resultadosTecnologicosAmbitoActividad = "";
        public const string resultadosTecnologicosAmbitoActividadOtros = "";
        public const string resultadosTecnologicosEntidadDestinataria = "";
        //public const string resultadosTecnologicosTipoEntidadDestinataria = "";
        //public const string resultadosTecnologicosTipoEntidadDestinatariaOtros = "";
        //public const string resultadosTecnologicosPaisEntidadDestinataria = "";
        //public const string resultadosTecnologicosCCAAEntidadDestinataria = "";
        //public const string resultadosTecnologicosCiudadEntidadDestinataria = "";

        public const string resultadosTecnologicosEntidadColaboradora = "http://w3id.org/roh/participates";
        //public const string resultadosTecnologicosTipoEntidadColaboradora = "";
        //public const string resultadosTecnologicosTipoEntidadColaboradoraOtros = "";
        //public const string resultadosTecnologicosPaisEntidadColaboradora = "";
        //public const string resultadosTecnologicosCCAAEntidadColaboradora = "";
        //public const string resultadosTecnologicosCiudadEntidadColaboradora = "";
        public const string resultadosTecnologicosFechaInicio = "http://vivoweb.org/ontology/core#start";
        public const string resultadosTecnologicosDuracionAnio = "http://w3id.org/roh/durationYears";
        public const string resultadosTecnologicosDuracionMes = "http://w3id.org/roh/durationMonths";
        public const string resultadosTecnologicosDuracionDia = "http://w3id.org/roh/durationDays";
        public const string resultadosTecnologicosResultadosRelevantes = "";
        public const string resultadosTecnologicosPalabrasClave = "http://vivoweb.org/ontology/core#freeTextKeyword";
    }
}
