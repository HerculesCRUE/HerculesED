using CurriculumvitaeOntology;
using Gnoss.ApiWrapper;
using Gnoss.ApiWrapper.ApiModel;
using Gnoss.ApiWrapper.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace DesnormalizadorHercules.Models.Actualizadores
{
    /// <summary>
    /// Clase para actualizar propiedades de CVs
    /// </summary>
    class ActualizadorCV : ActualizadorBase
    {
        /// <summary>
        /// Objeto para 'ModificarElementosCV'
        /// </summary>
        public class CVSection
        {
            /// <summary>
            /// Código de CVN
            /// </summary>
            public string cvnCode { get; set; }
            /// <summary>
            /// Grafo de la entidad
            /// </summary>
            public string graph { get; set; }
            /// <summary>
            /// rdf:type de la entidad
            /// </summary>
            public string rdfType { get; set; }
            /// <summary>
            /// rdf:type de la entidad auxiliar
            /// </summary>
            public string rdfTypeAux { get; set; }
            /// <summary>
            /// Propiedad que apunta a la sección
            /// </summary>
            public string sectionProperty { get; set; }
            /// <summary>
            /// Propiedad que apunta de la sección a la entidad auxiliar
            /// </summary>
            public string itemProperty { get; set; }
            public CVSection(string pCvnCode, string pGraph, string pRdfType, string pSectionProperty, string pItemProperty, string pRdfTypeAux)
            {
                cvnCode = pCvnCode;
                graph = pGraph;
                rdfType = pRdfType;
                sectionProperty = pSectionProperty;
                itemProperty = pItemProperty;
                rdfTypeAux = pRdfTypeAux;
            }
        }

        /// <summary>
        /// Objeto para 'ModificarOrganizacionesCV'
        /// </summary>
        public class OrgTitleCVTitleOrg
        {
            /// <summary>
            /// PropiedadAuxiliar
            /// </summary>
            public string propAux { get; set; }
            /// <summary>
            /// Grafo
            /// </summary>
            public string graph { get; set; }
            /// <summary>
            /// rdf:type de la entidad
            /// </summary>
            public string rdfType { get; set; }
            /// <summary>
            /// Propiedad con el título desnormalizado
            /// </summary>
            public string propTituloDesnormalizado { get; set; }
            /// <summary>
            /// Propiedad con la orgnización
            /// </summary>
            public string propOrganizacion { get; set; }
            public OrgTitleCVTitleOrg(string pGraph, string pRdfType, string pPropAux, string pPropTituloDesnormalizado, string pPropOrganizacion)
            {
                propAux = pPropAux;
                graph = pGraph;
                rdfType = pRdfType;
                propTituloDesnormalizado = pPropTituloDesnormalizado;
                propOrganizacion = pPropOrganizacion;
            }
        }

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
        /// <param name="pProjects">ID de proyectos</param>
        /// <param name="pGroups">ID de grupos</param>
        /// <param name="pDocuments">ID de documentos</param>
        /// <param name="pROs">ID de research objects</param>
        /// <param name="pPatents">ID de patentes</param>
        public void CrearCVs(List<string> pPersons = null, List<string> pProjects = null, List<string> pGroups = null, List<string> pDocuments = null, List<string> pROs = null, List<string> pPatents = null)
        {
            HashSet<string> filters = new HashSet<string>();
            if (pPersons != null && pPersons.Count > 0)
            {
                filters.Add($" FILTER(?person in (<{string.Join(">,<", pPersons)}>))");
            }
            if (pProjects != null && pProjects.Count > 0)
            {
                filters.Add($" ?projectAux <http://vivoweb.org/ontology/core#relates> ?relatesAux. ?relatesAux <http://w3id.org/roh/roleOf> ?person.  FILTER(?projectAux in (<{string.Join(">,<", pProjects)}>))");
            }
            if (pGroups != null && pGroups.Count > 0)
            {
                filters.Add($" ?groupAux <http://vivoweb.org/ontology/core#relates> ?relatesAux. ?relatesAux <http://w3id.org/roh/roleOf> ?person.  FILTER(?groupAux in (<{string.Join(">,<", pGroups)}>))");
            }
            if (pDocuments != null && pDocuments.Count > 0)
            {
                filters.Add($" ?docAux <http://purl.org/ontology/bibo/authorList> ?autoresAux. ?autoresAux <http://www.w3.org/1999/02/22-rdf-syntax-ns#member> ?person.  FILTER(?docAux in (<{string.Join(">,<", pDocuments)}>))");
            }
            if (pROs != null && pROs.Count > 0)
            {
                filters.Add($" ?roAux <http://purl.org/ontology/bibo/authorList> ?autoresAux. ?autoresAux <http://www.w3.org/1999/02/22-rdf-syntax-ns#member> ?person.  FILTER(?roAux in (<{string.Join(">,<", pROs)}>))");
            }
            if (pPatents != null && pPatents.Count > 0)
            {
                filters.Add($" ?patentAux <http://purl.org/ontology/bibo/authorList> ?autoresAux. ?autoresAux <http://www.w3.org/1999/02/22-rdf-syntax-ns#member> ?person.  FILTER(?patentAux in (<{string.Join(">,<", pPatents)}>))");
            }
            if (filters.Count == 0)
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
                    String select = @"SELECT distinct ?person from <http://gnoss.com/curriculumvitae.owl> from <http://gnoss.com/project.owl> from <http://gnoss.com/group.owl> from <http://gnoss.com/document.owl> from <http://gnoss.com/researchobject.owl>  ";
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
            HashSet<string> filters = new HashSet<string>();
            if (pPersons != null && pPersons.Count > 0)
            {
                filters.Add($" FILTER(?person in (<{string.Join(">,<", pPersons)}>))");
            }
            if (pDocuments != null && pDocuments.Count > 0)
            {
                filters.Add($" FILTER(?document in (<{string.Join(">,<", pDocuments)}>))");
            }
            if (pCVs != null && pCVs.Count > 0)
            {
                filters.Add($" FILTER(?cv in (<{string.Join(">,<", pCVs)}>))");
            }
            if (filters.Count == 0)
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
                    String select = @"select distinct ?cv ?scientificActivity ?document ?isValidated ?typeDocument  from <http://gnoss.com/document.owl> from <http://gnoss.com/person.owl>  from <http://gnoss.com/scientificactivitydocument.owl>  ";
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
                                }}order by desc(?cv) limit {limit}";
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
                    String select = @"select distinct ?cv ?scientificActivity ?item ?typeDocument from <http://gnoss.com/document.owl> from <http://gnoss.com/person.owl>  from <http://gnoss.com/scientificactivitydocument.owl>  ";
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
                                            ?scientificActivityDocument  <http://purl.org/dc/elements/1.1/identifier> ?typeDocument.
                                        }}                                        
                                    }}
                                }}order by desc(?cv) limit {limit}";
                    SparqlObject resultado = mResourceApi.VirtuosoQuery(select, where, "curriculumvitae");
                    EliminarDocumentosCV(resultado);
                    if (resultado.results.bindings.Count != limit)
                    {
                        break;
                    }
                }

                EliminarDuplicados(pCVs);
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
            HashSet<string> filters = new HashSet<string>();
            if (pPersons != null && pPersons.Count > 0)
            {
                filters.Add($" FILTER(?person in (<{string.Join(">,<", pPersons)}>))");
            }
            if (pDocuments != null && pDocuments.Count > 0)
            {
                filters.Add($" FILTER(?document in (<{string.Join(">,<", pDocuments)}>))");
            }
            if (pCVs != null && pCVs.Count > 0)
            {
                filters.Add($" FILTER(?cv in (<{string.Join(">,<", pCVs)}>))");
            }
            if (filters.Count == 0)
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
                    String select = @"select distinct ?cv ?scientificActivity ?propItem ?item from <http://gnoss.com/document.owl> from <http://gnoss.com/person.owl>  ";
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
                            }}order by desc(?cv) limit {limit}";
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
        /// <param name="pROs">IDs del research object</param>
        /// <param name="pCVs">IDs del CV</param>
        public void ModificarResearchObjects(List<string> pPersons = null, List<string> pROs = null, List<string> pCVs = null)
        {
            HashSet<string> filters = new HashSet<string>();
            if (pPersons != null && pPersons.Count > 0)
            {
                filters.Add($" FILTER(?person in ( <{string.Join(">,<", pPersons)}>))");
            }
            if (pROs != null && pROs.Count > 0)
            {
                filters.Add($" FILTER(?ro in ( <{string.Join(">,<", pROs)}>))");
            }
            if (pCVs != null && pCVs.Count > 0)
            {
                filters.Add($" FILTER(?cv in ( <{string.Join(">,<", pCVs)}>))");
            }
            if (filters.Count == 0)
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
                    String select = @"select distinct ?cv ?researchObject ?ro from <http://gnoss.com/researchobject.owl> from <http://gnoss.com/person.owl>   ";
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
                                }}order by desc(?cv) limit {limit}";
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
                    String select = @"select distinct ?cv ?researchObject ?item from <http://gnoss.com/researchobject.owl> from <http://gnoss.com/person.owl>  ";
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
                                }}order by desc(?cv) limit {limit}";
                    SparqlObject resultado = mResourceApi.VirtuosoQuery(select, where, "curriculumvitae");
                    EliminarResearchObjectsCV(resultado);
                    if (resultado.results.bindings.Count != limit)
                    {
                        break;
                    }
                }

                EliminarDuplicados(pCVs);
            }
        }

        /// <summary>
        /// Modifica la privacidad de los researchobjects de los CV en caso de que haya que hacerlo
        /// (Solo convierte en públicos aquellos researchobjects que sean privados pero deberían ser públicos)
        /// Depende de ActualizadorCV.CrearCVs
        /// </summary>
        /// <param name="pPersons">IDs de la persona</param>
        /// <param name="pROs">IDs del research object</param>
        /// <param name="pCVs">IDs del CV</param>
        public void CambiarPrivacidadResearchObjects(List<string> pPersons = null, List<string> pROs = null, List<string> pCVs = null)
        {
            HashSet<string> filters = new HashSet<string>();
            if (pPersons != null && pPersons.Count > 0)
            {
                filters.Add($" FILTER(?person in (<{string.Join(">,<", pPersons)}>))");
            }
            if (pROs != null && pROs.Count > 0)
            {
                filters.Add($" FILTER(?ro in (<{string.Join(">,<", pROs)}>))");
            }
            if (pCVs != null && pCVs.Count > 0)
            {
                filters.Add($" FILTER(?cv in (<{string.Join(">,<", pCVs)}>))");
            }
            if (filters.Count == 0)
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
                    String select = @"select distinct ?cv ?researchObject ?propItem ?item from <http://gnoss.com/researchobject.owl> from <http://gnoss.com/person.owl>  ";
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
                                }}order by desc(?cv) limit {limit}";
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
        /// <param name="pProjects">IDs del documento</param>
        /// <param name="pCVs">IDs del CV</param>
        public void ModificarProyectos(List<string> pPersons = null, List<string> pProjects = null, List<string> pCVs = null)
        {
            HashSet<string> filters = new HashSet<string>();
            if (pPersons != null && pPersons.Count > 0)
            {
                filters.Add($" FILTER(?person in (<{string.Join(">,<", pPersons)}>))");
            }
            if (pProjects != null && pProjects.Count > 0)
            {
                filters.Add($" FILTER(?project in (<{string.Join(">,<", pProjects)}>))");
            }
            if (pCVs != null && pCVs.Count > 0)
            {
                filters.Add($" FILTER(?cv in (<{string.Join(">,<", pCVs)}>))");
            }
            if (filters.Count == 0)
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
                    String select = @"select distinct ?cv ?scientificExperience ?project ?typeProject from <http://gnoss.com/project.owl> from <http://gnoss.com/person.owl>  from <http://gnoss.com/scientificexperienceproject.owl>  ";
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
                                }}order by desc(?cv) limit {limit}";
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
                    String select = @"select distinct ?cv ?scientificExperience ?project ?item ?typeProject from <http://gnoss.com/project.owl> from <http://gnoss.com/person.owl>  from <http://gnoss.com/scientificexperienceproject.owl>  ";
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
                                }}order by desc(?cv) limit {limit}";
                    SparqlObject resultado = mResourceApi.VirtuosoQuery(select, where, "curriculumvitae");
                    EliminarProyectosCV(resultado);
                    if (resultado.results.bindings.Count != limit)
                    {
                        break;
                    }
                }

                EliminarDuplicados(pCVs);
            }
        }

        /// <summary>
        /// Insertamos/eliminamos en los CV los grupos oficiales (con http://w3id.org/roh/isValidated='true' ) de los que el dueño del CV ha sido miembro y les ponemos privacidad pública
        /// Depende de ActualizadorCV.CrearCVs
        /// </summary>        
        /// <param name="pPersons">IDs de las personas</param>
        /// <param name="pGroups">IDs de los grupos</param>
        /// <param name="pCVs">IDs de los CVs</param>
        public void ModificarGrupos(List<string> pPersons = null, List<string> pGroups = null, List<string> pCVs = null)
        {
            HashSet<string> filters = new HashSet<string>();
            if (pPersons != null && pPersons.Count > 0)
            {
                filters.Add($" FILTER(?person in (<{string.Join(">,<", pPersons)}>))");
            }
            if (pGroups != null && pGroups.Count > 0)
            {
                filters.Add($" FILTER(?group in (<{string.Join(">,<", pGroups)}>))");
            }
            if (pCVs != null && pCVs.Count > 0)
            {
                filters.Add($" FILTER(?cv in (<{string.Join(">,<", pCVs)}>))");
            }
            if (filters.Count == 0)
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
                    String select = @"select distinct ?cv ?idSection ?item 
                                        'http://w3id.org/roh/RelatedGroup' as ?rdfTypeAux 
                                        'http://w3id.org/roh/scientificExperience' as ?sectionProperty
                                        'http://w3id.org/roh/groups' as ?auxProperty
                    from <http://gnoss.com/group.owl> from <http://gnoss.com/person.owl> ";
                    String where = @$"where{{
                                    {filter} 
                                    {{
                                        #DESEABLES                                        
                                        ?person a <http://xmlns.com/foaf/0.1/Person>.                                            
                                        ?item a <http://xmlns.com/foaf/0.1/Group>.
                                        ?item <http://w3id.org/roh/isValidated> 'true'.
                                        ?item <http://w3id.org/roh/crisIdentifier> ?crisIdentifier.
                                        ?cv a <http://w3id.org/roh/CV>.
                                        ?cv <http://w3id.org/roh/cvOf> ?person.
                                        ?cv <http://w3id.org/roh/scientificExperience> ?idSection.       
                                        ?item <http://vivoweb.org/ontology/core#relates> ?rol.
                                        ?rol <http://w3id.org/roh/roleOf> ?person.
                                    }}
                                    MINUS
                                    {{
                                        #ACTUALES
                                        ?person a <http://xmlns.com/foaf/0.1/Person>.                                            
                                        ?item a <http://xmlns.com/foaf/0.1/Group>.
                                        ?item <http://w3id.org/roh/isValidated> 'true'.
                                        ?cv a <http://w3id.org/roh/CV>.
                                        ?cv <http://w3id.org/roh/cvOf> ?person.
                                        ?cv <http://w3id.org/roh/scientificExperience> ?idSection.
                                        ?idSection <http://w3id.org/roh/groups> ?auxSection.
                                        ?auxSection <http://vivoweb.org/ontology/core#relatedBy> ?item.
                                    }}
                                }}order by desc(?cv) limit {limit}";
                    SparqlObject resultado = mResourceApi.VirtuosoQuery(select, where, "curriculumvitae");
                    InsertarItemsCV(resultado);

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
                    String select = @"select distinct ?cv ?idSection ?auxEntity 
                                        'http://w3id.org/roh/scientificExperience' as ?sectionProperty
                                        'http://w3id.org/roh/groups' as ?auxProperty      
                                        ?group from <http://gnoss.com/group.owl> from <http://gnoss.com/person.owl> ";
                    String where = @$"where{{
                                    {filter}                                    
                                    {{
                                        #ACTUALES
                                        ?person a <http://xmlns.com/foaf/0.1/Person>.                                            
                                        ?group a <http://xmlns.com/foaf/0.1/Group>.
                                        ?group <http://w3id.org/roh/isValidated> 'true'.
                                        ?cv a <http://w3id.org/roh/CV>.
                                        ?cv <http://w3id.org/roh/cvOf> ?person.
                                        ?cv <http://w3id.org/roh/scientificExperience> ?idSection.
                                        ?idSection <http://w3id.org/roh/groups> ?auxEntity.
                                        ?auxEntity <http://vivoweb.org/ontology/core#relatedBy> ?group.                                
                                    }}
                                    MINUS
                                    {{
                                        #DESEABLES                                        
                                        ?person a <http://xmlns.com/foaf/0.1/Person>.                                            
                                        ?group a <http://xmlns.com/foaf/0.1/Group>.
                                        ?group <http://w3id.org/roh/isValidated> 'true'.
                                        ?cv a <http://w3id.org/roh/CV>.
                                        ?cv <http://w3id.org/roh/cvOf> ?person.
                                        ?cv <http://w3id.org/roh/scientificExperience> ?idSection.       
                                        ?group <http://vivoweb.org/ontology/core#relates> ?rol.
                                        ?rol <http://w3id.org/roh/roleOf> ?person.
                                    }}
                                }}order by desc(?cv) limit {limit}";
                    SparqlObject resultado = mResourceApi.VirtuosoQuery(select, where, "curriculumvitae");
                    EliminarItemsCV(resultado);
                    if (resultado.results.bindings.Count != limit)
                    {
                        break;
                    }
                }

                EliminarDuplicados(pCVs);
            }
        }

        /// <summary>
        /// Insertamos/eliminamos en los CV las patentes oficiales (con http://w3id.org/roh/crisIdentifier ) de los que el dueño del CV es autor y les ponemos privacidad pública
        /// Depende de ActualizadorCV.CrearCVs
        /// </summary>        
        /// <param name="pPersons">IDs de la persona</param>
        /// <param name="pPatents">IDs de patentes</param>
        /// <param name="pCVs">IDs del CV</param>
        public void ModificarPatentes(List<string> pPersons = null, List<string> pPatents = null, List<string> pCVs = null)
        {
            HashSet<string> filters = new HashSet<string>();
            if (pPersons != null && pPersons.Count > 0)
            {
                filters.Add($" FILTER(?person in (<{string.Join(">,<", pPersons)}>))");
            }
            if (pPatents != null && pPatents.Count > 0)
            {
                filters.Add($" FILTER(?patent in (<{string.Join(">,<", pPatents)}>))");
            }
            if (pCVs != null && pCVs.Count > 0)
            {
                filters.Add($" FILTER(?cv in (<{string.Join(">,<", pCVs)}>))");
            }
            if (filters.Count == 0)
            {
                filters.Add("");
            }

            foreach (string filter in filters)
            {
                while (true)
                {
                    //Añadimos patentes
                    int limit = 500;
                    //TODO eliminar from
                    String select = @"select distinct ?cv ?idSection ?item 'http://w3id.org/roh/RelatedPatent' as ?rdfTypeAux 
                                        'http://w3id.org/roh/scientificExperience' as ?sectionProperty
                                        'http://w3id.org/roh/patents' as ?auxProperty from <http://gnoss.com/patent.owl> from <http://gnoss.com/person.owl>  ";
                    String where = @$"where{{
                                    {filter}
                                    {{
                                        #DESEABLES                                        
                                        ?person a <http://xmlns.com/foaf/0.1/Person>.                                            
                                        ?item a <http://purl.org/ontology/bibo/Patent>.
                                        ?item <http://w3id.org/roh/crisIdentifier> ?crisIdentifier.
                                        ?item <http://w3id.org/roh/crisIdentifier> ?crisIdentifier.
                                        ?cv a <http://w3id.org/roh/CV>.
                                        ?cv <http://w3id.org/roh/cvOf> ?person.
                                        ?cv <http://w3id.org/roh/scientificExperience> ?idSection.   
                                        ?item <http://purl.org/ontology/bibo/authorList> ?rol.
                                        ?rol <http://www.w3.org/1999/02/22-rdf-syntax-ns#member> ?person.
                                    }}
                                    MINUS
                                    {{
                                        #ACTUALES
                                        ?person a <http://xmlns.com/foaf/0.1/Person>.                                            
                                        ?item a <http://purl.org/ontology/bibo/Patent>.
                                        ?item <http://w3id.org/roh/crisIdentifier> ?crisIdentifier.
                                        ?cv a <http://w3id.org/roh/CV>.
                                        ?cv <http://w3id.org/roh/cvOf> ?person.
                                        ?cv <http://w3id.org/roh/scientificExperience> ?idSection.
                                        ?idSection <http://w3id.org/roh/patents> ?auxSection.
                                        ?auxSection <http://vivoweb.org/ontology/core#relatedBy> ?item.
                                    }}
                                }}order by desc(?cv) limit {limit}";
                    SparqlObject resultado = mResourceApi.VirtuosoQuery(select, where, "curriculumvitae");
                    InsertarItemsCV(resultado);
                    if (resultado.results.bindings.Count != limit)
                    {
                        break;
                    }
                }

                while (true)
                {
                    //Elminamos patentes
                    int limit = 500;
                    //TODO eliminar from 
                    String select = @"select distinct ?cv ?idSection ?auxEntity 
                                        'http://w3id.org/roh/scientificExperience' as ?sectionProperty
                                        'http://w3id.org/roh/patents' as ?auxProperty      
                                        ?group from <http://gnoss.com/patent.owl> from <http://gnoss.com/person.owl> ";
                    String where = @$"where{{
                                    {filter}                                    
                                    {{
                                        #ACTUALES
                                        ?person a <http://xmlns.com/foaf/0.1/Person>.                                            
                                        ?group a <http://xmlns.com/foaf/0.1/Group>.
                                        ?group <http://w3id.org/roh/isValidated> 'true'.
                                        ?cv a <http://w3id.org/roh/CV>.
                                        ?cv <http://w3id.org/roh/cvOf> ?person.
                                        ?cv <http://w3id.org/roh/scientificExperience> ?idSection.
                                        ?idSection <http://w3id.org/roh/groups> ?auxEntity.
                                        ?auxEntity <http://vivoweb.org/ontology/core#relatedBy> ?group.                                
                                    }}
                                    MINUS
                                    {{
                                        #DESEABLES                                        
                                        ?person a <http://xmlns.com/foaf/0.1/Person>.                                            
                                        ?group a <http://xmlns.com/foaf/0.1/Group>.
                                        ?group <http://w3id.org/roh/isValidated> 'true'.
                                        ?cv a <http://w3id.org/roh/CV>.
                                        ?cv <http://w3id.org/roh/cvOf> ?person.
                                        ?cv <http://w3id.org/roh/scientificExperience> ?idSection.       
                                        ?group <http://vivoweb.org/ontology/core#relates> ?rol.
                                        ?rol <http://w3id.org/roh/roleOf> ?person.
                                    }}
                                }}order by desc(?cv) limit {limit}";
                    SparqlObject resultado = mResourceApi.VirtuosoQuery(select, where, "curriculumvitae");
                    EliminarItemsCV(resultado);
                    if (resultado.results.bindings.Count != limit)
                    {
                        break;
                    }
                }

                EliminarDuplicados(pCVs);
            }
        }


        /// <summary>
        /// Modifica los elementos del CV agrgandolos/eliminandolos del CV
        /// Depende de ActualizadorCV.CrearCVs
        /// </summary>
        /// <param name="pPersons">>IDs de las personas</param>
        /// <param name="pCVs">IDs de los CVs</param>
        public void ModificarElementosCV(List<string> pPersons = null, List<string> pCVs = null)
        {
            List<CVSection> listaSecciones = new List<CVSection>();

            //Situación profesional actual
            listaSecciones.Add(new CVSection("010.010.000.000", "position", "http://vivoweb.org/ontology/core#Position", "http://w3id.org/roh/professionalSituation", "http://w3id.org/roh/currentProfessionalSituation", "http://w3id.org/roh/RelatedCurrentProfessionalSituation"));
            //Cargos y actividades desempeñados con anterioridad
            listaSecciones.Add(new CVSection("010.020.000.000", "position", "http://vivoweb.org/ontology/core#Position", "http://w3id.org/roh/professionalSituation", "http://w3id.org/roh/previousPositions", "http://w3id.org/roh/RelatedPreviousPositions"));

            //Estudios de 1º y 2º ciclo, y antiguos ciclos
            listaSecciones.Add(new CVSection("020.010.010.000", "academicdegree", "http://vivoweb.org/ontology/core#AcademicDegree", "http://w3id.org/roh/qualifications", "http://w3id.org/roh/firstSecondCycles", "http://w3id.org/roh/RelatedFirstSecondCycles"));
            //Doctorados
            listaSecciones.Add(new CVSection("020.010.020.000", "academicdegree", "http://vivoweb.org/ontology/core#AcademicDegree", "http://w3id.org/roh/qualifications", "http://w3id.org/roh/doctorates", "http://w3id.org/roh/RelatedDoctorates"));
            //Conocimiento de idiomas
            listaSecciones.Add(new CVSection("020.060.000.000", "languagecertificate", "http://w3id.org/roh/LanguageCertificate", "http://w3id.org/roh/qualifications", "http://w3id.org/roh/languageSkills", "http://w3id.org/roh/RelatedLanguageSkills"));
            //Otra formación universitaria de posgrado
            listaSecciones.Add(new CVSection("020.010.030.000", "academicdegree", "http://vivoweb.org/ontology/core#AcademicDegree", "http://w3id.org/roh/qualifications", "http://w3id.org/roh/postgraduates", "http://w3id.org/roh/RelatedPostGraduates"));
            //Formación especializada
            listaSecciones.Add(new CVSection("020.020.000.000", "academicdegree", "http://vivoweb.org/ontology/core#AcademicDegree", "http://w3id.org/roh/qualifications", "http://w3id.org/roh/specialisedTraining", "http://w3id.org/roh/RelatedSpecialisedTrainings"));
            //Cursos y semin. mejora docente
            listaSecciones.Add(new CVSection("020.050.000.000", "academicdegree", "http://vivoweb.org/ontology/core#AcademicDegree", "http://w3id.org/roh/qualifications", "http://w3id.org/roh/coursesAndSeminars", "http://w3id.org/roh/RelatedCoursesAndSeminars"));

            //Dirección tesis y/o proyectos
            listaSecciones.Add(new CVSection("030.040.000.000", "thesissupervision", "http://w3id.org/roh/ThesisSupervision", "http://w3id.org/roh/teachingExperience", "http://w3id.org/roh/thesisSupervisions", "http://w3id.org/roh/RelatedThesisSupervisions"));
            //Formación académica impartida
            listaSecciones.Add(new CVSection("030.010.000.000", "impartedacademictraining", "http://w3id.org/roh/ImpartedAcademicTraining", "http://w3id.org/roh/teachingExperience", "http://w3id.org/roh/impartedAcademicTrainings", "http://w3id.org/roh/RelatedImpartedAcademicTrainings"));
            //Tutorías académicas
            listaSecciones.Add(new CVSection("030.050.000.000", "tutorship", "http://w3id.org/roh/Tutorship", "http://w3id.org/roh/teachingExperience", "http://w3id.org/roh/academicTutorials", "http://w3id.org/roh/RelatedAcademicTutorials"));
            //Cursos y semin. impartidos
            listaSecciones.Add(new CVSection("030.060.000.000", "impartedcoursesseminars", "http://w3id.org/roh/ImpartedCoursesSeminars", "http://w3id.org/roh/teachingExperience", "http://w3id.org/roh/impartedCoursesSeminars", "http://w3id.org/roh/RelatedImpartedCoursesSeminars"));
            //Publicaciones docentes
            listaSecciones.Add(new CVSection("030.070.000.000", "teachingpublication", "http://w3id.org/roh/TeachingPublication", "http://w3id.org/roh/teachingExperience", "http://w3id.org/roh/teachingPublications", "http://w3id.org/roh/RelatedTeachingPublications"));
            //Participac. proyectos innov. docente
            listaSecciones.Add(new CVSection("030.080.000.000", "teachingproject", "http://w3id.org/roh/TeachingProject", "http://w3id.org/roh/teachingExperience", "http://w3id.org/roh/teachingProjects", "http://w3id.org/roh/RelatedTeachingProjects"));
            //Participac. congresos formac. docente
            listaSecciones.Add(new CVSection("030.090.000.000", "teachingcongress", "http://w3id.org/roh/TeachingCongress", "http://w3id.org/roh/teachingExperience", "http://w3id.org/roh/teachingCongress", "http://w3id.org/roh/RelatedTeachingCongress"));
            //Premios innov. docente
            listaSecciones.Add(new CVSection("060.030.080.000", "accreditation", "http://w3id.org/roh/Accreditation", "http://w3id.org/roh/teachingExperience", "http://w3id.org/roh/teachingInnovationAwardsReceived", "http://w3id.org/roh/RelatedTeachingInnovationAwardsReceived"));
            //Otras actividades
            listaSecciones.Add(new CVSection("030.100.000.000", "activity", "http://w3id.org/roh/Activity", "http://w3id.org/roh/teachingExperience", "http://w3id.org/roh/otherActivities", "http://w3id.org/roh/RelatedOtherActivities"));
            //Aportaciones relevantes
            listaSecciones.Add(new CVSection("030.110.000.000", "activity", "http://w3id.org/roh/Activity", "http://w3id.org/roh/teachingExperience", "http://w3id.org/roh/mostRelevantContributions", "http://w3id.org/roh/RelatedMostRelevantContributions"));

            //Obras artísticas dirigidas
            listaSecciones.Add(new CVSection("050.020.030.000", "supervisedartisticproject", "http://w3id.org/roh/SupervisedArtisticProject", "http://w3id.org/roh/scientificExperience", "http://w3id.org/roh/supervisedArtisticProjects", "http://w3id.org/roh/RelatedSupervisedArtisticProject"));
            //Resultados tecnológicos
            listaSecciones.Add(new CVSection("050.030.020.000", "technologicalresult", "http://w3id.org/roh/TechnologicalResult", "http://w3id.org/roh/scientificExperience", "http://w3id.org/roh/technologicalResults", "http://w3id.org/roh/RelatedTechnologicalResult"));

            //Comités científicos, técnicos y/o asesores
            listaSecciones.Add(new CVSection("060.020.010.000", "committee", "http://w3id.org/roh/Committee", "http://w3id.org/roh/scientificActivity", "http://w3id.org/roh/committees", "http://w3id.org/roh/RelatedCommittee"));
            //Organiz. activ. I+D+i
            listaSecciones.Add(new CVSection("060.020.030.000", "activity", "http://w3id.org/roh/Activity", "http://w3id.org/roh/scientificActivity", "http://w3id.org/roh/activitiesOrganization", "http://w3id.org/roh/RelatedActivityOrganization"));
            //Gestión I+D+i
            listaSecciones.Add(new CVSection("060.020.040.000", "activity", "http://w3id.org/roh/Activity", "http://w3id.org/roh/scientificActivity", "http://w3id.org/roh/activitiesManagement", "http://w3id.org/roh/RelatedActivityManagement"));
            //Producción científica
            listaSecciones.Add(new CVSection("060.010.000.000", "scientificproduction", "http://w3id.org/roh/ScientificProduction", "http://w3id.org/roh/scientificActivity", "http://w3id.org/roh/scientificProduction", "http://w3id.org/roh/RelatedScientificProduction"));
            //Otras actividades divulgación
            listaSecciones.Add(new CVSection("060.010.040.000", "activity", "http://w3id.org/roh/Activity", "http://w3id.org/roh/scientificActivity", "http://w3id.org/roh/otherDisseminationActivities", "http://w3id.org/roh/RelatedOtherDisseminationActivity"));
            //Foros y comités
            listaSecciones.Add(new CVSection("060.020.050.000", "activity", "http://w3id.org/roh/Activity", "http://w3id.org/roh/scientificActivity", "http://w3id.org/roh/forums", "http://w3id.org/roh/RelatedForum"));
            //Evaluación y revisión de proyectos y artículos de I+D+i
            listaSecciones.Add(new CVSection("060.020.060.000", "activity", "http://w3id.org/roh/Activity", "http://w3id.org/roh/scientificActivity", "http://w3id.org/roh/researchEvaluations", "http://w3id.org/roh/RelatedResearchEvaluation"));
            //Estancias en centros I+D+i
            listaSecciones.Add(new CVSection("060.010.050.000", "stay", "http://w3id.org/roh/Stay", "http://w3id.org/roh/scientificActivity", "http://w3id.org/roh/stays", "http://w3id.org/roh/RelatedStay"));
            //Ayudas y becas obtenidas
            listaSecciones.Add(new CVSection("060.030.010.000", "grant", "http://vivoweb.org/ontology/core#Grant", "http://w3id.org/roh/scientificActivity", "http://w3id.org/roh/grants", "http://w3id.org/roh/RelatedGrant"));
            //Otros modos de colaboración
            listaSecciones.Add(new CVSection("060.020.020.000", "collaboration", "http://w3id.org/roh/Collaboration", "http://w3id.org/roh/scientificActivity", "http://w3id.org/roh/otherCollaborations", "http://w3id.org/roh/RelatedOtherCollaboration"));
            //Sdades. Científicas y Asoc. Profesionales
            listaSecciones.Add(new CVSection("060.030.020.000", "society", "http://w3id.org/roh/Society", "http://w3id.org/roh/scientificActivity", "http://w3id.org/roh/societies", "http://w3id.org/roh/RelatedSociety"));
            //Consejos editoriales
            listaSecciones.Add(new CVSection("060.030.030.000", "council", "http://w3id.org/roh/Council", "http://w3id.org/roh/scientificActivity", "http://w3id.org/roh/councils", "http://w3id.org/roh/RelatedCouncil"));
            //Redes de cooperación
            listaSecciones.Add(new CVSection("060.030.040.000", "network", "http://w3id.org/roh/Network", "http://w3id.org/roh/scientificActivity", "http://w3id.org/roh/networks", "http://w3id.org/roh/RelatedNetwork"));
            //Premios, menciones y distinc.
            listaSecciones.Add(new CVSection("060.030.050.000", "accreditation", "http://w3id.org/roh/Accreditation", "http://w3id.org/roh/scientificActivity", "http://w3id.org/roh/prizes", "http://w3id.org/roh/RelatedPrize"));
            //Otras distinc. carrera profes./empr.
            listaSecciones.Add(new CVSection("060.030.060.000", "accreditation", "http://w3id.org/roh/Accreditation", "http://w3id.org/roh/scientificActivity", "http://w3id.org/roh/otherDistinctions", "http://w3id.org/roh/RelatedOtherDistinction"));
            //Períodos activ. investigadora
            listaSecciones.Add(new CVSection("060.030.070.000", "accreditation", "http://w3id.org/roh/Accreditation", "http://w3id.org/roh/scientificActivity", "http://w3id.org/roh/researchActivityPeriods", "http://w3id.org/roh/RelatedResearchActivityPeriod"));
            //Acreditaciones/reconocimientos
            listaSecciones.Add(new CVSection("060.030.090.000", "accreditation", "http://w3id.org/roh/Accreditation", "http://w3id.org/roh/scientificActivity", "http://w3id.org/roh/obtainedRecognitions", "http://w3id.org/roh/RelatedObtainedRecognition"));
            //Resumen de otros méritos
            listaSecciones.Add(new CVSection("060.030.100.000", "accreditation", "http://w3id.org/roh/Accreditation", "http://w3id.org/roh/scientificActivity", "http://w3id.org/roh/otherAchievements", "http://w3id.org/roh/RelatedOtherAchievement"));





            HashSet<string> filters = new HashSet<string>();
            if (pPersons != null && pPersons.Count > 0)
            {
                filters.Add($" FILTER(?person in (<{string.Join(">,<", pPersons)}>))");
            }
            if (pCVs != null && pCVs.Count > 0)
            {
                filters.Add($" FILTER(?cv in (<{string.Join(">,<", pCVs)}>))");
            }
            if (filters.Count == 0)
            {
                filters.Add("");
            }

            List<string> querySectionsAniadir = new List<string>();
            List<string> querySectionsEliminar = new List<string>();
            foreach (CVSection section in listaSecciones)
            {
                string querySectionAniadir = $@"
                                    {{
                                        {{
                                            #DESEABLES
                                            select distinct ?person ?cv ?idSection 
                                            '{section.rdfTypeAux}' as ?rdfTypeAux 
                                            ?item 
                                            '{section.sectionProperty}' as ?sectionProperty   
                                            '{section.itemProperty}' as ?auxProperty   
                                            ?crisIdentifier
                                            Where
                                            {{
                                                ?person a <http://xmlns.com/foaf/0.1/Person>.                                            
                                                ?item a <{section.rdfType}>.
                                                ?item <http://w3id.org/roh/cvnCode> ""{section.cvnCode }"".
                                                ?cv a <http://w3id.org/roh/CV>.
                                                ?cv <http://w3id.org/roh/cvOf> ?person.
                                                ?cv <{section.sectionProperty}> ?idSection.
                                                ?item <http://w3id.org/roh/owner> ?person.
                                                OPTIONAL{{?item <http://w3id.org/roh/crisIdentifier> ?crisIdentifier.}}
                                            }}
                                        }}
                                        MINUS
                                        {{
                                            #ACTUALES
                                            ?person a <http://xmlns.com/foaf/0.1/Person>.      
                                            ?cv a <http://w3id.org/roh/CV>.
                                            ?cv <http://w3id.org/roh/cvOf> ?person.
                                            ?cv <{section.sectionProperty}> ?idSection.
                                            ?idSection <{section.itemProperty}> ?auxEntity.
                                            ?auxEntity <http://vivoweb.org/ontology/core#relatedBy> ?item.        
                                        }}
                                    }}";
                querySectionsAniadir.Add(querySectionAniadir);

                string querySectionEliminar = $@"
                                    {{
                                        {{
                                            #ACTUALES                                            
                                            select distinct ?person ?cv ?idSection ?auxEntity '{section.sectionProperty}' as ?sectionProperty  '{section.itemProperty}' as ?auxProperty  ?item
                                            Where
                                            {{
                                                ?person a <http://xmlns.com/foaf/0.1/Person>.         
                                                ?cv a <http://w3id.org/roh/CV>.
                                                ?cv <http://w3id.org/roh/cvOf> ?person.
                                                ?cv <{section.sectionProperty}> ?idSection.
                                                ?idSection <{section.itemProperty}> ?auxEntity.
                                                ?auxEntity <http://vivoweb.org/ontology/core#relatedBy> ?item.  
                                            }}
                                        }}
                                        MINUS
                                        {{
                                            #DESEABLES
                                            ?person a <http://xmlns.com/foaf/0.1/Person>.                                            
                                            ?item a <{section.rdfType}>.
                                            ?item <http://w3id.org/roh/cvnCode> ""{section.cvnCode }"".
                                            ?cv a <http://w3id.org/roh/CV>.
                                            ?cv <http://w3id.org/roh/cvOf> ?person.
                                            ?cv <{section.sectionProperty}> ?idSection.
                                            ?item <http://w3id.org/roh/owner> ?person.
                                        }}
                                    }}";
                querySectionsEliminar.Add(querySectionEliminar);
            }

            foreach (string filter in filters)
            {
                while (true)
                {

                    //Añadimos items
                    int limit = 500;
                    //TODO eliminar from
                    String select = @$"select distinct ?cv ?idSection ?rdfTypeAux ?item ?sectionProperty ?auxProperty ?crisIdentifier from <http://gnoss.com/person.owl> from <http://gnoss.com/{string.Join(".owl> from <http://gnoss.com/", listaSecciones.Select(x => x.graph))}.owl> ";
                    String where = @$"where{{
                                    {filter}
                                    {string.Join("UNION", querySectionsAniadir)}
                                }}order by desc(?cv) limit {limit}";
                    SparqlObject resultado = mResourceApi.VirtuosoQuery(select, where, "curriculumvitae");
                    InsertarItemsCV(resultado);
                    if (resultado.results.bindings.Count != limit)
                    {
                        break;
                    }
                }

                while (true)
                {
                    //Eliminamos items
                    int limit = 500;
                    //TODO eliminar from
                    String select = @$"select distinct ?cv ?idSection ?auxEntity ?sectionProperty ?auxProperty from <http://gnoss.com/person.owl> from <http://gnoss.com/{string.Join(".owl> from <http://gnoss.com/", listaSecciones.Select(x => x.graph))}.owl> ";
                    String where = @$"where{{
                                    {filter}
                                    {string.Join("UNION", querySectionsEliminar)}
                                }}order by desc(?cv) limit {limit}";
                    SparqlObject resultado = mResourceApi.VirtuosoQuery(select, where, "curriculumvitae");
                    EliminarItemsCV(resultado);
                    if (resultado.results.bindings.Count != limit)
                    {
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Modifica los nombres de las organizaciones en el CV en función de como se llamen
        /// Depende de ActualizadorCV.CrearCVs
        /// </summary>
        /// <param name="pPersons">IDs de las personas</param>
        /// <param name="pCVs">IDs de los CVs</param>
        public void ModificarOrganizacionesCV(List<string> pPersons = null, List<string> pCVs = null)
        {
            List<OrgTitleCVTitleOrg> listaOrgs = new List<OrgTitleCVTitleOrg>();

            listaOrgs.Add(new OrgTitleCVTitleOrg("academicdegree", "http://vivoweb.org/ontology/core#AcademicDegree", "", "http://w3id.org/roh/conductedByTitle", "http://w3id.org/roh/conductedBy"));
            listaOrgs.Add(new OrgTitleCVTitleOrg("academicdegree", "http://vivoweb.org/ontology/core#AcademicDegree", "", "http://w3id.org/roh/deaEntityTitle", "http://w3id.org/roh/deaEntity"));
            listaOrgs.Add(new OrgTitleCVTitleOrg("accreditation", "http://w3id.org/roh/Accreditation", "", "http://w3id.org/roh/accreditationIssuedByTitle", "http://w3id.org/roh/accreditationIssuedBy"));
            listaOrgs.Add(new OrgTitleCVTitleOrg("activity", "http://w3id.org/roh/Activity", "", "http://w3id.org/roh/conductedByTitle", "http://w3id.org/roh/conductedBy"));
            listaOrgs.Add(new OrgTitleCVTitleOrg("activity", "http://w3id.org/roh/Activity", "", "http://w3id.org/roh/promotedByTitle", "http://w3id.org/roh/promotedBy"));
            listaOrgs.Add(new OrgTitleCVTitleOrg("activity", "http://w3id.org/roh/Activity", "", "http://w3id.org/roh/representedEntityTitle", "http://w3id.org/roh/representedEntity"));
            listaOrgs.Add(new OrgTitleCVTitleOrg("collaboration", "http://w3id.org/roh/Collaboration", "http://w3id.org/roh/participates", "http://w3id.org/roh/organizationTitle", "http://w3id.org/roh/organization"));
            listaOrgs.Add(new OrgTitleCVTitleOrg("committee", "http://w3id.org/roh/Committee", "", "http://w3id.org/roh/affiliatedOrganizationTitle", "http://vivoweb.org/ontology/core#affiliatedOrganization"));
            listaOrgs.Add(new OrgTitleCVTitleOrg("council", "http://w3id.org/roh/Council", "", "http://w3id.org/roh/affiliatedOrganizationTitle", "http://vivoweb.org/ontology/core#affiliatedOrganization"));
            listaOrgs.Add(new OrgTitleCVTitleOrg("document", "http://purl.org/ontology/bibo/Document", "", "http://w3id.org/roh/presentedAtOrganizerTitle", "http://w3id.org/roh/presentedAtOrganizer"));
            listaOrgs.Add(new OrgTitleCVTitleOrg("grant", "http://vivoweb.org/ontology/core#Grant", "", "http://w3id.org/roh/awardingEntityTitle", "http://w3id.org/roh/awardingEntity"));
            listaOrgs.Add(new OrgTitleCVTitleOrg("grant", "http://vivoweb.org/ontology/core#Grant", "", "http://w3id.org/roh/entityTitle", "http://w3id.org/roh/entity"));
            listaOrgs.Add(new OrgTitleCVTitleOrg("group", "http://xmlns.com/foaf/0.1/Group", "", "http://w3id.org/roh/affiliatedOrganizationTitle", "http://vivoweb.org/ontology/core#affiliatedOrganization"));
            listaOrgs.Add(new OrgTitleCVTitleOrg("impartedacademictraining", "http://w3id.org/roh/ImpartedAcademicTraining", "", "http://w3id.org/roh/evaluatedByTitle", "http://w3id.org/roh/evaluatedBy"));
            listaOrgs.Add(new OrgTitleCVTitleOrg("impartedacademictraining", "http://w3id.org/roh/ImpartedAcademicTraining", "", "http://w3id.org/roh/financedByTitle", "http://w3id.org/roh/financedBy"));
            listaOrgs.Add(new OrgTitleCVTitleOrg("impartedacademictraining", "http://w3id.org/roh/ImpartedAcademicTraining", "", "http://w3id.org/roh/promotedByTitle", "http://w3id.org/roh/promotedBy"));
            listaOrgs.Add(new OrgTitleCVTitleOrg("impartedcoursesseminars", "http://w3id.org/roh/ImpartedCoursesSeminars", "", "http://w3id.org/roh/promotedByTitle", "http://w3id.org/roh/promotedBy"));
            listaOrgs.Add(new OrgTitleCVTitleOrg("network", "http://w3id.org/roh/Network", "http://w3id.org/roh/participates", "http://w3id.org/roh/organizationTitle", "http://w3id.org/roh/organization"));
            listaOrgs.Add(new OrgTitleCVTitleOrg("network", "http://w3id.org/roh/Network", "", "http://w3id.org/roh/selectionEntityTitle", "http://w3id.org/roh/selectionEntity"));
            listaOrgs.Add(new OrgTitleCVTitleOrg("patent", "http://purl.org/ontology/bibo/Patent", "http://w3id.org/roh/operatingCompanies", "http://w3id.org/roh/organizationTitle", "http://w3id.org/roh/organization"));
            listaOrgs.Add(new OrgTitleCVTitleOrg("patent", "http://purl.org/ontology/bibo/Patent", "", "http://w3id.org/roh/ownerOrganizationTitle", "http://w3id.org/roh/ownerOrganization"));
            listaOrgs.Add(new OrgTitleCVTitleOrg("position", "http://vivoweb.org/ontology/core#Position", "", "http://w3id.org/roh/employerOrganizationTitle", "http://w3id.org/roh/employerOrganization"));
            listaOrgs.Add(new OrgTitleCVTitleOrg("project", "http://vivoweb.org/ontology/core#Project", "http://w3id.org/roh/grantedBy", "http://w3id.org/roh/organizationTitle", "http://w3id.org/roh/organization"));
            listaOrgs.Add(new OrgTitleCVTitleOrg("project", "http://vivoweb.org/ontology/core#Project", "http://w3id.org/roh/participates", "http://w3id.org/roh/organizationTitle", "http://w3id.org/roh/organization"));
            listaOrgs.Add(new OrgTitleCVTitleOrg("project", "http://vivoweb.org/ontology/core#Project", "", "http://w3id.org/roh/conductedByTitle", "http://w3id.org/roh/conductedBy"));
            listaOrgs.Add(new OrgTitleCVTitleOrg("society", "http://w3id.org/roh/Society", "", "http://w3id.org/roh/affiliatedOrganizationTitle", "http://vivoweb.org/ontology/core#affiliatedOrganization"));
            listaOrgs.Add(new OrgTitleCVTitleOrg("stay", "http://w3id.org/roh/Stay", "", "http://w3id.org/roh/entityTitle", "http://w3id.org/roh/entity"));
            listaOrgs.Add(new OrgTitleCVTitleOrg("stay", "http://w3id.org/roh/Stay", "", "http://w3id.org/roh/fundedByTitle", "http://w3id.org/roh/fundedBy"));
            listaOrgs.Add(new OrgTitleCVTitleOrg("teachingcongress", "http://w3id.org/roh/TeachingCongress", "", "http://w3id.org/roh/conductedByTitle", "http://w3id.org/roh/conductedBy"));
            listaOrgs.Add(new OrgTitleCVTitleOrg("teachingproject", "http://w3id.org/roh/TeachingProject", "", "http://w3id.org/roh/fundedByTitle", "http://w3id.org/roh/fundedBy"));
            listaOrgs.Add(new OrgTitleCVTitleOrg("teachingproject", "http://w3id.org/roh/TeachingProject", "http://w3id.org/roh/participates", "http://w3id.org/roh/organizationTitle", "http://w3id.org/roh/organization"));
            listaOrgs.Add(new OrgTitleCVTitleOrg("technologicalresult", "http://w3id.org/roh/TechnologicalResult", "http://w3id.org/roh/participates", "http://w3id.org/roh/organizationTitle", "http://w3id.org/roh/organization"));
            listaOrgs.Add(new OrgTitleCVTitleOrg("technologicalresult", "http://w3id.org/roh/TechnologicalResult", "http://w3id.org/roh/targetOrganizations", "http://w3id.org/roh/organizationTitle", "http://w3id.org/roh/organization"));
            listaOrgs.Add(new OrgTitleCVTitleOrg("thesissupervision", "http://w3id.org/roh/ThesisSupervision", "", "http://w3id.org/roh/promotedByTitle", "http://w3id.org/roh/promotedBy"));
            listaOrgs.Add(new OrgTitleCVTitleOrg("tutorship", "http://w3id.org/roh/Tutorship", "", "http://w3id.org/roh/conductedByTitle", "http://w3id.org/roh/conductedBy"));


            HashSet<string> filters = new HashSet<string>();
            if (pPersons != null && pPersons.Count > 0)
            {
                filters.Add($" FILTER(?person in (<{string.Join(">,<", pPersons)}>))");
            }
            if (pCVs != null && pCVs.Count > 0)
            {
                filters.Add($" FILTER(?cv in (<{string.Join(">,<", pCVs)}>))");
            }
            if (filters.Count == 0)
            {
                filters.Add("");
            }

            foreach (OrgTitleCVTitleOrg section in listaOrgs)
            {
                foreach (string filter in filters)
                {
                    while (true)
                    {
                        string[] propsAux = section.propAux.Split("|", StringSplitOptions.RemoveEmptyEntries);
                        string filterPropAux = "";
                        int i = 0;
                        foreach (string propAux in propsAux)
                        {
                            filterPropAux += $"?s{i} <{propAux}> ?s{i + 1}.\n";
                            i++;
                        }

                        int limit = 500;
                        //TODO eliminar from
                        String select = @$"select * from <http://gnoss.com/organization.owl> ";
                        String where = @$"where{{
                                    ?s0 a <{section.rdfType}>.
                                    {filterPropAux}
                                    OPTIONAL{{?s{i} <{section.propTituloDesnormalizado}> ?orgTituloDesnormalizado.}}
                                    ?s{i} <{section.propOrganizacion}> ?org.
                                    ?org <http://w3id.org/roh/title> ?titleOrg.
                                    FILTER(?orgTituloDesnormalizado!=?titleOrg)
                                }}limit {limit}";
                        SparqlObject resultado = mResourceApi.VirtuosoQuery(select, where, section.graph);
                        Parallel.ForEach(resultado.results.bindings, new ParallelOptions { MaxDegreeOfParallelism = ActualizadorBase.numParallel }, fila =>
                        {
                            //Entidad principal
                            string mainEntity = fila["s0"].value;
                            //Predicado
                            string predicado = section.propTituloDesnormalizado;
                            if (!string.IsNullOrEmpty(section.propAux))
                            {
                                predicado = section.propAux + "|" + predicado;
                            }
                            //Valor antiguo
                            string valorAntiguo = "";
                            if (fila.ContainsKey("orgTituloDesnormalizado"))
                            {
                                valorAntiguo = fila["orgTituloDesnormalizado"].value;
                                List<string> auxEntities = new List<string>();
                                foreach (string key in fila.Keys)
                                {
                                    if (key.StartsWith("s") && key != "s0")
                                    {
                                        auxEntities.Add(fila[key].value);
                                    }
                                }
                                if (auxEntities.Count > 0)
                                {
                                    valorAntiguo = string.Join("|", auxEntities) + "|" + valorAntiguo;
                                }
                            }
                            //Valor nuevo
                            string valorNuevo = fila["titleOrg"].value;
                            {
                                List<string> auxEntities = new List<string>();
                                foreach (string key in fila.Keys)
                                {
                                    if (key.StartsWith("s") && key != "s0")
                                    {
                                        auxEntities.Add(fila[key].value);
                                    }
                                }
                                if (auxEntities.Count > 0)
                                {
                                    valorNuevo = string.Join("|", auxEntities) + "|" + valorNuevo;
                                }
                            }
                            ActualizadorTriple(mainEntity, predicado, valorAntiguo, valorNuevo);
                        });
                        if (resultado.results.bindings.Count != limit)
                        {
                            break;
                        }
                    }
                }
            }


        }

        /// <summary>
        /// Elimina elementos duplicados de los CVs
        /// </summary>
        /// <param name="pCVs">IDs de cvs</param>
        public void EliminarDuplicados(List<string> pCVs = null)
        {
            HashSet<string> filters = new HashSet<string>();            
            if (pCVs != null && pCVs.Count > 0)
            {
                filters.Add($" FILTER(?cv in (<{string.Join(">,<", pCVs)}>))");
            }
            if (filters.Count == 0)
            {
                filters.Add("");
            }
            
            foreach (string filter in filters)
            {
                while (true)
                {
                    int limit = 500;
                    //TODO eliminar from
                    String select = @$"select * where{{ select ?cv ?item count(?o2) as ?numItems  ";
                    String where = @$"  where{{
                                            ?cv a <http://w3id.org/roh/CV>.
                                            ?cv ?p1 ?o1.
                                            ?o1 ?p2 ?o2.
                                            ?o2 <http://vivoweb.org/ontology/core#relatedBy> ?item.
                                            {filter}
                                        }}
                                    }}group by ?cv ?item HAVING (?numItems > 1)  order by desc(?cv) limit {limit}";
                    SparqlObject resultado = mResourceApi.VirtuosoQuery(select, where, "curriculumvitae");
                    Parallel.ForEach(resultado.results.bindings, new ParallelOptions { MaxDegreeOfParallelism = ActualizadorBase.numParallel }, fila =>
                    {
                        string cv = fila["cv"].value;
                        string item = fila["item"].value;

                        String selectIn = @$"select * ";
                        String whereIn = @$"  where{{
                                            <{cv}> ?p1 ?o1.
                                            ?o1 ?p2 ?o2.
                                            ?o2 <http://vivoweb.org/ontology/core#relatedBy> <{item}>.
                                        }}limit 10000 offset 1";
                        SparqlObject resultadoIn = mResourceApi.VirtuosoQuery(selectIn, whereIn, "curriculumvitae");
                        foreach(Dictionary<string,SparqlObject.Data> filain in resultadoIn.results.bindings)
                        {
                            string predicado = filain["p1"].value + "|" + filain["p2"].value;
                            string valorAntiguo = filain["o1"].value + "|" + filain["o2"].value;
                            ActualizadorTriple(cv, predicado, valorAntiguo, "");
                        }
                        
                    });
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
                        string[] nameSplit = name.Split(' ');
                        if (string.IsNullOrEmpty(firstName))
                        {
                            firstName = nameSplit[0];
                        }
                        if (string.IsNullOrEmpty(lastName))
                        {
                            if (nameSplit.Count() > 1)
                            {
                                lastName = nameSplit[1];
                            }
                            else
                            {
                                lastName = nameSplit[0];
                            }

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
        private void InsertarItemsCV(SparqlObject pDatosCargar)
        {
            //cv-->Identificador del CV
            //idSection-->Identificador de la sección del CV
            //rdfTypeAux-->RdfType de la auxiliar
            //item-->Entidad a añadir
            //sectionProperty-->Propiedad que apunta a la sección
            //auxProperty-->Propiedad que apunta de la sección a la auxiliar
            //crisIdentifier-->Identificador (opcional) si tiene valor es público

            Dictionary<Guid, List<TriplesToInclude>> triplesToInclude = new();
            foreach (Dictionary<string, SparqlObject.Data> fila in pDatosCargar.results.bindings)
            {
                string cv = fila["cv"].value;
                string section = fila["idSection"].value;
                string entity = fila["item"].value;

                //Obtenemos la auxiliar en la que cargar la entidad     
                string rdfTypePrefix = AniadirPrefijo(fila["rdfTypeAux"].value);
                rdfTypePrefix = rdfTypePrefix.Substring(rdfTypePrefix.IndexOf(":") + 1);
                string idNewAux = mResourceApi.GraphsUrl + "items/" + rdfTypePrefix + "_" + mResourceApi.GetShortGuid(cv) + "_" + Guid.NewGuid();
                List<TriplesToInclude> listaTriples = new();
                string idEntityAux = section + "|" + idNewAux;

                //Privacidad                  
                string predicadoPrivacidad = fila["sectionProperty"].value + "|" + fila["auxProperty"].value + "|http://w3id.org/roh/isPublic";
                if (fila.ContainsKey("crisIdentifier") && !string.IsNullOrEmpty(fila["crisIdentifier"].value))
                {
                    TriplesToInclude tr2 = new(idEntityAux + "|true", predicadoPrivacidad);
                    listaTriples.Add(tr2);
                }
                else
                {
                    TriplesToInclude tr2 = new(idEntityAux + "|false", predicadoPrivacidad);
                    listaTriples.Add(tr2);
                }

                //Entidad
                string predicadoEntidad = fila["sectionProperty"].value + "|" + fila["auxProperty"].value + "|http://vivoweb.org/ontology/core#relatedBy";
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
        private void EliminarItemsCV(SparqlObject pDatosCargar)
        {
            //cv-->Identificador del CV
            //idSection-->Identificador de la sección del CV
            //auxEntity-->Entidad auxiliar a eliminar
            //sectionProperty-->Propiedad que apunta a la sección
            //auxProperty-->Propiedad que apunta de la sección a la auxiliar

            Dictionary<Guid, List<RemoveTriples>> triplesToDelete = new();
            foreach (Dictionary<string, SparqlObject.Data> fila in pDatosCargar.results.bindings)
            {
                string cv = fila["cv"].value;
                string section = fila["idSection"].value;
                string item = fila["auxEntity"].value;

                RemoveTriples removeTriple = new();
                removeTriple.Predicate = fila["sectionProperty"].value + "|" + fila["auxProperty"].value;
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
