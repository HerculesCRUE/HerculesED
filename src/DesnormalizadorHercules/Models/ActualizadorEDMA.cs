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
    public static class ActualizadorEDMA
    {
        private readonly static string rutaOauth = $@"{System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase}Config/OAuthV3.config";
        private static ResourceApi resourceApi = new ResourceApi(rutaOauth);

        /// <summary>
        /// Actualiza todos los elementos desnormalizados
        /// </summary>
        public static void DesnormalizarTodo()
        {
            try
            {
                ActualizadorCV actualizadorCV = new(resourceApi);
                ActualizadorPerson actualizadorPersonas = new(resourceApi);
                ActualizadorGroup actualizadorGrupos = new(resourceApi);
                ActualizadorDocument actualizadorDocument = new(resourceApi);
                ActualizadorProject actualizadorProject = new(resourceApi);

                //Ejecuciones ordenadas en función de sus dependencias

                //No tienen dependencias
                actualizadorCV.CrearCVs();
                actualizadorGrupos.ActualizarGruposPublicos();
                actualizadorPersonas.ActualizarPersonasPublicas();
                actualizadorProject.ActualizarProyectosPublicos();
                actualizadorDocument.ActualizarPertenenciaGrupos();
                actualizadorDocument.ActualizarNumeroCitasMaximas();
                actualizadorDocument.ActualizarNumeroCitasCargadas();
                actualizadorDocument.ActualizarNumeroReferenciasCargadas();
                actualizadorDocument.ActualizarAreasDocumentos();
                actualizadorDocument.ActualizarTagsDocumentos();
                actualizadorGrupos.ActualizarPertenenciaLineas();
                actualizadorPersonas.ActualizarPertenenciaGrupos();
                actualizadorPersonas.ActualizarPertenenciaLineas();

                actualizadorProject.ActualizarPertenenciaGrupos();
                actualizadorProject.ActualizarNumeroAreasTematicas();
                actualizadorProject.ActualizarNumeroPublicaciones();

                //Dependen únicamente del CV
                actualizadorCV.ModificarDocumentos();
                actualizadorCV.CambiarPrivacidadDocumentos();
                actualizadorCV.ModificarGrupos();
                actualizadorCV.ModificarProyectos();
                actualizadorDocument.ActualizarDocumentosPublicos();
                actualizadorDocument.ActualizarPertenenciaPersonas();

                //Otras dependencias
                actualizadorProject.ActualizarPertenenciaPersonas();
                actualizadorPersonas.ActualizarNumeroPublicaciones();
                actualizadorPersonas.ActualizarNumeroProyectos();
                actualizadorPersonas.ActualizarAreasPersonas();
                actualizadorPersonas.ActualizarNumeroAreasTematicas();
                actualizadorPersonas.ActualizarNumeroColaboradores();
                actualizadorGrupos.ActualizarNumeroMiembros();
                actualizadorGrupos.ActualizarNumeroColaboradores();
                actualizadorGrupos.ActualizarNumeroProyectos();
                actualizadorGrupos.ActualizarNumeroPublicaciones();
                actualizadorGrupos.ActualizarNumeroAreasTematicas();
                actualizadorGrupos.ActualizarAreasGrupos();
                actualizadorProject.ActualizarNumeroColaboradores();
                actualizadorProject.ActualizarNumeroMiembros();
                // actualizadorPersonas.EliminarPersonasNoReferenciadas();



                //Reubicar
                actualizadorDocument.ActualizarIndiceImpacto();
            }
            catch (Exception)
            {

            }
        }


        /// <summary>
        /// Actualiza elementos desnormalizados que afectan a una persona
        /// </summary>
        /// <param name="pPerson">ID de la persona</param>
        public static void DesnormalizarDatosPersona(string pPerson = null)
        {
            try
            {
                ActualizadorCV actualizadorCV = new(resourceApi);
                ActualizadorPerson actualizadorPersonas = new(resourceApi);
                ActualizadorGroup actualizadorGrupos = new(resourceApi);
                ActualizadorDocument actualizadorDocument = new(resourceApi);
                ActualizadorProject actualizadorProject = new(resourceApi);

                //No tienen dependencias
                actualizadorCV.CrearCVs(pPerson);
                actualizadorPersonas.ActualizarPersonasPublicas(pPerson);
                actualizadorPersonas.ActualizarPertenenciaGrupos(pPerson);
                actualizadorPersonas.ActualizarPertenenciaLineas(pPerson);
                actualizadorProject.ActualizarPertenenciaPersonas(pPerson);

                //Dependen únicamente del CV
                actualizadorCV.ModificarDocumentos(pPerson);
                actualizadorCV.CambiarPrivacidadDocumentos(pPerson);
                actualizadorCV.ModificarGrupos(pPerson);
                actualizadorCV.ModificarProyectos(pPerson);
                actualizadorDocument.ActualizarPertenenciaPersonas(pPerson);

                //Otras dependencias
                actualizadorPersonas.ActualizarNumeroPublicaciones(pPerson);
                actualizadorPersonas.ActualizarNumeroProyectos(pPerson);
                actualizadorPersonas.ActualizarAreasPersonas(pPerson);
                actualizadorPersonas.ActualizarNumeroAreasTematicas(pPerson);
                actualizadorPersonas.ActualizarNumeroColaboradores(pPerson);
            }
            catch (Exception)
            {

            }
        }

        /// <summary>
        /// Actualiza elementos desnormalizados que afectan a un poyecto
        /// </summary>
        /// <param name="pProyecto">ID del proyecto</param>
        public static void DesnormalizarDatosProyecto(string pProyecto = null)
        {
            try
            {
                ActualizadorCV actualizadorCV = new(resourceApi);
                ActualizadorPerson actualizadorPersonas = new(resourceApi);
                ActualizadorGroup actualizadorGrupos = new(resourceApi);
                ActualizadorDocument actualizadorDocument = new(resourceApi);
                ActualizadorProject actualizadorProject = new(resourceApi);

                //No tienen dependencias
                actualizadorProject.ActualizarProyectosPublicos(pProyecto);
                actualizadorProject.ActualizarPertenenciaPersonas("", pProyecto);
                actualizadorProject.ActualizarPertenenciaGrupos("", pProyecto);
                actualizadorProject.ActualizarNumeroAreasTematicas(pProyecto);
                actualizadorProject.ActualizarNumeroPublicaciones(pProyecto);

                //Dependen únicamente del CV
                actualizadorCV.ModificarProyectos("", pProyecto);

                //Otras dependencias
                actualizadorProject.ActualizarNumeroColaboradores(pProyecto);
                actualizadorProject.ActualizarNumeroMiembros(pProyecto);
            }
            catch (Exception)
            {

            }
        }

        /// <summary>
        /// Actualiza elementos desnormalizados que afectan a un grupo
        /// </summary>
        /// <param name="pGrupo">ID del grupo</param>
        public static void DesnormalizarDatosGrupo(string pGrupo = null)
        {
            try
            {
                ActualizadorCV actualizadorCV = new(resourceApi);
                ActualizadorPerson actualizadorPersonas = new(resourceApi);
                ActualizadorGroup actualizadorGrupos = new(resourceApi);
                ActualizadorDocument actualizadorDocument = new(resourceApi);
                ActualizadorProject actualizadorProject = new(resourceApi);

                //No tienen dependencias
                actualizadorGrupos.ActualizarGruposPublicos(pGrupo);
                actualizadorDocument.ActualizarPertenenciaGrupos(pGrupo);
                actualizadorGrupos.ActualizarPertenenciaLineas(pGrupo);
                actualizadorPersonas.ActualizarPertenenciaGrupos("", pGrupo);
                actualizadorProject.ActualizarPertenenciaGrupos(pGrupo);

                //Dependen únicamente del CV
                actualizadorCV.ModificarGrupos("", pGrupo);

                //Otras dependencias
                actualizadorGrupos.ActualizarNumeroMiembros(pGrupo);
                actualizadorGrupos.ActualizarNumeroColaboradores(pGrupo);
                actualizadorGrupos.ActualizarNumeroProyectos(pGrupo);
                actualizadorGrupos.ActualizarNumeroPublicaciones(pGrupo);
                actualizadorGrupos.ActualizarNumeroAreasTematicas(pGrupo);
                actualizadorGrupos.ActualizarAreasGrupos(pGrupo);
            }
            catch (Exception)
            {

            }
        }

        /// <summary>
        /// Actualiza elementos desnormalizados que afectan a un documento
        /// </summary>
        /// <param name="pDocumento">ID del documento</param>
        public static void DesnormalizarDatosDocumento(string pDocumento)
        {
            try
            {
                ActualizadorCV actualizadorCV = new(resourceApi);
                ActualizadorPerson actualizadorPersonas = new(resourceApi);
                ActualizadorGroup actualizadorGrupos = new(resourceApi);
                ActualizadorDocument actualizadorDocument = new(resourceApi);
                ActualizadorProject actualizadorProject = new(resourceApi);

                //Ejecuciones ordenadas en función de sus dependencias

                //No tienen dependencias
                actualizadorDocument.ActualizarPertenenciaGrupos("", pDocumento);
                actualizadorDocument.ActualizarNumeroCitasMaximas(pDocumento);
                actualizadorDocument.ActualizarNumeroCitasCargadas(pDocumento);
                actualizadorDocument.ActualizarNumeroReferenciasCargadas(pDocumento);
                actualizadorDocument.ActualizarAreasDocumentos(pDocumento);
                actualizadorDocument.ActualizarTagsDocumentos(pDocumento);

                //Dependen únicamente del CV
                actualizadorCV.ModificarDocumentos("", pDocumento);
                actualizadorCV.CambiarPrivacidadDocumentos("", pDocumento);
                actualizadorDocument.ActualizarDocumentosPublicos(pDocumento);
                actualizadorDocument.ActualizarPertenenciaPersonas("", pDocumento);
            }
            catch (Exception)
            {

            }
        }


        /// <summary>
        /// IMPORTANTE!!! esto sólo debe usarse para pruebas, si se eliminan los datos no son recuperables
        /// Elimina los datos desnormalizados
        /// </summary>
        public static void EliminarDatosDesnormalizados()
        {
            bool eliminarDatos = false;
            //IMPORTANTE!!!
            //No descomentar, esto sólo debe usarse para pruebas, si se eliminan los datos no son recuperables
            if (eliminarDatos)
            {

                //Eliminamos los CV
                while (true)
                {
                    int limit = 500;
                    String select = @"SELECT ?cv ";
                    String where = @$"  where{{
                                            ?cv a <http://w3id.org/roh/CV>.
                                        }} limit {limit}";

                    SparqlObject resultado = resourceApi.VirtuosoQuery(select, where, "curriculumvitae");

                    Parallel.ForEach(resultado.results.bindings, new ParallelOptions { MaxDegreeOfParallelism = ActualizadorBase.numParallel }, fila =>
                    {
                        resourceApi.PersistentDelete(resourceApi.GetShortGuid(fila["cv"].value));
                    });
                    if (resultado.results.bindings.Count != limit)
                    {
                        break;
                    }
                }

                //Modificamos "http://w3id.org/roh/isPublic" de "http://xmlns.com/foaf/0.1/Group"
                ModificarPropiedadDeObjeto("http://w3id.org/roh/isPublic", "http://xmlns.com/foaf/0.1/Group", "group", "false");

                //Modificamos "http://w3id.org/roh/isPublic" de "http://xmlns.com/foaf/0.1/Person"
                ModificarPropiedadDeObjeto("http://w3id.org/roh/isPublic", "http://xmlns.com/foaf/0.1/Person", "person", "false");

                //Modificamos "http://w3id.org/roh/isPublic" de "http://purl.org/ontology/bibo/Document"
                ModificarPropiedadDeObjeto("http://w3id.org/roh/isPublic", "http://purl.org/ontology/bibo/Document", "document", "false");

                //Modificamos "http://w3id.org/roh/isPublic" de "http://vivoweb.org/ontology/core#Project"
                ModificarPropiedadDeObjeto("http://w3id.org/roh/isPublic", "http://vivoweb.org/ontology/core#Project", "project", "false");

                //Eliminamos "http://vivoweb.org/ontology/core#relates" de "http://xmlns.com/foaf/0.1/Person"
                EliminarPropiedadDeObjeto("http://vivoweb.org/ontology/core#relates", "http://xmlns.com/foaf/0.1/Person", "person");

                //Eliminamos "http://w3id.org/roh/lineResearch" de "http://xmlns.com/foaf/0.1/Person"
                EliminarPropiedadDeObjeto("http://w3id.org/roh/lineResearch", "http://xmlns.com/foaf/0.1/Person", "person");

                //Eliminamos "http://w3id.org/roh/publicAuthorList" de "http://purl.org/ontology/bibo/Document"
                EliminarPropiedadDeObjeto("http://w3id.org/roh/publicAuthorList", "http://purl.org/ontology/bibo/Document", "document");

                //Eliminamos "http://w3id.org/roh/publicationsNumber" de "http://xmlns.com/foaf/0.1/Person"
                EliminarPropiedadDeObjeto("http://w3id.org/roh/publicationsNumber", "http://xmlns.com/foaf/0.1/Person", "person");

                //Eliminamos "http://w3id.org/roh/publicAuthorList" de "http://vivoweb.org/ontology/core#Project"
                EliminarPropiedadDeObjeto("http://w3id.org/roh/publicAuthorList", "http://vivoweb.org/ontology/core#Project", "project");

                //Eliminamos "http://w3id.org/roh/projectsNumber" de "http://xmlns.com/foaf/0.1/Person"
                EliminarPropiedadDeObjeto("http://w3id.org/roh/projectsNumber", "http://xmlns.com/foaf/0.1/Person", "person");

                //Eliminamos "http://vivoweb.org/ontology/core#hasResearchArea" de "http://xmlns.com/foaf/0.1/Person"
                EliminarPropiedadDeObjeto("http://vivoweb.org/ontology/core#hasResearchArea", "http://xmlns.com/foaf/0.1/Person", "person");

                //Eliminamos "http://w3id.org/roh/themedAreasNumber" de "http://xmlns.com/foaf/0.1/Person"
                EliminarPropiedadDeObjeto("http://w3id.org/roh/themedAreasNumber", "http://xmlns.com/foaf/0.1/Person", "person");

                //Eliminamos "http://w3id.org/roh/collaboratorsNumber" de "http://xmlns.com/foaf/0.1/Person"
                EliminarPropiedadDeObjeto("http://w3id.org/roh/collaboratorsNumber", "http://xmlns.com/foaf/0.1/Person", "person");

                //Eliminamos "http://w3id.org/roh/researchersNumber" de "http://xmlns.com/foaf/0.1/Group"
                EliminarPropiedadDeObjeto("http://w3id.org/roh/researchersNumber", "http://xmlns.com/foaf/0.1/Group", "group");

                //Eliminamos "http://w3id.org/roh/publicGroupList" de "http://vivoweb.org/ontology/core#Project"
                EliminarPropiedadDeObjeto("http://w3id.org/roh/publicGroupList", "http://vivoweb.org/ontology/core#Project", "project");

                //Eliminamos "http://w3id.org/roh/collaboratorsNumber" de "http://xmlns.com/foaf/0.1/Group"
                EliminarPropiedadDeObjeto("http://w3id.org/roh/collaboratorsNumber", "http://xmlns.com/foaf/0.1/Group", "group");

                //Eliminamos "http://w3id.org/roh/projectsNumber" de "http://xmlns.com/foaf/0.1/Group"
                EliminarPropiedadDeObjeto("http://w3id.org/roh/projectsNumber", "http://xmlns.com/foaf/0.1/Group", "group");

                //Eliminamos "http://w3id.org/roh/isProducedBy" de "http://purl.org/ontology/bibo/Document"
                EliminarPropiedadDeObjeto("http://w3id.org/roh/isProducedBy", "http://purl.org/ontology/bibo/Document", "document");

                //Eliminamos "http://w3id.org/roh/publicationsNumber" de "http://xmlns.com/foaf/0.1/Group"
                EliminarPropiedadDeObjeto("http://w3id.org/roh/publicationsNumber", "http://xmlns.com/foaf/0.1/Group", "group");

                //Eliminamos "http://w3id.org/roh/themedAreasNumber" de "http://xmlns.com/foaf/0.1/Group"
                EliminarPropiedadDeObjeto("http://w3id.org/roh/themedAreasNumber", "http://xmlns.com/foaf/0.1/Group", "group");

                //Eliminamos "http://w3id.org/roh/lineResearch" de "http://xmlns.com/foaf/0.1/Group"
                EliminarPropiedadDeObjeto("http://w3id.org/roh/lineResearch", "http://xmlns.com/foaf/0.1/Group", "group");

                //Eliminamos "http://w3id.org/roh/hasKnowledgeArea" de "http://xmlns.com/foaf/0.1/Group"
                EliminarPropiedadDeObjeto("http://w3id.org/roh/hasKnowledgeArea", "http://xmlns.com/foaf/0.1/Group", "group");

                //Eliminamos "http://w3id.org/roh/citationCount" de "http://purl.org/ontology/bibo/Document"
                ModificarPropiedadDeObjeto("http://w3id.org/roh/citationCount", "http://purl.org/ontology/bibo/Document", "document", "0");

                //Eliminamos "http://w3id.org/roh/citationLoadedCount" de "http://purl.org/ontology/bibo/Document"
                ModificarPropiedadDeObjeto("http://w3id.org/roh/citationLoadedCount", "http://purl.org/ontology/bibo/Document", "document", "0");

                //Eliminamos "http://w3id.org/roh/referencesLoadedCount" de "http://purl.org/ontology/bibo/Document"
                ModificarPropiedadDeObjeto("http://w3id.org/roh/referencesLoadedCount", "http://purl.org/ontology/bibo/Document", "document", "0");

                //Eliminamos "http://w3id.org/roh/hasKnowledgeArea" de "http://purl.org/ontology/bibo/Document"
                EliminarPropiedadDeObjeto("http://w3id.org/roh/hasKnowledgeArea", "http://purl.org/ontology/bibo/Document", "document");

                //Eliminamos "http://vivoweb.org/ontology/core#freeTextKeyword" de "http://purl.org/ontology/bibo/Document"
                EliminarPropiedadDeObjeto("http://vivoweb.org/ontology/core#freeTextKeyword", "http://purl.org/ontology/bibo/Document", "document");

                //Eliminamos "http://w3id.org/roh/themedAreasNumber" de "http://vivoweb.org/ontology/core#Project"
                EliminarPropiedadDeObjeto("http://w3id.org/roh/themedAreasNumber", "http://vivoweb.org/ontology/core#Project", "project");

                //Eliminamos "http://w3id.org/roh/publicationsNumber" de "http://vivoweb.org/ontology/core#Project"
                EliminarPropiedadDeObjeto("http://w3id.org/roh/publicationsNumber", "http://vivoweb.org/ontology/core#Project", "project");

                //Eliminamos "http://w3id.org/roh/collaboratorsNumber" de "http://vivoweb.org/ontology/core#Project"
                EliminarPropiedadDeObjeto("http://w3id.org/roh/collaboratorsNumber", "http://vivoweb.org/ontology/core#Project", "project");

                //Eliminamos "http://w3id.org/roh/researchersNumber" de "http://vivoweb.org/ontology/core#Project"
                EliminarPropiedadDeObjeto("http://w3id.org/roh/researchersNumber", "http://vivoweb.org/ontology/core#Project", "project");
            }
        }

        private static void ModificarPropiedadDeObjeto(string pPropiedad, string pClase, string pGrafo, string pValor)
        {
            while (true)
            {
                int limit = 500;
                String select = @"SELECT ?item ?valor ";
                String where = @$"  where{{
                                            ?item a <{pClase}>.
                                            ?item <{pPropiedad}> ?valor.
                                            FILTER(?valor!='{pValor}')
                                        }}order by asc(?item) limit {limit}";

                SparqlObject resultado = resourceApi.VirtuosoQuery(select, where, pGrafo);

                List<string> ids = resultado.results.bindings.Select(x => x["item"].value).Distinct().ToList();

                Parallel.ForEach(ids, new ParallelOptions { MaxDegreeOfParallelism = ActualizadorBase.numParallel }, id =>
                {
                    Guid guid = resourceApi.GetShortGuid(id);
                    Dictionary<Guid, List<Gnoss.ApiWrapper.Model.TriplesToModify>> triples = new() { { guid, new List<TriplesToModify>() } };
                    foreach (string value in resultado.results.bindings.Where(x => x["item"].value == id).Select(x => x["valor"].value))
                    {
                        TriplesToModify t = new();
                        t.Predicate = pPropiedad;
                        t.NewValue = pValor;
                        t.OldValue = value;
                        triples[guid].Add(t);
                    }
                    resourceApi.ModifyPropertiesLoadedResources(triples);
                });
                if (resultado.results.bindings.Count != limit)
                {
                    break;
                }
            }
        }

        private static void EliminarPropiedadDeObjeto(string pPropiedad, string pClase, string pGrafo)
        {
            while (true)
            {
                int limit = 500;
                String select = @"SELECT ?item ?valor ";
                String where = @$"  where{{
                                            ?item a <{pClase}>.
                                            ?item <{pPropiedad}> ?valor
                                        }}order by asc(?item) limit {limit}";

                SparqlObject resultado = resourceApi.VirtuosoQuery(select, where, pGrafo);

                List<string> ids = resultado.results.bindings.Select(x => x["item"].value).Distinct().ToList();
                Parallel.ForEach(ids, new ParallelOptions { MaxDegreeOfParallelism = ActualizadorBase.numParallel }, id =>
                {
                    Guid guid = resourceApi.GetShortGuid(id);
                    Dictionary<Guid, List<RemoveTriples>> triples = new() { { guid, new List<RemoveTriples>() } };
                    foreach (string value in resultado.results.bindings.Where(x => x["item"].value == id).Select(x => x["valor"].value))
                    {
                        RemoveTriples t = new();
                        t.Predicate = pPropiedad;
                        t.Value = value;
                        triples[guid].Add(t);
                    }
                    resourceApi.DeletePropertiesLoadedResources(triples);
                });
                if (resultado.results.bindings.Count != limit)
                {
                    break;
                }
            }
        }

    }
}
