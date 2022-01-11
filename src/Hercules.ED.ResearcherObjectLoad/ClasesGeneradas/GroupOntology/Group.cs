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
using System.Diagnostics.CodeAnalysis;
using Organization = OrganizationOntology.Organization;
using ColaborationTypeGroup = ColaborationtypegroupOntology.ColaborationTypeGroup;

namespace GroupOntology
{
	[ExcludeFromCodeCoverage]
	public class Group : GnossOCBase
	{

		public Group() : base() { } 

		public Group(SemanticResourceModel pSemCmsModel, LanguageEnum idiomaUsuario) : base()
		{
			this.mGNOSSID = pSemCmsModel.RootEntities[0].Entity.Uri;
			SemanticPropertyModel propVivo_affiliatedOrganization = pSemCmsModel.GetPropertyByPath("http://vivoweb.org/ontology/core#affiliatedOrganization");
			if(propVivo_affiliatedOrganization != null && propVivo_affiliatedOrganization.PropertyValues.Count > 0)
			{
				this.Vivo_affiliatedOrganization = new Organization(propVivo_affiliatedOrganization.PropertyValues[0].RelatedEntity,idiomaUsuario);
			}
			this.Roh_hasResultsGroupClassification = new List<GroupClassification>();
			SemanticPropertyModel propRoh_hasResultsGroupClassification = pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/hasResultsGroupClassification");
			if(propRoh_hasResultsGroupClassification != null && propRoh_hasResultsGroupClassification.PropertyValues.Count > 0)
			{
				foreach (SemanticPropertyModel.PropertyValue propValue in propRoh_hasResultsGroupClassification.PropertyValues)
				{
					if(propValue.RelatedEntity!=null){
						GroupClassification roh_hasResultsGroupClassification = new GroupClassification(propValue.RelatedEntity,idiomaUsuario);
						this.Roh_hasResultsGroupClassification.Add(roh_hasResultsGroupClassification);
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
			this.Foaf_member = new List<BFO_0000023>();
			SemanticPropertyModel propFoaf_member = pSemCmsModel.GetPropertyByPath("http://xmlns.com/foaf/0.1/member");
			if(propFoaf_member != null && propFoaf_member.PropertyValues.Count > 0)
			{
				foreach (SemanticPropertyModel.PropertyValue propValue in propFoaf_member.PropertyValues)
				{
					if(propValue.RelatedEntity!=null){
						BFO_0000023 foaf_member = new BFO_0000023(propValue.RelatedEntity,idiomaUsuario);
						this.Foaf_member.Add(foaf_member);
					}
				}
			}
			SemanticPropertyModel propRoh_colaborationTypeGroup = pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/colaborationTypeGroup");
			if(propRoh_colaborationTypeGroup != null && propRoh_colaborationTypeGroup.PropertyValues.Count > 0)
			{
				this.Roh_colaborationTypeGroup = new ColaborationTypeGroup(propRoh_colaborationTypeGroup.PropertyValues[0].RelatedEntity,idiomaUsuario);
			}
			this.Roh_hasKnowledgeArea = new List<CategoryPath>();
			SemanticPropertyModel propRoh_hasKnowledgeArea = pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/hasKnowledgeArea");
			if(propRoh_hasKnowledgeArea != null && propRoh_hasKnowledgeArea.PropertyValues.Count > 0)
			{
				foreach (SemanticPropertyModel.PropertyValue propValue in propRoh_hasKnowledgeArea.PropertyValues)
				{
					if(propValue.RelatedEntity!=null){
						CategoryPath roh_hasKnowledgeArea = new CategoryPath(propValue.RelatedEntity,idiomaUsuario);
						this.Roh_hasKnowledgeArea.Add(roh_hasKnowledgeArea);
					}
				}
			}
			this.Roh_relevantResults = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/relevantResults"));
			SemanticPropertyModel propRoh_lineResearch = pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/lineResearch");
			this.Roh_lineResearch = new List<string>();
			if (propRoh_lineResearch != null && propRoh_lineResearch.PropertyValues.Count > 0)
			{
				foreach (SemanticPropertyModel.PropertyValue propValue in propRoh_lineResearch.PropertyValues)
				{
					this.Roh_lineResearch.Add(propValue.Value);
				}
			}
			this.Roh_publicationsNumber = GetNumberIntPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/publicationsNumber"));
			this.Roh_durationDays = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/durationDays"));
			this.Roh_normalizedCode = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/normalizedCode"));
			this.Vivo_description = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://vivoweb.org/ontology/core#description"));
			this.Roh_themedAreasNumber = GetNumberIntPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/themedAreasNumber"));
			this.Roh_otherRelevantResults = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/otherRelevantResults"));
			this.Roh_durationMonths = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/durationMonths"));
			this.Roh_directedPostdocsNumber = GetNumberIntPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/directedPostdocsNumber"));
			this.Roh_researchersNumber = GetNumberIntPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/researchersNumber"));
			this.Roh_durationYears = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/durationYears"));
			this.Roh_projectsNumber = GetNumberIntPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/projectsNumber"));
			this.Roh_crisIdentifier = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/crisIdentifier"));
			this.Roh_foundationDate= GetDateValuePropertySemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/foundationDate"));
			this.Roh_directedThesisNumber = GetNumberIntPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/directedThesisNumber"));
			this.Roh_collaboratorsNumber = GetNumberIntPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/collaboratorsNumber"));
			this.Roh_isPublic= GetBooleanPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/isPublic"));
			this.Roh_title = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/title"));
		}

		public Group(SemanticEntityModel pSemCmsModel, LanguageEnum idiomaUsuario) : base()
		{
			this.mGNOSSID = pSemCmsModel.Entity.Uri;
			this.mURL = pSemCmsModel.Properties.FirstOrDefault(p => p.PropertyValues.Any(prop => prop.DownloadUrl != null))?.FirstPropertyValue.DownloadUrl;
			SemanticPropertyModel propVivo_affiliatedOrganization = pSemCmsModel.GetPropertyByPath("http://vivoweb.org/ontology/core#affiliatedOrganization");
			if(propVivo_affiliatedOrganization != null && propVivo_affiliatedOrganization.PropertyValues.Count > 0)
			{
				this.Vivo_affiliatedOrganization = new Organization(propVivo_affiliatedOrganization.PropertyValues[0].RelatedEntity,idiomaUsuario);
			}
			this.Roh_hasResultsGroupClassification = new List<GroupClassification>();
			SemanticPropertyModel propRoh_hasResultsGroupClassification = pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/hasResultsGroupClassification");
			if(propRoh_hasResultsGroupClassification != null && propRoh_hasResultsGroupClassification.PropertyValues.Count > 0)
			{
				foreach (SemanticPropertyModel.PropertyValue propValue in propRoh_hasResultsGroupClassification.PropertyValues)
				{
					if(propValue.RelatedEntity!=null){
						GroupClassification roh_hasResultsGroupClassification = new GroupClassification(propValue.RelatedEntity,idiomaUsuario);
						this.Roh_hasResultsGroupClassification.Add(roh_hasResultsGroupClassification);
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
			this.Foaf_member = new List<BFO_0000023>();
			SemanticPropertyModel propFoaf_member = pSemCmsModel.GetPropertyByPath("http://xmlns.com/foaf/0.1/member");
			if(propFoaf_member != null && propFoaf_member.PropertyValues.Count > 0)
			{
				foreach (SemanticPropertyModel.PropertyValue propValue in propFoaf_member.PropertyValues)
				{
					if(propValue.RelatedEntity!=null){
						BFO_0000023 foaf_member = new BFO_0000023(propValue.RelatedEntity,idiomaUsuario);
						this.Foaf_member.Add(foaf_member);
					}
				}
			}
			SemanticPropertyModel propRoh_colaborationTypeGroup = pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/colaborationTypeGroup");
			if(propRoh_colaborationTypeGroup != null && propRoh_colaborationTypeGroup.PropertyValues.Count > 0)
			{
				this.Roh_colaborationTypeGroup = new ColaborationTypeGroup(propRoh_colaborationTypeGroup.PropertyValues[0].RelatedEntity,idiomaUsuario);
			}
			this.Roh_hasKnowledgeArea = new List<CategoryPath>();
			SemanticPropertyModel propRoh_hasKnowledgeArea = pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/hasKnowledgeArea");
			if(propRoh_hasKnowledgeArea != null && propRoh_hasKnowledgeArea.PropertyValues.Count > 0)
			{
				foreach (SemanticPropertyModel.PropertyValue propValue in propRoh_hasKnowledgeArea.PropertyValues)
				{
					if(propValue.RelatedEntity!=null){
						CategoryPath roh_hasKnowledgeArea = new CategoryPath(propValue.RelatedEntity,idiomaUsuario);
						this.Roh_hasKnowledgeArea.Add(roh_hasKnowledgeArea);
					}
				}
			}
			this.Roh_relevantResults = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/relevantResults"));
			SemanticPropertyModel propRoh_lineResearch = pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/lineResearch");
			this.Roh_lineResearch = new List<string>();
			if (propRoh_lineResearch != null && propRoh_lineResearch.PropertyValues.Count > 0)
			{
				foreach (SemanticPropertyModel.PropertyValue propValue in propRoh_lineResearch.PropertyValues)
				{
					this.Roh_lineResearch.Add(propValue.Value);
				}
			}
			this.Roh_publicationsNumber = GetNumberIntPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/publicationsNumber"));
			this.Roh_durationDays = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/durationDays"));
			this.Roh_normalizedCode = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/normalizedCode"));
			this.Vivo_description = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://vivoweb.org/ontology/core#description"));
			this.Roh_themedAreasNumber = GetNumberIntPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/themedAreasNumber"));
			this.Roh_otherRelevantResults = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/otherRelevantResults"));
			this.Roh_durationMonths = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/durationMonths"));
			this.Roh_directedPostdocsNumber = GetNumberIntPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/directedPostdocsNumber"));
			this.Roh_researchersNumber = GetNumberIntPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/researchersNumber"));
			this.Roh_durationYears = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/durationYears"));
			this.Roh_projectsNumber = GetNumberIntPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/projectsNumber"));
			this.Roh_crisIdentifier = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/crisIdentifier"));
			this.Roh_foundationDate= GetDateValuePropertySemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/foundationDate"));
			this.Roh_directedThesisNumber = GetNumberIntPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/directedThesisNumber"));
			this.Roh_collaboratorsNumber = GetNumberIntPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/collaboratorsNumber"));
			this.Roh_isPublic= GetBooleanPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/isPublic"));
			this.Roh_title = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/title"));
		}

		public virtual string RdfType { get { return "http://xmlns.com/foaf/0.1/Group"; } }
		public virtual string RdfsLabel { get { return "http://xmlns.com/foaf/0.1/Group"; } }
		[LABEL(LanguageEnum.es,"Entidad de afiliación")]
		[RDFProperty("http://vivoweb.org/ontology/core#affiliatedOrganization")]
		public  Organization Vivo_affiliatedOrganization  { get; set;} 
		public string IdVivo_affiliatedOrganization  { get; set;} 

		[LABEL(LanguageEnum.es,"Resultados de la clasificación del grupo")]
		[RDFProperty("http://w3id.org/roh/hasResultsGroupClassification")]
		public  List<GroupClassification> Roh_hasResultsGroupClassification { get; set;}

		[LABEL(LanguageEnum.es,"Investigadores principal")]
		[RDFProperty("http://w3id.org/roh/mainResearchers")]
		public  List<BFO_0000023> Roh_mainResearchers { get; set;}

		[LABEL(LanguageEnum.es,"Miembros")]
		[RDFProperty("http://xmlns.com/foaf/0.1/member")]
		public  List<BFO_0000023> Foaf_member { get; set;}

		[LABEL(LanguageEnum.es,"Tipo de colaboración")]
		[RDFProperty("http://w3id.org/roh/colaborationTypeGroup")]
		public  ColaborationTypeGroup Roh_colaborationTypeGroup  { get; set;} 
		public string IdRoh_colaborationTypeGroup  { get; set;} 

		[RDFProperty("http://w3id.org/roh/hasKnowledgeArea")]
		public  List<CategoryPath> Roh_hasKnowledgeArea { get; set;}

		[LABEL(LanguageEnum.es,"Resultados relevantes")]
		[RDFProperty("http://w3id.org/roh/relevantResults")]
		public  string Roh_relevantResults { get; set;}

		[RDFProperty("http://w3id.org/roh/lineResearch")]
		public  List<string> Roh_lineResearch { get; set;}

		[LABEL(LanguageEnum.es,"Número de publicaciones")]
		[RDFProperty("http://w3id.org/roh/publicationsNumber")]
		public  int? Roh_publicationsNumber { get; set;}

		[LABEL(LanguageEnum.es,"Duración (días)")]
		[RDFProperty("http://w3id.org/roh/durationDays")]
		public  string Roh_durationDays { get; set;}

		[LABEL(LanguageEnum.es,"Código normalizado")]
		[RDFProperty("http://w3id.org/roh/normalizedCode")]
		public  string Roh_normalizedCode { get; set;}

		[LABEL(LanguageEnum.es,"Descripción")]
		[RDFProperty("http://vivoweb.org/ontology/core#description")]
		public  string Vivo_description { get; set;}

		[LABEL(LanguageEnum.es,"Número de áreas temáticas")]
		[RDFProperty("http://w3id.org/roh/themedAreasNumber")]
		public  int? Roh_themedAreasNumber { get; set;}

		[LABEL(LanguageEnum.es,"Resultados relevantes, otros")]
		[RDFProperty("http://w3id.org/roh/otherRelevantResults")]
		public  string Roh_otherRelevantResults { get; set;}

		[LABEL(LanguageEnum.es,"Duración (meses)")]
		[RDFProperty("http://w3id.org/roh/durationMonths")]
		public  string Roh_durationMonths { get; set;}

		[LABEL(LanguageEnum.es,"Número de postdoctorados dirigidos")]
		[RDFProperty("http://w3id.org/roh/directedPostdocsNumber")]
		public  int? Roh_directedPostdocsNumber { get; set;}

		[LABEL(LanguageEnum.es,"Número de investigadores")]
		[RDFProperty("http://w3id.org/roh/researchersNumber")]
		public  int? Roh_researchersNumber { get; set;}

		[LABEL(LanguageEnum.es,"Duración (años)")]
		[RDFProperty("http://w3id.org/roh/durationYears")]
		public  string Roh_durationYears { get; set;}

		[LABEL(LanguageEnum.es,"Número de proyectos")]
		[RDFProperty("http://w3id.org/roh/projectsNumber")]
		public  int? Roh_projectsNumber { get; set;}

		[LABEL(LanguageEnum.es,"Identificador")]
		[RDFProperty("http://w3id.org/roh/crisIdentifier")]
		public  string Roh_crisIdentifier { get; set;}

		[LABEL(LanguageEnum.es,"Fecha de inicio")]
		[RDFProperty("http://w3id.org/roh/foundationDate")]
		public  DateTime? Roh_foundationDate { get; set;}

		[LABEL(LanguageEnum.es,"Número de tesis dirigidas")]
		[RDFProperty("http://w3id.org/roh/directedThesisNumber")]
		public  int? Roh_directedThesisNumber { get; set;}

		[LABEL(LanguageEnum.es,"Número de colaboradores")]
		[RDFProperty("http://w3id.org/roh/collaboratorsNumber")]
		public  int? Roh_collaboratorsNumber { get; set;}

		[LABEL(LanguageEnum.es,"http://w3id.org/roh/isPublic")]
		[RDFProperty("http://w3id.org/roh/isPublic")]
		public  bool Roh_isPublic { get; set;}

		[LABEL(LanguageEnum.es,"Nombre del grupo")]
		[RDFProperty("http://w3id.org/roh/title")]
		public  string Roh_title { get; set;}


		internal override void GetProperties()
		{
			base.GetProperties();
			propList.Add(new StringOntologyProperty("vivo:affiliatedOrganization", this.IdVivo_affiliatedOrganization));
			propList.Add(new StringOntologyProperty("roh:colaborationTypeGroup", this.IdRoh_colaborationTypeGroup));
			propList.Add(new StringOntologyProperty("roh:relevantResults", this.Roh_relevantResults));
			propList.Add(new ListStringOntologyProperty("roh:lineResearch", this.Roh_lineResearch));
			propList.Add(new StringOntologyProperty("roh:publicationsNumber", this.Roh_publicationsNumber.ToString()));
			propList.Add(new StringOntologyProperty("roh:durationDays", this.Roh_durationDays));
			propList.Add(new StringOntologyProperty("roh:normalizedCode", this.Roh_normalizedCode));
			propList.Add(new StringOntologyProperty("vivo:description", this.Vivo_description));
			propList.Add(new StringOntologyProperty("roh:themedAreasNumber", this.Roh_themedAreasNumber.ToString()));
			propList.Add(new StringOntologyProperty("roh:otherRelevantResults", this.Roh_otherRelevantResults));
			propList.Add(new StringOntologyProperty("roh:durationMonths", this.Roh_durationMonths));
			propList.Add(new StringOntologyProperty("roh:directedPostdocsNumber", this.Roh_directedPostdocsNumber.ToString()));
			propList.Add(new StringOntologyProperty("roh:researchersNumber", this.Roh_researchersNumber.ToString()));
			propList.Add(new StringOntologyProperty("roh:durationYears", this.Roh_durationYears));
			propList.Add(new StringOntologyProperty("roh:projectsNumber", this.Roh_projectsNumber.ToString()));
			propList.Add(new StringOntologyProperty("roh:crisIdentifier", this.Roh_crisIdentifier));
			if (this.Roh_foundationDate.HasValue){
				propList.Add(new DateOntologyProperty("roh:foundationDate", this.Roh_foundationDate.Value));
				}
			propList.Add(new StringOntologyProperty("roh:directedThesisNumber", this.Roh_directedThesisNumber.ToString()));
			propList.Add(new StringOntologyProperty("roh:collaboratorsNumber", this.Roh_collaboratorsNumber.ToString()));
			propList.Add(new BoolOntologyProperty("roh:isPublic", this.Roh_isPublic));
			propList.Add(new StringOntologyProperty("roh:title", this.Roh_title));
		}

		internal override void GetEntities()
		{
			base.GetEntities();
			if(Roh_hasResultsGroupClassification!=null){
				foreach(GroupClassification prop in Roh_hasResultsGroupClassification){
					prop.GetProperties();
					prop.GetEntities();
					OntologyEntity entityGroupClassification = new OntologyEntity("http://w3id.org/roh/GroupClassification", "http://w3id.org/roh/GroupClassification", "roh:hasResultsGroupClassification", prop.propList, prop.entList);
				entList.Add(entityGroupClassification);
				prop.Entity= entityGroupClassification;
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
			if(Foaf_member!=null){
				foreach(BFO_0000023 prop in Foaf_member){
					prop.GetProperties();
					prop.GetEntities();
					OntologyEntity entityBFO_0000023 = new OntologyEntity("http://purl.obolibrary.org/obo/BFO_0000023", "http://purl.obolibrary.org/obo/BFO_0000023", "foaf:member", prop.propList, prop.entList);
				entList.Add(entityBFO_0000023);
				prop.Entity= entityBFO_0000023;
				}
			}
			if(Roh_hasKnowledgeArea!=null){
				foreach(CategoryPath prop in Roh_hasKnowledgeArea){
					prop.GetProperties();
					prop.GetEntities();
					OntologyEntity entityCategoryPath = new OntologyEntity("http://w3id.org/roh/CategoryPath", "http://w3id.org/roh/CategoryPath", "roh:hasKnowledgeArea", prop.propList, prop.entList);
				entList.Add(entityCategoryPath);
				prop.Entity= entityCategoryPath;
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
			AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Group_{ResourceID}_{ArticleID}", "http://www.w3.org/1999/02/22-rdf-syntax-ns#type", $"<http://xmlns.com/foaf/0.1/Group>", list, " . ");
			AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Group_{ResourceID}_{ArticleID}", "http://www.w3.org/2000/01/rdf-schema#label", $"\"http://xmlns.com/foaf/0.1/Group\"", list, " . ");
			AgregarTripleALista($"{resourceAPI.GraphsUrl}{ResourceID}", "http://gnoss/hasEntidad", $"<{resourceAPI.GraphsUrl}items/Group_{ResourceID}_{ArticleID}>", list, " . ");
			if(this.Roh_hasResultsGroupClassification != null)
			{
			foreach(var item0 in this.Roh_hasResultsGroupClassification)
			{
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/GroupClassification_{ResourceID}_{item0.ArticleID}", "http://www.w3.org/1999/02/22-rdf-syntax-ns#type", $"<http://w3id.org/roh/GroupClassification>", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/GroupClassification_{ResourceID}_{item0.ArticleID}", "http://www.w3.org/2000/01/rdf-schema#label", $"\"http://w3id.org/roh/GroupClassification\"", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}{ResourceID}", "http://gnoss/hasEntidad", $"<{resourceAPI.GraphsUrl}items/GroupClassification_{ResourceID}_{item0.ArticleID}>", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Group_{ResourceID}_{ArticleID}", "http://w3id.org/roh/hasResultsGroupClassification", $"<{resourceAPI.GraphsUrl}items/GroupClassification_{ResourceID}_{item0.ArticleID}>", list, " . ");
				if(item0.IdsRoh_groupClassificationNode != null)
				{
					foreach(var item2 in item0.IdsRoh_groupClassificationNode)
					{
						AgregarTripleALista($"{resourceAPI.GraphsUrl}items/GroupClassification_{ResourceID}_{item0.ArticleID}", "http://w3id.org/roh/groupClassificationNode", $"<{item2}>", list, " . ");
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
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Group_{ResourceID}_{ArticleID}", "http://w3id.org/roh/mainResearchers", $"<{resourceAPI.GraphsUrl}items/BFO_0000023_{ResourceID}_{item0.ArticleID}>", list, " . ");
			if(item0.Vivo_hasResearchArea != null)
			{
			foreach(var item1 in item0.Vivo_hasResearchArea)
			{
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/ResearchArea_{ResourceID}_{item1.ArticleID}", "http://www.w3.org/1999/02/22-rdf-syntax-ns#type", $"<http://w3id.org/roh/ResearchArea>", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/ResearchArea_{ResourceID}_{item1.ArticleID}", "http://www.w3.org/2000/01/rdf-schema#label", $"\"http://w3id.org/roh/ResearchArea\"", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}{ResourceID}", "http://gnoss/hasEntidad", $"<{resourceAPI.GraphsUrl}items/ResearchArea_{ResourceID}_{item1.ArticleID}>", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/BFO_0000023_{ResourceID}_{item0.ArticleID}", "http://vivoweb.org/ontology/core#hasResearchArea", $"<{resourceAPI.GraphsUrl}items/ResearchArea_{ResourceID}_{item1.ArticleID}>", list, " . ");
				if(item1.Vivo_start != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/ResearchArea_{ResourceID}_{item1.ArticleID}",  "http://vivoweb.org/ontology/core#start", $"\"{item1.Vivo_start.Value.ToString("yyyyMMddHHmmss")}\"", list, " . ");
				}
				if(item1.Vivo_end != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/ResearchArea_{ResourceID}_{item1.ArticleID}",  "http://vivoweb.org/ontology/core#end", $"\"{item1.Vivo_end.Value.ToString("yyyyMMddHHmmss")}\"", list, " . ");
				}
				if(item1.Roh_title != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/ResearchArea_{ResourceID}_{item1.ArticleID}",  "http://w3id.org/roh/title", $"\"{GenerarTextoSinSaltoDeLinea(item1.Roh_title)}\"", list, " . ");
				}
			}
			}
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
				if(item0.IdRoh_roleOf != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/BFO_0000023_{ResourceID}_{item0.ArticleID}",  "http://w3id.org/roh/roleOf", $"<{item0.IdRoh_roleOf}>", list, " . ");
				}
			}
			}
			if(this.Foaf_member != null)
			{
			foreach(var item0 in this.Foaf_member)
			{
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/BFO_0000023_{ResourceID}_{item0.ArticleID}", "http://www.w3.org/1999/02/22-rdf-syntax-ns#type", $"<http://purl.obolibrary.org/obo/BFO_0000023>", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/BFO_0000023_{ResourceID}_{item0.ArticleID}", "http://www.w3.org/2000/01/rdf-schema#label", $"\"http://purl.obolibrary.org/obo/BFO_0000023\"", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}{ResourceID}", "http://gnoss/hasEntidad", $"<{resourceAPI.GraphsUrl}items/BFO_0000023_{ResourceID}_{item0.ArticleID}>", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Group_{ResourceID}_{ArticleID}", "http://xmlns.com/foaf/0.1/member", $"<{resourceAPI.GraphsUrl}items/BFO_0000023_{ResourceID}_{item0.ArticleID}>", list, " . ");
			if(item0.Vivo_hasResearchArea != null)
			{
			foreach(var item1 in item0.Vivo_hasResearchArea)
			{
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/ResearchArea_{ResourceID}_{item1.ArticleID}", "http://www.w3.org/1999/02/22-rdf-syntax-ns#type", $"<http://w3id.org/roh/ResearchArea>", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/ResearchArea_{ResourceID}_{item1.ArticleID}", "http://www.w3.org/2000/01/rdf-schema#label", $"\"http://w3id.org/roh/ResearchArea\"", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}{ResourceID}", "http://gnoss/hasEntidad", $"<{resourceAPI.GraphsUrl}items/ResearchArea_{ResourceID}_{item1.ArticleID}>", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/BFO_0000023_{ResourceID}_{item0.ArticleID}", "http://vivoweb.org/ontology/core#hasResearchArea", $"<{resourceAPI.GraphsUrl}items/ResearchArea_{ResourceID}_{item1.ArticleID}>", list, " . ");
				if(item1.Vivo_start != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/ResearchArea_{ResourceID}_{item1.ArticleID}",  "http://vivoweb.org/ontology/core#start", $"\"{item1.Vivo_start.Value.ToString("yyyyMMddHHmmss")}\"", list, " . ");
				}
				if(item1.Vivo_end != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/ResearchArea_{ResourceID}_{item1.ArticleID}",  "http://vivoweb.org/ontology/core#end", $"\"{item1.Vivo_end.Value.ToString("yyyyMMddHHmmss")}\"", list, " . ");
				}
				if(item1.Roh_title != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/ResearchArea_{ResourceID}_{item1.ArticleID}",  "http://w3id.org/roh/title", $"\"{GenerarTextoSinSaltoDeLinea(item1.Roh_title)}\"", list, " . ");
				}
			}
			}
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
				if(item0.IdRoh_roleOf != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/BFO_0000023_{ResourceID}_{item0.ArticleID}",  "http://w3id.org/roh/roleOf", $"<{item0.IdRoh_roleOf}>", list, " . ");
				}
			}
			}
			if(this.Roh_hasKnowledgeArea != null)
			{
			foreach(var item0 in this.Roh_hasKnowledgeArea)
			{
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/CategoryPath_{ResourceID}_{item0.ArticleID}", "http://www.w3.org/1999/02/22-rdf-syntax-ns#type", $"<http://w3id.org/roh/CategoryPath>", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/CategoryPath_{ResourceID}_{item0.ArticleID}", "http://www.w3.org/2000/01/rdf-schema#label", $"\"http://w3id.org/roh/CategoryPath\"", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}{ResourceID}", "http://gnoss/hasEntidad", $"<{resourceAPI.GraphsUrl}items/CategoryPath_{ResourceID}_{item0.ArticleID}>", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Group_{ResourceID}_{ArticleID}", "http://w3id.org/roh/hasKnowledgeArea", $"<{resourceAPI.GraphsUrl}items/CategoryPath_{ResourceID}_{item0.ArticleID}>", list, " . ");
				if(item0.IdsRoh_categoryNode != null)
				{
					foreach(var item2 in item0.IdsRoh_categoryNode)
					{
						AgregarTripleALista($"{resourceAPI.GraphsUrl}items/CategoryPath_{ResourceID}_{item0.ArticleID}", "http://w3id.org/roh/categoryNode",  $"<{item2}>", list, " . ");
					}
				}
			}
			}
				if(this.IdVivo_affiliatedOrganization != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Group_{ResourceID}_{ArticleID}",  "http://vivoweb.org/ontology/core#affiliatedOrganization", $"<{this.IdVivo_affiliatedOrganization}>", list, " . ");
				}
				if(this.IdRoh_colaborationTypeGroup != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Group_{ResourceID}_{ArticleID}",  "http://w3id.org/roh/colaborationTypeGroup", $"<{this.IdRoh_colaborationTypeGroup}>", list, " . ");
				}
				if(this.Roh_relevantResults != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Group_{ResourceID}_{ArticleID}",  "http://w3id.org/roh/relevantResults", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_relevantResults)}\"", list, " . ");
				}
				if(this.Roh_lineResearch != null)
				{
					foreach(var item2 in this.Roh_lineResearch)
					{
						AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Group_{ResourceID}_{ArticleID}", "http://w3id.org/roh/lineResearch", $"\"{GenerarTextoSinSaltoDeLinea(item2)}\"", list, " . ");
					}
				}
				if(this.Roh_publicationsNumber != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Group_{ResourceID}_{ArticleID}",  "http://w3id.org/roh/publicationsNumber", $"{this.Roh_publicationsNumber.Value.ToString()}", list, " . ");
				}
				if(this.Roh_durationDays != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Group_{ResourceID}_{ArticleID}",  "http://w3id.org/roh/durationDays", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_durationDays)}\"", list, " . ");
				}
				if(this.Roh_normalizedCode != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Group_{ResourceID}_{ArticleID}",  "http://w3id.org/roh/normalizedCode", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_normalizedCode)}\"", list, " . ");
				}
				if(this.Vivo_description != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Group_{ResourceID}_{ArticleID}",  "http://vivoweb.org/ontology/core#description", $"\"{GenerarTextoSinSaltoDeLinea(this.Vivo_description)}\"", list, " . ");
				}
				if(this.Roh_themedAreasNumber != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Group_{ResourceID}_{ArticleID}",  "http://w3id.org/roh/themedAreasNumber", $"{this.Roh_themedAreasNumber.Value.ToString()}", list, " . ");
				}
				if(this.Roh_otherRelevantResults != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Group_{ResourceID}_{ArticleID}",  "http://w3id.org/roh/otherRelevantResults", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_otherRelevantResults)}\"", list, " . ");
				}
				if(this.Roh_durationMonths != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Group_{ResourceID}_{ArticleID}",  "http://w3id.org/roh/durationMonths", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_durationMonths)}\"", list, " . ");
				}
				if(this.Roh_directedPostdocsNumber != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Group_{ResourceID}_{ArticleID}",  "http://w3id.org/roh/directedPostdocsNumber", $"{this.Roh_directedPostdocsNumber.Value.ToString()}", list, " . ");
				}
				if(this.Roh_researchersNumber != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Group_{ResourceID}_{ArticleID}",  "http://w3id.org/roh/researchersNumber", $"{this.Roh_researchersNumber.Value.ToString()}", list, " . ");
				}
				if(this.Roh_durationYears != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Group_{ResourceID}_{ArticleID}",  "http://w3id.org/roh/durationYears", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_durationYears)}\"", list, " . ");
				}
				if(this.Roh_projectsNumber != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Group_{ResourceID}_{ArticleID}",  "http://w3id.org/roh/projectsNumber", $"{this.Roh_projectsNumber.Value.ToString()}", list, " . ");
				}
				if(this.Roh_crisIdentifier != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Group_{ResourceID}_{ArticleID}",  "http://w3id.org/roh/crisIdentifier", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_crisIdentifier)}\"", list, " . ");
				}
				if(this.Roh_foundationDate != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Group_{ResourceID}_{ArticleID}",  "http://w3id.org/roh/foundationDate", $"\"{this.Roh_foundationDate.Value.ToString("yyyyMMddHHmmss")}\"", list, " . ");
				}
				if(this.Roh_directedThesisNumber != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Group_{ResourceID}_{ArticleID}",  "http://w3id.org/roh/directedThesisNumber", $"{this.Roh_directedThesisNumber.Value.ToString()}", list, " . ");
				}
				if(this.Roh_collaboratorsNumber != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Group_{ResourceID}_{ArticleID}",  "http://w3id.org/roh/collaboratorsNumber", $"{this.Roh_collaboratorsNumber.Value.ToString()}", list, " . ");
				}
				if(this.Roh_isPublic != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Group_{ResourceID}_{ArticleID}",  "http://w3id.org/roh/isPublic", $"\"{this.Roh_isPublic.ToString()}\"", list, " . ");
				}
				if(this.Roh_title != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Group_{ResourceID}_{ArticleID}",  "http://w3id.org/roh/title", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_title)}\"", list, " . ");
				}
			return list;
		}

		public override List<string> ToSearchGraphTriples(ResourceApi resourceAPI)
		{
			List<string> list = new List<string>();
			List<string> listaSearch = new List<string>();
			AgregarTags(list);
			AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}", "http://www.w3.org/1999/02/22-rdf-syntax-ns#type", $"\"group\"", list, " . ");
			AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}", "http://gnoss/type", $"\"http://xmlns.com/foaf/0.1/Group\"", list, " . ");
			AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}", "http://gnoss/hasfechapublicacion", $"{DateTime.Now.ToString("yyyyMMddHHmmss")}", list, " . ");
			AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}", "http://gnoss/hastipodoc", "\"5\"", list, " . ");
			AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}", "http://gnoss/hasfechamodificacion", $"{DateTime.Now.ToString("yyyyMMddHHmmss")}", list, " . ");
			AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}", "http://gnoss/hasnumeroVisitas", "0", list, " . ");
			AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}", "http://gnoss/hasprivacidadCom", "\"publico\"", list, " . ");
			AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}", "http://xmlns.com/foaf/0.1/firstName", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_title)}\"", list, " . ");
			AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}", "http://gnoss/hasnombrecompleto", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_title)}\"", list, " . ");
			string search = string.Empty;
			if(this.Roh_hasResultsGroupClassification != null)
			{
			foreach(var item0 in this.Roh_hasResultsGroupClassification)
			{
				AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}", "http://w3id.org/roh/hasResultsGroupClassification", $"<{resourceAPI.GraphsUrl}items/groupclassification_{ResourceID}_{item0.ArticleID}>", list, " . ");
				if(item0.IdsRoh_groupClassificationNode != null)
				{
					foreach(var item2 in item0.IdsRoh_groupClassificationNode)
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
						AgregarTripleALista($"{resourceAPI.GraphsUrl}items/groupclassification_{ResourceID}_{item0.ArticleID}", "http://w3id.org/roh/groupClassificationNode", $"<{itemRegex}>", list, " . ");
					}
				}
			}
			}
			if(this.Roh_mainResearchers != null)
			{
			foreach(var item0 in this.Roh_mainResearchers)
			{
				AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}", "http://w3id.org/roh/mainResearchers", $"<{resourceAPI.GraphsUrl}items/bfo_0000023_{ResourceID}_{item0.ArticleID}>", list, " . ");
			if(item0.Vivo_hasResearchArea != null)
			{
			foreach(var item1 in item0.Vivo_hasResearchArea)
			{
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/bfo_0000023_{ResourceID}_{item0.ArticleID}", "http://vivoweb.org/ontology/core#hasResearchArea", $"<{resourceAPI.GraphsUrl}items/researcharea_{ResourceID}_{item1.ArticleID}>", list, " . ");
				if(item1.Vivo_start != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/researcharea_{ResourceID}_{item1.ArticleID}",  "http://vivoweb.org/ontology/core#start", $"{item1.Vivo_start.Value.ToString("yyyyMMddHHmmss")}", list, " . ");
				}
				if(item1.Vivo_end != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/researcharea_{ResourceID}_{item1.ArticleID}",  "http://vivoweb.org/ontology/core#end", $"{item1.Vivo_end.Value.ToString("yyyyMMddHHmmss")}", list, " . ");
				}
				if(item1.Roh_title != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/researcharea_{ResourceID}_{item1.ArticleID}",  "http://w3id.org/roh/title", $"\"{GenerarTextoSinSaltoDeLinea(item1.Roh_title).ToLower()}\"", list, " . ");
				}
			}
			}
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
				if(item0.IdRoh_roleOf != null)
				{
					Regex regex = new Regex(@"\/items\/.+_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}");
					string itemRegex = item0.IdRoh_roleOf;
					if (regex.IsMatch(itemRegex))
					{
						itemRegex = $"http://gnoss/{resourceAPI.GetShortGuid(itemRegex).ToString().ToUpper()}";
					}
					else
					{
						itemRegex = itemRegex.ToLower();
					}
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/bfo_0000023_{ResourceID}_{item0.ArticleID}",  "http://w3id.org/roh/roleOf", $"<{itemRegex}>", list, " . ");
				}
			}
			}
			if(this.Foaf_member != null)
			{
			foreach(var item0 in this.Foaf_member)
			{
				AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}", "http://xmlns.com/foaf/0.1/member", $"<{resourceAPI.GraphsUrl}items/bfo_0000023_{ResourceID}_{item0.ArticleID}>", list, " . ");
			if(item0.Vivo_hasResearchArea != null)
			{
			foreach(var item1 in item0.Vivo_hasResearchArea)
			{
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/bfo_0000023_{ResourceID}_{item0.ArticleID}", "http://vivoweb.org/ontology/core#hasResearchArea", $"<{resourceAPI.GraphsUrl}items/researcharea_{ResourceID}_{item1.ArticleID}>", list, " . ");
				if(item1.Vivo_start != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/researcharea_{ResourceID}_{item1.ArticleID}",  "http://vivoweb.org/ontology/core#start", $"{item1.Vivo_start.Value.ToString("yyyyMMddHHmmss")}", list, " . ");
				}
				if(item1.Vivo_end != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/researcharea_{ResourceID}_{item1.ArticleID}",  "http://vivoweb.org/ontology/core#end", $"{item1.Vivo_end.Value.ToString("yyyyMMddHHmmss")}", list, " . ");
				}
				if(item1.Roh_title != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/researcharea_{ResourceID}_{item1.ArticleID}",  "http://w3id.org/roh/title", $"\"{GenerarTextoSinSaltoDeLinea(item1.Roh_title).ToLower()}\"", list, " . ");
				}
			}
			}
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
				if(item0.IdRoh_roleOf != null)
				{
					Regex regex = new Regex(@"\/items\/.+_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}");
					string itemRegex = item0.IdRoh_roleOf;
					if (regex.IsMatch(itemRegex))
					{
						itemRegex = $"http://gnoss/{resourceAPI.GetShortGuid(itemRegex).ToString().ToUpper()}";
					}
					else
					{
						itemRegex = itemRegex.ToLower();
					}
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/bfo_0000023_{ResourceID}_{item0.ArticleID}",  "http://w3id.org/roh/roleOf", $"<{itemRegex}>", list, " . ");
				}
			}
			}
			if(this.Roh_hasKnowledgeArea != null)
			{
			foreach(var item0 in this.Roh_hasKnowledgeArea)
			{
				AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}", "http://w3id.org/roh/hasKnowledgeArea", $"<{resourceAPI.GraphsUrl}items/categorypath_{ResourceID}_{item0.ArticleID}>", list, " . ");
				if(item0.IdsRoh_categoryNode != null)
				{
					foreach(var item2 in item0.IdsRoh_categoryNode)
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
						AgregarTripleALista($"{resourceAPI.GraphsUrl}items/categorypath_{ResourceID}_{item0.ArticleID}", "http://w3id.org/roh/categoryNode",  $"<{itemRegex}>", list, " . ");
					}
				}
			}
			}
				if(this.IdVivo_affiliatedOrganization != null)
				{
					Regex regex = new Regex(@"\/items\/.+_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}");
					string itemRegex = this.IdVivo_affiliatedOrganization;
					if (regex.IsMatch(itemRegex))
					{
						itemRegex = $"http://gnoss/{resourceAPI.GetShortGuid(itemRegex).ToString().ToUpper()}";
					}
					else
					{
						itemRegex = itemRegex.ToLower();
					}
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "http://vivoweb.org/ontology/core#affiliatedOrganization", $"<{itemRegex}>", list, " . ");
				}
				if(this.IdRoh_colaborationTypeGroup != null)
				{
					Regex regex = new Regex(@"\/items\/.+_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}");
					string itemRegex = this.IdRoh_colaborationTypeGroup;
					if (regex.IsMatch(itemRegex))
					{
						itemRegex = $"http://gnoss/{resourceAPI.GetShortGuid(itemRegex).ToString().ToUpper()}";
					}
					else
					{
						itemRegex = itemRegex.ToLower();
					}
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "http://w3id.org/roh/colaborationTypeGroup", $"<{itemRegex}>", list, " . ");
				}
				if(this.Roh_relevantResults != null)
				{
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "http://w3id.org/roh/relevantResults", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_relevantResults).ToLower()}\"", list, " . ");
				}
				if(this.Roh_lineResearch != null)
				{
					foreach(var item2 in this.Roh_lineResearch)
					{
						AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}", "http://w3id.org/roh/lineResearch", $"\"{GenerarTextoSinSaltoDeLinea(item2).ToLower()}\"", list, " . ");
					}
				}
				if(this.Roh_publicationsNumber != null)
				{
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "http://w3id.org/roh/publicationsNumber", $"{this.Roh_publicationsNumber.Value.ToString()}", list, " . ");
				}
				if(this.Roh_durationDays != null)
				{
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "http://w3id.org/roh/durationDays", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_durationDays).ToLower()}\"", list, " . ");
				}
				if(this.Roh_normalizedCode != null)
				{
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "http://w3id.org/roh/normalizedCode", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_normalizedCode).ToLower()}\"", list, " . ");
				}
				if(this.Vivo_description != null)
				{
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "http://vivoweb.org/ontology/core#description", $"\"{GenerarTextoSinSaltoDeLinea(this.Vivo_description).ToLower()}\"", list, " . ");
				}
				if(this.Roh_themedAreasNumber != null)
				{
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "http://w3id.org/roh/themedAreasNumber", $"{this.Roh_themedAreasNumber.Value.ToString()}", list, " . ");
				}
				if(this.Roh_otherRelevantResults != null)
				{
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "http://w3id.org/roh/otherRelevantResults", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_otherRelevantResults).ToLower()}\"", list, " . ");
				}
				if(this.Roh_durationMonths != null)
				{
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "http://w3id.org/roh/durationMonths", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_durationMonths).ToLower()}\"", list, " . ");
				}
				if(this.Roh_directedPostdocsNumber != null)
				{
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "http://w3id.org/roh/directedPostdocsNumber", $"{this.Roh_directedPostdocsNumber.Value.ToString()}", list, " . ");
				}
				if(this.Roh_researchersNumber != null)
				{
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "http://w3id.org/roh/researchersNumber", $"{this.Roh_researchersNumber.Value.ToString()}", list, " . ");
				}
				if(this.Roh_durationYears != null)
				{
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "http://w3id.org/roh/durationYears", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_durationYears).ToLower()}\"", list, " . ");
				}
				if(this.Roh_projectsNumber != null)
				{
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "http://w3id.org/roh/projectsNumber", $"{this.Roh_projectsNumber.Value.ToString()}", list, " . ");
				}
				if(this.Roh_crisIdentifier != null)
				{
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "http://w3id.org/roh/crisIdentifier", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_crisIdentifier).ToLower()}\"", list, " . ");
				}
				if(this.Roh_foundationDate != null)
				{
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "http://w3id.org/roh/foundationDate", $"{this.Roh_foundationDate.Value.ToString("yyyyMMddHHmmss")}", list, " . ");
				}
				if(this.Roh_directedThesisNumber != null)
				{
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "http://w3id.org/roh/directedThesisNumber", $"{this.Roh_directedThesisNumber.Value.ToString()}", list, " . ");
				}
				if(this.Roh_collaboratorsNumber != null)
				{
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "http://w3id.org/roh/collaboratorsNumber", $"{this.Roh_collaboratorsNumber.Value.ToString()}", list, " . ");
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
			return $"{resourceAPI.GraphsUrl}items/GroupOntology_{ResourceID}_{ArticleID}";
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
