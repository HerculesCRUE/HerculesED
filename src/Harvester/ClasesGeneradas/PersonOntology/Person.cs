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
using Organization = OrganizationOntology.Organization;
using Department = DepartmentOntology.Department;
using Concept = TaxonomyOntology.Concept;
using Group = GroupOntology.Group;

namespace PersonOntology
{
	public class Person : GnossOCBase
	{

		public Person() : base() { } 

		public Person(SemanticResourceModel pSemCmsModel, LanguageEnum idiomaUsuario) : base()
		{
			this.mGNOSSID = pSemCmsModel.RootEntities[0].Entity.Uri;
			SemanticPropertyModel propRoh_hasRole = pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/hasRole");
			if(propRoh_hasRole != null && propRoh_hasRole.PropertyValues.Count > 0)
			{
				this.Roh_hasRole = new Organization(propRoh_hasRole.PropertyValues[0].RelatedEntity,idiomaUsuario);
			}
			SemanticPropertyModel propVivo_departmentOrSchool = pSemCmsModel.GetPropertyByPath("http://vivoweb.org/ontology/core#departmentOrSchool");
			if(propVivo_departmentOrSchool != null && propVivo_departmentOrSchool.PropertyValues.Count > 0)
			{
				this.Vivo_departmentOrSchool = new Department(propVivo_departmentOrSchool.PropertyValues[0].RelatedEntity,idiomaUsuario);
			}
			this.Vivo_hasResearchArea = new List<Concept>();
			SemanticPropertyModel propVivo_hasResearchArea = pSemCmsModel.GetPropertyByPath("http://vivoweb.org/ontology/core#hasResearchArea");
			if(propVivo_hasResearchArea != null && propVivo_hasResearchArea.PropertyValues.Count > 0)
			{
				foreach (SemanticPropertyModel.PropertyValue propValue in propVivo_hasResearchArea.PropertyValues)
				{
					if(propValue.RelatedEntity!=null){
						Concept vivo_hasResearchArea = new Concept(propValue.RelatedEntity,idiomaUsuario);
						this.Vivo_hasResearchArea.Add(vivo_hasResearchArea);
					}
				}
			}
			this.Vivo_relates = new List<Group>();
			SemanticPropertyModel propVivo_relates = pSemCmsModel.GetPropertyByPath("http://vivoweb.org/ontology/core#relates");
			if(propVivo_relates != null && propVivo_relates.PropertyValues.Count > 0)
			{
				foreach (SemanticPropertyModel.PropertyValue propValue in propVivo_relates.PropertyValues)
				{
					if(propValue.RelatedEntity!=null){
						Group vivo_relates = new Group(propValue.RelatedEntity,idiomaUsuario);
						this.Vivo_relates.Add(vivo_relates);
					}
				}
			}
			this.Roh_publicationsNumber = GetNumberIntPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/publicationsNumber"));
			this.Vivo_scopusId = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://vivoweb.org/ontology/core#scopusId"));
			this.Roh_projectsNumber = GetNumberIntPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/projectsNumber"));
			this.Vcard_address = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("https://www.w3.org/2006/vcard/ns#address"));
			this.Foaf_homepage = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://xmlns.com/foaf/0.1/homepage"));
			this.Roh_hasPosition = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/hasPosition"));
			this.Vcard_email = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("https://www.w3.org/2006/vcard/ns#email"));
			this.Roh_crisIdentifier = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/crisIdentifier"));
			this.Vivo_description = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://vivoweb.org/ontology/core#description"));
			this.Roh_ORCID = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/ORCID"));
			SemanticPropertyModel propFoaf_nick = pSemCmsModel.GetPropertyByPath("http://xmlns.com/foaf/0.1/nick");
			this.Foaf_nick = new List<string>();
			if (propFoaf_nick != null && propFoaf_nick.PropertyValues.Count > 0)
			{
				foreach (SemanticPropertyModel.PropertyValue propValue in propFoaf_nick.PropertyValues)
				{
					this.Foaf_nick.Add(propValue.Value);
				}
			}
			this.Roh_h_index = GetNumberFloatPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/h-index"));
			this.Vcard_hasTelephone = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("https://www.w3.org/2006/vcard/ns#hasTelephone"));
			this.Roh_isSynchronized= GetBooleanPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/isSynchronized"));
			this.Foaf_firstName = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://xmlns.com/foaf/0.1/firstName"));
			this.Foaf_name = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://xmlns.com/foaf/0.1/name"));
			this.Foaf_lastName = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://xmlns.com/foaf/0.1/lastName"));
		}

		public Person(SemanticEntityModel pSemCmsModel, LanguageEnum idiomaUsuario) : base()
		{
			this.mGNOSSID = pSemCmsModel.Entity.Uri;
			this.mURL = pSemCmsModel.Properties.FirstOrDefault(p => p.PropertyValues.Any(prop => prop.DownloadUrl != null))?.FirstPropertyValue.DownloadUrl;
			SemanticPropertyModel propRoh_hasRole = pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/hasRole");
			if(propRoh_hasRole != null && propRoh_hasRole.PropertyValues.Count > 0)
			{
				this.Roh_hasRole = new Organization(propRoh_hasRole.PropertyValues[0].RelatedEntity,idiomaUsuario);
			}
			SemanticPropertyModel propVivo_departmentOrSchool = pSemCmsModel.GetPropertyByPath("http://vivoweb.org/ontology/core#departmentOrSchool");
			if(propVivo_departmentOrSchool != null && propVivo_departmentOrSchool.PropertyValues.Count > 0)
			{
				this.Vivo_departmentOrSchool = new Department(propVivo_departmentOrSchool.PropertyValues[0].RelatedEntity,idiomaUsuario);
			}
			this.Vivo_hasResearchArea = new List<Concept>();
			SemanticPropertyModel propVivo_hasResearchArea = pSemCmsModel.GetPropertyByPath("http://vivoweb.org/ontology/core#hasResearchArea");
			if(propVivo_hasResearchArea != null && propVivo_hasResearchArea.PropertyValues.Count > 0)
			{
				foreach (SemanticPropertyModel.PropertyValue propValue in propVivo_hasResearchArea.PropertyValues)
				{
					if(propValue.RelatedEntity!=null){
						Concept vivo_hasResearchArea = new Concept(propValue.RelatedEntity,idiomaUsuario);
						this.Vivo_hasResearchArea.Add(vivo_hasResearchArea);
					}
				}
			}
			this.Vivo_relates = new List<Group>();
			SemanticPropertyModel propVivo_relates = pSemCmsModel.GetPropertyByPath("http://vivoweb.org/ontology/core#relates");
			if(propVivo_relates != null && propVivo_relates.PropertyValues.Count > 0)
			{
				foreach (SemanticPropertyModel.PropertyValue propValue in propVivo_relates.PropertyValues)
				{
					if(propValue.RelatedEntity!=null){
						Group vivo_relates = new Group(propValue.RelatedEntity,idiomaUsuario);
						this.Vivo_relates.Add(vivo_relates);
					}
				}
			}
			this.Roh_publicationsNumber = GetNumberIntPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/publicationsNumber"));
			this.Vivo_scopusId = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://vivoweb.org/ontology/core#scopusId"));
			this.Roh_projectsNumber = GetNumberIntPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/projectsNumber"));
			this.Vcard_address = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("https://www.w3.org/2006/vcard/ns#address"));
			this.Foaf_homepage = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://xmlns.com/foaf/0.1/homepage"));
			this.Roh_hasPosition = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/hasPosition"));
			this.Vcard_email = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("https://www.w3.org/2006/vcard/ns#email"));
			this.Roh_crisIdentifier = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/crisIdentifier"));
			this.Vivo_description = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://vivoweb.org/ontology/core#description"));
			this.Roh_ORCID = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/ORCID"));
			SemanticPropertyModel propFoaf_nick = pSemCmsModel.GetPropertyByPath("http://xmlns.com/foaf/0.1/nick");
			this.Foaf_nick = new List<string>();
			if (propFoaf_nick != null && propFoaf_nick.PropertyValues.Count > 0)
			{
				foreach (SemanticPropertyModel.PropertyValue propValue in propFoaf_nick.PropertyValues)
				{
					this.Foaf_nick.Add(propValue.Value);
				}
			}
			this.Roh_h_index = GetNumberFloatPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/h-index"));
			this.Vcard_hasTelephone = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("https://www.w3.org/2006/vcard/ns#hasTelephone"));
			this.Roh_isSynchronized= GetBooleanPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/isSynchronized"));
			this.Foaf_firstName = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://xmlns.com/foaf/0.1/firstName"));
			this.Foaf_name = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://xmlns.com/foaf/0.1/name"));
			this.Foaf_lastName = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://xmlns.com/foaf/0.1/lastName"));
		}

		public virtual string RdfType { get { return "http://xmlns.com/foaf/0.1/Person"; } }
		public virtual string RdfsLabel { get { return "http://xmlns.com/foaf/0.1/Person"; } }
		[LABEL(LanguageEnum.es,"http://w3id.org/roh/hasRole")]
		[RDFProperty("http://w3id.org/roh/hasRole")]
		public  Organization Roh_hasRole  { get; set;} 
		public string IdRoh_hasRole  { get; set;} 

		[LABEL(LanguageEnum.es,"http://vivoweb.org/ontology/core#departmentOrSchool")]
		[RDFProperty("http://vivoweb.org/ontology/core#departmentOrSchool")]
		public  Department Vivo_departmentOrSchool  { get; set;} 
		public string IdVivo_departmentOrSchool  { get; set;} 

		[LABEL(LanguageEnum.es,"http://vivoweb.org/ontology/core#hasResearchArea")]
		[RDFProperty("http://vivoweb.org/ontology/core#hasResearchArea")]
		public  List<Concept> Vivo_hasResearchArea { get; set;}
		public List<string> IdsVivo_hasResearchArea { get; set;}

		[LABEL(LanguageEnum.es,"http://vivoweb.org/ontology/core#relates")]
		[RDFProperty("http://vivoweb.org/ontology/core#relates")]
		public  List<Group> Vivo_relates { get; set;}
		public List<string> IdsVivo_relates { get; set;}

		[LABEL(LanguageEnum.es,"http://w3id.org/roh/publicationsNumber")]
		[RDFProperty("http://w3id.org/roh/publicationsNumber")]
		public  int? Roh_publicationsNumber { get; set;}

		[LABEL(LanguageEnum.es,"http://vivoweb.org/ontology/core#scopusId")]
		[RDFProperty("http://vivoweb.org/ontology/core#scopusId")]
		public  string Vivo_scopusId { get; set;}

		[LABEL(LanguageEnum.es,"http://w3id.org/roh/projectsNumber")]
		[RDFProperty("http://w3id.org/roh/projectsNumber")]
		public  int? Roh_projectsNumber { get; set;}

		[LABEL(LanguageEnum.es,"https://www.w3.org/2006/vcard/ns#address")]
		[RDFProperty("https://www.w3.org/2006/vcard/ns#address")]
		public  string Vcard_address { get; set;}

		[LABEL(LanguageEnum.es,"http://xmlns.com/foaf/0.1/homepage")]
		[RDFProperty("http://xmlns.com/foaf/0.1/homepage")]
		public  string Foaf_homepage { get; set;}

		[LABEL(LanguageEnum.es,"http://w3id.org/roh/hasPosition")]
		[RDFProperty("http://w3id.org/roh/hasPosition")]
		public  string Roh_hasPosition { get; set;}

		[LABEL(LanguageEnum.es,"https://www.w3.org/2006/vcard/ns#email")]
		[RDFProperty("https://www.w3.org/2006/vcard/ns#email")]
		public  string Vcard_email { get; set;}

		[LABEL(LanguageEnum.es,"http://w3id.org/roh/crisIdentifier")]
		[RDFProperty("http://w3id.org/roh/crisIdentifier")]
		public  string Roh_crisIdentifier { get; set;}

		[LABEL(LanguageEnum.es,"http://vivoweb.org/ontology/core#description")]
		[RDFProperty("http://vivoweb.org/ontology/core#description")]
		public  string Vivo_description { get; set;}

		[LABEL(LanguageEnum.es,"http://w3id.org/roh/ORCID")]
		[RDFProperty("http://w3id.org/roh/ORCID")]
		public  string Roh_ORCID { get; set;}

		[LABEL(LanguageEnum.es,"http://xmlns.com/foaf/0.1/nick")]
		[RDFProperty("http://xmlns.com/foaf/0.1/nick")]
		public  List<string> Foaf_nick { get; set;}

		[LABEL(LanguageEnum.es,"http://w3id.org/roh/h-index")]
		[RDFProperty("http://w3id.org/roh/h-index")]
		public  float? Roh_h_index { get; set;}

		[LABEL(LanguageEnum.es,"https://www.w3.org/2006/vcard/ns#hasTelephone")]
		[RDFProperty("https://www.w3.org/2006/vcard/ns#hasTelephone")]
		public  string Vcard_hasTelephone { get; set;}

		[LABEL(LanguageEnum.es,"http://w3id.org/roh/isSynchronized")]
		[RDFProperty("http://w3id.org/roh/isSynchronized")]
		public  bool Roh_isSynchronized { get; set;}

		[LABEL(LanguageEnum.es,"http://xmlns.com/foaf/0.1/firstName")]
		[RDFProperty("http://xmlns.com/foaf/0.1/firstName")]
		public  string Foaf_firstName { get; set;}

		[LABEL(LanguageEnum.es,"http://xmlns.com/foaf/0.1/name")]
		[RDFProperty("http://xmlns.com/foaf/0.1/name")]
		public  string Foaf_name { get; set;}

		[LABEL(LanguageEnum.es,"http://xmlns.com/foaf/0.1/lastName")]
		[RDFProperty("http://xmlns.com/foaf/0.1/lastName")]
		public  string Foaf_lastName { get; set;}


		internal override void GetProperties()
		{
			base.GetProperties();
			propList.Add(new StringOntologyProperty("roh:hasRole", this.IdRoh_hasRole));
			propList.Add(new StringOntologyProperty("vivo:departmentOrSchool", this.IdVivo_departmentOrSchool));
			propList.Add(new ListStringOntologyProperty("vivo:hasResearchArea", this.IdsVivo_hasResearchArea));
			propList.Add(new ListStringOntologyProperty("vivo:relates", this.IdsVivo_relates));
			propList.Add(new StringOntologyProperty("roh:publicationsNumber", this.Roh_publicationsNumber.ToString()));
			propList.Add(new StringOntologyProperty("vivo:scopusId", this.Vivo_scopusId));
			propList.Add(new StringOntologyProperty("roh:projectsNumber", this.Roh_projectsNumber.ToString()));
			propList.Add(new StringOntologyProperty("vcard:address", this.Vcard_address));
			propList.Add(new StringOntologyProperty("foaf:homepage", this.Foaf_homepage));
			propList.Add(new StringOntologyProperty("roh:hasPosition", this.Roh_hasPosition));
			propList.Add(new StringOntologyProperty("vcard:email", this.Vcard_email));
			propList.Add(new StringOntologyProperty("roh:crisIdentifier", this.Roh_crisIdentifier));
			propList.Add(new StringOntologyProperty("vivo:description", this.Vivo_description));
			propList.Add(new StringOntologyProperty("roh:ORCID", this.Roh_ORCID));
			propList.Add(new ListStringOntologyProperty("foaf:nick", this.Foaf_nick));
			propList.Add(new StringOntologyProperty("roh:h-index", this.Roh_h_index.ToString()));
			propList.Add(new StringOntologyProperty("vcard:hasTelephone", this.Vcard_hasTelephone));
			propList.Add(new BoolOntologyProperty("roh:isSynchronized", this.Roh_isSynchronized));
			propList.Add(new StringOntologyProperty("foaf:firstName", this.Foaf_firstName));
			propList.Add(new StringOntologyProperty("foaf:name", this.Foaf_name));
			propList.Add(new StringOntologyProperty("foaf:lastName", this.Foaf_lastName));
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
			AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Person_{ResourceID}_{ArticleID}", "http://www.w3.org/1999/02/22-rdf-syntax-ns#type", $"<http://xmlns.com/foaf/0.1/Person>", list, " . ");
			AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Person_{ResourceID}_{ArticleID}", "http://www.w3.org/2000/01/rdf-schema#label", $"\"http://xmlns.com/foaf/0.1/Person\"", list, " . ");
			AgregarTripleALista($"{resourceAPI.GraphsUrl}{ResourceID}", "http://gnoss/hasEntidad", $"<{resourceAPI.GraphsUrl}items/Person_{ResourceID}_{ArticleID}>", list, " . ");
				if(this.IdRoh_hasRole != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Person_{ResourceID}_{ArticleID}",  "http://w3id.org/roh/hasRole", $"<{this.IdRoh_hasRole}>", list, " . ");
				}
				if(this.IdVivo_departmentOrSchool != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Person_{ResourceID}_{ArticleID}",  "http://vivoweb.org/ontology/core#departmentOrSchool", $"<{this.IdVivo_departmentOrSchool}>", list, " . ");
				}
				if(this.IdsVivo_hasResearchArea != null)
				{
					foreach(var item2 in this.IdsVivo_hasResearchArea)
					{
						AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Person_{ResourceID}_{ArticleID}", "http://vivoweb.org/ontology/core#hasResearchArea", $"<{item2}>", list, " . ");
					}
				}
				if(this.IdsVivo_relates != null)
				{
					foreach(var item2 in this.IdsVivo_relates)
					{
						AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Person_{ResourceID}_{ArticleID}", "http://vivoweb.org/ontology/core#relates", $"<{item2}>", list, " . ");
					}
				}
				if(this.Roh_publicationsNumber != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Person_{ResourceID}_{ArticleID}",  "http://w3id.org/roh/publicationsNumber", $"{this.Roh_publicationsNumber.Value.ToString()}", list, " . ");
				}
				if(this.Vivo_scopusId != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Person_{ResourceID}_{ArticleID}",  "http://vivoweb.org/ontology/core#scopusId", $"\"{GenerarTextoSinSaltoDeLinea(this.Vivo_scopusId)}\"", list, " . ");
				}
				if(this.Roh_projectsNumber != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Person_{ResourceID}_{ArticleID}",  "http://w3id.org/roh/projectsNumber", $"{this.Roh_projectsNumber.Value.ToString()}", list, " . ");
				}
				if(this.Vcard_address != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Person_{ResourceID}_{ArticleID}",  "https://www.w3.org/2006/vcard/ns#address", $"\"{GenerarTextoSinSaltoDeLinea(this.Vcard_address)}\"", list, " . ");
				}
				if(this.Foaf_homepage != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Person_{ResourceID}_{ArticleID}",  "http://xmlns.com/foaf/0.1/homepage", $"\"{GenerarTextoSinSaltoDeLinea(this.Foaf_homepage)}\"", list, " . ");
				}
				if(this.Roh_hasPosition != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Person_{ResourceID}_{ArticleID}",  "http://w3id.org/roh/hasPosition", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_hasPosition)}\"", list, " . ");
				}
				if(this.Vcard_email != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Person_{ResourceID}_{ArticleID}",  "https://www.w3.org/2006/vcard/ns#email", $"\"{GenerarTextoSinSaltoDeLinea(this.Vcard_email)}\"", list, " . ");
				}
				if(this.Roh_crisIdentifier != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Person_{ResourceID}_{ArticleID}",  "http://w3id.org/roh/crisIdentifier", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_crisIdentifier)}\"", list, " . ");
				}
				if(this.Vivo_description != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Person_{ResourceID}_{ArticleID}",  "http://vivoweb.org/ontology/core#description", $"\"{GenerarTextoSinSaltoDeLinea(this.Vivo_description)}\"", list, " . ");
				}
				if(this.Roh_ORCID != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Person_{ResourceID}_{ArticleID}",  "http://w3id.org/roh/ORCID", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_ORCID)}\"", list, " . ");
				}
				if(this.Foaf_nick != null)
				{
					foreach(var item2 in this.Foaf_nick)
					{
						AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Person_{ResourceID}_{ArticleID}", "http://xmlns.com/foaf/0.1/nick", $"\"{GenerarTextoSinSaltoDeLinea(item2)}\"", list, " . ");
					}
				}
				if(this.Roh_h_index != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Person_{ResourceID}_{ArticleID}",  "http://w3id.org/roh/h-index", $"{this.Roh_h_index.Value.ToString(new CultureInfo("en-US"))}", list, " . ");
				}
				if(this.Vcard_hasTelephone != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Person_{ResourceID}_{ArticleID}",  "https://www.w3.org/2006/vcard/ns#hasTelephone", $"\"{GenerarTextoSinSaltoDeLinea(this.Vcard_hasTelephone)}\"", list, " . ");
				}
				if(this.Roh_isSynchronized != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Person_{ResourceID}_{ArticleID}",  "http://w3id.org/roh/isSynchronized", $"\"{this.Roh_isSynchronized.ToString()}\"", list, " . ");
				}
				if(this.Foaf_firstName != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Person_{ResourceID}_{ArticleID}",  "http://xmlns.com/foaf/0.1/firstName", $"\"{GenerarTextoSinSaltoDeLinea(this.Foaf_firstName)}\"", list, " . ");
				}
				if(this.Foaf_name != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Person_{ResourceID}_{ArticleID}",  "http://xmlns.com/foaf/0.1/name", $"\"{GenerarTextoSinSaltoDeLinea(this.Foaf_name)}\"", list, " . ");
				}
				if(this.Foaf_lastName != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Person_{ResourceID}_{ArticleID}",  "http://xmlns.com/foaf/0.1/lastName", $"\"{GenerarTextoSinSaltoDeLinea(this.Foaf_lastName)}\"", list, " . ");
				}
			return list;
		}

		public override List<string> ToSearchGraphTriples(ResourceApi resourceAPI)
		{
			List<string> list = new List<string>();
			List<string> listaSearch = new List<string>();
			AgregarTags(list);
			AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}", "http://www.w3.org/1999/02/22-rdf-syntax-ns#type", $"\"person\"", list, " . ");
			AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}", "http://gnoss/type", $"\"http://xmlns.com/foaf/0.1/Person\"", list, " . ");
			AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}", "http://gnoss/hasfechapublicacion", $"{DateTime.Now.ToString("yyyyMMddHHmmss")}", list, " . ");
			AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}", "http://gnoss/hastipodoc", "\"5\"", list, " . ");
			AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}", "http://gnoss/hasfechamodificacion", $"{DateTime.Now.ToString("yyyyMMddHHmmss")}", list, " . ");
			AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}", "http://gnoss/hasnumeroVisitas", "0", list, " . ");
			AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}", "http://gnoss/hasprivacidadCom", "\"publico\"", list, " . ");
			AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}", "http://xmlns.com/foaf/0.1/firstName", $"\"{GenerarTextoSinSaltoDeLinea(this.Foaf_name)}\"", list, " . ");
			AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}", "http://gnoss/hasnombrecompleto", $"\"{GenerarTextoSinSaltoDeLinea(this.Foaf_name)}\"", list, " . ");
			string search = string.Empty;
				if(this.IdRoh_hasRole != null)
				{
					Regex regex = new Regex(@"\/items\/.+_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}");
					string itemRegex = this.IdRoh_hasRole;
					if (regex.IsMatch(itemRegex))
					{
						itemRegex = $"http://gnoss/{resourceAPI.GetShortGuid(itemRegex).ToString().ToUpper()}";
					}
					else
					{
						itemRegex = itemRegex.ToLower();
					}
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "http://w3id.org/roh/hasRole", $"<{itemRegex}>", list, " . ");
				}
				if(this.IdVivo_departmentOrSchool != null)
				{
					Regex regex = new Regex(@"\/items\/.+_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}");
					string itemRegex = this.IdVivo_departmentOrSchool;
					if (regex.IsMatch(itemRegex))
					{
						itemRegex = $"http://gnoss/{resourceAPI.GetShortGuid(itemRegex).ToString().ToUpper()}";
					}
					else
					{
						itemRegex = itemRegex.ToLower();
					}
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "http://vivoweb.org/ontology/core#departmentOrSchool", $"<{itemRegex}>", list, " . ");
				}
				if(this.IdsVivo_hasResearchArea != null)
				{
					foreach(var item2 in this.IdsVivo_hasResearchArea)
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
						AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}", "http://vivoweb.org/ontology/core#hasResearchArea", $"<{itemRegex}>", list, " . ");
					}
				}
				if(this.IdsVivo_relates != null)
				{
					foreach(var item2 in this.IdsVivo_relates)
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
						AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}", "http://vivoweb.org/ontology/core#relates", $"<{itemRegex}>", list, " . ");
					}
				}
				if(this.Roh_publicationsNumber != null)
				{
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "http://w3id.org/roh/publicationsNumber", $"{this.Roh_publicationsNumber.Value.ToString()}", list, " . ");
				}
				if(this.Vivo_scopusId != null)
				{
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "http://vivoweb.org/ontology/core#scopusId", $"\"{GenerarTextoSinSaltoDeLinea(this.Vivo_scopusId).ToLower()}\"", list, " . ");
				}
				if(this.Roh_projectsNumber != null)
				{
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "http://w3id.org/roh/projectsNumber", $"{this.Roh_projectsNumber.Value.ToString()}", list, " . ");
				}
				if(this.Vcard_address != null)
				{
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "https://www.w3.org/2006/vcard/ns#address", $"\"{GenerarTextoSinSaltoDeLinea(this.Vcard_address).ToLower()}\"", list, " . ");
				}
				if(this.Foaf_homepage != null)
				{
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "http://xmlns.com/foaf/0.1/homepage", $"\"{GenerarTextoSinSaltoDeLinea(this.Foaf_homepage).ToLower()}\"", list, " . ");
				}
				if(this.Roh_hasPosition != null)
				{
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "http://w3id.org/roh/hasPosition", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_hasPosition).ToLower()}\"", list, " . ");
				}
				if(this.Vcard_email != null)
				{
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "https://www.w3.org/2006/vcard/ns#email", $"\"{GenerarTextoSinSaltoDeLinea(this.Vcard_email).ToLower()}\"", list, " . ");
				}
				if(this.Roh_crisIdentifier != null)
				{
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "http://w3id.org/roh/crisIdentifier", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_crisIdentifier).ToLower()}\"", list, " . ");
				}
				if(this.Vivo_description != null)
				{
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "http://vivoweb.org/ontology/core#description", $"\"{GenerarTextoSinSaltoDeLinea(this.Vivo_description).ToLower()}\"", list, " . ");
				}
				if(this.Roh_ORCID != null)
				{
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "http://w3id.org/roh/ORCID", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_ORCID).ToLower()}\"", list, " . ");
				}
				if(this.Foaf_nick != null)
				{
					foreach(var item2 in this.Foaf_nick)
					{
						AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}", "http://xmlns.com/foaf/0.1/nick", $"\"{GenerarTextoSinSaltoDeLinea(item2).ToLower()}\"", list, " . ");
					}
				}
				if(this.Roh_h_index != null)
				{
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "http://w3id.org/roh/h-index", $"{this.Roh_h_index.Value.ToString(new CultureInfo("en-US"))}", list, " . ");
				}
				if(this.Vcard_hasTelephone != null)
				{
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "https://www.w3.org/2006/vcard/ns#hasTelephone", $"\"{GenerarTextoSinSaltoDeLinea(this.Vcard_hasTelephone).ToLower()}\"", list, " . ");
				}
				if(this.Roh_isSynchronized != null)
				{
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "http://w3id.org/roh/isSynchronized", $"\"{this.Roh_isSynchronized.ToString().ToLower()}\"", list, " . ");
				}
				if(this.Foaf_firstName != null)
				{
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "http://xmlns.com/foaf/0.1/firstName", $"\"{GenerarTextoSinSaltoDeLinea(this.Foaf_firstName).ToLower()}\"", list, " . ");
				}
				if(this.Foaf_name != null)
				{
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "http://xmlns.com/foaf/0.1/name", $"\"{GenerarTextoSinSaltoDeLinea(this.Foaf_name).ToLower()}\"", list, " . ");
				}
				if(this.Foaf_lastName != null)
				{
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "http://xmlns.com/foaf/0.1/lastName", $"\"{GenerarTextoSinSaltoDeLinea(this.Foaf_lastName).ToLower()}\"", list, " . ");
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
			string titulo = $"{this.Foaf_name.Replace("\r\n", "").Replace("\n", "").Replace("\r", "").Replace("\"", "\"\"").Replace("'", "''").Replace("|", "#PIPE#")}";
			string descripcion = $"{this.Foaf_name.Replace("\r\n", "").Replace("\n", "").Replace("\r", "").Replace("\"", "\"\"").Replace("'", "''").Replace("|", "#PIPE#")}";
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
			return $"{resourceAPI.GraphsUrl}items/PersonOntology_{ResourceID}_{ArticleID}";
		}

		private string GenerarTextoSinSaltoDeLinea(string pTexto)
		{
			return pTexto.Replace("\r\n", " ").Replace("\n", " ").Replace("\r", " ").Replace("\"", "\\\"");
		}

		internal void AddResourceTitle(ComplexOntologyResource resource)
		{
			resource.Title = this.Foaf_name;
		}

		internal void AddResourceDescription(ComplexOntologyResource resource)
		{
			resource.Description = this.Foaf_name;
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
