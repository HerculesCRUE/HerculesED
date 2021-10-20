using Gnoss.ApiWrapper;
using Gnoss.ApiWrapper.ApiModel;
using GuardadoCV.Models;
using GuardadoCV.Models.API;
using GuardadoCV.Models.API.Template;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Mvc;

namespace GuardadoCV.Controllers
{
    public class EdicionCVController : Controller
    {
        /// <summary>
        /// Obtiene los datos de una pestaña dentro del editor
        /// </summary>
        /// <param name="pId">Identificador de la entidad a recuperar</param>
        /// <param name="pRdfType">Rdf:type de la entidad a recuperar</param>
        /// <param name="pLang">Idioma para recuperar los datos</param>
        /// <returns></returns>
        [System.Web.Http.HttpGet]
        public ActionResult GetTab(string pId, string pRdfType, string pLang)
        {            
            AccionesEdicion accionesEdicion = new AccionesEdicion();
            return Json(accionesEdicion.GetTab(pId, pRdfType, pLang), JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Obtiene una minificha de una entidad de un listado
        /// </summary>
        /// <param name="pIdSection">Identificador de la sección</param>
        /// <param name="pRdfTypeTab">Rdftype del tab</param>
        /// <param name="pEntity">Identificador de la entidad</param>
        /// <param name="pLang">Idioma</param>
        /// <returns></returns>
        [System.Web.Http.HttpGet]
        public ActionResult GetItemMini(string pIdSection, string pRdfTypeTab, string pEntityID, string pLang)
        {
            AccionesEdicion accionesEdicion = new AccionesEdicion();
            return Json(accionesEdicion.GetItemMini(pIdSection, pRdfTypeTab, pEntityID, pLang), JsonRequestBehavior.AllowGet);
        }


        [System.Web.Http.HttpGet]
        public ActionResult GetEdit(string pIdSection, string pRdfTypeTab, string pEntityID, string pLang)
        {
            AccionesEdicion accionesEdicion = new AccionesEdicion();

            var json = Json(accionesEdicion.GetEdit(pIdSection, pRdfTypeTab, pEntityID, pLang), JsonRequestBehavior.AllowGet);
            json.MaxJsonLength = 500000000;
            return json;
        }

        [System.Web.Http.HttpGet]
        public ActionResult GetEditEntity(string pRdfType, string pEntityID, string pLang)
        {
            //TODO
            AccionesEdicion accionesEdicion = new AccionesEdicion();

            var json = Json(accionesEdicion.GetEditEntity(pRdfType, pEntityID, pLang), JsonRequestBehavior.AllowGet);
            json.MaxJsonLength = 500000000;
            return json;
        }

        [System.Web.Http.HttpPost]
        public ActionResult LoadProps(ItemsLoad pItemsLoad,string pLang)
        {
            AccionesEdicion accionesEdicion = new AccionesEdicion();

            var json = Json(accionesEdicion.LoadProps(pItemsLoad,pLang));
            json.MaxJsonLength = 500000000;
            return json;
        }

        //TODO es obligatorio añadirse a uno mismo como autor
        //TODO crear servicio que incluya/elimie en los CV a los autores



        //Añadir entidad

        //Eliminar entidad

    }
}
