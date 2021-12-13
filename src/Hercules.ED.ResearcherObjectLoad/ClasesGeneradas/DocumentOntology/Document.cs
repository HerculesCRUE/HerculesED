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
using Feature = FeatureOntology.Feature;
using Language = LanguageOntology.Language;
using Project = ProjectOntology.Project;
using Group = GroupOntology.Group;
using Person = PersonOntology.Person;
using PublicationType = PublicationtypeOntology.PublicationType;
using Document = DocumentOntology.Document;
using MainDocument = MaindocumentOntology.MainDocument;
using ScientificActivityDocument = ScientificactivitydocumentOntology.ScientificActivityDocument;

namespace DocumentOntology
{
	[ExcludeFromCodeCoverage]
	public class Document : GnossOCBase
	{

		public Document() : base() { } 

		public Document(SemanticResourceModel pSemCmsModel, LanguageEnum idiomaUsuario) : base()
		{
			this.mGNOSSID = pSemCmsModel.RootEntities[0].Entity.Uri;
			this.Roh_suggestedKnowledgeArea = new List<CategoryPath>();
			SemanticPropertyModel propRoh_suggestedKnowledgeArea = pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/suggestedKnowledgeArea");
			if(propRoh_suggestedKnowledgeArea != null && propRoh_suggestedKnowledgeArea.PropertyValues.Count > 0)
			{
				foreach (SemanticPropertyModel.PropertyValue propValue in propRoh_suggestedKnowledgeArea.PropertyValues)
				{
					if(propValue.RelatedEntity!=null){
						CategoryPath roh_suggestedKnowledgeArea = new CategoryPath(propValue.RelatedEntity,idiomaUsuario);
						this.Roh_suggestedKnowledgeArea.Add(roh_suggestedKnowledgeArea);
					}
				}
			}
			SemanticPropertyModel propVcard_hasCountryName = pSemCmsModel.GetPropertyByPath("https://www.w3.org/2006/vcard/ns#hasCountryName");
			if(propVcard_hasCountryName != null && propVcard_hasCountryName.PropertyValues.Count > 0)
			{
				this.Vcard_hasCountryName = new Feature(propVcard_hasCountryName.PropertyValues[0].RelatedEntity,idiomaUsuario);
			}
			this.Vcard_hasLanguage = new List<Language>();
			SemanticPropertyModel propVcard_hasLanguage = pSemCmsModel.GetPropertyByPath("https://www.w3.org/2006/vcard/ns#hasLanguage");
			if(propVcard_hasLanguage != null && propVcard_hasLanguage.PropertyValues.Count > 0)
			{
				foreach (SemanticPropertyModel.PropertyValue propValue in propVcard_hasLanguage.PropertyValues)
				{
					if(propValue.RelatedEntity!=null){
						Language vcard_hasLanguage = new Language(propValue.RelatedEntity,idiomaUsuario);
						this.Vcard_hasLanguage.Add(vcard_hasLanguage);
					}
				}
			}
			this.Bibo_authorList = new List<BFO_0000023>();
			SemanticPropertyModel propBibo_authorList = pSemCmsModel.GetPropertyByPath("http://purl.org/ontology/bibo/authorList");
			if(propBibo_authorList != null && propBibo_authorList.PropertyValues.Count > 0)
			{
				foreach (SemanticPropertyModel.PropertyValue propValue in propBibo_authorList.PropertyValues)
				{
					if(propValue.RelatedEntity!=null){
						BFO_0000023 bibo_authorList = new BFO_0000023(propValue.RelatedEntity,idiomaUsuario);
						this.Bibo_authorList.Add(bibo_authorList);
					}
				}
			}
			SemanticPropertyModel propRoh_project = pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/project");
			if(propRoh_project != null && propRoh_project.PropertyValues.Count > 0)
			{
				this.Roh_project = new Project(propRoh_project.PropertyValues[0].RelatedEntity,idiomaUsuario);
			}
			this.Roh_isProducedBy = new List<Group>();
			SemanticPropertyModel propRoh_isProducedBy = pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/isProducedBy");
			if(propRoh_isProducedBy != null && propRoh_isProducedBy.PropertyValues.Count > 0)
			{
				foreach (SemanticPropertyModel.PropertyValue propValue in propRoh_isProducedBy.PropertyValues)
				{
					if(propValue.RelatedEntity!=null){
						Group roh_isProducedBy = new Group(propValue.RelatedEntity,idiomaUsuario);
						this.Roh_isProducedBy.Add(roh_isProducedBy);
					}
				}
			}
			this.Roh_externalKnowledgeArea = new List<CategoryPath>();
			SemanticPropertyModel propRoh_externalKnowledgeArea = pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/externalKnowledgeArea");
			if(propRoh_externalKnowledgeArea != null && propRoh_externalKnowledgeArea.PropertyValues.Count > 0)
			{
				foreach (SemanticPropertyModel.PropertyValue propValue in propRoh_externalKnowledgeArea.PropertyValues)
				{
					if(propValue.RelatedEntity!=null){
						CategoryPath roh_externalKnowledgeArea = new CategoryPath(propValue.RelatedEntity,idiomaUsuario);
						this.Roh_externalKnowledgeArea.Add(roh_externalKnowledgeArea);
					}
				}
			}
			this.Roh_userKnowledgeArea = new List<CategoryPath>();
			SemanticPropertyModel propRoh_userKnowledgeArea = pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/userKnowledgeArea");
			if(propRoh_userKnowledgeArea != null && propRoh_userKnowledgeArea.PropertyValues.Count > 0)
			{
				foreach (SemanticPropertyModel.PropertyValue propValue in propRoh_userKnowledgeArea.PropertyValues)
				{
					if(propValue.RelatedEntity!=null){
						CategoryPath roh_userKnowledgeArea = new CategoryPath(propValue.RelatedEntity,idiomaUsuario);
						this.Roh_userKnowledgeArea.Add(roh_userKnowledgeArea);
					}
				}
			}
			SemanticPropertyModel propBibo_presentedAt = pSemCmsModel.GetPropertyByPath("http://purl.org/ontology/bibo/presentedAt");
			if(propBibo_presentedAt != null && propBibo_presentedAt.PropertyValues.Count > 0)
			{
				this.Bibo_presentedAt = new Event(propBibo_presentedAt.PropertyValues[0].RelatedEntity,idiomaUsuario);
			}
			this.Roh_correspondingAuthor = new List<Person>();
			SemanticPropertyModel propRoh_correspondingAuthor = pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/correspondingAuthor");
			if(propRoh_correspondingAuthor != null && propRoh_correspondingAuthor.PropertyValues.Count > 0)
			{
				foreach (SemanticPropertyModel.PropertyValue propValue in propRoh_correspondingAuthor.PropertyValues)
				{
					if(propValue.RelatedEntity!=null){
						Person roh_correspondingAuthor = new Person(propValue.RelatedEntity,idiomaUsuario);
						this.Roh_correspondingAuthor.Add(roh_correspondingAuthor);
					}
				}
			}
			SemanticPropertyModel propDc_type = pSemCmsModel.GetPropertyByPath("http://purl.org/dc/elements/1.1/type");
			if(propDc_type != null && propDc_type.PropertyValues.Count > 0)
			{
				this.Dc_type = new PublicationType(propDc_type.PropertyValues[0].RelatedEntity,idiomaUsuario);
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
			this.Bibo_identifier = new List<fDocument>();
			SemanticPropertyModel propBibo_identifier = pSemCmsModel.GetPropertyByPath("http://purl.org/ontology/bibo/identifier");
			if(propBibo_identifier != null && propBibo_identifier.PropertyValues.Count > 0)
			{
				foreach (SemanticPropertyModel.PropertyValue propValue in propBibo_identifier.PropertyValues)
				{
					if(propValue.RelatedEntity!=null){
						fDocument bibo_identifier = new fDocument(propValue.RelatedEntity,idiomaUsuario);
						this.Bibo_identifier.Add(bibo_identifier);
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
			SemanticPropertyModel propVcard_hasRegion = pSemCmsModel.GetPropertyByPath("https://www.w3.org/2006/vcard/ns#hasRegion");
			if(propVcard_hasRegion != null && propVcard_hasRegion.PropertyValues.Count > 0)
			{
				this.Vcard_hasRegion = new Feature(propVcard_hasRegion.PropertyValues[0].RelatedEntity,idiomaUsuario);
			}
			this.Roh_hasMetric = new List<PublicationMetric>();
			SemanticPropertyModel propRoh_hasMetric = pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/hasMetric");
			if(propRoh_hasMetric != null && propRoh_hasMetric.PropertyValues.Count > 0)
			{
				foreach (SemanticPropertyModel.PropertyValue propValue in propRoh_hasMetric.PropertyValues)
				{
					if(propValue.RelatedEntity!=null){
						PublicationMetric roh_hasMetric = new PublicationMetric(propValue.RelatedEntity,idiomaUsuario);
						this.Roh_hasMetric.Add(roh_hasMetric);
					}
				}
			}
			this.Bibo_cites = new List<Document>();
			SemanticPropertyModel propBibo_cites = pSemCmsModel.GetPropertyByPath("http://purl.org/ontology/bibo/cites");
			if(propBibo_cites != null && propBibo_cites.PropertyValues.Count > 0)
			{
				foreach (SemanticPropertyModel.PropertyValue propValue in propBibo_cites.PropertyValues)
				{
					if(propValue.RelatedEntity!=null){
						Document bibo_cites = new Document(propValue.RelatedEntity,idiomaUsuario);
						this.Bibo_cites.Add(bibo_cites);
					}
				}
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
			SemanticPropertyModel propVivo_hasPublicationVenue = pSemCmsModel.GetPropertyByPath("http://vivoweb.org/ontology/core#hasPublicationVenue");
			if(propVivo_hasPublicationVenue != null && propVivo_hasPublicationVenue.PropertyValues.Count > 0)
			{
				this.Vivo_hasPublicationVenue = new MainDocument(propVivo_hasPublicationVenue.PropertyValues[0].RelatedEntity,idiomaUsuario);
			}
			this.Roh_enrichedKnowledgeArea = new List<CategoryPath>();
			SemanticPropertyModel propRoh_enrichedKnowledgeArea = pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/enrichedKnowledgeArea");
			if(propRoh_enrichedKnowledgeArea != null && propRoh_enrichedKnowledgeArea.PropertyValues.Count > 0)
			{
				foreach (SemanticPropertyModel.PropertyValue propValue in propRoh_enrichedKnowledgeArea.PropertyValues)
				{
					if(propValue.RelatedEntity!=null){
						CategoryPath roh_enrichedKnowledgeArea = new CategoryPath(propValue.RelatedEntity,idiomaUsuario);
						this.Roh_enrichedKnowledgeArea.Add(roh_enrichedKnowledgeArea);
					}
				}
			}
			this.Roh_legalDeposit = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/legalDeposit"));
			this.Roh_scopus = GetNumberIntPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/scopus"));
			this.Roh_assessmentStatus = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/assessmentStatus"));
			this.Roh_publicationTitle = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/publicationTitle"));
			this.Vcard_locality = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("https://www.w3.org/2006/vcard/ns#locality"));
			this.Roh_authorsNumber = GetNumberIntPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/authorsNumber"));
			this.Roh_inrecs = GetNumberIntPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/inrecs"));
			SemanticPropertyModel propRoh_externalKeywords = pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/externalKeywords");
			this.Roh_externalKeywords = new List<string>();
			if (propRoh_externalKeywords != null && propRoh_externalKeywords.PropertyValues.Count > 0)
			{
				foreach (SemanticPropertyModel.PropertyValue propValue in propRoh_externalKeywords.PropertyValues)
				{
					this.Roh_externalKeywords.Add(propValue.Value);
				}
			}
			this.Roh_reviewsNumber = GetNumberIntPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/reviewsNumber"));
			this.Roh_userJournal = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/userJournal"));
			this.Bibo_abstract = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://purl.org/ontology/bibo/abstract"));
			this.Bibo_pmid = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://purl.org/ontology/bibo/pmid"));
			SemanticPropertyModel propVivo_freeTextKeyword = pSemCmsModel.GetPropertyByPath("http://vivoweb.org/ontology/core#freeTextKeyword");
			this.Vivo_freeTextKeyword = new List<string>();
			if (propVivo_freeTextKeyword != null && propVivo_freeTextKeyword.PropertyValues.Count > 0)
			{
				foreach (SemanticPropertyModel.PropertyValue propValue in propVivo_freeTextKeyword.PropertyValues)
				{
					this.Vivo_freeTextKeyword.Add(propValue.Value);
				}
			}
			this.Bibo_handle = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://purl.org/ontology/bibo/handle"));
			this.Bibo_doi = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://purl.org/ontology/bibo/doi"));
			this.Roh_wos = GetNumberIntPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/wos"));
			this.Bibo_issue = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://purl.org/ontology/bibo/issue"));
			SemanticPropertyModel propRoh_suggestedKeywords = pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/suggestedKeywords");
			this.Roh_suggestedKeywords = new List<string>();
			if (propRoh_suggestedKeywords != null && propRoh_suggestedKeywords.PropertyValues.Count > 0)
			{
				foreach (SemanticPropertyModel.PropertyValue propValue in propRoh_suggestedKeywords.PropertyValues)
				{
					this.Roh_suggestedKeywords.Add(propValue.Value);
				}
			}
			this.Bibo_volume = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://purl.org/ontology/bibo/volume"));
			SemanticPropertyModel propRoh_enrichedKeywords = pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/enrichedKeywords");
			this.Roh_enrichedKeywords = new List<string>();
			if (propRoh_enrichedKeywords != null && propRoh_enrichedKeywords.PropertyValues.Count > 0)
			{
				foreach (SemanticPropertyModel.PropertyValue propValue in propRoh_enrichedKeywords.PropertyValues)
				{
					this.Roh_enrichedKeywords.Add(propValue.Value);
				}
			}
			this.Dct_issued= GetDateValuePropertySemCms(pSemCmsModel.GetPropertyByPath("http://purl.org/dc/terms/issued"));
			this.Roh_typeOthers = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/typeOthers"));
			this.Bibo_pageStart = GetNumberIntPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://purl.org/ontology/bibo/pageStart"));
			this.Roh_crisIdentifier = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/crisIdentifier"));
			this.Roh_congressProceedingsPublication= GetBooleanPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/congressProceedingsPublication"));
			SemanticPropertyModel propVcard_url = pSemCmsModel.GetPropertyByPath("https://www.w3.org/2006/vcard/ns#url");
			this.Vcard_url = new List<string>();
			if (propVcard_url != null && propVcard_url.PropertyValues.Count > 0)
			{
				foreach (SemanticPropertyModel.PropertyValue propValue in propVcard_url.PropertyValues)
				{
					this.Vcard_url.Add(propValue.Value);
				}
			}
			this.Bibo_pageEnd = GetNumberIntPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://purl.org/ontology/bibo/pageEnd"));
			this.Roh_collection = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/collection"));
			SemanticPropertyModel propRoh_userKeywords = pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/userKeywords");
			this.Roh_userKeywords = new List<string>();
			if (propRoh_userKeywords != null && propRoh_userKeywords.PropertyValues.Count > 0)
			{
				foreach (SemanticPropertyModel.PropertyValue propValue in propRoh_userKeywords.PropertyValues)
				{
					this.Roh_userKeywords.Add(propValue.Value);
				}
			}
			this.Roh_title = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/title"));
			SemanticPropertyModel propRoh_scientificActivityDocument = pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/scientificActivityDocument");
			if(propRoh_scientificActivityDocument != null && propRoh_scientificActivityDocument.PropertyValues.Count > 0)
			{
				this.Roh_scientificActivityDocument = new ScientificActivityDocument(propRoh_scientificActivityDocument.PropertyValues[0].RelatedEntity,idiomaUsuario);
			}
			this.Roh_isValidated= GetBooleanPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/isValidated"));
			this.Roh_citationLoadedCount = GetNumberIntPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/citationLoadedCount")).Value;
			this.Roh_citationCount = GetNumberIntPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/citationCount")).Value;
			this.Roh_referencesLoadedCount = GetNumberIntPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/referencesLoadedCount")).Value;
			this.Roh_isPublic= GetBooleanPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/isPublic"));
		}

		public Document(SemanticEntityModel pSemCmsModel, LanguageEnum idiomaUsuario) : base()
		{
			this.mGNOSSID = pSemCmsModel.Entity.Uri;
			this.mURL = pSemCmsModel.Properties.FirstOrDefault(p => p.PropertyValues.Any(prop => prop.DownloadUrl != null))?.FirstPropertyValue.DownloadUrl;
			this.Roh_suggestedKnowledgeArea = new List<CategoryPath>();
			SemanticPropertyModel propRoh_suggestedKnowledgeArea = pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/suggestedKnowledgeArea");
			if(propRoh_suggestedKnowledgeArea != null && propRoh_suggestedKnowledgeArea.PropertyValues.Count > 0)
			{
				foreach (SemanticPropertyModel.PropertyValue propValue in propRoh_suggestedKnowledgeArea.PropertyValues)
				{
					if(propValue.RelatedEntity!=null){
						CategoryPath roh_suggestedKnowledgeArea = new CategoryPath(propValue.RelatedEntity,idiomaUsuario);
						this.Roh_suggestedKnowledgeArea.Add(roh_suggestedKnowledgeArea);
					}
				}
			}
			SemanticPropertyModel propVcard_hasCountryName = pSemCmsModel.GetPropertyByPath("https://www.w3.org/2006/vcard/ns#hasCountryName");
			if(propVcard_hasCountryName != null && propVcard_hasCountryName.PropertyValues.Count > 0)
			{
				this.Vcard_hasCountryName = new Feature(propVcard_hasCountryName.PropertyValues[0].RelatedEntity,idiomaUsuario);
			}
			this.Vcard_hasLanguage = new List<Language>();
			SemanticPropertyModel propVcard_hasLanguage = pSemCmsModel.GetPropertyByPath("https://www.w3.org/2006/vcard/ns#hasLanguage");
			if(propVcard_hasLanguage != null && propVcard_hasLanguage.PropertyValues.Count > 0)
			{
				foreach (SemanticPropertyModel.PropertyValue propValue in propVcard_hasLanguage.PropertyValues)
				{
					if(propValue.RelatedEntity!=null){
						Language vcard_hasLanguage = new Language(propValue.RelatedEntity,idiomaUsuario);
						this.Vcard_hasLanguage.Add(vcard_hasLanguage);
					}
				}
			}
			this.Bibo_authorList = new List<BFO_0000023>();
			SemanticPropertyModel propBibo_authorList = pSemCmsModel.GetPropertyByPath("http://purl.org/ontology/bibo/authorList");
			if(propBibo_authorList != null && propBibo_authorList.PropertyValues.Count > 0)
			{
				foreach (SemanticPropertyModel.PropertyValue propValue in propBibo_authorList.PropertyValues)
				{
					if(propValue.RelatedEntity!=null){
						BFO_0000023 bibo_authorList = new BFO_0000023(propValue.RelatedEntity,idiomaUsuario);
						this.Bibo_authorList.Add(bibo_authorList);
					}
				}
			}
			SemanticPropertyModel propRoh_project = pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/project");
			if(propRoh_project != null && propRoh_project.PropertyValues.Count > 0)
			{
				this.Roh_project = new Project(propRoh_project.PropertyValues[0].RelatedEntity,idiomaUsuario);
			}
			this.Roh_isProducedBy = new List<Group>();
			SemanticPropertyModel propRoh_isProducedBy = pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/isProducedBy");
			if(propRoh_isProducedBy != null && propRoh_isProducedBy.PropertyValues.Count > 0)
			{
				foreach (SemanticPropertyModel.PropertyValue propValue in propRoh_isProducedBy.PropertyValues)
				{
					if(propValue.RelatedEntity!=null){
						Group roh_isProducedBy = new Group(propValue.RelatedEntity,idiomaUsuario);
						this.Roh_isProducedBy.Add(roh_isProducedBy);
					}
				}
			}
			this.Roh_externalKnowledgeArea = new List<CategoryPath>();
			SemanticPropertyModel propRoh_externalKnowledgeArea = pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/externalKnowledgeArea");
			if(propRoh_externalKnowledgeArea != null && propRoh_externalKnowledgeArea.PropertyValues.Count > 0)
			{
				foreach (SemanticPropertyModel.PropertyValue propValue in propRoh_externalKnowledgeArea.PropertyValues)
				{
					if(propValue.RelatedEntity!=null){
						CategoryPath roh_externalKnowledgeArea = new CategoryPath(propValue.RelatedEntity,idiomaUsuario);
						this.Roh_externalKnowledgeArea.Add(roh_externalKnowledgeArea);
					}
				}
			}
			this.Roh_userKnowledgeArea = new List<CategoryPath>();
			SemanticPropertyModel propRoh_userKnowledgeArea = pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/userKnowledgeArea");
			if(propRoh_userKnowledgeArea != null && propRoh_userKnowledgeArea.PropertyValues.Count > 0)
			{
				foreach (SemanticPropertyModel.PropertyValue propValue in propRoh_userKnowledgeArea.PropertyValues)
				{
					if(propValue.RelatedEntity!=null){
						CategoryPath roh_userKnowledgeArea = new CategoryPath(propValue.RelatedEntity,idiomaUsuario);
						this.Roh_userKnowledgeArea.Add(roh_userKnowledgeArea);
					}
				}
			}
			SemanticPropertyModel propBibo_presentedAt = pSemCmsModel.GetPropertyByPath("http://purl.org/ontology/bibo/presentedAt");
			if(propBibo_presentedAt != null && propBibo_presentedAt.PropertyValues.Count > 0)
			{
				this.Bibo_presentedAt = new Event(propBibo_presentedAt.PropertyValues[0].RelatedEntity,idiomaUsuario);
			}
			this.Roh_correspondingAuthor = new List<Person>();
			SemanticPropertyModel propRoh_correspondingAuthor = pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/correspondingAuthor");
			if(propRoh_correspondingAuthor != null && propRoh_correspondingAuthor.PropertyValues.Count > 0)
			{
				foreach (SemanticPropertyModel.PropertyValue propValue in propRoh_correspondingAuthor.PropertyValues)
				{
					if(propValue.RelatedEntity!=null){
						Person roh_correspondingAuthor = new Person(propValue.RelatedEntity,idiomaUsuario);
						this.Roh_correspondingAuthor.Add(roh_correspondingAuthor);
					}
				}
			}
			SemanticPropertyModel propDc_type = pSemCmsModel.GetPropertyByPath("http://purl.org/dc/elements/1.1/type");
			if(propDc_type != null && propDc_type.PropertyValues.Count > 0)
			{
				this.Dc_type = new PublicationType(propDc_type.PropertyValues[0].RelatedEntity,idiomaUsuario);
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
			this.Bibo_identifier = new List<fDocument>();
			SemanticPropertyModel propBibo_identifier = pSemCmsModel.GetPropertyByPath("http://purl.org/ontology/bibo/identifier");
			if(propBibo_identifier != null && propBibo_identifier.PropertyValues.Count > 0)
			{
				foreach (SemanticPropertyModel.PropertyValue propValue in propBibo_identifier.PropertyValues)
				{
					if(propValue.RelatedEntity!=null){
						fDocument bibo_identifier = new fDocument(propValue.RelatedEntity,idiomaUsuario);
						this.Bibo_identifier.Add(bibo_identifier);
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
			SemanticPropertyModel propVcard_hasRegion = pSemCmsModel.GetPropertyByPath("https://www.w3.org/2006/vcard/ns#hasRegion");
			if(propVcard_hasRegion != null && propVcard_hasRegion.PropertyValues.Count > 0)
			{
				this.Vcard_hasRegion = new Feature(propVcard_hasRegion.PropertyValues[0].RelatedEntity,idiomaUsuario);
			}
			this.Roh_hasMetric = new List<PublicationMetric>();
			SemanticPropertyModel propRoh_hasMetric = pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/hasMetric");
			if(propRoh_hasMetric != null && propRoh_hasMetric.PropertyValues.Count > 0)
			{
				foreach (SemanticPropertyModel.PropertyValue propValue in propRoh_hasMetric.PropertyValues)
				{
					if(propValue.RelatedEntity!=null){
						PublicationMetric roh_hasMetric = new PublicationMetric(propValue.RelatedEntity,idiomaUsuario);
						this.Roh_hasMetric.Add(roh_hasMetric);
					}
				}
			}
			this.Bibo_cites = new List<Document>();
			SemanticPropertyModel propBibo_cites = pSemCmsModel.GetPropertyByPath("http://purl.org/ontology/bibo/cites");
			if(propBibo_cites != null && propBibo_cites.PropertyValues.Count > 0)
			{
				foreach (SemanticPropertyModel.PropertyValue propValue in propBibo_cites.PropertyValues)
				{
					if(propValue.RelatedEntity!=null){
						Document bibo_cites = new Document(propValue.RelatedEntity,idiomaUsuario);
						this.Bibo_cites.Add(bibo_cites);
					}
				}
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
			SemanticPropertyModel propVivo_hasPublicationVenue = pSemCmsModel.GetPropertyByPath("http://vivoweb.org/ontology/core#hasPublicationVenue");
			if(propVivo_hasPublicationVenue != null && propVivo_hasPublicationVenue.PropertyValues.Count > 0)
			{
				this.Vivo_hasPublicationVenue = new MainDocument(propVivo_hasPublicationVenue.PropertyValues[0].RelatedEntity,idiomaUsuario);
			}
			this.Roh_enrichedKnowledgeArea = new List<CategoryPath>();
			SemanticPropertyModel propRoh_enrichedKnowledgeArea = pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/enrichedKnowledgeArea");
			if(propRoh_enrichedKnowledgeArea != null && propRoh_enrichedKnowledgeArea.PropertyValues.Count > 0)
			{
				foreach (SemanticPropertyModel.PropertyValue propValue in propRoh_enrichedKnowledgeArea.PropertyValues)
				{
					if(propValue.RelatedEntity!=null){
						CategoryPath roh_enrichedKnowledgeArea = new CategoryPath(propValue.RelatedEntity,idiomaUsuario);
						this.Roh_enrichedKnowledgeArea.Add(roh_enrichedKnowledgeArea);
					}
				}
			}
			this.Roh_legalDeposit = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/legalDeposit"));
			this.Roh_scopus = GetNumberIntPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/scopus"));
			this.Roh_assessmentStatus = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/assessmentStatus"));
			this.Roh_publicationTitle = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/publicationTitle"));
			this.Vcard_locality = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("https://www.w3.org/2006/vcard/ns#locality"));
			this.Roh_authorsNumber = GetNumberIntPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/authorsNumber"));
			this.Roh_inrecs = GetNumberIntPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/inrecs"));
			SemanticPropertyModel propRoh_externalKeywords = pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/externalKeywords");
			this.Roh_externalKeywords = new List<string>();
			if (propRoh_externalKeywords != null && propRoh_externalKeywords.PropertyValues.Count > 0)
			{
				foreach (SemanticPropertyModel.PropertyValue propValue in propRoh_externalKeywords.PropertyValues)
				{
					this.Roh_externalKeywords.Add(propValue.Value);
				}
			}
			this.Roh_reviewsNumber = GetNumberIntPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/reviewsNumber"));
			this.Roh_userJournal = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/userJournal"));
			this.Bibo_abstract = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://purl.org/ontology/bibo/abstract"));
			this.Bibo_pmid = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://purl.org/ontology/bibo/pmid"));
			SemanticPropertyModel propVivo_freeTextKeyword = pSemCmsModel.GetPropertyByPath("http://vivoweb.org/ontology/core#freeTextKeyword");
			this.Vivo_freeTextKeyword = new List<string>();
			if (propVivo_freeTextKeyword != null && propVivo_freeTextKeyword.PropertyValues.Count > 0)
			{
				foreach (SemanticPropertyModel.PropertyValue propValue in propVivo_freeTextKeyword.PropertyValues)
				{
					this.Vivo_freeTextKeyword.Add(propValue.Value);
				}
			}
			this.Bibo_handle = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://purl.org/ontology/bibo/handle"));
			this.Bibo_doi = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://purl.org/ontology/bibo/doi"));
			this.Roh_wos = GetNumberIntPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/wos"));
			this.Bibo_issue = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://purl.org/ontology/bibo/issue"));
			SemanticPropertyModel propRoh_suggestedKeywords = pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/suggestedKeywords");
			this.Roh_suggestedKeywords = new List<string>();
			if (propRoh_suggestedKeywords != null && propRoh_suggestedKeywords.PropertyValues.Count > 0)
			{
				foreach (SemanticPropertyModel.PropertyValue propValue in propRoh_suggestedKeywords.PropertyValues)
				{
					this.Roh_suggestedKeywords.Add(propValue.Value);
				}
			}
			this.Bibo_volume = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://purl.org/ontology/bibo/volume"));
			SemanticPropertyModel propRoh_enrichedKeywords = pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/enrichedKeywords");
			this.Roh_enrichedKeywords = new List<string>();
			if (propRoh_enrichedKeywords != null && propRoh_enrichedKeywords.PropertyValues.Count > 0)
			{
				foreach (SemanticPropertyModel.PropertyValue propValue in propRoh_enrichedKeywords.PropertyValues)
				{
					this.Roh_enrichedKeywords.Add(propValue.Value);
				}
			}
			this.Dct_issued= GetDateValuePropertySemCms(pSemCmsModel.GetPropertyByPath("http://purl.org/dc/terms/issued"));
			this.Roh_typeOthers = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/typeOthers"));
			this.Bibo_pageStart = GetNumberIntPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://purl.org/ontology/bibo/pageStart"));
			this.Roh_crisIdentifier = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/crisIdentifier"));
			this.Roh_congressProceedingsPublication= GetBooleanPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/congressProceedingsPublication"));
			SemanticPropertyModel propVcard_url = pSemCmsModel.GetPropertyByPath("https://www.w3.org/2006/vcard/ns#url");
			this.Vcard_url = new List<string>();
			if (propVcard_url != null && propVcard_url.PropertyValues.Count > 0)
			{
				foreach (SemanticPropertyModel.PropertyValue propValue in propVcard_url.PropertyValues)
				{
					this.Vcard_url.Add(propValue.Value);
				}
			}
			this.Bibo_pageEnd = GetNumberIntPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://purl.org/ontology/bibo/pageEnd"));
			this.Roh_collection = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/collection"));
			SemanticPropertyModel propRoh_userKeywords = pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/userKeywords");
			this.Roh_userKeywords = new List<string>();
			if (propRoh_userKeywords != null && propRoh_userKeywords.PropertyValues.Count > 0)
			{
				foreach (SemanticPropertyModel.PropertyValue propValue in propRoh_userKeywords.PropertyValues)
				{
					this.Roh_userKeywords.Add(propValue.Value);
				}
			}
			this.Roh_title = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/title"));
			SemanticPropertyModel propRoh_scientificActivityDocument = pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/scientificActivityDocument");
			if(propRoh_scientificActivityDocument != null && propRoh_scientificActivityDocument.PropertyValues.Count > 0)
			{
				this.Roh_scientificActivityDocument = new ScientificActivityDocument(propRoh_scientificActivityDocument.PropertyValues[0].RelatedEntity,idiomaUsuario);
			}
			this.Roh_isValidated= GetBooleanPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/isValidated"));
			this.Roh_citationLoadedCount = GetNumberIntPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/citationLoadedCount")).Value;
			this.Roh_citationCount = GetNumberIntPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/citationCount")).Value;
			this.Roh_referencesLoadedCount = GetNumberIntPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/referencesLoadedCount")).Value;
			this.Roh_isPublic= GetBooleanPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/isPublic"));
		}

		public virtual string RdfType { get { return "http://purl.org/ontology/bibo/Document"; } }
		public virtual string RdfsLabel { get { return "http://purl.org/ontology/bibo/Document"; } }
		[LABEL(LanguageEnum.es,"http://w3id.org/roh/suggestedKnowledgeArea")]
		[RDFProperty("http://w3id.org/roh/suggestedKnowledgeArea")]
		public  List<CategoryPath> Roh_suggestedKnowledgeArea { get; set;}

		[LABEL(LanguageEnum.es,"https://www.w3.org/2006/vcard/ns#hasCountryName")]
		[RDFProperty("https://www.w3.org/2006/vcard/ns#hasCountryName")]
		public  Feature Vcard_hasCountryName  { get; set;} 
		public string IdVcard_hasCountryName  { get; set;} 

		[LABEL(LanguageEnum.es,"https://www.w3.org/2006/vcard/ns#hasLanguage")]
		[RDFProperty("https://www.w3.org/2006/vcard/ns#hasLanguage")]
		public  List<Language> Vcard_hasLanguage { get; set;}
		public List<string> IdsVcard_hasLanguage { get; set;}

		[LABEL(LanguageEnum.es,"http://purl.org/ontology/bibo/authorList")]
		[RDFProperty("http://purl.org/ontology/bibo/authorList")]
		public  List<BFO_0000023> Bibo_authorList { get; set;}

		[LABEL(LanguageEnum.es,"http://w3id.org/roh/project")]
		[RDFProperty("http://w3id.org/roh/project")]
		public  Project Roh_project  { get; set;} 
		public string IdRoh_project  { get; set;} 

		[LABEL(LanguageEnum.es,"http://w3id.org/roh/isProducedBy")]
		[RDFProperty("http://w3id.org/roh/isProducedBy")]
		public  List<Group> Roh_isProducedBy { get; set;}
		public List<string> IdsRoh_isProducedBy { get; set;}

		[LABEL(LanguageEnum.es,"http://w3id.org/roh/externalKnowledgeArea")]
		[RDFProperty("http://w3id.org/roh/externalKnowledgeArea")]
		public  List<CategoryPath> Roh_externalKnowledgeArea { get; set;}

		[LABEL(LanguageEnum.es,"http://w3id.org/roh/userKnowledgeArea")]
		[RDFProperty("http://w3id.org/roh/userKnowledgeArea")]
		public  List<CategoryPath> Roh_userKnowledgeArea { get; set;}

		[LABEL(LanguageEnum.es,"http://purl.org/ontology/bibo/presentedAt")]
		[RDFProperty("http://purl.org/ontology/bibo/presentedAt")]
		public  Event Bibo_presentedAt { get; set;}

		[LABEL(LanguageEnum.es,"http://w3id.org/roh/correspondingAuthor")]
		[RDFProperty("http://w3id.org/roh/correspondingAuthor")]
		public  List<Person> Roh_correspondingAuthor { get; set;}
		public List<string> IdsRoh_correspondingAuthor { get; set;}

		[LABEL(LanguageEnum.es,"http://purl.org/dc/elements/1.1/type")]
		[RDFProperty("http://purl.org/dc/elements/1.1/type")]
		public  PublicationType Dc_type  { get; set;} 
		public string IdDc_type  { get; set;} 

		[LABEL(LanguageEnum.es,"http://w3id.org/roh/publicGroupList")]
		[RDFProperty("http://w3id.org/roh/publicGroupList")]
		public  List<Group> Roh_publicGroupList { get; set;}
		public List<string> IdsRoh_publicGroupList { get; set;}

		[LABEL(LanguageEnum.es,"http://purl.org/ontology/bibo/identifier")]
		[RDFProperty("http://purl.org/ontology/bibo/identifier")]
		public  List<fDocument> Bibo_identifier { get; set;}

		[LABEL(LanguageEnum.es,"http://w3id.org/roh/publicAuthorList")]
		[RDFProperty("http://w3id.org/roh/publicAuthorList")]
		public  List<Person> Roh_publicAuthorList { get; set;}
		public List<string> IdsRoh_publicAuthorList { get; set;}

		[LABEL(LanguageEnum.es,"https://www.w3.org/2006/vcard/ns#hasRegion")]
		[RDFProperty("https://www.w3.org/2006/vcard/ns#hasRegion")]
		public  Feature Vcard_hasRegion  { get; set;} 
		public string IdVcard_hasRegion  { get; set;} 

		[LABEL(LanguageEnum.es,"http://w3id.org/roh/hasMetric")]
		[RDFProperty("http://w3id.org/roh/hasMetric")]
		public  List<PublicationMetric> Roh_hasMetric { get; set;}

		[LABEL(LanguageEnum.es,"http://purl.org/ontology/bibo/cites")]
		[RDFProperty("http://purl.org/ontology/bibo/cites")]
		public  List<Document> Bibo_cites { get; set;}
		public List<string> IdsBibo_cites { get; set;}

		[LABEL(LanguageEnum.es,"http://w3id.org/roh/hasKnowledgeArea")]
		[RDFProperty("http://w3id.org/roh/hasKnowledgeArea")]
		public  List<CategoryPath> Roh_hasKnowledgeArea { get; set;}

		[LABEL(LanguageEnum.es,"http://vivoweb.org/ontology/core#hasPublicationVenue")]
		[RDFProperty("http://vivoweb.org/ontology/core#hasPublicationVenue")]
		public  MainDocument Vivo_hasPublicationVenue  { get; set;} 
		public string IdVivo_hasPublicationVenue  { get; set;} 

		[LABEL(LanguageEnum.es,"http://w3id.org/roh/enrichedKnowledgeArea")]
		[RDFProperty("http://w3id.org/roh/enrichedKnowledgeArea")]
		public  List<CategoryPath> Roh_enrichedKnowledgeArea { get; set;}

		[LABEL(LanguageEnum.es,"http://w3id.org/roh/legalDeposit")]
		[RDFProperty("http://w3id.org/roh/legalDeposit")]
		public  string Roh_legalDeposit { get; set;}

		[LABEL(LanguageEnum.es,"http://w3id.org/roh/scopus")]
		[RDFProperty("http://w3id.org/roh/scopus")]
		public  int? Roh_scopus { get; set;}

		[LABEL(LanguageEnum.es,"http://w3id.org/roh/assessmentStatus")]
		[RDFProperty("http://w3id.org/roh/assessmentStatus")]
		public  string Roh_assessmentStatus { get; set;}

		[LABEL(LanguageEnum.es,"http://w3id.org/roh/publicationTitle")]
		[RDFProperty("http://w3id.org/roh/publicationTitle")]
		public  string Roh_publicationTitle { get; set;}

		[LABEL(LanguageEnum.es,"https://www.w3.org/2006/vcard/ns#locality")]
		[RDFProperty("https://www.w3.org/2006/vcard/ns#locality")]
		public  string Vcard_locality { get; set;}

		[LABEL(LanguageEnum.es,"http://w3id.org/roh/authorsNumber")]
		[RDFProperty("http://w3id.org/roh/authorsNumber")]
		public  int? Roh_authorsNumber { get; set;}

		[LABEL(LanguageEnum.es,"http://w3id.org/roh/inrecs")]
		[RDFProperty("http://w3id.org/roh/inrecs")]
		public  int? Roh_inrecs { get; set;}

		[LABEL(LanguageEnum.es,"http://w3id.org/roh/externalKeywords")]
		[RDFProperty("http://w3id.org/roh/externalKeywords")]
		public  List<string> Roh_externalKeywords { get; set;}

		[LABEL(LanguageEnum.es,"http://w3id.org/roh/reviewsNumber")]
		[RDFProperty("http://w3id.org/roh/reviewsNumber")]
		public  int? Roh_reviewsNumber { get; set;}

		[RDFProperty("http://w3id.org/roh/userJournal")]
		public  string Roh_userJournal { get; set;}

		[LABEL(LanguageEnum.es,"http://purl.org/ontology/bibo/abstract")]
		[RDFProperty("http://purl.org/ontology/bibo/abstract")]
		public  string Bibo_abstract { get; set;}

		[LABEL(LanguageEnum.es,"http://purl.org/ontology/bibo/pmid")]
		[RDFProperty("http://purl.org/ontology/bibo/pmid")]
		public  string Bibo_pmid { get; set;}

		[LABEL(LanguageEnum.es,"http://vivoweb.org/ontology/core#freeTextKeyword")]
		[RDFProperty("http://vivoweb.org/ontology/core#freeTextKeyword")]
		public  List<string> Vivo_freeTextKeyword { get; set;}

		[LABEL(LanguageEnum.es,"http://purl.org/ontology/bibo/handle")]
		[RDFProperty("http://purl.org/ontology/bibo/handle")]
		public  string Bibo_handle { get; set;}

		[LABEL(LanguageEnum.es,"http://purl.org/ontology/bibo/doi")]
		[RDFProperty("http://purl.org/ontology/bibo/doi")]
		public  string Bibo_doi { get; set;}

		[LABEL(LanguageEnum.es,"http://w3id.org/roh/wos")]
		[RDFProperty("http://w3id.org/roh/wos")]
		public  int? Roh_wos { get; set;}

		[LABEL(LanguageEnum.es,"http://purl.org/ontology/bibo/issue")]
		[RDFProperty("http://purl.org/ontology/bibo/issue")]
		public  string Bibo_issue { get; set;}

		[LABEL(LanguageEnum.es,"http://w3id.org/roh/suggestedKeywords")]
		[RDFProperty("http://w3id.org/roh/suggestedKeywords")]
		public  List<string> Roh_suggestedKeywords { get; set;}

		[LABEL(LanguageEnum.es,"http://purl.org/ontology/bibo/volume")]
		[RDFProperty("http://purl.org/ontology/bibo/volume")]
		public  string Bibo_volume { get; set;}

		[LABEL(LanguageEnum.es,"http://w3id.org/roh/enrichedKeywords")]
		[RDFProperty("http://w3id.org/roh/enrichedKeywords")]
		public  List<string> Roh_enrichedKeywords { get; set;}

		[LABEL(LanguageEnum.es,"http://purl.org/dc/terms/issued")]
		[RDFProperty("http://purl.org/dc/terms/issued")]
		public  DateTime? Dct_issued { get; set;}

		[RDFProperty("http://w3id.org/roh/typeOthers")]
		public  string Roh_typeOthers { get; set;}

		[LABEL(LanguageEnum.es,"http://purl.org/ontology/bibo/pageStart")]
		[RDFProperty("http://purl.org/ontology/bibo/pageStart")]
		public  int? Bibo_pageStart { get; set;}

		[LABEL(LanguageEnum.es,"http://w3id.org/roh/crisIdentifier")]
		[RDFProperty("http://w3id.org/roh/crisIdentifier")]
		public  string Roh_crisIdentifier { get; set;}

		[LABEL(LanguageEnum.es,"http://w3id.org/roh/congressProceedingsPublication")]
		[RDFProperty("http://w3id.org/roh/congressProceedingsPublication")]
		public  bool Roh_congressProceedingsPublication { get; set;}

		[LABEL(LanguageEnum.es,"https://www.w3.org/2006/vcard/ns#url")]
		[RDFProperty("https://www.w3.org/2006/vcard/ns#url")]
		public  List<string> Vcard_url { get; set;}

		[LABEL(LanguageEnum.es,"http://purl.org/ontology/bibo/pageEnd")]
		[RDFProperty("http://purl.org/ontology/bibo/pageEnd")]
		public  int? Bibo_pageEnd { get; set;}

		[LABEL(LanguageEnum.es,"http://w3id.org/roh/collection")]
		[RDFProperty("http://w3id.org/roh/collection")]
		public  string Roh_collection { get; set;}

		[LABEL(LanguageEnum.es,"http://w3id.org/roh/userKeywords")]
		[RDFProperty("http://w3id.org/roh/userKeywords")]
		public  List<string> Roh_userKeywords { get; set;}

		[LABEL(LanguageEnum.es,"http://w3id.org/roh/title")]
		[RDFProperty("http://w3id.org/roh/title")]
		public  string Roh_title { get; set;}

		[LABEL(LanguageEnum.es,"http://w3id.org/roh/scientificActivityDocument")]
		[RDFProperty("http://w3id.org/roh/scientificActivityDocument")]
		[Required]
		public  ScientificActivityDocument Roh_scientificActivityDocument  { get; set;} 
		public string IdRoh_scientificActivityDocument  { get; set;} 

		[LABEL(LanguageEnum.es,"http://w3id.org/roh/isValidated")]
		[RDFProperty("http://w3id.org/roh/isValidated")]
		public  bool Roh_isValidated { get; set;}

		[LABEL(LanguageEnum.es,"http://w3id.org/roh/citationLoadedCount")]
		[RDFProperty("http://w3id.org/roh/citationLoadedCount")]
		public  int Roh_citationLoadedCount { get; set;}

		[RDFProperty("http://w3id.org/roh/citationCount")]
		public  int Roh_citationCount { get; set;}

		[LABEL(LanguageEnum.es,"http://w3id.org/roh/referencesLoadedCount")]
		[RDFProperty("http://w3id.org/roh/referencesLoadedCount")]
		public  int Roh_referencesLoadedCount { get; set;}

		[LABEL(LanguageEnum.es,"http://w3id.org/roh/isPublic")]
		[RDFProperty("http://w3id.org/roh/isPublic")]
		public  bool Roh_isPublic { get; set;}


		internal override void GetProperties()
		{
			base.GetProperties();
			propList.Add(new StringOntologyProperty("vcard:hasCountryName", this.IdVcard_hasCountryName));
			propList.Add(new ListStringOntologyProperty("vcard:hasLanguage", this.IdsVcard_hasLanguage));
			propList.Add(new StringOntologyProperty("roh:project", this.IdRoh_project));
			propList.Add(new ListStringOntologyProperty("roh:isProducedBy", this.IdsRoh_isProducedBy));
			propList.Add(new ListStringOntologyProperty("roh:correspondingAuthor", this.IdsRoh_correspondingAuthor));
			propList.Add(new StringOntologyProperty("dc:type", this.IdDc_type));
			propList.Add(new ListStringOntologyProperty("roh:publicGroupList", this.IdsRoh_publicGroupList));
			propList.Add(new ListStringOntologyProperty("roh:publicAuthorList", this.IdsRoh_publicAuthorList));
			propList.Add(new StringOntologyProperty("vcard:hasRegion", this.IdVcard_hasRegion));
			propList.Add(new ListStringOntologyProperty("bibo:cites", this.IdsBibo_cites));
			propList.Add(new StringOntologyProperty("vivo:hasPublicationVenue", this.IdVivo_hasPublicationVenue));
			propList.Add(new StringOntologyProperty("roh:legalDeposit", this.Roh_legalDeposit));
			propList.Add(new StringOntologyProperty("roh:scopus", this.Roh_scopus.ToString()));
			propList.Add(new StringOntologyProperty("roh:assessmentStatus", this.Roh_assessmentStatus));
			propList.Add(new StringOntologyProperty("roh:publicationTitle", this.Roh_publicationTitle));
			propList.Add(new StringOntologyProperty("vcard:locality", this.Vcard_locality));
			propList.Add(new StringOntologyProperty("roh:authorsNumber", this.Roh_authorsNumber.ToString()));
			propList.Add(new StringOntologyProperty("roh:inrecs", this.Roh_inrecs.ToString()));
			propList.Add(new ListStringOntologyProperty("roh:externalKeywords", this.Roh_externalKeywords));
			propList.Add(new StringOntologyProperty("roh:reviewsNumber", this.Roh_reviewsNumber.ToString()));
			propList.Add(new StringOntologyProperty("roh:userJournal", this.Roh_userJournal));
			propList.Add(new StringOntologyProperty("bibo:abstract", this.Bibo_abstract));
			propList.Add(new StringOntologyProperty("bibo:pmid", this.Bibo_pmid));
			propList.Add(new ListStringOntologyProperty("vivo:freeTextKeyword", this.Vivo_freeTextKeyword));
			propList.Add(new StringOntologyProperty("bibo:handle", this.Bibo_handle));
			propList.Add(new StringOntologyProperty("bibo:doi", this.Bibo_doi));
			propList.Add(new StringOntologyProperty("roh:wos", this.Roh_wos.ToString()));
			propList.Add(new StringOntologyProperty("bibo:issue", this.Bibo_issue));
			propList.Add(new ListStringOntologyProperty("roh:suggestedKeywords", this.Roh_suggestedKeywords));
			propList.Add(new StringOntologyProperty("bibo:volume", this.Bibo_volume));
			propList.Add(new ListStringOntologyProperty("roh:enrichedKeywords", this.Roh_enrichedKeywords));
			if (this.Dct_issued.HasValue){
				propList.Add(new DateOntologyProperty("dct:issued", this.Dct_issued.Value));
				}
			propList.Add(new StringOntologyProperty("roh:typeOthers", this.Roh_typeOthers));
			propList.Add(new StringOntologyProperty("bibo:pageStart", this.Bibo_pageStart.ToString()));
			propList.Add(new StringOntologyProperty("roh:crisIdentifier", this.Roh_crisIdentifier));
			propList.Add(new BoolOntologyProperty("roh:congressProceedingsPublication", this.Roh_congressProceedingsPublication));
			propList.Add(new ListStringOntologyProperty("vcard:url", this.Vcard_url));
			propList.Add(new StringOntologyProperty("bibo:pageEnd", this.Bibo_pageEnd.ToString()));
			propList.Add(new StringOntologyProperty("roh:collection", this.Roh_collection));
			propList.Add(new ListStringOntologyProperty("roh:userKeywords", this.Roh_userKeywords));
			propList.Add(new StringOntologyProperty("roh:title", this.Roh_title));
			propList.Add(new StringOntologyProperty("roh:scientificActivityDocument", this.IdRoh_scientificActivityDocument));
			propList.Add(new BoolOntologyProperty("roh:isValidated", this.Roh_isValidated));
			propList.Add(new StringOntologyProperty("roh:citationLoadedCount", this.Roh_citationLoadedCount.ToString()));
			propList.Add(new StringOntologyProperty("roh:citationCount", this.Roh_citationCount.ToString()));
			propList.Add(new StringOntologyProperty("roh:referencesLoadedCount", this.Roh_referencesLoadedCount.ToString()));
			propList.Add(new BoolOntologyProperty("roh:isPublic", this.Roh_isPublic));
		}

		internal override void GetEntities()
		{
			base.GetEntities();
			if(Roh_suggestedKnowledgeArea!=null){
				foreach(CategoryPath prop in Roh_suggestedKnowledgeArea){
					prop.GetProperties();
					prop.GetEntities();
					OntologyEntity entityCategoryPath = new OntologyEntity("http://w3id.org/roh/CategoryPath", "http://w3id.org/roh/CategoryPath", "roh:suggestedKnowledgeArea", prop.propList, prop.entList);
				entList.Add(entityCategoryPath);
				prop.Entity= entityCategoryPath;
				}
			}
			if(Bibo_authorList!=null){
				foreach(BFO_0000023 prop in Bibo_authorList){
					prop.GetProperties();
					prop.GetEntities();
					OntologyEntity entityBFO_0000023 = new OntologyEntity("http://purl.obolibrary.org/obo/BFO_0000023", "http://purl.obolibrary.org/obo/BFO_0000023", "bibo:authorList", prop.propList, prop.entList);
				entList.Add(entityBFO_0000023);
				prop.Entity= entityBFO_0000023;
				}
			}
			if(Roh_externalKnowledgeArea!=null){
				foreach(CategoryPath prop in Roh_externalKnowledgeArea){
					prop.GetProperties();
					prop.GetEntities();
					OntologyEntity entityCategoryPath = new OntologyEntity("http://w3id.org/roh/CategoryPath", "http://w3id.org/roh/CategoryPath", "roh:externalKnowledgeArea", prop.propList, prop.entList);
				entList.Add(entityCategoryPath);
				prop.Entity= entityCategoryPath;
				}
			}
			if(Roh_userKnowledgeArea!=null){
				foreach(CategoryPath prop in Roh_userKnowledgeArea){
					prop.GetProperties();
					prop.GetEntities();
					OntologyEntity entityCategoryPath = new OntologyEntity("http://w3id.org/roh/CategoryPath", "http://w3id.org/roh/CategoryPath", "roh:userKnowledgeArea", prop.propList, prop.entList);
				entList.Add(entityCategoryPath);
				prop.Entity= entityCategoryPath;
				}
			}
			if(Bibo_presentedAt!=null){
				Bibo_presentedAt.GetProperties();
				Bibo_presentedAt.GetEntities();
				OntologyEntity entityBibo_presentedAt = new OntologyEntity("http://purl.org/ontology/bibo/Event", "http://purl.org/ontology/bibo/Event", "bibo:presentedAt", Bibo_presentedAt.propList, Bibo_presentedAt.entList);
				entList.Add(entityBibo_presentedAt);
			}
			if(Bibo_identifier!=null){
				foreach(fDocument prop in Bibo_identifier){
					prop.GetProperties();
					prop.GetEntities();
					OntologyEntity entityDocument = new OntologyEntity("http://xmlns.com/foaf/0.1/Document", "http://xmlns.com/foaf/0.1/Document", "bibo:identifier", prop.propList, prop.entList);
				entList.Add(entityDocument);
				prop.Entity= entityDocument;
				}
			}
			if(Roh_hasMetric!=null){
				foreach(PublicationMetric prop in Roh_hasMetric){
					prop.GetProperties();
					prop.GetEntities();
					OntologyEntity entityPublicationMetric = new OntologyEntity("http://w3id.org/roh/PublicationMetric", "http://w3id.org/roh/PublicationMetric", "roh:hasMetric", prop.propList, prop.entList);
				entList.Add(entityPublicationMetric);
				prop.Entity= entityPublicationMetric;
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
			if(Roh_enrichedKnowledgeArea!=null){
				foreach(CategoryPath prop in Roh_enrichedKnowledgeArea){
					prop.GetProperties();
					prop.GetEntities();
					OntologyEntity entityCategoryPath = new OntologyEntity("http://w3id.org/roh/CategoryPath", "http://w3id.org/roh/CategoryPath", "roh:enrichedKnowledgeArea", prop.propList, prop.entList);
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
			AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Document_{ResourceID}_{ArticleID}", "http://www.w3.org/1999/02/22-rdf-syntax-ns#type", $"<http://purl.org/ontology/bibo/Document>", list, " . ");
			AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Document_{ResourceID}_{ArticleID}", "http://www.w3.org/2000/01/rdf-schema#label", $"\"http://purl.org/ontology/bibo/Document\"", list, " . ");
			AgregarTripleALista($"{resourceAPI.GraphsUrl}{ResourceID}", "http://gnoss/hasEntidad", $"<{resourceAPI.GraphsUrl}items/Document_{ResourceID}_{ArticleID}>", list, " . ");
			if(this.Roh_suggestedKnowledgeArea != null)
			{
			foreach(var item0 in this.Roh_suggestedKnowledgeArea)
			{
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/CategoryPath_{ResourceID}_{item0.ArticleID}", "http://www.w3.org/1999/02/22-rdf-syntax-ns#type", $"<http://w3id.org/roh/CategoryPath>", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/CategoryPath_{ResourceID}_{item0.ArticleID}", "http://www.w3.org/2000/01/rdf-schema#label", $"\"http://w3id.org/roh/CategoryPath\"", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}{ResourceID}", "http://gnoss/hasEntidad", $"<{resourceAPI.GraphsUrl}items/CategoryPath_{ResourceID}_{item0.ArticleID}>", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Document_{ResourceID}_{ArticleID}", "http://w3id.org/roh/suggestedKnowledgeArea", $"<{resourceAPI.GraphsUrl}items/CategoryPath_{ResourceID}_{item0.ArticleID}>", list, " . ");
				if(item0.IdsRoh_categoryNode != null)
				{
					foreach(var item2 in item0.IdsRoh_categoryNode)
					{
						AgregarTripleALista($"{resourceAPI.GraphsUrl}items/CategoryPath_{ResourceID}_{item0.ArticleID}", "http://w3id.org/roh/categoryNode", $"<{item2}>", list, " . ");
					}
				}
			}
			}
			if(this.Roh_externalKnowledgeArea != null)
			{
			foreach(var item0 in this.Roh_externalKnowledgeArea)
			{
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/CategoryPath_{ResourceID}_{item0.ArticleID}", "http://www.w3.org/1999/02/22-rdf-syntax-ns#type", $"<http://w3id.org/roh/CategoryPath>", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/CategoryPath_{ResourceID}_{item0.ArticleID}", "http://www.w3.org/2000/01/rdf-schema#label", $"\"http://w3id.org/roh/CategoryPath\"", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}{ResourceID}", "http://gnoss/hasEntidad", $"<{resourceAPI.GraphsUrl}items/CategoryPath_{ResourceID}_{item0.ArticleID}>", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Document_{ResourceID}_{ArticleID}", "http://w3id.org/roh/externalKnowledgeArea", $"<{resourceAPI.GraphsUrl}items/CategoryPath_{ResourceID}_{item0.ArticleID}>", list, " . ");
				if(item0.IdsRoh_categoryNode != null)
				{
					foreach(var item2 in item0.IdsRoh_categoryNode)
					{
						AgregarTripleALista($"{resourceAPI.GraphsUrl}items/CategoryPath_{ResourceID}_{item0.ArticleID}", "http://w3id.org/roh/categoryNode", $"<{item2}>", list, " . ");
					}
				}
			}
			}
			if(this.Roh_userKnowledgeArea != null)
			{
			foreach(var item0 in this.Roh_userKnowledgeArea)
			{
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/CategoryPath_{ResourceID}_{item0.ArticleID}", "http://www.w3.org/1999/02/22-rdf-syntax-ns#type", $"<http://w3id.org/roh/CategoryPath>", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/CategoryPath_{ResourceID}_{item0.ArticleID}", "http://www.w3.org/2000/01/rdf-schema#label", $"\"http://w3id.org/roh/CategoryPath\"", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}{ResourceID}", "http://gnoss/hasEntidad", $"<{resourceAPI.GraphsUrl}items/CategoryPath_{ResourceID}_{item0.ArticleID}>", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Document_{ResourceID}_{ArticleID}", "http://w3id.org/roh/userKnowledgeArea", $"<{resourceAPI.GraphsUrl}items/CategoryPath_{ResourceID}_{item0.ArticleID}>", list, " . ");
				if(item0.IdsRoh_categoryNode != null)
				{
					foreach(var item2 in item0.IdsRoh_categoryNode)
					{
						AgregarTripleALista($"{resourceAPI.GraphsUrl}items/CategoryPath_{ResourceID}_{item0.ArticleID}", "http://w3id.org/roh/categoryNode", $"<{item2}>", list, " . ");
					}
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
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Document_{ResourceID}_{ArticleID}", "http://w3id.org/roh/hasKnowledgeArea", $"<{resourceAPI.GraphsUrl}items/CategoryPath_{ResourceID}_{item0.ArticleID}>", list, " . ");
				if(item0.IdsRoh_categoryNode != null)
				{
					foreach(var item2 in item0.IdsRoh_categoryNode)
					{
						AgregarTripleALista($"{resourceAPI.GraphsUrl}items/CategoryPath_{ResourceID}_{item0.ArticleID}", "http://w3id.org/roh/categoryNode", $"<{item2}>", list, " . ");
					}
				}
			}
			}
			if(this.Roh_enrichedKnowledgeArea != null)
			{
			foreach(var item0 in this.Roh_enrichedKnowledgeArea)
			{
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/CategoryPath_{ResourceID}_{item0.ArticleID}", "http://www.w3.org/1999/02/22-rdf-syntax-ns#type", $"<http://w3id.org/roh/CategoryPath>", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/CategoryPath_{ResourceID}_{item0.ArticleID}", "http://www.w3.org/2000/01/rdf-schema#label", $"\"http://w3id.org/roh/CategoryPath\"", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}{ResourceID}", "http://gnoss/hasEntidad", $"<{resourceAPI.GraphsUrl}items/CategoryPath_{ResourceID}_{item0.ArticleID}>", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Document_{ResourceID}_{ArticleID}", "http://w3id.org/roh/enrichedKnowledgeArea", $"<{resourceAPI.GraphsUrl}items/CategoryPath_{ResourceID}_{item0.ArticleID}>", list, " . ");
				if(item0.IdsRoh_categoryNode != null)
				{
					foreach(var item2 in item0.IdsRoh_categoryNode)
					{
						AgregarTripleALista($"{resourceAPI.GraphsUrl}items/CategoryPath_{ResourceID}_{item0.ArticleID}", "http://w3id.org/roh/categoryNode", $"<{item2}>", list, " . ");
					}
				}
			}
			}
			if(this.Bibo_authorList != null)
			{
			foreach(var item0 in this.Bibo_authorList)
			{
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/BFO_0000023_{ResourceID}_{item0.ArticleID}", "http://www.w3.org/1999/02/22-rdf-syntax-ns#type", $"<http://purl.obolibrary.org/obo/BFO_0000023>", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/BFO_0000023_{ResourceID}_{item0.ArticleID}", "http://www.w3.org/2000/01/rdf-schema#label", $"\"http://purl.obolibrary.org/obo/BFO_0000023\"", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}{ResourceID}", "http://gnoss/hasEntidad", $"<{resourceAPI.GraphsUrl}items/BFO_0000023_{ResourceID}_{item0.ArticleID}>", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Document_{ResourceID}_{ArticleID}", "http://purl.org/ontology/bibo/authorList", $"<{resourceAPI.GraphsUrl}items/BFO_0000023_{ResourceID}_{item0.ArticleID}>", list, " . ");
				if(item0.Foaf_nick != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/BFO_0000023_{ResourceID}_{item0.ArticleID}",  "http://xmlns.com/foaf/0.1/nick", $"\"{GenerarTextoSinSaltoDeLinea(item0.Foaf_nick)}\"", list, " . ");
				}
				if(item0.Rdf_comment != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/BFO_0000023_{ResourceID}_{item0.ArticleID}",  "http://www.w3.org/1999/02/22-rdf-syntax-ns#comment", $"{item0.Rdf_comment.ToString()}", list, " . ");
				}
				if(item0.IdRdf_member != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/BFO_0000023_{ResourceID}_{item0.ArticleID}",  "http://www.w3.org/1999/02/22-rdf-syntax-ns#member", $"<{item0.IdRdf_member}>", list, " . ");
				}
			}
			}
			if(this.Bibo_presentedAt != null)
			{
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Event_{ResourceID}_{this.Bibo_presentedAt.ArticleID}", "http://www.w3.org/1999/02/22-rdf-syntax-ns#type", $"<http://purl.org/ontology/bibo/Event>", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Event_{ResourceID}_{this.Bibo_presentedAt.ArticleID}", "http://www.w3.org/2000/01/rdf-schema#label", $"\"http://purl.org/ontology/bibo/Event\"", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}{ResourceID}", "http://gnoss/hasEntidad", $"<{resourceAPI.GraphsUrl}items/Event_{ResourceID}_{this.Bibo_presentedAt.ArticleID}>", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Document_{ResourceID}_{ArticleID}", "http://purl.org/ontology/bibo/presentedAt", $"<{resourceAPI.GraphsUrl}items/Event_{ResourceID}_{this.Bibo_presentedAt.ArticleID}>", list, " . ");
				if(this.Bibo_presentedAt.IdBibo_organizer != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Event_{ResourceID}_{this.Bibo_presentedAt.ArticleID}",  "http://purl.org/ontology/bibo/organizer", $"<{this.Bibo_presentedAt.IdBibo_organizer}>", list, " . ");
				}
				if(this.Bibo_presentedAt.IdVcard_hasCountryName != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Event_{ResourceID}_{this.Bibo_presentedAt.ArticleID}",  "https://www.w3.org/2006/vcard/ns#hasCountryName", $"<{this.Bibo_presentedAt.IdVcard_hasCountryName}>", list, " . ");
				}
				if(this.Bibo_presentedAt.IdVivo_geographicFocus != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Event_{ResourceID}_{this.Bibo_presentedAt.ArticleID}",  "http://vivoweb.org/ontology/core#geographicFocus", $"<{this.Bibo_presentedAt.IdVivo_geographicFocus}>", list, " . ");
				}
				if(this.Bibo_presentedAt.IdsDc_type != null)
				{
					foreach(var item2 in this.Bibo_presentedAt.IdsDc_type)
					{
						AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Event_{ResourceID}_{this.Bibo_presentedAt.ArticleID}", "http://purl.org/dc/elements/1.1/type", $"<{item2}>", list, " . ");
					}
				}
				if(this.Bibo_presentedAt.IdVcard_hasRegion != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Event_{ResourceID}_{this.Bibo_presentedAt.ArticleID}",  "https://www.w3.org/2006/vcard/ns#hasRegion", $"<{this.Bibo_presentedAt.IdVcard_hasRegion}>", list, " . ");
				}
				if(this.Bibo_presentedAt.Roh_geographicFocusOther != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Event_{ResourceID}_{this.Bibo_presentedAt.ArticleID}",  "http://w3id.org/roh/geographicFocusOther", $"\"{GenerarTextoSinSaltoDeLinea(this.Bibo_presentedAt.Roh_geographicFocusOther)}\"", list, " . ");
				}
				if(this.Bibo_presentedAt.Vivo_start != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Event_{ResourceID}_{this.Bibo_presentedAt.ArticleID}",  "http://vivoweb.org/ontology/core#start", $"\"{this.Bibo_presentedAt.Vivo_start.Value.ToString("yyyyMMddHHmmss")}\"", list, " . ");
				}
				if(this.Bibo_presentedAt.Vcard_locality != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Event_{ResourceID}_{this.Bibo_presentedAt.ArticleID}",  "https://www.w3.org/2006/vcard/ns#locality", $"\"{GenerarTextoSinSaltoDeLinea(this.Bibo_presentedAt.Vcard_locality)}\"", list, " . ");
				}
				if(this.Bibo_presentedAt.Roh_withExternalAdmissionsCommittee != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Event_{ResourceID}_{this.Bibo_presentedAt.ArticleID}",  "http://w3id.org/roh/withExternalAdmissionsCommittee", $"\"{this.Bibo_presentedAt.Roh_withExternalAdmissionsCommittee.ToString()}\"", list, " . ");
				}
				if(this.Bibo_presentedAt.Vivo_end != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Event_{ResourceID}_{this.Bibo_presentedAt.ArticleID}",  "http://vivoweb.org/ontology/core#end", $"\"{this.Bibo_presentedAt.Vivo_end.Value.ToString("yyyyMMddHHmmss")}\"", list, " . ");
				}
				if(this.Bibo_presentedAt.Dc_title != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Event_{ResourceID}_{this.Bibo_presentedAt.ArticleID}",  "http://purl.org/dc/elements/1.1/title", $"\"{GenerarTextoSinSaltoDeLinea(this.Bibo_presentedAt.Dc_title)}\"", list, " . ");
				}
			}
			if(this.Bibo_identifier != null)
			{
			foreach(var item0 in this.Bibo_identifier)
			{
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Document_{ResourceID}_{item0.ArticleID}", "http://www.w3.org/1999/02/22-rdf-syntax-ns#type", $"<http://xmlns.com/foaf/0.1/Document>", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Document_{ResourceID}_{item0.ArticleID}", "http://www.w3.org/2000/01/rdf-schema#label", $"\"http://xmlns.com/foaf/0.1/Document\"", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}{ResourceID}", "http://gnoss/hasEntidad", $"<{resourceAPI.GraphsUrl}items/Document_{ResourceID}_{item0.ArticleID}>", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Document_{ResourceID}_{ArticleID}", "http://purl.org/ontology/bibo/identifier", $"<{resourceAPI.GraphsUrl}items/Document_{ResourceID}_{item0.ArticleID}>", list, " . ");
				if(item0.Dc_title != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Document_{ResourceID}_{item0.ArticleID}",  "http://purl.org/dc/elements/1.1/title", $"\"{GenerarTextoSinSaltoDeLinea(item0.Dc_title)}\"", list, " . ");
				}
				if(item0.Foaf_topic != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Document_{ResourceID}_{item0.ArticleID}",  "http://xmlns.com/foaf/0.1/topic", $"\"{GenerarTextoSinSaltoDeLinea(item0.Foaf_topic)}\"", list, " . ");
				}
			}
			}
			if(this.Roh_hasMetric != null)
			{
			foreach(var item0 in this.Roh_hasMetric)
			{
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/PublicationMetric_{ResourceID}_{item0.ArticleID}", "http://www.w3.org/1999/02/22-rdf-syntax-ns#type", $"<http://w3id.org/roh/PublicationMetric>", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/PublicationMetric_{ResourceID}_{item0.ArticleID}", "http://www.w3.org/2000/01/rdf-schema#label", $"\"http://w3id.org/roh/PublicationMetric\"", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}{ResourceID}", "http://gnoss/hasEntidad", $"<{resourceAPI.GraphsUrl}items/PublicationMetric_{ResourceID}_{item0.ArticleID}>", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Document_{ResourceID}_{ArticleID}", "http://w3id.org/roh/hasMetric", $"<{resourceAPI.GraphsUrl}items/PublicationMetric_{ResourceID}_{item0.ArticleID}>", list, " . ");
				if(item0.Roh_metricName != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/PublicationMetric_{ResourceID}_{item0.ArticleID}",  "http://w3id.org/roh/metricName", $"\"{GenerarTextoSinSaltoDeLinea(item0.Roh_metricName)}\"", list, " . ");
				}
				if(item0.Roh_citationCount != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/PublicationMetric_{ResourceID}_{item0.ArticleID}",  "http://w3id.org/roh/citationCount", $"{item0.Roh_citationCount.ToString()}", list, " . ");
				}
			}
			}
				if(this.IdVcard_hasCountryName != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Document_{ResourceID}_{ArticleID}",  "https://www.w3.org/2006/vcard/ns#hasCountryName", $"<{this.IdVcard_hasCountryName}>", list, " . ");
				}
				if(this.IdsVcard_hasLanguage != null)
				{
					foreach(var item2 in this.IdsVcard_hasLanguage)
					{
						AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Document_{ResourceID}_{ArticleID}", "https://www.w3.org/2006/vcard/ns#hasLanguage", $"<{item2}>", list, " . ");
					}
				}
				if(this.IdRoh_project != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Document_{ResourceID}_{ArticleID}",  "http://w3id.org/roh/project", $"<{this.IdRoh_project}>", list, " . ");
				}
				if(this.IdsRoh_isProducedBy != null)
				{
					foreach(var item2 in this.IdsRoh_isProducedBy)
					{
						AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Document_{ResourceID}_{ArticleID}", "http://w3id.org/roh/isProducedBy", $"<{item2}>", list, " . ");
					}
				}
				if(this.IdsRoh_correspondingAuthor != null)
				{
					foreach(var item2 in this.IdsRoh_correspondingAuthor)
					{
						AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Document_{ResourceID}_{ArticleID}", "http://w3id.org/roh/correspondingAuthor", $"<{item2}>", list, " . ");
					}
				}
				if(this.IdDc_type != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Document_{ResourceID}_{ArticleID}",  "http://purl.org/dc/elements/1.1/type", $"<{this.IdDc_type}>", list, " . ");
				}
				if(this.IdsRoh_publicGroupList != null)
				{
					foreach(var item2 in this.IdsRoh_publicGroupList)
					{
						AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Document_{ResourceID}_{ArticleID}", "http://w3id.org/roh/publicGroupList", $"<{item2}>", list, " . ");
					}
				}
				if(this.IdsRoh_publicAuthorList != null)
				{
					foreach(var item2 in this.IdsRoh_publicAuthorList)
					{
						AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Document_{ResourceID}_{ArticleID}", "http://w3id.org/roh/publicAuthorList", $"<{item2}>", list, " . ");
					}
				}
				if(this.IdVcard_hasRegion != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Document_{ResourceID}_{ArticleID}",  "https://www.w3.org/2006/vcard/ns#hasRegion", $"<{this.IdVcard_hasRegion}>", list, " . ");
				}
				if(this.IdsBibo_cites != null)
				{
					foreach(var item2 in this.IdsBibo_cites)
					{
						AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Document_{ResourceID}_{ArticleID}", "http://purl.org/ontology/bibo/cites", $"<{item2}>", list, " . ");
					}
				}
				if(this.IdVivo_hasPublicationVenue != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Document_{ResourceID}_{ArticleID}",  "http://vivoweb.org/ontology/core#hasPublicationVenue", $"<{this.IdVivo_hasPublicationVenue}>", list, " . ");
				}
				if(this.Roh_legalDeposit != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Document_{ResourceID}_{ArticleID}",  "http://w3id.org/roh/legalDeposit", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_legalDeposit)}\"", list, " . ");
				}
				if(this.Roh_scopus != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Document_{ResourceID}_{ArticleID}",  "http://w3id.org/roh/scopus", $"{this.Roh_scopus.Value.ToString()}", list, " . ");
				}
				if(this.Roh_assessmentStatus != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Document_{ResourceID}_{ArticleID}",  "http://w3id.org/roh/assessmentStatus", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_assessmentStatus)}\"", list, " . ");
				}
				if(this.Roh_publicationTitle != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Document_{ResourceID}_{ArticleID}",  "http://w3id.org/roh/publicationTitle", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_publicationTitle)}\"", list, " . ");
				}
				if(this.Vcard_locality != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Document_{ResourceID}_{ArticleID}",  "https://www.w3.org/2006/vcard/ns#locality", $"\"{GenerarTextoSinSaltoDeLinea(this.Vcard_locality)}\"", list, " . ");
				}
				if(this.Roh_authorsNumber != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Document_{ResourceID}_{ArticleID}",  "http://w3id.org/roh/authorsNumber", $"{this.Roh_authorsNumber.Value.ToString()}", list, " . ");
				}
				if(this.Roh_inrecs != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Document_{ResourceID}_{ArticleID}",  "http://w3id.org/roh/inrecs", $"{this.Roh_inrecs.Value.ToString()}", list, " . ");
				}
				if(this.Roh_externalKeywords != null)
				{
					foreach(var item2 in this.Roh_externalKeywords)
					{
						AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Document_{ResourceID}_{ArticleID}", "http://w3id.org/roh/externalKeywords", $"\"{GenerarTextoSinSaltoDeLinea(item2)}\"", list, " . ");
					}
				}
				if(this.Roh_reviewsNumber != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Document_{ResourceID}_{ArticleID}",  "http://w3id.org/roh/reviewsNumber", $"{this.Roh_reviewsNumber.Value.ToString()}", list, " . ");
				}
				if(this.Roh_userJournal != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Document_{ResourceID}_{ArticleID}",  "http://w3id.org/roh/userJournal", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_userJournal)}\"", list, " . ");
				}
				if(this.Bibo_abstract != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Document_{ResourceID}_{ArticleID}",  "http://purl.org/ontology/bibo/abstract", $"\"{GenerarTextoSinSaltoDeLinea(this.Bibo_abstract)}\"", list, " . ");
				}
				if(this.Bibo_pmid != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Document_{ResourceID}_{ArticleID}",  "http://purl.org/ontology/bibo/pmid", $"\"{GenerarTextoSinSaltoDeLinea(this.Bibo_pmid)}\"", list, " . ");
				}
				if(this.Vivo_freeTextKeyword != null)
				{
					foreach(var item2 in this.Vivo_freeTextKeyword)
					{
						AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Document_{ResourceID}_{ArticleID}", "http://vivoweb.org/ontology/core#freeTextKeyword", $"\"{GenerarTextoSinSaltoDeLinea(item2)}\"", list, " . ");
					}
				}
				if(this.Bibo_handle != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Document_{ResourceID}_{ArticleID}",  "http://purl.org/ontology/bibo/handle", $"\"{GenerarTextoSinSaltoDeLinea(this.Bibo_handle)}\"", list, " . ");
				}
				if(this.Bibo_doi != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Document_{ResourceID}_{ArticleID}",  "http://purl.org/ontology/bibo/doi", $"\"{GenerarTextoSinSaltoDeLinea(this.Bibo_doi)}\"", list, " . ");
				}
				if(this.Roh_wos != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Document_{ResourceID}_{ArticleID}",  "http://w3id.org/roh/wos", $"{this.Roh_wos.Value.ToString()}", list, " . ");
				}
				if(this.Bibo_issue != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Document_{ResourceID}_{ArticleID}",  "http://purl.org/ontology/bibo/issue", $"\"{GenerarTextoSinSaltoDeLinea(this.Bibo_issue)}\"", list, " . ");
				}
				if(this.Roh_suggestedKeywords != null)
				{
					foreach(var item2 in this.Roh_suggestedKeywords)
					{
						AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Document_{ResourceID}_{ArticleID}", "http://w3id.org/roh/suggestedKeywords", $"\"{GenerarTextoSinSaltoDeLinea(item2)}\"", list, " . ");
					}
				}
				if(this.Bibo_volume != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Document_{ResourceID}_{ArticleID}",  "http://purl.org/ontology/bibo/volume", $"\"{GenerarTextoSinSaltoDeLinea(this.Bibo_volume)}\"", list, " . ");
				}
				if(this.Roh_enrichedKeywords != null)
				{
					foreach(var item2 in this.Roh_enrichedKeywords)
					{
						AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Document_{ResourceID}_{ArticleID}", "http://w3id.org/roh/enrichedKeywords", $"\"{GenerarTextoSinSaltoDeLinea(item2)}\"", list, " . ");
					}
				}
				if(this.Dct_issued != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Document_{ResourceID}_{ArticleID}",  "http://purl.org/dc/terms/issued", $"\"{this.Dct_issued.Value.ToString("yyyyMMddHHmmss")}\"", list, " . ");
				}
				if(this.Roh_typeOthers != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Document_{ResourceID}_{ArticleID}",  "http://w3id.org/roh/typeOthers", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_typeOthers)}\"", list, " . ");
				}
				if(this.Bibo_pageStart != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Document_{ResourceID}_{ArticleID}",  "http://purl.org/ontology/bibo/pageStart", $"{this.Bibo_pageStart.Value.ToString()}", list, " . ");
				}
				if(this.Roh_crisIdentifier != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Document_{ResourceID}_{ArticleID}",  "http://w3id.org/roh/crisIdentifier", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_crisIdentifier)}\"", list, " . ");
				}
				if(this.Roh_congressProceedingsPublication != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Document_{ResourceID}_{ArticleID}",  "http://w3id.org/roh/congressProceedingsPublication", $"\"{this.Roh_congressProceedingsPublication.ToString()}\"", list, " . ");
				}
				if(this.Vcard_url != null)
				{
					foreach(var item2 in this.Vcard_url)
					{
						AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Document_{ResourceID}_{ArticleID}", "https://www.w3.org/2006/vcard/ns#url", $"\"{GenerarTextoSinSaltoDeLinea(item2)}\"", list, " . ");
					}
				}
				if(this.Bibo_pageEnd != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Document_{ResourceID}_{ArticleID}",  "http://purl.org/ontology/bibo/pageEnd", $"{this.Bibo_pageEnd.Value.ToString()}", list, " . ");
				}
				if(this.Roh_collection != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Document_{ResourceID}_{ArticleID}",  "http://w3id.org/roh/collection", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_collection)}\"", list, " . ");
				}
				if(this.Roh_userKeywords != null)
				{
					foreach(var item2 in this.Roh_userKeywords)
					{
						AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Document_{ResourceID}_{ArticleID}", "http://w3id.org/roh/userKeywords", $"\"{GenerarTextoSinSaltoDeLinea(item2)}\"", list, " . ");
					}
				}
				if(this.Roh_title != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Document_{ResourceID}_{ArticleID}",  "http://w3id.org/roh/title", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_title)}\"", list, " . ");
				}
				if(this.IdRoh_scientificActivityDocument != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Document_{ResourceID}_{ArticleID}",  "http://w3id.org/roh/scientificActivityDocument", $"<{this.IdRoh_scientificActivityDocument}>", list, " . ");
				}
				if(this.Roh_isValidated != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Document_{ResourceID}_{ArticleID}",  "http://w3id.org/roh/isValidated", $"\"{this.Roh_isValidated.ToString()}\"", list, " . ");
				}
				if(this.Roh_citationLoadedCount != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Document_{ResourceID}_{ArticleID}",  "http://w3id.org/roh/citationLoadedCount", $"{this.Roh_citationLoadedCount.ToString()}", list, " . ");
				}
				if(this.Roh_citationCount != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Document_{ResourceID}_{ArticleID}",  "http://w3id.org/roh/citationCount", $"{this.Roh_citationCount.ToString()}", list, " . ");
				}
				if(this.Roh_referencesLoadedCount != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Document_{ResourceID}_{ArticleID}",  "http://w3id.org/roh/referencesLoadedCount", $"{this.Roh_referencesLoadedCount.ToString()}", list, " . ");
				}
				if(this.Roh_isPublic != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Document_{ResourceID}_{ArticleID}",  "http://w3id.org/roh/isPublic", $"\"{this.Roh_isPublic.ToString()}\"", list, " . ");
				}
			return list;
		}

		public override List<string> ToSearchGraphTriples(ResourceApi resourceAPI)
		{
			List<string> list = new List<string>();
			List<string> listaSearch = new List<string>();
			AgregarTags(list);
			AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}", "http://www.w3.org/1999/02/22-rdf-syntax-ns#type", $"\"document\"", list, " . ");
			AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}", "http://gnoss/type", $"\"http://purl.org/ontology/bibo/Document\"", list, " . ");
			AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}", "http://gnoss/hasfechapublicacion", $"{DateTime.Now.ToString("yyyyMMddHHmmss")}", list, " . ");
			AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}", "http://gnoss/hastipodoc", "\"5\"", list, " . ");
			AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}", "http://gnoss/hasfechamodificacion", $"{DateTime.Now.ToString("yyyyMMddHHmmss")}", list, " . ");
			AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}", "http://gnoss/hasnumeroVisitas", "0", list, " . ");
			AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}", "http://gnoss/hasprivacidadCom", "\"publico\"", list, " . ");
			AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}", "http://xmlns.com/foaf/0.1/firstName", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_title)}\"", list, " . ");
			AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}", "http://gnoss/hasnombrecompleto", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_title)}\"", list, " . ");
			string search = string.Empty;
			if(this.Roh_suggestedKnowledgeArea != null)
			{
			foreach(var item0 in this.Roh_suggestedKnowledgeArea)
			{
				AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}", "http://w3id.org/roh/suggestedKnowledgeArea", $"<{resourceAPI.GraphsUrl}items/categorypath_{ResourceID}_{item0.ArticleID}>", list, " . ");
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
						AgregarTripleALista($"{resourceAPI.GraphsUrl}items/categorypath_{ResourceID}_{item0.ArticleID}", "http://w3id.org/roh/categoryNode", $"<{itemRegex}>", list, " . ");
					}
				}
			}
			}
			if(this.Roh_externalKnowledgeArea != null)
			{
			foreach(var item0 in this.Roh_externalKnowledgeArea)
			{
				AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}", "http://w3id.org/roh/externalKnowledgeArea", $"<{resourceAPI.GraphsUrl}items/categorypath_{ResourceID}_{item0.ArticleID}>", list, " . ");
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
						AgregarTripleALista($"{resourceAPI.GraphsUrl}items/categorypath_{ResourceID}_{item0.ArticleID}", "http://w3id.org/roh/categoryNode", $"<{itemRegex}>", list, " . ");
					}
				}
			}
			}
			if(this.Roh_userKnowledgeArea != null)
			{
			foreach(var item0 in this.Roh_userKnowledgeArea)
			{
				AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}", "http://w3id.org/roh/userKnowledgeArea", $"<{resourceAPI.GraphsUrl}items/categorypath_{ResourceID}_{item0.ArticleID}>", list, " . ");
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
						AgregarTripleALista($"{resourceAPI.GraphsUrl}items/categorypath_{ResourceID}_{item0.ArticleID}", "http://w3id.org/roh/categoryNode", $"<{itemRegex}>", list, " . ");
					}
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
						AgregarTripleALista($"{resourceAPI.GraphsUrl}items/categorypath_{ResourceID}_{item0.ArticleID}", "http://w3id.org/roh/categoryNode", $"<{itemRegex}>", list, " . ");
					}
				}
			}
			}
			if(this.Roh_enrichedKnowledgeArea != null)
			{
			foreach(var item0 in this.Roh_enrichedKnowledgeArea)
			{
				AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}", "http://w3id.org/roh/enrichedKnowledgeArea", $"<{resourceAPI.GraphsUrl}items/categorypath_{ResourceID}_{item0.ArticleID}>", list, " . ");
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
						AgregarTripleALista($"{resourceAPI.GraphsUrl}items/categorypath_{ResourceID}_{item0.ArticleID}", "http://w3id.org/roh/categoryNode", $"<{itemRegex}>", list, " . ");
					}
				}
			}
			}
			if(this.Bibo_authorList != null)
			{
			foreach(var item0 in this.Bibo_authorList)
			{
				AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}", "http://purl.org/ontology/bibo/authorList", $"<{resourceAPI.GraphsUrl}items/bfo_0000023_{ResourceID}_{item0.ArticleID}>", list, " . ");
				if(item0.Foaf_nick != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/bfo_0000023_{ResourceID}_{item0.ArticleID}",  "http://xmlns.com/foaf/0.1/nick", $"\"{GenerarTextoSinSaltoDeLinea(item0.Foaf_nick).ToLower()}\"", list, " . ");
				}
				if(item0.Rdf_comment != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/bfo_0000023_{ResourceID}_{item0.ArticleID}",  "http://www.w3.org/1999/02/22-rdf-syntax-ns#comment", $"{item0.Rdf_comment.ToString()}", list, " . ");
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
			}
			}
			if(this.Bibo_presentedAt != null)
			{
				AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}", "http://purl.org/ontology/bibo/presentedAt", $"<{resourceAPI.GraphsUrl}items/event_{ResourceID}_{this.Bibo_presentedAt.ArticleID}>", list, " . ");
				if(this.Bibo_presentedAt.IdBibo_organizer != null)
				{
					Regex regex = new Regex(@"\/items\/.+_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}");
					string itemRegex = this.Bibo_presentedAt.IdBibo_organizer;
					if (regex.IsMatch(itemRegex))
					{
						itemRegex = $"http://gnoss/{resourceAPI.GetShortGuid(itemRegex).ToString().ToUpper()}";
					}
					else
					{
						itemRegex = itemRegex.ToLower();
					}
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/event_{ResourceID}_{this.Bibo_presentedAt.ArticleID}",  "http://purl.org/ontology/bibo/organizer", $"<{itemRegex}>", list, " . ");
				}
				if(this.Bibo_presentedAt.IdVcard_hasCountryName != null)
				{
					Regex regex = new Regex(@"\/items\/.+_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}");
					string itemRegex = this.Bibo_presentedAt.IdVcard_hasCountryName;
					if (regex.IsMatch(itemRegex))
					{
						itemRegex = $"http://gnoss/{resourceAPI.GetShortGuid(itemRegex).ToString().ToUpper()}";
					}
					else
					{
						itemRegex = itemRegex.ToLower();
					}
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/event_{ResourceID}_{this.Bibo_presentedAt.ArticleID}",  "https://www.w3.org/2006/vcard/ns#hasCountryName", $"<{itemRegex}>", list, " . ");
				}
				if(this.Bibo_presentedAt.IdVivo_geographicFocus != null)
				{
					Regex regex = new Regex(@"\/items\/.+_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}");
					string itemRegex = this.Bibo_presentedAt.IdVivo_geographicFocus;
					if (regex.IsMatch(itemRegex))
					{
						itemRegex = $"http://gnoss/{resourceAPI.GetShortGuid(itemRegex).ToString().ToUpper()}";
					}
					else
					{
						itemRegex = itemRegex.ToLower();
					}
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/event_{ResourceID}_{this.Bibo_presentedAt.ArticleID}",  "http://vivoweb.org/ontology/core#geographicFocus", $"<{itemRegex}>", list, " . ");
				}
				if(this.Bibo_presentedAt.IdsDc_type != null)
				{
					foreach(var item2 in this.Bibo_presentedAt.IdsDc_type)
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
						AgregarTripleALista($"{resourceAPI.GraphsUrl}items/event_{ResourceID}_{this.Bibo_presentedAt.ArticleID}", "http://purl.org/dc/elements/1.1/type", $"<{itemRegex}>", list, " . ");
					}
				}
				if(this.Bibo_presentedAt.IdVcard_hasRegion != null)
				{
					Regex regex = new Regex(@"\/items\/.+_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}");
					string itemRegex = this.Bibo_presentedAt.IdVcard_hasRegion;
					if (regex.IsMatch(itemRegex))
					{
						itemRegex = $"http://gnoss/{resourceAPI.GetShortGuid(itemRegex).ToString().ToUpper()}";
					}
					else
					{
						itemRegex = itemRegex.ToLower();
					}
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/event_{ResourceID}_{this.Bibo_presentedAt.ArticleID}",  "https://www.w3.org/2006/vcard/ns#hasRegion", $"<{itemRegex}>", list, " . ");
				}
				if(this.Bibo_presentedAt.Roh_geographicFocusOther != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/event_{ResourceID}_{this.Bibo_presentedAt.ArticleID}",  "http://w3id.org/roh/geographicFocusOther", $"\"{GenerarTextoSinSaltoDeLinea(this.Bibo_presentedAt.Roh_geographicFocusOther).ToLower()}\"", list, " . ");
				}
				if(this.Bibo_presentedAt.Vivo_start != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/event_{ResourceID}_{this.Bibo_presentedAt.ArticleID}",  "http://vivoweb.org/ontology/core#start", $"{this.Bibo_presentedAt.Vivo_start.Value.ToString("yyyyMMddHHmmss")}", list, " . ");
				}
				if(this.Bibo_presentedAt.Vcard_locality != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/event_{ResourceID}_{this.Bibo_presentedAt.ArticleID}",  "https://www.w3.org/2006/vcard/ns#locality", $"\"{GenerarTextoSinSaltoDeLinea(this.Bibo_presentedAt.Vcard_locality).ToLower()}\"", list, " . ");
				}
				if(this.Bibo_presentedAt.Roh_withExternalAdmissionsCommittee != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/event_{ResourceID}_{this.Bibo_presentedAt.ArticleID}",  "http://w3id.org/roh/withExternalAdmissionsCommittee", $"\"{this.Bibo_presentedAt.Roh_withExternalAdmissionsCommittee.ToString().ToLower()}\"", list, " . ");
				}
				if(this.Bibo_presentedAt.Vivo_end != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/event_{ResourceID}_{this.Bibo_presentedAt.ArticleID}",  "http://vivoweb.org/ontology/core#end", $"{this.Bibo_presentedAt.Vivo_end.Value.ToString("yyyyMMddHHmmss")}", list, " . ");
				}
				if(this.Bibo_presentedAt.Dc_title != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/event_{ResourceID}_{this.Bibo_presentedAt.ArticleID}",  "http://purl.org/dc/elements/1.1/title", $"\"{GenerarTextoSinSaltoDeLinea(this.Bibo_presentedAt.Dc_title).ToLower()}\"", list, " . ");
				}
			}
			if(this.Bibo_identifier != null)
			{
			foreach(var item0 in this.Bibo_identifier)
			{
				AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}", "http://purl.org/ontology/bibo/identifier", $"<{resourceAPI.GraphsUrl}items/document_{ResourceID}_{item0.ArticleID}>", list, " . ");
				if(item0.Dc_title != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/document_{ResourceID}_{item0.ArticleID}",  "http://purl.org/dc/elements/1.1/title", $"\"{GenerarTextoSinSaltoDeLinea(item0.Dc_title).ToLower()}\"", list, " . ");
				}
				if(item0.Foaf_topic != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/document_{ResourceID}_{item0.ArticleID}",  "http://xmlns.com/foaf/0.1/topic", $"\"{GenerarTextoSinSaltoDeLinea(item0.Foaf_topic).ToLower()}\"", list, " . ");
				}
			}
			}
			if(this.Roh_hasMetric != null)
			{
			foreach(var item0 in this.Roh_hasMetric)
			{
				AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}", "http://w3id.org/roh/hasMetric", $"<{resourceAPI.GraphsUrl}items/publicationmetric_{ResourceID}_{item0.ArticleID}>", list, " . ");
				if(item0.Roh_metricName != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/publicationmetric_{ResourceID}_{item0.ArticleID}",  "http://w3id.org/roh/metricName", $"\"{GenerarTextoSinSaltoDeLinea(item0.Roh_metricName).ToLower()}\"", list, " . ");
				}
				if(item0.Roh_citationCount != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/publicationmetric_{ResourceID}_{item0.ArticleID}",  "http://w3id.org/roh/citationCount", $"{item0.Roh_citationCount.ToString()}", list, " . ");
				}
			}
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
				if(this.IdsVcard_hasLanguage != null)
				{
					foreach(var item2 in this.IdsVcard_hasLanguage)
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
						AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}", "https://www.w3.org/2006/vcard/ns#hasLanguage", $"<{itemRegex}>", list, " . ");
					}
				}
				if(this.IdRoh_project != null)
				{
					Regex regex = new Regex(@"\/items\/.+_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}");
					string itemRegex = this.IdRoh_project;
					if (regex.IsMatch(itemRegex))
					{
						itemRegex = $"http://gnoss/{resourceAPI.GetShortGuid(itemRegex).ToString().ToUpper()}";
					}
					else
					{
						itemRegex = itemRegex.ToLower();
					}
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "http://w3id.org/roh/project", $"<{itemRegex}>", list, " . ");
				}
				if(this.IdsRoh_isProducedBy != null)
				{
					foreach(var item2 in this.IdsRoh_isProducedBy)
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
						AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}", "http://w3id.org/roh/isProducedBy", $"<{itemRegex}>", list, " . ");
					}
				}
				if(this.IdsRoh_correspondingAuthor != null)
				{
					foreach(var item2 in this.IdsRoh_correspondingAuthor)
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
						AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}", "http://w3id.org/roh/correspondingAuthor", $"<{itemRegex}>", list, " . ");
					}
				}
				if(this.IdDc_type != null)
				{
					Regex regex = new Regex(@"\/items\/.+_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}");
					string itemRegex = this.IdDc_type;
					if (regex.IsMatch(itemRegex))
					{
						itemRegex = $"http://gnoss/{resourceAPI.GetShortGuid(itemRegex).ToString().ToUpper()}";
					}
					else
					{
						itemRegex = itemRegex.ToLower();
					}
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "http://purl.org/dc/elements/1.1/type", $"<{itemRegex}>", list, " . ");
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
				if(this.IdsBibo_cites != null)
				{
					foreach(var item2 in this.IdsBibo_cites)
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
						AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}", "http://purl.org/ontology/bibo/cites", $"<{itemRegex}>", list, " . ");
					}
				}
				if(this.IdVivo_hasPublicationVenue != null)
				{
					Regex regex = new Regex(@"\/items\/.+_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}");
					string itemRegex = this.IdVivo_hasPublicationVenue;
					if (regex.IsMatch(itemRegex))
					{
						itemRegex = $"http://gnoss/{resourceAPI.GetShortGuid(itemRegex).ToString().ToUpper()}";
					}
					else
					{
						itemRegex = itemRegex.ToLower();
					}
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "http://vivoweb.org/ontology/core#hasPublicationVenue", $"<{itemRegex}>", list, " . ");
				}
				if(this.Roh_legalDeposit != null)
				{
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "http://w3id.org/roh/legalDeposit", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_legalDeposit).ToLower()}\"", list, " . ");
				}
				if(this.Roh_scopus != null)
				{
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "http://w3id.org/roh/scopus", $"{this.Roh_scopus.Value.ToString()}", list, " . ");
				}
				if(this.Roh_assessmentStatus != null)
				{
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "http://w3id.org/roh/assessmentStatus", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_assessmentStatus).ToLower()}\"", list, " . ");
				}
				if(this.Roh_publicationTitle != null)
				{
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "http://w3id.org/roh/publicationTitle", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_publicationTitle).ToLower()}\"", list, " . ");
				}
				if(this.Vcard_locality != null)
				{
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "https://www.w3.org/2006/vcard/ns#locality", $"\"{GenerarTextoSinSaltoDeLinea(this.Vcard_locality).ToLower()}\"", list, " . ");
				}
				if(this.Roh_authorsNumber != null)
				{
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "http://w3id.org/roh/authorsNumber", $"{this.Roh_authorsNumber.Value.ToString()}", list, " . ");
				}
				if(this.Roh_inrecs != null)
				{
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "http://w3id.org/roh/inrecs", $"{this.Roh_inrecs.Value.ToString()}", list, " . ");
				}
				if(this.Roh_externalKeywords != null)
				{
					foreach(var item2 in this.Roh_externalKeywords)
					{
						AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}", "http://w3id.org/roh/externalKeywords", $"\"{GenerarTextoSinSaltoDeLinea(item2).ToLower()}\"", list, " . ");
					}
				}
				if(this.Roh_reviewsNumber != null)
				{
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "http://w3id.org/roh/reviewsNumber", $"{this.Roh_reviewsNumber.Value.ToString()}", list, " . ");
				}
				if(this.Roh_userJournal != null)
				{
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "http://w3id.org/roh/userJournal", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_userJournal).ToLower()}\"", list, " . ");
				}
				if(this.Bibo_abstract != null)
				{
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "http://purl.org/ontology/bibo/abstract", $"\"{GenerarTextoSinSaltoDeLinea(this.Bibo_abstract).ToLower()}\"", list, " . ");
				}
				if(this.Bibo_pmid != null)
				{
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "http://purl.org/ontology/bibo/pmid", $"\"{GenerarTextoSinSaltoDeLinea(this.Bibo_pmid).ToLower()}\"", list, " . ");
				}
				if(this.Vivo_freeTextKeyword != null)
				{
					foreach(var item2 in this.Vivo_freeTextKeyword)
					{
						AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}", "http://vivoweb.org/ontology/core#freeTextKeyword", $"\"{GenerarTextoSinSaltoDeLinea(item2).ToLower()}\"", list, " . ");
					}
				}
				if(this.Bibo_handle != null)
				{
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "http://purl.org/ontology/bibo/handle", $"\"{GenerarTextoSinSaltoDeLinea(this.Bibo_handle).ToLower()}\"", list, " . ");
				}
				if(this.Bibo_doi != null)
				{
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "http://purl.org/ontology/bibo/doi", $"\"{GenerarTextoSinSaltoDeLinea(this.Bibo_doi).ToLower()}\"", list, " . ");
				}
				if(this.Roh_wos != null)
				{
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "http://w3id.org/roh/wos", $"{this.Roh_wos.Value.ToString()}", list, " . ");
				}
				if(this.Bibo_issue != null)
				{
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "http://purl.org/ontology/bibo/issue", $"\"{GenerarTextoSinSaltoDeLinea(this.Bibo_issue).ToLower()}\"", list, " . ");
				}
				if(this.Roh_suggestedKeywords != null)
				{
					foreach(var item2 in this.Roh_suggestedKeywords)
					{
						AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}", "http://w3id.org/roh/suggestedKeywords", $"\"{GenerarTextoSinSaltoDeLinea(item2).ToLower()}\"", list, " . ");
					}
				}
				if(this.Bibo_volume != null)
				{
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "http://purl.org/ontology/bibo/volume", $"\"{GenerarTextoSinSaltoDeLinea(this.Bibo_volume).ToLower()}\"", list, " . ");
				}
				if(this.Roh_enrichedKeywords != null)
				{
					foreach(var item2 in this.Roh_enrichedKeywords)
					{
						AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}", "http://w3id.org/roh/enrichedKeywords", $"\"{GenerarTextoSinSaltoDeLinea(item2).ToLower()}\"", list, " . ");
					}
				}
				if(this.Dct_issued != null)
				{
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "http://purl.org/dc/terms/issued", $"{this.Dct_issued.Value.ToString("yyyyMMddHHmmss")}", list, " . ");
				}
				if(this.Roh_typeOthers != null)
				{
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "http://w3id.org/roh/typeOthers", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_typeOthers).ToLower()}\"", list, " . ");
				}
				if(this.Bibo_pageStart != null)
				{
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "http://purl.org/ontology/bibo/pageStart", $"{this.Bibo_pageStart.Value.ToString()}", list, " . ");
				}
				if(this.Roh_crisIdentifier != null)
				{
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "http://w3id.org/roh/crisIdentifier", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_crisIdentifier).ToLower()}\"", list, " . ");
				}
				if(this.Roh_congressProceedingsPublication != null)
				{
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "http://w3id.org/roh/congressProceedingsPublication", $"\"{this.Roh_congressProceedingsPublication.ToString().ToLower()}\"", list, " . ");
				}
				if(this.Vcard_url != null)
				{
					foreach(var item2 in this.Vcard_url)
					{
						AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}", "https://www.w3.org/2006/vcard/ns#url", $"\"{GenerarTextoSinSaltoDeLinea(item2).ToLower()}\"", list, " . ");
					}
				}
				if(this.Bibo_pageEnd != null)
				{
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "http://purl.org/ontology/bibo/pageEnd", $"{this.Bibo_pageEnd.Value.ToString()}", list, " . ");
				}
				if(this.Roh_collection != null)
				{
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "http://w3id.org/roh/collection", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_collection).ToLower()}\"", list, " . ");
				}
				if(this.Roh_userKeywords != null)
				{
					foreach(var item2 in this.Roh_userKeywords)
					{
						AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}", "http://w3id.org/roh/userKeywords", $"\"{GenerarTextoSinSaltoDeLinea(item2).ToLower()}\"", list, " . ");
					}
				}
				if(this.Roh_title != null)
				{
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "http://w3id.org/roh/title", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_title).ToLower()}\"", list, " . ");
				}
				if(this.IdRoh_scientificActivityDocument != null)
				{
					Regex regex = new Regex(@"\/items\/.+_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}");
					string itemRegex = this.IdRoh_scientificActivityDocument;
					if (regex.IsMatch(itemRegex))
					{
						itemRegex = $"http://gnoss/{resourceAPI.GetShortGuid(itemRegex).ToString().ToUpper()}";
					}
					else
					{
						itemRegex = itemRegex.ToLower();
					}
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "http://w3id.org/roh/scientificActivityDocument", $"<{itemRegex}>", list, " . ");
				}
				if(this.Roh_isValidated != null)
				{
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "http://w3id.org/roh/isValidated", $"\"{this.Roh_isValidated.ToString().ToLower()}\"", list, " . ");
				}
				if(this.Roh_citationLoadedCount != null)
				{
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "http://w3id.org/roh/citationLoadedCount", $"{this.Roh_citationLoadedCount.ToString()}", list, " . ");
				}
				if(this.Roh_citationCount != null)
				{
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "http://w3id.org/roh/citationCount", $"{this.Roh_citationCount.ToString()}", list, " . ");
				}
				if(this.Roh_referencesLoadedCount != null)
				{
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "http://w3id.org/roh/referencesLoadedCount", $"{this.Roh_referencesLoadedCount.ToString()}", list, " . ");
				}
				if(this.Roh_isPublic != null)
				{
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "http://w3id.org/roh/isPublic", $"\"{this.Roh_isPublic.ToString().ToLower()}\"", list, " . ");
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
			return $"{resourceAPI.GraphsUrl}items/DocumentOntology_{ResourceID}_{ArticleID}";
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
