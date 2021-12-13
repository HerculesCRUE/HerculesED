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

namespace DocumentOntology
{
	[ExcludeFromCodeCoverage]
	public class PublicationMetric : GnossOCBase
	{

		public PublicationMetric() : base() { } 

		public PublicationMetric(SemanticEntityModel pSemCmsModel, LanguageEnum idiomaUsuario) : base()
		{
			this.mGNOSSID = pSemCmsModel.Entity.Uri;
			this.mURL = pSemCmsModel.Properties.FirstOrDefault(p => p.PropertyValues.Any(prop => prop.DownloadUrl != null))?.FirstPropertyValue.DownloadUrl;
			this.Roh_metricName = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/metricName"));
			this.Roh_citationCount = GetNumberIntPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/citationCount")).Value;
		}

		public virtual string RdfType { get { return "http://w3id.org/roh/PublicationMetric"; } }
		public virtual string RdfsLabel { get { return "http://w3id.org/roh/PublicationMetric"; } }
		public OntologyEntity Entity { get; set; }

		[LABEL(LanguageEnum.es,"http://w3id.org/roh/metricName")]
		[RDFProperty("http://w3id.org/roh/metricName")]
		public  string Roh_metricName { get; set;}

		[LABEL(LanguageEnum.es,"http://w3id.org/roh/citationCount")]
		[RDFProperty("http://w3id.org/roh/citationCount")]
		public  int Roh_citationCount { get; set;}


		internal override void GetProperties()
		{
			base.GetProperties();
			propList.Add(new StringOntologyProperty("roh:metricName", this.Roh_metricName));
			propList.Add(new StringOntologyProperty("roh:citationCount", this.Roh_citationCount.ToString()));
		}

		internal override void GetEntities()
		{
			base.GetEntities();
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


	}
}
