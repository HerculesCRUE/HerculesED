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
using ReferenceSource = ReferencesourceOntology.ReferenceSource;

namespace MaindocumentOntology
{
	[ExcludeFromCodeCoverage]
	public class ImpactIndex : GnossOCBase
	{

		public ImpactIndex() : base() { } 

		public ImpactIndex(SemanticEntityModel pSemCmsModel, LanguageEnum idiomaUsuario) : base()
		{
			this.mGNOSSID = pSemCmsModel.Entity.Uri;
			this.mURL = pSemCmsModel.Properties.FirstOrDefault(p => p.PropertyValues.Any(prop => prop.DownloadUrl != null))?.FirstPropertyValue.DownloadUrl;
			this.Roh_impactCategory = new List<ImpactCategory>();
			SemanticPropertyModel propRoh_impactCategory = pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/impactCategory");
			if(propRoh_impactCategory != null && propRoh_impactCategory.PropertyValues.Count > 0)
			{
				foreach (SemanticPropertyModel.PropertyValue propValue in propRoh_impactCategory.PropertyValues)
				{
					if(propValue.RelatedEntity!=null){
						ImpactCategory roh_impactCategory = new ImpactCategory(propValue.RelatedEntity,idiomaUsuario);
						this.Roh_impactCategory.Add(roh_impactCategory);
					}
				}
			}
			this.Roh_impactSourceOther = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/impactSourceOther"));
			this.Roh_impactIndexInYear = GetNumberFloatPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/impactIndexInYear")).Value;
			this.Roh_year = GetNumberIntPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/year")).Value;
			SemanticPropertyModel propRoh_impactSource = pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/impactSource");
			if(propRoh_impactSource != null && propRoh_impactSource.PropertyValues.Count > 0)
			{
				this.Roh_impactSource = new ReferenceSource(propRoh_impactSource.PropertyValues[0].RelatedEntity,idiomaUsuario);
			}
		}

		public virtual string RdfType { get { return "http://w3id.org/roh/ImpactIndex"; } }
		public virtual string RdfsLabel { get { return "http://w3id.org/roh/ImpactIndex"; } }
		public OntologyEntity Entity { get; set; }

		[RDFProperty("http://w3id.org/roh/impactCategory")]
		public  List<ImpactCategory> Roh_impactCategory { get; set;}

		[LABEL(LanguageEnum.es,"Fuente de impacto, otros")]
		[RDFProperty("http://w3id.org/roh/impactSourceOther")]
		public  string Roh_impactSourceOther { get; set;}

		[LABEL(LanguageEnum.es,"Índice de impacto en año de publicación")]
		[RDFProperty("http://w3id.org/roh/impactIndexInYear")]
		public  float Roh_impactIndexInYear { get; set;}

		[LABEL(LanguageEnum.es,"Año del índice de impacto")]
		[RDFProperty("http://w3id.org/roh/year")]
		public  int Roh_year { get; set;}

		[LABEL(LanguageEnum.es,"Fuente de impacto")]
		[RDFProperty("http://w3id.org/roh/impactSource")]
		[Required]
		public  ReferenceSource Roh_impactSource  { get; set;} 
		public string IdRoh_impactSource  { get; set;} 


		internal override void GetProperties()
		{
			base.GetProperties();
			propList.Add(new StringOntologyProperty("roh:impactSourceOther", this.Roh_impactSourceOther));
			propList.Add(new StringOntologyProperty("roh:impactIndexInYear", this.Roh_impactIndexInYear.ToString()));
			propList.Add(new StringOntologyProperty("roh:year", this.Roh_year.ToString()));
			propList.Add(new StringOntologyProperty("roh:impactSource", this.IdRoh_impactSource));
		}

		internal override void GetEntities()
		{
			base.GetEntities();
			if(Roh_impactCategory!=null){
				foreach(ImpactCategory prop in Roh_impactCategory){
					prop.GetProperties();
					prop.GetEntities();
					OntologyEntity entityImpactCategory = new OntologyEntity("http://w3id.org/roh/ImpactCategory", "http://w3id.org/roh/ImpactCategory", "roh:impactCategory", prop.propList, prop.entList);
				entList.Add(entityImpactCategory);
				prop.Entity= entityImpactCategory;
				}
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
