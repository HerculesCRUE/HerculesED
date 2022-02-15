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
using Person = PersonOntology.Person;

namespace CurriculumvitaeOntology
{
	[ExcludeFromCodeCoverage]
	public class CV : GnossOCBase
	{

		public CV() : base() { } 

		public CV(SemanticResourceModel pSemCmsModel, LanguageEnum idiomaUsuario) : base()
		{
			this.mGNOSSID = pSemCmsModel.RootEntities[0].Entity.Uri;
			SemanticPropertyModel propRoh_scientificExperience = pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/scientificExperience");
			if(propRoh_scientificExperience != null && propRoh_scientificExperience.PropertyValues.Count > 0)
			{
				this.Roh_scientificExperience = new ScientificExperience(propRoh_scientificExperience.PropertyValues[0].RelatedEntity,idiomaUsuario);
			}
			SemanticPropertyModel propRoh_scientificActivity = pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/scientificActivity");
			if(propRoh_scientificActivity != null && propRoh_scientificActivity.PropertyValues.Count > 0)
			{
				this.Roh_scientificActivity = new ScientificActivity(propRoh_scientificActivity.PropertyValues[0].RelatedEntity,idiomaUsuario);
			}
			SemanticPropertyModel propRoh_personalData = pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/personalData");
			if(propRoh_personalData != null && propRoh_personalData.PropertyValues.Count > 0)
			{
				this.Roh_personalData = new PersonalData(propRoh_personalData.PropertyValues[0].RelatedEntity,idiomaUsuario);
			}
			SemanticPropertyModel propRoh_cvOf = pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/cvOf");
			if(propRoh_cvOf != null && propRoh_cvOf.PropertyValues.Count > 0)
			{
				this.Roh_cvOf = new Person(propRoh_cvOf.PropertyValues[0].RelatedEntity,idiomaUsuario);
			}
			this.Foaf_name = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://xmlns.com/foaf/0.1/name"));
		}

		public CV(SemanticEntityModel pSemCmsModel, LanguageEnum idiomaUsuario) : base()
		{
			this.mGNOSSID = pSemCmsModel.Entity.Uri;
			this.mURL = pSemCmsModel.Properties.FirstOrDefault(p => p.PropertyValues.Any(prop => prop.DownloadUrl != null))?.FirstPropertyValue.DownloadUrl;
			SemanticPropertyModel propRoh_scientificExperience = pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/scientificExperience");
			if(propRoh_scientificExperience != null && propRoh_scientificExperience.PropertyValues.Count > 0)
			{
				this.Roh_scientificExperience = new ScientificExperience(propRoh_scientificExperience.PropertyValues[0].RelatedEntity,idiomaUsuario);
			}
			SemanticPropertyModel propRoh_scientificActivity = pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/scientificActivity");
			if(propRoh_scientificActivity != null && propRoh_scientificActivity.PropertyValues.Count > 0)
			{
				this.Roh_scientificActivity = new ScientificActivity(propRoh_scientificActivity.PropertyValues[0].RelatedEntity,idiomaUsuario);
			}
			SemanticPropertyModel propRoh_personalData = pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/personalData");
			if(propRoh_personalData != null && propRoh_personalData.PropertyValues.Count > 0)
			{
				this.Roh_personalData = new PersonalData(propRoh_personalData.PropertyValues[0].RelatedEntity,idiomaUsuario);
			}
			SemanticPropertyModel propRoh_cvOf = pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/cvOf");
			if(propRoh_cvOf != null && propRoh_cvOf.PropertyValues.Count > 0)
			{
				this.Roh_cvOf = new Person(propRoh_cvOf.PropertyValues[0].RelatedEntity,idiomaUsuario);
			}
			this.Foaf_name = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://xmlns.com/foaf/0.1/name"));
		}

		public virtual string RdfType { get { return "http://w3id.org/roh/CV"; } }
		public virtual string RdfsLabel { get { return "http://w3id.org/roh/CV"; } }
		[LABEL(LanguageEnum.es,"Usuario Gnoss")]
		[RDFProperty("http://w3id.org/roh/gnossUser")]
		public  object Roh_gnossUser  { get; set;} 
		public string IdRoh_gnossUser  { get; set;} 

		[LABEL(LanguageEnum.es,"http://w3id.org/roh/scientificExperience")]
		[RDFProperty("http://w3id.org/roh/scientificExperience")]
		public  ScientificExperience Roh_scientificExperience { get; set;}

		[LABEL(LanguageEnum.es,"http://w3id.org/roh/scientificActivity")]
		[RDFProperty("http://w3id.org/roh/scientificActivity")]
		public  ScientificActivity Roh_scientificActivity { get; set;}

		[LABEL(LanguageEnum.es,"http://w3id.org/roh/personalData")]
		[RDFProperty("http://w3id.org/roh/personalData")]
		public  PersonalData Roh_personalData { get; set;}

		[LABEL(LanguageEnum.es,"http://w3id.org/roh/cvOf")]
		[RDFProperty("http://w3id.org/roh/cvOf")]
		[Required]
		public  Person Roh_cvOf  { get; set;} 
		public string IdRoh_cvOf  { get; set;} 

		[LABEL(LanguageEnum.es,"http://xmlns.com/foaf/0.1/name")]
		[RDFProperty("http://xmlns.com/foaf/0.1/name")]
		public  string Foaf_name { get; set;}


		internal override void GetProperties()
		{
			base.GetProperties();
			//propList.Add(new StringOntologyProperty("roh:gnossUser", this.Roh_gnossUser));
			propList.Add(new StringOntologyProperty("roh:cvOf", this.IdRoh_cvOf));
			propList.Add(new StringOntologyProperty("foaf:name", this.Foaf_name));
		}

		internal override void GetEntities()
		{
			base.GetEntities();
			Roh_scientificExperience.GetProperties();
			Roh_scientificExperience.GetEntities();
			OntologyEntity entityRoh_scientificExperience = new OntologyEntity("http://w3id.org/roh/ScientificExperience", "http://w3id.org/roh/ScientificExperience", "roh:scientificExperience", Roh_scientificExperience.propList, Roh_scientificExperience.entList);
			entList.Add(entityRoh_scientificExperience);
			Roh_scientificActivity.GetProperties();
			Roh_scientificActivity.GetEntities();
			OntologyEntity entityRoh_scientificActivity = new OntologyEntity("http://w3id.org/roh/ScientificActivity", "http://w3id.org/roh/ScientificActivity", "roh:scientificActivity", Roh_scientificActivity.propList, Roh_scientificActivity.entList);
			entList.Add(entityRoh_scientificActivity);
			Roh_personalData.GetProperties();
			Roh_personalData.GetEntities();
			OntologyEntity entityRoh_personalData = new OntologyEntity("http://w3id.org/roh/PersonalData", "http://w3id.org/roh/PersonalData", "roh:personalData", Roh_personalData.propList, Roh_personalData.entList);
			entList.Add(entityRoh_personalData);
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
			AgregarTripleALista($"{resourceAPI.GraphsUrl}items/CV_{ResourceID}_{ArticleID}", "http://www.w3.org/1999/02/22-rdf-syntax-ns#type", $"<http://w3id.org/roh/CV>", list, " . ");
			AgregarTripleALista($"{resourceAPI.GraphsUrl}items/CV_{ResourceID}_{ArticleID}", "http://www.w3.org/2000/01/rdf-schema#label", $"\"http://w3id.org/roh/CV\"", list, " . ");
			AgregarTripleALista($"{resourceAPI.GraphsUrl}{ResourceID}", "http://gnoss/hasEntidad", $"<{resourceAPI.GraphsUrl}items/CV_{ResourceID}_{ArticleID}>", list, " . ");
			if(this.Roh_scientificExperience != null)
			{
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/ScientificExperience_{ResourceID}_{this.Roh_scientificExperience.ArticleID}", "http://www.w3.org/1999/02/22-rdf-syntax-ns#type", $"<http://w3id.org/roh/ScientificExperience>", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/ScientificExperience_{ResourceID}_{this.Roh_scientificExperience.ArticleID}", "http://www.w3.org/2000/01/rdf-schema#label", $"\"http://w3id.org/roh/ScientificExperience\"", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}{ResourceID}", "http://gnoss/hasEntidad", $"<{resourceAPI.GraphsUrl}items/ScientificExperience_{ResourceID}_{this.Roh_scientificExperience.ArticleID}>", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/CV_{ResourceID}_{ArticleID}", "http://w3id.org/roh/scientificExperience", $"<{resourceAPI.GraphsUrl}items/ScientificExperience_{ResourceID}_{this.Roh_scientificExperience.ArticleID}>", list, " . ");
			if(this.Roh_scientificExperience.Roh_supervisedArtisticProjects != null)
			{
			foreach(var item1 in this.Roh_scientificExperience.Roh_supervisedArtisticProjects)
			{
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/RelatedSupervisedArtisticProject_{ResourceID}_{item1.ArticleID}", "http://www.w3.org/1999/02/22-rdf-syntax-ns#type", $"<http://w3id.org/roh/RelatedSupervisedArtisticProject>", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/RelatedSupervisedArtisticProject_{ResourceID}_{item1.ArticleID}", "http://www.w3.org/2000/01/rdf-schema#label", $"\"http://w3id.org/roh/RelatedSupervisedArtisticProject\"", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}{ResourceID}", "http://gnoss/hasEntidad", $"<{resourceAPI.GraphsUrl}items/RelatedSupervisedArtisticProject_{ResourceID}_{item1.ArticleID}>", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/ScientificExperience_{ResourceID}_{this.Roh_scientificExperience.ArticleID}", "http://w3id.org/roh/supervisedArtisticProjects", $"<{resourceAPI.GraphsUrl}items/RelatedSupervisedArtisticProject_{ResourceID}_{item1.ArticleID}>", list, " . ");
			if(item1.Roh_relatedSupervisedArtisticProjectCV != null)
			{
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/RelatedSupervisedArtisticProjectCV_{ResourceID}_{item1.Roh_relatedSupervisedArtisticProjectCV.ArticleID}", "http://www.w3.org/1999/02/22-rdf-syntax-ns#type", $"<http://w3id.org/roh/RelatedSupervisedArtisticProjectCV>", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/RelatedSupervisedArtisticProjectCV_{ResourceID}_{item1.Roh_relatedSupervisedArtisticProjectCV.ArticleID}", "http://www.w3.org/2000/01/rdf-schema#label", $"\"http://w3id.org/roh/RelatedSupervisedArtisticProjectCV\"", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}{ResourceID}", "http://gnoss/hasEntidad", $"<{resourceAPI.GraphsUrl}items/RelatedSupervisedArtisticProjectCV_{ResourceID}_{item1.Roh_relatedSupervisedArtisticProjectCV.ArticleID}>", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/RelatedSupervisedArtisticProject_{ResourceID}_{item1.ArticleID}", "http://w3id.org/roh/relatedSupervisedArtisticProjectCV", $"<{resourceAPI.GraphsUrl}items/RelatedSupervisedArtisticProjectCV_{ResourceID}_{item1.Roh_relatedSupervisedArtisticProjectCV.ArticleID}>", list, " . ");
				if(item1.Roh_relatedSupervisedArtisticProjectCV.IdRoh_contributionGradeProject != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/RelatedSupervisedArtisticProjectCV_{ResourceID}_{item1.Roh_relatedSupervisedArtisticProjectCV.ArticleID}",  "http://w3id.org/roh/contributionGradeProject", $"<{item1.Roh_relatedSupervisedArtisticProjectCV.IdRoh_contributionGradeProject}>", list, " . ");
				}
				if(item1.Roh_relatedSupervisedArtisticProjectCV.Roh_curator != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/RelatedSupervisedArtisticProjectCV_{ResourceID}_{item1.Roh_relatedSupervisedArtisticProjectCV.ArticleID}",  "http://w3id.org/roh/curator", $"\"{item1.Roh_relatedSupervisedArtisticProjectCV.Roh_curator.ToString()}\"", list, " . ");
				}
				if(item1.Roh_relatedSupervisedArtisticProjectCV.Roh_contributionGradeProjectOther != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/RelatedSupervisedArtisticProjectCV_{ResourceID}_{item1.Roh_relatedSupervisedArtisticProjectCV.ArticleID}",  "http://w3id.org/roh/contributionGradeProjectOther", $"\"{GenerarTextoSinSaltoDeLinea(item1.Roh_relatedSupervisedArtisticProjectCV.Roh_contributionGradeProjectOther)}\"", list, " . ");
				}
			}
				if(item1.IdVivo_relatedBy != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/RelatedSupervisedArtisticProject_{ResourceID}_{item1.ArticleID}",  "http://vivoweb.org/ontology/core#relatedBy", $"<{item1.IdVivo_relatedBy}>", list, " . ");
				}
				if(item1.Roh_isPublic != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/RelatedSupervisedArtisticProject_{ResourceID}_{item1.ArticleID}",  "http://w3id.org/roh/isPublic", $"\"{item1.Roh_isPublic.ToString()}\"", list, " . ");
				}
			}
			}
			if(this.Roh_scientificExperience.Roh_competitiveProjects != null)
			{
			foreach(var item2 in this.Roh_scientificExperience.Roh_competitiveProjects)
			{
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/RelatedCompetitiveProject_{ResourceID}_{item2.ArticleID}", "http://www.w3.org/1999/02/22-rdf-syntax-ns#type", $"<http://w3id.org/roh/RelatedCompetitiveProject>", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/RelatedCompetitiveProject_{ResourceID}_{item2.ArticleID}", "http://www.w3.org/2000/01/rdf-schema#label", $"\"http://w3id.org/roh/RelatedCompetitiveProject\"", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}{ResourceID}", "http://gnoss/hasEntidad", $"<{resourceAPI.GraphsUrl}items/RelatedCompetitiveProject_{ResourceID}_{item2.ArticleID}>", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/ScientificExperience_{ResourceID}_{this.Roh_scientificExperience.ArticleID}", "http://w3id.org/roh/competitiveProjects", $"<{resourceAPI.GraphsUrl}items/RelatedCompetitiveProject_{ResourceID}_{item2.ArticleID}>", list, " . ");
			if(item2.Roh_relatedCompetitiveProjectCV != null)
			{
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/RelatedCompetitiveProjectCV_{ResourceID}_{item2.Roh_relatedCompetitiveProjectCV.ArticleID}", "http://www.w3.org/1999/02/22-rdf-syntax-ns#type", $"<http://w3id.org/roh/RelatedCompetitiveProjectCV>", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/RelatedCompetitiveProjectCV_{ResourceID}_{item2.Roh_relatedCompetitiveProjectCV.ArticleID}", "http://www.w3.org/2000/01/rdf-schema#label", $"\"http://w3id.org/roh/RelatedCompetitiveProjectCV\"", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}{ResourceID}", "http://gnoss/hasEntidad", $"<{resourceAPI.GraphsUrl}items/RelatedCompetitiveProjectCV_{ResourceID}_{item2.Roh_relatedCompetitiveProjectCV.ArticleID}>", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/RelatedCompetitiveProject_{ResourceID}_{item2.ArticleID}", "http://w3id.org/roh/relatedCompetitiveProjectCV", $"<{resourceAPI.GraphsUrl}items/RelatedCompetitiveProjectCV_{ResourceID}_{item2.Roh_relatedCompetitiveProjectCV.ArticleID}>", list, " . ");
				if(item2.Roh_relatedCompetitiveProjectCV.IdRoh_dedication != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/RelatedCompetitiveProjectCV_{ResourceID}_{item2.Roh_relatedCompetitiveProjectCV.ArticleID}",  "http://w3id.org/roh/dedication", $"<{item2.Roh_relatedCompetitiveProjectCV.IdRoh_dedication}>", list, " . ");
				}
				if(item2.Roh_relatedCompetitiveProjectCV.IdRoh_participationType != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/RelatedCompetitiveProjectCV_{ResourceID}_{item2.Roh_relatedCompetitiveProjectCV.ArticleID}",  "http://w3id.org/roh/participationType", $"<{item2.Roh_relatedCompetitiveProjectCV.IdRoh_participationType}>", list, " . ");
				}
				if(item2.Roh_relatedCompetitiveProjectCV.IdRoh_contributionGradeProject != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/RelatedCompetitiveProjectCV_{ResourceID}_{item2.Roh_relatedCompetitiveProjectCV.ArticleID}",  "http://w3id.org/roh/contributionGradeProject", $"<{item2.Roh_relatedCompetitiveProjectCV.IdRoh_contributionGradeProject}>", list, " . ");
				}
				if(item2.Roh_relatedCompetitiveProjectCV.Roh_contributionGradeProjectOther != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/RelatedCompetitiveProjectCV_{ResourceID}_{item2.Roh_relatedCompetitiveProjectCV.ArticleID}",  "http://w3id.org/roh/contributionGradeProjectOther", $"\"{GenerarTextoSinSaltoDeLinea(item2.Roh_relatedCompetitiveProjectCV.Roh_contributionGradeProjectOther)}\"", list, " . ");
				}
				if(item2.Roh_relatedCompetitiveProjectCV.Roh_participationTypeOther != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/RelatedCompetitiveProjectCV_{ResourceID}_{item2.Roh_relatedCompetitiveProjectCV.ArticleID}",  "http://w3id.org/roh/participationTypeOther", $"\"{GenerarTextoSinSaltoDeLinea(item2.Roh_relatedCompetitiveProjectCV.Roh_participationTypeOther)}\"", list, " . ");
				}
				if(item2.Roh_relatedCompetitiveProjectCV.Roh_applicantContribution != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/RelatedCompetitiveProjectCV_{ResourceID}_{item2.Roh_relatedCompetitiveProjectCV.ArticleID}",  "http://w3id.org/roh/applicantContribution", $"\"{GenerarTextoSinSaltoDeLinea(item2.Roh_relatedCompetitiveProjectCV.Roh_applicantContribution)}\"", list, " . ");
				}
			}
				if(item2.IdVivo_relatedBy != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/RelatedCompetitiveProject_{ResourceID}_{item2.ArticleID}",  "http://vivoweb.org/ontology/core#relatedBy", $"<{item2.IdVivo_relatedBy}>", list, " . ");
				}
				if(item2.Roh_isPublic != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/RelatedCompetitiveProject_{ResourceID}_{item2.ArticleID}",  "http://w3id.org/roh/isPublic", $"\"{item2.Roh_isPublic.ToString()}\"", list, " . ");
				}
			}
			}
			if(this.Roh_scientificExperience.Roh_groups != null)
			{
			foreach(var item3 in this.Roh_scientificExperience.Roh_groups)
			{
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/RelatedGroup_{ResourceID}_{item3.ArticleID}", "http://www.w3.org/1999/02/22-rdf-syntax-ns#type", $"<http://w3id.org/roh/RelatedGroup>", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/RelatedGroup_{ResourceID}_{item3.ArticleID}", "http://www.w3.org/2000/01/rdf-schema#label", $"\"http://w3id.org/roh/RelatedGroup\"", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}{ResourceID}", "http://gnoss/hasEntidad", $"<{resourceAPI.GraphsUrl}items/RelatedGroup_{ResourceID}_{item3.ArticleID}>", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/ScientificExperience_{ResourceID}_{this.Roh_scientificExperience.ArticleID}", "http://w3id.org/roh/groups", $"<{resourceAPI.GraphsUrl}items/RelatedGroup_{ResourceID}_{item3.ArticleID}>", list, " . ");
				if(item3.IdVivo_relatedBy != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/RelatedGroup_{ResourceID}_{item3.ArticleID}",  "http://vivoweb.org/ontology/core#relatedBy", $"<{item3.IdVivo_relatedBy}>", list, " . ");
				}
				if(item3.Roh_isPublic != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/RelatedGroup_{ResourceID}_{item3.ArticleID}",  "http://w3id.org/roh/isPublic", $"\"{item3.Roh_isPublic.ToString()}\"", list, " . ");
				}
			}
			}
			if(this.Roh_scientificExperience.Roh_technologicalResults != null)
			{
			foreach(var item4 in this.Roh_scientificExperience.Roh_technologicalResults)
			{
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/RelatedTechnologicalResult_{ResourceID}_{item4.ArticleID}", "http://www.w3.org/1999/02/22-rdf-syntax-ns#type", $"<http://w3id.org/roh/RelatedTechnologicalResult>", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/RelatedTechnologicalResult_{ResourceID}_{item4.ArticleID}", "http://www.w3.org/2000/01/rdf-schema#label", $"\"http://w3id.org/roh/RelatedTechnologicalResult\"", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}{ResourceID}", "http://gnoss/hasEntidad", $"<{resourceAPI.GraphsUrl}items/RelatedTechnologicalResult_{ResourceID}_{item4.ArticleID}>", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/ScientificExperience_{ResourceID}_{this.Roh_scientificExperience.ArticleID}", "http://w3id.org/roh/technologicalResults", $"<{resourceAPI.GraphsUrl}items/RelatedTechnologicalResult_{ResourceID}_{item4.ArticleID}>", list, " . ");
			if(item4.Roh_relatedTechnologicalResultCV != null)
			{
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/RelatedTechnologicalResultCV_{ResourceID}_{item4.Roh_relatedTechnologicalResultCV.ArticleID}", "http://www.w3.org/1999/02/22-rdf-syntax-ns#type", $"<http://w3id.org/roh/RelatedTechnologicalResultCV>", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/RelatedTechnologicalResultCV_{ResourceID}_{item4.Roh_relatedTechnologicalResultCV.ArticleID}", "http://www.w3.org/2000/01/rdf-schema#label", $"\"http://w3id.org/roh/RelatedTechnologicalResultCV\"", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}{ResourceID}", "http://gnoss/hasEntidad", $"<{resourceAPI.GraphsUrl}items/RelatedTechnologicalResultCV_{ResourceID}_{item4.Roh_relatedTechnologicalResultCV.ArticleID}>", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/RelatedTechnologicalResult_{ResourceID}_{item4.ArticleID}", "http://w3id.org/roh/relatedTechnologicalResultCV", $"<{resourceAPI.GraphsUrl}items/RelatedTechnologicalResultCV_{ResourceID}_{item4.Roh_relatedTechnologicalResultCV.ArticleID}>", list, " . ");
				if(item4.Roh_relatedTechnologicalResultCV.IdRoh_contributionGradeProject != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/RelatedTechnologicalResultCV_{ResourceID}_{item4.Roh_relatedTechnologicalResultCV.ArticleID}",  "http://w3id.org/roh/contributionGradeProject", $"<{item4.Roh_relatedTechnologicalResultCV.IdRoh_contributionGradeProject}>", list, " . ");
				}
				if(item4.Roh_relatedTechnologicalResultCV.Roh_contributionGradeProjectOther != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/RelatedTechnologicalResultCV_{ResourceID}_{item4.Roh_relatedTechnologicalResultCV.ArticleID}",  "http://w3id.org/roh/contributionGradeProjectOther", $"\"{GenerarTextoSinSaltoDeLinea(item4.Roh_relatedTechnologicalResultCV.Roh_contributionGradeProjectOther)}\"", list, " . ");
				}
				if(item4.Roh_relatedTechnologicalResultCV.Roh_expertTechnologist != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/RelatedTechnologicalResultCV_{ResourceID}_{item4.Roh_relatedTechnologicalResultCV.ArticleID}",  "http://w3id.org/roh/expertTechnologist", $"\"{item4.Roh_relatedTechnologicalResultCV.Roh_expertTechnologist.ToString()}\"", list, " . ");
				}
			}
				if(item4.IdVivo_relatedBy != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/RelatedTechnologicalResult_{ResourceID}_{item4.ArticleID}",  "http://vivoweb.org/ontology/core#relatedBy", $"<{item4.IdVivo_relatedBy}>", list, " . ");
				}
				if(item4.Roh_isPublic != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/RelatedTechnologicalResult_{ResourceID}_{item4.ArticleID}",  "http://w3id.org/roh/isPublic", $"\"{item4.Roh_isPublic.ToString()}\"", list, " . ");
				}
			}
			}
			if(this.Roh_scientificExperience.Roh_nonCompetitiveProjects != null)
			{
			foreach(var item5 in this.Roh_scientificExperience.Roh_nonCompetitiveProjects)
			{
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/RelatedNonCompetitiveProject_{ResourceID}_{item5.ArticleID}", "http://www.w3.org/1999/02/22-rdf-syntax-ns#type", $"<http://w3id.org/roh/RelatedNonCompetitiveProject>", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/RelatedNonCompetitiveProject_{ResourceID}_{item5.ArticleID}", "http://www.w3.org/2000/01/rdf-schema#label", $"\"http://w3id.org/roh/RelatedNonCompetitiveProject\"", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}{ResourceID}", "http://gnoss/hasEntidad", $"<{resourceAPI.GraphsUrl}items/RelatedNonCompetitiveProject_{ResourceID}_{item5.ArticleID}>", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/ScientificExperience_{ResourceID}_{this.Roh_scientificExperience.ArticleID}", "http://w3id.org/roh/nonCompetitiveProjects", $"<{resourceAPI.GraphsUrl}items/RelatedNonCompetitiveProject_{ResourceID}_{item5.ArticleID}>", list, " . ");
			if(item5.Roh_relatedNonCompetitiveProjectCV != null)
			{
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/RelatedNonCompetitiveProjectCV_{ResourceID}_{item5.Roh_relatedNonCompetitiveProjectCV.ArticleID}", "http://www.w3.org/1999/02/22-rdf-syntax-ns#type", $"<http://w3id.org/roh/RelatedNonCompetitiveProjectCV>", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/RelatedNonCompetitiveProjectCV_{ResourceID}_{item5.Roh_relatedNonCompetitiveProjectCV.ArticleID}", "http://www.w3.org/2000/01/rdf-schema#label", $"\"http://w3id.org/roh/RelatedNonCompetitiveProjectCV\"", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}{ResourceID}", "http://gnoss/hasEntidad", $"<{resourceAPI.GraphsUrl}items/RelatedNonCompetitiveProjectCV_{ResourceID}_{item5.Roh_relatedNonCompetitiveProjectCV.ArticleID}>", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/RelatedNonCompetitiveProject_{ResourceID}_{item5.ArticleID}", "http://w3id.org/roh/relatedNonCompetitiveProjectCV", $"<{resourceAPI.GraphsUrl}items/RelatedNonCompetitiveProjectCV_{ResourceID}_{item5.Roh_relatedNonCompetitiveProjectCV.ArticleID}>", list, " . ");
				if(item5.Roh_relatedNonCompetitiveProjectCV.IdRoh_dedication != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/RelatedNonCompetitiveProjectCV_{ResourceID}_{item5.Roh_relatedNonCompetitiveProjectCV.ArticleID}",  "http://w3id.org/roh/dedication", $"<{item5.Roh_relatedNonCompetitiveProjectCV.IdRoh_dedication}>", list, " . ");
				}
				if(item5.Roh_relatedNonCompetitiveProjectCV.IdRoh_participationType != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/RelatedNonCompetitiveProjectCV_{ResourceID}_{item5.Roh_relatedNonCompetitiveProjectCV.ArticleID}",  "http://w3id.org/roh/participationType", $"<{item5.Roh_relatedNonCompetitiveProjectCV.IdRoh_participationType}>", list, " . ");
				}
				if(item5.Roh_relatedNonCompetitiveProjectCV.IdRoh_contributionGradeProject != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/RelatedNonCompetitiveProjectCV_{ResourceID}_{item5.Roh_relatedNonCompetitiveProjectCV.ArticleID}",  "http://w3id.org/roh/contributionGradeProject", $"<{item5.Roh_relatedNonCompetitiveProjectCV.IdRoh_contributionGradeProject}>", list, " . ");
				}
				if(item5.Roh_relatedNonCompetitiveProjectCV.Roh_contributionGradeProjectOther != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/RelatedNonCompetitiveProjectCV_{ResourceID}_{item5.Roh_relatedNonCompetitiveProjectCV.ArticleID}",  "http://w3id.org/roh/contributionGradeProjectOther", $"\"{GenerarTextoSinSaltoDeLinea(item5.Roh_relatedNonCompetitiveProjectCV.Roh_contributionGradeProjectOther)}\"", list, " . ");
				}
				if(item5.Roh_relatedNonCompetitiveProjectCV.Roh_participationTypeOther != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/RelatedNonCompetitiveProjectCV_{ResourceID}_{item5.Roh_relatedNonCompetitiveProjectCV.ArticleID}",  "http://w3id.org/roh/participationTypeOther", $"\"{GenerarTextoSinSaltoDeLinea(item5.Roh_relatedNonCompetitiveProjectCV.Roh_participationTypeOther)}\"", list, " . ");
				}
				if(item5.Roh_relatedNonCompetitiveProjectCV.Roh_applicantContribution != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/RelatedNonCompetitiveProjectCV_{ResourceID}_{item5.Roh_relatedNonCompetitiveProjectCV.ArticleID}",  "http://w3id.org/roh/applicantContribution", $"\"{GenerarTextoSinSaltoDeLinea(item5.Roh_relatedNonCompetitiveProjectCV.Roh_applicantContribution)}\"", list, " . ");
				}
			}
				if(item5.IdVivo_relatedBy != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/RelatedNonCompetitiveProject_{ResourceID}_{item5.ArticleID}",  "http://vivoweb.org/ontology/core#relatedBy", $"<{item5.IdVivo_relatedBy}>", list, " . ");
				}
				if(item5.Roh_isPublic != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/RelatedNonCompetitiveProject_{ResourceID}_{item5.ArticleID}",  "http://w3id.org/roh/isPublic", $"\"{item5.Roh_isPublic.ToString()}\"", list, " . ");
				}
			}
			}
			if(this.Roh_scientificExperience.Roh_patents != null)
			{
			foreach(var item6 in this.Roh_scientificExperience.Roh_patents)
			{
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/RelatedPatent_{ResourceID}_{item6.ArticleID}", "http://www.w3.org/1999/02/22-rdf-syntax-ns#type", $"<http://w3id.org/roh/RelatedPatent>", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/RelatedPatent_{ResourceID}_{item6.ArticleID}", "http://www.w3.org/2000/01/rdf-schema#label", $"\"http://w3id.org/roh/RelatedPatent\"", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}{ResourceID}", "http://gnoss/hasEntidad", $"<{resourceAPI.GraphsUrl}items/RelatedPatent_{ResourceID}_{item6.ArticleID}>", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/ScientificExperience_{ResourceID}_{this.Roh_scientificExperience.ArticleID}", "http://w3id.org/roh/patents", $"<{resourceAPI.GraphsUrl}items/RelatedPatent_{ResourceID}_{item6.ArticleID}>", list, " . ");
				if(item6.IdVivo_relatedBy != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/RelatedPatent_{ResourceID}_{item6.ArticleID}",  "http://vivoweb.org/ontology/core#relatedBy", $"<{item6.IdVivo_relatedBy}>", list, " . ");
				}
				if(item6.Roh_isPublic != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/RelatedPatent_{ResourceID}_{item6.ArticleID}",  "http://w3id.org/roh/isPublic", $"\"{item6.Roh_isPublic.ToString()}\"", list, " . ");
				}
			}
			}
				if(this.Roh_scientificExperience.Roh_title != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/ScientificExperience_{ResourceID}_{this.Roh_scientificExperience.ArticleID}",  "http://w3id.org/roh/title", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_scientificExperience.Roh_title)}\"", list, " . ");
				}
			}
			if(this.Roh_scientificActivity != null)
			{
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/ScientificActivity_{ResourceID}_{this.Roh_scientificActivity.ArticleID}", "http://www.w3.org/1999/02/22-rdf-syntax-ns#type", $"<http://w3id.org/roh/ScientificActivity>", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/ScientificActivity_{ResourceID}_{this.Roh_scientificActivity.ArticleID}", "http://www.w3.org/2000/01/rdf-schema#label", $"\"http://w3id.org/roh/ScientificActivity\"", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}{ResourceID}", "http://gnoss/hasEntidad", $"<{resourceAPI.GraphsUrl}items/ScientificActivity_{ResourceID}_{this.Roh_scientificActivity.ArticleID}>", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/CV_{ResourceID}_{ArticleID}", "http://w3id.org/roh/scientificActivity", $"<{resourceAPI.GraphsUrl}items/ScientificActivity_{ResourceID}_{this.Roh_scientificActivity.ArticleID}>", list, " . ");
			if(this.Roh_scientificActivity.Roh_otherDistinctions != null)
			{
			foreach(var item1 in this.Roh_scientificActivity.Roh_otherDistinctions)
			{
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/RelatedOtherDistinction_{ResourceID}_{item1.ArticleID}", "http://www.w3.org/1999/02/22-rdf-syntax-ns#type", $"<http://w3id.org/roh/RelatedOtherDistinction>", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/RelatedOtherDistinction_{ResourceID}_{item1.ArticleID}", "http://www.w3.org/2000/01/rdf-schema#label", $"\"http://w3id.org/roh/RelatedOtherDistinction\"", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}{ResourceID}", "http://gnoss/hasEntidad", $"<{resourceAPI.GraphsUrl}items/RelatedOtherDistinction_{ResourceID}_{item1.ArticleID}>", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/ScientificActivity_{ResourceID}_{this.Roh_scientificActivity.ArticleID}", "http://w3id.org/roh/otherDistinctions", $"<{resourceAPI.GraphsUrl}items/RelatedOtherDistinction_{ResourceID}_{item1.ArticleID}>", list, " . ");
				if(item1.IdVivo_relatedBy != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/RelatedOtherDistinction_{ResourceID}_{item1.ArticleID}",  "http://vivoweb.org/ontology/core#relatedBy", $"<{item1.IdVivo_relatedBy}>", list, " . ");
				}
				if(item1.Roh_isPublic != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/RelatedOtherDistinction_{ResourceID}_{item1.ArticleID}",  "http://w3id.org/roh/isPublic", $"\"{item1.Roh_isPublic.ToString()}\"", list, " . ");
				}
			}
			}
			if(this.Roh_scientificActivity.Roh_generalQualityIndicators != null)
			{
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/GeneralQualityIndicator_{ResourceID}_{this.Roh_scientificActivity.Roh_generalQualityIndicators.ArticleID}", "http://www.w3.org/1999/02/22-rdf-syntax-ns#type", $"<http://w3id.org/roh/GeneralQualityIndicator>", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/GeneralQualityIndicator_{ResourceID}_{this.Roh_scientificActivity.Roh_generalQualityIndicators.ArticleID}", "http://www.w3.org/2000/01/rdf-schema#label", $"\"http://w3id.org/roh/GeneralQualityIndicator\"", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}{ResourceID}", "http://gnoss/hasEntidad", $"<{resourceAPI.GraphsUrl}items/GeneralQualityIndicator_{ResourceID}_{this.Roh_scientificActivity.Roh_generalQualityIndicators.ArticleID}>", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/ScientificActivity_{ResourceID}_{this.Roh_scientificActivity.ArticleID}", "http://w3id.org/roh/generalQualityIndicators", $"<{resourceAPI.GraphsUrl}items/GeneralQualityIndicator_{ResourceID}_{this.Roh_scientificActivity.Roh_generalQualityIndicators.ArticleID}>", list, " . ");
			if(this.Roh_scientificActivity.Roh_generalQualityIndicators.Roh_generalQualityIndicatorCV != null)
			{
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/GeneralQualityIndicatorCV_{ResourceID}_{this.Roh_scientificActivity.Roh_generalQualityIndicators.Roh_generalQualityIndicatorCV.ArticleID}", "http://www.w3.org/1999/02/22-rdf-syntax-ns#type", $"<http://w3id.org/roh/GeneralQualityIndicatorCV>", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/GeneralQualityIndicatorCV_{ResourceID}_{this.Roh_scientificActivity.Roh_generalQualityIndicators.Roh_generalQualityIndicatorCV.ArticleID}", "http://www.w3.org/2000/01/rdf-schema#label", $"\"http://w3id.org/roh/GeneralQualityIndicatorCV\"", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}{ResourceID}", "http://gnoss/hasEntidad", $"<{resourceAPI.GraphsUrl}items/GeneralQualityIndicatorCV_{ResourceID}_{this.Roh_scientificActivity.Roh_generalQualityIndicators.Roh_generalQualityIndicatorCV.ArticleID}>", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/GeneralQualityIndicator_{ResourceID}_{this.Roh_scientificActivity.Roh_generalQualityIndicators.ArticleID}", "http://w3id.org/roh/generalQualityIndicatorCV", $"<{resourceAPI.GraphsUrl}items/GeneralQualityIndicatorCV_{ResourceID}_{this.Roh_scientificActivity.Roh_generalQualityIndicators.Roh_generalQualityIndicatorCV.ArticleID}>", list, " . ");
				if(this.Roh_scientificActivity.Roh_generalQualityIndicators.Roh_generalQualityIndicatorCV.Roh_generalQualityIndicator != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/GeneralQualityIndicatorCV_{ResourceID}_{this.Roh_scientificActivity.Roh_generalQualityIndicators.Roh_generalQualityIndicatorCV.ArticleID}",  "http://w3id.org/roh/generalQualityIndicator", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_scientificActivity.Roh_generalQualityIndicators.Roh_generalQualityIndicatorCV.Roh_generalQualityIndicator)}\"", list, " . ");
				}
			}
			}
			if(this.Roh_scientificActivity.Roh_worksSubmittedConferences != null)
			{
			foreach(var item3 in this.Roh_scientificActivity.Roh_worksSubmittedConferences)
			{
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/RelatedWorkSubmittedConferences_{ResourceID}_{item3.ArticleID}", "http://www.w3.org/1999/02/22-rdf-syntax-ns#type", $"<http://w3id.org/roh/RelatedWorkSubmittedConferences>", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/RelatedWorkSubmittedConferences_{ResourceID}_{item3.ArticleID}", "http://www.w3.org/2000/01/rdf-schema#label", $"\"http://w3id.org/roh/RelatedWorkSubmittedConferences\"", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}{ResourceID}", "http://gnoss/hasEntidad", $"<{resourceAPI.GraphsUrl}items/RelatedWorkSubmittedConferences_{ResourceID}_{item3.ArticleID}>", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/ScientificActivity_{ResourceID}_{this.Roh_scientificActivity.ArticleID}", "http://w3id.org/roh/worksSubmittedConferences", $"<{resourceAPI.GraphsUrl}items/RelatedWorkSubmittedConferences_{ResourceID}_{item3.ArticleID}>", list, " . ");
			if(item3.Roh_relatedWorkSubmittedConferencesCV != null)
			{
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/RelatedWorkSubmittedConferencesCV_{ResourceID}_{item3.Roh_relatedWorkSubmittedConferencesCV.ArticleID}", "http://www.w3.org/1999/02/22-rdf-syntax-ns#type", $"<http://w3id.org/roh/RelatedWorkSubmittedConferencesCV>", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/RelatedWorkSubmittedConferencesCV_{ResourceID}_{item3.Roh_relatedWorkSubmittedConferencesCV.ArticleID}", "http://www.w3.org/2000/01/rdf-schema#label", $"\"http://w3id.org/roh/RelatedWorkSubmittedConferencesCV\"", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}{ResourceID}", "http://gnoss/hasEntidad", $"<{resourceAPI.GraphsUrl}items/RelatedWorkSubmittedConferencesCV_{ResourceID}_{item3.Roh_relatedWorkSubmittedConferencesCV.ArticleID}>", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/RelatedWorkSubmittedConferences_{ResourceID}_{item3.ArticleID}", "http://w3id.org/roh/relatedWorkSubmittedConferencesCV", $"<{resourceAPI.GraphsUrl}items/RelatedWorkSubmittedConferencesCV_{ResourceID}_{item3.Roh_relatedWorkSubmittedConferencesCV.ArticleID}>", list, " . ");
				if(item3.Roh_relatedWorkSubmittedConferencesCV.IdRoh_participationType != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/RelatedWorkSubmittedConferencesCV_{ResourceID}_{item3.Roh_relatedWorkSubmittedConferencesCV.ArticleID}",  "http://w3id.org/roh/participationType", $"<{item3.Roh_relatedWorkSubmittedConferencesCV.IdRoh_participationType}>", list, " . ");
				}
				if(item3.Roh_relatedWorkSubmittedConferencesCV.IdRoh_inscriptionType != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/RelatedWorkSubmittedConferencesCV_{ResourceID}_{item3.Roh_relatedWorkSubmittedConferencesCV.ArticleID}",  "http://w3id.org/roh/inscriptionType", $"<{item3.Roh_relatedWorkSubmittedConferencesCV.IdRoh_inscriptionType}>", list, " . ");
				}
				if(item3.Roh_relatedWorkSubmittedConferencesCV.Roh_correspondingAuthor != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/RelatedWorkSubmittedConferencesCV_{ResourceID}_{item3.Roh_relatedWorkSubmittedConferencesCV.ArticleID}",  "http://w3id.org/roh/correspondingAuthor", $"\"{item3.Roh_relatedWorkSubmittedConferencesCV.Roh_correspondingAuthor.ToString()}\"", list, " . ");
				}
				if(item3.Roh_relatedWorkSubmittedConferencesCV.Roh_inscriptionTypeOther != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/RelatedWorkSubmittedConferencesCV_{ResourceID}_{item3.Roh_relatedWorkSubmittedConferencesCV.ArticleID}",  "http://w3id.org/roh/inscriptionTypeOther", $"\"{GenerarTextoSinSaltoDeLinea(item3.Roh_relatedWorkSubmittedConferencesCV.Roh_inscriptionTypeOther)}\"", list, " . ");
				}
			}
				if(item3.IdVivo_relatedBy != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/RelatedWorkSubmittedConferences_{ResourceID}_{item3.ArticleID}",  "http://vivoweb.org/ontology/core#relatedBy", $"<{item3.IdVivo_relatedBy}>", list, " . ");
				}
				if(item3.Roh_isPublic != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/RelatedWorkSubmittedConferences_{ResourceID}_{item3.ArticleID}",  "http://w3id.org/roh/isPublic", $"\"{item3.Roh_isPublic.ToString()}\"", list, " . ");
				}
			}
			}
			if(this.Roh_scientificActivity.Roh_societies != null)
			{
			foreach(var item4 in this.Roh_scientificActivity.Roh_societies)
			{
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/RelatedSociety_{ResourceID}_{item4.ArticleID}", "http://www.w3.org/1999/02/22-rdf-syntax-ns#type", $"<http://w3id.org/roh/RelatedSociety>", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/RelatedSociety_{ResourceID}_{item4.ArticleID}", "http://www.w3.org/2000/01/rdf-schema#label", $"\"http://w3id.org/roh/RelatedSociety\"", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}{ResourceID}", "http://gnoss/hasEntidad", $"<{resourceAPI.GraphsUrl}items/RelatedSociety_{ResourceID}_{item4.ArticleID}>", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/ScientificActivity_{ResourceID}_{this.Roh_scientificActivity.ArticleID}", "http://w3id.org/roh/societies", $"<{resourceAPI.GraphsUrl}items/RelatedSociety_{ResourceID}_{item4.ArticleID}>", list, " . ");
				if(item4.IdVivo_relatedBy != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/RelatedSociety_{ResourceID}_{item4.ArticleID}",  "http://vivoweb.org/ontology/core#relatedBy", $"<{item4.IdVivo_relatedBy}>", list, " . ");
				}
				if(item4.Roh_isPublic != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/RelatedSociety_{ResourceID}_{item4.ArticleID}",  "http://w3id.org/roh/isPublic", $"\"{item4.Roh_isPublic.ToString()}\"", list, " . ");
				}
			}
			}
			if(this.Roh_scientificActivity.Roh_otherDisseminationActivities != null)
			{
			foreach(var item5 in this.Roh_scientificActivity.Roh_otherDisseminationActivities)
			{
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/relatedOtherDisseminationActivity_{ResourceID}_{item5.ArticleID}", "http://www.w3.org/1999/02/22-rdf-syntax-ns#type", $"<http://w3id.org/roh/relatedOtherDisseminationActivity>", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/relatedOtherDisseminationActivity_{ResourceID}_{item5.ArticleID}", "http://www.w3.org/2000/01/rdf-schema#label", $"\"http://w3id.org/roh/relatedOtherDisseminationActivity\"", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}{ResourceID}", "http://gnoss/hasEntidad", $"<{resourceAPI.GraphsUrl}items/relatedOtherDisseminationActivity_{ResourceID}_{item5.ArticleID}>", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/ScientificActivity_{ResourceID}_{this.Roh_scientificActivity.ArticleID}", "http://w3id.org/roh/otherDisseminationActivities", $"<{resourceAPI.GraphsUrl}items/relatedOtherDisseminationActivity_{ResourceID}_{item5.ArticleID}>", list, " . ");
			if(item5.Roh_relatedOtherDisseminationActivityCV != null)
			{
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/RelatedOtherDisseminationActivityCV_{ResourceID}_{item5.Roh_relatedOtherDisseminationActivityCV.ArticleID}", "http://www.w3.org/1999/02/22-rdf-syntax-ns#type", $"<http://w3id.org/roh/RelatedOtherDisseminationActivityCV>", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/RelatedOtherDisseminationActivityCV_{ResourceID}_{item5.Roh_relatedOtherDisseminationActivityCV.ArticleID}", "http://www.w3.org/2000/01/rdf-schema#label", $"\"http://w3id.org/roh/RelatedOtherDisseminationActivityCV\"", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}{ResourceID}", "http://gnoss/hasEntidad", $"<{resourceAPI.GraphsUrl}items/RelatedOtherDisseminationActivityCV_{ResourceID}_{item5.Roh_relatedOtherDisseminationActivityCV.ArticleID}>", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/relatedOtherDisseminationActivity_{ResourceID}_{item5.ArticleID}", "http://w3id.org/roh/relatedOtherDisseminationActivityCV", $"<{resourceAPI.GraphsUrl}items/RelatedOtherDisseminationActivityCV_{ResourceID}_{item5.Roh_relatedOtherDisseminationActivityCV.ArticleID}>", list, " . ");
				if(item5.Roh_relatedOtherDisseminationActivityCV.IdRoh_inscriptionType != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/RelatedOtherDisseminationActivityCV_{ResourceID}_{item5.Roh_relatedOtherDisseminationActivityCV.ArticleID}",  "http://w3id.org/roh/inscriptionType", $"<{item5.Roh_relatedOtherDisseminationActivityCV.IdRoh_inscriptionType}>", list, " . ");
				}
				if(item5.Roh_relatedOtherDisseminationActivityCV.Roh_correspondingAuthor != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/RelatedOtherDisseminationActivityCV_{ResourceID}_{item5.Roh_relatedOtherDisseminationActivityCV.ArticleID}",  "http://w3id.org/roh/correspondingAuthor", $"\"{item5.Roh_relatedOtherDisseminationActivityCV.Roh_correspondingAuthor.ToString()}\"", list, " . ");
				}
				if(item5.Roh_relatedOtherDisseminationActivityCV.Roh_inscriptionTypeOther != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/RelatedOtherDisseminationActivityCV_{ResourceID}_{item5.Roh_relatedOtherDisseminationActivityCV.ArticleID}",  "http://w3id.org/roh/inscriptionTypeOther", $"\"{GenerarTextoSinSaltoDeLinea(item5.Roh_relatedOtherDisseminationActivityCV.Roh_inscriptionTypeOther)}\"", list, " . ");
				}
			}
				if(item5.IdVivo_relatedBy != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/relatedOtherDisseminationActivity_{ResourceID}_{item5.ArticleID}",  "http://vivoweb.org/ontology/core#relatedBy", $"<{item5.IdVivo_relatedBy}>", list, " . ");
				}
				if(item5.Roh_isPublic != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/relatedOtherDisseminationActivity_{ResourceID}_{item5.ArticleID}",  "http://w3id.org/roh/isPublic", $"\"{item5.Roh_isPublic.ToString()}\"", list, " . ");
				}
			}
			}
			if(this.Roh_scientificActivity.Roh_councils != null)
			{
			foreach(var item6 in this.Roh_scientificActivity.Roh_councils)
			{
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/RelatedCouncil_{ResourceID}_{item6.ArticleID}", "http://www.w3.org/1999/02/22-rdf-syntax-ns#type", $"<http://w3id.org/roh/RelatedCouncil>", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/RelatedCouncil_{ResourceID}_{item6.ArticleID}", "http://www.w3.org/2000/01/rdf-schema#label", $"\"http://w3id.org/roh/RelatedCouncil\"", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}{ResourceID}", "http://gnoss/hasEntidad", $"<{resourceAPI.GraphsUrl}items/RelatedCouncil_{ResourceID}_{item6.ArticleID}>", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/ScientificActivity_{ResourceID}_{this.Roh_scientificActivity.ArticleID}", "http://w3id.org/roh/councils", $"<{resourceAPI.GraphsUrl}items/RelatedCouncil_{ResourceID}_{item6.ArticleID}>", list, " . ");
				if(item6.IdVivo_relatedBy != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/RelatedCouncil_{ResourceID}_{item6.ArticleID}",  "http://vivoweb.org/ontology/core#relatedBy", $"<{item6.IdVivo_relatedBy}>", list, " . ");
				}
				if(item6.Roh_isPublic != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/RelatedCouncil_{ResourceID}_{item6.ArticleID}",  "http://w3id.org/roh/isPublic", $"\"{item6.Roh_isPublic.ToString()}\"", list, " . ");
				}
			}
			}
			if(this.Roh_scientificActivity.Roh_forums != null)
			{
			foreach(var item7 in this.Roh_scientificActivity.Roh_forums)
			{
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/RelatedForum_{ResourceID}_{item7.ArticleID}", "http://www.w3.org/1999/02/22-rdf-syntax-ns#type", $"<http://w3id.org/roh/RelatedForum>", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/RelatedForum_{ResourceID}_{item7.ArticleID}", "http://www.w3.org/2000/01/rdf-schema#label", $"\"http://w3id.org/roh/RelatedForum\"", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}{ResourceID}", "http://gnoss/hasEntidad", $"<{resourceAPI.GraphsUrl}items/RelatedForum_{ResourceID}_{item7.ArticleID}>", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/ScientificActivity_{ResourceID}_{this.Roh_scientificActivity.ArticleID}", "http://w3id.org/roh/forums", $"<{resourceAPI.GraphsUrl}items/RelatedForum_{ResourceID}_{item7.ArticleID}>", list, " . ");
			if(item7.Roh_relatedForumCV != null)
			{
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/RelatedForumCV_{ResourceID}_{item7.Roh_relatedForumCV.ArticleID}", "http://www.w3.org/1999/02/22-rdf-syntax-ns#type", $"<http://w3id.org/roh/RelatedForumCV>", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/RelatedForumCV_{ResourceID}_{item7.Roh_relatedForumCV.ArticleID}", "http://www.w3.org/2000/01/rdf-schema#label", $"\"http://w3id.org/roh/RelatedForumCV\"", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}{ResourceID}", "http://gnoss/hasEntidad", $"<{resourceAPI.GraphsUrl}items/RelatedForumCV_{ResourceID}_{item7.Roh_relatedForumCV.ArticleID}>", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/RelatedForum_{ResourceID}_{item7.ArticleID}", "http://w3id.org/roh/relatedForumCV", $"<{resourceAPI.GraphsUrl}items/RelatedForumCV_{ResourceID}_{item7.Roh_relatedForumCV.ArticleID}>", list, " . ");
				if(item7.Roh_relatedForumCV.IdRoh_represents != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/RelatedForumCV_{ResourceID}_{item7.Roh_relatedForumCV.ArticleID}",  "http://w3id.org/roh/represents", $"<{item7.Roh_relatedForumCV.IdRoh_represents}>", list, " . ");
				}
				if(item7.Roh_relatedForumCV.Vivo_start != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/RelatedForumCV_{ResourceID}_{item7.Roh_relatedForumCV.ArticleID}",  "http://vivoweb.org/ontology/core#start", $"\"{item7.Roh_relatedForumCV.Vivo_start.Value.ToString("yyyyMMddHHmmss")}\"", list, " . ");
				}
				if(item7.Roh_relatedForumCV.Roh_professionalCategory != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/RelatedForumCV_{ResourceID}_{item7.Roh_relatedForumCV.ArticleID}",  "http://w3id.org/roh/professionalCategory", $"\"{GenerarTextoSinSaltoDeLinea(item7.Roh_relatedForumCV.Roh_professionalCategory)}\"", list, " . ");
				}
				if(item7.Roh_relatedForumCV.Vivo_end != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/RelatedForumCV_{ResourceID}_{item7.Roh_relatedForumCV.ArticleID}",  "http://vivoweb.org/ontology/core#end", $"\"{item7.Roh_relatedForumCV.Vivo_end.Value.ToString("yyyyMMddHHmmss")}\"", list, " . ");
				}
			}
				if(item7.IdVivo_relatedBy != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/RelatedForum_{ResourceID}_{item7.ArticleID}",  "http://vivoweb.org/ontology/core#relatedBy", $"<{item7.IdVivo_relatedBy}>", list, " . ");
				}
				if(item7.Roh_isPublic != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/RelatedForum_{ResourceID}_{item7.ArticleID}",  "http://w3id.org/roh/isPublic", $"\"{item7.Roh_isPublic.ToString()}\"", list, " . ");
				}
			}
			}
			if(this.Roh_scientificActivity.Roh_researchActivityPeriods != null)
			{
			foreach(var item8 in this.Roh_scientificActivity.Roh_researchActivityPeriods)
			{
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/RelatedResearchActivityPeriod_{ResourceID}_{item8.ArticleID}", "http://www.w3.org/1999/02/22-rdf-syntax-ns#type", $"<http://w3id.org/roh/RelatedResearchActivityPeriod>", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/RelatedResearchActivityPeriod_{ResourceID}_{item8.ArticleID}", "http://www.w3.org/2000/01/rdf-schema#label", $"\"http://w3id.org/roh/RelatedResearchActivityPeriod\"", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}{ResourceID}", "http://gnoss/hasEntidad", $"<{resourceAPI.GraphsUrl}items/RelatedResearchActivityPeriod_{ResourceID}_{item8.ArticleID}>", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/ScientificActivity_{ResourceID}_{this.Roh_scientificActivity.ArticleID}", "http://w3id.org/roh/researchActivityPeriods", $"<{resourceAPI.GraphsUrl}items/RelatedResearchActivityPeriod_{ResourceID}_{item8.ArticleID}>", list, " . ");
				if(item8.IdVivo_relatedBy != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/RelatedResearchActivityPeriod_{ResourceID}_{item8.ArticleID}",  "http://vivoweb.org/ontology/core#relatedBy", $"<{item8.IdVivo_relatedBy}>", list, " . ");
				}
				if(item8.Roh_isPublic != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/RelatedResearchActivityPeriod_{ResourceID}_{item8.ArticleID}",  "http://w3id.org/roh/isPublic", $"\"{item8.Roh_isPublic.ToString()}\"", list, " . ");
				}
			}
			}
			if(this.Roh_scientificActivity.Roh_scientificPublications != null)
			{
			foreach(var item9 in this.Roh_scientificActivity.Roh_scientificPublications)
			{
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/RelatedScientificPublication_{ResourceID}_{item9.ArticleID}", "http://www.w3.org/1999/02/22-rdf-syntax-ns#type", $"<http://w3id.org/roh/RelatedScientificPublication>", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/RelatedScientificPublication_{ResourceID}_{item9.ArticleID}", "http://www.w3.org/2000/01/rdf-schema#label", $"\"http://w3id.org/roh/RelatedScientificPublication\"", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}{ResourceID}", "http://gnoss/hasEntidad", $"<{resourceAPI.GraphsUrl}items/RelatedScientificPublication_{ResourceID}_{item9.ArticleID}>", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/ScientificActivity_{ResourceID}_{this.Roh_scientificActivity.ArticleID}", "http://w3id.org/roh/scientificPublications", $"<{resourceAPI.GraphsUrl}items/RelatedScientificPublication_{ResourceID}_{item9.ArticleID}>", list, " . ");
			if(item9.Roh_relatedScientificPublicationCV != null)
			{
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/RelatedScientificPublicationCV_{ResourceID}_{item9.Roh_relatedScientificPublicationCV.ArticleID}", "http://www.w3.org/1999/02/22-rdf-syntax-ns#type", $"<http://w3id.org/roh/RelatedScientificPublicationCV>", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/RelatedScientificPublicationCV_{ResourceID}_{item9.Roh_relatedScientificPublicationCV.ArticleID}", "http://www.w3.org/2000/01/rdf-schema#label", $"\"http://w3id.org/roh/RelatedScientificPublicationCV\"", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}{ResourceID}", "http://gnoss/hasEntidad", $"<{resourceAPI.GraphsUrl}items/RelatedScientificPublicationCV_{ResourceID}_{item9.Roh_relatedScientificPublicationCV.ArticleID}>", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/RelatedScientificPublication_{ResourceID}_{item9.ArticleID}", "http://w3id.org/roh/relatedScientificPublicationCV", $"<{resourceAPI.GraphsUrl}items/RelatedScientificPublicationCV_{ResourceID}_{item9.Roh_relatedScientificPublicationCV.ArticleID}>", list, " . ");
				if(item9.Roh_relatedScientificPublicationCV.IdRoh_contributionGrade != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/RelatedScientificPublicationCV_{ResourceID}_{item9.Roh_relatedScientificPublicationCV.ArticleID}",  "http://w3id.org/roh/contributionGrade", $"<{item9.Roh_relatedScientificPublicationCV.IdRoh_contributionGrade}>", list, " . ");
				}
				if(item9.Roh_relatedScientificPublicationCV.Roh_relevantResults != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/RelatedScientificPublicationCV_{ResourceID}_{item9.Roh_relatedScientificPublicationCV.ArticleID}",  "http://w3id.org/roh/relevantResults", $"\"{GenerarTextoSinSaltoDeLinea(item9.Roh_relatedScientificPublicationCV.Roh_relevantResults)}\"", list, " . ");
				}
				if(item9.Roh_relatedScientificPublicationCV.Roh_correspondingAuthor != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/RelatedScientificPublicationCV_{ResourceID}_{item9.Roh_relatedScientificPublicationCV.ArticleID}",  "http://w3id.org/roh/correspondingAuthor", $"\"{item9.Roh_relatedScientificPublicationCV.Roh_correspondingAuthor.ToString()}\"", list, " . ");
				}
				if(item9.Roh_relatedScientificPublicationCV.Roh_relevantPublication != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/RelatedScientificPublicationCV_{ResourceID}_{item9.Roh_relatedScientificPublicationCV.ArticleID}",  "http://w3id.org/roh/relevantPublication", $"\"{item9.Roh_relatedScientificPublicationCV.Roh_relevantPublication.ToString()}\"", list, " . ");
				}
			}
				if(item9.IdVivo_relatedBy != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/RelatedScientificPublication_{ResourceID}_{item9.ArticleID}",  "http://vivoweb.org/ontology/core#relatedBy", $"<{item9.IdVivo_relatedBy}>", list, " . ");
				}
				if(item9.Roh_isPublic != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/RelatedScientificPublication_{ResourceID}_{item9.ArticleID}",  "http://w3id.org/roh/isPublic", $"\"{item9.Roh_isPublic.ToString()}\"", list, " . ");
				}
			}
			}
			if(this.Roh_scientificActivity.Roh_otherAchievements != null)
			{
			foreach(var item10 in this.Roh_scientificActivity.Roh_otherAchievements)
			{
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/RelatedOtherAchievement_{ResourceID}_{item10.ArticleID}", "http://www.w3.org/1999/02/22-rdf-syntax-ns#type", $"<http://w3id.org/roh/RelatedOtherAchievement>", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/RelatedOtherAchievement_{ResourceID}_{item10.ArticleID}", "http://www.w3.org/2000/01/rdf-schema#label", $"\"http://w3id.org/roh/RelatedOtherAchievement\"", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}{ResourceID}", "http://gnoss/hasEntidad", $"<{resourceAPI.GraphsUrl}items/RelatedOtherAchievement_{ResourceID}_{item10.ArticleID}>", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/ScientificActivity_{ResourceID}_{this.Roh_scientificActivity.ArticleID}", "http://w3id.org/roh/otherAchievements", $"<{resourceAPI.GraphsUrl}items/RelatedOtherAchievement_{ResourceID}_{item10.ArticleID}>", list, " . ");
				if(item10.IdVivo_relatedBy != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/RelatedOtherAchievement_{ResourceID}_{item10.ArticleID}",  "http://vivoweb.org/ontology/core#relatedBy", $"<{item10.IdVivo_relatedBy}>", list, " . ");
				}
				if(item10.Roh_isPublic != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/RelatedOtherAchievement_{ResourceID}_{item10.ArticleID}",  "http://w3id.org/roh/isPublic", $"\"{item10.Roh_isPublic.ToString()}\"", list, " . ");
				}
			}
			}
			if(this.Roh_scientificActivity.Roh_researchObjects != null)
			{
			foreach(var item11 in this.Roh_scientificActivity.Roh_researchObjects)
			{
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/RelatedResearchObject_{ResourceID}_{item11.ArticleID}", "http://www.w3.org/1999/02/22-rdf-syntax-ns#type", $"<http://w3id.org/roh/RelatedResearchObject>", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/RelatedResearchObject_{ResourceID}_{item11.ArticleID}", "http://www.w3.org/2000/01/rdf-schema#label", $"\"http://w3id.org/roh/RelatedResearchObject\"", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}{ResourceID}", "http://gnoss/hasEntidad", $"<{resourceAPI.GraphsUrl}items/RelatedResearchObject_{ResourceID}_{item11.ArticleID}>", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/ScientificActivity_{ResourceID}_{this.Roh_scientificActivity.ArticleID}", "http://w3id.org/roh/researchObjects", $"<{resourceAPI.GraphsUrl}items/RelatedResearchObject_{ResourceID}_{item11.ArticleID}>", list, " . ");
				if(item11.IdVivo_relatedBy != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/RelatedResearchObject_{ResourceID}_{item11.ArticleID}",  "http://vivoweb.org/ontology/core#relatedBy", $"<{item11.IdVivo_relatedBy}>", list, " . ");
				}
				if(item11.Roh_isPublic != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/RelatedResearchObject_{ResourceID}_{item11.ArticleID}",  "http://w3id.org/roh/isPublic", $"\"{item11.Roh_isPublic.ToString()}\"", list, " . ");
				}
			}
			}
			if(this.Roh_scientificActivity.Roh_researchEvaluations != null)
			{
			foreach(var item12 in this.Roh_scientificActivity.Roh_researchEvaluations)
			{
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/RelatedResearchEvaluation_{ResourceID}_{item12.ArticleID}", "http://www.w3.org/1999/02/22-rdf-syntax-ns#type", $"<http://w3id.org/roh/RelatedResearchEvaluation>", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/RelatedResearchEvaluation_{ResourceID}_{item12.ArticleID}", "http://www.w3.org/2000/01/rdf-schema#label", $"\"http://w3id.org/roh/RelatedResearchEvaluation\"", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}{ResourceID}", "http://gnoss/hasEntidad", $"<{resourceAPI.GraphsUrl}items/RelatedResearchEvaluation_{ResourceID}_{item12.ArticleID}>", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/ScientificActivity_{ResourceID}_{this.Roh_scientificActivity.ArticleID}", "http://w3id.org/roh/researchEvaluations", $"<{resourceAPI.GraphsUrl}items/RelatedResearchEvaluation_{ResourceID}_{item12.ArticleID}>", list, " . ");
			if(item12.Roh_relatedResearchEvaluationCV != null)
			{
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/RelatedResearchEvaluationCV_{ResourceID}_{item12.Roh_relatedResearchEvaluationCV.ArticleID}", "http://www.w3.org/1999/02/22-rdf-syntax-ns#type", $"<http://w3id.org/roh/RelatedResearchEvaluationCV>", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/RelatedResearchEvaluationCV_{ResourceID}_{item12.Roh_relatedResearchEvaluationCV.ArticleID}", "http://www.w3.org/2000/01/rdf-schema#label", $"\"http://w3id.org/roh/RelatedResearchEvaluationCV\"", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}{ResourceID}", "http://gnoss/hasEntidad", $"<{resourceAPI.GraphsUrl}items/RelatedResearchEvaluationCV_{ResourceID}_{item12.Roh_relatedResearchEvaluationCV.ArticleID}>", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/RelatedResearchEvaluation_{ResourceID}_{item12.ArticleID}", "http://w3id.org/roh/relatedResearchEvaluationCV", $"<{resourceAPI.GraphsUrl}items/RelatedResearchEvaluationCV_{ResourceID}_{item12.Roh_relatedResearchEvaluationCV.ArticleID}>", list, " . ");
				if(item12.Roh_relatedResearchEvaluationCV.IdRoh_accessSystem != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/RelatedResearchEvaluationCV_{ResourceID}_{item12.Roh_relatedResearchEvaluationCV.ArticleID}",  "http://w3id.org/roh/accessSystem", $"<{item12.Roh_relatedResearchEvaluationCV.IdRoh_accessSystem}>", list, " . ");
				}
				if(item12.Roh_relatedResearchEvaluationCV.Vivo_start != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/RelatedResearchEvaluationCV_{ResourceID}_{item12.Roh_relatedResearchEvaluationCV.ArticleID}",  "http://vivoweb.org/ontology/core#start", $"\"{item12.Roh_relatedResearchEvaluationCV.Vivo_start.Value.ToString("yyyyMMddHHmmss")}\"", list, " . ");
				}
				if(item12.Roh_relatedResearchEvaluationCV.Roh_frequency != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/RelatedResearchEvaluationCV_{ResourceID}_{item12.Roh_relatedResearchEvaluationCV.ArticleID}",  "http://w3id.org/roh/frequency", $"{item12.Roh_relatedResearchEvaluationCV.Roh_frequency.Value.ToString(new CultureInfo("en-US"))}", list, " . ");
				}
				if(item12.Roh_relatedResearchEvaluationCV.Roh_performedTasks != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/RelatedResearchEvaluationCV_{ResourceID}_{item12.Roh_relatedResearchEvaluationCV.ArticleID}",  "http://w3id.org/roh/performedTasks", $"\"{GenerarTextoSinSaltoDeLinea(item12.Roh_relatedResearchEvaluationCV.Roh_performedTasks)}\"", list, " . ");
				}
				if(item12.Roh_relatedResearchEvaluationCV.Vivo_end != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/RelatedResearchEvaluationCV_{ResourceID}_{item12.Roh_relatedResearchEvaluationCV.ArticleID}",  "http://vivoweb.org/ontology/core#end", $"\"{item12.Roh_relatedResearchEvaluationCV.Vivo_end.Value.ToString("yyyyMMddHHmmss")}\"", list, " . ");
				}
			}
				if(item12.IdVivo_relatedBy != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/RelatedResearchEvaluation_{ResourceID}_{item12.ArticleID}",  "http://vivoweb.org/ontology/core#relatedBy", $"<{item12.IdVivo_relatedBy}>", list, " . ");
				}
				if(item12.Roh_isPublic != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/RelatedResearchEvaluation_{ResourceID}_{item12.ArticleID}",  "http://w3id.org/roh/isPublic", $"\"{item12.Roh_isPublic.ToString()}\"", list, " . ");
				}
			}
			}
			if(this.Roh_scientificActivity.Roh_activitiesOrganization != null)
			{
			foreach(var item13 in this.Roh_scientificActivity.Roh_activitiesOrganization)
			{
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/RelatedActivityOrganization_{ResourceID}_{item13.ArticleID}", "http://www.w3.org/1999/02/22-rdf-syntax-ns#type", $"<http://w3id.org/roh/RelatedActivityOrganization>", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/RelatedActivityOrganization_{ResourceID}_{item13.ArticleID}", "http://www.w3.org/2000/01/rdf-schema#label", $"\"http://w3id.org/roh/RelatedActivityOrganization\"", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}{ResourceID}", "http://gnoss/hasEntidad", $"<{resourceAPI.GraphsUrl}items/RelatedActivityOrganization_{ResourceID}_{item13.ArticleID}>", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/ScientificActivity_{ResourceID}_{this.Roh_scientificActivity.ArticleID}", "http://w3id.org/roh/activitiesOrganization", $"<{resourceAPI.GraphsUrl}items/RelatedActivityOrganization_{ResourceID}_{item13.ArticleID}>", list, " . ");
			if(item13.Roh_relatedActivityOrganizationCV != null)
			{
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/RelatedActivityOrganizationCV_{ResourceID}_{item13.Roh_relatedActivityOrganizationCV.ArticleID}", "http://www.w3.org/1999/02/22-rdf-syntax-ns#type", $"<http://w3id.org/roh/RelatedActivityOrganizationCV>", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/RelatedActivityOrganizationCV_{ResourceID}_{item13.Roh_relatedActivityOrganizationCV.ArticleID}", "http://www.w3.org/2000/01/rdf-schema#label", $"\"http://w3id.org/roh/RelatedActivityOrganizationCV\"", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}{ResourceID}", "http://gnoss/hasEntidad", $"<{resourceAPI.GraphsUrl}items/RelatedActivityOrganizationCV_{ResourceID}_{item13.Roh_relatedActivityOrganizationCV.ArticleID}>", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/RelatedActivityOrganization_{ResourceID}_{item13.ArticleID}", "http://w3id.org/roh/relatedActivityOrganizationCV", $"<{resourceAPI.GraphsUrl}items/RelatedActivityOrganizationCV_{ResourceID}_{item13.Roh_relatedActivityOrganizationCV.ArticleID}>", list, " . ");
				if(item13.Roh_relatedActivityOrganizationCV.IdRoh_participationType != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/RelatedActivityOrganizationCV_{ResourceID}_{item13.Roh_relatedActivityOrganizationCV.ArticleID}",  "http://w3id.org/roh/participationType", $"<{item13.Roh_relatedActivityOrganizationCV.IdRoh_participationType}>", list, " . ");
				}
				if(item13.Roh_relatedActivityOrganizationCV.Roh_durationDays != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/RelatedActivityOrganizationCV_{ResourceID}_{item13.Roh_relatedActivityOrganizationCV.ArticleID}",  "http://w3id.org/roh/durationDays", $"\"{GenerarTextoSinSaltoDeLinea(item13.Roh_relatedActivityOrganizationCV.Roh_durationDays)}\"", list, " . ");
				}
				if(item13.Roh_relatedActivityOrganizationCV.Vivo_start != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/RelatedActivityOrganizationCV_{ResourceID}_{item13.Roh_relatedActivityOrganizationCV.ArticleID}",  "http://vivoweb.org/ontology/core#start", $"\"{item13.Roh_relatedActivityOrganizationCV.Vivo_start.Value.ToString("yyyyMMddHHmmss")}\"", list, " . ");
				}
				if(item13.Roh_relatedActivityOrganizationCV.Roh_durationMonths != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/RelatedActivityOrganizationCV_{ResourceID}_{item13.Roh_relatedActivityOrganizationCV.ArticleID}",  "http://w3id.org/roh/durationMonths", $"\"{GenerarTextoSinSaltoDeLinea(item13.Roh_relatedActivityOrganizationCV.Roh_durationMonths)}\"", list, " . ");
				}
				if(item13.Roh_relatedActivityOrganizationCV.Roh_participationTypeOther != null)
				{
					foreach(var item2 in item13.Roh_relatedActivityOrganizationCV.Roh_participationTypeOther)
					{
						AgregarTripleALista($"{resourceAPI.GraphsUrl}items/RelatedActivityOrganizationCV_{ResourceID}_{item13.Roh_relatedActivityOrganizationCV.ArticleID}", "http://w3id.org/roh/participationTypeOther", $"\"{GenerarTextoSinSaltoDeLinea(item2)}\"", list, " . ");
					}
				}
				if(item13.Roh_relatedActivityOrganizationCV.Roh_durationYears != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/RelatedActivityOrganizationCV_{ResourceID}_{item13.Roh_relatedActivityOrganizationCV.ArticleID}",  "http://w3id.org/roh/durationYears", $"\"{GenerarTextoSinSaltoDeLinea(item13.Roh_relatedActivityOrganizationCV.Roh_durationYears)}\"", list, " . ");
				}
				if(item13.Roh_relatedActivityOrganizationCV.Vivo_end != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/RelatedActivityOrganizationCV_{ResourceID}_{item13.Roh_relatedActivityOrganizationCV.ArticleID}",  "http://vivoweb.org/ontology/core#end", $"\"{item13.Roh_relatedActivityOrganizationCV.Vivo_end.Value.ToString("yyyyMMddHHmmss")}\"", list, " . ");
				}
			}
				if(item13.IdVivo_relatedBy != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/RelatedActivityOrganization_{ResourceID}_{item13.ArticleID}",  "http://vivoweb.org/ontology/core#relatedBy", $"<{item13.IdVivo_relatedBy}>", list, " . ");
				}
				if(item13.Roh_isPublic != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/RelatedActivityOrganization_{ResourceID}_{item13.ArticleID}",  "http://w3id.org/roh/isPublic", $"\"{item13.Roh_isPublic.ToString()}\"", list, " . ");
				}
			}
			}
			if(this.Roh_scientificActivity.Roh_otherCollaborations != null)
			{
			foreach(var item14 in this.Roh_scientificActivity.Roh_otherCollaborations)
			{
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/RelatedOtherCollaboration_{ResourceID}_{item14.ArticleID}", "http://www.w3.org/1999/02/22-rdf-syntax-ns#type", $"<http://w3id.org/roh/RelatedOtherCollaboration>", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/RelatedOtherCollaboration_{ResourceID}_{item14.ArticleID}", "http://www.w3.org/2000/01/rdf-schema#label", $"\"http://w3id.org/roh/RelatedOtherCollaboration\"", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}{ResourceID}", "http://gnoss/hasEntidad", $"<{resourceAPI.GraphsUrl}items/RelatedOtherCollaboration_{ResourceID}_{item14.ArticleID}>", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/ScientificActivity_{ResourceID}_{this.Roh_scientificActivity.ArticleID}", "http://w3id.org/roh/otherCollaborations", $"<{resourceAPI.GraphsUrl}items/RelatedOtherCollaboration_{ResourceID}_{item14.ArticleID}>", list, " . ");
				if(item14.IdVivo_relatedBy != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/RelatedOtherCollaboration_{ResourceID}_{item14.ArticleID}",  "http://vivoweb.org/ontology/core#relatedBy", $"<{item14.IdVivo_relatedBy}>", list, " . ");
				}
				if(item14.Roh_isPublic != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/RelatedOtherCollaboration_{ResourceID}_{item14.ArticleID}",  "http://w3id.org/roh/isPublic", $"\"{item14.Roh_isPublic.ToString()}\"", list, " . ");
				}
			}
			}
			if(this.Roh_scientificActivity.Roh_networks != null)
			{
			foreach(var item15 in this.Roh_scientificActivity.Roh_networks)
			{
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/RelatedNetwork_{ResourceID}_{item15.ArticleID}", "http://www.w3.org/1999/02/22-rdf-syntax-ns#type", $"<http://w3id.org/roh/RelatedNetwork>", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/RelatedNetwork_{ResourceID}_{item15.ArticleID}", "http://www.w3.org/2000/01/rdf-schema#label", $"\"http://w3id.org/roh/RelatedNetwork\"", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}{ResourceID}", "http://gnoss/hasEntidad", $"<{resourceAPI.GraphsUrl}items/RelatedNetwork_{ResourceID}_{item15.ArticleID}>", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/ScientificActivity_{ResourceID}_{this.Roh_scientificActivity.ArticleID}", "http://w3id.org/roh/networks", $"<{resourceAPI.GraphsUrl}items/RelatedNetwork_{ResourceID}_{item15.ArticleID}>", list, " . ");
				if(item15.IdVivo_relatedBy != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/RelatedNetwork_{ResourceID}_{item15.ArticleID}",  "http://vivoweb.org/ontology/core#relatedBy", $"<{item15.IdVivo_relatedBy}>", list, " . ");
				}
				if(item15.Roh_isPublic != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/RelatedNetwork_{ResourceID}_{item15.ArticleID}",  "http://w3id.org/roh/isPublic", $"\"{item15.Roh_isPublic.ToString()}\"", list, " . ");
				}
			}
			}
			if(this.Roh_scientificActivity.Roh_grants != null)
			{
			foreach(var item16 in this.Roh_scientificActivity.Roh_grants)
			{
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/RelatedGrant_{ResourceID}_{item16.ArticleID}", "http://www.w3.org/1999/02/22-rdf-syntax-ns#type", $"<http://w3id.org/roh/RelatedGrant>", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/RelatedGrant_{ResourceID}_{item16.ArticleID}", "http://www.w3.org/2000/01/rdf-schema#label", $"\"http://w3id.org/roh/RelatedGrant\"", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}{ResourceID}", "http://gnoss/hasEntidad", $"<{resourceAPI.GraphsUrl}items/RelatedGrant_{ResourceID}_{item16.ArticleID}>", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/ScientificActivity_{ResourceID}_{this.Roh_scientificActivity.ArticleID}", "http://w3id.org/roh/grants", $"<{resourceAPI.GraphsUrl}items/RelatedGrant_{ResourceID}_{item16.ArticleID}>", list, " . ");
				if(item16.IdVivo_relatedBy != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/RelatedGrant_{ResourceID}_{item16.ArticleID}",  "http://vivoweb.org/ontology/core#relatedBy", $"<{item16.IdVivo_relatedBy}>", list, " . ");
				}
				if(item16.Roh_isPublic != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/RelatedGrant_{ResourceID}_{item16.ArticleID}",  "http://w3id.org/roh/isPublic", $"\"{item16.Roh_isPublic.ToString()}\"", list, " . ");
				}
			}
			}
			if(this.Roh_scientificActivity.Roh_activitiesManagement != null)
			{
			foreach(var item17 in this.Roh_scientificActivity.Roh_activitiesManagement)
			{
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/RelatedActivityManagement_{ResourceID}_{item17.ArticleID}", "http://www.w3.org/1999/02/22-rdf-syntax-ns#type", $"<http://w3id.org/roh/RelatedActivityManagement>", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/RelatedActivityManagement_{ResourceID}_{item17.ArticleID}", "http://www.w3.org/2000/01/rdf-schema#label", $"\"http://w3id.org/roh/RelatedActivityManagement\"", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}{ResourceID}", "http://gnoss/hasEntidad", $"<{resourceAPI.GraphsUrl}items/RelatedActivityManagement_{ResourceID}_{item17.ArticleID}>", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/ScientificActivity_{ResourceID}_{this.Roh_scientificActivity.ArticleID}", "http://w3id.org/roh/activitiesManagement", $"<{resourceAPI.GraphsUrl}items/RelatedActivityManagement_{ResourceID}_{item17.ArticleID}>", list, " . ");
			if(item17.Roh_relatedActivityManagementCV != null)
			{
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/RelatedActivityManagementCV_{ResourceID}_{item17.Roh_relatedActivityManagementCV.ArticleID}", "http://www.w3.org/1999/02/22-rdf-syntax-ns#type", $"<http://w3id.org/roh/RelatedActivityManagementCV>", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/RelatedActivityManagementCV_{ResourceID}_{item17.Roh_relatedActivityManagementCV.ArticleID}", "http://www.w3.org/2000/01/rdf-schema#label", $"\"http://w3id.org/roh/RelatedActivityManagementCV\"", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}{ResourceID}", "http://gnoss/hasEntidad", $"<{resourceAPI.GraphsUrl}items/RelatedActivityManagementCV_{ResourceID}_{item17.Roh_relatedActivityManagementCV.ArticleID}>", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/RelatedActivityManagement_{ResourceID}_{item17.ArticleID}", "http://w3id.org/roh/relatedActivityManagementCV", $"<{resourceAPI.GraphsUrl}items/RelatedActivityManagementCV_{ResourceID}_{item17.Roh_relatedActivityManagementCV.ArticleID}>", list, " . ");
			if(item17.Roh_relatedActivityManagementCV.Roh_hasKnowledgeArea != null)
			{
			foreach(var item19 in item17.Roh_relatedActivityManagementCV.Roh_hasKnowledgeArea)
			{
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/CategoryPath_{ResourceID}_{item19.ArticleID}", "http://www.w3.org/1999/02/22-rdf-syntax-ns#type", $"<http://w3id.org/roh/CategoryPath>", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/CategoryPath_{ResourceID}_{item19.ArticleID}", "http://www.w3.org/2000/01/rdf-schema#label", $"\"http://w3id.org/roh/CategoryPath\"", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}{ResourceID}", "http://gnoss/hasEntidad", $"<{resourceAPI.GraphsUrl}items/CategoryPath_{ResourceID}_{item19.ArticleID}>", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/RelatedActivityManagementCV_{ResourceID}_{item17.Roh_relatedActivityManagementCV.ArticleID}", "http://w3id.org/roh/hasKnowledgeArea", $"<{resourceAPI.GraphsUrl}items/CategoryPath_{ResourceID}_{item19.ArticleID}>", list, " . ");
				if(item19.IdsRoh_categoryNode != null)
				{
					foreach(var item2 in item19.IdsRoh_categoryNode)
					{
						AgregarTripleALista($"{resourceAPI.GraphsUrl}items/CategoryPath_{ResourceID}_{item19.ArticleID}", "http://w3id.org/roh/categoryNode",  $"<{item2}>", list, " . ");
					}
				}
			}
			}
				if(item17.Roh_relatedActivityManagementCV.IdRoh_accessSystem != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/RelatedActivityManagementCV_{ResourceID}_{item17.Roh_relatedActivityManagementCV.ArticleID}",  "http://w3id.org/roh/accessSystem", $"<{item17.Roh_relatedActivityManagementCV.IdRoh_accessSystem}>", list, " . ");
				}
				if(item17.Roh_relatedActivityManagementCV.Roh_specificTasks != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/RelatedActivityManagementCV_{ResourceID}_{item17.Roh_relatedActivityManagementCV.ArticleID}",  "http://w3id.org/roh/specificTasks", $"\"{GenerarTextoSinSaltoDeLinea(item17.Roh_relatedActivityManagementCV.Roh_specificTasks)}\"", list, " . ");
				}
				if(item17.Roh_relatedActivityManagementCV.Roh_durationDays != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/RelatedActivityManagementCV_{ResourceID}_{item17.Roh_relatedActivityManagementCV.ArticleID}",  "http://w3id.org/roh/durationDays", $"\"{GenerarTextoSinSaltoDeLinea(item17.Roh_relatedActivityManagementCV.Roh_durationDays)}\"", list, " . ");
				}
				if(item17.Roh_relatedActivityManagementCV.Vivo_start != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/RelatedActivityManagementCV_{ResourceID}_{item17.Roh_relatedActivityManagementCV.ArticleID}",  "http://vivoweb.org/ontology/core#start", $"\"{item17.Roh_relatedActivityManagementCV.Vivo_start.Value.ToString("yyyyMMddHHmmss")}\"", list, " . ");
				}
				if(item17.Roh_relatedActivityManagementCV.Roh_goals != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/RelatedActivityManagementCV_{ResourceID}_{item17.Roh_relatedActivityManagementCV.ArticleID}",  "http://w3id.org/roh/goals", $"\"{GenerarTextoSinSaltoDeLinea(item17.Roh_relatedActivityManagementCV.Roh_goals)}\"", list, " . ");
				}
				if(item17.Roh_relatedActivityManagementCV.Roh_performedTasks != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/RelatedActivityManagementCV_{ResourceID}_{item17.Roh_relatedActivityManagementCV.ArticleID}",  "http://w3id.org/roh/performedTasks", $"\"{GenerarTextoSinSaltoDeLinea(item17.Roh_relatedActivityManagementCV.Roh_performedTasks)}\"", list, " . ");
				}
				if(item17.Roh_relatedActivityManagementCV.Roh_accessSystemOther != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/RelatedActivityManagementCV_{ResourceID}_{item17.Roh_relatedActivityManagementCV.ArticleID}",  "http://w3id.org/roh/accessSystemOther", $"\"{GenerarTextoSinSaltoDeLinea(item17.Roh_relatedActivityManagementCV.Roh_accessSystemOther)}\"", list, " . ");
				}
				if(item17.Roh_relatedActivityManagementCV.Roh_durationMonths != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/RelatedActivityManagementCV_{ResourceID}_{item17.Roh_relatedActivityManagementCV.ArticleID}",  "http://w3id.org/roh/durationMonths", $"\"{GenerarTextoSinSaltoDeLinea(item17.Roh_relatedActivityManagementCV.Roh_durationMonths)}\"", list, " . ");
				}
				if(item17.Roh_relatedActivityManagementCV.Roh_durationYears != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/RelatedActivityManagementCV_{ResourceID}_{item17.Roh_relatedActivityManagementCV.ArticleID}",  "http://w3id.org/roh/durationYears", $"\"{GenerarTextoSinSaltoDeLinea(item17.Roh_relatedActivityManagementCV.Roh_durationYears)}\"", list, " . ");
				}
				if(item17.Roh_relatedActivityManagementCV.Vivo_end != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/RelatedActivityManagementCV_{ResourceID}_{item17.Roh_relatedActivityManagementCV.ArticleID}",  "http://vivoweb.org/ontology/core#end", $"\"{item17.Roh_relatedActivityManagementCV.Vivo_end.Value.ToString("yyyyMMddHHmmss")}\"", list, " . ");
				}
			}
				if(item17.IdVivo_relatedBy != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/RelatedActivityManagement_{ResourceID}_{item17.ArticleID}",  "http://vivoweb.org/ontology/core#relatedBy", $"<{item17.IdVivo_relatedBy}>", list, " . ");
				}
				if(item17.Roh_isPublic != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/RelatedActivityManagement_{ResourceID}_{item17.ArticleID}",  "http://w3id.org/roh/isPublic", $"\"{item17.Roh_isPublic.ToString()}\"", list, " . ");
				}
			}
			}
			if(this.Roh_scientificActivity.Roh_prizes != null)
			{
			foreach(var item18 in this.Roh_scientificActivity.Roh_prizes)
			{
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/RelatedPrize_{ResourceID}_{item18.ArticleID}", "http://www.w3.org/1999/02/22-rdf-syntax-ns#type", $"<http://w3id.org/roh/RelatedPrize>", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/RelatedPrize_{ResourceID}_{item18.ArticleID}", "http://www.w3.org/2000/01/rdf-schema#label", $"\"http://w3id.org/roh/RelatedPrize\"", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}{ResourceID}", "http://gnoss/hasEntidad", $"<{resourceAPI.GraphsUrl}items/RelatedPrize_{ResourceID}_{item18.ArticleID}>", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/ScientificActivity_{ResourceID}_{this.Roh_scientificActivity.ArticleID}", "http://w3id.org/roh/prizes", $"<{resourceAPI.GraphsUrl}items/RelatedPrize_{ResourceID}_{item18.ArticleID}>", list, " . ");
				if(item18.IdVivo_relatedBy != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/RelatedPrize_{ResourceID}_{item18.ArticleID}",  "http://vivoweb.org/ontology/core#relatedBy", $"<{item18.IdVivo_relatedBy}>", list, " . ");
				}
				if(item18.Roh_isPublic != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/RelatedPrize_{ResourceID}_{item18.ArticleID}",  "http://w3id.org/roh/isPublic", $"\"{item18.Roh_isPublic.ToString()}\"", list, " . ");
				}
			}
			}
			if(this.Roh_scientificActivity.Roh_committees != null)
			{
			foreach(var item19 in this.Roh_scientificActivity.Roh_committees)
			{
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/RelatedCommittee_{ResourceID}_{item19.ArticleID}", "http://www.w3.org/1999/02/22-rdf-syntax-ns#type", $"<http://w3id.org/roh/RelatedCommittee>", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/RelatedCommittee_{ResourceID}_{item19.ArticleID}", "http://www.w3.org/2000/01/rdf-schema#label", $"\"http://w3id.org/roh/RelatedCommittee\"", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}{ResourceID}", "http://gnoss/hasEntidad", $"<{resourceAPI.GraphsUrl}items/RelatedCommittee_{ResourceID}_{item19.ArticleID}>", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/ScientificActivity_{ResourceID}_{this.Roh_scientificActivity.ArticleID}", "http://w3id.org/roh/committees", $"<{resourceAPI.GraphsUrl}items/RelatedCommittee_{ResourceID}_{item19.ArticleID}>", list, " . ");
			if(item19.Roh_relatedCommitteeCV != null)
			{
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/RelatedCommitteeCV_{ResourceID}_{item19.Roh_relatedCommitteeCV.ArticleID}", "http://www.w3.org/1999/02/22-rdf-syntax-ns#type", $"<http://w3id.org/roh/RelatedCommitteeCV>", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/RelatedCommitteeCV_{ResourceID}_{item19.Roh_relatedCommitteeCV.ArticleID}", "http://www.w3.org/2000/01/rdf-schema#label", $"\"http://w3id.org/roh/RelatedCommitteeCV\"", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}{ResourceID}", "http://gnoss/hasEntidad", $"<{resourceAPI.GraphsUrl}items/RelatedCommitteeCV_{ResourceID}_{item19.Roh_relatedCommitteeCV.ArticleID}>", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/RelatedCommittee_{ResourceID}_{item19.ArticleID}", "http://w3id.org/roh/relatedCommitteeCV", $"<{resourceAPI.GraphsUrl}items/RelatedCommitteeCV_{ResourceID}_{item19.Roh_relatedCommitteeCV.ArticleID}>", list, " . ");
				if(item19.Roh_relatedCommitteeCV.Vivo_start != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/RelatedCommitteeCV_{ResourceID}_{item19.Roh_relatedCommitteeCV.ArticleID}",  "http://vivoweb.org/ontology/core#start", $"\"{item19.Roh_relatedCommitteeCV.Vivo_start.Value.ToString("yyyyMMddHHmmss")}\"", list, " . ");
				}
				if(item19.Roh_relatedCommitteeCV.Vivo_end != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/RelatedCommitteeCV_{ResourceID}_{item19.Roh_relatedCommitteeCV.ArticleID}",  "http://vivoweb.org/ontology/core#end", $"\"{item19.Roh_relatedCommitteeCV.Vivo_end.Value.ToString("yyyyMMddHHmmss")}\"", list, " . ");
				}
			}
				if(item19.IdVivo_relatedBy != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/RelatedCommittee_{ResourceID}_{item19.ArticleID}",  "http://vivoweb.org/ontology/core#relatedBy", $"<{item19.IdVivo_relatedBy}>", list, " . ");
				}
				if(item19.Roh_isPublic != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/RelatedCommittee_{ResourceID}_{item19.ArticleID}",  "http://w3id.org/roh/isPublic", $"\"{item19.Roh_isPublic.ToString()}\"", list, " . ");
				}
			}
			}
			if(this.Roh_scientificActivity.Roh_stays != null)
			{
			foreach(var item20 in this.Roh_scientificActivity.Roh_stays)
			{
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/RelatedStay_{ResourceID}_{item20.ArticleID}", "http://www.w3.org/1999/02/22-rdf-syntax-ns#type", $"<http://w3id.org/roh/RelatedStay>", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/RelatedStay_{ResourceID}_{item20.ArticleID}", "http://www.w3.org/2000/01/rdf-schema#label", $"\"http://w3id.org/roh/RelatedStay\"", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}{ResourceID}", "http://gnoss/hasEntidad", $"<{resourceAPI.GraphsUrl}items/RelatedStay_{ResourceID}_{item20.ArticleID}>", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/ScientificActivity_{ResourceID}_{this.Roh_scientificActivity.ArticleID}", "http://w3id.org/roh/stays", $"<{resourceAPI.GraphsUrl}items/RelatedStay_{ResourceID}_{item20.ArticleID}>", list, " . ");
				if(item20.IdVivo_relatedBy != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/RelatedStay_{ResourceID}_{item20.ArticleID}",  "http://vivoweb.org/ontology/core#relatedBy", $"<{item20.IdVivo_relatedBy}>", list, " . ");
				}
				if(item20.Roh_isPublic != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/RelatedStay_{ResourceID}_{item20.ArticleID}",  "http://w3id.org/roh/isPublic", $"\"{item20.Roh_isPublic.ToString()}\"", list, " . ");
				}
			}
			}
			if(this.Roh_scientificActivity.Roh_obtainedRecognitions != null)
			{
			foreach(var item21 in this.Roh_scientificActivity.Roh_obtainedRecognitions)
			{
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/RelatedObtainedRecognition_{ResourceID}_{item21.ArticleID}", "http://www.w3.org/1999/02/22-rdf-syntax-ns#type", $"<http://w3id.org/roh/RelatedObtainedRecognition>", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/RelatedObtainedRecognition_{ResourceID}_{item21.ArticleID}", "http://www.w3.org/2000/01/rdf-schema#label", $"\"http://w3id.org/roh/RelatedObtainedRecognition\"", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}{ResourceID}", "http://gnoss/hasEntidad", $"<{resourceAPI.GraphsUrl}items/RelatedObtainedRecognition_{ResourceID}_{item21.ArticleID}>", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/ScientificActivity_{ResourceID}_{this.Roh_scientificActivity.ArticleID}", "http://w3id.org/roh/obtainedRecognitions", $"<{resourceAPI.GraphsUrl}items/RelatedObtainedRecognition_{ResourceID}_{item21.ArticleID}>", list, " . ");
				if(item21.IdVivo_relatedBy != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/RelatedObtainedRecognition_{ResourceID}_{item21.ArticleID}",  "http://vivoweb.org/ontology/core#relatedBy", $"<{item21.IdVivo_relatedBy}>", list, " . ");
				}
				if(item21.Roh_isPublic != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/RelatedObtainedRecognition_{ResourceID}_{item21.ArticleID}",  "http://w3id.org/roh/isPublic", $"\"{item21.Roh_isPublic.ToString()}\"", list, " . ");
				}
			}
			}
			if(this.Roh_scientificActivity.Roh_worksSubmittedSeminars != null)
			{
			foreach(var item22 in this.Roh_scientificActivity.Roh_worksSubmittedSeminars)
			{
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/RelatedWorkSubmittedSeminars_{ResourceID}_{item22.ArticleID}", "http://www.w3.org/1999/02/22-rdf-syntax-ns#type", $"<http://w3id.org/roh/RelatedWorkSubmittedSeminars>", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/RelatedWorkSubmittedSeminars_{ResourceID}_{item22.ArticleID}", "http://www.w3.org/2000/01/rdf-schema#label", $"\"http://w3id.org/roh/RelatedWorkSubmittedSeminars\"", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}{ResourceID}", "http://gnoss/hasEntidad", $"<{resourceAPI.GraphsUrl}items/RelatedWorkSubmittedSeminars_{ResourceID}_{item22.ArticleID}>", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/ScientificActivity_{ResourceID}_{this.Roh_scientificActivity.ArticleID}", "http://w3id.org/roh/worksSubmittedSeminars", $"<{resourceAPI.GraphsUrl}items/RelatedWorkSubmittedSeminars_{ResourceID}_{item22.ArticleID}>", list, " . ");
			if(item22.Roh_relatedWorkSubmittedSeminars != null)
			{
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/RelatedWorkSubmittedSeminarsCV_{ResourceID}_{item22.Roh_relatedWorkSubmittedSeminars.ArticleID}", "http://www.w3.org/1999/02/22-rdf-syntax-ns#type", $"<http://w3id.org/roh/RelatedWorkSubmittedSeminarsCV>", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/RelatedWorkSubmittedSeminarsCV_{ResourceID}_{item22.Roh_relatedWorkSubmittedSeminars.ArticleID}", "http://www.w3.org/2000/01/rdf-schema#label", $"\"http://w3id.org/roh/RelatedWorkSubmittedSeminarsCV\"", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}{ResourceID}", "http://gnoss/hasEntidad", $"<{resourceAPI.GraphsUrl}items/RelatedWorkSubmittedSeminarsCV_{ResourceID}_{item22.Roh_relatedWorkSubmittedSeminars.ArticleID}>", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/RelatedWorkSubmittedSeminars_{ResourceID}_{item22.ArticleID}", "http://w3id.org/roh/relatedWorkSubmittedSeminars", $"<{resourceAPI.GraphsUrl}items/RelatedWorkSubmittedSeminarsCV_{ResourceID}_{item22.Roh_relatedWorkSubmittedSeminars.ArticleID}>", list, " . ");
				if(item22.Roh_relatedWorkSubmittedSeminars.IdRoh_inscriptionType != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/RelatedWorkSubmittedSeminarsCV_{ResourceID}_{item22.Roh_relatedWorkSubmittedSeminars.ArticleID}",  "http://w3id.org/roh/inscriptionType", $"<{item22.Roh_relatedWorkSubmittedSeminars.IdRoh_inscriptionType}>", list, " . ");
				}
				if(item22.Roh_relatedWorkSubmittedSeminars.Roh_correspondingAuthor != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/RelatedWorkSubmittedSeminarsCV_{ResourceID}_{item22.Roh_relatedWorkSubmittedSeminars.ArticleID}",  "http://w3id.org/roh/correspondingAuthor", $"\"{item22.Roh_relatedWorkSubmittedSeminars.Roh_correspondingAuthor.ToString()}\"", list, " . ");
				}
				if(item22.Roh_relatedWorkSubmittedSeminars.Roh_inscriptionTypeOther != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/RelatedWorkSubmittedSeminarsCV_{ResourceID}_{item22.Roh_relatedWorkSubmittedSeminars.ArticleID}",  "http://w3id.org/roh/inscriptionTypeOther", $"\"{GenerarTextoSinSaltoDeLinea(item22.Roh_relatedWorkSubmittedSeminars.Roh_inscriptionTypeOther)}\"", list, " . ");
				}
			}
				if(item22.IdVivo_relatedBy != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/RelatedWorkSubmittedSeminars_{ResourceID}_{item22.ArticleID}",  "http://vivoweb.org/ontology/core#relatedBy", $"<{item22.IdVivo_relatedBy}>", list, " . ");
				}
				if(item22.Roh_isPublic != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/RelatedWorkSubmittedSeminars_{ResourceID}_{item22.ArticleID}",  "http://w3id.org/roh/isPublic", $"\"{item22.Roh_isPublic.ToString()}\"", list, " . ");
				}
			}
			}
				if(this.Roh_scientificActivity.Roh_title != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/ScientificActivity_{ResourceID}_{this.Roh_scientificActivity.ArticleID}",  "http://w3id.org/roh/title", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_scientificActivity.Roh_title)}\"", list, " . ");
				}
			}
			if(this.Roh_personalData != null)
			{
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/PersonalData_{ResourceID}_{this.Roh_personalData.ArticleID}", "http://www.w3.org/1999/02/22-rdf-syntax-ns#type", $"<http://w3id.org/roh/PersonalData>", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/PersonalData_{ResourceID}_{this.Roh_personalData.ArticleID}", "http://www.w3.org/2000/01/rdf-schema#label", $"\"http://w3id.org/roh/PersonalData\"", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}{ResourceID}", "http://gnoss/hasEntidad", $"<{resourceAPI.GraphsUrl}items/PersonalData_{ResourceID}_{this.Roh_personalData.ArticleID}>", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/CV_{ResourceID}_{ArticleID}", "http://w3id.org/roh/personalData", $"<{resourceAPI.GraphsUrl}items/PersonalData_{ResourceID}_{this.Roh_personalData.ArticleID}>", list, " . ");
			if(this.Roh_personalData.Roh_hasFax != null)
			{
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/TelephoneType_{ResourceID}_{this.Roh_personalData.Roh_hasFax.ArticleID}", "http://www.w3.org/1999/02/22-rdf-syntax-ns#type", $"<https://www.w3.org/2006/vcard/ns#TelephoneType>", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/TelephoneType_{ResourceID}_{this.Roh_personalData.Roh_hasFax.ArticleID}", "http://www.w3.org/2000/01/rdf-schema#label", $"\"https://www.w3.org/2006/vcard/ns#TelephoneType\"", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}{ResourceID}", "http://gnoss/hasEntidad", $"<{resourceAPI.GraphsUrl}items/TelephoneType_{ResourceID}_{this.Roh_personalData.Roh_hasFax.ArticleID}>", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/PersonalData_{ResourceID}_{this.Roh_personalData.ArticleID}", "http://w3id.org/roh/hasFax", $"<{resourceAPI.GraphsUrl}items/TelephoneType_{ResourceID}_{this.Roh_personalData.Roh_hasFax.ArticleID}>", list, " . ");
				if(this.Roh_personalData.Roh_hasFax.Roh_hasExtension != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/TelephoneType_{ResourceID}_{this.Roh_personalData.Roh_hasFax.ArticleID}",  "http://w3id.org/roh/hasExtension", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_personalData.Roh_hasFax.Roh_hasExtension)}\"", list, " . ");
				}
				if(this.Roh_personalData.Roh_hasFax.Roh_hasInternationalCode != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/TelephoneType_{ResourceID}_{this.Roh_personalData.Roh_hasFax.ArticleID}",  "http://w3id.org/roh/hasInternationalCode", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_personalData.Roh_hasFax.Roh_hasInternationalCode)}\"", list, " . ");
				}
				if(this.Roh_personalData.Roh_hasFax.Vcard_hasValue != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/TelephoneType_{ResourceID}_{this.Roh_personalData.Roh_hasFax.ArticleID}",  "https://www.w3.org/2006/vcard/ns#hasValue", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_personalData.Roh_hasFax.Vcard_hasValue)}\"", list, " . ");
				}
			}
			if(this.Roh_personalData.Vcard_hasTelephone != null)
			{
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/TelephoneType_{ResourceID}_{this.Roh_personalData.Vcard_hasTelephone.ArticleID}", "http://www.w3.org/1999/02/22-rdf-syntax-ns#type", $"<https://www.w3.org/2006/vcard/ns#TelephoneType>", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/TelephoneType_{ResourceID}_{this.Roh_personalData.Vcard_hasTelephone.ArticleID}", "http://www.w3.org/2000/01/rdf-schema#label", $"\"https://www.w3.org/2006/vcard/ns#TelephoneType\"", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}{ResourceID}", "http://gnoss/hasEntidad", $"<{resourceAPI.GraphsUrl}items/TelephoneType_{ResourceID}_{this.Roh_personalData.Vcard_hasTelephone.ArticleID}>", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/PersonalData_{ResourceID}_{this.Roh_personalData.ArticleID}", "https://www.w3.org/2006/vcard/ns#hasTelephone", $"<{resourceAPI.GraphsUrl}items/TelephoneType_{ResourceID}_{this.Roh_personalData.Vcard_hasTelephone.ArticleID}>", list, " . ");
				if(this.Roh_personalData.Vcard_hasTelephone.Roh_hasExtension != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/TelephoneType_{ResourceID}_{this.Roh_personalData.Vcard_hasTelephone.ArticleID}",  "http://w3id.org/roh/hasExtension", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_personalData.Vcard_hasTelephone.Roh_hasExtension)}\"", list, " . ");
				}
				if(this.Roh_personalData.Vcard_hasTelephone.Roh_hasInternationalCode != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/TelephoneType_{ResourceID}_{this.Roh_personalData.Vcard_hasTelephone.ArticleID}",  "http://w3id.org/roh/hasInternationalCode", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_personalData.Vcard_hasTelephone.Roh_hasInternationalCode)}\"", list, " . ");
				}
				if(this.Roh_personalData.Vcard_hasTelephone.Vcard_hasValue != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/TelephoneType_{ResourceID}_{this.Roh_personalData.Vcard_hasTelephone.ArticleID}",  "https://www.w3.org/2006/vcard/ns#hasValue", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_personalData.Vcard_hasTelephone.Vcard_hasValue)}\"", list, " . ");
				}
			}
			if(this.Roh_personalData.Roh_hasMobilePhone != null)
			{
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/TelephoneType_{ResourceID}_{this.Roh_personalData.Roh_hasMobilePhone.ArticleID}", "http://www.w3.org/1999/02/22-rdf-syntax-ns#type", $"<https://www.w3.org/2006/vcard/ns#TelephoneType>", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/TelephoneType_{ResourceID}_{this.Roh_personalData.Roh_hasMobilePhone.ArticleID}", "http://www.w3.org/2000/01/rdf-schema#label", $"\"https://www.w3.org/2006/vcard/ns#TelephoneType\"", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}{ResourceID}", "http://gnoss/hasEntidad", $"<{resourceAPI.GraphsUrl}items/TelephoneType_{ResourceID}_{this.Roh_personalData.Roh_hasMobilePhone.ArticleID}>", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/PersonalData_{ResourceID}_{this.Roh_personalData.ArticleID}", "http://w3id.org/roh/hasMobilePhone", $"<{resourceAPI.GraphsUrl}items/TelephoneType_{ResourceID}_{this.Roh_personalData.Roh_hasMobilePhone.ArticleID}>", list, " . ");
				if(this.Roh_personalData.Roh_hasMobilePhone.Roh_hasExtension != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/TelephoneType_{ResourceID}_{this.Roh_personalData.Roh_hasMobilePhone.ArticleID}",  "http://w3id.org/roh/hasExtension", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_personalData.Roh_hasMobilePhone.Roh_hasExtension)}\"", list, " . ");
				}
				if(this.Roh_personalData.Roh_hasMobilePhone.Roh_hasInternationalCode != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/TelephoneType_{ResourceID}_{this.Roh_personalData.Roh_hasMobilePhone.ArticleID}",  "http://w3id.org/roh/hasInternationalCode", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_personalData.Roh_hasMobilePhone.Roh_hasInternationalCode)}\"", list, " . ");
				}
				if(this.Roh_personalData.Roh_hasMobilePhone.Vcard_hasValue != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/TelephoneType_{ResourceID}_{this.Roh_personalData.Roh_hasMobilePhone.ArticleID}",  "https://www.w3.org/2006/vcard/ns#hasValue", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_personalData.Roh_hasMobilePhone.Vcard_hasValue)}\"", list, " . ");
				}
			}
			if(this.Roh_personalData.Roh_birthplace != null)
			{
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Address_{ResourceID}_{this.Roh_personalData.Roh_birthplace.ArticleID}", "http://www.w3.org/1999/02/22-rdf-syntax-ns#type", $"<https://www.w3.org/2006/vcard/ns#Address>", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Address_{ResourceID}_{this.Roh_personalData.Roh_birthplace.ArticleID}", "http://www.w3.org/2000/01/rdf-schema#label", $"\"https://www.w3.org/2006/vcard/ns#Address\"", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}{ResourceID}", "http://gnoss/hasEntidad", $"<{resourceAPI.GraphsUrl}items/Address_{ResourceID}_{this.Roh_personalData.Roh_birthplace.ArticleID}>", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/PersonalData_{ResourceID}_{this.Roh_personalData.ArticleID}", "http://w3id.org/roh/birthplace", $"<{resourceAPI.GraphsUrl}items/Address_{ResourceID}_{this.Roh_personalData.Roh_birthplace.ArticleID}>", list, " . ");
				if(this.Roh_personalData.Roh_birthplace.IdVcard_hasCountryName != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Address_{ResourceID}_{this.Roh_personalData.Roh_birthplace.ArticleID}",  "https://www.w3.org/2006/vcard/ns#hasCountryName", $"<{this.Roh_personalData.Roh_birthplace.IdVcard_hasCountryName}>", list, " . ");
				}
				if(this.Roh_personalData.Roh_birthplace.IdRoh_hasProvince != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Address_{ResourceID}_{this.Roh_personalData.Roh_birthplace.ArticleID}",  "http://w3id.org/roh/hasProvince", $"<{this.Roh_personalData.Roh_birthplace.IdRoh_hasProvince}>", list, " . ");
				}
				if(this.Roh_personalData.Roh_birthplace.IdVcard_hasRegion != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Address_{ResourceID}_{this.Roh_personalData.Roh_birthplace.ArticleID}",  "https://www.w3.org/2006/vcard/ns#hasRegion", $"<{this.Roh_personalData.Roh_birthplace.IdVcard_hasRegion}>", list, " . ");
				}
				if(this.Roh_personalData.Roh_birthplace.Vcard_postal_code != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Address_{ResourceID}_{this.Roh_personalData.Roh_birthplace.ArticleID}",  "https://www.w3.org/2006/vcard/ns#postal-code", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_personalData.Roh_birthplace.Vcard_postal_code)}\"", list, " . ");
				}
				if(this.Roh_personalData.Roh_birthplace.Vcard_extended_address != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Address_{ResourceID}_{this.Roh_personalData.Roh_birthplace.ArticleID}",  "https://www.w3.org/2006/vcard/ns#extended-address", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_personalData.Roh_birthplace.Vcard_extended_address)}\"", list, " . ");
				}
				if(this.Roh_personalData.Roh_birthplace.Vcard_street_address != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Address_{ResourceID}_{this.Roh_personalData.Roh_birthplace.ArticleID}",  "https://www.w3.org/2006/vcard/ns#street-address", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_personalData.Roh_birthplace.Vcard_street_address)}\"", list, " . ");
				}
				if(this.Roh_personalData.Roh_birthplace.Vcard_locality != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Address_{ResourceID}_{this.Roh_personalData.Roh_birthplace.ArticleID}",  "https://www.w3.org/2006/vcard/ns#locality", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_personalData.Roh_birthplace.Vcard_locality)}\"", list, " . ");
				}
			}
			if(this.Roh_personalData.Vcard_address != null)
			{
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Address_{ResourceID}_{this.Roh_personalData.Vcard_address.ArticleID}", "http://www.w3.org/1999/02/22-rdf-syntax-ns#type", $"<https://www.w3.org/2006/vcard/ns#Address>", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Address_{ResourceID}_{this.Roh_personalData.Vcard_address.ArticleID}", "http://www.w3.org/2000/01/rdf-schema#label", $"\"https://www.w3.org/2006/vcard/ns#Address\"", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}{ResourceID}", "http://gnoss/hasEntidad", $"<{resourceAPI.GraphsUrl}items/Address_{ResourceID}_{this.Roh_personalData.Vcard_address.ArticleID}>", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/PersonalData_{ResourceID}_{this.Roh_personalData.ArticleID}", "https://www.w3.org/2006/vcard/ns#address", $"<{resourceAPI.GraphsUrl}items/Address_{ResourceID}_{this.Roh_personalData.Vcard_address.ArticleID}>", list, " . ");
				if(this.Roh_personalData.Vcard_address.IdVcard_hasCountryName != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Address_{ResourceID}_{this.Roh_personalData.Vcard_address.ArticleID}",  "https://www.w3.org/2006/vcard/ns#hasCountryName", $"<{this.Roh_personalData.Vcard_address.IdVcard_hasCountryName}>", list, " . ");
				}
				if(this.Roh_personalData.Vcard_address.IdRoh_hasProvince != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Address_{ResourceID}_{this.Roh_personalData.Vcard_address.ArticleID}",  "http://w3id.org/roh/hasProvince", $"<{this.Roh_personalData.Vcard_address.IdRoh_hasProvince}>", list, " . ");
				}
				if(this.Roh_personalData.Vcard_address.IdVcard_hasRegion != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Address_{ResourceID}_{this.Roh_personalData.Vcard_address.ArticleID}",  "https://www.w3.org/2006/vcard/ns#hasRegion", $"<{this.Roh_personalData.Vcard_address.IdVcard_hasRegion}>", list, " . ");
				}
				if(this.Roh_personalData.Vcard_address.Vcard_postal_code != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Address_{ResourceID}_{this.Roh_personalData.Vcard_address.ArticleID}",  "https://www.w3.org/2006/vcard/ns#postal-code", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_personalData.Vcard_address.Vcard_postal_code)}\"", list, " . ");
				}
				if(this.Roh_personalData.Vcard_address.Vcard_extended_address != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Address_{ResourceID}_{this.Roh_personalData.Vcard_address.ArticleID}",  "https://www.w3.org/2006/vcard/ns#extended-address", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_personalData.Vcard_address.Vcard_extended_address)}\"", list, " . ");
				}
				if(this.Roh_personalData.Vcard_address.Vcard_street_address != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Address_{ResourceID}_{this.Roh_personalData.Vcard_address.ArticleID}",  "https://www.w3.org/2006/vcard/ns#street-address", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_personalData.Vcard_address.Vcard_street_address)}\"", list, " . ");
				}
				if(this.Roh_personalData.Vcard_address.Vcard_locality != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Address_{ResourceID}_{this.Roh_personalData.Vcard_address.ArticleID}",  "https://www.w3.org/2006/vcard/ns#locality", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_personalData.Vcard_address.Vcard_locality)}\"", list, " . ");
				}
			}
			if(this.Roh_personalData.Roh_otherIds != null)
			{
			foreach(var item6 in this.Roh_personalData.Roh_otherIds)
			{
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Document_{ResourceID}_{item6.ArticleID}", "http://www.w3.org/1999/02/22-rdf-syntax-ns#type", $"<http://xmlns.com/foaf/0.1/Document>", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Document_{ResourceID}_{item6.ArticleID}", "http://www.w3.org/2000/01/rdf-schema#label", $"\"http://xmlns.com/foaf/0.1/Document\"", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}{ResourceID}", "http://gnoss/hasEntidad", $"<{resourceAPI.GraphsUrl}items/Document_{ResourceID}_{item6.ArticleID}>", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/PersonalData_{ResourceID}_{this.Roh_personalData.ArticleID}", "http://w3id.org/roh/otherIds", $"<{resourceAPI.GraphsUrl}items/Document_{ResourceID}_{item6.ArticleID}>", list, " . ");
				if(item6.Dc_title != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Document_{ResourceID}_{item6.ArticleID}",  "http://purl.org/dc/elements/1.1/title", $"\"{GenerarTextoSinSaltoDeLinea(item6.Dc_title)}\"", list, " . ");
				}
				if(item6.Foaf_topic != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Document_{ResourceID}_{item6.ArticleID}",  "http://xmlns.com/foaf/0.1/topic", $"\"{GenerarTextoSinSaltoDeLinea(item6.Foaf_topic)}\"", list, " . ");
				}
			}
			}
				if(this.Roh_personalData.IdFoaf_gender != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/PersonalData_{ResourceID}_{this.Roh_personalData.ArticleID}",  "http://xmlns.com/foaf/0.1/gender", $"<{this.Roh_personalData.IdFoaf_gender}>", list, " . ");
				}
				if(this.Roh_personalData.IdSchema_nationality != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/PersonalData_{ResourceID}_{this.Roh_personalData.ArticleID}",  "http://www.schema.org/nationality", $"<{this.Roh_personalData.IdSchema_nationality}>", list, " . ");
				}
				if(this.Roh_personalData.Roh_nie != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/PersonalData_{ResourceID}_{this.Roh_personalData.ArticleID}",  "http://w3id.org/roh/nie", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_personalData.Roh_nie)}\"", list, " . ");
				}
				if(this.Roh_personalData.Vivo_researcherId != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/PersonalData_{ResourceID}_{this.Roh_personalData.ArticleID}",  "http://vivoweb.org/ontology/core#researcherId", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_personalData.Vivo_researcherId)}\"", list, " . ");
				}
				if(this.Roh_personalData.Vivo_scopusId != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/PersonalData_{ResourceID}_{this.Roh_personalData.ArticleID}",  "http://vivoweb.org/ontology/core#scopusId", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_personalData.Vivo_scopusId)}\"", list, " . ");
				}
				if(this.Roh_personalData.Foaf_familyName != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/PersonalData_{ResourceID}_{this.Roh_personalData.ArticleID}",  "http://xmlns.com/foaf/0.1/familyName", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_personalData.Foaf_familyName)}\"", list, " . ");
				}
				if(this.Roh_personalData.Roh_secondFamilyName != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/PersonalData_{ResourceID}_{this.Roh_personalData.ArticleID}",  "http://w3id.org/roh/secondFamilyName", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_personalData.Roh_secondFamilyName)}\"", list, " . ");
				}
				if(this.Roh_personalData.Vcard_email != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/PersonalData_{ResourceID}_{this.Roh_personalData.ArticleID}",  "https://www.w3.org/2006/vcard/ns#email", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_personalData.Vcard_email)}\"", list, " . ");
				}
				if(this.Roh_personalData.Foaf_img != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/PersonalData_{ResourceID}_{this.Roh_personalData.ArticleID}",  "http://xmlns.com/foaf/0.1/img", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_personalData.Foaf_img)}\"", list, " . ");
				}
				if(this.Roh_personalData.Roh_dni != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/PersonalData_{ResourceID}_{this.Roh_personalData.ArticleID}",  "http://w3id.org/roh/dni", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_personalData.Roh_dni)}\"", list, " . ");
				}
				if(this.Roh_personalData.Foaf_homepage != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/PersonalData_{ResourceID}_{this.Roh_personalData.ArticleID}",  "http://xmlns.com/foaf/0.1/homepage", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_personalData.Foaf_homepage)}\"", list, " . ");
				}
				if(this.Roh_personalData.Roh_ORCID != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/PersonalData_{ResourceID}_{this.Roh_personalData.ArticleID}",  "http://w3id.org/roh/ORCID", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_personalData.Roh_ORCID)}\"", list, " . ");
				}
				if(this.Roh_personalData.Roh_passport != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/PersonalData_{ResourceID}_{this.Roh_personalData.ArticleID}",  "http://w3id.org/roh/passport", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_personalData.Roh_passport)}\"", list, " . ");
				}
				if(this.Roh_personalData.Vcard_birth_date != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/PersonalData_{ResourceID}_{this.Roh_personalData.ArticleID}",  "https://www.w3.org/2006/vcard/ns#birth-date", $"\"{this.Roh_personalData.Vcard_birth_date.Value.ToString("yyyyMMddHHmmss")}\"", list, " . ");
				}
				if(this.Roh_personalData.Foaf_firstName != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/PersonalData_{ResourceID}_{this.Roh_personalData.ArticleID}",  "http://xmlns.com/foaf/0.1/firstName", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_personalData.Foaf_firstName)}\"", list, " . ");
				}
			}
				if(this.IdRoh_gnossUser != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/CV_{ResourceID}_{ArticleID}",  "http://w3id.org/roh/gnossUser", $"<{this.IdRoh_gnossUser}>", list, " . ");
				}
				if(this.IdRoh_cvOf != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/CV_{ResourceID}_{ArticleID}",  "http://w3id.org/roh/cvOf", $"<{this.IdRoh_cvOf}>", list, " . ");
				}
				if(this.Foaf_name != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/CV_{ResourceID}_{ArticleID}",  "http://xmlns.com/foaf/0.1/name", $"\"{GenerarTextoSinSaltoDeLinea(this.Foaf_name)}\"", list, " . ");
				}
			return list;
		}

		public override List<string> ToSearchGraphTriples(ResourceApi resourceAPI)
		{
			List<string> list = new List<string>();
			List<string> listaSearch = new List<string>();
			AgregarTags(list);
			AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}", "http://www.w3.org/1999/02/22-rdf-syntax-ns#type", $"\"curriculumvitae\"", list, " . ");
			AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}", "http://gnoss/type", $"\"http://w3id.org/roh/CV\"", list, " . ");
			AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}", "http://gnoss/hasfechapublicacion", $"{DateTime.Now.ToString("yyyyMMddHHmmss")}", list, " . ");
			AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}", "http://gnoss/hastipodoc", "\"5\"", list, " . ");
			AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}", "http://gnoss/hasfechamodificacion", $"{DateTime.Now.ToString("yyyyMMddHHmmss")}", list, " . ");
			AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}", "http://gnoss/hasnumeroVisitas", "0", list, " . ");
			AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}", "http://gnoss/hasprivacidadCom", "\"publico\"", list, " . ");
			AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}", "http://xmlns.com/foaf/0.1/firstName", $"\"{GenerarTextoSinSaltoDeLinea(this.Foaf_name)}\"", list, " . ");
			AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}", "http://gnoss/hasnombrecompleto", $"\"{GenerarTextoSinSaltoDeLinea(this.Foaf_name)}\"", list, " . ");
			string search = string.Empty;
			if(this.Roh_scientificExperience != null)
			{
				AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}", "http://w3id.org/roh/scientificExperience", $"<{resourceAPI.GraphsUrl}items/scientificexperience_{ResourceID}_{this.Roh_scientificExperience.ArticleID}>", list, " . ");
			if(this.Roh_scientificExperience.Roh_supervisedArtisticProjects != null)
			{
			foreach(var item1 in this.Roh_scientificExperience.Roh_supervisedArtisticProjects)
			{
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/scientificexperience_{ResourceID}_{this.Roh_scientificExperience.ArticleID}", "http://w3id.org/roh/supervisedArtisticProjects", $"<{resourceAPI.GraphsUrl}items/relatedsupervisedartisticproject_{ResourceID}_{item1.ArticleID}>", list, " . ");
			if(item1.Roh_relatedSupervisedArtisticProjectCV != null)
			{
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/relatedsupervisedartisticproject_{ResourceID}_{item1.ArticleID}", "http://w3id.org/roh/relatedSupervisedArtisticProjectCV", $"<{resourceAPI.GraphsUrl}items/relatedsupervisedartisticprojectcv_{ResourceID}_{item1.Roh_relatedSupervisedArtisticProjectCV.ArticleID}>", list, " . ");
				if(item1.Roh_relatedSupervisedArtisticProjectCV.IdRoh_contributionGradeProject != null)
				{
					Regex regex = new Regex(@"\/items\/.+_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}");
					string itemRegex = item1.Roh_relatedSupervisedArtisticProjectCV.IdRoh_contributionGradeProject;
					if (regex.IsMatch(itemRegex))
					{
						itemRegex = $"http://gnoss/{resourceAPI.GetShortGuid(itemRegex).ToString().ToUpper()}";
					}
					else
					{
						itemRegex = itemRegex.ToLower();
					}
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/relatedsupervisedartisticprojectcv_{ResourceID}_{item1.Roh_relatedSupervisedArtisticProjectCV.ArticleID}",  "http://w3id.org/roh/contributionGradeProject", $"<{itemRegex}>", list, " . ");
				}
				if(item1.Roh_relatedSupervisedArtisticProjectCV.Roh_curator != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/relatedsupervisedartisticprojectcv_{ResourceID}_{item1.Roh_relatedSupervisedArtisticProjectCV.ArticleID}",  "http://w3id.org/roh/curator", $"\"{item1.Roh_relatedSupervisedArtisticProjectCV.Roh_curator.ToString().ToLower()}\"", list, " . ");
				}
				if(item1.Roh_relatedSupervisedArtisticProjectCV.Roh_contributionGradeProjectOther != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/relatedsupervisedartisticprojectcv_{ResourceID}_{item1.Roh_relatedSupervisedArtisticProjectCV.ArticleID}",  "http://w3id.org/roh/contributionGradeProjectOther", $"\"{GenerarTextoSinSaltoDeLinea(item1.Roh_relatedSupervisedArtisticProjectCV.Roh_contributionGradeProjectOther).ToLower()}\"", list, " . ");
				}
			}
				if(item1.IdVivo_relatedBy != null)
				{
					Regex regex = new Regex(@"\/items\/.+_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}");
					string itemRegex = item1.IdVivo_relatedBy;
					if (regex.IsMatch(itemRegex))
					{
						itemRegex = $"http://gnoss/{resourceAPI.GetShortGuid(itemRegex).ToString().ToUpper()}";
					}
					else
					{
						itemRegex = itemRegex.ToLower();
					}
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/relatedsupervisedartisticproject_{ResourceID}_{item1.ArticleID}",  "http://vivoweb.org/ontology/core#relatedBy", $"<{itemRegex}>", list, " . ");
				}
				if(item1.Roh_isPublic != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/relatedsupervisedartisticproject_{ResourceID}_{item1.ArticleID}",  "http://w3id.org/roh/isPublic", $"\"{item1.Roh_isPublic.ToString().ToLower()}\"", list, " . ");
				}
			}
			}
			if(this.Roh_scientificExperience.Roh_competitiveProjects != null)
			{
			foreach(var item2 in this.Roh_scientificExperience.Roh_competitiveProjects)
			{
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/scientificexperience_{ResourceID}_{this.Roh_scientificExperience.ArticleID}", "http://w3id.org/roh/competitiveProjects", $"<{resourceAPI.GraphsUrl}items/relatedcompetitiveproject_{ResourceID}_{item2.ArticleID}>", list, " . ");
			if(item2.Roh_relatedCompetitiveProjectCV != null)
			{
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/relatedcompetitiveproject_{ResourceID}_{item2.ArticleID}", "http://w3id.org/roh/relatedCompetitiveProjectCV", $"<{resourceAPI.GraphsUrl}items/relatedcompetitiveprojectcv_{ResourceID}_{item2.Roh_relatedCompetitiveProjectCV.ArticleID}>", list, " . ");
				if(item2.Roh_relatedCompetitiveProjectCV.IdRoh_dedication != null)
				{
					Regex regex = new Regex(@"\/items\/.+_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}");
					string itemRegex = item2.Roh_relatedCompetitiveProjectCV.IdRoh_dedication;
					if (regex.IsMatch(itemRegex))
					{
						itemRegex = $"http://gnoss/{resourceAPI.GetShortGuid(itemRegex).ToString().ToUpper()}";
					}
					else
					{
						itemRegex = itemRegex.ToLower();
					}
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/relatedcompetitiveprojectcv_{ResourceID}_{item2.Roh_relatedCompetitiveProjectCV.ArticleID}",  "http://w3id.org/roh/dedication", $"<{itemRegex}>", list, " . ");
				}
				if(item2.Roh_relatedCompetitiveProjectCV.IdRoh_participationType != null)
				{
					Regex regex = new Regex(@"\/items\/.+_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}");
					string itemRegex = item2.Roh_relatedCompetitiveProjectCV.IdRoh_participationType;
					if (regex.IsMatch(itemRegex))
					{
						itemRegex = $"http://gnoss/{resourceAPI.GetShortGuid(itemRegex).ToString().ToUpper()}";
					}
					else
					{
						itemRegex = itemRegex.ToLower();
					}
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/relatedcompetitiveprojectcv_{ResourceID}_{item2.Roh_relatedCompetitiveProjectCV.ArticleID}",  "http://w3id.org/roh/participationType", $"<{itemRegex}>", list, " . ");
				}
				if(item2.Roh_relatedCompetitiveProjectCV.IdRoh_contributionGradeProject != null)
				{
					Regex regex = new Regex(@"\/items\/.+_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}");
					string itemRegex = item2.Roh_relatedCompetitiveProjectCV.IdRoh_contributionGradeProject;
					if (regex.IsMatch(itemRegex))
					{
						itemRegex = $"http://gnoss/{resourceAPI.GetShortGuid(itemRegex).ToString().ToUpper()}";
					}
					else
					{
						itemRegex = itemRegex.ToLower();
					}
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/relatedcompetitiveprojectcv_{ResourceID}_{item2.Roh_relatedCompetitiveProjectCV.ArticleID}",  "http://w3id.org/roh/contributionGradeProject", $"<{itemRegex}>", list, " . ");
				}
				if(item2.Roh_relatedCompetitiveProjectCV.Roh_contributionGradeProjectOther != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/relatedcompetitiveprojectcv_{ResourceID}_{item2.Roh_relatedCompetitiveProjectCV.ArticleID}",  "http://w3id.org/roh/contributionGradeProjectOther", $"\"{GenerarTextoSinSaltoDeLinea(item2.Roh_relatedCompetitiveProjectCV.Roh_contributionGradeProjectOther).ToLower()}\"", list, " . ");
				}
				if(item2.Roh_relatedCompetitiveProjectCV.Roh_participationTypeOther != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/relatedcompetitiveprojectcv_{ResourceID}_{item2.Roh_relatedCompetitiveProjectCV.ArticleID}",  "http://w3id.org/roh/participationTypeOther", $"\"{GenerarTextoSinSaltoDeLinea(item2.Roh_relatedCompetitiveProjectCV.Roh_participationTypeOther).ToLower()}\"", list, " . ");
				}
				if(item2.Roh_relatedCompetitiveProjectCV.Roh_applicantContribution != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/relatedcompetitiveprojectcv_{ResourceID}_{item2.Roh_relatedCompetitiveProjectCV.ArticleID}",  "http://w3id.org/roh/applicantContribution", $"\"{GenerarTextoSinSaltoDeLinea(item2.Roh_relatedCompetitiveProjectCV.Roh_applicantContribution).ToLower()}\"", list, " . ");
				}
			}
				if(item2.IdVivo_relatedBy != null)
				{
					Regex regex = new Regex(@"\/items\/.+_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}");
					string itemRegex = item2.IdVivo_relatedBy;
					if (regex.IsMatch(itemRegex))
					{
						itemRegex = $"http://gnoss/{resourceAPI.GetShortGuid(itemRegex).ToString().ToUpper()}";
					}
					else
					{
						itemRegex = itemRegex.ToLower();
					}
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/relatedcompetitiveproject_{ResourceID}_{item2.ArticleID}",  "http://vivoweb.org/ontology/core#relatedBy", $"<{itemRegex}>", list, " . ");
				}
				if(item2.Roh_isPublic != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/relatedcompetitiveproject_{ResourceID}_{item2.ArticleID}",  "http://w3id.org/roh/isPublic", $"\"{item2.Roh_isPublic.ToString().ToLower()}\"", list, " . ");
				}
			}
			}
			if(this.Roh_scientificExperience.Roh_groups != null)
			{
			foreach(var item3 in this.Roh_scientificExperience.Roh_groups)
			{
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/scientificexperience_{ResourceID}_{this.Roh_scientificExperience.ArticleID}", "http://w3id.org/roh/groups", $"<{resourceAPI.GraphsUrl}items/relatedgroup_{ResourceID}_{item3.ArticleID}>", list, " . ");
				if(item3.IdVivo_relatedBy != null)
				{
					Regex regex = new Regex(@"\/items\/.+_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}");
					string itemRegex = item3.IdVivo_relatedBy;
					if (regex.IsMatch(itemRegex))
					{
						itemRegex = $"http://gnoss/{resourceAPI.GetShortGuid(itemRegex).ToString().ToUpper()}";
					}
					else
					{
						itemRegex = itemRegex.ToLower();
					}
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/relatedgroup_{ResourceID}_{item3.ArticleID}",  "http://vivoweb.org/ontology/core#relatedBy", $"<{itemRegex}>", list, " . ");
				}
				if(item3.Roh_isPublic != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/relatedgroup_{ResourceID}_{item3.ArticleID}",  "http://w3id.org/roh/isPublic", $"\"{item3.Roh_isPublic.ToString().ToLower()}\"", list, " . ");
				}
			}
			}
			if(this.Roh_scientificExperience.Roh_technologicalResults != null)
			{
			foreach(var item4 in this.Roh_scientificExperience.Roh_technologicalResults)
			{
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/scientificexperience_{ResourceID}_{this.Roh_scientificExperience.ArticleID}", "http://w3id.org/roh/technologicalResults", $"<{resourceAPI.GraphsUrl}items/relatedtechnologicalresult_{ResourceID}_{item4.ArticleID}>", list, " . ");
			if(item4.Roh_relatedTechnologicalResultCV != null)
			{
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/relatedtechnologicalresult_{ResourceID}_{item4.ArticleID}", "http://w3id.org/roh/relatedTechnologicalResultCV", $"<{resourceAPI.GraphsUrl}items/relatedtechnologicalresultcv_{ResourceID}_{item4.Roh_relatedTechnologicalResultCV.ArticleID}>", list, " . ");
				if(item4.Roh_relatedTechnologicalResultCV.IdRoh_contributionGradeProject != null)
				{
					Regex regex = new Regex(@"\/items\/.+_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}");
					string itemRegex = item4.Roh_relatedTechnologicalResultCV.IdRoh_contributionGradeProject;
					if (regex.IsMatch(itemRegex))
					{
						itemRegex = $"http://gnoss/{resourceAPI.GetShortGuid(itemRegex).ToString().ToUpper()}";
					}
					else
					{
						itemRegex = itemRegex.ToLower();
					}
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/relatedtechnologicalresultcv_{ResourceID}_{item4.Roh_relatedTechnologicalResultCV.ArticleID}",  "http://w3id.org/roh/contributionGradeProject", $"<{itemRegex}>", list, " . ");
				}
				if(item4.Roh_relatedTechnologicalResultCV.Roh_contributionGradeProjectOther != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/relatedtechnologicalresultcv_{ResourceID}_{item4.Roh_relatedTechnologicalResultCV.ArticleID}",  "http://w3id.org/roh/contributionGradeProjectOther", $"\"{GenerarTextoSinSaltoDeLinea(item4.Roh_relatedTechnologicalResultCV.Roh_contributionGradeProjectOther).ToLower()}\"", list, " . ");
				}
				if(item4.Roh_relatedTechnologicalResultCV.Roh_expertTechnologist != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/relatedtechnologicalresultcv_{ResourceID}_{item4.Roh_relatedTechnologicalResultCV.ArticleID}",  "http://w3id.org/roh/expertTechnologist", $"\"{item4.Roh_relatedTechnologicalResultCV.Roh_expertTechnologist.ToString().ToLower()}\"", list, " . ");
				}
			}
				if(item4.IdVivo_relatedBy != null)
				{
					Regex regex = new Regex(@"\/items\/.+_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}");
					string itemRegex = item4.IdVivo_relatedBy;
					if (regex.IsMatch(itemRegex))
					{
						itemRegex = $"http://gnoss/{resourceAPI.GetShortGuid(itemRegex).ToString().ToUpper()}";
					}
					else
					{
						itemRegex = itemRegex.ToLower();
					}
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/relatedtechnologicalresult_{ResourceID}_{item4.ArticleID}",  "http://vivoweb.org/ontology/core#relatedBy", $"<{itemRegex}>", list, " . ");
				}
				if(item4.Roh_isPublic != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/relatedtechnologicalresult_{ResourceID}_{item4.ArticleID}",  "http://w3id.org/roh/isPublic", $"\"{item4.Roh_isPublic.ToString().ToLower()}\"", list, " . ");
				}
			}
			}
			if(this.Roh_scientificExperience.Roh_nonCompetitiveProjects != null)
			{
			foreach(var item5 in this.Roh_scientificExperience.Roh_nonCompetitiveProjects)
			{
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/scientificexperience_{ResourceID}_{this.Roh_scientificExperience.ArticleID}", "http://w3id.org/roh/nonCompetitiveProjects", $"<{resourceAPI.GraphsUrl}items/relatednoncompetitiveproject_{ResourceID}_{item5.ArticleID}>", list, " . ");
			if(item5.Roh_relatedNonCompetitiveProjectCV != null)
			{
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/relatednoncompetitiveproject_{ResourceID}_{item5.ArticleID}", "http://w3id.org/roh/relatedNonCompetitiveProjectCV", $"<{resourceAPI.GraphsUrl}items/relatednoncompetitiveprojectcv_{ResourceID}_{item5.Roh_relatedNonCompetitiveProjectCV.ArticleID}>", list, " . ");
				if(item5.Roh_relatedNonCompetitiveProjectCV.IdRoh_dedication != null)
				{
					Regex regex = new Regex(@"\/items\/.+_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}");
					string itemRegex = item5.Roh_relatedNonCompetitiveProjectCV.IdRoh_dedication;
					if (regex.IsMatch(itemRegex))
					{
						itemRegex = $"http://gnoss/{resourceAPI.GetShortGuid(itemRegex).ToString().ToUpper()}";
					}
					else
					{
						itemRegex = itemRegex.ToLower();
					}
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/relatednoncompetitiveprojectcv_{ResourceID}_{item5.Roh_relatedNonCompetitiveProjectCV.ArticleID}",  "http://w3id.org/roh/dedication", $"<{itemRegex}>", list, " . ");
				}
				if(item5.Roh_relatedNonCompetitiveProjectCV.IdRoh_participationType != null)
				{
					Regex regex = new Regex(@"\/items\/.+_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}");
					string itemRegex = item5.Roh_relatedNonCompetitiveProjectCV.IdRoh_participationType;
					if (regex.IsMatch(itemRegex))
					{
						itemRegex = $"http://gnoss/{resourceAPI.GetShortGuid(itemRegex).ToString().ToUpper()}";
					}
					else
					{
						itemRegex = itemRegex.ToLower();
					}
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/relatednoncompetitiveprojectcv_{ResourceID}_{item5.Roh_relatedNonCompetitiveProjectCV.ArticleID}",  "http://w3id.org/roh/participationType", $"<{itemRegex}>", list, " . ");
				}
				if(item5.Roh_relatedNonCompetitiveProjectCV.IdRoh_contributionGradeProject != null)
				{
					Regex regex = new Regex(@"\/items\/.+_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}");
					string itemRegex = item5.Roh_relatedNonCompetitiveProjectCV.IdRoh_contributionGradeProject;
					if (regex.IsMatch(itemRegex))
					{
						itemRegex = $"http://gnoss/{resourceAPI.GetShortGuid(itemRegex).ToString().ToUpper()}";
					}
					else
					{
						itemRegex = itemRegex.ToLower();
					}
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/relatednoncompetitiveprojectcv_{ResourceID}_{item5.Roh_relatedNonCompetitiveProjectCV.ArticleID}",  "http://w3id.org/roh/contributionGradeProject", $"<{itemRegex}>", list, " . ");
				}
				if(item5.Roh_relatedNonCompetitiveProjectCV.Roh_contributionGradeProjectOther != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/relatednoncompetitiveprojectcv_{ResourceID}_{item5.Roh_relatedNonCompetitiveProjectCV.ArticleID}",  "http://w3id.org/roh/contributionGradeProjectOther", $"\"{GenerarTextoSinSaltoDeLinea(item5.Roh_relatedNonCompetitiveProjectCV.Roh_contributionGradeProjectOther).ToLower()}\"", list, " . ");
				}
				if(item5.Roh_relatedNonCompetitiveProjectCV.Roh_participationTypeOther != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/relatednoncompetitiveprojectcv_{ResourceID}_{item5.Roh_relatedNonCompetitiveProjectCV.ArticleID}",  "http://w3id.org/roh/participationTypeOther", $"\"{GenerarTextoSinSaltoDeLinea(item5.Roh_relatedNonCompetitiveProjectCV.Roh_participationTypeOther).ToLower()}\"", list, " . ");
				}
				if(item5.Roh_relatedNonCompetitiveProjectCV.Roh_applicantContribution != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/relatednoncompetitiveprojectcv_{ResourceID}_{item5.Roh_relatedNonCompetitiveProjectCV.ArticleID}",  "http://w3id.org/roh/applicantContribution", $"\"{GenerarTextoSinSaltoDeLinea(item5.Roh_relatedNonCompetitiveProjectCV.Roh_applicantContribution).ToLower()}\"", list, " . ");
				}
			}
				if(item5.IdVivo_relatedBy != null)
				{
					Regex regex = new Regex(@"\/items\/.+_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}");
					string itemRegex = item5.IdVivo_relatedBy;
					if (regex.IsMatch(itemRegex))
					{
						itemRegex = $"http://gnoss/{resourceAPI.GetShortGuid(itemRegex).ToString().ToUpper()}";
					}
					else
					{
						itemRegex = itemRegex.ToLower();
					}
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/relatednoncompetitiveproject_{ResourceID}_{item5.ArticleID}",  "http://vivoweb.org/ontology/core#relatedBy", $"<{itemRegex}>", list, " . ");
				}
				if(item5.Roh_isPublic != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/relatednoncompetitiveproject_{ResourceID}_{item5.ArticleID}",  "http://w3id.org/roh/isPublic", $"\"{item5.Roh_isPublic.ToString().ToLower()}\"", list, " . ");
				}
			}
			}
			if(this.Roh_scientificExperience.Roh_patents != null)
			{
			foreach(var item6 in this.Roh_scientificExperience.Roh_patents)
			{
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/scientificexperience_{ResourceID}_{this.Roh_scientificExperience.ArticleID}", "http://w3id.org/roh/patents", $"<{resourceAPI.GraphsUrl}items/relatedpatent_{ResourceID}_{item6.ArticleID}>", list, " . ");
				if(item6.IdVivo_relatedBy != null)
				{
					Regex regex = new Regex(@"\/items\/.+_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}");
					string itemRegex = item6.IdVivo_relatedBy;
					if (regex.IsMatch(itemRegex))
					{
						itemRegex = $"http://gnoss/{resourceAPI.GetShortGuid(itemRegex).ToString().ToUpper()}";
					}
					else
					{
						itemRegex = itemRegex.ToLower();
					}
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/relatedpatent_{ResourceID}_{item6.ArticleID}",  "http://vivoweb.org/ontology/core#relatedBy", $"<{itemRegex}>", list, " . ");
				}
				if(item6.Roh_isPublic != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/relatedpatent_{ResourceID}_{item6.ArticleID}",  "http://w3id.org/roh/isPublic", $"\"{item6.Roh_isPublic.ToString().ToLower()}\"", list, " . ");
				}
			}
			}
				if(this.Roh_scientificExperience.Roh_title != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/scientificexperience_{ResourceID}_{this.Roh_scientificExperience.ArticleID}",  "http://w3id.org/roh/title", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_scientificExperience.Roh_title).ToLower()}\"", list, " . ");
				}
			}
			if(this.Roh_scientificActivity != null)
			{
				AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}", "http://w3id.org/roh/scientificActivity", $"<{resourceAPI.GraphsUrl}items/scientificactivity_{ResourceID}_{this.Roh_scientificActivity.ArticleID}>", list, " . ");
			if(this.Roh_scientificActivity.Roh_otherDistinctions != null)
			{
			foreach(var item1 in this.Roh_scientificActivity.Roh_otherDistinctions)
			{
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/scientificactivity_{ResourceID}_{this.Roh_scientificActivity.ArticleID}", "http://w3id.org/roh/otherDistinctions", $"<{resourceAPI.GraphsUrl}items/relatedotherdistinction_{ResourceID}_{item1.ArticleID}>", list, " . ");
				if(item1.IdVivo_relatedBy != null)
				{
					Regex regex = new Regex(@"\/items\/.+_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}");
					string itemRegex = item1.IdVivo_relatedBy;
					if (regex.IsMatch(itemRegex))
					{
						itemRegex = $"http://gnoss/{resourceAPI.GetShortGuid(itemRegex).ToString().ToUpper()}";
					}
					else
					{
						itemRegex = itemRegex.ToLower();
					}
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/relatedotherdistinction_{ResourceID}_{item1.ArticleID}",  "http://vivoweb.org/ontology/core#relatedBy", $"<{itemRegex}>", list, " . ");
				}
				if(item1.Roh_isPublic != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/relatedotherdistinction_{ResourceID}_{item1.ArticleID}",  "http://w3id.org/roh/isPublic", $"\"{item1.Roh_isPublic.ToString().ToLower()}\"", list, " . ");
				}
			}
			}
			if(this.Roh_scientificActivity.Roh_generalQualityIndicators != null)
			{
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/scientificactivity_{ResourceID}_{this.Roh_scientificActivity.ArticleID}", "http://w3id.org/roh/generalQualityIndicators", $"<{resourceAPI.GraphsUrl}items/generalqualityindicator_{ResourceID}_{this.Roh_scientificActivity.Roh_generalQualityIndicators.ArticleID}>", list, " . ");
			if(this.Roh_scientificActivity.Roh_generalQualityIndicators.Roh_generalQualityIndicatorCV != null)
			{
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/generalqualityindicator_{ResourceID}_{this.Roh_scientificActivity.Roh_generalQualityIndicators.ArticleID}", "http://w3id.org/roh/generalQualityIndicatorCV", $"<{resourceAPI.GraphsUrl}items/generalqualityindicatorcv_{ResourceID}_{this.Roh_scientificActivity.Roh_generalQualityIndicators.Roh_generalQualityIndicatorCV.ArticleID}>", list, " . ");
				if(this.Roh_scientificActivity.Roh_generalQualityIndicators.Roh_generalQualityIndicatorCV.Roh_generalQualityIndicator != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/generalqualityindicatorcv_{ResourceID}_{this.Roh_scientificActivity.Roh_generalQualityIndicators.Roh_generalQualityIndicatorCV.ArticleID}",  "http://w3id.org/roh/generalQualityIndicator", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_scientificActivity.Roh_generalQualityIndicators.Roh_generalQualityIndicatorCV.Roh_generalQualityIndicator).ToLower()}\"", list, " . ");
				}
			}
			}
			if(this.Roh_scientificActivity.Roh_worksSubmittedConferences != null)
			{
			foreach(var item3 in this.Roh_scientificActivity.Roh_worksSubmittedConferences)
			{
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/scientificactivity_{ResourceID}_{this.Roh_scientificActivity.ArticleID}", "http://w3id.org/roh/worksSubmittedConferences", $"<{resourceAPI.GraphsUrl}items/relatedworksubmittedconferences_{ResourceID}_{item3.ArticleID}>", list, " . ");
			if(item3.Roh_relatedWorkSubmittedConferencesCV != null)
			{
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/relatedworksubmittedconferences_{ResourceID}_{item3.ArticleID}", "http://w3id.org/roh/relatedWorkSubmittedConferencesCV", $"<{resourceAPI.GraphsUrl}items/relatedworksubmittedconferencescv_{ResourceID}_{item3.Roh_relatedWorkSubmittedConferencesCV.ArticleID}>", list, " . ");
				if(item3.Roh_relatedWorkSubmittedConferencesCV.IdRoh_participationType != null)
				{
					Regex regex = new Regex(@"\/items\/.+_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}");
					string itemRegex = item3.Roh_relatedWorkSubmittedConferencesCV.IdRoh_participationType;
					if (regex.IsMatch(itemRegex))
					{
						itemRegex = $"http://gnoss/{resourceAPI.GetShortGuid(itemRegex).ToString().ToUpper()}";
					}
					else
					{
						itemRegex = itemRegex.ToLower();
					}
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/relatedworksubmittedconferencescv_{ResourceID}_{item3.Roh_relatedWorkSubmittedConferencesCV.ArticleID}",  "http://w3id.org/roh/participationType", $"<{itemRegex}>", list, " . ");
				}
				if(item3.Roh_relatedWorkSubmittedConferencesCV.IdRoh_inscriptionType != null)
				{
					Regex regex = new Regex(@"\/items\/.+_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}");
					string itemRegex = item3.Roh_relatedWorkSubmittedConferencesCV.IdRoh_inscriptionType;
					if (regex.IsMatch(itemRegex))
					{
						itemRegex = $"http://gnoss/{resourceAPI.GetShortGuid(itemRegex).ToString().ToUpper()}";
					}
					else
					{
						itemRegex = itemRegex.ToLower();
					}
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/relatedworksubmittedconferencescv_{ResourceID}_{item3.Roh_relatedWorkSubmittedConferencesCV.ArticleID}",  "http://w3id.org/roh/inscriptionType", $"<{itemRegex}>", list, " . ");
				}
				if(item3.Roh_relatedWorkSubmittedConferencesCV.Roh_correspondingAuthor != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/relatedworksubmittedconferencescv_{ResourceID}_{item3.Roh_relatedWorkSubmittedConferencesCV.ArticleID}",  "http://w3id.org/roh/correspondingAuthor", $"\"{item3.Roh_relatedWorkSubmittedConferencesCV.Roh_correspondingAuthor.ToString().ToLower()}\"", list, " . ");
				}
				if(item3.Roh_relatedWorkSubmittedConferencesCV.Roh_inscriptionTypeOther != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/relatedworksubmittedconferencescv_{ResourceID}_{item3.Roh_relatedWorkSubmittedConferencesCV.ArticleID}",  "http://w3id.org/roh/inscriptionTypeOther", $"\"{GenerarTextoSinSaltoDeLinea(item3.Roh_relatedWorkSubmittedConferencesCV.Roh_inscriptionTypeOther).ToLower()}\"", list, " . ");
				}
			}
				if(item3.IdVivo_relatedBy != null)
				{
					Regex regex = new Regex(@"\/items\/.+_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}");
					string itemRegex = item3.IdVivo_relatedBy;
					if (regex.IsMatch(itemRegex))
					{
						itemRegex = $"http://gnoss/{resourceAPI.GetShortGuid(itemRegex).ToString().ToUpper()}";
					}
					else
					{
						itemRegex = itemRegex.ToLower();
					}
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/relatedworksubmittedconferences_{ResourceID}_{item3.ArticleID}",  "http://vivoweb.org/ontology/core#relatedBy", $"<{itemRegex}>", list, " . ");
				}
				if(item3.Roh_isPublic != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/relatedworksubmittedconferences_{ResourceID}_{item3.ArticleID}",  "http://w3id.org/roh/isPublic", $"\"{item3.Roh_isPublic.ToString().ToLower()}\"", list, " . ");
				}
			}
			}
			if(this.Roh_scientificActivity.Roh_societies != null)
			{
			foreach(var item4 in this.Roh_scientificActivity.Roh_societies)
			{
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/scientificactivity_{ResourceID}_{this.Roh_scientificActivity.ArticleID}", "http://w3id.org/roh/societies", $"<{resourceAPI.GraphsUrl}items/relatedsociety_{ResourceID}_{item4.ArticleID}>", list, " . ");
				if(item4.IdVivo_relatedBy != null)
				{
					Regex regex = new Regex(@"\/items\/.+_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}");
					string itemRegex = item4.IdVivo_relatedBy;
					if (regex.IsMatch(itemRegex))
					{
						itemRegex = $"http://gnoss/{resourceAPI.GetShortGuid(itemRegex).ToString().ToUpper()}";
					}
					else
					{
						itemRegex = itemRegex.ToLower();
					}
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/relatedsociety_{ResourceID}_{item4.ArticleID}",  "http://vivoweb.org/ontology/core#relatedBy", $"<{itemRegex}>", list, " . ");
				}
				if(item4.Roh_isPublic != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/relatedsociety_{ResourceID}_{item4.ArticleID}",  "http://w3id.org/roh/isPublic", $"\"{item4.Roh_isPublic.ToString().ToLower()}\"", list, " . ");
				}
			}
			}
			if(this.Roh_scientificActivity.Roh_otherDisseminationActivities != null)
			{
			foreach(var item5 in this.Roh_scientificActivity.Roh_otherDisseminationActivities)
			{
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/scientificactivity_{ResourceID}_{this.Roh_scientificActivity.ArticleID}", "http://w3id.org/roh/otherDisseminationActivities", $"<{resourceAPI.GraphsUrl}items/relatedotherdisseminationactivity_{ResourceID}_{item5.ArticleID}>", list, " . ");
			if(item5.Roh_relatedOtherDisseminationActivityCV != null)
			{
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/relatedotherdisseminationactivity_{ResourceID}_{item5.ArticleID}", "http://w3id.org/roh/relatedOtherDisseminationActivityCV", $"<{resourceAPI.GraphsUrl}items/relatedotherdisseminationactivitycv_{ResourceID}_{item5.Roh_relatedOtherDisseminationActivityCV.ArticleID}>", list, " . ");
				if(item5.Roh_relatedOtherDisseminationActivityCV.IdRoh_inscriptionType != null)
				{
					Regex regex = new Regex(@"\/items\/.+_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}");
					string itemRegex = item5.Roh_relatedOtherDisseminationActivityCV.IdRoh_inscriptionType;
					if (regex.IsMatch(itemRegex))
					{
						itemRegex = $"http://gnoss/{resourceAPI.GetShortGuid(itemRegex).ToString().ToUpper()}";
					}
					else
					{
						itemRegex = itemRegex.ToLower();
					}
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/relatedotherdisseminationactivitycv_{ResourceID}_{item5.Roh_relatedOtherDisseminationActivityCV.ArticleID}",  "http://w3id.org/roh/inscriptionType", $"<{itemRegex}>", list, " . ");
				}
				if(item5.Roh_relatedOtherDisseminationActivityCV.Roh_correspondingAuthor != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/relatedotherdisseminationactivitycv_{ResourceID}_{item5.Roh_relatedOtherDisseminationActivityCV.ArticleID}",  "http://w3id.org/roh/correspondingAuthor", $"\"{item5.Roh_relatedOtherDisseminationActivityCV.Roh_correspondingAuthor.ToString().ToLower()}\"", list, " . ");
				}
				if(item5.Roh_relatedOtherDisseminationActivityCV.Roh_inscriptionTypeOther != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/relatedotherdisseminationactivitycv_{ResourceID}_{item5.Roh_relatedOtherDisseminationActivityCV.ArticleID}",  "http://w3id.org/roh/inscriptionTypeOther", $"\"{GenerarTextoSinSaltoDeLinea(item5.Roh_relatedOtherDisseminationActivityCV.Roh_inscriptionTypeOther).ToLower()}\"", list, " . ");
				}
			}
				if(item5.IdVivo_relatedBy != null)
				{
					Regex regex = new Regex(@"\/items\/.+_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}");
					string itemRegex = item5.IdVivo_relatedBy;
					if (regex.IsMatch(itemRegex))
					{
						itemRegex = $"http://gnoss/{resourceAPI.GetShortGuid(itemRegex).ToString().ToUpper()}";
					}
					else
					{
						itemRegex = itemRegex.ToLower();
					}
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/relatedotherdisseminationactivity_{ResourceID}_{item5.ArticleID}",  "http://vivoweb.org/ontology/core#relatedBy", $"<{itemRegex}>", list, " . ");
				}
				if(item5.Roh_isPublic != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/relatedotherdisseminationactivity_{ResourceID}_{item5.ArticleID}",  "http://w3id.org/roh/isPublic", $"\"{item5.Roh_isPublic.ToString().ToLower()}\"", list, " . ");
				}
			}
			}
			if(this.Roh_scientificActivity.Roh_councils != null)
			{
			foreach(var item6 in this.Roh_scientificActivity.Roh_councils)
			{
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/scientificactivity_{ResourceID}_{this.Roh_scientificActivity.ArticleID}", "http://w3id.org/roh/councils", $"<{resourceAPI.GraphsUrl}items/relatedcouncil_{ResourceID}_{item6.ArticleID}>", list, " . ");
				if(item6.IdVivo_relatedBy != null)
				{
					Regex regex = new Regex(@"\/items\/.+_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}");
					string itemRegex = item6.IdVivo_relatedBy;
					if (regex.IsMatch(itemRegex))
					{
						itemRegex = $"http://gnoss/{resourceAPI.GetShortGuid(itemRegex).ToString().ToUpper()}";
					}
					else
					{
						itemRegex = itemRegex.ToLower();
					}
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/relatedcouncil_{ResourceID}_{item6.ArticleID}",  "http://vivoweb.org/ontology/core#relatedBy", $"<{itemRegex}>", list, " . ");
				}
				if(item6.Roh_isPublic != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/relatedcouncil_{ResourceID}_{item6.ArticleID}",  "http://w3id.org/roh/isPublic", $"\"{item6.Roh_isPublic.ToString().ToLower()}\"", list, " . ");
				}
			}
			}
			if(this.Roh_scientificActivity.Roh_forums != null)
			{
			foreach(var item7 in this.Roh_scientificActivity.Roh_forums)
			{
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/scientificactivity_{ResourceID}_{this.Roh_scientificActivity.ArticleID}", "http://w3id.org/roh/forums", $"<{resourceAPI.GraphsUrl}items/relatedforum_{ResourceID}_{item7.ArticleID}>", list, " . ");
			if(item7.Roh_relatedForumCV != null)
			{
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/relatedforum_{ResourceID}_{item7.ArticleID}", "http://w3id.org/roh/relatedForumCV", $"<{resourceAPI.GraphsUrl}items/relatedforumcv_{ResourceID}_{item7.Roh_relatedForumCV.ArticleID}>", list, " . ");
				if(item7.Roh_relatedForumCV.IdRoh_represents != null)
				{
					Regex regex = new Regex(@"\/items\/.+_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}");
					string itemRegex = item7.Roh_relatedForumCV.IdRoh_represents;
					if (regex.IsMatch(itemRegex))
					{
						itemRegex = $"http://gnoss/{resourceAPI.GetShortGuid(itemRegex).ToString().ToUpper()}";
					}
					else
					{
						itemRegex = itemRegex.ToLower();
					}
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/relatedforumcv_{ResourceID}_{item7.Roh_relatedForumCV.ArticleID}",  "http://w3id.org/roh/represents", $"<{itemRegex}>", list, " . ");
				}
				if(item7.Roh_relatedForumCV.Vivo_start != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/relatedforumcv_{ResourceID}_{item7.Roh_relatedForumCV.ArticleID}",  "http://vivoweb.org/ontology/core#start", $"{item7.Roh_relatedForumCV.Vivo_start.Value.ToString("yyyyMMddHHmmss")}", list, " . ");
				}
				if(item7.Roh_relatedForumCV.Roh_professionalCategory != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/relatedforumcv_{ResourceID}_{item7.Roh_relatedForumCV.ArticleID}",  "http://w3id.org/roh/professionalCategory", $"\"{GenerarTextoSinSaltoDeLinea(item7.Roh_relatedForumCV.Roh_professionalCategory).ToLower()}\"", list, " . ");
				}
				if(item7.Roh_relatedForumCV.Vivo_end != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/relatedforumcv_{ResourceID}_{item7.Roh_relatedForumCV.ArticleID}",  "http://vivoweb.org/ontology/core#end", $"{item7.Roh_relatedForumCV.Vivo_end.Value.ToString("yyyyMMddHHmmss")}", list, " . ");
				}
			}
				if(item7.IdVivo_relatedBy != null)
				{
					Regex regex = new Regex(@"\/items\/.+_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}");
					string itemRegex = item7.IdVivo_relatedBy;
					if (regex.IsMatch(itemRegex))
					{
						itemRegex = $"http://gnoss/{resourceAPI.GetShortGuid(itemRegex).ToString().ToUpper()}";
					}
					else
					{
						itemRegex = itemRegex.ToLower();
					}
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/relatedforum_{ResourceID}_{item7.ArticleID}",  "http://vivoweb.org/ontology/core#relatedBy", $"<{itemRegex}>", list, " . ");
				}
				if(item7.Roh_isPublic != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/relatedforum_{ResourceID}_{item7.ArticleID}",  "http://w3id.org/roh/isPublic", $"\"{item7.Roh_isPublic.ToString().ToLower()}\"", list, " . ");
				}
			}
			}
			if(this.Roh_scientificActivity.Roh_researchActivityPeriods != null)
			{
			foreach(var item8 in this.Roh_scientificActivity.Roh_researchActivityPeriods)
			{
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/scientificactivity_{ResourceID}_{this.Roh_scientificActivity.ArticleID}", "http://w3id.org/roh/researchActivityPeriods", $"<{resourceAPI.GraphsUrl}items/relatedresearchactivityperiod_{ResourceID}_{item8.ArticleID}>", list, " . ");
				if(item8.IdVivo_relatedBy != null)
				{
					Regex regex = new Regex(@"\/items\/.+_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}");
					string itemRegex = item8.IdVivo_relatedBy;
					if (regex.IsMatch(itemRegex))
					{
						itemRegex = $"http://gnoss/{resourceAPI.GetShortGuid(itemRegex).ToString().ToUpper()}";
					}
					else
					{
						itemRegex = itemRegex.ToLower();
					}
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/relatedresearchactivityperiod_{ResourceID}_{item8.ArticleID}",  "http://vivoweb.org/ontology/core#relatedBy", $"<{itemRegex}>", list, " . ");
				}
				if(item8.Roh_isPublic != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/relatedresearchactivityperiod_{ResourceID}_{item8.ArticleID}",  "http://w3id.org/roh/isPublic", $"\"{item8.Roh_isPublic.ToString().ToLower()}\"", list, " . ");
				}
			}
			}
			if(this.Roh_scientificActivity.Roh_scientificPublications != null)
			{
			foreach(var item9 in this.Roh_scientificActivity.Roh_scientificPublications)
			{
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/scientificactivity_{ResourceID}_{this.Roh_scientificActivity.ArticleID}", "http://w3id.org/roh/scientificPublications", $"<{resourceAPI.GraphsUrl}items/relatedscientificpublication_{ResourceID}_{item9.ArticleID}>", list, " . ");
			if(item9.Roh_relatedScientificPublicationCV != null)
			{
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/relatedscientificpublication_{ResourceID}_{item9.ArticleID}", "http://w3id.org/roh/relatedScientificPublicationCV", $"<{resourceAPI.GraphsUrl}items/relatedscientificpublicationcv_{ResourceID}_{item9.Roh_relatedScientificPublicationCV.ArticleID}>", list, " . ");
				if(item9.Roh_relatedScientificPublicationCV.IdRoh_contributionGrade != null)
				{
					Regex regex = new Regex(@"\/items\/.+_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}");
					string itemRegex = item9.Roh_relatedScientificPublicationCV.IdRoh_contributionGrade;
					if (regex.IsMatch(itemRegex))
					{
						itemRegex = $"http://gnoss/{resourceAPI.GetShortGuid(itemRegex).ToString().ToUpper()}";
					}
					else
					{
						itemRegex = itemRegex.ToLower();
					}
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/relatedscientificpublicationcv_{ResourceID}_{item9.Roh_relatedScientificPublicationCV.ArticleID}",  "http://w3id.org/roh/contributionGrade", $"<{itemRegex}>", list, " . ");
				}
				if(item9.Roh_relatedScientificPublicationCV.Roh_relevantResults != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/relatedscientificpublicationcv_{ResourceID}_{item9.Roh_relatedScientificPublicationCV.ArticleID}",  "http://w3id.org/roh/relevantResults", $"\"{GenerarTextoSinSaltoDeLinea(item9.Roh_relatedScientificPublicationCV.Roh_relevantResults).ToLower()}\"", list, " . ");
				}
				if(item9.Roh_relatedScientificPublicationCV.Roh_correspondingAuthor != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/relatedscientificpublicationcv_{ResourceID}_{item9.Roh_relatedScientificPublicationCV.ArticleID}",  "http://w3id.org/roh/correspondingAuthor", $"\"{item9.Roh_relatedScientificPublicationCV.Roh_correspondingAuthor.ToString().ToLower()}\"", list, " . ");
				}
				if(item9.Roh_relatedScientificPublicationCV.Roh_relevantPublication != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/relatedscientificpublicationcv_{ResourceID}_{item9.Roh_relatedScientificPublicationCV.ArticleID}",  "http://w3id.org/roh/relevantPublication", $"\"{item9.Roh_relatedScientificPublicationCV.Roh_relevantPublication.ToString().ToLower()}\"", list, " . ");
				}
			}
				if(item9.IdVivo_relatedBy != null)
				{
					Regex regex = new Regex(@"\/items\/.+_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}");
					string itemRegex = item9.IdVivo_relatedBy;
					if (regex.IsMatch(itemRegex))
					{
						itemRegex = $"http://gnoss/{resourceAPI.GetShortGuid(itemRegex).ToString().ToUpper()}";
					}
					else
					{
						itemRegex = itemRegex.ToLower();
					}
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/relatedscientificpublication_{ResourceID}_{item9.ArticleID}",  "http://vivoweb.org/ontology/core#relatedBy", $"<{itemRegex}>", list, " . ");
				}
				if(item9.Roh_isPublic != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/relatedscientificpublication_{ResourceID}_{item9.ArticleID}",  "http://w3id.org/roh/isPublic", $"\"{item9.Roh_isPublic.ToString().ToLower()}\"", list, " . ");
				}
			}
			}
			if(this.Roh_scientificActivity.Roh_otherAchievements != null)
			{
			foreach(var item10 in this.Roh_scientificActivity.Roh_otherAchievements)
			{
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/scientificactivity_{ResourceID}_{this.Roh_scientificActivity.ArticleID}", "http://w3id.org/roh/otherAchievements", $"<{resourceAPI.GraphsUrl}items/relatedotherachievement_{ResourceID}_{item10.ArticleID}>", list, " . ");
				if(item10.IdVivo_relatedBy != null)
				{
					Regex regex = new Regex(@"\/items\/.+_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}");
					string itemRegex = item10.IdVivo_relatedBy;
					if (regex.IsMatch(itemRegex))
					{
						itemRegex = $"http://gnoss/{resourceAPI.GetShortGuid(itemRegex).ToString().ToUpper()}";
					}
					else
					{
						itemRegex = itemRegex.ToLower();
					}
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/relatedotherachievement_{ResourceID}_{item10.ArticleID}",  "http://vivoweb.org/ontology/core#relatedBy", $"<{itemRegex}>", list, " . ");
				}
				if(item10.Roh_isPublic != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/relatedotherachievement_{ResourceID}_{item10.ArticleID}",  "http://w3id.org/roh/isPublic", $"\"{item10.Roh_isPublic.ToString().ToLower()}\"", list, " . ");
				}
			}
			}
			if(this.Roh_scientificActivity.Roh_researchObjects != null)
			{
			foreach(var item11 in this.Roh_scientificActivity.Roh_researchObjects)
			{
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/scientificactivity_{ResourceID}_{this.Roh_scientificActivity.ArticleID}", "http://w3id.org/roh/researchObjects", $"<{resourceAPI.GraphsUrl}items/relatedresearchobject_{ResourceID}_{item11.ArticleID}>", list, " . ");
				if(item11.IdVivo_relatedBy != null)
				{
					Regex regex = new Regex(@"\/items\/.+_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}");
					string itemRegex = item11.IdVivo_relatedBy;
					if (regex.IsMatch(itemRegex))
					{
						itemRegex = $"http://gnoss/{resourceAPI.GetShortGuid(itemRegex).ToString().ToUpper()}";
					}
					else
					{
						itemRegex = itemRegex.ToLower();
					}
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/relatedresearchobject_{ResourceID}_{item11.ArticleID}",  "http://vivoweb.org/ontology/core#relatedBy", $"<{itemRegex}>", list, " . ");
				}
				if(item11.Roh_isPublic != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/relatedresearchobject_{ResourceID}_{item11.ArticleID}",  "http://w3id.org/roh/isPublic", $"\"{item11.Roh_isPublic.ToString().ToLower()}\"", list, " . ");
				}
			}
			}
			if(this.Roh_scientificActivity.Roh_researchEvaluations != null)
			{
			foreach(var item12 in this.Roh_scientificActivity.Roh_researchEvaluations)
			{
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/scientificactivity_{ResourceID}_{this.Roh_scientificActivity.ArticleID}", "http://w3id.org/roh/researchEvaluations", $"<{resourceAPI.GraphsUrl}items/relatedresearchevaluation_{ResourceID}_{item12.ArticleID}>", list, " . ");
			if(item12.Roh_relatedResearchEvaluationCV != null)
			{
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/relatedresearchevaluation_{ResourceID}_{item12.ArticleID}", "http://w3id.org/roh/relatedResearchEvaluationCV", $"<{resourceAPI.GraphsUrl}items/relatedresearchevaluationcv_{ResourceID}_{item12.Roh_relatedResearchEvaluationCV.ArticleID}>", list, " . ");
				if(item12.Roh_relatedResearchEvaluationCV.IdRoh_accessSystem != null)
				{
					Regex regex = new Regex(@"\/items\/.+_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}");
					string itemRegex = item12.Roh_relatedResearchEvaluationCV.IdRoh_accessSystem;
					if (regex.IsMatch(itemRegex))
					{
						itemRegex = $"http://gnoss/{resourceAPI.GetShortGuid(itemRegex).ToString().ToUpper()}";
					}
					else
					{
						itemRegex = itemRegex.ToLower();
					}
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/relatedresearchevaluationcv_{ResourceID}_{item12.Roh_relatedResearchEvaluationCV.ArticleID}",  "http://w3id.org/roh/accessSystem", $"<{itemRegex}>", list, " . ");
				}
				if(item12.Roh_relatedResearchEvaluationCV.Vivo_start != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/relatedresearchevaluationcv_{ResourceID}_{item12.Roh_relatedResearchEvaluationCV.ArticleID}",  "http://vivoweb.org/ontology/core#start", $"{item12.Roh_relatedResearchEvaluationCV.Vivo_start.Value.ToString("yyyyMMddHHmmss")}", list, " . ");
				}
				if(item12.Roh_relatedResearchEvaluationCV.Roh_frequency != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/relatedresearchevaluationcv_{ResourceID}_{item12.Roh_relatedResearchEvaluationCV.ArticleID}",  "http://w3id.org/roh/frequency", $"{item12.Roh_relatedResearchEvaluationCV.Roh_frequency.Value.ToString(new CultureInfo("en-US"))}", list, " . ");
				}
				if(item12.Roh_relatedResearchEvaluationCV.Roh_performedTasks != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/relatedresearchevaluationcv_{ResourceID}_{item12.Roh_relatedResearchEvaluationCV.ArticleID}",  "http://w3id.org/roh/performedTasks", $"\"{GenerarTextoSinSaltoDeLinea(item12.Roh_relatedResearchEvaluationCV.Roh_performedTasks).ToLower()}\"", list, " . ");
				}
				if(item12.Roh_relatedResearchEvaluationCV.Vivo_end != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/relatedresearchevaluationcv_{ResourceID}_{item12.Roh_relatedResearchEvaluationCV.ArticleID}",  "http://vivoweb.org/ontology/core#end", $"{item12.Roh_relatedResearchEvaluationCV.Vivo_end.Value.ToString("yyyyMMddHHmmss")}", list, " . ");
				}
			}
				if(item12.IdVivo_relatedBy != null)
				{
					Regex regex = new Regex(@"\/items\/.+_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}");
					string itemRegex = item12.IdVivo_relatedBy;
					if (regex.IsMatch(itemRegex))
					{
						itemRegex = $"http://gnoss/{resourceAPI.GetShortGuid(itemRegex).ToString().ToUpper()}";
					}
					else
					{
						itemRegex = itemRegex.ToLower();
					}
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/relatedresearchevaluation_{ResourceID}_{item12.ArticleID}",  "http://vivoweb.org/ontology/core#relatedBy", $"<{itemRegex}>", list, " . ");
				}
				if(item12.Roh_isPublic != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/relatedresearchevaluation_{ResourceID}_{item12.ArticleID}",  "http://w3id.org/roh/isPublic", $"\"{item12.Roh_isPublic.ToString().ToLower()}\"", list, " . ");
				}
			}
			}
			if(this.Roh_scientificActivity.Roh_activitiesOrganization != null)
			{
			foreach(var item13 in this.Roh_scientificActivity.Roh_activitiesOrganization)
			{
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/scientificactivity_{ResourceID}_{this.Roh_scientificActivity.ArticleID}", "http://w3id.org/roh/activitiesOrganization", $"<{resourceAPI.GraphsUrl}items/relatedactivityorganization_{ResourceID}_{item13.ArticleID}>", list, " . ");
			if(item13.Roh_relatedActivityOrganizationCV != null)
			{
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/relatedactivityorganization_{ResourceID}_{item13.ArticleID}", "http://w3id.org/roh/relatedActivityOrganizationCV", $"<{resourceAPI.GraphsUrl}items/relatedactivityorganizationcv_{ResourceID}_{item13.Roh_relatedActivityOrganizationCV.ArticleID}>", list, " . ");
				if(item13.Roh_relatedActivityOrganizationCV.IdRoh_participationType != null)
				{
					Regex regex = new Regex(@"\/items\/.+_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}");
					string itemRegex = item13.Roh_relatedActivityOrganizationCV.IdRoh_participationType;
					if (regex.IsMatch(itemRegex))
					{
						itemRegex = $"http://gnoss/{resourceAPI.GetShortGuid(itemRegex).ToString().ToUpper()}";
					}
					else
					{
						itemRegex = itemRegex.ToLower();
					}
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/relatedactivityorganizationcv_{ResourceID}_{item13.Roh_relatedActivityOrganizationCV.ArticleID}",  "http://w3id.org/roh/participationType", $"<{itemRegex}>", list, " . ");
				}
				if(item13.Roh_relatedActivityOrganizationCV.Roh_durationDays != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/relatedactivityorganizationcv_{ResourceID}_{item13.Roh_relatedActivityOrganizationCV.ArticleID}",  "http://w3id.org/roh/durationDays", $"\"{GenerarTextoSinSaltoDeLinea(item13.Roh_relatedActivityOrganizationCV.Roh_durationDays).ToLower()}\"", list, " . ");
				}
				if(item13.Roh_relatedActivityOrganizationCV.Vivo_start != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/relatedactivityorganizationcv_{ResourceID}_{item13.Roh_relatedActivityOrganizationCV.ArticleID}",  "http://vivoweb.org/ontology/core#start", $"{item13.Roh_relatedActivityOrganizationCV.Vivo_start.Value.ToString("yyyyMMddHHmmss")}", list, " . ");
				}
				if(item13.Roh_relatedActivityOrganizationCV.Roh_durationMonths != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/relatedactivityorganizationcv_{ResourceID}_{item13.Roh_relatedActivityOrganizationCV.ArticleID}",  "http://w3id.org/roh/durationMonths", $"\"{GenerarTextoSinSaltoDeLinea(item13.Roh_relatedActivityOrganizationCV.Roh_durationMonths).ToLower()}\"", list, " . ");
				}
				if(item13.Roh_relatedActivityOrganizationCV.Roh_participationTypeOther != null)
				{
					foreach(var item2 in item13.Roh_relatedActivityOrganizationCV.Roh_participationTypeOther)
					{
						AgregarTripleALista($"{resourceAPI.GraphsUrl}items/relatedactivityorganizationcv_{ResourceID}_{item13.Roh_relatedActivityOrganizationCV.ArticleID}", "http://w3id.org/roh/participationTypeOther", $"\"{GenerarTextoSinSaltoDeLinea(item2).ToLower()}\"", list, " . ");
					}
				}
				if(item13.Roh_relatedActivityOrganizationCV.Roh_durationYears != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/relatedactivityorganizationcv_{ResourceID}_{item13.Roh_relatedActivityOrganizationCV.ArticleID}",  "http://w3id.org/roh/durationYears", $"\"{GenerarTextoSinSaltoDeLinea(item13.Roh_relatedActivityOrganizationCV.Roh_durationYears).ToLower()}\"", list, " . ");
				}
				if(item13.Roh_relatedActivityOrganizationCV.Vivo_end != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/relatedactivityorganizationcv_{ResourceID}_{item13.Roh_relatedActivityOrganizationCV.ArticleID}",  "http://vivoweb.org/ontology/core#end", $"{item13.Roh_relatedActivityOrganizationCV.Vivo_end.Value.ToString("yyyyMMddHHmmss")}", list, " . ");
				}
			}
				if(item13.IdVivo_relatedBy != null)
				{
					Regex regex = new Regex(@"\/items\/.+_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}");
					string itemRegex = item13.IdVivo_relatedBy;
					if (regex.IsMatch(itemRegex))
					{
						itemRegex = $"http://gnoss/{resourceAPI.GetShortGuid(itemRegex).ToString().ToUpper()}";
					}
					else
					{
						itemRegex = itemRegex.ToLower();
					}
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/relatedactivityorganization_{ResourceID}_{item13.ArticleID}",  "http://vivoweb.org/ontology/core#relatedBy", $"<{itemRegex}>", list, " . ");
				}
				if(item13.Roh_isPublic != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/relatedactivityorganization_{ResourceID}_{item13.ArticleID}",  "http://w3id.org/roh/isPublic", $"\"{item13.Roh_isPublic.ToString().ToLower()}\"", list, " . ");
				}
			}
			}
			if(this.Roh_scientificActivity.Roh_otherCollaborations != null)
			{
			foreach(var item14 in this.Roh_scientificActivity.Roh_otherCollaborations)
			{
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/scientificactivity_{ResourceID}_{this.Roh_scientificActivity.ArticleID}", "http://w3id.org/roh/otherCollaborations", $"<{resourceAPI.GraphsUrl}items/relatedothercollaboration_{ResourceID}_{item14.ArticleID}>", list, " . ");
				if(item14.IdVivo_relatedBy != null)
				{
					Regex regex = new Regex(@"\/items\/.+_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}");
					string itemRegex = item14.IdVivo_relatedBy;
					if (regex.IsMatch(itemRegex))
					{
						itemRegex = $"http://gnoss/{resourceAPI.GetShortGuid(itemRegex).ToString().ToUpper()}";
					}
					else
					{
						itemRegex = itemRegex.ToLower();
					}
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/relatedothercollaboration_{ResourceID}_{item14.ArticleID}",  "http://vivoweb.org/ontology/core#relatedBy", $"<{itemRegex}>", list, " . ");
				}
				if(item14.Roh_isPublic != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/relatedothercollaboration_{ResourceID}_{item14.ArticleID}",  "http://w3id.org/roh/isPublic", $"\"{item14.Roh_isPublic.ToString().ToLower()}\"", list, " . ");
				}
			}
			}
			if(this.Roh_scientificActivity.Roh_networks != null)
			{
			foreach(var item15 in this.Roh_scientificActivity.Roh_networks)
			{
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/scientificactivity_{ResourceID}_{this.Roh_scientificActivity.ArticleID}", "http://w3id.org/roh/networks", $"<{resourceAPI.GraphsUrl}items/relatednetwork_{ResourceID}_{item15.ArticleID}>", list, " . ");
				if(item15.IdVivo_relatedBy != null)
				{
					Regex regex = new Regex(@"\/items\/.+_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}");
					string itemRegex = item15.IdVivo_relatedBy;
					if (regex.IsMatch(itemRegex))
					{
						itemRegex = $"http://gnoss/{resourceAPI.GetShortGuid(itemRegex).ToString().ToUpper()}";
					}
					else
					{
						itemRegex = itemRegex.ToLower();
					}
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/relatednetwork_{ResourceID}_{item15.ArticleID}",  "http://vivoweb.org/ontology/core#relatedBy", $"<{itemRegex}>", list, " . ");
				}
				if(item15.Roh_isPublic != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/relatednetwork_{ResourceID}_{item15.ArticleID}",  "http://w3id.org/roh/isPublic", $"\"{item15.Roh_isPublic.ToString().ToLower()}\"", list, " . ");
				}
			}
			}
			if(this.Roh_scientificActivity.Roh_grants != null)
			{
			foreach(var item16 in this.Roh_scientificActivity.Roh_grants)
			{
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/scientificactivity_{ResourceID}_{this.Roh_scientificActivity.ArticleID}", "http://w3id.org/roh/grants", $"<{resourceAPI.GraphsUrl}items/relatedgrant_{ResourceID}_{item16.ArticleID}>", list, " . ");
				if(item16.IdVivo_relatedBy != null)
				{
					Regex regex = new Regex(@"\/items\/.+_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}");
					string itemRegex = item16.IdVivo_relatedBy;
					if (regex.IsMatch(itemRegex))
					{
						itemRegex = $"http://gnoss/{resourceAPI.GetShortGuid(itemRegex).ToString().ToUpper()}";
					}
					else
					{
						itemRegex = itemRegex.ToLower();
					}
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/relatedgrant_{ResourceID}_{item16.ArticleID}",  "http://vivoweb.org/ontology/core#relatedBy", $"<{itemRegex}>", list, " . ");
				}
				if(item16.Roh_isPublic != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/relatedgrant_{ResourceID}_{item16.ArticleID}",  "http://w3id.org/roh/isPublic", $"\"{item16.Roh_isPublic.ToString().ToLower()}\"", list, " . ");
				}
			}
			}
			if(this.Roh_scientificActivity.Roh_activitiesManagement != null)
			{
			foreach(var item17 in this.Roh_scientificActivity.Roh_activitiesManagement)
			{
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/scientificactivity_{ResourceID}_{this.Roh_scientificActivity.ArticleID}", "http://w3id.org/roh/activitiesManagement", $"<{resourceAPI.GraphsUrl}items/relatedactivitymanagement_{ResourceID}_{item17.ArticleID}>", list, " . ");
			if(item17.Roh_relatedActivityManagementCV != null)
			{
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/relatedactivitymanagement_{ResourceID}_{item17.ArticleID}", "http://w3id.org/roh/relatedActivityManagementCV", $"<{resourceAPI.GraphsUrl}items/relatedactivitymanagementcv_{ResourceID}_{item17.Roh_relatedActivityManagementCV.ArticleID}>", list, " . ");
			if(item17.Roh_relatedActivityManagementCV.Roh_hasKnowledgeArea != null)
			{
			foreach(var item19 in item17.Roh_relatedActivityManagementCV.Roh_hasKnowledgeArea)
			{
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/relatedactivitymanagementcv_{ResourceID}_{item17.Roh_relatedActivityManagementCV.ArticleID}", "http://w3id.org/roh/hasKnowledgeArea", $"<{resourceAPI.GraphsUrl}items/categorypath_{ResourceID}_{item19.ArticleID}>", list, " . ");
				if(item19.IdsRoh_categoryNode != null)
				{
					foreach(var item2 in item19.IdsRoh_categoryNode)
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
						AgregarTripleALista($"{resourceAPI.GraphsUrl}items/categorypath_{ResourceID}_{item19.ArticleID}", "http://w3id.org/roh/categoryNode",  $"<{itemRegex}>", list, " . ");
					}
				}
			}
			}
				if(item17.Roh_relatedActivityManagementCV.IdRoh_accessSystem != null)
				{
					Regex regex = new Regex(@"\/items\/.+_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}");
					string itemRegex = item17.Roh_relatedActivityManagementCV.IdRoh_accessSystem;
					if (regex.IsMatch(itemRegex))
					{
						itemRegex = $"http://gnoss/{resourceAPI.GetShortGuid(itemRegex).ToString().ToUpper()}";
					}
					else
					{
						itemRegex = itemRegex.ToLower();
					}
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/relatedactivitymanagementcv_{ResourceID}_{item17.Roh_relatedActivityManagementCV.ArticleID}",  "http://w3id.org/roh/accessSystem", $"<{itemRegex}>", list, " . ");
				}
				if(item17.Roh_relatedActivityManagementCV.Roh_specificTasks != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/relatedactivitymanagementcv_{ResourceID}_{item17.Roh_relatedActivityManagementCV.ArticleID}",  "http://w3id.org/roh/specificTasks", $"\"{GenerarTextoSinSaltoDeLinea(item17.Roh_relatedActivityManagementCV.Roh_specificTasks).ToLower()}\"", list, " . ");
				}
				if(item17.Roh_relatedActivityManagementCV.Roh_durationDays != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/relatedactivitymanagementcv_{ResourceID}_{item17.Roh_relatedActivityManagementCV.ArticleID}",  "http://w3id.org/roh/durationDays", $"\"{GenerarTextoSinSaltoDeLinea(item17.Roh_relatedActivityManagementCV.Roh_durationDays).ToLower()}\"", list, " . ");
				}
				if(item17.Roh_relatedActivityManagementCV.Vivo_start != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/relatedactivitymanagementcv_{ResourceID}_{item17.Roh_relatedActivityManagementCV.ArticleID}",  "http://vivoweb.org/ontology/core#start", $"{item17.Roh_relatedActivityManagementCV.Vivo_start.Value.ToString("yyyyMMddHHmmss")}", list, " . ");
				}
				if(item17.Roh_relatedActivityManagementCV.Roh_goals != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/relatedactivitymanagementcv_{ResourceID}_{item17.Roh_relatedActivityManagementCV.ArticleID}",  "http://w3id.org/roh/goals", $"\"{GenerarTextoSinSaltoDeLinea(item17.Roh_relatedActivityManagementCV.Roh_goals).ToLower()}\"", list, " . ");
				}
				if(item17.Roh_relatedActivityManagementCV.Roh_performedTasks != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/relatedactivitymanagementcv_{ResourceID}_{item17.Roh_relatedActivityManagementCV.ArticleID}",  "http://w3id.org/roh/performedTasks", $"\"{GenerarTextoSinSaltoDeLinea(item17.Roh_relatedActivityManagementCV.Roh_performedTasks).ToLower()}\"", list, " . ");
				}
				if(item17.Roh_relatedActivityManagementCV.Roh_accessSystemOther != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/relatedactivitymanagementcv_{ResourceID}_{item17.Roh_relatedActivityManagementCV.ArticleID}",  "http://w3id.org/roh/accessSystemOther", $"\"{GenerarTextoSinSaltoDeLinea(item17.Roh_relatedActivityManagementCV.Roh_accessSystemOther).ToLower()}\"", list, " . ");
				}
				if(item17.Roh_relatedActivityManagementCV.Roh_durationMonths != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/relatedactivitymanagementcv_{ResourceID}_{item17.Roh_relatedActivityManagementCV.ArticleID}",  "http://w3id.org/roh/durationMonths", $"\"{GenerarTextoSinSaltoDeLinea(item17.Roh_relatedActivityManagementCV.Roh_durationMonths).ToLower()}\"", list, " . ");
				}
				if(item17.Roh_relatedActivityManagementCV.Roh_durationYears != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/relatedactivitymanagementcv_{ResourceID}_{item17.Roh_relatedActivityManagementCV.ArticleID}",  "http://w3id.org/roh/durationYears", $"\"{GenerarTextoSinSaltoDeLinea(item17.Roh_relatedActivityManagementCV.Roh_durationYears).ToLower()}\"", list, " . ");
				}
				if(item17.Roh_relatedActivityManagementCV.Vivo_end != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/relatedactivitymanagementcv_{ResourceID}_{item17.Roh_relatedActivityManagementCV.ArticleID}",  "http://vivoweb.org/ontology/core#end", $"{item17.Roh_relatedActivityManagementCV.Vivo_end.Value.ToString("yyyyMMddHHmmss")}", list, " . ");
				}
			}
				if(item17.IdVivo_relatedBy != null)
				{
					Regex regex = new Regex(@"\/items\/.+_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}");
					string itemRegex = item17.IdVivo_relatedBy;
					if (regex.IsMatch(itemRegex))
					{
						itemRegex = $"http://gnoss/{resourceAPI.GetShortGuid(itemRegex).ToString().ToUpper()}";
					}
					else
					{
						itemRegex = itemRegex.ToLower();
					}
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/relatedactivitymanagement_{ResourceID}_{item17.ArticleID}",  "http://vivoweb.org/ontology/core#relatedBy", $"<{itemRegex}>", list, " . ");
				}
				if(item17.Roh_isPublic != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/relatedactivitymanagement_{ResourceID}_{item17.ArticleID}",  "http://w3id.org/roh/isPublic", $"\"{item17.Roh_isPublic.ToString().ToLower()}\"", list, " . ");
				}
			}
			}
			if(this.Roh_scientificActivity.Roh_prizes != null)
			{
			foreach(var item18 in this.Roh_scientificActivity.Roh_prizes)
			{
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/scientificactivity_{ResourceID}_{this.Roh_scientificActivity.ArticleID}", "http://w3id.org/roh/prizes", $"<{resourceAPI.GraphsUrl}items/relatedprize_{ResourceID}_{item18.ArticleID}>", list, " . ");
				if(item18.IdVivo_relatedBy != null)
				{
					Regex regex = new Regex(@"\/items\/.+_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}");
					string itemRegex = item18.IdVivo_relatedBy;
					if (regex.IsMatch(itemRegex))
					{
						itemRegex = $"http://gnoss/{resourceAPI.GetShortGuid(itemRegex).ToString().ToUpper()}";
					}
					else
					{
						itemRegex = itemRegex.ToLower();
					}
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/relatedprize_{ResourceID}_{item18.ArticleID}",  "http://vivoweb.org/ontology/core#relatedBy", $"<{itemRegex}>", list, " . ");
				}
				if(item18.Roh_isPublic != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/relatedprize_{ResourceID}_{item18.ArticleID}",  "http://w3id.org/roh/isPublic", $"\"{item18.Roh_isPublic.ToString().ToLower()}\"", list, " . ");
				}
			}
			}
			if(this.Roh_scientificActivity.Roh_committees != null)
			{
			foreach(var item19 in this.Roh_scientificActivity.Roh_committees)
			{
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/scientificactivity_{ResourceID}_{this.Roh_scientificActivity.ArticleID}", "http://w3id.org/roh/committees", $"<{resourceAPI.GraphsUrl}items/relatedcommittee_{ResourceID}_{item19.ArticleID}>", list, " . ");
			if(item19.Roh_relatedCommitteeCV != null)
			{
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/relatedcommittee_{ResourceID}_{item19.ArticleID}", "http://w3id.org/roh/relatedCommitteeCV", $"<{resourceAPI.GraphsUrl}items/relatedcommitteecv_{ResourceID}_{item19.Roh_relatedCommitteeCV.ArticleID}>", list, " . ");
				if(item19.Roh_relatedCommitteeCV.Vivo_start != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/relatedcommitteecv_{ResourceID}_{item19.Roh_relatedCommitteeCV.ArticleID}",  "http://vivoweb.org/ontology/core#start", $"{item19.Roh_relatedCommitteeCV.Vivo_start.Value.ToString("yyyyMMddHHmmss")}", list, " . ");
				}
				if(item19.Roh_relatedCommitteeCV.Vivo_end != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/relatedcommitteecv_{ResourceID}_{item19.Roh_relatedCommitteeCV.ArticleID}",  "http://vivoweb.org/ontology/core#end", $"{item19.Roh_relatedCommitteeCV.Vivo_end.Value.ToString("yyyyMMddHHmmss")}", list, " . ");
				}
			}
				if(item19.IdVivo_relatedBy != null)
				{
					Regex regex = new Regex(@"\/items\/.+_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}");
					string itemRegex = item19.IdVivo_relatedBy;
					if (regex.IsMatch(itemRegex))
					{
						itemRegex = $"http://gnoss/{resourceAPI.GetShortGuid(itemRegex).ToString().ToUpper()}";
					}
					else
					{
						itemRegex = itemRegex.ToLower();
					}
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/relatedcommittee_{ResourceID}_{item19.ArticleID}",  "http://vivoweb.org/ontology/core#relatedBy", $"<{itemRegex}>", list, " . ");
				}
				if(item19.Roh_isPublic != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/relatedcommittee_{ResourceID}_{item19.ArticleID}",  "http://w3id.org/roh/isPublic", $"\"{item19.Roh_isPublic.ToString().ToLower()}\"", list, " . ");
				}
			}
			}
			if(this.Roh_scientificActivity.Roh_stays != null)
			{
			foreach(var item20 in this.Roh_scientificActivity.Roh_stays)
			{
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/scientificactivity_{ResourceID}_{this.Roh_scientificActivity.ArticleID}", "http://w3id.org/roh/stays", $"<{resourceAPI.GraphsUrl}items/relatedstay_{ResourceID}_{item20.ArticleID}>", list, " . ");
				if(item20.IdVivo_relatedBy != null)
				{
					Regex regex = new Regex(@"\/items\/.+_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}");
					string itemRegex = item20.IdVivo_relatedBy;
					if (regex.IsMatch(itemRegex))
					{
						itemRegex = $"http://gnoss/{resourceAPI.GetShortGuid(itemRegex).ToString().ToUpper()}";
					}
					else
					{
						itemRegex = itemRegex.ToLower();
					}
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/relatedstay_{ResourceID}_{item20.ArticleID}",  "http://vivoweb.org/ontology/core#relatedBy", $"<{itemRegex}>", list, " . ");
				}
				if(item20.Roh_isPublic != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/relatedstay_{ResourceID}_{item20.ArticleID}",  "http://w3id.org/roh/isPublic", $"\"{item20.Roh_isPublic.ToString().ToLower()}\"", list, " . ");
				}
			}
			}
			if(this.Roh_scientificActivity.Roh_obtainedRecognitions != null)
			{
			foreach(var item21 in this.Roh_scientificActivity.Roh_obtainedRecognitions)
			{
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/scientificactivity_{ResourceID}_{this.Roh_scientificActivity.ArticleID}", "http://w3id.org/roh/obtainedRecognitions", $"<{resourceAPI.GraphsUrl}items/relatedobtainedrecognition_{ResourceID}_{item21.ArticleID}>", list, " . ");
				if(item21.IdVivo_relatedBy != null)
				{
					Regex regex = new Regex(@"\/items\/.+_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}");
					string itemRegex = item21.IdVivo_relatedBy;
					if (regex.IsMatch(itemRegex))
					{
						itemRegex = $"http://gnoss/{resourceAPI.GetShortGuid(itemRegex).ToString().ToUpper()}";
					}
					else
					{
						itemRegex = itemRegex.ToLower();
					}
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/relatedobtainedrecognition_{ResourceID}_{item21.ArticleID}",  "http://vivoweb.org/ontology/core#relatedBy", $"<{itemRegex}>", list, " . ");
				}
				if(item21.Roh_isPublic != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/relatedobtainedrecognition_{ResourceID}_{item21.ArticleID}",  "http://w3id.org/roh/isPublic", $"\"{item21.Roh_isPublic.ToString().ToLower()}\"", list, " . ");
				}
			}
			}
			if(this.Roh_scientificActivity.Roh_worksSubmittedSeminars != null)
			{
			foreach(var item22 in this.Roh_scientificActivity.Roh_worksSubmittedSeminars)
			{
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/scientificactivity_{ResourceID}_{this.Roh_scientificActivity.ArticleID}", "http://w3id.org/roh/worksSubmittedSeminars", $"<{resourceAPI.GraphsUrl}items/relatedworksubmittedseminars_{ResourceID}_{item22.ArticleID}>", list, " . ");
			if(item22.Roh_relatedWorkSubmittedSeminars != null)
			{
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/relatedworksubmittedseminars_{ResourceID}_{item22.ArticleID}", "http://w3id.org/roh/relatedWorkSubmittedSeminars", $"<{resourceAPI.GraphsUrl}items/relatedworksubmittedseminarscv_{ResourceID}_{item22.Roh_relatedWorkSubmittedSeminars.ArticleID}>", list, " . ");
				if(item22.Roh_relatedWorkSubmittedSeminars.IdRoh_inscriptionType != null)
				{
					Regex regex = new Regex(@"\/items\/.+_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}");
					string itemRegex = item22.Roh_relatedWorkSubmittedSeminars.IdRoh_inscriptionType;
					if (regex.IsMatch(itemRegex))
					{
						itemRegex = $"http://gnoss/{resourceAPI.GetShortGuid(itemRegex).ToString().ToUpper()}";
					}
					else
					{
						itemRegex = itemRegex.ToLower();
					}
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/relatedworksubmittedseminarscv_{ResourceID}_{item22.Roh_relatedWorkSubmittedSeminars.ArticleID}",  "http://w3id.org/roh/inscriptionType", $"<{itemRegex}>", list, " . ");
				}
				if(item22.Roh_relatedWorkSubmittedSeminars.Roh_correspondingAuthor != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/relatedworksubmittedseminarscv_{ResourceID}_{item22.Roh_relatedWorkSubmittedSeminars.ArticleID}",  "http://w3id.org/roh/correspondingAuthor", $"\"{item22.Roh_relatedWorkSubmittedSeminars.Roh_correspondingAuthor.ToString().ToLower()}\"", list, " . ");
				}
				if(item22.Roh_relatedWorkSubmittedSeminars.Roh_inscriptionTypeOther != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/relatedworksubmittedseminarscv_{ResourceID}_{item22.Roh_relatedWorkSubmittedSeminars.ArticleID}",  "http://w3id.org/roh/inscriptionTypeOther", $"\"{GenerarTextoSinSaltoDeLinea(item22.Roh_relatedWorkSubmittedSeminars.Roh_inscriptionTypeOther).ToLower()}\"", list, " . ");
				}
			}
				if(item22.IdVivo_relatedBy != null)
				{
					Regex regex = new Regex(@"\/items\/.+_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}");
					string itemRegex = item22.IdVivo_relatedBy;
					if (regex.IsMatch(itemRegex))
					{
						itemRegex = $"http://gnoss/{resourceAPI.GetShortGuid(itemRegex).ToString().ToUpper()}";
					}
					else
					{
						itemRegex = itemRegex.ToLower();
					}
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/relatedworksubmittedseminars_{ResourceID}_{item22.ArticleID}",  "http://vivoweb.org/ontology/core#relatedBy", $"<{itemRegex}>", list, " . ");
				}
				if(item22.Roh_isPublic != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/relatedworksubmittedseminars_{ResourceID}_{item22.ArticleID}",  "http://w3id.org/roh/isPublic", $"\"{item22.Roh_isPublic.ToString().ToLower()}\"", list, " . ");
				}
			}
			}
				if(this.Roh_scientificActivity.Roh_title != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/scientificactivity_{ResourceID}_{this.Roh_scientificActivity.ArticleID}",  "http://w3id.org/roh/title", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_scientificActivity.Roh_title).ToLower()}\"", list, " . ");
				}
			}
			if(this.Roh_personalData != null)
			{
				AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}", "http://w3id.org/roh/personalData", $"<{resourceAPI.GraphsUrl}items/personaldata_{ResourceID}_{this.Roh_personalData.ArticleID}>", list, " . ");
			if(this.Roh_personalData.Roh_hasFax != null)
			{
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/personaldata_{ResourceID}_{this.Roh_personalData.ArticleID}", "http://w3id.org/roh/hasFax", $"<{resourceAPI.GraphsUrl}items/telephonetype_{ResourceID}_{this.Roh_personalData.Roh_hasFax.ArticleID}>", list, " . ");
				if(this.Roh_personalData.Roh_hasFax.Roh_hasExtension != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/telephonetype_{ResourceID}_{this.Roh_personalData.Roh_hasFax.ArticleID}",  "http://w3id.org/roh/hasExtension", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_personalData.Roh_hasFax.Roh_hasExtension).ToLower()}\"", list, " . ");
				}
				if(this.Roh_personalData.Roh_hasFax.Roh_hasInternationalCode != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/telephonetype_{ResourceID}_{this.Roh_personalData.Roh_hasFax.ArticleID}",  "http://w3id.org/roh/hasInternationalCode", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_personalData.Roh_hasFax.Roh_hasInternationalCode).ToLower()}\"", list, " . ");
				}
				if(this.Roh_personalData.Roh_hasFax.Vcard_hasValue != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/telephonetype_{ResourceID}_{this.Roh_personalData.Roh_hasFax.ArticleID}",  "https://www.w3.org/2006/vcard/ns#hasValue", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_personalData.Roh_hasFax.Vcard_hasValue).ToLower()}\"", list, " . ");
				}
			}
			if(this.Roh_personalData.Vcard_hasTelephone != null)
			{
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/personaldata_{ResourceID}_{this.Roh_personalData.ArticleID}", "https://www.w3.org/2006/vcard/ns#hasTelephone", $"<{resourceAPI.GraphsUrl}items/telephonetype_{ResourceID}_{this.Roh_personalData.Vcard_hasTelephone.ArticleID}>", list, " . ");
				if(this.Roh_personalData.Vcard_hasTelephone.Roh_hasExtension != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/telephonetype_{ResourceID}_{this.Roh_personalData.Vcard_hasTelephone.ArticleID}",  "http://w3id.org/roh/hasExtension", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_personalData.Vcard_hasTelephone.Roh_hasExtension).ToLower()}\"", list, " . ");
				}
				if(this.Roh_personalData.Vcard_hasTelephone.Roh_hasInternationalCode != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/telephonetype_{ResourceID}_{this.Roh_personalData.Vcard_hasTelephone.ArticleID}",  "http://w3id.org/roh/hasInternationalCode", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_personalData.Vcard_hasTelephone.Roh_hasInternationalCode).ToLower()}\"", list, " . ");
				}
				if(this.Roh_personalData.Vcard_hasTelephone.Vcard_hasValue != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/telephonetype_{ResourceID}_{this.Roh_personalData.Vcard_hasTelephone.ArticleID}",  "https://www.w3.org/2006/vcard/ns#hasValue", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_personalData.Vcard_hasTelephone.Vcard_hasValue).ToLower()}\"", list, " . ");
				}
			}
			if(this.Roh_personalData.Roh_hasMobilePhone != null)
			{
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/personaldata_{ResourceID}_{this.Roh_personalData.ArticleID}", "http://w3id.org/roh/hasMobilePhone", $"<{resourceAPI.GraphsUrl}items/telephonetype_{ResourceID}_{this.Roh_personalData.Roh_hasMobilePhone.ArticleID}>", list, " . ");
				if(this.Roh_personalData.Roh_hasMobilePhone.Roh_hasExtension != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/telephonetype_{ResourceID}_{this.Roh_personalData.Roh_hasMobilePhone.ArticleID}",  "http://w3id.org/roh/hasExtension", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_personalData.Roh_hasMobilePhone.Roh_hasExtension).ToLower()}\"", list, " . ");
				}
				if(this.Roh_personalData.Roh_hasMobilePhone.Roh_hasInternationalCode != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/telephonetype_{ResourceID}_{this.Roh_personalData.Roh_hasMobilePhone.ArticleID}",  "http://w3id.org/roh/hasInternationalCode", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_personalData.Roh_hasMobilePhone.Roh_hasInternationalCode).ToLower()}\"", list, " . ");
				}
				if(this.Roh_personalData.Roh_hasMobilePhone.Vcard_hasValue != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/telephonetype_{ResourceID}_{this.Roh_personalData.Roh_hasMobilePhone.ArticleID}",  "https://www.w3.org/2006/vcard/ns#hasValue", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_personalData.Roh_hasMobilePhone.Vcard_hasValue).ToLower()}\"", list, " . ");
				}
			}
			if(this.Roh_personalData.Roh_birthplace != null)
			{
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/personaldata_{ResourceID}_{this.Roh_personalData.ArticleID}", "http://w3id.org/roh/birthplace", $"<{resourceAPI.GraphsUrl}items/address_{ResourceID}_{this.Roh_personalData.Roh_birthplace.ArticleID}>", list, " . ");
				if(this.Roh_personalData.Roh_birthplace.IdVcard_hasCountryName != null)
				{
					Regex regex = new Regex(@"\/items\/.+_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}");
					string itemRegex = this.Roh_personalData.Roh_birthplace.IdVcard_hasCountryName;
					if (regex.IsMatch(itemRegex))
					{
						itemRegex = $"http://gnoss/{resourceAPI.GetShortGuid(itemRegex).ToString().ToUpper()}";
					}
					else
					{
						itemRegex = itemRegex.ToLower();
					}
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/address_{ResourceID}_{this.Roh_personalData.Roh_birthplace.ArticleID}",  "https://www.w3.org/2006/vcard/ns#hasCountryName", $"<{itemRegex}>", list, " . ");
				}
				if(this.Roh_personalData.Roh_birthplace.IdRoh_hasProvince != null)
				{
					Regex regex = new Regex(@"\/items\/.+_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}");
					string itemRegex = this.Roh_personalData.Roh_birthplace.IdRoh_hasProvince;
					if (regex.IsMatch(itemRegex))
					{
						itemRegex = $"http://gnoss/{resourceAPI.GetShortGuid(itemRegex).ToString().ToUpper()}";
					}
					else
					{
						itemRegex = itemRegex.ToLower();
					}
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/address_{ResourceID}_{this.Roh_personalData.Roh_birthplace.ArticleID}",  "http://w3id.org/roh/hasProvince", $"<{itemRegex}>", list, " . ");
				}
				if(this.Roh_personalData.Roh_birthplace.IdVcard_hasRegion != null)
				{
					Regex regex = new Regex(@"\/items\/.+_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}");
					string itemRegex = this.Roh_personalData.Roh_birthplace.IdVcard_hasRegion;
					if (regex.IsMatch(itemRegex))
					{
						itemRegex = $"http://gnoss/{resourceAPI.GetShortGuid(itemRegex).ToString().ToUpper()}";
					}
					else
					{
						itemRegex = itemRegex.ToLower();
					}
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/address_{ResourceID}_{this.Roh_personalData.Roh_birthplace.ArticleID}",  "https://www.w3.org/2006/vcard/ns#hasRegion", $"<{itemRegex}>", list, " . ");
				}
				if(this.Roh_personalData.Roh_birthplace.Vcard_postal_code != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/address_{ResourceID}_{this.Roh_personalData.Roh_birthplace.ArticleID}",  "https://www.w3.org/2006/vcard/ns#postal-code", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_personalData.Roh_birthplace.Vcard_postal_code).ToLower()}\"", list, " . ");
				}
				if(this.Roh_personalData.Roh_birthplace.Vcard_extended_address != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/address_{ResourceID}_{this.Roh_personalData.Roh_birthplace.ArticleID}",  "https://www.w3.org/2006/vcard/ns#extended-address", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_personalData.Roh_birthplace.Vcard_extended_address).ToLower()}\"", list, " . ");
				}
				if(this.Roh_personalData.Roh_birthplace.Vcard_street_address != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/address_{ResourceID}_{this.Roh_personalData.Roh_birthplace.ArticleID}",  "https://www.w3.org/2006/vcard/ns#street-address", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_personalData.Roh_birthplace.Vcard_street_address).ToLower()}\"", list, " . ");
				}
				if(this.Roh_personalData.Roh_birthplace.Vcard_locality != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/address_{ResourceID}_{this.Roh_personalData.Roh_birthplace.ArticleID}",  "https://www.w3.org/2006/vcard/ns#locality", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_personalData.Roh_birthplace.Vcard_locality).ToLower()}\"", list, " . ");
				}
			}
			if(this.Roh_personalData.Vcard_address != null)
			{
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/personaldata_{ResourceID}_{this.Roh_personalData.ArticleID}", "https://www.w3.org/2006/vcard/ns#address", $"<{resourceAPI.GraphsUrl}items/address_{ResourceID}_{this.Roh_personalData.Vcard_address.ArticleID}>", list, " . ");
				if(this.Roh_personalData.Vcard_address.IdVcard_hasCountryName != null)
				{
					Regex regex = new Regex(@"\/items\/.+_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}");
					string itemRegex = this.Roh_personalData.Vcard_address.IdVcard_hasCountryName;
					if (regex.IsMatch(itemRegex))
					{
						itemRegex = $"http://gnoss/{resourceAPI.GetShortGuid(itemRegex).ToString().ToUpper()}";
					}
					else
					{
						itemRegex = itemRegex.ToLower();
					}
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/address_{ResourceID}_{this.Roh_personalData.Vcard_address.ArticleID}",  "https://www.w3.org/2006/vcard/ns#hasCountryName", $"<{itemRegex}>", list, " . ");
				}
				if(this.Roh_personalData.Vcard_address.IdRoh_hasProvince != null)
				{
					Regex regex = new Regex(@"\/items\/.+_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}");
					string itemRegex = this.Roh_personalData.Vcard_address.IdRoh_hasProvince;
					if (regex.IsMatch(itemRegex))
					{
						itemRegex = $"http://gnoss/{resourceAPI.GetShortGuid(itemRegex).ToString().ToUpper()}";
					}
					else
					{
						itemRegex = itemRegex.ToLower();
					}
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/address_{ResourceID}_{this.Roh_personalData.Vcard_address.ArticleID}",  "http://w3id.org/roh/hasProvince", $"<{itemRegex}>", list, " . ");
				}
				if(this.Roh_personalData.Vcard_address.IdVcard_hasRegion != null)
				{
					Regex regex = new Regex(@"\/items\/.+_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}");
					string itemRegex = this.Roh_personalData.Vcard_address.IdVcard_hasRegion;
					if (regex.IsMatch(itemRegex))
					{
						itemRegex = $"http://gnoss/{resourceAPI.GetShortGuid(itemRegex).ToString().ToUpper()}";
					}
					else
					{
						itemRegex = itemRegex.ToLower();
					}
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/address_{ResourceID}_{this.Roh_personalData.Vcard_address.ArticleID}",  "https://www.w3.org/2006/vcard/ns#hasRegion", $"<{itemRegex}>", list, " . ");
				}
				if(this.Roh_personalData.Vcard_address.Vcard_postal_code != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/address_{ResourceID}_{this.Roh_personalData.Vcard_address.ArticleID}",  "https://www.w3.org/2006/vcard/ns#postal-code", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_personalData.Vcard_address.Vcard_postal_code).ToLower()}\"", list, " . ");
				}
				if(this.Roh_personalData.Vcard_address.Vcard_extended_address != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/address_{ResourceID}_{this.Roh_personalData.Vcard_address.ArticleID}",  "https://www.w3.org/2006/vcard/ns#extended-address", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_personalData.Vcard_address.Vcard_extended_address).ToLower()}\"", list, " . ");
				}
				if(this.Roh_personalData.Vcard_address.Vcard_street_address != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/address_{ResourceID}_{this.Roh_personalData.Vcard_address.ArticleID}",  "https://www.w3.org/2006/vcard/ns#street-address", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_personalData.Vcard_address.Vcard_street_address).ToLower()}\"", list, " . ");
				}
				if(this.Roh_personalData.Vcard_address.Vcard_locality != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/address_{ResourceID}_{this.Roh_personalData.Vcard_address.ArticleID}",  "https://www.w3.org/2006/vcard/ns#locality", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_personalData.Vcard_address.Vcard_locality).ToLower()}\"", list, " . ");
				}
			}
			if(this.Roh_personalData.Roh_otherIds != null)
			{
			foreach(var item6 in this.Roh_personalData.Roh_otherIds)
			{
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/personaldata_{ResourceID}_{this.Roh_personalData.ArticleID}", "http://w3id.org/roh/otherIds", $"<{resourceAPI.GraphsUrl}items/document_{ResourceID}_{item6.ArticleID}>", list, " . ");
				if(item6.Dc_title != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/document_{ResourceID}_{item6.ArticleID}",  "http://purl.org/dc/elements/1.1/title", $"\"{GenerarTextoSinSaltoDeLinea(item6.Dc_title).ToLower()}\"", list, " . ");
				}
				if(item6.Foaf_topic != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/document_{ResourceID}_{item6.ArticleID}",  "http://xmlns.com/foaf/0.1/topic", $"\"{GenerarTextoSinSaltoDeLinea(item6.Foaf_topic).ToLower()}\"", list, " . ");
				}
			}
			}
				if(this.Roh_personalData.IdFoaf_gender != null)
				{
					Regex regex = new Regex(@"\/items\/.+_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}");
					string itemRegex = this.Roh_personalData.IdFoaf_gender;
					if (regex.IsMatch(itemRegex))
					{
						itemRegex = $"http://gnoss/{resourceAPI.GetShortGuid(itemRegex).ToString().ToUpper()}";
					}
					else
					{
						itemRegex = itemRegex.ToLower();
					}
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/personaldata_{ResourceID}_{this.Roh_personalData.ArticleID}",  "http://xmlns.com/foaf/0.1/gender", $"<{itemRegex}>", list, " . ");
				}
				if(this.Roh_personalData.IdSchema_nationality != null)
				{
					Regex regex = new Regex(@"\/items\/.+_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}");
					string itemRegex = this.Roh_personalData.IdSchema_nationality;
					if (regex.IsMatch(itemRegex))
					{
						itemRegex = $"http://gnoss/{resourceAPI.GetShortGuid(itemRegex).ToString().ToUpper()}";
					}
					else
					{
						itemRegex = itemRegex.ToLower();
					}
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/personaldata_{ResourceID}_{this.Roh_personalData.ArticleID}",  "http://www.schema.org/nationality", $"<{itemRegex}>", list, " . ");
				}
				if(this.Roh_personalData.Roh_nie != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/personaldata_{ResourceID}_{this.Roh_personalData.ArticleID}",  "http://w3id.org/roh/nie", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_personalData.Roh_nie).ToLower()}\"", list, " . ");
				}
				if(this.Roh_personalData.Vivo_researcherId != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/personaldata_{ResourceID}_{this.Roh_personalData.ArticleID}",  "http://vivoweb.org/ontology/core#researcherId", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_personalData.Vivo_researcherId).ToLower()}\"", list, " . ");
				}
				if(this.Roh_personalData.Vivo_scopusId != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/personaldata_{ResourceID}_{this.Roh_personalData.ArticleID}",  "http://vivoweb.org/ontology/core#scopusId", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_personalData.Vivo_scopusId).ToLower()}\"", list, " . ");
				}
				if(this.Roh_personalData.Foaf_familyName != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/personaldata_{ResourceID}_{this.Roh_personalData.ArticleID}",  "http://xmlns.com/foaf/0.1/familyName", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_personalData.Foaf_familyName).ToLower()}\"", list, " . ");
				}
				if(this.Roh_personalData.Roh_secondFamilyName != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/personaldata_{ResourceID}_{this.Roh_personalData.ArticleID}",  "http://w3id.org/roh/secondFamilyName", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_personalData.Roh_secondFamilyName).ToLower()}\"", list, " . ");
				}
				if(this.Roh_personalData.Vcard_email != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/personaldata_{ResourceID}_{this.Roh_personalData.ArticleID}",  "https://www.w3.org/2006/vcard/ns#email", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_personalData.Vcard_email).ToLower()}\"", list, " . ");
				}
				if(this.Roh_personalData.Foaf_img != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/personaldata_{ResourceID}_{this.Roh_personalData.ArticleID}",  "http://xmlns.com/foaf/0.1/img", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_personalData.Foaf_img).ToLower()}\"", list, " . ");
				}
				if(this.Roh_personalData.Roh_dni != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/personaldata_{ResourceID}_{this.Roh_personalData.ArticleID}",  "http://w3id.org/roh/dni", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_personalData.Roh_dni).ToLower()}\"", list, " . ");
				}
				if(this.Roh_personalData.Foaf_homepage != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/personaldata_{ResourceID}_{this.Roh_personalData.ArticleID}",  "http://xmlns.com/foaf/0.1/homepage", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_personalData.Foaf_homepage).ToLower()}\"", list, " . ");
				}
				if(this.Roh_personalData.Roh_ORCID != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/personaldata_{ResourceID}_{this.Roh_personalData.ArticleID}",  "http://w3id.org/roh/ORCID", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_personalData.Roh_ORCID).ToLower()}\"", list, " . ");
				}
				if(this.Roh_personalData.Roh_passport != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/personaldata_{ResourceID}_{this.Roh_personalData.ArticleID}",  "http://w3id.org/roh/passport", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_personalData.Roh_passport).ToLower()}\"", list, " . ");
				}
				if(this.Roh_personalData.Vcard_birth_date != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/personaldata_{ResourceID}_{this.Roh_personalData.ArticleID}",  "https://www.w3.org/2006/vcard/ns#birth-date", $"{this.Roh_personalData.Vcard_birth_date.Value.ToString("yyyyMMddHHmmss")}", list, " . ");
				}
				if(this.Roh_personalData.Foaf_firstName != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/personaldata_{ResourceID}_{this.Roh_personalData.ArticleID}",  "http://xmlns.com/foaf/0.1/firstName", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_personalData.Foaf_firstName).ToLower()}\"", list, " . ");
				}
			}
				if(this.IdRoh_gnossUser != null)
				{
					Regex regex = new Regex(@"\/items\/.+_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}");
					string itemRegex = this.IdRoh_gnossUser;
					if (regex.IsMatch(itemRegex))
					{
						itemRegex = $"http://gnoss/{resourceAPI.GetShortGuid(itemRegex).ToString().ToUpper()}";
					}
					else
					{
						itemRegex = itemRegex.ToLower();
					}
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "http://w3id.org/roh/gnossUser", $"<{itemRegex}>", list, " . ");
				}
				if(this.IdRoh_cvOf != null)
				{
					Regex regex = new Regex(@"\/items\/.+_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}");
					string itemRegex = this.IdRoh_cvOf;
					if (regex.IsMatch(itemRegex))
					{
						itemRegex = $"http://gnoss/{resourceAPI.GetShortGuid(itemRegex).ToString().ToUpper()}";
					}
					else
					{
						itemRegex = itemRegex.ToLower();
					}
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "http://w3id.org/roh/cvOf", $"<{itemRegex}>", list, " . ");
				}
				if(this.Foaf_name != null)
				{
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "http://xmlns.com/foaf/0.1/name", $"\"{GenerarTextoSinSaltoDeLinea(this.Foaf_name).ToLower()}\"", list, " . ");
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
			string titulo = $"{this.Foaf_name.Replace("\r\n", "").Replace("\n", "").Replace("\r", "").Replace("\"", "\"\"").Replace("'", "''").Replace("|", "#PIPE#")}";
			string descripcion = $"{this.Foaf_name.Replace("\r\n", "").Replace("\n", "").Replace("\r", "").Replace("\"", "\"\"").Replace("'", "''").Replace("|", "#PIPE#")}";
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
			return $"{resourceAPI.GraphsUrl}items/CurriculumvitaeOntology_{ResourceID}_{ArticleID}";
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
