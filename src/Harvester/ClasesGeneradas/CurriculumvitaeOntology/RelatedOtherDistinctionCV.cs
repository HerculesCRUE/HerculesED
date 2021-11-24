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

namespace CurriculumvitaeOntology
{
	public class RelatedOtherDistinctionCV : GnossOCBase
	{

		public RelatedOtherDistinctionCV() : base() { } 

		public RelatedOtherDistinctionCV(SemanticEntityModel pSemCmsModel, LanguageEnum idiomaUsuario) : base()
		{
			this.mGNOSSID = pSemCmsModel.Entity.Uri;
			this.mURL = pSemCmsModel.Properties.FirstOrDefault(p => p.PropertyValues.Any(prop => prop.DownloadUrl != null))?.FirstPropertyValue.DownloadUrl;
			this.Vivo_start = new List<DateTime>();
			SemanticPropertyModel propVivo_start = pSemCmsModel.GetPropertyByPath("http://vivoweb.org/ontology/core#start");
			if (propVivo_start != null && propVivo_start.PropertyValues.Count > 0)
			{
				foreach (SemanticPropertyModel.PropertyValue propValue in propVivo_start.PropertyValues)
				{
					DateTime fecha = new DateTime();
					DateTime.TryParse(propValue.Value,out fecha);
					this.Vivo_start.Add(fecha);
				}
			}
			this.Vivo_end = new List<DateTime>();
			SemanticPropertyModel propVivo_end = pSemCmsModel.GetPropertyByPath("http://vivoweb.org/ontology/core#end");
			if (propVivo_end != null && propVivo_end.PropertyValues.Count > 0)
			{
				foreach (SemanticPropertyModel.PropertyValue propValue in propVivo_end.PropertyValues)
				{
					DateTime fecha = new DateTime();
					DateTime.TryParse(propValue.Value,out fecha);
					this.Vivo_end.Add(fecha);
				}
			}
		}

		public virtual string RdfType { get { return "http://w3id.org/roh/RelatedOtherDistinctionCV"; } }
		public virtual string RdfsLabel { get { return "http://w3id.org/roh/RelatedOtherDistinctionCV"; } }
		public OntologyEntity Entity { get; set; }

		[LABEL(LanguageEnum.es,"http://vivoweb.org/ontology/core#start")]
		[RDFProperty("http://vivoweb.org/ontology/core#start")]
		public  List<DateTime> Vivo_start { get; set;}

		[LABEL(LanguageEnum.es,"http://vivoweb.org/ontology/core#end")]
		[RDFProperty("http://vivoweb.org/ontology/core#end")]
		public  List<DateTime> Vivo_end { get; set;}


		internal override void GetProperties()
		{
			base.GetProperties();
			foreach (DateTime fecha in this.Vivo_start){
				propList.Add(new DateOntologyProperty("vivo:start", fecha));
			}
			foreach (DateTime fecha in this.Vivo_end){
				propList.Add(new DateOntologyProperty("vivo:end", fecha));
			}
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
