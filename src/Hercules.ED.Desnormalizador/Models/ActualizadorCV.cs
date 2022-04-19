using CurriculumvitaeOntology;
using Gnoss.ApiWrapper;
using Gnoss.ApiWrapper.ApiModel;
using Gnoss.ApiWrapper.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace DesnormalizadorHercules.Models
{
    /// <summary>
    /// Clase para actualizar propiedades de CVs
    /// </summary>
    class ActualizadorCV : ActualizadorBase
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="pResourceApi">API Wrapper de GNOSS</param>
        public ActualizadorCV(ResourceApi pResourceApi) : base(pResourceApi)
        {
        }

        #region Métodos públicos

        /// <summary>
        /// Crea un currículum para los investigadores activos (http://w3id.org/roh/isActive 'true')
        /// No tiene dependencias
        /// </summary>
        /// <param name="pPersons">ID de las personas</param>
        public void CrearCVs(List<string> pPersons = null)
        {
            List<string> filters = new List<string>();
            if (pPersons != null && pPersons.Count > 0)
            {
                filters.Add($" FILTER(?person in (<{string.Join(">,<", pPersons)}>))");
            }
            else
            {
                filters.Add("");
            }
            foreach (string filter in filters)
            {
                while (true)
                {
                    //Creamos CVs
                    int limit = 50;
                    //TODO eliminar from
                    String select = @"SELECT distinct ?person from <http://gnoss.com/curriculumvitae.owl> ";
                    String where = @$"  where{{
                                            {filter}
                                            ?person a <http://xmlns.com/foaf/0.1/Person>.
                                            ?person <http://w3id.org/roh/isActive> 'true'.
                                            MINUS{{ ?cv  <http://w3id.org/roh/cvOf> ?person}}
                                        }} limit {limit}";

                    SparqlObject resultado = mResourceApi.VirtuosoQuery(select, where, "person");

                    // Personas que no poseen actualmente un CV y deberían tenerlo
                    List<string> persons = new();
                    foreach (Dictionary<string, SparqlObject.Data> fila in resultado.results.bindings)
                    {
                        persons.Add(fila["person"].value);
                    }

                    // Obtenemos los CV a cargar
                    mResourceApi.ChangeOntoly("curriculumvitae");
                    List<CV> listaCVCargar = GenerateCVFromPersons(persons);
                    Parallel.ForEach(listaCVCargar, new ParallelOptions { MaxDegreeOfParallelism = ActualizadorBase.numParallel }, cv =>
                    {
                        ComplexOntologyResource resource = cv.ToGnossApiResource(mResourceApi, new());
                        int numIntentos = 0;
                        while (!resource.Uploaded)
                        {
                            numIntentos++;
                            if (numIntentos > MAX_INTENTOS)
                            {
                                break;
                            }
                            if (listaCVCargar.Last() == cv)
                            {
                                mResourceApi.LoadComplexSemanticResource(resource, true, true);
                            }
                            else
                            {
                                mResourceApi.LoadComplexSemanticResource(resource);
                            }
                        }
                    });

                    if (resultado.results.bindings.Count != limit)
                    {
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Insertamos/eliminamos en los CV las publicaciones de las que el dueño del CV es autor con la privacidad correspondiente
        /// Depende de ActualizadorCV.CrearCVs
        /// </summary>        
        /// <param name="pPersons">IDs de la persona</param>
        /// <param name="pDocuments">IDs del documento</param>
        /// <param name="pCVs">IDs del CV</param>
        public void ModificarDocumentos(List<string> pPersons = null, List<string> pDocuments = null, List<string> pCVs = null)
        {
            List<string> filters = new List<string>();
            if (pPersons != null && pPersons.Count > 0)
            {
                filters.Add($" FILTER(?person in (<{string.Join(">,<", pPersons)}>))");
            }
            else if (pDocuments != null && pDocuments.Count > 0)
            {
                filters.Add($" FILTER(?document in (<{string.Join(">,<", pDocuments)}>))");
            }
            else if (pCVs != null && pCVs.Count > 0)
            {
                filters.Add($" FILTER(?cv in (<{string.Join(">,<", pCVs)}>))");
            }
            else
            {
                filters.Add("");
            }

            foreach (string filter in filters)
            {
                while (true)
                {
                    //Añadimos documentos
                    int limit = 500;
                    //TODO eliminar from
                    String select = @"SELECT * WHERE{select distinct ?cv ?scientificActivity ?document ?isValidated ?typeDocument  from <http://gnoss.com/document.owl> from <http://gnoss.com/person.owl>  from <http://gnoss.com/scientificactivitydocument.owl>  ";
                    String where = @$"where{{
                                    {filter}
                                    {{
                                        #DESEABLES
                                        select distinct ?person ?cv ?scientificActivity ?document ?isValidated ?typeDocument
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
                                            OPTIONAL{{?document <http://w3id.org/roh/isValidated> ?isValidated.}}
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
                                    }}
                                }}}}order by desc(?cv) limit {limit}";
                    SparqlObject resultado = mResourceApi.VirtuosoQuery(select, where, "curriculumvitae");
                    InsertarDocumentosCV(resultado);
                    if (resultado.results.bindings.Count != limit)
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
                                    }}
                                    MINUS
                                    {{
                                        #DESEABLES
                                        select distinct ?person ?cv ?scientificActivity ?document ?typeDocument
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
                    EliminarDocumentosCV(resultado);
                    if (resultado.results.bindings.Count != limit)
                    {
                        break;
                    }
                }

                while (true)
                {
                    //Elminamos duplicados
                    int limit = 500;
                    //TODO eliminar from
                    String select = @"SELECT * WHERE{select distinct ?cv (group_concat(?item;separator="";"") as ?items) count(?item) as ?numItems  ?document from <http://gnoss.com/document.owl> from <http://gnoss.com/person.owl> ";
                    String where = @$"where{{
                                    {filter}                                    
                                    {{
                                        ?person a <http://xmlns.com/foaf/0.1/Person>.                                            
                                        ?document a <http://purl.org/ontology/bibo/Document>.
                                        ?cv a <http://w3id.org/roh/CV>.
                                        ?cv <http://w3id.org/roh/cvOf> ?person.
                                        ?cv <http://w3id.org/roh/scientificActivity> ?scientificActivity.
                                        ?scientificActivity ?p ?item.
                                        ?item <http://vivoweb.org/ontology/core#relatedBy> ?document.
                                    }}
                                }}}}GROUP BY ?cv ?document HAVING (?numItems > 1)  order by desc(?cv) limit {limit}";
                    SparqlObject resultado = mResourceApi.VirtuosoQuery(select, where, "curriculumvitae");
                    EliminarDocumentosDuplicadosCV(resultado);
                    if (resultado.results.bindings.Count != limit)
                    {
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Modifica la privacidad de las publicaciones de los CV en caso de que haya que hacerlo
        /// (Solo convierte en públicos aquellos documentos que sean privados pero deberían ser públicos)
        /// Depende de ActualizadorCV.CrearCVs
        /// </summary>
        /// <param name="pPersons">IDs de la persona</param>
        /// <param name="pDocuments">IDs del documento</param>
        /// <param name="pCVs">IDs del CV</param>
        public void CambiarPrivacidadDocumentos(List<string> pPersons = null, List<string> pDocuments = null, List<string> pCVs = null)
        {
            List<string> filters = new List<string>();
            if (pPersons != null && pPersons.Count > 0)
            {
                filters.Add($" FILTER(?person in (<{string.Join(">,<", pPersons)}>))");
            }
            else if (pDocuments != null && pDocuments.Count > 0)
            {
                filters.Add($" FILTER(?document in (<{string.Join(">,<", pDocuments)}>))");
            }
            else if (pCVs != null && pCVs.Count > 0)
            {
                filters.Add($" FILTER(?cv in (<{string.Join(">,<", pCVs)}>))");
            }
            else
            {
                filters.Add("");
            }

            foreach (string filter in filters)
            {

                while (true)
                {
                    //Publicamos los documentos
                    int limit = 500;
                    //TODO eliminar from
                    String select = @"SELECT * WHERE{select distinct ?cv ?scientificActivity ?propItem ?item from <http://gnoss.com/document.owl> from <http://gnoss.com/person.owl>  ";
                    String where = @$"where{{
                                {filter}
                                {{
                                    ?person a <http://xmlns.com/foaf/0.1/Person>.                                            
                                    ?document a <http://purl.org/ontology/bibo/Document>.
                                    ?document <http://w3id.org/roh/isValidated> 'true'.
                                    ?cv a <http://w3id.org/roh/CV>.
                                    ?cv <http://w3id.org/roh/cvOf> ?person.
                                    ?cv <http://w3id.org/roh/scientificActivity> ?scientificActivity.                                        
                                    ?scientificActivity ?propItem ?item.
                                    ?item <http://vivoweb.org/ontology/core#relatedBy> ?document.
                                    ?item <http://w3id.org/roh/isPublic> 'false'.
                                }}
                            }}}}order by desc(?cv) limit {limit}";
                    SparqlObject resultado = mResourceApi.VirtuosoQuery(select, where, "curriculumvitae");
                    PublicarDocumentosCV(resultado);
                    if (resultado.results.bindings.Count != limit)
                    {
                        break;
                    }
                }
            }
        }


        /// <summary>       
        /// Insertamos/eliminamos en los CV los researchobjects de las que el dueño del CV es autor con la privacidad correspondiente
        /// Depende de ActualizadorCV.CrearCVs
        /// </summary>        
        /// <param name="pPersons">IDs de la persona</param>
        /// <param name="pResearchObjects">IDs del research object</param>
        /// <param name="pCVs">IDs del CV</param>
        public void ModificarResearchObjects(List<string> pPersons = null, List<string> pResearchObjects = null, List<string> pCVs = null)
        {
            List<string> filters = new List<string>();
            if (pPersons != null && pPersons.Count > 0)
            {
                filters.Add($" FILTER(?person in ( <{string.Join(">,<", pPersons)}>))");
            }
            else if (pResearchObjects != null && pResearchObjects.Count > 0)
            {
                filters.Add($" FILTER(?ro in ( <{string.Join(">,<", pResearchObjects)}>))");
            }
            else if (pCVs != null && pCVs.Count > 0)
            {
                filters.Add($" FILTER(?cv in ( <{string.Join(">,<", pCVs)}>))");
            }
            else
            {
                filters.Add("");
            }

            foreach (string filter in filters)
            {
                while (true)
                {
                    //Añadimos documentos
                    int limit = 500;
                    //TODO eliminar from
                    String select = @"SELECT * WHERE{select distinct ?cv ?researchObject ?ro from <http://gnoss.com/researchobject.owl> from <http://gnoss.com/person.owl>   ";
                    String where = @$"where{{
                                    {filter}
                                    {{
                                        #DESEABLES
                                        select distinct ?person ?cv ?researchObject ?ro 
                                        Where
                                        {{
                                            ?person a <http://xmlns.com/foaf/0.1/Person>.                                            
                                            ?ro a <http://w3id.org/roh/ResearchObject>.
                                            ?cv a <http://w3id.org/roh/CV>.
                                            ?cv <http://w3id.org/roh/cvOf> ?person.
                                            ?cv <http://w3id.org/roh/researchObject> ?researchObject.
                                            ?ro <http://purl.org/ontology/bibo/authorList> ?autor.
                                            ?autor <http://www.w3.org/1999/02/22-rdf-syntax-ns#member> ?person.
                                        }}
                                    }}
                                    MINUS
                                    {{
                                        #ACTUALES
                                        ?person a <http://xmlns.com/foaf/0.1/Person>.                                            
                                        ?ro a <http://w3id.org/roh/ResearchObject>.
                                        ?cv a <http://w3id.org/roh/CV>.
                                        ?cv <http://w3id.org/roh/cvOf> ?person.
                                        ?cv <http://w3id.org/roh/researchObject> ?researchObject.
                                        ?researchObject <http://w3id.org/roh/researchObjects> ?item.
                                        ?item <http://vivoweb.org/ontology/core#relatedBy> ?ro.
                                    }}
                                }}}}order by desc(?cv) limit {limit}";
                    SparqlObject resultado = mResourceApi.VirtuosoQuery(select, where, "curriculumvitae");
                    InsertarResearchObjectsCV(resultado);
                    if (resultado.results.bindings.Count != limit)
                    {
                        break;
                    }
                }

                while (true)
                {
                    //Elminamos documentos
                    int limit = 500;
                    //TODO eliminar from
                    String select = @"SELECT * WHERE{select distinct ?cv ?researchObject ?item from <http://gnoss.com/researchobject.owl> from <http://gnoss.com/person.owl>  ";
                    String where = @$"where{{
                                    {filter}                                    
                                    {{
                                        #ACTUALES
                                        ?person a <http://xmlns.com/foaf/0.1/Person>.                                            
                                        ?ro a <http://w3id.org/roh/ResearchObject>.
                                        ?cv a <http://w3id.org/roh/CV>.
                                        ?cv <http://w3id.org/roh/cvOf> ?person.
                                        ?cv <http://w3id.org/roh/researchObject> ?researchObject.
                                        ?researchObject <http://w3id.org/roh/researchObjects> ?item.
                                        ?item <http://vivoweb.org/ontology/core#relatedBy> ?ro.
                                    }}
                                    MINUS
                                    {{
                                        #DESEABLES
                                        select distinct ?person ?cv ?researchObject ?ro 
                                        Where
                                        {{
                                            ?person a <http://xmlns.com/foaf/0.1/Person>.                                            
                                            ?ro a <http://w3id.org/roh/ResearchObject>.
                                            ?cv a <http://w3id.org/roh/CV>.
                                            ?cv <http://w3id.org/roh/cvOf> ?person.
                                            ?cv <http://w3id.org/roh/researchObject> ?researchObject.
                                            ?ro <http://purl.org/ontology/bibo/authorList> ?autor.
                                            ?autor <http://www.w3.org/1999/02/22-rdf-syntax-ns#member> ?person.
                                        }}                                      
                                    }}
                                }}}}order by desc(?cv) limit {limit}";
                    SparqlObject resultado = mResourceApi.VirtuosoQuery(select, where, "curriculumvitae");
                    EliminarResearchObjectsCV(resultado);
                    if (resultado.results.bindings.Count != limit)
                    {
                        break;
                    }
                }

                while (true)
                {
                    //Elminamos duplicados
                    int limit = 500;
                    //TODO eliminar from
                    String select = @"SELECT * WHERE{select distinct ?cv ?researchObject (group_concat(?item;separator="";"") as ?items) count(?item) as ?numItems  ?ro from <http://gnoss.com/researchobject.owl> from <http://gnoss.com/person.owl> ";
                    String where = @$"where{{
                                    {filter}                                    
                                    {{
                                        ?person a <http://xmlns.com/foaf/0.1/Person>.                                            
                                        ?ro a <http://w3id.org/roh/ResearchObject>.
                                        ?cv a <http://w3id.org/roh/CV>.
                                        ?cv <http://w3id.org/roh/cvOf> ?person.
                                        ?cv <http://w3id.org/roh/researchObject> ?researchObject.
                                        ?researchObject ?p ?item.
                                        ?item <http://vivoweb.org/ontology/core#relatedBy> ?ro.
                                    }}
                                }}}}GROUP BY ?cv ?researchObject ?ro HAVING (?numItems > 1)  order by desc(?cv) limit {limit}";
                    SparqlObject resultado = mResourceApi.VirtuosoQuery(select, where, "curriculumvitae");
                    EliminarResearchObjectsDuplicadosCV(resultado);
                    if (resultado.results.bindings.Count != limit)
                    {
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Modifica la privacidad de los researchobjects de los CV en caso de que haya que hacerlo
        /// (Solo convierte en públicos aquellos researchobjects que sean privados pero deberían ser públicos)
        /// Depende de ActualizadorCV.CrearCVs
        /// </summary>
        /// <param name="pPersons">IDs de la persona</param>
        /// <param name="pResearchObjects">IDs del research object</param>
        /// <param name="pCVs">IDs del CV</param>
        public void CambiarPrivacidadResearchObjects(List<string> pPersons = null, List<string> pResearchObjects = null, List<string> pCVs = null)
        {
            List<string> filters = new List<string>();
            if (pPersons != null && pPersons.Count > 0)
            {
                filters.Add($" FILTER(?person in (<{string.Join(">,<", pPersons)}>))");
            }
            else if (pResearchObjects != null && pResearchObjects.Count > 0)
            {
                filters.Add($" FILTER(?ro in (<{string.Join(">,<", pResearchObjects)}>))");
            }
            else if (pCVs != null && pCVs.Count > 0)
            {
                filters.Add($" FILTER(?cv in (<{string.Join(">,<", pCVs)}>))");
            }
            else
            {
                filters.Add("");
            }

            foreach (string filter in filters)
            {
                while (true)
                {
                    //Publicamos los documentos
                    int limit = 500;
                    //TODO eliminar from
                    String select = @"SELECT * WHERE{select distinct ?cv ?researchObject ?propItem ?item from <http://gnoss.com/researchobject.owl> from <http://gnoss.com/person.owl>  ";
                    String where = @$"where{{
                                    {filter}
                                    {{
                                        ?person a <http://xmlns.com/foaf/0.1/Person>.                                            
                                        ?ro a <http://w3id.org/roh/ResearchObject>.
                                        ?ro <http://w3id.org/roh/isValidated> 'true'.
                                        ?cv a <http://w3id.org/roh/CV>.
                                        ?cv <http://w3id.org/roh/cvOf> ?person.
                                        ?cv <http://w3id.org/roh/researchObject> ?researchObject.
                                        ?researchObject ?propItem ?item.
                                        ?item <http://vivoweb.org/ontology/core#relatedBy> ?ro.
                                        ?item <http://w3id.org/roh/isPublic> 'false'.
                                    }}
                                }}}}order by desc(?cv) limit {limit}";
                    SparqlObject resultado = mResourceApi.VirtuosoQuery(select, where, "curriculumvitae");
                    PublicarResearchObjectsCV(resultado);
                    if (resultado.results.bindings.Count != limit)
                    {
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Insertamos/eliminamos en los CV los proyectos oficiales (con http://w3id.org/roh/isValidated='true' ) de los que el dueño del CV ha sido miembro y les ponemos privacidad pública
        /// Depende de ActualizadorCV.CrearCVs
        /// </summary>        
        /// <param name="pPersons">IDs de la persona</param>
        /// <param name="pProyectos">IDs del documento</param>
        /// <param name="pCVs">IDs del CV</param>
        public void ModificarProyectos(List<string> pPersons = null, List<string> pProyectos = null, List<string> pCVs = null)
        {
            List<string> filters = new List<string>();
            if (pPersons != null && pPersons.Count > 0)
            {
                filters.Add($" FILTER(?person in (<{string.Join(">,<", pPersons)}>))");
            }
            else if (pProyectos != null && pProyectos.Count > 0)
            {
                filters.Add($" FILTER(?project in (<{string.Join(">,<", pProyectos)}>))");
            }
            else if (pCVs != null && pCVs.Count > 0)
            {
                filters.Add($" FILTER(?cv in (<{string.Join(">,<", pCVs)}>))");
            }
            else
            {
                filters.Add("");
            }

            foreach (string filter in filters)
            {
                while (true)
                {
                    //Añadimos proyectos
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
                                            ?project <http://w3id.org/roh/isValidated> 'true'.
                                            ?cv a <http://w3id.org/roh/CV>.
                                            ?cv <http://w3id.org/roh/cvOf> ?person.
                                            ?cv <http://w3id.org/roh/scientificExperience> ?scientificExperience.
                                            ?project <http://vivoweb.org/ontology/core#relates> ?rol.
                                            ?rol <http://w3id.org/roh/roleOf> ?person.
                                            ?project <http://w3id.org/roh/scientificExperienceProject> ?scientificExperienceProject.
                                            ?scientificExperienceProject <http://purl.org/dc/elements/1.1/identifier> ?typeProject.
                                        }}
                                    }}
                                    MINUS
                                    {{
                                        #ACTUALES
                                        ?person a <http://xmlns.com/foaf/0.1/Person>.                                            
                                        ?project a <http://vivoweb.org/ontology/core#Project>.
                                        ?project <http://w3id.org/roh/isValidated> 'true'.
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
                    InsertarProyectosCV(resultado);
                    if (resultado.results.bindings.Count != limit)
                    {
                        break;
                    }
                }

                while (true)
                {
                    //Elminamos proyectos
                    int limit = 500;
                    //TODO eliminar from select distinct ?cv ?scientificActivity ?item ?typeDocument
                    String select = @"SELECT * WHERE{select distinct ?cv ?scientificExperience ?project ?item ?typeProject from <http://gnoss.com/project.owl> from <http://gnoss.com/person.owl>  from <http://gnoss.com/scientificexperienceproject.owl>  ";
                    String where = @$"where{{
                                    {filter}
                                    
                                        #ACTUALES
                                        ?person a <http://xmlns.com/foaf/0.1/Person>.                                            
                                        ?project a <http://vivoweb.org/ontology/core#Project>.
                                        ?project <http://w3id.org/roh/isValidated> 'true'.
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
                                            ?project <http://w3id.org/roh/isValidated> 'true'.
                                            ?cv a <http://w3id.org/roh/CV>.
                                            ?cv <http://w3id.org/roh/cvOf> ?person.
                                            ?cv <http://w3id.org/roh/scientificExperience> ?scientificExperience.
                                            ?project <http://vivoweb.org/ontology/core#relates> ?rol.
                                            ?rol <http://w3id.org/roh/roleOf> ?person.
                                            ?project <http://w3id.org/roh/scientificExperienceProject> ?scientificExperienceProject.
                                            ?scientificExperienceProject <http://purl.org/dc/elements/1.1/identifier> ?typeProject.
                                        }}
                                    }}
                                }}}}order by desc(?cv) limit {limit}";
                    SparqlObject resultado = mResourceApi.VirtuosoQuery(select, where, "curriculumvitae");
                    EliminarProyectosCV(resultado);
                    if (resultado.results.bindings.Count != limit)
                    {
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Insertamos/eliminamos en los CV los grupos oficiales (con http://w3id.org/roh/isValidated='true' ) de los que el dueño del CV ha sido miembro y les ponemos privacidad pública
        /// Depende de ActualizadorCV.CrearCVs
        /// </summary>        
        /// <param name="pPersons">IDs de la persona</param>
        /// <param name="pGroups">IDs del grupo</param>
        /// <param name="pCVs">ID del CV</param>
        public void ModificarGrupos(List<string> pPersons = null, List<string> pGroups = null, List<string> pCVs = null)
        {
            List<string> filters = new List<string>();
            if (pPersons != null && pPersons.Count > 0)
            {
                filters.Add($" FILTER(?person in (<{string.Join(">,<", pPersons)}>))");
            }
            else if (pGroups != null && pGroups.Count > 0)
            {
                filters.Add($" FILTER(?group in (<{string.Join(">,<", pGroups)}>))");
            }
            else if (pCVs != null && pCVs.Count > 0)
            {
                filters.Add($" FILTER(?cv in (<{string.Join(">,<", pCVs)}>))");
            }
            else
            {
                filters.Add("");
            }

            foreach (string filter in filters)
            {
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
                                        ?group <http://w3id.org/roh/isValidated> 'true'.
                                        ?cv a <http://w3id.org/roh/CV>.
                                        ?cv <http://w3id.org/roh/cvOf> ?person.
                                        ?cv <http://w3id.org/roh/scientificExperience> ?scientificExperience.       
                                        ?group <http://vivoweb.org/ontology/core#relates> ?rol.
                                        ?rol <http://w3id.org/roh/roleOf> ?person.
                                    }}
                                    MINUS
                                    {{
                                        #ACTUALES
                                        ?person a <http://xmlns.com/foaf/0.1/Person>.                                            
                                        ?group a <http://xmlns.com/foaf/0.1/Group>.
                                        ?group <http://w3id.org/roh/isValidated> 'true'.
                                        ?cv a <http://w3id.org/roh/CV>.
                                        ?cv <http://w3id.org/roh/cvOf> ?person.
                                        ?cv <http://w3id.org/roh/scientificExperience> ?scientificExperience.
                                        ?scientificExperience <http://w3id.org/roh/groups> ?item.
                                        ?item <http://vivoweb.org/ontology/core#relatedBy> ?group.
                                    }}
                                }}}}order by desc(?cv) limit {limit}";
                    SparqlObject resultado = mResourceApi.VirtuosoQuery(select, where, "curriculumvitae");
                    InsertarItemsCV(resultado, "http://w3id.org/roh/RelatedGroup", "http://w3id.org/roh/scientificExperience", "http://w3id.org/roh/groups", "group", "scientificExperience", true);

                    if (resultado.results.bindings.Count != limit)
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
                                        ?group <http://w3id.org/roh/isValidated> 'true'.
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
                                        ?group <http://w3id.org/roh/isValidated> 'true'.
                                        ?cv a <http://w3id.org/roh/CV>.
                                        ?cv <http://w3id.org/roh/cvOf> ?person.
                                        ?cv <http://w3id.org/roh/scientificExperience> ?scientificExperience.       
                                        ?group <http://vivoweb.org/ontology/core#relates> ?rol.
                                        ?rol <http://w3id.org/roh/roleOf> ?person.
                                    }}
                                }}}}order by desc(?cv) limit {limit}";
                    SparqlObject resultado = mResourceApi.VirtuosoQuery(select, where, "curriculumvitae");
                    EliminarItemsCV(resultado, "http://w3id.org/roh/scientificExperience", "http://w3id.org/roh/groups", "item", "scientificExperience");
                    if (resultado.results.bindings.Count != limit)
                    {
                        break;
                    }
                }
            }
        }

        #endregion

        #region Métodos privados
        /// <summary>
        /// Genera objetos CV de las personas pasadas por parámetro
        /// </summary>
        /// <param name="personsIDs">Identificadores de las personas</param>
        /// <returns></returns>
        private List<CV> GenerateCVFromPersons(List<string> personsIDs)
        {
            Dictionary<string, CV> listaCV = new();
            if (personsIDs.Count > 0)
            {
                var personasIDsStr = string.Join(',', personsIDs.Select(item => "<" + item + ">"));

                //Nombre
                {
                    String select = @"SELECT DISTINCT ?person ?name ?firstName ?lastName";
                    String where = @$"where{{
                                        ?person a <http://xmlns.com/foaf/0.1/Person>.
                                        ?person <http://xmlns.com/foaf/0.1/name> ?name.
                                        OPTIONAL{{?person <http://xmlns.com/foaf/0.1/firstName> ?firstName}}
                                        OPTIONAL{{?person <http://xmlns.com/foaf/0.1/lastName> ?lastName}}
                                        FILTER( ?person IN ( {personasIDsStr} )).
                        }}";
                    SparqlObject resultado = mResourceApi.VirtuosoQuery(select, where, "person");

                    // Personas que no poseen actualmente un CV
                    foreach (Dictionary<string, SparqlObject.Data> fila in resultado.results.bindings)
                    {
                        string person = fila["person"].value;
                        string name = fila["name"].value;
                        string firstName = "";
                        string lastName = "";
                        if (fila.ContainsKey("firstName"))
                        {
                            firstName = fila["firstName"].value;
                        }
                        if (fila.ContainsKey("lastName"))
                        {
                            lastName = fila["lastName"].value;
                        }
                        CV cv = new();
                        if (listaCV.ContainsKey(person))
                        {
                            cv = listaCV[person];
                        }
                        else
                        {
                            listaCV.Add(person, cv);
                        }
                        cv.Foaf_name = name;
                        cv.IdRoh_cvOf = person;
                        cv.Roh_professionalSituation = new ProfessionalSituation() { Roh_title = "-" };
                        cv.Roh_qualifications = new Qualifications() { Roh_title = "-" };
                        cv.Roh_teachingExperience = new TeachingExperience() { Roh_title = "-" };
                        cv.Roh_scientificExperience = new ScientificExperience() { Roh_title = "-" };
                        cv.Roh_scientificActivity = new ScientificActivity() { Roh_title = "-" };
                        cv.Roh_researchObject = new ResearchObjects() { Roh_title = "-" };
                        cv.Roh_freeTextSummary = new FreeTextSummary() { Roh_title = "-" };
                        cv.Roh_personalData = new PersonalData() { Foaf_firstName = firstName, Foaf_familyName = lastName };
                    }
                }

                //Email
                {
                    String select = @"SELECT DISTINCT ?person ?email";
                    String where = @$"where{{
                                        ?person a <http://xmlns.com/foaf/0.1/Person>.
                                        ?person <https://www.w3.org/2006/vcard/ns#email> ?email.
                                        FILTER( ?person IN ( {personasIDsStr} )).
                        }}";
                    SparqlObject resultado = mResourceApi.VirtuosoQuery(select, where, "person");

                    // Personas que no poseen actualmente un CV
                    foreach (Dictionary<string, SparqlObject.Data> fila in resultado.results.bindings)
                    {
                        string person = fila["person"].value;
                        string email = fila["email"].value;
                        if (listaCV.ContainsKey(person))
                        {
                            CV cv = listaCV[person];
                            cv.Roh_personalData.Vcard_email = email;
                        }
                    }
                }

                //Teléfono
                {
                    String select = @"SELECT DISTINCT ?person ?telephone";
                    String where = @$"where{{
                                        ?person a <http://xmlns.com/foaf/0.1/Person>.
                                        ?person <https://www.w3.org/2006/vcard/ns#hasTelephone> ?telephone.
                                        FILTER( ?person IN ( {personasIDsStr} )).
                        }}";
                    SparqlObject resultado = mResourceApi.VirtuosoQuery(select, where, "person");

                    // Personas que no poseen actualmente un CV
                    foreach (Dictionary<string, SparqlObject.Data> fila in resultado.results.bindings)
                    {
                        string person = fila["person"].value;
                        string telephone = fila["telephone"].value;
                        if (listaCV.ContainsKey(person))
                        {
                            CV cv = listaCV[person];
                            cv.Roh_personalData.Vcard_hasTelephone = new TelephoneType() { Vcard_hasValue = telephone };
                        }
                    }
                }

                //Página
                {
                    String select = @"SELECT DISTINCT ?person ?homepage";
                    String where = @$"where{{
                                        ?person a <http://xmlns.com/foaf/0.1/Person>.
                                        ?person <http://xmlns.com/foaf/0.1/homepage> ?homepage.
                                        FILTER( ?person IN ( {personasIDsStr} )).
                        }}";
                    SparqlObject resultado = mResourceApi.VirtuosoQuery(select, where, "person");

                    // Personas que no poseen actualmente un CV
                    foreach (Dictionary<string, SparqlObject.Data> fila in resultado.results.bindings)
                    {
                        string person = fila["person"].value;
                        string homepage = fila["homepage"].value;
                        if (listaCV.ContainsKey(person))
                        {
                            CV cv = listaCV[person];
                            cv.Roh_personalData.Foaf_homepage = homepage;
                        }
                    }
                }

                //ORCID
                //SCOPUS
                //ResearcherId
                {
                    String select = @"SELECT DISTINCT ?person ?orcid ?scopusId ?researcherId";
                    String where = @$"where{{
                                        ?person a <http://xmlns.com/foaf/0.1/Person>.
                                        OPTIONAL{{?person <http://w3id.org/roh/ORCID> ?orcid.}}
                                        OPTIONAL{{?person <http://vivoweb.org/ontology/core#scopusId> ?scopusId.}}
                                        OPTIONAL{{?person <http://vivoweb.org/ontology/core#researcherId> ?researcherId.}}
                                        FILTER( ?person IN ( {personasIDsStr} )).
                        }}";
                    SparqlObject resultado = mResourceApi.VirtuosoQuery(select, where, "person");

                    // Personas que no poseen actualmente un CV
                    foreach (Dictionary<string, SparqlObject.Data> fila in resultado.results.bindings)
                    {
                        string person = fila["person"].value;
                        string orcid = "";
                        string scopusId = "";
                        string researcherId = "";
                        if (fila.ContainsKey("orcid"))
                        {
                            orcid = fila["orcid"].value;
                        }
                        if (fila.ContainsKey("scopusId"))
                        {
                            scopusId = fila["scopusId"].value;
                        }
                        if (fila.ContainsKey("researcherId"))
                        {
                            researcherId = fila["researcherId"].value;
                        }
                        if (listaCV.ContainsKey(person))
                        {
                            CV cv = listaCV[person];
                            cv.Roh_personalData.Roh_ORCID = orcid;
                            cv.Roh_personalData.Vivo_scopusId = scopusId;
                            cv.Roh_personalData.Vivo_researcherId = researcherId;
                        }
                    }
                }

                //Otros IDs
                {
                    String select = @"SELECT DISTINCT ?person ?semanticScholarId";
                    String where = @$"where{{
                                        ?person a <http://xmlns.com/foaf/0.1/Person>.
                                        ?person <http://w3id.org/roh/semanticScholarId> ?semanticScholarId.
                                        FILTER( ?person IN ( {personasIDsStr} )).
                        }}";
                    SparqlObject resultado = mResourceApi.VirtuosoQuery(select, where, "person");

                    // Personas que no poseen actualmente un CV
                    foreach (Dictionary<string, SparqlObject.Data> fila in resultado.results.bindings)
                    {
                        string person = fila["person"].value;
                        string semanticScholarId = fila["semanticScholarId"].value;
                        if (listaCV.ContainsKey(person))
                        {
                            CV cv = listaCV[person];
                            if (cv.Roh_personalData.Roh_otherIds == null)
                            {
                                cv.Roh_personalData.Roh_otherIds = new List<Document>();
                            }
                            cv.Roh_personalData.Roh_otherIds.Add(new Document() { Foaf_topic = "SemanticScholar", Dc_title = semanticScholarId });
                        }
                    }
                }

                //Direccion
                {
                    String select = @"SELECT DISTINCT ?person ?address";
                    String where = @$"where{{
                                        ?person a <http://xmlns.com/foaf/0.1/Person>.
                                        ?person <https://www.w3.org/2006/vcard/ns#address> ?address.
                                        FILTER( ?person IN ( {personasIDsStr} )).
                        }}";
                    SparqlObject resultado = mResourceApi.VirtuosoQuery(select, where, "person");

                    // Personas que no poseen actualmente un CV
                    foreach (Dictionary<string, SparqlObject.Data> fila in resultado.results.bindings)
                    {
                        string person = fila["person"].value;
                        string address = fila["address"].value;
                        if (listaCV.ContainsKey(person))
                        {
                            CV cv = listaCV[person];
                            if (cv.Roh_personalData.Vcard_address == null)
                            {
                                cv.Roh_personalData.Vcard_address = new Address();
                            }
                            cv.Roh_personalData.Vcard_address.Vcard_locality = address;
                        }
                    }
                }
            }
            return listaCV.Values.ToList();
        }

        /// <summary>
        /// Inserta documentos en un CV
        /// </summary>
        /// <param name="pDatosCargar">Datos</param>
        private void InsertarDocumentosCV(SparqlObject pDatosCargar)
        {
            Dictionary<Guid, List<TriplesToInclude>> triplesToInclude = new();
            foreach (Dictionary<string, SparqlObject.Data> fila in pDatosCargar.results.bindings)
            {
                string cv = fila["cv"].value;
                string scientificActivity = fila["scientificActivity"].value;
                string document = fila["document"].value;
                string typeDocument = fila["typeDocument"].value;
                string isValidated = "false";
                if (fila.ContainsKey("isValidated"))
                {
                    isValidated = fila["isValidated"].value;
                }

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
                }

                //Obtenemos la auxiliar en la que cargar la entidad  
                string rdfTypePrefix = AniadirPrefijo(rdftype);
                rdfTypePrefix = rdfTypePrefix.Substring(rdfTypePrefix.IndexOf(":") + 1);
                string idNewAux = mResourceApi.GraphsUrl + "items/" + rdfTypePrefix + "_" + mResourceApi.GetShortGuid(cv) + "_" + Guid.NewGuid();
                List<TriplesToInclude> listaTriples = new();
                string idEntityAux = scientificActivity + "|" + idNewAux;

                //Privacidad            
                string predicadoPrivacidad = "http://w3id.org/roh/scientificActivity|" + property + "|http://w3id.org/roh/isPublic";
                TriplesToInclude tr2 = new(idEntityAux + "|" + isValidated, predicadoPrivacidad);
                listaTriples.Add(tr2);

                //Entidad
                string predicadoEntidad = "http://w3id.org/roh/scientificActivity|" + property + "|http://vivoweb.org/ontology/core#relatedBy";
                TriplesToInclude tr1 = new(idEntityAux + "|" + document, predicadoEntidad);
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

            Parallel.ForEach(triplesToInclude.Keys, new ParallelOptions { MaxDegreeOfParallelism = ActualizadorBase.numParallel }, idCV =>
            {
                List<List<TriplesToInclude>> listasDeListas = SplitList(triplesToInclude[idCV], 50).ToList();
                foreach (List<TriplesToInclude> triples in listasDeListas)
                {
                    mResourceApi.InsertPropertiesLoadedResources(new() { { idCV, triples } });
                }
            });
        }

        /// <summary>
        /// Cambia a públicos documentos de un CV
        /// </summary>
        /// <param name="pDatosCargar">Datos</param>
        private void PublicarDocumentosCV(SparqlObject pDatosCargar)
        {
            Dictionary<Guid, List<TriplesToModify>> triplesToModify = new();
            foreach (Dictionary<string, SparqlObject.Data> fila in pDatosCargar.results.bindings)
            {
                string cv = fila["cv"].value;
                string scientificActivity = fila["scientificActivity"].value;
                string propItem = fila["propItem"].value;
                string item = fila["item"].value;


                TriplesToModify triple = new()
                {
                    OldValue = scientificActivity + "|" + item + "|false",
                    NewValue = scientificActivity + "|" + item + "|true",
                    Predicate = "http://w3id.org/roh/scientificActivity|" + propItem + "|http://w3id.org/roh/isPublic"
                };

                Guid idCV = mResourceApi.GetShortGuid(cv);
                if (triplesToModify.ContainsKey(idCV))
                {
                    triplesToModify[idCV].Add(triple);
                }
                else
                {
                    triplesToModify.Add(mResourceApi.GetShortGuid(cv), new List<TriplesToModify>() { triple });
                }
            }

            Parallel.ForEach(triplesToModify.Keys, new ParallelOptions { MaxDegreeOfParallelism = ActualizadorBase.numParallel }, idCV =>
            {
                List<List<TriplesToModify>> listasDeListas = SplitList(triplesToModify[idCV], 50).ToList();
                foreach (List<TriplesToModify> triples in listasDeListas)
                {
                    mResourceApi.ModifyPropertiesLoadedResources(new() { { idCV, triples } });
                }
            });
        }

        /// <summary>
        /// Elimina documentos en un CV
        /// </summary>
        /// <param name="pDatosCargar">Datos</param>
        private void EliminarDocumentosCV(SparqlObject pDatosCargar)
        {
            Dictionary<Guid, List<RemoveTriples>> triplesToDelete = new();
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
                }

                RemoveTriples removeTriple = new();
                removeTriple.Predicate = "http://w3id.org/roh/scientificActivity|" + property;
                removeTriple.Value = scientificActivity + "|" + item;
                Guid idCV = mResourceApi.GetShortGuid(cv);
                if (triplesToDelete.ContainsKey(idCV))
                {
                    triplesToDelete[idCV].Add(removeTriple);
                }
                else
                {
                    triplesToDelete.Add(idCV, new() { removeTriple });
                }
            }

            Parallel.ForEach(triplesToDelete.Keys, new ParallelOptions { MaxDegreeOfParallelism = ActualizadorBase.numParallel }, idCV =>
            {
                List<List<RemoveTriples>> listasDeListas = SplitList(triplesToDelete[idCV], 50).ToList();
                foreach (List<RemoveTriples> triples in listasDeListas)
                {
                    mResourceApi.DeletePropertiesLoadedResources(new() { { idCV, triples } });
                }
            });
        }

        /// <summary>
        /// Elimina documentos duplicados en un CV
        /// </summary>
        /// <param name="pDatosCargar">Datos</param>
        private void EliminarDocumentosDuplicadosCV(SparqlObject pDatosCargar)
        {
            Dictionary<Guid, List<RemoveTriples>> triplesToDelete = new();
            foreach (Dictionary<string, SparqlObject.Data> fila in pDatosCargar.results.bindings)
            {
                string cv = fila["cv"].value;
                List<string> items = fila["items"].value.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries).ToList();
                items.RemoveAt(0);

                String select = @"  select distinct ?cv ?scientificActivity ?prop ?item from <http://gnoss.com/document.owl>";
                String where = @$"  where                                 
                                    {{
                                        ?cv a <http://w3id.org/roh/CV>.
                                        ?cv <http://w3id.org/roh/scientificActivity> ?scientificActivity.
                                        ?scientificActivity ?prop ?item.
                                        FILTER(?item in (<{string.Join(">,<", items)}>))                                 
                                        FILTER(?cv = <{cv}>)                                 
                                    }}";
                SparqlObject resultado = mResourceApi.VirtuosoQuery(select, where, "curriculumvitae");
                foreach (Dictionary<string, SparqlObject.Data> filaIn in resultado.results.bindings)
                {
                    string scientificActivity = filaIn["scientificActivity"].value;
                    string prop = filaIn["prop"].value;
                    string item = filaIn["item"].value;
                    RemoveTriples removeTriple = new();
                    removeTriple.Predicate = "http://w3id.org/roh/scientificActivity|" + prop;
                    removeTriple.Value = scientificActivity + "|" + item;
                    Guid idCV = mResourceApi.GetShortGuid(cv);
                    if (triplesToDelete.ContainsKey(idCV))
                    {
                        triplesToDelete[idCV].Add(removeTriple);
                    }
                    else
                    {
                        triplesToDelete.Add(idCV, new() { removeTriple });
                    }
                }
            }

            Parallel.ForEach(triplesToDelete.Keys, new ParallelOptions { MaxDegreeOfParallelism = ActualizadorBase.numParallel }, idCV =>
            {
                List<List<RemoveTriples>> listasDeListas = SplitList(triplesToDelete[idCV], 50).ToList();
                foreach (List<RemoveTriples> triples in listasDeListas)
                {
                    mResourceApi.DeletePropertiesLoadedResources(new() { { idCV, triples } });
                }
            });
        }

        /// <summary>
        /// Inserta ResearchObjects en un CV
        /// </summary>
        /// <param name="pDatosCargar">Datos</param>
        private void InsertarResearchObjectsCV(SparqlObject pDatosCargar)
        {
            Dictionary<Guid, List<TriplesToInclude>> triplesToInclude = new();
            foreach (Dictionary<string, SparqlObject.Data> fila in pDatosCargar.results.bindings)
            {
                string cv = fila["cv"].value;
                string researchObject = fila["researchObject"].value;
                string ro = fila["ro"].value;

                string rdftype = "http://w3id.org/roh/RelatedResearchObject";
                string property = "http://w3id.org/roh/researchObjects";

                //Obtenemos la auxiliar en la que cargar la entidad  
                string rdfTypePrefix = AniadirPrefijo(rdftype);
                rdfTypePrefix = rdfTypePrefix.Substring(rdfTypePrefix.IndexOf(":") + 1);
                string idNewAux = mResourceApi.GraphsUrl + "items/" + rdfTypePrefix + "_" + mResourceApi.GetShortGuid(cv) + "_" + Guid.NewGuid();
                List<TriplesToInclude> listaTriples = new();
                string idEntityAux = researchObject + "|" + idNewAux;

                //Privacidad            
                string predicadoPrivacidad = "http://w3id.org/roh/researchObject|" + property + "|http://w3id.org/roh/isPublic";
                TriplesToInclude tr2 = new(idEntityAux + "|true", predicadoPrivacidad);
                listaTriples.Add(tr2);

                //Entidad
                string predicadoEntidad = "http://w3id.org/roh/researchObject|" + property + "|http://vivoweb.org/ontology/core#relatedBy";
                TriplesToInclude tr1 = new(idEntityAux + "|" + ro, predicadoEntidad);
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

            Parallel.ForEach(triplesToInclude.Keys, new ParallelOptions { MaxDegreeOfParallelism = ActualizadorBase.numParallel }, idCV =>
            {
                List<List<TriplesToInclude>> listasDeListas = SplitList(triplesToInclude[idCV], 50).ToList();
                foreach (List<TriplesToInclude> triples in listasDeListas)
                {
                    mResourceApi.InsertPropertiesLoadedResources(new() { { idCV, triples } });
                }
            });
        }

        /// <summary>
        /// Cambia a públicos ResearchObjects de un CV
        /// </summary>
        /// <param name="pDatosCargar">Datos</param>
        private void PublicarResearchObjectsCV(SparqlObject pDatosCargar)
        {
            Dictionary<Guid, List<TriplesToModify>> triplesToModify = new();
            foreach (Dictionary<string, SparqlObject.Data> fila in pDatosCargar.results.bindings)
            {
                string cv = fila["cv"].value;
                string researchObject = fila["researchObject"].value;
                string propItem = fila["propItem"].value;
                string item = fila["item"].value;


                TriplesToModify triple = new()
                {
                    OldValue = researchObject + "|" + item + "|false",
                    NewValue = researchObject + "|" + item + "|true",
                    Predicate = "http://w3id.org/roh/researchObject|" + propItem + "|http://w3id.org/roh/isPublic"
                };

                Guid idCV = mResourceApi.GetShortGuid(cv);
                if (triplesToModify.ContainsKey(idCV))
                {
                    triplesToModify[idCV].Add(triple);
                }
                else
                {
                    triplesToModify.Add(mResourceApi.GetShortGuid(cv), new List<TriplesToModify>() { triple });
                }
            }

            Parallel.ForEach(triplesToModify.Keys, new ParallelOptions { MaxDegreeOfParallelism = ActualizadorBase.numParallel }, idCV =>
            {
                List<List<TriplesToModify>> listasDeListas = SplitList(triplesToModify[idCV], 50).ToList();
                foreach (List<TriplesToModify> triples in listasDeListas)
                {
                    mResourceApi.ModifyPropertiesLoadedResources(new() { { idCV, triples } });
                }
            });
        }

        /// <summary>
        /// Elimina ResearchObjects en un CV
        /// </summary>
        /// <param name="pDatosCargar">Datos</param>
        private void EliminarResearchObjectsCV(SparqlObject pDatosCargar)
        {
            Dictionary<Guid, List<RemoveTriples>> triplesToDelete = new();
            foreach (Dictionary<string, SparqlObject.Data> fila in pDatosCargar.results.bindings)
            {
                string cv = fila["cv"].value;
                string researchObject = fila["researchObject"].value;
                string item = fila["item"].value;

                string property = "http://w3id.org/roh/researchObjects";
                RemoveTriples removeTriple = new();
                removeTriple.Predicate = "http://w3id.org/roh/researchObject|" + property;
                removeTriple.Value = researchObject + "|" + item;
                Guid idCV = mResourceApi.GetShortGuid(cv);
                if (triplesToDelete.ContainsKey(idCV))
                {
                    triplesToDelete[idCV].Add(removeTriple);
                }
                else
                {
                    triplesToDelete.Add(idCV, new() { removeTriple });
                }
            }

            Parallel.ForEach(triplesToDelete.Keys, new ParallelOptions { MaxDegreeOfParallelism = ActualizadorBase.numParallel }, idCV =>
            {
                List<List<RemoveTriples>> listasDeListas = SplitList(triplesToDelete[idCV], 50).ToList();
                foreach (List<RemoveTriples> triples in listasDeListas)
                {
                    mResourceApi.DeletePropertiesLoadedResources(new() { { idCV, triples } });
                }
            });
        }

        /// <summary>
        ///  Elimina ResearchObjects duplicados en un CV
        /// </summary>
        /// <param name="pDatosCargar">Datos</param>
        private void EliminarResearchObjectsDuplicadosCV(SparqlObject pDatosCargar)
        {
            Dictionary<Guid, List<RemoveTriples>> triplesToDelete = new();
            foreach (Dictionary<string, SparqlObject.Data> fila in pDatosCargar.results.bindings)
            {
                string cv = fila["cv"].value;
                string ResearchObjects = fila["researchObject"].value;
                List<string> items = fila["items"].value.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries).ToList();
                items.RemoveAt(0);
                foreach (string item in items)
                {
                    RemoveTriples removeTriple = new();
                    removeTriple.Predicate = "http://w3id.org/roh/researchObject|http://w3id.org/roh/researchObjects";
                    removeTriple.Value = ResearchObjects + "|" + item;
                    Guid idCV = mResourceApi.GetShortGuid(cv);
                    if (triplesToDelete.ContainsKey(idCV))
                    {
                        triplesToDelete[idCV].Add(removeTriple);
                    }
                    else
                    {
                        triplesToDelete.Add(idCV, new() { removeTriple });
                    }
                }
            }

            Parallel.ForEach(triplesToDelete.Keys, new ParallelOptions { MaxDegreeOfParallelism = ActualizadorBase.numParallel }, idCV =>
            {
                List<List<RemoveTriples>> listasDeListas = SplitList(triplesToDelete[idCV], 50).ToList();
                foreach (List<RemoveTriples> triples in listasDeListas)
                {
                    mResourceApi.DeletePropertiesLoadedResources(new() { { idCV, triples } });
                }
            });
        }

        /// <summary>
        /// Inserta proyectos en un CV
        /// </summary>
        /// <param name="pDatosCargar">Datos</param>
        private void InsertarProyectosCV(SparqlObject pDatosCargar)
        {
            //http://gnoss.com/items/scientificexperienceproject_SEP1
            Dictionary<Guid, List<TriplesToInclude>> triplesToInclude = new();
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
                string idNewAux = mResourceApi.GraphsUrl + "items/" + rdfTypePrefix + "_" + mResourceApi.GetShortGuid(cv) + "_" + Guid.NewGuid();
                List<TriplesToInclude> listaTriples = new();
                string idEntityAux = scientificExperience + "|" + idNewAux;

                //Privacidad, true (son proyectos oficiales)
                string predicadoPrivacidad = "http://w3id.org/roh/scientificExperience|" + property + "|http://w3id.org/roh/isPublic";
                TriplesToInclude tr2 = new(idEntityAux + "|true", predicadoPrivacidad);
                listaTriples.Add(tr2);

                //Entidad
                string predicadoEntidad = "http://w3id.org/roh/scientificExperience|" + property + "|http://vivoweb.org/ontology/core#relatedBy";
                TriplesToInclude tr1 = new(idEntityAux + "|" + project, predicadoEntidad);
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

            Parallel.ForEach(triplesToInclude.Keys, new ParallelOptions { MaxDegreeOfParallelism = ActualizadorBase.numParallel }, idCV =>
            {
                mResourceApi.InsertPropertiesLoadedResources(new Dictionary<Guid, List<TriplesToInclude>>() { { idCV, triplesToInclude[idCV] } });
            });
        }

        /// <summary>
        /// Elimina proyectos en un CV
        /// </summary>
        /// <param name="pDatosCargar">Datos</param>
        private void EliminarProyectosCV(SparqlObject pDatosCargar)
        {
            Dictionary<Guid, List<RemoveTriples>> triplesToDelete = new();
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

                RemoveTriples removeTriple = new();
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

            Parallel.ForEach(triplesToDelete.Keys, new ParallelOptions { MaxDegreeOfParallelism = ActualizadorBase.numParallel }, idCV =>
            {
                mResourceApi.DeletePropertiesLoadedResources(new Dictionary<Guid, List<RemoveTriples>>() { { idCV, triplesToDelete[idCV] } });
            });
        }

        /// <summary>
        /// Inserta items en un CV
        /// </summary>
        /// <param name="pDatosCargar">Datos</param>
        /// <param name="pRdfType">rdf:type de la entidad auxiliar principal</param>
        /// <param name="pSectionProperty">propiedad de la sección</param>
        /// <param name="pProperty">propiedad que apunta a la entidad</param>
        /// <param name="pVarEntity">variable en la que está la entidad</param>
        /// <param name="pVarSection">variable en la que está la sección</param>
        /// <param name="pPublic">indica si se carga como público o como privado</param>
        private void InsertarItemsCV(SparqlObject pDatosCargar, string pRdfType, string pSectionProperty, string pProperty, string pVarEntity, string pVarSection, bool pPublic)
        {
            Dictionary<Guid, List<TriplesToInclude>> triplesToInclude = new();
            foreach (Dictionary<string, SparqlObject.Data> fila in pDatosCargar.results.bindings)
            {
                string cv = fila["cv"].value;
                string section = fila[pVarSection].value;
                string entity = fila[pVarEntity].value;

                //Obtenemos la auxiliar en la que cargar la entidad     
                string rdfTypePrefix = AniadirPrefijo(pRdfType);
                rdfTypePrefix = rdfTypePrefix.Substring(rdfTypePrefix.IndexOf(":") + 1);
                string idNewAux = mResourceApi.GraphsUrl + "items/" + rdfTypePrefix + "_" + mResourceApi.GetShortGuid(cv) + "_" + Guid.NewGuid();
                List<TriplesToInclude> listaTriples = new();
                string idEntityAux = section + "|" + idNewAux;

                //Privacidad                  
                string predicadoPrivacidad = pSectionProperty + "|" + pProperty + "|http://w3id.org/roh/isPublic";
                TriplesToInclude tr2 = new(idEntityAux + "|" + pPublic.ToString().ToLower(), predicadoPrivacidad);
                listaTriples.Add(tr2);

                //Entidad
                string predicadoEntidad = pSectionProperty + "|" + pProperty + "|http://vivoweb.org/ontology/core#relatedBy";
                TriplesToInclude tr1 = new(idEntityAux + "|" + entity, predicadoEntidad);
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

            Parallel.ForEach(triplesToInclude.Keys, new ParallelOptions { MaxDegreeOfParallelism = ActualizadorBase.numParallel }, idCV =>
            {
                mResourceApi.InsertPropertiesLoadedResources(new Dictionary<Guid, List<TriplesToInclude>>() { { idCV, triplesToInclude[idCV] } });
            });
        }

        /// <summary>
        /// Elimina items de un CV
        /// </summary>
        /// <param name="pDatosCargar">Datos</param>
        /// <param name="pSectionProperty">propiedad de la sección</param>
        /// <param name="pProperty">propiedad que apunta a la entidad</param>
        /// <param name="pVarItem">variable en la que está el ítem</param>
        /// <param name="pVarSection">variable en la que está la sección</param>
        private void EliminarItemsCV(SparqlObject pDatosCargar, string pSectionProperty, string pProperty, string pVarItem, string pVarSection)
        {
            Dictionary<Guid, List<RemoveTriples>> triplesToDelete = new();
            foreach (Dictionary<string, SparqlObject.Data> fila in pDatosCargar.results.bindings)
            {
                string cv = fila["cv"].value;
                string section = fila[pVarSection].value;
                string item = fila[pVarItem].value;

                RemoveTriples removeTriple = new();
                removeTriple.Predicate = pSectionProperty + "|" + pProperty;
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

            Parallel.ForEach(triplesToDelete.Keys, new ParallelOptions { MaxDegreeOfParallelism = ActualizadorBase.numParallel }, idCV =>
            {
                mResourceApi.DeletePropertiesLoadedResources(new Dictionary<Guid, List<RemoveTriples>>() { { idCV, triplesToDelete[idCV] } });
            });
        }

        #endregion
    }
}
