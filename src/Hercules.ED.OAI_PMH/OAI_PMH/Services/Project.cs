﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OAI_PMH.Controllers;
using OAI_PMH.Models.SGI.Project;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OAI_PMH.Services
{
    public class Project
    {
        public static Dictionary<string, DateTime> GetModifiedProjects(string from, ConfigService pConfig)
        {
            string accessToken = Token.CheckToken(pConfig);
            Dictionary<string, DateTime> idDictionary = new();
            List<string> idList = new();
            RestClient client = new(pConfig.GetUrlBaseProyecto() + "proyectos/modificados-ids?q=fechaModificacion=ge=\"" + from + "\"");
            client.AddDefaultHeader("Authorization", "Bearer " + accessToken);
            var request = new RestRequest(Method.GET);
            IRestResponse response = client.Execute(request);

            if (!String.IsNullOrEmpty(response.Content))
            {
                idList = response.Content[1..^1].Split(',').ToList();
                foreach (string id in idList)
                {
                    idDictionary.Add("Proyecto_" + id, DateTime.UtcNow);
                }
            }
            return idDictionary;
        }

        public static Proyecto GetProyecto(string id, ConfigService pConfig)
        {
            string accessToken = Token.CheckToken(pConfig);
            string identifier = id.Split('_')[1];
            Proyecto proyecto = new();
            RestClient client = new(pConfig.GetUrlBaseProyecto() + "proyectos/" + identifier);
            client.AddDefaultHeader("Authorization", "Bearer " + accessToken);
            var request = new RestRequest(Method.GET);
            IRestResponse response = client.Execute(request);
            var json = JObject.Parse(response.Content);
            proyecto = JsonConvert.DeserializeObject<Proyecto>(json.ToString());
            proyecto.Contexto = GetContexto(identifier, pConfig);
            proyecto.Equipo = GetEquipo(identifier, pConfig);
            proyecto.EntidadesGestoras = GetEntidadesGestoras(identifier, pConfig);
            proyecto.EntidadesConvocantes = GetEntidadesConvocantes(identifier, pConfig);
            proyecto.EntidadesFinanciadoras = GetEntidadesFinanciadoras(identifier, pConfig);
            proyecto.ResumenAnualidades = GetAnualidades(identifier, pConfig);
            return proyecto;
        }

        public static ContextoProyecto GetContexto(string id, ConfigService pConfig)
        {
            string accessToken = Token.CheckToken(pConfig);
            ContextoProyecto contexto = new();
            RestClient client = new(pConfig.GetUrlBaseProyecto() + "proyectos/" + id + "/contexto");
            client.AddDefaultHeader("Authorization", "Bearer " + accessToken);
            var request = new RestRequest(Method.GET);
            IRestResponse response = client.Execute(request);
            contexto = JsonConvert.DeserializeObject<ContextoProyecto>(response.Content);
            return contexto;
        }

        public static List<ProyectoEquipo> GetEquipo(string id, ConfigService pConfig)
        {
            string accessToken = Token.CheckToken(pConfig);
            List<ProyectoEquipo> equipo = new();
            RestClient client = new(pConfig.GetUrlBaseProyecto() + "proyectos/" + id + "/equipos");
            client.AddDefaultHeader("Authorization", "Bearer " + accessToken);
            var request = new RestRequest(Method.GET);
            IRestResponse response = client.Execute(request);
            equipo = JsonConvert.DeserializeObject<List<ProyectoEquipo>>(response.Content);
            return equipo;
        }

        public static List<ProyectoEntidadGestora> GetEntidadesGestoras(string id, ConfigService pConfig)
        {
            string accessToken = Token.CheckToken(pConfig);
            List<ProyectoEntidadGestora> entidadesGestoras = new();
            RestClient client = new(pConfig.GetUrlBaseProyecto() + "proyectos/" + id + "/entidadgestoras");
            client.AddDefaultHeader("Authorization", "Bearer " + accessToken);
            var request = new RestRequest(Method.GET);
            IRestResponse response = client.Execute(request);
            entidadesGestoras = JsonConvert.DeserializeObject<List<ProyectoEntidadGestora>>(response.Content);
            return entidadesGestoras;
        }

        public static List<ProyectoEntidadConvocante> GetEntidadesConvocantes(string id, ConfigService pConfig)
        {
            string accessToken = Token.CheckToken(pConfig);
            List<ProyectoEntidadConvocante> entidadesConvocantes = new();
            RestClient client = new(pConfig.GetUrlBaseProyecto() + "proyectos/" + id + "/entidadconvocantes");
            client.AddDefaultHeader("Authorization", "Bearer " + accessToken);
            var request = new RestRequest(Method.GET);
            IRestResponse response = client.Execute(request);
            entidadesConvocantes = JsonConvert.DeserializeObject<List<ProyectoEntidadConvocante>>(response.Content);
            return entidadesConvocantes;
        }

        public static List<ProyectoEntidadFinanciadora> GetEntidadesFinanciadoras(string id, ConfigService pConfig)
        {
            string accessToken = Token.CheckToken(pConfig);
            List<ProyectoEntidadFinanciadora> entidadesFinanciadoras = new();
            RestClient client = new(pConfig.GetUrlBaseProyecto() + "proyectos/" + id + "/entidadfinanciadoras");
            client.AddDefaultHeader("Authorization", "Bearer " + accessToken);
            var request = new RestRequest(Method.GET);
            IRestResponse response = client.Execute(request);
            entidadesFinanciadoras = JsonConvert.DeserializeObject<List<ProyectoEntidadFinanciadora>>(response.Content);
            return entidadesFinanciadoras;
        }

        public static List<ProyectoAnualidadResumen> GetAnualidades(string id, ConfigService pConfig)
        {
            string accessToken = Token.CheckToken(pConfig);
            List<ProyectoAnualidadResumen> anualidades = new();
            RestClient client = new(pConfig.GetUrlBaseProyecto() + "proyectos/" + id + "/anualidades");
            client.AddDefaultHeader("Authorization", "Bearer " + accessToken);
            var request = new RestRequest(Method.GET);
            IRestResponse response = client.Execute(request);
            anualidades = JsonConvert.DeserializeObject<List<ProyectoAnualidadResumen>>(response.Content);
            return anualidades;
        }
    }
}