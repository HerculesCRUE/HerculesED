using Gnoss.ApiWrapper;
using Gnoss.ApiWrapper.ApiModel;
using Gnoss.ApiWrapper.Model;
using Harvester.Models;
using Newtonsoft.Json;
using OAI_PMH.Models.SGI.Organization;
using OAI_PMH.Models.SGI.PersonalData;
using OAI_PMH.Models.SGI.Project;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Harvester
{
    class Loader
    {
        //Syncs
        private static readonly string Syncs = @"C:\Data\Syncs.json";

        //Resource API
        public static ResourceApi mResourceApi { get; set; }

        public static void LoadMainEntities()
        {
            Harvester h = new(new IHarvesterServices());

            //Harvest organizations
            List<Empresa> organizationList = h.HarvestOrganizations();

            //Harvest people
            List<Persona> peopleList = h.HarvestPeople();

            //Harvest projects
            List<Proyecto> projectList = h.HarvestProjects();

            //Resource list to load
            List<ComplexOntologyResource> resourcesToLoadList = new();

            //Load organizations
            ChangeOntology("organization");
            Dictionary<string, string> organizationsToLoad = GetOrganizations(ref resourcesToLoadList, peopleList, organizationList);
            resourcesToLoadList.Clear();

            //Load people
            ChangeOntology("person");
            Dictionary<string, string> peopleToLoad = GetPeople(ref resourcesToLoadList, peopleList, organizationsToLoad);
            resourcesToLoadList.Clear();

            //Load projects
            ChangeOntology("project");
            //Dictionary<string, string> projectsToLoad = GetProjects(peopleLoad, peopleToLoad, ref resourcesToLoadList, peopleList, organizationsToLoad);
            resourcesToLoadList.Clear();

            //Write sync date to file
            WriteSyncDate(resourcesToLoadList.Count);
        }

        private static Dictionary<string, string> GetOrganizations(ref List<ComplexOntologyResource> pResourcesToLoadList, List<Persona> pPeopleList, List<Empresa> pOrganizationList)
        {
            Dictionary<string, string> organizationsDic = new();

            foreach (Empresa organization in pOrganizationList)
            {
                //Check if organization exits
                string select = "SELECT * ";
                string where = "WHERE { ?s <http://w3id.org/roh/crisIdentifier> " + organization.Id + " }";
                SparqlObject result = mResourceApi.VirtuosoQuery(select, where, "organization");

                //If it does, modify resource
                if (result != null)
                {

                }
                //If not, create resource
                else
                {
                    OrganizationOntology.Organization newOrganization = new();
                }
            }
            return organizationsDic;
        }

        private static Dictionary<string, string> GetPeople(ref List<ComplexOntologyResource> pResourcesToLoadList, List<Persona> pPeopleList, Dictionary<string, string> organizationsToLoad)
        {
            Dictionary<string, string> peopleDic = new();

            foreach (Persona person in pPeopleList)
            {
                //Check if person exits
                string select = "SELECT * ";
                string where = "WHERE { ?s <http://w3id.org/roh/crisIdentifier> " + person.Id + " }";
                SparqlObject result = mResourceApi.VirtuosoQuery(select, where, "person");

                //If they do, modify resource
                if (result != null)
                {

                }
                //If not, create resource
                else
                {
                    //PersonOntology.Person newPerson = new();
                    //newPerson.Foaf_firstName = person.Nombre;
                    //newPerson.Foaf_lastName = person.Apellidos;
                    //newPerson.Foaf_name = person.Nombre + " " + person.Apellidos;
                    //newPerson.Roh_isSynchronized = true;
                    //newPerson.Roh_hasPosition = person.;
                    //newPerson.IdVivo_departmentOrSchool = person.Vinculacion.DepartamentoPDI.ToString();
                    //newPerson.Vivo_hasResearchArea = person.

                }
            }
            return peopleDic;
        }

        private static Dictionary<string, string> GetProjects(HashSet<string> pPeopleToLoad, Dictionary<string, string> pLoadedPeopleDic, Dictionary<string, string> pLoadedOrganizationsList, ref List<ComplexOntologyResource> pResourcesToLoadList, List<Proyecto> pProjectList, Dictionary<string, string> pOrganizationsToLoad, Dictionary<string, string> pPersonList)
        {
            Dictionary<string, string> projectsDic = new();
            return projectsDic;
        }

        private static void ChangeOntology(string pOntology)
        {
            mResourceApi.ChangeOntoly(pOntology);
            Thread.Sleep(1000);
            while (mResourceApi.OntologyNameWithoutExtension != pOntology)
            {
                mResourceApi.ChangeOntoly(pOntology);
                Thread.Sleep(1000);
            }
        }

        private static void LoadData(List<ComplexOntologyResource> pResourcesToLoadList)
        {
            foreach (ComplexOntologyResource resourceToLoad in pResourcesToLoadList)
            {
                mResourceApi.LoadComplexSemanticResource(resourceToLoad);
            }
        }

        public static void WriteSyncDate(int resourcesNumber)
        {
            string syncs = File.ReadAllText(Syncs);
            List<Sync> syncList = JsonConvert.DeserializeObject<List<Sync>>(syncs);
            syncList.Add(new Sync()
            {
                Date = DateTime.UtcNow.ToString("yyyy-MM-ddThh:mm:ssZ"),
                LoadedResourcesNumber = resourcesNumber
            });
            syncs = JsonConvert.SerializeObject(syncList);
            File.WriteAllText(Syncs, syncs);
        }
    }
}