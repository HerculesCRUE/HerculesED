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
using Feature = FeatureOntology.Feature;
using GeographicRegion = GeographicregionOntology.GeographicRegion;
using OrganizationType = OrganizationtypeOntology.OrganizationType;

namespace CouncilOntology
{
	[ExcludeFromCodeCoverage]
	public class Council : GnossOCBase
	{

		public Council() : base() { } 

		public Council(SemanticResourceModel pSemCmsModel, LanguageEnum idiomaUsuario) : base()
		{
			this.mGNOSSID = pSemCmsModel.RootEntities[0].Entity.Uri;
			SemanticPropertyModel propVivo_affiliatedOrganization = pSemCmsModel.GetPropertyByPath("http://vivoweb.org/ontology/core#affiliatedOrganization");
			if(propVivo_affiliatedOrganization != null && propVivo_affiliatedOrganization.PropertyValues.Count > 0)
			{
				this.Vivo_affiliatedOrganization = new Organization(propVivo_affiliatedOrganization.PropertyValues[0].RelatedEntity,idiomaUsuario);
			}
			SemanticPropertyModel propRoh_affiliatedOrganizationHasCountryName = pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/affiliatedOrganizationHasCountryName");
			if(propRoh_affiliatedOrganizationHasCountryName != null && propRoh_affiliatedOrganizationHasCountryName.PropertyValues.Count > 0)
			{
				this.Roh_affiliatedOrganizationHasCountryName = new Feature(propRoh_affiliatedOrganizationHasCountryName.PropertyValues[0].RelatedEntity,idiomaUsuario);
			}
			SemanticPropertyModel propVcard_hasRegion = pSemCmsModel.GetPropertyByPath("https://www.w3.org/2006/vcard/ns#hasRegion");
			if(propVcard_hasRegion != null && propVcard_hasRegion.PropertyValues.Count > 0)
			{
				this.Vcard_hasRegion = new Feature(propVcard_hasRegion.PropertyValues[0].RelatedEntity,idiomaUsuario);
			}
			SemanticPropertyModel propVivo_geographicFocus = pSemCmsModel.GetPropertyByPath("http://vivoweb.org/ontology/core#geographicFocus");
			if(propVivo_geographicFocus != null && propVivo_geographicFocus.PropertyValues.Count > 0)
			{
				this.Vivo_geographicFocus = new GeographicRegion(propVivo_geographicFocus.PropertyValues[0].RelatedEntity,idiomaUsuario);
			}
			SemanticPropertyModel propVcard_hasCountryName = pSemCmsModel.GetPropertyByPath("https://www.w3.org/2006/vcard/ns#hasCountryName");
			if(propVcard_hasCountryName != null && propVcard_hasCountryName.PropertyValues.Count > 0)
			{
				this.Vcard_hasCountryName = new Feature(propVcard_hasCountryName.PropertyValues[0].RelatedEntity,idiomaUsuario);
			}
			SemanticPropertyModel propRoh_affiliatedOrganizationType = pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/affiliatedOrganizationType");
			if(propRoh_affiliatedOrganizationType != null && propRoh_affiliatedOrganizationType.PropertyValues.Count > 0)
			{
				this.Roh_affiliatedOrganizationType = new OrganizationType(propRoh_affiliatedOrganizationType.PropertyValues[0].RelatedEntity,idiomaUsuario);
			}
			SemanticPropertyModel propRoh_affiliatedOrganizationHasRegion = pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/affiliatedOrganizationHasRegion");
			if(propRoh_affiliatedOrganizationHasRegion != null && propRoh_affiliatedOrganizationHasRegion.PropertyValues.Count > 0)
			{
				this.Roh_affiliatedOrganizationHasRegion = new Feature(propRoh_affiliatedOrganizationHasRegion.PropertyValues[0].RelatedEntity,idiomaUsuario);
			}
			SemanticPropertyModel propRoh_professionalCategory = pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/professionalCategory");
			this.Roh_professionalCategory = new List<string>();
			if (propRoh_professionalCategory != null && propRoh_professionalCategory.PropertyValues.Count > 0)
			{
				foreach (SemanticPropertyModel.PropertyValue propValue in propRoh_professionalCategory.PropertyValues)
				{
					this.Roh_professionalCategory.Add(propValue.Value);
				}
			}
			this.Roh_durationMonths = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/durationMonths"));
			this.Roh_geographicFocusOther = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/geographicFocusOther"));
			this.Roh_durationDays = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/durationDays"));
			this.Vivo_start= GetDateValuePropertySemCms(pSemCmsModel.GetPropertyByPath("http://vivoweb.org/ontology/core#start"));
			this.Roh_affiliatedOrganizationTypeOther = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/affiliatedOrganizationTypeOther"));
			this.Vcard_locality = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("https://www.w3.org/2006/vcard/ns#locality"));
			this.Roh_performedTasks = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/performedTasks"));
			this.Roh_affiliatedOrganizationLocality = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/affiliatedOrganizationLocality"));
			this.Roh_durationYears = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/durationYears"));
			this.Roh_crisIdentifier = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/crisIdentifier"));
			this.Roh_members = GetNumberIntPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/members"));
			this.Roh_affiliatedOrganizationTitle = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/affiliatedOrganizationTitle"));
			this.Roh_title = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/title"));
		}

		public Council(SemanticEntityModel pSemCmsModel, LanguageEnum idiomaUsuario) : base()
		{
			this.mGNOSSID = pSemCmsModel.Entity.Uri;
			this.mURL = pSemCmsModel.Properties.FirstOrDefault(p => p.PropertyValues.Any(prop => prop.DownloadUrl != null))?.FirstPropertyValue.DownloadUrl;
			SemanticPropertyModel propVivo_affiliatedOrganization = pSemCmsModel.GetPropertyByPath("http://vivoweb.org/ontology/core#affiliatedOrganization");
			if(propVivo_affiliatedOrganization != null && propVivo_affiliatedOrganization.PropertyValues.Count > 0)
			{
				this.Vivo_affiliatedOrganization = new Organization(propVivo_affiliatedOrganization.PropertyValues[0].RelatedEntity,idiomaUsuario);
			}
			SemanticPropertyModel propRoh_affiliatedOrganizationHasCountryName = pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/affiliatedOrganizationHasCountryName");
			if(propRoh_affiliatedOrganizationHasCountryName != null && propRoh_affiliatedOrganizationHasCountryName.PropertyValues.Count > 0)
			{
				this.Roh_affiliatedOrganizationHasCountryName = new Feature(propRoh_affiliatedOrganizationHasCountryName.PropertyValues[0].RelatedEntity,idiomaUsuario);
			}
			SemanticPropertyModel propVcard_hasRegion = pSemCmsModel.GetPropertyByPath("https://www.w3.org/2006/vcard/ns#hasRegion");
			if(propVcard_hasRegion != null && propVcard_hasRegion.PropertyValues.Count > 0)
			{
				this.Vcard_hasRegion = new Feature(propVcard_hasRegion.PropertyValues[0].RelatedEntity,idiomaUsuario);
			}
			SemanticPropertyModel propVivo_geographicFocus = pSemCmsModel.GetPropertyByPath("http://vivoweb.org/ontology/core#geographicFocus");
			if(propVivo_geographicFocus != null && propVivo_geographicFocus.PropertyValues.Count > 0)
			{
				this.Vivo_geographicFocus = new GeographicRegion(propVivo_geographicFocus.PropertyValues[0].RelatedEntity,idiomaUsuario);
			}
			SemanticPropertyModel propVcard_hasCountryName = pSemCmsModel.GetPropertyByPath("https://www.w3.org/2006/vcard/ns#hasCountryName");
			if(propVcard_hasCountryName != null && propVcard_hasCountryName.PropertyValues.Count > 0)
			{
				this.Vcard_hasCountryName = new Feature(propVcard_hasCountryName.PropertyValues[0].RelatedEntity,idiomaUsuario);
			}
			SemanticPropertyModel propRoh_affiliatedOrganizationType = pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/affiliatedOrganizationType");
			if(propRoh_affiliatedOrganizationType != null && propRoh_affiliatedOrganizationType.PropertyValues.Count > 0)
			{
				this.Roh_affiliatedOrganizationType = new OrganizationType(propRoh_affiliatedOrganizationType.PropertyValues[0].RelatedEntity,idiomaUsuario);
			}
			SemanticPropertyModel propRoh_affiliatedOrganizationHasRegion = pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/affiliatedOrganizationHasRegion");
			if(propRoh_affiliatedOrganizationHasRegion != null && propRoh_affiliatedOrganizationHasRegion.PropertyValues.Count > 0)
			{
				this.Roh_affiliatedOrganizationHasRegion = new Feature(propRoh_affiliatedOrganizationHasRegion.PropertyValues[0].RelatedEntity,idiomaUsuario);
			}
			SemanticPropertyModel propRoh_professionalCategory = pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/professionalCategory");
			this.Roh_professionalCategory = new List<string>();
			if (propRoh_professionalCategory != null && propRoh_professionalCategory.PropertyValues.Count > 0)
			{
				foreach (SemanticPropertyModel.PropertyValue propValue in propRoh_professionalCategory.PropertyValues)
				{
					this.Roh_professionalCategory.Add(propValue.Value);
				}
			}
			this.Roh_durationMonths = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/durationMonths"));
			this.Roh_geographicFocusOther = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/geographicFocusOther"));
			this.Roh_durationDays = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/durationDays"));
			this.Vivo_start= GetDateValuePropertySemCms(pSemCmsModel.GetPropertyByPath("http://vivoweb.org/ontology/core#start"));
			this.Roh_affiliatedOrganizationTypeOther = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/affiliatedOrganizationTypeOther"));
			this.Vcard_locality = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("https://www.w3.org/2006/vcard/ns#locality"));
			this.Roh_performedTasks = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/performedTasks"));
			this.Roh_affiliatedOrganizationLocality = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/affiliatedOrganizationLocality"));
			this.Roh_durationYears = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/durationYears"));
			this.Roh_crisIdentifier = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/crisIdentifier"));
			this.Roh_members = GetNumberIntPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/members"));
			this.Roh_affiliatedOrganizationTitle = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/affiliatedOrganizationTitle"));
			this.Roh_title = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/title"));
		}

		public virtual string RdfType { get { return "http://w3id.org/roh/Council"; } }
		public virtual string RdfsLabel { get { return "http://w3id.org/roh/Council"; } }
		[LABEL(LanguageEnum.es,"affiliatedOrganization")]
		[RDFProperty("http://vivoweb.org/ontology/core#affiliatedOrganization")]
		public  Organization Vivo_affiliatedOrganization  { get; set;} 
		public string IdVivo_affiliatedOrganization  { get; set;} 

		[LABEL(LanguageEnum.es,"http://w3id.org/roh/affiliatedOrganizationHasCountryName")]
		[RDFProperty("http://w3id.org/roh/affiliatedOrganizationHasCountryName")]
		public  Feature Roh_affiliatedOrganizationHasCountryName  { get; set;} 
		public string IdRoh_affiliatedOrganizationHasCountryName  { get; set;} 

		[LABEL(LanguageEnum.es,"hasRegion")]
		[RDFProperty("https://www.w3.org/2006/vcard/ns#hasRegion")]
		public  Feature Vcard_hasRegion  { get; set;} 
		public string IdVcard_hasRegion  { get; set;} 

		[LABEL(LanguageEnum.es,"geographicFocus")]
		[RDFProperty("http://vivoweb.org/ontology/core#geographicFocus")]
		public  GeographicRegion Vivo_geographicFocus  { get; set;} 
		public string IdVivo_geographicFocus  { get; set;} 

		[LABEL(LanguageEnum.es,"hasCountryName")]
		[RDFProperty("https://www.w3.org/2006/vcard/ns#hasCountryName")]
		public  Feature Vcard_hasCountryName  { get; set;} 
		public string IdVcard_hasCountryName  { get; set;} 

		[LABEL(LanguageEnum.es,"http://w3id.org/roh/affiliatedOrganizationType")]
		[RDFProperty("http://w3id.org/roh/affiliatedOrganizationType")]
		public  OrganizationType Roh_affiliatedOrganizationType  { get; set;} 
		public string IdRoh_affiliatedOrganizationType  { get; set;} 

		[LABEL(LanguageEnum.es,"http://w3id.org/roh/affiliatedOrganizationHasRegion")]
		[RDFProperty("http://w3id.org/roh/affiliatedOrganizationHasRegion")]
		public  Feature Roh_affiliatedOrganizationHasRegion  { get; set;} 
		public string IdRoh_affiliatedOrganizationHasRegion  { get; set;} 

		[LABEL(LanguageEnum.es,"professionalCategory")]
		[RDFProperty("http://w3id.org/roh/professionalCategory")]
		public  List<string> Roh_professionalCategory { get; set;}

		[LABEL(LanguageEnum.es,"durationMonths")]
		[RDFProperty("http://w3id.org/roh/durationMonths")]
		public  string Roh_durationMonths { get; set;}

		[LABEL(LanguageEnum.es,"geographicFocusOther")]
		[RDFProperty("http://w3id.org/roh/geographicFocusOther")]
		public  string Roh_geographicFocusOther { get; set;}

		[LABEL(LanguageEnum.es,"durationDays")]
		[RDFProperty("http://w3id.org/roh/durationDays")]
		public  string Roh_durationDays { get; set;}

		[LABEL(LanguageEnum.es,"start")]
		[RDFProperty("http://vivoweb.org/ontology/core#start")]
		public  DateTime? Vivo_start { get; set;}

		[RDFProperty("http://w3id.org/roh/affiliatedOrganizationTypeOther")]
		public  string Roh_affiliatedOrganizationTypeOther { get; set;}

		[LABEL(LanguageEnum.es,"locality")]
		[RDFProperty("https://www.w3.org/2006/vcard/ns#locality")]
		public  string Vcard_locality { get; set;}

		[LABEL(LanguageEnum.es,"performedTasks")]
		[RDFProperty("http://w3id.org/roh/performedTasks")]
		public  string Roh_performedTasks { get; set;}

		[RDFProperty("http://w3id.org/roh/affiliatedOrganizationLocality")]
		public  string Roh_affiliatedOrganizationLocality { get; set;}

		[LABEL(LanguageEnum.es,"durationYears")]
		[RDFProperty("http://w3id.org/roh/durationYears")]
		public  string Roh_durationYears { get; set;}

		[LABEL(LanguageEnum.es,"crisIdentifier")]
		[RDFProperty("http://w3id.org/roh/crisIdentifier")]
		public  string Roh_crisIdentifier { get; set;}

		[LABEL(LanguageEnum.es,"hmembers")]
		[RDFProperty("http://w3id.org/roh/members")]
		public  int? Roh_members { get; set;}

		[RDFProperty("http://w3id.org/roh/affiliatedOrganizationTitle")]
		public  string Roh_affiliatedOrganizationTitle { get; set;}

		[LABEL(LanguageEnum.es,"title")]
		[RDFProperty("http://w3id.org/roh/title")]
		public  string Roh_title { get; set;}


		internal override void GetProperties()
		{
			base.GetProperties();
			propList.Add(new StringOntologyProperty("vivo:affiliatedOrganization", this.IdVivo_affiliatedOrganization));
			propList.Add(new StringOntologyProperty("roh:affiliatedOrganizationHasCountryName", this.IdRoh_affiliatedOrganizationHasCountryName));
			propList.Add(new StringOntologyProperty("vcard:hasRegion", this.IdVcard_hasRegion));
			propList.Add(new StringOntologyProperty("vivo:geographicFocus", this.IdVivo_geographicFocus));
			propList.Add(new StringOntologyProperty("vcard:hasCountryName", this.IdVcard_hasCountryName));
			propList.Add(new StringOntologyProperty("roh:affiliatedOrganizationType", this.IdRoh_affiliatedOrganizationType));
			propList.Add(new StringOntologyProperty("roh:affiliatedOrganizationHasRegion", this.IdRoh_affiliatedOrganizationHasRegion));
			propList.Add(new ListStringOntologyProperty("roh:professionalCategory", this.Roh_professionalCategory));
			propList.Add(new StringOntologyProperty("roh:durationMonths", this.Roh_durationMonths));
			propList.Add(new StringOntologyProperty("roh:geographicFocusOther", this.Roh_geographicFocusOther));
			propList.Add(new StringOntologyProperty("roh:durationDays", this.Roh_durationDays));
			if (this.Vivo_start.HasValue){
				propList.Add(new DateOntologyProperty("vivo:start", this.Vivo_start.Value));
				}
			propList.Add(new StringOntologyProperty("roh:affiliatedOrganizationTypeOther", this.Roh_affiliatedOrganizationTypeOther));
			propList.Add(new StringOntologyProperty("vcard:locality", this.Vcard_locality));
			propList.Add(new StringOntologyProperty("roh:performedTasks", this.Roh_performedTasks));
			propList.Add(new StringOntologyProperty("roh:affiliatedOrganizationLocality", this.Roh_affiliatedOrganizationLocality));
			propList.Add(new StringOntologyProperty("roh:durationYears", this.Roh_durationYears));
			propList.Add(new StringOntologyProperty("roh:crisIdentifier", this.Roh_crisIdentifier));
			propList.Add(new StringOntologyProperty("roh:members", this.Roh_members.ToString()));
			propList.Add(new StringOntologyProperty("roh:affiliatedOrganizationTitle", this.Roh_affiliatedOrganizationTitle));
			propList.Add(new StringOntologyProperty("roh:title", this.Roh_title));
		}

		internal override void GetEntities()
		{
			base.GetEntities();
		} 
		public virtual ComplexOntologyResource ToGnossApiResource(ResourceApi resourceAPI, List<string> listaDeCategorias)
		{
			return ToGnossApiResource(resourceAPI, listaDeCategorias, Guid.Empty, Guid.Empty);
		}

		public virtual ComplexOntologyResource ToGnossApiResource(ResourceApi resourceAPI, List<string> listaDeCategorias, Guid idrecurso, Guid idarticulo)
		{
			ComplexOntologyResource resource = new ComplexOntologyResource();
			Ontology ontology=null;
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
			AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Council_{ResourceID}_{ArticleID}", "http://www.w3.org/1999/02/22-rdf-syntax-ns#type", $"<http://w3id.org/roh/Council>", list, " . ");
			AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Council_{ResourceID}_{ArticleID}", "http://www.w3.org/2000/01/rdf-schema#label", $"\"http://w3id.org/roh/Council\"", list, " . ");
			AgregarTripleALista($"{resourceAPI.GraphsUrl}{ResourceID}", "http://gnoss/hasEntidad", $"<{resourceAPI.GraphsUrl}items/Council_{ResourceID}_{ArticleID}>", list, " . ");
				if(this.IdVivo_affiliatedOrganization != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Council_{ResourceID}_{ArticleID}",  "http://vivoweb.org/ontology/core#affiliatedOrganization", $"<{this.IdVivo_affiliatedOrganization}>", list, " . ");
				}
				if(this.IdRoh_affiliatedOrganizationHasCountryName != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Council_{ResourceID}_{ArticleID}",  "http://w3id.org/roh/affiliatedOrganizationHasCountryName", $"<{this.IdRoh_affiliatedOrganizationHasCountryName}>", list, " . ");
				}
				if(this.IdVcard_hasRegion != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Council_{ResourceID}_{ArticleID}",  "https://www.w3.org/2006/vcard/ns#hasRegion", $"<{this.IdVcard_hasRegion}>", list, " . ");
				}
				if(this.IdVivo_geographicFocus != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Council_{ResourceID}_{ArticleID}",  "http://vivoweb.org/ontology/core#geographicFocus", $"<{this.IdVivo_geographicFocus}>", list, " . ");
				}
				if(this.IdVcard_hasCountryName != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Council_{ResourceID}_{ArticleID}",  "https://www.w3.org/2006/vcard/ns#hasCountryName", $"<{this.IdVcard_hasCountryName}>", list, " . ");
				}
				if(this.IdRoh_affiliatedOrganizationType != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Council_{ResourceID}_{ArticleID}",  "http://w3id.org/roh/affiliatedOrganizationType", $"<{this.IdRoh_affiliatedOrganizationType}>", list, " . ");
				}
				if(this.IdRoh_affiliatedOrganizationHasRegion != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Council_{ResourceID}_{ArticleID}",  "http://w3id.org/roh/affiliatedOrganizationHasRegion", $"<{this.IdRoh_affiliatedOrganizationHasRegion}>", list, " . ");
				}
				if(this.Roh_professionalCategory != null)
				{
					foreach(var item2 in this.Roh_professionalCategory)
					{
						AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Council_{ResourceID}_{ArticleID}", "http://w3id.org/roh/professionalCategory", $"\"{GenerarTextoSinSaltoDeLinea(item2)}\"", list, " . ");
					}
				}
				if(this.Roh_durationMonths != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Council_{ResourceID}_{ArticleID}",  "http://w3id.org/roh/durationMonths", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_durationMonths)}\"", list, " . ");
				}
				if(this.Roh_geographicFocusOther != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Council_{ResourceID}_{ArticleID}",  "http://w3id.org/roh/geographicFocusOther", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_geographicFocusOther)}\"", list, " . ");
				}
				if(this.Roh_durationDays != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Council_{ResourceID}_{ArticleID}",  "http://w3id.org/roh/durationDays", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_durationDays)}\"", list, " . ");
				}
				if(this.Vivo_start != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Council_{ResourceID}_{ArticleID}",  "http://vivoweb.org/ontology/core#start", $"\"{this.Vivo_start.Value.ToString("yyyyMMddHHmmss")}\"", list, " . ");
				}
				if(this.Roh_affiliatedOrganizationTypeOther != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Council_{ResourceID}_{ArticleID}",  "http://w3id.org/roh/affiliatedOrganizationTypeOther", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_affiliatedOrganizationTypeOther)}\"", list, " . ");
				}
				if(this.Vcard_locality != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Council_{ResourceID}_{ArticleID}",  "https://www.w3.org/2006/vcard/ns#locality", $"\"{GenerarTextoSinSaltoDeLinea(this.Vcard_locality)}\"", list, " . ");
				}
				if(this.Roh_performedTasks != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Council_{ResourceID}_{ArticleID}",  "http://w3id.org/roh/performedTasks", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_performedTasks)}\"", list, " . ");
				}
				if(this.Roh_affiliatedOrganizationLocality != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Council_{ResourceID}_{ArticleID}",  "http://w3id.org/roh/affiliatedOrganizationLocality", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_affiliatedOrganizationLocality)}\"", list, " . ");
				}
				if(this.Roh_durationYears != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Council_{ResourceID}_{ArticleID}",  "http://w3id.org/roh/durationYears", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_durationYears)}\"", list, " . ");
				}
				if(this.Roh_crisIdentifier != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Council_{ResourceID}_{ArticleID}",  "http://w3id.org/roh/crisIdentifier", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_crisIdentifier)}\"", list, " . ");
				}
				if(this.Roh_members != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Council_{ResourceID}_{ArticleID}",  "http://w3id.org/roh/members", $"{this.Roh_members.Value.ToString()}", list, " . ");
				}
				if(this.Roh_affiliatedOrganizationTitle != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Council_{ResourceID}_{ArticleID}",  "http://w3id.org/roh/affiliatedOrganizationTitle", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_affiliatedOrganizationTitle)}\"", list, " . ");
				}
				if(this.Roh_title != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Council_{ResourceID}_{ArticleID}",  "http://w3id.org/roh/title", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_title)}\"", list, " . ");
				}
			return list;
		}

		public override List<string> ToSearchGraphTriples(ResourceApi resourceAPI)
		{
			List<string> list = new List<string>();
			List<string> listaSearch = new List<string>();
			AgregarTags(list);
			AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}", "http://www.w3.org/1999/02/22-rdf-syntax-ns#type", $"\"council\"", list, " . ");
			AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}", "http://gnoss/type", $"\"http://w3id.org/roh/Council\"", list, " . ");
			AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}", "http://gnoss/hasfechapublicacion", $"{DateTime.Now.ToString("yyyyMMddHHmmss")}", list, " . ");
			AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}", "http://gnoss/hastipodoc", "\"5\"", list, " . ");
			AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}", "http://gnoss/hasfechamodificacion", $"{DateTime.Now.ToString("yyyyMMddHHmmss")}", list, " . ");
			AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}", "http://gnoss/hasnumeroVisitas", "0", list, " . ");
			AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}", "http://gnoss/hasprivacidadCom", "\"publico\"", list, " . ");
			AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}", "http://xmlns.com/foaf/0.1/firstName", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_title)}\"", list, " . ");
			AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}", "http://gnoss/hasnombrecompleto", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_title)}\"", list, " . ");
			string search = string.Empty;
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
				if(this.IdRoh_affiliatedOrganizationHasCountryName != null)
				{
					Regex regex = new Regex(@"\/items\/.+_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}");
					string itemRegex = this.IdRoh_affiliatedOrganizationHasCountryName;
					if (regex.IsMatch(itemRegex))
					{
						itemRegex = $"http://gnoss/{resourceAPI.GetShortGuid(itemRegex).ToString().ToUpper()}";
					}
					else
					{
						itemRegex = itemRegex.ToLower();
					}
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "http://w3id.org/roh/affiliatedOrganizationHasCountryName", $"<{itemRegex}>", list, " . ");
				}
				if(this.IdVcard_hasRegion != null)
				{
					Regex regex = new Regex(@"\/items\/.+_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}");
					string itemRegex = this.IdVcard_hasRegion;
					if (regex.IsMatch(itemRegex))
					{
						itemRegex = $"http://gnoss/{resourceAPI.GetShortGuid(itemRegex).ToString().ToUpper()}";
					}
					else
					{
						itemRegex = itemRegex.ToLower();
					}
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "https://www.w3.org/2006/vcard/ns#hasRegion", $"<{itemRegex}>", list, " . ");
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
				if(this.IdVcard_hasCountryName != null)
				{
					Regex regex = new Regex(@"\/items\/.+_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}");
					string itemRegex = this.IdVcard_hasCountryName;
					if (regex.IsMatch(itemRegex))
					{
						itemRegex = $"http://gnoss/{resourceAPI.GetShortGuid(itemRegex).ToString().ToUpper()}";
					}
					else
					{
						itemRegex = itemRegex.ToLower();
					}
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "https://www.w3.org/2006/vcard/ns#hasCountryName", $"<{itemRegex}>", list, " . ");
				}
				if(this.IdRoh_affiliatedOrganizationType != null)
				{
					Regex regex = new Regex(@"\/items\/.+_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}");
					string itemRegex = this.IdRoh_affiliatedOrganizationType;
					if (regex.IsMatch(itemRegex))
					{
						itemRegex = $"http://gnoss/{resourceAPI.GetShortGuid(itemRegex).ToString().ToUpper()}";
					}
					else
					{
						itemRegex = itemRegex.ToLower();
					}
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "http://w3id.org/roh/affiliatedOrganizationType", $"<{itemRegex}>", list, " . ");
				}
				if(this.IdRoh_affiliatedOrganizationHasRegion != null)
				{
					Regex regex = new Regex(@"\/items\/.+_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}");
					string itemRegex = this.IdRoh_affiliatedOrganizationHasRegion;
					if (regex.IsMatch(itemRegex))
					{
						itemRegex = $"http://gnoss/{resourceAPI.GetShortGuid(itemRegex).ToString().ToUpper()}";
					}
					else
					{
						itemRegex = itemRegex.ToLower();
					}
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "http://w3id.org/roh/affiliatedOrganizationHasRegion", $"<{itemRegex}>", list, " . ");
				}
				if(this.Roh_professionalCategory != null)
				{
					foreach(var item2 in this.Roh_professionalCategory)
					{
						AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}", "http://w3id.org/roh/professionalCategory", $"\"{GenerarTextoSinSaltoDeLinea(item2).ToLower()}\"", list, " . ");
					}
				}
				if(this.Roh_durationMonths != null)
				{
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "http://w3id.org/roh/durationMonths", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_durationMonths).ToLower()}\"", list, " . ");
				}
				if(this.Roh_geographicFocusOther != null)
				{
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "http://w3id.org/roh/geographicFocusOther", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_geographicFocusOther).ToLower()}\"", list, " . ");
				}
				if(this.Roh_durationDays != null)
				{
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "http://w3id.org/roh/durationDays", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_durationDays).ToLower()}\"", list, " . ");
				}
				if(this.Vivo_start != null)
				{
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "http://vivoweb.org/ontology/core#start", $"{this.Vivo_start.Value.ToString("yyyyMMddHHmmss")}", list, " . ");
				}
				if(this.Roh_affiliatedOrganizationTypeOther != null)
				{
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "http://w3id.org/roh/affiliatedOrganizationTypeOther", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_affiliatedOrganizationTypeOther).ToLower()}\"", list, " . ");
				}
				if(this.Vcard_locality != null)
				{
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "https://www.w3.org/2006/vcard/ns#locality", $"\"{GenerarTextoSinSaltoDeLinea(this.Vcard_locality).ToLower()}\"", list, " . ");
				}
				if(this.Roh_performedTasks != null)
				{
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "http://w3id.org/roh/performedTasks", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_performedTasks).ToLower()}\"", list, " . ");
				}
				if(this.Roh_affiliatedOrganizationLocality != null)
				{
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "http://w3id.org/roh/affiliatedOrganizationLocality", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_affiliatedOrganizationLocality).ToLower()}\"", list, " . ");
				}
				if(this.Roh_durationYears != null)
				{
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "http://w3id.org/roh/durationYears", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_durationYears).ToLower()}\"", list, " . ");
				}
				if(this.Roh_crisIdentifier != null)
				{
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "http://w3id.org/roh/crisIdentifier", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_crisIdentifier).ToLower()}\"", list, " . ");
				}
				if(this.Roh_members != null)
				{
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "http://w3id.org/roh/members", $"{this.Roh_members.Value.ToString()}", list, " . ");
				}
				if(this.Roh_affiliatedOrganizationTitle != null)
				{
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "http://w3id.org/roh/affiliatedOrganizationTitle", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_affiliatedOrganizationTitle).ToLower()}\"", list, " . ");
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
				AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}", "http://gnoss/search", $"\"{GenerarTextoSinSaltoDeLinea(search.ToLower())}\"", list, " . ");
			}
			return list;
		}

		public override KeyValuePair<Guid, string> ToAcidData(ResourceApi resourceAPI)
		{

			//Insert en la tabla Documento
			string tags = "";
			foreach(string tag in tagList)
			{
				tags += $"{tag}, ";
			}
			if (!string.IsNullOrEmpty(tags))
			{
				tags = tags.Substring(0, tags.LastIndexOf(','));
			}
			string titulo = $"{this.Roh_title.Replace("\r\n", "").Replace("\n", "").Replace("\r", "").Replace("\"", "\"\"").Replace("'", "''").Replace("|", "#PIPE#")}";
			string descripcion = $"{this.Roh_title.Replace("\r\n", "").Replace("\n", "").Replace("\r", "").Replace("\"", "\"\"").Replace("'", "''").Replace("|", "#PIPE#")}";
			string tablaDoc = $"'{titulo}', '{descripcion}', '{resourceAPI.GraphsUrl}', '{tags}'";
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
			return $"{resourceAPI.GraphsUrl}items/CouncilOntology_{ResourceID}_{ArticleID}";
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
