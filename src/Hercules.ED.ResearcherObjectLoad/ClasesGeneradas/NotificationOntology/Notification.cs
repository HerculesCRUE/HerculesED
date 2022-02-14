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

namespace NotificationOntology
{
	[ExcludeFromCodeCoverage]
	public class Notification : GnossOCBase
	{

		public Notification() : base() { } 

		public Notification(SemanticEntityModel pSemCmsModel, LanguageEnum idiomaUsuario) : base()
		{
			this.mGNOSSID = pSemCmsModel.Entity.Uri;
			this.mURL = pSemCmsModel.Properties.FirstOrDefault(p => p.PropertyValues.Any(prop => prop.DownloadUrl != null))?.FirstPropertyValue.DownloadUrl;
var item0 = GetDateValuePropertySemCms(pSemCmsModel.GetPropertyByPath("http://purl.org/dc/elements/1.1/date"));
if(item0.HasValue){
			this.Dc_date = item0.Value;
}
			this.Dc_description = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://purl.org/dc/elements/1.1/description"));
			this.Roh_gnossUser = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/gnossUser"));
			this.Dc_type = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://purl.org/dc/elements/1.1/type"));
			this.Dc_source = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://purl.org/dc/elements/1.1/source"));
		}

		public virtual string RdfType { get { return "http://w3id.org/roh/Notification"; } }
		public virtual string RdfsLabel { get { return "http://w3id.org/roh/Notification"; } }
		[LABEL(LanguageEnum.es,"Fecha: ")]
		[RDFProperty("http://purl.org/dc/elements/1.1/date")]
		public  DateTime Dc_date { get; set;}

		[LABEL(LanguageEnum.es,"Contenido: ")]
		[RDFProperty("http://purl.org/dc/elements/1.1/description")]
		public  string Dc_description { get; set;}

		[LABEL(LanguageEnum.es,"Usuario Gnoss")]
		[RDFProperty("http://w3id.org/roh/gnossUser")]
		public  string Roh_gnossUser { get; set;}

		[LABEL(LanguageEnum.es,"Tipo: ")]
		[RDFProperty("http://purl.org/dc/elements/1.1/type")]
		public  string Dc_type { get; set;}

		[LABEL(LanguageEnum.es,"Fuente: ")]
		[RDFProperty("http://purl.org/dc/elements/1.1/source")]
		public  string Dc_source { get; set;}


		internal override void GetProperties()
		{
			base.GetProperties();
			propList.Add(new DateOntologyProperty("dc:date", this.Dc_date));
			propList.Add(new StringOntologyProperty("dc:description", this.Dc_description));
			propList.Add(new StringOntologyProperty("roh:gnossUser", this.Roh_gnossUser));
			propList.Add(new StringOntologyProperty("dc:type", this.Dc_type));
			propList.Add(new StringOntologyProperty("dc:source", this.Dc_source));
		}

		internal override void GetEntities()
		{
			base.GetEntities();
		} 
		public virtual SecondaryResource ToGnossApiResource(ResourceApi resourceAPI,string identificador)
		{
			SecondaryResource resource = new SecondaryResource();
			List<SecondaryEntity> listSecondaryEntity = null;
			GetProperties();
			SecondaryOntology ontology = new SecondaryOntology(resourceAPI.GraphsUrl, resourceAPI.OntologyUrl, "http://w3id.org/roh/Notification", "http://w3id.org/roh/Notification", prefList, propList,identificador,listSecondaryEntity, null);
			resource.SecondaryOntology = ontology;
			AddImages(resource);
			AddFiles(resource);
			return resource;
		}

		public override List<string> ToOntologyGnossTriples(ResourceApi resourceAPI)
		{
			List<string> list = new List<string>();
			AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Notification_{ResourceID}_{ArticleID}", "http://www.w3.org/1999/02/22-rdf-syntax-ns#type", $"<http://w3id.org/roh/Notification>", list, " . ");
			AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Notification_{ResourceID}_{ArticleID}", "http://www.w3.org/2000/01/rdf-schema#label", $"\"http://w3id.org/roh/Notification\"", list, " . ");
			AgregarTripleALista($"{resourceAPI.GraphsUrl}{ResourceID}", "http://gnoss/hasEntidad", $"<{resourceAPI.GraphsUrl}items/Notification_{ResourceID}_{ArticleID}>", list, " . ");
				if(this.Dc_date != null && this.Dc_date != DateTime.MinValue)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Notification_{ResourceID}_{ArticleID}",  "http://purl.org/dc/elements/1.1/date", $"\"{this.Dc_date.ToString("yyyyMMddHHmmss")}\"", list, " . ");
				}
				if(this.Dc_description != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Notification_{ResourceID}_{ArticleID}",  "http://purl.org/dc/elements/1.1/description", $"\"{GenerarTextoSinSaltoDeLinea(this.Dc_description)}\"", list, " . ");
				}
				if(this.Roh_gnossUser != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Notification_{ResourceID}_{ArticleID}",  "http://w3id.org/roh/gnossUser", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_gnossUser)}\"", list, " . ");
				}
				if(this.Dc_type != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Notification_{ResourceID}_{ArticleID}",  "http://purl.org/dc/elements/1.1/type", $"\"{GenerarTextoSinSaltoDeLinea(this.Dc_type)}\"", list, " . ");
				}
				if(this.Dc_source != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Notification_{ResourceID}_{ArticleID}",  "http://purl.org/dc/elements/1.1/source", $"\"{GenerarTextoSinSaltoDeLinea(this.Dc_source)}\"", list, " . ");
				}
			return list;
		}

		public override List<string> ToSearchGraphTriples(ResourceApi resourceAPI)
		{
			List<string> list = new List<string>();
			List<string> listaSearch = new List<string>();
			string search = string.Empty;
				if(this.Dc_date != null && this.Dc_date != DateTime.MinValue)
				{
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "http://purl.org/dc/elements/1.1/date", $"{this.Dc_date.ToString("yyyyMMddHHmmss")}", list, " . ");
				}
				if(this.Dc_description != null)
				{
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "http://purl.org/dc/elements/1.1/description", $"\"{GenerarTextoSinSaltoDeLinea(this.Dc_description).ToLower()}\"", list, " . ");
				}
				if(this.Roh_gnossUser != null)
				{
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "http://w3id.org/roh/gnossUser", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_gnossUser).ToLower()}\"", list, " . ");
				}
				if(this.Dc_type != null)
				{
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "http://purl.org/dc/elements/1.1/type", $"\"{GenerarTextoSinSaltoDeLinea(this.Dc_type).ToLower()}\"", list, " . ");
				}
				if(this.Dc_source != null)
				{
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "http://purl.org/dc/elements/1.1/source", $"\"{GenerarTextoSinSaltoDeLinea(this.Dc_source).ToLower()}\"", list, " . ");
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
			KeyValuePair<Guid, string> valor = new KeyValuePair<Guid, string>();

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
			return $"{resourceAPI.GraphsUrl}items/NotificationOntology_{ResourceID}_{ArticleID}";
		}

		private string GenerarTextoSinSaltoDeLinea(string pTexto)
		{
			return pTexto.Replace("\r\n", " ").Replace("\n", " ").Replace("\r", " ").Replace("\"", "\\\"");
		}

		internal void AddResourceTitle(ComplexOntologyResource resource)
		{
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
