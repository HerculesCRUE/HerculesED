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

namespace ProjectOntology
{
	public class Funding : GnossOCBase
	{

		public Funding() : base() { } 

		public Funding(SemanticEntityModel pSemCmsModel, LanguageEnum idiomaUsuario) : base()
		{
			this.mGNOSSID = pSemCmsModel.Entity.Uri;
			this.mURL = pSemCmsModel.Properties.FirstOrDefault(p => p.PropertyValues.Any(prop => prop.DownloadUrl != null))?.FirstPropertyValue.DownloadUrl;
			this.Roh_fundedBy = new List<FundingProgram>();
			SemanticPropertyModel propRoh_fundedBy = pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/fundedBy");
			if(propRoh_fundedBy != null && propRoh_fundedBy.PropertyValues.Count > 0)
			{
				foreach (SemanticPropertyModel.PropertyValue propValue in propRoh_fundedBy.PropertyValues)
				{
					if(propValue.RelatedEntity!=null){
						FundingProgram roh_fundedBy = new FundingProgram(propValue.RelatedEntity,idiomaUsuario);
						this.Roh_fundedBy.Add(roh_fundedBy);
					}
				}
			}
			this.Roh_mixedPercentage = GetNumberFloatPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/mixedPercentage"));
			this.Roh_creditPercentage = GetNumberFloatPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/creditPercentage"));
			this.Vivo_identifier = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://vivoweb.org/ontology/core#identifier"));
			this.Roh_grantsPercentage = GetNumberFloatPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/grantsPercentage"));
			this.Roh_monetaryAmount = GetNumberFloatPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/monetaryAmount"));
		}

		public virtual string RdfType { get { return "http://w3id.org/roh/Funding"; } }
		public virtual string RdfsLabel { get { return "http://w3id.org/roh/Funding"; } }
		public OntologyEntity Entity { get; set; }

		[RDFProperty("http://w3id.org/roh/fundedBy")]
		public  List<FundingProgram> Roh_fundedBy { get; set;}

		[LABEL(LanguageEnum.es,"http://w3id.org/roh/mixedPercentage")]
		[RDFProperty("http://w3id.org/roh/mixedPercentage")]
		public  float? Roh_mixedPercentage { get; set;}

		[LABEL(LanguageEnum.es,"http://w3id.org/roh/creditPercentage")]
		[RDFProperty("http://w3id.org/roh/creditPercentage")]
		public  float? Roh_creditPercentage { get; set;}

		[LABEL(LanguageEnum.es,"http://vivoweb.org/ontology/core#identifier")]
		[RDFProperty("http://vivoweb.org/ontology/core#identifier")]
		public  string Vivo_identifier { get; set;}

		[LABEL(LanguageEnum.es,"http://w3id.org/roh/grantsPercentage")]
		[RDFProperty("http://w3id.org/roh/grantsPercentage")]
		public  float? Roh_grantsPercentage { get; set;}

		[LABEL(LanguageEnum.es,"http://w3id.org/roh/monetaryAmount")]
		[RDFProperty("http://w3id.org/roh/monetaryAmount")]
		public  float? Roh_monetaryAmount { get; set;}


		internal override void GetProperties()
		{
			base.GetProperties();
			propList.Add(new StringOntologyProperty("roh:mixedPercentage", this.Roh_mixedPercentage.ToString()));
			propList.Add(new StringOntologyProperty("roh:creditPercentage", this.Roh_creditPercentage.ToString()));
			propList.Add(new StringOntologyProperty("vivo:identifier", this.Vivo_identifier));
			propList.Add(new StringOntologyProperty("roh:grantsPercentage", this.Roh_grantsPercentage.ToString()));
			propList.Add(new StringOntologyProperty("roh:monetaryAmount", this.Roh_monetaryAmount.ToString()));
		}

		internal override void GetEntities()
		{
			base.GetEntities();
			if(Roh_fundedBy!=null){
				foreach(FundingProgram prop in Roh_fundedBy){
					prop.GetProperties();
					prop.GetEntities();
					OntologyEntity entityFundingProgram = new OntologyEntity("http://w3id.org/roh/FundingProgram", "http://w3id.org/roh/FundingProgram", "roh:fundedBy", prop.propList, prop.entList);
				entList.Add(entityFundingProgram);
				prop.Entity= entityFundingProgram;
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
