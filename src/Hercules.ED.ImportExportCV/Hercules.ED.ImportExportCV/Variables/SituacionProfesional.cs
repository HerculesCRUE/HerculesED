namespace ImportadorWebCV.Variables
{
    class SituacionProfesional
    {
        /// <summary>
        /// Situación profesional actual - 010.010.000.000
        /// </summary>
        public const string situacionProfesionalGestionDocente = "http://w3id.org/roh/teachingManagement";
        public const string situacionProfesionalEntidadEmpleadora = "http://w3id.org/roh/employerOrganization";
        public const string situacionProfesionalEntidadEmpleadoraNombre = "http://w3id.org/roh/employerOrganizationTitle";
        public const string situacionProfesionalTipoEntidadEmpleadora = "http://w3id.org/roh/employerOrganizationType";
        public const string situacionProfesionalTipoEntidadEmpleadoraOtros = "http://w3id.org/roh/employerOrganizationTypeOther";
        public const string situacionProfesionalFacultadEscuela = "http://w3id.org/roh/center";
        public const string situacionProfesionalDepartamento = "http://w3id.org/roh/department";
        public const string situacionProfesionalCiudadEntidadEmpleadora = "https://www.w3.org/2006/vcard/ns#locality";
        public const string situacionProfesionalPaisEntidadEmpleadora = "https://www.w3.org/2006/vcard/ns#hasCountryName";
        public const string situacionProfesionalCCAAEntidadEmpleadora = "https://www.w3.org/2006/vcard/ns#hasRegion";
        public const string situacionProfesionalFijoCodInternacional = "https://www.w3.org/2006/vcard/ns#hasTelephone@@@https://www.w3.org/2006/vcard/ns#TelephoneType|http://w3id.org/roh/hasInternationalCode";
        public const string situacionProfesionalFijoNumero = "https://www.w3.org/2006/vcard/ns#hasTelephone@@@https://www.w3.org/2006/vcard/ns#TelephoneType|https://www.w3.org/2006/vcard/ns#hasValue";
        public const string situacionProfesionalFijoExtension = "https://www.w3.org/2006/vcard/ns#hasTelephone@@@https://www.w3.org/2006/vcard/ns#TelephoneType|http://w3id.org/roh/hasExtension";
        public const string situacionProfesionalFaxCodInternacional = "http://w3id.org/roh/hasFax@@@https://www.w3.org/2006/vcard/ns#TelephoneType|http://w3id.org/roh/hasInternationalCode";
        public const string situacionProfesionalFaxNumero = "http://w3id.org/roh/hasFax@@@https://www.w3.org/2006/vcard/ns#TelephoneType|https://www.w3.org/2006/vcard/ns#hasValue";
        public const string situacionProfesionalFaxExtension = "http://w3id.org/roh/hasFax@@@https://www.w3.org/2006/vcard/ns#TelephoneType|http://w3id.org/roh/hasExtension";
        public const string situacionProfesionalCorreoElectronico = "https://www.w3.org/2006/vcard/ns#email";
        public const string situacionProfesionalCategoriaProfesional = "http://w3id.org/roh/professionalCategory";
        public const string situacionProfesionalFechaInicio = "http://vivoweb.org/ontology/core#start";
        public const string situacionProfesionalModalidadContrato = "http://w3id.org/roh/contractModality";
        public const string situacionProfesionalModalidadContratoOtros = "http://w3id.org/roh/contractModalityOther";
        public const string situacionProfesionalRegimenDedicacion = "http://w3id.org/roh/dedication";
        public const string situacionProfesionalCodUnescoPrimaria = "http://w3id.org/roh/unescoPrimary@@@http://w3id.org/roh/CategoryPath|http://w3id.org/roh/categoryNode";
        public const string situacionProfesionalCodUnescoSecundaria = "http://w3id.org/roh/unescoSecondary@@@http://w3id.org/roh/CategoryPath|http://w3id.org/roh/categoryNode";
        public const string situacionProfesionalCodUnescoTerciaria = "http://w3id.org/roh/unescoTertiary@@@http://w3id.org/roh/CategoryPath|http://w3id.org/roh/categoryNode";
        public const string situacionProfesionalFuncionesDesempeñadas = "http://w3id.org/roh/concreteFunctions";
        public const string situacionProfesionalPalabrasClave = "http://vivoweb.org/ontology/core#freeTextKeyword@@@http://w3id.org/roh/CategoryPath|http://w3id.org/roh/categoryNode";
        public const string situacionProfesionalInteresDocencia = "http://purl.org/ontology/bibo/abstract";
        public const string situacionProfesionalAmbitoActividadGestion = "http://w3id.org/roh/scopeManagementActivity";
        public const string situacionProfesionalAmbitoActividadGestionOtros = "http://w3id.org/roh/scopeManagementActivityOther";

        /// <summary>
        /// Cargos y actividades desempeñados con anterioridad - 010.020.000.000
        /// </summary>
        public const string cargosActividadesGestionDocente = "http://w3id.org/roh/teachingManagement";
        public const string cargosActividadesEntidadEmpleadora = "http://w3id.org/roh/employerOrganization";
        public const string cargosActividadesEntidadEmpleadoraNombre = "http://w3id.org/roh/employerOrganizationTitle";
        public const string cargosActividadesTipoEntidadEmpleadora = "http://w3id.org/roh/employerOrganizationType";
        public const string cargosActividadesTipoEntidadEmpleadoraOtros = "http://w3id.org/roh/employerOrganizationTypeOther";
        public const string cargosActividadesFacultadEscuela = "http://w3id.org/roh/center";
        public const string cargosActividadesDepartamento = "http://w3id.org/roh/department";
        public const string cargosActividadesCiudadEntidadEmpleadora = "https://www.w3.org/2006/vcard/ns#locality";
        public const string cargosActividadesPaisEntidadEmpleadora = "https://www.w3.org/2006/vcard/ns#hasCountryName";
        public const string cargosActividadesCCAAEntidadEmpleadora = "https://www.w3.org/2006/vcard/ns#hasRegion";
        public const string cargosActividadesFijoCodInternacional = "https://www.w3.org/2006/vcard/ns#hasTelephone@@@https://www.w3.org/2006/vcard/ns#TelephoneType|http://w3id.org/roh/hasInternationalCode";
        public const string cargosActividadesFijoNumero = "https://www.w3.org/2006/vcard/ns#hasTelephone@@@https://www.w3.org/2006/vcard/ns#TelephoneType|https://www.w3.org/2006/vcard/ns#hasValue";
        public const string cargosActividadesFijoExtension = "https://www.w3.org/2006/vcard/ns#hasTelephone@@@https://www.w3.org/2006/vcard/ns#TelephoneType|http://w3id.org/roh/hasExtension";
        public const string cargosActividadesFaxCodInternacional = "http://w3id.org/roh/hasFax@@@https://www.w3.org/2006/vcard/ns#TelephoneType|http://w3id.org/roh/hasInternationalCode";
        public const string cargosActividadesFaxNumero = "http://w3id.org/roh/hasFax@@@https://www.w3.org/2006/vcard/ns#TelephoneType|https://www.w3.org/2006/vcard/ns#hasValue";
        public const string cargosActividadesFaxExtension = "http://w3id.org/roh/hasFax@@@https://www.w3.org/2006/vcard/ns#TelephoneType|http://w3id.org/roh/hasExtension";
        public const string cargosActividadesCorreoElectronico = "https://www.w3.org/2006/vcard/ns#email";
        public const string cargosActividadesCategoriaProfesional = "http://w3id.org/roh/professionalCategory";
        public const string cargosActividadesFechaInicio = "http://vivoweb.org/ontology/core#start";
        public const string cargosActividadesDuracionAnio = "http://w3id.org/roh/durationYears";
        public const string cargosActividadesDuracionMes = "http://w3id.org/roh/durationMonths";
        public const string cargosActividadesDuracionDia = "http://w3id.org/roh/durationDays";
        public const string cargosActividadesModalidadContrato = "http://w3id.org/roh/contractModality";
        public const string cargosActividadesModalidadContratoOtros = "http://w3id.org/roh/contractModalityOther";
        public const string cargosActividadesRegimenDedicacion = "http://w3id.org/roh/dedication";
        public const string cargosActividadesCodUnescoPrimaria = "http://w3id.org/roh/unescoPrimary@@@http://w3id.org/roh/CategoryPath|http://w3id.org/roh/categoryNode";
        public const string cargosActividadesCodUnescoSecundaria = "http://w3id.org/roh/unescoSecondary@@@http://w3id.org/roh/CategoryPath|http://w3id.org/roh/categoryNode";
        public const string cargosActividadesCodUnescoTerciaria = "http://w3id.org/roh/unescoTertiary@@@http://w3id.org/roh/CategoryPath|http://w3id.org/roh/categoryNode";
        public const string cargosActividadesFuncionesDesempeñadas = "http://w3id.org/roh/concreteFunctions";
        public const string cargosActividadesPalabrasClave = "http://vivoweb.org/ontology/core#freeTextKeyword@@@http://w3id.org/roh/CategoryPath|http://w3id.org/roh/categoryNode";
        public const string cargosActividadesInteresDocencia = "http://purl.org/ontology/bibo/abstract";
        public const string cargosActividadesAmbitoActividadGestion = "http://w3id.org/roh/scopeManagementActivity";
        public const string cargosActividadesAmbitoActividadGestionOtros = "http://w3id.org/roh/scopeManagementActivityOther";
        public const string cargosActividadesFechaFinalizacion = "http://vivoweb.org/ontology/core#end";


    }
}
