// using System.Collections.Generic;
// using ZenodoConnect.ROs.Zenodo.Models;
// using ZenodoConnect.ROs.Zenodo.Models.Inicial;

// using Newtonsoft.Json.Linq;
// using System;
// using System.Threading;

// using Newtonsoft.Json;


// namespace ZenodoConnect.ROs.Zenodo.Controllers
// {
//     public class ROZenodoControllerJSON //: //ROScopusLogic
//     {
//         public ROZenodoLogic ZenodoLogic;
//         public ROZenodoControllerJSON(ROZenodoLogic ZenodoLogic)
//         {
//             this.ZenodoLogic = ZenodoLogic;

//         }

//         public List<Publication> getListPublicatio(Root objInicial)
//         {
//             List<Publication> sol = new List<Publication>();
//             foreach (PublicacionInicial rec in objInicial.Data.Records.records.publicacionInicial)
//             {
//                 Publication publicacion = cambioDeModeloPublicacion(rec, true);
//                 sol.Add(publicacion);
//             }
//             return sol;
//         }


        
//         public Publication cambioDeModeloPublicacion(PublicacionInicial objInicial, Boolean publicacion_principal)
//         {
//             Publication publicacion = new Publication();
//             if (objInicial != null)
//             {
//                 publicacion.IDs = getIDs(objInicial);
//                 publicacion.title = getTitle(objInicial);
//                 publicacion.Abstract = getAbstract(objInicial);
//                 publicacion.language = getLanguage(objInicial);
//                 publicacion.doi = getDoi(objInicial);
//                 publicacion.url = getLinks(objInicial);
//                 publicacion.dataIssued = getDate(objInicial);
//                 publicacion.pageStart = getPageStart(objInicial);
//                 publicacion.pageEnd = getPageEnd(objInicial);
//                 publicacion.hasKnowledgeArea = getKnowledgeAreas(objInicial);
//                 publicacion.freetextKeyword = getFreetextKeyword(objInicial);
//                 publicacion.correspondingAuthor = getAuthorPrincipal(objInicial);
//                 publicacion.seqOfAuthors = getAuthors(objInicial);
//                 publicacion.hasPublicationVenue = getJournal(objInicial);
//                 publicacion.hasMetric = getPublicationMetric(objInicial);
//                 if(publicacion_principal==true){
//                 publicacion.bibliografia = getBiblografia(objInicial);
//                 publicacion.citas = getCitas(objInicial);
//                 }
//                 return publicacion;
//             }
//             else
//             {
//                 return null;
//             }

//         }

//         public List<String> getIDs(PublicacionInicial objInicial)
//         {
//             return null;
//         }


//         public string getTitle(PublicacionInicial objInicial)
//         {
//             return null;
//         }

//         public string getAbstract(PublicacionInicial objInicial)
//         {
//             return null;
//         }

//         public string getLanguage(PublicacionInicial objInicial)
//         {
//             return null;
//         }
//         public string getDoi(PublicacionInicial objInicial)
//         {
//             return null;
//         }
//         public List<string> getLinks(PublicacionInicial objInicial)
//         {
//             return new List<string>();
//         }

//         public DateTimeValue getDate(PublicacionInicial objInicial)
//         {
//             DateTimeValue date = new DateTimeValue();
//             date.datimeTime =null;
//             return date;
//         }        

//         public string getPageStart(PublicacionInicial objInicial)
//         {
//             return null;
//         }

//         public string getPageEnd(PublicacionInicial objInicial)
//         {
//             return null;
//         }

//         public List<KnowledgeArea> getKnowledgeAreas(PublicacionInicial objInicial)
//         {
//             List<KnowledgeArea> result = new List<KnowledgeArea>();
//             KnowledgeArea area = null; 
//             result.Add(area);
//             return result;
//         }

//         public List<string> getFreetextKeyword(PublicacionInicial objInicial)
//         {
//             return new List<string>();
//         }

//         public Person getAuthorPrincipal(PublicacionInicial objInicial)
//         {
//             return new Person();
//         }
//         public List<Person> getAuthors(PublicacionInicial objInicial)
//         {
//             return new List<Person>();
//         }

//         public Journal getJournal(PublicacionInicial objInicial)
//         {
//             return new Journal();;
//         }

       

//         public PublicationMetric getPublicationMetric(PublicacionInicial objInicial)
//         {
//             PublicationMetric metricPublicacion = new PublicationMetric();
//             return metricPublicacion;
//         }

//         public List<Publication> getBiblografia(PublicacionInicial objInicial)
//         {
//             return new List<Publication>();
//         }

//         public List<Publication> getCitas(PublicacionInicial objInicial)
//         {
//             return new List<Publication>();
//         }




//     }
// }