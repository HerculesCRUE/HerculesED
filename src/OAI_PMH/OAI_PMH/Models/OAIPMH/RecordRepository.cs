using OAI_PMH.Models.SGI;
using OAI_PMH.Models.SGI.Organization;
using OAI_PMH.Models.SGI.PersonalData;
using OAI_PMH.Models.SGI.Project;
using OAI_PMH.Services;
using OaiPmhNet;
using OaiPmhNet.Converters;
using OaiPmhNet.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace OAI_PMH.Models.OAIPMH
{
    public class RecordRepository : IRecordRepository
    {
        private readonly IDateConverter _dateConverter;

        public RecordRepository()
        {
            _dateConverter = new DateConverter();
        }

        public RecordContainer GetIdentifiers(ArgumentContainer arguments, IResumptionToken resumptionToken = null)
        {
            return GetRecords(arguments, resumptionToken);
        }

        public Record GetRecord(string identifier, string metadataPrefix)
        {
            Record record = new();
            string set = identifier.Split('_')[0];
            DateTime date = DateTime.UtcNow;

            switch (set)
            {
                case "Persona":
                    Persona persona = PersonalData.GetPersona(identifier);
                    record = ToRecord(persona, set, identifier, date, metadataPrefix);
                    break;
                case "Proyecto":
                    Proyecto proyecto = Project.GetProyecto(identifier);
                    record = ToRecord(proyecto, set, identifier, date, metadataPrefix);
                    break;
                case "Organizacion":               
                    Empresa organizacion = Organization.GetEmpresa(identifier);
                    record = ToRecord(organizacion, set, identifier, date, metadataPrefix);
                    break;
                default:
                    break;
            }
            return record;
        }

        private static Record ToRecord(SGI_Base pObject, string pSet, string pId, DateTime pDate, string pMetadataPrefix)
        {
            Record record = new()
            {
                Header = new RecordHeader()
                {
                    Identifier = pId,
                    SetSpecs = new List<string>() { pSet },
                    Datestamp = pDate
                }
            };

            switch (pMetadataPrefix)
            {
                case "EDMA":
                    record.Metadata = new RecordMetadata()
                    {
                        Content = XElement.Parse(pObject.ToXML())
                    };
                    break;
            }
            return record;
        }

        public RecordContainer GetRecords(ArgumentContainer arguments, IResumptionToken resumptionToken = null)
        {
            RecordContainer container = new RecordContainer();
            DateTime startDate = DateTime.MinValue;
            if (_dateConverter.TryDecode(arguments.From, out DateTime from))
            {
                startDate = from;
            }

            List<XML> listxml = new();

            if (arguments.Verb == OaiVerb.ListIdentifiers.ToString())
            {
                switch (arguments.Set)
                {
                    case "Persona":
                        Dictionary<string, DateTime> modifiedPeopleIds = PersonalData.GetModifiedPeople(arguments.From);
                        List<Record> personRecordList = new();
                        foreach (string personId in modifiedPeopleIds.Keys)
                        {
                            personRecordList.Add(ToIdentifiersRecord("Persona", personId, modifiedPeopleIds[personId]));
                        }
                        container.Records = personRecordList;
                        break;
                    case "Organizacion":
                        Dictionary<string, DateTime> modifiedOrganizationsIds = Organization.GetModifiedOrganizations(arguments.From);
                        List<Record> organizationRecordList = new();
                        foreach (string organizationId in modifiedOrganizationsIds.Keys)
                        {
                            organizationRecordList.Add(ToIdentifiersRecord("Organizacion", organizationId, modifiedOrganizationsIds[organizationId]));
                        }
                        container.Records = organizationRecordList;
                        break;
                    case "Proyecto":
                        Dictionary<string, DateTime> modifiedProjectsIds = Project.GetModifiedProjects(arguments.From);
                        List<Record> projectRecordList = new();
                        foreach (string projectId in modifiedProjectsIds.Keys)
                        {
                            projectRecordList.Add(ToIdentifiersRecord("Proyecto", projectId, modifiedProjectsIds[projectId]));
                        }
                        container.Records = projectRecordList;
                        break;
                }
            }
            else
            {
                switch (arguments.Set)
                {
                    case "Persona":
                        Dictionary<string, DateTime> modifiedPeopleIds = PersonalData.GetModifiedPeople(arguments.From);
                        List<Persona> peopleList = new();
                        foreach (string personId in modifiedPeopleIds.Keys)
                        {
                            peopleList.Add(PersonalData.GetPersona(personId));
                        }
                        break;
                    case "Organizacion":
                        Dictionary<string, DateTime> modifiedOrganizationsIds = Organization.GetModifiedOrganizations(arguments.From);
                        List<Empresa> organizationsList = new();
                        foreach (string organizationId in modifiedOrganizationsIds.Keys)
                        {
                            organizationsList.Add(Organization.GetEmpresa(organizationId));
                        }
                        break;
                    case "Proyecto":
                        Dictionary<string, DateTime> modifiedProjectsIds = Project.GetModifiedProjects(arguments.From);
                        List<Proyecto> projectsList = new();
                        foreach (string projectId in modifiedProjectsIds.Keys)
                        {
                            projectsList.Add(Project.GetProyecto(projectId));
                        }
                        break;
                }
            }
            return container;
        }

        private static Record ToIdentifiersRecord(string pSet, string pId, DateTime pDate)
        {
            Record record = new()
            {
                Header = new RecordHeader()
                {
                    Identifier = pId,
                    SetSpecs = new List<string>() { pSet },
                    Datestamp = pDate
                }
            };
            return record;
        }
    }
}
