using DesnormalizadorHercules.Models.Actualizadores;
using DesnormalizadorHercules.Models.Services;
using Gnoss.ApiWrapper;
using Gnoss.ApiWrapper.ApiModel;
using Gnoss.ApiWrapper.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DesnormalizadorHercules
{
    public static class Temporal
    {
        private readonly static string rutaOauth = $@"{System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase}Config/ConfigOAuth/OAuthV3.config";
        private static ResourceApi resourceApi = new ResourceApi(rutaOauth);
        private static UserApi userApi = new UserApi(rutaOauth);
        private static CommunityApi communityApi = new CommunityApi(rutaOauth);

        public static void CrearPersonas()
        {

            //Antonio Skaremta-->skarmeta22
            //Manuel Campos-->manuel-camp2
            //Francisco Esquembre-->francisco-es
            //Jose Tomas Palma Mendez-->jose-tomas
            //Diana Castilla-->diana-castil
            //Felix Cesareo Gomez de Leon Hijes-->felix-cesare
            //Fernando Jimenez Barrionuevo-->fernando-jim
            //Gracia Sanchez Carpena-->gracia-sanch
            //Jose Manuel Juarez Herrero-->jose-juarez
            //Maria Antonia Cardenas Viedma-->maria-carden


            //http://gnoss.com/items/Person_ad372791-7287-425d-8238-83931a5d9818_b06212ce-8c39-4608-8542-ec97463a6fdb
            //8310
            //31248453

            //Dictionary<Guid, List<TriplesToModify>> triples = new Dictionary<Guid, List<TriplesToModify>>();
            //TriplesToModify t = new TriplesToModify("true", "false", "http://w3id.org/roh/isActive");
            //triples.Add(new Guid("eb4c7a6c-09af-4f35-b932-84c9881d6daa"), new List<TriplesToModify>() { t });
            //resourceApi.ModifyPropertiesLoadedResources(triples);
            ////http://gnoss.com/items/Person_eb4c7a6c-09af-4f35-b932-84c9881d6daa_7abc78ac-8bad-444f-bfeb-450f9fe0c1ba

            /*
            List<RemoveTriples> triplesRemove = new();
            
                triplesRemove.Add(new RemoveTriples()
                {
                    Predicate = "http://w3id.org/roh/ORCID",
                    Value = "0000-0001-5844-4163"
                });
            if (triplesRemove.Count > 0)
            {
                var resultadox = resourceApi.DeletePropertiesLoadedResources(new Dictionary<Guid, List<Gnoss.ApiWrapper.Model.RemoveTriples>>() { { resourceApi.GetShortGuid("http://gnoss.com/items/Person_6b4f5547-1691-48c0-a42e-8b8c59733eda_7ebe180a-d766-461c-8aa9-b1f215eab90d>"), triplesRemove } });
            }*/

            /*List<string> notificaciones = resourceApi.VirtuosoQuery("select *", $@"where
                                                                                            {{
                                                                                                ?s a <http://w3id.org/roh/Notification>
                                                                                            }}", "notification").results.bindings.Select(x=>x["s"].value).ToList();

            foreach(string id in notificaciones)
            {
                try
                {
                    Guid id2 = resourceApi.GetShortGuid(id);
                    if (id2 != Guid.Empty)
                    {
                        resourceApi.PersistentDelete(id2);
                    }
                }catch(Exception)
                {
                    List<string> ids = new List<string>() { id };
                    resourceApi.DeleteSecondaryEntitiesList(ref ids);
                }
            }*/

            string cv = "http://gnoss.com/items/CV_693c2f58-d466-4ff5-841a-34f51499efe2_997709d4-b8d6-49ae-9504-aad506f9bbd9";
            List<string> filesPDF = resourceApi.VirtuosoQuery("select *", $@"where
                                                                                    {{
                                                                                        ?s <http://w3id.org/roh/generatedPDFFile> ?gpdff.
                                                                                        FILTER(?s=<{cv}>)
                                                                                    }}", "curriculumvitae").results.bindings.Select(x => x["gpdff"].value).ToList();
            List<RemoveTriples> triplesRemove = filesPDF.Select(x => new RemoveTriples() { Predicate = "http://w3id.org/roh/generatedPDFFile", Value = x }).ToList();
            if (triplesRemove.Count > 0)
            {
                var resultadox = resourceApi.DeletePropertiesLoadedResources(new Dictionary<Guid, List<Gnoss.ApiWrapper.Model.RemoveTriples>>() { { resourceApi.GetShortGuid(cv), triplesRemove } });
            }

            //Antonio Skaremta 28710458
            AltaUsuarioGnoss("Antonio", "Skarmeta", "antonio--skarmeta@pruebagnoss.com", "skarmeta22", "28710458", "AdrianSaavedra-GNOSS", "12070100");

            //Manuel Campos 34822542
            AltaUsuarioGnoss("Manuel", "Campos", "manuel--campos@pruebagnoss.com", "manuel-camp2", "34822542", "manuelCampos-github", "22222222");

            //Francisco Esquembre 27443184
            AltaUsuarioGnoss("Francisco", "Esquembre", "francisco--esquembre@pruebagnoss.com", "francisco-es", "27443184", "franciscoEsquembre-github", "33333333");

            //José Tomás 8310
            AltaUsuarioGnoss("Jose", "Tomas", "jose---tomas@pruebagnoss.com", "jose-tomas", "31248453", "", "");

            //Diana Castilla 27281387213879
            AltaUsuarioGnoss("Diana", "Castilla", "diana---castilla@pruebagnoss.com", "diana-castil", "27281387213879", "", "");


            //Felix Cesareo Gomez de Leon Hijes
            AltaUsuarioGnoss("Felix", "Cesareo", "Felix--Cesareo@pruebagnoss.com", "felix-cesare", "22463209", "", "");

            //Fernando Jimenez Barrionuevo
            AltaUsuarioGnoss("Fernando", "Jimenez", "Fernando---Jimenez@pruebagnoss.com", "fernando-jim", "29084098", "", "");

            //Gracia Sanchez Carpena
            AltaUsuarioGnoss("Gracia", "Sanchez", "Gracia---Sanchez@pruebagnoss.com", "gracia-sanch", "22144772", "", "");

            //Jose Manuel Juarez Herrero
            AltaUsuarioGnoss("Jose", "Juarez", "Jose---Juarez@pruebagnoss.com", "jose-juarez", "48479115", "", "");

            //Maria Antonia Cardenas Viedma
            AltaUsuarioGnoss("Maria", "Cardenas", "Maria---Cardenas@pruebagnoss.com", "maria-carden", "26476225", "", "");

            //Elena Garcia Barriocanal
            AltaUsuarioGnoss("Elena", "Garcia Barriocanal", "elena--garcia@pruebagnoss.com", "elena-garcia", "11335577992468", "", "");

            //Miguel Ángel Sicilia
            AltaUsuarioGnoss("Miguel Angel", "Sicilia", "miguel--sicilia@pruebagnoss.com", "miguel-angel", "224466880013579", "", "");

            //Marçal Mora Cantallops
            AltaUsuarioGnoss("Marcal", "Mora Cantallops", "marcal--mora@pruebagnoss.com", "marcal-mora-", "113355112223334", "", "");

            //Juan Manuel Dodero
            AltaUsuarioGnoss("Juan", "Manuel Dodero", "juan--manuel@pruebagnoss.com", "juan-manuel-", "31256195", "", "");

            //Andres Muñoz Ortega
            AltaUsuarioGnoss("Andres", "Munoz Ortega", "andres--munoz@pruebagnoss.com", "andres-munoz", "48466315", "", "");

            //Daniela Fernandez
            AltaUsuarioGnoss("Daniela", "Fernandez", "daniela--fernandez@pruebagnoss.com", "daniela-fern", "113636170221114", "", "");

            //Daniela Fernandez
            AltaUsuarioGnoss("Daniela", "Fernandez", "daniela--fernandez@pruebagnoss.com", "daniela-fern", "113636170221114", "", "");

            //Isabel Hernández García
            AltaUsuarioGnoss("Isabel", "Hernandez", "isabel--hernandez@pruebagnoss.com", "isabel-herna", "74336159", "", "");

            //Pedro Martín Peña
            AltaUsuarioGnoss("Pedro", "Martin Pena", "sugerencias.cvn@pruebagnoss.es", "pedro-martin", "46462369", "", "");

            //Test Fecyt Vacio
            AltaUsuarioGnoss("Test", "Fecyt Vacio", "test-fecyt-vacio@pruebagnoss.com", "test-fecyt-v", "22559987903", "", "");

            AltaUsuarioGnoss("Joaquin", "Tribaldos", "joaquin.tribaldos@fecyt.es", "joaquin-trib", "235312462347", "", "");

            AltaUsuarioGnoss("Alfonso", "Lopez", "alfonso.lopez@fecyt.es", "alfonso-lope", "712345634685", "", "");

            AltaUsuarioGnoss("Antonio", "Sanchez", "Antonio.sanchez@fecyt.es", "antonio-sanc", "346244756785", "", "");

            AltaUsuarioGnoss("Estefania", "Guitierrez", "estefania.gutierrez@fecyt.es", "estefania-gu", "324625873546", "", "");

            AltaUsuarioGnoss("Aurelia", "Andres", "aurelia.andres@fecyt.es", "aurelia-and1", "579865434363", "", "");
        }

        public static User AltaUsuarioGnoss(string pNombre, string pApellidos, string pEmail, string pNombreCorto, string pID, string pUsuarioGitHub, string pUsuarioFigShare)
        {
            User user = userApi.GetUserByShortName(pNombreCorto);

            if (user == null)
            {
                user = new User();
                try
                {
                    user.name = pNombre;
                    user.last_name = pApellidos;
                    user.email = pEmail;
                    user.password = "123gnoss";
                    user.community_short_name = "hercules";
                    user.user_short_name = pNombreCorto;
                    user.user_id = Guid.NewGuid();
                    user = userApi.CreateUser(user);
                }
                catch (Exception ex)
                {
                }
            }
            //Vincular con la persona
            //Obtenemos la persona
            Dictionary<string, SparqlObject.Data> fila = resourceApi.VirtuosoQuery("select *", $@"where
                                                                                            {{
                                                                                                ?s <http://w3id.org/roh/crisIdentifier> '{pID}'. 
                                                                                                OPTIONAL{{?s <http://w3id.org/roh/gnossUser> ?user }}
                                                                                                OPTIONAL{{?s <http://w3id.org/roh/usuarioGitHub> ?userGit }}
                                                                                                OPTIONAL{{?s <http://w3id.org/roh/usuarioFigShare> ?userFigShare }}
                                                                                            }}", "person").results.bindings.First();
            string idPerona = fila["s"].value;

            Dictionary<string, string> dicPropiedadValorActual = new Dictionary<string, string>();
            Dictionary<string, string> dicPropiedadValorCargar = new Dictionary<string, string>();
            //USER
            dicPropiedadValorActual["http://w3id.org/roh/gnossUser"] = "";
            if (fila.ContainsKey("user"))
            {
                dicPropiedadValorActual["http://w3id.org/roh/gnossUser"] = fila["user"].value;
            }
            dicPropiedadValorCargar["http://w3id.org/roh/gnossUser"] = "http://gnoss/" + user.user_id.ToString().ToUpper();
            //USERGit
            dicPropiedadValorActual["http://w3id.org/roh/usuarioGitHub"] = "";
            if (fila.ContainsKey("userGit"))
            {
                dicPropiedadValorActual["http://w3id.org/roh/usuarioGitHub"] = fila["userGit"].value;
            }
            dicPropiedadValorCargar["http://w3id.org/roh/usuarioGitHub"] = pUsuarioGitHub;
            //USERFigShare
            dicPropiedadValorActual["http://w3id.org/roh/usuarioFigShare"] = "";
            if (fila.ContainsKey("userFigShare"))
            {
                dicPropiedadValorActual["http://w3id.org/roh/usuarioFigShare"] = fila["userFigShare"].value;
            }
            dicPropiedadValorCargar["http://w3id.org/roh/usuarioFigShare"] = pUsuarioFigShare;

            foreach (string prop in dicPropiedadValorCargar.Keys)
            {
                if (!string.IsNullOrEmpty(dicPropiedadValorCargar[prop]) && string.IsNullOrEmpty(dicPropiedadValorActual[prop]))
                {
                    //Insertamos
                    Dictionary<Guid, List<TriplesToInclude>> triples = new() { { resourceApi.GetShortGuid(idPerona), new List<TriplesToInclude>() } };
                    TriplesToInclude t = new();
                    t.Predicate = prop;
                    t.NewValue = dicPropiedadValorCargar[prop];
                    triples[resourceApi.GetShortGuid(idPerona)].Add(t);
                    var resultado = resourceApi.InsertPropertiesLoadedResources(triples);
                }
                else if (!string.IsNullOrEmpty(dicPropiedadValorCargar[prop]) &&
                   !string.IsNullOrEmpty(dicPropiedadValorActual[prop]) &&
                   dicPropiedadValorCargar[prop] != dicPropiedadValorActual[prop])
                {
                    //Modificamos
                    //Si el valor nuevo y el viejo no son nulos -->modificamos
                    TriplesToModify t = new();
                    t.NewValue = dicPropiedadValorCargar[prop];
                    t.OldValue = dicPropiedadValorActual[prop];
                    t.Predicate = prop;
                    var resultado = resourceApi.ModifyPropertiesLoadedResources(new Dictionary<Guid, List<Gnoss.ApiWrapper.Model.TriplesToModify>>() { { resourceApi.GetShortGuid(idPerona), new List<Gnoss.ApiWrapper.Model.TriplesToModify>() { t } } });
                }
            }

            //Cambio correo skarmeta
            if (pID == "28710458")
            {
                Dictionary<Guid, List<TriplesToModify>> triples = new Dictionary<Guid, List<TriplesToModify>>();
                TriplesToModify t = new TriplesToModify("alvaro.palacios@um.es", "skarmeta@um.es", "https://www.w3.org/2006/vcard/ns#email");
                triples.Add(resourceApi.GetShortGuid(idPerona), new List<TriplesToModify>() { t });
                resourceApi.ModifyPropertiesLoadedResources(triples);
            }

            //Cambio correo Isabel Hernández García
            if (pID == "74336159")
            {
                Dictionary<Guid, List<TriplesToModify>> triples = new Dictionary<Guid, List<TriplesToModify>>();
                TriplesToModify t = new TriplesToModify("isabel.m.h@um.es", "isabelhg@um.es", "https://www.w3.org/2006/vcard/ns#email");
                triples.Add(resourceApi.GetShortGuid(idPerona), new List<TriplesToModify>() { t });
                resourceApi.ModifyPropertiesLoadedResources(triples);
            }

            return user;
        }

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

                    Parallel.ForEach(resultado.results.bindings, new ParallelOptions { MaxDegreeOfParallelism = 10 }, fila =>
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

        public static void EliminarEntidadesCV()
        {
            String select = @"SELECT ?cv ?pLvl1 ?oLvl1 ?pLvl2 ?oLvl2 ?entity";
            String where = @$"  where{{
                                            ?cv a <http://w3id.org/roh/CV>.
                                            ?cv ?pLvl1 ?oLvl1.
                                            ?oLvl1 ?pLvl2 ?oLvl2.
                                            ?oLvl2 <http://vivoweb.org/ontology/core#relatedBy> ?entity.
                                            FILTER(?cv =<http://gnoss.com/items/CV_693c2f58-d466-4ff5-841a-34f51499efe2_997709d4-b8d6-49ae-9504-aad506f9bbd9>)
                                        }}";

            SparqlObject resultado = resourceApi.VirtuosoQuery(select, where, "curriculumvitae");

            List<Guid> eliminar = new List<Guid>();
            Dictionary<Guid, List<RemoveTriples>> dicEliminar = new Dictionary<Guid, List<RemoveTriples>>();
            foreach (Dictionary<string, SparqlObject.Data> fila in resultado.results.bindings)
            {
                eliminar.Add(resourceApi.GetShortGuid( fila["entity"].value));
                Guid idCV = resourceApi.GetShortGuid(fila["cv"].value);
                if(!dicEliminar.ContainsKey(idCV))
                {
                    dicEliminar.Add(idCV,new List<RemoveTriples>());
                }
                RemoveTriples remove = new RemoveTriples(fila["oLvl1"].value + "|" + fila["oLvl2"].value, fila["pLvl1"].value + "|" + fila["pLvl2"].value);
                dicEliminar[idCV].Add(remove);
            }
            Dictionary<Guid,bool> resp=resourceApi.DeletePropertiesLoadedResources(dicEliminar);

            foreach(Guid elim in eliminar)
            {
                try
                {
                    resourceApi.PersistentDelete(elim);
                }catch(Exception ex)
                {

                }
            }

        }

        public static void InsertarColaDesnormalizador(RabbitServiceWriterDenormalizer rabbitService, string queue)
        {
            List<Tuple<DenormalizerItemQueue.ItemType, string, string>> cargar = new List<Tuple<DenormalizerItemQueue.ItemType, string, string>>();
            cargar.Add(new Tuple<DenormalizerItemQueue.ItemType, string, string>(Models.Services.DenormalizerItemQueue.ItemType.document, "document", "http://purl.org/ontology/bibo/Document"));
            cargar.Add(new Tuple<DenormalizerItemQueue.ItemType, string, string>(Models.Services.DenormalizerItemQueue.ItemType.group, "group", "http://xmlns.com/foaf/0.1/Group"));
            cargar.Add(new Tuple<DenormalizerItemQueue.ItemType, string, string>(Models.Services.DenormalizerItemQueue.ItemType.person, "person", "http://xmlns.com/foaf/0.1/Person"));
            cargar.Add(new Tuple<DenormalizerItemQueue.ItemType, string, string>(Models.Services.DenormalizerItemQueue.ItemType.project, "project", "http://vivoweb.org/ontology/core#Project"));
            cargar.Add(new Tuple<DenormalizerItemQueue.ItemType, string, string>(Models.Services.DenormalizerItemQueue.ItemType.researchobject, "researchobject", "http://w3id.org/roh/ResearchObject"));


            foreach (Tuple<DenormalizerItemQueue.ItemType, string, string> tupla in cargar)
            {
                SparqlObject sparqlObject = resourceApi.VirtuosoQuery("select ?s", $"where{{?s a <{tupla.Item3}>}}", tupla.Item2);

                List<string> items = new List<string>();
                foreach (Dictionary<string, SparqlObject.Data> fila in sparqlObject.results.bindings)
                {
                    items.Add(fila["s"].value);
                }
                List<List<string>> listItems = DesnormalizadorHercules.Models.Actualizadores.ActualizadorBase.SplitList(items, 100).ToList();
                foreach (List<string> itemsIn in listItems)
                {
                    DenormalizerItemQueue item = new DenormalizerItemQueue(tupla.Item1, new HashSet<string>(itemsIn));
                    rabbitService.PublishMessage(item);
                }
            }


        }

        public static void ActualizarPertenenciaDocumentosProyectos()
        {
            ActualizadorDocument actualizadorDocument = new ActualizadorDocument(resourceApi);

            HashSet<string> filters = new HashSet<string>();
            if (filters.Count == 0)
            {
                filters.Add("");
            }

            foreach (string filter in filters)
            {
                while (true)
                {
                    //Añadimos a documentos
                    int limit = 500;
                    //TODO eliminar from
                    String select = @"select distinct ?doc max(?project) as ?project  from <http://gnoss.com/curriculumvitae.owl>  from <http://gnoss.com/person.owl> from <http://gnoss.com/project.owl>  ";
                    String where = @$"where{{
                                    {filter}
                                    {{
                                        select distinct ?project as ?project ?doc
                                        Where{{
                                            ?project a <http://vivoweb.org/ontology/core#Project>.
                                            ?project <http://vivoweb.org/ontology/core#relates> ?members.
                                            ?members <http://w3id.org/roh/roleOf> ?person.
                                            ?person a <http://xmlns.com/foaf/0.1/Person>.
                                            OPTIONAL{{?members <http://vivoweb.org/ontology/core#start> ?fechaPersonaInit.}}
                                            OPTIONAL{{?members <http://vivoweb.org/ontology/core#end> ?fechaPersonaEnd.}}
                                            BIND(IF(bound(?fechaPersonaEnd), xsd:integer(?fechaPersonaEnd), 30000000000000) as ?fechaPersonaEndAux)
                                            BIND(IF(bound(?fechaPersonaInit), xsd:integer(?fechaPersonaInit), 10000000000000) as ?fechaPersonaInitAux)
                                            ?doc a <http://purl.org/ontology/bibo/Document>.
                                            ?doc <http://w3id.org/roh/isValidated> 'true'.
                                            ?doc <http://purl.org/ontology/bibo/authorList> ?autores.
                                            ?autores <http://www.w3.org/1999/02/22-rdf-syntax-ns#member> ?person.
                                            ?doc <http://purl.org/dc/terms/issued> ?fechaPublicacion.
                                            FILTER(xsd:integer(?fechaPublicacion)>= ?fechaPersonaInitAux AND xsd:integer(?fechaPublicacion)<= ?fechaPersonaEndAux)
                                        }}
                                    }}
                                    MINUS
                                    {{
                                        ?doc a <http://purl.org/ontology/bibo/Document>.
                                        ?doc <http://w3id.org/roh/project> ?X.
                                    }}
                                }}order by desc(?doc) limit {limit}";
                    SparqlObject resultado = resourceApi.VirtuosoQuery(select, where, "document");
                    actualizadorDocument.InsercionMultiple(resultado.results.bindings, "http://w3id.org/roh/project", "doc", "project");
                    if (resultado.results.bindings.Count != limit)
                    {
                        break;
                    }
                }
            }
        }


    }
}
