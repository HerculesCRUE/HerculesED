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
using Gender = GenderOntology.Gender;
using Feature = FeatureOntology.Feature;

namespace CurriculumvitaeOntology
{
	[ExcludeFromCodeCoverage]
	public class PersonalData : GnossOCBase
	{

		public PersonalData() : base() { } 

		public PersonalData(SemanticEntityModel pSemCmsModel, LanguageEnum idiomaUsuario) : base()
		{
			this.mGNOSSID = pSemCmsModel.Entity.Uri;
			this.mURL = pSemCmsModel.Properties.FirstOrDefault(p => p.PropertyValues.Any(prop => prop.DownloadUrl != null))?.FirstPropertyValue.DownloadUrl;
			SemanticPropertyModel propRoh_hasFax = pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/hasFax");
			if(propRoh_hasFax != null && propRoh_hasFax.PropertyValues.Count > 0)
			{
				this.Roh_hasFax = new TelephoneType(propRoh_hasFax.PropertyValues[0].RelatedEntity,idiomaUsuario);
			}
			SemanticPropertyModel propRoh_birthplace = pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/birthplace");
			if(propRoh_birthplace != null && propRoh_birthplace.PropertyValues.Count > 0)
			{
				this.Roh_birthplace = new Address(propRoh_birthplace.PropertyValues[0].RelatedEntity,idiomaUsuario);
			}
			this.Roh_otherIds = new List<Document>();
			SemanticPropertyModel propRoh_otherIds = pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/otherIds");
			if(propRoh_otherIds != null && propRoh_otherIds.PropertyValues.Count > 0)
			{
				foreach (SemanticPropertyModel.PropertyValue propValue in propRoh_otherIds.PropertyValues)
				{
					if(propValue.RelatedEntity!=null){
						Document roh_otherIds = new Document(propValue.RelatedEntity,idiomaUsuario);
						this.Roh_otherIds.Add(roh_otherIds);
					}
				}
			}
			SemanticPropertyModel propVcard_address = pSemCmsModel.GetPropertyByPath("https://www.w3.org/2006/vcard/ns#address");
			if(propVcard_address != null && propVcard_address.PropertyValues.Count > 0)
			{
				this.Vcard_address = new Address(propVcard_address.PropertyValues[0].RelatedEntity,idiomaUsuario);
			}
			SemanticPropertyModel propFoaf_gender = pSemCmsModel.GetPropertyByPath("http://xmlns.com/foaf/0.1/gender");
			if(propFoaf_gender != null && propFoaf_gender.PropertyValues.Count > 0)
			{
				this.Foaf_gender = new Gender(propFoaf_gender.PropertyValues[0].RelatedEntity,idiomaUsuario);
			}
			SemanticPropertyModel propSchema_nationality = pSemCmsModel.GetPropertyByPath("http://www.schema.org/nationality");
			if(propSchema_nationality != null && propSchema_nationality.PropertyValues.Count > 0)
			{
				this.Schema_nationality = new Feature(propSchema_nationality.PropertyValues[0].RelatedEntity,idiomaUsuario);
			}
			this.Vcard_hasTelephone = new List<TelephoneType>();
			SemanticPropertyModel propVcard_hasTelephone = pSemCmsModel.GetPropertyByPath("https://www.w3.org/2006/vcard/ns#hasTelephone");
			if(propVcard_hasTelephone != null && propVcard_hasTelephone.PropertyValues.Count > 0)
			{
				foreach (SemanticPropertyModel.PropertyValue propValue in propVcard_hasTelephone.PropertyValues)
				{
					if(propValue.RelatedEntity!=null){
						TelephoneType vcard_hasTelephone = new TelephoneType(propValue.RelatedEntity,idiomaUsuario);
						this.Vcard_hasTelephone.Add(vcard_hasTelephone);
					}
				}
			}
			SemanticPropertyModel propRoh_hasMobilePhone = pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/hasMobilePhone");
			if(propRoh_hasMobilePhone != null && propRoh_hasMobilePhone.PropertyValues.Count > 0)
			{
				this.Roh_hasMobilePhone = new TelephoneType(propRoh_hasMobilePhone.PropertyValues[0].RelatedEntity,idiomaUsuario);
			}
			this.Roh_nie = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/nie"));
			this.Vivo_researcherId = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://vivoweb.org/ontology/core#researcherId"));
			this.Vivo_scopusId = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://vivoweb.org/ontology/core#scopusId"));
			this.Foaf_img = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://xmlns.com/foaf/0.1/img"));
			this.Roh_dni = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/dni"));
			SemanticPropertyModel propFoaf_homepage = pSemCmsModel.GetPropertyByPath("http://xmlns.com/foaf/0.1/homepage");
			this.Foaf_homepage = new List<string>();
			if (propFoaf_homepage != null && propFoaf_homepage.PropertyValues.Count > 0)
			{
				foreach (SemanticPropertyModel.PropertyValue propValue in propFoaf_homepage.PropertyValues)
				{
					this.Foaf_homepage.Add(propValue.Value);
				}
			}
			this.Roh_ORCID = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/ORCID"));
			this.Roh_passport = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/passport"));
			this.Vcard_birth_date= GetDateValuePropertySemCms(pSemCmsModel.GetPropertyByPath("https://www.w3.org/2006/vcard/ns#birth-date"));
			this.Vcard_email = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("https://www.w3.org/2006/vcard/ns#email"));
		}

		public virtual string RdfType { get { return "http://w3id.org/roh/PersonalData"; } }
		public virtual string RdfsLabel { get { return "http://w3id.org/roh/PersonalData"; } }
		public OntologyEntity Entity { get; set; }

		[LABEL(LanguageEnum.es,"http://w3id.org/roh/hasFax")]
		[RDFProperty("http://w3id.org/roh/hasFax")]
		public  TelephoneType Roh_hasFax { get; set;}

		[LABEL(LanguageEnum.es,"http://w3id.org/roh/birthplace")]
		[RDFProperty("http://w3id.org/roh/birthplace")]
		public  Address Roh_birthplace { get; set;}

		[LABEL(LanguageEnum.es,"http://w3id.org/roh/otherIds")]
		[RDFProperty("http://w3id.org/roh/otherIds")]
		public  List<Document> Roh_otherIds { get; set;}

		[LABEL(LanguageEnum.es,"https://www.w3.org/2006/vcard/ns#address")]
		[RDFProperty("https://www.w3.org/2006/vcard/ns#address")]
		public  Address Vcard_address { get; set;}

		[LABEL(LanguageEnum.es,"http://xmlns.com/foaf/0.1/gender")]
		[RDFProperty("http://xmlns.com/foaf/0.1/gender")]
		public  Gender Foaf_gender  { get; set;} 
		public string IdFoaf_gender  { get; set;} 

		[LABEL(LanguageEnum.es,"http://www.schema.org/nationality")]
		[RDFProperty("http://www.schema.org/nationality")]
		public  Feature Schema_nationality  { get; set;} 
		public string IdSchema_nationality  { get; set;} 

		[LABEL(LanguageEnum.es,"https://www.w3.org/2006/vcard/ns#hasTelephone")]
		[RDFProperty("https://www.w3.org/2006/vcard/ns#hasTelephone")]
		public  List<TelephoneType> Vcard_hasTelephone { get; set;}

		[LABEL(LanguageEnum.es,"http://w3id.org/roh/hasMobilePhone")]
		[RDFProperty("http://w3id.org/roh/hasMobilePhone")]
		public  TelephoneType Roh_hasMobilePhone { get; set;}

		[LABEL(LanguageEnum.es,"http://w3id.org/roh/nie")]
		[RDFProperty("http://w3id.org/roh/nie")]
		public  string Roh_nie { get; set;}

		[LABEL(LanguageEnum.es,"http://vivoweb.org/ontology/core#researcherId")]
		[RDFProperty("http://vivoweb.org/ontology/core#researcherId")]
		public  string Vivo_researcherId { get; set;}

		[LABEL(LanguageEnum.es,"http://vivoweb.org/ontology/core#scopusId")]
		[RDFProperty("http://vivoweb.org/ontology/core#scopusId")]
		public  string Vivo_scopusId { get; set;}

		[LABEL(LanguageEnum.es,"Imagen")]
		[RDFProperty("http://xmlns.com/foaf/0.1/img")]
		public  string Foaf_img { get; set;}

		[LABEL(LanguageEnum.es,"http://w3id.org/roh/dni")]
		[RDFProperty("http://w3id.org/roh/dni")]
		public  string Roh_dni { get; set;}

		[LABEL(LanguageEnum.es,"http://xmlns.com/foaf/0.1/homepage")]
		[RDFProperty("http://xmlns.com/foaf/0.1/homepage")]
		public  List<string> Foaf_homepage { get; set;}

		[LABEL(LanguageEnum.es,"http://w3id.org/roh/ORCID")]
		[RDFProperty("http://w3id.org/roh/ORCID")]
		public  string Roh_ORCID { get; set;}

		[LABEL(LanguageEnum.es,"http://w3id.org/roh/passport")]
		[RDFProperty("http://w3id.org/roh/passport")]
		public  string Roh_passport { get; set;}

		[LABEL(LanguageEnum.es,"https://www.w3.org/2006/vcard/ns#birth-date")]
		[RDFProperty("https://www.w3.org/2006/vcard/ns#birth-date")]
		public  DateTime? Vcard_birth_date { get; set;}

		[LABEL(LanguageEnum.es,"https://www.w3.org/2006/vcard/ns#email")]
		[RDFProperty("https://www.w3.org/2006/vcard/ns#email")]
		public  string Vcard_email { get; set;}


		internal override void GetProperties()
		{
			base.GetProperties();
			propList.Add(new StringOntologyProperty("foaf:gender", this.IdFoaf_gender));
			propList.Add(new StringOntologyProperty("schema:nationality", this.IdSchema_nationality));
			propList.Add(new StringOntologyProperty("roh:nie", this.Roh_nie));
			propList.Add(new StringOntologyProperty("vivo:researcherId", this.Vivo_researcherId));
			propList.Add(new StringOntologyProperty("vivo:scopusId", this.Vivo_scopusId));
			propList.Add(new StringOntologyProperty("foaf:img", this.Foaf_img));
			propList.Add(new StringOntologyProperty("roh:dni", this.Roh_dni));
			propList.Add(new ListStringOntologyProperty("foaf:homepage", this.Foaf_homepage));
			propList.Add(new StringOntologyProperty("roh:ORCID", this.Roh_ORCID));
			propList.Add(new StringOntologyProperty("roh:passport", this.Roh_passport));
			if (this.Vcard_birth_date.HasValue){
				propList.Add(new DateOntologyProperty("vcard:birth-date", this.Vcard_birth_date.Value));
				}
			propList.Add(new StringOntologyProperty("vcard:email", this.Vcard_email));
		}

		internal override void GetEntities()
		{
			base.GetEntities();
			if(Roh_hasFax!=null){
				Roh_hasFax.GetProperties();
				Roh_hasFax.GetEntities();
				OntologyEntity entityRoh_hasFax = new OntologyEntity("https://www.w3.org/2006/vcard/ns#TelephoneType", "https://www.w3.org/2006/vcard/ns#TelephoneType", "roh:hasFax", Roh_hasFax.propList, Roh_hasFax.entList);
				entList.Add(entityRoh_hasFax);
			}
			if(Roh_birthplace!=null){
				Roh_birthplace.GetProperties();
				Roh_birthplace.GetEntities();
				OntologyEntity entityRoh_birthplace = new OntologyEntity("https://www.w3.org/2006/vcard/ns#Address", "https://www.w3.org/2006/vcard/ns#Address", "roh:birthplace", Roh_birthplace.propList, Roh_birthplace.entList);
				entList.Add(entityRoh_birthplace);
			}
			if(Roh_otherIds!=null){
				foreach(Document prop in Roh_otherIds){
					prop.GetProperties();
					prop.GetEntities();
					OntologyEntity entityDocument = new OntologyEntity("http://xmlns.com/foaf/0.1/Document", "http://xmlns.com/foaf/0.1/Document", "roh:otherIds", prop.propList, prop.entList);
				entList.Add(entityDocument);
				prop.Entity= entityDocument;
				}
			}
			if(Vcard_address!=null){
				Vcard_address.GetProperties();
				Vcard_address.GetEntities();
				OntologyEntity entityVcard_address = new OntologyEntity("https://www.w3.org/2006/vcard/ns#Address", "https://www.w3.org/2006/vcard/ns#Address", "vcard:address", Vcard_address.propList, Vcard_address.entList);
				entList.Add(entityVcard_address);
			}
			if(Vcard_hasTelephone!=null){
				foreach(TelephoneType prop in Vcard_hasTelephone){
					prop.GetProperties();
					prop.GetEntities();
					OntologyEntity entityTelephoneType = new OntologyEntity("https://www.w3.org/2006/vcard/ns#TelephoneType", "https://www.w3.org/2006/vcard/ns#TelephoneType", "vcard:hasTelephone", prop.propList, prop.entList);
				entList.Add(entityTelephoneType);
				prop.Entity= entityTelephoneType;
				}
			}
			if(Roh_hasMobilePhone!=null){
				Roh_hasMobilePhone.GetProperties();
				Roh_hasMobilePhone.GetEntities();
				OntologyEntity entityRoh_hasMobilePhone = new OntologyEntity("https://www.w3.org/2006/vcard/ns#TelephoneType", "https://www.w3.org/2006/vcard/ns#TelephoneType", "roh:hasMobilePhone", Roh_hasMobilePhone.propList, Roh_hasMobilePhone.entList);
				entList.Add(entityRoh_hasMobilePhone);
			}
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

		private string GenerarTextoSinSaltoDeLinea(string pTexto)
		{
			return pTexto.Replace("\r\n", " ").Replace("\n", " ").Replace("\r", " ").Replace("\"", "\\\"");
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

		internal override void AddImages(ComplexOntologyResource pResource)
		{
			base.AddImages(pResource);
			List<ImageAction> actionListimg = new List<ImageAction>();
			actionListimg.Add(new ImageAction(0,234, ImageTransformationType.Crop, 100));
			pResource.AttachImage(this.Foaf_img, actionListimg,"foaf:img", true, this.GetExtension(this.Foaf_img), this.Entity);
			this.Roh_hasFax.AddImages(pResource);
			this.Roh_birthplace.AddImages(pResource);
			if(Roh_otherIds!=null){
				foreach (Document prop in Roh_otherIds)
			{
				prop.AddImages(pResource);
				}
			}
			this.Vcard_address.AddImages(pResource);
			if(Vcard_hasTelephone!=null){
				foreach (TelephoneType prop in Vcard_hasTelephone)
			{
				prop.AddImages(pResource);
				}
			}
			this.Roh_hasMobilePhone.AddImages(pResource);
		}

	}
}
