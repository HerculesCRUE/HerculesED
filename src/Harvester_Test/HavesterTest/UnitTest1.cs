using Harvester;
using Harvester.Models;
using Newtonsoft.Json;
using OAI_PMH.Models.SGI.Organization;
using OAI_PMH.Models.SGI.PersonalData;
using OAI_PMH.Models.SGI.Project;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using Xunit;

namespace HavesterTest
{
    public class UnitTest1
    {
        [Fact]
        public void TestHarvestPeople()
        {

            try
            {
                Harvester.Harvester h = new Harvester.Harvester(new HarvesterServicesMock());
                var lista = h.HaversterServices.ListIdentifiers("xml_examples//identifiers_persona.xml");
                Persona person = new();
                List<Persona> peopleList = new();
                List<IdentifierOAIPMH> personIdList = lista;
                string id = "Persona_34898526";
                string xml = h.HaversterServices.GetRecord(id,"xml_examples//record_persona.xml");
                XmlSerializer serializer = new(typeof(Persona));
                using (StringReader sr = new(xml))
                {
                    person = (Persona)serializer.Deserialize(sr);
                }
                peopleList.Add(person);
                Assert.True(peopleList.Count > 0,"Test Harvest People");


            }
            catch
            {
                Assert.True(false,"error en el test HarvestPeople");
            }            



        } 
        [Fact]
        public void TestHarvestOrganizations()
        {

            try
            {
            //to do 
            throw new NotImplementedException("falta el archivo de empresas");

            Empresa organization = new();
            List<Empresa> organizationsList = new();

                Harvester.Harvester h = new Harvester.Harvester(new HarvesterServicesMock());

                List<IdentifierOAIPMH> organizationIdList = h.HaversterServices.ListIdentifiers("xml_examples//identifiers_empresa.xml");
                string id = "buscar en el doc el id";
                string xml = h.HaversterServices.GetRecord(id, "");
                XmlSerializer serializer = new(typeof(Empresa));
                using (StringReader sr = new(xml))
                {
                   organization = (Empresa)serializer.Deserialize(sr);
                }
                organizationsList.Add(organization);
                Assert.True(organizationsList.Count>0,"Test harvest organizations");



            }

            catch
            {
                Assert.True(false);
            }
            
          

        }
        [Fact]
        public void TestHarvestProjects()
        {
            try { 
            Proyecto project = new();
            List<Proyecto> projectList = new();
            Harvester.Harvester h = new Harvester.Harvester(new HarvesterServicesMock());
            List<IdentifierOAIPMH> projectIdList = h.HaversterServices.ListIdentifiers("xml_examples//identifiers_proyecto.xml"); ;


            string id = "Proyecto_1";
            string xml = h.HaversterServices.GetRecord(id, "xml_examples//record_proyecto.xml");
                XmlSerializer serializer = new(typeof(Proyecto));
                using (StringReader sr = new(xml))
                {
                    project = (Proyecto)serializer.Deserialize(sr);
                }
                projectList.Add(project);

                Assert.True(projectList.Count > 0, "Test harvest organizations");



            }

            catch (Exception)
            {
                Assert.True(false);
            }

        }

 
      
       
        [Fact]
        public void TestGetLastSyncDated()
        {
            try { 
            string syncs = File.ReadAllText("xml_examples//Syncs.json");
            List<Harvester.Models.Sync> syncList = JsonConvert.DeserializeObject<List<Sync>>(syncs);

            string firstSyncDate = syncList[0].Date;
            string lastSyncDate = syncList[^1].Date;

            Assert.True(firstSyncDate.Equals("2021-10-15T15:04:49Z"));
            Assert.True(lastSyncDate.Equals("2021 - 10 - 15T00: 00:00Z"));

               }
            catch
            {
                Assert.True(false, "Fallo en test get las Sync Dated");
            }
        }


    }
}
