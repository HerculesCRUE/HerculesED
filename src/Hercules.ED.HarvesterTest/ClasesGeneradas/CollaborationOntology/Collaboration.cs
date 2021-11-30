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
using Feature = FeatureOntology.Feature;
using Organization = OrganizationOntology.Organization;
using RelationshipType = RelationshiptypeOntology.RelationshipType;

namespace CollaborationOntology
{
	public class Collaboration : GnossOCBase
	{

		public Collaboration() : base() { }

		public Collaboration(SemanticResourceModel pSemCmsModel, LanguageEnum idiomaUsuario) : base()
		{
			this.mGNOSSID = pSemCmsModel.RootEntities[0].Entity.Uri;
			SemanticPropertyModel propVcard_hasRegion = pSemCmsModel.GetPropertyByPath("https://www.w3.org/2006/vcard/ns#hasRegion");
			if (propVcard_hasRegion != null && propVcard_hasRegion.PropertyValues.Count > 0)
			{
				this.Vcard_hasRegion = new Feature(propVcard_hasRegion.PropertyValues[0].RelatedEntity, idiomaUsuario);
			}
			this.Vivo_freeTextKeywords = new List<CategoryPath>();
			SemanticPropertyModel propVivo_freeTextKeywords = pSemCmsModel.GetPropertyByPath("http://vivoweb.org/ontology/core#freeTextKeywords");
			if (propVivo_freeTextKeywords != null && propVivo_freeTextKeywords.PropertyValues.Count > 0)
			{
				foreach (SemanticPropertyModel.PropertyValue propValue in propVivo_freeTextKeywords.PropertyValues)
				{
					if (propValue.RelatedEntity != null)
					{
						CategoryPath vivo_freeTextKeywords = new CategoryPath(propValue.RelatedEntity, idiomaUsuario);
						this.Vivo_freeTextKeywords.Add(vivo_freeTextKeywords);
					}
				}
			}
			this.Roh_researchers = new List<BFO_0000023>();
			SemanticPropertyModel propRoh_researchers = pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/researchers");
			if (propRoh_researchers != null && propRoh_researchers.PropertyValues.Count > 0)
			{
				foreach (SemanticPropertyModel.PropertyValue propValue in propRoh_researchers.PropertyValues)
				{
					if (propValue.RelatedEntity != null)
					{
						BFO_0000023 roh_researchers = new BFO_0000023(propValue.RelatedEntity, idiomaUsuario);
						this.Roh_researchers.Add(roh_researchers);
					}
				}
			}
			this.Roh_participates = new List<Organization>();
			SemanticPropertyModel propRoh_participates = pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/participates");
			if (propRoh_participates != null && propRoh_participates.PropertyValues.Count > 0)
			{
				foreach (SemanticPropertyModel.PropertyValue propValue in propRoh_participates.PropertyValues)
				{
					if (propValue.RelatedEntity != null)
					{
						Organization roh_participates = new Organization(propValue.RelatedEntity, idiomaUsuario);
						this.Roh_participates.Add(roh_participates);
					}
				}
			}
			SemanticPropertyModel propVcard_hasCountryName = pSemCmsModel.GetPropertyByPath("https://www.w3.org/2006/vcard/ns#hasCountryName");
			if (propVcard_hasCountryName != null && propVcard_hasCountryName.PropertyValues.Count > 0)
			{
				this.Vcard_hasCountryName = new Feature(propVcard_hasCountryName.PropertyValues[0].RelatedEntity, idiomaUsuario);
			}
			SemanticPropertyModel propRoh_relationshipType = pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/relationshipType");
			if (propRoh_relationshipType != null && propRoh_relationshipType.PropertyValues.Count > 0)
			{
				this.Roh_relationshipType = new RelationshipType(propRoh_relationshipType.PropertyValues[0].RelatedEntity, idiomaUsuario);
			}
			this.Roh_relevantResults = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/relevantResults"));
			this.Vcard_locality = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("https://www.w3.org/2006/vcard/ns#locality"));
			this.Roh_crisIdentifier = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/crisIdentifier"));
			this.Roh_relationshipTypeOther = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/relationshipTypeOther"));
			this.Vivo_start = GetDateValuePropertySemCms(pSemCmsModel.GetPropertyByPath("http://vivoweb.org/ontology/core#start"));
			this.Roh_title = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/title"));
		}

		public Collaboration(SemanticEntityModel pSemCmsModel, LanguageEnum idiomaUsuario) : base()
		{
			this.mGNOSSID = pSemCmsModel.Entity.Uri;
			this.mURL = pSemCmsModel.Properties.FirstOrDefault(p => p.PropertyValues.Any(prop => prop.DownloadUrl != null))?.FirstPropertyValue.DownloadUrl;
			SemanticPropertyModel propVcard_hasRegion = pSemCmsModel.GetPropertyByPath("https://www.w3.org/2006/vcard/ns#hasRegion");
			if (propVcard_hasRegion != null && propVcard_hasRegion.PropertyValues.Count > 0)
			{
				this.Vcard_hasRegion = new Feature(propVcard_hasRegion.PropertyValues[0].RelatedEntity, idiomaUsuario);
			}
			this.Vivo_freeTextKeywords = new List<CategoryPath>();
			SemanticPropertyModel propVivo_freeTextKeywords = pSemCmsModel.GetPropertyByPath("http://vivoweb.org/ontology/core#freeTextKeywords");
			if (propVivo_freeTextKeywords != null && propVivo_freeTextKeywords.PropertyValues.Count > 0)
			{
				foreach (SemanticPropertyModel.PropertyValue propValue in propVivo_freeTextKeywords.PropertyValues)
				{
					if (propValue.RelatedEntity != null)
					{
						CategoryPath vivo_freeTextKeywords = new CategoryPath(propValue.RelatedEntity, idiomaUsuario);
						this.Vivo_freeTextKeywords.Add(vivo_freeTextKeywords);
					}
				}
			}
			this.Roh_researchers = new List<BFO_0000023>();
			SemanticPropertyModel propRoh_researchers = pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/researchers");
			if (propRoh_researchers != null && propRoh_researchers.PropertyValues.Count > 0)
			{
				foreach (SemanticPropertyModel.PropertyValue propValue in propRoh_researchers.PropertyValues)
				{
					if (propValue.RelatedEntity != null)
					{
						BFO_0000023 roh_researchers = new BFO_0000023(propValue.RelatedEntity, idiomaUsuario);
						this.Roh_researchers.Add(roh_researchers);
					}
				}
			}
			this.Roh_participates = new List<Organization>();
			SemanticPropertyModel propRoh_participates = pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/participates");
			if (propRoh_participates != null && propRoh_participates.PropertyValues.Count > 0)
			{
				foreach (SemanticPropertyModel.PropertyValue propValue in propRoh_participates.PropertyValues)
				{
					if (propValue.RelatedEntity != null)
					{
						Organization roh_participates = new Organization(propValue.RelatedEntity, idiomaUsuario);
						this.Roh_participates.Add(roh_participates);
					}
				}
			}
			SemanticPropertyModel propVcard_hasCountryName = pSemCmsModel.GetPropertyByPath("https://www.w3.org/2006/vcard/ns#hasCountryName");
			if (propVcard_hasCountryName != null && propVcard_hasCountryName.PropertyValues.Count > 0)
			{
				this.Vcard_hasCountryName = new Feature(propVcard_hasCountryName.PropertyValues[0].RelatedEntity, idiomaUsuario);
			}
			SemanticPropertyModel propRoh_relationshipType = pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/relationshipType");
			if (propRoh_relationshipType != null && propRoh_relationshipType.PropertyValues.Count > 0)
			{
				this.Roh_relationshipType = new RelationshipType(propRoh_relationshipType.PropertyValues[0].RelatedEntity, idiomaUsuario);
			}
			this.Roh_relevantResults = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/relevantResults"));
			this.Vcard_locality = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("https://www.w3.org/2006/vcard/ns#locality"));
			this.Roh_crisIdentifier = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/crisIdentifier"));
			this.Roh_relationshipTypeOther = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/relationshipTypeOther"));
			this.Vivo_start = GetDateValuePropertySemCms(pSemCmsModel.GetPropertyByPath("http://vivoweb.org/ontology/core#start"));
			this.Roh_title = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/title"));
		}

		public virtual string RdfType { get { return "http://w3id.org/roh/Collaboration"; } }
		public virtual string RdfsLabel { get { return "http://w3id.org/roh/Collaboration"; } }
		[LABEL(LanguageEnum.es, "hasRegion")]
		[RDFProperty("https://www.w3.org/2006/vcard/ns#hasRegion")]
		public Feature Vcard_hasRegion { get; set; }
		public string IdVcard_hasRegion { get; set; }

		[LABEL(LanguageEnum.es, "freeTextKeywords")]
		[RDFProperty("http://vivoweb.org/ontology/core#freeTextKeywords")]
		public List<CategoryPath> Vivo_freeTextKeywords { get; set; }

		[LABEL(LanguageEnum.es, "researchers")]
		[RDFProperty("http://w3id.org/roh/researchers")]
		public List<BFO_0000023> Roh_researchers { get; set; }

		[LABEL(LanguageEnum.es, "participates")]
		[RDFProperty("http://w3id.org/roh/participates")]
		public List<Organization> Roh_participates { get; set; }
		public List<string> IdsRoh_participates { get; set; }

		[LABEL(LanguageEnum.es, "hasCountryName")]
		[RDFProperty("https://www.w3.org/2006/vcard/ns#hasCountryName")]
		public Feature Vcard_hasCountryName { get; set; }
		public string IdVcard_hasCountryName { get; set; }

		[LABEL(LanguageEnum.es, "relationshipType")]
		[RDFProperty("http://w3id.org/roh/relationshipType")]
		public RelationshipType Roh_relationshipType { get; set; }
		public string IdRoh_relationshipType { get; set; }

		[LABEL(LanguageEnum.es, "relevantResults")]
		[RDFProperty("http://w3id.org/roh/relevantResults")]
		public string Roh_relevantResults { get; set; }

		[LABEL(LanguageEnum.es, "locality")]
		[RDFProperty("https://www.w3.org/2006/vcard/ns#locality")]
		public string Vcard_locality { get; set; }

		[LABEL(LanguageEnum.es, "crisIdentifier")]
		[RDFProperty("http://w3id.org/roh/crisIdentifier")]
		public string Roh_crisIdentifier { get; set; }

		[LABEL(LanguageEnum.es, "relationshipTypeOther")]
		[RDFProperty("http://w3id.org/roh/relationshipTypeOther")]
		public string Roh_relationshipTypeOther { get; set; }

		[LABEL(LanguageEnum.es, "start")]
		[RDFProperty("http://vivoweb.org/ontology/core#start")]
		public DateTime? Vivo_start { get; set; }

		[LABEL(LanguageEnum.es, "title")]
		[RDFProperty("http://w3id.org/roh/title")]
		public string Roh_title { get; set; }


		internal override void GetProperties()
		{
			base.GetProperties();
			propList.Add(new StringOntologyProperty("vcard:hasRegion", this.IdVcard_hasRegion));
			propList.Add(new ListStringOntologyProperty("roh:participates", this.IdsRoh_participates));
			propList.Add(new StringOntologyProperty("vcard:hasCountryName", this.IdVcard_hasCountryName));
			propList.Add(new StringOntologyProperty("roh:relationshipType", this.IdRoh_relationshipType));
			propList.Add(new StringOntologyProperty("roh:relevantResults", this.Roh_relevantResults));
			propList.Add(new StringOntologyProperty("vcard:locality", this.Vcard_locality));
			propList.Add(new StringOntologyProperty("roh:crisIdentifier", this.Roh_crisIdentifier));
			propList.Add(new StringOntologyProperty("roh:relationshipTypeOther", this.Roh_relationshipTypeOther));
			if (this.Vivo_start.HasValue)
			{
				propList.Add(new DateOntologyProperty("vivo:start", this.Vivo_start.Value));
			}
			propList.Add(new StringOntologyProperty("roh:title", this.Roh_title));
		}

		internal override void GetEntities()
		{
			base.GetEntities();
			if (Vivo_freeTextKeywords != null)
			{
				foreach (CategoryPath prop in Vivo_freeTextKeywords)
				{
					prop.GetProperties();
					prop.GetEntities();
					OntologyEntity entityCategoryPath = new OntologyEntity("http://w3id.org/roh/CategoryPath", "http://w3id.org/roh/CategoryPath", "vivo:freeTextKeywords", prop.propList, prop.entList);
					entList.Add(entityCategoryPath);
					prop.Entity = entityCategoryPath;
				}
			}
			if (Roh_researchers != null)
			{
				foreach (BFO_0000023 prop in Roh_researchers)
				{
					prop.GetProperties();
					prop.GetEntities();
					OntologyEntity entityBFO_0000023 = new OntologyEntity("http://purl.obolibrary.org/obo/BFO_0000023", "http://purl.obolibrary.org/obo/BFO_0000023", "roh:researchers", prop.propList, prop.entList);
					entList.Add(entityBFO_0000023);
					prop.Entity = entityBFO_0000023;
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
			Ontology ontology = null;
			GetEntities();
			GetProperties();
			if (idrecurso.Equals(Guid.Empty) && idarticulo.Equals(Guid.Empty))
			{
				ontology = new Ontology(resourceAPI.GraphsUrl, resourceAPI.OntologyUrl, RdfType, RdfsLabel, prefList, propList, entList);
			}
			else
			{
				ontology = new Ontology(resourceAPI.GraphsUrl, resourceAPI.OntologyUrl, RdfType, RdfsLabel, prefList, propList, entList, idrecurso, idarticulo);
			}
			resource.Id = GNOSSID;
			resource.Ontology = ontology;
			resource.TextCategories = listaDeCategorias;
			AddResourceTitle(resource);
			AddResourceDescription(resource);
			AddImages(resource);
			AddFiles(resource);
			return resource;
		}

		public override List<string> ToOntologyGnossTriples(ResourceApi resourceAPI)
		{
			List<string> list = new List<string>();
			AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Collaboration_{ResourceID}_{ArticleID}", "http://www.w3.org/1999/02/22-rdf-syntax-ns#type", $"<http://w3id.org/roh/Collaboration>", list, " . ");
			AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Collaboration_{ResourceID}_{ArticleID}", "http://www.w3.org/2000/01/rdf-schema#label", $"\"http://w3id.org/roh/Collaboration\"", list, " . ");
			AgregarTripleALista($"{resourceAPI.GraphsUrl}{ResourceID}", "http://gnoss/hasEntidad", $"<{resourceAPI.GraphsUrl}items/Collaboration_{ResourceID}_{ArticleID}>", list, " . ");
			if (this.Vivo_freeTextKeywords != null)
			{
				foreach (var item0 in this.Vivo_freeTextKeywords)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/CategoryPath_{ResourceID}_{item0.ArticleID}", "http://www.w3.org/1999/02/22-rdf-syntax-ns#type", $"<http://w3id.org/roh/CategoryPath>", list, " . ");
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/CategoryPath_{ResourceID}_{item0.ArticleID}", "http://www.w3.org/2000/01/rdf-schema#label", $"\"http://w3id.org/roh/CategoryPath\"", list, " . ");
					AgregarTripleALista($"{resourceAPI.GraphsUrl}{ResourceID}", "http://gnoss/hasEntidad", $"<{resourceAPI.GraphsUrl}items/CategoryPath_{ResourceID}_{item0.ArticleID}>", list, " . ");
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Collaboration_{ResourceID}_{ArticleID}", "http://vivoweb.org/ontology/core#freeTextKeywords", $"<{resourceAPI.GraphsUrl}items/CategoryPath_{ResourceID}_{item0.ArticleID}>", list, " . ");
					if (item0.IdsRoh_categoryNode != null)
					{
						foreach (var item2 in item0.IdsRoh_categoryNode)
						{
							AgregarTripleALista($"{resourceAPI.GraphsUrl}items/CategoryPath_{ResourceID}_{item0.ArticleID}", "http://w3id.org/roh/categoryNode", $"<{item2}>", list, " . ");
						}
					}
				}
			}
			if (this.Roh_researchers != null)
			{
				foreach (var item0 in this.Roh_researchers)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/BFO_0000023_{ResourceID}_{item0.ArticleID}", "http://www.w3.org/1999/02/22-rdf-syntax-ns#type", $"<http://purl.obolibrary.org/obo/BFO_0000023>", list, " . ");
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/BFO_0000023_{ResourceID}_{item0.ArticleID}", "http://www.w3.org/2000/01/rdf-schema#label", $"\"http://purl.obolibrary.org/obo/BFO_0000023\"", list, " . ");
					AgregarTripleALista($"{resourceAPI.GraphsUrl}{ResourceID}", "http://gnoss/hasEntidad", $"<{resourceAPI.GraphsUrl}items/BFO_0000023_{ResourceID}_{item0.ArticleID}>", list, " . ");
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Collaboration_{ResourceID}_{ArticleID}", "http://w3id.org/roh/researchers", $"<{resourceAPI.GraphsUrl}items/BFO_0000023_{ResourceID}_{item0.ArticleID}>", list, " . ");
					if (item0.Foaf_nick != null)
					{
						AgregarTripleALista($"{resourceAPI.GraphsUrl}items/BFO_0000023_{ResourceID}_{item0.ArticleID}", "http://xmlns.com/foaf/0.1/nick", $"\"{GenerarTextoSinSaltoDeLinea(item0.Foaf_nick)}\"", list, " . ");
					}
					if (item0.IdRdf_member != null)
					{
						AgregarTripleALista($"{resourceAPI.GraphsUrl}items/BFO_0000023_{ResourceID}_{item0.ArticleID}", "http://www.w3.org/1999/02/22-rdf-syntax-ns#member", $"<{item0.IdRdf_member}>", list, " . ");
					}
					if (item0.Rdf_comment != null)
					{
						AgregarTripleALista($"{resourceAPI.GraphsUrl}items/BFO_0000023_{ResourceID}_{item0.ArticleID}", "http://www.w3.org/1999/02/22-rdf-syntax-ns#comment", $"{item0.Rdf_comment.ToString()}", list, " . ");
					}
				}
			}
			if (this.IdVcard_hasRegion != null)
			{
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Collaboration_{ResourceID}_{ArticleID}", "https://www.w3.org/2006/vcard/ns#hasRegion", $"<{this.IdVcard_hasRegion}>", list, " . ");
			}
			if (this.IdsRoh_participates != null)
			{
				foreach (var item2 in this.IdsRoh_participates)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Collaboration_{ResourceID}_{ArticleID}", "http://w3id.org/roh/participates", $"<{item2}>", list, " . ");
				}
			}
			if (this.IdVcard_hasCountryName != null)
			{
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Collaboration_{ResourceID}_{ArticleID}", "https://www.w3.org/2006/vcard/ns#hasCountryName", $"<{this.IdVcard_hasCountryName}>", list, " . ");
			}
			if (this.IdRoh_relationshipType != null)
			{
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Collaboration_{ResourceID}_{ArticleID}", "http://w3id.org/roh/relationshipType", $"<{this.IdRoh_relationshipType}>", list, " . ");
			}
			if (this.Roh_relevantResults != null)
			{
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Collaboration_{ResourceID}_{ArticleID}", "http://w3id.org/roh/relevantResults", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_relevantResults)}\"", list, " . ");
			}
			if (this.Vcard_locality != null)
			{
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Collaboration_{ResourceID}_{ArticleID}", "https://www.w3.org/2006/vcard/ns#locality", $"\"{GenerarTextoSinSaltoDeLinea(this.Vcard_locality)}\"", list, " . ");
			}
			if (this.Roh_crisIdentifier != null)
			{
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Collaboration_{ResourceID}_{ArticleID}", "http://w3id.org/roh/crisIdentifier", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_crisIdentifier)}\"", list, " . ");
			}
			if (this.Roh_relationshipTypeOther != null)
			{
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Collaboration_{ResourceID}_{ArticleID}", "http://w3id.org/roh/relationshipTypeOther", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_relationshipTypeOther)}\"", list, " . ");
			}
			if (this.Vivo_start != null)
			{
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Collaboration_{ResourceID}_{ArticleID}", "http://vivoweb.org/ontology/core#start", $"\"{this.Vivo_start.Value.ToString("yyyyMMddHHmmss")}\"", list, " . ");
			}
			if (this.Roh_title != null)
			{
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Collaboration_{ResourceID}_{ArticleID}", "http://w3id.org/roh/title", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_title)}\"", list, " . ");
			}
			return list;
		}

		public override List<string> ToSearchGraphTriples(ResourceApi resourceAPI)
		{
			List<string> list = new List<string>();
			List<string> listaSearch = new List<string>();
			AgregarTags(list);
			AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}", "http://www.w3.org/1999/02/22-rdf-syntax-ns#type", $"\"collaboration\"", list, " . ");
			AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}", "http://gnoss/type", $"\"http://w3id.org/roh/Collaboration\"", list, " . ");
			AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}", "http://gnoss/hasfechapublicacion", $"{DateTime.Now.ToString("yyyyMMddHHmmss")}", list, " . ");
			AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}", "http://gnoss/hastipodoc", "\"5\"", list, " . ");
			AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}", "http://gnoss/hasfechamodificacion", $"{DateTime.Now.ToString("yyyyMMddHHmmss")}", list, " . ");
			AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}", "http://gnoss/hasnumeroVisitas", "0", list, " . ");
			AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}", "http://gnoss/hasprivacidadCom", "\"publico\"", list, " . ");
			AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}", "http://xmlns.com/foaf/0.1/firstName", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_title)}\"", list, " . ");
			AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}", "http://gnoss/hasnombrecompleto", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_title)}\"", list, " . ");
			string search = string.Empty;
			if (this.Vivo_freeTextKeywords != null)
			{
				foreach (var item0 in this.Vivo_freeTextKeywords)
				{
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}", "http://vivoweb.org/ontology/core#freeTextKeywords", $"<{resourceAPI.GraphsUrl}items/categorypath_{ResourceID}_{item0.ArticleID}>", list, " . ");
					if (item0.IdsRoh_categoryNode != null)
					{
						foreach (var item2 in item0.IdsRoh_categoryNode)
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
			if (this.Roh_researchers != null)
			{
				foreach (var item0 in this.Roh_researchers)
				{
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}", "http://w3id.org/roh/researchers", $"<{resourceAPI.GraphsUrl}items/bfo_0000023_{ResourceID}_{item0.ArticleID}>", list, " . ");
					if (item0.Foaf_nick != null)
					{
						AgregarTripleALista($"{resourceAPI.GraphsUrl}items/bfo_0000023_{ResourceID}_{item0.ArticleID}", "http://xmlns.com/foaf/0.1/nick", $"\"{GenerarTextoSinSaltoDeLinea(item0.Foaf_nick).ToLower()}\"", list, " . ");
					}
					if (item0.IdRdf_member != null)
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
						AgregarTripleALista($"{resourceAPI.GraphsUrl}items/bfo_0000023_{ResourceID}_{item0.ArticleID}", "http://www.w3.org/1999/02/22-rdf-syntax-ns#member", $"<{itemRegex}>", list, " . ");
					}
					if (item0.Rdf_comment != null)
					{
						AgregarTripleALista($"{resourceAPI.GraphsUrl}items/bfo_0000023_{ResourceID}_{item0.ArticleID}", "http://www.w3.org/1999/02/22-rdf-syntax-ns#comment", $"{item0.Rdf_comment.ToString()}", list, " . ");
					}
				}
			}
			if (this.IdVcard_hasRegion != null)
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
				AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}", "https://www.w3.org/2006/vcard/ns#hasRegion", $"<{itemRegex}>", list, " . ");
			}
			if (this.IdsRoh_participates != null)
			{
				foreach (var item2 in this.IdsRoh_participates)
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
			if (this.IdVcard_hasCountryName != null)
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
				AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}", "https://www.w3.org/2006/vcard/ns#hasCountryName", $"<{itemRegex}>", list, " . ");
			}
			if (this.IdRoh_relationshipType != null)
			{
				Regex regex = new Regex(@"\/items\/.+_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}");
				string itemRegex = this.IdRoh_relationshipType;
				if (regex.IsMatch(itemRegex))
				{
					itemRegex = $"http://gnoss/{resourceAPI.GetShortGuid(itemRegex).ToString().ToUpper()}";
				}
				else
				{
					itemRegex = itemRegex.ToLower();
				}
				AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}", "http://w3id.org/roh/relationshipType", $"<{itemRegex}>", list, " . ");
			}
			if (this.Roh_relevantResults != null)
			{
				AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}", "http://w3id.org/roh/relevantResults", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_relevantResults).ToLower()}\"", list, " . ");
			}
			if (this.Vcard_locality != null)
			{
				AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}", "https://www.w3.org/2006/vcard/ns#locality", $"\"{GenerarTextoSinSaltoDeLinea(this.Vcard_locality).ToLower()}\"", list, " . ");
			}
			if (this.Roh_crisIdentifier != null)
			{
				AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}", "http://w3id.org/roh/crisIdentifier", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_crisIdentifier).ToLower()}\"", list, " . ");
			}
			if (this.Roh_relationshipTypeOther != null)
			{
				AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}", "http://w3id.org/roh/relationshipTypeOther", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_relationshipTypeOther).ToLower()}\"", list, " . ");
			}
			if (this.Vivo_start != null)
			{
				AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}", "http://vivoweb.org/ontology/core#start", $"{this.Vivo_start.Value.ToString("yyyyMMddHHmmss")}", list, " . ");
			}
			if (this.Roh_title != null)
			{
				AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}", "http://w3id.org/roh/title", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_title).ToLower()}\"", list, " . ");
			}
			if (listaSearch != null && listaSearch.Count > 0)
			{
				foreach (string valorSearch in listaSearch)
				{
					search += $"{valorSearch} ";
				}
			}
			if (!string.IsNullOrEmpty(search))
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
			if (propiedad is IList)
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
						foreach (string valor in listaValores)
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
			return $"{resourceAPI.GraphsUrl}items/CollaborationOntology_{ResourceID}_{ArticleID}";
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
			if (!string.IsNullOrEmpty(pObjeto) && !pObjeto.Equals("\"\"") && !pObjeto.Equals("<>"))
			{
				pLista.Add($"<{pSujeto}> <{pPredicado}> {pObjeto}{pDatosExtra}");
			}
		}

		private void AgregarTags(List<string> pListaTriples)
		{
			foreach (string tag in tagList)
			{
				AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}", "http://rdfs.org/sioc/types#Tag", tag.ToLower(), pListaTriples, " . ");
			}
		}


	}
}
