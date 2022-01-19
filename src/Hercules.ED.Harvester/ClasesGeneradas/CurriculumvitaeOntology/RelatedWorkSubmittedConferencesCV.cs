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
using ParticipationTypeDocument = ParticipationtypedocumentOntology.ParticipationTypeDocument;
using EventInscriptionType = EventinscriptiontypeOntology.EventInscriptionType;

namespace CurriculumvitaeOntology
{
	public class RelatedWorkSubmittedConferencesCV : GnossOCBase
	{

		public RelatedWorkSubmittedConferencesCV() : base() { } 

		public RelatedWorkSubmittedConferencesCV(SemanticEntityModel pSemCmsModel, LanguageEnum idiomaUsuario) : base()
		{
			this.mGNOSSID = pSemCmsModel.Entity.Uri;
			this.mURL = pSemCmsModel.Properties.FirstOrDefault(p => p.PropertyValues.Any(prop => prop.DownloadUrl != null))?.FirstPropertyValue.DownloadUrl;
			SemanticPropertyModel propRoh_participationType = pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/participationType");
			if(propRoh_participationType != null && propRoh_participationType.PropertyValues.Count > 0)
			{
				this.Roh_participationType = new ParticipationTypeDocument(propRoh_participationType.PropertyValues[0].RelatedEntity,idiomaUsuario);
			}
			SemanticPropertyModel propRoh_inscriptionType = pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/inscriptionType");
			if(propRoh_inscriptionType != null && propRoh_inscriptionType.PropertyValues.Count > 0)
			{
				this.Roh_inscriptionType = new EventInscriptionType(propRoh_inscriptionType.PropertyValues[0].RelatedEntity,idiomaUsuario);
			}
			this.Roh_correspondingAuthor= GetBooleanPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/correspondingAuthor"));
			this.Roh_inscriptionTypeOther = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/inscriptionTypeOther"));
		}

		public virtual string RdfType { get { return "http://w3id.org/roh/RelatedWorkSubmittedConferencesCV"; } }
		public virtual string RdfsLabel { get { return "http://w3id.org/roh/RelatedWorkSubmittedConferencesCV"; } }
		public OntologyEntity Entity { get; set; }

		[LABEL(LanguageEnum.es,"http://w3id.org/roh/participationType")]
		[RDFProperty("http://w3id.org/roh/participationType")]
		public  ParticipationTypeDocument Roh_participationType  { get; set;} 
		public string IdRoh_participationType  { get; set;} 

		[LABEL(LanguageEnum.es,"http://w3id.org/roh/inscriptionType")]
		[RDFProperty("http://w3id.org/roh/inscriptionType")]
		public  EventInscriptionType Roh_inscriptionType  { get; set;} 
		public string IdRoh_inscriptionType  { get; set;} 

		[LABEL(LanguageEnum.es,"http://w3id.org/roh/correspondingAuthor")]
		[RDFProperty("http://w3id.org/roh/correspondingAuthor")]
		public  bool Roh_correspondingAuthor { get; set;}

		[LABEL(LanguageEnum.es,"http://w3id.org/roh/inscriptionTypeOther")]
		[RDFProperty("http://w3id.org/roh/inscriptionTypeOther")]
		public  string Roh_inscriptionTypeOther { get; set;}


		internal override void GetProperties()
		{
			base.GetProperties();
			propList.Add(new StringOntologyProperty("roh:participationType", this.IdRoh_participationType));
			propList.Add(new StringOntologyProperty("roh:inscriptionType", this.IdRoh_inscriptionType));
			propList.Add(new BoolOntologyProperty("roh:correspondingAuthor", this.Roh_correspondingAuthor));
			propList.Add(new StringOntologyProperty("roh:inscriptionTypeOther", this.Roh_inscriptionTypeOther));
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
