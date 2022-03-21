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
            Dictionary<string, string> organizationsToLoad = GetOrganizations(ref resourcesToLoadList, organizationList);
            //TODO: Cargar Organizaciones
            resourcesToLoadList.Clear();

            //Load people
            ChangeOntology("person");
            Dictionary<string, string> peopleToLoad = GetPeople(ref resourcesToLoadList, peopleList);
            //TODO: Cargar Personas
            resourcesToLoadList.Clear();

            //Load projects
            ChangeOntology("project");
            Dictionary<string, string> projectsToLoad = GetProjects(ref resourcesToLoadList, projectList, peopleToLoad);
            //TODO: Cargar Proyectos
            resourcesToLoadList.Clear();

            //Write sync date to file
            WriteSyncDate(resourcesToLoadList.Count);
        }

        private static Dictionary<string, string> GetOrganizations(ref List<ComplexOntologyResource> pResourcesToLoadList, List<Empresa> pOrganizationList)
        {
            Dictionary<string, string> organizationsDic = new();

            foreach (Empresa organization in pOrganizationList)
            {
                OrganizationOntology.Organization organizationOntology = CrearOrganizacion(organization);

                //Guardamos los IDs en la lista.
                if (!organizationsDic.ContainsKey(organizationOntology.Roh_crisIdentifier))
                {
                    //Creamos el recurso.
                    ComplexOntologyResource resource = organizationOntology.ToGnossApiResource(mResourceApi, new List<string>());
                    pResourcesToLoadList.Add(resource);
                    organizationsDic.Add(organizationOntology.Roh_crisIdentifier, resource.GnossId);
                }
            }

            return organizationsDic;
        }

        private static Dictionary<string, string> GetPeople(ref List<ComplexOntologyResource> pResourcesToLoadList, List<Persona> pPeopleList)
        {
            Dictionary<string, string> peopleDic = new();

            foreach (Persona person in pPeopleList)
            {
                PersonOntology.Person personOntology = CrearPersona(person);

                //Guardamos los IDs en la lista.
                if (!peopleDic.ContainsKey(personOntology.Roh_crisIdentifier))
                {
                    //Creamos el recurso.
                    ComplexOntologyResource resource = personOntology.ToGnossApiResource(mResourceApi, new List<string>());
                    pResourcesToLoadList.Add(resource);
                    peopleDic.Add(personOntology.Roh_crisIdentifier, resource.GnossId);
                }
            }

            return peopleDic;
        }

        private static Dictionary<string, string> GetProjects(ref List<ComplexOntologyResource> pResourcesToLoadList, List<Proyecto> pProjectList, Dictionary<string, string> pDicPersonasGnossId)
        {
            Dictionary<string, string> projectsDic = new();

            foreach (Proyecto project in pProjectList)
            {
                if(project.CodigoExterno != "PCGFOPE2")
                {
                    continue;
                }

                ProjectOntology.Project projectOntology = CrearProyecto(project, pDicPersonasGnossId);

                //Guardamos los IDs en la lista.
                if (!projectsDic.ContainsKey(projectOntology.Roh_crisIdentifier))
                {
                    //Creamos el recurso.
                    //ComplexOntologyResource resource = projectOntology.ToGnossApiResource(mResourceApi, new List<string>());
                    //pResourcesToLoadList.Add(resource);
                    //projectsDic.Add(projectOntology.Roh_crisIdentifier, resource.GnossId);
                }
            }


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

        public static PersonOntology.Person CrearPersona(Persona pDatos)
        {
            PersonOntology.Person persona = new PersonOntology.Person();
            persona.Roh_crisIdentifier = pDatos.Id;
            persona.Roh_isSynchronized = true;
            if (!string.IsNullOrEmpty(pDatos.Nombre))
            {
                persona.Foaf_firstName = pDatos.Nombre;
            }
            if (!string.IsNullOrEmpty(pDatos.Apellidos))
            {
                persona.Foaf_lastName = pDatos.Apellidos;
            }
            if (!string.IsNullOrEmpty(pDatos.Nombre) && !string.IsNullOrEmpty(pDatos.Apellidos))
            {
                persona.Foaf_name = pDatos.Nombre + " " + pDatos.Apellidos;
            }
            if (pDatos.Emails != null && pDatos.Emails.Any())
            {
                persona.Vcard_email = new List<string>();
                foreach (Email item in pDatos.Emails)
                {
                    persona.Vcard_email.Add(item.email);
                }
            }
            if (pDatos.DatosContacto.Telefonos != null && pDatos.DatosContacto.Telefonos.Any())
            {
                persona.Vcard_hasTelephone = new List<string>();
                foreach (string item in pDatos.DatosContacto.Telefonos)
                {
                    persona.Vcard_hasTelephone.Add(item);
                }
            }
            if (pDatos.DatosContacto.Moviles != null && pDatos.DatosContacto.Moviles.Any())
            {
                //persona.Vcard_hasMobilePhone = new List<string>();
                foreach (string item in pDatos.DatosContacto.Moviles)
                {
                    //persona.Vcard_hasMobilePhone.Add(item);
                }
            }
            if (pDatos.Activo.HasValue)
            {
                persona.Roh_isActive = pDatos.Activo.Value;
            }
            if(pDatos.TipoDocumento != null && !string.IsNullOrEmpty(pDatos.TipoDocumento.Id))
            {
                switch(pDatos.TipoDocumento.Id)
                {
                    case "D":
                        //persona.Roh_dni = pDatos.TipoDocumento.Nombre;
                        break;
                    case "E":
                        //persona.Roh_nie = pDatos.TipoDocumento.Nombre;
                        break;
                }
            }
            if(pDatos.DatosPersonales != null)
            {
                //persona.Vcard_birthdate = pDatos.DatosPersonales.Fechanacimiento;
            }
            if(pDatos.Fotografia != null && !string.IsNullOrEmpty(pDatos.Fotografia.Contenido))
            {
                //persona.Foaf_img = pDatos.Fotografia.Contenido;
            }
            if(pDatos.Sexo != null && !string.IsNullOrEmpty(pDatos.Sexo.Id))
            {
                switch(pDatos.Sexo.Id)
                {
                    case "V":
                        //persona.IdFoaf_gender = $@"http://gnoss.com/items/gender_000";
                        break;
                    case "M":
                        //persona.IdFoaf_gender = $@"http://gnoss.com/items/gender_010";
                        break;
                }
            }

            // TODO: Posible cambio Treelogic
            if (pDatos.Vinculacion != null && pDatos.Vinculacion.Departamento != null && !string.IsNullOrEmpty(pDatos.Vinculacion.Departamento.Id))
            {
                persona.IdVivo_departmentOrSchool = $@"http://gnoss.com/items/department_{pDatos.Vinculacion.Departamento.Id}";
            }
            if (pDatos.Vinculacion != null && pDatos.Vinculacion.CategoriaProfesional != null && !string.IsNullOrEmpty(pDatos.Vinculacion.CategoriaProfesional.Nombre))
            {
                persona.Roh_hasPosition = pDatos.Vinculacion.CategoriaProfesional.Nombre;
            }

            return persona;
        }
        public static OrganizationOntology.Organization CrearOrganizacion(Empresa pDatos)
        {
            OrganizationOntology.Organization organization = new OrganizationOntology.Organization();
            organization.Roh_crisIdentifier = pDatos.Id;
            organization.Roh_isSynchronized = true;
            organization.Roh_title = pDatos.Nombre;
            return organization;
        }
        public static ProjectOntology.Project CrearProyecto(Proyecto pDatos, Dictionary<string, string> pDicPersonasGnossId)
        {
            ProjectOntology.Project project = new ProjectOntology.Project();
            project.Roh_crisIdentifier = pDatos.Id;
            project.Roh_isSynchronized = true;
            
            if(!string.IsNullOrEmpty(pDatos.Titulo))
            {
                project.Roh_title = pDatos.Titulo;
            }
            if(!string.IsNullOrEmpty(pDatos.Observaciones))
            {
                project.Vivo_description = pDatos.Observaciones;
            }
            if(!string.IsNullOrEmpty(pDatos.Acronimo))
            {
                project.Vivo_abbreviation = pDatos.Acronimo;
            }
            if(pDatos.Equipo != null && pDatos.Equipo.Any())
            {
                project.Vivo_relates = new List<ProjectOntology.BFO_0000023>();
                int orden = 1;
                foreach(ProyectoEquipo item in pDatos.Equipo)
                {
                    ProjectOntology.BFO_0000023 persona = new ProjectOntology.BFO_0000023();
                    persona.Rdf_comment = orden;
                    if(pDicPersonasGnossId != null && pDicPersonasGnossId.ContainsKey(item.PersonaRef))
                    {
                        persona.IdRdf_member = pDicPersonasGnossId[item.PersonaRef];
                    }
                    //TODO: Fecha 
                    //persona.Vivo_start = item.FechaInicio;
                    //persona.Vivo_end = item.FechaFin;
                    project.Vivo_relates.Add(persona);
                    orden++;
                }
            }
            if(project.Vivo_relates != null && project.Vivo_relates.Any())
            {
                project.Roh_researchersNumber = project.Vivo_relates.Count();
            }
            else
            {
                project.Roh_researchersNumber = 0;
            }

            // TODO: Continuar el desarrollo...

            return project;
        }
    }
}