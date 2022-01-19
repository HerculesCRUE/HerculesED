using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GuardadoCV.Models.API.Response
{
    /// <summary>
    /// Json de respuesta básico
    /// </summary>
    public class JsonResult
    {
        /// <summary>
        /// Indica si la operación ha ido bien
        /// </summary>
        public bool ok { get; set; }
        /// <summary>
        /// Muestra el error
        /// </summary>
        public string error { get; set; }
        /// <summary>
        /// Devuelve un ID si es posible
        /// </summary>
        public string id { get; set; }
    }

}