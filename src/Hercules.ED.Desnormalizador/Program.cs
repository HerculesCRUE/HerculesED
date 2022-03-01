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
            ActualizadorEDMA.FusionNombre(new List<string> { "skarmeta" });
            ActualizadorEDMA.FusionNombre(new List<string> { "oscar", "canovas" });
            ActualizadorEDMA.FusionNombre(new List<string> { "jorge", "bernal" });
            ActualizadorEDMA.FusionNombre(new List<string> { "alberto", "caballero" });
            ActualizadorEDMA.FusionNombre(new List<string> { "garcia", "sola" });
            ActualizadorEDMA.FusionNombre(new List<string> { "garcia", "carrillo" });
            ActualizadorEDMA.FusionNombre(new List<string> { "felix", "garcia", "clemente" });
            ActualizadorEDMA.FusionNombre(new List<string> { "elena", "garcia", "torroglosa" });
            ActualizadorEDMA.FusionNombre(new List<string> { "t", "garcia", "valverde" });            
            ActualizadorEDMA.FusionNombre(new List<string> { "pedro", "garcia", "lopez" });
            ActualizadorEDMA.FusionNombre(new List<string> { "fernando", "jimenez" });


            bool eliminarDatos = false;
            if (eliminarDatos)
            {
                ActualizadorEDMA.EliminarDatosDesnormalizados();
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
            //Antonio Skaremta 28710458
            AltaUsuarioGnoss("Antonio", "Skarmeta", "antonio--skarmeta@pruebagnoss.com", "skarmeta22", "28710458", "AdrianSaavedra-GNOSS", "12070100");

            //Manuel Campos 34822542
            AltaUsuarioGnoss("Manuel", "Campos", "manuel--campos@pruebagnoss.com", "manuel-camp2", "34822542", "manuelCampos-github", "22222222");

            //Francisco Esquembre 27443184
            AltaUsuarioGnoss("Francisco", "Esquembre", "francisco--esquembre@pruebagnoss.com", "francisco-es", "27443184", "franciscoEsquembre-github", "33333333");
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
