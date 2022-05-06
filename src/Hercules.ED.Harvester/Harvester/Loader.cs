﻿using Gnoss.ApiWrapper;
using Gnoss.ApiWrapper.ApiModel;
using Gnoss.ApiWrapper.Model;
using Harvester.Models;
using Harvester.Models.SGI.Autorizaciones;
using Harvester.Models.SGI.ProteccionIndustrialIntelectual;
using Hercules.MA.ServicioExterno.Controllers.Utilidades;
using Newtonsoft.Json;
using OAI_PMH.Models.SGI;
using OAI_PMH.Models.SGI.Grupos;
using OAI_PMH.Models.SGI.Organization;
using OAI_PMH.Models.SGI.PersonalData;
using OAI_PMH.Models.SGI.Project;
using ProjectauthorizationOntology;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Harvester
{
    public class Loader
    {
        private static Harvester harvester;
        private static IHarvesterServices harvesterServices;
        private static ReadConfig _Config;

        private static string RUTA_PREFIJOS = $@"{System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase}Utilidades/prefijos.json";
        private static string mPrefijos = string.Join(" ", JsonConvert.DeserializeObject<List<string>>(File.ReadAllText(RUTA_PREFIJOS)));

        //Resource API
        public static ResourceApi mResourceApi { get; set; }

        /// <summary>
        /// Main.
        /// </summary>
        /// <param name="pResourceApi">ResourceAPI.</param>
        public Loader(ResourceApi pResourceApi)
        {
            harvesterServices = new IHarvesterServices();
            harvester = new Harvester(harvesterServices);
            _Config = new ReadConfig();
            mResourceApi = pResourceApi;
        }

        /// <summary>
        /// Carga las entidades principales.
        /// </summary>
        public void LoadMainEntities()
        {
            //Dictionary<string, string> dicOrganizaciones = GetEntityBBDD("http://xmlns.com/foaf/0.1/Organization", "organization");
            //Dictionary<string, string> dicPersonas = GetEntityBBDD("http://xmlns.com/foaf/0.1/Person", "person");
            //Dictionary<string, string> dicProyectos = GetEntityBBDD("http://vivoweb.org/ontology/core#Project", "project");
            Dictionary<string, Tuple<string, string>> dicOrganizaciones = new Dictionary<string, Tuple<string, string>>();
            Dictionary<string, Tuple<string, string>> dicPersonas = new Dictionary<string, Tuple<string, string>>();
            Dictionary<string, Tuple<string, string>> dicProyectos = new Dictionary<string, Tuple<string, string>>();
            IniciacionDiccionarios(ref dicProyectos, ref dicPersonas, ref dicOrganizaciones);
            Utilidades.Utilidades.IniciadorDiccionarioPaises();
            Utilidades.Utilidades.IniciadorDiccionarioRegion();

            //TODO eliminar
            goto Testing;

            //Compruebo que no hay ficheros pendientes de procesar
            mResourceApi.ChangeOntoly("organization");
            ProcesarFichero(_Config, "Organizacion", dicOrganizaciones: dicOrganizaciones);
            mResourceApi.ChangeOntoly("person");
            ProcesarFichero(_Config, "Persona", dicPersonas: dicPersonas);
            mResourceApi.ChangeOntoly("project");
            ProcesarFichero(_Config, "Proyecto", dicOrganizaciones, dicProyectos, dicPersonas);
            ProcesarFichero(_Config, "PRC", dicProyectos);
            mResourceApi.ChangeOntoly("projectauthorization");
            ProcesarFichero(_Config, "AutorizacionProyecto");
            mResourceApi.ChangeOntoly("group");
            ProcesarFichero(_Config, "Grupo");
        //TODO: Invencion

        //TODO eliminar
        Testing:

            string fecha = DateTime.Now.ToString("yyyy-MM-ddT00:00:00") + "Z";
            fecha = "2022-01-01T00:00:00Z";

            //Genero los ficheros con los datos a procesar desde la fecha
            GuardarIdentificadores(_Config, "Organizacion", fecha);
            GuardarIdentificadores(_Config, "Persona", fecha);
            GuardarIdentificadores(_Config, "Proyecto", fecha);
            GuardarIdentificadores(_Config, "PRC", fecha, true);
            GuardarIdentificadores(_Config, "AutorizacionProyecto", fecha);
            GuardarIdentificadores(_Config, "Invencion", fecha);
            GuardarIdentificadores(_Config, "Grupo", fecha);

            //Actualizo la última fecha de carga
            UpdateLastDate(_Config, fecha);

            //Proceso los ficheros
            mResourceApi.ChangeOntoly("organization");
            ProcesarFichero(_Config, "Organizacion", dicOrganizaciones: dicOrganizaciones);
            mResourceApi.ChangeOntoly("person");
            ProcesarFichero(_Config, "Persona", dicPersonas: dicPersonas);
            mResourceApi.ChangeOntoly("project");
            ProcesarFichero(_Config, "Proyecto", dicOrganizaciones, dicProyectos, dicPersonas);
            ProcesarFichero(_Config, "PRC", dicProyectos);
            mResourceApi.ChangeOntoly("projectauthorization");
            ProcesarFichero(_Config, "AutorizacionProyecto");
            //mResourceApi.ChangeOntoly("group");
            //ProcesarFichero(_Config, "Grupo");
            //TODO: Invencion
        }

        /// <summary>
        /// Obtiene los identificadores de los datos modificados.
        /// </summary>
        /// <param name="pConfig"></param>
        /// <param name="pSet"></param>
        /// <param name="pFecha"></param>
        public void GuardarIdentificadores(ReadConfig pConfig, string pSet, string pFecha, bool pPRC = false)
        {
            if (pPRC == false)
            {
                harvester.Harvest(pConfig, pSet, pFecha);
            }
            else
            {
                harvester.HarvestPRC(pConfig, pSet, pFecha);
            }
        }

        /// <summary>
        /// Obtiene los datos de los ficheros y los carga.
        /// </summary>
        /// <param name="pConfig"></param>
        /// <param name="pSet"></param>
        /// <param name="dicOrganizaciones"></param>
        /// <param name="dicProyectos"></param>
        /// <param name="dicPersonas"></param>
        public void ProcesarFichero(ReadConfig pConfig, string pSet, [Optional] Dictionary<string, Tuple<string, string>> dicOrganizaciones,
            [Optional] Dictionary<string, Tuple<string, string>> dicProyectos, [Optional] Dictionary<string, Tuple<string, string>> dicPersonas)
        {
            string directorioPendientes = $@"{pConfig.GetLogCargas()}\{pSet}\pending\";
            string directorioProcesados = $@"{pConfig.GetLogCargas()}\{pSet}\processed";

            if (!Directory.Exists(directorioPendientes))
            {
                Directory.CreateDirectory(directorioPendientes);
            }
            if (!Directory.Exists(directorioProcesados))
            {
                Directory.CreateDirectory(directorioProcesados);
            }

            foreach (string fichero in Directory.EnumerateFiles(directorioPendientes))
            {
                string ficheroProcesado = directorioProcesados + fichero.Substring(fichero.LastIndexOf("\\"));
                List<string> idsACargar = File.ReadAllLines(fichero).ToList();

                if (File.Exists(ficheroProcesado))
                {
                    List<string> listaIdsCargados = File.ReadAllLines(ficheroProcesado).ToList();
                    idsACargar = idsACargar.Except(listaIdsCargados).ToList();
                }
                idsACargar.Sort();

                string xmlResult = string.Empty;
                XmlSerializer xmlSerializer = null;
                ComplexOntologyResource resource = null;

                foreach (string id in idsACargar)
                {
                    switch (pSet)
                    {
                        case "Organizacion":

                            // Obtención de datos en bruto.
                            Empresa organization = new Empresa();
                            xmlResult = harvesterServices.GetRecord(id);

                            if (string.IsNullOrEmpty(xmlResult))
                            {
                                File.AppendAllText(ficheroProcesado, id + Environment.NewLine);
                                continue;
                            }

                            xmlSerializer = new(typeof(Empresa));
                            using (StringReader sr = new(xmlResult))
                            {
                                organization = (Empresa)xmlSerializer.Deserialize(sr);
                            }

                            // Cambio de modelo. TODO: Mirar propiedades.
                            OrganizationOntology.Organization empresaOntology = CrearOrganizacionOntology(organization);

                            //Si no me llega el cris identifier o los datos obligatorios salto a la siguiente
                            if (string.IsNullOrEmpty(empresaOntology.Roh_crisIdentifier) && string.IsNullOrEmpty(empresaOntology.Roh_title))
                            {
                                // Guardamos el ID cargado.
                                File.AppendAllText(ficheroProcesado, id + Environment.NewLine);
                                continue;
                            }

                            resource = empresaOntology.ToGnossApiResource(mResourceApi, null);
                            if (dicOrganizaciones.ContainsKey(empresaOntology.Roh_crisIdentifier))
                            {
                                // Modificación.
                                mResourceApi.ModifyComplexOntologyResource(resource, false, false);
                            }
                            else
                            {
                                // Carga.                   
                                mResourceApi.LoadComplexSemanticResource(resource, false, false);
                                dicOrganizaciones[empresaOntology.Roh_crisIdentifier] = new Tuple<string, string>(resource.GnossId, "");
                            }

                            // Guardamos el ID cargado.
                            File.AppendAllText(ficheroProcesado, id + Environment.NewLine);
                            break;

                        case "Persona":

                            // Obtención de datos en bruto.
                            Persona persona = new Persona();
                            xmlResult = harvesterServices.GetRecord(id);

                            if (string.IsNullOrEmpty(xmlResult))
                            {
                                File.AppendAllText(ficheroProcesado, id + Environment.NewLine);
                                continue;
                            }

                            xmlSerializer = new(typeof(Persona));
                            using (StringReader sr = new(xmlResult))
                            {
                                persona = (Persona)xmlSerializer.Deserialize(sr);
                            }


                            // Cambio de modelo. TODO: Mirar propiedades.
                            PersonOntology.Person personOntology = CrearPersona(persona);

                            //Si no me llega el cris identifier o los datos obligatorios salto a la siguiente
                            if (string.IsNullOrEmpty(personOntology.Roh_crisIdentifier) && string.IsNullOrEmpty(personOntology.Foaf_name)
                                && string.IsNullOrEmpty(personOntology.Foaf_firstName) && string.IsNullOrEmpty(personOntology.Foaf_lastName))
                            {
                                // Guardamos el ID cargado.
                                File.AppendAllText(ficheroProcesado, id + Environment.NewLine);
                                continue;
                            }

                            resource = personOntology.ToGnossApiResource(mResourceApi, null);
                            if (dicPersonas.ContainsKey(personOntology.Roh_crisIdentifier))
                            {
                                // Modificación.
                                mResourceApi.ModifyComplexOntologyResource(resource, false, false);
                            }
                            else
                            {
                                // Carga.                   
                                mResourceApi.LoadComplexSemanticResource(resource, false, false);
                                dicPersonas[personOntology.Roh_crisIdentifier] = new Tuple<string, string>(resource.GnossId, "");
                            }

                            // Guardamos el ID cargado.
                            File.AppendAllText(ficheroProcesado, id + Environment.NewLine);
                            break;

                        case "Proyecto":

                            // Obtención de datos en bruto.
                            Proyecto proyecto = new Proyecto();
                            xmlResult = harvesterServices.GetRecord(id);

                            if (string.IsNullOrEmpty(xmlResult))
                            {
                                File.AppendAllText(ficheroProcesado, id + Environment.NewLine);
                                continue;
                            }

                            xmlSerializer = new(typeof(Proyecto));
                            using (StringReader sr = new(xmlResult))
                            {
                                proyecto = (Proyecto)xmlSerializer.Deserialize(sr);
                            }

                            CreacionAuxiliaresProyecto(proyecto, dicProyectos, dicPersonas, dicOrganizaciones);

                            //Si no me llega el cris identifier o los datos obligatorios salto a la siguiente
                            if (string.IsNullOrEmpty(proyecto.Id) && string.IsNullOrEmpty(proyecto.Titulo))
                            {
                                // Guardamos el ID cargado.
                                File.AppendAllText(ficheroProcesado, id + Environment.NewLine);
                                continue;
                            }

                            // Cambio de modelo. TODO: Mirar propiedades.
                            ProjectOntology.Project projectOntology = CrearProyecto(proyecto, dicPersonas: dicPersonas, dicOrganizaciones: dicOrganizaciones);

                            resource = projectOntology.ToGnossApiResource(mResourceApi, null);
                            if (dicProyectos.ContainsKey(projectOntology.Roh_crisIdentifier))
                            {
                                // Modificación.
                                //    mResourceApi.ModifyComplexOntologyResource(resource, false, false);
                            }
                            else
                            {
                                // Carga.                   
                                //    mResourceApi.LoadComplexSemanticResource(resource, false, false);
                                //    pDicRecursosCargados[projectOntology.Roh_crisIdentifier] = resource.GnossId;
                            }

                            // Guardamos el ID cargado.
                            File.AppendAllText(ficheroProcesado, id + Environment.NewLine);
                            break;

                        case "PRC":
                            string idRecurso = id.Split("||")[0];
                            string estado = id.Split("||")[1];

                            Guid guid = mResourceApi.GetShortGuid(idRecurso);
                            Dictionary<string, string> data = GetValues(idRecurso);

                            foreach (KeyValuePair<string, string> item in data)
                            {
                                if (!string.IsNullOrEmpty(item.Value))
                                {
                                    if (item.Key == "projectAux")
                                    {
                                        //Borrado(guid, "http://w3id.org/roh/projectAux", item.Value);
                                    }
                                    else if (item.Key == "status")
                                    {
                                        //Modificacion(guid, "http://w3id.org/roh/validationStatusPRC", estado, item.Value);
                                    }
                                    else
                                    {
                                        switch (estado)
                                        {
                                            case "VALIDADO":
                                                //Modificacion(guid, "http://w3id.org/roh/isValidated", "true", item.Value);
                                                break;
                                            default:
                                                //Modificacion(guid, "http://w3id.org/roh/isValidated", "false", item.Value);
                                                break;
                                        }
                                    }
                                }
                            }

                            // Guardamos el ID cargado.
                            File.AppendAllText(ficheroProcesado, id + Environment.NewLine);
                            break;

                        case "AutorizacionProyecto":
                            // Obtención de datos en bruto.
                            Autorizacion autorizacion = new Autorizacion();
                            xmlResult = harvesterServices.GetRecord(id);

                            if (string.IsNullOrEmpty(xmlResult))
                            {
                                File.AppendAllText(ficheroProcesado, id + Environment.NewLine);
                                continue;
                            }

                            xmlSerializer = new(typeof(Autorizacion));
                            using (StringReader sr = new(xmlResult))
                            {
                                try
                                {
                                    autorizacion = (Autorizacion)xmlSerializer.Deserialize(sr);
                                }
                                catch (Exception)
                                {
                                    continue;
                                }
                            }

                            // Cambio de modelo.
                            ProjectAuthorization projectAuthOntology = CrearAutorizacion(autorizacion);

                            // TODO: Hacer tema del diccionario.
                            //resource = projectAuthOntology.ToGnossApiResource(mResourceApi, null);
                            //if (pDicRecursosCargados.ContainsKey(projectAuthOntology.Roh_crisIdentifier))
                            //{
                            //    // Modificación.
                            //    mResourceApi.ModifyComplexOntologyResource(resource, false, false);
                            //}
                            //else
                            //{
                            //    // Carga.                   
                            //    mResourceApi.LoadComplexSemanticResource(resource, false, false);
                            //    pDicRecursosCargados[projectAuthOntology.Roh_crisIdentifier] = resource.GnossId;
                            //}

                            // Guardamos el ID cargado.
                            File.AppendAllText(ficheroProcesado, id + Environment.NewLine);
                            break;

                        case "Invencion":
                            // Obtención de datos en bruto.
                            Invencion invencion = new Invencion();
                            xmlResult = harvesterServices.GetRecord(id);

                            if (string.IsNullOrEmpty(xmlResult))
                            {
                                File.AppendAllText(ficheroProcesado, id + Environment.NewLine);
                                continue;
                            }

                            xmlSerializer = new(typeof(Invencion));
                            using (StringReader sr = new(xmlResult))
                            {
                                invencion = (Invencion)xmlSerializer.Deserialize(sr);
                            }

                            //Cambio de modelo.TODO: Mirar propiedades.
                            //ProjectAuthorization projectAuthOntology = CrearProyecto(proyecto);

                            //resource = projectAuthOntology.ToGnossApiResource(mResourceApi, null);
                            //if (pDicRecursosCargados.ContainsKey(projectAuthOntology.Roh_crisIdentifier))
                            //{
                            //    // Modificación.
                            //    mResourceApi.ModifyComplexOntologyResource(resource, false, false);
                            //}
                            //else
                            //{
                            //    // Carga.                   
                            //    mResourceApi.LoadComplexSemanticResource(resource, false, false);
                            //    pDicRecursosCargados[projectAuthOntology.Roh_crisIdentifier] = resource.GnossId;
                            //}

                            // Guardamos el ID cargado.
                            File.AppendAllText(ficheroProcesado, id + Environment.NewLine);
                            break;

                        case "Grupo":
                            // Obtención de datos en bruto.
                            Grupo grupo = new Grupo();
                            xmlResult = harvesterServices.GetRecord(id);

                            if (string.IsNullOrEmpty(xmlResult))
                            {
                                File.AppendAllText(ficheroProcesado, id + Environment.NewLine);
                                continue;
                            }

                            xmlSerializer = new(typeof(Invencion));
                            using (StringReader sr = new(xmlResult))
                            {
                                grupo = (Grupo)xmlSerializer.Deserialize(sr);
                            }

                            //Cambio de modelo.TODO: Mirar propiedades.
                            GroupOntology.Group group = CrearGrupo(grupo);

                            resource = group.ToGnossApiResource(mResourceApi, null);
                            //if (pDicRecursosCargados.ContainsKey(projectAuthOntology.Roh_crisIdentifier))
                            //{
                            //    // Modificación.
                            //    mResourceApi.ModifyComplexOntologyResource(resource, false, false);
                            //}
                            //else
                            //{
                            //    // Carga.                   
                            //    mResourceApi.LoadComplexSemanticResource(resource, false, false);
                            //    pDicRecursosCargados[projectAuthOntology.Roh_crisIdentifier] = resource.GnossId;
                            //}

                            // Guardamos el ID cargado.
                            File.AppendAllText(ficheroProcesado, id + Environment.NewLine);
                            break;
                    }

                    // Borra el fichero.
                    File.Delete(fichero);
                }
            }
        }

        private void IniciacionDiccionarios(ref Dictionary<string, Tuple<string, string>> dicProyectos,
           ref Dictionary<string, Tuple<string, string>> dicPersonas, ref Dictionary<string, Tuple<string, string>> dicOrganizaciones)
        {
            dicProyectos = new Dictionary<string, Tuple<string, string>>();
            Dictionary<string, string> dicProyectosAux = GetEntityBBDD("http://vivoweb.org/ontology/core#Project", "project");
            foreach (KeyValuePair<string, string> keyValue in dicProyectosAux)
            {
                dicProyectos.Add(keyValue.Key, new Tuple<string, string>(keyValue.Value, ""));
            }

            dicPersonas = new Dictionary<string, Tuple<string, string>>();
            Dictionary<string, string> dicPersonasAux = GetEntityBBDD("http://xmlns.com/foaf/0.1/Person", "person");
            foreach (KeyValuePair<string, string> keyValue in dicPersonasAux)
            {
                dicPersonas.Add(keyValue.Key, new Tuple<string, string>(keyValue.Value, ""));
            }

            dicOrganizaciones = new Dictionary<string, Tuple<string, string>>();
            Dictionary<string, string> dicOrganizacionesAux = GetEntityBBDD("http://xmlns.com/foaf/0.1/Organization", "organization");
            foreach (KeyValuePair<string, string> keyValue in dicOrganizacionesAux)
            {
                dicOrganizaciones.Add(keyValue.Key, new Tuple<string, string>(keyValue.Value, ""));
            }
        }

        private void CreacionAuxiliaresProyecto(Proyecto proyecto, Dictionary<string, Tuple<string, string>> dicProyectos,
            Dictionary<string, Tuple<string, string>> dicPersonas, Dictionary<string, Tuple<string, string>> dicOrganizaciones)
        {
            List<string> listaOrganizaciones = new List<string>();
            //TODO
            //listaOrganizaciones.AddRange(proyecto.EntidadesGestoras.Select(x => x.EntidadRef).ToList());
            //listaOrganizaciones.AddRange(proyecto.EntidadesConvocantes.Select(x => x.EntidadRef).ToList());
            listaOrganizaciones.AddRange(proyecto.EntidadesFinanciadoras.Select(x => x.EntidadRef).ToList());

            dicProyectos = GetEntityByCRIS("http://vivoweb.org/ontology/core#Project", "project", new List<string>() { proyecto.Id });
            dicPersonas = GetEntityByCRIS("http://xmlns.com/foaf/0.1/Person", "person", proyecto.Equipo.Select(x => x.PersonaRef).ToList());
            dicOrganizaciones = GetEntityByCRIS("http://xmlns.com/foaf/0.1/Organization", "organization", listaOrganizaciones);
        }

        private Dictionary<string, string> GetValues(string pIdRecurso)
        {
            Dictionary<string, string> dicResultados = new Dictionary<string, string>();
            dicResultados.Add("projectAux", "");
            dicResultados.Add("isValidated", "");
            dicResultados.Add("validationStatusPRC", ""); // TODO: Nombre de la propiedad

            string valorEnviado = string.Empty;
            StringBuilder select = new StringBuilder();
            StringBuilder where = new StringBuilder();

            select.Append(mPrefijos);
            select.Append("SELECT DISTINCT ?projectAux ?isValidated ?validationStatusPRC ");
            where.Append("WHERE { ");
            where.Append("?s a bibo:Document. ");
            where.Append("OPTIONAL{?s roh:projectAux ?projectAux. } ");
            where.Append("OPTIONAL{?s roh:isValidated ?isValidated. } ");
            where.Append("OPTIONAL{?s roh:validationStatusPRC ?validationStatusPRC. } "); // TODO: Nombre de la propiedad
            where.Append($@"FILTER(?s = <{pIdRecurso}>) ");
            where.Append("} ");

            SparqlObject resultadoQuery = mResourceApi.VirtuosoQuery(select.ToString(), where.ToString(), "document");

            if (resultadoQuery != null && resultadoQuery.results != null && resultadoQuery.results.bindings != null && resultadoQuery.results.bindings.Count > 0)
            {
                foreach (Dictionary<string, SparqlObject.Data> fila in resultadoQuery.results.bindings)
                {
                    if (fila.ContainsKey("projectAux"))
                    {
                        dicResultados["projectAux"] = UtilidadesAPI.GetValorFilaSparqlObject(fila, "projectAux");
                    }
                    if (fila.ContainsKey("isValidated"))
                    {
                        dicResultados["isValidated"] = UtilidadesAPI.GetValorFilaSparqlObject(fila, "isValidated");
                    }
                    if (fila.ContainsKey("validationStatusPRC"))
                    {
                        dicResultados["validationStatusPRC"] = UtilidadesAPI.GetValorFilaSparqlObject(fila, "validationStatusPRC");
                    }
                }
            }

            return dicResultados;
        }

        /// <summary>
        /// Inserta un triple.
        /// </summary>
        /// <param name="pGuid"></param>
        /// <param name="pPropiedad"></param>
        /// <param name="pValorNuevo"></param>
        private void Insercion(Guid pGuid, string pPropiedad, string pValorNuevo)
        {
            Dictionary<Guid, List<TriplesToInclude>> dicInsercion = new Dictionary<Guid, List<TriplesToInclude>>();
            List<TriplesToInclude> listaTriplesInsercion = new List<TriplesToInclude>();
            TriplesToInclude triple = new TriplesToInclude();
            triple.Predicate = pPropiedad;
            triple.NewValue = pValorNuevo;
            listaTriplesInsercion.Add(triple);
            dicInsercion.Add(pGuid, listaTriplesInsercion);
            mResourceApi.InsertPropertiesLoadedResources(dicInsercion);
        }

        /// <summary>
        /// Modifica un triple.
        /// </summary>
        /// <param name="pGuid"></param>
        /// <param name="pPropiedad"></param>
        /// <param name="pValorNuevo"></param>
        /// <param name="pValorAntiguo"></param>
        private void Modificacion(Guid pGuid, string pPropiedad, string pValorNuevo, string pValorAntiguo)
        {
            Dictionary<Guid, List<TriplesToModify>> dicModificacion = new Dictionary<Guid, List<TriplesToModify>>();
            List<TriplesToModify> listaTriplesModificacion = new List<TriplesToModify>();
            TriplesToModify triple = new TriplesToModify();
            triple.Predicate = pPropiedad;
            triple.NewValue = pValorNuevo;
            triple.OldValue = pValorAntiguo;
            listaTriplesModificacion.Add(triple);
            dicModificacion.Add(pGuid, listaTriplesModificacion);
            mResourceApi.ModifyPropertiesLoadedResources(dicModificacion);
        }

        /// <summary>
        /// Borra un triple.
        /// </summary>
        /// <param name="pGuid"></param>
        /// <param name="pPropiedad"></param>
        /// <param name="pValorAntiguo"></param>
        private void Borrado(Guid pGuid, string pPropiedad, string pValorAntiguo)
        {
            Dictionary<Guid, List<RemoveTriples>> dicBorrado = new Dictionary<Guid, List<RemoveTriples>>();
            List<RemoveTriples> listaTriplesBorrado = new List<RemoveTriples>();
            RemoveTriples triple = new RemoveTriples();
            triple.Predicate = pPropiedad;
            triple.Value = pValorAntiguo;
            triple.Title = false;
            triple.Description = false;
            listaTriplesBorrado.Add(triple);
            dicBorrado.Add(pGuid, listaTriplesBorrado);
            mResourceApi.DeletePropertiesLoadedResources(dicBorrado);
        }

        /// <summary>
        /// Modifica el fichero con la última fecha.
        /// </summary>
        /// <param name="pConfig"></param>
        public void UpdateLastDate(ReadConfig pConfig, string pFecha)
        {
            File.WriteAllText(pConfig.GetLastUpdateDate(), pFecha);
        }

        /// <summary>
        /// Obtiene el ID del recurso junto a su CrisIdentifier.
        /// </summary>
        /// <param name="pOntology"></param>
        /// <returns></returns>
        private Dictionary<string, string> GetEntityBBDD(string pRdfType, string pOntology)
        {
            Dictionary<string, string> dicResultados = new Dictionary<string, string>();

            int limit = 10000;
            int offset = 0;
            while (true)
            {
                SparqlObject resultadoQuery = null;
                StringBuilder select = new StringBuilder(), where = new StringBuilder();

                // Consulta sparql.
                select.Append("SELECT ?s ?crisIdentifier ");
                where.Append("WHERE { ");
                where.Append($@"?s a <{pRdfType}>. ");
                where.Append("?s <http://w3id.org/roh/crisIdentifier> ?crisIdentifier. ");
                where.Append("} ");

                resultadoQuery = mResourceApi.VirtuosoQuery(select.ToString(), where.ToString(), pOntology);

                if (resultadoQuery != null && resultadoQuery.results != null && resultadoQuery.results.bindings != null && resultadoQuery.results.bindings.Count > 0)
                {
                    offset += limit;

                    foreach (Dictionary<string, SparqlObject.Data> fila in resultadoQuery.results.bindings)
                    {
                        string id = fila["crisIdentifier"].value;
                        string nombre = fila["s"].value;
                        dicResultados.Add(id, nombre);
                    }

                    if (resultadoQuery.results.bindings.Count < limit)
                    {
                        break;
                    }
                }
                else
                {
                    break;
                }
            }

            return dicResultados;
        }

        private Dictionary<string, Tuple<string, string>> GetEntityByCRIS(string pRdfType, string pOntology, List<string> lista)
        {
            Dictionary<string, Tuple<string, string>> dicResultados = new Dictionary<string, Tuple<string, string>>();

            int limit = 10000;
            int offset = 0;
            while (true)
            {
                string select = "SELECT DISTINCT ?s ?crisIdentifier ?nombrePersona";
                string filtroProject = $@"strends(?crisIdentifier, '|{string.Join("') OR  strends(?crisIdentifier , '|", lista)}')";//TODO - eliminar por el general
                string filtroGeneral = $@"?crisIdentifier in ('{string.Join("', '", lista)}') ";

                string filtro = pOntology.Equals("project") ? filtroProject : filtroGeneral;
                string where = $@"WHERE {{
    ?s a <{pRdfType}> .
    ?s <http://w3id.org/roh/crisIdentifier> ?crisIdentifier . 
    OPTIONAL{{ ?s <http://xmlns.com/foaf/0.1/name> ?nombrePersona }}
    FILTER(
        {filtro}
    )
}} ";

                SparqlObject resultadoQuery = mResourceApi.VirtuosoQuery(select, where, pOntology);

                if (resultadoQuery != null && resultadoQuery.results != null && resultadoQuery.results.bindings != null && resultadoQuery.results.bindings.Count > 0)
                {
                    offset += limit;

                    foreach (Dictionary<string, SparqlObject.Data> fila in resultadoQuery.results.bindings)
                    {
                        string crisId = fila["crisIdentifier"].value;
                        string identificador = fila["s"].value;
                        string nombrePersona = "";
                        if (fila.ContainsKey("nombrePersona"))
                        {
                            nombrePersona = fila["nombrePersona"].value;
                        }
                        dicResultados.Add(crisId, new Tuple<string, string>(identificador, nombrePersona));
                    }

                    if (resultadoQuery.results.bindings.Count < limit)
                    {
                        break;
                    }
                }
                else
                {
                    break;
                }

            }

            return dicResultados;
        }

        //TODO
        public static PersonOntology.Person CrearPersona(Persona pDatos)
        {
            PersonOntology.Person persona = new PersonOntology.Person();
            persona.Roh_crisIdentifier = pDatos.Id;
            persona.Roh_isSynchronized = true;
            if (!string.IsNullOrEmpty(pDatos.Nombre))
            {
                persona.Foaf_firstName = pDatos.Nombre;
            }
            if (!string.IsNullOrEmpty(pDatos.Apellidos))
            {
                persona.Foaf_lastName = pDatos.Apellidos;
            }
            if (!string.IsNullOrEmpty(pDatos.Nombre) && !string.IsNullOrEmpty(pDatos.Apellidos))
            {
                persona.Foaf_name = pDatos.Nombre + " " + pDatos.Apellidos;
            }
            if (pDatos.Emails != null && pDatos.Emails.Any())
            {
                persona.Vcard_email = new List<string>();
                foreach (Email item in pDatos.Emails)
                {
                    persona.Vcard_email.Add(item.email);
                }
            }
            if (!string.IsNullOrEmpty(pDatos.DatosContacto?.PaisContacto?.Nombre) || !string.IsNullOrEmpty(pDatos.DatosContacto?.ComAutonomaContacto?.Nombre)
                || !string.IsNullOrEmpty(pDatos.DatosContacto?.CiudadContacto) || !string.IsNullOrEmpty(pDatos.DatosContacto?.CodigoPostalContacto)
                || !string.IsNullOrEmpty(pDatos.DatosContacto?.DireccionContacto))
            {
                string direccionContacto = string.IsNullOrEmpty(pDatos.DatosContacto?.DireccionContacto) ? "" : pDatos.DatosContacto.DireccionContacto;
                direccionContacto += string.IsNullOrEmpty(pDatos.DatosContacto?.CodigoPostalContacto) ? "" : ", " + pDatos.DatosContacto.CodigoPostalContacto;
                direccionContacto += string.IsNullOrEmpty(pDatos.DatosContacto?.CiudadContacto) ? "" : ", " + pDatos.DatosContacto.CiudadContacto;
                direccionContacto += string.IsNullOrEmpty(pDatos.DatosContacto?.ProvinciaContacto?.Nombre) ? "" : ", " + pDatos.DatosContacto.ProvinciaContacto.Nombre;
                direccionContacto += string.IsNullOrEmpty(pDatos.DatosContacto?.PaisContacto?.Nombre) ? "" : ", " + pDatos.DatosContacto.PaisContacto.Nombre;

                persona.Vcard_address = direccionContacto;
            }
            if (pDatos.DatosContacto?.Telefonos != null && pDatos.DatosContacto.Telefonos.Any())
            {
                persona.Vcard_hasTelephone = new List<string>();
                foreach (string item in pDatos.DatosContacto.Telefonos)
                {
                    persona.Vcard_hasTelephone.Add(item);
                }
            }
            if (pDatos.Activo.HasValue)
            {
                persona.Roh_isActive = pDatos.Activo.Value;
            }
            // TODO: Posible cambio Treelogic
            if (!string.IsNullOrEmpty(pDatos.Vinculacion?.Departamento?.Id))
            {
                persona.IdVivo_departmentOrSchool = $@"http://gnoss.com/items/department_{pDatos.Vinculacion.Departamento.Id}";//TODO
            }
            if (!string.IsNullOrEmpty(pDatos.Vinculacion?.CategoriaProfesional?.Nombre))
            {
                //Cargo en la universidad
                persona.Roh_hasPosition = pDatos.Vinculacion.CategoriaProfesional.Nombre;
            }

            persona.Roh_lastUpdatedDate = DateTime.UtcNow;
            //insertar en BBDD y asignar gnossid

            return persona;
        }

        private static string IdentificadorPais(string pais)
        {
            return mResourceApi.GraphsUrl + "items/feature_PCLD_" + Utilidades.Utilidades.dicPaises[pais];
        }
        private static string IdentificadorRegion(string region)
        {
            return mResourceApi.GraphsUrl + "items/feature_ADM1_" + Utilidades.Utilidades.dicRegiones[region];
        }

        private static string NombreEmpresa(string id)
        {
            // Obtención de datos en bruto.
            Empresa organization = new Empresa();
            string xmlResult = harvesterServices.GetRecord(id);

            if (string.IsNullOrEmpty(xmlResult))
            {
                return "";
            }

            XmlSerializer xmlSerializer = new(typeof(Empresa));
            using (StringReader sr = new(xmlResult))
            {
                organization = (Empresa)xmlSerializer.Deserialize(sr);
            }
            return organization.Nombre;
        }

        private static List<AcademicdegreeOntology.AcademicDegree> CrearPosgrado(Persona pDatos, Dictionary<string, Tuple<string, string>> dicOrganizaciones)
        {
            List<AcademicdegreeOntology.AcademicDegree> listaPosgrado = new List<AcademicdegreeOntology.AcademicDegree>();
            AcademicdegreeOntology.AcademicDegree academicDegree = new AcademicdegreeOntology.AcademicDegree();
            foreach (OAI_PMH.Models.SGI.FormacionAcademica.Posgrado posgrado in pDatos.Posgrado)
            {
                academicDegree.IdRoh_formationActivityType = FormationType(posgrado.TipoFormacionHomologada.Nombre);
                academicDegree.IdVcard_hasCountryName = IdentificadorPais(posgrado.PaisEntidadTitulacion.Id);
                academicDegree.IdVcard_hasRegion = IdentificadorRegion(posgrado.CcaaRegionEntidadTitulacion.Id);
                academicDegree.Vcard_locality = posgrado.CiudadEntidadTitulacion;
                academicDegree.Dct_issued = posgrado.FechaTitulacion;
                academicDegree.Roh_qualification = posgrado.CalificacionObtenida;
                academicDegree.Roh_approvedDegree = posgrado.TituloHomologado != null ? (bool)posgrado.TituloHomologado : false;
                academicDegree.Roh_approvedDate = posgrado.FechaHomologacion;
                //Titulacion posgrado
                academicDegree.Roh_title = posgrado.NombreTituloPosgrado;

                //Entidad Titulacion
                academicDegree.Roh_conductedByTitle = NombreEmpresa(posgrado.EntidadTitulacion.EntidadRef);
                academicDegree.IdRoh_conductedBy = dicOrganizaciones[posgrado.EntidadTitulacion.EntidadRef].Item1;
            }

            return listaPosgrado;
        }

        /// <summary>
        /// Tipo de formación
        /// </summary>
        /// <param name="tipoFormacion"></param>
        /// <returns></returns>
        private static string FormationType(string tipoFormacion)
        {
            string id = mResourceApi.GraphsUrl + "items/formationtype_";
            switch (tipoFormacion)
            {
                case "Master":
                    id += "034";
                    break;
                case "Posgrado":
                    id += "050";
                    break;
                case "Extensión universitaria":
                    id += "178";
                    break;
                case "Especialidad":
                    id += "179";
                    break;
                default:
                    return null;
            }
            return id;
        }

        /// <summary>
        /// Premio
        /// </summary>
        /// <param name="premio"></param>
        /// <returns></returns>
        private static string PrizeType(string premio)
        {
            string id = mResourceApi.GraphsUrl + "items/prizetype_";
            switch (premio)
            {
                case "Premio extraordinario de licenciatura":
                    id += "000";
                    break;
                case "Premio fin de carrera":
                    id += "010";
                    break;
                case "Texto de otros":
                    id += "OTHERS";
                    break;
                default:
                    return null;
            }
            return id;
        }

        /// <summary>
        /// Tipo de titulación
        /// </summary>
        /// <param name="tipoTitulacion"></param>
        /// <returns></returns>
        private static string UniversityDegreeType(string tipoTitulacion)
        {
            string id = mResourceApi.GraphsUrl + "items/universitydegreetype_";
            switch (tipoTitulacion)
            {
                case "Doctor":
                    id += "940";
                    break;
                case "Titulado medio":
                    id += "950";
                    break;
                case "Titulado superior":
                    id += "960";
                    break;
                case "Texto de otros":
                    id += "OTHERS";
                    break;
                default:
                    return null;
            }
            return id;
        }

        //TODO -comprobar confluence
        /// <summary>
        /// Tipo nota media expediente
        /// </summary>
        /// <param name="qualificationType"></param>
        /// <returns></returns>
        private static string QualificationType(string qualificationType)
        {
            string id = mResourceApi.GraphsUrl + "items/qualificationtype_";
            switch (qualificationType)
            {
                case "Aprobado":
                    id += "000";
                    break;
                case "Notable":
                    id += "010";
                    break;
                case "Sobresaliente":
                    id += "020";
                    break;
                case "Matrícula de Honor":
                    id += "030";
                    break;
                default:
                    return null;
            }
            return id;
        }

        private static List<AcademicdegreeOntology.AcademicDegree> CrearCiclos(Persona pDatos, Dictionary<string, Tuple<string, string>> dicOrganizaciones)
        {
            List<AcademicdegreeOntology.AcademicDegree> listaCiclos = new List<AcademicdegreeOntology.AcademicDegree>();
            AcademicdegreeOntology.AcademicDegree academicDegree = new AcademicdegreeOntology.AcademicDegree();
            foreach (OAI_PMH.Models.SGI.FormacionAcademica.Ciclos ciclo in pDatos.Ciclos)
            {
                academicDegree.IdVcard_hasCountryName = IdentificadorPais(ciclo.PaisEntidadTitulacion.Id);
                academicDegree.IdVcard_hasRegion = IdentificadorRegion(ciclo.CcaaRegionEntidadTitulacion.Id);
                academicDegree.Vcard_locality = ciclo.CiudadEntidadTitulacion;
                academicDegree.Dct_issued = ciclo.FechaTitulacion;
                //TODO academicDegree.IdRoh_mark = QualificationType(ciclo.NotaMediaExpediente);
                academicDegree.Roh_approvedDate = ciclo.FechaHomologacion;
                academicDegree.Roh_approvedDegree = ciclo.TituloHomologado != null ? (bool)ciclo.TituloHomologado : false;

                //Tipo titulacion
                //academicDegree.IdRoh_universityDegreeType = UniversityDegreeType(ciclo.tit);

                //Premio
                academicDegree.IdRoh_prize = PrizeType(ciclo.Premio.Id);

                //Titulacion
                academicDegree.Roh_title = ciclo.NombreTitulo;

                //Titulo Extranjero
                academicDegree.Roh_foreignTitle = ciclo.TituloExtranjero;

                //Entidad titulacion
                academicDegree.Roh_conductedByTitle = NombreEmpresa(ciclo.EntidadTitulacion.EntidadRef);
                academicDegree.IdRoh_conductedBy = dicOrganizaciones[ciclo.EntidadTitulacion.EntidadRef].Item1;

                listaCiclos.Add(academicDegree);
            }
            return listaCiclos;
        }

        private static List<AcademicdegreeOntology.AcademicDegree> CrearDoctorados(Persona pDatos, Dictionary<string, Tuple<string, string>> dicOrganizaciones,
            Dictionary<string, Tuple<string, string>> dicPersonas)
        {
            List<AcademicdegreeOntology.AcademicDegree> listadoDoctorados = new List<AcademicdegreeOntology.AcademicDegree>();
            AcademicdegreeOntology.AcademicDegree academicDegree = new AcademicdegreeOntology.AcademicDegree();
            foreach (OAI_PMH.Models.SGI.FormacionAcademica.Doctorados doctorado in pDatos.Doctorados)
            {
                academicDegree.Roh_deaDate = doctorado.FechaTitulacionDEA;
                academicDegree.IdVcard_hasCountryName = IdentificadorPais(doctorado.PaisEntidadTitulacion.Id);
                academicDegree.IdVcard_hasRegion = IdentificadorRegion(doctorado.CcaaRegionEntidadTitulacion.Id);
                academicDegree.Vcard_locality = doctorado.CiudadEntidadTitulacion;
                academicDegree.Dct_issued = doctorado.FechaTitulacion;
                academicDegree.Roh_thesisTitle = doctorado.TituloTesis;
                academicDegree.Roh_qualification = doctorado.CalificacionObtenida;
                academicDegree.Roh_qualityMention = doctorado.MencionCalidad != null ? (bool)doctorado.MencionCalidad : false;

                //Programa doctorado
                academicDegree.Roh_title = doctorado.ProgramaDoctorado;

                //Entidad titulacion
                academicDegree.Roh_conductedByTitle = NombreEmpresa(doctorado.EntidadTitulacion.EntidadRef);
                academicDegree.IdRoh_conductedBy = dicOrganizaciones[doctorado.EntidadTitulacion.EntidadRef].Item1;

                //Entidad titulacion DEA
                academicDegree.Roh_deaEntityTitle = NombreEmpresa(doctorado.EntidadTitulacionDEA.EntidadRef);
                academicDegree.IdRoh_deaEntity = dicOrganizaciones[doctorado.EntidadTitulacionDEA.EntidadRef].Item1;

                //Director tesis
                //TODO metodo cargar datos persona
                //academicDegree.Roh_directorName = dicPersonas[doctorado.DirectorTesis].Item1;

                //Codirectores
                //academicDegree.Roh_codirector.Add = dicPersonas[doctorado.CoDirectorTesis].Item1;

                //Doctorado UE
                academicDegree.Roh_europeanDoctorate = doctorado.DoctoradoEuropeo != null ? (bool)doctorado.DoctoradoEuropeo : false;
                academicDegree.Roh_europeanDoctorateDate = doctorado.FechaMencionDoctoradoEuropeo;

                //Premio extraordinario
                academicDegree.Roh_doctorExtraordinaryAward = doctorado.PremioExtraordinarioDoctor != null ? (bool)doctorado.PremioExtraordinarioDoctor : false;
                academicDegree.Roh_doctorExtraordinaryAwardDate = doctorado.FechaPremioExtraordinarioDoctor;

                //Titulo homologado
                academicDegree.Roh_approvedDegree = doctorado.TituloHomologado != null ? (bool)doctorado.TituloHomologado : false;
                academicDegree.Roh_approvedDate = doctorado.FechaHomologacion;

                listadoDoctorados.Add(academicDegree);
            }

            return listadoDoctorados;
        }

        /// <summary>
        /// Tipo de formación
        /// </summary>
        /// <param name="formationActivityType"></param>
        /// <returns></returns>
        private static string FormationActivityType(string formationActivityType)
        {
            string id = mResourceApi.GraphsUrl + "items/formationactivitytype_";
            switch (formationActivityType)
            {
                case "Curso":
                    id += "011";
                    break;
                case "Prácticas":
                    id += "051";
                    break;
                case "Estancias":
                    id += "184";
                    break;
                case "Texto de otros":
                    id += "OTHERS";
                    break;
                default:
                    return null;
            }
            return id;
        }

        private static List<AcademicdegreeOntology.AcademicDegree> CrearFormacionEspecializada(Persona pDatos, Dictionary<string, Tuple<string, string>> dicOrganizaciones,
           Dictionary<string, Tuple<string, string>> dicPersonas)
        {
            List<AcademicdegreeOntology.AcademicDegree> listaFormacionEspecializada = new List<AcademicdegreeOntology.AcademicDegree>();
            AcademicdegreeOntology.AcademicDegree academicDegree = new AcademicdegreeOntology.AcademicDegree();
            foreach (OAI_PMH.Models.SGI.FormacionAcademica.FormacionEspecializada formacionEspecializada in pDatos.FormacionEspecializada)
            {
                academicDegree.IdRoh_formationActivityType = FormationActivityType(formacionEspecializada.TipoFormacion.Nombre);
                academicDegree.Roh_title = formacionEspecializada.NombreTitulo;
                academicDegree.IdVcard_hasCountryName = IdentificadorPais(formacionEspecializada.PaisEntidadTitulacion.Id);
                academicDegree.IdVcard_hasRegion = IdentificadorRegion(formacionEspecializada.CcaaRegionEntidadTitulacion.Id);
                academicDegree.Vcard_locality = formacionEspecializada.CiudadEntidadTitulacion;
                academicDegree.Roh_goals = formacionEspecializada.Objetivos;
                academicDegree.Roh_durationHours = formacionEspecializada.DuracionTitulacion;
                academicDegree.Vivo_end = formacionEspecializada.FechaTitulacion;

                //Entidad titulacion
                academicDegree.Roh_conductedByTitle = NombreEmpresa(formacionEspecializada.EntidadTitulacion.EntidadRef);
                academicDegree.IdRoh_conductedBy = dicOrganizaciones[formacionEspecializada.EntidadTitulacion.EntidadRef].Item1;

                //Responsable
                //academicDegree.Roh_trainerNick = formacionEspecializada.ResponsableFormacion;

                listaFormacionEspecializada.Add(academicDegree);
            }
            return listaFormacionEspecializada;
        }

        /// <summary>
        /// Tipo de trabajo dirigido
        /// </summary>
        /// <param name="projectCharacterType"></param>
        /// <returns></returns>
        private static string ProjectCharacterType(string projectCharacterType)
        {
            string id = mResourceApi.GraphsUrl + "items/projectcharactertype_";
            switch (projectCharacterType)
            {
                case "Proyecto de fin de carrera":
                    id += "055";
                    break;
                case "Tesina":
                    id += "066";
                    break;
                case "Tesis Doctoral":
                    id += "067";
                    break;
                case "Trabajo conducente a la obtención de DEA":
                    id += "071";
                    break;
                case "Texto de otros":
                    id += "OTHERS";
                    break;
                default:
                    return null;
            }
            return id;
        }

        private static List<ThesissupervisionOntology.ThesisSupervision> CrearTesis(Persona pDatos, Dictionary<string, Tuple<string, string>> dicOrganizaciones,
           Dictionary<string, Tuple<string, string>> dicPersonas)
        {
            List<ThesissupervisionOntology.ThesisSupervision> listaTesis = new List<ThesissupervisionOntology.ThesisSupervision>();
            ThesissupervisionOntology.ThesisSupervision thesisSupervision = new ThesissupervisionOntology.ThesisSupervision();
            foreach (OAI_PMH.Models.SGI.ActividadDocente.Tesis tesis in pDatos.Tesis)
            {
                thesisSupervision.IdRoh_projectCharacterType = ProjectCharacterType(tesis.TipoProyecto.Nombre);
                thesisSupervision.Roh_title = tesis.TituloTrabajo;
                thesisSupervision.IdVcard_hasCountryName = IdentificadorPais(tesis.PaisEntidadRealizacion.Id);
                thesisSupervision.IdVcard_hasRegion = IdentificadorRegion(tesis.CcaaRegionEntidadRealizacion.Id);
                thesisSupervision.Vcard_locality = tesis.CiudadEntidadRealizacion;
                thesisSupervision.Dct_issued = tesis.FechaDefensa;
                thesisSupervision.Roh_qualification = tesis.CalificacionObtenida;
                thesisSupervision.Roh_europeanDoctorateDate = tesis.FechaMencionDoctoradoEuropeo;
                thesisSupervision.Roh_qualityMention = tesis.MencionCalidad != null ? (bool)tesis.MencionCalidad : false;
                thesisSupervision.Roh_europeanDoctorate = tesis.DoctoradoEuropeo != null ? (bool)tesis.DoctoradoEuropeo : false;
                thesisSupervision.Roh_qualityMentionDate = tesis.FechaMencionCalidad;

                //Palabras clave
                //TODO ?

                //Entidad realizacion
                thesisSupervision.IdRoh_promotedBy = dicOrganizaciones[tesis.EntidadRealizacion.EntidadRef].Item1;
                thesisSupervision.Roh_promotedByTitle = NombreEmpresa(tesis.EntidadRealizacion.EntidadRef);

                //Alumno TODO - check
                Persona alumno = ObtenerPersona(tesis.Alumno);
                thesisSupervision.Roh_studentName = alumno.Nombre;
                thesisSupervision.Roh_studentFirstSurname = alumno.Apellidos;
                thesisSupervision.Roh_studentNick = alumno.Nombre + " " + alumno.Apellidos;

                //Codirectores
                //thesisSupervision.cod = dicPersonas[tesis.CoDirectorTesis.PersonaRef].Item1;

                listaTesis.Add(thesisSupervision);
            }
            return listaTesis;
        }

        /// <summary>
        /// Tipo de labor docente
        /// </summary>
        /// <param name="teachingtype"></param>
        /// <returns></returns>
        private static string TeachingType(string teachingtype)
        {
            string id = mResourceApi.GraphsUrl + "items/teachingtype_";
            switch (teachingtype)
            {
                case "Docencia internacional":
                    id += "014";
                    break;
                case "Docencia oficial":
                    id += "015";
                    break;
                case "Docencia no oficial":
                    id += "016";
                    break;
                default:
                    return null;
            }
            return id;
        }

        /// <summary>
        /// Tipo de trabajo de programa.
        /// </summary>
        /// <param name="programType"></param>
        /// <returns></returns>
        private static string ProgramType(string programType)
        {
            string id = mResourceApi.GraphsUrl + "items/programtype_";
            switch (programType)
            {
                case "Arquitectura":
                    id += "020";
                    break;
                case "Arquitectura técnica":
                    id += "030";
                    break;
                case "Diplomatura":
                    id += "240";
                    break;
                case "Doctorado":
                    id += "250";
                    break;
                case "Ingeniería":
                    id += "420";
                    break;
                case "Ingeniería Técnica":
                    id += "430";
                    break;
                case "Licenciatura":
                    id += "470";
                    break;
                case "Máster oficial":
                    id += "480";
                    break;
                case "Texto de otros":
                    id += "OTHERS";
                    break;
                default:
                    return null;
            }
            return id;
        }

        /// <summary>
        /// Tipo de docencia
        /// </summary>
        /// <param name="modalityTeachingType"></param>
        /// <returns></returns>
        private static string ModalityTeachingType(string modalityTeachingType)
        {
            string id = mResourceApi.GraphsUrl + "items/modalityteachingtype_";
            switch (modalityTeachingType)
            {
                case "Clínico":
                    id += "060";
                    break;
                case "Prácticas de Laboratorio":
                    id += "700";
                    break;
                case "Práctica (Aula-Problemas)":
                    id += "705";
                    break;
                case "Teórica presencial":
                    id += "840";
                    break;
                case "Virtual":
                    id += "860";
                    break;
                case "Texto de otros":
                    id += "OTHERS";
                    break;
                default:
                    return null;
            }
            return id;
        }

        /// <summary>
        /// Tipo de asignatura
        /// </summary>
        /// <param name="courseType"></param>
        /// <returns></returns>
        private static string CourseType(string courseType)
        {
            string id = mResourceApi.GraphsUrl + "items/coursetype_";
            switch (courseType)
            {
                case "Troncal":
                    id += "000";
                    break;
                case "Optativa":
                    id += "020";
                    break;
                case "Obligatoria":
                    id += "010";
                    break;
                case "Libre configuración":
                    id += "030";
                    break;
                case "Doctorado/a":
                    id += "050";
                    break;
                case "Texto de otros":
                    id += "OTHERS";
                    break;
                default:
                    return null;
            }
            return id;
        }

        /// <summary>
        /// hourscreditsectstype_" + hoursCreditsECTSType
        /// </summary>
        /// <param name="hoursCreditsECTSType"></param>
        /// <returns></returns>
        private static string HoursCreditsECTSType(string hoursCreditsECTSType)
        {
            string id = mResourceApi.GraphsUrl + "items/hourscreditsectstype_";
            switch (hoursCreditsECTSType)
            {
                case "Creditos":
                    id += "000";
                    break;
                case "No competitivo":
                    id += "010";
                    break;
                default:
                    return null;
            }
            return id;
        }

        /// <summary>
        /// language_" + language
        /// </summary>
        /// <param name="language"></param>
        /// <returns></returns>
        private static string Language(string language)
        {
            return mResourceApi.GraphsUrl + "items/language_" + language;
        }

        /// <summary>
        /// Tipo de convocatoria
        /// </summary>
        /// <param name="courseType"></param>
        /// <returns></returns>
        private static string CallType(string callType)
        {
            string id = mResourceApi.GraphsUrl + "items/calltype_";
            switch (callType)
            {
                case "Competitivo":
                    id += "1040";
                    break;
                case "No competitivo":
                    id += "1050";
                    break;
                case "Texto de otros":
                    id += "OTHERS";
                    break;
                default:
                    return null;
            }
            return id;
        }

        /// <summary>
        /// Tipo del ámbito geográfico
        /// </summary>
        /// <param name="geographicRegion"></param>
        /// <returns></returns>
        private static string GeographicRegion(string geographicRegion)
        {
            string id = mResourceApi.GraphsUrl + "items/geographicregion_";
            switch (geographicRegion)
            {
                case "Autonómica":
                    id += "000";
                    break;
                case "Internacional no UE":
                    id += "030";
                    break;
                case "Nacional":
                    id += "010";
                    break;
                case "Unión Europea":
                    id += "020";
                    break;
                case "Texto de otros":
                    id += "OTHERS";
                    break;
                default:
                    return null;
            }
            return id;
        }

        private static List<ImpartedacademictrainingOntology.ImpartedAcademicTraining> CrearFormacionAcademicaImpartida(Persona pDatos, Dictionary<string, Tuple<string, string>> dicOrganizaciones,
           Dictionary<string, Tuple<string, string>> dicPersonas)
        {
            List<ImpartedacademictrainingOntology.ImpartedAcademicTraining> listaFormacionAcademicaImpartida = new List<ImpartedacademictrainingOntology.ImpartedAcademicTraining>();
            ImpartedacademictrainingOntology.ImpartedAcademicTraining academicTraining = new ImpartedacademictrainingOntology.ImpartedAcademicTraining();
            foreach (OAI_PMH.Models.SGI.ActividadDocente.FormacionAcademicaImpartida formacionAcademica in pDatos.FormacionAcademicaImpartida)
            {
                academicTraining.IdRoh_teachingType = TeachingType(formacionAcademica.TipoDocente.Nombre);
                academicTraining.IdVcard_hasCountryName = IdentificadorPais(formacionAcademica.PaisEntidadRealizacion.Id);
                academicTraining.IdVcard_hasRegion = IdentificadorRegion(formacionAcademica.CcaaRegionEntidadRealizacion.Id);
                academicTraining.Vcard_locality = formacionAcademica.CiudadEntidadRealizacion;
                academicTraining.Roh_department = formacionAcademica.Departamento;
                academicTraining.IdRoh_programType = ProgramType(formacionAcademica.TipoPrograma.Nombre);
                academicTraining.Roh_teaches = formacionAcademica.NombreAsignaturaCurso;
                academicTraining.IdRoh_modalityTeachingType = ModalityTeachingType(formacionAcademica.TipoDocencia.Nombre);
                academicTraining.IdRoh_courseType = CourseType(formacionAcademica.TipoAsignatura.Nombre);
                academicTraining.Roh_course = formacionAcademica.Curso;
                academicTraining.IdRoh_hoursCreditsECTSType = HoursCreditsECTSType(formacionAcademica.TipoHorasCreditos.Nombre);
                academicTraining.Roh_numberECTSHours = formacionAcademica.NumHorasCreditos != null ? (float?)formacionAcademica.NumHorasCreditos : null;
                academicTraining.IdVcard_hasLanguage = Language(formacionAcademica.Idioma);
                academicTraining.Roh_frequency = (float?)formacionAcademica.FrecuenciaActividad;
                academicTraining.Roh_competencies = formacionAcademica.Competencias;
                academicTraining.Roh_professionalCategory = formacionAcademica.CategoriaProfesional;
                //academicTraining.Roh_qualification = formacionAcademica.calificacion;
                //academicTraining.Roh_maxQualification = formacionAcademica.maxcalificacion;
                //academicTraining.IdRoh_evaluatedByHasCountryName = IdentificadorPais(formacionAcademica.EntidadEvaluacion.pais.id);
                //academicTraining.evaluatedregion = IdentificadorRegion(formacionAcademica.EntidadEvaluacion.region.id);
                //academicTraining.Roh_evaluatedByLocality = formacionAcademica.EntidadEvaluacion.ciudad;
                //academicTraining.IdRoh_evaluationType = EvaluationType(formacionAcademica.TipoEvaluacion) ; 
                //academicTraining.IdRoh_financedByHasCountryName = IdentificadorPais(formacionAcademica.EntidadFinanciadora);
                //academicTraining.IdRoh_financedByHasRegion = IdentificadorRegion(formacionAcademica.EntidadFinanciadora);
                //academicTraining.Roh_financedByLocality = formacionAcademica.EntidadFinanciadora.ciudad;
                //academicTraining.IdRoh_callType = CallType(formacionAcademica.TipoConvocatoria);
                academicTraining.IdVivo_geographicFocus = GeographicRegion(formacionAcademica.AmbitoGeografico);
                //academicTraining.Roh_center = formacionAcademica.facultad;
                academicTraining.Vivo_start = formacionAcademica.FechaInicio;
                academicTraining.Vivo_end = formacionAcademica.FechaFinalizacion;

                //Titulacion universitaria
                academicTraining.Roh_title = formacionAcademica.TitulacionUniversitaria;

                //Entidad realizacion
                academicTraining.IdRoh_promotedBy = dicOrganizaciones[formacionAcademica.EntidadRealizacion.EntidadRef].Item1;
                academicTraining.Roh_promotedByTitle = NombreEmpresa(formacionAcademica.EntidadRealizacion.EntidadRef);

                //Entidad financiadora
                //academicTraining.IdRoh_financedBy = dicOrganizaciones[formacionAcademica.EntidadFinanciadora.EntidadRef].Item1;
                //academicTraining.Roh_financedByTitle =  NombreEmpresa(formacionAcademica.EntidadFinanciadora);

                //Entidad evaluacion
                //academicTraining.IdRoh_evaluatedBy = dicOrganizaciones[formacionAcademica.EntidadEvaluacion.EntidadRef].Item1;
                //academicTraining.Roh_evaluatedByTitle =  NombreEmpresa(formacionAcademica.EntidadEvaluacion.EntidadRef);

                listaFormacionAcademicaImpartida.Add(academicTraining);
            }
            return listaFormacionAcademicaImpartida;
        }

        private static List<ImpartedcoursesseminarsOntology.ImpartedCoursesSeminars> CrearSeminariosCursos(Persona pDatos, Dictionary<string, Tuple<string, string>> dicOrganizaciones,
           Dictionary<string, Tuple<string, string>> dicPersonas)
        {
            List<ImpartedcoursesseminarsOntology.ImpartedCoursesSeminars> listaSeminariosCursos = new List<ImpartedcoursesseminarsOntology.ImpartedCoursesSeminars>();
            ImpartedcoursesseminarsOntology.ImpartedCoursesSeminars impartedCourses = new ImpartedcoursesseminarsOntology.ImpartedCoursesSeminars();
            foreach (OAI_PMH.Models.SGI.ActividadDocente.SeminariosCursos seminariosCursos in pDatos.SeminariosCursos)
            {
                //TODO impartedCourses.Roh_eventType = EventType(semianrioscursos.tipoevento);
                impartedCourses.Roh_title = seminariosCursos.NombreEvento;
                //impartedCourses.IdVcard_hasCountryName = IdentificadorPais(seminariosCursos.paisentidadorganizadora);
                //impartedCourses.IdVcard_hasRegion = IdentificadorRegion(seminariosCursos.region);
                impartedCourses.Vcard_locality = seminariosCursos.CiudadEntidadOrganizacionEvento;
                impartedCourses.Roh_goals = seminariosCursos.ObjetivosCurso;
                impartedCourses.Roh_targetProfile = seminariosCursos.PerfilDestinatarios;
                impartedCourses.IdVcard_hasLanguage = seminariosCursos.Idioma;
                impartedCourses.Vivo_start = seminariosCursos.FechaTitulacion;
                //impartedCourses.Roh_durationHours = seminariosCursos.horasimpartidas;
                impartedCourses.IdRoh_participationType = seminariosCursos.TipoParticipacion.Nombre;
                impartedCourses.Roh_correspondingAuthor = seminariosCursos.AutorCorrespondencia != null ? (bool)seminariosCursos.AutorCorrespondencia : false;

                //Entidad orgnaizadora
                impartedCourses.Roh_promotedByTitle = NombreEmpresa(seminariosCursos.EntidadOrganizacionEvento.EntidadRef);
                impartedCourses.IdRoh_promotedBy = dicOrganizaciones[seminariosCursos.EntidadOrganizacionEvento.EntidadRef].Item1;

                //ISBN
                impartedCourses.Roh_isbn = seminariosCursos.ISBN;

                //ISSN
                impartedCourses.Bibo_issn = seminariosCursos.ISSN;

                //IDPublicacion
                //impartedCourses.Bibo_handle = seminariosCursos.IdentificadoresPublicacion.
                //impartedCourses.Bibo_doi = ;
                //impartedCourses.Bibo_pmid = ;


                listaSeminariosCursos.Add(impartedCourses);
            }
            return listaSeminariosCursos;
        }

        private static List<AccreditationOntology.Accreditation> CrearSexenios(Persona pDatos)
        {
            List<AccreditationOntology.Accreditation> listaSexenios = new List<AccreditationOntology.Accreditation>();
            if (pDatos.Sexenios != null)
            {
                AccreditationOntology.Accreditation sexenio = new AccreditationOntology.Accreditation();
                sexenio.Roh_recognizedPeriods = Convert.ToInt32(pDatos.Sexenios.Numero);
                sexenio.IdVcard_hasCountryName = IdentificadorPais(pDatos.Sexenios.PaisRef);

                listaSexenios.Add(sexenio);
            }

            return listaSexenios;
        }

        public static OrganizationOntology.Organization CrearOrganizacionOntology(Empresa pDatos)
        {
            OrganizationOntology.Organization organization = new OrganizationOntology.Organization();
            organization.Roh_crisIdentifier = pDatos.Id;
            organization.Roh_title = pDatos.Nombre;
            organization.Vcard_locality = pDatos.DatosContacto?.Direccion;

            return organization;
        }

        //TODO
        private static OrganizationOntology.Organization CrearEntidadGestora(string entidadGestoraID)
        {
            OrganizationOntology.Organization organization = new OrganizationOntology.Organization();
            Empresa empresa = new Empresa();
            string emp = harvesterServices.GetRecord("Organizacion_" + entidadGestoraID);
            XmlSerializer xmlSerializer = new(typeof(Empresa));
            using (StringReader sr = new(emp))
            {
                empresa = (Empresa)xmlSerializer.Deserialize(sr);
            }
            organization.Roh_crisIdentifier = entidadGestoraID;
            organization.Roh_title = empresa.Nombre;
            organization.Vcard_locality = empresa.DatosContacto?.Direccion;

            //insertar

            return organization;
        }

        //TODO
        private static string CrearEntidadConvocante(string entidadConvocanteID)
        {
            OrganizationOntology.Organization organization = new OrganizationOntology.Organization();
            Empresa empresa = new Empresa();
            string emp = harvesterServices.GetRecord("Organizacion_" + entidadConvocanteID);
            XmlSerializer xmlSerializer = new(typeof(Empresa));
            using (StringReader sr = new(emp))
            {
                empresa = (Empresa)xmlSerializer.Deserialize(sr);
            }
            organization.Roh_crisIdentifier = entidadConvocanteID;
            organization.Roh_title = empresa.Nombre;
            organization.Vcard_locality = empresa.DatosContacto?.Direccion;

            //insertar

            ProjectOntology.OrganizationAux organizationAux = new ProjectOntology.OrganizationAux();
            organizationAux.Roh_organization = organization;
            //comprobar o cambiar por identificador al añadir
            //organizationAux.IdRoh_organization = organization.GNOSSID;
            organizationAux.Roh_organizationTitle = empresa.Nombre;
            organizationAux.Vcard_locality = empresa.DatosContacto?.Direccion;
            //insertar

            return organizationAux.GNOSSID;//asignar si no se autoasigna
        }

        //TODO
        private static string CrearEntidadFinanciadora(string entidadFinanciadoraID)
        {
            OrganizationOntology.Organization organization = new OrganizationOntology.Organization();
            Empresa empresa = new Empresa();
            string emp = harvesterServices.GetRecord("Organizacion_" + entidadFinanciadoraID);
            XmlSerializer xmlSerializer = new(typeof(Empresa));
            using (StringReader sr = new(emp))
            {
                empresa = (Empresa)xmlSerializer.Deserialize(sr);
            }
            organization.Roh_crisIdentifier = entidadFinanciadoraID;
            organization.Roh_title = empresa.Nombre;
            organization.Vcard_locality = empresa.DatosContacto?.Direccion;

            //insertar

            ProjectOntology.OrganizationAux organizationAux = new ProjectOntology.OrganizationAux();
            organizationAux.Roh_organization = organization;
            //comprobar o cambiar por identificador al añadir
            //organizationAux.IdRoh_organization = organization.GNOSSID;
            organizationAux.Roh_organizationTitle = empresa.Nombre;
            organizationAux.Vcard_locality = empresa.DatosContacto?.Direccion;
            //insertar

            return organizationAux.GNOSSID;//TODO asignar si no se autoasigna
        }

        public static ProjectAuthorization CrearAutorizacion(Autorizacion pAutorizacionProyecto)
        {
            ProjectAuthorization autorizacion = new ProjectAuthorization();
            Persona persona = new Persona();
            autorizacion.Roh_crisIdentifier = pAutorizacionProyecto.id.ToString();
            autorizacion.Roh_title = pAutorizacionProyecto.tituloProyecto;
            autorizacion.IdRoh_owner = GetPersonGnossId(pAutorizacionProyecto.solicitanteRef);

            if (string.IsNullOrEmpty(autorizacion.IdRoh_owner))
            {
                //Obtengo los datos de la persona del SGI
                persona = ObtenerPersona(pAutorizacionProyecto.solicitanteRef);

                //Creo la persona en BBDD
                PersonOntology.Person personOntology = CrearPersona(persona);

                ComplexOntologyResource resource = personOntology.ToGnossApiResource(mResourceApi, null);

                // Carga.                   
                //mResourceApi.LoadComplexSemanticResource(resource, false, false);
                //dicAutorizacion[personOntology.Roh_crisIdentifier] = resource.GnossId;//necesario incluirla en diccionario?
            }

            if (!string.IsNullOrEmpty(autorizacion.Roh_crisIdentifier) && !string.IsNullOrEmpty(autorizacion.Roh_title)
                && !string.IsNullOrEmpty(autorizacion.IdRoh_owner))
            {
                return autorizacion;
            }
            else
            {
                return null;
            }
        }

        public static GroupOntology.Group CrearGrupo(Grupo grupo)
        {
            GroupOntology.Group groupOntology = new GroupOntology.Group();
            groupOntology.Roh_title = grupo.nombre;
            groupOntology.Roh_normalizedCode = grupo.codigo;
            groupOntology.Roh_foundationDate = grupo.fechaInicio;
            Tuple<string, string, string> duracion = RestarFechas(grupo.fechaInicio, grupo.fechaFin);
            groupOntology.Roh_durationYears = duracion.Item1;
            groupOntology.Roh_durationMonths = duracion.Item2;
            groupOntology.Roh_durationDays = duracion.Item3;
            //groupOntology.Roh_hasKnowledgeArea = grupo.palabrasClave;

            return groupOntology;
        }

        public static string GetPersonGnossId(string pCrisIdentifier)
        {
            SparqlObject resultadoQuery = null;
            StringBuilder select = new StringBuilder(), where = new StringBuilder();

            // Consulta sparql.
            select.Append("SELECT ?s ");
            where.Append("WHERE { ");
            where.Append($@"?s <http://w3id.org/roh/crisIdentifier> '{pCrisIdentifier}'. ");
            where.Append("} ");

            resultadoQuery = mResourceApi.VirtuosoQuery(select.ToString(), where.ToString(), "person");

            if (resultadoQuery != null && resultadoQuery.results != null && resultadoQuery.results.bindings != null && resultadoQuery.results.bindings.Count > 0)
            {
                foreach (Dictionary<string, SparqlObject.Data> fila in resultadoQuery.results.bindings)
                {
                    return fila["s"].value;
                }
            }

            return string.Empty;
        }

        private static Persona ObtenerPersona(string personaRef)
        {
            Persona persona = new Persona();
            string person = harvesterServices.GetRecord("Persona_" + personaRef);
            XmlSerializer xmlSerializer = new(typeof(Persona));
            using (StringReader sr = new(person))
            {
                persona = (Persona)xmlSerializer.Deserialize(sr);
            }
            return persona;
        }

        private static void TipoProyecto(ProjectOntology.Project project, Proyecto pDatos)
        {
            if (string.IsNullOrEmpty(project.IdRoh_scientificExperienceProject))
            {
                switch (pDatos.ClasificacionCVN)
                {
                    case "AYUDAS":
                        //project.IdRoh_scientificExperienceProject = mResourceApi.GraphsUrl + "items/scientificexperienceproject_SEP1";
                        break;
                    case "COMPETITIVOS":
                        project.IdRoh_scientificExperienceProject = mResourceApi.GraphsUrl + "items/scientificexperienceproject_SEP1";
                        break;
                    case "NO_COMPETITIVOS":
                        project.IdRoh_scientificExperienceProject = mResourceApi.GraphsUrl + "items/scientificexperienceproject_SEP2";
                        break;
                    default:
                        break;
                }
            }
        }

        /// <summary>
        /// Obtiene los años, meses y dias entre la fecha de inicio y de fin
        /// </summary>
        /// <param name="fechaInicio"></param>
        /// <param name="fechaFin"></param>
        /// <returns></returns>
        public static Tuple<string, string, string> RestarFechas(DateTime fechaInicio, DateTime fechaFin)
        {
            int total = (fechaFin - fechaInicio).Days;
            int anios = total / 365;
            int meses = (total - (365 * anios)) / 30;
            int dias = total - (365 * anios) - (30 * meses);
            return new Tuple<string, string, string>(anios.ToString(), meses.ToString(), dias.ToString());
        }

        public static ProjectOntology.Project CrearProyecto(Proyecto pDatos,
            Dictionary<string, Tuple<string, string>> dicPersonas, Dictionary<string, Tuple<string, string>> dicOrganizaciones)
        {
            ProjectOntology.Project project = new ProjectOntology.Project();
            project.Roh_crisIdentifier = pDatos.Id;
            project.Roh_isValidated = true;
            project.Roh_validationStatusProject = "validado";

            TipoProyecto(project, pDatos);

            //Añado el tipo de proyecto en caso de ser no competitivo
            // 875 - Coordinación, 876 - Cooperación
            if (project.IdRoh_scientificExperienceProject.Equals(mResourceApi.GraphsUrl + "items/scientificexperienceproject_SEP2")
                && pDatos.CoordinadorExterno != null)
            {
                string projectType = mResourceApi.GraphsUrl + "items/projecttype_";
                project.IdRoh_projectType = (bool)pDatos.CoordinadorExterno ? projectType + "875" : projectType + "876";
            }

            if (!string.IsNullOrEmpty(pDatos.Titulo))
            {
                project.Roh_title = pDatos.Titulo;
            }
            if (!string.IsNullOrEmpty(pDatos.Observaciones))
            {
                project.Vivo_description = pDatos.Observaciones;
            }
            //TODO -revisar
            if (pDatos.Equipo != null && pDatos.Equipo.Any())
            {
                project.Vivo_relates = new List<ProjectOntology.BFO_0000023>();
                int orden = 1;
                foreach (ProyectoEquipo item in pDatos.Equipo)
                {
                    ProjectOntology.BFO_0000023 BFO = new ProjectOntology.BFO_0000023();
                    BFO.Rdf_comment = orden;
                    if (dicPersonas.ContainsKey(item.PersonaRef))
                    {
                        BFO.IdRoh_roleOf = dicPersonas[item.PersonaRef].Item1;
                    }
                    //else
                    //{
                    //    PersonOntology.Person nuevaPersona = new PersonOntology.Person();

                    //    //Pido los datos de la persona para insertarla
                    //    Persona persona = ObtenerPersona(item.PersonaRef);

                    //    //Si la persona no tiene nombre no la inserto
                    //    if (!string.IsNullOrEmpty(persona.Nombre))
                    //    {
                    //        nuevaPersona = CrearPersona(persona);

                    //        //mResourceApi.LoadComplexSemanticResource(nuevaPersona, false, false);
                    //        dicPersonas[nuevaPersona.Roh_crisIdentifier] = new Tuple<string, string>("", "");//TODO //nuevaPersona.GnossId;
                    //        BFO.IdRoh_roleOf = dicPersonas[item.PersonaRef].Item1;
                    //    }
                    //}                    
                    BFO.Roh_isIP = item.RolProyecto.RolPrincipal;
                    if (!string.IsNullOrEmpty(item.FechaInicio))
                    {
                        BFO.Vivo_start = Convert.ToDateTime(item.FechaInicio);
                    }
                    if (!string.IsNullOrEmpty(item.FechaFin))
                    {
                        BFO.Vivo_end = Convert.ToDateTime(item.FechaFin);
                    }
                    //grado contribucion (CVN_PARTICIPATION_B) insertado en CV
                    project.Vivo_relates.Add(BFO);
                    orden++;
                }

                //Indico el número de investigadores ? TODO
                project.Roh_researchersNumber = pDatos.Equipo.Select(x=>x.PersonaRef).GroupBy(x=>x).Count();
            }

            //Añado las entidades financiadoras que no existan en BBDD
            foreach (ProyectoEntidadFinanciadora entidadFinanciadora in pDatos.EntidadesFinanciadoras)
            {
                if (!dicOrganizaciones.ContainsKey(entidadFinanciadora.Id))
                {
                    CrearEntidadFinanciadora(entidadFinanciadora.EntidadRef);
                }
                //project.Roh_grantedBy = organizaciones;
            }

            //Añado las entidades gestoras que no existan en BBDD
            foreach (ProyectoEntidadGestora entidadGestora in pDatos.EntidadesGestoras)
            {
                if (!dicOrganizaciones.ContainsKey(entidadGestora.Id))
                {
                    OrganizationOntology.Organization organizacion = CrearEntidadGestora(entidadGestora.EntidadRef);
                    project.Roh_conductedByTitle = organizacion.Roh_title;
                    project.IdRoh_conductedBy = organizacion.GNOSSID;
                }
            }

            //Añado las entidades participación que no existan en BBDD
            foreach (ProyectoEntidadConvocante entidadConvocante in pDatos.EntidadesConvocantes)
            {
                if (!dicOrganizaciones.ContainsKey(entidadConvocante.Id))
                {
                    CrearEntidadConvocante(entidadConvocante.Id);
                }
                //project.Roh_participates = organizaciones;
            }

            //TODO - revisar
            double porcentajeSubvencion = 0;
            double porcentajeCredito = 0;
            double porcentajeMixto = 0;
            List<ProjectOntology.OrganizationAux> organizaciones = new List<ProjectOntology.OrganizationAux>();
            foreach (ProyectoEntidadFinanciadora entidadFinanciadora in pDatos.EntidadesFinanciadoras)
            {
                //ProjectOntology.OrganizationAux organizacion = new ProjectOntology.OrganizationAux();
                //organizacion.Roh_organizationTitle = entidadFinanciadora.EntidadRef;//TODO
                //TODO ? string entFinanciadora = harvesterServices.GetRecord("Organizacion_" + entidadFinanciadora.EntidadRef);

                if (entidadFinanciadora.TipoFinanciacion != null && entidadFinanciadora.PorcentajeFinanciacion != null)
                {
                    if (entidadFinanciadora.TipoFinanciacion.Nombre.Equals("Subvención"))
                    {
                        porcentajeSubvencion += (double)entidadFinanciadora.PorcentajeFinanciacion;
                    }
                    else if (entidadFinanciadora.TipoFinanciacion.Nombre.Equals("Préstamo"))
                    {
                        porcentajeCredito += (double)entidadFinanciadora.PorcentajeFinanciacion;
                    }
                    else if (entidadFinanciadora.TipoFinanciacion.Nombre.Equals("Mixto"))
                    {
                        porcentajeMixto += (double)entidadFinanciadora.PorcentajeFinanciacion;
                    }
                }
            }

            project.Roh_grantsPercentage = (float)porcentajeSubvencion;
            project.Roh_creditPercentage = (float)porcentajeCredito;
            project.Roh_mixedPercentage = (float)porcentajeMixto;

            //TODO - revisar
            double cuantiaTotal = pDatos.TotalImporteConcedido != null ? (double)pDatos.TotalImporteConcedido : 0;
            if (cuantiaTotal == 0)
            {
                foreach (ProyectoAnualidadResumen anualidadResumen in pDatos.ResumenAnualidades)
                {
                    if (anualidadResumen.Presupuestar != null && (bool)anualidadResumen.Presupuestar)//TODO - contamos con el dato de presupuestar?
                    {
                        cuantiaTotal += anualidadResumen.TotalGastosConcedido;
                    }
                }
                project.Roh_monetaryAmount = (float)cuantiaTotal;
            }

            project.Vivo_start = pDatos.FechaInicio != null ? Convert.ToDateTime(pDatos.FechaInicio) : null;

            //Si está informada la FechaFinDefinitiva prevalecerá sobre la FechaFin y será la considerada como fecha de finalización del proyecto,
            //independientemente de que sea mayor o menor que la fecha de fin inicial.
            //Añado la fecha de finalizacion si el proyecto es de tipo competitivo.
            if (project.IdRoh_scientificExperienceProject.Equals(mResourceApi.GraphsUrl + "items/scientificexperienceproject_SEP1")){
                project.Vivo_end = pDatos.FechaFinDefinitiva != null ? Convert.ToDateTime(pDatos.FechaFinDefinitiva) : Convert.ToDateTime(pDatos.FechaFin);
            }

            if (pDatos.FechaFinDefinitiva != null)
            {

                if (pDatos.FechaInicio != null)
                {
                    Tuple<string, string, string> duration = RestarFechas(Convert.ToDateTime(pDatos.FechaInicio), Convert.ToDateTime(pDatos.FechaFinDefinitiva));
                    project.Roh_durationYears = duration.Item1;
                    project.Roh_durationMonths = duration.Item2;
                    project.Roh_durationDays = duration.Item3;
                }
            }
            else
            {

                if (pDatos.FechaInicio != null)
                {
                    Tuple<string, string, string> duration = RestarFechas(Convert.ToDateTime(pDatos.FechaInicio), Convert.ToDateTime(pDatos.FechaFin));
                    project.Roh_durationYears = duration.Item1;
                    project.Roh_durationMonths = duration.Item2;
                    project.Roh_durationDays = duration.Item3;
                }
            }

            project.Roh_relevantResults = pDatos.Contexto?.ResultadosPrevistos;
            project.Roh_projectCode = pDatos.CodigoExterno;

            // TODO

            return project;
        }
    }
}