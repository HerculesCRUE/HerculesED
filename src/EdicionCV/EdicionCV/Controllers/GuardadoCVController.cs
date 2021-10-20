using Gnoss.ApiWrapper;
using Gnoss.ApiWrapper.Model;
using GuardadoCV.Models;
using GuardadoCV.Models.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;

namespace GuardadoCV.Controllers
{
    public class GuardadoCVController : Controller
    {
        /// <summary>
        /// Cambia la privacidad de un item
        /// </summary>
        /// <param name="pIdSection">Identificador de la sección</param>
        /// <param name="pRdfTypeTab">Rdftype del tab</param>
        /// <param name="pEntity">Identificador de la entidad</param>
        /// <param name="pIsPublic">TRUE si es público</param>
        /// <returns></returns>
        [System.Web.Http.HttpPost]
        public ActionResult ChangePrivacityItem(string pIdSection, string pRdfTypeTab, string pEntity, bool pIsPublic)
        {
            AccionesGuardado accionesGuardado = new AccionesGuardado();
            return Json(accionesGuardado.ChangePrivacityItem(pIdSection, pRdfTypeTab, pEntity, pIsPublic));
        }

        /// <summary>
        /// Elimina un item de un listado
        /// </summary>
        /// <param name="pEntity">Entidad a eliminar</param>
        /// <returns></returns>
        [System.Web.Http.HttpPost]
        public ActionResult RemoveItem(string pEntity)
        {
            AccionesGuardado accionesGuardado = new AccionesGuardado();
            return Json(accionesGuardado.RemoveItem(pEntity));
        }

        /// <summary>
        /// Crea o actualiza una entidad
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        [System.Web.Http.HttpPost]
        public ActionResult UpdateEntity(Entity entity, string cvID, string sectionID, string rdfTypeTab)
        {
            AccionesGuardado accionesGuardado = new AccionesGuardado();
            return Json(accionesGuardado.ActualizarEntidad(entity, cvID, sectionID, rdfTypeTab));
        }



        //Añadir entidad

        //Eliminar entidad
    }
}
