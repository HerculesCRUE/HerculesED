using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using Gnoss.ApiWrapper;
using Gnoss.ApiWrapper.Model;
using Gnoss.ApiWrapper.Helpers;
using GnossBase;
using Es.Riam.Gnoss.Web.MVC.Models;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Collections;
using Gnoss.ApiWrapper.Exceptions;
using Group = GroupOntology.Group;
using Organization = OrganizationOntology.Organization;
using Person = PersonOntology.Person;
using GeographicRegion = GeographicregionOntology.GeographicRegion;
using ProjectType = ProjecttypeOntology.ProjectType;
using ProjectModality = ProjectmodalityOntology.ProjectModality;
using ScientificExperienceProject = ScientificexperienceprojectOntology.ScientificExperienceProject;

namespace ProjectOntology
{
	public class Project : GnossOCBase
	{

		public Project() : base() { } 

		public Project(SemanticResourceModel pSemCmsModel, LanguageEnum idiomaUsuario) : base()
		{
			this.mGNOSSID = pSemCmsModel.RootEntities[0].Entity.Uri;
			SemanticPropertyModel propRoh_isSupportedBy = pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/isSupportedBy");
			if(propRoh_isSupportedBy != null && propRoh_isSupportedBy.PropertyValues.Count > 0)
			{
				this.Roh_isSupportedBy = new Funding(propRoh_isSupportedBy.PropertyValues[0].RelatedEntity,idiomaUsuario);
			}
			this.Roh_publicGroupList = new List<Group>();
			SemanticPropertyModel propRoh_publicGroupList = pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/publicGroupList");
			if(propRoh_publicGroupList != null && propRoh_publicGroupList.PropertyValues.Count > 0)
			{
				foreach (SemanticPropertyModel.PropertyValue propValue in propRoh_publicGroupList.PropertyValues)
				{
					if(propValue.RelatedEntity!=null){
						Group roh_publicGroupList = new Group(propValue.RelatedEntity,idiomaUsuario);
						this.Roh_publicGroupList.Add(roh_publicGroupList);
					}
				}
			}
			this.Roh_grantedBy = new List<Organization>();
			SemanticPropertyModel propRoh_grantedBy = pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/grantedBy");
			if(propRoh_grantedBy != null && propRoh_grantedBy.PropertyValues.Count > 0)
			{
				foreach (SemanticPropertyModel.PropertyValue propValue in propRoh_grantedBy.PropertyValues)
				{
					if(propValue.RelatedEntity!=null){
						Organization roh_grantedBy = new Organization(propValue.RelatedEntity,idiomaUsuario);
						this.Roh_grantedBy.Add(roh_grantedBy);
					}
				}
			}
			this.Roh_hasProjectClassification = new List<ProjectClassification>();
			SemanticPropertyModel propRoh_hasProjectClassification = pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/hasProjectClassification");
			if(propRoh_hasProjectClassification != null && propRoh_hasProjectClassification.PropertyValues.Count > 0)
			{
				foreach (SemanticPropertyModel.PropertyValue propValue in propRoh_hasProjectClassification.PropertyValues)
				{
					if(propValue.RelatedEntity!=null){
						ProjectClassification roh_hasProjectClassification = new ProjectClassification(propValue.RelatedEntity,idiomaUsuario);
						this.Roh_hasProjectClassification.Add(roh_hasProjectClassification);
					}
				}
			}
			this.Roh_hasResultsProjectClassification = new List<ProjectClassification>();
			SemanticPropertyModel propRoh_hasResultsProjectClassification = pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/hasResultsProjectClassification");
			if(propRoh_hasResultsProjectClassification != null && propRoh_hasResultsProjectClassification.PropertyValues.Count > 0)
			{
				foreach (SemanticPropertyModel.PropertyValue propValue in propRoh_hasResultsProjectClassification.PropertyValues)
				{
					if(propValue.RelatedEntity!=null){
						ProjectClassification roh_hasResultsProjectClassification = new ProjectClassification(propValue.RelatedEntity,idiomaUsuario);
						this.Roh_hasResultsProjectClassification.Add(roh_hasResultsProjectClassification);
					}
				}
			}
			this.Roh_mainResearchers = new List<BFO_0000023>();
			SemanticPropertyModel propRoh_mainResearchers = pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/mainResearchers");
			if(propRoh_mainResearchers != null && propRoh_mainResearchers.PropertyValues.Count > 0)
			{
				foreach (SemanticPropertyModel.PropertyValue propValue in propRoh_mainResearchers.PropertyValues)
				{
					if(propValue.RelatedEntity!=null){
						BFO_0000023 roh_mainResearchers = new BFO_0000023(propValue.RelatedEntity,idiomaUsuario);
						this.Roh_mainResearchers.Add(roh_mainResearchers);
					}
				}
			}
			this.Roh_publicAuthorList = new List<Person>();
			SemanticPropertyModel propRoh_publicAuthorList = pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/publicAuthorList");
			if(propRoh_publicAuthorList != null && propRoh_publicAuthorList.PropertyValues.Count > 0)
			{
				foreach (SemanticPropertyModel.PropertyValue propValue in propRoh_publicAuthorList.PropertyValues)
				{
					if(propValue.RelatedEntity!=null){
						Person roh_publicAuthorList = new Person(propValue.RelatedEntity,idiomaUsuario);
						this.Roh_publicAuthorList.Add(roh_publicAuthorList);
					}
				}
			}
			this.Roh_participates = new List<Organization>();
			SemanticPropertyModel propRoh_participates = pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/participates");
			if(propRoh_participates != null && propRoh_participates.PropertyValues.Count > 0)
			{
				foreach (SemanticPropertyModel.PropertyValue propValue in propRoh_participates.PropertyValues)
				{
					if(propValue.RelatedEntity!=null){
						Organization roh_participates = new Organization(propValue.RelatedEntity,idiomaUsuario);
						this.Roh_participates.Add(roh_participates);
					}
				}
			}
			SemanticPropertyModel propRoh_conductedBy = pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/conductedBy");
			if(propRoh_conductedBy != null && propRoh_conductedBy.PropertyValues.Count > 0)
			{
				this.Roh_conductedBy = new Organization(propRoh_conductedBy.PropertyValues[0].RelatedEntity,idiomaUsuario);
			}
			SemanticPropertyModel propVivo_geographicFocus = pSemCmsModel.GetPropertyByPath("http://vivoweb.org/ontology/core#geographicFocus");
			if(propVivo_geographicFocus != null && propVivo_geographicFocus.PropertyValues.Count > 0)
			{
				this.Vivo_geographicFocus = new GeographicRegion(propVivo_geographicFocus.PropertyValues[0].RelatedEntity,idiomaUsuario);
			}
			SemanticPropertyModel propRoh_projectType = pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/projectType");
			if(propRoh_projectType != null && propRoh_projectType.PropertyValues.Count > 0)
			{
				this.Roh_projectType = new ProjectType(propRoh_projectType.PropertyValues[0].RelatedEntity,idiomaUsuario);
			}
			this.Vivo_relates = new List<BFO_0000023>();
			SemanticPropertyModel propVivo_relates = pSemCmsModel.GetPropertyByPath("http://vivoweb.org/ontology/core#relates");
			if(propVivo_relates != null && propVivo_relates.PropertyValues.Count > 0)
			{
				foreach (SemanticPropertyModel.PropertyValue propValue in propVivo_relates.PropertyValues)
				{
					if(propValue.RelatedEntity!=null){
						BFO_0000023 vivo_relates = new BFO_0000023(propValue.RelatedEntity,idiomaUsuario);
						this.Vivo_relates.Add(vivo_relates);
					}
				}
			}
			SemanticPropertyModel propRoh_modality = pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/modality");
			if(propRoh_modality != null && propRoh_modality.PropertyValues.Count > 0)
			{
				this.Roh_modality = new ProjectModality(propRoh_modality.PropertyValues[0].RelatedEntity,idiomaUsuario);
			}
			this.Roh_relevantResults = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/relevantResults"));
			this.Roh_peopleYearNumber = GetNumberIntPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/peopleYearNumber"));
			this.Roh_durationMonths = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/durationMonths"));
			this.Roh_geographicFocusOther = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/geographicFocusOther"));
			this.Roh_researchersNumber = GetNumberIntPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/researchersNumber"));
			this.Roh_durationDays = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/durationDays"));
			this.Vivo_start= GetDateValuePropertySemCms(pSemCmsModel.GetPropertyByPath("http://vivoweb.org/ontology/core#start"));
			this.Roh_durationYears = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/durationYears"));
			this.Roh_crisIdentifier = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/crisIdentifier"));
			this.Vivo_end= GetDateValuePropertySemCms(pSemCmsModel.GetPropertyByPath("http://vivoweb.org/ontology/core#end"));
			this.Vivo_description = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://vivoweb.org/ontology/core#description"));
			this.Roh_monetaryAmount = GetNumberFloatPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/monetaryAmount"));
			this.Vivo_abbreviation = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://vivoweb.org/ontology/core#abbreviation"));
			SemanticPropertyModel propRoh_scientificExperienceProject = pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/scientificExperienceProject");
			if(propRoh_scientificExperienceProject != null && propRoh_scientificExperienceProject.PropertyValues.Count > 0)
			{
				this.Roh_scientificExperienceProject = new ScientificExperienceProject(propRoh_scientificExperienceProject.PropertyValues[0].RelatedEntity,idiomaUsuario);
			}
			this.Roh_classificationCVN = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/classificationCVN"));
			this.Roh_isSynchronized= GetBooleanPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/isSynchronized"));
			this.Roh_isPublic= GetBooleanPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/isPublic"));
			this.Roh_title = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/title"));
		}

		public Project(SemanticEntityModel pSemCmsModel, LanguageEnum idiomaUsuario) : base()
		{
			this.mGNOSSID = pSemCmsModel.Entity.Uri;
			this.mURL = pSemCmsModel.Properties.FirstOrDefault(p => p.PropertyValues.Any(prop => prop.DownloadUrl != null))?.FirstPropertyValue.DownloadUrl;
			SemanticPropertyModel propRoh_isSupportedBy = pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/isSupportedBy");
			if(propRoh_isSupportedBy != null && propRoh_isSupportedBy.PropertyValues.Count > 0)
			{
				this.Roh_isSupportedBy = new Funding(propRoh_isSupportedBy.PropertyValues[0].RelatedEntity,idiomaUsuario);
			}
			this.Roh_publicGroupList = new List<Group>();
			SemanticPropertyModel propRoh_publicGroupList = pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/publicGroupList");
			if(propRoh_publicGroupList != null && propRoh_publicGroupList.PropertyValues.Count > 0)
			{
				foreach (SemanticPropertyModel.PropertyValue propValue in propRoh_publicGroupList.PropertyValues)
				{
					if(propValue.RelatedEntity!=null){
						Group roh_publicGroupList = new Group(propValue.RelatedEntity,idiomaUsuario);
						this.Roh_publicGroupList.Add(roh_publicGroupList);
					}
				}
			}
			this.Roh_grantedBy = new List<Organization>();
			SemanticPropertyModel propRoh_grantedBy = pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/grantedBy");
			if(propRoh_grantedBy != null && propRoh_grantedBy.PropertyValues.Count > 0)
			{
				foreach (SemanticPropertyModel.PropertyValue propValue in propRoh_grantedBy.PropertyValues)
				{
					if(propValue.RelatedEntity!=null){
						Organization roh_grantedBy = new Organization(propValue.RelatedEntity,idiomaUsuario);
						this.Roh_grantedBy.Add(roh_grantedBy);
					}
				}
			}
			this.Roh_hasProjectClassification = new List<ProjectClassification>();
			SemanticPropertyModel propRoh_hasProjectClassification = pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/hasProjectClassification");
			if(propRoh_hasProjectClassification != null && propRoh_hasProjectClassification.PropertyValues.Count > 0)
			{
				foreach (SemanticPropertyModel.PropertyValue propValue in propRoh_hasProjectClassification.PropertyValues)
				{
					if(propValue.RelatedEntity!=null){
						ProjectClassification roh_hasProjectClassification = new ProjectClassification(propValue.RelatedEntity,idiomaUsuario);
						this.Roh_hasProjectClassification.Add(roh_hasProjectClassification);
					}
				}
			}
			this.Roh_hasResultsProjectClassification = new List<ProjectClassification>();
			SemanticPropertyModel propRoh_hasResultsProjectClassification = pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/hasResultsProjectClassification");
			if(propRoh_hasResultsProjectClassification != null && propRoh_hasResultsProjectClassification.PropertyValues.Count > 0)
			{
				foreach (SemanticPropertyModel.PropertyValue propValue in propRoh_hasResultsProjectClassification.PropertyValues)
				{
					if(propValue.RelatedEntity!=null){
						ProjectClassification roh_hasResultsProjectClassification = new ProjectClassification(propValue.RelatedEntity,idiomaUsuario);
						this.Roh_hasResultsProjectClassification.Add(roh_hasResultsProjectClassification);
					}
				}
			}
			this.Roh_mainResearchers = new List<BFO_0000023>();
			SemanticPropertyModel propRoh_mainResearchers = pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/mainResearchers");
			if(propRoh_mainResearchers != null && propRoh_mainResearchers.PropertyValues.Count > 0)
			{
				foreach (SemanticPropertyModel.PropertyValue propValue in propRoh_mainResearchers.PropertyValues)
				{
					if(propValue.RelatedEntity!=null){
						BFO_0000023 roh_mainResearchers = new BFO_0000023(propValue.RelatedEntity,idiomaUsuario);
						this.Roh_mainResearchers.Add(roh_mainResearchers);
					}
				}
			}
			this.Roh_publicAuthorList = new List<Person>();
			SemanticPropertyModel propRoh_publicAuthorList = pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/publicAuthorList");
			if(propRoh_publicAuthorList != null && propRoh_publicAuthorList.PropertyValues.Count > 0)
			{
				foreach (SemanticPropertyModel.PropertyValue propValue in propRoh_publicAuthorList.PropertyValues)
				{
					if(propValue.RelatedEntity!=null){
						Person roh_publicAuthorList = new Person(propValue.RelatedEntity,idiomaUsuario);
						this.Roh_publicAuthorList.Add(roh_publicAuthorList);
					}
				}
			}
			this.Roh_participates = new List<Organization>();
			SemanticPropertyModel propRoh_participates = pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/participates");
			if(propRoh_participates != null && propRoh_participates.PropertyValues.Count > 0)
			{
				foreach (SemanticPropertyModel.PropertyValue propValue in propRoh_participates.PropertyValues)
				{
					if(propValue.RelatedEntity!=null){
						Organization roh_participates = new Organization(propValue.RelatedEntity,idiomaUsuario);
						this.Roh_participates.Add(roh_participates);
					}
				}
			}
			SemanticPropertyModel propRoh_conductedBy = pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/conductedBy");
			if(propRoh_conductedBy != null && propRoh_conductedBy.PropertyValues.Count > 0)
			{
				this.Roh_conductedBy = new Organization(propRoh_conductedBy.PropertyValues[0].RelatedEntity,idiomaUsuario);
			}
			SemanticPropertyModel propVivo_geographicFocus = pSemCmsModel.GetPropertyByPath("http://vivoweb.org/ontology/core#geographicFocus");
			if(propVivo_geographicFocus != null && propVivo_geographicFocus.PropertyValues.Count > 0)
			{
				this.Vivo_geographicFocus = new GeographicRegion(propVivo_geographicFocus.PropertyValues[0].RelatedEntity,idiomaUsuario);
			}
			SemanticPropertyModel propRoh_projectType = pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/projectType");
			if(propRoh_projectType != null && propRoh_projectType.PropertyValues.Count > 0)
			{
				this.Roh_projectType = new ProjectType(propRoh_projectType.PropertyValues[0].RelatedEntity,idiomaUsuario);
			}
			this.Vivo_relates = new List<BFO_0000023>();
			SemanticPropertyModel propVivo_relates = pSemCmsModel.GetPropertyByPath("http://vivoweb.org/ontology/core#relates");
			if(propVivo_relates != null && propVivo_relates.PropertyValues.Count > 0)
			{
				foreach (SemanticPropertyModel.PropertyValue propValue in propVivo_relates.PropertyValues)
				{
					if(propValue.RelatedEntity!=null){
						BFO_0000023 vivo_relates = new BFO_0000023(propValue.RelatedEntity,idiomaUsuario);
						this.Vivo_relates.Add(vivo_relates);
					}
				}
			}
			SemanticPropertyModel propRoh_modality = pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/modality");
			if(propRoh_modality != null && propRoh_modality.PropertyValues.Count > 0)
			{
				this.Roh_modality = new ProjectModality(propRoh_modality.PropertyValues[0].RelatedEntity,idiomaUsuario);
			}
			this.Roh_relevantResults = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/relevantResults"));
			this.Roh_peopleYearNumber = GetNumberIntPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/peopleYearNumber"));
			this.Roh_durationMonths = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/durationMonths"));
			this.Roh_geographicFocusOther = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/geographicFocusOther"));
			this.Roh_researchersNumber = GetNumberIntPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/researchersNumber"));
			this.Roh_durationDays = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/durationDays"));
			this.Vivo_start= GetDateValuePropertySemCms(pSemCmsModel.GetPropertyByPath("http://vivoweb.org/ontology/core#start"));
			this.Roh_durationYears = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/durationYears"));
			this.Roh_crisIdentifier = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/crisIdentifier"));
			this.Vivo_end= GetDateValuePropertySemCms(pSemCmsModel.GetPropertyByPath("http://vivoweb.org/ontology/core#end"));
			this.Vivo_description = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://vivoweb.org/ontology/core#description"));
			this.Roh_monetaryAmount = GetNumberFloatPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/monetaryAmount"));
			this.Vivo_abbreviation = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://vivoweb.org/ontology/core#abbreviation"));
			SemanticPropertyModel propRoh_scientificExperienceProject = pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/scientificExperienceProject");
			if(propRoh_scientificExperienceProject != null && propRoh_scientificExperienceProject.PropertyValues.Count > 0)
			{
				this.Roh_scientificExperienceProject = new ScientificExperienceProject(propRoh_scientificExperienceProject.PropertyValues[0].RelatedEntity,idiomaUsuario);
			}
			this.Roh_classificationCVN = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/classificationCVN"));
			this.Roh_isSynchronized= GetBooleanPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/isSynchronized"));
			this.Roh_isPublic= GetBooleanPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/isPublic"));
			this.Roh_title = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/title"));
		}

		public virtual string RdfType { get { return "http://vivoweb.org/ontology/core#Project"; } }
		public virtual string RdfsLabel { get { return "http://vivoweb.org/ontology/core#Project"; } }
		[LABEL(LanguageEnum.es,"http://w3id.org/roh/isSupportedBy")]
		[RDFProperty("http://w3id.org/roh/isSupportedBy")]
		public  Funding Roh_isSupportedBy { get; set;}

		[LABEL(LanguageEnum.es,"http://w3id.org/roh/publicGroupList")]
		[RDFProperty("http://w3id.org/roh/publicGroupList")]
		public  List<Group> Roh_publicGroupList { get; set;}
		public List<string> IdsRoh_publicGroupList { get; set;}

		[LABEL(LanguageEnum.es,"http://w3id.org/roh/grantedBy")]
		[RDFProperty("http://w3id.org/roh/grantedBy")]
		public  List<Organization> Roh_grantedBy { get; set;}
		public List<string> IdsRoh_grantedBy { get; set;}

		[LABEL(LanguageEnum.es,"http://w3id.org/roh/hasProjectClassification")]
		[RDFProperty("http://w3id.org/roh/hasProjectClassification")]
		public  List<ProjectClassification> Roh_hasProjectClassification { get; set;}

		[LABEL(LanguageEnum.es,"http://w3id.org/roh/hasResultsProjectClassification")]
		[RDFProperty("http://w3id.org/roh/hasResultsProjectClassification")]
		public  List<ProjectClassification> Roh_hasResultsProjectClassification { get; set;}

		[LABEL(LanguageEnum.es,"http://w3id.org/roh/mainResearchers")]
		[RDFProperty("http://w3id.org/roh/mainResearchers")]
		public  List<BFO_0000023> Roh_mainResearchers { get; set;}

		[LABEL(LanguageEnum.es,"http://w3id.org/roh/publicAuthorList")]
		[RDFProperty("http://w3id.org/roh/publicAuthorList")]
		public  List<Person> Roh_publicAuthorList { get; set;}
		public List<string> IdsRoh_publicAuthorList { get; set;}

		[LABEL(LanguageEnum.es,"http://w3id.org/roh/participates")]
		[RDFProperty("http://w3id.org/roh/participates")]
		public  List<Organization> Roh_participates { get; set;}
		public List<string> IdsRoh_participates { get; set;}

		[LABEL(LanguageEnum.es,"http://w3id.org/roh/conductedBy")]
		[RDFProperty("http://w3id.org/roh/conductedBy")]
		public  Organization Roh_conductedBy  { get; set;} 
		public string IdRoh_conductedBy  { get; set;} 

		[LABEL(LanguageEnum.es,"http://vivoweb.org/ontology/core#geographicFocus")]
		[RDFProperty("http://vivoweb.org/ontology/core#geographicFocus")]
		public  GeographicRegion Vivo_geographicFocus  { get; set;} 
		public string IdVivo_geographicFocus  { get; set;} 

		[LABEL(LanguageEnum.es,"http://w3id.org/roh/projectType")]
		[RDFProperty("http://w3id.org/roh/projectType")]
		public  ProjectType Roh_projectType  { get; set;} 
		public string IdRoh_projectType  { get; set;} 

		[LABEL(LanguageEnum.es,"http://vivoweb.org/ontology/core#relates")]
		[RDFProperty("http://vivoweb.org/ontology/core#relates")]
		public  List<BFO_0000023> Vivo_relates { get; set;}

		[LABEL(LanguageEnum.es,"http://w3id.org/roh/modality")]
		[RDFProperty("http://w3id.org/roh/modality")]
		public  ProjectModality Roh_modality  { get; set;} 
		public string IdRoh_modality  { get; set;} 

		[LABEL(LanguageEnum.es,"http://w3id.org/roh/relevantResults")]
		[RDFProperty("http://w3id.org/roh/relevantResults")]
		public  string Roh_relevantResults { get; set;}

		[LABEL(LanguageEnum.es,"http://w3id.org/roh/peopleYearNumber")]
		[RDFProperty("http://w3id.org/roh/peopleYearNumber")]
		public  int? Roh_peopleYearNumber { get; set;}

		[LABEL(LanguageEnum.es,"http://w3id.org/roh/durationMonths")]
		[RDFProperty("http://w3id.org/roh/durationMonths")]
		public  string Roh_durationMonths { get; set;}

		[LABEL(LanguageEnum.es,"http://w3id.org/roh/geographicFocusOther")]
		[RDFProperty("http://w3id.org/roh/geographicFocusOther")]
		public  string Roh_geographicFocusOther { get; set;}

		[LABEL(LanguageEnum.es,"http://w3id.org/roh/researchersNumber")]
		[RDFProperty("http://w3id.org/roh/researchersNumber")]
		public  int? Roh_researchersNumber { get; set;}

		[LABEL(LanguageEnum.es,"http://w3id.org/roh/durationDays")]
		[RDFProperty("http://w3id.org/roh/durationDays")]
		public  string Roh_durationDays { get; set;}

		[LABEL(LanguageEnum.es,"http://vivoweb.org/ontology/core#start")]
		[RDFProperty("http://vivoweb.org/ontology/core#start")]
		public  DateTime? Vivo_start { get; set;}

		[LABEL(LanguageEnum.es,"http://w3id.org/roh/durationYears")]
		[RDFProperty("http://w3id.org/roh/durationYears")]
		public  string Roh_durationYears { get; set;}

		[LABEL(LanguageEnum.es,"http://w3id.org/roh/crisIdentifier")]
		[RDFProperty("http://w3id.org/roh/crisIdentifier")]
		public  string Roh_crisIdentifier { get; set;}

		[LABEL(LanguageEnum.es,"http://vivoweb.org/ontology/core#end")]
		[RDFProperty("http://vivoweb.org/ontology/core#end")]
		public  DateTime? Vivo_end { get; set;}

		[LABEL(LanguageEnum.es,"http://vivoweb.org/ontology/core#description")]
		[RDFProperty("http://vivoweb.org/ontology/core#description")]
		public  string Vivo_description { get; set;}

		[LABEL(LanguageEnum.es,"http://w3id.org/roh/monetaryAmount")]
		[RDFProperty("http://w3id.org/roh/monetaryAmount")]
		public  float? Roh_monetaryAmount { get; set;}

		[LABEL(LanguageEnum.es,"http://vivoweb.org/ontology/core#abbreviation")]
		[RDFProperty("http://vivoweb.org/ontology/core#abbreviation")]
		public  string Vivo_abbreviation { get; set;}

		[LABEL(LanguageEnum.es,"http://w3id.org/roh/scientificExperienceProject")]
		[RDFProperty("http://w3id.org/roh/scientificExperienceProject")]
		[Required]
		public  ScientificExperienceProject Roh_scientificExperienceProject  { get; set;} 
		public string IdRoh_scientificExperienceProject  { get; set;} 

		[LABEL(LanguageEnum.es,"Clasificación CVN")]
		[RDFProperty("http://w3id.org/roh/classificationCVN")]
		public  string Roh_classificationCVN { get; set;}

		[LABEL(LanguageEnum.es,"http://w3id.org/roh/isSynchronized")]
		[RDFProperty("http://w3id.org/roh/isSynchronized")]
		public  bool Roh_isSynchronized { get; set;}

		[LABEL(LanguageEnum.es,"http://w3id.org/roh/isPublic")]
		[RDFProperty("http://w3id.org/roh/isPublic")]
		public  bool Roh_isPublic { get; set;}

		[LABEL(LanguageEnum.es,"http://w3id.org/roh/title")]
		[RDFProperty("http://w3id.org/roh/title")]
		public  string Roh_title { get; set;}


		internal override void GetProperties()
		{
			base.GetProperties();
			propList.Add(new ListStringOntologyProperty("roh:publicGroupList", this.IdsRoh_publicGroupList));
			propList.Add(new ListStringOntologyProperty("roh:grantedBy", this.IdsRoh_grantedBy));
			propList.Add(new ListStringOntologyProperty("roh:publicAuthorList", this.IdsRoh_publicAuthorList));
			propList.Add(new ListStringOntologyProperty("roh:participates", this.IdsRoh_participates));
			propList.Add(new StringOntologyProperty("roh:conductedBy", this.IdRoh_conductedBy));
			propList.Add(new StringOntologyProperty("vivo:geographicFocus", this.IdVivo_geographicFocus));
			propList.Add(new StringOntologyProperty("roh:projectType", this.IdRoh_projectType));
			propList.Add(new StringOntologyProperty("roh:modality", this.IdRoh_modality));
			propList.Add(new StringOntologyProperty("roh:relevantResults", this.Roh_relevantResults));
			propList.Add(new StringOntologyProperty("roh:peopleYearNumber", this.Roh_peopleYearNumber.ToString()));
			propList.Add(new StringOntologyProperty("roh:durationMonths", this.Roh_durationMonths));
			propList.Add(new StringOntologyProperty("roh:geographicFocusOther", this.Roh_geographicFocusOther));
			propList.Add(new StringOntologyProperty("roh:researchersNumber", this.Roh_researchersNumber.ToString()));
			propList.Add(new StringOntologyProperty("roh:durationDays", this.Roh_durationDays));
			if (this.Vivo_start.HasValue){
				propList.Add(new DateOntologyProperty("vivo:start", this.Vivo_start.Value));
				}
			propList.Add(new StringOntologyProperty("roh:durationYears", this.Roh_durationYears));
			propList.Add(new StringOntologyProperty("roh:crisIdentifier", this.Roh_crisIdentifier));
			if (this.Vivo_end.HasValue){
				propList.Add(new DateOntologyProperty("vivo:end", this.Vivo_end.Value));
				}
			propList.Add(new StringOntologyProperty("vivo:description", this.Vivo_description));
			propList.Add(new StringOntologyProperty("roh:monetaryAmount", this.Roh_monetaryAmount.ToString()));
			propList.Add(new StringOntologyProperty("vivo:abbreviation", this.Vivo_abbreviation));
			propList.Add(new StringOntologyProperty("roh:scientificExperienceProject", this.IdRoh_scientificExperienceProject));
			propList.Add(new StringOntologyProperty("roh:classificationCVN", this.Roh_classificationCVN));
			propList.Add(new BoolOntologyProperty("roh:isSynchronized", this.Roh_isSynchronized));
			propList.Add(new BoolOntologyProperty("roh:isPublic", this.Roh_isPublic));
			propList.Add(new StringOntologyProperty("roh:title", this.Roh_title));
		}

		internal override void GetEntities()
		{
			base.GetEntities();
			if(Roh_isSupportedBy!=null){
				Roh_isSupportedBy.GetProperties();
				Roh_isSupportedBy.GetEntities();
				OntologyEntity entityRoh_isSupportedBy = new OntologyEntity("http://w3id.org/roh/Funding", "http://w3id.org/roh/Funding", "roh:isSupportedBy", Roh_isSupportedBy.propList, Roh_isSupportedBy.entList);
				entList.Add(entityRoh_isSupportedBy);
			}
			if(Roh_hasProjectClassification!=null){
				foreach(ProjectClassification prop in Roh_hasProjectClassification){
					prop.GetProperties();
					prop.GetEntities();
					OntologyEntity entityProjectClassification = new OntologyEntity("http://w3id.org/roh/ProjectClassification", "http://w3id.org/roh/ProjectClassification", "roh:hasProjectClassification", prop.propList, prop.entList);
				entList.Add(entityProjectClassification);
				prop.Entity= entityProjectClassification;
				}
			}
			if(Roh_hasResultsProjectClassification!=null){
				foreach(ProjectClassification prop in Roh_hasResultsProjectClassification){
					prop.GetProperties();
					prop.GetEntities();
					OntologyEntity entityProjectClassification = new OntologyEntity("http://w3id.org/roh/ProjectClassification", "http://w3id.org/roh/ProjectClassification", "roh:hasResultsProjectClassification", prop.propList, prop.entList);
				entList.Add(entityProjectClassification);
				prop.Entity= entityProjectClassification;
				}
			}
			if(Roh_mainResearchers!=null){
				foreach(BFO_0000023 prop in Roh_mainResearchers){
					prop.GetProperties();
					prop.GetEntities();
					OntologyEntity entityBFO_0000023 = new OntologyEntity("http://purl.obolibrary.org/obo/BFO_0000023", "http://purl.obolibrary.org/obo/BFO_0000023", "roh:mainResearchers", prop.propList, prop.entList);
				entList.Add(entityBFO_0000023);
				prop.Entity= entityBFO_0000023;
				}
			}
			if(Vivo_relates!=null){
				foreach(BFO_0000023 prop in Vivo_relates){
					prop.GetProperties();
					prop.GetEntities();
					OntologyEntity entityBFO_0000023 = new OntologyEntity("http://purl.obolibrary.org/obo/BFO_0000023", "http://purl.obolibrary.org/obo/BFO_0000023", "vivo:relates", prop.propList, prop.entList);
				entList.Add(entityBFO_0000023);
				prop.Entity= entityBFO_0000023;
				}
			}
		} 
		public virtual ComplexOntologyResource ToGnossApiResource(ResourceApi resourceAPI, List<string> listaDeCategorias)
		{
			return ToGnossApiResource(resourceAPI, listaDeCategorias, Guid.Empty, Guid.Empty);
		}

		public virtual ComplexOntologyResource ToGnossApiResource(ResourceApi resourceAPI, List<string> listaDeCategorias, Guid idrecurso, Guid idarticulo)
		{
			ComplexOntologyResource resource = new ComplexOntologyResource();
			Ontology ontology=null;
			GetEntities();
			GetProperties();
			if(idrecurso.Equals(Guid.Empty) && idarticulo.Equals(Guid.Empty))
			{
				ontology = new Ontology(resourceAPI.GraphsUrl, resourceAPI.OntologyUrl, RdfType, RdfsLabel, prefList, propList, entList);
			}
			else{
				ontology = new Ontology(resourceAPI.GraphsUrl, resourceAPI.OntologyUrl, RdfType, RdfsLabel, prefList, propList, entList,idrecurso,idarticulo);
			}
			resource.Id = GNOSSID;
			resource.Ontology = ontology;
			resource.TextCategories=listaDeCategorias;
			AddResourceTitle(resource);
			AddResourceDescription(resource);
			AddImages(resource);
			AddFiles(resource);
			return resource;
		}

		public override List<string> ToOntologyGnossTriples(ResourceApi resourceAPI)
		{
			List<string> list = new List<string>();
			AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Project_{ResourceID}_{ArticleID}", "http://www.w3.org/1999/02/22-rdf-syntax-ns#type", $"<http://vivoweb.org/ontology/core#Project>", list, " . ");
			AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Project_{ResourceID}_{ArticleID}", "http://www.w3.org/2000/01/rdf-schema#label", $"\"http://vivoweb.org/ontology/core#Project\"", list, " . ");
			AgregarTripleALista($"{resourceAPI.GraphsUrl}{ResourceID}", "http://gnoss/hasEntidad", $"<{resourceAPI.GraphsUrl}items/Project_{ResourceID}_{ArticleID}>", list, " . ");
			if(this.Roh_isSupportedBy != null)
			{
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Funding_{ResourceID}_{this.Roh_isSupportedBy.ArticleID}", "http://www.w3.org/1999/02/22-rdf-syntax-ns#type", $"<http://w3id.org/roh/Funding>", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Funding_{ResourceID}_{this.Roh_isSupportedBy.ArticleID}", "http://www.w3.org/2000/01/rdf-schema#label", $"\"http://w3id.org/roh/Funding\"", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}{ResourceID}", "http://gnoss/hasEntidad", $"<{resourceAPI.GraphsUrl}items/Funding_{ResourceID}_{this.Roh_isSupportedBy.ArticleID}>", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Project_{ResourceID}_{ArticleID}", "http://w3id.org/roh/isSupportedBy", $"<{resourceAPI.GraphsUrl}items/Funding_{ResourceID}_{this.Roh_isSupportedBy.ArticleID}>", list, " . ");
			if(this.Roh_isSupportedBy.Roh_fundedBy != null)
			{
			foreach(var item1 in this.Roh_isSupportedBy.Roh_fundedBy)
			{
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/FundingProgram_{ResourceID}_{item1.ArticleID}", "http://www.w3.org/1999/02/22-rdf-syntax-ns#type", $"<http://w3id.org/roh/FundingProgram>", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/FundingProgram_{ResourceID}_{item1.ArticleID}", "http://www.w3.org/2000/01/rdf-schema#label", $"\"http://w3id.org/roh/FundingProgram\"", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}{ResourceID}", "http://gnoss/hasEntidad", $"<{resourceAPI.GraphsUrl}items/FundingProgram_{ResourceID}_{item1.ArticleID}>", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Funding_{ResourceID}_{this.Roh_isSupportedBy.ArticleID}", "http://w3id.org/roh/fundedBy", $"<{resourceAPI.GraphsUrl}items/FundingProgram_{ResourceID}_{item1.ArticleID}>", list, " . ");
				if(item1.IdRoh_promotedBy != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/FundingProgram_{ResourceID}_{item1.ArticleID}",  "http://w3id.org/roh/promotedBy", $"<{item1.IdRoh_promotedBy}>", list, " . ");
				}
				if(item1.Vivo_identifier != null)
				{
					foreach(var item2 in item1.Vivo_identifier)
					{
						AgregarTripleALista($"{resourceAPI.GraphsUrl}items/FundingProgram_{ResourceID}_{item1.ArticleID}", "http://vivoweb.org/ontology/core#identifier", $"\"{GenerarTextoSinSaltoDeLinea(item2)}\"", list, " . ");
					}
				}
				if(item1.Roh_title != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/FundingProgram_{ResourceID}_{item1.ArticleID}",  "http://w3id.org/roh/title", $"\"{GenerarTextoSinSaltoDeLinea(item1.Roh_title)}\"", list, " . ");
				}
			}
			}
				if(this.Roh_isSupportedBy.Roh_mixedPercentage != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Funding_{ResourceID}_{this.Roh_isSupportedBy.ArticleID}",  "http://w3id.org/roh/mixedPercentage", $"{this.Roh_isSupportedBy.Roh_mixedPercentage.Value.ToString(new CultureInfo("en-US"))}", list, " . ");
				}
				if(this.Roh_isSupportedBy.Roh_creditPercentage != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Funding_{ResourceID}_{this.Roh_isSupportedBy.ArticleID}",  "http://w3id.org/roh/creditPercentage", $"{this.Roh_isSupportedBy.Roh_creditPercentage.Value.ToString(new CultureInfo("en-US"))}", list, " . ");
				}
				if(this.Roh_isSupportedBy.Vivo_identifier != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Funding_{ResourceID}_{this.Roh_isSupportedBy.ArticleID}",  "http://vivoweb.org/ontology/core#identifier", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_isSupportedBy.Vivo_identifier)}\"", list, " . ");
				}
				if(this.Roh_isSupportedBy.Roh_grantsPercentage != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Funding_{ResourceID}_{this.Roh_isSupportedBy.ArticleID}",  "http://w3id.org/roh/grantsPercentage", $"{this.Roh_isSupportedBy.Roh_grantsPercentage.Value.ToString(new CultureInfo("en-US"))}", list, " . ");
				}
				if(this.Roh_isSupportedBy.Roh_monetaryAmount != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Funding_{ResourceID}_{this.Roh_isSupportedBy.ArticleID}",  "http://w3id.org/roh/monetaryAmount", $"{this.Roh_isSupportedBy.Roh_monetaryAmount.Value.ToString(new CultureInfo("en-US"))}", list, " . ");
				}
			}
			if(this.Roh_hasProjectClassification != null)
			{
			foreach(var item0 in this.Roh_hasProjectClassification)
			{
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/ProjectClassification_{ResourceID}_{item0.ArticleID}", "http://www.w3.org/1999/02/22-rdf-syntax-ns#type", $"<http://w3id.org/roh/ProjectClassification>", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/ProjectClassification_{ResourceID}_{item0.ArticleID}", "http://www.w3.org/2000/01/rdf-schema#label", $"\"http://w3id.org/roh/ProjectClassification\"", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}{ResourceID}", "http://gnoss/hasEntidad", $"<{resourceAPI.GraphsUrl}items/ProjectClassification_{ResourceID}_{item0.ArticleID}>", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Project_{ResourceID}_{ArticleID}", "http://w3id.org/roh/hasProjectClassification", $"<{resourceAPI.GraphsUrl}items/ProjectClassification_{ResourceID}_{item0.ArticleID}>", list, " . ");
				if(item0.IdsRoh_projectClassificationNode != null)
				{
					foreach(var item2 in item0.IdsRoh_projectClassificationNode)
					{
						AgregarTripleALista($"{resourceAPI.GraphsUrl}items/ProjectClassification_{ResourceID}_{item0.ArticleID}", "http://w3id.org/roh/projectClassificationNode", $"<{item2}>", list, " . ");
					}
				}
			}
			}
			if(this.Roh_hasResultsProjectClassification != null)
			{
			foreach(var item0 in this.Roh_hasResultsProjectClassification)
			{
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/ProjectClassification_{ResourceID}_{item0.ArticleID}", "http://www.w3.org/1999/02/22-rdf-syntax-ns#type", $"<http://w3id.org/roh/ProjectClassification>", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/ProjectClassification_{ResourceID}_{item0.ArticleID}", "http://www.w3.org/2000/01/rdf-schema#label", $"\"http://w3id.org/roh/ProjectClassification\"", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}{ResourceID}", "http://gnoss/hasEntidad", $"<{resourceAPI.GraphsUrl}items/ProjectClassification_{ResourceID}_{item0.ArticleID}>", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Project_{ResourceID}_{ArticleID}", "http://w3id.org/roh/hasResultsProjectClassification", $"<{resourceAPI.GraphsUrl}items/ProjectClassification_{ResourceID}_{item0.ArticleID}>", list, " . ");
				if(item0.IdsRoh_projectClassificationNode != null)
				{
					foreach(var item2 in item0.IdsRoh_projectClassificationNode)
					{
						AgregarTripleALista($"{resourceAPI.GraphsUrl}items/ProjectClassification_{ResourceID}_{item0.ArticleID}", "http://w3id.org/roh/projectClassificationNode", $"<{item2}>", list, " . ");
					}
				}
			}
			}
			if(this.Roh_mainResearchers != null)
			{
			foreach(var item0 in this.Roh_mainResearchers)
			{
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/BFO_0000023_{ResourceID}_{item0.ArticleID}", "http://www.w3.org/1999/02/22-rdf-syntax-ns#type", $"<http://purl.obolibrary.org/obo/BFO_0000023>", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/BFO_0000023_{ResourceID}_{item0.ArticleID}", "http://www.w3.org/2000/01/rdf-schema#label", $"\"http://purl.obolibrary.org/obo/BFO_0000023\"", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}{ResourceID}", "http://gnoss/hasEntidad", $"<{resourceAPI.GraphsUrl}items/BFO_0000023_{ResourceID}_{item0.ArticleID}>", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Project_{ResourceID}_{ArticleID}", "http://w3id.org/roh/mainResearchers", $"<{resourceAPI.GraphsUrl}items/BFO_0000023_{ResourceID}_{item0.ArticleID}>", list, " . ");
				if(item0.Vivo_start != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/BFO_0000023_{ResourceID}_{item0.ArticleID}",  "http://vivoweb.org/ontology/core#start", $"\"{item0.Vivo_start.Value.ToString("yyyyMMddHHmmss")}\"", list, " . ");
				}
				if(item0.Vivo_end != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/BFO_0000023_{ResourceID}_{item0.ArticleID}",  "http://vivoweb.org/ontology/core#end", $"\"{item0.Vivo_end.Value.ToString("yyyyMMddHHmmss")}\"", list, " . ");
				}
				if(item0.Foaf_nick != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/BFO_0000023_{ResourceID}_{item0.ArticleID}",  "http://xmlns.com/foaf/0.1/nick", $"\"{GenerarTextoSinSaltoDeLinea(item0.Foaf_nick)}\"", list, " . ");
				}
				if(item0.IdRdf_member != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/BFO_0000023_{ResourceID}_{item0.ArticleID}",  "http://www.w3.org/1999/02/22-rdf-syntax-ns#member", $"<{item0.IdRdf_member}>", list, " . ");
				}
				if(item0.Rdf_comment != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/BFO_0000023_{ResourceID}_{item0.ArticleID}",  "http://www.w3.org/1999/02/22-rdf-syntax-ns#comment", $"{item0.Rdf_comment.ToString()}", list, " . ");
				}
			}
			}
			if(this.Vivo_relates != null)
			{
			foreach(var item0 in this.Vivo_relates)
			{
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/BFO_0000023_{ResourceID}_{item0.ArticleID}", "http://www.w3.org/1999/02/22-rdf-syntax-ns#type", $"<http://purl.obolibrary.org/obo/BFO_0000023>", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/BFO_0000023_{ResourceID}_{item0.ArticleID}", "http://www.w3.org/2000/01/rdf-schema#label", $"\"http://purl.obolibrary.org/obo/BFO_0000023\"", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}{ResourceID}", "http://gnoss/hasEntidad", $"<{resourceAPI.GraphsUrl}items/BFO_0000023_{ResourceID}_{item0.ArticleID}>", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Project_{ResourceID}_{ArticleID}", "http://vivoweb.org/ontology/core#relates", $"<{resourceAPI.GraphsUrl}items/BFO_0000023_{ResourceID}_{item0.ArticleID}>", list, " . ");
				if(item0.Vivo_start != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/BFO_0000023_{ResourceID}_{item0.ArticleID}",  "http://vivoweb.org/ontology/core#start", $"\"{item0.Vivo_start.Value.ToString("yyyyMMddHHmmss")}\"", list, " . ");
				}
				if(item0.Vivo_end != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/BFO_0000023_{ResourceID}_{item0.ArticleID}",  "http://vivoweb.org/ontology/core#end", $"\"{item0.Vivo_end.Value.ToString("yyyyMMddHHmmss")}\"", list, " . ");
				}
				if(item0.Foaf_nick != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/BFO_0000023_{ResourceID}_{item0.ArticleID}",  "http://xmlns.com/foaf/0.1/nick", $"\"{GenerarTextoSinSaltoDeLinea(item0.Foaf_nick)}\"", list, " . ");
				}
				if(item0.IdRdf_member != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/BFO_0000023_{ResourceID}_{item0.ArticleID}",  "http://www.w3.org/1999/02/22-rdf-syntax-ns#member", $"<{item0.IdRdf_member}>", list, " . ");
				}
				if(item0.Rdf_comment != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/BFO_0000023_{ResourceID}_{item0.ArticleID}",  "http://www.w3.org/1999/02/22-rdf-syntax-ns#comment", $"{item0.Rdf_comment.ToString()}", list, " . ");
				}
			}
			}
				if(this.IdsRoh_publicGroupList != null)
				{
					foreach(var item2 in this.IdsRoh_publicGroupList)
					{
						AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Project_{ResourceID}_{ArticleID}", "http://w3id.org/roh/publicGroupList", $"<{item2}>", list, " . ");
					}
				}
				if(this.IdsRoh_grantedBy != null)
				{
					foreach(var item2 in this.IdsRoh_grantedBy)
					{
						AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Project_{ResourceID}_{ArticleID}", "http://w3id.org/roh/grantedBy", $"<{item2}>", list, " . ");
					}
				}
				if(this.IdsRoh_publicAuthorList != null)
				{
					foreach(var item2 in this.IdsRoh_publicAuthorList)
					{
						AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Project_{ResourceID}_{ArticleID}", "http://w3id.org/roh/publicAuthorList", $"<{item2}>", list, " . ");
					}
				}
				if(this.IdsRoh_participates != null)
				{
					foreach(var item2 in this.IdsRoh_participates)
					{
						AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Project_{ResourceID}_{ArticleID}", "http://w3id.org/roh/participates", $"<{item2}>", list, " . ");
					}
				}
				if(this.IdRoh_conductedBy != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Project_{ResourceID}_{ArticleID}",  "http://w3id.org/roh/conductedBy", $"<{this.IdRoh_conductedBy}>", list, " . ");
				}
				if(this.IdVivo_geographicFocus != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Project_{ResourceID}_{ArticleID}",  "http://vivoweb.org/ontology/core#geographicFocus", $"<{this.IdVivo_geographicFocus}>", list, " . ");
				}
				if(this.IdRoh_projectType != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Project_{ResourceID}_{ArticleID}",  "http://w3id.org/roh/projectType", $"<{this.IdRoh_projectType}>", list, " . ");
				}
				if(this.IdRoh_modality != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Project_{ResourceID}_{ArticleID}",  "http://w3id.org/roh/modality", $"<{this.IdRoh_modality}>", list, " . ");
				}
				if(this.Roh_relevantResults != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Project_{ResourceID}_{ArticleID}",  "http://w3id.org/roh/relevantResults", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_relevantResults)}\"", list, " . ");
				}
				if(this.Roh_peopleYearNumber != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Project_{ResourceID}_{ArticleID}",  "http://w3id.org/roh/peopleYearNumber", $"{this.Roh_peopleYearNumber.Value.ToString()}", list, " . ");
				}
				if(this.Roh_durationMonths != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Project_{ResourceID}_{ArticleID}",  "http://w3id.org/roh/durationMonths", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_durationMonths)}\"", list, " . ");
				}
				if(this.Roh_geographicFocusOther != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Project_{ResourceID}_{ArticleID}",  "http://w3id.org/roh/geographicFocusOther", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_geographicFocusOther)}\"", list, " . ");
				}
				if(this.Roh_researchersNumber != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Project_{ResourceID}_{ArticleID}",  "http://w3id.org/roh/researchersNumber", $"{this.Roh_researchersNumber.Value.ToString()}", list, " . ");
				}
				if(this.Roh_durationDays != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Project_{ResourceID}_{ArticleID}",  "http://w3id.org/roh/durationDays", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_durationDays)}\"", list, " . ");
				}
				if(this.Vivo_start != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Project_{ResourceID}_{ArticleID}",  "http://vivoweb.org/ontology/core#start", $"\"{this.Vivo_start.Value.ToString("yyyyMMddHHmmss")}\"", list, " . ");
				}
				if(this.Roh_durationYears != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Project_{ResourceID}_{ArticleID}",  "http://w3id.org/roh/durationYears", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_durationYears)}\"", list, " . ");
				}
				if(this.Roh_crisIdentifier != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Project_{ResourceID}_{ArticleID}",  "http://w3id.org/roh/crisIdentifier", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_crisIdentifier)}\"", list, " . ");
				}
				if(this.Vivo_end != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Project_{ResourceID}_{ArticleID}",  "http://vivoweb.org/ontology/core#end", $"\"{this.Vivo_end.Value.ToString("yyyyMMddHHmmss")}\"", list, " . ");
				}
				if(this.Vivo_description != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Project_{ResourceID}_{ArticleID}",  "http://vivoweb.org/ontology/core#description", $"\"{GenerarTextoSinSaltoDeLinea(this.Vivo_description)}\"", list, " . ");
				}
				if(this.Roh_monetaryAmount != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Project_{ResourceID}_{ArticleID}",  "http://w3id.org/roh/monetaryAmount", $"{this.Roh_monetaryAmount.Value.ToString(new CultureInfo("en-US"))}", list, " . ");
				}
				if(this.Vivo_abbreviation != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Project_{ResourceID}_{ArticleID}",  "http://vivoweb.org/ontology/core#abbreviation", $"\"{GenerarTextoSinSaltoDeLinea(this.Vivo_abbreviation)}\"", list, " . ");
				}
				if(this.IdRoh_scientificExperienceProject != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Project_{ResourceID}_{ArticleID}",  "http://w3id.org/roh/scientificExperienceProject", $"<{this.IdRoh_scientificExperienceProject}>", list, " . ");
				}
				if(this.Roh_classificationCVN != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Project_{ResourceID}_{ArticleID}",  "http://w3id.org/roh/classificationCVN", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_classificationCVN)}\"", list, " . ");
				}
				if(this.Roh_isSynchronized != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Project_{ResourceID}_{ArticleID}",  "http://w3id.org/roh/isSynchronized", $"\"{this.Roh_isSynchronized.ToString()}\"", list, " . ");
				}
				if(this.Roh_isPublic != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Project_{ResourceID}_{ArticleID}",  "http://w3id.org/roh/isPublic", $"\"{this.Roh_isPublic.ToString()}\"", list, " . ");
				}
				if(this.Roh_title != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Project_{ResourceID}_{ArticleID}",  "http://w3id.org/roh/title", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_title)}\"", list, " . ");
				}
			return list;
		}

		public override List<string> ToSearchGraphTriples(ResourceApi resourceAPI)
		{
			List<string> list = new List<string>();
			List<string> listaSearch = new List<string>();
			AgregarTags(list);
			AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}", "http://www.w3.org/1999/02/22-rdf-syntax-ns#type", $"\"project\"", list, " . ");
			AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}", "http://gnoss/type", $"\"http://vivoweb.org/ontology/core#Project\"", list, " . ");
			AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}", "http://gnoss/hasfechapublicacion", $"{DateTime.Now.ToString("yyyyMMddHHmmss")}", list, " . ");
			AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}", "http://gnoss/hastipodoc", "\"5\"", list, " . ");
			AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}", "http://gnoss/hasfechamodificacion", $"{DateTime.Now.ToString("yyyyMMddHHmmss")}", list, " . ");
			AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}", "http://gnoss/hasnumeroVisitas", "0", list, " . ");
			AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}", "http://gnoss/hasprivacidadCom", "\"publico\"", list, " . ");
			AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}", "http://xmlns.com/foaf/0.1/firstName", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_title)}\"", list, " . ");
			AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}", "http://gnoss/hasnombrecompleto", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_title)}\"", list, " . ");
			string search = string.Empty;
			if(this.Roh_isSupportedBy != null)
			{
				AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}", "http://w3id.org/roh/isSupportedBy", $"<{resourceAPI.GraphsUrl}items/funding_{ResourceID}_{this.Roh_isSupportedBy.ArticleID}>", list, " . ");
			if(this.Roh_isSupportedBy.Roh_fundedBy != null)
			{
			foreach(var item1 in this.Roh_isSupportedBy.Roh_fundedBy)
			{
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/funding_{ResourceID}_{this.Roh_isSupportedBy.ArticleID}", "http://w3id.org/roh/fundedBy", $"<{resourceAPI.GraphsUrl}items/fundingprogram_{ResourceID}_{item1.ArticleID}>", list, " . ");
				if(item1.IdRoh_promotedBy != null)
				{
					Regex regex = new Regex(@"\/items\/.+_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}");
					string itemRegex = item1.IdRoh_promotedBy;
					if (regex.IsMatch(itemRegex))
					{
						itemRegex = $"http://gnoss/{resourceAPI.GetShortGuid(itemRegex).ToString().ToUpper()}";
					}
					else
					{
						itemRegex = itemRegex.ToLower();
					}
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/fundingprogram_{ResourceID}_{item1.ArticleID}",  "http://w3id.org/roh/promotedBy", $"<{itemRegex}>", list, " . ");
				}
				if(item1.Vivo_identifier != null)
				{
					foreach(var item2 in item1.Vivo_identifier)
					{
						AgregarTripleALista($"{resourceAPI.GraphsUrl}items/fundingprogram_{ResourceID}_{item1.ArticleID}", "http://vivoweb.org/ontology/core#identifier", $"\"{GenerarTextoSinSaltoDeLinea(item2).ToLower()}\"", list, " . ");
					}
				}
				if(item1.Roh_title != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/fundingprogram_{ResourceID}_{item1.ArticleID}",  "http://w3id.org/roh/title", $"\"{GenerarTextoSinSaltoDeLinea(item1.Roh_title).ToLower()}\"", list, " . ");
				}
			}
			}
				if(this.Roh_isSupportedBy.Roh_mixedPercentage != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/funding_{ResourceID}_{this.Roh_isSupportedBy.ArticleID}",  "http://w3id.org/roh/mixedPercentage", $"{this.Roh_isSupportedBy.Roh_mixedPercentage.Value.ToString(new CultureInfo("en-US"))}", list, " . ");
				}
				if(this.Roh_isSupportedBy.Roh_creditPercentage != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/funding_{ResourceID}_{this.Roh_isSupportedBy.ArticleID}",  "http://w3id.org/roh/creditPercentage", $"{this.Roh_isSupportedBy.Roh_creditPercentage.Value.ToString(new CultureInfo("en-US"))}", list, " . ");
				}
				if(this.Roh_isSupportedBy.Vivo_identifier != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/funding_{ResourceID}_{this.Roh_isSupportedBy.ArticleID}",  "http://vivoweb.org/ontology/core#identifier", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_isSupportedBy.Vivo_identifier).ToLower()}\"", list, " . ");
				}
				if(this.Roh_isSupportedBy.Roh_grantsPercentage != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/funding_{ResourceID}_{this.Roh_isSupportedBy.ArticleID}",  "http://w3id.org/roh/grantsPercentage", $"{this.Roh_isSupportedBy.Roh_grantsPercentage.Value.ToString(new CultureInfo("en-US"))}", list, " . ");
				}
				if(this.Roh_isSupportedBy.Roh_monetaryAmount != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/funding_{ResourceID}_{this.Roh_isSupportedBy.ArticleID}",  "http://w3id.org/roh/monetaryAmount", $"{this.Roh_isSupportedBy.Roh_monetaryAmount.Value.ToString(new CultureInfo("en-US"))}", list, " . ");
				}
			}
			if(this.Roh_hasProjectClassification != null)
			{
			foreach(var item0 in this.Roh_hasProjectClassification)
			{
				AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}", "http://w3id.org/roh/hasProjectClassification", $"<{resourceAPI.GraphsUrl}items/projectclassification_{ResourceID}_{item0.ArticleID}>", list, " . ");
				if(item0.IdsRoh_projectClassificationNode != null)
				{
					foreach(var item2 in item0.IdsRoh_projectClassificationNode)
					{
					Regex regex = new Regex(@"\/items\/.+_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}");
					string itemRegex = item2;
					if (regex.IsMatch(itemRegex))
					{
						itemRegex = $"http://gnoss/{resourceAPI.GetShortGuid(itemRegex).ToString().ToUpper()}";
					}
					else
					{
						itemRegex = itemRegex.ToLower();
					}
						AgregarTripleALista($"{resourceAPI.GraphsUrl}items/projectclassification_{ResourceID}_{item0.ArticleID}", "http://w3id.org/roh/projectClassificationNode", $"<{itemRegex}>", list, " . ");
					}
				}
			}
			}
			if(this.Roh_hasResultsProjectClassification != null)
			{
			foreach(var item0 in this.Roh_hasResultsProjectClassification)
			{
				AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}", "http://w3id.org/roh/hasResultsProjectClassification", $"<{resourceAPI.GraphsUrl}items/projectclassification_{ResourceID}_{item0.ArticleID}>", list, " . ");
				if(item0.IdsRoh_projectClassificationNode != null)
				{
					foreach(var item2 in item0.IdsRoh_projectClassificationNode)
					{
					Regex regex = new Regex(@"\/items\/.+_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}");
					string itemRegex = item2;
					if (regex.IsMatch(itemRegex))
					{
						itemRegex = $"http://gnoss/{resourceAPI.GetShortGuid(itemRegex).ToString().ToUpper()}";
					}
					else
					{
						itemRegex = itemRegex.ToLower();
					}
						AgregarTripleALista($"{resourceAPI.GraphsUrl}items/projectclassification_{ResourceID}_{item0.ArticleID}", "http://w3id.org/roh/projectClassificationNode", $"<{itemRegex}>", list, " . ");
					}
				}
			}
			}
			if(this.Roh_mainResearchers != null)
			{
			foreach(var item0 in this.Roh_mainResearchers)
			{
				AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}", "http://w3id.org/roh/mainResearchers", $"<{resourceAPI.GraphsUrl}items/bfo_0000023_{ResourceID}_{item0.ArticleID}>", list, " . ");
				if(item0.Vivo_start != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/bfo_0000023_{ResourceID}_{item0.ArticleID}",  "http://vivoweb.org/ontology/core#start", $"{item0.Vivo_start.Value.ToString("yyyyMMddHHmmss")}", list, " . ");
				}
				if(item0.Vivo_end != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/bfo_0000023_{ResourceID}_{item0.ArticleID}",  "http://vivoweb.org/ontology/core#end", $"{item0.Vivo_end.Value.ToString("yyyyMMddHHmmss")}", list, " . ");
				}
				if(item0.Foaf_nick != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/bfo_0000023_{ResourceID}_{item0.ArticleID}",  "http://xmlns.com/foaf/0.1/nick", $"\"{GenerarTextoSinSaltoDeLinea(item0.Foaf_nick).ToLower()}\"", list, " . ");
				}
				if(item0.IdRdf_member != null)
				{
					Regex regex = new Regex(@"\/items\/.+_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}");
					string itemRegex = item0.IdRdf_member;
					if (regex.IsMatch(itemRegex))
					{
						itemRegex = $"http://gnoss/{resourceAPI.GetShortGuid(itemRegex).ToString().ToUpper()}";
					}
					else
					{
						itemRegex = itemRegex.ToLower();
					}
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/bfo_0000023_{ResourceID}_{item0.ArticleID}",  "http://www.w3.org/1999/02/22-rdf-syntax-ns#member", $"<{itemRegex}>", list, " . ");
				}
				if(item0.Rdf_comment != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/bfo_0000023_{ResourceID}_{item0.ArticleID}",  "http://www.w3.org/1999/02/22-rdf-syntax-ns#comment", $"{item0.Rdf_comment.ToString()}", list, " . ");
				}
			}
			}
			if(this.Vivo_relates != null)
			{
			foreach(var item0 in this.Vivo_relates)
			{
				AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}", "http://vivoweb.org/ontology/core#relates", $"<{resourceAPI.GraphsUrl}items/bfo_0000023_{ResourceID}_{item0.ArticleID}>", list, " . ");
				if(item0.Vivo_start != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/bfo_0000023_{ResourceID}_{item0.ArticleID}",  "http://vivoweb.org/ontology/core#start", $"{item0.Vivo_start.Value.ToString("yyyyMMddHHmmss")}", list, " . ");
				}
				if(item0.Vivo_end != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/bfo_0000023_{ResourceID}_{item0.ArticleID}",  "http://vivoweb.org/ontology/core#end", $"{item0.Vivo_end.Value.ToString("yyyyMMddHHmmss")}", list, " . ");
				}
				if(item0.Foaf_nick != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/bfo_0000023_{ResourceID}_{item0.ArticleID}",  "http://xmlns.com/foaf/0.1/nick", $"\"{GenerarTextoSinSaltoDeLinea(item0.Foaf_nick).ToLower()}\"", list, " . ");
				}
				if(item0.IdRdf_member != null)
				{
					Regex regex = new Regex(@"\/items\/.+_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}");
					string itemRegex = item0.IdRdf_member;
					if (regex.IsMatch(itemRegex))
					{
						itemRegex = $"http://gnoss/{resourceAPI.GetShortGuid(itemRegex).ToString().ToUpper()}";
					}
					else
					{
						itemRegex = itemRegex.ToLower();
					}
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/bfo_0000023_{ResourceID}_{item0.ArticleID}",  "http://www.w3.org/1999/02/22-rdf-syntax-ns#member", $"<{itemRegex}>", list, " . ");
				}
				if(item0.Rdf_comment != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/bfo_0000023_{ResourceID}_{item0.ArticleID}",  "http://www.w3.org/1999/02/22-rdf-syntax-ns#comment", $"{item0.Rdf_comment.ToString()}", list, " . ");
				}
			}
			}
				if(this.IdsRoh_publicGroupList != null)
				{
					foreach(var item2 in this.IdsRoh_publicGroupList)
					{
					Regex regex = new Regex(@"\/items\/.+_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}");
					string itemRegex = item2;
					if (regex.IsMatch(itemRegex))
					{
						itemRegex = $"http://gnoss/{resourceAPI.GetShortGuid(itemRegex).ToString().ToUpper()}";
					}
					else
					{
						itemRegex = itemRegex.ToLower();
					}
						AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}", "http://w3id.org/roh/publicGroupList", $"<{itemRegex}>", list, " . ");
					}
				}
				if(this.IdsRoh_grantedBy != null)
				{
					foreach(var item2 in this.IdsRoh_grantedBy)
					{
					Regex regex = new Regex(@"\/items\/.+_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}");
					string itemRegex = item2;
					if (regex.IsMatch(itemRegex))
					{
						itemRegex = $"http://gnoss/{resourceAPI.GetShortGuid(itemRegex).ToString().ToUpper()}";
					}
					else
					{
						itemRegex = itemRegex.ToLower();
					}
						AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}", "http://w3id.org/roh/grantedBy", $"<{itemRegex}>", list, " . ");
					}
				}
				if(this.IdsRoh_publicAuthorList != null)
				{
					foreach(var item2 in this.IdsRoh_publicAuthorList)
					{
					Regex regex = new Regex(@"\/items\/.+_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}");
					string itemRegex = item2;
					if (regex.IsMatch(itemRegex))
					{
						itemRegex = $"http://gnoss/{resourceAPI.GetShortGuid(itemRegex).ToString().ToUpper()}";
					}
					else
					{
						itemRegex = itemRegex.ToLower();
					}
						AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}", "http://w3id.org/roh/publicAuthorList", $"<{itemRegex}>", list, " . ");
					}
				}
				if(this.IdsRoh_participates != null)
				{
					foreach(var item2 in this.IdsRoh_participates)
					{
					Regex regex = new Regex(@"\/items\/.+_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}");
					string itemRegex = item2;
					if (regex.IsMatch(itemRegex))
					{
						itemRegex = $"http://gnoss/{resourceAPI.GetShortGuid(itemRegex).ToString().ToUpper()}";
					}
					else
					{
						itemRegex = itemRegex.ToLower();
					}
						AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}", "http://w3id.org/roh/participates", $"<{itemRegex}>", list, " . ");
					}
				}
				if(this.IdRoh_conductedBy != null)
				{
					Regex regex = new Regex(@"\/items\/.+_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}");
					string itemRegex = this.IdRoh_conductedBy;
					if (regex.IsMatch(itemRegex))
					{
						itemRegex = $"http://gnoss/{resourceAPI.GetShortGuid(itemRegex).ToString().ToUpper()}";
					}
					else
					{
						itemRegex = itemRegex.ToLower();
					}
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "http://w3id.org/roh/conductedBy", $"<{itemRegex}>", list, " . ");
				}
				if(this.IdVivo_geographicFocus != null)
				{
					Regex regex = new Regex(@"\/items\/.+_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}");
					string itemRegex = this.IdVivo_geographicFocus;
					if (regex.IsMatch(itemRegex))
					{
						itemRegex = $"http://gnoss/{resourceAPI.GetShortGuid(itemRegex).ToString().ToUpper()}";
					}
					else
					{
						itemRegex = itemRegex.ToLower();
					}
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "http://vivoweb.org/ontology/core#geographicFocus", $"<{itemRegex}>", list, " . ");
				}
				if(this.IdRoh_projectType != null)
				{
					Regex regex = new Regex(@"\/items\/.+_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}");
					string itemRegex = this.IdRoh_projectType;
					if (regex.IsMatch(itemRegex))
					{
						itemRegex = $"http://gnoss/{resourceAPI.GetShortGuid(itemRegex).ToString().ToUpper()}";
					}
					else
					{
						itemRegex = itemRegex.ToLower();
					}
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "http://w3id.org/roh/projectType", $"<{itemRegex}>", list, " . ");
				}
				if(this.IdRoh_modality != null)
				{
					Regex regex = new Regex(@"\/items\/.+_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}");
					string itemRegex = this.IdRoh_modality;
					if (regex.IsMatch(itemRegex))
					{
						itemRegex = $"http://gnoss/{resourceAPI.GetShortGuid(itemRegex).ToString().ToUpper()}";
					}
					else
					{
						itemRegex = itemRegex.ToLower();
					}
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "http://w3id.org/roh/modality", $"<{itemRegex}>", list, " . ");
				}
				if(this.Roh_relevantResults != null)
				{
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "http://w3id.org/roh/relevantResults", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_relevantResults).ToLower()}\"", list, " . ");
				}
				if(this.Roh_peopleYearNumber != null)
				{
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "http://w3id.org/roh/peopleYearNumber", $"{this.Roh_peopleYearNumber.Value.ToString()}", list, " . ");
				}
				if(this.Roh_durationMonths != null)
				{
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "http://w3id.org/roh/durationMonths", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_durationMonths).ToLower()}\"", list, " . ");
				}
				if(this.Roh_geographicFocusOther != null)
				{
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "http://w3id.org/roh/geographicFocusOther", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_geographicFocusOther).ToLower()}\"", list, " . ");
				}
				if(this.Roh_researchersNumber != null)
				{
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "http://w3id.org/roh/researchersNumber", $"{this.Roh_researchersNumber.Value.ToString()}", list, " . ");
				}
				if(this.Roh_durationDays != null)
				{
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "http://w3id.org/roh/durationDays", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_durationDays).ToLower()}\"", list, " . ");
				}
				if(this.Vivo_start != null)
				{
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "http://vivoweb.org/ontology/core#start", $"{this.Vivo_start.Value.ToString("yyyyMMddHHmmss")}", list, " . ");
				}
				if(this.Roh_durationYears != null)
				{
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "http://w3id.org/roh/durationYears", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_durationYears).ToLower()}\"", list, " . ");
				}
				if(this.Roh_crisIdentifier != null)
				{
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "http://w3id.org/roh/crisIdentifier", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_crisIdentifier).ToLower()}\"", list, " . ");
				}
				if(this.Vivo_end != null)
				{
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "http://vivoweb.org/ontology/core#end", $"{this.Vivo_end.Value.ToString("yyyyMMddHHmmss")}", list, " . ");
				}
				if(this.Vivo_description != null)
				{
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "http://vivoweb.org/ontology/core#description", $"\"{GenerarTextoSinSaltoDeLinea(this.Vivo_description).ToLower()}\"", list, " . ");
				}
				if(this.Roh_monetaryAmount != null)
				{
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "http://w3id.org/roh/monetaryAmount", $"{this.Roh_monetaryAmount.Value.ToString(new CultureInfo("en-US"))}", list, " . ");
				}
				if(this.Vivo_abbreviation != null)
				{
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "http://vivoweb.org/ontology/core#abbreviation", $"\"{GenerarTextoSinSaltoDeLinea(this.Vivo_abbreviation).ToLower()}\"", list, " . ");
				}
				if(this.IdRoh_scientificExperienceProject != null)
				{
					Regex regex = new Regex(@"\/items\/.+_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}");
					string itemRegex = this.IdRoh_scientificExperienceProject;
					if (regex.IsMatch(itemRegex))
					{
						itemRegex = $"http://gnoss/{resourceAPI.GetShortGuid(itemRegex).ToString().ToUpper()}";
					}
					else
					{
						itemRegex = itemRegex.ToLower();
					}
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "http://w3id.org/roh/scientificExperienceProject", $"<{itemRegex}>", list, " . ");
				}
				if(this.Roh_classificationCVN != null)
				{
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "http://w3id.org/roh/classificationCVN", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_classificationCVN).ToLower()}\"", list, " . ");
				}
				if(this.Roh_isSynchronized != null)
				{
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "http://w3id.org/roh/isSynchronized", $"\"{this.Roh_isSynchronized.ToString().ToLower()}\"", list, " . ");
				}
				if(this.Roh_isPublic != null)
				{
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "http://w3id.org/roh/isPublic", $"\"{this.Roh_isPublic.ToString().ToLower()}\"", list, " . ");
				}
				if(this.Roh_title != null)
				{
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "http://w3id.org/roh/title", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_title).ToLower()}\"", list, " . ");
				}
			if (listaSearch != null && listaSearch.Count > 0)
			{
				foreach(string valorSearch in listaSearch)
				{
					search += $"{valorSearch} ";
				}
			}
			if(!string.IsNullOrEmpty(search))
			{
				AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}", "http://gnoss/search", $"\"{search.ToLower()}\"", list, " . ");
			}
			return list;
		}

		public override KeyValuePair<Guid, string> ToAcidData(ResourceApi resourceAPI)
		{

			//Insert en la tabla Documento
			string titulo = $"{this.Roh_title.Replace("\r\n", "").Replace("\n", "").Replace("\r", "").Replace("\"", "\"\"").Replace("'", "''").Replace("|", "#PIPE#")}";
			string descripcion = $"{this.Roh_title.Replace("\r\n", "").Replace("\n", "").Replace("\r", "").Replace("\"", "\"\"").Replace("'", "''").Replace("|", "#PIPE#")}";
			string tablaDoc = $"'{titulo}', '{descripcion}', '{resourceAPI.GraphsUrl}'";
			KeyValuePair<Guid, string> valor = new KeyValuePair<Guid, string>(ResourceID, tablaDoc);

			return valor;
		}

		protected List<object> ObtenerObjetosDePropiedad(object propiedad)
		{
			List<object> lista = new List<object>();
			if(propiedad is IList)
			{
				foreach (object item in (IList)propiedad)
				{
					lista.Add(item);
				}
			}
			else
			{
				lista.Add(propiedad);
			}
			return lista;
		}
		protected List<string> ObtenerStringDePropiedad(object propiedad)
		{
			List<string> lista = new List<string>();
			if (propiedad is IList)
			{
				foreach (string item in (IList)propiedad)
				{
					lista.Add(item);
				}
			}
			else if (propiedad is IDictionary)
			{
				foreach (object key in ((IDictionary)propiedad).Keys)
				{
					if (((IDictionary)propiedad)[key] is IList)
					{
						List<string> listaValores = (List<string>)((IDictionary)propiedad)[key];
						foreach(string valor in listaValores)
						{
							lista.Add(valor);
						}
					}
					else
					{
					lista.Add((string)((IDictionary)propiedad)[key]);
					}
				}
			}
			else if (propiedad is string)
			{
				lista.Add((string)propiedad);
			}
			return lista;
		}
		public override string GetURI(ResourceApi resourceAPI)
		{
			return $"{resourceAPI.GraphsUrl}items/ProjectOntology_{ResourceID}_{ArticleID}";
		}

		private string GenerarTextoSinSaltoDeLinea(string pTexto)
		{
			return pTexto.Replace("\r\n", " ").Replace("\n", " ").Replace("\r", " ").Replace("\"", "\\\"");
		}

		internal void AddResourceTitle(ComplexOntologyResource resource)
		{
			resource.Title = this.Roh_title;
		}

		internal void AddResourceDescription(ComplexOntologyResource resource)
		{
			resource.Description = this.Roh_title;
		}

		private void AgregarTripleALista(string pSujeto, string pPredicado, string pObjeto, List<string> pLista, string pDatosExtra)
		{
			if(!string.IsNullOrEmpty(pObjeto) && !pObjeto.Equals("\"\"") && !pObjeto.Equals("<>"))
			{
				pLista.Add($"<{pSujeto}> <{pPredicado}> {pObjeto}{pDatosExtra}");
			} 
		} 

		private void AgregarTags(List<string> pListaTriples)
		{
			foreach(string tag in tagList)
			{
				AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}", "http://rdfs.org/sioc/types#Tag", tag.ToLower(), pListaTriples, " . ");
			}
		}


	}
}
