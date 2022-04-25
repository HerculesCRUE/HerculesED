using Gnoss.ApiWrapper;
using Gnoss.ApiWrapper.ApiModel;
using Gnoss.ApiWrapper.Model;
using Harvester.Models;
using Harvester.Models.SGI.Autorizaciones;
using Harvester.Models.SGI.ProteccionIndustrialIntelectual;
using Hercules.MA.ServicioExterno.Controllers.Utilidades;
using Newtonsoft.Json;
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
            Dictionary<string, string> dicOrganizaciones = new Dictionary<string, string>();
            Dictionary<string, string> dicPersonas = new Dictionary<string, string>();
            Dictionary<string, string> dicProyectos = new Dictionary<string, string>();


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
            //mResourceApi.ChangeOntoly("projectauthorization");
            //ProcesarFichero(_Config, "AutorizacionProyecto", null);

        //TODO eliminar
        Testing:

            string fecha = DateTime.Now.ToString("yyyy-MM-ddT00:00:00") + "Z";
            fecha = "2022-01-01T00:00:00Z";

            //Genero los ficheros con los datos a procesar desde la fecha
            GuardarIdentificadores(_Config, "Organizacion", fecha);
            GuardarIdentificadores(_Config, "Persona", fecha);
            GuardarIdentificadores(_Config, "Proyecto", fecha);
            //GuardarIdentificadores(_Config, "PRC", fecha, true);
            //GuardarIdentificadores(_Config, "AutorizacionProyecto", fecha, true);
            //GuardarIdentificadores(_Config, "Invencion", fecha, true);

            //Actualizo la última fecha de carga
            UpdateLastDate(_Config, fecha);

            //Proceso los ficheros
            mResourceApi.ChangeOntoly("organization");
            ProcesarFichero(_Config, "Organizacion", dicOrganizaciones: dicOrganizaciones);
            mResourceApi.ChangeOntoly("person");
            ProcesarFichero(_Config, "Persona", dicPersonas: dicPersonas);
            mResourceApi.ChangeOntoly("project");
            ProcesarFichero(_Config, "Proyecto", dicOrganizaciones, dicProyectos, dicPersonas);
            //ProcesarFichero(_Config, "PRC", dicProyectos);
            //mResourceApi.ChangeOntoly("projectauthorization");
            //ProcesarFichero(_Config, "AutorizacionProyecto", null);
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
        /// <param name="pDicRecursosCargados"></param>
        public void ProcesarFichero(ReadConfig pConfig, string pSet, [Optional] Dictionary<string, string> dicOrganizaciones,
            [Optional] Dictionary<string, string> dicProyectos, [Optional] Dictionary<string, string> dicPersonas)
        {
            string directorioPendientes = $@"{pConfig.GetLogCargas()}/{pSet}/pending/";
            string directorioProcesados = $@"{pConfig.GetLogCargas()}/{pSet}/processed/";

            if (Directory.Exists(directorioPendientes))
            {
                foreach (string fichero in Directory.EnumerateFiles(directorioPendientes))
                {
                    string ficheroProcesado = directorioProcesados + fichero.Substring(fichero.LastIndexOf("/"));
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

                                //CreacionAuxiliarOrganizacion(organization.Id);

                                // Cambio de modelo. TODO: Mirar propiedades.
                                OrganizationOntology.Organization empresaOntology = CrearOrganizacion(organization);

                                //Si no me llega el cris identifier o los datos obligatorios salto a la siguiente
                                if (string.IsNullOrEmpty(empresaOntology.Roh_crisIdentifier) && string.IsNullOrEmpty(empresaOntology.Roh_title))
                                {
                                    continue;
                                }

                                resource = empresaOntology.ToGnossApiResource(mResourceApi, null);
                                if (dicOrganizaciones.ContainsKey(empresaOntology.Roh_crisIdentifier))
                                {
                                    // Modificación.
                                    //mResourceApi.ModifyComplexOntologyResource(resource, false, false);
                                }
                                else
                                {
                                    // Carga.                   
                                    //mResourceApi.LoadComplexSemanticResource(resource, false, false);
                                    //pDicRecursosCargados[empresaOntology.Roh_crisIdentifier] = resource.GnossId;
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

                                //TODO CreacionAuxiliarPersona();

                                // Cambio de modelo. TODO: Mirar propiedades.
                                PersonOntology.Person personOntology = CrearPersona(persona);

                                //Si no me llega el cris identifier o los datos obligatorios salto a la siguiente
                                if (string.IsNullOrEmpty(personOntology.Roh_crisIdentifier) && string.IsNullOrEmpty(personOntology.Foaf_name)
                                    && string.IsNullOrEmpty(personOntology.Foaf_firstName) && string.IsNullOrEmpty(personOntology.Foaf_lastName))
                                {
                                    continue;
                                }

                                resource = personOntology.ToGnossApiResource(mResourceApi, null);
                                if (dicPersonas.ContainsKey(personOntology.Roh_crisIdentifier))
                                {
                                    // Modificación.
                                    //mResourceApi.ModifyComplexOntologyResource(resource, false, false);
                                }
                                else
                                {
                                    // Carga.                   
                                    //mResourceApi.LoadComplexSemanticResource(resource, false, false);
                                    //pDicRecursosCargados[personOntology.Roh_crisIdentifier] = resource.GnossId;
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

                                // Cambio de modelo. TODO: Mirar propiedades.
                                ProjectOntology.Project projectOntology = CrearProyecto(proyecto, dicProyectos, dicPersonas, dicOrganizaciones);

                                //Si no me llega el cris identifier o los datos obligatorios salto a la siguiente
                                //if (string.IsNullOrEmpty(projectOntology.Roh_crisIdentifier) && string.IsNullOrEmpty(projectOntology.Roh_title))
                                //{
                                //    continue;
                                //}

                                //resource = projectOntology.ToGnossApiResource(mResourceApi, null);
                                //if (pDicRecursosCargados.ContainsKey(projectOntology.Roh_crisIdentifier))
                                //{
                                //    // Modificación.
                                //    mResourceApi.ModifyComplexOntologyResource(resource, false, false);
                                //}
                                //else
                                //{
                                //    // Carga.                   
                                //    mResourceApi.LoadComplexSemanticResource(resource, false, false);
                                //    pDicRecursosCargados[projectOntology.Roh_crisIdentifier] = resource.GnossId;
                                //}

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
                                AutorizacionProyecto autorizacion = new AutorizacionProyecto();
                                xmlResult = harvesterServices.GetRecord(id);

                                if (string.IsNullOrEmpty(xmlResult))
                                {
                                    File.AppendAllText(ficheroProcesado, id + Environment.NewLine);
                                    continue;
                                }

                                xmlSerializer = new(typeof(AutorizacionProyecto));
                                using (StringReader sr = new(xmlResult))
                                {
                                    autorizacion = (AutorizacionProyecto)xmlSerializer.Deserialize(sr);
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

                        }

                        // Borra el fichero.
                        File.Delete(fichero);
                    }
                }
            }
        }

        private void CreacionAuxiliaresProyecto(Proyecto proyecto, Dictionary<string, string> dicProyectos,
            Dictionary<string, string> dicPersonas, Dictionary<string, string> dicOrganizaciones)
        {
            string record = harvesterServices.GetRecord("Proyecto_" + proyecto.Id);
            List<string> listaOrganizaciones = new List<string>();
            //TODO
            //listaOrganizaciones.AddRange(proyecto.EntidadesGestoras.Select(x => x.EntidadRef).ToList());
            //listaOrganizaciones.AddRange(proyecto.EntidadesConvocantes.Select(x => x.EntidadRef).ToList());
            listaOrganizaciones.AddRange(proyecto.EntidadesFinanciadoras.Select(x => x.EntidadRef).ToList());

            Dictionary<string, string> dicBBDDProy = GetEntityByCRIS("http://vivoweb.org/ontology/core#Project", "project", new List<string>() { proyecto.Id });
            Dictionary<string, string> dicBBDDPers = GetEntityByCRIS("http://xmlns.com/foaf/0.1/Person", "person", proyecto.Equipo.Select(x => x.PersonaRef).ToList());
            Dictionary<string, string> dicBBDDOrg = GetEntityByCRIS("http://xmlns.com/foaf/0.1/Organization", "organization", listaOrganizaciones);


            XmlSerializer xmlSerializer = null;
            Proyecto project = new Proyecto();

            xmlSerializer = new(typeof(Proyecto));
            using (StringReader sr = new(record))
            {
                project = (Proyecto)xmlSerializer.Deserialize(sr);
            }


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

        private Dictionary<string, string> GetEntityByCRIS(string pRdfType, string pOntology, List<string> lista)
        {
            Dictionary<string, string> dicResultados = new Dictionary<string, string>();

            int limit = 10000;
            int offset = 0;
            while (true)
            {
                SparqlObject resultadoQuery = null;
                string select = "SELECT ?s ?crisIdentifier ";
                string filtroProject = $@"strends(?crisIdentifier, '|{string.Join("') OR  strends(?crisIdentifier , '|", lista)}')";//TODO - eliminar por el general
                string filtroGeneral = $@"?crisIdentifier in ('{string.Join("', '", lista)}') ";

                string filtro = pOntology.Equals("project") ? filtroProject : filtroGeneral;
                string where = $@"WHERE {{
    ?s a <{pRdfType}> .
    ?s <http://w3id.org/roh/crisIdentifier> ?crisIdentifier . 
    FILTER(
        {filtro}
    )
}} ";

                resultadoQuery = mResourceApi.VirtuosoQuery(select, where, pOntology);

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
            if (pDatos.DatosContacto.Telefonos != null && pDatos.DatosContacto.Telefonos.Any())
            {
                persona.Vcard_hasTelephone = new List<string>();
                foreach (string item in pDatos.DatosContacto.Telefonos)
                {
                    persona.Vcard_hasTelephone.Add(item);
                }
            }
            if (pDatos.DatosContacto.Moviles != null && pDatos.DatosContacto.Moviles.Any())
            {
                //persona.Vcard_hasMobilePhone = new List<string>();
                foreach (string item in pDatos.DatosContacto.Moviles)
                {
                    //persona.Vcard_hasMobilePhone.Add(item);
                }
            }
            if (pDatos.Activo.HasValue)
            {
                persona.Roh_isActive = pDatos.Activo.Value;
            }
            if (pDatos.TipoDocumento != null && !string.IsNullOrEmpty(pDatos.TipoDocumento.Id))
            {
                switch (pDatos.TipoDocumento.Id)
                {
                    case "D":
                        //persona.Roh_dni = pDatos.NumeroDocumento;
                        break;
                    case "E":
                        //persona.Roh_nie = pDatos.NumeroDocumento;
                        break;
                }
            }
            if (pDatos.DatosPersonales != null)
            {
                //persona.Vcard_birthdate = pDatos.DatosPersonales.Fechanacimiento;
            }
            if (pDatos.Fotografia != null && !string.IsNullOrEmpty(pDatos.Fotografia.Contenido))
            {
                //persona.Foaf_img = pDatos.Fotografia.Contenido;
            }
            if (pDatos.Sexo != null && !string.IsNullOrEmpty(pDatos.Sexo.Id))
            {
                switch (pDatos.Sexo.Id)
                {
                    case "V":
                        //persona.IdFoaf_gender = $@"http://gnoss.com/items/gender_000";
                        break;
                    case "M":
                        //persona.IdFoaf_gender = $@"http://gnoss.com/items/gender_010";
                        break;
                }
            }

            // TODO: Posible cambio Treelogic
            if (pDatos.Vinculacion != null && pDatos.Vinculacion.Departamento != null && !string.IsNullOrEmpty(pDatos.Vinculacion.Departamento.Id))
            {
                persona.IdVivo_departmentOrSchool = $@"http://gnoss.com/items/department_{pDatos.Vinculacion.Departamento.Id}";
            }
            if (pDatos.Vinculacion != null && pDatos.Vinculacion.CategoriaProfesional != null && !string.IsNullOrEmpty(pDatos.Vinculacion.CategoriaProfesional.Nombre))
            {
                persona.Roh_hasPosition = pDatos.Vinculacion.CategoriaProfesional.Nombre;
            }

            return persona;
        }
        public static OrganizationOntology.Organization CrearOrganizacion(Empresa pDatos)
        {
            OrganizationOntology.Organization organization = new OrganizationOntology.Organization();
            organization.Roh_crisIdentifier = pDatos.Id;
            organization.Roh_isSynchronized = true;
            organization.Roh_title = pDatos.Nombre;
            return organization;
        }
        public static ProjectOntology.Project CrearProyecto(Proyecto pDatos, Dictionary<string, string> dicProyectos,
            Dictionary<string, string> dicPersonas, Dictionary<string, string> dicOrganizaciones)
        {
            ProjectOntology.Project project = new ProjectOntology.Project();
            project.Roh_crisIdentifier = pDatos.Id;
            //project.Roh_isSynchronized = true;

            if (!string.IsNullOrEmpty(pDatos.Titulo))
            {
                project.Roh_title = pDatos.Titulo;
            }
            if (!string.IsNullOrEmpty(pDatos.Observaciones))
            {
                project.Vivo_description = pDatos.Observaciones;
            }
            if (!string.IsNullOrEmpty(pDatos.Acronimo))
            {
                //project.Vivo_abbreviation = pDatos.Acronimo;
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
                        BFO.IdRoh_roleOf = dicPersonas[item.PersonaRef];
                    }
                    else
                    {
                        PersonOntology.Person nuevaPersona = new PersonOntology.Person();
                        Persona persona = new Persona();

                        //Pido los datos de la persona para insertarla
                        string person = harvesterServices.GetRecord("Persona_" + item.PersonaRef);
                        XmlSerializer xmlSerializer = new(typeof(Persona));
                        using (StringReader sr = new(person))
                        {
                            persona = (Persona)xmlSerializer.Deserialize(sr);
                        }

                        //Si la persona no tiene nombre no la inserto
                        if (!string.IsNullOrEmpty(persona.Nombre))
                        {
                            nuevaPersona.Foaf_name = persona.Nombre + " " + persona.Apellidos;
                            nuevaPersona.Foaf_firstName = persona.Nombre;
                            nuevaPersona.Foaf_lastName = persona.Apellidos;
                            nuevaPersona.Roh_isActive = persona.Activo != null ? (bool)persona.Activo : false;
                            nuevaPersona.Roh_crisIdentifier = persona.Id;
                            //TODO - ¿isIP?
                            nuevaPersona.Roh_isIPGroupActually = false;
                            nuevaPersona.Roh_isIPGroupHistorically = false;
                            nuevaPersona.Roh_isIPProjectActually = item.RolProyecto.RolPrincipal;//TODO - check
                            nuevaPersona.Roh_isIPProjectHistorically = false;

                            //mResourceApi.LoadComplexSemanticResource(nuevaPersona, false, false);
                            dicPersonas[nuevaPersona.Roh_crisIdentifier] = "";//TODO //nuevaPersona.GnossId;
                            BFO.IdRoh_roleOf = dicPersonas[item.PersonaRef];
                        }
                    }

                    BFO.Roh_isIP = item.RolProyecto.RolPrincipal;
                    if (!string.IsNullOrEmpty(item.FechaInicio))
                    {
                        BFO.Vivo_start = Convert.ToDateTime(item.FechaInicio);
                    }
                    if (!string.IsNullOrEmpty(item.FechaFin))
                    {
                        BFO.Vivo_end = Convert.ToDateTime(item.FechaFin);
                    }
                    project.Vivo_relates.Add(BFO);
                    orden++;
                }

                //Indico el número de investigadores
                project.Roh_researchersNumber = orden;
            }

            ////TODO - si se añaden el resto de entidades la inserción debería ser previa de manera general
            //if (!dicOrganizaciones.ContainsKey(entidadFinanciadora.Id))
            //{
            //    ProjectOntology.OrganizationAux organizationAux = new ProjectOntology.OrganizationAux();
            //    organizationAux.Roh_organizationTitle = entidadFinanciadora.EntidadRef;

            //    organizationAux.IdRoh_organization = dicOrganizaciones[entidadFinanciadora.Id];
            //}

            //project.Roh_grantedBy = organizaciones;


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
            double totalGasto = 0;
            foreach (ProyectoAnualidadResumen anualidadResumen in pDatos.ResumenAnualidades)
            {
                if (anualidadResumen.Presupuestar.Equals("true"))//TODO - contamos con el dato de presupuestar?
                {
                    totalGasto += anualidadResumen.TotalGastosPresupuesto;
                }
            }
            project.Roh_monetaryAmount = (float)totalGasto;

            //TODO - revisar
            project.Vivo_start = Convert.ToDateTime(pDatos.FechaInicio);
            project.Vivo_end = Convert.ToDateTime(pDatos.FechaFin);



            // TODO

            return project;
        }
    }
}