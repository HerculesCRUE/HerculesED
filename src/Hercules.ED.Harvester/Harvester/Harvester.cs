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
    public class Harvester
    {
        //Syncs
        private static readonly string Syncs = @"C:\Data\Syncs.json";

        //Last sync date
        private static readonly string LastSyncDate = GetLastSyncDate();
        public IHaversterServices HaversterServices;

        public Harvester(IHaversterServices haversterServices)
        {
            this.HaversterServices = haversterServices; ;
        }





        //Harvest people data
        public List<Persona> HarvestPeople()
        {
            Persona person = new();
            List<Persona> peopleList = new();
            List<IdentifierOAIPMH> personIdList = HaversterServices.ListIdentifiers(LastSyncDate, set:"Persona");

            foreach (var personId in personIdList)
            {
                string id = personId.Identifier;
                // TODO: QUITAR
                //id = "Persona_28710458";
                string xml = HaversterServices.GetRecord(id);
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
        public List<Empresa> HarvestOrganizations()
        {
            Empresa organization = new();
            List<Empresa> organizationsList = new();
            List<IdentifierOAIPMH> organizationIdList = HaversterServices.ListIdentifiers(LastSyncDate, set:"Organizacion");

            foreach (var organizationId in organizationIdList)
            {
                string id = organizationId.Identifier;
                string xml = HaversterServices.GetRecord(id);
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
        public List<Proyecto> HarvestProjects()
        {
            Proyecto project = new();
            List<Proyecto> projectList = new();
            List<IdentifierOAIPMH> projectIdList = HaversterServices.ListIdentifiers(LastSyncDate, set:"Proyecto");

            foreach (var projectId in projectIdList)
            {
                string id = projectId.Identifier;
                string xml = HaversterServices.GetRecord(id);
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

        public static string GetLastSyncDate()
        {
            string syncs = File.ReadAllText(Syncs);
            List<Sync> syncList = JsonConvert.DeserializeObject<List<Sync>>(syncs);
            string lastSyncDate = syncList[^1].Date;
            return lastSyncDate;
        }
    }
}
