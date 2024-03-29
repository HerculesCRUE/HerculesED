﻿using EditorCV.Controllers;
using EditorCV.Models.ValidacionProyectos;
using Gnoss.ApiWrapper;
using Gnoss.ApiWrapper.ApiModel;
using Gnoss.ApiWrapper.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EditorCV.Models.Utils;

namespace EditorCV.Models
{
    public class AccionesEnvioProyecto: AccionesEnvio
    {


        /// <summary>
        /// Envia un proyecto a validación.
        /// </summary>
        /// <param name="pConfig">Configuración.</param>
        /// <param name="pIdRecurso">ID del recurso que apunta al proyecto.</param>
        /// <param name="pIdPersona">ID del recurso de la persona.</param>
        /// <param name="pIdAutorizacion">ID del recurso de la autorización.</param>
        public void EnvioProyecto(ConfigService pConfig, string pIdRecurso, string pIdPersona, string pIdAutorizacion)
        {
            string pIdProyecto = "";
            string selectProyecto = "select distinct ?proyecto";
            string whereProyecto = $@"where{{
    <{pIdRecurso}> <http://vivoweb.org/ontology/core#relatedBy> ?proyecto .
}}";
            SparqlObject query = mResourceApi.VirtuosoQuery(selectProyecto, whereProyecto, "curriculumvitae");
            if (query.results.bindings.Count != 0)
            {
                foreach (Dictionary<string, SparqlObject.Data> res in query.results.bindings)
                {
                    pIdProyecto = res["proyecto"].value;
                }
            }
            if (string.IsNullOrEmpty(pIdProyecto))
            {
                return;
            }

            NotificacionProyecto proyecto = CrearProyecto(pIdProyecto, pIdPersona, pIdAutorizacion);
            try
            {
                RestClient client = new(pConfig.GetUrlEnvioProyecto());
                client.AddDefaultHeader("Authorization", "Bearer " + GetTokenCSP(pConfig));
                var request = new RestRequest(Method.POST);
                request.AddJsonBody(proyecto);
                IRestResponse response = client.Execute(request);
                if ((int)response.StatusCode < 200 || (int)response.StatusCode >= 300)
                {
                    throw new InvalidOperationException("Response not 200");
                }
            }
            catch (Exception)
            {
                //
            }

            CambioEstadoEnvio(pIdProyecto);
        }

        /// <summary>
        /// Cambia el estado del proyecto <paramref name="pIdProyecto"/> a enviado o pendiente.
        /// </summary>
        /// <param name="pIdProyecto">Identificador del proyecto</param>
        public void CambioEstadoEnvio(string pIdProyecto)
        {
            // Comprobar si está el triple del estado.
            string valorEnviado = string.Empty;
            string select = "";
            string where = "";

            select += mPrefijos;
            select += "SELECT DISTINCT ?enviado ";
            where = @$"WHERE {{ 
                        ?s a vivo:Project .
                        OPTIONAL{{?s roh:validationStatusProject ?enviado. }}
                        FILTER(?s = <{pIdProyecto}>)
                    }}";

            SparqlObject resultadoQuery = mResourceApi.VirtuosoQuery(select.ToString(), where.ToString(), "project");
            foreach (Dictionary<string, SparqlObject.Data> fila in resultadoQuery.results.bindings)
            {
                valorEnviado = UtilidadesAPI.GetValorFilaSparqlObject(fila, "enviado");
            }

            mResourceApi.ChangeOntoly("document");
            Guid guid = mResourceApi.GetShortGuid(pIdProyecto);

            if (string.IsNullOrEmpty(valorEnviado))
            {
                // Inserción.
                Insercion(guid, "http://w3id.org/roh/validationStatusProject", "pendiente");
            }
            else
            {
                // Modificación.
                Modificacion(guid, "http://w3id.org/roh/validationStatusProject", "pendiente", valorEnviado);
            }
        }

        /// <summary>
        /// Crea el proyecto notificado para enviar a validar.
        /// </summary>
        /// <param name="pIdProyecto">ID del recurso del proyecto.</param>
        /// <param name="pIdPersona">ID del recurso de la persona.</param>
        /// <param name="pIdAutorizacion">ID del recurso de la autorizacion.</param>
        /// <returns></returns>
        public NotificacionProyecto CrearProyecto(string pIdProyecto, string pIdPersona, string pIdAutorizacion)
        {
            // Obtención de datos de Proyecto.
            Dictionary<string, string> dicDatosProyecto = GetDatosProyecto(pIdProyecto);

            NotificacionProyecto notificacion = new NotificacionProyecto();
            notificacion.proyectoCVNId = pIdProyecto;
            //notificacion.proyectoCVNId = mResourceApi.GetShortGuid(pIdProyecto).ToString();
            notificacion.autorizacionId = GetAutorizacion(pIdAutorizacion); // Obtención del crisIdentifier de la autorización.
            notificacion.solicitanteRef = GetSolicitanteRef(pIdPersona); // Obtención del crisIdentifier de la persona solicitante.
            notificacion.titulo = dicDatosProyecto["titulo"];
            notificacion.fechaInicio = dicDatosProyecto["fechaInicio"];
            notificacion.fechaFin = dicDatosProyecto["fechaFin"];

            return notificacion;
        }

        /// <summary>
        /// Obtiene los datos de los Proyectos a enviar a validación.
        /// </summary>
        /// <param name="pIdProyecto">ID del recurso del proyecto.</param>
        /// <returns>Datos necesarios del proyecto.</returns>
        public Dictionary<string, string> GetDatosProyecto(string pIdProyecto)
        {
            Dictionary<string, string> dicResultados = new Dictionary<string, string>();
            StringBuilder select = new StringBuilder(), where = new StringBuilder();

            select.Append(mPrefijos);
            select.Append("SELECT ?titulo ?fechaInicio ?fechaFin ");
            where.Append("WHERE { ");
            where.Append("?s a vivo:Project. ");
            where.Append("?s roh:title ?titulo. ");
            where.Append("OPTIONAL{ ?s vivo:start ?fechaInicio. } ");
            where.Append("OPTIONAL{ ?s vivo:end ?fechaFin. } ");
            where.Append($@"FILTER(?s = <{pIdProyecto}>) ");
            where.Append("} ");

            SparqlObject resultadoQuery = mResourceApi.VirtuosoQuery(select.ToString(), where.ToString(), "project");

            if (resultadoQuery != null && resultadoQuery.results != null && resultadoQuery.results.bindings != null && resultadoQuery.results.bindings.Count > 0)
            {
                foreach (Dictionary<string, SparqlObject.Data> fila in resultadoQuery.results.bindings)
                {
                    if (fila.ContainsKey("titulo"))
                    {
                        dicResultados["titulo"] = UtilidadesAPI.GetValorFilaSparqlObject(fila, "titulo");
                    }
                    if (fila.ContainsKey("fechaInicio"))
                    {
                        string fecha = "2000-01-01";
                        if (!string.IsNullOrEmpty(UtilidadesAPI.GetValorFilaSparqlObject(fila, "fechaInicio")))
                        {
                            fecha = ConstruirFecha(UtilidadesAPI.GetValorFilaSparqlObject(fila, "fechaInicio"));
                        }
                        dicResultados["fechaInicio"] = fecha;
                    }
                    if (fila.ContainsKey("fechaFin"))
                    {
                        string fecha = "3000-01-01";
                        if (!string.IsNullOrEmpty(UtilidadesAPI.GetValorFilaSparqlObject(fila, "fechaFin")))
                        {
                            fecha = ConstruirFecha(UtilidadesAPI.GetValorFilaSparqlObject(fila, "fechaFin"));
                        }
                        dicResultados["fechaFin"] = fecha;
                    }
                }
            }

            return dicResultados;
        }

        /// <summary>
        /// Obtiene el ID de la persona que va a solicitar la autorización.
        /// </summary>
        /// <param name="pIdPersona">ID del recurso de la persona.</param>
        /// <returns>Identificador de la persona.</returns>
        public string GetSolicitanteRef(string pIdPersona)
        {
            StringBuilder select = new StringBuilder(), where = new StringBuilder();

            select.Append(mPrefijos);
            select.Append("SELECT DISTINCT ?crisIdentifier ");
            where.Append("WHERE { ");
            where.Append("?s a foaf:Person. ");
            where.Append("?s roh:crisIdentifier ?crisIdentifier. ");
            where.Append($@"FILTER(?s = <{pIdPersona}>) ");
            where.Append("} ");

            SparqlObject resultadoQuery = mResourceApi.VirtuosoQuery(select.ToString(), where.ToString(), "person");

            if (resultadoQuery != null && resultadoQuery.results != null && resultadoQuery.results.bindings != null && resultadoQuery.results.bindings.Count > 0)
            {
                return UtilidadesAPI.GetValorFilaSparqlObject(resultadoQuery.results.bindings[0], "crisIdentifier");
            }

            return string.Empty;
        }

        /// <summary>
        /// Obtiene el ID de la autorización.
        /// </summary>
        /// <param name="pIdAutorizacion">ID del recurso de la autorización.</param>
        /// <returns>Identificador de la autorización.</returns>
        public int GetAutorizacion(string pIdAutorizacion)
        {
            StringBuilder select = new StringBuilder(), where = new StringBuilder();

            select.Append(mPrefijos);
            select.Append("SELECT DISTINCT ?crisIdentifier ");
            where.Append("WHERE { ");
            where.Append("?s a roh:ProjectAuthorization. ");
            where.Append("?s roh:crisIdentifier ?crisIdentifier. ");
            where.Append($@"FILTER(?s = <{pIdAutorizacion}>) ");
            where.Append("} ");

            SparqlObject resultadoQuery = mResourceApi.VirtuosoQuery(select.ToString(), where.ToString(), "projectauthorization");

            if (resultadoQuery != null && resultadoQuery.results != null && resultadoQuery.results.bindings != null && resultadoQuery.results.bindings.Count > 0)
            {
                return int.Parse(resultadoQuery.results.bindings[0]["crisIdentifier"].value.Split("|").Last());
            }

            return 0;
        }

        /// <summary>
        /// Formatea la fecha.
        /// </summary>
        /// <param name="pFechaSparql">Fecha en formato SPARQL.</param>
        /// <returns>Fecha formateada.</returns>
        public string ConstruirFecha(string pFechaSparql)
        {
            string dia = pFechaSparql.Substring(6, 2);
            string mes = pFechaSparql.Substring(4, 2);
            string anyo = pFechaSparql.Substring(0, 4);
            return $@"{anyo}-{mes}-{dia}T00:00:00Z";
        }
    }
}
