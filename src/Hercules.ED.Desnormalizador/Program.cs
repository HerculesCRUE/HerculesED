using DesnormalizadorHercules.Models;
using Gnoss.ApiWrapper;
using Gnoss.ApiWrapper.ApiModel;
using Gnoss.ApiWrapper.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;

namespace DesnormalizadorHercules
{
    class Program
    {
        static void Main()
        {
            //TODO eliminar
            CrearPersonas();

            bool eliminarCVs = false;
            if (eliminarCVs)
            {
                ActualizadorEDMA.EliminarCVs();
            }
            while (true)
            {
                try
                {

                    ActualizadorEDMA.DesnormalizarTodo();
                }
                catch (Exception)
                {

                }
                Thread.Sleep(10000);
            }

        }

        private static void CrearPersonas()
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

            string rutaOauth = $@"{System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase}Config/OAuthV3.config";
            UserApi userApi = new UserApi(rutaOauth);
            ResourceApi resourceApi = new ResourceApi(rutaOauth);

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
        }

        public static User AltaUsuarioGnoss(string pNombre, string pApellidos, string pEmail, string pNombreCorto,string pID, string pUsuarioGitHub,string pUsuarioFigShare)
        {
            string rutaOauth = $@"{System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase}Config/OAuthV3.config";
            UserApi userApi = new UserApi(rutaOauth);
            ResourceApi resourceApi = new ResourceApi(rutaOauth);

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
                if(!string.IsNullOrEmpty(dicPropiedadValorCargar[prop]) && string.IsNullOrEmpty(dicPropiedadValorActual[prop]))
                {
                    //Insertamos
                    Dictionary<Guid, List<TriplesToInclude>> triples = new() { { resourceApi.GetShortGuid(idPerona), new List<TriplesToInclude>() } };
                    TriplesToInclude t = new();
                    t.Predicate = prop;
                    t.NewValue = dicPropiedadValorCargar[prop];
                    triples[resourceApi.GetShortGuid(idPerona)].Add(t);
                    var resultado = resourceApi.InsertPropertiesLoadedResources(triples);
                }else if(!string.IsNullOrEmpty(dicPropiedadValorCargar[prop]) && 
                    !string.IsNullOrEmpty(dicPropiedadValorActual[prop]) &&
                    dicPropiedadValorCargar[prop]!= dicPropiedadValorActual[prop])
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

            return user;
        }
    }
}
