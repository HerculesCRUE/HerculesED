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
using Person = PersonOntology.Person;

namespace PatentOntology
{
	public class BFO_0000023 : GnossOCBase
	{

		public BFO_0000023() : base() { } 

		public BFO_0000023(SemanticEntityModel pSemCmsModel, LanguageEnum idiomaUsuario) : base()
		{
			this.mGNOSSID = pSemCmsModel.Entity.Uri;
			this.mURL = pSemCmsModel.Properties.FirstOrDefault(p => p.PropertyValues.Any(prop => prop.DownloadUrl != null))?.FirstPropertyValue.DownloadUrl;
			this.Foaf_nick = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://xmlns.com/foaf/0.1/nick"));
			this.Vivo_preferredDisplayOrder = GetNumberIntPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://vivoweb.org/ontology/core#preferredDisplayOrder")).Value;
			SemanticPropertyModel propRoh_roleOf = pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/roleOf");
			if(propRoh_roleOf != null && propRoh_roleOf.PropertyValues.Count > 0)
			{
				this.Roh_roleOf = new Person(propRoh_roleOf.PropertyValues[0].RelatedEntity,idiomaUsuario);
			}
		}

		public virtual string RdfType { get { return "http://purl.obolibrary.org/obo/BFO_0000023"; } }
		public virtual string RdfsLabel { get { return "http://purl.obolibrary.org/obo/BFO_0000023"; } }
		public OntologyEntity Entity { get; set; }

		[LABEL(LanguageEnum.es,"Firma")]
		[RDFProperty("http://xmlns.com/foaf/0.1/nick")]
		public  string Foaf_nick { get; set;}

		[LABEL(LanguageEnum.es,"Posición")]
		[RDFProperty("http://vivoweb.org/ontology/core#preferredDisplayOrder")]
		public  int Vivo_preferredDisplayOrder { get; set;}

		[LABEL(LanguageEnum.es,"Persona")]
		[RDFProperty("http://w3id.org/roh/roleOf")]
		[Required]
		public  Person Roh_roleOf  { get; set;} 
		public string IdRoh_roleOf  { get; set;} 


		internal override void GetProperties()
		{
			base.GetProperties();
			propList.Add(new StringOntologyProperty("foaf:nick", this.Foaf_nick));
			propList.Add(new StringOntologyProperty("vivo:preferredDisplayOrder", this.Vivo_preferredDisplayOrder.ToString()));
			propList.Add(new StringOntologyProperty("roh:roleOf", this.IdRoh_roleOf));
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
