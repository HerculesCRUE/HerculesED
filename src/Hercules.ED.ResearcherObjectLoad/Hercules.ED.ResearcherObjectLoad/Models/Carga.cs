using Gnoss.ApiWrapper;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using System.IO.Compression;
using Gnoss.ApiWrapper.ApiModel;
using DocumentOntology;
using PersonOntology;
using Gnoss.ApiWrapper.Model;
using Hercules.ED.ResearcherObjectLoad.Models.ObjetoJson;
using Hercules.ED.ResearcherObjectLoad.Config;
using System.Threading;
using Hercules.ED.DisambiguationEngine.Models;
using System.Reflection;
using Hercules.ED.ResearcherObjectLoad.Models.DisambiguationObjects;

namespace Hercules.ED.ResearcherObjectLoad.Models
{
    public class Carga
    {
        public static ResourceApi mResourceApi;
        public static CommunityApi mCommunityApi;
        public static ConfigService configuracion;

        #region --- Constantes
        public const string DISAMBIGUATION_PERSON = "DisambiguationPerson";
        public const string DISAMBIGUATION_PUBLICATION = "DisambiguationPublication";
        public const string DISAMBIGUATION_RO = "DisambiguationRO";
        public const string DISAMBIGUATION_RO_GITHUB = "DisambiguationRoGithub";

        public const string JOURNAL_ARTICLE = "Journal Article";
        public const string BOOK = "Book";
        public const string CHAPTER = "Chapter";
        public const string CONFERENCE_PAPER = "Conference Paper";
        public const string REVISTA_JOURNAL = "Journal";
        public const string REVISTA_BOOK = "Book";

        public const int MAX_INTENTOS = 100;
        public const int NUM_HILOS = 6;
        #endregion

        public static void CargaMain()
        {
            ProcesarFichero(configuracion.GetRutaDirectorioLectura(), configuracion.GetRutaDirectorioEscritura());
        }

        public static void ProcesarFichero(string pRutaLectura, string pRutaEscritura)
        {
            DirectoryInfo directorio = new DirectoryInfo(pRutaLectura);

            // Obtención de las categorías.
            Tuple<Dictionary<string, string>, Dictionary<string, string>> tupla = ObtenerDatosTesauro();

            while (true)
            {
                foreach (var fichero in directorio.GetFiles("*.json"))
                {
                    // Diccionarios para almacenar los vinculos de los recursos a desambiguar con los IDs de los recursos a cargar
                    Dictionary<HashSet<string>, string> dicGnossIdPerson = new Dictionary<HashSet<string>, string>();

                    List<DisambiguableEntity> listaDesambiguarBBDD = new List<DisambiguableEntity>();


                    //Listado con los datos para desambiguar
                    List<DisambiguableEntity> listaDesambiguar = new List<DisambiguableEntity>();

                    //Diccionarios con los datos iniciales a cargar
                    Dictionary<string, Person> dicIdPersona = new Dictionary<string, Person>();
                    Dictionary<string, Document> dicIdPublication = new Dictionary<string, Document>();
                    Dictionary<string, ResearchobjectOntology.ResearchObject> dicIdRo = new Dictionary<string, ResearchobjectOntology.ResearchObject>();

                    //Diccionarios con los objetos JSON de los datos obtenidos
                    Dictionary<string, Publication> dicIdDatosPub = new Dictionary<string, Publication>();
                    Dictionary<string, ResearchObjectFigShare> dicIdDatosRoFigshare = new Dictionary<string, ResearchObjectFigShare>();
                    Dictionary<string, ResearchObjectGitHub> dicIdDatosRoGitHub = new Dictionary<string, ResearchObjectGitHub>();

                    string idPersona = null;
                    if (fichero.Name.StartsWith("figshare___"))
                    {
                        string idAutor = fichero.Name.Split("___")[1];
                        Dictionary<string, DisambiguableEntity> researchobjectsBBDD = ObtenerResearchObjectsBBDD("http://w3id.org/roh/usuarioFigShare", idAutor);
                        Dictionary<string, DisambiguableEntity> personasBBDD = ObtenerPersonasRelacionaBBDD("http://w3id.org/roh/usuarioFigShare", idAutor);
                        listaDesambiguarBBDD.AddRange(researchobjectsBBDD.Values.ToList());
                        listaDesambiguarBBDD.AddRange(personasBBDD.Values.ToList());
                        idPersona = personasBBDD.First(x => ((DisambiguationPerson)(x.Value)).figShareId == idAutor).Key;

                        // Obtención de los datos del JSON.
                        string jsonString = File.ReadAllText(fichero.FullName);
                        List<ResearchObjectFigShare> listaResearchObjects = JsonConvert.DeserializeObject<List<ResearchObjectFigShare>>(jsonString);
                        foreach (ResearchObjectFigShare researchObject in listaResearchObjects)
                        {
                            // --- ROs
                            DisambiguationRO disambiguationRo = GetDisambiguationRO(researchObject);
                            string idRo = disambiguationRo.ID;
                            listaDesambiguar.Add(disambiguationRo);

                            // --- Autores
                            if (researchObject.autores != null && researchObject.autores.Any())
                            {
                                List<DisambiguationPerson> coautores = new List<DisambiguationPerson>();
                                foreach (PersonRO autor in researchObject.autores)
                                {
                                    DisambiguationPerson disambiguationPerson = GetDisambiguationPerson(pPersonaRo: autor);
                                    string idPerson = disambiguationPerson.ID;
                                    coautores.Add(disambiguationPerson);
                                }
                                foreach (DisambiguationPerson coautor in coautores)
                                {
                                    coautor.coautores = new HashSet<string>(coautores.Where(x => x.ID != coautor.ID).Select(x => x.ID));
                                }
                                disambiguationRo.autores = new HashSet<string>(coautores.Select(x => x.ID));
                                listaDesambiguar.AddRange(coautores);
                            }

                            dicIdRo.Add(idRo, ConstruirRO("FigShare", researchObject, null, tupla.Item1, tupla.Item2));
                            dicIdDatosRoFigshare.Add(idRo, researchObject);
                        }
                    }
                    else if (fichero.Name.StartsWith("github___"))
                    {
                        string idAutor = fichero.Name.Split("___")[1];
                        Dictionary<string, DisambiguableEntity> researchobjectsBBDD = ObtenerResearchObjectsBBDD("http://w3id.org/roh/usuarioGitHub", idAutor);
                        Dictionary<string, DisambiguableEntity> personasBBDD = ObtenerPersonasRelacionaBBDD("http://w3id.org/roh/usuarioGitHub", idAutor);
                        listaDesambiguarBBDD.AddRange(researchobjectsBBDD.Values.ToList());
                        listaDesambiguarBBDD.AddRange(personasBBDD.Values.ToList());
                        idPersona = personasBBDD.First(x => ((DisambiguationPerson)(x.Value)).gitHubId == idAutor).Key;

                        // Obtención de los datos del JSON.
                        string jsonString = File.ReadAllText(fichero.FullName);
                        List<ResearchObjectGitHub> listaGithubData = JsonConvert.DeserializeObject<List<ResearchObjectGitHub>>(jsonString);
                        foreach (ResearchObjectGitHub githubObject in listaGithubData)
                        {
                            // --- ROs
                            DisambiguationRO disambiguationRoGitHub = GetDisambiguationRoGithub(githubObject);
                            string idRo = disambiguationRoGitHub.ID;
                            listaDesambiguar.Add(disambiguationRoGitHub);

                            List<DisambiguationPerson> coautores = new List<DisambiguationPerson>();
                            foreach (string nombre in githubObject.listaAutores)
                            {
                                DisambiguationPerson disambiguationPerson = GetDisambiguationPerson(pPersonaGit: nombre);
                                string idPerson = disambiguationPerson.ID;
                                coautores.Add(disambiguationPerson);
                            }
                            foreach (DisambiguationPerson coautor in coautores)
                            {
                                coautor.coautores = new HashSet<string>(coautores.Where(x => x.ID != coautor.ID).Select(x => x.ID));
                            }
                            disambiguationRoGitHub.autores = new HashSet<string>(coautores.Select(x => x.ID));
                            listaDesambiguar.AddRange(coautores);

                            dicIdRo.Add(idRo, ConstruirRO("GitHub", null, githubObject, tupla.Item1, tupla.Item2));
                            dicIdDatosRoGitHub.Add(idRo, githubObject);
                        }
                    }
                    else
                    {
                        string idAutor = fichero.Name.Split("___")[0];
                        // Obtención de los datos cargados de BBDD.                        
                        Dictionary<string, DisambiguableEntity> documentosBBDD = ObtenerPublicacionesBBDD(idAutor);
                        Dictionary<string, DisambiguableEntity> personasBBDD = ObtenerPersonasRelacionaBBDD("http://w3id.org/roh/ORCID", idAutor);
                        listaDesambiguarBBDD.AddRange(documentosBBDD.Values.ToList());
                        listaDesambiguarBBDD.AddRange(personasBBDD.Values.ToList());
                        idPersona = personasBBDD.First(x => ((DisambiguationPerson)(x.Value)).orcid == idAutor).Key;

                        // Obtención de los datos del JSON.
                        string jsonString = File.ReadAllText(fichero.FullName);
                        List<Publication> listaPublicaciones = JsonConvert.DeserializeObject<List<Publication>>(jsonString);

                        foreach (Publication publication in listaPublicaciones)
                        {
                            // --- Publicación
                            DisambiguationPublication disambiguationPub = GetDisambiguationPublication(publication);
                            disambiguationPub.autores = new HashSet<string>();
                            string idPub = disambiguationPub.ID;
                            listaDesambiguar.Add(disambiguationPub);

                            // --- Autores
                            if (publication.seqOfAuthors != null && publication.seqOfAuthors.Any())
                            {
                                List<DisambiguationPerson> coautores = new List<DisambiguationPerson>();
                                foreach (PersonaPub autor in publication.seqOfAuthors)
                                {
                                    DisambiguationPerson disambiguationPerson = GetDisambiguationPerson(autor);
                                    string idPerson = disambiguationPerson.ID;
                                    coautores.Add(disambiguationPerson);
                                    dicIdPersona.Add(idPerson, ConstruirPersona(autor));
                                    if (disambiguationPerson.documentos == null)
                                    {
                                        disambiguationPerson.documentos = new HashSet<string>();
                                    }
                                    disambiguationPerson.documentos.Add(publication.ID);
                                }
                                foreach (DisambiguationPerson coautor in coautores)
                                {
                                    coautor.coautores = new HashSet<string>(coautores.Where(x => x.ID != coautor.ID).Select(x => x.ID));
                                }
                                listaDesambiguar.AddRange(coautores);
                                disambiguationPub.autores = new HashSet<string>(coautores.Select(x => x.ID));
                            }

                            dicIdDatosPub.Add(idPub, publication);
                            dicIdPublication.Add(idPub, ConstruirDocument(publication, tupla.Item1, tupla.Item2));
                        }
                    }


                    if (!string.IsNullOrEmpty(idPersona) && (dicIdDatosPub.Count > 0 || dicIdDatosRoFigshare.Count > 0 || dicIdDatosRoGitHub.Count > 0))
                    {
                        // Obtención de la lista de equivalencias.
                        Dictionary<string, HashSet<string>> listaEquivalencias = Disambiguation.Disambiguate(listaDesambiguar, listaDesambiguarBBDD);

                        //Listados con los objetos encontrados en la BBDD
                        HashSet<string> idPersonasBBDD = new HashSet<string>();
                        HashSet<string> idDocumentosBBDD = new HashSet<string>();
                        HashSet<string> idROsBBDD = new HashSet<string>();
                        foreach (KeyValuePair<string, HashSet<string>> item in listaEquivalencias)
                        {
                            if (!Guid.TryParse(item.Key, out var newGuid))
                            {
                                string tipo = item.Value.First().Split("|")[0];
                                if (tipo == DISAMBIGUATION_PERSON)
                                {
                                    idPersonasBBDD.Add(item.Key);
                                }
                                else if (tipo == DISAMBIGUATION_PUBLICATION)
                                {
                                    idDocumentosBBDD.Add(item.Key);
                                }
                                else
                                {
                                    idROsBBDD.Add(item.Key);
                                }
                            }
                        }

                        //Datos de los objetos encontrados en la BBDD
                        List<Tuple<string, string, string, string, string, string>> datosPersonasBBDD = ObtenerPersonas(idPersonasBBDD);

                        #region 1º PERSONAS Procesamos las personas, actualizando las que corresponda
                        Dictionary<Person, HashSet<string>> listaPersonasCargarEquivalencias = new Dictionary<Person, HashSet<string>>();
                        foreach (KeyValuePair<string, HashSet<string>> item in listaEquivalencias)
                        {
                            if (Guid.TryParse(item.Key, out var newGuid))
                            {
                                // Recurso NO cargado previamente en BBDD.
                                string tipo = string.Empty;
                                HashSet<string> listaIds = new HashSet<string>();
                                foreach (string id in item.Value)
                                {
                                    tipo = id.Split("|")[0];
                                    listaIds.Add(id.Split("|")[1]);
                                }
                                string idA = listaIds.FirstOrDefault();
                                if (tipo == DISAMBIGUATION_PERSON && listaIds.ToList().Any())
                                {
                                    //Esta variable sólo se carga en los documentos, no en los ROs
                                    if (dicIdPersona != null && dicIdPersona.Any())
                                    {
                                        CrearPersonDesambiguada(idA, listaIds, dicIdPersona, listaPersonasCargarEquivalencias);
                                    }
                                }
                            }
                            else
                            {
                                // Recurso cargado previamente en BBDD. Modificamos datos cuando corresponda
                                string idRecursoBBDD = item.Key;
                                string tipo = string.Empty;
                                HashSet<string> listaIds = new HashSet<string>();
                                foreach (string id in item.Value)
                                {
                                    tipo = id.Split("|")[0];
                                    listaIds.Add(id.Split("|")[1]);
                                }

                                if (tipo == DISAMBIGUATION_PERSON && listaIds.ToList().Any())
                                {
                                    dicGnossIdPerson.Add(new HashSet<string>(listaIds), idRecursoBBDD);

                                    string orcid = datosPersonasBBDD.FirstOrDefault(x => x.Item1 == idRecursoBBDD).Item2;
                                    string crisIdentifier = datosPersonasBBDD.FirstOrDefault(x => x.Item1 == idRecursoBBDD).Item3;
                                    string nombre = datosPersonasBBDD.FirstOrDefault(x => x.Item1 == idRecursoBBDD).Item4;
                                    string apellidos = datosPersonasBBDD.FirstOrDefault(x => x.Item1 == idRecursoBBDD).Item5;
                                    string nombreCompleto = datosPersonasBBDD.FirstOrDefault(x => x.Item1 == idRecursoBBDD).Item6;

                                    if (string.IsNullOrEmpty(orcid))
                                    {
                                        InsertarOrcid(idRecursoBBDD, listaIds.ToList(), dicIdPersona);
                                    }

                                    if (string.IsNullOrEmpty(crisIdentifier))
                                    {
                                        ModificarNombres(idRecursoBBDD, nombre, apellidos, nombreCompleto, listaIds.ToList(), dicIdPersona);
                                    }
                                }
                            }
                        }


                        // Creación de los ComplexOntologyResources.
                        List<ComplexOntologyResource> listaPersonasCargar = new List<ComplexOntologyResource>();
                        mResourceApi.ChangeOntoly("person");
                        foreach (Person persona in listaPersonasCargarEquivalencias.Keys)
                        {
                            ComplexOntologyResource resourcePersona = persona.ToGnossApiResource(mResourceApi, null);
                            listaPersonasCargar.Add(resourcePersona);
                            dicGnossIdPerson.Add(listaPersonasCargarEquivalencias[persona], resourcePersona.GnossId);
                        }
                        #endregion

                        #region 2º PUBLICACIONES
                        Dictionary<Document, HashSet<string>> listaDocumentosCargarEquivalencias = new Dictionary<Document, HashSet<string>>();
                        Dictionary<string, string> listaDocumentosCargados = new Dictionary<string, string>();

                        foreach (KeyValuePair<string, HashSet<string>> item in listaEquivalencias)
                        {
                            if (Guid.TryParse(item.Key, out var newGuid))
                            {
                                // Recurso NO cargado previamente en BBDD.
                                string tipo = string.Empty;
                                HashSet<string> listaIds = new HashSet<string>();
                                foreach (string id in item.Value)
                                {
                                    tipo = id.Split("|")[0];
                                    listaIds.Add(id.Split("|")[1]);
                                }
                                string idA = listaIds.FirstOrDefault();

                                if (tipo == DISAMBIGUATION_PUBLICATION && listaIds.Any())
                                {
                                    CrearDocumentDesambiguado(idA, listaIds, dicIdDatosPub, listaDocumentosCargarEquivalencias, tupla.Item1, tupla.Item2);
                                }
                            }
                            else
                            {
                                // Recurso cargado previamente en BBDD.
                                string tipo = string.Empty;
                                HashSet<string> listaIds = new HashSet<string>();
                                foreach (string id in item.Value)
                                {
                                    tipo = id.Split("|")[0];
                                    listaIds.Add(id.Split("|")[1]);
                                }
                                string idA = listaIds.FirstOrDefault();

                                if (tipo == DISAMBIGUATION_PUBLICATION && listaIds.Any())
                                {
                                    listaDocumentosCargados.Add(idA, item.Key);
                                    CrearDocumentDesambiguado(idA, listaIds, dicIdDatosPub, listaDocumentosCargarEquivalencias, tupla.Item1, tupla.Item2);
                                }
                            }
                        }
                        // Creación del vínculo entre los documentos y las personas (Document apunta a Person).
                        foreach (KeyValuePair<Document, HashSet<string>> item in listaDocumentosCargarEquivalencias)
                        {
                            foreach (string id in item.Value)
                            {
                                Publication pubAux = dicIdDatosPub[id];
                                // Autores.
                                foreach (BFO_0000023 bfo_0000023 in item.Key.Bibo_authorList)
                                {
                                    string idAutor = bfo_0000023.IdRdf_member;
                                    foreach (KeyValuePair<HashSet<string>, string> item2 in dicGnossIdPerson)
                                    {
                                        if (item2.Key.Contains(idAutor))
                                        {
                                            bfo_0000023.IdRdf_member = item2.Value;
                                            break;
                                        }
                                    }
                                    if (Guid.TryParse(bfo_0000023.IdRdf_member, out Guid aux))
                                    {
                                        throw new Exception("No existe la persona");
                                    }
                                }
                            }
                        }

                        //Creación de los ComplexOntologyResources.
                        List<ComplexOntologyResource> listaDocumentosCargar = new List<ComplexOntologyResource>();
                        Dictionary<string, Document> listaDocumentosModificar = new Dictionary<string, Document>();
                        mResourceApi.ChangeOntoly("document");
                        foreach (Document documento in listaDocumentosCargarEquivalencias.Keys)
                        {
                            if (documento.Bibo_authorList.Exists(x => x.IdRdf_member == idPersona))
                            {
                                string idBBDD = listaDocumentosCargarEquivalencias[documento].Intersect(listaDocumentosCargados.Keys).FirstOrDefault();
                                if (string.IsNullOrEmpty(idBBDD))
                                {
                                    ComplexOntologyResource resourceDocumento = documento.ToGnossApiResource(mResourceApi, null);
                                    listaDocumentosCargar.Add(resourceDocumento);
                                }
                                else
                                {
                                    idBBDD = listaDocumentosCargados[idBBDD];
                                    listaDocumentosModificar.Add(idBBDD, documento);
                                }
                            }
                        }
                        #endregion

                        #region 3º RESEARCHOBJECT 
                        Dictionary<ResearchobjectOntology.ResearchObject, HashSet<string>> listaROsCargarEquivalencias = new Dictionary<ResearchobjectOntology.ResearchObject, HashSet<string>>();
                        Dictionary<string, string> listaROsCargados = new Dictionary<string, string>();

                        foreach (KeyValuePair<string, HashSet<string>> item in listaEquivalencias)
                        {
                            if (Guid.TryParse(item.Key, out var newGuid))
                            {
                                // Recurso NO cargado previamente en BBDD.
                                string tipo = string.Empty;
                                HashSet<string> listaIds = new HashSet<string>();
                                foreach (string id in item.Value)
                                {
                                    tipo = id.Split("|")[0];
                                    listaIds.Add(id.Split("|")[1]);
                                }
                                string idA = listaIds.FirstOrDefault();

                                if (tipo == DISAMBIGUATION_RO && listaIds.ToList().Any())
                                {
                                    if (dicIdDatosRoFigshare.Count > 0)
                                    {
                                        CrearRoFigshareDesambiguado(idA, listaIds, dicIdDatosRoFigshare, listaROsCargarEquivalencias, tupla.Item1, tupla.Item2);
                                    }
                                    if (dicIdDatosRoGitHub.Count > 0)
                                    {
                                        CrearRoGitHubDesambiguado(idA, listaIds, dicIdDatosRoGitHub, listaROsCargarEquivalencias, tupla.Item1, tupla.Item2);
                                    }
                                }
                            }
                            else
                            {
                                // Recurso cargado previamente en BBDD.
                                string tipo = string.Empty;
                                HashSet<string> listaIds = new HashSet<string>();
                                foreach (string id in item.Value)
                                {
                                    tipo = id.Split("|")[0];
                                    listaIds.Add(id.Split("|")[1]);
                                }
                                string idA = listaIds.FirstOrDefault();

                                if (tipo == DISAMBIGUATION_RO && listaIds.Any())
                                {
                                    listaROsCargados.Add(idA, item.Key);
                                    if (dicIdDatosRoFigshare.Count > 0)
                                    {
                                        CrearRoFigshareDesambiguado(idA, listaIds, dicIdDatosRoFigshare, listaROsCargarEquivalencias, tupla.Item1, tupla.Item2);
                                    }
                                    if (dicIdDatosRoGitHub.Count > 0)
                                    {
                                        CrearRoGitHubDesambiguado(idA, listaIds, dicIdDatosRoGitHub, listaROsCargarEquivalencias, tupla.Item1, tupla.Item2);
                                    }
                                }
                            }
                        }

                        // Creación del vínculo entre los ros y las personas (Research object apunta a Person).
                        foreach (KeyValuePair<ResearchobjectOntology.ResearchObject, HashSet<string>> item in listaROsCargarEquivalencias)
                        {
                            foreach (string id in item.Value)
                            {
                                // Autores.
                                foreach (ResearchobjectOntology.BFO_0000023 bfo_0000023 in item.Key.Bibo_authorList)
                                {
                                    string idAutor = bfo_0000023.IdRdf_member;
                                    foreach (KeyValuePair<HashSet<string>, string> item2 in dicGnossIdPerson)
                                    {
                                        if (item2.Key.Contains(idAutor))
                                        {
                                            bfo_0000023.IdRdf_member = item2.Value;
                                            break;
                                        }
                                    }
                                }
                                //En los ROs no cargamos las personas que no se reconozcan
                                item.Key.Bibo_authorList.RemoveAll(x => Guid.TryParse(x.IdRdf_member, out Guid aux));
                            }
                        }

                        //Creación de los ComplexOntologyResources.
                        List<ComplexOntologyResource> listaROsCargar = new List<ComplexOntologyResource>();
                        Dictionary<string, ResearchobjectOntology.ResearchObject> listaROsModificar = new Dictionary<string, ResearchobjectOntology.ResearchObject>();
                        mResourceApi.ChangeOntoly("researchobject");
                        foreach (ResearchobjectOntology.ResearchObject researchobject in listaROsCargarEquivalencias.Keys)
                        {
                            if (researchobject.Bibo_authorList.Exists(x => x.IdRdf_member == idPersona))
                            {
                                string idBBDD = listaROsCargarEquivalencias[researchobject].Intersect(listaROsCargados.Keys).FirstOrDefault();
                                if (string.IsNullOrEmpty(idBBDD))
                                {
                                    ComplexOntologyResource resourceResearchObject = researchobject.ToGnossApiResource(mResourceApi, null);
                                    listaROsCargar.Add(resourceResearchObject);
                                }
                                else
                                {
                                    idBBDD = listaROsCargados[idBBDD];
                                    listaROsModificar.Add(idBBDD, researchobject);
                                }
                            }
                        }

                        #endregion

                        // ------------------------------ CARGA
                        CargarDatos(listaPersonasCargar);
                        CargarDatos(listaDocumentosCargar);
                        CargarDatos(listaROsCargar);

                        //Modificación
                        Parallel.ForEach(listaDocumentosModificar, new ParallelOptions { MaxDegreeOfParallelism = NUM_HILOS }, recursoModificar =>
                        {
                            string[] idSplit = recursoModificar.Key.Split('_');
                            Document doc = listaDocumentosModificar[recursoModificar.Key];
                            ModificarDocumento(doc, recursoModificar.Key, tupla.Item1, tupla.Item2);
                            ComplexOntologyResource complexOntologyResource = doc.ToGnossApiResource(mResourceApi, null, new Guid(idSplit[idSplit.Length - 2]), new Guid(idSplit[idSplit.Length - 1]));

                            int numIntentos = 0;
                            while (!complexOntologyResource.Modified)
                            {
                                numIntentos++;

                                if (numIntentos > MAX_INTENTOS)
                                {
                                    break;
                                }
                                if (listaDocumentosModificar.Last().Key == recursoModificar.Key)
                                {
                                    mResourceApi.ModifyComplexOntologyResource(complexOntologyResource, false, true);
                                }
                                else
                                {
                                    mResourceApi.ModifyComplexOntologyResource(complexOntologyResource, false, true);
                                }
                            }
                        });

                        //Modificación
                        Parallel.ForEach(listaROsModificar, new ParallelOptions { MaxDegreeOfParallelism = NUM_HILOS }, recursoModificar =>
                        {
                            string[] idSplit = recursoModificar.Key.Split('_');
                            ResearchobjectOntology.ResearchObject ro = listaROsModificar[recursoModificar.Key];
                            ModificarRO(ro, recursoModificar.Key, tupla.Item1, tupla.Item2);
                            ComplexOntologyResource complexOntologyResource = ro.ToGnossApiResource(mResourceApi, null, new Guid(idSplit[idSplit.Length - 2]), new Guid(idSplit[idSplit.Length - 1]));

                            int numIntentos = 0;
                            while (!complexOntologyResource.Modified)
                            {
                                numIntentos++;

                                if (numIntentos > MAX_INTENTOS)
                                {
                                    break;
                                }
                                if (listaROsModificar.Last().Key == recursoModificar.Key)
                                {
                                    mResourceApi.ModifyComplexOntologyResource(complexOntologyResource, false, true);
                                }
                                else
                                {
                                    mResourceApi.ModifyComplexOntologyResource(complexOntologyResource, false, true);
                                }
                            }
                        });
                    }

                    // Hace una copia del fichero y elimina el original.
                    //CrearZip(pRutaEscritura, fichero.Name, jsonString);
                    //File.Delete(fichero.FullName);
                }

                Thread.Sleep(5000);
            }
        }

        /// <summary>
        /// Obtiene las personas relacionadas en función del valor de una propiedad.
        /// </summary>
        /// <param name="pProperty">Propiedad para buscar en la persona</param>
        /// <param name="pPropertyValue">Valor de la propiedad para buscar en la persona</param>
        /// <returns>Diccionario con el ID del recurso cargado como clave, y el objeto desambiguable como valor.</returns>
        private static Dictionary<string, DisambiguableEntity> ObtenerPersonasRelacionaBBDD(string pProperty, string pPropertyValue)
        {
            Dictionary<string, DisambiguableEntity> listaPersonas = new Dictionary<string, DisambiguableEntity>();
            {
                int limit = 10000;
                int offset = 0;
                bool salirBucle = false;

                // Consulta sparql.
                do
                {
                    //TODO from
                    //Obttenemos todas las personas hasta con 2 niveles de coautoria tanto en researchObjects como en Documentos               
                    string select = "SELECT * WHERE { SELECT DISTINCT ?persona3 ?orcid3 ?usuarioFigShare3 ?usuarioGitHub3 ?nombreCompleto FROM <http://gnoss.com/document.owl> FROM <http://gnoss.com/researchobject.owl> ";
                    string where = $@"WHERE {{
                                ?documento a ?rdfType. 
                                FILTER(?rdfType in (<http://purl.org/ontology/bibo/Document>,<http://w3id.org/roh/ResearchObject>))
                                ?documento <http://purl.org/ontology/bibo/authorList> ?listaAutores. 
                                ?listaAutores <http://www.w3.org/1999/02/22-rdf-syntax-ns#member> ?persona. 
                                ?persona <{pProperty}> ?propIdentificador. 
                                FILTER(?propIdentificador = '{pPropertyValue}')
                                ?documento <http://purl.org/ontology/bibo/authorList> ?listaAutores2. 
                                ?listaAutores2 <http://www.w3.org/1999/02/22-rdf-syntax-ns#member> ?persona2.
                                ?documento2 <http://purl.org/ontology/bibo/authorList> ?listaAutores3.  
                                ?listaAutores3 <http://www.w3.org/1999/02/22-rdf-syntax-ns#member> ?persona2. 
                                ?listaAutores3 <http://www.w3.org/1999/02/22-rdf-syntax-ns#member> ?persona3. 
                                OPTIONAL{{?persona3 <http://w3id.org/roh/ORCID> ?orcid3. }}
                                OPTIONAL{{?persona3 <http://w3id.org/roh/usuarioFigShare> ?usuarioFigShare3. }}
                                OPTIONAL{{?persona3 <http://w3id.org/roh/usuarioGitHub> ?usuarioGitHub3. }}
                                ?persona3 <http://xmlns.com/foaf/0.1/name> ?nombreCompleto.
                            }} ORDER BY DESC(?persona3) }} LIMIT {limit} OFFSET {offset}";

                    SparqlObject resultadoQuery = mResourceApi.VirtuosoQuery(select, where, "person");
                    if (resultadoQuery != null && resultadoQuery.results != null && resultadoQuery.results.bindings != null && resultadoQuery.results.bindings.Count > 0)
                    {
                        offset += limit;
                        foreach (Dictionary<string, SparqlObject.Data> fila in resultadoQuery.results.bindings)
                        {
                            PersonaPub persona = new PersonaPub();
                            if (fila.ContainsKey("orcid3"))
                            {
                                persona.orcid = fila["orcid3"].value;
                            }
                            persona.name = new Name();
                            persona.name.nombre_completo = new List<string>() { fila["nombreCompleto"].value };
                            DisambiguationPerson person = GetDisambiguationPerson(persona);
                            person.ID = fila["persona3"].value;
                            if (fila.ContainsKey("usuarioFigShare3"))
                            {
                                person.figShareId = fila["usuarioFigShare3"].value;
                            }
                            if (fila.ContainsKey("usuarioGitHub3"))
                            {
                                person.gitHubId = fila["usuarioGitHub3"].value;
                            }
                            listaPersonas.Add(fila["persona3"].value, person);
                        }
                        if (resultadoQuery.results.bindings.Count < limit)
                        {
                            salirBucle = true;
                        }
                    }
                    else
                    {
                        salirBucle = true;
                    }
                } while (!salirBucle);
            }

            {
                int limit = 10000;
                int offset = 0;
                bool salirBucle = false;

                Dictionary<string, HashSet<string>> autoresDocRos = new Dictionary<string, HashSet<string>>();
                // Consulta sparql.
                do
                {
                    //TODO from
                    //Obtenemos las coautorias
                    string select = "SELECT * WHERE { SELECT DISTINCT ?documento2 ?persona3   FROM <http://gnoss.com/document.owl> FROM <http://gnoss.com/researchobject.owl> ";
                    string where = $@"WHERE {{
                                ?documento a ?rdfType. 
                                FILTER(?rdfType in (<http://purl.org/ontology/bibo/Document>,<http://w3id.org/roh/ResearchObject>))
                                ?documento <http://purl.org/ontology/bibo/authorList> ?listaAutores. 
                                ?listaAutores <http://www.w3.org/1999/02/22-rdf-syntax-ns#member> ?persona. 
                                 ?persona <{pProperty}> ?propIdentificador. 
                                FILTER(?propIdentificador = '{pPropertyValue}')
                                ?documento <http://purl.org/ontology/bibo/authorList> ?listaAutores2. 
                                ?listaAutores2 <http://www.w3.org/1999/02/22-rdf-syntax-ns#member> ?persona2.
                                ?listaAutores3 <http://www.w3.org/1999/02/22-rdf-syntax-ns#member> ?persona2. 
                                ?listaAutores3 <http://www.w3.org/1999/02/22-rdf-syntax-ns#member> ?persona3. 
                                ?documento2 <http://purl.org/ontology/bibo/authorList> ?listaAutores3. 
                            }} ORDER BY DESC(?documento2) }} LIMIT {limit} OFFSET {offset}";

                    SparqlObject resultadoQuery = mResourceApi.VirtuosoQuery(select, where, "person");
                    if (resultadoQuery != null && resultadoQuery.results != null && resultadoQuery.results.bindings != null && resultadoQuery.results.bindings.Count > 0)
                    {
                        offset += limit;
                        foreach (Dictionary<string, SparqlObject.Data> fila in resultadoQuery.results.bindings)
                        {
                            string persona = fila["persona3"].value;
                            string documento = fila["documento2"].value;
                            if (!autoresDocRos.ContainsKey(documento))
                            {
                                autoresDocRos[documento] = new HashSet<string>();
                            }
                            autoresDocRos[documento].Add(persona);
                        }
                        if (resultadoQuery.results.bindings.Count < limit)
                        {
                            salirBucle = true;
                        }
                    }
                    else
                    {
                        salirBucle = true;
                    }
                } while (!salirBucle);

                foreach (string idDoc in autoresDocRos.Keys)
                {
                    foreach (string idPersona in autoresDocRos[idDoc])
                    {
                        DisambiguationPerson persona = (DisambiguationPerson)(listaPersonas.Where(x => x.Key == idPersona).FirstOrDefault().Value);
                        if (persona != null)
                        {
                            if (persona.documentos == null)
                            {
                                persona.documentos = new HashSet<string>();
                            }
                            if (persona.coautores == null)
                            {
                                persona.coautores = new HashSet<string>();
                            }
                            persona.documentos.Add(idDoc);
                            persona.coautores.UnionWith(new HashSet<string>(autoresDocRos[idDoc].Except(new HashSet<string>() { idPersona })));
                        }
                    }
                }
            }
            return listaPersonas;
        }

        /// <summary>
        /// Obtiene las publicaciones cargadas de BBDD mediante un orcid.
        /// </summary>
        /// <param name="pOrcid">Código ORCID de la persona a obtener los datos.</param>
        /// <returns>Diccionario con el ID del recurso cargado como clave, y el objeto desambiguable como valor.</returns>
        private static Dictionary<string, DisambiguableEntity> ObtenerPublicacionesBBDD(string pOrcid)
        {
            Dictionary<string, DisambiguableEntity> listaDocumentos = new Dictionary<string, DisambiguableEntity>();
            int limit = 10000;
            int offset = 0;
            bool salirBucle = false;

            // Consulta sparql.
            do
            {
                string select = "SELECT * WHERE { SELECT DISTINCT ?documento ?doi ?titulo FROM <http://gnoss.com/person.owl> ";
                string where = $@"WHERE {{
                                ?documento a <http://purl.org/ontology/bibo/Document>. 
                                OPTIONAL{{?documento <http://purl.org/ontology/bibo/doi> ?doi. }}
                                ?documento <http://w3id.org/roh/title> ?titulo. 
                                ?documento <http://purl.org/ontology/bibo/authorList> ?listaAutores. 
                                ?listaAutores <http://www.w3.org/1999/02/22-rdf-syntax-ns#member> ?persona. 
                                ?persona <http://w3id.org/roh/ORCID> ?orcid. 
                                FILTER(?orcid = '{pOrcid}') 
                            }} ORDER BY DESC(?documento) }} LIMIT {limit} OFFSET {offset}";

                SparqlObject resultadoQuery = mResourceApi.VirtuosoQuery(select, where, "document");
                if (resultadoQuery != null && resultadoQuery.results != null && resultadoQuery.results.bindings != null && resultadoQuery.results.bindings.Count > 0)
                {
                    offset += limit;
                    foreach (Dictionary<string, SparqlObject.Data> fila in resultadoQuery.results.bindings)
                    {
                        Publication publicacion = new Publication();
                        if (fila.ContainsKey("doi"))
                        {
                            publicacion.doi = fila["doi"].value;
                        }
                        publicacion.title = fila["titulo"].value;
                        DisambiguationPublication pub = GetDisambiguationPublication(publicacion);
                        pub.ID = fila["documento"].value;
                        pub.autores = new HashSet<string>();
                        listaDocumentos.Add(fila["documento"].value, pub);
                    }
                    if (resultadoQuery.results.bindings.Count < limit)
                    {
                        salirBucle = true;
                    }
                }
                else
                {
                    salirBucle = true;
                }
            } while (!salirBucle);

            List<List<string>> listaListasDocs = SplitList(listaDocumentos.Keys.ToList(), 1000).ToList();

            limit = 10000;
            offset = 0;
            salirBucle = false;
            foreach (List<string> lista in listaListasDocs)
            {
                // Consulta sparql.
                do
                {
                    string select = "SELECT * WHERE { SELECT DISTINCT ?documento ?autor ";
                    string where = $@"WHERE {{
                                ?documento a <http://purl.org/ontology/bibo/Document>. 
                                ?documento <http://purl.org/ontology/bibo/authorList> ?listaAutores. 
                                ?listaAutores <http://www.w3.org/1999/02/22-rdf-syntax-ns#member> ?autor. 
                                FILTER(?documento in (<{string.Join(">,<", lista)}>)) 
                            }} ORDER BY DESC(?documento) }} LIMIT {limit} OFFSET {offset}";

                    SparqlObject resultadoQuery = mResourceApi.VirtuosoQuery(select, where, "document");
                    if (resultadoQuery != null && resultadoQuery.results != null && resultadoQuery.results.bindings != null && resultadoQuery.results.bindings.Count > 0)
                    {
                        offset += limit;
                        foreach (Dictionary<string, SparqlObject.Data> fila in resultadoQuery.results.bindings)
                        {
                            string doc = fila["documento"].value;
                            string autor = fila["autor"].value;
                            ((DisambiguationPublication)listaDocumentos[doc]).autores.Add(autor);
                        }
                        if (resultadoQuery.results.bindings.Count < limit)
                        {
                            salirBucle = true;
                        }
                    }
                    else
                    {
                        salirBucle = true;
                    }
                } while (!salirBucle);
            }

            return listaDocumentos;
        }

        /// <summary>
        /// Obtiene los research objects de una persona en función del valor de una propiedad.
        /// </summary>
        /// <param name="pProperty">Propiedad para buscar en la persona</param>
        /// <param name="pPropertyValue">Valor de la propiedad para buscar en la persona</param>
        /// <returns>Diccionario con el ID del recurso cargado como clave, y el objeto desambiguable como valor.</returns>
        private static Dictionary<string, DisambiguableEntity> ObtenerResearchObjectsBBDD(string pProperty, string pPropertyValue)
        {
            Dictionary<string, DisambiguableEntity> listaDocumentos = new Dictionary<string, DisambiguableEntity>();
            int limit = 10000;
            int offset = 0;
            bool salirBucle = false;

            // Consulta sparql.
            do
            {
                string select = "SELECT * WHERE { SELECT DISTINCT ?documento ?doi ?titulo ?idFigShare ?idGit FROM <http://gnoss.com/person.owl> ";
                string where = $@"WHERE {{
                                ?documento a <http://w3id.org/roh/ResearchObject>. 
                                OPTIONAL{{?documento <http://purl.org/ontology/bibo/doi> ?doi. }}
                                OPTIONAL{{?documento <http://w3id.org/roh/idFigShare> ?idFigShare. }}
                                OPTIONAL{{?documento <http://w3id.org/roh/idGit> ?idGit. }}
                                ?documento <http://w3id.org/roh/title> ?titulo. 
                                ?documento <http://purl.org/ontology/bibo/authorList> ?listaAutores. 
                                ?listaAutores <http://www.w3.org/1999/02/22-rdf-syntax-ns#member> ?persona. 
                                ?persona <{pProperty}> ?propIdentificador. 
                                FILTER(?propIdentificador = '{pPropertyValue}')
                            }} ORDER BY DESC(?documento) }} LIMIT {limit} OFFSET {offset}";

                SparqlObject resultadoQuery = mResourceApi.VirtuosoQuery(select, where, "researchobject");
                if (resultadoQuery != null && resultadoQuery.results != null && resultadoQuery.results.bindings != null && resultadoQuery.results.bindings.Count > 0)
                {
                    offset += limit;
                    foreach (Dictionary<string, SparqlObject.Data> fila in resultadoQuery.results.bindings)
                    {
                        if (fila.ContainsKey("idFigShare"))
                        {
                            ResearchObjectFigShare researchObject = new ResearchObjectFigShare();
                            if (fila.ContainsKey("doi"))
                            {
                                researchObject.doi = fila["doi"].value;
                            }
                            researchObject.id = int.Parse(fila["idFigShare"].value);
                            researchObject.titulo = fila["titulo"].value;
                            DisambiguationRO pub = GetDisambiguationRO(researchObject);
                            pub.ID = fila["documento"].value;
                            pub.autores = new HashSet<string>();
                            listaDocumentos.Add(fila["documento"].value, pub);
                        }
                        else if (fila.ContainsKey("idGit"))
                        {
                            ResearchObjectGitHub researchObject = new ResearchObjectGitHub();
                            researchObject.id = int.Parse(fila["idGit"].value);
                            researchObject.titulo = fila["titulo"].value;
                            DisambiguationRO pub = GetDisambiguationRoGithub(researchObject);
                            pub.ID = fila["documento"].value;
                            pub.autores = new HashSet<string>();
                            listaDocumentos.Add(fila["documento"].value, pub);
                        }

                    }
                    if (resultadoQuery.results.bindings.Count < limit)
                    {
                        salirBucle = true;
                    }
                }
                else
                {
                    salirBucle = true;
                }
            } while (!salirBucle);

            List<List<string>> listaListasDocs = SplitList(listaDocumentos.Keys.ToList(), 1000).ToList();

            limit = 10000;
            offset = 0;
            salirBucle = false;
            foreach (List<string> lista in listaListasDocs)
            {
                // Consulta sparql.
                do
                {
                    string select = "SELECT * WHERE { SELECT DISTINCT ?documento ?autor ";
                    string where = $@"WHERE {{
                                ?documento a <http://w3id.org/roh/ResearchObject>. 
                                ?documento <http://purl.org/ontology/bibo/authorList> ?listaAutores. 
                                ?listaAutores <http://www.w3.org/1999/02/22-rdf-syntax-ns#member> ?autor. 
                                FILTER(?documento in (<{string.Join(">,<", lista)}>)) 
                            }} ORDER BY DESC(?documento) }} LIMIT {limit} OFFSET {offset}";

                    SparqlObject resultadoQuery = mResourceApi.VirtuosoQuery(select, where, "researchobject");
                    if (resultadoQuery != null && resultadoQuery.results != null && resultadoQuery.results.bindings != null && resultadoQuery.results.bindings.Count > 0)
                    {
                        offset += limit;
                        foreach (Dictionary<string, SparqlObject.Data> fila in resultadoQuery.results.bindings)
                        {
                            string doc = fila["documento"].value;
                            string autor = fila["autor"].value;
                            ((DisambiguationRO)listaDocumentos[doc]).autores.Add(autor);
                        }
                        if (resultadoQuery.results.bindings.Count < limit)
                        {
                            salirBucle = true;
                        }
                    }
                    else
                    {
                        salirBucle = true;
                    }
                } while (!salirBucle);
            }

            return listaDocumentos;
        }

        /// <summary>
        /// Obtiene las personas iguales para poder desambiguarlas.
        /// </summary>
        /// <param name="idPersona">ID de la persona a desambiguar.</param>
        /// <param name="pListaIds">Listado de IDs de las personas iguales.</param>
        /// <param name="pDicIdPersona">Diccionario con el ID de la persona y el objeto al que corresponde.</param>
        /// <param name="pListaPersonasCreadas">Diccionario con el objeto creado y la lista de IDs de las personas que le corresponden.</param>
        private static void CrearPersonDesambiguada(string idPersona, HashSet<string> pListaIds, Dictionary<string, Person> pDicIdPersona, Dictionary<Person, HashSet<string>> pListaPersonasCreadas)
        {
            Person personaA = pDicIdPersona[idPersona];
            foreach (string idSimilar in pListaIds)
            {
                Person personaB = pDicIdPersona[idSimilar];
                FusionarPerson(personaA, personaB);
            }

            HashSet<string> listaTotalIds = pListaIds;
            listaTotalIds.Add(idPersona);
            pListaPersonasCreadas.Add(personaA, listaTotalIds);
        }

        /// <summary>
        /// Compara las propiedades para rellenar los datos faltantes.
        /// </summary>
        /// <param name="pPersonaA">Primera persona (A).</param>
        /// <param name="pPersonaB">Segunda persona (B).</param>
        private static void FusionarPerson(Person pPersonaA, Person pPersonaB)
        {
            if (string.IsNullOrEmpty(pPersonaA.Roh_ORCID) && !string.IsNullOrEmpty(pPersonaB.Roh_ORCID))
            {
                pPersonaA.Roh_ORCID = pPersonaB.Roh_ORCID;
            }

            if (string.IsNullOrEmpty(pPersonaA.Vivo_researcherId) && !string.IsNullOrEmpty(pPersonaB.Vivo_researcherId))
            {
                pPersonaA.Vivo_researcherId = pPersonaB.Vivo_researcherId;
            }

            int nombreCompletoA = 0;
            int nombreCompletoB = 0;

            if (!string.IsNullOrEmpty(pPersonaA.Foaf_firstName) && !string.IsNullOrEmpty(pPersonaA.Foaf_lastName))
            {
                nombreCompletoA = $@"{pPersonaA.Foaf_firstName} {pPersonaA.Foaf_lastName}".Trim().Length;
            }

            if (!string.IsNullOrEmpty(pPersonaB.Foaf_firstName) && !string.IsNullOrEmpty(pPersonaB.Foaf_lastName))
            {
                nombreCompletoB = $@"{pPersonaB.Foaf_firstName} {pPersonaB.Foaf_lastName}".Trim().Length;
            }

            if (!string.IsNullOrEmpty(pPersonaA.Foaf_firstName) && !string.IsNullOrEmpty(pPersonaB.Foaf_firstName))
            {
                if ((nombreCompletoA < nombreCompletoB) || (nombreCompletoA == nombreCompletoB))
                {
                    pPersonaA.Foaf_firstName = pPersonaB.Foaf_firstName.Trim();
                }
            }

            if (!string.IsNullOrEmpty(pPersonaA.Foaf_lastName) && !string.IsNullOrEmpty(pPersonaB.Foaf_lastName))
            {
                if ((nombreCompletoA < nombreCompletoB) || (nombreCompletoA == nombreCompletoB))
                {
                    pPersonaA.Foaf_lastName = pPersonaB.Foaf_lastName.Trim();
                }
            }

            pPersonaA.Foaf_name = $@"{pPersonaA.Foaf_firstName} {pPersonaA.Foaf_lastName}";

            if (string.IsNullOrEmpty(pPersonaA.Roh_semanticScholarId) && !string.IsNullOrEmpty(pPersonaB.Roh_semanticScholarId))
            {
                pPersonaA.Roh_semanticScholarId = pPersonaB.Roh_semanticScholarId;
            }
        }

        /// <summary>
        /// Obtiene las publicaciones iguales para poder desambiguarlas.
        /// </summary>
        /// <param name="idPublicacion"></param>
        /// <param name="pListaIds"></param>
        /// <param name="pDicIdPublicacion"></param>
        /// <param name="pListaPublicacionesCreadas"></param>
        /// <param name="pDicAreasBroader"></param>
        /// <param name="pDicAreasNombre"></param>
        private static void CrearDocumentDesambiguado(string idPublicacion, HashSet<string> pListaIds, Dictionary<string, Publication> pDicIdPublicacion, Dictionary<Document, HashSet<string>> pListaPublicacionesCreadas, Dictionary<string, string> pDicAreasBroader, Dictionary<string, string> pDicAreasNombre)
        {
            Publication documentoA = pDicIdPublicacion[idPublicacion];
            Document documentoCreado = new Document();

            foreach (string idSimilar in pListaIds)
            {
                Publication documentoB = pDicIdPublicacion[idSimilar];
                documentoCreado = ConstruirDocument(documentoA, pDicAreasBroader, pDicAreasNombre, pPublicacionB: documentoB);
            }

            HashSet<string> listaTotalIds = pListaIds;
            listaTotalIds.Add(idPublicacion);
            pListaPublicacionesCreadas.Add(documentoCreado, listaTotalIds);
        }

        private static void CrearRoFigshareDesambiguado(string idRo, HashSet<string> pListaIds, Dictionary<string, ResearchObjectFigShare> pDicIdRo, Dictionary<ResearchobjectOntology.ResearchObject, HashSet<string>> pListaRosCreados, Dictionary<string, string> pDicAreasBroader, Dictionary<string, string> pDicAreasNombre)
        {
            ResearchObjectFigShare roA = pDicIdRo[idRo];
            ResearchobjectOntology.ResearchObject roCreado = new ResearchobjectOntology.ResearchObject();

            foreach (string idSimilar in pListaIds)
            {
                ResearchObjectFigShare roB = pDicIdRo[idSimilar];
                roCreado = ConstruirRO("FigShare", roA, null, pDicAreasBroader, pDicAreasNombre, pResearchObjectB: roB);
            }

            HashSet<string> listaTotalIds = pListaIds;
            listaTotalIds.Add(idRo);
            pListaRosCreados.Add(roCreado, listaTotalIds);
        }

        private static void CrearRoGitHubDesambiguado(string idRo, HashSet<string> pListaIds, Dictionary<string, ResearchObjectGitHub> pDicIdRo, Dictionary<ResearchobjectOntology.ResearchObject, HashSet<string>> pListaRosCreados, Dictionary<string, string> pDicAreasBroader, Dictionary<string, string> pDicAreasNombre)
        {
            ResearchObjectGitHub roA = pDicIdRo[idRo];
            ResearchobjectOntology.ResearchObject roCreado = new ResearchobjectOntology.ResearchObject();

            foreach (string idSimilar in pListaIds)
            {
                ResearchObjectGitHub roB = pDicIdRo[idSimilar];
                roCreado = ConstruirRO("GitHub", null, roA, pDicAreasBroader, pDicAreasNombre, pGitHubObjB: roB);
            }

            HashSet<string> listaTotalIds = pListaIds;
            listaTotalIds.Add(idRo);
            pListaRosCreados.Add(roCreado, listaTotalIds);
        }

        public static ResearchobjectOntology.ResearchObject ConstruirRO(string pTipo, ResearchObjectFigShare pResearchObject, ResearchObjectGitHub pGitHubObj, Dictionary<string, string> pDicAreasBroader, Dictionary<string, string> pDicAreasNombre, ResearchObjectFigShare pResearchObjectB = null, ResearchObjectGitHub pGitHubObjB = null)
        {
            ResearchobjectOntology.ResearchObject ro = new ResearchobjectOntology.ResearchObject();

            // Estado de validación (IsValidated)
            ro.Roh_isValidated = true;

            if (pTipo == "FigShare")
            {
                // ID
                if (pResearchObject.id.HasValue)
                {
                    ro.Roh_idFigShare = pResearchObject.id.Value.ToString();

                    if (pResearchObjectB != null && pResearchObjectB.id.HasValue && string.IsNullOrEmpty(ro.Roh_idFigShare))
                    {
                        ro.Roh_idFigShare = pResearchObjectB.id.Value.ToString();
                    }
                }

                // DOI
                if (!string.IsNullOrEmpty(pResearchObject.doi))
                {
                    ro.Bibo_doi = pResearchObject.doi;

                    if (pResearchObjectB != null && !string.IsNullOrEmpty(pResearchObjectB.doi) && string.IsNullOrEmpty(ro.Bibo_doi))
                    {
                        ro.Bibo_doi = pResearchObjectB.doi;
                    }
                }

                // ResearchObject Type
                if (!string.IsNullOrEmpty(pResearchObject.tipo))
                {
                    switch (pResearchObject.tipo)
                    {
                        case "dataset":
                            ro.IdDc_type = "http://gnoss.com/items/researchobjecttype_1";
                            break;
                        case "presentation":
                            ro.IdDc_type = "http://gnoss.com/items/researchobjecttype_2";
                            break;
                        case "figure":
                            ro.IdDc_type = "http://gnoss.com/items/researchobjecttype_3";
                            break;
                    }

                    if (pResearchObjectB != null && !string.IsNullOrEmpty(pResearchObjectB.tipo) && string.IsNullOrEmpty(ro.IdDc_type))
                    {
                        switch (pResearchObjectB.tipo)
                        {
                            case "dataset":
                                ro.IdDc_type = "http://gnoss.com/items/researchobjecttype_1";
                                break;
                            case "presentation":
                                ro.IdDc_type = "http://gnoss.com/items/researchobjecttype_2";
                                break;
                            case "figure":
                                ro.IdDc_type = "http://gnoss.com/items/researchobjecttype_3";
                                break;
                        }
                    }
                }

                // Título.
                if (!string.IsNullOrEmpty(pResearchObject.titulo))
                {
                    ro.Roh_title = pResearchObject.titulo;

                    if (pResearchObjectB != null && !string.IsNullOrEmpty(pResearchObjectB.titulo) && string.IsNullOrEmpty(ro.Roh_title))
                    {
                        ro.Roh_title = pResearchObjectB.titulo;
                    }
                }

                // Descripción.
                if (!string.IsNullOrEmpty(pResearchObject.descripcion))
                {
                    ro.Bibo_abstract = pResearchObject.descripcion;

                    if (pResearchObjectB != null && !string.IsNullOrEmpty(pResearchObjectB.descripcion) && string.IsNullOrEmpty(ro.Bibo_abstract))
                    {
                        ro.Bibo_abstract = pResearchObjectB.descripcion;
                    }
                }

                // URL
                if (!string.IsNullOrEmpty(pResearchObject.url))
                {
                    ro.Vcard_url = pResearchObject.url;

                    if (pResearchObjectB != null && !string.IsNullOrEmpty(pResearchObjectB.url) && string.IsNullOrEmpty(ro.Vcard_url))
                    {
                        ro.Vcard_url = pResearchObjectB.url;
                    }
                }

                // Fecha Publicación
                if (!string.IsNullOrEmpty(pResearchObject.fechaPublicacion))
                {
                    int dia = Int32.Parse(pResearchObject.fechaPublicacion.Split(" ")[0].Split("/")[1]);
                    int mes = Int32.Parse(pResearchObject.fechaPublicacion.Split(" ")[0].Split("/")[0]);
                    int anyo = Int32.Parse(pResearchObject.fechaPublicacion.Split(" ")[0].Split("/")[2]);

                    ro.Roh_updatedDate = new DateTime(anyo, mes, dia);

                    if (pResearchObjectB != null && !string.IsNullOrEmpty(pResearchObjectB.fechaPublicacion) && ro.Roh_updatedDate == null)
                    {
                        dia = Int32.Parse(pResearchObjectB.fechaPublicacion.Split(" ")[0].Split("/")[0]);
                        mes = Int32.Parse(pResearchObjectB.fechaPublicacion.Split(" ")[0].Split("/")[1]);
                        anyo = Int32.Parse(pResearchObjectB.fechaPublicacion.Split(" ")[0].Split("/")[2]);

                        ro.Roh_updatedDate = new DateTime(anyo, mes, dia);
                    }
                }

                // Etiquetas
                if (pResearchObject.etiquetas != null && pResearchObject.etiquetas.Any())
                {
                    ro.Roh_externalKeywords = pResearchObject.etiquetas;

                    if (pResearchObjectB != null && pResearchObjectB.etiquetas != null && pResearchObjectB.etiquetas.Any())
                    {
                        ro.Roh_externalKeywords = pResearchObjectB.etiquetas;
                    }
                }

                // Etiquetas Enriquecidas
                if (pResearchObject.etiquetasEnriquecidas != null && pResearchObject.etiquetasEnriquecidas.Any())
                {
                    ro.Roh_enrichedKeywords = pResearchObject.etiquetasEnriquecidas;

                    if (pResearchObjectB != null && pResearchObjectB.etiquetasEnriquecidas != null && pResearchObjectB.etiquetasEnriquecidas.Any())
                    {
                        ro.Roh_enrichedKeywords = pResearchObjectB.etiquetasEnriquecidas;
                    }
                }

                // Categorias Enriquecidas
                HashSet<string> listaIDs = new HashSet<string>();
                if (pResearchObject.categoriasEnriquecidas != null && pResearchObject.categoriasEnriquecidas.Count > 0)
                {
                    ro.Roh_enrichedKnowledgeArea = new List<ResearchobjectOntology.CategoryPath>();
                    foreach (string area in pResearchObject.categoriasEnriquecidas)
                    {
                        if (pDicAreasNombre.ContainsKey(area.ToLower()))
                        {
                            ResearchobjectOntology.CategoryPath categoria = new ResearchobjectOntology.CategoryPath();
                            categoria.IdsRoh_categoryNode = new List<string>();
                            categoria.IdsRoh_categoryNode.Add(pDicAreasNombre[area.ToLower()]);
                            string idHijo = pDicAreasNombre[area.ToLower()];
                            string idHijoAux = idHijo;
                            if (!listaIDs.Contains(idHijo))
                            {
                                while (!idHijo.EndsWith(".0.0.0"))
                                {
                                    categoria.IdsRoh_categoryNode.Add(pDicAreasBroader[idHijo]);
                                    idHijo = pDicAreasBroader[idHijo];
                                }
                                if (categoria.IdsRoh_categoryNode.Count > 0)
                                {
                                    ro.Roh_enrichedKnowledgeArea.Add(categoria);
                                }
                            }
                            listaIDs.Add(idHijoAux);
                        }
                    }

                    if (pResearchObjectB != null && pResearchObjectB.categoriasEnriquecidas != null && pResearchObjectB.categoriasEnriquecidas.Any())
                    {
                        ro.Roh_enrichedKnowledgeArea = new List<ResearchobjectOntology.CategoryPath>();
                        foreach (string area in pResearchObjectB.categoriasEnriquecidas)
                        {
                            if (pDicAreasNombre.ContainsKey(area.ToLower()))
                            {
                                ResearchobjectOntology.CategoryPath categoria = new ResearchobjectOntology.CategoryPath();
                                categoria.IdsRoh_categoryNode = new List<string>();
                                categoria.IdsRoh_categoryNode.Add(pDicAreasNombre[area.ToLower()]);
                                string idHijo = pDicAreasNombre[area.ToLower()];
                                string idHijoAux = idHijo;
                                if (!listaIDs.Contains(idHijo))
                                {
                                    while (!idHijo.EndsWith(".0.0.0"))
                                    {
                                        categoria.IdsRoh_categoryNode.Add(pDicAreasBroader[idHijo]);
                                        idHijo = pDicAreasBroader[idHijo];
                                    }
                                    if (categoria.IdsRoh_categoryNode.Count > 0)
                                    {
                                        ro.Roh_enrichedKnowledgeArea.Add(categoria);
                                    }
                                }
                                listaIDs.Add(idHijoAux);
                            }
                        }
                    }
                }

                // Licencia
                if (!string.IsNullOrEmpty(pResearchObject.licencia))
                {
                    ro.Dct_license = pResearchObject.licencia;

                    if (pResearchObjectB != null && !string.IsNullOrEmpty(pResearchObjectB.licencia) && string.IsNullOrEmpty(ro.Dct_license))
                    {
                        ro.Dct_license = pResearchObjectB.licencia;
                    }
                }

                // Autores
                if (pResearchObject.autores != null && pResearchObject.autores.Any())
                {
                    ro.Bibo_authorList = new List<ResearchobjectOntology.BFO_0000023>();
                    int orden = 1;
                    foreach (PersonRO personaRO in pResearchObject.autores)
                    {
                        ResearchobjectOntology.BFO_0000023 bfo_0000023 = new ResearchobjectOntology.BFO_0000023();
                        bfo_0000023.Rdf_comment = orden;
                        bfo_0000023.IdRdf_member = personaRO.ID;
                        ro.Bibo_authorList.Add(bfo_0000023);
                        orden++;
                    }
                }

            }
            else if (pTipo == "GitHub")
            {
                // ID
                if (pGitHubObj.id.HasValue)
                {
                    ro.Roh_idGit = pGitHubObj.id.Value.ToString();

                    if (pGitHubObjB != null && pGitHubObjB.id.HasValue && string.IsNullOrEmpty(ro.Roh_idGit))
                    {
                        ro.Roh_idGit = pGitHubObjB.id.Value.ToString();
                    }
                }

                // ResearchObject Type
                if (!string.IsNullOrEmpty(pGitHubObj.tipo))
                {
                    ro.IdDc_type = "http://gnoss.com/items/researchobjecttype_9";

                    if (pGitHubObjB != null && !string.IsNullOrEmpty(pGitHubObjB.tipo) && string.IsNullOrEmpty(ro.IdDc_type))
                    {
                        ro.IdDc_type = "http://gnoss.com/items/researchobjecttype_9";
                    }
                }

                // Título.
                if (!string.IsNullOrEmpty(pGitHubObj.titulo))
                {
                    ro.Roh_title = pGitHubObj.titulo;

                    if (pGitHubObjB != null && !string.IsNullOrEmpty(pGitHubObjB.titulo) && string.IsNullOrEmpty(ro.Roh_title))
                    {
                        ro.Roh_title = pGitHubObjB.titulo;
                    }
                }

                // Descripción.
                if (!string.IsNullOrEmpty(pGitHubObj.descripcion))
                {
                    ro.Bibo_abstract = pGitHubObj.descripcion;

                    if (pGitHubObjB != null && !string.IsNullOrEmpty(pGitHubObjB.descripcion) && string.IsNullOrEmpty(ro.Bibo_abstract))
                    {
                        ro.Bibo_abstract = pGitHubObjB.descripcion;
                    }
                }

                // URL
                if (!string.IsNullOrEmpty(pGitHubObj.url))
                {
                    ro.Vcard_url = pGitHubObj.url;

                    if (pGitHubObjB != null && !string.IsNullOrEmpty(pGitHubObjB.url) && string.IsNullOrEmpty(ro.Vcard_url))
                    {
                        ro.Vcard_url = pGitHubObjB.url;
                    }
                }

                // Fecha Actualización
                if (!string.IsNullOrEmpty(pGitHubObj.fechaActualizacion))
                {
                    int dia = Int32.Parse(pGitHubObj.fechaActualizacion.Split(" ")[0].Split("/")[1]);
                    int mes = Int32.Parse(pGitHubObj.fechaActualizacion.Split(" ")[0].Split("/")[0]);
                    int anyo = Int32.Parse(pGitHubObj.fechaActualizacion.Split(" ")[0].Split("/")[2]);

                    ro.Roh_updatedDate = new DateTime(anyo, mes, dia);

                    if (pGitHubObjB != null && !string.IsNullOrEmpty(pGitHubObjB.fechaActualizacion) && ro.Roh_updatedDate == null)
                    {
                        dia = Int32.Parse(pGitHubObjB.fechaActualizacion.Split(" ")[0].Split("/")[1]);
                        mes = Int32.Parse(pGitHubObjB.fechaActualizacion.Split(" ")[0].Split("/")[0]);
                        anyo = Int32.Parse(pGitHubObjB.fechaActualizacion.Split(" ")[0].Split("/")[2]);

                        ro.Roh_updatedDate = new DateTime(anyo, mes, dia);
                    }
                }

                // Fecha Creación
                if (!string.IsNullOrEmpty(pGitHubObj.fechaCreacion))
                {
                    int dia = Int32.Parse(pGitHubObj.fechaCreacion.Split(" ")[0].Split("/")[1]);
                    int mes = Int32.Parse(pGitHubObj.fechaCreacion.Split(" ")[0].Split("/")[0]);
                    int anyo = Int32.Parse(pGitHubObj.fechaCreacion.Split(" ")[0].Split("/")[2]);

                    ro.Dct_issued = new DateTime(anyo, mes, dia);

                    if (pGitHubObjB != null && !string.IsNullOrEmpty(pGitHubObjB.fechaCreacion) && ro.Roh_updatedDate == null)
                    {
                        dia = Int32.Parse(pGitHubObjB.fechaCreacion.Split(" ")[0].Split("/")[1]);
                        mes = Int32.Parse(pGitHubObjB.fechaCreacion.Split(" ")[0].Split("/")[0]);
                        anyo = Int32.Parse(pGitHubObjB.fechaCreacion.Split(" ")[0].Split("/")[2]);

                        ro.Dct_issued = new DateTime(anyo, mes, dia);
                    }
                }

                // Lenguajes de programación.
                if (pGitHubObj.lenguajes != null && pGitHubObj.lenguajes.Any())
                {
                    ro.Vcard_hasLanguage = new List<ResearchobjectOntology.Language>();

                    foreach (KeyValuePair<string, float> item in pGitHubObj.lenguajes)
                    {
                        ResearchobjectOntology.Language lenguajeProg = new ResearchobjectOntology.Language();
                        lenguajeProg.Roh_title = item.Key;
                        lenguajeProg.Roh_percentage = item.Value;
                        ro.Vcard_hasLanguage.Add(lenguajeProg);
                    }

                    if (pGitHubObjB != null && pGitHubObjB.lenguajes != null && pGitHubObjB.lenguajes.Any() && ro.Vcard_hasLanguage == null)
                    {
                        ro.Vcard_hasLanguage = new List<ResearchobjectOntology.Language>();

                        foreach (KeyValuePair<string, float> item in pGitHubObjB.lenguajes)
                        {
                            ResearchobjectOntology.Language lenguajeProg = new ResearchobjectOntology.Language();
                            lenguajeProg.Roh_title = item.Key;
                            lenguajeProg.Roh_percentage = item.Value;
                            ro.Vcard_hasLanguage.Add(lenguajeProg);
                        }
                    }
                }

                // Licencia
                if (!string.IsNullOrEmpty(pGitHubObj.licencia))
                {
                    ro.Dct_license = pGitHubObj.licencia;

                    if (pGitHubObjB != null && !string.IsNullOrEmpty(pGitHubObjB.licencia) && string.IsNullOrEmpty(ro.Dct_license))
                    {
                        ro.Dct_license = pGitHubObjB.licencia;
                    }
                }

                // Número de Releases
                if (pGitHubObj.numReleases.HasValue)
                {
                    ro.Roh_releasesNumber = pGitHubObj.numReleases.Value;

                    if (pGitHubObjB != null && pGitHubObjB.numReleases.HasValue && ro.Roh_releasesNumber == null)
                    {
                        ro.Roh_releasesNumber = pGitHubObjB.numReleases.Value;
                    }
                }

                // Número de Forks
                if (pGitHubObj.numForks.HasValue)
                {
                    ro.Roh_forksNumber = pGitHubObj.numForks.Value;

                    if (pGitHubObjB != null && pGitHubObjB.numForks.HasValue && ro.Roh_forksNumber == null)
                    {
                        ro.Roh_forksNumber = pGitHubObjB.numForks.Value;
                    }
                }

                // Número de Issues
                if (pGitHubObj.numIssues.HasValue)
                {
                    ro.Roh_issuesNumber = pGitHubObj.numIssues.Value;

                    if (pGitHubObjB != null && pGitHubObjB.numIssues.HasValue && ro.Roh_issuesNumber == null)
                    {
                        ro.Roh_issuesNumber = pGitHubObjB.numIssues.Value;
                    }
                }

                // Etiquetas
                if (pGitHubObj.etiquetas != null && pGitHubObj.etiquetas.Any())
                {
                    ro.Roh_externalKeywords = pGitHubObj.etiquetas;

                    if (pGitHubObjB != null && pGitHubObjB.etiquetas != null && pGitHubObjB.etiquetas.Any())
                    {
                        ro.Roh_externalKeywords = pGitHubObjB.etiquetas;
                    }
                }

                // Etiquetas Enriquecidas
                if (pGitHubObj.etiquetasEnriquecidas != null && pGitHubObj.etiquetasEnriquecidas.Any())
                {
                    ro.Roh_enrichedKeywords = pGitHubObj.etiquetasEnriquecidas;

                    if (pGitHubObjB != null && pGitHubObjB.etiquetasEnriquecidas != null && pGitHubObjB.etiquetasEnriquecidas.Any())
                    {
                        ro.Roh_enrichedKeywords = pGitHubObjB.etiquetasEnriquecidas;
                    }
                }

                // Categorias Enriquecidas
                HashSet<string> listaIDs = new HashSet<string>();
                if (pGitHubObj.categoriasEnriquecidas != null && pGitHubObj.categoriasEnriquecidas.Count > 0)
                {
                    ro.Roh_enrichedKnowledgeArea = new List<ResearchobjectOntology.CategoryPath>();
                    foreach (string area in pGitHubObj.categoriasEnriquecidas)
                    {
                        if (pDicAreasNombre.ContainsKey(area.ToLower()))
                        {
                            ResearchobjectOntology.CategoryPath categoria = new ResearchobjectOntology.CategoryPath();
                            categoria.IdsRoh_categoryNode = new List<string>();
                            categoria.IdsRoh_categoryNode.Add(pDicAreasNombre[area.ToLower()]);
                            string idHijo = pDicAreasNombre[area.ToLower()];
                            string idHijoAux = idHijo;
                            if (!listaIDs.Contains(idHijo))
                            {
                                while (!idHijo.EndsWith(".0.0.0"))
                                {
                                    categoria.IdsRoh_categoryNode.Add(pDicAreasBroader[idHijo]);
                                    idHijo = pDicAreasBroader[idHijo];
                                }
                                if (categoria.IdsRoh_categoryNode.Count > 0)
                                {
                                    ro.Roh_enrichedKnowledgeArea.Add(categoria);
                                }
                            }
                            listaIDs.Add(idHijoAux);
                        }
                    }

                    if (pGitHubObjB != null && pGitHubObjB.categoriasEnriquecidas != null && pGitHubObjB.categoriasEnriquecidas.Any())
                    {
                        ro.Roh_enrichedKnowledgeArea = new List<ResearchobjectOntology.CategoryPath>();
                        foreach (string area in pGitHubObjB.categoriasEnriquecidas)
                        {
                            if (pDicAreasNombre.ContainsKey(area.ToLower()))
                            {
                                ResearchobjectOntology.CategoryPath categoria = new ResearchobjectOntology.CategoryPath();
                                categoria.IdsRoh_categoryNode = new List<string>();
                                categoria.IdsRoh_categoryNode.Add(pDicAreasNombre[area.ToLower()]);
                                string idHijo = pDicAreasNombre[area.ToLower()];
                                string idHijoAux = idHijo;
                                if (!listaIDs.Contains(idHijo))
                                {
                                    while (!idHijo.EndsWith(".0.0.0"))
                                    {
                                        categoria.IdsRoh_categoryNode.Add(pDicAreasBroader[idHijo]);
                                        idHijo = pDicAreasBroader[idHijo];
                                    }
                                    if (categoria.IdsRoh_categoryNode.Count > 0)
                                    {
                                        ro.Roh_enrichedKnowledgeArea.Add(categoria);
                                    }
                                }
                                listaIDs.Add(idHijoAux);
                            }
                        }
                    }
                }

                // Autores.
                if (pGitHubObj.listaAutores != null && pGitHubObj.listaAutores.Any())
                {
                    ro.Bibo_authorList = new List<ResearchobjectOntology.BFO_0000023>();
                    int seqAutor = 1;
                    foreach (string nombre in pGitHubObj.listaAutores)
                    {
                        string idRecursoPersona = ComprobarPersonaUsuarioGitHub(nombre);
                        if (!string.IsNullOrEmpty(idRecursoPersona))
                        {
                            // Autores.   
                            ResearchobjectOntology.BFO_0000023 miembro = new ResearchobjectOntology.BFO_0000023();
                            miembro.IdRdf_member = idRecursoPersona;
                            miembro.Rdf_comment = seqAutor;
                            seqAutor++;
                            ro.Bibo_authorList.Add(miembro);
                        }
                    }

                    if (pGitHubObjB != null && pGitHubObjB.listaAutores != null && pGitHubObjB.listaAutores.Any())
                    {
                        ro.Bibo_authorList = new List<ResearchobjectOntology.BFO_0000023>();
                        seqAutor = 1;

                        foreach (string nombre in pGitHubObjB.listaAutores)
                        {
                            string idRecursoPersona = ComprobarPersonaUsuarioGitHub(nombre);
                            if (!string.IsNullOrEmpty(idRecursoPersona))
                            {
                                // Autores.   
                                ResearchobjectOntology.BFO_0000023 miembro = new ResearchobjectOntology.BFO_0000023();
                                miembro.IdRdf_member = idRecursoPersona;
                                miembro.Rdf_comment = seqAutor;
                                seqAutor++;
                                ro.Bibo_authorList.Add(miembro);
                            }
                        }
                    }
                }
            }

            return ro;
        }

        /// <summary>
        /// Compara las propiedades para rellenar los datos faltantes. 
        /// TODO: Sacar a métodos el código duplicado. 
        /// </summary>
        /// <param name="pPublicacion"></param>
        /// <param name="pDicAreasBroader"></param>
        /// <param name="pDicAreasNombre"></param>
        /// <param name="pPublicacionB"></param>
        /// <returns></returns>
        public static Document ConstruirDocument(Publication pPublicacion, Dictionary<string, string> pDicAreasBroader, Dictionary<string, string> pDicAreasNombre, Publication pPublicacionB = null)
        {
            Document document = new Document();

            // Estado de validación (IsValidated)
            document.Roh_isValidated = true;

            // ID DOI (Doi)
            if (!string.IsNullOrEmpty(pPublicacion.doi))
            {
                document.Bibo_doi = pPublicacion.doi;

                if (pPublicacionB != null && !string.IsNullOrEmpty(pPublicacionB.doi) && string.IsNullOrEmpty(document.Bibo_doi))
                {
                    document.Bibo_doi = pPublicacionB.doi;
                }
            }

            // Identificadores
            HashSet<string> listaIds = new HashSet<string>();
            if (pPublicacion.iDs != null && pPublicacion.iDs.Any())
            {
                foreach (string id in pPublicacion.iDs)
                {
                    listaIds.Add(id);
                }
            }
            if (listaIds.Count == 0 && pPublicacionB != null)
            {
                if (pPublicacionB.iDs != null && pPublicacionB.iDs.Any())
                {
                    foreach (string id in pPublicacionB.iDs)
                    {
                        listaIds.Add(id);
                    }
                }
            }
            if (listaIds.Any())
            {
                document.Bibo_identifier = new List<fDocument>();

                foreach (string id in listaIds)
                {
                    if (id.Contains(":"))
                    {
                        if (id.ToLower().Contains("wos"))
                        {
                            fDocument fDocumento = new fDocument();
                            fDocumento.Foaf_topic = "WoS";
                            fDocumento.Dc_title = id.Split(":")[1].Trim();
                            document.Bibo_identifier.Add(fDocumento);
                        }

                        if (id.ToLower().Contains("semanticscholar"))
                        {
                            fDocument fDocumento = new fDocument();
                            fDocumento.Foaf_topic = "SemanticScholar";
                            fDocumento.Dc_title = id.Split(":")[1].Trim();
                            document.Bibo_identifier.Add(fDocumento);
                        }

                        if (id.ToLower().Contains("mag"))
                        {
                            fDocument fDocumento = new fDocument();
                            fDocumento.Foaf_topic = "MAG";
                            fDocumento.Dc_title = id.Split(":")[1].Trim();
                            document.Bibo_identifier.Add(fDocumento);
                        }

                        if (id.ToLower().Contains("pubmedcentral"))
                        {
                            fDocument fDocumento = new fDocument();
                            fDocumento.Foaf_topic = "PubMedCentral";
                            fDocumento.Dc_title = id.Split(":")[1].Trim();
                            document.Bibo_identifier.Add(fDocumento);
                        }

                        if (id.ToLower().Contains("scopus_id"))
                        {
                            fDocument fDocumento = new fDocument();
                            fDocumento.Foaf_topic = "Scopus";
                            fDocumento.Dc_title = id.Split(":")[1].Trim();
                            document.Bibo_identifier.Add(fDocumento);
                        }

                        if (id.ToLower().Contains("arxiv"))
                        {
                            fDocument fDocumento = new fDocument();
                            fDocumento.Foaf_topic = "ArXiv";
                            fDocumento.Dc_title = id.Split(":")[1].Trim();
                            document.Bibo_identifier.Add(fDocumento);
                        }

                        if (id.ToLower().Contains("medline"))
                        {
                            fDocument fDocumento = new fDocument();
                            fDocumento.Foaf_topic = "Medline";
                            fDocumento.Dc_title = id.Split(":")[1].Trim();
                            document.Bibo_identifier.Add(fDocumento);
                        }
                    }
                }
            }

            // Título (Title)
            if (!string.IsNullOrEmpty(pPublicacion.title))
            {
                document.Roh_title = pPublicacion.title;

                if (pPublicacionB != null && !string.IsNullOrEmpty(pPublicacionB.title) && string.IsNullOrEmpty(document.Roh_title))
                {
                    document.Roh_title = pPublicacionB.title;
                }
            }

            // Descripción (Abstract)
            if (!string.IsNullOrEmpty(pPublicacion.@abstract))
            {
                string descpLimpia = pPublicacion.@abstract;
                if (pPublicacion.@abstract.StartsWith("[") && pPublicacion.@abstract.EndsWith("]"))
                {
                    descpLimpia = pPublicacion.@abstract.Remove(0, 1);
                    descpLimpia = descpLimpia.Remove(descpLimpia.Length - 1, 1);
                }
                document.Bibo_abstract = descpLimpia.Trim();

                if (pPublicacionB != null && !string.IsNullOrEmpty(pPublicacionB.@abstract) && string.IsNullOrEmpty(document.Bibo_abstract))
                {
                    string descpLimpiaB = pPublicacionB.@abstract;
                    if (pPublicacionB.@abstract.StartsWith("[") && pPublicacionB.@abstract.EndsWith("]"))
                    {
                        descpLimpiaB = pPublicacionB.@abstract.Remove(0, 1);
                        descpLimpiaB = descpLimpiaB.Remove(descpLimpiaB.Length - 1, 1);
                    }
                    document.Bibo_abstract = descpLimpiaB.Trim();
                }
            }

            // Tipo de publicación (TypeOfPublication)
            if (pPublicacion.typeOfPublication != null)
            {
                if (pPublicacion.typeOfPublication == JOURNAL_ARTICLE)
                {
                    document.IdRoh_scientificActivityDocument = "http://gnoss.com/items/scientificactivitydocument_SAD1";
                    document.IdDc_type = "http://gnoss.com/items/publicationtype_020";
                }
                else if (pPublicacion.typeOfPublication == BOOK)
                {
                    document.IdRoh_scientificActivityDocument = "http://gnoss.com/items/scientificactivitydocument_SAD1";
                    document.IdDc_type = "http://gnoss.com/items/publicationtype_032";
                }
                else if (pPublicacion.typeOfPublication == CHAPTER)
                {
                    document.IdRoh_scientificActivityDocument = "http://gnoss.com/items/scientificactivitydocument_SAD1";
                    document.IdDc_type = "http://gnoss.com/items/publicationtype_004";
                }
                else if (pPublicacion.typeOfPublication == CONFERENCE_PAPER)
                {
                    document.IdRoh_scientificActivityDocument = "http://gnoss.com/items/scientificactivitydocument_SAD2";
                }
                else
                {
                    document.IdRoh_scientificActivityDocument = "http://gnoss.com/items/scientificactivitydocument_SAD1";
                    document.IdDc_type = "http://gnoss.com/items/publicationtype_OTHERS";
                    document.Roh_typeOthers = "Otros";
                }
            }
            else if (pPublicacionB != null && pPublicacionB.typeOfPublication != null)
            {
                if (pPublicacionB.typeOfPublication == JOURNAL_ARTICLE)
                {
                    document.IdRoh_scientificActivityDocument = "http://gnoss.com/items/scientificactivitydocument_SAD1";
                    document.IdDc_type = "http://gnoss.com/items/publicationtype_020";
                }
                else if (pPublicacionB.typeOfPublication == BOOK)
                {
                    document.IdRoh_scientificActivityDocument = "http://gnoss.com/items/scientificactivitydocument_SAD1";
                    document.IdDc_type = "http://gnoss.com/items/publicationtype_032";
                }
                else if (pPublicacionB.typeOfPublication == CHAPTER)
                {
                    document.IdRoh_scientificActivityDocument = "http://gnoss.com/items/scientificactivitydocument_SAD1";
                    document.IdDc_type = "http://gnoss.com/items/publicationtype_004";
                }
                else if (pPublicacionB.typeOfPublication == CONFERENCE_PAPER)
                {
                    document.IdRoh_scientificActivityDocument = "http://gnoss.com/items/scientificactivitydocument_SAD2";
                }
                else
                {
                    document.IdRoh_scientificActivityDocument = "http://gnoss.com/items/scientificactivitydocument_SAD1";
                    document.IdDc_type = "http://gnoss.com/items/publicationtype_OTHERS";
                    document.Roh_typeOthers = "Otros";
                }
            }

            // Etiquetas Externas (ExternalFreeTextKeywords)
            HashSet<string> listaSinRepetirEtiquetas = new HashSet<string>();
            if (pPublicacion.freetextKeywords != null && pPublicacion.freetextKeywords.Count > 0)
            {
                HashSet<string> etiquetasExternas = new HashSet<string>();
                foreach (FreetextKeyword etiquetas in pPublicacion.freetextKeywords)
                {
                    foreach (string tag in etiquetas.freetextKeyword)
                    {
                        if (!listaSinRepetirEtiquetas.Contains(tag.ToLower()))
                        {
                            etiquetasExternas.Add(tag);
                            listaSinRepetirEtiquetas.Add(tag.ToLower());
                        }
                    }
                }
            }
            if (pPublicacionB != null && pPublicacionB.freetextKeywords != null && pPublicacionB.freetextKeywords.Count > 0)
            {
                HashSet<string> etiquetasExternas = new HashSet<string>();
                foreach (FreetextKeyword etiquetas in pPublicacionB.freetextKeywords)
                {
                    foreach (string tag in etiquetas.freetextKeyword)
                    {
                        if (!listaSinRepetirEtiquetas.Contains(tag.ToLower()))
                        {
                            etiquetasExternas.Add(tag);
                            listaSinRepetirEtiquetas.Add(tag.ToLower());
                        }
                    }
                }
            }
            document.Roh_externalKeywords = listaSinRepetirEtiquetas.ToList();

            // Etiquetas Enriquecidas (EnrichedFreeTextKeywords)
            HashSet<string> etiquetasEnriquecidas = new HashSet<string>();
            if (pPublicacion.freetextKeyword_enriquecidas != null && pPublicacion.freetextKeyword_enriquecidas.Count > 0)
            {
                foreach (FreetextKeywordEnriquecida tag in pPublicacion.freetextKeyword_enriquecidas)
                {
                    if (!listaSinRepetirEtiquetas.Contains(tag.word.ToLower()))
                    {
                        etiquetasEnriquecidas.Add(tag.word);
                        listaSinRepetirEtiquetas.Add(tag.word.ToLower());
                    }
                }
            }
            if (pPublicacionB != null && pPublicacionB.freetextKeyword_enriquecidas != null && pPublicacionB.freetextKeyword_enriquecidas.Count > 0)
            {
                foreach (FreetextKeywordEnriquecida tag in pPublicacionB.freetextKeyword_enriquecidas)
                {
                    if (!listaSinRepetirEtiquetas.Contains(tag.word.ToLower()))
                    {
                        etiquetasEnriquecidas.Add(tag.word);
                        listaSinRepetirEtiquetas.Add(tag.word.ToLower());
                    }
                }
            }
            document.Roh_enrichedKeywords = etiquetasEnriquecidas.ToList();

            // Fecha de publicación (Issued)
            if (pPublicacion.dataIssued != null && pPublicacion.dataIssued.datimeTime != null)
            {
                document.Dct_issued = pPublicacion.dataIssued.datimeTime;

                if (pPublicacionB != null && pPublicacionB.dataIssued != null && pPublicacionB.dataIssued.datimeTime != null && document.Dct_issued == null)
                {
                    document.Roh_title = pPublicacionB.title;
                }
            }

            // URL (Url)
            HashSet<string> urlSinRepetir = new HashSet<string>();
            if (pPublicacion.url != null && pPublicacion.url.Count() > 0)
            {
                foreach (string url in pPublicacion.url)
                {
                    urlSinRepetir.Add(url);
                }
            }
            if (pPublicacionB != null && pPublicacionB.url != null && pPublicacionB.url.Count() > 0)
            {
                foreach (string url in pPublicacionB.url)
                {
                    urlSinRepetir.Add(url);
                }
            }
            if (urlSinRepetir != null && urlSinRepetir.Any())
            {
                document.Vcard_url = urlSinRepetir.ToList().First(); // TODO: Mirar el tema de las diversas URLs.
            }

            // Página de inicio (PageStart)
            if (!string.IsNullOrEmpty(pPublicacion.pageStart) && Int32.TryParse(pPublicacion.pageStart, out int n1))
            {
                document.Bibo_pageStart = pPublicacion.pageStart;

                if (pPublicacionB != null && !string.IsNullOrEmpty(pPublicacionB.pageStart) && Int32.TryParse(pPublicacionB.pageStart, out int n11) && document.Bibo_pageStart == null)
                {
                    document.Bibo_pageStart = pPublicacionB.pageStart;
                }
            }

            // Página de fin (PageEnd)
            if (!string.IsNullOrEmpty(pPublicacion.pageEnd) && Int32.TryParse(pPublicacion.pageEnd, out int n3))
            {
                document.Bibo_pageEnd = pPublicacion.pageEnd;

                if (pPublicacionB != null && !string.IsNullOrEmpty(pPublicacionB.pageEnd) && Int32.TryParse(pPublicacion.pageEnd, out int n33) && document.Bibo_pageEnd == null)
                {
                    document.Bibo_pageEnd = pPublicacionB.pageEnd;
                }
            }

            // Áreas de conocimiento externas (ExternalKnowledgeArea)
            HashSet<string> listaIDs = new HashSet<string>();
            if (pPublicacion.hasKnowledgeAreas != null && pPublicacion.hasKnowledgeAreas.Count > 0)
            {
                document.Roh_externalKnowledgeArea = new List<DocumentOntology.CategoryPath>();
                foreach (HasKnowledgeArea knowledgearea in pPublicacion.hasKnowledgeAreas)
                {
                    if (knowledgearea.resource.ToLower() == "hércules")
                    {
                        foreach (KnowledgeArea area in knowledgearea.knowledgeArea)
                        {
                            if (pDicAreasNombre.ContainsKey(area.name.ToLower()))
                            {
                                DocumentOntology.CategoryPath categoria = new DocumentOntology.CategoryPath();
                                categoria.IdsRoh_categoryNode = new List<string>();
                                categoria.IdsRoh_categoryNode.Add(pDicAreasNombre[area.name.ToLower()]);
                                string idHijo = pDicAreasNombre[area.name.ToLower()];
                                string idHijoAux = idHijo;
                                if (!listaIDs.Contains(idHijo))
                                {
                                    while (!idHijo.EndsWith(".0.0.0"))
                                    {
                                        categoria.IdsRoh_categoryNode.Add(pDicAreasBroader[idHijo]);
                                        idHijo = pDicAreasBroader[idHijo];
                                    }
                                    if (categoria.IdsRoh_categoryNode.Count > 0)
                                    {
                                        document.Roh_externalKnowledgeArea.Add(categoria);
                                    }
                                }
                                listaIDs.Add(idHijoAux);
                            }
                        }
                    }
                }
            }
            if ((document.Roh_externalKnowledgeArea == null || document.Roh_externalKnowledgeArea.Count == 0) && pPublicacionB != null && pPublicacionB.hasKnowledgeAreas != null && pPublicacionB.hasKnowledgeAreas.Count > 0)
            {
                document.Roh_externalKnowledgeArea = new List<DocumentOntology.CategoryPath>();
                foreach (HasKnowledgeArea knowledgearea in pPublicacionB.hasKnowledgeAreas)
                {
                    if (knowledgearea.resource.ToLower() == "hércules")
                    {
                        foreach (KnowledgeArea area in knowledgearea.knowledgeArea)
                        {
                            if (pDicAreasNombre.ContainsKey(area.name.ToLower()))
                            {
                                DocumentOntology.CategoryPath categoria = new DocumentOntology.CategoryPath();
                                categoria.IdsRoh_categoryNode = new List<string>();
                                categoria.IdsRoh_categoryNode.Add(pDicAreasNombre[area.name.ToLower()]);
                                string idHijo = pDicAreasNombre[area.name.ToLower()];
                                string idHijoAux = idHijo;
                                if (!listaIDs.Contains(idHijo))
                                {
                                    while (!idHijo.EndsWith(".0.0.0"))
                                    {
                                        categoria.IdsRoh_categoryNode.Add(pDicAreasBroader[idHijo]);
                                        idHijo = pDicAreasBroader[idHijo];
                                    }
                                    if (categoria.IdsRoh_categoryNode.Count > 0)
                                    {
                                        document.Roh_externalKnowledgeArea.Add(categoria);
                                    }
                                }
                                listaIDs.Add(idHijoAux);
                            }
                        }
                    }
                }
            }

            // Áreas de conocimiento enriquecidas (EnrichedKnowledgeArea)
            if (pPublicacion.topics_enriquecidos != null && pPublicacion.topics_enriquecidos.Count > 0)
            {
                document.Roh_enrichedKnowledgeArea = new List<DocumentOntology.CategoryPath>();
                foreach (TopicsEnriquecido area in pPublicacion.topics_enriquecidos)
                {
                    if (pDicAreasNombre.ContainsKey(area.word.ToLower()))
                    {
                        DocumentOntology.CategoryPath categoria = new DocumentOntology.CategoryPath();
                        categoria.IdsRoh_categoryNode = new List<string>();
                        categoria.IdsRoh_categoryNode.Add(pDicAreasNombre[area.word.ToLower()]);
                        string idHijo = pDicAreasNombre[area.word.ToLower()];
                        string idHijoAux = idHijo;
                        if (!listaIDs.Contains(idHijo))
                        {
                            while (!idHijo.EndsWith(".0.0.0"))
                            {
                                categoria.IdsRoh_categoryNode.Add(pDicAreasBroader[idHijo]);
                                idHijo = pDicAreasBroader[idHijo];
                            }
                            if (categoria.IdsRoh_categoryNode.Count > 0)
                            {
                                document.Roh_enrichedKnowledgeArea.Add(categoria);
                            }
                        }
                        listaIDs.Add(idHijoAux);
                    }
                }
            }
            if ((document.Roh_enrichedKnowledgeArea == null || document.Roh_enrichedKnowledgeArea.Count == 0) && pPublicacionB != null && pPublicacionB.topics_enriquecidos != null && pPublicacionB.topics_enriquecidos.Count > 0)
            {
                document.Roh_enrichedKnowledgeArea = new List<DocumentOntology.CategoryPath>();
                foreach (TopicsEnriquecido area in pPublicacionB.topics_enriquecidos)
                {
                    if (pDicAreasNombre.ContainsKey(area.word.ToLower()))
                    {
                        DocumentOntology.CategoryPath categoria = new DocumentOntology.CategoryPath();
                        categoria.IdsRoh_categoryNode = new List<string>();
                        categoria.IdsRoh_categoryNode.Add(pDicAreasNombre[area.word.ToLower()]);
                        string idHijo = pDicAreasNombre[area.word.ToLower()];
                        string idHijoAux = idHijo;
                        if (!listaIDs.Contains(idHijo))
                        {
                            while (!idHijo.EndsWith(".0.0.0"))
                            {
                                categoria.IdsRoh_categoryNode.Add(pDicAreasBroader[idHijo]);
                                idHijo = pDicAreasBroader[idHijo];
                            }
                            if (categoria.IdsRoh_categoryNode.Count > 0)
                            {
                                document.Roh_enrichedKnowledgeArea.Add(categoria);
                            }
                        }
                        listaIDs.Add(idHijoAux);
                    }
                }
            }

            // Métricas (HasMetric)
            if (pPublicacion.hasMetric != null && pPublicacion.hasMetric.Count > 0)
            {
                HashSet<string> listaMetricas = new HashSet<string>();
                foreach (HasMetric itemMetric in pPublicacion.hasMetric)
                {
                    if (itemMetric.metricName.ToLower() == "wos")
                    {
                        document.Roh_wos = Int32.Parse(itemMetric.citationCount);
                    }
                    else
                    {
                        if (!listaMetricas.Contains(itemMetric.metricName.ToLower()))
                        {
                            PublicationMetric publicationMetric = new PublicationMetric();
                            publicationMetric.Roh_metricName = itemMetric.metricName;
                            publicationMetric.Roh_citationCount = Int32.Parse(itemMetric.citationCount);
                            document.Roh_hasMetric = new List<PublicationMetric>() { publicationMetric };
                            listaMetricas.Add(itemMetric.metricName.ToLower());
                        }
                    }
                }
            }
            if (document.Roh_wos == null || (document.Roh_hasMetric == null || document.Roh_hasMetric.Count == 0))
            {
                if (pPublicacionB != null && pPublicacionB.hasMetric != null && pPublicacionB.hasMetric.Count > 0)
                {
                    HashSet<string> listaMetricas = new HashSet<string>();
                    foreach (HasMetric itemMetric in pPublicacionB.hasMetric)
                    {
                        if (itemMetric.metricName.ToLower() == "wos")
                        {
                            document.Roh_wos = Int32.Parse(itemMetric.citationCount);
                        }
                        else
                        {
                            if (!listaMetricas.Contains(itemMetric.metricName.ToLower()))
                            {
                                PublicationMetric publicationMetric = new PublicationMetric();
                                publicationMetric.Roh_metricName = itemMetric.metricName;
                                publicationMetric.Roh_citationCount = Int32.Parse(itemMetric.citationCount);
                                document.Roh_hasMetric = new List<PublicationMetric>() { publicationMetric };
                                listaMetricas.Add(itemMetric.metricName.ToLower());
                            }
                        }
                    }
                }
            }

            // Revista (HasPublicationVenue)
            if (pPublicacion.hasPublicationVenue != null && !string.IsNullOrEmpty(pPublicacion.hasPublicationVenue.name) && (pPublicacion.hasPublicationVenue.type == "Journal" || string.IsNullOrEmpty(pPublicacion.hasPublicationVenue.type)))
            {
                // Comprobar si la revista existe o no.
                string idRevista = string.Empty;

                // Comprobar ISSN
                if (string.IsNullOrEmpty(idRevista) && pPublicacion.hasPublicationVenue.issn != null && pPublicacion.hasPublicationVenue.issn.Count > 0)
                {
                    foreach (string issn in pPublicacion.hasPublicationVenue.issn)
                    {
                        idRevista = ComprobarRevistaISSN(issn);
                        if (!string.IsNullOrEmpty(idRevista))
                        {
                            break;
                        }
                    }
                }

                // Comprobar EISSN
                if (string.IsNullOrEmpty(idRevista) && !string.IsNullOrEmpty(pPublicacion.hasPublicationVenue.eissn))
                {
                    idRevista = ComprobarRevistaEISSN(pPublicacion.hasPublicationVenue.eissn);
                }

                // Comprobar Título
                if (string.IsNullOrEmpty(idRevista) && !string.IsNullOrEmpty(pPublicacion.hasPublicationVenue.name))
                {
                    idRevista = ComprobarRevistaTitulo(pPublicacion.hasPublicationVenue.name);
                }

                // Apunte a la revista.
                if (!string.IsNullOrEmpty(idRevista))
                {
                    document.IdVivo_hasPublicationVenue = idRevista;
                }
            }
            if (string.IsNullOrEmpty(document.IdVivo_hasPublicationVenue) && pPublicacionB != null)
            {
                // Comprobar si la revista existe o no.
                string idRevista = string.Empty;

                // Comprobar ISSN
                if (string.IsNullOrEmpty(idRevista) && pPublicacionB.hasPublicationVenue.issn != null && pPublicacionB.hasPublicationVenue.issn.Count > 0)
                {
                    foreach (string issn in pPublicacionB.hasPublicationVenue.issn)
                    {
                        idRevista = ComprobarRevistaISSN(issn);
                        if (!string.IsNullOrEmpty(idRevista))
                        {
                            break;
                        }
                    }
                }

                // Comprobar EISSN
                if (string.IsNullOrEmpty(idRevista) && !string.IsNullOrEmpty(pPublicacionB.hasPublicationVenue.eissn))
                {
                    idRevista = ComprobarRevistaEISSN(pPublicacionB.hasPublicationVenue.eissn);
                }

                // Comprobar Título
                if (string.IsNullOrEmpty(idRevista) && !string.IsNullOrEmpty(pPublicacionB.hasPublicationVenue.name))
                {
                    idRevista = ComprobarRevistaTitulo(pPublicacionB.hasPublicationVenue.name);
                }

                // Apunte a la revista.
                if (!string.IsNullOrEmpty(idRevista))
                {
                    document.IdVivo_hasPublicationVenue = idRevista;
                }
            }

            // Referencias (Bibliografía)
            if (pPublicacion.bibliografia != null && pPublicacion.bibliografia.Any())
            {
                document.Roh_references = new List<Reference>();
                foreach (Bibliografia bibliografia in pPublicacion.bibliografia)
                {
                    Reference reference = new Reference();

                    if (!string.IsNullOrEmpty(bibliografia.doi))
                    {
                        reference.Bibo_doi = bibliografia.doi;
                    }

                    if (!string.IsNullOrEmpty(bibliografia.url))
                    {
                        reference.Vcard_url = bibliografia.url;
                    }

                    if (bibliografia.anyoPublicacion.HasValue)
                    {
                        int anyo = bibliografia.anyoPublicacion.Value;
                        reference.Dct_issued = new DateTime(anyo, 1, 1);
                    }

                    if (!string.IsNullOrEmpty(bibliografia.titulo))
                    {
                        reference.Roh_title = bibliografia.titulo;
                    }

                    if (!string.IsNullOrEmpty(bibliografia.revista))
                    {
                        reference.Roh_hasPublicationVenueText = bibliografia.revista;
                    }

                    if (bibliografia.autores != null && bibliografia.autores.Any())
                    {
                        reference.Roh_authorList = new List<ReferenceAuthor>();

                        foreach (KeyValuePair<string, string> item in bibliografia.autores)
                        {
                            ReferenceAuthor autorRef = new ReferenceAuthor();

                            if (!string.IsNullOrEmpty(item.Key))
                            {
                                autorRef.Foaf_name = item.Key;
                            }

                            if (!string.IsNullOrEmpty(item.Value))
                            {
                                autorRef.Roh_semanticScholarId = item.Value;
                            }

                            if (!string.IsNullOrEmpty(autorRef.Foaf_name))
                            {
                                reference.Roh_authorList.Add(autorRef);
                            }
                        }
                    }

                    if (!string.IsNullOrEmpty(reference.Roh_title))
                    {
                        document.Roh_references.Add(reference);
                    }
                }
            }
            if ((document.Roh_references == null || !document.Roh_references.Any()) && pPublicacionB != null && pPublicacionB.bibliografia != null && pPublicacionB.bibliografia.Any())
            {
                document.Roh_references = new List<Reference>();
                foreach (Bibliografia bibliografia in pPublicacionB.bibliografia)
                {
                    Reference reference = new Reference();

                    if (!string.IsNullOrEmpty(bibliografia.doi))
                    {
                        reference.Bibo_doi = bibliografia.doi;
                    }

                    if (!string.IsNullOrEmpty(bibliografia.url))
                    {
                        reference.Vcard_url = bibliografia.url;
                    }

                    if (bibliografia.anyoPublicacion.HasValue)
                    {
                        int anyo = bibliografia.anyoPublicacion.Value;
                        reference.Dct_issued = new DateTime(anyo, 1, 1);
                    }

                    if (!string.IsNullOrEmpty(bibliografia.titulo))
                    {
                        reference.Roh_title = bibliografia.titulo;
                    }

                    if (!string.IsNullOrEmpty(bibliografia.revista))
                    {
                        reference.Roh_hasPublicationVenueText = bibliografia.revista;
                    }

                    if (bibliografia.autores != null && bibliografia.autores.Any())
                    {
                        reference.Roh_authorList = new List<ReferenceAuthor>();

                        foreach (KeyValuePair<string, string> item in bibliografia.autores)
                        {
                            ReferenceAuthor autorRef = new ReferenceAuthor();

                            if (!string.IsNullOrEmpty(item.Key))
                            {
                                autorRef.Foaf_name = item.Key;
                            }

                            if (!string.IsNullOrEmpty(item.Value))
                            {
                                autorRef.Roh_semanticScholarId = item.Value;
                            }

                            if (!string.IsNullOrEmpty(autorRef.Foaf_name))
                            {
                                reference.Roh_authorList.Add(autorRef);
                            }
                        }
                    }

                    if (!string.IsNullOrEmpty(reference.Roh_title))
                    {
                        document.Roh_references.Add(reference);
                    }
                }
            }

            // Autores
            if (pPublicacion.seqOfAuthors != null && pPublicacion.seqOfAuthors.Any())
            {
                document.Bibo_authorList = new List<BFO_0000023>();
                int orden = 1;
                foreach (PersonaPub personaPub in pPublicacion.seqOfAuthors)
                {
                    BFO_0000023 bfo_0000023 = new BFO_0000023();
                    if (!string.IsNullOrEmpty(personaPub.nick))
                    {
                        bfo_0000023.Foaf_nick = personaPub.nick;
                    }
                    else
                    {
                        bfo_0000023.Foaf_nick = personaPub.name.nombre_completo.FirstOrDefault();
                    }
                    bfo_0000023.Rdf_comment = orden;
                    bfo_0000023.IdRdf_member = personaPub.ID;
                    document.Bibo_authorList.Add(bfo_0000023);
                    orden++;
                }
            }

            return document;
        }

        /// <summary>
        /// Permite cargar los recursos.
        /// </summary>
        /// <param name="pListaRecursosCargar">Lista de recursos a cargar.</param>
        private static void CargarDatos(List<ComplexOntologyResource> pListaRecursosCargar)
        {
            //Carga.
            Parallel.ForEach(pListaRecursosCargar, new ParallelOptions { MaxDegreeOfParallelism = NUM_HILOS }, recursoCargar =>
            {
                int numIntentos = 0;
                while (!recursoCargar.Uploaded)
                {
                    numIntentos++;

                    if (numIntentos > MAX_INTENTOS)
                    {
                        break;
                    }
                    if (pListaRecursosCargar.Last() == recursoCargar)
                    {
                        mResourceApi.LoadComplexSemanticResource(recursoCargar, false, true);
                    }
                    else
                    {
                        mResourceApi.LoadComplexSemanticResource(recursoCargar);
                    }
                }
            });
        }




        /// <summary>
        /// Consulta en SPARQL si existe el documento con el DOI.
        /// </summary>
        /// <param name="pId">ID del recurso.</param>
        /// <returns>ID del recurso.</returns>
        public static string ObtenerDoiPublicacion(string pId)
        {
            // Consulta sparql.
            string select = "SELECT ?doi";
            string where = $@"WHERE {{
                                <{pId}> <http://purl.org/ontology/bibo/doi> ?doi
                            }}";

            SparqlObject resultadoQuery = mResourceApi.VirtuosoQuery(select, where, "document");
            if (resultadoQuery != null && resultadoQuery.results != null && resultadoQuery.results.bindings != null && resultadoQuery.results.bindings.Count > 0)
            {
                foreach (Dictionary<string, SparqlObject.Data> fila in resultadoQuery.results.bindings)
                {
                    return fila["doi"].value;
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// Consulta en SPARQL si existe el documento con el crisidentifier.
        /// </summary>
        /// <param name="pId">ID del recurso.</param>
        /// <returns>ID del recurso.</returns>
        public static string ObtenerCrisIdentifierPublicacion(string pId)
        {
            // Consulta sparql.
            string select = "SELECT ?crisIdentifier";
            string where = $@"WHERE {{
                                <{pId}> <http://w3id.org/roh/crisIdentifier> ?crisIdentifier
                            }}";

            SparqlObject resultadoQuery = mResourceApi.VirtuosoQuery(select, where, "document");
            if (resultadoQuery != null && resultadoQuery.results != null && resultadoQuery.results.bindings != null && resultadoQuery.results.bindings.Count > 0)
            {
                foreach (Dictionary<string, SparqlObject.Data> fila in resultadoQuery.results.bindings)
                {
                    return fila["crisIdentifier"].value;
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// Consulta en SPARQL si existe el documento con roh:isPublic.
        /// </summary>
        /// <param name="pId">ID del recurso.</param>
        /// <returns>ID del recurso.</returns>
        public static bool ObtenerIsPublicPublicacionResearchObject(string pId)
        {
            // Consulta sparql.
            string select = "SELECT ?isPublic FROM <http://gnoss.com/researchobject.owl>";
            string where = $@"WHERE {{
                                <{pId}> <http://w3id.org/roh/isPublic> ?isPublic
                            }}";

            SparqlObject resultadoQuery = mResourceApi.VirtuosoQuery(select, where, "document");
            if (resultadoQuery != null && resultadoQuery.results != null && resultadoQuery.results.bindings != null && resultadoQuery.results.bindings.Count > 0)
            {
                foreach (Dictionary<string, SparqlObject.Data> fila in resultadoQuery.results.bindings)
                {
                    return fila["isPublic"].value.ToLower() == "true";
                }
            }

            return false;
        }

        /// <summary>
        /// Consulta en SPARQL si existe el documento con las userkeywords.
        /// </summary>
        /// <param name="pId">ID del recurso.</param>
        /// <returns>ID del recurso.</returns>
        public static List<string> ObtenerUserKeywordsPublicacionResearchObject(string pId)
        {
            List<string> listaEtiquetas = new List<string>();

            // Consulta sparql.
            string select = "SELECT ?userKeywords FROM <http://gnoss.com/researchobject.owl>";
            string where = $@"WHERE {{
                                <{pId}> <http://w3id.org/roh/userKeywords> ?userKeywords
                            }}";

            SparqlObject resultadoQuery = mResourceApi.VirtuosoQuery(select, where, "document");
            if (resultadoQuery != null && resultadoQuery.results != null && resultadoQuery.results.bindings != null && resultadoQuery.results.bindings.Count > 0)
            {
                foreach (Dictionary<string, SparqlObject.Data> fila in resultadoQuery.results.bindings)
                {
                    listaEtiquetas.Add(fila["userKeywords"].value);
                }
            }

            return listaEtiquetas;
        }

        /// <summary>
        /// Consulta en SPARQL si existe el documento con las suggestedkeywords.
        /// </summary>
        /// <param name="pId">ID del recurso.</param>
        /// <returns>ID del recurso.</returns>
        public static List<string> ObtenerSuggestedKeywordsPublicacionResearchObject(string pId)
        {
            List<string> listaEtiquetas = new List<string>();

            // Consulta sparql.
            string select = "SELECT ?suggestedKeywords FROM <http://gnoss.com/researchobject.owl>";
            string where = $@"WHERE {{
                                <{pId}> <http://w3id.org/roh/suggestedKeywords> ?suggestedKeywords
                            }}";

            SparqlObject resultadoQuery = mResourceApi.VirtuosoQuery(select, where, "document");
            if (resultadoQuery != null && resultadoQuery.results != null && resultadoQuery.results.bindings != null && resultadoQuery.results.bindings.Count > 0)
            {
                foreach (Dictionary<string, SparqlObject.Data> fila in resultadoQuery.results.bindings)
                {
                    listaEtiquetas.Add(fila["suggestedKeywords"].value);
                }
            }

            return listaEtiquetas;
        }

        /// <summary>
        /// Consulta en SPARQL si existe el documento con las userknowledgearea.
        /// </summary>
        /// <param name="pId">ID del recurso.</param>
        /// <returns></returns>
        public static Dictionary<string, List<string>> ObtenerUserKnowledgeAreaPublicacionREsearchObject(string pId)
        {
            Dictionary<string, List<string>> listaCategorias = new Dictionary<string, List<string>>();

            string select = "SELECT * FROM <http://gnoss.com/researchobject.owl>";
            string where = $@"WHERE {{
                                <{pId}> <http://w3id.org/roh/userKnowledgeArea> ?userAreas.
                                ?userAreas <http://w3id.org/roh/categoryNode> ?nodo.
                            }}";
            SparqlObject resultado = mResourceApi.VirtuosoQuery(select, where, "document");


            foreach (Dictionary<string, SparqlObject.Data> fila in resultado.results.bindings)
            {
                string userAreas = fila["userAreas"].value;
                string nodo = fila["nodo"].value;
                if (!listaCategorias.ContainsKey(userAreas))
                {
                    listaCategorias.Add(userAreas, new List<string>());
                }
                listaCategorias[userAreas].Add(nodo);
            }

            return listaCategorias;
        }

        /// <summary>
        /// Consulta en SPARQL si existe el documento con las suggestedknowledgearea.
        /// </summary>
        /// <param name="pId">ID del recurso.</param>
        /// <returns></returns>
        public static Dictionary<string, List<string>> ObtenerSuggestedKnowledgeAreaPublicacionResearchObject(string pId)
        {
            Dictionary<string, List<string>> listaCategorias = new Dictionary<string, List<string>>();

            string select = "SELECT * FROM <http://gnoss.com/researchobject.owl>";
            string where = $@"WHERE {{
                                <{pId}> <http://w3id.org/roh/suggestedKnowledgeArea> ?userAreas.
                                ?userAreas <http://w3id.org/roh/categoryNode> ?nodo.
                            }}";
            SparqlObject resultado = mResourceApi.VirtuosoQuery(select, where, "document");

            foreach (Dictionary<string, SparqlObject.Data> fila in resultado.results.bindings)
            {
                string userAreas = fila["userAreas"].value;
                string nodo = fila["nodo"].value;
                if (!listaCategorias.ContainsKey(userAreas))
                {
                    listaCategorias.Add(userAreas, new List<string>());
                }
                listaCategorias[userAreas].Add(nodo);
            }

            return listaCategorias;
        }

        /// <summary>
        /// Consulta en SPARQL si existe el documento con el proyecto asignado.
        /// </summary>
        /// <param name="pId">ID del recurso.</param>
        /// <returns></returns>
        public static string ObtenerProjectPublicacionResearchObject(string pId)
        {
            // Consulta sparql.
            string select = "SELECT ?project FROM <http://gnoss.com/researchobject.owl>";
            string where = $@"WHERE {{
                                <{pId}> <http://w3id.org/roh/project> ?project
                            }}";

            SparqlObject resultadoQuery = mResourceApi.VirtuosoQuery(select, where, "document");
            if (resultadoQuery != null && resultadoQuery.results != null && resultadoQuery.results.bindings != null && resultadoQuery.results.bindings.Count > 0)
            {
                foreach (Dictionary<string, SparqlObject.Data> fila in resultadoQuery.results.bindings)
                {
                    return fila["project"].value;
                }
            }

            return string.Empty;
        }


        /// <summary>
        /// Consulta en SPARQL si existe el documento con el estado.
        /// </summary>
        /// <param name="pId">ID del recurso.</param>
        /// <returns></returns>
        public static string ObtenerAssessmentStatusPublicacionResearchObject(string pId)
        {
            // Consulta sparql.
            string select = "SELECT ?estado FROM <http://gnoss.com/researchobject.owl>";
            string where = $@"WHERE {{
                                <{pId}> <http://w3id.org/roh/assessmentStatus> ?estado
                            }}";

            SparqlObject resultadoQuery = mResourceApi.VirtuosoQuery(select, where, "document");
            if (resultadoQuery != null && resultadoQuery.results != null && resultadoQuery.results.bindings != null && resultadoQuery.results.bindings.Count > 0)
            {
                foreach (Dictionary<string, SparqlObject.Data> fila in resultadoQuery.results.bindings)
                {
                    return fila["estado"].value;
                }
            }

            return string.Empty;
        }

        private static Dictionary<string, string> ISSN_Revista = new Dictionary<string, string>();

        /// <summary>
        /// Comprueba en SPARQL si existe la revista con el ISSN.
        /// </summary>
        /// <param name="pISSN">ISSN a consultar.</param>
        /// <returns>ID del recurso.</returns>
        public static string ComprobarRevistaISSN(string pISSN)
        {
            if (ISSN_Revista.Count == 0)
            {
                int limit = 10000;
                int offset = 0;
                bool salirBucle = false;

                // Consulta sparql.
                do
                {
                    string select = "SELECT * WHERE { SELECT ?revista ?issn ";
                    string where = $@"WHERE {{
                                ?revista a <http://w3id.org/roh/MainDocument>. 
                                ?revista <http://purl.org/ontology/bibo/issn> ?issn. 
                            }} ORDER BY DESC(?revista) }} LIMIT {limit} OFFSET {offset}";

                    SparqlObject resultadoQuery = mResourceApi.VirtuosoQuery(select, where, "maindocument");
                    if (resultadoQuery != null && resultadoQuery.results != null && resultadoQuery.results.bindings != null && resultadoQuery.results.bindings.Count > 0)
                    {
                        offset += limit;
                        foreach (Dictionary<string, SparqlObject.Data> fila in resultadoQuery.results.bindings)
                        {
                            ISSN_Revista[fila["issn"].value.ToLower()] = fila["revista"].value;
                        }
                        if (resultadoQuery.results.bindings.Count < limit)
                        {
                            salirBucle = true;
                        }
                    }
                    else
                    {
                        salirBucle = true;
                    }
                } while (!salirBucle);
            }
            if (ISSN_Revista.ContainsKey(pISSN.Trim().ToLower()))
            {
                return ISSN_Revista[pISSN.Trim().ToLower()];
            }
            return string.Empty;
        }

        /// <summary>
        /// Comprueba en SPARQL si existe la revista con el ISBN.
        /// </summary>
        /// <param name="pISBN">ISBN a consultar.</param>
        /// <returns>ID del recurso.</returns>
        public static string ComprobarRevistaISBN(string pISBN)
        {
            // Consulta sparql.
            string select = "SELECT ?revista ";
            string where = $@"WHERE {{
                                ?revista a <http://w3id.org/roh/MainDocument>. 
                                ?revista <http://purl.org/ontology/bibo/isbn> ?isbn. 
                                FILTER(?isbn = '{pISBN}')
                            }}";

            SparqlObject resultadoQuery = mResourceApi.VirtuosoQuery(select, where, "maindocument");
            if (resultadoQuery != null && resultadoQuery.results != null && resultadoQuery.results.bindings != null && resultadoQuery.results.bindings.Count > 0)
            {
                foreach (Dictionary<string, SparqlObject.Data> fila in resultadoQuery.results.bindings)
                {
                    return fila["revista"].value;
                }
            }

            return string.Empty;
        }


        private static Dictionary<string, string> EISSN_Revista = new Dictionary<string, string>();

        /// <summary>
        /// Comprueba en SPARQL si existe la revista con el ISSN.
        /// </summary>
        /// <param name="pEissn">ISSN a consultar.</param>
        /// <returns>ID del recurso.</returns>
        public static string ComprobarRevistaEISSN(string pEissn)
        {
            if (EISSN_Revista.Count == 0)
            {
                int limit = 10000;
                int offset = 0;
                bool salirBucle = false;

                // Consulta sparql.
                do
                {
                    string select = "SELECT * WHERE { SELECT ?revista ?eissn ";
                    string where = $@"WHERE {{
                                ?revista a <http://w3id.org/roh/MainDocument>. 
                                ?revista <http://purl.org/ontology/bibo/eissn> ?eissn. 
                            }} ORDER BY DESC(?revista) }} LIMIT {limit} OFFSET {offset}";

                    SparqlObject resultadoQuery = mResourceApi.VirtuosoQuery(select, where, "maindocument");
                    if (resultadoQuery != null && resultadoQuery.results != null && resultadoQuery.results.bindings != null && resultadoQuery.results.bindings.Count > 0)
                    {
                        offset += limit;
                        foreach (Dictionary<string, SparqlObject.Data> fila in resultadoQuery.results.bindings)
                        {
                            EISSN_Revista[fila["eissn"].value.ToLower()] = fila["revista"].value;
                        }
                        if (resultadoQuery.results.bindings.Count < limit)
                        {
                            salirBucle = true;
                        }
                    }
                    else
                    {
                        salirBucle = true;
                    }
                } while (!salirBucle);
            }
            if (EISSN_Revista.ContainsKey(pEissn.Trim().ToLower()))
            {
                return EISSN_Revista[pEissn.Trim().ToLower()];
            }
            return string.Empty;
        }

        private static Dictionary<string, string> Titulo_Revista = new Dictionary<string, string>();

        /// <summary>
        /// Comprueba en SPARQL si existe la revista con el título.
        /// </summary>
        /// <param name="pTitulo">Título a consultar.</param>
        /// <returns>ID del recurso.</returns>
        public static string ComprobarRevistaTitulo(string pTitulo)
        {
            if (Titulo_Revista.Count == 0)
            {
                int limit = 10000;
                int offset = 0;
                bool salirBucle = false;

                // Consulta sparql.
                do
                {
                    string select = "SELECT * WHERE { SELECT ?revista ?titulo ";
                    string where = $@"WHERE {{
                                ?revista a <http://w3id.org/roh/MainDocument>. 
                                ?revista <http://w3id.org/roh/title> ?titulo. 
                            }} ORDER BY DESC(?revista) }} LIMIT {limit} OFFSET {offset}";

                    SparqlObject resultadoQuery = mResourceApi.VirtuosoQuery(select, where, "maindocument");
                    if (resultadoQuery != null && resultadoQuery.results != null && resultadoQuery.results.bindings != null && resultadoQuery.results.bindings.Count > 0)
                    {
                        offset += limit;
                        foreach (Dictionary<string, SparqlObject.Data> fila in resultadoQuery.results.bindings)
                        {
                            Titulo_Revista[fila["titulo"].value.ToLower()] = fila["revista"].value;
                        }
                        if (resultadoQuery.results.bindings.Count < limit)
                        {
                            salirBucle = true;
                        }
                    }
                    else
                    {
                        salirBucle = true;
                    }
                } while (!salirBucle);
            }
            if (Titulo_Revista.ContainsKey(pTitulo.Trim().ToLower()))
            {
                return Titulo_Revista[pTitulo.Trim().ToLower()];
            }
            return string.Empty;
        }

        /// <summary>
        /// Consulta en SPARQL si existe una publicación.
        /// </summary>
        /// <param name="pDOI"></param>
        /// <returns>ID del recurso.</returns>
        public static string ComprobarPublicacionDoi(string pDOI)
        {
            if (string.IsNullOrEmpty(pDOI))
            {
                return string.Empty;
            }

            // Consulta sparql.
            string select = "SELECT ?documento ";
            string where = $@"WHERE {{
                                ?documento a <http://purl.org/ontology/bibo/Document>. 
                                ?documento <http://purl.org/ontology/bibo/doi> ?doi. 
                                FILTER(?doi = '{pDOI}')
                            }}";

            SparqlObject resultadoQuery = mResourceApi.VirtuosoQuery(select, where, "document");
            if (resultadoQuery != null && resultadoQuery.results != null && resultadoQuery.results.bindings != null && resultadoQuery.results.bindings.Count > 0)
            {
                foreach (Dictionary<string, SparqlObject.Data> fila in resultadoQuery.results.bindings)
                {
                    return fila["documento"].value;
                }
            }

            return string.Empty;
        }

        public static string ComprobarPublicacionesIdFuente(string pId, string pNombreFuente)
        {
            if (string.IsNullOrEmpty(pId))
            {
                return string.Empty;
            }

            // Consulta sparql.
            string select = "SELECT ?documento ";
            string where = $@"WHERE {{
                                ?documento a <http://purl.org/ontology/bibo/Document>. 
                                ?documento <http://purl.org/ontology/bibo/identifier> ?identificador.
                                ?identificador <http://purl.org/dc/elements/1.1/title> ?id.
                                ?identificador <http://xmlns.com/foaf/0.1/topic> ?nombreFuente.
                                FILTER(?id = '{pId}')
                                FILTER(?nombreFuente = '{pNombreFuente}')
                            }}";

            SparqlObject resultadoQuery = mResourceApi.VirtuosoQuery(select, where, "document");
            if (resultadoQuery != null && resultadoQuery.results != null && resultadoQuery.results.bindings != null && resultadoQuery.results.bindings.Count > 0)
            {
                foreach (Dictionary<string, SparqlObject.Data> fila in resultadoQuery.results.bindings)
                {
                    return fila["documento"].value;
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// Consulta en SPARQL si existe un documento.
        /// </summary>
        /// <param name="pTitulo"></param>
        /// <returns>ID del recurso.</returns>
        public static string ComprobarPublicacionTitulo(string pTitulo)
        {
            // Consulta sparql.
            string select = "SELECT ?documento ";
            string where = $@"WHERE {{
                                ?documento a <http://purl.org/ontology/bibo/Document>. 
                                ?documento  <http://w3id.org/roh/title> ?titulo. 
                                FILTER(?titulo = '{pTitulo.Replace("'", "\\'")}')
                            }}";

            try
            {
                SparqlObject resultadoQuery = mResourceApi.VirtuosoQuery(select, where, "document");
                if (resultadoQuery != null && resultadoQuery.results != null && resultadoQuery.results.bindings != null && resultadoQuery.results.bindings.Count > 0)
                {
                    foreach (Dictionary<string, SparqlObject.Data> fila in resultadoQuery.results.bindings)
                    {
                        return fila["documento"].value;
                    }
                }
            }
            catch (Exception e)
            {
                return string.Empty;
            }
            return string.Empty;
        }

        /// <summary>
        /// Consulta en SPARQL si existe la persona.
        /// </summary>
        /// <param name="pORCID">ID ORCID.</param>
        /// <returns></returns>
        public static string ComprobarPersonaOrcid(string pORCID)
        {
            // Consulta sparql.
            string select = "SELECT ?person";
            string where = $@"WHERE {{
                                ?person a <http://xmlns.com/foaf/0.1/Person>. 
                                ?person <http://w3id.org/roh/ORCID> ?orcid. 
                                FILTER(?orcid = '{pORCID}')
                            }}";

            SparqlObject resultadoQuery = mResourceApi.VirtuosoQuery(select, where, "person");
            if (resultadoQuery != null && resultadoQuery.results != null && resultadoQuery.results.bindings != null && resultadoQuery.results.bindings.Count > 0)
            {
                foreach (Dictionary<string, SparqlObject.Data> fila in resultadoQuery.results.bindings)
                {
                    return fila["person"].value;
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// Consulta en SPARQL si existe la persona.
        /// </summary>
        /// <param name="pSemanticScholarId">ID Semantic Scholar.</param>
        /// <returns></returns>
        public static string ComprobarPersonaSemanticScholarId(string pSemanticScholarId)
        {
            // Consulta sparql.
            string select = "SELECT ?person";
            string where = $@"WHERE {{
                                ?person a <http://xmlns.com/foaf/0.1/Person>. 
                                ?person <http://w3id.org/roh/semanticScholarId> ?semanticScholarId. 
                                FILTER(?semanticScholarId = '{pSemanticScholarId}')
                            }}";

            SparqlObject resultadoQuery = mResourceApi.VirtuosoQuery(select, where, "person");
            if (resultadoQuery != null && resultadoQuery.results != null && resultadoQuery.results.bindings != null && resultadoQuery.results.bindings.Count > 0)
            {
                foreach (Dictionary<string, SparqlObject.Data> fila in resultadoQuery.results.bindings)
                {
                    return fila["person"].value;
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// Consulta en SPARQL si existe la persona.
        /// </summary>
        /// <param name="pNombre"></param>
        /// <returns></returns>
        public static string ComprobarPersonaNombre(string pNombre)
        {
            // Consulta sparql.
            string select = "SELECT ?person";
            string where = $@"WHERE {{
                                ?person a <http://xmlns.com/foaf/0.1/Person>. 
                                ?person <http://xmlns.com/foaf/0.1/name> ?nombre. 
                                FILTER(?nombre = '{pNombre.Replace("'", "\\'")}')
                            }}";

            SparqlObject resultadoQuery = mResourceApi.VirtuosoQuery(select, where, "person");
            if (resultadoQuery != null && resultadoQuery.results != null && resultadoQuery.results.bindings != null && resultadoQuery.results.bindings.Count > 0)
            {
                foreach (Dictionary<string, SparqlObject.Data> fila in resultadoQuery.results.bindings)
                {
                    return fila["person"].value;
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// Consulta en SPARQL si hay alguna persona con el usuario de git.
        /// TODO: Revisar si hay más de una persona con el mismo ID.
        /// </summary>
        /// <param name="pNombre"></param>
        /// <returns></returns>
        public static string ComprobarPersonaUsuarioGitHub(string pNombre)
        {
            // Consulta sparql.
            string select = "SELECT ?person";
            string where = $@"WHERE {{
                                ?person a <http://xmlns.com/foaf/0.1/Person>. 
                                ?person <http://w3id.org/roh/usuarioGitHub> ?nombre. 
                                FILTER(?nombre = '{pNombre}')
                            }}";

            SparqlObject resultadoQuery = mResourceApi.VirtuosoQuery(select, where, "person");
            if (resultadoQuery != null && resultadoQuery.results != null && resultadoQuery.results.bindings != null && resultadoQuery.results.bindings.Count > 0)
            {
                foreach (Dictionary<string, SparqlObject.Data> fila in resultadoQuery.results.bindings)
                {
                    return fila["person"].value;
                }
            }

            return string.Empty;
        }


        /// <summary>
        /// Contruye el objeto persona a cargar.
        /// </summary>
        /// <param name="pNombreCompleto">Nombre completo (nombre + apellidos)</param>
        /// <param name="pNombre">Nombre de la persona.</param>
        /// <param name="pApellidos">Apellidos de la persona.</param>
        /// <returns>Objeto persona con los datos.</returns>
        public static Person ConstruirPersona(PersonaPub pPersona = null, PersonRO pPersonaRO = null)
        {
            if (pPersona != null)
            {
                Person person = new Person();

                if (pPersona.name != null && pPersona.name.given != null && pPersona.name.given.Any() && pPersona.name.given[0] != null && pPersona.name.familia != null && pPersona.name.familia.Any() && pPersona.name.familia[0] != null)
                {
                    person.Foaf_name = pPersona.name.given[0].Trim() + " " + pPersona.name.familia[0].Trim();
                    person.Foaf_firstName = pPersona.name.given[0].Trim();
                    person.Foaf_lastName = pPersona.name.familia[0].Trim();
                }
                else if (pPersona.name != null && pPersona.name.nombre_completo != null && pPersona.name.nombre_completo.Any())
                {
                    person.Foaf_name = pPersona.name.nombre_completo[0].Trim();
                    if (person.Foaf_name.Contains(","))
                    {
                        person.Foaf_firstName = pPersona.name.nombre_completo[0].Split(',')[1].Trim();
                        person.Foaf_lastName = pPersona.name.nombre_completo[0].Split(',')[0].Trim();
                    }
                    else if (person.Foaf_name.Contains(" "))
                    {
                        person.Foaf_firstName = pPersona.name.nombre_completo[0].Trim().Split(' ')[0].Trim();
                        person.Foaf_lastName = pPersona.name.nombre_completo[0].Trim().Substring(pPersona.name.nombre_completo[0].Trim().IndexOf(' ')).Trim();
                    }
                    else
                    {
                        person.Foaf_firstName = pPersona.name.nombre_completo[0].Trim();
                    }
                }
                else
                {
                    if (pPersona.nick.Contains(","))
                    {
                        person.Foaf_firstName = pPersona.nick.Split(',')[1].Trim();
                        person.Foaf_lastName = pPersona.nick.Split(',')[0].Trim();
                    }
                    else if (pPersona.nick.Contains(" "))
                    {
                        person.Foaf_firstName = pPersona.nick.Trim().Split(' ')[1].Trim();
                        person.Foaf_lastName = pPersona.nick.Trim().Split(' ')[0].Trim();
                    }
                    else
                    {
                        person.Foaf_firstName = pPersona.nick.Trim();
                    }
                    person.Foaf_name = person.Foaf_firstName + " " + person.Foaf_lastName;
                }

                if (!string.IsNullOrEmpty(pPersona.orcid))
                {
                    person.Roh_ORCID = pPersona.orcid;
                }

                if (!string.IsNullOrEmpty(pPersona.researcherID))
                {
                    person.Vivo_researcherId = pPersona.researcherID;
                }

                if (pPersona.iDs != null && pPersona.iDs.Any())
                {
                    foreach (string item in pPersona.iDs)
                    {
                        if (item.ToLower().Contains("semanticscholar"))
                        {
                            person.Roh_semanticScholarId = item.Split(":")[1].Trim();
                        }
                    }
                }

                return person;
            }

            if (pPersonaRO != null)
            {
                Person person = new Person();

                if (!string.IsNullOrEmpty(pPersonaRO.orcid))
                {
                    person.Roh_ORCID = pPersonaRO.orcid;
                }

                if (!string.IsNullOrEmpty(pPersonaRO.nombreCompleto) && pPersonaRO.nombreCompleto.Contains(" "))
                {
                    // TODO
                    person.Foaf_firstName = pPersonaRO.nombreCompleto.Split(" ")[0].Trim();
                    person.Foaf_lastName = pPersonaRO.nombreCompleto.Substring(pPersonaRO.nombreCompleto.IndexOf(" ")).Trim();
                    person.Foaf_name = pPersonaRO.nombreCompleto;
                }
                else if (!string.IsNullOrEmpty(pPersonaRO.nombreCompleto))
                {
                    person.Foaf_name = pPersonaRO.nombreCompleto;
                }

                return person;
            }

            return null;
        }

        /// <summary>
        /// Limpia el ORCID de la persona.
        /// </summary>
        /// <param name="pORCID">Código ORCID a limpiar.</param>
        /// <returns>ORCID limpio.</returns>
        public static string LimpiarORCID(string pORCID)
        {
            try
            {
                Uri uri = new Uri(pORCID);
                return uri.AbsolutePath.Substring(1);
            }
            catch (Exception e)
            {

            }

            return pORCID;
        }

        /// <summary>
        /// Permite crear un archivo Zip de un único fichero.
        /// </summary>
        /// <param name="pRutaEscritura">Ruta dónde se va a guardar el archivo zip.</param>
        /// <param name="pNombreFichero">Nombre del fichero.</param>
        /// <param name="pData">Datos a guardar.</param>
        private static void CrearZip(string pRutaEscritura, string pNombreFichero, string pData)
        {
            using (FileStream zipToOpen = new FileStream($@"{pRutaEscritura}\{pNombreFichero.Split('.')[0]}.zip", FileMode.Create))
            {
                using (ZipArchive archive = new ZipArchive(zipToOpen, ZipArchiveMode.Update))
                {
                    ZipArchiveEntry readmeEntry = archive.CreateEntry(pNombreFichero);
                    using (StreamWriter writer = new StreamWriter(readmeEntry.Open()))
                    {
                        writer.Write(pData);
                    }
                }
            }
        }

        /// <summary>
        /// Obtiene los datos del tesauro.
        /// </summary>
        /// <returns>Tupla con los dos diccionarios.</returns>
        private static Tuple<Dictionary<string, string>, Dictionary<string, string>> ObtenerDatosTesauro()
        {
            Dictionary<string, string> dicAreasBroader = new Dictionary<string, string>();
            Dictionary<string, string> dicAreasNombre = new Dictionary<string, string>();

            string select = @"SELECT DISTINCT * ";
            string where = @$"WHERE {{
                ?concept a <http://www.w3.org/2008/05/skos#Concept>.
                ?concept <http://www.w3.org/2008/05/skos#prefLabel> ?nombre.
                ?concept <http://purl.org/dc/elements/1.1/source> 'researcharea'
                OPTIONAL{{?concept <http://www.w3.org/2008/05/skos#broader> ?broader}}
                }}";
            SparqlObject resultado = mResourceApi.VirtuosoQuery(select, where, "taxonomy");

            foreach (Dictionary<string, SparqlObject.Data> fila in resultado.results.bindings)
            {
                string concept = fila["concept"].value;
                string nombre = fila["nombre"].value;
                string broader = "";
                if (fila.ContainsKey("broader"))
                {
                    broader = fila["broader"].value;
                }
                dicAreasBroader.Add(concept, broader);
                if (!dicAreasNombre.ContainsKey(nombre.ToLower()))
                {
                    dicAreasNombre.Add(nombre.ToLower(), concept);
                }
            }

            return new Tuple<Dictionary<string, string>, Dictionary<string, string>>(dicAreasBroader, dicAreasNombre);
        }

        /// <summary>
        /// Obtiene el ID indicado de una publicación dada.
        /// </summary>
        /// <param name="pPublicacion">Publicación a obtener el ID.</param>
        /// <param name="pId">ID a obtener.</param>
        /// <returns>ID obtenido.</returns>
        private static string GetPublicationId(Publication pPublicacion, string pId)
        {
            if (pPublicacion.iDs != null && pPublicacion.iDs.Any())
            {
                foreach (string id in pPublicacion.iDs)
                {
                    if (id.ToLower().Contains(pId))
                    {
                        return id.Split(":")[1].Trim();
                    }
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// Obtiene el objeto para desambiguar publicaciones.
        /// </summary>
        /// <param name="pPublicacion">Publicación a convertir.</param>
        /// <returns>Objeto para desambiguar.</returns>
        private static DisambiguationPublication GetDisambiguationPublication(Publication pPublicacion)
        {
            pPublicacion.ID = Guid.NewGuid().ToString();

            DisambiguationPublication pub = new DisambiguationPublication()
            {
                ID = pPublicacion.ID,
                doi = pPublicacion.doi,
                title = pPublicacion.title
            };

            return pub;
        }

        private static DisambiguationRO GetDisambiguationRO(ResearchObjectFigShare pResearchObject)
        {
            pResearchObject.ID = Guid.NewGuid().ToString();

            DisambiguationRO ro = new DisambiguationRO()
            {
                ID = pResearchObject.ID,
                doi = pResearchObject.doi,
                title = pResearchObject.titulo,
                idFigshare = pResearchObject.id.ToString()
            };

            return ro;
        }

        private static DisambiguationRO GetDisambiguationRoGithub(ResearchObjectGitHub pGithubObj)
        {
            pGithubObj.ID = Guid.NewGuid().ToString();

            DisambiguationRO ro = new DisambiguationRO()
            {
                ID = pGithubObj.ID,
                title = pGithubObj.titulo,
                idGithub = pGithubObj.id.ToString()
            };

            return ro;
        }

        /// <summary>
        /// Obtiene el ID indicado de una persona dada.
        /// </summary>
        /// <param name="pPersona">Persona a obtener el ID.</param>
        /// <param name="pId">ID a obtener.</param>
        /// <returns>ID obtenido.</returns>
        private static string GetPersonId(PersonaPub pPersona, string pId)
        {
            if (pPersona.iDs != null && pPersona.iDs.Any())
            {
                foreach (string id in pPersona.iDs)
                {
                    if (id.ToLower().Contains(pId))
                    {
                        return id.Split(":")[1].Trim();
                    }
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// Obtiene el objeto para desambiguar personas.
        /// </summary>
        /// <param name="pPersona">Persona a convertir.</param>
        /// <returns>Objeto para desambiguar.</returns>
        private static DisambiguationPerson GetDisambiguationPerson(PersonaPub pPersona = null, PersonRO pPersonaRo = null, string pPersonaGit = null)
        {
            if (pPersona != null)
            {
                pPersona.ID = Guid.NewGuid().ToString();
                string nombreCompleto = string.Empty;

                if (pPersona.name != null && pPersona.name.nombre_completo != null && pPersona.name.nombre_completo.Any())
                {
                    nombreCompleto = pPersona.name.nombre_completo[0];
                }

                DisambiguationPerson persona = new DisambiguationPerson()
                {
                    ID = pPersona.ID,
                    orcid = pPersona.orcid,
                    completeName = nombreCompleto
                };

                return persona;
            }

            if (pPersonaRo != null)
            {
                pPersonaRo.ID = Guid.NewGuid().ToString();
                DisambiguationPerson persona = new DisambiguationPerson()
                {
                    ID = pPersonaRo.ID,
                    orcid = pPersonaRo.orcid,
                    figShareId = pPersonaRo.id.ToString(),
                    completeName = pPersonaRo.nombreCompleto
                };

                return persona;
            }

            if (pPersonaGit != null)
            {
                DisambiguationPerson persona = new DisambiguationPerson()
                {
                    ID = Guid.NewGuid().ToString(),
                    gitHubId = pPersonaGit
                };

                return persona;
            }

            return null;
        }

        /// <summary>
        /// Comprueba las publicaciones para que no hayan duplicados.
        /// </summary>
        /// <param name="pPublicacion">Publicación a comprobar.</param>
        /// <returns>ID del recurso cargado.</returns>
        private static string ComprobarPublicacion(Publication pPublicacion)
        {
            // ID del recurso.
            string idDocumento = string.Empty;

            // Comprueba si existe la publicación mediante el DOI.
            idDocumento = ComprobarPublicacionDoi(pPublicacion.doi);

            // Comprueba si existe la publicación mediante los otros IDs.
            if (pPublicacion.iDs != null && pPublicacion.iDs.Count > 0)
            {
                foreach (string id in pPublicacion.iDs)
                {
                    if (id.Contains(":"))
                    {
                        if (string.IsNullOrEmpty(idDocumento) && id.ToLower().Contains("wos"))
                        {
                            // Comprueba si existe la publicación mediante el WOS ID.
                            idDocumento = ComprobarPublicacionesIdFuente(id.Split(":")[1].Trim(), "WoS");
                        }

                        if (string.IsNullOrEmpty(idDocumento) && id.ToLower().Contains("semanticscholar"))
                        {
                            // Comprueba si existe la publicación mediante el SemanticScholar ID.
                            idDocumento = ComprobarPublicacionesIdFuente(id.Split(":")[1].Trim(), "SemanticScholar");
                        }

                        if (string.IsNullOrEmpty(idDocumento) && id.ToLower().Contains("mag"))
                        {
                            // Comprueba si existe la publicación mediante el MAG ID.
                            idDocumento = ComprobarPublicacionesIdFuente(id.Split(":")[1].Trim(), "MAG");
                        }

                        if (string.IsNullOrEmpty(idDocumento) && id.ToLower().Contains("pubmedcentral"))
                        {
                            // Comprueba si existe la publicación mediante el DOI.
                            idDocumento = ComprobarPublicacionesIdFuente(id.Split(":")[1].Trim(), "PubMedCentral");
                        }

                        if (string.IsNullOrEmpty(idDocumento) && id.ToLower().Contains("scopus_id"))
                        {
                            // Comprueba si existe la publicación mediante el Scopus ID.
                            idDocumento = ComprobarPublicacionesIdFuente(id.Split(":")[1].Trim(), "Scopus");
                        }

                        if (string.IsNullOrEmpty(idDocumento) && id.ToLower().Contains("arxiv"))
                        {
                            // Comprueba si existe la publicación mediante el ID de arXiv.
                            idDocumento = ComprobarPublicacionesIdFuente(id.Split(":")[1].Trim(), "ArXiv");
                        }

                        if (string.IsNullOrEmpty(idDocumento) && id.ToLower().Contains("arxiv"))
                        {
                            // Comprueba si existe la publicación mediante el ID de arXiv.
                            idDocumento = ComprobarPublicacionesIdFuente(id.Split(":")[1].Trim(), "Medline");
                        }
                    }
                }
            }

            // TODO: Comprueba si está la publicación cargada. (ASIO) (De momento, solo compruebo el titulo)
            if (string.IsNullOrEmpty(pPublicacion.title))
            {
                idDocumento = ComprobarPublicacionTitulo(pPublicacion.title);
            }

            return idDocumento;
        }

        private static string ComprobarPersona(PersonaPub pPersona)
        {
            string idPersona = string.Empty;

            // Comprobar si existe la persona en SPARQL con el ORCID.
            idPersona = ComprobarPersonaOrcid(LimpiarORCID(pPersona.orcid));

            // Comprobar si existe la persona en SPARQL con el SemanticScholar ID.
            if (string.IsNullOrEmpty(idPersona) && pPersona.iDs != null && pPersona.iDs.Count > 0)
            {
                foreach (string id in pPersona.iDs)
                {
                    if (id.Contains(":"))
                    {
                        if (id.ToLower().Contains("semanticscholar"))
                        {
                            idPersona = ComprobarPersonaSemanticScholarId(id.Split(":")[1].Trim());
                        }
                    }
                }
            }

            return idPersona;
        }

        /// <summary>
        /// Método para dividir listas
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="pItems">Listado</param>
        /// <param name="pSize">Tamaño</param>
        /// <returns></returns>
        public static IEnumerable<List<T>> SplitList<T>(List<T> pItems, int pSize)
        {
            for (int i = 0; i < pItems.Count; i += pSize)
            {
                yield return pItems.GetRange(i, Math.Min(pSize, pItems.Count - i));
            }
        }

        public static List<Tuple<string, string, string, string, string, string>> ObtenerPersonas(HashSet<string> pIdsPersonas)
        {
            List<Tuple<string, string, string, string, string, string>> listaResultados = new List<Tuple<string, string, string, string, string, string>>();

            List<List<string>> listaListas = SplitList(pIdsPersonas.ToList(), 1000).ToList();

            foreach (List<string> lista in listaListas)
            {
                int limit = 10000;
                int offset = 0;
                bool salirBucle = false;

                // Consulta sparql.
                do
                {
                    string select = "SELECT * WHERE { SELECT DISTINCT ?s ?orcid ?crisIdentifier ?nombre ?apellidos ?nombreCompleto ";
                    string where = $@"WHERE {{
                                ?s <http://xmlns.com/foaf/0.1/name> ?nombreCompleto.
                                OPTIONAL{{?s <http://w3id.org/roh/ORCID> ?orcid. }}
                                OPTIONAL{{?s <http://w3id.org/roh/crisIdentifier> ?crisIdentifier. }}
                                OPTIONAL{{?s <http://xmlns.com/foaf/0.1/firstName> ?nombre. }}
                                OPTIONAL{{?s <http://xmlns.com/foaf/0.1/lastName> ?apellidos. }}
                                FILTER(?s IN (<{string.Join(">,<", lista)}>))
                            }} ORDER BY DESC(?orcid) }} LIMIT {limit} OFFSET {offset}";

                    SparqlObject resultadoQuery = mResourceApi.VirtuosoQuery(select, where, "person");
                    if (resultadoQuery != null && resultadoQuery.results != null && resultadoQuery.results.bindings != null && resultadoQuery.results.bindings.Count > 0)
                    {
                        offset += limit;
                        foreach (Dictionary<string, SparqlObject.Data> fila in resultadoQuery.results.bindings)
                        {
                            // Comprobaciones
                            string s = fila["s"].value;
                            string orcid = string.Empty;
                            string crisIdentifier = string.Empty;
                            string nombre = string.Empty;
                            string apellidos = string.Empty;
                            string nombreCompleto = fila["nombreCompleto"].value;

                            if (fila.ContainsKey("orcid"))
                            {
                                orcid = fila["orcid"].value;
                            }
                            if (fila.ContainsKey("crisIdentifier"))
                            {
                                crisIdentifier = fila["crisIdentifier"].value;
                            }
                            if (fila.ContainsKey("nombre"))
                            {
                                nombre = fila["nombre"].value;
                            }
                            if (fila.ContainsKey("apellidos"))
                            {
                                apellidos = fila["apellidos"].value;
                            }

                            Tuple<string, string, string, string, string, string> tuplaDatos = new(
                                s,
                                orcid,
                                crisIdentifier,
                                nombre,
                                apellidos,
                                nombreCompleto
                            );
                            listaResultados.Add(tuplaDatos);
                        }
                        if (resultadoQuery.results.bindings.Count < limit)
                        {
                            salirBucle = true;
                        }
                    }
                    else
                    {
                        salirBucle = true;
                    }
                } while (!salirBucle);
            }

            return listaResultados;
        }

        public static List<Tuple<string, string>> ObtenerDocumentos(HashSet<string> pIdsDocumentos)
        {
            List<Tuple<string, string>> listaResultados = new List<Tuple<string, string>>();

            List<List<string>> listaListas = SplitList(pIdsDocumentos.ToList(), 1000).ToList();

            foreach (List<string> lista in listaListas)
            {
                int limit = 10000;
                int offset = 0;
                bool salirBucle = false;

                // Consulta sparql.
                do
                {
                    string select = "SELECT * WHERE { SELECT DISTINCT ?s ?crisIdentifier  ";
                    string where = $@"WHERE {{
                                ?s ?p ?o.
                                OPTIONAL{{?s <http://w3id.org/roh/crisIdentifier> ?crisIdentifier. }}
                                FILTER(?s IN (<{string.Join(">,<", lista)}>))
                            }} ORDER BY DESC(?s) }} LIMIT {limit} OFFSET {offset}";

                    SparqlObject resultadoQuery = mResourceApi.VirtuosoQuery(select, where, "document");
                    if (resultadoQuery != null && resultadoQuery.results != null && resultadoQuery.results.bindings != null && resultadoQuery.results.bindings.Count > 0)
                    {
                        offset += limit;
                        foreach (Dictionary<string, SparqlObject.Data> fila in resultadoQuery.results.bindings)
                        {
                            // Comprobaciones
                            string s = fila["s"].value;
                            string crisIdentifier = string.Empty;

                            if (fila.ContainsKey("crisIdentifier"))
                            {
                                crisIdentifier = fila["crisIdentifier"].value;
                            }

                            Tuple<string, string> tuplaDatos = new(
                                s,
                                crisIdentifier
                            );
                            listaResultados.Add(tuplaDatos);
                        }
                        if (resultadoQuery.results.bindings.Count < limit)
                        {
                            salirBucle = true;
                        }
                    }
                    else
                    {
                        salirBucle = true;
                    }
                } while (!salirBucle);
            }

            return listaResultados;
        }

        public static void InsertarOrcid(string pIdRecurso, List<string> pListaId, Dictionary<string, Person> pDicPersonas)
        {
            string orcid = string.Empty;

            foreach (string id in pListaId)
            {
                Person persona = pDicPersonas.FirstOrDefault(x => x.Key == id).Value;
                if (!string.IsNullOrEmpty(persona.Roh_ORCID))
                {
                    orcid = persona.Roh_ORCID;
                    break;
                }
            }

            if (!string.IsNullOrEmpty(orcid))
            {
                mResourceApi.ChangeOntoly("person");
                Guid guid = new Guid(pIdRecurso.Split("_")[1]);
                TriplesToInclude triple = new TriplesToInclude();
                triple.Predicate = "http://w3id.org/roh/ORCID";
                triple.NewValue = orcid;
                Dictionary<Guid, List<TriplesToInclude>> dic = new Dictionary<Guid, List<TriplesToInclude>>();
                dic.Add(guid, new List<TriplesToInclude>() { triple });
                mResourceApi.InsertPropertiesLoadedResources(dic); // -------------------- INSERCIÓN
            }
        }

        public static void ModificarNombres(string pIdRecurso, string pNombre, string pApellidos, string pNombreCompleto, List<string> pListaId, Dictionary<string, Person> pDicPersonas)
        {
            string nombre = pNombre;
            string apellidos = pApellidos;
            string nombreCompleto = pNombreCompleto;

            foreach (string id in pListaId)
            {
                Person persona = pDicPersonas.FirstOrDefault(x => x.Key == id).Value;
                if (!string.IsNullOrEmpty(persona.Foaf_name) && persona.Foaf_name.Count() > nombreCompleto.Count())
                {
                    nombreCompleto = persona.Foaf_name;
                    nombre = persona.Foaf_firstName;
                    apellidos = persona.Foaf_lastName;
                }
            }

            mResourceApi.ChangeOntoly("person");
            Guid guid = new Guid(pIdRecurso.Split("_")[1]);
            List<TriplesToModify> listaTriples = new List<TriplesToModify>();

            // Nombre Completo
            if (nombreCompleto != pNombreCompleto)
            {
                TriplesToModify triple = new TriplesToModify();
                triple.Predicate = "http://xmlns.com/foaf/0.1/name";
                triple.NewValue = nombreCompleto;
                triple.OldValue = pNombreCompleto;
                listaTriples.Add(triple);
            }

            // Nombre
            if (nombre != pNombre)
            {
                TriplesToModify triple = new TriplesToModify();
                triple.Predicate = "http://xmlns.com/foaf/0.1/firstName";
                triple.NewValue = nombre;
                triple.OldValue = pNombre;
                listaTriples.Add(triple);
            }

            // Apellidos
            if (apellidos != pApellidos)
            {
                TriplesToModify triple = new TriplesToModify();
                triple.Predicate = "http://xmlns.com/foaf/0.1/lastName";
                triple.NewValue = apellidos;
                triple.OldValue = pApellidos;
                listaTriples.Add(triple);
            }

            Dictionary<Guid, List<TriplesToModify>> dic = new Dictionary<Guid, List<TriplesToModify>>();
            dic.Add(guid, listaTriples);

            if (listaTriples != null && listaTriples.Any())
            {
                mResourceApi.ModifyPropertiesLoadedResources(dic); // -------------------- MODIFICACIÓN
            }
        }


        public static void ModificarRos(string pIdRecurso, HashSet<string> pListaId, Dictionary<HashSet<string>, ResearchobjectOntology.ResearchObject> pDicRos, Dictionary<string, string> pDicAreasBroader, Dictionary<string, string> pDicAreasNombre)
        {
            HashSet<string> listaAux = new HashSet<string>();
            foreach (string id in pListaId)
            {
                foreach (KeyValuePair<HashSet<string>, ResearchobjectOntology.ResearchObject> item in pDicRos)
                {
                    if (item.Key.Contains(id))
                    {
                        listaAux = item.Key;

                        mResourceApi.ChangeOntoly("researchobject");
                        Guid guid = new Guid(pIdRecurso.Split("_")[1]);
                        ComplexOntologyResource resourceRo = item.Value.ToGnossApiResource(mResourceApi, null);
                        mResourceApi.ModifyComplexOntologyResource(resourceRo, true, true); // -------------------- MODIFICACIÓN
                        break;
                    }
                }

                if (listaAux != null && listaAux.Any())
                {
                    pDicRos.Remove(pDicRos.FirstOrDefault(x => x.Key.SequenceEqual(listaAux)).Key);
                }
            }
        }

        /// <summary>
        /// Permite borrar y cargar un documento existente con datos actualizados.
        /// </summary>
        /// <param name="pPublicacion">Publicación con los datos a cargar.</param>
        /// <param name="pIdDocumento">ID del recurso.</param>
        /// <param name="pDicAreasBroader">Diccionario con los hijos.</param>
        /// <param name="pDicAreasNombre">Diccionario con las áreas temáticas.</param>        
        /// <returns></returns>
        public static void ModificarDocumento(Document pDocument, string pIdDocumento, Dictionary<string, string> pDicAreasBroader, Dictionary<string, string> pDicAreasNombre)
        {
            //TODO revisar campos

            // Recuperación del Crisidentifier
            pDocument.Roh_crisIdentifier = ObtenerCrisIdentifierPublicacion(pIdDocumento);

            // Recuperación del roh:isPpublic
            pDocument.Roh_isPublic = ObtenerIsPublicPublicacionResearchObject(pIdDocumento);

            // Recuperación de las UserKeywords
            pDocument.Roh_userKeywords = ObtenerUserKeywordsPublicacionResearchObject(pIdDocumento);

            // Recuperación de las SuggestedKeywords
            pDocument.Roh_suggestedKeywords = ObtenerSuggestedKeywordsPublicacionResearchObject(pIdDocumento);

            //TODO
            // Recuperación de las UserKnowledgeArea
            Dictionary<string, List<string>> userKnowledgeArea = ObtenerUserKnowledgeAreaPublicacionREsearchObject(pIdDocumento);

            //TODO
            // Recuperación de las SuggestedKnowledgeArea
            Dictionary<string, List<string>> suggestedKnowledgeArea = ObtenerSuggestedKnowledgeAreaPublicacionResearchObject(pIdDocumento);

            // Recuperación del Project
            pDocument.IdRoh_project = ObtenerProjectPublicacionResearchObject(pIdDocumento);

            //TODO
            // Recuperación del AssessmentStatus
            Tuple<string> assessmentStatus = new Tuple<string>(ObtenerAssessmentStatusPublicacionResearchObject(pIdDocumento));
        }

        /// <summary>
        /// Permite borrar y cargar un researchobject existente con datos actualizados.
        /// </summary>
        /// <param name="pResearchObject">researchobject con los datos a cargar.</param>
        /// <param name="pIdResearchObject">ID del recurso.</param>
        /// <param name="pDicAreasBroader">Diccionario con los hijos.</param>
        /// <param name="pDicAreasNombre">Diccionario con las áreas temáticas.</param>        
        /// <returns></returns>
        public static void ModificarRO(ResearchobjectOntology.ResearchObject pResearchObject, string pIdResearchObject, Dictionary<string, string> pDicAreasBroader, Dictionary<string, string> pDicAreasNombre)
        {
            //TODO revisar campos

            // Recuperación del roh:isPpublic
            pResearchObject.Roh_isPublic = ObtenerIsPublicPublicacionResearchObject(pIdResearchObject);

            // Recuperación de las UserKeywords
            pResearchObject.Roh_userKeywords = ObtenerUserKeywordsPublicacionResearchObject(pIdResearchObject);

            // Recuperación de las SuggestedKeywords
            pResearchObject.Roh_suggestedKeywords = ObtenerSuggestedKeywordsPublicacionResearchObject(pIdResearchObject);

            //TODO
            // Recuperación de las UserKnowledgeArea
            Dictionary<string, List<string>> userKnowledgeArea = ObtenerUserKnowledgeAreaPublicacionREsearchObject(pIdResearchObject);

            //TODO
            // Recuperación de las SuggestedKnowledgeArea
            Dictionary<string, List<string>> suggestedKnowledgeArea = ObtenerSuggestedKnowledgeAreaPublicacionResearchObject(pIdResearchObject);

            // Recuperación del Project
            pResearchObject.IdRoh_project = ObtenerProjectPublicacionResearchObject(pIdResearchObject);

            //TODO
            // Recuperación del AssessmentStatus
            Tuple<string> assessmentStatus = new Tuple<string>(ObtenerAssessmentStatusPublicacionResearchObject(pIdResearchObject));
        }
    }
}
