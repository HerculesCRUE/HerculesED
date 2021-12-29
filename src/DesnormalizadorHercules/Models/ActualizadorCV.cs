using CurriculumvitaeOntology;
using Gnoss.ApiWrapper;
using Gnoss.ApiWrapper.ApiModel;
using Gnoss.ApiWrapper.Model;
using PersonOntology;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace DesnormalizadorHercules.Models
{
    class ActualizadorCV : ActualizadorBase
    {
        public ActualizadorCV(ResourceApi pResourceApi):base(pResourceApi)
        {
        }
        private static string rutaOauth = $@"{System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase}Config/OAuthV3.config";
        private static ResourceApi mResourceApi = new ResourceApi(rutaOauth);
        private static CommunityApi mCommunityApi = new CommunityApi(rutaOauth);
        private static Guid mIdComunidad = mCommunityApi.GetCommunityId();

        public void ActualizarDocumentos(string pPerson = null, string pDocument = null, string pCV = null)
        {
            string graphsUrl = mResourceApi.GraphsUrl;
            if (!string.IsNullOrEmpty(graphsUrl))
            {
                string filter = "";
                if (!string.IsNullOrEmpty(pPerson))
                {
                    filter = $" FILTER(?person =<{pPerson}>)";
                }
                if (!string.IsNullOrEmpty(pDocument))
                {
                    filter = $" FILTER(?document =<{pDocument}>)";
                }
                if (!string.IsNullOrEmpty(pCV))
                {
                    filter = $" FILTER(?cv =<{pCV}>)";
                }





                while (true)
                {
                    //Añadimos documentos
                    int limit = 500;
                    //TODO eliminar from
                    String select = @"SELECT * WHERE{select distinct ?cv ?scientificActivity ?document  ?typeDocument  from <http://gnoss.com/document.owl> from <http://gnoss.com/person.owl>  from <http://gnoss.com/scientificactivitydocument.owl>  ";
                    String where = @$"where{{
                                    {filter}
                                    {{
                                        #DESEABLES
                                        select distinct ?cv ?scientificActivity ?document ?typeDocument
                                        Where
                                        {{
                                            ?person a <http://xmlns.com/foaf/0.1/Person>.                                            
                                            ?document a <http://purl.org/ontology/bibo/Document>.
                                            ?cv a <http://w3id.org/roh/CV>.
                                            ?cv <http://w3id.org/roh/cvOf> ?person.
                                            ?cv <http://w3id.org/roh/scientificActivity> ?scientificActivity.
                                            ?document <http://purl.org/ontology/bibo/authorList> ?autor.
                                            ?autor <http://www.w3.org/1999/02/22-rdf-syntax-ns#member> ?person.
                                            ?document <http://w3id.org/roh/scientificActivityDocument> ?scientificActivityDocument.
                                            ?scientificActivityDocument <http://purl.org/dc/elements/1.1/identifier> ?typeDocument.
                                        }}
                                    }}
                                    MINUS
                                    {{
                                        #ACTUALES
                                        ?person a <http://xmlns.com/foaf/0.1/Person>.                                            
                                        ?document a <http://purl.org/ontology/bibo/Document>.
                                        ?cv a <http://w3id.org/roh/CV>.
                                        ?cv <http://w3id.org/roh/cvOf> ?person.
                                        ?cv <http://w3id.org/roh/scientificActivity> ?scientificActivity.
                                        {{
                                                ?scientificActivity <http://w3id.org/roh/scientificPublications> ?item.
                                                ?item <http://vivoweb.org/ontology/core#relatedBy> ?document.
                                                BIND(""SAD1"" as ?typeDocument)
                                        }}
                                        UNION
                                        {{
                                                ?scientificActivity <http://w3id.org/roh/worksSubmittedConferences> ?item.
                                                ?item <http://vivoweb.org/ontology/core#relatedBy> ?document.
                                                BIND(""SAD2"" as ?typeDocument)
                                        }}
                                        UNION
                                        {{
                                                ?scientificActivity <http://w3id.org/roh/worksSubmittedSeminars> ?item.
                                                ?item <http://vivoweb.org/ontology/core#relatedBy> ?document.
                                                BIND(""SAD3"" as ?typeDocument)
                                        }}
                                        UNION
                                        {{
                                                ?scientificActivity <http://w3id.org/roh/otherDisseminationActivities> ?item.
                                                ?item <http://vivoweb.org/ontology/core#relatedBy> ?document.
                                                BIND(""SAD4"" as ?typeDocument)
                                        }}
                                    }}
                                }}}}order by desc(?cv) limit {limit}";
                    SparqlObject resultado = mResourceApi.VirtuosoQuery(select, where, "curriculumvitae");
                    InsertarDocumentosCV(resultado, graphsUrl);
                    if (resultado.results.bindings.Count() != limit)
                    {
                        break;
                    }
                }

                while (true)
                {
                    //Elminamos documentos
                    int limit = 500;
                    //TODO eliminar from
                    String select = @"SELECT * WHERE{select distinct ?cv ?scientificActivity ?item ?typeDocument from <http://gnoss.com/document.owl> from <http://gnoss.com/person.owl>  from <http://gnoss.com/scientificactivitydocument.owl>  ";
                    String where = @$"where{{
                                    {filter}                                    
                                    {{
                                        #ACTUALES
                                        ?person a <http://xmlns.com/foaf/0.1/Person>.                                            
                                        ?document a <http://purl.org/ontology/bibo/Document>.
                                        ?cv a <http://w3id.org/roh/CV>.
                                        ?cv <http://w3id.org/roh/cvOf> ?person.
                                        ?cv <http://w3id.org/roh/scientificActivity> ?scientificActivity.
                                        {{
                                                ?scientificActivity <http://w3id.org/roh/scientificPublications> ?item.
                                                ?item <http://vivoweb.org/ontology/core#relatedBy> ?document.
                                                BIND(""SAD1"" as ?typeDocument)
                                        }}
                                        UNION
                                        {{
                                                ?scientificActivity <http://w3id.org/roh/worksSubmittedConferences> ?item.
                                                ?item <http://vivoweb.org/ontology/core#relatedBy> ?document.
                                                BIND(""SAD2"" as ?typeDocument)
                                        }}
                                        UNION
                                        {{
                                                ?scientificActivity <http://w3id.org/roh/worksSubmittedSeminars> ?item.
                                                ?item <http://vivoweb.org/ontology/core#relatedBy> ?document.
                                                BIND(""SAD3"" as ?typeDocument)
                                        }}
                                        UNION
                                        {{
                                                ?scientificActivity <http://w3id.org/roh/otherDisseminationActivities> ?item.
                                                ?item <http://vivoweb.org/ontology/core#relatedBy> ?document.
                                                BIND(""SAD4"" as ?typeDocument)
                                        }}
                                    }}
                                    MINUS
                                    {{
                                        #DESEABLES
                                        select distinct ?cv ?scientificActivity ?document ?typeDocument
                                        Where
                                        {{
                                            ?person a <http://xmlns.com/foaf/0.1/Person>.                                            
                                            ?document a <http://purl.org/ontology/bibo/Document>.
                                            ?cv a <http://w3id.org/roh/CV>.
                                            ?cv <http://w3id.org/roh/cvOf> ?person.
                                            ?cv <http://w3id.org/roh/scientificActivity> ?scientificActivity.
                                            ?document <http://purl.org/ontology/bibo/authorList> ?autor.
                                            ?autor <http://www.w3.org/1999/02/22-rdf-syntax-ns#member> ?person.
                                            ?document <http://w3id.org/roh/scientificActivityDocument> ?scientificActivityDocument.
                                            ?scientificActivityDocument <http://purl.org/dc/elements/1.1/identifier> ?typeDocument.
                                        }}                                        
                                    }}
                                }}}}order by desc(?cv) limit {limit}";
                    SparqlObject resultado = mResourceApi.VirtuosoQuery(select, where, "curriculumvitae");
                    EliminarDocumentosCV(resultado, graphsUrl);
                    if (resultado.results.bindings.Count() != limit)
                    {
                        break;
                    }
                }
            }
        }


        public void ActualizarProyectos(string pPerson = null, string pProyecto = null, string pCV = null)
        {
            string graphsUrl = mResourceApi.GraphsUrl;
            if (!string.IsNullOrEmpty(graphsUrl))
            {
                string filter = "";
                if (!string.IsNullOrEmpty(pPerson))
                {
                    filter = $" FILTER(?person =<{pPerson}>)";
                }
                if (!string.IsNullOrEmpty(pProyecto))
                {
                    filter = $" FILTER(?project =<{pProyecto}>)";
                }
                if (!string.IsNullOrEmpty(pCV))
                {
                    filter = $" FILTER(?cv =<{pCV}>)";
                }
                while (true)
                {
                    //TODO esto esta mal, los autores de los proectos hay que reisarlos
                    //Añadimos documentos
                    int limit = 500;
                    //TODO eliminar from
                    String select = @"SELECT * WHERE{select distinct ?cv ?scientificExperience ?project ?typeProject from <http://gnoss.com/project.owl> from <http://gnoss.com/person.owl>  from <http://gnoss.com/scientificexperienceproject.owl>  ";
                    String where = @$"where{{
                                    {filter}
                                    {{
                                        #DESEABLES
                                        select distinct ?cv ?scientificExperience ?project ?typeProject 
                                        Where
                                        {{
                                            ?person a <http://xmlns.com/foaf/0.1/Person>.                                            
                                            ?project a <http://vivoweb.org/ontology/core#Project>.
                                            ?cv a <http://w3id.org/roh/CV>.
                                            ?cv <http://w3id.org/roh/cvOf> ?person.
                                            ?cv <http://w3id.org/roh/scientificExperience> ?scientificExperience.
                                            ?project ?propRol ?rol.
                                            FILTER(?propRol in (<http://vivoweb.org/ontology/core#relates>,<http://w3id.org/roh/mainResearchers>))
                                            ?rol <http://www.w3.org/1999/02/22-rdf-syntax-ns#member> ?person.
                                            ?project <http://w3id.org/roh/scientificExperienceProject> ?scientificExperienceProject.
                                            ?scientificExperienceProject <http://purl.org/dc/elements/1.1/identifier> ?typeProject.
                                        }}
                                    }}
                                    MINUS
                                    {{
                                        #ACTUALES
                                        ?person a <http://xmlns.com/foaf/0.1/Person>.                                            
                                        ?project a <http://vivoweb.org/ontology/core#Project>.
                                        ?cv a <http://w3id.org/roh/CV>.
                                        ?cv <http://w3id.org/roh/cvOf> ?person.
                                        ?cv <http://w3id.org/roh/scientificExperience> ?scientificExperience.
                                        {{
                                                ?scientificExperience <http://w3id.org/roh/competitiveProjects> ?item.
                                                ?item <http://vivoweb.org/ontology/core#relatedBy> ?project.
                                                BIND(""SEP1"" as ?typeProject)
                                        }}
                                        UNION
                                        {{
                                                ?scientificExperience <http://w3id.org/roh/nonCompetitiveProjects> ?item.
                                                ?item <http://vivoweb.org/ontology/core#relatedBy> ?project.
                                                BIND(""SEP2"" as ?typeProject)
                                        }}
                                    }}
                                }}}}order by desc(?cv) limit {limit}";
                    SparqlObject resultado = mResourceApi.VirtuosoQuery(select, where, "curriculumvitae");
                    InsertarProyectosCV(resultado, graphsUrl);
                    if (resultado.results.bindings.Count() != limit)
                    {
                        break;
                    }
                }

                while (true)
                {
                    //Elminamos documentos
                    int limit = 500;
                    //TODO eliminar from select distinct ?cv ?scientificActivity ?item ?typeDocument
                    String select = @"SELECT * WHERE{select distinct ?cv ?scientificExperience ?project ?item ?typeProject from <http://gnoss.com/project.owl> from <http://gnoss.com/person.owl>  from <http://gnoss.com/scientificexperienceproject.owl>  ";
                    String where = @$"where{{
                                    {filter}
                                    
                                        #ACTUALES
                                        ?person a <http://xmlns.com/foaf/0.1/Person>.                                            
                                        ?project a <http://vivoweb.org/ontology/core#Project>.
                                        ?cv a <http://w3id.org/roh/CV>.
                                        ?cv <http://w3id.org/roh/cvOf> ?person.
                                        ?cv <http://w3id.org/roh/scientificExperience> ?scientificExperience.
                                        {{
                                                ?scientificExperience <http://w3id.org/roh/competitiveProjects> ?item.
                                                ?item <http://vivoweb.org/ontology/core#relatedBy> ?project.
                                                BIND(""SEP1"" as ?typeProject)
                                        }}
                                        UNION
                                        {{
                                                ?scientificExperience <http://w3id.org/roh/nonCompetitiveProjects> ?item.
                                                ?item <http://vivoweb.org/ontology/core#relatedBy> ?project.
                                                BIND(""SEP2"" as ?typeProject)
                                        }}                                       
                                    
                                    MINUS
                                    {{
                                        #DESEABLES
                                        select distinct ?cv ?scientificExperience ?project ?typeProject 
                                        Where
                                        {{
                                            ?person a <http://xmlns.com/foaf/0.1/Person>.                                            
                                            ?project a <http://vivoweb.org/ontology/core#Project>.
                                            ?cv a <http://w3id.org/roh/CV>.
                                            ?cv <http://w3id.org/roh/cvOf> ?person.
                                            ?cv <http://w3id.org/roh/scientificExperience> ?scientificExperience.
                                            ?project ?propRol ?rol.
                                            FILTER(?propRol in (<http://vivoweb.org/ontology/core#relates>,<http://w3id.org/roh/mainResearchers>))
                                            ?rol <http://www.w3.org/1999/02/22-rdf-syntax-ns#member> ?person.
                                            ?project <http://w3id.org/roh/scientificExperienceProject> ?scientificExperienceProject.
                                            ?scientificExperienceProject <http://purl.org/dc/elements/1.1/identifier> ?typeProject.
                                        }}
                                    }}
                                }}}}order by desc(?cv) limit {limit}";
                    SparqlObject resultado = mResourceApi.VirtuosoQuery(select, where, "curriculumvitae");
                    EliminarProyectosCV(resultado, graphsUrl);
                    if (resultado.results.bindings.Count() != limit)
                    {
                        break;
                    }
                }
            }
        }

        public void ActualizarGrupos(string pPerson = null, string pGroup = null, string pCV = null)
        {
            string graphsUrl = mResourceApi.GraphsUrl;
            if (!string.IsNullOrEmpty(graphsUrl))
            {
                string filter = "";
                if (!string.IsNullOrEmpty(pPerson))
                {
                    filter = $" FILTER(?person =<{pPerson}>)";
                }
                if (!string.IsNullOrEmpty(pGroup))
                {
                    filter = $" FILTER(?group =<{pGroup}>)";
                }
                if (!string.IsNullOrEmpty(pCV))
                {
                    filter = $" FILTER(?cv =<{pCV}>)";
                }
                while (true)
                {                    
                    //Añadimos grupos
                    int limit = 500;
                    //TODO eliminar from
                    String select = @"SELECT * WHERE{select distinct ?cv ?scientificExperience ?group from <http://gnoss.com/group.owl> from <http://gnoss.com/person.owl> ";
                    String where = @$"where{{
                                    {filter} 
                                    {{
                                        #DESEABLES                                        
                                        ?person a <http://xmlns.com/foaf/0.1/Person>.                                            
                                        ?group a <http://xmlns.com/foaf/0.1/Group>.
                                        ?group <http://w3id.org/roh/crisIdentifier> ?crisIdentifier.
                                        ?cv a <http://w3id.org/roh/CV>.
                                        ?cv <http://w3id.org/roh/cvOf> ?person.
                                        ?cv <http://w3id.org/roh/scientificExperience> ?scientificExperience.                                            
                                        {{
                                            ?group <http://w3id.org/roh/mainResearchers> ?rol.
                                            ?rol <http://w3id.org/roh/roleOf> ?person.
                                        }}
                                        UNION
                                        {{
                                            ?group <http://xmlns.com/foaf/0.1/member> ?rol.
                                            ?rol <http://w3id.org/roh/roleOf> ?person.
                                        }}
                                    }}
                                    MINUS
                                    {{
                                        #ACTUALES
                                        ?person a <http://xmlns.com/foaf/0.1/Person>.                                            
                                        ?group a <http://xmlns.com/foaf/0.1/Group>.
                                        ?group <http://w3id.org/roh/crisIdentifier> ?crisIdentifier.
                                        ?cv a <http://w3id.org/roh/CV>.
                                        ?cv <http://w3id.org/roh/cvOf> ?person.
                                        ?cv <http://w3id.org/roh/scientificExperience> ?scientificExperience.
                                        ?scientificExperience <http://w3id.org/roh/groups> ?item.
                                        ?item <http://vivoweb.org/ontology/core#relatedBy> ?group.
                                    }}
                                }}}}order by desc(?cv) limit {limit}";
                    SparqlObject resultado = mResourceApi.VirtuosoQuery(select, where, "curriculumvitae");
                    InsertarItemsCV(resultado, graphsUrl, "http://w3id.org/roh/RelatedGroup", "http://w3id.org/roh/scientificExperience", "http://w3id.org/roh/groups", "group", "scientificExperience");

                    if (resultado.results.bindings.Count() != limit)
                    {
                        break;
                    }
                }

                while (true)
                {
                    //Elminamos grupos
                    int limit = 500;
                    //TODO eliminar from 
                    String select = @"SELECT * WHERE{select distinct ?cv ?scientificExperience ?group ?item from <http://gnoss.com/group.owl> from <http://gnoss.com/person.owl> ";
                    String where = @$"where{{
                                    {filter}                                    
                                    {{
                                        #ACTUALES
                                        ?person a <http://xmlns.com/foaf/0.1/Person>.                                            
                                        ?group a <http://xmlns.com/foaf/0.1/Group>.
                                        ?group <http://w3id.org/roh/crisIdentifier> ?crisIdentifier.
                                        ?cv a <http://w3id.org/roh/CV>.
                                        ?cv <http://w3id.org/roh/cvOf> ?person.
                                        ?cv <http://w3id.org/roh/scientificExperience> ?scientificExperience.
                                        ?scientificExperience <http://w3id.org/roh/groups> ?item.
                                        ?item <http://vivoweb.org/ontology/core#relatedBy> ?group.                                
                                    }}
                                    MINUS
                                    {{
                                        #DESEABLES                                        
                                        ?person a <http://xmlns.com/foaf/0.1/Person>.                                            
                                        ?group a <http://xmlns.com/foaf/0.1/Group>.
                                        ?group <http://w3id.org/roh/crisIdentifier> ?crisIdentifier.
                                        ?cv a <http://w3id.org/roh/CV>.
                                        ?cv <http://w3id.org/roh/cvOf> ?person.
                                        ?cv <http://w3id.org/roh/scientificExperience> ?scientificExperience.                                            
                                        {{
                                            ?group <http://w3id.org/roh/mainResearchers> ?rol.
                                            ?rol <http://w3id.org/roh/roleOf> ?person.
                                        }}
                                        UNION
                                        {{
                                            ?group <http://xmlns.com/foaf/0.1/member> ?rol.
                                            ?rol <http://w3id.org/roh/roleOf> ?person.
                                        }}
                                    }}
                                }}}}order by desc(?cv) limit {limit}";
                    SparqlObject resultado = mResourceApi.VirtuosoQuery(select, where, "curriculumvitae");
                    EliminarItemsCV(resultado, graphsUrl, "http://w3id.org/roh/scientificExperience", "http://w3id.org/roh/groups","item", "scientificExperience");
                    if (resultado.results.bindings.Count() != limit)
                    {
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Crea un currículum para todos los investigadores de las universidades indicadas
        /// </summary>
        /// <param name="pPerson">Lista de IDs de las personas.</param>
        /// <param name="universidades">Universidades a las que tienen que pertenecer los investigadores para crearse el CV.</param>
        public void CrearCVs(string pPerson = null, List<string> universidades = null)
        {


            // TMP - prove with a current element for test
            Guid tmpElement = new Guid("e32c3594-bbad-4861-87c3-1258f265a27a");
            mResourceApi.PersistentDelete(tmpElement);

            List<ComplexOntologyResource> listaRecursosCargar = new List<ComplexOntologyResource>();

            string graphsUrl = mResourceApi.GraphsUrl;
            if (!string.IsNullOrEmpty(graphsUrl))
            {
                string filter = "";
                if (!string.IsNullOrEmpty(pPerson))
                {
                    filter = $" FILTER(?person =<{pPerson}>)";
                }

                if (universidades == null)
                {
                    universidades = new List<string>();
                    universidades.Add("Universidad de Murcia");
                }
                var universidadesStr = string.Join(',', universidades.Select(item => "'" + item + "'"));

                while (true)
                {
                    // http://vivoweb.org/ontology/core#departmentOrSchool
                    //Añadimos documentos
                    int limit = 50;
                    //TODO eliminar from
                    String select = @"SELECT ?person WHERE{select distinct ?person from <http://gnoss.com/person.owl> from <http://gnoss.com/curriculumvitae.owl> ";
                    String where = @$"where{{
                                    {filter}

                                    ?person <http://w3id.org/roh/hasRole> ?orgAux.
                                    ?orgAux <http://w3id.org/roh/title> ?orgTitle.
                                    FILTER ( ?orgTitle IN ({universidadesStr})).
                                    FILTER NOT EXISTS {{ ?cv <http://w3id.org/roh/cvOf> ?person }}

                                }}}} limit {limit}";

                    try
                    {

                        SparqlObject resultado = mResourceApi.VirtuosoQuery(select, where, "organization");

                        // Personas que no poseen actualmente un CV
                        List<string> persons = new List<string>();
                        foreach (Dictionary<string, SparqlObject.Data> fila in resultado.results.bindings)
                        {
                            persons.Add(fila["person"].value);
                        }

                        // Obtenemos toda la información que nos interesa de las personas
                        var peopleData = getDataFromPeople(persons);

                        List<CV> listCV = new List<CV>();
                        mResourceApi.ChangeOntoly("curriculumvitae");
                        foreach (KeyValuePair<string, Person> pd in peopleData)
                        {

                            CV newCv = new CV();

                            // Cargamos los datos personales
                            var persData = new PersonalData();
                            if (pd.Value.Vcard_email != null)
                            {
                                persData.Vcard_email = pd.Value.Vcard_email[0];
                            }
                            // tmpPD.Vcard_hasTelephone = pd.Value.Vcard_hasTelephone.Select(e => new TelephoneType( e, GnossBase.GnossOCBase.LanguageEnum.es));
                            persData.Foaf_homepage = pd.Value.Foaf_homepage;
                            persData.Roh_ORCID = pd.Value.Roh_ORCID;
                            persData.Vivo_scopusId = pd.Value.Vivo_scopusId;
                            persData.Vivo_researcherId = pd.Value.Vivo_researcherId;
                            // tmpPD.Vcard_address. = pd.Value.Vcard_address;

                            // Añadimos el teléfono
                            persData.Vcard_hasTelephone = new List<TelephoneType>();
                            if (pd.Value.Vcard_hasTelephone != null)
                            {
                                foreach (string tel in pd.Value.Vcard_hasTelephone)
                                {
                                    var telObj = new TelephoneType();
                                    telObj.Vcard_hasValue = tel;
                                    persData.Vcard_hasTelephone.Add(telObj);
                                }
                            }

                            // Añadimos el personalData en el currículum
                            newCv.Roh_personalData = persData;

                            // Cargamos los datos del CV
                            if (pd.Value.Foaf_nick != null && pd.Value.Foaf_nick.Count > 0)
                            {
                                newCv.Foaf_name = pd.Value.Foaf_nick[0];
                            } else if (pd.Value.Foaf_name != null)
                            {
                                newCv.Foaf_name = pd.Value.Foaf_name;
                            }
                            newCv.IdRoh_cvOf = pd.Key;

                            // Cargamos la actividad científica y la experiencia científica
                            var scientificExperience = new ScientificExperience();
                            scientificExperience.Roh_title = "-";

                            var scientificActivity = new ScientificActivity();
                            scientificActivity.Roh_title = "-";

                            newCv.Roh_scientificExperience = scientificExperience;
                            newCv.Roh_scientificActivity = scientificActivity;

                            listCV.Add(newCv);


                            //Creamos el recurso.
                            ComplexOntologyResource resource = newCv.ToGnossApiResource(mResourceApi, new List<string>());
                            listaRecursosCargar.Add(resource);
                        }                        
                        CargarDatos(listaRecursosCargar);
                        listaRecursosCargar.Clear();




                        if (resultado.results.bindings.Count() != limit)
                        {
                            break;
                        }
                    }
                    catch (Exception e) {}
                    
                }
            }
        }


        /// <summary>
        /// Obtiene los de las entidades persona para cargarlos en el CV
        /// </summary>
        /// <param name="personsIDs">Lista de IDs de las personas.</param>
        private Dictionary<string, Person> getDataFromPeople(List<string> personsIDs)
        {
            var persons = new Dictionary<string, Person>();

            var personasIDsStr = string.Join(',', personsIDs.Select(item => "<" + item + ">"));

            // http://vivoweb.org/ontology/core#departmentOrSchool
            //Añadimos documentos
            int limit = 1000;
            //TODO eliminar from
            String select = @"SELECT DISTINCT ?person ?p ?o";
            String where = @$"where{{
                                        ?person ?p ?o.
                                        FILTER( ?person IN ( {personasIDsStr} )).
                        }}order by desc(?person) limit {limit}";

            try
            {
                SparqlObject resultado = mResourceApi.VirtuosoQuery(select, where, "person");

                // Personas que no poseen actualmente un CV
                foreach (Dictionary<string, SparqlObject.Data> fila in resultado.results.bindings)
                {
                    if (!persons.ContainsKey(fila["person"].value))
                    {
                        persons.Add(fila["person"].value, new Person());
                    }
                    switch (fila["p"].value)
                    {
                        case "http://xmlns.com/foaf/0.1/nick":
                            persons[fila["person"].value].Foaf_nick = new List<string> { fila["o"].value };
                            break;
                        case "http://xmlns.com/foaf/0.1/name":
                            persons[fila["person"].value].Foaf_name = fila["o"].value;
                            break;
                        case "http://w3id.org/roh/ORCID":
                            persons[fila["person"].value].Roh_ORCID = fila["o"].value;
                            break;
                        case "https://www.w3.org/2006/vcard/ns#email":
                            if (persons[fila["person"].value].Vcard_email != null && persons[fila["person"].value].Vcard_email.Count > 0)
                            {
                                persons[fila["person"].value].Vcard_email.Add(fila["o"].value);
                            }
                            else
                            {
                                persons[fila["person"].value].Vcard_email = new List<string> { fila["o"].value };
                            }
                            break;
                        case "https://www.w3.org/2006/vcard/ns#address":
                            persons[fila["person"].value].Vcard_address = fila["o"].value;
                            break;
                        case "https://www.w3.org/2006/vcard/ns#hasTelephone":
                            if (persons[fila["person"].value].Vcard_hasTelephone != null && persons[fila["person"].value].Vcard_hasTelephone.Count > 0)
                            {
                                persons[fila["person"].value].Vcard_hasTelephone.Add(fila["o"].value);
                            }
                            else
                            {
                                persons[fila["person"].value].Vcard_hasTelephone = new List<string> { fila["o"].value };
                            }
                            break;
                        case "http://xmlns.com/foaf/0.1/homepage":
                            if (persons[fila["person"].value].Foaf_homepage != null && persons[fila["person"].value].Foaf_homepage.Count > 0)
                            {
                                persons[fila["person"].value].Foaf_homepage.Add(fila["o"].value);
                            }
                            else
                            {
                                persons[fila["person"].value].Foaf_homepage = new List<string> { fila["o"].value };
                            }
                            break;
                        case "http://vivoweb.org/ontology/core#scopusId":
                            persons[fila["person"].value].Vivo_scopusId = fila["o"].value;
                            break;
                        case "http://vivoweb.org/ontology/core#researcherId":
                            persons[fila["person"].value].Vivo_researcherId = fila["o"].value;
                            break;
                    }
                    // persons.Add(fila["person"].value);
                }

            } catch (Exception e) { }
            

            return persons;
        }

        /// <summary>
        /// Permite cargar los recursos.
        /// </summary>
        /// <param name="pListaRecursosCargar">Lista de recursos a cargar.</param>
        private void CargarDatos(List<ComplexOntologyResource> pListaRecursosCargar)
        {
            //Carga.
            foreach (ComplexOntologyResource recursoCargar in pListaRecursosCargar)
            {
                if (pListaRecursosCargar.Last() == recursoCargar)
                {
                    mResourceApi.LoadComplexSemanticResource(recursoCargar, true, true);
                }
                else
                {
                    mResourceApi.LoadComplexSemanticResource(recursoCargar);
                }
            }
        }

        private void InsertarDocumentosCV(SparqlObject pDatosCargar,string graphsUrl)
        {
            Dictionary<Guid, List<TriplesToInclude>> triplesToInclude = new Dictionary<Guid, List<TriplesToInclude>>();
            foreach (Dictionary<string, SparqlObject.Data> fila in pDatosCargar.results.bindings)
            {
                string cv = fila["cv"].value;
                string scientificActivity = fila["scientificActivity"].value;
                string document = fila["document"].value;
                string typeDocument = fila["typeDocument"].value;

                string rdftype = "";
                string property = "";
                switch (typeDocument)
                {
                    case "SAD1":
                        rdftype = "http://w3id.org/roh/RelatedScientificPublication";
                        property = "http://w3id.org/roh/scientificPublications";
                        break;
                    case "SAD2":
                        rdftype = "http://w3id.org/roh/RelatedWorkSubmittedConferences";
                        property = "http://w3id.org/roh/worksSubmittedConferences";
                        break;
                    case "SAD3":
                        rdftype = "http://w3id.org/roh/RelatedWorkSubmittedSeminars";
                        property = "http://w3id.org/roh/worksSubmittedSeminars";
                        break;
                    case "SAD4":
                        rdftype = "http://w3id.org/roh/RelatedOtherDisseminationActivity";
                        property = "http://w3id.org/roh/otherDisseminationActivities";
                        break;
                }

                //Obtenemos la auxiliar en la que cargar la entidad  
                string rdfTypePrefix = AniadirPrefijo(rdftype);
                rdfTypePrefix = rdfTypePrefix.Substring(rdfTypePrefix.IndexOf(":") + 1);
                string idNewAux = graphsUrl + "items/" + rdfTypePrefix + "_" + mResourceApi.GetShortGuid(cv) + "_" + Guid.NewGuid();
                List<TriplesToInclude> listaTriples = new List<TriplesToInclude>();
                string idEntityAux = scientificActivity + "|" + idNewAux;

                //TODO
                //Privacidad, por defecto false                    
                string predicadoPrivacidad = "http://w3id.org/roh/scientificActivity|" + property + "|http://w3id.org/roh/isPublic";
                TriplesToInclude tr2 = new TriplesToInclude(idEntityAux + "|false", predicadoPrivacidad);
                listaTriples.Add(tr2);

                //Entidad
                string predicadoEntidad = "http://w3id.org/roh/scientificActivity|" + property + "|http://vivoweb.org/ontology/core#relatedBy";
                TriplesToInclude tr1 = new TriplesToInclude(idEntityAux + "|" + document, predicadoEntidad);
                listaTriples.Add(tr1);

                Guid idCV = mResourceApi.GetShortGuid(cv);
                if (triplesToInclude.ContainsKey(idCV))
                {
                    triplesToInclude[idCV].AddRange(listaTriples);
                }
                else
                {
                    triplesToInclude.Add(mResourceApi.GetShortGuid(cv), listaTriples);
                }
            }

            foreach(Guid idCV in triplesToInclude.Keys)
            {
                mResourceApi.InsertPropertiesLoadedResources(new Dictionary<Guid, List<TriplesToInclude>>() { { idCV, triplesToInclude[idCV] } });
            }
        }

        private void EliminarDocumentosCV(SparqlObject pDatosCargar, string graphsUrl)
        {
            Dictionary<Guid, List<RemoveTriples>> triplesToDelete = new Dictionary<Guid, List<RemoveTriples>>();
            foreach (Dictionary<string, SparqlObject.Data> fila in pDatosCargar.results.bindings)
            {
                string cv = fila["cv"].value;
                string scientificActivity = fila["scientificActivity"].value;
                string item = fila["item"].value;
                string typeDocument = fila["typeDocument"].value;

                string property = "";
                switch (typeDocument)
                {
                    case "SAD1":
                        property = "http://w3id.org/roh/scientificPublications";
                        break;
                    case "SAD2":
                        property = "http://w3id.org/roh/worksSubmittedConferences";
                        break;
                    case "SAD3":
                        property = "http://w3id.org/roh/worksSubmittedSeminars";
                        break;
                    case "SAD4":
                        property = "http://w3id.org/roh/otherDisseminationActivities";
                        break;
                }

                RemoveTriples removeTriple = new RemoveTriples();
                removeTriple.Predicate = "http://w3id.org/roh/scientificActivity|" + property;
                removeTriple.Value = scientificActivity + "|" + item;
                Guid idCV = mResourceApi.GetShortGuid(cv);
                if (triplesToDelete.ContainsKey(idCV))
                {
                    triplesToDelete[idCV].Add(removeTriple);
                }
                else
                {
                    triplesToDelete.Add(idCV,new List<RemoveTriples>() { removeTriple });
                }
            }

            foreach (Guid idCV in triplesToDelete.Keys)
            {
                mResourceApi.DeletePropertiesLoadedResources(new Dictionary<Guid, List<RemoveTriples>>() { { idCV, triplesToDelete[idCV] } });
            }
        }

        private void InsertarProyectosCV(SparqlObject pDatosCargar, string graphsUrl)
        {
            //http://gnoss.com/items/scientificexperienceproject_SEP1
            Dictionary<Guid, List<TriplesToInclude>> triplesToInclude = new Dictionary<Guid, List<TriplesToInclude>>();
            foreach (Dictionary<string, SparqlObject.Data> fila in pDatosCargar.results.bindings)
            {
                string cv = fila["cv"].value;
                string scientificExperience = fila["scientificExperience"].value;
                string project = fila["project"].value;
                string typeProject = fila["typeProject"].value;

                string rdftype = "";
                string property = "";
                switch (typeProject)
                {
                    case "SEP1":
                        rdftype = "http://w3id.org/roh/RelatedCompetitiveProject";
                        property = "http://w3id.org/roh/competitiveProjects";
                        break;
                    case "SEP2":
                        rdftype = "http://w3id.org/roh/RelatedNonCompetitiveProject";
                        property = "http://w3id.org/roh/nonCompetitiveProjects";
                        break;
                }

                //Obtenemos la auxiliar en la que cargar la entidad     
                string rdfTypePrefix = AniadirPrefijo(rdftype);
                rdfTypePrefix = rdfTypePrefix.Substring(rdfTypePrefix.IndexOf(":") + 1);
                string idNewAux = graphsUrl + "items/" + rdfTypePrefix + "_" + mResourceApi.GetShortGuid(cv) + "_" + Guid.NewGuid();
                List<TriplesToInclude> listaTriples = new List<TriplesToInclude>();
                string idEntityAux = scientificExperience + "|" + idNewAux;

                //TODO
                //Privacidad, por defecto false                    
                string predicadoPrivacidad = "http://w3id.org/roh/scientificExperience|" + property + "|http://w3id.org/roh/isPublic";
                TriplesToInclude tr2 = new TriplesToInclude(idEntityAux + "|false", predicadoPrivacidad);
                listaTriples.Add(tr2);

                //Entidad
                string predicadoEntidad = "http://w3id.org/roh/scientificExperience|" + property + "|http://vivoweb.org/ontology/core#relatedBy";
                TriplesToInclude tr1 = new TriplesToInclude(idEntityAux + "|" + project, predicadoEntidad);
                listaTriples.Add(tr1);

                Guid idCV = mResourceApi.GetShortGuid(cv);
                if (triplesToInclude.ContainsKey(idCV))
                {
                    triplesToInclude[idCV].AddRange(listaTriples);
                }
                else
                {
                    triplesToInclude.Add(mResourceApi.GetShortGuid(cv), listaTriples);
                }
            }

            foreach (Guid idCV in triplesToInclude.Keys)
            {
                mResourceApi.InsertPropertiesLoadedResources(new Dictionary<Guid, List<TriplesToInclude>>() { { idCV, triplesToInclude[idCV] } });
            }
        }

        private void EliminarProyectosCV(SparqlObject pDatosCargar, string graphsUrl)
        {
            Dictionary<Guid, List<RemoveTriples>> triplesToDelete = new Dictionary<Guid, List<RemoveTriples>>();
            foreach (Dictionary<string, SparqlObject.Data> fila in pDatosCargar.results.bindings)
            {
                string cv = fila["cv"].value;
                string scientificExperience = fila["scientificExperience"].value;
                string item = fila["item"].value;
                string typeProject = fila["typeProject"].value;

                string property = "";
                switch (typeProject)
                {
                    case "SEP1":
                        property = "http://w3id.org/roh/competitiveProjects";
                        break;
                    case "SEP2":
                        property = "http://w3id.org/roh/nonCompetitiveProjects";
                        break;
                }

                RemoveTriples removeTriple = new RemoveTriples();
                removeTriple.Predicate = "http://w3id.org/roh/scientificExperience|" + property;
                removeTriple.Value = scientificExperience + "|" + item;
                Guid idCV = mResourceApi.GetShortGuid(cv);
                if (triplesToDelete.ContainsKey(idCV))
                {
                    triplesToDelete[idCV].Add(removeTriple);
                }
                else
                {
                    triplesToDelete.Add(idCV, new List<RemoveTriples>() { removeTriple });
                }
            }

            foreach (Guid idCV in triplesToDelete.Keys)
            {
                mResourceApi.DeletePropertiesLoadedResources(new Dictionary<Guid, List<RemoveTriples>>() { { idCV, triplesToDelete[idCV] } });
            }
        }

        private void InsertarItemsCV(SparqlObject pDatosCargar, string graphsUrl,string pRdfType, string pSectionProperty, string pProperty, string pVarEntity, string pVarSection)
        {
            Dictionary<Guid, List<TriplesToInclude>> triplesToInclude = new Dictionary<Guid, List<TriplesToInclude>>();
            foreach (Dictionary<string, SparqlObject.Data> fila in pDatosCargar.results.bindings)
            {
                string cv = fila["cv"].value;
                string section = fila[pVarSection].value;
                string entity = fila[pVarEntity].value;

                //Obtenemos la auxiliar en la que cargar la entidad     
                string rdfTypePrefix = AniadirPrefijo(pRdfType);
                rdfTypePrefix = rdfTypePrefix.Substring(rdfTypePrefix.IndexOf(":") + 1);
                string idNewAux = graphsUrl + "items/" + rdfTypePrefix + "_" + mResourceApi.GetShortGuid(cv) + "_" + Guid.NewGuid();
                List<TriplesToInclude> listaTriples = new List<TriplesToInclude>();
                string idEntityAux = section + "|" + idNewAux;

                //TODO
                //Privacidad, por defecto false                    
                string predicadoPrivacidad = pSectionProperty+"|" + pProperty + "|http://w3id.org/roh/isPublic";
                TriplesToInclude tr2 = new TriplesToInclude(idEntityAux + "|false", predicadoPrivacidad);
                listaTriples.Add(tr2);

                //Entidad
                string predicadoEntidad = pSectionProperty+"|" + pProperty + "|http://vivoweb.org/ontology/core#relatedBy";
                TriplesToInclude tr1 = new TriplesToInclude(idEntityAux + "|" + entity, predicadoEntidad);
                listaTriples.Add(tr1);

                Guid idCV = mResourceApi.GetShortGuid(cv);
                if (triplesToInclude.ContainsKey(idCV))
                {
                    triplesToInclude[idCV].AddRange(listaTriples);
                }
                else
                {
                    triplesToInclude.Add(mResourceApi.GetShortGuid(cv), listaTriples);
                }
            }

            foreach (Guid idCV in triplesToInclude.Keys)
            {
                mResourceApi.InsertPropertiesLoadedResources(new Dictionary<Guid, List<TriplesToInclude>>() { { idCV, triplesToInclude[idCV] } });
            }
        }

        private void EliminarItemsCV(SparqlObject pDatosCargar, string graphsUrl, string pSectionProperty, string pProperty, string pVarItem, string pVarSection)
        {
            Dictionary<Guid, List<RemoveTriples>> triplesToDelete = new Dictionary<Guid, List<RemoveTriples>>();
            foreach (Dictionary<string, SparqlObject.Data> fila in pDatosCargar.results.bindings)
            {
                string cv = fila["cv"].value;
                string section = fila[pVarSection].value;
                string item = fila[pVarItem].value;

                RemoveTriples removeTriple = new RemoveTriples();
                removeTriple.Predicate = pSectionProperty+"|" + pProperty;
                removeTriple.Value = section + "|" + item;
                Guid idCV = mResourceApi.GetShortGuid(cv);
                if (triplesToDelete.ContainsKey(idCV))
                {
                    triplesToDelete[idCV].Add(removeTriple);
                }
                else
                {
                    triplesToDelete.Add(idCV, new List<RemoveTriples>() { removeTriple });
                }
            }

            foreach (Guid idCV in triplesToDelete.Keys)
            {
                mResourceApi.DeletePropertiesLoadedResources(new Dictionary<Guid, List<RemoveTriples>>() { { idCV, triplesToDelete[idCV] } });
            }
        }


    }
}
