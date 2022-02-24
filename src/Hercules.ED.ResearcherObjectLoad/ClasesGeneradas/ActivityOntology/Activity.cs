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
using Organization = OrganizationOntology.Organization;
using Feature = FeatureOntology.Feature;
using ManagementTypeActivity = ManagementtypeactivityOntology.ManagementTypeActivity;
using OrganizationType = OrganizationtypeOntology.OrganizationType;
using TargetGroupProfile = TargetgroupprofileOntology.TargetGroupProfile;
using ParticipationTypeActivity = ParticipationtypeactivityOntology.ParticipationTypeActivity;
using AccessSystemActivity = AccesssystemactivityOntology.AccessSystemActivity;
using GeographicRegion = GeographicregionOntology.GeographicRegion;
using ActivityModality = ActivitymodalityOntology.ActivityModality;

namespace ActivityOntology
{
	[ExcludeFromCodeCoverage]
	public class Activity : GnossOCBase
	{

		public Activity() : base() { } 

		public Activity(SemanticResourceModel pSemCmsModel, LanguageEnum idiomaUsuario) : base()
		{
			this.mGNOSSID = pSemCmsModel.RootEntities[0].Entity.Uri;
			SemanticPropertyModel propRoh_promotedBy = pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/promotedBy");
			if(propRoh_promotedBy != null && propRoh_promotedBy.PropertyValues.Count > 0)
			{
				this.Roh_promotedBy = new Organization(propRoh_promotedBy.PropertyValues[0].RelatedEntity,idiomaUsuario);
			}
			this.Roh_representedEntityHasRegion = new List<Feature>();
			SemanticPropertyModel propRoh_representedEntityHasRegion = pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/representedEntityHasRegion");
			if(propRoh_representedEntityHasRegion != null && propRoh_representedEntityHasRegion.PropertyValues.Count > 0)
			{
				foreach (SemanticPropertyModel.PropertyValue propValue in propRoh_representedEntityHasRegion.PropertyValues)
				{
					if(propValue.RelatedEntity!=null){
						Feature roh_representedEntityHasRegion = new Feature(propValue.RelatedEntity,idiomaUsuario);
						this.Roh_representedEntityHasRegion.Add(roh_representedEntityHasRegion);
					}
				}
			}
			SemanticPropertyModel propRoh_managementType = pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/managementType");
			if(propRoh_managementType != null && propRoh_managementType.PropertyValues.Count > 0)
			{
				this.Roh_managementType = new ManagementTypeActivity(propRoh_managementType.PropertyValues[0].RelatedEntity,idiomaUsuario);
			}
			SemanticPropertyModel propVcard_hasCountryName = pSemCmsModel.GetPropertyByPath("https://www.w3.org/2006/vcard/ns#hasCountryName");
			if(propVcard_hasCountryName != null && propVcard_hasCountryName.PropertyValues.Count > 0)
			{
				this.Vcard_hasCountryName = new Feature(propVcard_hasCountryName.PropertyValues[0].RelatedEntity,idiomaUsuario);
			}
			SemanticPropertyModel propRoh_conductedByType = pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/conductedByType");
			if(propRoh_conductedByType != null && propRoh_conductedByType.PropertyValues.Count > 0)
			{
				this.Roh_conductedByType = new OrganizationType(propRoh_conductedByType.PropertyValues[0].RelatedEntity,idiomaUsuario);
			}
			SemanticPropertyModel propRoh_targetGroupProfile = pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/targetGroupProfile");
			if(propRoh_targetGroupProfile != null && propRoh_targetGroupProfile.PropertyValues.Count > 0)
			{
				this.Roh_targetGroupProfile = new TargetGroupProfile(propRoh_targetGroupProfile.PropertyValues[0].RelatedEntity,idiomaUsuario);
			}
			SemanticPropertyModel propRoh_participationType = pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/participationType");
			if(propRoh_participationType != null && propRoh_participationType.PropertyValues.Count > 0)
			{
				this.Roh_participationType = new ParticipationTypeActivity(propRoh_participationType.PropertyValues[0].RelatedEntity,idiomaUsuario);
			}
			SemanticPropertyModel propRoh_promotedByHasCountryName = pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/promotedByHasCountryName");
			if(propRoh_promotedByHasCountryName != null && propRoh_promotedByHasCountryName.PropertyValues.Count > 0)
			{
				this.Roh_promotedByHasCountryName = new Feature(propRoh_promotedByHasCountryName.PropertyValues[0].RelatedEntity,idiomaUsuario);
			}
			SemanticPropertyModel propRoh_promotedByType = pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/promotedByType");
			if(propRoh_promotedByType != null && propRoh_promotedByType.PropertyValues.Count > 0)
			{
				this.Roh_promotedByType = new OrganizationType(propRoh_promotedByType.PropertyValues[0].RelatedEntity,idiomaUsuario);
			}
			SemanticPropertyModel propRoh_accessSystemActivity = pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/accessSystemActivity");
			if(propRoh_accessSystemActivity != null && propRoh_accessSystemActivity.PropertyValues.Count > 0)
			{
				this.Roh_accessSystemActivity = new AccessSystemActivity(propRoh_accessSystemActivity.PropertyValues[0].RelatedEntity,idiomaUsuario);
			}
			SemanticPropertyModel propRoh_promotedByHasRegion = pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/promotedByHasRegion");
			if(propRoh_promotedByHasRegion != null && propRoh_promotedByHasRegion.PropertyValues.Count > 0)
			{
				this.Roh_promotedByHasRegion = new Feature(propRoh_promotedByHasRegion.PropertyValues[0].RelatedEntity,idiomaUsuario);
			}
			SemanticPropertyModel propRoh_conductedBy = pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/conductedBy");
			if(propRoh_conductedBy != null && propRoh_conductedBy.PropertyValues.Count > 0)
			{
				this.Roh_conductedBy = new Organization(propRoh_conductedBy.PropertyValues[0].RelatedEntity,idiomaUsuario);
			}
			SemanticPropertyModel propVcard_hasRegion = pSemCmsModel.GetPropertyByPath("https://www.w3.org/2006/vcard/ns#hasRegion");
			if(propVcard_hasRegion != null && propVcard_hasRegion.PropertyValues.Count > 0)
			{
				this.Vcard_hasRegion = new Feature(propVcard_hasRegion.PropertyValues[0].RelatedEntity,idiomaUsuario);
			}
			SemanticPropertyModel propVivo_geographicFocus = pSemCmsModel.GetPropertyByPath("http://vivoweb.org/ontology/core#geographicFocus");
			if(propVivo_geographicFocus != null && propVivo_geographicFocus.PropertyValues.Count > 0)
			{
				this.Vivo_geographicFocus = new GeographicRegion(propVivo_geographicFocus.PropertyValues[0].RelatedEntity,idiomaUsuario);
			}
			this.Roh_representedEntityHasCountryName = new List<Feature>();
			SemanticPropertyModel propRoh_representedEntityHasCountryName = pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/representedEntityHasCountryName");
			if(propRoh_representedEntityHasCountryName != null && propRoh_representedEntityHasCountryName.PropertyValues.Count > 0)
			{
				foreach (SemanticPropertyModel.PropertyValue propValue in propRoh_representedEntityHasCountryName.PropertyValues)
				{
					if(propValue.RelatedEntity!=null){
						Feature roh_representedEntityHasCountryName = new Feature(propValue.RelatedEntity,idiomaUsuario);
						this.Roh_representedEntityHasCountryName.Add(roh_representedEntityHasCountryName);
					}
				}
			}
			SemanticPropertyModel propRoh_activityModality = pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/activityModality");
			if(propRoh_activityModality != null && propRoh_activityModality.PropertyValues.Count > 0)
			{
				this.Roh_activityModality = new ActivityModality(propRoh_activityModality.PropertyValues[0].RelatedEntity,idiomaUsuario);
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
			this.Roh_representedEntity = new List<Organization>();
			SemanticPropertyModel propRoh_representedEntity = pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/representedEntity");
			if(propRoh_representedEntity != null && propRoh_representedEntity.PropertyValues.Count > 0)
			{
				foreach (SemanticPropertyModel.PropertyValue propValue in propRoh_representedEntity.PropertyValues)
				{
					if(propValue.RelatedEntity!=null){
						Organization roh_representedEntity = new Organization(propValue.RelatedEntity,idiomaUsuario);
						this.Roh_representedEntity.Add(roh_representedEntity);
					}
				}
			}
			this.Roh_representedEntityType = new List<OrganizationType>();
			SemanticPropertyModel propRoh_representedEntityType = pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/representedEntityType");
			if(propRoh_representedEntityType != null && propRoh_representedEntityType.PropertyValues.Count > 0)
			{
				foreach (SemanticPropertyModel.PropertyValue propValue in propRoh_representedEntityType.PropertyValues)
				{
					if(propValue.RelatedEntity!=null){
						OrganizationType roh_representedEntityType = new OrganizationType(propValue.RelatedEntity,idiomaUsuario);
						this.Roh_representedEntityType.Add(roh_representedEntityType);
					}
				}
			}
			this.Roh_promotedByLocality = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/promotedByLocality"));
			this.Roh_geographicFocusOther = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/geographicFocusOther"));
			this.Roh_durationDays = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/durationDays"));
			this.Vivo_start= GetDateValuePropertySemCms(pSemCmsModel.GetPropertyByPath("http://vivoweb.org/ontology/core#start"));
			this.Roh_accessSystemActivityOther = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/accessSystemActivityOther"));
			this.Roh_frequency = GetNumberIntPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/frequency"));
			this.Roh_goals = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/goals"));
			this.Roh_profesionalCategory = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/profesionalCategory"));
			this.Vcard_locality = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("https://www.w3.org/2006/vcard/ns#locality"));
			this.Roh_conductedByTitle = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/conductedByTitle"));
			this.Roh_representedEntityLocality = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/representedEntityLocality"));
			this.Roh_concreteFunctions = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/concreteFunctions"));
			this.Roh_promotedByTitle = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/promotedByTitle"));
			this.Dc_type = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://purl.org/dc/elements/1.1/type"));
			this.Roh_personNumber = GetNumberIntPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/personNumber"));
			this.Roh_managementTypeOther = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/managementTypeOther"));
			this.Roh_durationMonths = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/durationMonths"));
			this.Roh_conductedByTypeOther = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/conductedByTypeOther"));
			this.Roh_averageAnnualBudget = GetNumberFloatPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/averageAnnualBudget"));
			this.Roh_participationTypeOther = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/participationTypeOther"));
			this.Roh_promotedByTypeOther = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/promotedByTypeOther"));
			this.Roh_durationYears = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/durationYears"));
			this.Roh_representedEntityTypeOther = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/representedEntityTypeOther"));
			this.Roh_representedEntityTitle = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/representedEntityTitle"));
			this.Roh_crisIdentifier = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/crisIdentifier"));
			this.Roh_attendants = GetNumberIntPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/attendants"));
			this.Vivo_end= GetDateValuePropertySemCms(pSemCmsModel.GetPropertyByPath("http://vivoweb.org/ontology/core#end"));
			this.Roh_activityModalityOther = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/activityModalityOther"));
			this.Roh_functions = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/functions"));
			this.Roh_classificationCVN = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/classificationCVN"));
			this.Roh_title = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/title"));
		}

		public Activity(SemanticEntityModel pSemCmsModel, LanguageEnum idiomaUsuario) : base()
		{
			this.mGNOSSID = pSemCmsModel.Entity.Uri;
			this.mURL = pSemCmsModel.Properties.FirstOrDefault(p => p.PropertyValues.Any(prop => prop.DownloadUrl != null))?.FirstPropertyValue.DownloadUrl;
			SemanticPropertyModel propRoh_promotedBy = pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/promotedBy");
			if(propRoh_promotedBy != null && propRoh_promotedBy.PropertyValues.Count > 0)
			{
				this.Roh_promotedBy = new Organization(propRoh_promotedBy.PropertyValues[0].RelatedEntity,idiomaUsuario);
			}
			this.Roh_representedEntityHasRegion = new List<Feature>();
			SemanticPropertyModel propRoh_representedEntityHasRegion = pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/representedEntityHasRegion");
			if(propRoh_representedEntityHasRegion != null && propRoh_representedEntityHasRegion.PropertyValues.Count > 0)
			{
				foreach (SemanticPropertyModel.PropertyValue propValue in propRoh_representedEntityHasRegion.PropertyValues)
				{
					if(propValue.RelatedEntity!=null){
						Feature roh_representedEntityHasRegion = new Feature(propValue.RelatedEntity,idiomaUsuario);
						this.Roh_representedEntityHasRegion.Add(roh_representedEntityHasRegion);
					}
				}
			}
			SemanticPropertyModel propRoh_managementType = pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/managementType");
			if(propRoh_managementType != null && propRoh_managementType.PropertyValues.Count > 0)
			{
				this.Roh_managementType = new ManagementTypeActivity(propRoh_managementType.PropertyValues[0].RelatedEntity,idiomaUsuario);
			}
			SemanticPropertyModel propVcard_hasCountryName = pSemCmsModel.GetPropertyByPath("https://www.w3.org/2006/vcard/ns#hasCountryName");
			if(propVcard_hasCountryName != null && propVcard_hasCountryName.PropertyValues.Count > 0)
			{
				this.Vcard_hasCountryName = new Feature(propVcard_hasCountryName.PropertyValues[0].RelatedEntity,idiomaUsuario);
			}
			SemanticPropertyModel propRoh_conductedByType = pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/conductedByType");
			if(propRoh_conductedByType != null && propRoh_conductedByType.PropertyValues.Count > 0)
			{
				this.Roh_conductedByType = new OrganizationType(propRoh_conductedByType.PropertyValues[0].RelatedEntity,idiomaUsuario);
			}
			SemanticPropertyModel propRoh_targetGroupProfile = pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/targetGroupProfile");
			if(propRoh_targetGroupProfile != null && propRoh_targetGroupProfile.PropertyValues.Count > 0)
			{
				this.Roh_targetGroupProfile = new TargetGroupProfile(propRoh_targetGroupProfile.PropertyValues[0].RelatedEntity,idiomaUsuario);
			}
			SemanticPropertyModel propRoh_participationType = pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/participationType");
			if(propRoh_participationType != null && propRoh_participationType.PropertyValues.Count > 0)
			{
				this.Roh_participationType = new ParticipationTypeActivity(propRoh_participationType.PropertyValues[0].RelatedEntity,idiomaUsuario);
			}
			SemanticPropertyModel propRoh_promotedByHasCountryName = pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/promotedByHasCountryName");
			if(propRoh_promotedByHasCountryName != null && propRoh_promotedByHasCountryName.PropertyValues.Count > 0)
			{
				this.Roh_promotedByHasCountryName = new Feature(propRoh_promotedByHasCountryName.PropertyValues[0].RelatedEntity,idiomaUsuario);
			}
			SemanticPropertyModel propRoh_promotedByType = pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/promotedByType");
			if(propRoh_promotedByType != null && propRoh_promotedByType.PropertyValues.Count > 0)
			{
				this.Roh_promotedByType = new OrganizationType(propRoh_promotedByType.PropertyValues[0].RelatedEntity,idiomaUsuario);
			}
			SemanticPropertyModel propRoh_accessSystemActivity = pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/accessSystemActivity");
			if(propRoh_accessSystemActivity != null && propRoh_accessSystemActivity.PropertyValues.Count > 0)
			{
				this.Roh_accessSystemActivity = new AccessSystemActivity(propRoh_accessSystemActivity.PropertyValues[0].RelatedEntity,idiomaUsuario);
			}
			SemanticPropertyModel propRoh_promotedByHasRegion = pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/promotedByHasRegion");
			if(propRoh_promotedByHasRegion != null && propRoh_promotedByHasRegion.PropertyValues.Count > 0)
			{
				this.Roh_promotedByHasRegion = new Feature(propRoh_promotedByHasRegion.PropertyValues[0].RelatedEntity,idiomaUsuario);
			}
			SemanticPropertyModel propRoh_conductedBy = pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/conductedBy");
			if(propRoh_conductedBy != null && propRoh_conductedBy.PropertyValues.Count > 0)
			{
				this.Roh_conductedBy = new Organization(propRoh_conductedBy.PropertyValues[0].RelatedEntity,idiomaUsuario);
			}
			SemanticPropertyModel propVcard_hasRegion = pSemCmsModel.GetPropertyByPath("https://www.w3.org/2006/vcard/ns#hasRegion");
			if(propVcard_hasRegion != null && propVcard_hasRegion.PropertyValues.Count > 0)
			{
				this.Vcard_hasRegion = new Feature(propVcard_hasRegion.PropertyValues[0].RelatedEntity,idiomaUsuario);
			}
			SemanticPropertyModel propVivo_geographicFocus = pSemCmsModel.GetPropertyByPath("http://vivoweb.org/ontology/core#geographicFocus");
			if(propVivo_geographicFocus != null && propVivo_geographicFocus.PropertyValues.Count > 0)
			{
				this.Vivo_geographicFocus = new GeographicRegion(propVivo_geographicFocus.PropertyValues[0].RelatedEntity,idiomaUsuario);
			}
			this.Roh_representedEntityHasCountryName = new List<Feature>();
			SemanticPropertyModel propRoh_representedEntityHasCountryName = pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/representedEntityHasCountryName");
			if(propRoh_representedEntityHasCountryName != null && propRoh_representedEntityHasCountryName.PropertyValues.Count > 0)
			{
				foreach (SemanticPropertyModel.PropertyValue propValue in propRoh_representedEntityHasCountryName.PropertyValues)
				{
					if(propValue.RelatedEntity!=null){
						Feature roh_representedEntityHasCountryName = new Feature(propValue.RelatedEntity,idiomaUsuario);
						this.Roh_representedEntityHasCountryName.Add(roh_representedEntityHasCountryName);
					}
				}
			}
			SemanticPropertyModel propRoh_activityModality = pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/activityModality");
			if(propRoh_activityModality != null && propRoh_activityModality.PropertyValues.Count > 0)
			{
				this.Roh_activityModality = new ActivityModality(propRoh_activityModality.PropertyValues[0].RelatedEntity,idiomaUsuario);
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
			this.Roh_representedEntity = new List<Organization>();
			SemanticPropertyModel propRoh_representedEntity = pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/representedEntity");
			if(propRoh_representedEntity != null && propRoh_representedEntity.PropertyValues.Count > 0)
			{
				foreach (SemanticPropertyModel.PropertyValue propValue in propRoh_representedEntity.PropertyValues)
				{
					if(propValue.RelatedEntity!=null){
						Organization roh_representedEntity = new Organization(propValue.RelatedEntity,idiomaUsuario);
						this.Roh_representedEntity.Add(roh_representedEntity);
					}
				}
			}
			this.Roh_representedEntityType = new List<OrganizationType>();
			SemanticPropertyModel propRoh_representedEntityType = pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/representedEntityType");
			if(propRoh_representedEntityType != null && propRoh_representedEntityType.PropertyValues.Count > 0)
			{
				foreach (SemanticPropertyModel.PropertyValue propValue in propRoh_representedEntityType.PropertyValues)
				{
					if(propValue.RelatedEntity!=null){
						OrganizationType roh_representedEntityType = new OrganizationType(propValue.RelatedEntity,idiomaUsuario);
						this.Roh_representedEntityType.Add(roh_representedEntityType);
					}
				}
			}
			this.Roh_promotedByLocality = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/promotedByLocality"));
			this.Roh_geographicFocusOther = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/geographicFocusOther"));
			this.Roh_durationDays = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/durationDays"));
			this.Vivo_start= GetDateValuePropertySemCms(pSemCmsModel.GetPropertyByPath("http://vivoweb.org/ontology/core#start"));
			this.Roh_accessSystemActivityOther = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/accessSystemActivityOther"));
			this.Roh_frequency = GetNumberIntPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/frequency"));
			this.Roh_goals = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/goals"));
			this.Roh_profesionalCategory = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/profesionalCategory"));
			this.Vcard_locality = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("https://www.w3.org/2006/vcard/ns#locality"));
			this.Roh_conductedByTitle = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/conductedByTitle"));
			this.Roh_representedEntityLocality = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/representedEntityLocality"));
			this.Roh_concreteFunctions = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/concreteFunctions"));
			this.Roh_promotedByTitle = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/promotedByTitle"));
			this.Dc_type = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://purl.org/dc/elements/1.1/type"));
			this.Roh_personNumber = GetNumberIntPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/personNumber"));
			this.Roh_managementTypeOther = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/managementTypeOther"));
			this.Roh_durationMonths = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/durationMonths"));
			this.Roh_conductedByTypeOther = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/conductedByTypeOther"));
			this.Roh_averageAnnualBudget = GetNumberFloatPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/averageAnnualBudget"));
			this.Roh_participationTypeOther = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/participationTypeOther"));
			this.Roh_promotedByTypeOther = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/promotedByTypeOther"));
			this.Roh_durationYears = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/durationYears"));
			this.Roh_representedEntityTypeOther = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/representedEntityTypeOther"));
			this.Roh_representedEntityTitle = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/representedEntityTitle"));
			this.Roh_crisIdentifier = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/crisIdentifier"));
			this.Roh_attendants = GetNumberIntPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/attendants"));
			this.Vivo_end= GetDateValuePropertySemCms(pSemCmsModel.GetPropertyByPath("http://vivoweb.org/ontology/core#end"));
			this.Roh_activityModalityOther = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/activityModalityOther"));
			this.Roh_functions = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/functions"));
			this.Roh_classificationCVN = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/classificationCVN"));
			this.Roh_title = GetPropertyValueSemCms(pSemCmsModel.GetPropertyByPath("http://w3id.org/roh/title"));
		}

		public virtual string RdfType { get { return "http://w3id.org/roh/Activity"; } }
		public virtual string RdfsLabel { get { return "http://w3id.org/roh/Activity"; } }
		[LABEL(LanguageEnum.es,"http://w3id.org/roh/promotedBy")]
		[RDFProperty("http://w3id.org/roh/promotedBy")]
		public  Organization Roh_promotedBy  { get; set;} 
		public string IdRoh_promotedBy  { get; set;} 

		[LABEL(LanguageEnum.es,"http://w3id.org/roh/representedEntityHasRegion")]
		[RDFProperty("http://w3id.org/roh/representedEntityHasRegion")]
		public  List<Feature> Roh_representedEntityHasRegion { get; set;}
		public List<string> IdsRoh_representedEntityHasRegion { get; set;}

		[LABEL(LanguageEnum.es,"http://w3id.org/roh/managementType")]
		[RDFProperty("http://w3id.org/roh/managementType")]
		public  ManagementTypeActivity Roh_managementType  { get; set;} 
		public string IdRoh_managementType  { get; set;} 

		[LABEL(LanguageEnum.es,"https://www.w3.org/2006/vcard/ns#hasCountryName")]
		[RDFProperty("https://www.w3.org/2006/vcard/ns#hasCountryName")]
		public  Feature Vcard_hasCountryName  { get; set;} 
		public string IdVcard_hasCountryName  { get; set;} 

		[LABEL(LanguageEnum.es,"http://w3id.org/roh/conductedByType")]
		[RDFProperty("http://w3id.org/roh/conductedByType")]
		public  OrganizationType Roh_conductedByType  { get; set;} 
		public string IdRoh_conductedByType  { get; set;} 

		[LABEL(LanguageEnum.es,"http://w3id.org/roh/targetGroupProfile")]
		[RDFProperty("http://w3id.org/roh/targetGroupProfile")]
		public  TargetGroupProfile Roh_targetGroupProfile  { get; set;} 
		public string IdRoh_targetGroupProfile  { get; set;} 

		[LABEL(LanguageEnum.es,"http://w3id.org/roh/participationType")]
		[RDFProperty("http://w3id.org/roh/participationType")]
		public  ParticipationTypeActivity Roh_participationType  { get; set;} 
		public string IdRoh_participationType  { get; set;} 

		[LABEL(LanguageEnum.es,"http://w3id.org/roh/promotedByHasCountryName")]
		[RDFProperty("http://w3id.org/roh/promotedByHasCountryName")]
		public  Feature Roh_promotedByHasCountryName  { get; set;} 
		public string IdRoh_promotedByHasCountryName  { get; set;} 

		[LABEL(LanguageEnum.es,"http://w3id.org/roh/promotedByType")]
		[RDFProperty("http://w3id.org/roh/promotedByType")]
		public  OrganizationType Roh_promotedByType  { get; set;} 
		public string IdRoh_promotedByType  { get; set;} 

		[LABEL(LanguageEnum.es,"http://w3id.org/roh/accessSystemActivity")]
		[RDFProperty("http://w3id.org/roh/accessSystemActivity")]
		public  AccessSystemActivity Roh_accessSystemActivity  { get; set;} 
		public string IdRoh_accessSystemActivity  { get; set;} 

		[LABEL(LanguageEnum.es,"http://w3id.org/roh/promotedByHasRegion")]
		[RDFProperty("http://w3id.org/roh/promotedByHasRegion")]
		public  Feature Roh_promotedByHasRegion  { get; set;} 
		public string IdRoh_promotedByHasRegion  { get; set;} 

		[LABEL(LanguageEnum.es,"http://w3id.org/roh/conductedBy")]
		[RDFProperty("http://w3id.org/roh/conductedBy")]
		public  Organization Roh_conductedBy  { get; set;} 
		public string IdRoh_conductedBy  { get; set;} 

		[LABEL(LanguageEnum.es,"https://www.w3.org/2006/vcard/ns#hasRegion")]
		[RDFProperty("https://www.w3.org/2006/vcard/ns#hasRegion")]
		public  Feature Vcard_hasRegion  { get; set;} 
		public string IdVcard_hasRegion  { get; set;} 

		[LABEL(LanguageEnum.es,"http://vivoweb.org/ontology/core#geographicFocus")]
		[RDFProperty("http://vivoweb.org/ontology/core#geographicFocus")]
		public  GeographicRegion Vivo_geographicFocus  { get; set;} 
		public string IdVivo_geographicFocus  { get; set;} 

		[LABEL(LanguageEnum.es,"http://w3id.org/roh/representedEntityHasCountryName")]
		[RDFProperty("http://w3id.org/roh/representedEntityHasCountryName")]
		public  List<Feature> Roh_representedEntityHasCountryName { get; set;}
		public List<string> IdsRoh_representedEntityHasCountryName { get; set;}

		[LABEL(LanguageEnum.es,"http://w3id.org/roh/activityModality")]
		[RDFProperty("http://w3id.org/roh/activityModality")]
		public  ActivityModality Roh_activityModality  { get; set;} 
		public string IdRoh_activityModality  { get; set;} 

		[LABEL(LanguageEnum.es,"http://w3id.org/roh/hasKnowledgeArea")]
		[RDFProperty("http://w3id.org/roh/hasKnowledgeArea")]
		public  List<CategoryPath> Roh_hasKnowledgeArea { get; set;}

		[LABEL(LanguageEnum.es,"http://w3id.org/roh/representedEntity")]
		[RDFProperty("http://w3id.org/roh/representedEntity")]
		public  List<Organization> Roh_representedEntity { get; set;}
		public List<string> IdsRoh_representedEntity { get; set;}

		[LABEL(LanguageEnum.es,"http://w3id.org/roh/representedEntityType")]
		[RDFProperty("http://w3id.org/roh/representedEntityType")]
		public  List<OrganizationType> Roh_representedEntityType { get; set;}
		public List<string> IdsRoh_representedEntityType { get; set;}

		[RDFProperty("http://w3id.org/roh/promotedByLocality")]
		public  string Roh_promotedByLocality { get; set;}

		[LABEL(LanguageEnum.es,"http://w3id.org/roh/geographicFocusOther")]
		[RDFProperty("http://w3id.org/roh/geographicFocusOther")]
		public  string Roh_geographicFocusOther { get; set;}

		[RDFProperty("http://w3id.org/roh/durationDays")]
		public  string Roh_durationDays { get; set;}

		[RDFProperty("http://vivoweb.org/ontology/core#start")]
		public  DateTime? Vivo_start { get; set;}

		[RDFProperty("http://w3id.org/roh/accessSystemActivityOther")]
		public  string Roh_accessSystemActivityOther { get; set;}

		[RDFProperty("http://w3id.org/roh/frequency")]
		public  int? Roh_frequency { get; set;}

		[RDFProperty("http://w3id.org/roh/goals")]
		public  string Roh_goals { get; set;}

		[RDFProperty("http://w3id.org/roh/profesionalCategory")]
		public  string Roh_profesionalCategory { get; set;}

		[LABEL(LanguageEnum.es,"https://www.w3.org/2006/vcard/ns#locality")]
		[RDFProperty("https://www.w3.org/2006/vcard/ns#locality")]
		public  string Vcard_locality { get; set;}

		[RDFProperty("http://w3id.org/roh/conductedByTitle")]
		public  string Roh_conductedByTitle { get; set;}

		[RDFProperty("http://w3id.org/roh/representedEntityLocality")]
		public  string Roh_representedEntityLocality { get; set;}

		[RDFProperty("http://w3id.org/roh/concreteFunctions")]
		public  string Roh_concreteFunctions { get; set;}

		[RDFProperty("http://w3id.org/roh/promotedByTitle")]
		public  string Roh_promotedByTitle { get; set;}

		[LABEL(LanguageEnum.es,"http://purl.org/dc/elements/1.1/type")]
		[RDFProperty("http://purl.org/dc/elements/1.1/type")]
		public  string Dc_type { get; set;}

		[RDFProperty("http://w3id.org/roh/personNumber")]
		public  int? Roh_personNumber { get; set;}

		[RDFProperty("http://w3id.org/roh/managementTypeOther")]
		public  string Roh_managementTypeOther { get; set;}

		[RDFProperty("http://w3id.org/roh/durationMonths")]
		public  string Roh_durationMonths { get; set;}

		[RDFProperty("http://w3id.org/roh/conductedByTypeOther")]
		public  string Roh_conductedByTypeOther { get; set;}

		[LABEL(LanguageEnum.es,"http://w3id.org/roh/averageAnnualBudget")]
		[RDFProperty("http://w3id.org/roh/averageAnnualBudget")]
		public  float? Roh_averageAnnualBudget { get; set;}

		[RDFProperty("http://w3id.org/roh/participationTypeOther")]
		public  string Roh_participationTypeOther { get; set;}

		[RDFProperty("http://w3id.org/roh/promotedByTypeOther")]
		public  string Roh_promotedByTypeOther { get; set;}

		[RDFProperty("http://w3id.org/roh/durationYears")]
		public  string Roh_durationYears { get; set;}

		[RDFProperty("http://w3id.org/roh/representedEntityTypeOther")]
		public  string Roh_representedEntityTypeOther { get; set;}

		[RDFProperty("http://w3id.org/roh/representedEntityTitle")]
		public  string Roh_representedEntityTitle { get; set;}

		[LABEL(LanguageEnum.es,"http://w3id.org/roh/crisIdentifier")]
		[RDFProperty("http://w3id.org/roh/crisIdentifier")]
		public  string Roh_crisIdentifier { get; set;}

		[LABEL(LanguageEnum.es,"http://w3id.org/roh/attendants")]
		[RDFProperty("http://w3id.org/roh/attendants")]
		public  int? Roh_attendants { get; set;}

		[RDFProperty("http://vivoweb.org/ontology/core#end")]
		public  DateTime? Vivo_end { get; set;}

		[LABEL(LanguageEnum.es,"http://w3id.org/roh/activityModalityOther")]
		[RDFProperty("http://w3id.org/roh/activityModalityOther")]
		public  string Roh_activityModalityOther { get; set;}

		[RDFProperty("http://w3id.org/roh/functions")]
		public  string Roh_functions { get; set;}

		[LABEL(LanguageEnum.es,"http://w3id.org/roh/classificationCVN")]
		[RDFProperty("http://w3id.org/roh/classificationCVN")]
		public  string Roh_classificationCVN { get; set;}

		[LABEL(LanguageEnum.es,"http://w3id.org/roh/title")]
		[RDFProperty("http://w3id.org/roh/title")]
		public  string Roh_title { get; set;}


		internal override void GetProperties()
		{
			base.GetProperties();
			propList.Add(new StringOntologyProperty("roh:promotedBy", this.IdRoh_promotedBy));
			propList.Add(new ListStringOntologyProperty("roh:representedEntityHasRegion", this.IdsRoh_representedEntityHasRegion));
			propList.Add(new StringOntologyProperty("roh:managementType", this.IdRoh_managementType));
			propList.Add(new StringOntologyProperty("vcard:hasCountryName", this.IdVcard_hasCountryName));
			propList.Add(new StringOntologyProperty("roh:conductedByType", this.IdRoh_conductedByType));
			propList.Add(new StringOntologyProperty("roh:targetGroupProfile", this.IdRoh_targetGroupProfile));
			propList.Add(new StringOntologyProperty("roh:participationType", this.IdRoh_participationType));
			propList.Add(new StringOntologyProperty("roh:promotedByHasCountryName", this.IdRoh_promotedByHasCountryName));
			propList.Add(new StringOntologyProperty("roh:promotedByType", this.IdRoh_promotedByType));
			propList.Add(new StringOntologyProperty("roh:accessSystemActivity", this.IdRoh_accessSystemActivity));
			propList.Add(new StringOntologyProperty("roh:promotedByHasRegion", this.IdRoh_promotedByHasRegion));
			propList.Add(new StringOntologyProperty("roh:conductedBy", this.IdRoh_conductedBy));
			propList.Add(new StringOntologyProperty("vcard:hasRegion", this.IdVcard_hasRegion));
			propList.Add(new StringOntologyProperty("vivo:geographicFocus", this.IdVivo_geographicFocus));
			propList.Add(new ListStringOntologyProperty("roh:representedEntityHasCountryName", this.IdsRoh_representedEntityHasCountryName));
			propList.Add(new StringOntologyProperty("roh:activityModality", this.IdRoh_activityModality));
			propList.Add(new ListStringOntologyProperty("roh:representedEntity", this.IdsRoh_representedEntity));
			propList.Add(new ListStringOntologyProperty("roh:representedEntityType", this.IdsRoh_representedEntityType));
			propList.Add(new StringOntologyProperty("roh:promotedByLocality", this.Roh_promotedByLocality));
			propList.Add(new StringOntologyProperty("roh:geographicFocusOther", this.Roh_geographicFocusOther));
			propList.Add(new StringOntologyProperty("roh:durationDays", this.Roh_durationDays));
			if (this.Vivo_start.HasValue){
				propList.Add(new DateOntologyProperty("vivo:start", this.Vivo_start.Value));
				}
			propList.Add(new StringOntologyProperty("roh:accessSystemActivityOther", this.Roh_accessSystemActivityOther));
			propList.Add(new StringOntologyProperty("roh:frequency", this.Roh_frequency.ToString()));
			propList.Add(new StringOntologyProperty("roh:goals", this.Roh_goals));
			propList.Add(new StringOntologyProperty("roh:profesionalCategory", this.Roh_profesionalCategory));
			propList.Add(new StringOntologyProperty("vcard:locality", this.Vcard_locality));
			propList.Add(new StringOntologyProperty("roh:conductedByTitle", this.Roh_conductedByTitle));
			propList.Add(new StringOntologyProperty("roh:representedEntityLocality", this.Roh_representedEntityLocality));
			propList.Add(new StringOntologyProperty("roh:concreteFunctions", this.Roh_concreteFunctions));
			propList.Add(new StringOntologyProperty("roh:promotedByTitle", this.Roh_promotedByTitle));
			propList.Add(new StringOntologyProperty("dc:type", this.Dc_type));
			propList.Add(new StringOntologyProperty("roh:personNumber", this.Roh_personNumber.ToString()));
			propList.Add(new StringOntologyProperty("roh:managementTypeOther", this.Roh_managementTypeOther));
			propList.Add(new StringOntologyProperty("roh:durationMonths", this.Roh_durationMonths));
			propList.Add(new StringOntologyProperty("roh:conductedByTypeOther", this.Roh_conductedByTypeOther));
			propList.Add(new StringOntologyProperty("roh:averageAnnualBudget", this.Roh_averageAnnualBudget.ToString()));
			propList.Add(new StringOntologyProperty("roh:participationTypeOther", this.Roh_participationTypeOther));
			propList.Add(new StringOntologyProperty("roh:promotedByTypeOther", this.Roh_promotedByTypeOther));
			propList.Add(new StringOntologyProperty("roh:durationYears", this.Roh_durationYears));
			propList.Add(new StringOntologyProperty("roh:representedEntityTypeOther", this.Roh_representedEntityTypeOther));
			propList.Add(new StringOntologyProperty("roh:representedEntityTitle", this.Roh_representedEntityTitle));
			propList.Add(new StringOntologyProperty("roh:crisIdentifier", this.Roh_crisIdentifier));
			propList.Add(new StringOntologyProperty("roh:attendants", this.Roh_attendants.ToString()));
			if (this.Vivo_end.HasValue){
				propList.Add(new DateOntologyProperty("vivo:end", this.Vivo_end.Value));
				}
			propList.Add(new StringOntologyProperty("roh:activityModalityOther", this.Roh_activityModalityOther));
			propList.Add(new StringOntologyProperty("roh:functions", this.Roh_functions));
			propList.Add(new StringOntologyProperty("roh:classificationCVN", this.Roh_classificationCVN));
			propList.Add(new StringOntologyProperty("roh:title", this.Roh_title));
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
		public virtual ComplexOntologyResource ToGnossApiResource(ResourceApi resourceAPI, List<string> listaDeCategorias)
		{
			return ToGnossApiResource(resourceAPI, listaDeCategorias, Guid.Empty, Guid.Empty);
		}

		public virtual ComplexOntologyResource ToGnossApiResource(ResourceApi resourceAPI, List<string> listaDeCategorias, Guid idrecurso, Guid idarticulo)
		{
			ComplexOntologyResource resource = new ComplexOntologyResource();
			Ontology ontology=null;
			GetEntities();
			GetProperties();
			if(idrecurso.Equals(Guid.Empty) && idarticulo.Equals(Guid.Empty))
			{
				ontology = new Ontology(resourceAPI.GraphsUrl, resourceAPI.OntologyUrl, RdfType, RdfsLabel, prefList, propList, entList);
			}
			else{
				ontology = new Ontology(resourceAPI.GraphsUrl, resourceAPI.OntologyUrl, RdfType, RdfsLabel, prefList, propList, entList,idrecurso,idarticulo);
			}
			resource.Id = GNOSSID;
			resource.Ontology = ontology;
			resource.TextCategories=listaDeCategorias;
			AddResourceTitle(resource);
			AddResourceDescription(resource);
			AddImages(resource);
			AddFiles(resource);
			return resource;
		}

		public override List<string> ToOntologyGnossTriples(ResourceApi resourceAPI)
		{
			List<string> list = new List<string>();
			AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Activity_{ResourceID}_{ArticleID}", "http://www.w3.org/1999/02/22-rdf-syntax-ns#type", $"<http://w3id.org/roh/Activity>", list, " . ");
			AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Activity_{ResourceID}_{ArticleID}", "http://www.w3.org/2000/01/rdf-schema#label", $"\"http://w3id.org/roh/Activity\"", list, " . ");
			AgregarTripleALista($"{resourceAPI.GraphsUrl}{ResourceID}", "http://gnoss/hasEntidad", $"<{resourceAPI.GraphsUrl}items/Activity_{ResourceID}_{ArticleID}>", list, " . ");
			if(this.Roh_hasKnowledgeArea != null)
			{
			foreach(var item0 in this.Roh_hasKnowledgeArea)
			{
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/CategoryPath_{ResourceID}_{item0.ArticleID}", "http://www.w3.org/1999/02/22-rdf-syntax-ns#type", $"<http://w3id.org/roh/CategoryPath>", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/CategoryPath_{ResourceID}_{item0.ArticleID}", "http://www.w3.org/2000/01/rdf-schema#label", $"\"http://w3id.org/roh/CategoryPath\"", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}{ResourceID}", "http://gnoss/hasEntidad", $"<{resourceAPI.GraphsUrl}items/CategoryPath_{ResourceID}_{item0.ArticleID}>", list, " . ");
				AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Activity_{ResourceID}_{ArticleID}", "http://w3id.org/roh/hasKnowledgeArea", $"<{resourceAPI.GraphsUrl}items/CategoryPath_{ResourceID}_{item0.ArticleID}>", list, " . ");
				if(item0.IdsRoh_categoryNode != null)
				{
					foreach(var item2 in item0.IdsRoh_categoryNode)
					{
						AgregarTripleALista($"{resourceAPI.GraphsUrl}items/CategoryPath_{ResourceID}_{item0.ArticleID}", "http://w3id.org/roh/categoryNode",  $"<{item2}>", list, " . ");
					}
				}
			}
			}
				if(this.IdRoh_promotedBy != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Activity_{ResourceID}_{ArticleID}",  "http://w3id.org/roh/promotedBy", $"<{this.IdRoh_promotedBy}>", list, " . ");
				}
				if(this.IdsRoh_representedEntityHasRegion != null)
				{
					foreach(var item2 in this.IdsRoh_representedEntityHasRegion)
					{
						AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Activity_{ResourceID}_{ArticleID}", "http://w3id.org/roh/representedEntityHasRegion", $"<{item2}>", list, " . ");
					}
				}
				if(this.IdRoh_managementType != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Activity_{ResourceID}_{ArticleID}",  "http://w3id.org/roh/managementType", $"<{this.IdRoh_managementType}>", list, " . ");
				}
				if(this.IdVcard_hasCountryName != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Activity_{ResourceID}_{ArticleID}",  "https://www.w3.org/2006/vcard/ns#hasCountryName", $"<{this.IdVcard_hasCountryName}>", list, " . ");
				}
				if(this.IdRoh_conductedByType != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Activity_{ResourceID}_{ArticleID}",  "http://w3id.org/roh/conductedByType", $"<{this.IdRoh_conductedByType}>", list, " . ");
				}
				if(this.IdRoh_targetGroupProfile != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Activity_{ResourceID}_{ArticleID}",  "http://w3id.org/roh/targetGroupProfile", $"<{this.IdRoh_targetGroupProfile}>", list, " . ");
				}
				if(this.IdRoh_participationType != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Activity_{ResourceID}_{ArticleID}",  "http://w3id.org/roh/participationType", $"<{this.IdRoh_participationType}>", list, " . ");
				}
				if(this.IdRoh_promotedByHasCountryName != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Activity_{ResourceID}_{ArticleID}",  "http://w3id.org/roh/promotedByHasCountryName", $"<{this.IdRoh_promotedByHasCountryName}>", list, " . ");
				}
				if(this.IdRoh_promotedByType != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Activity_{ResourceID}_{ArticleID}",  "http://w3id.org/roh/promotedByType", $"<{this.IdRoh_promotedByType}>", list, " . ");
				}
				if(this.IdRoh_accessSystemActivity != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Activity_{ResourceID}_{ArticleID}",  "http://w3id.org/roh/accessSystemActivity", $"<{this.IdRoh_accessSystemActivity}>", list, " . ");
				}
				if(this.IdRoh_promotedByHasRegion != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Activity_{ResourceID}_{ArticleID}",  "http://w3id.org/roh/promotedByHasRegion", $"<{this.IdRoh_promotedByHasRegion}>", list, " . ");
				}
				if(this.IdRoh_conductedBy != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Activity_{ResourceID}_{ArticleID}",  "http://w3id.org/roh/conductedBy", $"<{this.IdRoh_conductedBy}>", list, " . ");
				}
				if(this.IdVcard_hasRegion != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Activity_{ResourceID}_{ArticleID}",  "https://www.w3.org/2006/vcard/ns#hasRegion", $"<{this.IdVcard_hasRegion}>", list, " . ");
				}
				if(this.IdVivo_geographicFocus != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Activity_{ResourceID}_{ArticleID}",  "http://vivoweb.org/ontology/core#geographicFocus", $"<{this.IdVivo_geographicFocus}>", list, " . ");
				}
				if(this.IdsRoh_representedEntityHasCountryName != null)
				{
					foreach(var item2 in this.IdsRoh_representedEntityHasCountryName)
					{
						AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Activity_{ResourceID}_{ArticleID}", "http://w3id.org/roh/representedEntityHasCountryName", $"<{item2}>", list, " . ");
					}
				}
				if(this.IdRoh_activityModality != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Activity_{ResourceID}_{ArticleID}",  "http://w3id.org/roh/activityModality", $"<{this.IdRoh_activityModality}>", list, " . ");
				}
				if(this.IdsRoh_representedEntity != null)
				{
					foreach(var item2 in this.IdsRoh_representedEntity)
					{
						AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Activity_{ResourceID}_{ArticleID}", "http://w3id.org/roh/representedEntity", $"<{item2}>", list, " . ");
					}
				}
				if(this.IdsRoh_representedEntityType != null)
				{
					foreach(var item2 in this.IdsRoh_representedEntityType)
					{
						AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Activity_{ResourceID}_{ArticleID}", "http://w3id.org/roh/representedEntityType", $"<{item2}>", list, " . ");
					}
				}
				if(this.Roh_promotedByLocality != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Activity_{ResourceID}_{ArticleID}",  "http://w3id.org/roh/promotedByLocality", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_promotedByLocality)}\"", list, " . ");
				}
				if(this.Roh_geographicFocusOther != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Activity_{ResourceID}_{ArticleID}",  "http://w3id.org/roh/geographicFocusOther", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_geographicFocusOther)}\"", list, " . ");
				}
				if(this.Roh_durationDays != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Activity_{ResourceID}_{ArticleID}",  "http://w3id.org/roh/durationDays", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_durationDays)}\"", list, " . ");
				}
				if(this.Vivo_start != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Activity_{ResourceID}_{ArticleID}",  "http://vivoweb.org/ontology/core#start", $"\"{this.Vivo_start.Value.ToString("yyyyMMddHHmmss")}\"", list, " . ");
				}
				if(this.Roh_accessSystemActivityOther != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Activity_{ResourceID}_{ArticleID}",  "http://w3id.org/roh/accessSystemActivityOther", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_accessSystemActivityOther)}\"", list, " . ");
				}
				if(this.Roh_frequency != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Activity_{ResourceID}_{ArticleID}",  "http://w3id.org/roh/frequency", $"{this.Roh_frequency.Value.ToString()}", list, " . ");
				}
				if(this.Roh_goals != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Activity_{ResourceID}_{ArticleID}",  "http://w3id.org/roh/goals", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_goals)}\"", list, " . ");
				}
				if(this.Roh_profesionalCategory != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Activity_{ResourceID}_{ArticleID}",  "http://w3id.org/roh/profesionalCategory", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_profesionalCategory)}\"", list, " . ");
				}
				if(this.Vcard_locality != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Activity_{ResourceID}_{ArticleID}",  "https://www.w3.org/2006/vcard/ns#locality", $"\"{GenerarTextoSinSaltoDeLinea(this.Vcard_locality)}\"", list, " . ");
				}
				if(this.Roh_conductedByTitle != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Activity_{ResourceID}_{ArticleID}",  "http://w3id.org/roh/conductedByTitle", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_conductedByTitle)}\"", list, " . ");
				}
				if(this.Roh_representedEntityLocality != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Activity_{ResourceID}_{ArticleID}",  "http://w3id.org/roh/representedEntityLocality", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_representedEntityLocality)}\"", list, " . ");
				}
				if(this.Roh_concreteFunctions != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Activity_{ResourceID}_{ArticleID}",  "http://w3id.org/roh/concreteFunctions", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_concreteFunctions)}\"", list, " . ");
				}
				if(this.Roh_promotedByTitle != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Activity_{ResourceID}_{ArticleID}",  "http://w3id.org/roh/promotedByTitle", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_promotedByTitle)}\"", list, " . ");
				}
				if(this.Dc_type != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Activity_{ResourceID}_{ArticleID}",  "http://purl.org/dc/elements/1.1/type", $"\"{GenerarTextoSinSaltoDeLinea(this.Dc_type)}\"", list, " . ");
				}
				if(this.Roh_personNumber != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Activity_{ResourceID}_{ArticleID}",  "http://w3id.org/roh/personNumber", $"{this.Roh_personNumber.Value.ToString()}", list, " . ");
				}
				if(this.Roh_managementTypeOther != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Activity_{ResourceID}_{ArticleID}",  "http://w3id.org/roh/managementTypeOther", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_managementTypeOther)}\"", list, " . ");
				}
				if(this.Roh_durationMonths != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Activity_{ResourceID}_{ArticleID}",  "http://w3id.org/roh/durationMonths", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_durationMonths)}\"", list, " . ");
				}
				if(this.Roh_conductedByTypeOther != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Activity_{ResourceID}_{ArticleID}",  "http://w3id.org/roh/conductedByTypeOther", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_conductedByTypeOther)}\"", list, " . ");
				}
				if(this.Roh_averageAnnualBudget != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Activity_{ResourceID}_{ArticleID}",  "http://w3id.org/roh/averageAnnualBudget", $"{this.Roh_averageAnnualBudget.Value.ToString(new CultureInfo("en-US"))}", list, " . ");
				}
				if(this.Roh_participationTypeOther != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Activity_{ResourceID}_{ArticleID}",  "http://w3id.org/roh/participationTypeOther", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_participationTypeOther)}\"", list, " . ");
				}
				if(this.Roh_promotedByTypeOther != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Activity_{ResourceID}_{ArticleID}",  "http://w3id.org/roh/promotedByTypeOther", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_promotedByTypeOther)}\"", list, " . ");
				}
				if(this.Roh_durationYears != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Activity_{ResourceID}_{ArticleID}",  "http://w3id.org/roh/durationYears", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_durationYears)}\"", list, " . ");
				}
				if(this.Roh_representedEntityTypeOther != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Activity_{ResourceID}_{ArticleID}",  "http://w3id.org/roh/representedEntityTypeOther", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_representedEntityTypeOther)}\"", list, " . ");
				}
				if(this.Roh_representedEntityTitle != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Activity_{ResourceID}_{ArticleID}",  "http://w3id.org/roh/representedEntityTitle", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_representedEntityTitle)}\"", list, " . ");
				}
				if(this.Roh_crisIdentifier != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Activity_{ResourceID}_{ArticleID}",  "http://w3id.org/roh/crisIdentifier", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_crisIdentifier)}\"", list, " . ");
				}
				if(this.Roh_attendants != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Activity_{ResourceID}_{ArticleID}",  "http://w3id.org/roh/attendants", $"{this.Roh_attendants.Value.ToString()}", list, " . ");
				}
				if(this.Vivo_end != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Activity_{ResourceID}_{ArticleID}",  "http://vivoweb.org/ontology/core#end", $"\"{this.Vivo_end.Value.ToString("yyyyMMddHHmmss")}\"", list, " . ");
				}
				if(this.Roh_activityModalityOther != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Activity_{ResourceID}_{ArticleID}",  "http://w3id.org/roh/activityModalityOther", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_activityModalityOther)}\"", list, " . ");
				}
				if(this.Roh_functions != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Activity_{ResourceID}_{ArticleID}",  "http://w3id.org/roh/functions", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_functions)}\"", list, " . ");
				}
				if(this.Roh_classificationCVN != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Activity_{ResourceID}_{ArticleID}",  "http://w3id.org/roh/classificationCVN", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_classificationCVN)}\"", list, " . ");
				}
				if(this.Roh_title != null)
				{
					AgregarTripleALista($"{resourceAPI.GraphsUrl}items/Activity_{ResourceID}_{ArticleID}",  "http://w3id.org/roh/title", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_title)}\"", list, " . ");
				}
			return list;
		}

		public override List<string> ToSearchGraphTriples(ResourceApi resourceAPI)
		{
			List<string> list = new List<string>();
			List<string> listaSearch = new List<string>();
			AgregarTags(list);
			AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}", "http://www.w3.org/1999/02/22-rdf-syntax-ns#type", $"\"activity\"", list, " . ");
			AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}", "http://gnoss/type", $"\"http://w3id.org/roh/Activity\"", list, " . ");
			AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}", "http://gnoss/hasfechapublicacion", $"{DateTime.Now.ToString("yyyyMMddHHmmss")}", list, " . ");
			AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}", "http://gnoss/hastipodoc", "\"5\"", list, " . ");
			AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}", "http://gnoss/hasfechamodificacion", $"{DateTime.Now.ToString("yyyyMMddHHmmss")}", list, " . ");
			AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}", "http://gnoss/hasnumeroVisitas", "0", list, " . ");
			AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}", "http://gnoss/hasprivacidadCom", "\"publico\"", list, " . ");
			AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}", "http://xmlns.com/foaf/0.1/firstName", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_title)}\"", list, " . ");
			AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}", "http://gnoss/hasnombrecompleto", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_title)}\"", list, " . ");
			string search = string.Empty;
			if(this.Roh_hasKnowledgeArea != null)
			{
			foreach(var item0 in this.Roh_hasKnowledgeArea)
			{
				AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}", "http://w3id.org/roh/hasKnowledgeArea", $"<{resourceAPI.GraphsUrl}items/categorypath_{ResourceID}_{item0.ArticleID}>", list, " . ");
				if(item0.IdsRoh_categoryNode != null)
				{
					foreach(var item2 in item0.IdsRoh_categoryNode)
					{
					Regex regex = new Regex(@"\/items\/.+_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}");
					string itemRegex = item2;
					if (regex.IsMatch(itemRegex))
					{
						itemRegex = $"http://gnoss/{resourceAPI.GetShortGuid(itemRegex).ToString().ToUpper()}";
					}
					else
					{
						itemRegex = itemRegex.ToLower();
					}
						AgregarTripleALista($"{resourceAPI.GraphsUrl}items/categorypath_{ResourceID}_{item0.ArticleID}", "http://w3id.org/roh/categoryNode",  $"<{itemRegex}>", list, " . ");
					}
				}
			}
			}
				if(this.IdRoh_promotedBy != null)
				{
					Regex regex = new Regex(@"\/items\/.+_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}");
					string itemRegex = this.IdRoh_promotedBy;
					if (regex.IsMatch(itemRegex))
					{
						itemRegex = $"http://gnoss/{resourceAPI.GetShortGuid(itemRegex).ToString().ToUpper()}";
					}
					else
					{
						itemRegex = itemRegex.ToLower();
					}
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "http://w3id.org/roh/promotedBy", $"<{itemRegex}>", list, " . ");
				}
				if(this.IdsRoh_representedEntityHasRegion != null)
				{
					foreach(var item2 in this.IdsRoh_representedEntityHasRegion)
					{
					Regex regex = new Regex(@"\/items\/.+_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}");
					string itemRegex = item2;
					if (regex.IsMatch(itemRegex))
					{
						itemRegex = $"http://gnoss/{resourceAPI.GetShortGuid(itemRegex).ToString().ToUpper()}";
					}
					else
					{
						itemRegex = itemRegex.ToLower();
					}
						AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}", "http://w3id.org/roh/representedEntityHasRegion", $"<{itemRegex}>", list, " . ");
					}
				}
				if(this.IdRoh_managementType != null)
				{
					Regex regex = new Regex(@"\/items\/.+_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}");
					string itemRegex = this.IdRoh_managementType;
					if (regex.IsMatch(itemRegex))
					{
						itemRegex = $"http://gnoss/{resourceAPI.GetShortGuid(itemRegex).ToString().ToUpper()}";
					}
					else
					{
						itemRegex = itemRegex.ToLower();
					}
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "http://w3id.org/roh/managementType", $"<{itemRegex}>", list, " . ");
				}
				if(this.IdVcard_hasCountryName != null)
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
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "https://www.w3.org/2006/vcard/ns#hasCountryName", $"<{itemRegex}>", list, " . ");
				}
				if(this.IdRoh_conductedByType != null)
				{
					Regex regex = new Regex(@"\/items\/.+_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}");
					string itemRegex = this.IdRoh_conductedByType;
					if (regex.IsMatch(itemRegex))
					{
						itemRegex = $"http://gnoss/{resourceAPI.GetShortGuid(itemRegex).ToString().ToUpper()}";
					}
					else
					{
						itemRegex = itemRegex.ToLower();
					}
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "http://w3id.org/roh/conductedByType", $"<{itemRegex}>", list, " . ");
				}
				if(this.IdRoh_targetGroupProfile != null)
				{
					Regex regex = new Regex(@"\/items\/.+_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}");
					string itemRegex = this.IdRoh_targetGroupProfile;
					if (regex.IsMatch(itemRegex))
					{
						itemRegex = $"http://gnoss/{resourceAPI.GetShortGuid(itemRegex).ToString().ToUpper()}";
					}
					else
					{
						itemRegex = itemRegex.ToLower();
					}
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "http://w3id.org/roh/targetGroupProfile", $"<{itemRegex}>", list, " . ");
				}
				if(this.IdRoh_participationType != null)
				{
					Regex regex = new Regex(@"\/items\/.+_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}");
					string itemRegex = this.IdRoh_participationType;
					if (regex.IsMatch(itemRegex))
					{
						itemRegex = $"http://gnoss/{resourceAPI.GetShortGuid(itemRegex).ToString().ToUpper()}";
					}
					else
					{
						itemRegex = itemRegex.ToLower();
					}
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "http://w3id.org/roh/participationType", $"<{itemRegex}>", list, " . ");
				}
				if(this.IdRoh_promotedByHasCountryName != null)
				{
					Regex regex = new Regex(@"\/items\/.+_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}");
					string itemRegex = this.IdRoh_promotedByHasCountryName;
					if (regex.IsMatch(itemRegex))
					{
						itemRegex = $"http://gnoss/{resourceAPI.GetShortGuid(itemRegex).ToString().ToUpper()}";
					}
					else
					{
						itemRegex = itemRegex.ToLower();
					}
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "http://w3id.org/roh/promotedByHasCountryName", $"<{itemRegex}>", list, " . ");
				}
				if(this.IdRoh_promotedByType != null)
				{
					Regex regex = new Regex(@"\/items\/.+_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}");
					string itemRegex = this.IdRoh_promotedByType;
					if (regex.IsMatch(itemRegex))
					{
						itemRegex = $"http://gnoss/{resourceAPI.GetShortGuid(itemRegex).ToString().ToUpper()}";
					}
					else
					{
						itemRegex = itemRegex.ToLower();
					}
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "http://w3id.org/roh/promotedByType", $"<{itemRegex}>", list, " . ");
				}
				if(this.IdRoh_accessSystemActivity != null)
				{
					Regex regex = new Regex(@"\/items\/.+_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}");
					string itemRegex = this.IdRoh_accessSystemActivity;
					if (regex.IsMatch(itemRegex))
					{
						itemRegex = $"http://gnoss/{resourceAPI.GetShortGuid(itemRegex).ToString().ToUpper()}";
					}
					else
					{
						itemRegex = itemRegex.ToLower();
					}
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "http://w3id.org/roh/accessSystemActivity", $"<{itemRegex}>", list, " . ");
				}
				if(this.IdRoh_promotedByHasRegion != null)
				{
					Regex regex = new Regex(@"\/items\/.+_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}");
					string itemRegex = this.IdRoh_promotedByHasRegion;
					if (regex.IsMatch(itemRegex))
					{
						itemRegex = $"http://gnoss/{resourceAPI.GetShortGuid(itemRegex).ToString().ToUpper()}";
					}
					else
					{
						itemRegex = itemRegex.ToLower();
					}
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "http://w3id.org/roh/promotedByHasRegion", $"<{itemRegex}>", list, " . ");
				}
				if(this.IdRoh_conductedBy != null)
				{
					Regex regex = new Regex(@"\/items\/.+_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}");
					string itemRegex = this.IdRoh_conductedBy;
					if (regex.IsMatch(itemRegex))
					{
						itemRegex = $"http://gnoss/{resourceAPI.GetShortGuid(itemRegex).ToString().ToUpper()}";
					}
					else
					{
						itemRegex = itemRegex.ToLower();
					}
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "http://w3id.org/roh/conductedBy", $"<{itemRegex}>", list, " . ");
				}
				if(this.IdVcard_hasRegion != null)
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
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "https://www.w3.org/2006/vcard/ns#hasRegion", $"<{itemRegex}>", list, " . ");
				}
				if(this.IdVivo_geographicFocus != null)
				{
					Regex regex = new Regex(@"\/items\/.+_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}");
					string itemRegex = this.IdVivo_geographicFocus;
					if (regex.IsMatch(itemRegex))
					{
						itemRegex = $"http://gnoss/{resourceAPI.GetShortGuid(itemRegex).ToString().ToUpper()}";
					}
					else
					{
						itemRegex = itemRegex.ToLower();
					}
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "http://vivoweb.org/ontology/core#geographicFocus", $"<{itemRegex}>", list, " . ");
				}
				if(this.IdsRoh_representedEntityHasCountryName != null)
				{
					foreach(var item2 in this.IdsRoh_representedEntityHasCountryName)
					{
					Regex regex = new Regex(@"\/items\/.+_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}");
					string itemRegex = item2;
					if (regex.IsMatch(itemRegex))
					{
						itemRegex = $"http://gnoss/{resourceAPI.GetShortGuid(itemRegex).ToString().ToUpper()}";
					}
					else
					{
						itemRegex = itemRegex.ToLower();
					}
						AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}", "http://w3id.org/roh/representedEntityHasCountryName", $"<{itemRegex}>", list, " . ");
					}
				}
				if(this.IdRoh_activityModality != null)
				{
					Regex regex = new Regex(@"\/items\/.+_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}");
					string itemRegex = this.IdRoh_activityModality;
					if (regex.IsMatch(itemRegex))
					{
						itemRegex = $"http://gnoss/{resourceAPI.GetShortGuid(itemRegex).ToString().ToUpper()}";
					}
					else
					{
						itemRegex = itemRegex.ToLower();
					}
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "http://w3id.org/roh/activityModality", $"<{itemRegex}>", list, " . ");
				}
				if(this.IdsRoh_representedEntity != null)
				{
					foreach(var item2 in this.IdsRoh_representedEntity)
					{
					Regex regex = new Regex(@"\/items\/.+_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}");
					string itemRegex = item2;
					if (regex.IsMatch(itemRegex))
					{
						itemRegex = $"http://gnoss/{resourceAPI.GetShortGuid(itemRegex).ToString().ToUpper()}";
					}
					else
					{
						itemRegex = itemRegex.ToLower();
					}
						AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}", "http://w3id.org/roh/representedEntity", $"<{itemRegex}>", list, " . ");
					}
				}
				if(this.IdsRoh_representedEntityType != null)
				{
					foreach(var item2 in this.IdsRoh_representedEntityType)
					{
					Regex regex = new Regex(@"\/items\/.+_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}_[0-9A-Fa-f]{8}[-]?(?:[0-9A-Fa-f]{4}[-]?){3}[0-9A-Fa-f]{12}");
					string itemRegex = item2;
					if (regex.IsMatch(itemRegex))
					{
						itemRegex = $"http://gnoss/{resourceAPI.GetShortGuid(itemRegex).ToString().ToUpper()}";
					}
					else
					{
						itemRegex = itemRegex.ToLower();
					}
						AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}", "http://w3id.org/roh/representedEntityType", $"<{itemRegex}>", list, " . ");
					}
				}
				if(this.Roh_promotedByLocality != null)
				{
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "http://w3id.org/roh/promotedByLocality", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_promotedByLocality).ToLower()}\"", list, " . ");
				}
				if(this.Roh_geographicFocusOther != null)
				{
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "http://w3id.org/roh/geographicFocusOther", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_geographicFocusOther).ToLower()}\"", list, " . ");
				}
				if(this.Roh_durationDays != null)
				{
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "http://w3id.org/roh/durationDays", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_durationDays).ToLower()}\"", list, " . ");
				}
				if(this.Vivo_start != null)
				{
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "http://vivoweb.org/ontology/core#start", $"{this.Vivo_start.Value.ToString("yyyyMMddHHmmss")}", list, " . ");
				}
				if(this.Roh_accessSystemActivityOther != null)
				{
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "http://w3id.org/roh/accessSystemActivityOther", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_accessSystemActivityOther).ToLower()}\"", list, " . ");
				}
				if(this.Roh_frequency != null)
				{
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "http://w3id.org/roh/frequency", $"{this.Roh_frequency.Value.ToString()}", list, " . ");
				}
				if(this.Roh_goals != null)
				{
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "http://w3id.org/roh/goals", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_goals).ToLower()}\"", list, " . ");
				}
				if(this.Roh_profesionalCategory != null)
				{
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "http://w3id.org/roh/profesionalCategory", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_profesionalCategory).ToLower()}\"", list, " . ");
				}
				if(this.Vcard_locality != null)
				{
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "https://www.w3.org/2006/vcard/ns#locality", $"\"{GenerarTextoSinSaltoDeLinea(this.Vcard_locality).ToLower()}\"", list, " . ");
				}
				if(this.Roh_conductedByTitle != null)
				{
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "http://w3id.org/roh/conductedByTitle", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_conductedByTitle).ToLower()}\"", list, " . ");
				}
				if(this.Roh_representedEntityLocality != null)
				{
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "http://w3id.org/roh/representedEntityLocality", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_representedEntityLocality).ToLower()}\"", list, " . ");
				}
				if(this.Roh_concreteFunctions != null)
				{
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "http://w3id.org/roh/concreteFunctions", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_concreteFunctions).ToLower()}\"", list, " . ");
				}
				if(this.Roh_promotedByTitle != null)
				{
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "http://w3id.org/roh/promotedByTitle", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_promotedByTitle).ToLower()}\"", list, " . ");
				}
				if(this.Dc_type != null)
				{
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "http://purl.org/dc/elements/1.1/type", $"\"{GenerarTextoSinSaltoDeLinea(this.Dc_type).ToLower()}\"", list, " . ");
				}
				if(this.Roh_personNumber != null)
				{
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "http://w3id.org/roh/personNumber", $"{this.Roh_personNumber.Value.ToString()}", list, " . ");
				}
				if(this.Roh_managementTypeOther != null)
				{
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "http://w3id.org/roh/managementTypeOther", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_managementTypeOther).ToLower()}\"", list, " . ");
				}
				if(this.Roh_durationMonths != null)
				{
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "http://w3id.org/roh/durationMonths", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_durationMonths).ToLower()}\"", list, " . ");
				}
				if(this.Roh_conductedByTypeOther != null)
				{
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "http://w3id.org/roh/conductedByTypeOther", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_conductedByTypeOther).ToLower()}\"", list, " . ");
				}
				if(this.Roh_averageAnnualBudget != null)
				{
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "http://w3id.org/roh/averageAnnualBudget", $"{this.Roh_averageAnnualBudget.Value.ToString(new CultureInfo("en-US"))}", list, " . ");
				}
				if(this.Roh_participationTypeOther != null)
				{
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "http://w3id.org/roh/participationTypeOther", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_participationTypeOther).ToLower()}\"", list, " . ");
				}
				if(this.Roh_promotedByTypeOther != null)
				{
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "http://w3id.org/roh/promotedByTypeOther", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_promotedByTypeOther).ToLower()}\"", list, " . ");
				}
				if(this.Roh_durationYears != null)
				{
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "http://w3id.org/roh/durationYears", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_durationYears).ToLower()}\"", list, " . ");
				}
				if(this.Roh_representedEntityTypeOther != null)
				{
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "http://w3id.org/roh/representedEntityTypeOther", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_representedEntityTypeOther).ToLower()}\"", list, " . ");
				}
				if(this.Roh_representedEntityTitle != null)
				{
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "http://w3id.org/roh/representedEntityTitle", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_representedEntityTitle).ToLower()}\"", list, " . ");
				}
				if(this.Roh_crisIdentifier != null)
				{
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "http://w3id.org/roh/crisIdentifier", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_crisIdentifier).ToLower()}\"", list, " . ");
				}
				if(this.Roh_attendants != null)
				{
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "http://w3id.org/roh/attendants", $"{this.Roh_attendants.Value.ToString()}", list, " . ");
				}
				if(this.Vivo_end != null)
				{
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "http://vivoweb.org/ontology/core#end", $"{this.Vivo_end.Value.ToString("yyyyMMddHHmmss")}", list, " . ");
				}
				if(this.Roh_activityModalityOther != null)
				{
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "http://w3id.org/roh/activityModalityOther", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_activityModalityOther).ToLower()}\"", list, " . ");
				}
				if(this.Roh_functions != null)
				{
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "http://w3id.org/roh/functions", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_functions).ToLower()}\"", list, " . ");
				}
				if(this.Roh_classificationCVN != null)
				{
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "http://w3id.org/roh/classificationCVN", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_classificationCVN).ToLower()}\"", list, " . ");
				}
				if(this.Roh_title != null)
				{
					AgregarTripleALista($"http://gnoss/{ResourceID.ToString().ToUpper()}",  "http://w3id.org/roh/title", $"\"{GenerarTextoSinSaltoDeLinea(this.Roh_title).ToLower()}\"", list, " . ");
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

			//Insert en la tabla Documento
			string tags = "";
			foreach(string tag in tagList)
			{
				tags += $"{tag}, ";
			}
			if (!string.IsNullOrEmpty(tags))
			{
				tags = tags.Substring(0, tags.LastIndexOf(','));
			}
			string titulo = $"{this.Roh_title.Replace("\r\n", "").Replace("\n", "").Replace("\r", "").Replace("\"", "\"\"").Replace("'", "''").Replace("|", "#PIPE#")}";
			string descripcion = $"{this.Roh_title.Replace("\r\n", "").Replace("\n", "").Replace("\r", "").Replace("\"", "\"\"").Replace("'", "''").Replace("|", "#PIPE#")}";
			string tablaDoc = $"'{titulo}', '{descripcion}', '{resourceAPI.GraphsUrl}', '{tags}'";
			KeyValuePair<Guid, string> valor = new KeyValuePair<Guid, string>(ResourceID, tablaDoc);

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
			return $"{resourceAPI.GraphsUrl}items/ActivityOntology_{ResourceID}_{ArticleID}";
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
