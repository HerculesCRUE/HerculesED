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
using AccessSystemActivity = AccesssystemactivityOntology.AccessSystemActivity;

namespace CurriculumvitaeOntology
{
	public class RelatedActivityManagementCV : GnossOCBase
	{

		public RelatedActivityManagementCV() : base() { } 

		public RelatedActivityManagementCV(SemanticEntityModel pSemCmsModel, LanguageEnum idiomaUsuario) : base()
		{
			this.mGNOSSID = pSemCmsModel.Entity.Uri;
			this.mURL = pSemCmsModel.Properties.FirstOrDefault(p => p.PropertyValues.Any(prop => prop.DownloadUrl != null))?.FirstPropertyValue.DownloadUrl;
			SemanticPropertyModel propRoh_accessSystem = pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/accessSystem");
			if(propRoh_accessSystem != null && propRoh_accessSystem.PropertyValues.Count > 0)
			{
				this.Roh_accessSystem = new AccessSystemActivity(propRoh_accessSystem.PropertyValues[0].RelatedEntity,idiomaUsuario);
			}
			this.Roh_hasKnowledgeArea = new List<CategoryPath>();
			SemanticPropertyModel propRoh_hasKnowledgeArea = pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/hasKnowledgeArea");
			if(propRoh_hasKnowledgeArea != null && propRoh_hasKnowledgeArea.PropertyValues.Count > 0)
			{
				foreach (SemanticPropertyModel.PropertyValue propValue in propRoh_hasKnowledgeArea.PropertyValues)
				{
					if(propValue.RelatedEntity!=null){
						CategoryPath roh_hasKnowledgeArea = new CategoryPath(propValue.RelatedEntity,idiomaUsuario);
						this.Roh_hasKnowledgeArea.Add(roh_hasKnowledgeArea);
					}
				}
			}
			this.Roh_specificTasks = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/specificTasks"));
			this.Roh_durationDays = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/durationDays"));
			this.Vivo_start= GetDateValuePropertySemCms(pSemCmsModel.GetPropertyByPath("http://vivoweb.org/ontology/core#start"));
			this.Roh_goals = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/goals"));
			this.Roh_performedTasks = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/performedTasks"));
			this.Roh_accessSystemOther = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/accessSystemOther"));
			this.Roh_durationMonths = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/durationMonths"));
			this.Roh_durationYears = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/durationYears"));
			this.Vivo_end= GetDateValuePropertySemCms(pSemCmsModel.GetPropertyByPath("http://vivoweb.org/ontology/core#end"));
		}

		public virtual string RdfType { get { return "http://w3id.org/roh/RelatedActivityManagementCV"; } }
		public virtual string RdfsLabel { get { return "http://w3id.org/roh/RelatedActivityManagementCV"; } }
		public OntologyEntity Entity { get; set; }

		[LABEL(LanguageEnum.es,"http://w3id.org/roh/accessSystem")]
		[RDFProperty("http://w3id.org/roh/accessSystem")]
		public  AccessSystemActivity Roh_accessSystem  { get; set;} 
		public string IdRoh_accessSystem  { get; set;} 

		[LABEL(LanguageEnum.es,"http://w3id.org/roh/hasKnowledgeArea")]
		[RDFProperty("http://w3id.org/roh/hasKnowledgeArea")]
		public  List<CategoryPath> Roh_hasKnowledgeArea { get; set;}

		[LABEL(LanguageEnum.es,"http://w3id.org/roh/specificTasks")]
		[RDFProperty("http://w3id.org/roh/specificTasks")]
		public  string Roh_specificTasks { get; set;}

		[LABEL(LanguageEnum.es,"http://w3id.org/roh/durationDays")]
		[RDFProperty("http://w3id.org/roh/durationDays")]
		public  string Roh_durationDays { get; set;}

		[LABEL(LanguageEnum.es,"http://vivoweb.org/ontology/core#start")]
		[RDFProperty("http://vivoweb.org/ontology/core#start")]
		public  DateTime? Vivo_start { get; set;}

		[LABEL(LanguageEnum.es,"http://w3id.org/roh/goals")]
		[RDFProperty("http://w3id.org/roh/goals")]
		public  string Roh_goals { get; set;}

		[LABEL(LanguageEnum.es,"http://w3id.org/roh/performedTasks")]
		[RDFProperty("http://w3id.org/roh/performedTasks")]
		public  string Roh_performedTasks { get; set;}

		[LABEL(LanguageEnum.es,"http://w3id.org/roh/accessSystemOther")]
		[RDFProperty("http://w3id.org/roh/accessSystemOther")]
		public  string Roh_accessSystemOther { get; set;}

		[LABEL(LanguageEnum.es,"http://w3id.org/roh/durationMonths")]
		[RDFProperty("http://w3id.org/roh/durationMonths")]
		public  string Roh_durationMonths { get; set;}

		[LABEL(LanguageEnum.es,"http://w3id.org/roh/durationYears")]
		[RDFProperty("http://w3id.org/roh/durationYears")]
		public  string Roh_durationYears { get; set;}

		[LABEL(LanguageEnum.es,"http://vivoweb.org/ontology/core#end")]
		[RDFProperty("http://vivoweb.org/ontology/core#end")]
		public  DateTime? Vivo_end { get; set;}


		internal override void GetProperties()
		{
			base.GetProperties();
			propList.Add(new StringOntologyProperty("roh:accessSystem", this.IdRoh_accessSystem));
			propList.Add(new StringOntologyProperty("roh:specificTasks", this.Roh_specificTasks));
			propList.Add(new StringOntologyProperty("roh:durationDays", this.Roh_durationDays));
			if (this.Vivo_start.HasValue){
				propList.Add(new DateOntologyProperty("vivo:start", this.Vivo_start.Value));
				}
			propList.Add(new StringOntologyProperty("roh:goals", this.Roh_goals));
			propList.Add(new StringOntologyProperty("roh:performedTasks", this.Roh_performedTasks));
			propList.Add(new StringOntologyProperty("roh:accessSystemOther", this.Roh_accessSystemOther));
			propList.Add(new StringOntologyProperty("roh:durationMonths", this.Roh_durationMonths));
			propList.Add(new StringOntologyProperty("roh:durationYears", this.Roh_durationYears));
			if (this.Vivo_end.HasValue){
				propList.Add(new DateOntologyProperty("vivo:end", this.Vivo_end.Value));
				}
		}

		internal override void GetEntities()
		{
			base.GetEntities();
			if(Roh_hasKnowledgeArea!=null){
				foreach(CategoryPath prop in Roh_hasKnowledgeArea){
					prop.GetProperties();
					prop.GetEntities();
					OntologyEntity entityCategoryPath = new OntologyEntity("http://w3id.org/roh/CategoryPath", "http://w3id.org/roh/CategoryPath", "roh:hasKnowledgeArea", prop.propList, prop.entList);
				entList.Add(entityCategoryPath);
				prop.Entity= entityCategoryPath;
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
