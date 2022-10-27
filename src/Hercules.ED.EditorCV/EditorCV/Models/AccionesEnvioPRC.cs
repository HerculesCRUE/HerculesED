using EditorCV.Controllers;
using EditorCV.Models.EnvioPRC;
using EditorCV.Models.Utils;
using Gnoss.ApiWrapper;
using Gnoss.ApiWrapper.ApiModel;
using Gnoss.ApiWrapper.Model;
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
    public class AccionesEnvioPRC: AccionesEnvio
    {
        private static Dictionary<string, string> dicPropiedadesPublicaciones = new Dictionary<string, string>();
        private static Dictionary<string, string> dicPropiedadesCongresos = new Dictionary<string, string>();

        readonly ConfigService _Configuracion;

        public AccionesEnvioPRC(ConfigService pConfig)
        {
            _Configuracion = pConfig;
        }

        /// <summary>
        /// Devuelve un diccionario con todos los proyectos de <paramref name="pIdPersona"/>, junto a su titulo, fecha de inicio, fecha de fin y organización.
        /// </summary>
        /// <param name="pIdPersona"></param>
        /// <returns></returns>
        public Dictionary<string, Dictionary<string, string>> ObtenerDatosEnvioPRC(string pIdPersona)
        {
            Dictionary<string, Dictionary<string, string>> listadoProyectos = new Dictionary<string, Dictionary<string, string>>();
            DateTime fechaFinMaximaProyectosEnvioPRC = DateTime.Now;
            fechaFinMaximaProyectosEnvioPRC = fechaFinMaximaProyectosEnvioPRC.AddMonths(-_Configuracion.GetMaxMonthsValidationProjectsDocument());
            string fechaFinMaximaProyectosEnvioPRCString = fechaFinMaximaProyectosEnvioPRC.ToString("yyyyMMdd000000");
            string select = $@"select distinct  ?project ?titulo ?fechaInicio ?fechaFin ?organizacion";
            string where = $@"
where {{
    ?project a <http://vivoweb.org/ontology/core#Project>.
    ?project <http://vivoweb.org/ontology/core#relates> ?rol .
    ?project <http://w3id.org/roh/isValidated> 'true'.
    ?rol <http://w3id.org/roh/roleOf> <{pIdPersona}> .
    OPTIONAL{{?project <http://w3id.org/roh/title> ?titulo}}
    OPTIONAL{{?project <http://vivoweb.org/ontology/core#start> ?fechaInicio}}
    OPTIONAL{{?project <http://vivoweb.org/ontology/core#end> ?fechaFin}}
    OPTIONAL{{?project <http://w3id.org/roh/conductedByTitle> ?organizacion}}
    FILTER(!BOUND(?fechaFin) OR xsd:long(?fechaFin)>{fechaFinMaximaProyectosEnvioPRCString})
}}";
            SparqlObject resultadoQuery = mResourceApi.VirtuosoQuery(select, where, "project");
            if (resultadoQuery.results.bindings.Count != 0)
            {
                foreach (Dictionary<string, SparqlObject.Data> res in resultadoQuery.results.bindings)
                {
                    if (res.ContainsKey("project"))
                    {
                        Dictionary<string, string> keyValues = new Dictionary<string, string>();
                        if (res.ContainsKey("titulo"))
                        {
                            keyValues.Add("titulo", res["titulo"].value);
                        }
                        if (res.ContainsKey("fechaInicio"))
                        {
                            keyValues.Add("fechaInicio", ConversorFechas(res["fechaInicio"].value));
                        }
                        if (res.ContainsKey("fechaFin"))
                        {
                            keyValues.Add("fechaFin", ConversorFechas(res["fechaFin"].value));
                        }
                        if (res.ContainsKey("organizacion"))
                        {
                            keyValues.Add("organizacion", res["organizacion"].value);
                        }

                        listadoProyectos.Add(res["project"].value, keyValues);
                    }
                }
            }
            return listadoProyectos;
        }

        /// <summary>
        /// Convierte un string con formato 20221231000000 a 31/12/2022
        /// </summary>
        /// <param name="fecha"></param>
        /// <returns></returns>
        private string ConversorFechas(string fecha)
        {
            string fechaConvertida = "";
            if (!string.IsNullOrEmpty(fecha) && fecha.Length > 8)
            {
                string anio = fecha.Substring(0, 4);
                string mes = fecha.Substring(4, 2);
                string dia = fecha.Substring(6, 2);
                fechaConvertida = dia + "/" + mes + "/" + anio;
            }

            return fechaConvertida;
        }

        /// <summary>
        /// Permite enviar a Producción Científica los datos necesarios para la validación.
        /// </summary>
        /// <param name="pConfig">Configuración.</param>
        /// <param name="pIdRecurso">ID del recurso que apunta al documento.</param>
        /// <param name="pIdProyecto">ID del recurso del proyecto.</param>
        public void EnvioPRC(ConfigService pConfig, string pIdRecurso, List<string> pIdProyecto)
        {
            string pIdDocumento = ObtenerIdDocumento(pIdRecurso);
            if (string.IsNullOrEmpty(pIdDocumento))
            {
                return;
            }

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

            ProduccionCientifica PRC = CrearPRC(pIdDocumento, false);

            #region --- Inserción y obtención del Proyecto asociado.
            // Comprobar si está el triple.
            List<string> idProyectoAux = new List<string>();
            string selectProyectoAsociado = "";
            string whereProyectoAsociado = "";

            selectProyectoAsociado = mPrefijos;
            selectProyectoAsociado += "SELECT DISTINCT ?proyecto ";
            whereProyectoAsociado = $@"WHERE {{ 
                                            ?s a bibo:Document . 
                                            OPTIONAL{{?s roh:projectAux ?proyecto . }}
                                            FILTER(?s = <{pIdDocumento}>) 
                                        }} ";

            resultadoQuery = mResourceApi.VirtuosoQuery(selectProyectoAsociado, whereProyectoAsociado, "document");

            if (resultadoQuery != null && resultadoQuery.results != null && resultadoQuery.results.bindings != null && resultadoQuery.results.bindings.Count > 0)
            {
                foreach (Dictionary<string, SparqlObject.Data> fila in resultadoQuery.results.bindings)
                {
                    if (fila.Any())
                    {
                        idProyectoAux.Add(UtilidadesAPI.GetValorFilaSparqlObject(fila, "proyecto"));
                    }
                }
            }

            mResourceApi.ChangeOntoly("document");
            Guid guid = mResourceApi.GetShortGuid(pIdDocumento);

            if (!idProyectoAux.Any())
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
            string selectIdProyecto = "";
            string whereIdProyecto = "";

            selectIdProyecto = mPrefijos;
            selectIdProyecto += "SELECT DISTINCT ?crisIdentifier ";
            whereIdProyecto = $@"WHERE {{ 
                                    ?s a vivo:Project .
                                    OPTIONAL{{?s roh:crisIdentifier ?crisIdentifier . }}
                                    FILTER(?s in (<{string.Join(">,<", pIdProyecto)}>) )
                                }} ";

            resultadoQuery = mResourceApi.VirtuosoQuery(selectIdProyecto, whereIdProyecto, "project");

            if (resultadoQuery != null && resultadoQuery.results != null && resultadoQuery.results.bindings != null && resultadoQuery.results.bindings.Count > 0)
            {
                PRC.proyectos = new List<float>();
                foreach (Dictionary<string, SparqlObject.Data> fila in resultadoQuery.results.bindings)
                {
                    string crisIdentifier = UtilidadesAPI.GetValorFilaSparqlObject(fila, "crisIdentifier");
                    if (!string.IsNullOrEmpty(crisIdentifier))
                    {
                        if (crisIdentifier.Contains("|"))
                        {
                            crisIdentifier = crisIdentifier.Split("|").Last();
                        }
                        PRC.proyectos.Add(float.Parse(crisIdentifier));
                    }
                }
            }
            #endregion

            #region --- Obtención de Revistas
            if (PRC.epigrafeCVN == "060.010.010.000")
            {
                PRC.indicesImpacto = new List<IndiceImpacto>();
                Dictionary<string, string> dicDataRevista = new Dictionary<string, string>();
                string selectIndicesImpacto = "";
                string whereIndicesImpacto = "";
                selectIndicesImpacto = mPrefijos;
                selectIndicesImpacto += $@"SELECT DISTINCT ?titulo ?editor ?issn ?formato";
                whereIndicesImpacto = $@"WHERE {{
                                           ?s a bibo:Document.
                                           OPTIONAL{{
                                                ?s vivo:hasPublicationVenue ?revista . 
                                                OPTIONAL{{?revista bibo:editor ?editor . }} 
                                                OPTIONAL{{?revista bibo:issn ?issn . }}
                                                OPTIONAL{{
                                                    ?revista roh:format ?formatoAux .
                                                    ?formatoAux dc:identifier ?formato .
                                               }} 
                                            }}
                                           OPTIONAL{{?s roh:hasPublicationVenueJournalText ?titulo . }}                                           
                                           FILTER(?s = <{pIdDocumento}>)
                                       }} ";

                resultadoQuery = mResourceApi.VirtuosoQueryMultipleGraph(selectIndicesImpacto, whereIndicesImpacto, new List<string> { "document","maindocument", "documentformat" });
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

                string selectFuenteImpacto = "";
                string whereFuenteImpacto = "";
                selectFuenteImpacto = mPrefijos;
                selectFuenteImpacto += $@"SELECT DISTINCT ?fuenteImpacto ?anio ?indiceImpacto ?cuartil ?posicionPublicacion ?numeroRevistas";
                whereFuenteImpacto = $@"WHERE {{ 
                                            ?s a bibo:Document . 
                                            OPTIONAL{{?s vivo:hasPublicationVenue ?revista .
                                                OPTIONAL{{
                                                    ?revista roh:impactIndex ?indicesImpacto . 
                                                    OPTIONAL{{
                                                        ?indicesImpacto roh:impactSource ?impactSource .
                                                        ?impactSource dc:identifier ?fuenteImpacto . 
                                                    }} 
                                                    ?indicesImpacto roh:year ?anio . 
                                                    ?indicesImpacto roh:impactIndexInYear ?indiceImpacto .
                                                    OPTIONAL{{
                                                        ?indicesImpacto  roh:impactCategory ?categoria . 
                                                        ?categoria roh:quartile ?cuartil . 
                                                        OPTIONAL{{
                                                            ?categoria roh:publicationPosition ?posicionPublicacion .
                                                            ?categoria roh:journalNumberInCat ?numeroRevistas .
                                                        }}
                                                    }}
                                                }}
                                            }}
                                            OPTIONAL{{?s roh:hasPublicationVenueJournalText ?titulo . }} 
                                            
                                            FILTER(?s = <{pIdDocumento}>) 
                                        }} ";

                resultadoQuery = mResourceApi.VirtuosoQueryMultipleGraph(selectFuenteImpacto, whereFuenteImpacto, new List<string> { "document" ,"maindocument", "documentformat", "referencesource" });
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

                        if (!string.IsNullOrEmpty(indiceImpacto.fuenteImpacto))
                        PRC.indicesImpacto.Add(indiceImpacto);
                    }
                }

                // Nombre de la revista 
                if (dicDataRevista != null && dicDataRevista.Any() && !string.IsNullOrEmpty(dicDataRevista["titulo"]))
                {
                    CampoProduccionCientifica campoAux = new CampoProduccionCientifica();
                    campoAux.codigoCVN = "060.010.010.210";
                    campoAux.valores = new List<string>() { dicDataRevista["titulo"] };
                    PRC.campos.Add(campoAux);
                }

                // Editorial
                if (dicDataRevista != null && dicDataRevista.Any() && !string.IsNullOrEmpty(dicDataRevista["editor"]))
                {
                    CampoProduccionCientifica campoAux = new CampoProduccionCientifica();
                    campoAux.codigoCVN = "060.010.010.100";
                    campoAux.valores = new List<string>() { dicDataRevista["editor"] };
                    PRC.campos.Add(campoAux);
                }

                // ISSN
                if (dicDataRevista != null && dicDataRevista.Any() && !string.IsNullOrEmpty(dicDataRevista["issn"]))
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
                string selectObtencionProyecto = "";
                string whereObtencionProyecto = "";

                selectObtencionProyecto = mPrefijos;
                selectObtencionProyecto += $@"SELECT DISTINCT ?title ?issued ?type ?supportType ?numVol ?paginas ?doi ?handle ?pmid ?openAccess";
                whereObtencionProyecto = $@"WHERE {{
                                                ?s a bibo:Document . 
                                                ?s roh:title ?title .
                                                OPTIONAL{{?s dct:issued ?issued . }} 
                                                OPTIONAL{{
                                                    ?s dc:type ?typeAux . 
                                                    ?typeAux dc:identifier ?type . 
                                                }} 
                                                OPTIONAL{{
                                                    ?s roh:supportType ?support .
                                                    ?support dc:identifier ?supportType . 
                                                }}
                                                OPTIONAL{{?s bibo:volume ?volume . }} 
                                                OPTIONAL{{?s bibo:issue ?issue . }}
                                                BIND(CONCAT(?volume, ""-"", ?issue) AS ?numVol) . 
                                                OPTIONAL{{?s bibo:pageStart ?pageStart . }}
                                                OPTIONAL{{?s bibo:pageEnd ?pageEnd . }} 
                                                BIND(CONCAT(?pageStart, ""-"", ?pageEnd) AS ?paginas) .
                                                OPTIONAL{{?s bibo:doi ?doi . }}
                                                OPTIONAL{{?s bibo:handle ?handle . }} 
                                                OPTIONAL{{?s bibo:pmid ?pmid . }}
                                                OPTIONAL{{?s roh:openAccess ?openAccess . }}
                                                FILTER(?s = <{pIdDocumento}>) 
                                            }} ";

                resultadoQuery = mResourceApi.VirtuosoQueryMultipleGraph(selectObtencionProyecto, whereObtencionProyecto,new List<string> { "document", "documentformat", "publicationtype" });

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
                if (listaValores != null && listaValores.Any())
                {
                    CampoProduccionCientifica campoAux = new CampoProduccionCientifica();
                    campoAux.codigoCVN = "060.010.010.400";
                    campoAux.valores = listaValores;
                    PRC.campos.Add(campoAux);
                }
                if (listaIds != null && listaIds.Any())
                {
                    CampoProduccionCientifica campoAux = new CampoProduccionCientifica();
                    campoAux.codigoCVN = "060.010.010.410";
                    campoAux.valores = listaIds;
                    PRC.campos.Add(campoAux);
                }
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
                string selectDatosCongresos = "";
                string whereDatosCongresos = "";

                selectDatosCongresos = mPrefijos;
                selectDatosCongresos += $@"SELECT DISTINCT ?title ?type ?supportType ?fechaCelebracion ?fechaFinalizacion ?tipoEvento ?geographicFocus ?presentedAt ?publicationVenueText ?doi ?handle ?pmid ?isbn ?issn ?";
                whereDatosCongresos = $@"WHERE {{
                                            ?s a bibo:Document . 
                                            ?s roh:title ?title . 
                                            OPTIONAL{{
                                                ?s dc:type ?pubTypeAux . 
                                                ?pubTypeAux dc:identifier ?type .
                                            }}
                                            OPTIONAL{{
                                                ?s roh:supportType ?supType .
                                                ?supType dc:identifier ?supportType .
                                            }}
                                            OPTIONAL{{?s roh:presentedAtStart ?fechaCelebracion . }}
                                            OPTIONAL{{?s roh:presentedAtEnd ?fechaFinalizacion . }} 
                                            OPTIONAL{{
                                                ?s roh:presentedAtType ?tipoEventoAux . 
                                                ?tipoEventoAux dc:identifier ?tipoEvento .
                                            }} 
                                            OPTIONAL{{
                                                ?s roh:presentedAtGeographicFocus ?geographicFocusAux .
                                                ?geographicFocusAux dc:identifier ?geographicFocus . 
                                            }}
                                            OPTIONAL{{?s bibo:presentedAt ?presentedAt . }} 
                                            OPTIONAL{{?s roh:hasPublicationVenueText ?publicationVenueText . }}
                                            OPTIONAL{{?s bibo:doi ?doi . }} 
                                            OPTIONAL{{?s bibo:handle ?handle . }} 
                                            OPTIONAL{{?s bibo:pmid ?pmid . }} 
                                            OPTIONAL{{?s roh:isbn ?isbn . }} 
                                            OPTIONAL{{?s bibo:issn ?issn . }} 
                                            OPTIONAL{{
                                                ?s roh:participationType ?participationTypeAux .
                                                ?participationTypeAux dc:identifier ?participationType .
                                            }}
                                            FILTER(?s = <{pIdDocumento}>) 
                                        }} ";

                resultadoQuery = mResourceApi.VirtuosoQueryMultipleGraph(selectDatosCongresos, whereDatosCongresos,new List<string> { "document", "documentformat", "publicationtype" });

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
                            }
                        }
                    }
                }
                // TODO Añadir Clase 1 clase 2 Clase 3 al congreso (consulta + envío)
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
                if (listaValores != null && listaValores.Any())
                {
                    CampoProduccionCientifica campoAux = new CampoProduccionCientifica();
                    campoAux.codigoCVN = "060.010.010.400";
                    campoAux.valores = listaValores;
                    PRC.campos.Add(campoAux);
                }
                if (listaIds != null && listaIds.Any())
                {
                    CampoProduccionCientifica campoAux = new CampoProduccionCientifica();
                    campoAux.codigoCVN = "060.010.010.410";
                    campoAux.valores = listaIds;
                    PRC.campos.Add(campoAux);
                }
            }
            #endregion

            #region --- Envío a SGI.
            try
            {
                IRestResponse response = null;

                if (PRC.epigrafeCVN.Equals("060.010.010.000") && !PRC.campos.Any(x => x.codigoCVN.Equals("060.010.010.010")))
                {
                    throw new Exception("El recurso no tiene tipo de proyecto");
                }

                if (valorEnviado == "rechazado")
                {
                    RestClient client = new($@"{pConfig.GetUrlProduccionCientifica()}/{PRC.idRef}");
                    client.AddDefaultHeader("Authorization", "Bearer " + GetTokenPRC(pConfig));
                    var request = new RestRequest(Method.PUT);
                    request.AddJsonBody(PRC);
                    string jsonString = JsonConvert.SerializeObject(PRC);
                    response = client.Execute(request);
                }
                else
                {
                    RestClient client = new(pConfig.GetUrlProduccionCientifica());
                    client.AddDefaultHeader("Authorization", "Bearer " + GetTokenPRC(pConfig));
                    var request = new RestRequest(Method.POST);
                    request.AddJsonBody(PRC);
                    string jsonString = JsonConvert.SerializeObject(PRC);
                    response = client.Execute(request);
                }

                if ((int)response.StatusCode < 200 || (int)response.StatusCode >= 300)
                {
                    throw new Exception(response.StatusCode.ToString() + ", " + response.Content);
                }
            }
            catch (Exception)
            {
                throw;
            }
            #endregion

            #region --- Cambio del estado del envío.           
            mResourceApi.ChangeOntoly("document");
            guid = mResourceApi.GetShortGuid(pIdDocumento);

            if (string.IsNullOrEmpty(valorEnviado))
            {
                // Inserción.
                Insercion(guid, "http://w3id.org/roh/validationStatusPRC", "pendiente");
            }
            else
            {
                // Modificación.
                Modificacion(guid, "http://w3id.org/roh/validationStatusPRC", "pendiente", valorEnviado);
            }
            #endregion
        }
        
        /// <summary>
        /// Permite enviar a eliminar en Producción Científica los datos necesarios para la validación.
        /// </summary>
        /// <param name="pConfig">Configuración.</param>
        /// <param name="pIdRecurso">ID del recurso que apunta al documento.</param>
        /// <param name="pIdProyecto">ID del recurso del proyecto.</param>
        public void EnvioEliminacionPRC(ConfigService pConfig, string pIdRecurso)
        {
            string pIdDocumento = ObtenerIdDocumento(pIdRecurso);
            if (string.IsNullOrEmpty(pIdDocumento))
            {
                return;
            }

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

            ProduccionCientifica PRC = CrearPRC(pIdDocumento, true);

            #region --- Obtención de Revistas
            if (PRC.epigrafeCVN == "060.010.010.000")
            {
                PRC.indicesImpacto = new List<IndiceImpacto>();
                Dictionary<string, string> dicDataRevista = new Dictionary<string, string>();
                string selectIndicesImpacto = "";
                string whereIndicesImpacto = "";
                selectIndicesImpacto = mPrefijos;
                selectIndicesImpacto += $@"SELECT DISTINCT ?titulo ?editor ?issn ?formato";
                whereIndicesImpacto = $@"WHERE {{
                                           ?s a bibo:Document.
                                           OPTIONAL{{?s vivo:hasPublicationVenue ?revista . }}
                                           ?revista roh:title ?titulo . 
                                           OPTIONAL{{?revista bibo:editor ?editor . }} 
                                           OPTIONAL{{?revista bibo:issn ?issn . }}
                                           OPTIONAL{{
                                                ?revista roh:format ?formatoAux .
                                                ?formatoAux dc:identifier ?formato .
                                           }} 
                                           FILTER(?s = <{pIdDocumento}>)
                                       }} ";

                resultadoQuery = mResourceApi.VirtuosoQueryMultipleGraph(selectIndicesImpacto, whereIndicesImpacto,new List<string> { "document", "maindocument", "documentformat" });
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

                string selectFuenteImpacto = "";
                string whereFuenteImpacto = "";
                selectFuenteImpacto = mPrefijos;
                selectFuenteImpacto += $@"SELECT DISTINCT ?fuenteImpacto ?anio ?indiceImpacto ?cuartil ?posicionPublicacion ?numeroRevistas";
                whereFuenteImpacto = $@"WHERE {{ 
                                            ?s a bibo:Document . 
                                            OPTIONAL{{?s vivo:hasPublicationVenue ?revista . }}
                                            ?revista roh:title ?titulo . 
                                            OPTIONAL{{
                                                ?revista roh:impactIndex ?indicesImpacto . 
                                                OPTIONAL{{
                                                    ?indicesImpacto roh:impactSource ?impactSource .
                                                    ?impactSource dc:identifier ?fuenteImpacto . 
                                                }} 
                                                ?indicesImpacto roh:year ?anio . 
                                                ?indicesImpacto roh:impactIndexInYear ?indiceImpacto .
                                                OPTIONAL{{
                                                    ?indicesImpacto  roh:impactCategory ?categoria . 
                                                    ?categoria roh:quartile ?cuartil . 
                                                    OPTIONAL{{
                                                        ?categoria roh:publicationPosition ?posicionPublicacion .
                                                        ?categoria roh:journalNumberInCat ?numeroRevistas .
                                                    }}
                                                }}
                                            }}
                                            FILTER(?s = <{pIdDocumento}>) 
                                        }} ";

                resultadoQuery = mResourceApi.VirtuosoQueryMultipleGraph(selectFuenteImpacto, whereFuenteImpacto,new List<string> { "document", "maindocument", "documentformat", "referencesource" });
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
                if (dicDataRevista != null && dicDataRevista.Any() && !string.IsNullOrEmpty(dicDataRevista["titulo"]))
                {
                    CampoProduccionCientifica campoAux = new CampoProduccionCientifica();
                    campoAux.codigoCVN = "060.010.010.210";
                    campoAux.valores = new List<string>() { dicDataRevista["titulo"] };
                    PRC.campos.Add(campoAux);
                }

                // Editorial
                if (dicDataRevista != null && dicDataRevista.Any() && !string.IsNullOrEmpty(dicDataRevista["editor"]))
                {
                    CampoProduccionCientifica campoAux = new CampoProduccionCientifica();
                    campoAux.codigoCVN = "060.010.010.100";
                    campoAux.valores = new List<string>() { dicDataRevista["editor"] };
                    PRC.campos.Add(campoAux);
                }

                // ISSN
                if (dicDataRevista != null && dicDataRevista.Any() && !string.IsNullOrEmpty(dicDataRevista["issn"]))
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
                string selectObtencionProyecto = "";
                string whereObtencionProyecto = "";

                selectObtencionProyecto = mPrefijos;
                selectObtencionProyecto += $@"SELECT DISTINCT ?title ?issued ?type ?supportType ?numVol ?paginas ?doi ?handle ?pmid ?openAccess";
                whereObtencionProyecto = $@"WHERE {{
                                                ?s a bibo:Document . 
                                                ?s roh:title ?title .
                                                OPTIONAL{{?s dct:issued ?issued . }} 
                                                OPTIONAL{{
                                                    ?s dc:type ?typeAux . 
                                                    ?typeAux dc:identifier ?type . 
                                                }} 
                                                OPTIONAL{{
                                                    ?s roh:supportType ?support .
                                                    ?support dc:identifier ?supportType . 
                                                }}
                                                OPTIONAL{{?s bibo:volume ?volume . }} 
                                                OPTIONAL{{?s bibo:issue ?issue . }}
                                                BIND(CONCAT(?volume, ""-"", ?issue) AS ?numVol) . 
                                                OPTIONAL{{?s bibo:pageStart ?pageStart . }}
                                                OPTIONAL{{?s bibo:pageEnd ?pageEnd . }} 
                                                BIND(CONCAT(?pageStart, ""-"", ?pageEnd) AS ?paginas) .
                                                OPTIONAL{{?s bibo:doi ?doi . }}
                                                OPTIONAL{{?s bibo:handle ?handle . }} 
                                                OPTIONAL{{?s bibo:pmid ?pmid . }}
                                                OPTIONAL{{?s roh:openAccess ?openAccess . }}
                                                FILTER(?s = <{pIdDocumento}>) 
                                            }} ";

                resultadoQuery = mResourceApi.VirtuosoQueryMultipleGraph(selectObtencionProyecto, whereObtencionProyecto, new List<string> { "document", "documentformat", "publicationtype" });

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
                                else if (item == "title")
                                {
                                    if (fila[item].value != "-")
                                    {
                                        campo.valores = new List<string>() { "[Petición de borrado] " + fila[item].value };
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
                if (listaValores != null && listaValores.Any())
                {
                    CampoProduccionCientifica campoAux = new CampoProduccionCientifica();
                    campoAux.codigoCVN = "060.010.010.400";
                    campoAux.valores = listaValores;
                    PRC.campos.Add(campoAux);
                }
                if (listaIds != null && listaIds.Any())
                {
                    CampoProduccionCientifica campoAux = new CampoProduccionCientifica();
                    campoAux.codigoCVN = "060.010.010.410";
                    campoAux.valores = listaIds;
                    PRC.campos.Add(campoAux);
                }
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
                string selectDatosCongresos = "";
                string whereDatosCongresos = "";

                selectDatosCongresos = mPrefijos;
                selectDatosCongresos += $@"SELECT DISTINCT ?title ?type ?supportType ?fechaCelebracion ?fechaFinalizacion ?tipoEvento ?geographicFocus ?presentedAt ?publicationVenueText ?doi ?handle ?pmid ?isbn ?issn ?participationType";
                whereDatosCongresos = $@"WHERE {{
                                            ?s a bibo:Document . 
                                            ?s roh:title ?title . 
                                            OPTIONAL{{
                                                ?s dc:type ?pubTypeAux . 
                                                ?pubTypeAux dc:identifier ?type .
                                            }}
                                            OPTIONAL{{
                                                ?s roh:supportType ?supType .
                                                ?supType dc:identifier ?supportType .
                                            }}
                                            OPTIONAL{{?s roh:presentedAtStart ?fechaCelebracion . }}
                                            OPTIONAL{{?s roh:presentedAtEnd ?fechaFinalizacion . }} 
                                            OPTIONAL{{
                                                ?s roh:presentedAtType ?tipoEventoAux . 
                                                ?tipoEventoAux dc:identifier ?tipoEvento .
                                            }} 
                                            OPTIONAL{{
                                                ?s roh:presentedAtGeographicFocus ?geographicFocusAux .
                                                ?geographicFocusAux dc:identifier ?geographicFocus . 
                                            }}
                                            OPTIONAL{{?s bibo:presentedAt ?presentedAt . }} 
                                            OPTIONAL{{?s roh:hasPublicationVenueText ?publicationVenueText . }}
                                            OPTIONAL{{?s bibo:doi ?doi . }} 
                                            OPTIONAL{{?s bibo:handle ?handle . }} 
                                            OPTIONAL{{?s bibo:pmid ?pmid . }} 
                                            OPTIONAL{{?s roh:isbn ?isbn . }} 
                                            OPTIONAL{{?s bibo:issn ?issn . }} 
                                            OPTIONAL{{
                                                ?s roh:participationType ?participationTypeAux .
                                                ?participationTypeAux dc:identifier ?participationType .
                                            }}
                                            FILTER(?s = <{pIdDocumento}>) 
                                        }} ";

                resultadoQuery = mResourceApi.VirtuosoQueryMultipleGraph(selectDatosCongresos, whereDatosCongresos, new List<string> { "document" , "documentformat" , "publicationtype" });

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
                if (listaValores != null && listaValores.Any())
                {
                    CampoProduccionCientifica campoAux = new CampoProduccionCientifica();
                    campoAux.codigoCVN = "060.010.010.400";
                    campoAux.valores = listaValores;
                    PRC.campos.Add(campoAux);
                }
                if (listaIds != null && listaIds.Any())
                {
                    CampoProduccionCientifica campoAux = new CampoProduccionCientifica();
                    campoAux.codigoCVN = "060.010.010.410";
                    campoAux.valores = listaIds;
                    PRC.campos.Add(campoAux);
                }
            }
            #endregion

            #region --- Envío a SGI.
            try
            {
                IRestResponse response = null;

                if (PRC.epigrafeCVN.Equals("060.010.010.000") && !PRC.campos.Any(x => x.codigoCVN.Equals("060.010.010.010")))
                {
                    throw new Exception("El recurso no tiene tipo de proyecto");
                }

                if (valorEnviado == "rechazado")
                {
                    RestClient client = new($@"{pConfig.GetUrlProduccionCientifica()}/{PRC.idRef}");
                    client.AddDefaultHeader("Authorization", "Bearer " + GetTokenPRC(pConfig));
                    var request = new RestRequest(Method.PUT);
                    request.AddJsonBody(PRC);
                    string jsonString = Newtonsoft.Json.JsonConvert.SerializeObject(PRC);
                    response = client.Execute(request);
                }
                else
                {
                    RestClient client = new(pConfig.GetUrlProduccionCientifica());
                    client.AddDefaultHeader("Authorization", "Bearer " + GetTokenPRC(pConfig));
                    var request = new RestRequest(Method.POST);
                    request.AddJsonBody(PRC);
                    string jsonString = Newtonsoft.Json.JsonConvert.SerializeObject(PRC);
                    response = client.Execute(request);
                }

                if ((int)response.StatusCode < 200 || (int)response.StatusCode >= 300)
                {
                    throw new Exception(response.StatusCode.ToString() + ", " + response.Content);
                }
            }
            catch (Exception)
            {
                throw;
            }
            #endregion

            #region --- Cambio del estado del envío.           
            mResourceApi.ChangeOntoly("document");
            Guid guid = mResourceApi.GetShortGuid(pIdDocumento);

            if (string.IsNullOrEmpty(valorEnviado))
            {
                // Inserción.
                Insercion(guid, "http://w3id.org/roh/validationDeleteStatusPRC", "pendiente");
            }
            else
            {
                // Modificación.
                Modificacion(guid, "http://w3id.org/roh/validationDeleteStatusPRC", "pendiente", valorEnviado);
            }
            #endregion
        }

        private ProduccionCientifica CrearPRC(string pIdDocumento,bool pEliminar)
        {
            ProduccionCientifica PRC = new ProduccionCientifica();
            string propStatus = "";
            // Identificador.
            if (pEliminar)
            {
                PRC.idRef = "Eliminar_" + pIdDocumento.Split('/').LastOrDefault();
                propStatus = "roh:validationDeleteStatusPRC";
            }
            else
            {
                PRC.idRef = pIdDocumento.Split('/').LastOrDefault();
                propStatus = "roh:validationStatusPRC";
            }
            PRC.estado = "PENDIENTE";
            PRC.campos = new List<CampoProduccionCientifica>();

            #region --- Estado de validación
            // Comprobar si está el triple del estado.
            string valorEnviado = string.Empty;

            string selectEstadoValidacion = mPrefijos;
            selectEstadoValidacion += "SELECT DISTINCT ?enviado ";
            string whereEstadoValidacion = $@"WHERE {{
                                                ?s a bibo:Document . 
                                                OPTIONAL{{?s {propStatus} ?enviado . }} 
                                                FILTER(?s = <{pIdDocumento}>) 
                                            }} ";

            SparqlObject resultadoQuery = mResourceApi.VirtuosoQuery(selectEstadoValidacion, whereEstadoValidacion, "document");

            if (resultadoQuery != null && resultadoQuery.results != null && resultadoQuery.results.bindings != null && resultadoQuery.results.bindings.Count > 0)
            {
                foreach (Dictionary<string, SparqlObject.Data> fila in resultadoQuery.results.bindings)
                {
                    valorEnviado = UtilidadesAPI.GetValorFilaSparqlObject(fila, "enviado");
                }
            }
            #endregion

            #region --- Tipo del Documento.

            // Consulta sparql (Tipo del documento).
            string selectTipoDoc = mPrefijos;
            selectTipoDoc += "SELECT DISTINCT ?tipoDocumento ";
            string whereTipoDoc = $@"WHERE {{ 
                                ?s a bibo:Document . 
                                ?s roh:scientificActivityDocument ?tipoDocumento . 
                                FILTER(?s = <{pIdDocumento}>) 
                            }} ";

            resultadoQuery = mResourceApi.VirtuosoQuery(selectTipoDoc, whereTipoDoc, "document");

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
                    }
                }
            }
            #endregion

            #region --- Autores.
            // Consulta sparql (Obtención de datos de la persona).
            string selectAutores = mPrefijos;
            selectAutores += "SELECT DISTINCT ?crisIdentifier ?orcid ?orden ?nombre ?apellidos ?firma ";
            string whereAutores = $@"WHERE {{
                                ?s a bibo:Document . 
                                OPTIONAL{{
                                    ?s bibo:authorList ?listaAutores . 
                                    ?listaAutores rdf:member ?persona .
                                    ?listaAutores rdf:comment ?orden .
                                    ?persona foaf:firstName ?nombre .
                                    ?persona foaf:lastName ?apellidos .
                                    OPTIONAL{{?persona roh:crisIdentifier ?crisIdentifier . }} 
                                    OPTIONAL{{?persona roh:ORCID ?orcid . }} 
                                    OPTIONAL{{?persona foaf:nick ?firma . }} 
                                }} 
                                FILTER(?s = <{pIdDocumento}>) 
                            }} ORDER BY ?orden ";

            resultadoQuery = mResourceApi.VirtuosoQueryMultipleGraph(selectAutores, whereAutores, new List<string> { "document", "person" });

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
            return PRC;
        }
        private string ObtenerIdDocumento(string pIdRecurso)
        {
            string pIdDocumento = "";
            string selectProyecto = "select distinct ?documento";
            string whereProyecto = $@"where{{
    <{pIdRecurso}> <http://vivoweb.org/ontology/core#relatedBy> ?documento .
}}";
            SparqlObject query = mResourceApi.VirtuosoQuery(selectProyecto, whereProyecto, "curriculumvitae");
            if (query.results.bindings.Count != 0)
            {
                pIdDocumento = query.results.bindings[0]["documento"].value;                
            }
            return pIdDocumento;
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
            //060.010.010.210 - Nombre de la revista
            dicPropiedadesPublicaciones.Add("isbn", "060.010.010.160");
            dicPropiedadesPublicaciones.Add("issn", "060.010.010.160");
            //060.010.010.100 - Editorial
            // Volume e Issue
            dicPropiedadesPublicaciones.Add("numVol", "060.010.010.080");
            // PageEnd y PageStart
            dicPropiedadesPublicaciones.Add("paginas", "060.010.010.090");
            //060.010.010.400 - Identificadores digitales 
            //060.010.010.410 - Tipo identificadores digitales
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
            dicPropiedadesCongresos.Add("type", "060.010.010.010");
            dicPropiedadesCongresos.Add("supportType", "060.010.010.070");
            dicPropiedadesCongresos.Add("fechaCelebracion", "060.010.020.190");
            dicPropiedadesCongresos.Add("fechaFinalizacion", "060.010.020.380");
            dicPropiedadesCongresos.Add("tipoEvento", "060.010.020.010");
            dicPropiedadesCongresos.Add("geographicFocus", "060.010.020.080");
            dicPropiedadesCongresos.Add("presentedAt", "060.010.020.100");
            dicPropiedadesCongresos.Add("publicationVenueText", "060.010.020.370");
            dicPropiedadesCongresos.Add("issn", "060.010.020.320");
            dicPropiedadesCongresos.Add("isbn", "060.010.020.320");
            dicPropiedadesCongresos.Add("participationType", "060.010.020.050");
            dicPropiedadesCongresos.Add("doi", "");
            dicPropiedadesCongresos.Add("handle", "");
            dicPropiedadesCongresos.Add("pmid", "");
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
