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

namespace SupervisedartisticprojectOntology
{
	[ExcludeFromCodeCoverage]
	public class SupervisedArtisticProject : GnossOCBase
	{

		public SupervisedArtisticProject() : base() { } 

		public SupervisedArtisticProject(SemanticResourceModel pSemCmsModel, LanguageEnum idiomaUsuario) : base()
		{
			this.mGNOSSID = pSemCmsModel.RootEntities[0].Entity.Uri;
			SemanticPropertyModel propVcard_hasRegion = pSemCmsModel.GetPropertyByPath("https://www.w3.org/2006/vcard/ns#hasRegion");
			if(propVcard_hasRegion != null && propVcard_hasRegion.PropertyValues.Count > 0)
			{
				this.Vcard_hasRegion = new Feature(propVcard_hasRegion.PropertyValues[0].RelatedEntity,idiomaUsuario);
			}
			SemanticPropertyModel propVcard_hasCountryName = pSemCmsModel.GetPropertyByPath("https://www.w3.org/2006/vcard/ns#hasCountryName");
			if(propVcard_hasCountryName != null && propVcard_hasCountryName.PropertyValues.Count > 0)
			{
				this.Vcard_hasCountryName = new Feature(propVcard_hasCountryName.PropertyValues[0].RelatedEntity,idiomaUsuario);
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
			this.Roh_cataloguing= GetBooleanPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/cataloguing"));
			this.Roh_exhibitionForum = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/exhibitionForum"));
			this.Roh_others = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/others"));
			this.Roh_crisIdentifier = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/crisIdentifier"));
			this.Vivo_description = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://vivoweb.org/ontology/core#description"));
			this.Vivo_start= GetDateValuePropertySemCms(pSemCmsModel.GetPropertyByPath("http://vivoweb.org/ontology/core#start"));
			this.Roh_monographic= GetBooleanPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/monographic"));
			this.Roh_award= GetBooleanPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/award"));
			this.Roh_catalogue= GetBooleanPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/catalogue"));
			this.Vcard_locality = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("https://www.w3.org/2006/vcard/ns#locality"));
			this.Roh_collective= GetBooleanPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/collective"));
			this.Roh_title = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/title"));
		}

		public SupervisedArtisticProject(SemanticEntityModel pSemCmsModel, LanguageEnum idiomaUsuario) : base()
		{
			this.mGNOSSID = pSemCmsModel.Entity.Uri;
			this.mURL = pSemCmsModel.Properties.FirstOrDefault(p => p.PropertyValues.Any(prop => prop.DownloadUrl != null))?.FirstPropertyValue.DownloadUrl;
			SemanticPropertyModel propVcard_hasRegion = pSemCmsModel.GetPropertyByPath("https://www.w3.org/2006/vcard/ns#hasRegion");
			if(propVcard_hasRegion != null && propVcard_hasRegion.PropertyValues.Count > 0)
			{
				this.Vcard_hasRegion = new Feature(propVcard_hasRegion.PropertyValues[0].RelatedEntity,idiomaUsuario);
			}
			SemanticPropertyModel propVcard_hasCountryName = pSemCmsModel.GetPropertyByPath("https://www.w3.org/2006/vcard/ns#hasCountryName");
			if(propVcard_hasCountryName != null && propVcard_hasCountryName.PropertyValues.Count > 0)
			{
				this.Vcard_hasCountryName = new Feature(propVcard_hasCountryName.PropertyValues[0].RelatedEntity,idiomaUsuario);
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
			this.Roh_cataloguing= GetBooleanPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/cataloguing"));
			this.Roh_exhibitionForum = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/exhibitionForum"));
			this.Roh_others = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/others"));
			this.Roh_crisIdentifier = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/crisIdentifier"));
			this.Vivo_description = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://vivoweb.org/ontology/core#description"));
			this.Vivo_start= GetDateValuePropertySemCms(pSemCmsModel.GetPropertyByPath("http://vivoweb.org/ontology/core#start"));
			this.Roh_monographic= GetBooleanPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/monographic"));
			this.Roh_award= GetBooleanPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/award"));
			this.Roh_catalogue= GetBooleanPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/catalogue"));
			this.Vcard_locality = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("https://www.w3.org/2006/vcard/ns#locality"));
			this.Roh_collective= GetBooleanPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/collective"));
			this.Roh_title = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/title"));
		}

		public virtual string RdfType { get { return "http://w3id.org/roh/SupervisedArtisticProject"; } }
		public virtual string RdfsLabel { get { return "http://w3id.org/roh/SupervisedArtisticProject"; } }
		[LABEL(LanguageEnum.es,"https://www.w3.org/2006/vcard/ns#hasRegion")]
		[RDFProperty("https://www.w3.org/2006/vcard/ns#hasRegion")]
		public  Feature Vcard_hasRegion  { get; set;} 
		public string IdVcard_hasRegion  { get; set;} 

		[LABEL(LanguageEnum.es,"https://www.w3.org/2006/vcard/ns#hasCountryName")]
		[RDFProperty("https://www.w3.org/2006/vcard/ns#hasCountryName")]
		public  Feature Vcard_hasCountryName  { get; set;} 
		public string IdVcard_hasCountryName  { get; set;} 

		[LABEL(LanguageEnum.es,"http://vivoweb.org/ontology/core#relates")]
		[RDFProperty("http://vivoweb.org/ontology/core#relates")]
		public  List<BFO_0000023> Vivo_relates { get; set;}

		[LABEL(LanguageEnum.es,"http://w3id.org/roh/cataloguing")]
		[RDFProperty("http://w3id.org/roh/cataloguing")]
		public  bool Roh_cataloguing { get; set;}

		[LABEL(LanguageEnum.es,"http://w3id.org/roh/exhibitionForum")]
		[RDFProperty("http://w3id.org/roh/exhibitionForum")]
		public  string Roh_exhibitionForum { get; set;}

		[LABEL(LanguageEnum.es,"http://w3id.org/roh/others")]
		[RDFProperty("http://w3id.org/roh/others")]
		public  string Roh_others { get; set;}

		[LABEL(LanguageEnum.es,"http://w3id.org/roh/crisIdentifier")]
		[RDFProperty("http://w3id.org/roh/crisIdentifier")]
		public  string Roh_crisIdentifier { get; set;}

		[LABEL(LanguageEnum.es,"http://vivoweb.org/ontology/core#description")]
		[RDFProperty("http://vivoweb.org/ontology/core#description")]
		public  string Vivo_description { get; set;}

		[LABEL(LanguageEnum.es,"http://vivoweb.org/ontology/core#start")]
		[RDFProperty("http://vivoweb.org/ontology/core#start")]
		public  DateTime? Vivo_start { get; set;}

		[LABEL(LanguageEnum.es,"http://w3id.org/roh/monographic")]
		[RDFProperty("http://w3id.org/roh/monographic")]
		public  bool Roh_monographic { get; set;}

		[LABEL(LanguageEnum.es,"http://w3id.org/roh/award")]
		[RDFProperty("http://w3id.org/roh/award")]
		public  bool Roh_award { get; set;}

		[LABEL(LanguageEnum.es,"http://w3id.org/roh/catalogue")]
		[RDFProperty("http://w3id.org/roh/catalogue")]
		public  bool Roh_catalogue { get; set;}

		[LABEL(LanguageEnum.es,"https://www.w3.org/2006/vcard/ns#locality")]
		[RDFProperty("https://www.w3.org/2006/vcard/ns#locality")]
		public  string Vcard_locality { get; set;}

		[LABEL(LanguageEnum.es,"http://w3id.org/roh/collective")]
		[RDFProperty("http://w3id.org/roh/collective")]
		public  bool Roh_collective { get; set;}

		[LABEL(LanguageEnum.es,"http://w3id.org/roh/title")]
		[RDFProperty("http://w3id.org/roh/title")]
		public  string Roh_title { get; set;}


		internal override void GetProperties()
		{
			base.GetProperties();
			propList.Add(new StringOntologyProperty("vcard:hasRegion", this.IdVcard_hasRegion));
			propList.Add(new StringOntologyProperty("vcard:hasCountryName", this.IdVcard_hasCountryName));
			propList.Add(new BoolOntologyProperty("roh:cataloguing", this.Roh_cataloguing));
			propList.Add(new StringOntologyProperty("roh:exhibitionForum", this.Roh_exhibitionForum));
			propList.Add(new StringOntologyProperty("roh:others", this.Roh_others));
			propList.Add(new StringOntologyProperty("roh:crisIdentifier", this.Roh_crisIdentifier));
			propList.Add(new StringOntologyProperty("vivo:description", this.Vivo_description));
			if (this.Vivo_start.HasValue){
				propList.Add(new DateOntologyProperty("vivo:start", this.Vivo_start.Value));
				}
			propList.Add(new BoolOntologyProperty("roh:monographic", this.Roh_monographic));
			propList.Add(new BoolOntologyProperty("roh:award", this.Roh_award));
			propList.Add(new BoolOntologyProperty("roh:catalogue", this.Roh_catalogue));
			propList.Add(new StringOntologyProperty("vcard:locality", this.Vcard_locality));
			propList.Add(new BoolOntologyProperty("roh:collective", this.Roh_collective));
			propList.Add(new StringOntologyProperty("roh:title", this.Roh_title));
		}

		internal override void GetEntities()
		{
			base.GetEntities();
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
			AgregarTripleALista($"{resourceAPI.GraphsUrl}items/SupervisedArtisticProject_{ResourceID}_{ArticleID}", "http://www.w3.org/1999/02/22-rdf-syntax-ns#type", $"<http://w3id.org/roh/SupervisedArtisticProject>", list, " . ");
			AgregarTripleALista($"{resourceAPI.GraphsUrl}items/SupervisedArtisticProject_{ResourceID}_{ArticleID}", "http://www.w3.org/2000/01/rdf-schema#label", $"\"http://w3id.org/roh/SupervisedArtisticProject\"", list, " . ");
			AgregarTripleALista($"{resourceAPI.GraphsUrl}{ResourceID}", "http://gnoss/hasEntidad", $"<{resourceAPI.GraphsUrl}items/SupervisedArtisticProject_{ResourceID}_{ArticleID}>", list, " . ");
			if(this.Vivo_relates != null)
			{
			foreach(var item0 in this.Vivo_relates)
			{
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/BFO_0000023_{ResourceID}_{item0.ArticleID}", "http://www.w3.org/1999/02/22-rdf-syntax-ns#type", $"<http://purl.obolibrary.org/obo/BFO_0000023>", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/BFO_0000023_{ResourceID}_{item0.ArticleID}", "http://www.w3.org/2000/01/rdf-schema#label", $"\"http://purl.obolibrary.org/obo/BFO_0000023\"", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}{ResourceID}", "http://gnoss/hasEntidad", $"<{resourceAPI.GraphsUrl}items/BFO_0000023_{ResourceID}_{item0.ArticleID}>", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/SupervisedArtisticProject_{ResourceID}_{ArticleID}", "http://vivoweb.org/ontology/core#relates", $"<{resourceAPI.GraphsUrl}items/BFO_0000023_{ResourceID}_{item0.ArticleID}>", list, " . ");
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
				if(this.IdVcard_hasRegion != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/SupervisedArtisticProject_{ResourceID}_{ArticleID}",  "https://www.w3.org/2006/vcard/ns#hasRegion", $"<{this.IdVcard_hasRegion}>", list, " . ");
				}
				if(this.IdVcard_hasCountryName != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/SupervisedArtisticProject_{ResourceID}_{ArticleID}",  "https://www.w3.org/2006/vcard/ns#hasCountryName", $"<{this.IdVcard_hasCountryName}>", list, " . ");
				}
				if(this.Roh_cataloguing != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/SupervisedArtisticProject_{ResourceID}_{ArticleID}",  "http://w3id.org/roh/cataloguing", $"\"{this.Roh_cataloguing.ToString()}\"", list, " . ");
				}
				if(this.Roh_exhibitionForum != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/SupervisedArtisticProject_{ResourceID}_{ArticleID}",  "http://w3id.org/roh/exhibitionForum", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_exhibitionForum)}\"", list, " . ");
				}
				if(this.Roh_others != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/SupervisedArtisticProject_{ResourceID}_{ArticleID}",  "http://w3id.org/roh/others", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_others)}\"", list, " . ");
				}
				if(this.Roh_crisIdentifier != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/SupervisedArtisticProject_{ResourceID}_{ArticleID}",  "http://w3id.org/roh/crisIdentifier", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_crisIdentifier)}\"", list, " . ");
				}
				if(this.Vivo_description != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/SupervisedArtisticProject_{ResourceID}_{ArticleID}",  "http://vivoweb.org/ontology/core#description", $"\"{GenerarTextoSinSaltoDeLinea(this.Vivo_description)}\"", list, " . ");
				}
				if(this.Vivo_start != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/SupervisedArtisticProject_{ResourceID}_{ArticleID}",  "http://vivoweb.org/ontology/core#start", $"\"{this.Vivo_start.Value.ToString("yyyyMMddHHmmss")}\"", list, " . ");
				}
				if(this.Roh_monographic != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/SupervisedArtisticProject_{ResourceID}_{ArticleID}",  "http://w3id.org/roh/monographic", $"\"{this.Roh_monographic.ToString()}\"", list, " . ");
				}
				if(this.Roh_award != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/SupervisedArtisticProject_{ResourceID}_{ArticleID}",  "http://w3id.org/roh/award", $"\"{this.Roh_award.ToString()}\"", list, " . ");
				}
				if(this.Roh_catalogue != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/SupervisedArtisticProject_{ResourceID}_{ArticleID}",  "http://w3id.org/roh/catalogue", $"\"{this.Roh_catalogue.ToString()}\"", list, " . ");
				}
				if(this.Vcard_locality != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/SupervisedArtisticProject_{ResourceID}_{ArticleID}",  "https://www.w3.org/2006/vcard/ns#locality", $"\"{GenerarTextoSinSaltoDeLinea(this.Vcard_locality)}\"", list, " . ");
				}
				if(this.Roh_collective != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/SupervisedArtisticProject_{ResourceID}_{ArticleID}",  "http://w3id.org/roh/collective", $"\"{this.Roh_collective.ToString()}\"", list, " . ");
				}
				if(this.Roh_title != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/SupervisedArtisticProject_{ResourceID}_{ArticleID}",  "http://w3id.org/roh/title", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_title)}\"", list, " . ");
				}
			return list;
		}

		public override List<string> ToSearchGraphTriples(ResourceApi resourceAPI)
		{
			List<string> list = new List<string>();
			List<string> listaSearch = new List<string>();
			AgregarTags(list);
			AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}", "http://www.w3.org/1999/02/22-rdf-syntax-ns#type", $"\"supervisedartisticproject\"", list, " . ");
			AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}", "http://gnoss/type", $"\"http://w3id.org/roh/SupervisedArtisticProject\"", list, " . ");
			AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}", "http://gnoss/hasfechapublicacion", $"{DateTime.Now.ToString("yyyyMMddHHmmss")}", list, " . ");
			AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}", "http://gnoss/hastipodoc", "\"5\"", list, " . ");
			AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}", "http://gnoss/hasfechamodificacion", $"{DateTime.Now.ToString("yyyyMMddHHmmss")}", list, " . ");
			AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}", "http://gnoss/hasnumeroVisitas", "0", list, " . ");
			AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}", "http://gnoss/hasprivacidadCom", "\"publico\"", list, " . ");
			AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}", "http://xmlns.com/foaf/0.1/firstName", $"\"{GenerarTextoSinSaltoDeLinea(this.Vivo_description)}\"", list, " . ");
			AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}", "http://gnoss/hasnombrecompleto", $"\"{GenerarTextoSinSaltoDeLinea(this.Vivo_description)}\"", list, " . ");
			string search = string.Empty;
			if(this.Vivo_relates != null)
			{
			foreach(var item0 in this.Vivo_relates)
			{
				AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}", "http://vivoweb.org/ontology/core#relates", $"<{resourceAPI.GraphsUrl}items/bfo_0000023_{ResourceID}_{item0.ArticleID}>", list, " . ");
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
				if(this.Roh_cataloguing != null)
				{
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "http://w3id.org/roh/cataloguing", $"\"{this.Roh_cataloguing.ToString().ToLower()}\"", list, " . ");
				}
				if(this.Roh_exhibitionForum != null)
				{
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "http://w3id.org/roh/exhibitionForum", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_exhibitionForum).ToLower()}\"", list, " . ");
				}
				if(this.Roh_others != null)
				{
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "http://w3id.org/roh/others", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_others).ToLower()}\"", list, " . ");
				}
				if(this.Roh_crisIdentifier != null)
				{
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "http://w3id.org/roh/crisIdentifier", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_crisIdentifier).ToLower()}\"", list, " . ");
				}
				if(this.Vivo_description != null)
				{
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "http://vivoweb.org/ontology/core#description", $"\"{GenerarTextoSinSaltoDeLinea(this.Vivo_description).ToLower()}\"", list, " . ");
				}
				if(this.Vivo_start != null)
				{
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "http://vivoweb.org/ontology/core#start", $"{this.Vivo_start.Value.ToString("yyyyMMddHHmmss")}", list, " . ");
				}
				if(this.Roh_monographic != null)
				{
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "http://w3id.org/roh/monographic", $"\"{this.Roh_monographic.ToString().ToLower()}\"", list, " . ");
				}
				if(this.Roh_award != null)
				{
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "http://w3id.org/roh/award", $"\"{this.Roh_award.ToString().ToLower()}\"", list, " . ");
				}
				if(this.Roh_catalogue != null)
				{
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "http://w3id.org/roh/catalogue", $"\"{this.Roh_catalogue.ToString().ToLower()}\"", list, " . ");
				}
				if(this.Vcard_locality != null)
				{
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "https://www.w3.org/2006/vcard/ns#locality", $"\"{GenerarTextoSinSaltoDeLinea(this.Vcard_locality).ToLower()}\"", list, " . ");
				}
				if(this.Roh_collective != null)
				{
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "http://w3id.org/roh/collective", $"\"{this.Roh_collective.ToString().ToLower()}\"", list, " . ");
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
			return $"{resourceAPI.GraphsUrl}items/SupervisedartisticprojectOntology_{ResourceID}_{ArticleID}";
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
