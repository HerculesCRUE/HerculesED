namespace ImportadorWebCV.Variables
{
    class DatosIdentificacion
    {
        /// <summary>
        /// Datos de identificacion
        /// </summary>
        public const string nombre = "http://xmlns.com/foaf/0.1/firstName";
        public const string primerApellido = "http://xmlns.com/foaf/0.1/familyName";
        public const string segundoApellido = "http://w3id.org/roh/secondFamilyName";
        public const string genero = "http://xmlns.com/foaf/0.1/gender";
        public const string nacionalidad = "http://www.schema.org/nationality";
        public const string fechaNacimiento = "https://www.w3.org/2006/vcard/ns#birth-date";
        public const string direccionNacimientoPais = "http://w3id.org/roh/birthplace@@@https://www.w3.org/2006/vcard/ns#Address|https://www.w3.org/2006/vcard/ns#hasCountryName";
        public const string direccionNacimientoRegion = "http://w3id.org/roh/birthplace@@@https://www.w3.org/2006/vcard/ns#Address|https://www.w3.org/2006/vcard/ns#hasRegion";
        public const string direccionNacimientoCiudad = "http://w3id.org/roh/birthplace@@@https://www.w3.org/2006/vcard/ns#Address|https://www.w3.org/2006/vcard/ns#locality";
        public const string dni = "http://w3id.org/roh/dni";
        public const string nie = "http://w3id.org/roh/nie";
        public const string pasaporte = "http://w3id.org/roh/passport";
        public const string imagenDigital = "http://xmlns.com/foaf/0.1/img";
        public const string direccionContacto = "https://www.w3.org/2006/vcard/ns#address@@@https://www.w3.org/2006/vcard/ns#Address|https://www.w3.org/2006/vcard/ns#street-address";
        public const string direccionContactoResto = "https://www.w3.org/2006/vcard/ns#address@@@https://www.w3.org/2006/vcard/ns#Address|https://www.w3.org/2006/vcard/ns#extended-address";
        public const string direccionContactoCodPostal = "https://www.w3.org/2006/vcard/ns#address@@@https://www.w3.org/2006/vcard/ns#Address|https://www.w3.org/2006/vcard/ns#postal-code";
        public const string direccionContactoPais = "https://www.w3.org/2006/vcard/ns#address@@@https://www.w3.org/2006/vcard/ns#Address|https://www.w3.org/2006/vcard/ns#hasCountryName";
        public const string direccionContactoRegion = "https://www.w3.org/2006/vcard/ns#address@@@https://www.w3.org/2006/vcard/ns#Address|https://www.w3.org/2006/vcard/ns#hasRegion";
        public const string direccionContactoCiudad = "https://www.w3.org/2006/vcard/ns#address@@@https://www.w3.org/2006/vcard/ns#Address|https://www.w3.org/2006/vcard/ns#locality";
        public const string direccionContactoProvincia = "https://www.w3.org/2006/vcard/ns#address@@@https://www.w3.org/2006/vcard/ns#Address|http://w3id.org/roh/hasProvince";
        public const string telefonoExtension = "https://www.w3.org/2006/vcard/ns#hasTelephone@@@https://www.w3.org/2006/vcard/ns#TelephoneType|http://w3id.org/roh/hasExtension";
        public const string telefonoNumero = "https://www.w3.org/2006/vcard/ns#hasTelephone@@@https://www.w3.org/2006/vcard/ns#TelephoneType|https://www.w3.org/2006/vcard/ns#hasValue";
        public const string telefonoCodInternacional = "https://www.w3.org/2006/vcard/ns#hasTelephone@@@https://www.w3.org/2006/vcard/ns#TelephoneType|http://w3id.org/roh/hasInternationalCode";
        public const string faxExtension = "http://w3id.org/roh/hasFax@@@https://www.w3.org/2006/vcard/ns#TelephoneType|http://w3id.org/roh/hasExtension";
        public const string faxNumero = "http://w3id.org/roh/hasFax@@@https://www.w3.org/2006/vcard/ns#TelephoneType|https://www.w3.org/2006/vcard/ns#hasValue";
        public const string faxCodInternacional = "http://w3id.org/roh/hasFax@@@https://www.w3.org/2006/vcard/ns#TelephoneType|http://w3id.org/roh/hasInternationalCode";
        public const string email = "https://www.w3.org/2006/vcard/ns#email";
        public const string movilExtension = "http://w3id.org/roh/hasMobilePhone@@@https://www.w3.org/2006/vcard/ns#TelephoneType|http://w3id.org/roh/hasExtension";
        public const string movilNumero = "http://w3id.org/roh/hasMobilePhone@@@https://www.w3.org/2006/vcard/ns#TelephoneType|https://www.w3.org/2006/vcard/ns#hasValue";
        public const string movilCodInternacional = "http://w3id.org/roh/hasMobilePhone@@@https://www.w3.org/2006/vcard/ns#TelephoneType|http://w3id.org/roh/hasInternationalCode";
        public const string ORCID = "http://w3id.org/roh/ORCID";
        public const string scopus = "http://vivoweb.org/ontology/core#scopusId";
        public const string researcherId = "http://vivoweb.org/ontology/core#researcherId";
        public const string otroIdentificador = "http://w3id.org/roh/otherIds@@@http://xmlns.com/foaf/0.1/Document|http://xmlns.com/foaf/0.1/topic";
        public const string otroIdentificadorTitulo = "http://w3id.org/roh/otherIds@@@http://xmlns.com/foaf/0.1/Document|http://purl.org/dc/elements/1.1/title";
    }
}
