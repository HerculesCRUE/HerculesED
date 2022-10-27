using Gnoss.ApiWrapper;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.IO.Compression;
using Gnoss.ApiWrapper.ApiModel;
using DocumentOntology;
using PersonOntology;
using Gnoss.ApiWrapper.Model;
using Hercules.ED.ResearcherObjectLoad.Models.ObjetoJson;
using Hercules.ED.ResearcherObjectLoad.Config;
using System.Threading;
using Hercules.CommonsEDMA.DisambiguationEngine.Models;
using Hercules.ED.ResearcherObjectLoad.Models.DisambiguationObjects;
using System.Collections.Concurrent;
using Hercules.ED.ResearcherObjectLoad.Utils;
using static Hercules.ED.ResearcherObjectLoad.Program;
using System.Globalization;

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

        public const int MAX_INTENTOS = 10;
        public const int NUM_HILOS = 6;

        private static string RUTA_PAISES = $@"{System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase}Config/paises.json";
        private static string RUTA_ESTADOS = $@"{System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase}Config/estados-usa.json";
        #endregion

        private static Dictionary<string, string> ISSN_Revista = new Dictionary<string, string>();
        private static Dictionary<string, string> Titulo_Revista = new Dictionary<string, string>();
        private static Dictionary<string, string> EISSN_Revista = new Dictionary<string, string>();
        private static Dictionary<string, string> dicPaises = new Dictionary<string, string>();
        private static Dictionary<string, string> dicEstados = new Dictionary<string, string>();

        public static void CargaMain()
        {
            ProcesarFichero(configuracion.GetRutaDirectorioLectura(), configuracion.GetRutaDirectorioEscritura());
        }

        public static void ProcesarFichero(string pRutaLectura, string pRutaEscritura)
        {
            DirectoryInfo directorio = new DirectoryInfo(pRutaLectura);
            Disambiguation.mResourceApi = mResourceApi;
            IniciadorDiccionarioPaises();
            IniciadorDiccionarioEstadosUSA();

            // Obtención de las categorías.
            // TODO: Falta la asignación por ID y no por nombre. A la espera que elhuyar nos envíe los IDs, en lugar de los nombres.
            Tuple<Dictionary<string, string>, Dictionary<string, string>> tupla = ObtenerDatosTesauro();

            while (true)
            {
                foreach (var fichero in directorio.GetFiles("*.json"))
                {
                    try
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
                        Dictionary<string, ResearchObjectZenodo> dicIdDatosRoZenodo = new Dictionary<string, ResearchObjectZenodo>();

                        //Diccionario para almacenar las notificaciones
                        ConcurrentBag<NotificationOntology.Notification> notificaciones = new ConcurrentBag<NotificationOntology.Notification>();

                        string jsonString = String.Empty;

                        string idPersona = null;

                        if (fichero.Name.StartsWith("figshare___"))
                        {
                            string tokenFigshare = fichero.Name.Split("___")[1].Split(".").First();
                            Dictionary<string, string> dicDatosPersona = UtilityFigShare.ObtenerORCIDPorTokenFigshare(tokenFigshare);
                            string orcidAutor = dicDatosPersona.Keys.First();
                            string idFigShare = dicDatosPersona.Values.First();

                            // Obtención de los datos del JSON.
                            jsonString = File.ReadAllText(fichero.FullName);
                            List<ResearchObjectFigShare> listaFigShareData = JsonConvert.DeserializeObject<List<ResearchObjectFigShare>>(jsonString);
                            HashSet<string> listaFigShare = new HashSet<string>();

                            if (listaFigShareData != null && listaFigShareData.Any())
                            {
                                foreach (ResearchObjectFigShare researchObject in listaFigShareData)
                                {
                                    // --- ROs
                                    DisambiguationRO disambiguationRo = UtilityFigShare.GetDisambiguationRO(researchObject);
                                    string idRo = disambiguationRo.ID;
                                    listaDesambiguar.Add(disambiguationRo);

                                    // --- Autores
                                    if (researchObject.autores != null && researchObject.autores.Any())
                                    {
                                        List<DisambiguationPerson> coautores = new List<DisambiguationPerson>();
                                        foreach (Person_JSON autor in researchObject.autores)
                                        {
                                            DisambiguationPerson disambiguationPerson = UtilityPersona.GetDisambiguationPerson(pPersonaRo: autor);
                                            coautores.Add(disambiguationPerson);
                                        }
                                        foreach (DisambiguationPerson coautor in coautores)
                                        {
                                            coautor.coautores = new HashSet<string>(coautores.Where(x => x.ID != coautor.ID).Select(x => x.ID));
                                        }
                                        disambiguationRo.autores = new HashSet<string>(coautores.Select(x => x.ID));
                                        listaDesambiguar.AddRange(coautores);
                                    }

                                    dicIdRo.Add(idRo, Utility.ConstruirRO("FigShare", researchObject, tupla.Item1, tupla.Item2));
                                    dicIdDatosRoFigshare.Add(idRo, researchObject);
                                }
                                Dictionary<string, DisambiguableEntity> researchobjectsBBDD = ObtenerROPorID(orcidAutor, listaFigShare, "http://w3id.org/roh/idFigShare");
                                ConcurrentDictionary<string, DisambiguationPerson> personasBBDD = UtilityPersona.ObtenerPersonasRelacionaBBDDRO(orcidAutor, listadoFigShare: listaFigShareData);
                                listaDesambiguarBBDD.AddRange(researchobjectsBBDD.Values.ToList());
                                listaDesambiguarBBDD.AddRange(personasBBDD.Values.ToList());
                                idPersona = personasBBDD.First(x => (x.Value).figShareId == idFigShare).Key;
                            }
                        }
                        else if (fichero.Name.StartsWith("github___"))
                        {
                            string idGitHubAutor = fichero.Name.Split("___")[1];
                            string orcidAutor = UtilityGitHub.ObtenerORCIDPorGitHubID(idGitHubAutor);
                            List<string> lista = new List<string>() { UtilityGitHub.ObtenerPersonaPorGitHubID(idGitHubAutor) };
                            DisambiguationPerson personaGitHub = UtilityPersona.ObtenerDatosBasicosPersona(lista);

                            // Obtención de los datos del JSON.
                            jsonString = File.ReadAllText(fichero.FullName);
                            List<ResearchObjectGitHub> listaGithubData = JsonConvert.DeserializeObject<List<ResearchObjectGitHub>>(jsonString);
                            HashSet<string> listadoGitHub = new HashSet<string>();

                            if (listaGithubData != null && listaGithubData.Any())
                            {
                                foreach (ResearchObjectGitHub githubObject in listaGithubData)
                                {
                                    // --- ROs
                                    DisambiguationRO disambiguationRoGitHub = UtilityGitHub.GetDisambiguationRoGithub(githubObject);
                                    string idRo = disambiguationRoGitHub.ID;
                                    if (githubObject.id != null)
                                    {
                                        string githubStringAux = Convert.ToString(githubObject.id);
                                        listadoGitHub.Add(githubStringAux);
                                    }
                                    listaDesambiguar.Add(disambiguationRoGitHub);

                                    List<DisambiguationPerson> coautores = new List<DisambiguationPerson>();
                                    foreach (string nombre in githubObject.listaAutores)
                                    {
                                        DisambiguationPerson disambiguationPerson = UtilityPersona.GetDisambiguationPerson(pPersonaGit: nombre);
                                        coautores.Add(disambiguationPerson);

                                        if (disambiguationPerson.documentos == null)
                                        {
                                            disambiguationPerson.documentos = new HashSet<string>();
                                        }
                                        disambiguationPerson.documentos.Add(personaGitHub.ID);
                                        disambiguationPerson.departamento = personaGitHub.departamento;
                                        disambiguationPerson.organizacion = personaGitHub.organizacion;
                                        disambiguationPerson.grupos = personaGitHub.grupos;
                                        disambiguationPerson.proyectos = personaGitHub.proyectos;
                                    }
                                    foreach (DisambiguationPerson coautor in coautores)
                                    {
                                        coautor.coautores = new HashSet<string>(coautores.Where(x => x.ID != coautor.ID).Select(x => x.ID));
                                        coautor.distincts = new HashSet<string>(coautores.Where(x => x.ID != coautor.ID).Select(x => x.ID));
                                        coautor.departamento = personaGitHub.departamento;
                                        coautor.organizacion = personaGitHub.organizacion;
                                        coautor.grupos = personaGitHub.grupos;
                                        coautor.proyectos = personaGitHub.proyectos;
                                    }
                                    disambiguationRoGitHub.autores = new HashSet<string>(coautores.Select(x => x.ID));
                                    listaDesambiguar.AddRange(coautores);

                                    dicIdRo.Add(idRo, Utility.ConstruirRO("GitHub", githubObject, tupla.Item1, tupla.Item2));
                                    dicIdDatosRoGitHub.Add(idRo, githubObject);
                                }
                                Dictionary<string, DisambiguableEntity> researchobjectsBBDD = ObtenerROPorID(orcidAutor, listadoGitHub, "http://w3id.org/roh/idGit");
                                ConcurrentDictionary<string, DisambiguationPerson> personasBBDD = UtilityPersona.ObtenerPersonasRelacionaBBDDRO(orcidAutor, listadoGitHub: listaGithubData);
                                listaDesambiguarBBDD.AddRange(researchobjectsBBDD.Values.ToList());
                                listaDesambiguarBBDD.AddRange(personasBBDD.Values.ToList());
                                idPersona = personasBBDD.First(x => (x.Value).gitHubId == idGitHubAutor).Key;
                            }
                        }
                        else if (fichero.Name.StartsWith("zenodo___"))
                        {
                            string idAutor = fichero.Name.Split("___")[1];
                            List<string> lista = new List<string>() { UtilityPersona.ObtenerPersonaPorORCID(idAutor) };
                            DisambiguationPerson personaZenodo = UtilityPersona.ObtenerDatosBasicosPersona(lista);

                            // Obtención de los datos del JSON.
                            jsonString = File.ReadAllText(fichero.FullName);
                            List<ResearchObjectZenodo> listaZenodoData = JsonConvert.DeserializeObject<List<ResearchObjectZenodo>>(jsonString);
                            HashSet<string> listadoZenodo = new HashSet<string>();

                            if (listaZenodoData != null && listaZenodoData.Any())
                            {
                                foreach (ResearchObjectZenodo zenodoObject in listaZenodoData)
                                {
                                    // --- ROs
                                    DisambiguationRO disambiguationRoZenodo = UtilityZenodo.GetDisambiguationRoZenodo(zenodoObject);
                                    string idRo = disambiguationRoZenodo.ID;
                                    if (zenodoObject.id != null)
                                    {
                                        string zenodoStringAux = Convert.ToString(zenodoObject.id);
                                        listadoZenodo.Add(zenodoStringAux);
                                    }
                                    listaDesambiguar.Add(disambiguationRoZenodo);

                                    List<DisambiguationPerson> coautores = new List<DisambiguationPerson>();
                                    foreach (Person_JSON persona in zenodoObject.autores)
                                    {
                                        DisambiguationPerson disambiguationPerson = UtilityPersona.GetDisambiguationPerson(pPersonaRo: persona);
                                        coautores.Add(disambiguationPerson);

                                        if (disambiguationPerson.documentos == null)
                                        {
                                            disambiguationPerson.documentos = new HashSet<string>();
                                        }
                                        disambiguationPerson.documentos.Add(personaZenodo.ID);
                                        disambiguationPerson.departamento = personaZenodo.departamento;
                                        disambiguationPerson.organizacion = personaZenodo.organizacion;
                                        disambiguationPerson.grupos = personaZenodo.grupos;
                                        disambiguationPerson.proyectos = personaZenodo.proyectos;
                                    }
                                    foreach (DisambiguationPerson coautor in coautores)
                                    {
                                        coautor.coautores = new HashSet<string>(coautores.Where(x => x.ID != coautor.ID).Select(x => x.ID));
                                        coautor.distincts = new HashSet<string>(coautores.Where(x => x.ID != coautor.ID).Select(x => x.ID));
                                        coautor.departamento = personaZenodo.departamento;
                                        coautor.organizacion = personaZenodo.organizacion;
                                        coautor.grupos = personaZenodo.grupos;
                                        coautor.proyectos = personaZenodo.proyectos;
                                    }
                                    disambiguationRoZenodo.autores = new HashSet<string>(coautores.Select(x => x.ID));
                                    listaDesambiguar.AddRange(coautores);

                                    dicIdRo.Add(idRo, Utility.ConstruirRO("Zenodo", zenodoObject, tupla.Item1, tupla.Item2));
                                    dicIdDatosRoZenodo.Add(idRo, zenodoObject);
                                }
                                Dictionary<string, DisambiguableEntity> researchobjectsBBDD = ObtenerROPorID(idAutor, listadoZenodo, "http://w3id.org/roh/idZenodo");
                                ConcurrentDictionary<string, DisambiguationPerson> personasBBDD = UtilityPersona.ObtenerPersonasRelacionaBBDDRO(idAutor, listadoZenodo: listaZenodoData);
                                listaDesambiguarBBDD.AddRange(researchobjectsBBDD.Values.ToList());
                                listaDesambiguarBBDD.AddRange(personasBBDD.Values.ToList());
                                idPersona = personasBBDD.First(x => (x.Value).orcid == idAutor).Key;
                            }
                        }
                        else
                        {
                            //Obtenemos al propietario del JSON
                            string idAutor = "http://gnoss.com/items/" + fichero.Name.Split("___")[0];
                            List<string> lista = new List<string>() { idAutor };
                            DisambiguationPerson personaDocumento = UtilityPersona.ObtenerDatosBasicosPersona(lista);

                            // Obtención de los datos del JSON.
                            jsonString = File.ReadAllText(fichero.FullName);
                            List<Publication> listaPublicaciones = JsonConvert.DeserializeObject<List<Publication>>(jsonString);
                            HashSet<string> listadoDOI = new HashSet<string>();

                            if (listaPublicaciones != null && listaPublicaciones.Any())
                            {
                                foreach (Publication publication in listaPublicaciones)
                                {
                                    // --- Publicación
                                    DisambiguationPublication disambiguationPub = GetDisambiguationPublication(publication);
                                    listadoDOI.Add(disambiguationPub.doi);

                                    PublicationType(publication.typeOfPublication, pDisambiguationPub: disambiguationPub);

                                    disambiguationPub.autores = new HashSet<string>();
                                    string idPub = disambiguationPub.ID;
                                    listaDesambiguar.Add(disambiguationPub);

                                    // --- Autores
                                    if (publication.seqOfAuthors != null && publication.seqOfAuthors.Any())
                                    {
                                        List<DisambiguationPerson> coautores = new List<DisambiguationPerson>();
                                        foreach (PersonaPub autor in publication.seqOfAuthors)
                                        {
                                            DisambiguationPerson disambiguationPerson = UtilityPersona.GetDisambiguationPerson(autor);
                                            string idPerson = disambiguationPerson.ID;
                                            coautores.Add(disambiguationPerson);
                                            dicIdPersona.Add(idPerson, UtilityPersona.ConstruirPersona(autor));
                                            if (disambiguationPerson.documentos == null)
                                            {
                                                disambiguationPerson.documentos = new HashSet<string>();
                                            }
                                            disambiguationPerson.documentos.Add(publication.ID);
                                            disambiguationPerson.departamento = personaDocumento.departamento;
                                            disambiguationPerson.organizacion = personaDocumento.organizacion;
                                            disambiguationPerson.grupos = personaDocumento.grupos;
                                            disambiguationPerson.proyectos = personaDocumento.proyectos;
                                            disambiguationPerson.distincts = new HashSet<string>(publication.seqOfAuthors.Where(x => x.ID != disambiguationPerson.ID).Select(x => x.ID));
                                        }
                                        foreach (DisambiguationPerson coautor in coautores)
                                        {
                                            coautor.coautores = new HashSet<string>(coautores.Where(x => x.ID != coautor.ID).Select(x => x.ID));
                                            coautor.distincts = new HashSet<string>(coautores.Where(x => x.ID != coautor.ID).Select(x => x.ID));
                                            coautor.departamento = personaDocumento.departamento;
                                            coautor.organizacion = personaDocumento.organizacion;
                                            coautor.grupos = personaDocumento.grupos;
                                            coautor.proyectos = personaDocumento.proyectos;
                                        }
                                        listaDesambiguar.AddRange(coautores);
                                        disambiguationPub.autores = new HashSet<string>(coautores.Select(x => x.ID));
                                    }

                                    dicIdDatosPub.Add(idPub, publication);
                                    dicIdPublication.Add(idPub, ConstruirDocument(publication, tupla.Item1, tupla.Item2));
                                }
                                // Obtención de los datos cargados de BBDD.                        
                                Dictionary<string, DisambiguableEntity> documentosBBDD = ObtenerPublicacionesBBDDPorGnossId(listadoDOI, idAutor);
                                ConcurrentDictionary<string, DisambiguationPerson> personasBBDD = UtilityPersona.ObtenerPersonasRelacionaBBDD(listaPublicaciones, idAutor);
                                listaDesambiguarBBDD.AddRange(documentosBBDD.Values.ToList());
                                listaDesambiguarBBDD.AddRange(personasBBDD.Values.ToList());
                                idPersona = idAutor;
                            }
                        }

                        if (!string.IsNullOrEmpty(idPersona) && (dicIdDatosPub.Count > 0 || dicIdDatosRoFigshare.Count > 0 || dicIdDatosRoGitHub.Count > 0 || dicIdDatosRoZenodo.Count > 0))
                        {
                            // Las publicaciones que nos vengan del JSON siempre son diferentes.
                            List<string> idsPubicacionesJSON = listaDesambiguar.Where(x => x is DisambiguationPublication).Select(x => ((DisambiguationPublication)x).ID).ToList();
                            foreach (DisambiguableEntity disambiguableEntity in listaDesambiguar)
                            {
                                if (disambiguableEntity is DisambiguationPublication)
                                {
                                    disambiguableEntity.distincts = new HashSet<string>(idsPubicacionesJSON.Except(new List<string>() { disambiguableEntity.ID }));
                                }
                            }
                            Dictionary<string, Dictionary<string, List<string>>> dicccionarioPersonaIgnorarPublicaciones = ObtenerPublicacionesIgnorarPersonas();

                            HashSet<string> idsPersonasActualizar = new HashSet<string>();
                            HashSet<string> idsDocumentosActualizar = new HashSet<string>();
                            HashSet<string> idsResearchObjectsActualizar = new HashSet<string>();


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
                            List<Tuple<string, string, string, string, string, string>> datosPersonasBBDD = UtilityPersona.ObtenerPersonas(idPersonasBBDD);

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
                                    if (tipo == DISAMBIGUATION_PERSON && listaIds.Any())
                                    {
                                        //Esta variable sólo se carga en los documentos, no en los ROs
                                        if (dicIdPersona != null && dicIdPersona.Any())
                                        {
                                            UtilityPersona.CrearPersonDesambiguada(idA, listaIds, dicIdPersona, listaPersonasCargarEquivalencias);
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

                                    if (tipo == DISAMBIGUATION_PERSON && listaIds.Any())
                                    {
                                        dicGnossIdPerson.Add(new HashSet<string>(listaIds), idRecursoBBDD);

                                        string orcid = datosPersonasBBDD.FirstOrDefault(x => x.Item1 == idRecursoBBDD).Item2;

                                        if (string.IsNullOrEmpty(orcid) && dicIdPersona != null && dicIdPersona.Any())
                                        {
                                            InsertarOrcid(idRecursoBBDD, listaIds.ToList(), dicIdPersona);
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
                                        // Atributos que no hay que machacar de los documentos.
                                        DocumentoBBDD datosDocumentoBBDD = UtilityPersona.ObtenerDatosDocumento(item.Key);
                                        listaDocumentosCargados.Add(idA, item.Key);
                                        CrearDocumentDesambiguado(idA, listaIds, dicIdDatosPub, listaDocumentosCargarEquivalencias, tupla.Item1, tupla.Item2, pDocCargado: datosDocumentoBBDD);
                                    }
                                }
                            }
                            // Creación del vínculo entre los documentos y las personas (Document apunta a Person).
                            foreach (KeyValuePair<Document, HashSet<string>> item in listaDocumentosCargarEquivalencias)
                            {
                                foreach (string id in item.Value)
                                {
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
                                //Eliminamos del documento todos los autores que lo tengan dentro de 'dicccionarioPersonaIgnorarPublicaciones'
                                foreach (BFO_0000023 autoria in documento.Bibo_authorList.ToList())
                                {
                                    string idAutor = autoria.IdRdf_member;
                                    bool eliminarAutor = false;
                                    if (dicccionarioPersonaIgnorarPublicaciones.ContainsKey(idAutor))
                                    {
                                        foreach (string idNombre in dicccionarioPersonaIgnorarPublicaciones[idAutor].Keys)
                                        {
                                            foreach (string idValor in dicccionarioPersonaIgnorarPublicaciones[idAutor][idNombre])
                                            {
                                                switch (idNombre)
                                                {
                                                    case "doi":
                                                        if (documento.Bibo_doi == idValor)
                                                        {
                                                            eliminarAutor = true;
                                                        }
                                                        break;
                                                    case "handle":
                                                        if (documento.Bibo_handle == idValor)
                                                        {
                                                            eliminarAutor = true;
                                                        }
                                                        break;
                                                    case "pmid":
                                                        if (documento.Bibo_pmid == idValor)
                                                        {
                                                            eliminarAutor = true;
                                                        }
                                                        break;
                                                    default:
                                                        if (documento.Bibo_identifier != null && documento.Bibo_identifier.Exists(x => x.Foaf_topic == idNombre && x.Dc_title == idValor))
                                                        {
                                                            eliminarAutor = true;
                                                        }
                                                        break;
                                                }
                                            }
                                        }
                                    }
                                    if (eliminarAutor)
                                    {
                                        documento.Bibo_authorList.Remove(autoria);
                                    }
                                }



                                if (documento.Bibo_authorList.Exists(x => x.IdRdf_member == idPersona))
                                {
                                    string idBBDD = listaDocumentosCargarEquivalencias[documento].Intersect(listaDocumentosCargados.Keys).FirstOrDefault();
                                    if (string.IsNullOrEmpty(idBBDD))
                                    {
                                        ComplexOntologyResource resourceDocumento = documento.ToGnossApiResource(mResourceApi, null);

                                        foreach (BFO_0000023 autor in documento.Bibo_authorList)
                                        {
                                            NotificationOntology.Notification notificacion = new NotificationOntology.Notification();
                                            notificacion.IdRoh_trigger = null;
                                            notificacion.Roh_tabPropertyCV = "http://w3id.org/roh/scientificActivity";
                                            notificacion.Roh_entity = resourceDocumento.GnossId;
                                            notificacion.IdRoh_owner = autor.IdRdf_member;
                                            notificacion.Dct_issued = DateTime.UtcNow;
                                            notificacion.Roh_type = "create";
                                            notificacion.CvnCode = Utility.IdentificadorFECYT(documento.IdRoh_scientificActivityDocument);
                                            notificaciones.Add(notificacion);
                                        }

                                        listaDocumentosCargar.Add(resourceDocumento);
                                    }
                                    else
                                    {
                                        foreach (BFO_0000023 autor in documento.Bibo_authorList)
                                        {
                                            NotificationOntology.Notification notificacion = new NotificationOntology.Notification();
                                            notificacion.IdRoh_trigger = null;
                                            notificacion.Roh_tabPropertyCV = "http://w3id.org/roh/scientificActivity";
                                            notificacion.Roh_entity = listaDocumentosCargados[idBBDD];
                                            notificacion.IdRoh_owner = autor.IdRdf_member;
                                            notificacion.Dct_issued = DateTime.UtcNow;
                                            notificacion.Roh_type = "edit";
                                            notificacion.CvnCode = Utility.IdentificadorFECYT(listaDocumentosCargarEquivalencias.FirstOrDefault(x => x.Value.Contains(idBBDD)).Key.IdRoh_scientificActivityDocument);
                                            if (string.IsNullOrEmpty(notificacion.CvnCode))
                                            {
                                                notificacion.CvnCode = Utility.IdentificadorFECYT(documento.IdRoh_scientificActivityDocument);
                                            }

                                            notificaciones.Add(notificacion);
                                        }

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

                                    if (tipo == DISAMBIGUATION_RO && listaIds.Any())
                                    {
                                        if (dicIdDatosRoFigshare.Count > 0)
                                        {
                                            UtilityFigShare.CrearRoFigshareDesambiguado(idA, listaIds, dicIdDatosRoFigshare, listaROsCargarEquivalencias, tupla.Item1, tupla.Item2);
                                        }
                                        if (dicIdDatosRoGitHub.Count > 0)
                                        {
                                            UtilityGitHub.CrearRoGitHubDesambiguado(idA, listaIds, dicIdDatosRoGitHub, listaROsCargarEquivalencias, tupla.Item1, tupla.Item2);
                                        }
                                        if (dicIdDatosRoZenodo.Count > 0)
                                        {
                                            UtilityZenodo.CrearRoZenodoDesambiguado(idA, listaIds, dicIdDatosRoZenodo, listaROsCargarEquivalencias, tupla.Item1, tupla.Item2);
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
                                            UtilityFigShare.CrearRoFigshareDesambiguado(idA, listaIds, dicIdDatosRoFigshare, listaROsCargarEquivalencias, tupla.Item1, tupla.Item2);
                                        }
                                        if (dicIdDatosRoGitHub.Count > 0)
                                        {
                                            UtilityGitHub.CrearRoGitHubDesambiguado(idA, listaIds, dicIdDatosRoGitHub, listaROsCargarEquivalencias, tupla.Item1, tupla.Item2);
                                        }
                                        if (dicIdDatosRoZenodo.Count > 0)
                                        {
                                            UtilityZenodo.CrearRoZenodoDesambiguado(idA, listaIds, dicIdDatosRoZenodo, listaROsCargarEquivalencias, tupla.Item1, tupla.Item2);
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

                                        foreach (ResearchobjectOntology.BFO_0000023 autor in researchobject.Bibo_authorList)
                                        {
                                            NotificationOntology.Notification notificacion = new NotificationOntology.Notification();
                                            notificacion.IdRoh_trigger = null;
                                            notificacion.Roh_tabPropertyCV = "http://w3id.org/roh/researchObject";
                                            notificacion.Roh_entity = resourceResearchObject.GnossId;
                                            notificacion.IdRoh_owner = autor.IdRdf_member;
                                            notificacion.Dct_issued = DateTime.UtcNow;
                                            notificacion.Roh_type = "create";
                                            notificacion.CvnCode = "";

                                            notificaciones.Add(notificacion);
                                        }

                                        listaROsCargar.Add(resourceResearchObject);
                                    }
                                    else
                                    {
                                        foreach (ResearchobjectOntology.BFO_0000023 autor in researchobject.Bibo_authorList)
                                        {
                                            NotificationOntology.Notification notificacion = new NotificationOntology.Notification();
                                            notificacion.IdRoh_trigger = null;
                                            notificacion.Roh_tabPropertyCV = "http://w3id.org/roh/researchObject";
                                            notificacion.Roh_entity = listaROsCargados[idBBDD];
                                            notificacion.IdRoh_owner = autor.IdRdf_member;
                                            notificacion.Dct_issued = DateTime.UtcNow;
                                            notificacion.Roh_type = "edit";
                                            notificacion.CvnCode = "";

                                            notificaciones.Add(notificacion);
                                        }

                                        idBBDD = listaROsCargados[idBBDD];
                                        listaROsModificar.Add(idBBDD, researchobject);
                                    }
                                }
                            }

                            #endregion

                            // ------------------------------ CARGA
                            idsPersonasActualizar.UnionWith(CargarDatos(listaPersonasCargar));
                            idsDocumentosActualizar.UnionWith(CargarDatos(listaDocumentosCargar));
                            idsResearchObjectsActualizar.UnionWith(CargarDatos(listaROsCargar));

                            idsDocumentosActualizar.UnionWith(listaDocumentosModificar.Keys);
                            idsResearchObjectsActualizar.UnionWith(listaROsModificar.Keys);

                            //Modificación
                            Parallel.ForEach(listaDocumentosModificar, new ParallelOptions { MaxDegreeOfParallelism = NUM_HILOS }, recursoModificar =>
                            {
                                string[] idSplit = recursoModificar.Key.Split('_');
                                Document doc = listaDocumentosModificar[recursoModificar.Key];
                                ModificarDocumento(doc, recursoModificar.Key);
                                ComplexOntologyResource complexOntologyResource = doc.ToGnossApiResource(mResourceApi, null, new Guid(idSplit[idSplit.Length - 2]), new Guid(idSplit[idSplit.Length - 1]));

                                int numIntentos = 0;
                                while (!complexOntologyResource.Modified)
                                {
                                    numIntentos++;

                                    if (numIntentos > MAX_INTENTOS)
                                    {
                                        break;
                                    }

                                    mResourceApi.ModifyComplexOntologyResource(complexOntologyResource, false, true);
                                }
                            });

                            //Modificación
                            Parallel.ForEach(listaROsModificar, new ParallelOptions { MaxDegreeOfParallelism = NUM_HILOS }, recursoModificar =>
                            {
                                string[] idSplit = recursoModificar.Key.Split('_');
                                ResearchobjectOntology.ResearchObject ro = listaROsModificar[recursoModificar.Key];
                                ModificarRO(ro, recursoModificar.Key);
                                ComplexOntologyResource complexOntologyResource = ro.ToGnossApiResource(mResourceApi, null, new Guid(idSplit[idSplit.Length - 2]), new Guid(idSplit[idSplit.Length - 1]));

                                int numIntentos = 0;
                                while (!complexOntologyResource.Modified)
                                {
                                    numIntentos++;

                                    if (numIntentos > MAX_INTENTOS)
                                    {
                                        break;
                                    }

                                    mResourceApi.ModifyComplexOntologyResource(complexOntologyResource, false, true);
                                }
                            });

                            //Insertamos en la cola del desnormalizador
                            RabbitServiceWriterDenormalizer rabbitServiceWriterDenormalizer = new RabbitServiceWriterDenormalizer(configuracion);
                            if (idsPersonasActualizar.Count > 0)
                            {
                                rabbitServiceWriterDenormalizer.PublishMessage(new DenormalizerItemQueue(DenormalizerItemQueue.ItemType.person, idsPersonasActualizar));
                            }
                            if (idsDocumentosActualizar.Count > 0)
                            {
                                rabbitServiceWriterDenormalizer.PublishMessage(new DenormalizerItemQueue(DenormalizerItemQueue.ItemType.document, idsDocumentosActualizar));
                            }
                            if (idsResearchObjectsActualizar.Count > 0)
                            {
                                rabbitServiceWriterDenormalizer.PublishMessage(new DenormalizerItemQueue(DenormalizerItemQueue.ItemType.researchobject, idsResearchObjectsActualizar));
                            }


                            //Cargamos las notificaciones
                            List<NotificationOntology.Notification> notificacionesCargar = notificaciones.ToList();
                            mResourceApi.ChangeOntoly("notification");
                            Parallel.ForEach(notificacionesCargar, new ParallelOptions { MaxDegreeOfParallelism = NUM_HILOS }, notificacion =>
                            {
                                ComplexOntologyResource recursoCargar = notificacion.ToGnossApiResource(mResourceApi);
                                int numIntentos = 0;
                                while (!recursoCargar.Uploaded)
                                {
                                    numIntentos++;

                                    if (numIntentos > 5)
                                    {
                                        break;
                                    }
                                    mResourceApi.LoadComplexSemanticResource(recursoCargar);
                                }
                            });
                        }

                        // Notificación de fin de la carga
                        if (!string.IsNullOrEmpty(idPersona))
                        {
                            mResourceApi.ChangeOntoly("notification");
                            NotificationOntology.Notification notificacion = new NotificationOntology.Notification();
                            notificacion.IdRoh_owner = idPersona;
                            notificacion.Dct_issued = DateTime.UtcNow;
                            notificacion.Roh_type = "loadExternalSource";

                            ComplexOntologyResource recursoCargar = notificacion.ToGnossApiResource(mResourceApi);
                            int numIntentos = 0;
                            while (!recursoCargar.Uploaded)
                            {
                                numIntentos++;

                                if (numIntentos > 5)
                                {
                                    break;
                                }
                                mResourceApi.LoadComplexSemanticResource(recursoCargar);
                            }
                        }

                        // Hace una copia del fichero y elimina el original.
                        CrearZip(pRutaEscritura, fichero.Name, jsonString);
                        File.Delete(fichero.FullName);
                    }
                    catch (Exception ex)
                    {
                        FileLogger.Log($@"ERROR - {ex.Message}");
                        File.Delete(fichero.FullName);
                    }
                }

                Thread.Sleep(60000);
            }
        }

        private static Dictionary<string, Dictionary<string, List<string>>> ObtenerPublicacionesIgnorarPersonas()
        {
            Dictionary<string, Dictionary<string, List<string>>> dicccionarioPersonaIgnorarPublicaciones = new Dictionary<string, Dictionary<string, List<string>>>();
            int limit = 10000;
            int offset = 0;
            while (true)
            {
                //Selecciono los RO de autor

                string select = $"SELECT * WHERE {{ SELECT DISTINCT ?person ?idName ?idValue";
                string where = $@"WHERE {{
                                    ?person a <http://xmlns.com/foaf/0.1/Person> . 
                                    ?person <http://w3id.org/roh/ignorePublication> ?ignorePublication. 
                                    ?ignorePublication <http://xmlns.com/foaf/0.1/topic> ?idName. 
                                    ?ignorePublication <http://w3id.org/roh/title> ?idValue. 
                                }} ORDER BY DESC(?ro) }} LIMIT {limit} OFFSET {offset}";

                SparqlObject resultadoQuery = mResourceApi.VirtuosoQuery(select, where, "person");
                if (resultadoQuery != null && resultadoQuery.results != null && resultadoQuery.results.bindings != null && resultadoQuery.results.bindings.Count > 0)
                {
                    offset += limit;
                    foreach (Dictionary<string, SparqlObject.Data> fila in resultadoQuery.results.bindings)
                    {
                        string person = fila["person"].value;
                        string idName = fila["idName"].value;
                        string idValue = fila["idValue"].value;
                        if (!dicccionarioPersonaIgnorarPublicaciones.ContainsKey(person))
                        {
                            dicccionarioPersonaIgnorarPublicaciones.Add(person, new Dictionary<string, List<string>>());
                        }
                        if (!dicccionarioPersonaIgnorarPublicaciones[person].ContainsKey(idName))
                        {
                            dicccionarioPersonaIgnorarPublicaciones[person].Add(idName, new List<string>());
                        }
                        dicccionarioPersonaIgnorarPublicaciones[person][idName].Add(idValue);
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
            return dicccionarioPersonaIgnorarPublicaciones;
        }

        public static void IniciadorDiccionarioPaises()
        {
            List<Pais> listaPaises = JsonConvert.DeserializeObject<List<Pais>>(File.ReadAllText(RUTA_PAISES));
            foreach (Pais pais in listaPaises)
            {
                dicPaises.Add(pais.name.ToLower(), pais.country_code);
            }
        }

        public static void IniciadorDiccionarioEstadosUSA()
        {
            dicEstados = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(RUTA_ESTADOS));
        }

        /// <summary>
        /// Obtiene los RO pertenecientes al autor con ORCID <paramref name="orcidAutor"/>
        /// </summary>
        /// <param name="orcidAutor"></param>
        /// <param name="listadoID"></param>
        /// <param name="propiedadID"></param>
        /// <returns></returns>
        private static Dictionary<string, DisambiguableEntity> ObtenerROPorID(string orcidAutor, HashSet<string> listadoID, string propiedadID)
        {
            Dictionary<string, DisambiguableEntity> listaRO = new Dictionary<string, DisambiguableEntity>();
            int limit = 10000;
            int offset = 0;

            // Consulta sparql.
            while (true)
            {
                //Selecciono los RO de autor

                string select = $"SELECT * WHERE {{ SELECT DISTINCT ?ro ?titulo";
                string where = $@"WHERE {{
                                ?ro a <http://w3id.org/roh/ResearchObject> . 
                                ?ro <http://w3id.org/roh/title> ?titulo. 
                                ?ro <http://purl.org/ontology/bibo/authorList> ?listaAutores. 
                                ?listaAutores <http://www.w3.org/1999/02/22-rdf-syntax-ns#member> ?persona. 
                                ?persona <http://w3id.org/roh/ORCID> ?orcid. 
                                FILTER(?orcid = '{orcidAutor}') 
                            }} ORDER BY DESC(?ro) }} LIMIT {limit} OFFSET {offset}";

                SparqlObject resultadoQuery = mResourceApi.VirtuosoQueryMultipleGraph(select, where, new() { "researchobject", "person" });
                if (resultadoQuery != null && resultadoQuery.results != null && resultadoQuery.results.bindings != null && resultadoQuery.results.bindings.Count > 0)
                {
                    offset += limit;
                    foreach (Dictionary<string, SparqlObject.Data> fila in resultadoQuery.results.bindings)
                    {
                        ResearchObjectZenodo researchObject = new ResearchObjectZenodo();
                        if (fila.ContainsKey("id"))
                        {
                            researchObject.ID = fila["id"].value;
                        }
                        researchObject.titulo = fila["titulo"].value;
                        DisambiguationRO pub = UtilityZenodo.GetDisambiguationRoZenodo(researchObject);
                        pub.ID = fila["ro"].value;
                        pub.autores = new HashSet<string>();
                        listaRO.Add(fila["ro"].value, pub);
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

            //Añado los documentos por ID
            while (true)
            {
                offset = 0;
                string select = "SELECT * WHERE { SELECT DISTINCT ?ro ?id ?titulo ";
                string where = $@"WHERE {{
                                ?ro a <http://w3id.org/roh/ResearchObject> .                                 
                                ?ro <{propiedadID}> ?id . FILTER(?id in (""{string.Join("\",\"", listadoID)}""))                                
                                ?ro <http://w3id.org/roh/title> ?titulo. 
                            }} ORDER BY DESC(?ro) }} LIMIT {limit} OFFSET {offset}";

                SparqlObject resultadoQuery = mResourceApi.VirtuosoQuery(select, where, "document");
                if (resultadoQuery != null && resultadoQuery.results != null && resultadoQuery.results.bindings != null && resultadoQuery.results.bindings.Count > 0)
                {
                    offset += limit;
                    foreach (Dictionary<string, SparqlObject.Data> fila in resultadoQuery.results.bindings)
                    {
                        ResearchObjectZenodo publicacion = new ResearchObjectZenodo();
                        if (fila.ContainsKey("id"))
                        {
                            publicacion.ID = fila["id"].value;
                        }
                        publicacion.titulo = fila["titulo"].value;
                        DisambiguationRO pub = UtilityZenodo.GetDisambiguationRoZenodo(publicacion);
                        pub.ID = fila["ro"].value;
                        pub.autores = new HashSet<string>();
                        listaRO.TryAdd(fila["ro"].value, pub);
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

            List<List<string>> listaListasDocs = Utility.SplitList(listaRO.Keys.ToList(), 500).ToList();
            ObtenerAutoresRO(listaListasDocs, listaRO);

            return listaRO;
        }

        /// <summary>
        /// Obtiene las publicaciones cargadas de BBDD mediante un orcid.
        /// </summary>
        /// <param name="pOrcid">Código ORCID de la persona a obtener los datos.</param>
        /// <returns>Diccionario con el ID del recurso cargado como clave, y el objeto desambiguable como valor.</returns>
        private static Dictionary<string, DisambiguableEntity> ObtenerPublicacionesBBDDPorGnossId(HashSet<string> listadoDOI, string pGnossId)
        {
            Dictionary<string, DisambiguableEntity> listaDocumentos = new Dictionary<string, DisambiguableEntity>();
            int limit = 10000;
            int offset = 0;

            Dictionary<string, Publication> dicDocAux = new Dictionary<string, Publication>();

            // Consulta sparql.
            while (true)
            {
                string select = $@"SELECT * 
                                   WHERE {{ 
                                       SELECT DISTINCT ?documento ?doi ?titulo ?id ?fuenteId ?sad";
                string where = $@"WHERE {{
                                ?documento a <http://purl.org/ontology/bibo/Document>. 
                                OPTIONAL{{?documento <http://purl.org/ontology/bibo/doi> ?doi. }}
                                OPTIONAL{{?documento <http://purl.org/ontology/bibo/identifier> ?identificadores. 
                                ?identificadores <http://xmlns.com/foaf/0.1/topic> ?fuenteId.
                                ?identificadores <http://purl.org/dc/elements/1.1/title> ?id.
                                }}
                                OPTIONAL{{ ?documento <http://w3id.org/roh/scientificActivityDocument> ?sad.}}
                                ?documento <http://w3id.org/roh/title> ?titulo. 
                                ?documento <http://purl.org/ontology/bibo/authorList> ?listaAutores. 
                                ?listaAutores <http://www.w3.org/1999/02/22-rdf-syntax-ns#member> <{pGnossId}>. 
                            }} ORDER BY DESC(?documento) }} LIMIT {limit} OFFSET {offset}";

                SparqlObject resultadoQuery = mResourceApi.VirtuosoQueryMultipleGraph(select, where, new() { "document", "person" });
                if (resultadoQuery != null && resultadoQuery.results != null && resultadoQuery.results.bindings != null && resultadoQuery.results.bindings.Count > 0)
                {
                    offset += limit;
                    foreach (Dictionary<string, SparqlObject.Data> fila in resultadoQuery.results.bindings)
                    {
                        Publication publicacion = new Publication();
                        if (fila.ContainsKey("sad") && !string.IsNullOrEmpty(fila["sad"].value))
                        {
                            publicacion.typeOfPublication = fila["sad"].value;
                        }
                        if (fila.ContainsKey("doi"))
                        {
                            publicacion.doi = fila["doi"].value;
                        }
                        publicacion.title = fila["titulo"].value;
                        if (fila.ContainsKey("id") && fila.ContainsKey("fuenteId"))
                        {
                            publicacion.iDs = new List<string>();
                            publicacion.iDs.Add(fila["fuenteId"].value + ":" + fila["id"].value);
                        }

                        if (!dicDocAux.ContainsKey(fila["documento"].value))
                        {
                            dicDocAux.Add(fila["documento"].value, publicacion);
                        }
                        else
                        {
                            dicDocAux[fila["documento"].value].iDs.Add(fila["fuenteId"].value + ":" + fila["id"].value);
                        }
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

            foreach (KeyValuePair<string, Publication> item in dicDocAux)
            {
                DisambiguationPublication pub = GetDisambiguationPublication(item.Value);
                pub.ID = item.Key;
                pub.scientificActivityDocument = item.Value.typeOfPublication;
                pub.autores = new HashSet<string>();
                listaDocumentos.Add(item.Key, pub);
            }

            // Añado los documentos por DOI.
            while (true)
            {
                offset = 0;
                string select = "SELECT * WHERE { SELECT DISTINCT ?documento ?doi ?titulo ?sad ";
                string where = $@"WHERE {{
                                ?documento a <http://purl.org/ontology/bibo/Document>. 
                                ?documento <http://purl.org/ontology/bibo/doi> ?doi.
                                ?documento <http://w3id.org/roh/title> ?titulo. 
                                OPTIONAL{{ ?documento <http://w3id.org/roh/scientificActivityDocument> ?sad.}}
                                FILTER(?doi in (""{string.Join("\",\"", listadoDOI)}""))
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
                        if (fila.ContainsKey("sad") && !string.IsNullOrEmpty(fila["sad"].value))
                        {
                            pub.scientificActivityDocument = fila["sad"].value;
                        }
                        pub.ID = fila["documento"].value;
                        pub.autores = new HashSet<string>();
                        listaDocumentos.TryAdd(fila["documento"].value, pub);
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

            List<List<string>> listaListasDocs = Utility.SplitList(listaDocumentos.Keys.ToList(), 500).ToList();
            ObtenerAutoresPublicacion(listaListasDocs, listaDocumentos);

            return listaDocumentos;
        }

        /// <summary>
        /// Obtiene el listado de autores de los documentos listados en <paramref name="listaListas"/>
        /// y los almacena en <paramref name="listaDocumentos"/>
        /// </summary>
        /// <param name="listaListas"></param>
        /// <param name="listaDocumentos"></param>
        private static void ObtenerAutoresPublicacion(List<List<string>> listaListas, Dictionary<string, DisambiguableEntity> listaDocumentos)
        {
            int limit = 10000;
            int offset = 0;
            foreach (List<string> lista in listaListas)
            {
                // Consulta sparql.
                while (true)
                {
                    SparqlObject resultadoQuery = GetAutorsFromDocumentQuery(lista, limit, offset, "document", "documento");

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
                            break;
                        }
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Obtiene el listado de autores de los RO listados en <paramref name= "listaListas" />
        /// y los almacena en <paramref name="listaRO"/> 
        /// </summary>
        /// <param name="listaListas"></param>
        /// <param name="listaRO"></param>
        private static void ObtenerAutoresRO(List<List<string>> listaListas, Dictionary<string, DisambiguableEntity> listaRO)
        {
            int limit = 10000;
            int offset = 0;
            foreach (List<string> lista in listaListas)
            {
                // Consulta sparql.
                while (true)
                {
                    SparqlObject resultadoQuery = GetAutorsFromDocumentQuery(lista, limit, offset, "researchobject", "ro");
                    if (resultadoQuery != null && resultadoQuery.results != null && resultadoQuery.results.bindings != null && resultadoQuery.results.bindings.Count > 0)
                    {
                        offset += limit;
                        foreach (Dictionary<string, SparqlObject.Data> fila in resultadoQuery.results.bindings)
                        {
                            string doc = fila["ro"].value;
                            string autor = fila["autor"].value;
                            ((DisambiguationRO)listaRO[doc]).autores.Add(autor);
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
        private static void CrearDocumentDesambiguado(string idPublicacion, HashSet<string> pListaIds,
            Dictionary<string, Publication> pDicIdPublicacion, Dictionary<Document, HashSet<string>> pListaPublicacionesCreadas,
            Dictionary<string, string> pDicAreasBroader, Dictionary<string, string> pDicAreasNombre, DocumentoBBDD pDocCargado = null)
        {
            Publication documentoA = pDicIdPublicacion[idPublicacion];
            Document documentoCreado = new Document();

            foreach (string idSimilar in pListaIds)
            {
                Publication documentoB = pDicIdPublicacion[idSimilar];
                documentoCreado = ConstruirDocument(documentoA, pDicAreasBroader, pDicAreasNombre, pPublicacionB: documentoB, pDocCargado: pDocCargado);
            }

            HashSet<string> listaTotalIds = pListaIds;
            listaTotalIds.Add(idPublicacion);
            pListaPublicacionesCreadas.Add(documentoCreado, listaTotalIds);
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
        public static Document ConstruirDocument(Publication pPublicacion, Dictionary<string, string> pDicAreasBroader,
            Dictionary<string, string> pDicAreasNombre, Publication pPublicacionB = null, DocumentoBBDD pDocCargado = null)
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

            // Volumen
            if (!string.IsNullOrEmpty(pPublicacion.volume))
            {
                document.Bibo_volume = pPublicacion.volume;

                if (pPublicacionB != null && !string.IsNullOrEmpty(pPublicacionB.volume) && string.IsNullOrEmpty(document.Bibo_volume))
                {
                    document.Bibo_volume = pPublicacionB.volume;
                }
            }

            // Numero
            if (!string.IsNullOrEmpty(pPublicacion.articleNumber))
            {
                document.Bibo_issue = pPublicacion.articleNumber;

                if (pPublicacionB != null && !string.IsNullOrEmpty(pPublicacionB.articleNumber) && string.IsNullOrEmpty(document.Bibo_issue))
                {
                    document.Bibo_issue = pPublicacionB.articleNumber;
                }
            }

            // OpenAccess
            if (pPublicacion.openAccess.HasValue)
            {
                document.Roh_openAccess = pPublicacion.openAccess.Value;

                if (pPublicacionB != null && pPublicacionB.openAccess.HasValue && !document.Roh_openAccess)
                {
                    document.Roh_openAccess = pPublicacionB.openAccess.Value;
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

            // Titulo de la Conferencia
            if (pPublicacion.conferencia != null && !string.IsNullOrEmpty(pPublicacion.conferencia.titulo))
            {
                document.Bibo_presentedAt = pPublicacion.conferencia.titulo;

                if (pPublicacionB != null && pPublicacionB.conferencia != null && !string.IsNullOrEmpty(pPublicacionB.conferencia.titulo) && string.IsNullOrEmpty(document.Bibo_presentedAt))
                {
                    document.Bibo_presentedAt = pPublicacionB.conferencia.titulo;
                }
            }

            // Fecha inicio de la Conferencia
            if (pPublicacion.conferencia != null && !string.IsNullOrEmpty(pPublicacion.conferencia.fechaInicio))
            {
                string fecha = pPublicacion.conferencia.fechaInicio;
                document.Roh_presentedAtStart = new DateTime(Int32.Parse(fecha.Split('-')[0]), Int32.Parse(fecha.Split('-')[1]), Int32.Parse(fecha.Split('-')[2]));

                if (pPublicacionB != null && pPublicacionB.conferencia != null && !string.IsNullOrEmpty(pPublicacionB.conferencia.fechaInicio) && document.Roh_presentedAtStart == null)
                {
                    string fechaB = pPublicacionB.conferencia.fechaInicio;
                    document.Roh_presentedAtStart = new DateTime(Int32.Parse(fechaB.Split('-')[0]), Int32.Parse(fechaB.Split('-')[1]), Int32.Parse(fechaB.Split('-')[2]));
                }
            }

            // Fecha fin de la Conferencia
            if (pPublicacion.conferencia != null && !string.IsNullOrEmpty(pPublicacion.conferencia.fechaFin))
            {
                string fecha = pPublicacion.conferencia.fechaFin;
                document.Roh_presentedAtEnd = new DateTime(Int32.Parse(fecha.Split('-')[0]), Int32.Parse(fecha.Split('-')[1]), Int32.Parse(fecha.Split('-')[2]));

                if (pPublicacionB != null && pPublicacionB.conferencia != null && !string.IsNullOrEmpty(pPublicacionB.conferencia.fechaFin) && document.Roh_presentedAtEnd == null)
                {
                    string fechaB = pPublicacionB.conferencia.fechaFin;
                    document.Roh_presentedAtEnd = new DateTime(Int32.Parse(fechaB.Split('-')[0]), Int32.Parse(fechaB.Split('-')[1]), Int32.Parse(fechaB.Split('-')[2]));
                }
            }

            // Ciudad de celebración de la Conferencia
            if (pPublicacion.conferencia != null && !string.IsNullOrEmpty(pPublicacion.conferencia.ciudad))
            {
                if (dicEstados.ContainsKey(pPublicacion.conferencia.pais.ToUpper()))
                {
                    document.Roh_presentedAtLocality = pPublicacion.conferencia.ciudad + ", " + dicEstados[pPublicacion.conferencia.pais.ToUpper()];
                }

                if (pPublicacionB != null && pPublicacionB.conferencia != null && !string.IsNullOrEmpty(pPublicacionB.conferencia.ciudad) && string.IsNullOrEmpty(document.Roh_presentedAtLocality))
                {
                    if (dicEstados.ContainsKey(pPublicacionB.conferencia.pais.ToUpper()))
                    {
                        document.Roh_presentedAtLocality = pPublicacionB.conferencia.ciudad + ", " + dicEstados[pPublicacionB.conferencia.pais.ToUpper()];
                    }
                }
            }

            // País de celebración de la Conferencia
            if (pPublicacion.conferencia != null && !string.IsNullOrEmpty(pPublicacion.conferencia.pais))
            {
                if (dicEstados.ContainsKey(pPublicacion.conferencia.pais.ToUpper()))
                {
                    document.IdRoh_presentedAtHasCountryName = $@"{mResourceApi.GraphsUrl}items/feature_PCLD_840"; // Estados Unidos de América
                }
                else if (dicPaises.ContainsKey(pPublicacion.conferencia.pais.ToLower()))
                {
                    document.IdRoh_presentedAtHasCountryName = $@"{mResourceApi.GraphsUrl}items/feature_PCLD_{dicPaises[pPublicacion.conferencia.pais.ToLower()]}";
                }

                if (pPublicacionB != null && pPublicacionB.conferencia != null && !string.IsNullOrEmpty(pPublicacionB.conferencia.pais) && string.IsNullOrEmpty(document.IdRoh_presentedAtHasCountryName))
                {
                    if (dicEstados.ContainsKey(pPublicacionB.conferencia.pais.ToUpper()))
                    {
                        document.IdRoh_presentedAtHasCountryName = $@"{mResourceApi.GraphsUrl}items/feature_PCLD_840"; // Estados Unidos de América
                    }
                    else if (dicPaises.ContainsKey(pPublicacionB.conferencia.pais.ToLower()))
                    {
                        document.IdRoh_presentedAtHasCountryName = $@"{mResourceApi.GraphsUrl}items/feature_PCLD_{dicPaises[pPublicacionB.conferencia.pais.ToLower()]}";
                    }
                }
            }

            // Tipo de publicación (TypeOfPublication)
            if (pPublicacion.typeOfPublication != null)
            {
                PublicationType(pPublicacion.typeOfPublication, document);
            }
            else if (pPublicacionB != null && pPublicacionB.typeOfPublication != null)
            {
                PublicationType(pPublicacionB.typeOfPublication, document);
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
            List<EnrichedKeyWord> etiquetasEnriquecidas = new List<EnrichedKeyWord>();
            if (pPublicacion.freetextKeyword_enriquecidas != null && pPublicacion.freetextKeyword_enriquecidas.Count > 0)
            {
                foreach (FreetextKeywordEnriquecida tag in pPublicacion.freetextKeyword_enriquecidas)
                {
                    if (!listaSinRepetirEtiquetas.Contains(tag.word.ToLower()))
                    {
                        etiquetasEnriquecidas.Add(new EnrichedKeyWord() { Roh_title = tag.word, Roh_score = float.Parse(tag.porcentaje.Replace(",", "."), CultureInfo.InvariantCulture) });
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
                        etiquetasEnriquecidas.Add(new EnrichedKeyWord() { Roh_title = tag.word, Roh_score = float.Parse(tag.porcentaje.Replace(",", "."), CultureInfo.InvariantCulture) });
                        listaSinRepetirEtiquetas.Add(tag.word.ToLower());
                    }
                }
            }
            document.Roh_enrichedKeywords = etiquetasEnriquecidas.ToList();

            // Etiquetas del Usuario (UserKeyWords)
            if (pDocCargado != null && pDocCargado.etiquetas != null && pDocCargado.etiquetas.Any())
            {
                document.Roh_userKeywords = pDocCargado.etiquetas.ToList();
            }

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
            if (pPublicacion.url != null && pPublicacion.url.Count > 0)
            {
                foreach (string url in pPublicacion.url)
                {
                    urlSinRepetir.Add(url);
                }
            }
            if (pPublicacionB != null && pPublicacionB.url != null && pPublicacionB.url.Count > 0)
            {
                foreach (string url in pPublicacionB.url)
                {
                    urlSinRepetir.Add(url);
                }
            }
            if (urlSinRepetir != null && urlSinRepetir.Any())
            {
                document.Vcard_url = urlSinRepetir.First(); // TODO: Mirar el tema de las diversas URLs.
            }

            // PDF
            if (pDocCargado != null && !string.IsNullOrEmpty(pDocCargado.urlPdf) && string.IsNullOrEmpty(pPublicacion.pdf))
            {
                document.Roh_hasFile = pDocCargado.urlPdf;
            }
            else
            {
                document.Roh_hasFile = pPublicacion.pdf;

                if (pPublicacionB != null && !string.IsNullOrEmpty(pPublicacionB.pdf) && string.IsNullOrEmpty(document.Roh_hasFile))
                {
                    document.Roh_hasFile = pPublicacionB.pdf;
                }
            }

            // Página de inicio (PageStart)
            if (!string.IsNullOrEmpty(pPublicacion.pageStart) && int.TryParse(pPublicacion.pageStart, out int n1))
            {
                document.Bibo_pageStart = pPublicacion.pageStart;

                if (pPublicacionB != null && !string.IsNullOrEmpty(pPublicacionB.pageStart) && int.TryParse(pPublicacionB.pageStart, out int n11) && document.Bibo_pageStart == null)
                {
                    document.Bibo_pageStart = pPublicacionB.pageStart;
                }
            }

            // Página de fin (PageEnd)
            if (!string.IsNullOrEmpty(pPublicacion.pageEnd) && int.TryParse(pPublicacion.pageEnd, out int n3))
            {
                document.Bibo_pageEnd = pPublicacion.pageEnd;

                if (pPublicacionB != null && !string.IsNullOrEmpty(pPublicacionB.pageEnd) && int.TryParse(pPublicacion.pageEnd, out int n33) && document.Bibo_pageEnd == null)
                {
                    document.Bibo_pageEnd = pPublicacionB.pageEnd;
                }
            }

            // Origen de fuentes
            HashSet<string> origenesSinRepetir = new HashSet<string>();
            if (pPublicacion.dataOriginList != null && pPublicacion.dataOriginList.Any())
            {
                foreach (string origen in pPublicacion.dataOriginList)
                {
                    origenesSinRepetir.Add(origen);
                }
            }
            if (pPublicacionB != null && pPublicacionB.dataOriginList != null && pPublicacion.dataOriginList.Any())
            {
                foreach (string origen in pPublicacionB.dataOriginList)
                {
                    origenesSinRepetir.Add(origen);
                }
            }
            document.Roh_dataOrigin = origenesSinRepetir.ToList();

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
            if ((document.Roh_externalKnowledgeArea == null ||
                document.Roh_externalKnowledgeArea.Count == 0) && pPublicacionB != null && pPublicacionB.hasKnowledgeAreas != null && pPublicacionB.hasKnowledgeAreas.Count > 0)
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
            if ((document.Roh_enrichedKnowledgeArea == null ||
                document.Roh_enrichedKnowledgeArea.Count == 0) && pPublicacionB != null && pPublicacionB.topics_enriquecidos != null && pPublicacionB.topics_enriquecidos.Count > 0)
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

            // Areas de conocimiento del Usuario (UserKnowledgeArea) 
            // TODO: Cuando se haga el cambio de los IDs, hay que coger el ID en lugar del nombre.
            if (pDocCargado != null && pDocCargado.categorias != null && pDocCargado.categorias.Any())
            {
                document.Roh_userKnowledgeArea = new List<DocumentOntology.CategoryPath>();
                foreach (string area in pDocCargado.categorias)
                {
                    if (pDicAreasNombre.ContainsKey(area.ToLower()))
                    {
                        DocumentOntology.CategoryPath categoria = new DocumentOntology.CategoryPath();
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
                                document.Roh_userKnowledgeArea.Add(categoria);
                            }
                        }
                        listaIDs.Add(idHijoAux);
                    }
                }
            }

            // Métricas (HasMetric)
            if (pPublicacion.hasMetric != null && pPublicacion.hasMetric.Count > 0)
            {
                foreach (HasMetric itemMetric in pPublicacion.hasMetric)
                {
                    if (itemMetric.metricName.ToLower() == "wos")
                    {
                        document.Roh_wosCitationCount = int.Parse(itemMetric.citationCount);
                    }

                    if (itemMetric.metricName.ToLower() == "scopus")
                    {
                        document.Roh_scopusCitationCount = int.Parse(itemMetric.citationCount);
                    }

                    if (itemMetric.metricName.ToLower() == "semanticscholar")
                    {
                        document.Roh_semanticScholarCitationCount = int.Parse(itemMetric.citationCount);
                    }
                }
            }
            if (document.Roh_wosCitationCount == null)
            {
                if (pPublicacionB != null && pPublicacionB.hasMetric != null && pPublicacionB.hasMetric.Count > 0)
                {
                    foreach (HasMetric itemMetric in pPublicacionB.hasMetric)
                    {
                        if (itemMetric.metricName.ToLower() == "wos")
                        {
                            document.Roh_wosCitationCount = int.Parse(itemMetric.citationCount);
                        }
                    }
                }
            }
            if (document.Roh_scopusCitationCount == null)
            {
                if (pPublicacionB != null && pPublicacionB.hasMetric != null && pPublicacionB.hasMetric.Count > 0)
                {
                    foreach (HasMetric itemMetric in pPublicacionB.hasMetric)
                    {
                        if (itemMetric.metricName.ToLower() == "scopus")
                        {
                            document.Roh_scopusCitationCount = int.Parse(itemMetric.citationCount);
                        }
                    }
                }
            }

            // Revista (HasPublicationVenue)
            if (pPublicacion.hasPublicationVenue != null && !string.IsNullOrEmpty(pPublicacion.hasPublicationVenue.name) && (pPublicacion.hasPublicationVenue.type == "Journal" ||
                string.IsNullOrEmpty(pPublicacion.hasPublicationVenue.type)))
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

                // Comprobar Título 
                if (string.IsNullOrEmpty(idRevista) && !string.IsNullOrEmpty(pPublicacion.hasPublicationVenue.name))
                {
                    idRevista = ComprobarRevistaTitulo(pPublicacion.hasPublicationVenue.name);
                }

                // Apunte a la revista.
                if (!string.IsNullOrEmpty(idRevista))
                {
                    document.IdVivo_hasPublicationVenue = idRevista;
                    document.IdRoh_supportType = mResourceApi.GraphsUrl + "items/documentformat_057";
                }
                else
                {
                    if (!string.IsNullOrEmpty(pPublicacion.hasPublicationVenue.name))
                    {
                        document.Roh_hasPublicationVenueJournalText = pPublicacion.hasPublicationVenue.name;
                        document.IdRoh_supportType = mResourceApi.GraphsUrl + "items/documentformat_057";
                    }

                    if (pPublicacion.hasPublicationVenue.issn != null && pPublicacion.hasPublicationVenue.issn.Count > 0)
                    {
                        document.Bibo_issn = pPublicacion.hasPublicationVenue.issn[0];
                        document.IdRoh_supportType = mResourceApi.GraphsUrl + "items/documentformat_057";
                    }
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

                // Comprobar Título
                if (string.IsNullOrEmpty(idRevista) && !string.IsNullOrEmpty(pPublicacionB.hasPublicationVenue.name))
                {
                    idRevista = ComprobarRevistaTitulo(pPublicacionB.hasPublicationVenue.name);
                }

                // Apunte a la revista.
                if (!string.IsNullOrEmpty(idRevista))
                {
                    document.IdVivo_hasPublicationVenue = idRevista;
                    document.IdRoh_supportType = mResourceApi.GraphsUrl + "items/documentformat_057";
                }
                else
                {
                    if (!string.IsNullOrEmpty(pPublicacionB.hasPublicationVenue.name))
                    {
                        document.Roh_hasPublicationVenueJournalText = pPublicacionB.hasPublicationVenue.name;
                        document.IdRoh_supportType = mResourceApi.GraphsUrl + "items/documentformat_057";
                    }

                    if (pPublicacionB.hasPublicationVenue.issn != null && pPublicacionB.hasPublicationVenue.issn.Count > 0)
                    {
                        document.Bibo_issn = pPublicacionB.hasPublicationVenue.issn[0];
                        document.IdRoh_supportType = mResourceApi.GraphsUrl + "items/documentformat_057";
                    }
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
                int? ordenAux = pPublicacion.seqOfAuthors.Max(x => x.orden);
                document.Bibo_authorList = new List<BFO_0000023>();
                int contador = 0;
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

                    if (personaPub.orden.HasValue)
                    {
                        // Orden no nulo.
                        bfo_0000023.Rdf_comment = personaPub.orden.Value;
                    }
                    else if (ordenAux.HasValue)
                    {
                        // Si el orden es nulo, coge el orden máximo y le suma uno.
                        ordenAux++;
                        bfo_0000023.Rdf_comment = ordenAux.Value;
                    }
                    else
                    {
                        // Si el orden es nulo, le asignamos un orden a parte.
                        contador++;
                        bfo_0000023.Rdf_comment = contador;
                    }

                    bfo_0000023.IdRdf_member = personaPub.ID;
                    document.Bibo_authorList.Add(bfo_0000023);
                }
            }

            return document;
        }

        private static void PublicationType(string pType, Document pDocument = null, DisambiguationPublication pDisambiguationPub = null)
        {
            switch (pType)
            {
                case JOURNAL_ARTICLE:
                    if (pDocument != null)
                    {
                        pDocument.IdRoh_scientificActivityDocument = $"{mResourceApi.GraphsUrl}items/scientificactivitydocument_SAD1";
                        pDocument.IdDc_type = $"{mResourceApi.GraphsUrl}items/publicationtype_020";
                    }
                    if (pDisambiguationPub != null)
                    {
                        pDisambiguationPub.scientificActivityDocument = $"{mResourceApi.GraphsUrl}items/scientificactivitydocument_SAD1";
                    }
                    break;

                case BOOK:
                    if (pDocument != null)
                    {
                        pDocument.IdRoh_scientificActivityDocument = $"{mResourceApi.GraphsUrl}items/scientificactivitydocument_SAD1";
                        pDocument.IdDc_type = $"{mResourceApi.GraphsUrl}items/publicationtype_032";
                    }
                    if (pDisambiguationPub != null)
                    {
                        pDisambiguationPub.scientificActivityDocument = $"{mResourceApi.GraphsUrl}items/scientificactivitydocument_SAD1";
                    }
                    break;

                case CHAPTER:
                    if (pDocument != null)
                    {
                        pDocument.IdRoh_scientificActivityDocument = $"{mResourceApi.GraphsUrl}items/scientificactivitydocument_SAD1";
                        pDocument.IdDc_type = $"{mResourceApi.GraphsUrl}items/publicationtype_004";
                    }
                    if (pDisambiguationPub != null)
                    {
                        pDisambiguationPub.scientificActivityDocument = $"{mResourceApi.GraphsUrl}items/scientificactivitydocument_SAD1";
                    }
                    break;

                case CONFERENCE_PAPER:
                    if (pDocument != null)
                    {
                        pDocument.IdRoh_scientificActivityDocument = $"{mResourceApi.GraphsUrl}items/scientificactivitydocument_SAD2";
                    }
                    if (pDisambiguationPub != null)
                    {
                        pDisambiguationPub.scientificActivityDocument = $"{mResourceApi.GraphsUrl}items/scientificactivitydocument_SAD2";
                    }
                    break;

                default:
                    if (pDocument != null)
                    {
                        pDocument.IdRoh_scientificActivityDocument = $"{mResourceApi.GraphsUrl}items/scientificactivitydocument_SAD1";
                        pDocument.IdDc_type = $"{mResourceApi.GraphsUrl}items/publicationtype_OTHERS";
                        pDocument.Roh_typeOthers = "Otros";
                    }
                    if (pDisambiguationPub != null)
                    {
                        pDisambiguationPub.scientificActivityDocument = $"{mResourceApi.GraphsUrl}items/scientificactivitydocument_SAD1";
                    }
                    break;
            }
        }

        /// <summary>
        /// Permite cargar los recursos.
        /// </summary>
        /// <param name="pListaRecursosCargar">Lista de recursos a cargar.</param>
        private static List<string> CargarDatos(List<ComplexOntologyResource> pListaRecursosCargar)
        {
            ConcurrentBag<string> idsItems = new ConcurrentBag<string>();
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
                    string id = "";
                    if (pListaRecursosCargar.Last() == recursoCargar)
                    {
                        id = mResourceApi.LoadComplexSemanticResource(recursoCargar, false, true);
                    }
                    else
                    {
                        id = mResourceApi.LoadComplexSemanticResource(recursoCargar);
                    }
                    if (recursoCargar.Uploaded)
                    {
                        idsItems.Add(id);
                    }
                }
            });
            return idsItems.Distinct().ToList();
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
                return resultadoQuery.results.bindings.First()["crisIdentifier"].value;
            }

            return string.Empty;
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
            string select = $"SELECT ?userKeywords";
            string where = $@"WHERE {{
                                <{pId}> <http://w3id.org/roh/userKeywords> ?userKeywords
                            }}";

            SparqlObject resultadoQuery = mResourceApi.VirtuosoQueryMultipleGraph(select, where, new() { "document", "researchobject" });
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
            string select = $"SELECT ?suggestedKeywords ";
            string where = $@"WHERE {{
                                <{pId}> <http://w3id.org/roh/suggestedKeywords> ?suggestedKeywords
                            }}";

            SparqlObject resultadoQuery = mResourceApi.VirtuosoQueryMultipleGraph(select, where, new() { "document", "researchobject" });
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
        public static Dictionary<string, List<string>> ObtenerUserKnowledgeAreaPublicacionResearchObject(string pId)
        {
            Dictionary<string, List<string>> listaCategorias = new Dictionary<string, List<string>>();
            string select = $"SELECT *";
            string where = $@"WHERE {{
                                <{pId}> <http://w3id.org/roh/userKnowledgeArea> ?userAreas.
                                ?userAreas <http://w3id.org/roh/categoryNode> ?nodo.
                            }}";
            SparqlObject resultado = mResourceApi.VirtuosoQueryMultipleGraph(select, where, new() { "document", "researchobject" });


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
            string select = $"SELECT *";
            string where = $@"WHERE {{
                                <{pId}> <http://w3id.org/roh/suggestedKnowledgeArea> ?userAreas.
                                ?userAreas <http://w3id.org/roh/categoryNode> ?nodo.
                            }}";
            SparqlObject resultado = mResourceApi.VirtuosoQueryMultipleGraph(select, where, new() { "document", "researchobject" });

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
        public static List<string> ObtenerProjectPublicacionResearchObject(string pId)
        {
            List<string> listaIds = new List<string>();

            // Consulta sparql.
            string select = $"SELECT ?project";
            string where = $@"WHERE {{
                                <{pId}> <http://w3id.org/roh/project> ?project
                            }}";

            SparqlObject resultadoQuery = mResourceApi.VirtuosoQueryMultipleGraph(select, where, new() { "document", "researchobject" });
            if (resultadoQuery != null && resultadoQuery.results != null && resultadoQuery.results.bindings != null && resultadoQuery.results.bindings.Count > 0)
            {
                foreach (Dictionary<string, SparqlObject.Data> fila in resultadoQuery.results.bindings)
                {
                    listaIds.Add(fila["project"].value);
                }
            }

            return listaIds;
        }


        /// <summary>
        /// Consulta en SPARQL si existe el documento con el estado.
        /// </summary>
        /// <param name="pId">ID del recurso.</param>
        /// <returns></returns>
        public static string ObtenerAssessmentStatusPublicacionResearchObject(string pId)
        {
            // Consulta sparql.
            string select = $"SELECT ?estado ";
            string where = $@"WHERE {{
                                <{pId}> <http://w3id.org/roh/assessmentStatus> ?estado
                            }}";

            SparqlObject resultadoQuery = mResourceApi.VirtuosoQueryMultipleGraph(select, where, new() { "document", "researchobject" });
            if (resultadoQuery != null && resultadoQuery.results != null && resultadoQuery.results.bindings != null && resultadoQuery.results.bindings.Count > 0)
            {
                return resultadoQuery.results.bindings.First()["estado"].value;
            }

            return string.Empty;
        }

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

                // Consulta sparql.
                while (true)
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
                            break;
                        }
                    }
                    else
                    {
                        break;
                    }
                }
            }
            if (ISSN_Revista.ContainsKey(pISSN.Trim().ToLower()))
            {
                return ISSN_Revista[pISSN.Trim().ToLower()];
            }
            return string.Empty;
        }

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

                // Consulta sparql.
                while (true)
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
                            break;
                        }
                    }
                    else
                    {
                        break;
                    }
                }
            }
            if (EISSN_Revista.ContainsKey(pEissn.Trim().ToLower()))
            {
                return EISSN_Revista[pEissn.Trim().ToLower()];
            }
            return string.Empty;
        }

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

                // Consulta sparql.
                while (true)
                {
                    string select = "SELECT * WHERE { SELECT ?revista ?titulo ";
                    string where = $@"WHERE {{
                                ?revista a <http://w3id.org/roh/MainDocument>. 
                                ?revista <http://w3id.org/roh/title> ?titulo. 
                            }} ORDER BY DESC(?revista) }} LIMIT {limit} OFFSET {offset}";

                    SparqlObject resultadoQuery = mResourceApi.VirtuosoQuery(select, where, "maindocument");
                    if (resultadoQuery != null && resultadoQuery.results != null && resultadoQuery.results.bindings != null && resultadoQuery.results.bindings.Count == 1)
                    {
                        offset += limit;
                        foreach (Dictionary<string, SparqlObject.Data> fila in resultadoQuery.results.bindings)
                        {
                            Titulo_Revista[fila["titulo"].value.ToLower()] = fila["revista"].value;
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
            }
            if (Titulo_Revista.ContainsKey(pTitulo.Trim().ToLower()))
            {
                return Titulo_Revista[pTitulo.Trim().ToLower()];
            }
            return string.Empty;
        }

        /// <summary>
        /// Permite crear un archivo Zip de un único fichero.
        /// </summary>
        /// <param name="pRutaEscritura">Ruta dónde se va a guardar el archivo zip.</param>
        /// <param name="pNombreFichero">Nombre del fichero.</param>
        /// <param name="pData">Datos a guardar.</param>
        private static void CrearZip(string pRutaEscritura, string pNombreFichero, string pData)
        {
            using (FileStream zipToOpen = new FileStream($@"{pRutaEscritura}/{pNombreFichero.Split('.')[0]}.zip", FileMode.Create))
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

            // ?concept <http://purl.org/dc/elements/1.1/identifier> ?id.

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
        /// Obtiene el objeto para desambiguar publicaciones.
        /// </summary>
        /// <param name="pPublicacion">Publicación a convertir.</param>
        /// <returns>Objeto para desambiguar.</returns>
        private static DisambiguationPublication GetDisambiguationPublication(Publication pPublicacion)
        {
            pPublicacion.ID = Guid.NewGuid().ToString();
            string wosIdValue = "";
            string scopusIdValue = "";
            
            // Se comprueba que los IDs no son nulos.
            if (pPublicacion.iDs != null)
            {
                // Se comprueba que contiene identificador WoS.
                if (pPublicacion.iDs.FirstOrDefault(x => x.ToLower().Contains("wos")) != null
                    && pPublicacion.iDs.FirstOrDefault(x => x.ToLower().Contains("wos")).Any())
                {
                    wosIdValue = pPublicacion.iDs.First(x => x.ToLower().Contains("wos")).Split(":")[1].Trim();
                }

                // Se comprueba que contiene identificador Scopus.
                if (pPublicacion.iDs.FirstOrDefault(x => x.ToLower().Contains("scopus")) != null
                    && pPublicacion.iDs.FirstOrDefault(x => x.ToLower().Contains("scopus")).Any())
                {
                    scopusIdValue = pPublicacion.iDs.First(x => x.ToLower().Contains("scopus")).Split(":")[1].Trim();
                }
            }

            DisambiguationPublication pub = new DisambiguationPublication()
            {
                ID = pPublicacion.ID,
                doi = pPublicacion.doi,
                title = pPublicacion.title,
                wosId = wosIdValue,
                scopusId = scopusIdValue
            };

            return pub;
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

        /// <summary>
        /// Permite borrar y cargar un documento existente con datos actualizados.
        /// </summary>
        /// <param name="pPublicacion">Publicación con los datos a cargar.</param>
        /// <param name="pIdDocumento">ID del recurso.</param>
        /// <param name="pDicAreasBroader">Diccionario con los hijos.</param>
        /// <param name="pDicAreasNombre">Diccionario con las áreas temáticas.</param>        
        /// <returns></returns>
        public static void ModificarDocumento(Document pDocument, string pIdDocumento)
        {
            // Recuperación del Crisidentifier
            pDocument.Roh_crisIdentifier = ObtenerCrisIdentifierPublicacion(pIdDocumento);

            // Recuperación de las UserKeywords
            pDocument.Roh_userKeywords = ObtenerUserKeywordsPublicacionResearchObject(pIdDocumento);

            // Recuperación de las SuggestedKeywords
            pDocument.Roh_suggestedKeywords = ObtenerSuggestedKeywordsPublicacionResearchObject(pIdDocumento);

            // Recuperación del Project
            pDocument.IdsRoh_project = ObtenerProjectPublicacionResearchObject(pIdDocumento);
        }

        /// <summary>
        /// Permite borrar y cargar un researchobject existente con datos actualizados.
        /// </summary>
        /// <param name="pResearchObject">researchobject con los datos a cargar.</param>
        /// <param name="pIdResearchObject">ID del recurso.</param>
        /// <param name="pDicAreasBroader">Diccionario con los hijos.</param>
        /// <param name="pDicAreasNombre">Diccionario con las áreas temáticas.</param>        
        /// <returns></returns>
        public static void ModificarRO(ResearchobjectOntology.ResearchObject pResearchObject, string pIdResearchObject)
        {
            // Recuperación de las UserKeywords
            pResearchObject.Roh_userKeywords = ObtenerUserKeywordsPublicacionResearchObject(pIdResearchObject);

            // Recuperación de las SuggestedKeywords
            pResearchObject.Roh_suggestedKeywords = ObtenerSuggestedKeywordsPublicacionResearchObject(pIdResearchObject);

            // Recuperación del Project
            pResearchObject.IdRoh_project = ObtenerProjectPublicacionResearchObject(pIdResearchObject).First();
        }




        /// <summary>
        /// Consulta para obtener los autores de un documento
        /// </summary>
        /// <param name="lista">Listado de los documentos</param>
        /// <param name="limit">Límite de resultados</param>
        /// <param name="offset">Ofset de resultados</param>
        /// <returns>Devuelve Sparql Object</returns>
        private static SparqlObject GetAutorsFromDocumentQuery(List<string> lista, int limit, int offset, string type, string varType)
        {
            string select = "SELECT * WHERE { SELECT DISTINCT ?"+varType+" ?autor ";
            string where = $@"WHERE {{
                                ?{varType} a <http://purl.org/ontology/bibo/Document>. 
                                ?{varType} <http://purl.org/ontology/bibo/authorList> ?listaAutores. 
                                ?listaAutores <http://www.w3.org/1999/02/22-rdf-syntax-ns#member> ?autor. 
                                FILTER(?{varType} in (<{string.Join(">,<", lista)}>)) 
                            }} ORDER BY DESC(?{varType}) }} LIMIT {limit} OFFSET {offset}";

            return mResourceApi.VirtuosoQuery(select, where, type);
        }
    }
}
