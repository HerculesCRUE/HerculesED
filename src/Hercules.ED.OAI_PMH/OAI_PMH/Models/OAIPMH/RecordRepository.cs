using OAI_PMH.Models.SGI;
using OAI_PMH.Models.SGI.ActividadDocente;
using OAI_PMH.Models.SGI.FormacionAcademica;
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
                case "FormacionAcademica-Ciclos":
                    Ciclos ciclo = AcademicFormation.GetFormacionAcademicaCiclos(identifier);
                    record = ToRecord(ciclo, set, identifier, date, metadataPrefix);
                    break;
                case "FormacionAcademica-Doctorados":
                    Doctorados doctorado = AcademicFormation.GetFormacionAcademicaDoctorados(identifier);
                    record = ToRecord(doctorado, set, identifier, date, metadataPrefix);
                    break;
                case "FormacionAcademica-Posgrado":
                    Posgrado posgrado = AcademicFormation.GetFormacionAcademicaPosgrado(identifier);
                    record = ToRecord(posgrado, set, identifier, date, metadataPrefix);
                    break;
                case "FormacionAcademica-Especializada":
                    FormacionEspecializada especializada = AcademicFormation.GetFormacionAcademicaEspecializada(identifier);
                    record = ToRecord(especializada, set, identifier, date, metadataPrefix);
                    break;
                case "FormacionAcademica-Idiomas":
                    ConocimientoIdiomas idiomas = AcademicFormation.GetFormacionAcademicaIdiomas(identifier);
                    record = ToRecord(idiomas, set, identifier, date, metadataPrefix);
                    break;
                case "Tesis":
                    Tesis tesis = DocentActivity.GetTesis(identifier);
                    record = ToRecord(tesis, set, identifier, date, metadataPrefix);
                    break;
                case "FormacionImpartida":
                    FormacionAcademicaImpartida formacionImpartida = DocentActivity.GetAcademicFormationProvided(identifier);
                    record = ToRecord(formacionImpartida, set, identifier, date, metadataPrefix);
                    break;
                case "Seminarios":
                    SeminariosCursos seminario = DocentActivity.GetSeminars(identifier);
                    record = ToRecord(seminario, set, identifier, date, metadataPrefix);
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
                    case "FormacionAcademica-Ciclos":
                        Dictionary<string, DateTime> modifiedCiclosIds = AcademicFormation.GetModifiedCiclos(arguments.From);
                        List<Record> ciclosRecordList = new();
                        foreach (string id in modifiedCiclosIds.Keys)
                        {
                            ciclosRecordList.Add(ToIdentifiersRecord("FormacionAcademica-Ciclos", id, modifiedCiclosIds[id]));
                        }
                        container.Records = ciclosRecordList;
                        break;
                    case "FormacionAcademica-Doctorados":
                        Dictionary<string, DateTime> modifiedDoctoradosIds = AcademicFormation.GetModifiedDoctorados(arguments.From);
                        List<Record> doctoradosRecordList = new();
                        foreach (string id in modifiedDoctoradosIds.Keys)
                        {
                            doctoradosRecordList.Add(ToIdentifiersRecord("FormacionAcademica-Doctorados", id, modifiedDoctoradosIds[id]));
                        }
                        container.Records = doctoradosRecordList;
                        break;
                    case "FormacionAcademica-Posgrado":
                        Dictionary<string, DateTime> modifiedPosgradoIds = AcademicFormation.GetModifiedPosgrado(arguments.From);
                        List<Record> posgradoRecordList = new();
                        foreach (string id in modifiedPosgradoIds.Keys)
                        {
                            posgradoRecordList.Add(ToIdentifiersRecord("FormacionAcademica-Posgrado", id, modifiedPosgradoIds[id]));
                        }
                        container.Records = posgradoRecordList;
                        break;
                    case "FormacionAcademica-Especializada":
                        Dictionary<string, DateTime> modifiedEspecializadaIds = AcademicFormation.GetModifiedEspecializada(arguments.From);
                        List<Record> especializadaRecordList = new();
                        foreach (string id in modifiedEspecializadaIds.Keys)
                        {
                            especializadaRecordList.Add(ToIdentifiersRecord("FormacionAcademica-Especializada", id, modifiedEspecializadaIds[id]));
                        }
                        container.Records = especializadaRecordList;
                        break;
                    case "FormacionAcademica-Idiomas":
                        Dictionary<string, DateTime> modifiedIdiomasIds = AcademicFormation.GetModifiedIdiomas(arguments.From);
                        List<Record> idiomasRecordList = new();
                        foreach (string id in modifiedIdiomasIds.Keys)
                        {
                            idiomasRecordList.Add(ToIdentifiersRecord("FormacionAcademica-Idiomas", id, modifiedIdiomasIds[id]));
                        }
                        container.Records = idiomasRecordList;
                        break;
                    case "Tesis":
                        Dictionary<string, DateTime> modifiedTesisIds = DocentActivity.GetModifiedTesis(arguments.From);
                        List<Record> tesisRecordList = new();
                        foreach (string id in modifiedTesisIds.Keys)
                        {
                            tesisRecordList.Add(ToIdentifiersRecord("Tesis", id, modifiedTesisIds[id]));
                        }
                        container.Records = tesisRecordList;
                        break;
                    case "FormacionImpartida":
                        Dictionary<string, DateTime> modifiedFormacionImpartidaIds = DocentActivity.GetModifiedAcademicFormationProvided(arguments.From);
                        List<Record> formacionImpartidaRecordList = new();
                        foreach (string id in modifiedFormacionImpartidaIds.Keys)
                        {
                            formacionImpartidaRecordList.Add(ToIdentifiersRecord("FormacionImpartida", id, modifiedFormacionImpartidaIds[id]));
                        }
                        container.Records = formacionImpartidaRecordList;
                        break;
                    case "Seminarios":
                        Dictionary<string, DateTime> modifiedSeminariosIds = DocentActivity.GetModifiedSeminars(arguments.From);
                        List<Record> seminariosRecordList = new();
                        foreach (string id in modifiedSeminariosIds.Keys)
                        {
                            seminariosRecordList.Add(ToIdentifiersRecord("Seminarios", id, modifiedSeminariosIds[id]));
                        }
                        container.Records = seminariosRecordList;
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
                    case "FormacionAcademica-Ciclos":
                        Dictionary<string, DateTime> modifiedCiclosIds = AcademicFormation.GetModifiedCiclos(arguments.From);
                        List<Ciclos> ciclosList = new();
                        foreach (string id in modifiedCiclosIds.Keys)
                        {
                            ciclosList.Add(AcademicFormation.GetFormacionAcademicaCiclos(id));
                        }
                        break;
                    case "FormacionAcademica-Doctorados":
                        Dictionary<string, DateTime> modifiedDoctoradosIds = AcademicFormation.GetModifiedDoctorados(arguments.From);
                        List<Doctorados> doctoradosList = new();
                        foreach (string id in modifiedDoctoradosIds.Keys)
                        {
                            doctoradosList.Add(AcademicFormation.GetFormacionAcademicaDoctorados(id));
                        }
                        break;
                    case "FormacionAcademica-Posgrado":
                        Dictionary<string, DateTime> modifiedPosgradoIds = AcademicFormation.GetModifiedPosgrado(arguments.From);
                        List<Posgrado> posgradoList = new();
                        foreach (string id in modifiedPosgradoIds.Keys)
                        {
                            posgradoList.Add(AcademicFormation.GetFormacionAcademicaPosgrado(id));
                        }
                        break;
                    case "FormacionAcademica-Especializada":
                        Dictionary<string, DateTime> modifiedEspecializadaIds = AcademicFormation.GetModifiedEspecializada(arguments.From);
                        List<FormacionEspecializada> especializadaList = new();
                        foreach (string id in modifiedEspecializadaIds.Keys)
                        {
                            especializadaList.Add(AcademicFormation.GetFormacionAcademicaEspecializada(id));
                        }
                        break;
                    case "FormacionAcademica-Idiomas":
                        Dictionary<string, DateTime> modifiedIdiomasIds = AcademicFormation.GetModifiedIdiomas(arguments.From);
                        List<ConocimientoIdiomas> idiomaList = new();
                        foreach (string id in modifiedIdiomasIds.Keys)
                        {
                            idiomaList.Add(AcademicFormation.GetFormacionAcademicaIdiomas(id));
                        }
                        break;
                    case "Tesis":
                        Dictionary<string, DateTime> modifiedTesisIds = DocentActivity.GetModifiedTesis(arguments.From);
                        List<Tesis> tesisList = new();
                        foreach (string id in modifiedTesisIds.Keys)
                        {
                            tesisList.Add(DocentActivity.GetTesis(id));
                        }
                        break;
                    case "FormacionImpartida":
                        Dictionary<string, DateTime> modifiedFormacionImpartidaIds = DocentActivity.GetModifiedAcademicFormationProvided(arguments.From);
                        List<FormacionAcademicaImpartida> formacionImpartidaList = new();
                        foreach (string id in modifiedFormacionImpartidaIds.Keys)
                        {
                            formacionImpartidaList.Add(DocentActivity.GetAcademicFormationProvided(id));
                        }
                        break;
                    case "Seminarios":
                        Dictionary<string, DateTime> modifiedSeminariosIds = DocentActivity.GetModifiedSeminars(arguments.From);
                        List<SeminariosCursos> seminariosList = new();
                        foreach (string id in modifiedSeminariosIds.Keys)
                        {
                            seminariosList.Add(DocentActivity.GetSeminars(id));
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
