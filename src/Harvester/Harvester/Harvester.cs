using Harvester.Models;
using Newtonsoft.Json;
using OAI_PMH.Models.SGI.Organization;
using OAI_PMH.Models.SGI.PersonalData;
using OAI_PMH.Models.SGI.Project;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace Harvester
{
    class Harvester
    {
        //Syncs
        private static readonly string Syncs = @"C:\Data\Syncs.json";

        //Last sync date
        private static readonly string LastSyncDate = GetLastSyncDate();

        //Harvest people data
        public static List<Persona> HarvestPeople()
        {
            Persona person = new();
            List<Persona> peopleList = new();
            List<IdentifierOAIPMH> personIdList = ListIdentifiers(LastSyncDate, set:"Persona");

            foreach (var personId in personIdList)
            {
                string id = personId.Identifier;
                string xml = GetRecord(id);
                XmlSerializer serializer = new(typeof(Persona));
                using (StringReader sr = new(xml))
                {
                    person = (Persona)serializer.Deserialize(sr);
                }
                peopleList.Add(person);
                Console.WriteLine("Fetched " + id);
            }
            return peopleList;
        }

        //Harvest organizations data
        public static List<Empresa> HarvestOrganizations()
        {
            Empresa organization = new();
            List<Empresa> organizationsList = new();
            List<IdentifierOAIPMH> organizationIdList = ListIdentifiers(LastSyncDate, set:"Organizacion");

            foreach (var organizationId in organizationIdList)
            {
                string id = organizationId.Identifier;
                string xml = GetRecord(id);
                XmlSerializer serializer = new(typeof(Empresa));
                using (StringReader sr = new(xml))
                {
                    organization = (Empresa)serializer.Deserialize(sr);
                }
                organizationsList.Add(organization);
                Console.WriteLine("Fetched " + id);
            }
            return organizationsList;
        }

        //Harvest projects data
        public static List<Proyecto> HarvestProjects()
        {
            Proyecto project = new();
            List<Proyecto> projectList = new();
            List<IdentifierOAIPMH> projectIdList = ListIdentifiers(LastSyncDate, set:"Proyecto");

            foreach (var projectId in projectIdList)
            {
                string id = projectId.Identifier;
                string xml = GetRecord(id);
                XmlSerializer serializer = new(typeof(Proyecto));
                using (StringReader sr = new(xml))
                {
                    project = (Proyecto)serializer.Deserialize(sr);
                }
                projectList.Add(project);
                Console.WriteLine("Fetched " + id);
            }
            return projectList;
        }

        //Return list of modified identifiers
        private static List<IdentifierOAIPMH> ListIdentifiers(string from, string until = null, string set = null)
        {
            List<IdentifierOAIPMH> idList = new();
            string uri = "https://localhost:44300/OAI_PMH?verb=ListIdentifiers&metadataPrefix=EDMA";
            if (from != null)
            {
                uri += $"&from={from}";
            }
            if (until != null)
            {
                uri += $"&until={until}";
            }
            if (set != null)
            {
                uri += $"&set={set}";
            }

            WebRequest wrGETURL = WebRequest.Create(uri);
            Stream stream = wrGETURL.GetResponse().GetResponseStream();

            XDocument XMLresponse = XDocument.Load(stream);
            XNamespace nameSpace = XMLresponse.Root.GetDefaultNamespace();
            XElement idListElement = XMLresponse.Root.Element(nameSpace + "ListIdentifiers");

            if (idListElement != null)
            {
                IEnumerable<XElement> headerList = idListElement.Descendants(nameSpace + "header");
                
                foreach (var header in headerList)
                {
                    header.Attribute(nameSpace + "status");
                    string identifier = header.Element(nameSpace + "identifier").Value;
                    string date = header.Element(nameSpace + "datestamp").Value;
                    string setSpec = header.Element(nameSpace + "setSpec").Value;
                    IdentifierOAIPMH identifierOAIPMH = new()
                    {
                        Date = DateTime.Parse(date),
                        Identifier = identifier,
                        Set = setSpec,
                        Deleted = false
                    };
                    idList.Add(identifierOAIPMH);
                }
            }
            return idList;
        }

        private static string GetRecord(string id)
        {
            string uri = "https://localhost:44300/OAI_PMH?verb=GetRecord&identifier=" + id + "&metadataPrefix=EDMA";

            WebRequest wrGETURL = WebRequest.Create(uri);
            Stream stream = wrGETURL.GetResponse().GetResponseStream();
            XDocument XMLresponse = XDocument.Load(stream);
            XNamespace nameSpace = XMLresponse.Root.GetDefaultNamespace();
            string record = XMLresponse.Root.Element(nameSpace + "GetRecord").Descendants(nameSpace + "metadata").First().FirstNode.ToString();
            record = record.Replace("xmlns=\"" + nameSpace + "\"", "");
            return record;
        }

        public static string GetLastSyncDate()
        {
            string syncs = File.ReadAllText(Syncs);
            List<Sync> syncList = JsonConvert.DeserializeObject<List<Sync>>(syncs);
            string lastSyncDate = syncList[^1].Date;
            return lastSyncDate;
        }
    }
}
