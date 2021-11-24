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
using OrganizationType = OrganizationtypeOntology.OrganizationType;

namespace OrganizationOntology
{
	public class Organization : GnossOCBase
	{

		public Organization() : base() { }

		public Organization(SemanticResourceModel pSemCmsModel, LanguageEnum idiomaUsuario) : base()
		{
			this.mGNOSSID = pSemCmsModel.RootEntities[0].Entity.Uri;
			SemanticPropertyModel propVcard_hasRegion = pSemCmsModel.GetPropertyByPath("https://www.w3.org/2006/vcard/ns#hasRegion");
			if (propVcard_hasRegion != null && propVcard_hasRegion.PropertyValues.Count > 0)
			{
				this.Vcard_hasRegion = new Feature(propVcard_hasRegion.PropertyValues[0].RelatedEntity, idiomaUsuario);
			}
			SemanticPropertyModel propVcard_hasCountryName = pSemCmsModel.GetPropertyByPath("https://www.w3.org/2006/vcard/ns#hasCountryName");
			if (propVcard_hasCountryName != null && propVcard_hasCountryName.PropertyValues.Count > 0)
			{
				this.Vcard_hasCountryName = new Feature(propVcard_hasCountryName.PropertyValues[0].RelatedEntity, idiomaUsuario);
			}
			SemanticPropertyModel propDc_type = pSemCmsModel.GetPropertyByPath("http://purl.org/dc/elements/1.1/type");
			if (propDc_type != null && propDc_type.PropertyValues.Count > 0)
			{
				this.Dc_type = new OrganizationType(propDc_type.PropertyValues[0].RelatedEntity, idiomaUsuario);
			}
			this.Roh_crisIdentifier = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/crisIdentifier"));
			this.Roh_otherTypeMoreInfo = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/otherTypeMoreInfo"));
			this.Vcard_locality = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("https://www.w3.org/2006/vcard/ns#locality"));
			this.Roh_isSynchronized = GetBooleanPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/isSynchronized"));
			this.Roh_title = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/title"));
		}

		public Organization(SemanticEntityModel pSemCmsModel, LanguageEnum idiomaUsuario) : base()
		{
			this.mGNOSSID = pSemCmsModel.Entity.Uri;
			this.mURL = pSemCmsModel.Properties.FirstOrDefault(p => p.PropertyValues.Any(prop => prop.DownloadUrl != null))?.FirstPropertyValue.DownloadUrl;
			SemanticPropertyModel propVcard_hasRegion = pSemCmsModel.GetPropertyByPath("https://www.w3.org/2006/vcard/ns#hasRegion");
			if (propVcard_hasRegion != null && propVcard_hasRegion.PropertyValues.Count > 0)
			{
				this.Vcard_hasRegion = new Feature(propVcard_hasRegion.PropertyValues[0].RelatedEntity, idiomaUsuario);
			}
			SemanticPropertyModel propVcard_hasCountryName = pSemCmsModel.GetPropertyByPath("https://www.w3.org/2006/vcard/ns#hasCountryName");
			if (propVcard_hasCountryName != null && propVcard_hasCountryName.PropertyValues.Count > 0)
			{
				this.Vcard_hasCountryName = new Feature(propVcard_hasCountryName.PropertyValues[0].RelatedEntity, idiomaUsuario);
			}
			SemanticPropertyModel propDc_type = pSemCmsModel.GetPropertyByPath("http://purl.org/dc/elements/1.1/type");
			if (propDc_type != null && propDc_type.PropertyValues.Count > 0)
			{
				this.Dc_type = new OrganizationType(propDc_type.PropertyValues[0].RelatedEntity, idiomaUsuario);
			}
			this.Roh_crisIdentifier = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/crisIdentifier"));
			this.Roh_otherTypeMoreInfo = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/otherTypeMoreInfo"));
			this.Vcard_locality = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("https://www.w3.org/2006/vcard/ns#locality"));
			this.Roh_isSynchronized = GetBooleanPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/isSynchronized"));
			this.Roh_title = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/title"));
		}

		public virtual string RdfType { get { return "http://xmlns.com/foaf/0.1/Organization"; } }
		public virtual string RdfsLabel { get { return "http://xmlns.com/foaf/0.1/Organization"; } }
		[LABEL(LanguageEnum.es, "https://www.w3.org/2006/vcard/ns#hasRegion")]
		[RDFProperty("https://www.w3.org/2006/vcard/ns#hasRegion")]
		public Feature Vcard_hasRegion { get; set; }
		public string IdVcard_hasRegion { get; set; }

		[LABEL(LanguageEnum.es, "https://www.w3.org/2006/vcard/ns#hasCountryName")]
		[RDFProperty("https://www.w3.org/2006/vcard/ns#hasCountryName")]
		public Feature Vcard_hasCountryName { get; set; }
		public string IdVcard_hasCountryName { get; set; }

		[LABEL(LanguageEnum.es, "http://purl.org/dc/elements/1.1/type")]
		[RDFProperty("http://purl.org/dc/elements/1.1/type")]
		public OrganizationType Dc_type { get; set; }
		public string IdDc_type { get; set; }

		[LABEL(LanguageEnum.es, "http://w3id.org/roh/crisIdentifier")]
		[RDFProperty("http://w3id.org/roh/crisIdentifier")]
		public string Roh_crisIdentifier { get; set; }

		[LABEL(LanguageEnum.es, "http://w3id.org/roh/otherTypeMoreInfo")]
		[RDFProperty("http://w3id.org/roh/otherTypeMoreInfo")]
		public string Roh_otherTypeMoreInfo { get; set; }

		[LABEL(LanguageEnum.es, "https://www.w3.org/2006/vcard/ns#locality")]
		[RDFProperty("https://www.w3.org/2006/vcard/ns#locality")]
		public string Vcard_locality { get; set; }

		[LABEL(LanguageEnum.es, "http://w3id.org/roh/isSynchronized")]
		[RDFProperty("http://w3id.org/roh/isSynchronized")]
		public bool Roh_isSynchronized { get; set; }

		[LABEL(LanguageEnum.es, "http://w3id.org/roh/title")]
		[RDFProperty("http://w3id.org/roh/title")]
		public string Roh_title { get; set; }


		internal override void GetProperties()
		{
			base.GetProperties();
			propList.Add(new StringOntologyProperty("vcard:hasRegion", this.IdVcard_hasRegion));
			propList.Add(new StringOntologyProperty("vcard:hasCountryName", this.IdVcard_hasCountryName));
			propList.Add(new StringOntologyProperty("dc:type", this.IdDc_type));
			propList.Add(new StringOntologyProperty("roh:crisIdentifier", this.Roh_crisIdentifier));
			propList.Add(new StringOntologyProperty("roh:otherTypeMoreInfo", this.Roh_otherTypeMoreInfo));
			propList.Add(new StringOntologyProperty("vcard:locality", this.Vcard_locality));
			propList.Add(new BoolOntologyProperty("roh:isSynchronized", this.Roh_isSynchronized));
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
			Ontology ontology = null;
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
			AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Organization_{ResourceID}_{ArticleID}", "http://www.w3.org/1999/02/22-rdf-syntax-ns#type", $"<http://xmlns.com/foaf/0.1/Organization>", list, " . ");
			AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Organization_{ResourceID}_{ArticleID}", "http://www.w3.org/2000/01/rdf-schema#label", $"\"http://xmlns.com/foaf/0.1/Organization\"", list, " . ");
			AgregarTripleALista($"{resourceAPI.GraphsUrl}{ResourceID}", "http://gnoss/hasEntidad", $"<{resourceAPI.GraphsUrl}items/Organization_{ResourceID}_{ArticleID}>", list, " . ");
			if (this.IdVcard_hasRegion != null)
			{
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Organization_{ResourceID}_{ArticleID}", "https://www.w3.org/2006/vcard/ns#hasRegion", $"<{this.IdVcard_hasRegion}>", list, " . ");
			}
			if (this.IdVcard_hasCountryName != null)
			{
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Organization_{ResourceID}_{ArticleID}", "https://www.w3.org/2006/vcard/ns#hasCountryName", $"<{this.IdVcard_hasCountryName}>", list, " . ");
			}
			if (this.IdDc_type != null)
			{
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Organization_{ResourceID}_{ArticleID}", "http://purl.org/dc/elements/1.1/type", $"<{this.IdDc_type}>", list, " . ");
			}
			if (this.Roh_crisIdentifier != null)
			{
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Organization_{ResourceID}_{ArticleID}", "http://w3id.org/roh/crisIdentifier", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_crisIdentifier)}\"", list, " . ");
			}
			if (this.Roh_otherTypeMoreInfo != null)
			{
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Organization_{ResourceID}_{ArticleID}", "http://w3id.org/roh/otherTypeMoreInfo", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_otherTypeMoreInfo)}\"", list, " . ");
			}
			if (this.Vcard_locality != null)
			{
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Organization_{ResourceID}_{ArticleID}", "https://www.w3.org/2006/vcard/ns#locality", $"\"{GenerarTextoSinSaltoDeLinea(this.Vcard_locality)}\"", list, " . ");
			}
			if (this.Roh_isSynchronized != null)
			{
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Organization_{ResourceID}_{ArticleID}", "http://w3id.org/roh/isSynchronized", $"\"{this.Roh_isSynchronized.ToString()}\"", list, " . ");
			}
			if (this.Roh_title != null)
			{
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Organization_{ResourceID}_{ArticleID}", "http://w3id.org/roh/title", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_title)}\"", list, " . ");
			}
			return list;
		}

		public override List<string> ToSearchGraphTriples(ResourceApi resourceAPI)
		{
			List<string> list = new List<string>();
			List<string> listaSearch = new List<string>();
			AgregarTags(list);
			AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}", "http://www.w3.org/1999/02/22-rdf-syntax-ns#type", $"\"organization\"", list, " . ");
			AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}", "http://gnoss/type", $"\"http://xmlns.com/foaf/0.1/Organization\"", list, " . ");
			AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}", "http://gnoss/hasfechapublicacion", $"{DateTime.Now.ToString("yyyyMMddHHmmss")}", list, " . ");
			AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}", "http://gnoss/hastipodoc", "\"5\"", list, " . ");
			AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}", "http://gnoss/hasfechamodificacion", $"{DateTime.Now.ToString("yyyyMMddHHmmss")}", list, " . ");
			AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}", "http://gnoss/hasnumeroVisitas", "0", list, " . ");
			AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}", "http://gnoss/hasprivacidadCom", "\"publico\"", list, " . ");
			AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}", "http://xmlns.com/foaf/0.1/firstName", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_title)}\"", list, " . ");
			AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}", "http://gnoss/hasnombrecompleto", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_title)}\"", list, " . ");
			string search = string.Empty;
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
			if (this.IdDc_type != null)
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
				AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}", "http://purl.org/dc/elements/1.1/type", $"<{itemRegex}>", list, " . ");
			}
			if (this.Roh_crisIdentifier != null)
			{
				AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}", "http://w3id.org/roh/crisIdentifier", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_crisIdentifier).ToLower()}\"", list, " . ");
			}
			if (this.Roh_otherTypeMoreInfo != null)
			{
				AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}", "http://w3id.org/roh/otherTypeMoreInfo", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_otherTypeMoreInfo).ToLower()}\"", list, " . ");
			}
			if (this.Vcard_locality != null)
			{
				AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}", "https://www.w3.org/2006/vcard/ns#locality", $"\"{GenerarTextoSinSaltoDeLinea(this.Vcard_locality).ToLower()}\"", list, " . ");
			}
			if (this.Roh_isSynchronized != null)
			{
				AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}", "http://w3id.org/roh/isSynchronized", $"\"{this.Roh_isSynchronized.ToString().ToLower()}\"", list, " . ");
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
			return $"{resourceAPI.GraphsUrl}items/OrganizationOntology_{ResourceID}_{ArticleID}";
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
