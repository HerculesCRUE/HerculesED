using EditorCV.Controllers;
using EditorCV.Models.EnvioPRC;
using Gnoss.ApiWrapper;
using Gnoss.ApiWrapper.ApiModel;
using Gnoss.ApiWrapper.Model;
using Hercules.MA.ServicioExterno.Controllers.Utilidades;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EditorCV.Models
{
    public class AccionesEnvioPRC
    {
        #region --- Constantes   
        private static string RUTA_OAUTH = $@"{System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase}Config/ConfigOAuth/OAuthV3.config";
        private static ResourceApi mResourceApi = new ResourceApi(RUTA_OAUTH);
        private static string RUTA_PREFIJOS = $@"{System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase}Models/Utils/prefijos.json";
        private static string mPrefijos = string.Join(" ", JsonConvert.DeserializeObject<List<string>>(File.ReadAllText(RUTA_PREFIJOS)));
        #endregion

        private static Dictionary<string, string> dicPropiedadesPublicaciones = new Dictionary<string, string>();
        private static Dictionary<string, string> dicPropiedadesCongresos = new Dictionary<string, string>();

        /// <summary>
        /// Permite enviar a Producción Científica los datos necesarios para la validación.
        /// </summary>
        /// <param name="pId">ID del documento.</param>
        public void EnvioPRC(ConfigService pConfig, string pIdDocumento, string pIdProyecto)
        {
            // Rellena el diccionario de propiedades.
            if (!dicPropiedadesPublicaciones.Any())
            {
                RellenarDiccionarioPublicaciones();
            }
            if (!dicPropiedadesCongresos.Any())
            {
                RellenarDiccionarioCongresos();
            }

            SparqlObject resultadoQuery = null;
            StringBuilder select = new StringBuilder(), where = new StringBuilder();

            ProduccionCientifica PRC = new ProduccionCientifica();

            // Identificador.
            PRC.idRef = pIdDocumento;
            PRC.estado = "PENDIENTE";
            PRC.campos = new List<CampoProduccionCientifica>();

            #region --- Tipo del Documento.
            // Consulta sparql (Tipo del documento).
            select.Append(mPrefijos);
            select.Append("SELECT DISTINCT ?tipoDocumento ");
            where.Append("WHERE { ");
            where.Append("?s a bibo:Document. ");
            where.Append("?s roh:scientificActivityDocument ?tipoDocumento. ");
            where.Append($@"FILTER(?s = <{pIdDocumento}>) ");
            where.Append("} ");

            resultadoQuery = mResourceApi.VirtuosoQuery(select.ToString(), where.ToString(), "document");

            if (resultadoQuery != null && resultadoQuery.results != null && resultadoQuery.results.bindings != null && resultadoQuery.results.bindings.Count > 0)
            {
                foreach (Dictionary<string, SparqlObject.Data> fila in resultadoQuery.results.bindings)
                {
                    string tipo = UtilidadesAPI.GetValorFilaSparqlObject(fila, "tipoDocumento");

                    switch (tipo)
                    {
                        case "http://gnoss.com/items/scientificactivitydocument_SAD1":
                            PRC.epigrafeCVN = "060.010.010.000";
                            break;
                        case "http://gnoss.com/items/scientificactivitydocument_SAD2":
                            PRC.epigrafeCVN = "060.010.020.000";
                            break;
                        case "http://gnoss.com/items/scientificactivitydocument_SAD3":
                            PRC.epigrafeCVN = "060.010.030.000";
                            break;
                        case "http://gnoss.com/items/scientificactivitydocument_SAD4":
                            PRC.epigrafeCVN = "060.010.040.000";
                            break;
                    }
                }
            }
            #endregion

            #region --- Autores.
            // Consulta sparql (Obtención de datos de la persona).
            select = new StringBuilder();
            where = new StringBuilder();

            select.Append(mPrefijos);
            select.Append("SELECT DISTINCT ?crisIdentifier ?orcid ?orden ?nombre ?apellidos ?firma ");
            where.Append("FROM <http://gnoss.com/person.owl> ");
            where.Append("WHERE { ");
            where.Append("?s a bibo:Document. ");
            where.Append("OPTIONAL{ ");
            where.Append("?s bibo:authorList ?listaAutores. ");
            where.Append("?listaAutores rdf:member ?persona. ");
            where.Append("?listaAutores rdf:comment ?orden. ");
            where.Append("?persona foaf:firstName ?nombre. ");
            where.Append("?persona foaf:lastName ?apellidos. ");
            where.Append("OPTIONAL{?persona roh:crisIdentifier ?crisIdentifier. } ");
            where.Append("OPTIONAL{?persona roh:ORCID ?orcid. } ");
            where.Append("OPTIONAL{?persona foaf:nick ?firma. } ");
            where.Append("} ");
            where.Append($@"FILTER(?s = <{pIdDocumento}>) ");
            where.Append("} ORDER BY ?orden ");

            resultadoQuery = mResourceApi.VirtuosoQuery(select.ToString(), where.ToString(), "document");

            if (resultadoQuery != null && resultadoQuery.results != null && resultadoQuery.results.bindings != null && resultadoQuery.results.bindings.Count > 0)
            {
                foreach (Dictionary<string, SparqlObject.Data> fila in resultadoQuery.results.bindings)
                {
                    if (PRC.autores == null)
                    {
                        PRC.autores = new List<Autor>();
                    }

                    Autor autor = new Autor();
                    autor.personaRef = UtilidadesAPI.GetValorFilaSparqlObject(fila, "crisIdentifier");
                    autor.firma = UtilidadesAPI.GetValorFilaSparqlObject(fila, "firma");
                    autor.nombre = UtilidadesAPI.GetValorFilaSparqlObject(fila, "nombre");
                    autor.apellidos = UtilidadesAPI.GetValorFilaSparqlObject(fila, "apellidos");
                    autor.orden = int.Parse(UtilidadesAPI.GetValorFilaSparqlObject(fila, "orden"));
                    autor.orcidId = UtilidadesAPI.GetValorFilaSparqlObject(fila, "orcid");
                    autor.ip = false; // No tenemos IP en los documentos.
                    PRC.autores.Add(autor);
                }
            }
            #endregion

            #region --- Inserción y obtención del Proyecto asociado.
            // Comprobar si está el triple.
            string idProyectoAux = string.Empty;
            select = new StringBuilder();
            where = new StringBuilder();

            select.Append(mPrefijos);
            select.Append("SELECT DISTINCT ?proyecto ");
            where.Append("WHERE { ");
            where.Append("?s a bibo:Document. ");
            where.Append("OPTIONAL{?s roh:projectAux ?proyecto. } ");
            where.Append($@"FILTER(?s = <{pIdDocumento}>) ");
            where.Append("} ");

            resultadoQuery = mResourceApi.VirtuosoQuery(select.ToString(), where.ToString(), "document");

            if (resultadoQuery != null && resultadoQuery.results != null && resultadoQuery.results.bindings != null && resultadoQuery.results.bindings.Count > 0)
            {
                foreach (Dictionary<string, SparqlObject.Data> fila in resultadoQuery.results.bindings)
                {
                    idProyectoAux = UtilidadesAPI.GetValorFilaSparqlObject(fila, "proyecto");
                }
            }

            mResourceApi.ChangeOntoly("document");
            Guid guid = mResourceApi.GetShortGuid(pIdDocumento);

            if (string.IsNullOrEmpty(idProyectoAux))
            {
                // Inserción.
                Insercion(guid, "http://w3id.org/roh/projectAux", pIdProyecto);
            }
            else
            {
                // Modificación.
                Modificacion(guid, "http://w3id.org/roh/projectAux", pIdProyecto, idProyectoAux);
            }

            // Consulta sparql (Obtención del ID del proyecto).
            select = new StringBuilder();
            where = new StringBuilder();

            select.Append(mPrefijos);
            select.Append("SELECT DISTINCT ?crisIdentifier ");
            where.Append("WHERE { ");
            where.Append("?s a vivo:Project. ");
            where.Append("OPTIONAL{?s roh:crisIdentifier ?crisIdentifier. } ");
            where.Append($@"FILTER(?s = <{pIdProyecto}>) ");
            where.Append("} ");

            resultadoQuery = mResourceApi.VirtuosoQuery(select.ToString(), where.ToString(), "project");

            if (resultadoQuery != null && resultadoQuery.results != null && resultadoQuery.results.bindings != null && resultadoQuery.results.bindings.Count > 0)
            {
                foreach (Dictionary<string, SparqlObject.Data> fila in resultadoQuery.results.bindings)
                {
                    string crisIdentifier = UtilidadesAPI.GetValorFilaSparqlObject(fila, "crisIdentifier");
                    if (!string.IsNullOrEmpty(crisIdentifier))
                    {
                        PRC.proyectos = new List<float>() { float.Parse(140012 + "") };
                        //PRC.proyectos = new List<float>() { float.Parse(crisIdentifier) };
                    }
                }
            }
            #endregion

            #region --- Obtención de Revistas
            if (PRC.epigrafeCVN == "060.010.010.000")
            {
                PRC.indicesImpacto = new List<IndiceImpacto>();
                Dictionary<string, string> dicDataRevista = new Dictionary<string, string>();
                select = new StringBuilder();
                where = new StringBuilder();
                select.Append(mPrefijos);
                select.Append("SELECT DISTINCT ?titulo ?editor ?issn ?formato ");
                where.Append("FROM <http://gnoss.com/maindocument.owl> ");
                where.Append("FROM <http://gnoss.com/documentformat.owl> ");
                where.Append("WHERE { ");
                where.Append("?s a bibo:Document. ");
                where.Append("OPTIONAL{?s vivo:hasPublicationVenue ?revista. } ");
                where.Append("?revista roh:title ?titulo. ");
                where.Append("OPTIONAL{?revista bibo:editor ?editor. } ");
                where.Append("OPTIONAL{?revista bibo:issn ?issn. } ");
                where.Append("OPTIONAL{?revista roh:format ?formatoAux. ?formatoAux dc:identifier ?formato. } ");
                where.Append($@"FILTER(?s = <{pIdDocumento}>) ");
                where.Append("} ");

                resultadoQuery = mResourceApi.VirtuosoQuery(select.ToString(), where.ToString(), "document");
                if (resultadoQuery != null && resultadoQuery.results != null && resultadoQuery.results.bindings != null && resultadoQuery.results.bindings.Count > 0)
                {
                    foreach (Dictionary<string, SparqlObject.Data> fila in resultadoQuery.results.bindings)
                    {
                        dicDataRevista.Add("titulo", UtilidadesAPI.GetValorFilaSparqlObject(fila, "titulo"));
                        dicDataRevista.Add("editor", UtilidadesAPI.GetValorFilaSparqlObject(fila, "editor"));
                        dicDataRevista.Add("formato", UtilidadesAPI.GetValorFilaSparqlObject(fila, "formato"));
                        dicDataRevista.Add("issn", UtilidadesAPI.GetValorFilaSparqlObject(fila, "issn"));
                    }
                }

                select = new StringBuilder();
                where = new StringBuilder();
                select.Append(mPrefijos);
                select.Append("SELECT DISTINCT ?fuenteImpacto ?anio ?indiceImpacto ?cuartil ?posicionPublicacion ?numeroRevistas ");
                where.Append("FROM <http://gnoss.com/maindocument.owl> ");
                where.Append("FROM <http://gnoss.com/documentformat.owl> ");
                where.Append("FROM <http://gnoss.com/referencesource.owl> ");
                where.Append("WHERE { ");
                where.Append("?s a bibo:Document. ");
                where.Append("OPTIONAL{?s vivo:hasPublicationVenue ?revista. } ");
                where.Append("?revista roh:title ?titulo. ");
                where.Append("OPTIONAL{ ");
                where.Append("?revista roh:impactIndex ?indicesImpacto. ");
                where.Append("OPTIONAL{?indicesImpacto roh:impactSource ?impactSource. ?impactSource dc:identifier ?fuenteImpacto. } ");
                where.Append("?indicesImpacto roh:year ?anio. ");
                where.Append("?indicesImpacto roh:impactIndexInYear ?indiceImpacto. ");
                where.Append("OPTIONAL{ ");
                where.Append("?indicesImpacto  roh:impactCategory ?categoria. ");
                where.Append("?categoria roh:quartile ?cuartil. ");
                where.Append("OPTIONAL{?categoria roh:publicationPosition ?posicionPublicacion. ?categoria roh:journalNumberInCat ?numeroRevistas. }}} ");
                where.Append($@"FILTER(?s = <{pIdDocumento}>) ");
                where.Append("} ");

                resultadoQuery = mResourceApi.VirtuosoQuery(select.ToString(), where.ToString(), "document");
                if (resultadoQuery != null && resultadoQuery.results != null && resultadoQuery.results.bindings != null && resultadoQuery.results.bindings.Count > 0)
                {
                    foreach (Dictionary<string, SparqlObject.Data> fila in resultadoQuery.results.bindings)
                    {
                        IndiceImpacto indiceImpacto = new IndiceImpacto();
                        if (!string.IsNullOrEmpty(UtilidadesAPI.GetValorFilaSparqlObject(fila, "fuenteImpacto")))
                        {
                            indiceImpacto.fuenteImpacto = UtilidadesAPI.GetValorFilaSparqlObject(fila, "fuenteImpacto");
                        }
                        if (!string.IsNullOrEmpty(UtilidadesAPI.GetValorFilaSparqlObject(fila, "indiceImpacto")))
                        {
                            indiceImpacto.indice = float.Parse(UtilidadesAPI.GetValorFilaSparqlObject(fila, "indiceImpacto"));
                        }
                        if (!string.IsNullOrEmpty(UtilidadesAPI.GetValorFilaSparqlObject(fila, "anio")))
                        {
                            indiceImpacto.anio = UtilidadesAPI.GetValorFilaSparqlObject(fila, "anio");
                        }
                        if (!string.IsNullOrEmpty(UtilidadesAPI.GetValorFilaSparqlObject(fila, "posicionPublicacion")))
                        {
                            indiceImpacto.posicionPublicacion = float.Parse(UtilidadesAPI.GetValorFilaSparqlObject(fila, "posicionPublicacion"));
                        }
                        if (!string.IsNullOrEmpty(UtilidadesAPI.GetValorFilaSparqlObject(fila, "numeroRevistas")))
                        {
                            indiceImpacto.numeroRevistas = float.Parse(UtilidadesAPI.GetValorFilaSparqlObject(fila, "numeroRevistas"));
                        }

                        bool revista25 = false;
                        if (!string.IsNullOrEmpty(UtilidadesAPI.GetValorFilaSparqlObject(fila, "cuartil")) && Int32.Parse(UtilidadesAPI.GetValorFilaSparqlObject(fila, "cuartil")) == 1)
                        {
                            revista25 = true;
                        }
                        indiceImpacto.revista25 = revista25;

                        PRC.indicesImpacto.Add(indiceImpacto);
                    }
                }

                // Nombre de la revista 
                if (!string.IsNullOrEmpty(dicDataRevista["titulo"]))
                {
                    CampoProduccionCientifica campoAux = new CampoProduccionCientifica();
                    campoAux.codigoCVN = "060.010.010.210";
                    campoAux.valores = new List<string>() { dicDataRevista["titulo"] };
                    PRC.campos.Add(campoAux);
                }

                // Editorial
                if (!string.IsNullOrEmpty(dicDataRevista["editor"]))
                {
                    CampoProduccionCientifica campoAux = new CampoProduccionCientifica();
                    campoAux.codigoCVN = "060.010.010.100";
                    campoAux.valores = new List<string>() { dicDataRevista["editor"] };
                    PRC.campos.Add(campoAux);
                }

                // ISSN
                if (!string.IsNullOrEmpty(dicDataRevista["issn"]))
                {
                    CampoProduccionCientifica campoAux = new CampoProduccionCientifica();
                    campoAux.codigoCVN = "060.010.010.160";
                    campoAux.valores = new List<string>() { dicDataRevista["issn"] };
                    PRC.campos.Add(campoAux);
                }
            }
            #endregion

            #region --- Obtención de datos de PUBLICACIONES.       
            if (PRC.epigrafeCVN == "060.010.010.000")
            {
                // Diccionario de IDS.
                Dictionary<string, string> dicIds = new Dictionary<string, string>();
                dicIds.Add("040", "");
                dicIds.Add("120", "");
                dicIds.Add("130", "");

                // Consulta sparql (Obtención de datos del proyecto).
                select = new StringBuilder();
                where = new StringBuilder();

                select.Append(mPrefijos);
                select.Append("SELECT DISTINCT ?title ?issued ?type ?supportType ?numVol ?paginas ?doi ?handle ?pmid ?openAccess ");
                where.Append("WHERE { ");
                where.Append("?s a bibo:Document. ");
                where.Append("?s roh:title ?title. ");
                where.Append("OPTIONAL{?s dct:issued ?issued. } ");
                where.Append("OPTIONAL{?s dc:type ?typeAux. ?typeAux dc:identifier ?type. } ");
                where.Append("OPTIONAL{?s roh:supportType ?support. ?support dc:identifier ?supportType. } ");
                where.Append("OPTIONAL{?s bibo:volume ?volume. } ");
                where.Append("OPTIONAL{?s bibo:issue ?issue. } ");
                where.Append($"BIND(CONCAT(?volume, \"-\", ?issue) AS ?numVol). ");
                where.Append("OPTIONAL{?s bibo:pageStart ?pageStart. } ");
                where.Append("OPTIONAL{?s bibo:pageEnd ?pageEnd. } ");
                where.Append($"BIND(CONCAT(?pageStart, \"-\", ?pageEnd) AS ?paginas). ");
                where.Append("OPTIONAL{?s bibo:doi ?doi. } ");
                where.Append("OPTIONAL{?s bibo:handle ?handle. } ");
                where.Append("OPTIONAL{?s bibo:pmid ?pmid. } ");
                where.Append("OPTIONAL{?s roh:openAccess ?openAccess. } ");
                where.Append($@"FILTER(?s = <{pIdDocumento}>) ");
                where.Append("} ");

                resultadoQuery = mResourceApi.VirtuosoQuery(select.ToString(), where.ToString(), "document");

                if (resultadoQuery != null && resultadoQuery.results != null && resultadoQuery.results.bindings != null && resultadoQuery.results.bindings.Count > 0)
                {
                    foreach (Dictionary<string, SparqlObject.Data> fila in resultadoQuery.results.bindings)
                    {
                        foreach (string item in fila.Keys)
                        {
                            if (dicPropiedadesPublicaciones.ContainsKey(item))
                            {
                                CampoProduccionCientifica campo = new CampoProduccionCientifica();
                                campo.codigoCVN = dicPropiedadesPublicaciones[item];

                                if (item == "issued")
                                {
                                    string dia = fila[item].value.Substring(6, 2);
                                    string mes = fila[item].value.Substring(4, 2);
                                    string anyo = fila[item].value.Substring(0, 4);
                                    string fecha = $@"{anyo}-{mes}-{dia}";
                                    campo.valores = new List<string>() { fecha };
                                }
                                else if (item == "openAccess")
                                {
                                    if (fila[item].value == "true")
                                    {
                                        campo.valores = new List<string>() { "true" };
                                    }
                                    else
                                    {
                                        campo.valores = new List<string>() { "false" };
                                    }
                                }
                                else if (item == "doi" || item == "handle" || item == "pmid")
                                {
                                    switch(item)
                                    {
                                        case "doi":
                                            dicIds["040"] = fila[item].value;
                                            break;
                                        case "handle":
                                            dicIds["120"] = fila[item].value;
                                            break;
                                        case "pmid":
                                            dicIds["130"] = fila[item].value;
                                            break;
                                    }
                                }
                                else
                                {
                                    if (fila[item].value != "-")
                                    {
                                        campo.valores = new List<string>() { fila[item].value };
                                    }
                                }

                                if (!string.IsNullOrEmpty(campo.codigoCVN) && campo.valores != null && campo.valores.Any())
                                {
                                    PRC.campos.Add(campo);
                                }
                            }
                        }
                    }
                }

                List<string> listaIds = new List<string>();
                List<string> listaValores = new List<string>();
                foreach (KeyValuePair<string, string> item in dicIds)
                {
                    if (!string.IsNullOrEmpty(item.Value))
                    {
                        listaIds.Add(item.Key);
                        listaValores.Add(item.Value);
                    }
                }
                CampoProduccionCientifica campoAux = new CampoProduccionCientifica();
                campoAux.codigoCVN = "060.010.010.400";
                campoAux.valores = listaValores;
                PRC.campos.Add(campoAux);
                campoAux = new CampoProduccionCientifica();
                campoAux.codigoCVN = "060.010.010.410";
                campoAux.valores = listaIds;
                PRC.campos.Add(campoAux);

            }
            #endregion

            #region --- Obtención de datos de CONGRESOS.
            if (PRC.epigrafeCVN == "060.010.020.000")
            {
                // Diccionario de IDS.
                Dictionary<string, string> dicIds = new Dictionary<string, string>();
                dicIds.Add("040", "");
                dicIds.Add("120", "");
                dicIds.Add("130", "");

                // Consulta sparql (Obtención de datos del proyecto).
                select = new StringBuilder();
                where = new StringBuilder();

                select.Append(mPrefijos);
                select.Append("SELECT DISTINCT ?title ?fechaCelebracion ?fechaFinalizacion ?tipoEvento ?geographicFocus ?presentedAt ?publicationVenueText ?doi ?handle ?pmid ?isbn ?issn ?participationType ");
                where.Append("WHERE { ");
                where.Append("?s a bibo:Document. ");
                where.Append("?s roh:title ?title. ");
                where.Append("OPTIONAL{?s roh:presentedAtStart ?fechaCelebracion. } ");
                where.Append("OPTIONAL{?s roh:presentedAtEnd ?fechaFinalizacion. } ");
                where.Append("OPTIONAL{?s roh:presentedAtType ?tipoEventoAux. ?tipoEventoAux dc:identifier ?tipoEvento. } ");
                where.Append("OPTIONAL{?s roh:presentedAtGeographicFocus ?geographicFocusAux. ?geographicFocusAux dc:identifier ?geographicFocus. } ");
                where.Append("OPTIONAL{?s bibo:presentedAt ?presentedAt. } ");
                where.Append("OPTIONAL{?s roh:hasPublicationVenueText ?publicationVenueText. } ");
                where.Append("OPTIONAL{?s bibo:doi ?doi. } ");
                where.Append("OPTIONAL{?s bibo:handle ?handle. } ");
                where.Append("OPTIONAL{?s bibo:pmid ?pmid. } ");
                where.Append("OPTIONAL{?s roh:isbn ?isbn. } ");
                where.Append("OPTIONAL{?s bibo:issn ?issn. } ");
                where.Append("OPTIONAL{?s roh:participationType ?participationTypeAux. ?participationTypeAux dc:identifier ?participationType. } ");
                where.Append($@"FILTER(?s = <{pIdDocumento}>) ");
                where.Append("} ");

                resultadoQuery = mResourceApi.VirtuosoQuery(select.ToString(), where.ToString(), "document");

                if (resultadoQuery != null && resultadoQuery.results != null && resultadoQuery.results.bindings != null && resultadoQuery.results.bindings.Count > 0)
                {
                    foreach (Dictionary<string, SparqlObject.Data> fila in resultadoQuery.results.bindings)
                    {
                        foreach (string item in fila.Keys)
                        {
                            if (dicPropiedadesCongresos.ContainsKey(item))
                            {
                                CampoProduccionCientifica campo = new CampoProduccionCientifica();
                                campo.codigoCVN = dicPropiedadesCongresos[item];

                                if (item == "fechaCelebracion" || item == "fechaFinalizacion")
                                {
                                    string dia = fila[item].value.Substring(6, 2);
                                    string mes = fila[item].value.Substring(4, 2);
                                    string anyo = fila[item].value.Substring(0, 4);
                                    string fecha = $@"{anyo}-{mes}-{dia}";
                                    campo.valores = new List<string>() { fecha };
                                }
                                else if (item == "doi" || item == "handle" || item == "pmid")
                                {
                                    switch (item)
                                    {
                                        case "doi":
                                            dicIds["040"] = fila[item].value;
                                            break;
                                        case "handle":
                                            dicIds["120"] = fila[item].value;
                                            break;
                                        case "pmid":
                                            dicIds["130"] = fila[item].value;
                                            break;
                                    }
                                }
                                else
                                {
                                    if (fila[item].value != "-")
                                    {
                                        campo.valores = new List<string>() { fila[item].value };
                                    }
                                }

                                if (!string.IsNullOrEmpty(campo.codigoCVN) && campo.valores != null && campo.valores.Any())
                                {
                                    PRC.campos.Add(campo);
                                }

                                //ÑAPA
                                //campo = new CampoProduccionCientifica();
                                //campo.codigoCVN = "060.010.020.000";
                                //campo.valores = new List<string>() { "020" };
                                //PRC.campos.Add(campo);
                            }
                        }
                    }
                }
                List<string> listaIds = new List<string>();
                List<string> listaValores = new List<string>();
                foreach (KeyValuePair<string, string> item in dicIds)
                {
                    if (!string.IsNullOrEmpty(item.Value))
                    {
                        listaIds.Add(item.Key);
                        listaValores.Add(item.Value);
                    }
                }
                CampoProduccionCientifica campoAux = new CampoProduccionCientifica();
                campoAux.codigoCVN = "060.010.010.400";
                campoAux.valores = listaValores;
                PRC.campos.Add(campoAux);
                campoAux = new CampoProduccionCientifica();
                campoAux.codigoCVN = "060.010.010.410";
                campoAux.valores = listaIds;
                PRC.campos.Add(campoAux);
            }
            #endregion

            #region --- Envío a SGI.
            try
            {
                RestClient client = new(pConfig.GetUrlProduccionCientifica());
                client.AddDefaultHeader("Authorization", "Bearer " + GetToken(pConfig));
                var request = new RestRequest(Method.POST);
                request.AddJsonBody(PRC);
                IRestResponse response = client.Execute(request);

                if ((int)response.StatusCode != 200)
                {
                    throw new Exception();
                }
            }
            catch (Exception)
            {
                throw;
            }
            #endregion

            #region --- Cambio del estado del envío.
            // Comprobar si está el triple del estado.
            string valorEnviado = string.Empty;
            select = new StringBuilder();
            where = new StringBuilder();

            select.Append(mPrefijos);
            select.Append("SELECT DISTINCT ?enviado ");
            where.Append("WHERE { ");
            where.Append("?s a bibo:Document. ");
            where.Append("OPTIONAL{?s roh:validationStatusPRC ?enviado. } ");
            where.Append($@"FILTER(?s = <{pIdDocumento}>) ");
            where.Append("} ");

            resultadoQuery = mResourceApi.VirtuosoQuery(select.ToString(), where.ToString(), "document");

            if (resultadoQuery != null && resultadoQuery.results != null && resultadoQuery.results.bindings != null && resultadoQuery.results.bindings.Count > 0)
            {
                foreach (Dictionary<string, SparqlObject.Data> fila in resultadoQuery.results.bindings)
                {
                    valorEnviado = UtilidadesAPI.GetValorFilaSparqlObject(fila, "enviado");
                }
            }

            mResourceApi.ChangeOntoly("document");
            guid = mResourceApi.GetShortGuid(pIdDocumento);

            if (string.IsNullOrEmpty(valorEnviado))
            {
                // Inserción.
                Insercion(guid, "http://w3id.org/roh/validationStatusPRC", "PENDIENTE");
            }
            else
            {
                // Modificación.
                Modificacion(guid, "http://w3id.org/roh/validationStatusPRC", "PENDIENTE", valorEnviado);
            }
            #endregion
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
        /// Mapea el código CVN con la propiedad usada en SPARQL de PUBLICACIONES.
        /// </summary>
        private void RellenarDiccionarioPublicaciones()
        {
            // <060.010.010.000> Publicaciones
            dicPropiedadesPublicaciones.Add("title", "060.010.010.030");
            dicPropiedadesPublicaciones.Add("issued", "060.010.010.140");
            dicPropiedadesPublicaciones.Add("type", "060.010.010.010");
            dicPropiedadesPublicaciones.Add("supportType", "060.010.010.070");
            //060.010.010.210 - Nombre de la revista (HECHO ARRIBA)
            dicPropiedadesPublicaciones.Add("isbn", "060.010.010.160"); // TODO: PREGUNTAR A ALICIA
            dicPropiedadesPublicaciones.Add("issn", "060.010.010.160"); // TODO: PREGUNTAR A ALICIA
            //060.010.010.100 - Editorial (HECHO ARRIBA)
            dicPropiedadesPublicaciones.Add("numVol", "060.010.010.080"); // Volume e Issue
            dicPropiedadesPublicaciones.Add("paginas", "060.010.010.090"); // PageEnd y PageStart
            //060.010.010.400 - Identificadores digitales // TODO: PREGUNTAR A ALICIA
            //060.010.010.410 - Tipo identificadores digitales // TODO: PREGUNTAR A ALICIA
            dicPropiedadesPublicaciones.Add("openAccess", "TIPO_OPEN_ACCESS");
            dicPropiedadesPublicaciones.Add("doi", "");
            dicPropiedadesPublicaciones.Add("handle", "");
            dicPropiedadesPublicaciones.Add("pmid", "");
        }

        /// <summary>
        /// Mapea el código CVN con la propiedad usada en SPARQL de CONGRESOS.
        /// </summary>
        private void RellenarDiccionarioCongresos()
        {
            dicPropiedadesCongresos.Add("title", "060.010.020.030");
            dicPropiedadesCongresos.Add("supportType", "060.010.010.070");
            dicPropiedadesCongresos.Add("fechaCelebracion", "060.010.020.190");
            dicPropiedadesCongresos.Add("fechaFinalizacion", "060.010.020.380");
            dicPropiedadesCongresos.Add("tipoEvento", "060.010.020.010");
            dicPropiedadesCongresos.Add("geographicFocus", "060.010.020.080");
            dicPropiedadesCongresos.Add("presentedAt", "060.010.020.100");
            dicPropiedadesCongresos.Add("publicationVenueText", "060.010.020.370");
            dicPropiedadesCongresos.Add("issn", "060.010.020.320"); // TODO: PREGUNTAR A ALICIA
            dicPropiedadesCongresos.Add("isbn", "060.010.020.320"); // TODO: PREGUNTAR A ALICIA
            dicPropiedadesCongresos.Add("participationType", "060.010.020.050");
            dicPropiedadesCongresos.Add("doi", "");
            dicPropiedadesCongresos.Add("handle", "");
            dicPropiedadesCongresos.Add("pmid", "");
        }

        /// <summary>
        /// Obtención del token.
        /// </summary>
        /// <returns></returns>
        private string GetToken(ConfigService pConfig)
        {
            // TODO: Sacar a archivo de configuración.
            Uri url = new Uri(pConfig.GetUrlToken());
            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("client_id", "front"),
                new KeyValuePair<string, string>("username", pConfig.GetUsernameESB()),
                new KeyValuePair<string, string>("password", pConfig.GetPasswordESB()),
                new KeyValuePair<string, string>("grant_type", "password")
            });

            string result = httpCall(url.ToString(), "POST", content).Result;
            var json = JObject.Parse(result);

            return json["access_token"].ToString();
        }

        /// <summary>
        /// Llamada para la obtención del token.
        /// </summary>
        /// <param name="pUrl"></param>
        /// <param name="pMethod"></param>
        /// <param name="pBody"></param>
        /// <returns></returns>
        protected async Task<string> httpCall(string pUrl, string pMethod, FormUrlEncodedContent pBody)
        {
            HttpResponseMessage response;
            using (var httpClient = new HttpClient())
            {
                using (var request = new HttpRequestMessage(new HttpMethod(pMethod), pUrl))
                {
                    request.Content = pBody;

                    int intentos = 3;
                    while (true)
                    {
                        try
                        {
                            response = await httpClient.SendAsync(request);
                            break;
                        }
                        catch
                        {
                            intentos--;
                            if (intentos == 0)
                            {
                                throw;
                            }
                            else
                            {
                                Thread.Sleep(1000);
                            }
                        }
                    }
                }
            }
            if (response.Content != null)
            {
                return await response.Content.ReadAsStringAsync();
            }
            else
            {
                return string.Empty;
            }
        }
    }
}
