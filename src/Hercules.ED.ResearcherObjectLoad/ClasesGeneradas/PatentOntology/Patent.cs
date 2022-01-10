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
using IndustrialPropertyType = IndustrialpropertytypeOntology.IndustrialPropertyType;

namespace PatentOntology
{
	[ExcludeFromCodeCoverage]
	public class Patent : GnossOCBase
	{

		public Patent() : base() { } 

		public Patent(SemanticResourceModel pSemCmsModel, LanguageEnum idiomaUsuario) : base()
		{
			this.mGNOSSID = pSemCmsModel.RootEntities[0].Entity.Uri;
			SemanticPropertyModel propRoh_ownerOrganization = pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/ownerOrganization");
			if(propRoh_ownerOrganization != null && propRoh_ownerOrganization.PropertyValues.Count > 0)
			{
				this.Roh_ownerOrganization = new Organization(propRoh_ownerOrganization.PropertyValues[0].RelatedEntity,idiomaUsuario);
			}
			SemanticPropertyModel propVcard_hasAddress = pSemCmsModel.GetPropertyByPath("https://www.w3.org/2006/vcard/ns#hasAddress");
			if(propVcard_hasAddress != null && propVcard_hasAddress.PropertyValues.Count > 0)
			{
				this.Vcard_hasAddress = new Address(propVcard_hasAddress.PropertyValues[0].RelatedEntity,idiomaUsuario);
			}
			this.Roh_operatingCompanies = new List<Organization>();
			SemanticPropertyModel propRoh_operatingCompanies = pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/operatingCompanies");
			if(propRoh_operatingCompanies != null && propRoh_operatingCompanies.PropertyValues.Count > 0)
			{
				foreach (SemanticPropertyModel.PropertyValue propValue in propRoh_operatingCompanies.PropertyValues)
				{
					if(propValue.RelatedEntity!=null){
						Organization roh_operatingCompanies = new Organization(propValue.RelatedEntity,idiomaUsuario);
						this.Roh_operatingCompanies.Add(roh_operatingCompanies);
					}
				}
			}
			SemanticPropertyModel propRoh_industrialPropertyType = pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/industrialPropertyType");
			if(propRoh_industrialPropertyType != null && propRoh_industrialPropertyType.PropertyValues.Count > 0)
			{
				this.Roh_industrialPropertyType = new IndustrialPropertyType(propRoh_industrialPropertyType.PropertyValues[0].RelatedEntity,idiomaUsuario);
			}
			this.Roh_operatingCountries = new List<Address>();
			SemanticPropertyModel propRoh_operatingCountries = pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/operatingCountries");
			if(propRoh_operatingCountries != null && propRoh_operatingCountries.PropertyValues.Count > 0)
			{
				foreach (SemanticPropertyModel.PropertyValue propValue in propRoh_operatingCountries.PropertyValues)
				{
					if(propValue.RelatedEntity!=null){
						Address roh_operatingCountries = new Address(propValue.RelatedEntity,idiomaUsuario);
						this.Roh_operatingCountries.Add(roh_operatingCountries);
					}
				}
			}
			this.Roh_patentInventor = new List<BFO_0000023>();
			SemanticPropertyModel propRoh_patentInventor = pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/patentInventor");
			if(propRoh_patentInventor != null && propRoh_patentInventor.PropertyValues.Count > 0)
			{
				foreach (SemanticPropertyModel.PropertyValue propValue in propRoh_patentInventor.PropertyValues)
				{
					if(propValue.RelatedEntity!=null){
						BFO_0000023 roh_patentInventor = new BFO_0000023(propValue.RelatedEntity,idiomaUsuario);
						this.Roh_patentInventor.Add(roh_patentInventor);
					}
				}
			}
			this.Roh_spanishPatent= GetBooleanPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/spanishPatent"));
			this.Roh_relevantResults = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/relevantResults"));
			this.Roh_applicationNumber = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/applicationNumber"));
			this.Roh_knowHow= GetBooleanPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/knowHow"));
			this.Roh_results= GetBooleanPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/results"));
			this.Roh_patentNumber = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/patentNumber"));
			this.Roh_industrialPropertyTypeOther = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/industrialPropertyTypeOther"));
			this.Roh_title = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/title"));
			this.Roh_exclusiveUse= GetBooleanPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/exclusiveUse"));
			this.Roh_relatedRights= GetBooleanPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/relatedRights"));
			SemanticPropertyModel propRoh_products = pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/products");
			this.Roh_products = new List<string>();
			if (propRoh_products != null && propRoh_products.PropertyValues.Count > 0)
			{
				foreach (SemanticPropertyModel.PropertyValue propValue in propRoh_products.PropertyValues)
				{
					this.Roh_products.Add(propValue.Value);
				}
			}
			this.Roh_referenceCode = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/referenceCode"));
			this.Vivo_dateFiled= GetDateValuePropertySemCms(pSemCmsModel.GetPropertyByPath("http://vivoweb.org/ontology/core#dateFiled"));
			this.Roh_europeanPatent= GetBooleanPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/europeanPatent"));
			this.Roh_authorsRights= GetBooleanPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/authorsRights"));
			this.Vivo_dateIssued= GetDateValuePropertySemCms(pSemCmsModel.GetPropertyByPath("http://vivoweb.org/ontology/core#dateIssued"));
			this.Roh_tradeSecret= GetBooleanPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/tradeSecret"));
			this.Roh_qualityDescription = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/qualityDescription"));
			this.Roh_innovativeEnterprise= GetBooleanPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/innovativeEnterprise"));
			this.Vivo_freeTextKeywords = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://vivoweb.org/ontology/core#freeTextKeywords"));
			this.Roh_internationalPatent= GetBooleanPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/internationalPatent"));
			this.Roh_crisIdentifier = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/crisIdentifier"));
			this.Roh_licenses= GetBooleanPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/licenses"));
			this.Roh_pctPatent= GetBooleanPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/pctPatent"));
		}

		public Patent(SemanticEntityModel pSemCmsModel, LanguageEnum idiomaUsuario) : base()
		{
			this.mGNOSSID = pSemCmsModel.Entity.Uri;
			this.mURL = pSemCmsModel.Properties.FirstOrDefault(p => p.PropertyValues.Any(prop => prop.DownloadUrl != null))?.FirstPropertyValue.DownloadUrl;
			SemanticPropertyModel propRoh_ownerOrganization = pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/ownerOrganization");
			if(propRoh_ownerOrganization != null && propRoh_ownerOrganization.PropertyValues.Count > 0)
			{
				this.Roh_ownerOrganization = new Organization(propRoh_ownerOrganization.PropertyValues[0].RelatedEntity,idiomaUsuario);
			}
			SemanticPropertyModel propVcard_hasAddress = pSemCmsModel.GetPropertyByPath("https://www.w3.org/2006/vcard/ns#hasAddress");
			if(propVcard_hasAddress != null && propVcard_hasAddress.PropertyValues.Count > 0)
			{
				this.Vcard_hasAddress = new Address(propVcard_hasAddress.PropertyValues[0].RelatedEntity,idiomaUsuario);
			}
			this.Roh_operatingCompanies = new List<Organization>();
			SemanticPropertyModel propRoh_operatingCompanies = pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/operatingCompanies");
			if(propRoh_operatingCompanies != null && propRoh_operatingCompanies.PropertyValues.Count > 0)
			{
				foreach (SemanticPropertyModel.PropertyValue propValue in propRoh_operatingCompanies.PropertyValues)
				{
					if(propValue.RelatedEntity!=null){
						Organization roh_operatingCompanies = new Organization(propValue.RelatedEntity,idiomaUsuario);
						this.Roh_operatingCompanies.Add(roh_operatingCompanies);
					}
				}
			}
			SemanticPropertyModel propRoh_industrialPropertyType = pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/industrialPropertyType");
			if(propRoh_industrialPropertyType != null && propRoh_industrialPropertyType.PropertyValues.Count > 0)
			{
				this.Roh_industrialPropertyType = new IndustrialPropertyType(propRoh_industrialPropertyType.PropertyValues[0].RelatedEntity,idiomaUsuario);
			}
			this.Roh_operatingCountries = new List<Address>();
			SemanticPropertyModel propRoh_operatingCountries = pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/operatingCountries");
			if(propRoh_operatingCountries != null && propRoh_operatingCountries.PropertyValues.Count > 0)
			{
				foreach (SemanticPropertyModel.PropertyValue propValue in propRoh_operatingCountries.PropertyValues)
				{
					if(propValue.RelatedEntity!=null){
						Address roh_operatingCountries = new Address(propValue.RelatedEntity,idiomaUsuario);
						this.Roh_operatingCountries.Add(roh_operatingCountries);
					}
				}
			}
			this.Roh_patentInventor = new List<BFO_0000023>();
			SemanticPropertyModel propRoh_patentInventor = pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/patentInventor");
			if(propRoh_patentInventor != null && propRoh_patentInventor.PropertyValues.Count > 0)
			{
				foreach (SemanticPropertyModel.PropertyValue propValue in propRoh_patentInventor.PropertyValues)
				{
					if(propValue.RelatedEntity!=null){
						BFO_0000023 roh_patentInventor = new BFO_0000023(propValue.RelatedEntity,idiomaUsuario);
						this.Roh_patentInventor.Add(roh_patentInventor);
					}
				}
			}
			this.Roh_spanishPatent= GetBooleanPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/spanishPatent"));
			this.Roh_relevantResults = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/relevantResults"));
			this.Roh_applicationNumber = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/applicationNumber"));
			this.Roh_knowHow= GetBooleanPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/knowHow"));
			this.Roh_results= GetBooleanPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/results"));
			this.Roh_patentNumber = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/patentNumber"));
			this.Roh_industrialPropertyTypeOther = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/industrialPropertyTypeOther"));
			this.Roh_title = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/title"));
			this.Roh_exclusiveUse= GetBooleanPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/exclusiveUse"));
			this.Roh_relatedRights= GetBooleanPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/relatedRights"));
			SemanticPropertyModel propRoh_products = pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/products");
			this.Roh_products = new List<string>();
			if (propRoh_products != null && propRoh_products.PropertyValues.Count > 0)
			{
				foreach (SemanticPropertyModel.PropertyValue propValue in propRoh_products.PropertyValues)
				{
					this.Roh_products.Add(propValue.Value);
				}
			}
			this.Roh_referenceCode = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/referenceCode"));
			this.Vivo_dateFiled= GetDateValuePropertySemCms(pSemCmsModel.GetPropertyByPath("http://vivoweb.org/ontology/core#dateFiled"));
			this.Roh_europeanPatent= GetBooleanPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/europeanPatent"));
			this.Roh_authorsRights= GetBooleanPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/authorsRights"));
			this.Vivo_dateIssued= GetDateValuePropertySemCms(pSemCmsModel.GetPropertyByPath("http://vivoweb.org/ontology/core#dateIssued"));
			this.Roh_tradeSecret= GetBooleanPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/tradeSecret"));
			this.Roh_qualityDescription = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/qualityDescription"));
			this.Roh_innovativeEnterprise= GetBooleanPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/innovativeEnterprise"));
			this.Vivo_freeTextKeywords = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://vivoweb.org/ontology/core#freeTextKeywords"));
			this.Roh_internationalPatent= GetBooleanPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/internationalPatent"));
			this.Roh_crisIdentifier = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/crisIdentifier"));
			this.Roh_licenses= GetBooleanPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/licenses"));
			this.Roh_pctPatent= GetBooleanPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/pctPatent"));
		}

		public virtual string RdfType { get { return "http://purl.org/ontology/bibo/Patent"; } }
		public virtual string RdfsLabel { get { return "http://purl.org/ontology/bibo/Patent"; } }
		[LABEL(LanguageEnum.es,"Entidad titular de derechos")]
		[RDFProperty("http://w3id.org/roh/ownerOrganization")]
		public  Organization Roh_ownerOrganization  { get; set;} 
		public string IdRoh_ownerOrganization  { get; set;} 

		[LABEL(LanguageEnum.es,"Lugar de inscripción")]
		[RDFProperty("https://www.w3.org/2006/vcard/ns#hasAddress")]
		public  Address Vcard_hasAddress { get; set;}

		[LABEL(LanguageEnum.es,"http://w3id.org/roh/operatingCompanies")]
		[RDFProperty("http://w3id.org/roh/operatingCompanies")]
		public  List<Organization> Roh_operatingCompanies { get; set;}
		public List<string> IdsRoh_operatingCompanies { get; set;}

		[LABEL(LanguageEnum.es,"Tipo de propiedad industrial")]
		[RDFProperty("http://w3id.org/roh/industrialPropertyType")]
		public  IndustrialPropertyType Roh_industrialPropertyType  { get; set;} 
		public string IdRoh_industrialPropertyType  { get; set;} 

		[LABEL(LanguageEnum.es,"Países de explotación")]
		[RDFProperty("http://w3id.org/roh/operatingCountries")]
		public  List<Address> Roh_operatingCountries { get; set;}

		[LABEL(LanguageEnum.es,"Inventores de patente")]
		[RDFProperty("http://w3id.org/roh/patentInventor")]
		public  List<BFO_0000023> Roh_patentInventor { get; set;}

		[LABEL(LanguageEnum.es,"Patente española")]
		[RDFProperty("http://w3id.org/roh/spanishPatent")]
		public  bool Roh_spanishPatent { get; set;}

		[LABEL(LanguageEnum.es,"Resultados relevantes")]
		[RDFProperty("http://w3id.org/roh/relevantResults")]
		public  string Roh_relevantResults { get; set;}

		[LABEL(LanguageEnum.es,"Nº de solicitud")]
		[RDFProperty("http://w3id.org/roh/applicationNumber")]
		public  string Roh_applicationNumber { get; set;}

		[LABEL(LanguageEnum.es,"Modalidad de know-how")]
		[RDFProperty("http://w3id.org/roh/knowHow")]
		public  bool Roh_knowHow { get; set;}

		[LABEL(LanguageEnum.es,"Resultado")]
		[RDFProperty("http://w3id.org/roh/results")]
		public  bool Roh_results { get; set;}

		[LABEL(LanguageEnum.es,"Nº de patente")]
		[RDFProperty("http://w3id.org/roh/patentNumber")]
		public  string Roh_patentNumber { get; set;}

		[LABEL(LanguageEnum.es,"Tipo de propiedad industrial, otros")]
		[RDFProperty("http://w3id.org/roh/industrialPropertyTypeOther")]
		public  string Roh_industrialPropertyTypeOther { get; set;}

		[LABEL(LanguageEnum.es,"Título propiedad industrial registrada")]
		[RDFProperty("http://w3id.org/roh/title")]
		public  string Roh_title { get; set;}

		[LABEL(LanguageEnum.es,"Explotación en exclusiva")]
		[RDFProperty("http://w3id.org/roh/exclusiveUse")]
		public  bool Roh_exclusiveUse { get; set;}

		[LABEL(LanguageEnum.es,"Derechos conexos")]
		[RDFProperty("http://w3id.org/roh/relatedRights")]
		public  bool Roh_relatedRights { get; set;}

		[LABEL(LanguageEnum.es,"Productos")]
		[RDFProperty("http://w3id.org/roh/products")]
		public  List<string> Roh_products { get; set;}

		[LABEL(LanguageEnum.es,"Cód. de referencia/registro")]
		[RDFProperty("http://w3id.org/roh/referenceCode")]
		public  string Roh_referenceCode { get; set;}

		[LABEL(LanguageEnum.es,"Fecha de registro")]
		[RDFProperty("http://vivoweb.org/ontology/core#dateFiled")]
		public  DateTime? Vivo_dateFiled { get; set;}

		[LABEL(LanguageEnum.es,"Patente europea")]
		[RDFProperty("http://w3id.org/roh/europeanPatent")]
		public  bool Roh_europeanPatent { get; set;}

		[LABEL(LanguageEnum.es,"Derechos de autor")]
		[RDFProperty("http://w3id.org/roh/authorsRights")]
		public  bool Roh_authorsRights { get; set;}

		[LABEL(LanguageEnum.es,"Fecha de concesión")]
		[RDFProperty("http://vivoweb.org/ontology/core#dateIssued")]
		public  DateTime? Vivo_dateIssued { get; set;}

		[LABEL(LanguageEnum.es,"Secreto empresarial")]
		[RDFProperty("http://w3id.org/roh/tradeSecret")]
		public  bool Roh_tradeSecret { get; set;}

		[LABEL(LanguageEnum.es,"Descripción de cualidades")]
		[RDFProperty("http://w3id.org/roh/qualityDescription")]
		public  string Roh_qualityDescription { get; set;}

		[LABEL(LanguageEnum.es,"Generada empresa innovadora")]
		[RDFProperty("http://w3id.org/roh/innovativeEnterprise")]
		public  bool Roh_innovativeEnterprise { get; set;}

		[LABEL(LanguageEnum.es,"Palabras clave")]
		[RDFProperty("http://vivoweb.org/ontology/core#freeTextKeywords")]
		public  string Vivo_freeTextKeywords { get; set;}

		[LABEL(LanguageEnum.es,"Patente internacional no UE")]
		[RDFProperty("http://w3id.org/roh/internationalPatent")]
		public  bool Roh_internationalPatent { get; set;}

		[LABEL(LanguageEnum.es,"Identificador")]
		[RDFProperty("http://w3id.org/roh/crisIdentifier")]
		public  string Roh_crisIdentifier { get; set;}

		[LABEL(LanguageEnum.es,"Licencias")]
		[RDFProperty("http://w3id.org/roh/licenses")]
		public  bool Roh_licenses { get; set;}

		[LABEL(LanguageEnum.es,"Patente PCT")]
		[RDFProperty("http://w3id.org/roh/pctPatent")]
		public  bool Roh_pctPatent { get; set;}


		internal override void GetProperties()
		{
			base.GetProperties();
			propList.Add(new StringOntologyProperty("roh:ownerOrganization", this.IdRoh_ownerOrganization));
			propList.Add(new ListStringOntologyProperty("roh:operatingCompanies", this.IdsRoh_operatingCompanies));
			propList.Add(new StringOntologyProperty("roh:industrialPropertyType", this.IdRoh_industrialPropertyType));
			propList.Add(new BoolOntologyProperty("roh:spanishPatent", this.Roh_spanishPatent));
			propList.Add(new StringOntologyProperty("roh:relevantResults", this.Roh_relevantResults));
			propList.Add(new StringOntologyProperty("roh:applicationNumber", this.Roh_applicationNumber));
			propList.Add(new BoolOntologyProperty("roh:knowHow", this.Roh_knowHow));
			propList.Add(new BoolOntologyProperty("roh:results", this.Roh_results));
			propList.Add(new StringOntologyProperty("roh:patentNumber", this.Roh_patentNumber));
			propList.Add(new StringOntologyProperty("roh:industrialPropertyTypeOther", this.Roh_industrialPropertyTypeOther));
			propList.Add(new StringOntologyProperty("roh:title", this.Roh_title));
			propList.Add(new BoolOntologyProperty("roh:exclusiveUse", this.Roh_exclusiveUse));
			propList.Add(new BoolOntologyProperty("roh:relatedRights", this.Roh_relatedRights));
			propList.Add(new ListStringOntologyProperty("roh:products", this.Roh_products));
			propList.Add(new StringOntologyProperty("roh:referenceCode", this.Roh_referenceCode));
			if (this.Vivo_dateFiled.HasValue){
				propList.Add(new DateOntologyProperty("vivo:dateFiled", this.Vivo_dateFiled.Value));
				}
			propList.Add(new BoolOntologyProperty("roh:europeanPatent", this.Roh_europeanPatent));
			propList.Add(new BoolOntologyProperty("roh:authorsRights", this.Roh_authorsRights));
			if (this.Vivo_dateIssued.HasValue){
				propList.Add(new DateOntologyProperty("vivo:dateIssued", this.Vivo_dateIssued.Value));
				}
			propList.Add(new BoolOntologyProperty("roh:tradeSecret", this.Roh_tradeSecret));
			propList.Add(new StringOntologyProperty("roh:qualityDescription", this.Roh_qualityDescription));
			propList.Add(new BoolOntologyProperty("roh:innovativeEnterprise", this.Roh_innovativeEnterprise));
			propList.Add(new StringOntologyProperty("vivo:freeTextKeywords", this.Vivo_freeTextKeywords));
			propList.Add(new BoolOntologyProperty("roh:internationalPatent", this.Roh_internationalPatent));
			propList.Add(new StringOntologyProperty("roh:crisIdentifier", this.Roh_crisIdentifier));
			propList.Add(new BoolOntologyProperty("roh:licenses", this.Roh_licenses));
			propList.Add(new BoolOntologyProperty("roh:pctPatent", this.Roh_pctPatent));
		}

		internal override void GetEntities()
		{
			base.GetEntities();
			if(Vcard_hasAddress!=null){
				Vcard_hasAddress.GetProperties();
				Vcard_hasAddress.GetEntities();
				OntologyEntity entityVcard_hasAddress = new OntologyEntity("https://www.w3.org/2006/vcard/ns#Address", "https://www.w3.org/2006/vcard/ns#Address", "vcard:hasAddress", Vcard_hasAddress.propList, Vcard_hasAddress.entList);
				entList.Add(entityVcard_hasAddress);
			}
			if(Roh_operatingCountries!=null){
				foreach(Address prop in Roh_operatingCountries){
					prop.GetProperties();
					prop.GetEntities();
					OntologyEntity entityAddress = new OntologyEntity("https://www.w3.org/2006/vcard/ns#Address", "https://www.w3.org/2006/vcard/ns#Address", "roh:operatingCountries", prop.propList, prop.entList);
				entList.Add(entityAddress);
				prop.Entity= entityAddress;
				}
			}
			if(Roh_patentInventor!=null){
				foreach(BFO_0000023 prop in Roh_patentInventor){
					prop.GetProperties();
					prop.GetEntities();
					OntologyEntity entityBFO_0000023 = new OntologyEntity("http://purl.obolibrary.org/obo/BFO_0000023", "http://purl.obolibrary.org/obo/BFO_0000023", "roh:patentInventor", prop.propList, prop.entList);
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
			AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Patent_{ResourceID}_{ArticleID}", "http://www.w3.org/1999/02/22-rdf-syntax-ns#type", $"<http://purl.org/ontology/bibo/Patent>", list, " . ");
			AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Patent_{ResourceID}_{ArticleID}", "http://www.w3.org/2000/01/rdf-schema#label", $"\"http://purl.org/ontology/bibo/Patent\"", list, " . ");
			AgregarTripleALista($"{resourceAPI.GraphsUrl}{ResourceID}", "http://gnoss/hasEntidad", $"<{resourceAPI.GraphsUrl}items/Patent_{ResourceID}_{ArticleID}>", list, " . ");
			if(this.Vcard_hasAddress != null)
			{
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Address_{ResourceID}_{this.Vcard_hasAddress.ArticleID}", "http://www.w3.org/1999/02/22-rdf-syntax-ns#type", $"<https://www.w3.org/2006/vcard/ns#Address>", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Address_{ResourceID}_{this.Vcard_hasAddress.ArticleID}", "http://www.w3.org/2000/01/rdf-schema#label", $"\"https://www.w3.org/2006/vcard/ns#Address\"", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}{ResourceID}", "http://gnoss/hasEntidad", $"<{resourceAPI.GraphsUrl}items/Address_{ResourceID}_{this.Vcard_hasAddress.ArticleID}>", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Patent_{ResourceID}_{ArticleID}", "https://www.w3.org/2006/vcard/ns#hasAddress", $"<{resourceAPI.GraphsUrl}items/Address_{ResourceID}_{this.Vcard_hasAddress.ArticleID}>", list, " . ");
				if(this.Vcard_hasAddress.IdVcard_hasRegion != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Address_{ResourceID}_{this.Vcard_hasAddress.ArticleID}",  "https://www.w3.org/2006/vcard/ns#hasRegion", $"<{this.Vcard_hasAddress.IdVcard_hasRegion}>", list, " . ");
				}
				if(this.Vcard_hasAddress.IdVcard_hasCountryName != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Address_{ResourceID}_{this.Vcard_hasAddress.ArticleID}",  "https://www.w3.org/2006/vcard/ns#hasCountryName", $"<{this.Vcard_hasAddress.IdVcard_hasCountryName}>", list, " . ");
				}
				if(this.Vcard_hasAddress.Vcard_locality != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Address_{ResourceID}_{this.Vcard_hasAddress.ArticleID}",  "https://www.w3.org/2006/vcard/ns#locality", $"\"{GenerarTextoSinSaltoDeLinea(this.Vcard_hasAddress.Vcard_locality)}\"", list, " . ");
				}
			}
			if(this.Roh_operatingCountries != null)
			{
			foreach(var item0 in this.Roh_operatingCountries)
			{
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Address_{ResourceID}_{item0.ArticleID}", "http://www.w3.org/1999/02/22-rdf-syntax-ns#type", $"<https://www.w3.org/2006/vcard/ns#Address>", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Address_{ResourceID}_{item0.ArticleID}", "http://www.w3.org/2000/01/rdf-schema#label", $"\"https://www.w3.org/2006/vcard/ns#Address\"", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}{ResourceID}", "http://gnoss/hasEntidad", $"<{resourceAPI.GraphsUrl}items/Address_{ResourceID}_{item0.ArticleID}>", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Patent_{ResourceID}_{ArticleID}", "http://w3id.org/roh/operatingCountries", $"<{resourceAPI.GraphsUrl}items/Address_{ResourceID}_{item0.ArticleID}>", list, " . ");
				if(item0.IdVcard_hasRegion != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Address_{ResourceID}_{item0.ArticleID}",  "https://www.w3.org/2006/vcard/ns#hasRegion", $"<{item0.IdVcard_hasRegion}>", list, " . ");
				}
				if(item0.IdVcard_hasCountryName != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Address_{ResourceID}_{item0.ArticleID}",  "https://www.w3.org/2006/vcard/ns#hasCountryName", $"<{item0.IdVcard_hasCountryName}>", list, " . ");
				}
				if(item0.Vcard_locality != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Address_{ResourceID}_{item0.ArticleID}",  "https://www.w3.org/2006/vcard/ns#locality", $"\"{GenerarTextoSinSaltoDeLinea(item0.Vcard_locality)}\"", list, " . ");
				}
			}
			}
			if(this.Roh_patentInventor != null)
			{
			foreach(var item0 in this.Roh_patentInventor)
			{
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/BFO_0000023_{ResourceID}_{item0.ArticleID}", "http://www.w3.org/1999/02/22-rdf-syntax-ns#type", $"<http://purl.obolibrary.org/obo/BFO_0000023>", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/BFO_0000023_{ResourceID}_{item0.ArticleID}", "http://www.w3.org/2000/01/rdf-schema#label", $"\"http://purl.obolibrary.org/obo/BFO_0000023\"", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}{ResourceID}", "http://gnoss/hasEntidad", $"<{resourceAPI.GraphsUrl}items/BFO_0000023_{ResourceID}_{item0.ArticleID}>", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Patent_{ResourceID}_{ArticleID}", "http://w3id.org/roh/patentInventor", $"<{resourceAPI.GraphsUrl}items/BFO_0000023_{ResourceID}_{item0.ArticleID}>", list, " . ");
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
				if(this.IdRoh_ownerOrganization != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Patent_{ResourceID}_{ArticleID}",  "http://w3id.org/roh/ownerOrganization", $"<{this.IdRoh_ownerOrganization}>", list, " . ");
				}
				if(this.IdsRoh_operatingCompanies != null)
				{
					foreach(var item2 in this.IdsRoh_operatingCompanies)
					{
						AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Patent_{ResourceID}_{ArticleID}", "http://w3id.org/roh/operatingCompanies", $"<{item2}>", list, " . ");
					}
				}
				if(this.IdRoh_industrialPropertyType != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Patent_{ResourceID}_{ArticleID}",  "http://w3id.org/roh/industrialPropertyType", $"<{this.IdRoh_industrialPropertyType}>", list, " . ");
				}
				if(this.Roh_spanishPatent != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Patent_{ResourceID}_{ArticleID}",  "http://w3id.org/roh/spanishPatent", $"\"{this.Roh_spanishPatent.ToString()}\"", list, " . ");
				}
				if(this.Roh_relevantResults != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Patent_{ResourceID}_{ArticleID}",  "http://w3id.org/roh/relevantResults", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_relevantResults)}\"", list, " . ");
				}
				if(this.Roh_applicationNumber != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Patent_{ResourceID}_{ArticleID}",  "http://w3id.org/roh/applicationNumber", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_applicationNumber)}\"", list, " . ");
				}
				if(this.Roh_knowHow != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Patent_{ResourceID}_{ArticleID}",  "http://w3id.org/roh/knowHow", $"\"{this.Roh_knowHow.ToString()}\"", list, " . ");
				}
				if(this.Roh_results != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Patent_{ResourceID}_{ArticleID}",  "http://w3id.org/roh/results", $"\"{this.Roh_results.ToString()}\"", list, " . ");
				}
				if(this.Roh_patentNumber != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Patent_{ResourceID}_{ArticleID}",  "http://w3id.org/roh/patentNumber", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_patentNumber)}\"", list, " . ");
				}
				if(this.Roh_industrialPropertyTypeOther != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Patent_{ResourceID}_{ArticleID}",  "http://w3id.org/roh/industrialPropertyTypeOther", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_industrialPropertyTypeOther)}\"", list, " . ");
				}
				if(this.Roh_title != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Patent_{ResourceID}_{ArticleID}",  "http://w3id.org/roh/title", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_title)}\"", list, " . ");
				}
				if(this.Roh_exclusiveUse != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Patent_{ResourceID}_{ArticleID}",  "http://w3id.org/roh/exclusiveUse", $"\"{this.Roh_exclusiveUse.ToString()}\"", list, " . ");
				}
				if(this.Roh_relatedRights != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Patent_{ResourceID}_{ArticleID}",  "http://w3id.org/roh/relatedRights", $"\"{this.Roh_relatedRights.ToString()}\"", list, " . ");
				}
				if(this.Roh_products != null)
				{
					foreach(var item2 in this.Roh_products)
					{
						AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Patent_{ResourceID}_{ArticleID}", "http://w3id.org/roh/products", $"\"{GenerarTextoSinSaltoDeLinea(item2)}\"", list, " . ");
					}
				}
				if(this.Roh_referenceCode != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Patent_{ResourceID}_{ArticleID}",  "http://w3id.org/roh/referenceCode", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_referenceCode)}\"", list, " . ");
				}
				if(this.Vivo_dateFiled != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Patent_{ResourceID}_{ArticleID}",  "http://vivoweb.org/ontology/core#dateFiled", $"\"{this.Vivo_dateFiled.Value.ToString("yyyyMMddHHmmss")}\"", list, " . ");
				}
				if(this.Roh_europeanPatent != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Patent_{ResourceID}_{ArticleID}",  "http://w3id.org/roh/europeanPatent", $"\"{this.Roh_europeanPatent.ToString()}\"", list, " . ");
				}
				if(this.Roh_authorsRights != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Patent_{ResourceID}_{ArticleID}",  "http://w3id.org/roh/authorsRights", $"\"{this.Roh_authorsRights.ToString()}\"", list, " . ");
				}
				if(this.Vivo_dateIssued != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Patent_{ResourceID}_{ArticleID}",  "http://vivoweb.org/ontology/core#dateIssued", $"\"{this.Vivo_dateIssued.Value.ToString("yyyyMMddHHmmss")}\"", list, " . ");
				}
				if(this.Roh_tradeSecret != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Patent_{ResourceID}_{ArticleID}",  "http://w3id.org/roh/tradeSecret", $"\"{this.Roh_tradeSecret.ToString()}\"", list, " . ");
				}
				if(this.Roh_qualityDescription != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Patent_{ResourceID}_{ArticleID}",  "http://w3id.org/roh/qualityDescription", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_qualityDescription)}\"", list, " . ");
				}
				if(this.Roh_innovativeEnterprise != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Patent_{ResourceID}_{ArticleID}",  "http://w3id.org/roh/innovativeEnterprise", $"\"{this.Roh_innovativeEnterprise.ToString()}\"", list, " . ");
				}
				if(this.Vivo_freeTextKeywords != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Patent_{ResourceID}_{ArticleID}",  "http://vivoweb.org/ontology/core#freeTextKeywords", $"\"{GenerarTextoSinSaltoDeLinea(this.Vivo_freeTextKeywords)}\"", list, " . ");
				}
				if(this.Roh_internationalPatent != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Patent_{ResourceID}_{ArticleID}",  "http://w3id.org/roh/internationalPatent", $"\"{this.Roh_internationalPatent.ToString()}\"", list, " . ");
				}
				if(this.Roh_crisIdentifier != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Patent_{ResourceID}_{ArticleID}",  "http://w3id.org/roh/crisIdentifier", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_crisIdentifier)}\"", list, " . ");
				}
				if(this.Roh_licenses != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Patent_{ResourceID}_{ArticleID}",  "http://w3id.org/roh/licenses", $"\"{this.Roh_licenses.ToString()}\"", list, " . ");
				}
				if(this.Roh_pctPatent != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Patent_{ResourceID}_{ArticleID}",  "http://w3id.org/roh/pctPatent", $"\"{this.Roh_pctPatent.ToString()}\"", list, " . ");
				}
			return list;
		}

		public override List<string> ToSearchGraphTriples(ResourceApi resourceAPI)
		{
			List<string> list = new List<string>();
			List<string> listaSearch = new List<string>();
			AgregarTags(list);
			AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}", "http://www.w3.org/1999/02/22-rdf-syntax-ns#type", $"\"patent\"", list, " . ");
			AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}", "http://gnoss/type", $"\"http://purl.org/ontology/bibo/Patent\"", list, " . ");
			AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}", "http://gnoss/hasfechapublicacion", $"{DateTime.Now.ToString("yyyyMMddHHmmss")}", list, " . ");
			AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}", "http://gnoss/hastipodoc", "\"5\"", list, " . ");
			AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}", "http://gnoss/hasfechamodificacion", $"{DateTime.Now.ToString("yyyyMMddHHmmss")}", list, " . ");
			AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}", "http://gnoss/hasnumeroVisitas", "0", list, " . ");
			AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}", "http://gnoss/hasprivacidadCom", "\"publico\"", list, " . ");
			AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}", "http://xmlns.com/foaf/0.1/firstName", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_title)}\"", list, " . ");
			AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}", "http://gnoss/hasnombrecompleto", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_title)}\"", list, " . ");
			string search = string.Empty;
			if(this.Vcard_hasAddress != null)
			{
				AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}", "https://www.w3.org/2006/vcard/ns#hasAddress", $"<{resourceAPI.GraphsUrl}items/address_{ResourceID}_{this.Vcard_hasAddress.ArticleID}>", list, " . ");
				if(this.Vcard_hasAddress.IdVcard_hasRegion != null)
				{
					Regex regex = new Regex(@"\/items\/.+_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}");
					string itemRegex = this.Vcard_hasAddress.IdVcard_hasRegion;
					if (regex.IsMatch(itemRegex))
					{
						itemRegex = $"http://gnoss/{resourceAPI.GetShortGuid(itemRegex).ToString().ToUpper()}";
					}
					else
					{
						itemRegex = itemRegex.ToLower();
					}
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/address_{ResourceID}_{this.Vcard_hasAddress.ArticleID}",  "https://www.w3.org/2006/vcard/ns#hasRegion", $"<{itemRegex}>", list, " . ");
				}
				if(this.Vcard_hasAddress.IdVcard_hasCountryName != null)
				{
					Regex regex = new Regex(@"\/items\/.+_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}");
					string itemRegex = this.Vcard_hasAddress.IdVcard_hasCountryName;
					if (regex.IsMatch(itemRegex))
					{
						itemRegex = $"http://gnoss/{resourceAPI.GetShortGuid(itemRegex).ToString().ToUpper()}";
					}
					else
					{
						itemRegex = itemRegex.ToLower();
					}
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/address_{ResourceID}_{this.Vcard_hasAddress.ArticleID}",  "https://www.w3.org/2006/vcard/ns#hasCountryName", $"<{itemRegex}>", list, " . ");
				}
				if(this.Vcard_hasAddress.Vcard_locality != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/address_{ResourceID}_{this.Vcard_hasAddress.ArticleID}",  "https://www.w3.org/2006/vcard/ns#locality", $"\"{GenerarTextoSinSaltoDeLinea(this.Vcard_hasAddress.Vcard_locality).ToLower()}\"", list, " . ");
				}
			}
			if(this.Roh_operatingCountries != null)
			{
			foreach(var item0 in this.Roh_operatingCountries)
			{
				AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}", "http://w3id.org/roh/operatingCountries", $"<{resourceAPI.GraphsUrl}items/address_{ResourceID}_{item0.ArticleID}>", list, " . ");
				if(item0.IdVcard_hasRegion != null)
				{
					Regex regex = new Regex(@"\/items\/.+_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}");
					string itemRegex = item0.IdVcard_hasRegion;
					if (regex.IsMatch(itemRegex))
					{
						itemRegex = $"http://gnoss/{resourceAPI.GetShortGuid(itemRegex).ToString().ToUpper()}";
					}
					else
					{
						itemRegex = itemRegex.ToLower();
					}
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/address_{ResourceID}_{item0.ArticleID}",  "https://www.w3.org/2006/vcard/ns#hasRegion", $"<{itemRegex}>", list, " . ");
				}
				if(item0.IdVcard_hasCountryName != null)
				{
					Regex regex = new Regex(@"\/items\/.+_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}");
					string itemRegex = item0.IdVcard_hasCountryName;
					if (regex.IsMatch(itemRegex))
					{
						itemRegex = $"http://gnoss/{resourceAPI.GetShortGuid(itemRegex).ToString().ToUpper()}";
					}
					else
					{
						itemRegex = itemRegex.ToLower();
					}
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/address_{ResourceID}_{item0.ArticleID}",  "https://www.w3.org/2006/vcard/ns#hasCountryName", $"<{itemRegex}>", list, " . ");
				}
				if(item0.Vcard_locality != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/address_{ResourceID}_{item0.ArticleID}",  "https://www.w3.org/2006/vcard/ns#locality", $"\"{GenerarTextoSinSaltoDeLinea(item0.Vcard_locality).ToLower()}\"", list, " . ");
				}
			}
			}
			if(this.Roh_patentInventor != null)
			{
			foreach(var item0 in this.Roh_patentInventor)
			{
				AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}", "http://w3id.org/roh/patentInventor", $"<{resourceAPI.GraphsUrl}items/bfo_0000023_{ResourceID}_{item0.ArticleID}>", list, " . ");
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
				if(this.IdRoh_ownerOrganization != null)
				{
					Regex regex = new Regex(@"\/items\/.+_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}");
					string itemRegex = this.IdRoh_ownerOrganization;
					if (regex.IsMatch(itemRegex))
					{
						itemRegex = $"http://gnoss/{resourceAPI.GetShortGuid(itemRegex).ToString().ToUpper()}";
					}
					else
					{
						itemRegex = itemRegex.ToLower();
					}
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "http://w3id.org/roh/ownerOrganization", $"<{itemRegex}>", list, " . ");
				}
				if(this.IdsRoh_operatingCompanies != null)
				{
					foreach(var item2 in this.IdsRoh_operatingCompanies)
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
						AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}", "http://w3id.org/roh/operatingCompanies", $"<{itemRegex}>", list, " . ");
					}
				}
				if(this.IdRoh_industrialPropertyType != null)
				{
					Regex regex = new Regex(@"\/items\/.+_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}");
					string itemRegex = this.IdRoh_industrialPropertyType;
					if (regex.IsMatch(itemRegex))
					{
						itemRegex = $"http://gnoss/{resourceAPI.GetShortGuid(itemRegex).ToString().ToUpper()}";
					}
					else
					{
						itemRegex = itemRegex.ToLower();
					}
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "http://w3id.org/roh/industrialPropertyType", $"<{itemRegex}>", list, " . ");
				}
				if(this.Roh_spanishPatent != null)
				{
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "http://w3id.org/roh/spanishPatent", $"\"{this.Roh_spanishPatent.ToString().ToLower()}\"", list, " . ");
				}
				if(this.Roh_relevantResults != null)
				{
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "http://w3id.org/roh/relevantResults", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_relevantResults).ToLower()}\"", list, " . ");
				}
				if(this.Roh_applicationNumber != null)
				{
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "http://w3id.org/roh/applicationNumber", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_applicationNumber).ToLower()}\"", list, " . ");
				}
				if(this.Roh_knowHow != null)
				{
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "http://w3id.org/roh/knowHow", $"\"{this.Roh_knowHow.ToString().ToLower()}\"", list, " . ");
				}
				if(this.Roh_results != null)
				{
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "http://w3id.org/roh/results", $"\"{this.Roh_results.ToString().ToLower()}\"", list, " . ");
				}
				if(this.Roh_patentNumber != null)
				{
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "http://w3id.org/roh/patentNumber", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_patentNumber).ToLower()}\"", list, " . ");
				}
				if(this.Roh_industrialPropertyTypeOther != null)
				{
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "http://w3id.org/roh/industrialPropertyTypeOther", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_industrialPropertyTypeOther).ToLower()}\"", list, " . ");
				}
				if(this.Roh_title != null)
				{
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "http://w3id.org/roh/title", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_title).ToLower()}\"", list, " . ");
				}
				if(this.Roh_exclusiveUse != null)
				{
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "http://w3id.org/roh/exclusiveUse", $"\"{this.Roh_exclusiveUse.ToString().ToLower()}\"", list, " . ");
				}
				if(this.Roh_relatedRights != null)
				{
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "http://w3id.org/roh/relatedRights", $"\"{this.Roh_relatedRights.ToString().ToLower()}\"", list, " . ");
				}
				if(this.Roh_products != null)
				{
					foreach(var item2 in this.Roh_products)
					{
						AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}", "http://w3id.org/roh/products", $"\"{GenerarTextoSinSaltoDeLinea(item2).ToLower()}\"", list, " . ");
					}
				}
				if(this.Roh_referenceCode != null)
				{
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "http://w3id.org/roh/referenceCode", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_referenceCode).ToLower()}\"", list, " . ");
				}
				if(this.Vivo_dateFiled != null)
				{
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "http://vivoweb.org/ontology/core#dateFiled", $"{this.Vivo_dateFiled.Value.ToString("yyyyMMddHHmmss")}", list, " . ");
				}
				if(this.Roh_europeanPatent != null)
				{
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "http://w3id.org/roh/europeanPatent", $"\"{this.Roh_europeanPatent.ToString().ToLower()}\"", list, " . ");
				}
				if(this.Roh_authorsRights != null)
				{
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "http://w3id.org/roh/authorsRights", $"\"{this.Roh_authorsRights.ToString().ToLower()}\"", list, " . ");
				}
				if(this.Vivo_dateIssued != null)
				{
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "http://vivoweb.org/ontology/core#dateIssued", $"{this.Vivo_dateIssued.Value.ToString("yyyyMMddHHmmss")}", list, " . ");
				}
				if(this.Roh_tradeSecret != null)
				{
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "http://w3id.org/roh/tradeSecret", $"\"{this.Roh_tradeSecret.ToString().ToLower()}\"", list, " . ");
				}
				if(this.Roh_qualityDescription != null)
				{
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "http://w3id.org/roh/qualityDescription", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_qualityDescription).ToLower()}\"", list, " . ");
				}
				if(this.Roh_innovativeEnterprise != null)
				{
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "http://w3id.org/roh/innovativeEnterprise", $"\"{this.Roh_innovativeEnterprise.ToString().ToLower()}\"", list, " . ");
				}
				if(this.Vivo_freeTextKeywords != null)
				{
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "http://vivoweb.org/ontology/core#freeTextKeywords", $"\"{GenerarTextoSinSaltoDeLinea(this.Vivo_freeTextKeywords).ToLower()}\"", list, " . ");
				}
				if(this.Roh_internationalPatent != null)
				{
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "http://w3id.org/roh/internationalPatent", $"\"{this.Roh_internationalPatent.ToString().ToLower()}\"", list, " . ");
				}
				if(this.Roh_crisIdentifier != null)
				{
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "http://w3id.org/roh/crisIdentifier", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_crisIdentifier).ToLower()}\"", list, " . ");
				}
				if(this.Roh_licenses != null)
				{
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "http://w3id.org/roh/licenses", $"\"{this.Roh_licenses.ToString().ToLower()}\"", list, " . ");
				}
				if(this.Roh_pctPatent != null)
				{
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "http://w3id.org/roh/pctPatent", $"\"{this.Roh_pctPatent.ToString().ToLower()}\"", list, " . ");
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
			return $"{resourceAPI.GraphsUrl}items/PatentOntology_{ResourceID}_{ArticleID}";
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
