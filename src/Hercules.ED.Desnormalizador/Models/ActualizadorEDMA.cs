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
        private static CommunityApi communityApi = new CommunityApi(rutaOauth);

        private static Guid communityID = communityApi.GetCommunityId();

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
                ActualizadorRO actualizadorRO = new(resourceApi);

                //CV + dependencias
                actualizadorCV.CrearCVs();
                actualizadorCV.ModificarDocumentos();
                actualizadorCV.CambiarPrivacidadDocumentos();
                actualizadorCV.ModificarResearchObjects();
                actualizadorCV.CambiarPrivacidadResearchObjects();
                actualizadorCV.ModificarGrupos();
                actualizadorCV.ModificarProyectos();

                //Persona sin dependencias
                actualizadorPersonas.ActualizarNumeroPublicacionesValidadas();
                actualizadorPersonas.ActualizarPertenenciaLineas();
                //depende de doc
                actualizadorPersonas.ActualizarAreasPersonas();
                actualizadorPersonas.ActualizarIPGruposActuales(); 
                actualizadorPersonas.ActualizarIPGruposHistoricos();
                actualizadorPersonas.ActualizarIPProyectosActuales();
                actualizadorPersonas.ActualizarIPProyectosHistoricos();

                //Persona con dependencias
                actualizadorPersonas.ActualizarNumeroColaboradoresPublicos();                
                actualizadorPersonas.ActualizarNumeroPublicacionesPublicas();
                actualizadorPersonas.ActualizarNumeroProyectosValidados();
                actualizadorPersonas.ActualizarNumeroProyectosPublicos();
                actualizadorPersonas.ActualizarNumeroAreasTematicas();
                actualizadorPersonas.ActualizarNumeroIPProyectos();

                //Grupo sin dependencias                
                actualizadorGrupos.ActualizarGruposValidados();
                actualizadorGrupos.ActualizarMiembros();
                actualizadorGrupos.ActualizarPertenenciaLineas();

                //Grupo con dependencias
                actualizadorGrupos.ActualizarNumeroMiembros();
                actualizadorGrupos.ActualizarNumeroPublicaciones();                
                actualizadorGrupos.ActualizarNumeroColaboradores();
                actualizadorGrupos.ActualizarNumeroAreasTematicas();
                actualizadorGrupos.ActualizarAreasGrupos();
                actualizadorGrupos.ActualizarNumeroProyectos();
                actualizadorGrupos.ActualizarMiembrosUnificados();

                //Proyectos sin dependencias
                actualizadorProject.ActualizarProyectosValidados();
                actualizadorProject.ActualizarMiembros();
                actualizadorProject.ActualizarPertenenciaGrupos();

                //Proyectos con dependencias
                actualizadorProject.ActualizarNumeroAreasTematicas();
                actualizadorProject.ActualizarNumeroColaboradores();
                actualizadorProject.ActualizarNumeroMiembros();
                actualizadorProject.ActualizarNumeroPublicaciones();
                actualizadorProject.ActualizarMiembrosUnificados();


                //Ejecuciones ordenadas en función de sus dependencias

                //No tienen dependencias



                actualizadorDocument.ActualizarDocumentosValidados();
                actualizadorRO.ActualizarROsValidados();                
                actualizadorDocument.ActualizarPertenenciaGrupos();                       
                actualizadorDocument.ActualizarNumeroCitasMaximas();                
                
                actualizadorDocument.ActualizarAreasDocumentos();
                actualizadorDocument.ActualizarTagsDocumentos();
                actualizadorRO.ActualizarAreasRO();
                actualizadorRO.ActualizarTagsRO();




                actualizadorDocument.ActualizarAnios();

                //TODO agregar la fuente para el factor de impacto
                actualizadorDocument.ActualizarIndicesImpacto();
                //actualizadorDocument.ActualizarIndiceImpacto();

                //TODO hacer bien
                //actualizadorDocument.ActualizarCuartil();

                //Dependen únicamente del CV


                //Desnormalizar posicion de firma

                //Otras dependencias








                //TODO desnormalizar unmnero de IP

                //TODO nombres org


                actualizadorCV.ModificarElementosCV();







            }
            catch (Exception)
            {

            }
        }


        ///// <summary>
        ///// Actualiza elementos desnormalizados que afectan a una persona
        ///// </summary>
        ///// <param name="pPerson">ID de la persona</param>
        //public static void DesnormalizarDatosPersona(string pPerson = null)
        //{
        //    try
        //    {
        //        ActualizadorCV actualizadorCV = new(resourceApi);
        //        ActualizadorPerson actualizadorPersonas = new(resourceApi);
        //        ActualizadorGroup actualizadorGrupos = new(resourceApi);
        //        ActualizadorDocument actualizadorDocument = new(resourceApi);
        //        ActualizadorProject actualizadorProject = new(resourceApi);

        //        //No tienen dependencias
        //        actualizadorCV.CrearCVs(pPerson);
        //        actualizadorPersonas.ActualizarPertenenciaLineas(pPerson);

        //        //Dependen únicamente del CV
        //        actualizadorCV.ModificarDocumentos(pPerson);
        //        actualizadorCV.CambiarPrivacidadDocumentos(pPerson);
        //        actualizadorCV.ModificarGrupos(pPerson);
        //        actualizadorCV.ModificarProyectos(pPerson);

        //        //Otras dependencias
        //        actualizadorPersonas.ActualizarAreasPersonas(pPerson);
        //        actualizadorPersonas.ActualizarNumeroAreasTematicas(pPerson);
        //        actualizadorPersonas.ActualizarNumeroColaboradoresPublicos(pPerson);
        //    }
        //    catch (Exception)
        //    {

        //    }
        //}

        ///// <summary>
        ///// Actualiza elementos desnormalizados que afectan a un poyecto
        ///// </summary>
        ///// <param name="pProyecto">ID del proyecto</param>
        //public static void DesnormalizarDatosProyecto(string pProyecto = null)
        //{
        //    try
        //    {
        //        ActualizadorCV actualizadorCV = new(resourceApi);
        //        ActualizadorPerson actualizadorPersonas = new(resourceApi);
        //        ActualizadorGroup actualizadorGrupos = new(resourceApi);
        //        ActualizadorDocument actualizadorDocument = new(resourceApi);
        //        ActualizadorProject actualizadorProject = new(resourceApi);

        //        //No tienen dependencias
        //        actualizadorProject.ActualizarProyectosValidados(pProyecto);
        //        actualizadorProject.ActualizarPertenenciaGrupos("", pProyecto);
        //        actualizadorProject.ActualizarNumeroAreasTematicas(pProyecto);
        //        actualizadorProject.ActualizarNumeroPublicaciones(pProyecto);

        //        //Dependen únicamente del CV
        //        actualizadorCV.ModificarProyectos("", pProyecto);

        //        //Otras dependencias
        //        actualizadorProject.ActualizarNumeroColaboradores(pProyecto);
        //        actualizadorProject.ActualizarNumeroMiembros(pProyecto);
        //    }
        //    catch (Exception)
        //    {

        //    }
        //}

        ///// <summary>
        ///// Actualiza elementos desnormalizados que afectan a un grupo
        ///// </summary>
        ///// <param name="pGrupo">ID del grupo</param>
        //public static void DesnormalizarDatosGrupo(string pGrupo = null)
        //{
        //    try
        //    {
        //        ActualizadorCV actualizadorCV = new(resourceApi);
        //        ActualizadorPerson actualizadorPersonas = new(resourceApi);
        //        ActualizadorGroup actualizadorGrupos = new(resourceApi);
        //        ActualizadorDocument actualizadorDocument = new(resourceApi);
        //        ActualizadorProject actualizadorProject = new(resourceApi);

        //        //No tienen dependencias
        //        actualizadorGrupos.ActualizarGruposValidados(pGrupo);
        //        actualizadorDocument.ActualizarPertenenciaGrupos(pGrupo);
        //        actualizadorGrupos.ActualizarPertenenciaLineas(pGrupo);
        //        actualizadorProject.ActualizarPertenenciaGrupos(pGrupo);

        //        //Dependen únicamente del CV
        //        actualizadorCV.ModificarGrupos("", pGrupo);

        //        //Otras dependencias
        //        actualizadorGrupos.ActualizarNumeroMiembros(pGrupo);
        //        actualizadorGrupos.ActualizarNumeroColaboradores(pGrupo);
        //        actualizadorGrupos.ActualizarNumeroProyectos(pGrupo);
        //        actualizadorGrupos.ActualizarNumeroPublicaciones(pGrupo);
        //        actualizadorGrupos.ActualizarNumeroAreasTematicas(pGrupo);
        //        actualizadorGrupos.ActualizarAreasGrupos(pGrupo);
        //    }
        //    catch (Exception)
        //    {

        //    }
        //}

        ///// <summary>
        ///// Actualiza elementos desnormalizados que afectan a un documento
        ///// </summary>
        ///// <param name="pDocumento">ID del documento</param>
        //public static void DesnormalizarDatosDocumento(string pDocumento)
        //{
        //    try
        //    {
        //        ActualizadorCV actualizadorCV = new(resourceApi);
        //        ActualizadorPerson actualizadorPersonas = new(resourceApi);
        //        ActualizadorGroup actualizadorGrupos = new(resourceApi);
        //        ActualizadorDocument actualizadorDocument = new(resourceApi);
        //        ActualizadorProject actualizadorProject = new(resourceApi);

        //        //Ejecuciones ordenadas en función de sus dependencias

        //        //No tienen dependencias
        //        actualizadorDocument.ActualizarPertenenciaGrupos("", pDocumento);
        //        actualizadorDocument.ActualizarNumeroCitasMaximas(pDocumento);
        //        actualizadorDocument.ActualizarAreasDocumentos(pDocumento);
        //        actualizadorDocument.ActualizarTagsDocumentos(pDocumento);
        //        actualizadorDocument.ActualizarDocumentosValidados(pDocumento);

        //        //Dependen únicamente del CV
        //        actualizadorCV.ModificarDocumentos("", pDocumento);
        //        actualizadorCV.CambiarPrivacidadDocumentos("", pDocumento);
                
        //    }
        //    catch (Exception)
        //    {

        //    }
        //}

        /// <summary>
        /// IMPORTANTE!!! esto sólo debe usarse para pruebas, si se eliminan los datos no son recuperables
        /// Elimina los datos desnormalizados
        /// </summary>
        public static void EliminarCVs()
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
            }
        }


        /// <summary>
        /// Realiza la fusión de 2 entidades
        /// <param name="pIdMalo">Identificador a eliminar</param>
        /// <param name="pIdBueno">Identificador donde meter la fusión</param>
        /// </summary>
        public static void Fusion(string pIdMalo, string pIdBueno)
        {
            while (true)
            {
                string select = "SELECT DISTINCT ?id ?type ?prop ";
                string where = $@"WHERE {{                                
                                ?id ?prop <http://gnoss/{resourceApi.GetShortGuid(pIdMalo).ToString().ToUpper()}>.
                                OPTIONAL{{?id <http://www.w3.org/1999/02/22-rdf-syntax-ns#type> ?type.}}
                                
                            }} ";
                SparqlObject resultadoQuery = resourceApi.VirtuosoQuery(select, where, communityID);
                if (resultadoQuery.results.bindings.Count == 0)
                {
                    break;
                }
                if (resultadoQuery != null && resultadoQuery.results != null && resultadoQuery.results.bindings != null && resultadoQuery.results.bindings.Count > 0)
                {
                    foreach (Dictionary<string, SparqlObject.Data> fila in resultadoQuery.results.bindings)
                    {
                        string id = fila["id"].value;
                        string prop = fila["prop"].value;
                        //Si el ID es un GUID modificamos directamente
                        if (Guid.TryParse(id.Replace("http://gnoss/", ""), out Guid x))
                        {
                            TriplesToModify t = new();
                            t.NewValue = pIdBueno;
                            t.OldValue = pIdMalo;
                            t.Predicate = prop;
                            resourceApi.ModifyPropertiesLoadedResources(new Dictionary<Guid, List<Gnoss.ApiWrapper.Model.TriplesToModify>>() { { resourceApi.GetShortGuid(id), new List<Gnoss.ApiWrapper.Model.TriplesToModify>() { t } } });
                        }
                        else
                        {
                            Guid main = resourceApi.GetShortGuid(id);
                            string rdftype = resourceApi.VirtuosoQuery("select ?type", "where{<http://gnoss/" + main.ToString().ToUpper() + "> a ?type}", new Guid("b836078b-78a0-4939-b809-3f2ccf4e5c01")).results.bindings[0]["type"].value;
                            List<string> props = new List<string>() { prop };
                            List<string> entities = new List<string>() { id };

                            string oAux = id;
                            while (true)
                            {
                                SparqlObject respuesta = resourceApi.VirtuosoQuery("select ?s ?p", "where{?s ?p <" + oAux + ">. FILTER(?p!=<http://gnoss/hasEntidad> )}", rdftype);
                                if (respuesta.results.bindings.Count == 0)
                                {
                                    entities.Remove(entities.Last());
                                    break;
                                }
                                else
                                {
                                    Dictionary<string, SparqlObject.Data> fila2 = respuesta.results.bindings[0];
                                    if (resourceApi.GetShortGuid(id) != resourceApi.GetShortGuid(fila2["s"].value))
                                    {
                                        entities.Remove(entities.Last());
                                        break;
                                    }
                                    else
                                    {
                                        entities.Add(fila2["s"].value);
                                        props.Add(fila2["p"].value);
                                        oAux = fila2["s"].value;
                                    }
                                }
                            }
                            props.Reverse();
                            entities.Reverse();
                            TriplesToModify t = new();
                            t.NewValue = string.Join("|", entities) + "|" + pIdBueno;
                            t.OldValue = string.Join("|", entities) + "|" + pIdMalo;
                            t.Predicate = string.Join("|", props);
                            resourceApi.ModifyPropertiesLoadedResources(new Dictionary<Guid, List<Gnoss.ApiWrapper.Model.TriplesToModify>>() { { resourceApi.GetShortGuid(id), new List<Gnoss.ApiWrapper.Model.TriplesToModify>() { t } } });
                        }
                    }
                }
            }
            resourceApi.PersistentDelete(resourceApi.GetShortGuid(pIdMalo));
        }


    }
}
