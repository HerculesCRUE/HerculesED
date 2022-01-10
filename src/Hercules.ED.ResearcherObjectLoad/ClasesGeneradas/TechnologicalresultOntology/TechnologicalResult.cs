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
using GeographicRegion = GeographicregionOntology.GeographicRegion;

namespace TechnologicalresultOntology
{
	[ExcludeFromCodeCoverage]
	public class TechnologicalResult : GnossOCBase
	{

		public TechnologicalResult() : base() { } 

		public TechnologicalResult(SemanticResourceModel pSemCmsModel, LanguageEnum idiomaUsuario) : base()
		{
			this.mGNOSSID = pSemCmsModel.RootEntities[0].Entity.Uri;
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
			SemanticPropertyModel propVivo_geographicFocus = pSemCmsModel.GetPropertyByPath("http://vivoweb.org/ontology/core#geographicFocus");
			if(propVivo_geographicFocus != null && propVivo_geographicFocus.PropertyValues.Count > 0)
			{
				this.Vivo_geographicFocus = new GeographicRegion(propVivo_geographicFocus.PropertyValues[0].RelatedEntity,idiomaUsuario);
			}
			this.Roh_targetOrganizations = new List<Organization>();
			SemanticPropertyModel propRoh_targetOrganizations = pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/targetOrganizations");
			if(propRoh_targetOrganizations != null && propRoh_targetOrganizations.PropertyValues.Count > 0)
			{
				foreach (SemanticPropertyModel.PropertyValue propValue in propRoh_targetOrganizations.PropertyValues)
				{
					if(propValue.RelatedEntity!=null){
						Organization roh_targetOrganizations = new Organization(propValue.RelatedEntity,idiomaUsuario);
						this.Roh_targetOrganizations.Add(roh_targetOrganizations);
					}
				}
			}
			SemanticPropertyModel propRoh_principalInvestigator = pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/principalInvestigator");
			if(propRoh_principalInvestigator != null && propRoh_principalInvestigator.PropertyValues.Count > 0)
			{
				this.Roh_principalInvestigator = new BFO_0000023(propRoh_principalInvestigator.PropertyValues[0].RelatedEntity,idiomaUsuario);
			}
			SemanticPropertyModel propRoh_coprincipalInvestigator = pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/coprincipalInvestigator");
			if(propRoh_coprincipalInvestigator != null && propRoh_coprincipalInvestigator.PropertyValues.Count > 0)
			{
				this.Roh_coprincipalInvestigator = new BFO_0000023(propRoh_coprincipalInvestigator.PropertyValues[0].RelatedEntity,idiomaUsuario);
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
			this.Roh_newTechniques= GetBooleanPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/newTechniques"));
			this.Roh_spinoffCompanies= GetBooleanPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/spinoffCompanies"));
			this.Roh_activityResults= GetBooleanPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/activityResults"));
			this.Roh_standardisation= GetBooleanPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/standardisation"));
			this.Roh_durationMonths = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/durationMonths"));
			this.Roh_durationDays = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/durationDays"));
			this.Vivo_start= GetDateValuePropertySemCms(pSemCmsModel.GetPropertyByPath("http://vivoweb.org/ontology/core#start"));
			this.Roh_collaborationAgreements= GetBooleanPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/collaborationAgreements"));
			SemanticPropertyModel propVivo_freeTextKeywords = pSemCmsModel.GetPropertyByPath("http://vivoweb.org/ontology/core#freeTextKeywords");
			this.Vivo_freeTextKeywords = new List<string>();
			if (propVivo_freeTextKeywords != null && propVivo_freeTextKeywords.PropertyValues.Count > 0)
			{
				foreach (SemanticPropertyModel.PropertyValue propValue in propVivo_freeTextKeywords.PropertyValues)
				{
					this.Vivo_freeTextKeywords.Add(propValue.Value);
				}
			}
			this.Roh_durationYears = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/durationYears"));
			this.Roh_crisIdentifier = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/crisIdentifier"));
			this.Vivo_description = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://vivoweb.org/ontology/core#description"));
		}

		public TechnologicalResult(SemanticEntityModel pSemCmsModel, LanguageEnum idiomaUsuario) : base()
		{
			this.mGNOSSID = pSemCmsModel.Entity.Uri;
			this.mURL = pSemCmsModel.Properties.FirstOrDefault(p => p.PropertyValues.Any(prop => prop.DownloadUrl != null))?.FirstPropertyValue.DownloadUrl;
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
			SemanticPropertyModel propVivo_geographicFocus = pSemCmsModel.GetPropertyByPath("http://vivoweb.org/ontology/core#geographicFocus");
			if(propVivo_geographicFocus != null && propVivo_geographicFocus.PropertyValues.Count > 0)
			{
				this.Vivo_geographicFocus = new GeographicRegion(propVivo_geographicFocus.PropertyValues[0].RelatedEntity,idiomaUsuario);
			}
			this.Roh_targetOrganizations = new List<Organization>();
			SemanticPropertyModel propRoh_targetOrganizations = pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/targetOrganizations");
			if(propRoh_targetOrganizations != null && propRoh_targetOrganizations.PropertyValues.Count > 0)
			{
				foreach (SemanticPropertyModel.PropertyValue propValue in propRoh_targetOrganizations.PropertyValues)
				{
					if(propValue.RelatedEntity!=null){
						Organization roh_targetOrganizations = new Organization(propValue.RelatedEntity,idiomaUsuario);
						this.Roh_targetOrganizations.Add(roh_targetOrganizations);
					}
				}
			}
			SemanticPropertyModel propRoh_principalInvestigator = pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/principalInvestigator");
			if(propRoh_principalInvestigator != null && propRoh_principalInvestigator.PropertyValues.Count > 0)
			{
				this.Roh_principalInvestigator = new BFO_0000023(propRoh_principalInvestigator.PropertyValues[0].RelatedEntity,idiomaUsuario);
			}
			SemanticPropertyModel propRoh_coprincipalInvestigator = pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/coprincipalInvestigator");
			if(propRoh_coprincipalInvestigator != null && propRoh_coprincipalInvestigator.PropertyValues.Count > 0)
			{
				this.Roh_coprincipalInvestigator = new BFO_0000023(propRoh_coprincipalInvestigator.PropertyValues[0].RelatedEntity,idiomaUsuario);
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
			this.Roh_newTechniques= GetBooleanPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/newTechniques"));
			this.Roh_spinoffCompanies= GetBooleanPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/spinoffCompanies"));
			this.Roh_activityResults= GetBooleanPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/activityResults"));
			this.Roh_standardisation= GetBooleanPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/standardisation"));
			this.Roh_durationMonths = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/durationMonths"));
			this.Roh_durationDays = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/durationDays"));
			this.Vivo_start= GetDateValuePropertySemCms(pSemCmsModel.GetPropertyByPath("http://vivoweb.org/ontology/core#start"));
			this.Roh_collaborationAgreements= GetBooleanPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/collaborationAgreements"));
			SemanticPropertyModel propVivo_freeTextKeywords = pSemCmsModel.GetPropertyByPath("http://vivoweb.org/ontology/core#freeTextKeywords");
			this.Vivo_freeTextKeywords = new List<string>();
			if (propVivo_freeTextKeywords != null && propVivo_freeTextKeywords.PropertyValues.Count > 0)
			{
				foreach (SemanticPropertyModel.PropertyValue propValue in propVivo_freeTextKeywords.PropertyValues)
				{
					this.Vivo_freeTextKeywords.Add(propValue.Value);
				}
			}
			this.Roh_durationYears = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/durationYears"));
			this.Roh_crisIdentifier = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/crisIdentifier"));
			this.Vivo_description = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://vivoweb.org/ontology/core#description"));
		}

		public virtual string RdfType { get { return "http://w3id.org/roh/TechnologicalResult"; } }
		public virtual string RdfsLabel { get { return "http://w3id.org/roh/TechnologicalResult"; } }
		[LABEL(LanguageEnum.es,"http://w3id.org/roh/participates")]
		[RDFProperty("http://w3id.org/roh/participates")]
		public  List<Organization> Roh_participates { get; set;}
		public List<string> IdsRoh_participates { get; set;}

		[LABEL(LanguageEnum.es,"http://vivoweb.org/ontology/core#geographicFocus")]
		[RDFProperty("http://vivoweb.org/ontology/core#geographicFocus")]
		public  GeographicRegion Vivo_geographicFocus  { get; set;} 
		public string IdVivo_geographicFocus  { get; set;} 

		[LABEL(LanguageEnum.es,"http://w3id.org/roh/targetOrganizations")]
		[RDFProperty("http://w3id.org/roh/targetOrganizations")]
		public  List<Organization> Roh_targetOrganizations { get; set;}
		public List<string> IdsRoh_targetOrganizations { get; set;}

		[LABEL(LanguageEnum.es,"http://w3id.org/roh/principalInvestigator")]
		[RDFProperty("http://w3id.org/roh/principalInvestigator")]
		public  BFO_0000023 Roh_principalInvestigator { get; set;}

		[LABEL(LanguageEnum.es,"http://w3id.org/roh/coprincipalInvestigator")]
		[RDFProperty("http://w3id.org/roh/coprincipalInvestigator")]
		public  BFO_0000023 Roh_coprincipalInvestigator { get; set;}

		[LABEL(LanguageEnum.es,"http://w3id.org/roh/hasKnowledgeArea")]
		[RDFProperty("http://w3id.org/roh/hasKnowledgeArea")]
		public  List<CategoryPath> Roh_hasKnowledgeArea { get; set;}

		[LABEL(LanguageEnum.es,"http://w3id.org/roh/relevantResults")]
		[RDFProperty("http://w3id.org/roh/relevantResults")]
		public  string Roh_relevantResults { get; set;}

		[LABEL(LanguageEnum.es,"http://w3id.org/roh/newTechniques")]
		[RDFProperty("http://w3id.org/roh/newTechniques")]
		public  bool Roh_newTechniques { get; set;}

		[LABEL(LanguageEnum.es,"http://w3id.org/roh/spinoffCompanies")]
		[RDFProperty("http://w3id.org/roh/spinoffCompanies")]
		public  bool Roh_spinoffCompanies { get; set;}

		[LABEL(LanguageEnum.es,"http://w3id.org/roh/activityResults")]
		[RDFProperty("http://w3id.org/roh/activityResults")]
		public  bool Roh_activityResults { get; set;}

		[LABEL(LanguageEnum.es,"http://w3id.org/roh/standardisation")]
		[RDFProperty("http://w3id.org/roh/standardisation")]
		public  bool Roh_standardisation { get; set;}

		[LABEL(LanguageEnum.es,"http://w3id.org/roh/durationMonths")]
		[RDFProperty("http://w3id.org/roh/durationMonths")]
		public  string Roh_durationMonths { get; set;}

		[LABEL(LanguageEnum.es,"http://w3id.org/roh/durationDays")]
		[RDFProperty("http://w3id.org/roh/durationDays")]
		public  string Roh_durationDays { get; set;}

		[LABEL(LanguageEnum.es,"http://vivoweb.org/ontology/core#start")]
		[RDFProperty("http://vivoweb.org/ontology/core#start")]
		public  DateTime? Vivo_start { get; set;}

		[LABEL(LanguageEnum.es,"http://w3id.org/roh/collaborationAgreements")]
		[RDFProperty("http://w3id.org/roh/collaborationAgreements")]
		public  bool Roh_collaborationAgreements { get; set;}

		[LABEL(LanguageEnum.es,"http://vivoweb.org/ontology/core#freeTextKeywords")]
		[RDFProperty("http://vivoweb.org/ontology/core#freeTextKeywords")]
		public  List<string> Vivo_freeTextKeywords { get; set;}

		[LABEL(LanguageEnum.es,"http://w3id.org/roh/durationYears")]
		[RDFProperty("http://w3id.org/roh/durationYears")]
		public  string Roh_durationYears { get; set;}

		[LABEL(LanguageEnum.es,"http://w3id.org/roh/crisIdentifier")]
		[RDFProperty("http://w3id.org/roh/crisIdentifier")]
		public  string Roh_crisIdentifier { get; set;}

		[LABEL(LanguageEnum.es,"http://vivoweb.org/ontology/core#description")]
		[RDFProperty("http://vivoweb.org/ontology/core#description")]
		public  string Vivo_description { get; set;}


		internal override void GetProperties()
		{
			base.GetProperties();
			propList.Add(new ListStringOntologyProperty("roh:participates", this.IdsRoh_participates));
			propList.Add(new StringOntologyProperty("vivo:geographicFocus", this.IdVivo_geographicFocus));
			propList.Add(new ListStringOntologyProperty("roh:targetOrganizations", this.IdsRoh_targetOrganizations));
			propList.Add(new StringOntologyProperty("roh:relevantResults", this.Roh_relevantResults));
			propList.Add(new BoolOntologyProperty("roh:newTechniques", this.Roh_newTechniques));
			propList.Add(new BoolOntologyProperty("roh:spinoffCompanies", this.Roh_spinoffCompanies));
			propList.Add(new BoolOntologyProperty("roh:activityResults", this.Roh_activityResults));
			propList.Add(new BoolOntologyProperty("roh:standardisation", this.Roh_standardisation));
			propList.Add(new StringOntologyProperty("roh:durationMonths", this.Roh_durationMonths));
			propList.Add(new StringOntologyProperty("roh:durationDays", this.Roh_durationDays));
			if (this.Vivo_start.HasValue){
				propList.Add(new DateOntologyProperty("vivo:start", this.Vivo_start.Value));
				}
			propList.Add(new BoolOntologyProperty("roh:collaborationAgreements", this.Roh_collaborationAgreements));
			propList.Add(new ListStringOntologyProperty("vivo:freeTextKeywords", this.Vivo_freeTextKeywords));
			propList.Add(new StringOntologyProperty("roh:durationYears", this.Roh_durationYears));
			propList.Add(new StringOntologyProperty("roh:crisIdentifier", this.Roh_crisIdentifier));
			propList.Add(new StringOntologyProperty("vivo:description", this.Vivo_description));
		}

		internal override void GetEntities()
		{
			base.GetEntities();
			if(Roh_principalInvestigator!=null){
				Roh_principalInvestigator.GetProperties();
				Roh_principalInvestigator.GetEntities();
				OntologyEntity entityRoh_principalInvestigator = new OntologyEntity("http://purl.obolibrary.org/obo/BFO_0000023", "http://purl.obolibrary.org/obo/BFO_0000023", "roh:principalInvestigator", Roh_principalInvestigator.propList, Roh_principalInvestigator.entList);
				entList.Add(entityRoh_principalInvestigator);
			}
			if(Roh_coprincipalInvestigator!=null){
				Roh_coprincipalInvestigator.GetProperties();
				Roh_coprincipalInvestigator.GetEntities();
				OntologyEntity entityRoh_coprincipalInvestigator = new OntologyEntity("http://purl.obolibrary.org/obo/BFO_0000023", "http://purl.obolibrary.org/obo/BFO_0000023", "roh:coprincipalInvestigator", Roh_coprincipalInvestigator.propList, Roh_coprincipalInvestigator.entList);
				entList.Add(entityRoh_coprincipalInvestigator);
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
			AgregarTripleALista($"{resourceAPI.GraphsUrl}items/TechnologicalResult_{ResourceID}_{ArticleID}", "http://www.w3.org/1999/02/22-rdf-syntax-ns#type", $"<http://w3id.org/roh/TechnologicalResult>", list, " . ");
			AgregarTripleALista($"{resourceAPI.GraphsUrl}items/TechnologicalResult_{ResourceID}_{ArticleID}", "http://www.w3.org/2000/01/rdf-schema#label", $"\"http://w3id.org/roh/TechnologicalResult\"", list, " . ");
			AgregarTripleALista($"{resourceAPI.GraphsUrl}{ResourceID}", "http://gnoss/hasEntidad", $"<{resourceAPI.GraphsUrl}items/TechnologicalResult_{ResourceID}_{ArticleID}>", list, " . ");
			if(this.Roh_principalInvestigator != null)
			{
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/BFO_0000023_{ResourceID}_{this.Roh_principalInvestigator.ArticleID}", "http://www.w3.org/1999/02/22-rdf-syntax-ns#type", $"<http://purl.obolibrary.org/obo/BFO_0000023>", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/BFO_0000023_{ResourceID}_{this.Roh_principalInvestigator.ArticleID}", "http://www.w3.org/2000/01/rdf-schema#label", $"\"http://purl.obolibrary.org/obo/BFO_0000023\"", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}{ResourceID}", "http://gnoss/hasEntidad", $"<{resourceAPI.GraphsUrl}items/BFO_0000023_{ResourceID}_{this.Roh_principalInvestigator.ArticleID}>", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/TechnologicalResult_{ResourceID}_{ArticleID}", "http://w3id.org/roh/principalInvestigator", $"<{resourceAPI.GraphsUrl}items/BFO_0000023_{ResourceID}_{this.Roh_principalInvestigator.ArticleID}>", list, " . ");
				if(this.Roh_principalInvestigator.Foaf_nick != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/BFO_0000023_{ResourceID}_{this.Roh_principalInvestigator.ArticleID}",  "http://xmlns.com/foaf/0.1/nick", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_principalInvestigator.Foaf_nick)}\"", list, " . ");
				}
				if(this.Roh_principalInvestigator.IdRoh_roleOf != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/BFO_0000023_{ResourceID}_{this.Roh_principalInvestigator.ArticleID}",  "http://w3id.org/roh/roleOf", $"<{this.Roh_principalInvestigator.IdRoh_roleOf}>", list, " . ");
				}
			}
			if(this.Roh_coprincipalInvestigator != null)
			{
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/BFO_0000023_{ResourceID}_{this.Roh_coprincipalInvestigator.ArticleID}", "http://www.w3.org/1999/02/22-rdf-syntax-ns#type", $"<http://purl.obolibrary.org/obo/BFO_0000023>", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/BFO_0000023_{ResourceID}_{this.Roh_coprincipalInvestigator.ArticleID}", "http://www.w3.org/2000/01/rdf-schema#label", $"\"http://purl.obolibrary.org/obo/BFO_0000023\"", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}{ResourceID}", "http://gnoss/hasEntidad", $"<{resourceAPI.GraphsUrl}items/BFO_0000023_{ResourceID}_{this.Roh_coprincipalInvestigator.ArticleID}>", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/TechnologicalResult_{ResourceID}_{ArticleID}", "http://w3id.org/roh/coprincipalInvestigator", $"<{resourceAPI.GraphsUrl}items/BFO_0000023_{ResourceID}_{this.Roh_coprincipalInvestigator.ArticleID}>", list, " . ");
				if(this.Roh_coprincipalInvestigator.Foaf_nick != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/BFO_0000023_{ResourceID}_{this.Roh_coprincipalInvestigator.ArticleID}",  "http://xmlns.com/foaf/0.1/nick", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_coprincipalInvestigator.Foaf_nick)}\"", list, " . ");
				}
				if(this.Roh_coprincipalInvestigator.IdRoh_roleOf != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/BFO_0000023_{ResourceID}_{this.Roh_coprincipalInvestigator.ArticleID}",  "http://w3id.org/roh/roleOf", $"<{this.Roh_coprincipalInvestigator.IdRoh_roleOf}>", list, " . ");
				}
			}
			if(this.Roh_hasKnowledgeArea != null)
			{
			foreach(var item0 in this.Roh_hasKnowledgeArea)
			{
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/CategoryPath_{ResourceID}_{item0.ArticleID}", "http://www.w3.org/1999/02/22-rdf-syntax-ns#type", $"<http://w3id.org/roh/CategoryPath>", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/CategoryPath_{ResourceID}_{item0.ArticleID}", "http://www.w3.org/2000/01/rdf-schema#label", $"\"http://w3id.org/roh/CategoryPath\"", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}{ResourceID}", "http://gnoss/hasEntidad", $"<{resourceAPI.GraphsUrl}items/CategoryPath_{ResourceID}_{item0.ArticleID}>", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/TechnologicalResult_{ResourceID}_{ArticleID}", "http://w3id.org/roh/hasKnowledgeArea", $"<{resourceAPI.GraphsUrl}items/CategoryPath_{ResourceID}_{item0.ArticleID}>", list, " . ");
				if(item0.IdsRoh_categoryNode != null)
				{
					foreach(var item2 in item0.IdsRoh_categoryNode)
					{
						AgregarTripleALista($"{resourceAPI.GraphsUrl}items/CategoryPath_{ResourceID}_{item0.ArticleID}", "http://w3id.org/roh/categoryNode",  $"<{item2}>", list, " . ");
					}
				}
			}
			}
				if(this.IdsRoh_participates != null)
				{
					foreach(var item2 in this.IdsRoh_participates)
					{
						AgregarTripleALista($"{resourceAPI.GraphsUrl}items/TechnologicalResult_{ResourceID}_{ArticleID}", "http://w3id.org/roh/participates", $"<{item2}>", list, " . ");
					}
				}
				if(this.IdVivo_geographicFocus != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/TechnologicalResult_{ResourceID}_{ArticleID}",  "http://vivoweb.org/ontology/core#geographicFocus", $"<{this.IdVivo_geographicFocus}>", list, " . ");
				}
				if(this.IdsRoh_targetOrganizations != null)
				{
					foreach(var item2 in this.IdsRoh_targetOrganizations)
					{
						AgregarTripleALista($"{resourceAPI.GraphsUrl}items/TechnologicalResult_{ResourceID}_{ArticleID}", "http://w3id.org/roh/targetOrganizations", $"<{item2}>", list, " . ");
					}
				}
				if(this.Roh_relevantResults != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/TechnologicalResult_{ResourceID}_{ArticleID}",  "http://w3id.org/roh/relevantResults", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_relevantResults)}\"", list, " . ");
				}
				if(this.Roh_newTechniques != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/TechnologicalResult_{ResourceID}_{ArticleID}",  "http://w3id.org/roh/newTechniques", $"\"{this.Roh_newTechniques.ToString()}\"", list, " . ");
				}
				if(this.Roh_spinoffCompanies != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/TechnologicalResult_{ResourceID}_{ArticleID}",  "http://w3id.org/roh/spinoffCompanies", $"\"{this.Roh_spinoffCompanies.ToString()}\"", list, " . ");
				}
				if(this.Roh_activityResults != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/TechnologicalResult_{ResourceID}_{ArticleID}",  "http://w3id.org/roh/activityResults", $"\"{this.Roh_activityResults.ToString()}\"", list, " . ");
				}
				if(this.Roh_standardisation != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/TechnologicalResult_{ResourceID}_{ArticleID}",  "http://w3id.org/roh/standardisation", $"\"{this.Roh_standardisation.ToString()}\"", list, " . ");
				}
				if(this.Roh_durationMonths != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/TechnologicalResult_{ResourceID}_{ArticleID}",  "http://w3id.org/roh/durationMonths", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_durationMonths)}\"", list, " . ");
				}
				if(this.Roh_durationDays != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/TechnologicalResult_{ResourceID}_{ArticleID}",  "http://w3id.org/roh/durationDays", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_durationDays)}\"", list, " . ");
				}
				if(this.Vivo_start != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/TechnologicalResult_{ResourceID}_{ArticleID}",  "http://vivoweb.org/ontology/core#start", $"\"{this.Vivo_start.Value.ToString("yyyyMMddHHmmss")}\"", list, " . ");
				}
				if(this.Roh_collaborationAgreements != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/TechnologicalResult_{ResourceID}_{ArticleID}",  "http://w3id.org/roh/collaborationAgreements", $"\"{this.Roh_collaborationAgreements.ToString()}\"", list, " . ");
				}
				if(this.Vivo_freeTextKeywords != null)
				{
					foreach(var item2 in this.Vivo_freeTextKeywords)
					{
						AgregarTripleALista($"{resourceAPI.GraphsUrl}items/TechnologicalResult_{ResourceID}_{ArticleID}", "http://vivoweb.org/ontology/core#freeTextKeywords", $"\"{GenerarTextoSinSaltoDeLinea(item2)}\"", list, " . ");
					}
				}
				if(this.Roh_durationYears != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/TechnologicalResult_{ResourceID}_{ArticleID}",  "http://w3id.org/roh/durationYears", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_durationYears)}\"", list, " . ");
				}
				if(this.Roh_crisIdentifier != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/TechnologicalResult_{ResourceID}_{ArticleID}",  "http://w3id.org/roh/crisIdentifier", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_crisIdentifier)}\"", list, " . ");
				}
				if(this.Vivo_description != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/TechnologicalResult_{ResourceID}_{ArticleID}",  "http://vivoweb.org/ontology/core#description", $"\"{GenerarTextoSinSaltoDeLinea(this.Vivo_description)}\"", list, " . ");
				}
			return list;
		}

		public override List<string> ToSearchGraphTriples(ResourceApi resourceAPI)
		{
			List<string> list = new List<string>();
			List<string> listaSearch = new List<string>();
			AgregarTags(list);
			AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}", "http://www.w3.org/1999/02/22-rdf-syntax-ns#type", $"\"technologicalresult\"", list, " . ");
			AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}", "http://gnoss/type", $"\"http://w3id.org/roh/TechnologicalResult\"", list, " . ");
			AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}", "http://gnoss/hasfechapublicacion", $"{DateTime.Now.ToString("yyyyMMddHHmmss")}", list, " . ");
			AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}", "http://gnoss/hastipodoc", "\"5\"", list, " . ");
			AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}", "http://gnoss/hasfechamodificacion", $"{DateTime.Now.ToString("yyyyMMddHHmmss")}", list, " . ");
			AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}", "http://gnoss/hasnumeroVisitas", "0", list, " . ");
			AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}", "http://gnoss/hasprivacidadCom", "\"publico\"", list, " . ");
			AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}", "http://xmlns.com/foaf/0.1/firstName", $"\"{GenerarTextoSinSaltoDeLinea(this.Vivo_description)}\"", list, " . ");
			AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}", "http://gnoss/hasnombrecompleto", $"\"{GenerarTextoSinSaltoDeLinea(this.Vivo_description)}\"", list, " . ");
			string search = string.Empty;
			if(this.Roh_principalInvestigator != null)
			{
				AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}", "http://w3id.org/roh/principalInvestigator", $"<{resourceAPI.GraphsUrl}items/bfo_0000023_{ResourceID}_{this.Roh_principalInvestigator.ArticleID}>", list, " . ");
				if(this.Roh_principalInvestigator.Foaf_nick != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/bfo_0000023_{ResourceID}_{this.Roh_principalInvestigator.ArticleID}",  "http://xmlns.com/foaf/0.1/nick", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_principalInvestigator.Foaf_nick).ToLower()}\"", list, " . ");
				}
				if(this.Roh_principalInvestigator.IdRoh_roleOf != null)
				{
					Regex regex = new Regex(@"\/items\/.+_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}");
					string itemRegex = this.Roh_principalInvestigator.IdRoh_roleOf;
					if (regex.IsMatch(itemRegex))
					{
						itemRegex = $"http://gnoss/{resourceAPI.GetShortGuid(itemRegex).ToString().ToUpper()}";
					}
					else
					{
						itemRegex = itemRegex.ToLower();
					}
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/bfo_0000023_{ResourceID}_{this.Roh_principalInvestigator.ArticleID}",  "http://w3id.org/roh/roleOf", $"<{itemRegex}>", list, " . ");
				}
			}
			if(this.Roh_coprincipalInvestigator != null)
			{
				AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}", "http://w3id.org/roh/coprincipalInvestigator", $"<{resourceAPI.GraphsUrl}items/bfo_0000023_{ResourceID}_{this.Roh_coprincipalInvestigator.ArticleID}>", list, " . ");
				if(this.Roh_coprincipalInvestigator.Foaf_nick != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/bfo_0000023_{ResourceID}_{this.Roh_coprincipalInvestigator.ArticleID}",  "http://xmlns.com/foaf/0.1/nick", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_coprincipalInvestigator.Foaf_nick).ToLower()}\"", list, " . ");
				}
				if(this.Roh_coprincipalInvestigator.IdRoh_roleOf != null)
				{
					Regex regex = new Regex(@"\/items\/.+_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}");
					string itemRegex = this.Roh_coprincipalInvestigator.IdRoh_roleOf;
					if (regex.IsMatch(itemRegex))
					{
						itemRegex = $"http://gnoss/{resourceAPI.GetShortGuid(itemRegex).ToString().ToUpper()}";
					}
					else
					{
						itemRegex = itemRegex.ToLower();
					}
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/bfo_0000023_{ResourceID}_{this.Roh_coprincipalInvestigator.ArticleID}",  "http://w3id.org/roh/roleOf", $"<{itemRegex}>", list, " . ");
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
				if(this.IdsRoh_targetOrganizations != null)
				{
					foreach(var item2 in this.IdsRoh_targetOrganizations)
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
						AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}", "http://w3id.org/roh/targetOrganizations", $"<{itemRegex}>", list, " . ");
					}
				}
				if(this.Roh_relevantResults != null)
				{
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "http://w3id.org/roh/relevantResults", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_relevantResults).ToLower()}\"", list, " . ");
				}
				if(this.Roh_newTechniques != null)
				{
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "http://w3id.org/roh/newTechniques", $"\"{this.Roh_newTechniques.ToString().ToLower()}\"", list, " . ");
				}
				if(this.Roh_spinoffCompanies != null)
				{
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "http://w3id.org/roh/spinoffCompanies", $"\"{this.Roh_spinoffCompanies.ToString().ToLower()}\"", list, " . ");
				}
				if(this.Roh_activityResults != null)
				{
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "http://w3id.org/roh/activityResults", $"\"{this.Roh_activityResults.ToString().ToLower()}\"", list, " . ");
				}
				if(this.Roh_standardisation != null)
				{
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "http://w3id.org/roh/standardisation", $"\"{this.Roh_standardisation.ToString().ToLower()}\"", list, " . ");
				}
				if(this.Roh_durationMonths != null)
				{
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "http://w3id.org/roh/durationMonths", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_durationMonths).ToLower()}\"", list, " . ");
				}
				if(this.Roh_durationDays != null)
				{
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "http://w3id.org/roh/durationDays", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_durationDays).ToLower()}\"", list, " . ");
				}
				if(this.Vivo_start != null)
				{
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "http://vivoweb.org/ontology/core#start", $"{this.Vivo_start.Value.ToString("yyyyMMddHHmmss")}", list, " . ");
				}
				if(this.Roh_collaborationAgreements != null)
				{
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "http://w3id.org/roh/collaborationAgreements", $"\"{this.Roh_collaborationAgreements.ToString().ToLower()}\"", list, " . ");
				}
				if(this.Vivo_freeTextKeywords != null)
				{
					foreach(var item2 in this.Vivo_freeTextKeywords)
					{
						AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}", "http://vivoweb.org/ontology/core#freeTextKeywords", $"\"{GenerarTextoSinSaltoDeLinea(item2).ToLower()}\"", list, " . ");
					}
				}
				if(this.Roh_durationYears != null)
				{
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "http://w3id.org/roh/durationYears", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_durationYears).ToLower()}\"", list, " . ");
				}
				if(this.Roh_crisIdentifier != null)
				{
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "http://w3id.org/roh/crisIdentifier", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_crisIdentifier).ToLower()}\"", list, " . ");
				}
				if(this.Vivo_description != null)
				{
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "http://vivoweb.org/ontology/core#description", $"\"{GenerarTextoSinSaltoDeLinea(this.Vivo_description).ToLower()}\"", list, " . ");
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
			string titulo = $"{this.Vivo_description.Replace("\r\n", "").Replace("\n", "").Replace("\r", "").Replace("\"", "\"\"").Replace("'", "''").Replace("|", "#PIPE#")}";
			string descripcion = $"{this.Vivo_description.Replace("\r\n", "").Replace("\n", "").Replace("\r", "").Replace("\"", "\"\"").Replace("'", "''").Replace("|", "#PIPE#")}";
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
			return $"{resourceAPI.GraphsUrl}items/TechnologicalresultOntology_{ResourceID}_{ArticleID}";
		}

		private string GenerarTextoSinSaltoDeLinea(string pTexto)
		{
			return pTexto.Replace("\r\n", " ").Replace("\n", " ").Replace("\r", " ").Replace("\"", "\\\"");
		}

		internal void AddResourceTitle(ComplexOntologyResource resource)
		{
			resource.Title = this.Vivo_description;
		}

		internal void AddResourceDescription(ComplexOntologyResource resource)
		{
			resource.Description = this.Vivo_description;
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
