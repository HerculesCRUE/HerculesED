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
using TechnologicalResult = TechnologicalresultOntology.TechnologicalResult;

namespace CurriculumvitaeOntology
{
	[ExcludeFromCodeCoverage]
	public class RelatedTechnologicalResult : GnossOCBase
	{

		public RelatedTechnologicalResult() : base() { } 

		public RelatedTechnologicalResult(SemanticEntityModel pSemCmsModel, LanguageEnum idiomaUsuario) : base()
		{
			this.mGNOSSID = pSemCmsModel.Entity.Uri;
			this.mURL = pSemCmsModel.Properties.FirstOrDefault(p => p.PropertyValues.Any(prop => prop.DownloadUrl != null))?.FirstPropertyValue.DownloadUrl;
			SemanticPropertyModel propRoh_relatedTechnologicalResultCV = pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/relatedTechnologicalResultCV");
			if(propRoh_relatedTechnologicalResultCV != null && propRoh_relatedTechnologicalResultCV.PropertyValues.Count > 0)
			{
				this.Roh_relatedTechnologicalResultCV = new RelatedTechnologicalResultCV(propRoh_relatedTechnologicalResultCV.PropertyValues[0].RelatedEntity,idiomaUsuario);
			}
			SemanticPropertyModel propVivo_relatedBy = pSemCmsModel.GetPropertyByPath("http://vivoweb.org/ontology/core#relatedBy");
			if(propVivo_relatedBy != null && propVivo_relatedBy.PropertyValues.Count > 0)
			{
				this.Vivo_relatedBy = new TechnologicalResult(propVivo_relatedBy.PropertyValues[0].RelatedEntity,idiomaUsuario);
			}
			this.Roh_isPublic= GetBooleanPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/isPublic"));
		}

		public virtual string RdfType { get { return "http://w3id.org/roh/RelatedTechnologicalResult"; } }
		public virtual string RdfsLabel { get { return "http://w3id.org/roh/RelatedTechnologicalResult"; } }
		public OntologyEntity Entity { get; set; }

		[LABEL(LanguageEnum.es,"http://w3id.org/roh/relatedTechnologicalResultCV")]
		[RDFProperty("http://w3id.org/roh/relatedTechnologicalResultCV")]
		public  RelatedTechnologicalResultCV Roh_relatedTechnologicalResultCV { get; set;}

		[LABEL(LanguageEnum.es,"http://vivoweb.org/ontology/core#relatedBy")]
		[RDFProperty("http://vivoweb.org/ontology/core#relatedBy")]
		[Required]
		public  TechnologicalResult Vivo_relatedBy  { get; set;} 
		public string IdVivo_relatedBy  { get; set;} 

		[LABEL(LanguageEnum.es,"http://w3id.org/roh/isPublic")]
		[RDFProperty("http://w3id.org/roh/isPublic")]
		public  bool Roh_isPublic { get; set;}


		internal override void GetProperties()
		{
			base.GetProperties();
			propList.Add(new StringOntologyProperty("vivo:relatedBy", this.IdVivo_relatedBy));
			propList.Add(new BoolOntologyProperty("roh:isPublic", this.Roh_isPublic));
		}

		internal override void GetEntities()
		{
			base.GetEntities();
			if(Roh_relatedTechnologicalResultCV!=null){
				Roh_relatedTechnologicalResultCV.GetProperties();
				Roh_relatedTechnologicalResultCV.GetEntities();
				OntologyEntity entityRoh_relatedTechnologicalResultCV = new OntologyEntity("http://w3id.org/roh/RelatedTechnologicalResultCV", "http://w3id.org/roh/RelatedTechnologicalResultCV", "roh:relatedTechnologicalResultCV", Roh_relatedTechnologicalResultCV.propList, Roh_relatedTechnologicalResultCV.entList);
				entList.Add(entityRoh_relatedTechnologicalResultCV);
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


	}
}
