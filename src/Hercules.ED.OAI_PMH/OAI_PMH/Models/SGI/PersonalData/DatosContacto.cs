﻿using OAI_PMH.Models.SGI.OrganicStructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OAI_PMH.Models.SGI.PersonalData
{
    public class DatosContacto : SGI_Base
    {
        /// <summary>
        /// Pais de contacto
        /// </summary>
        public Pais PaisContacto { get; set; }
        /// <summary>
        /// Comunidad autónoma de contaco
        /// </summary>
        public ComunidadAutonoma ComAutonomaContacto { get; set; }
        /// <summary>
        /// Provincia de contacto
        /// </summary>
        public Provincia ProvinciaContacto { get; set; }
        /// <summary>
        /// Ciudad de contacto
        /// </summary>
        public string CiudadContacto { get; set; }
        /// <summary>
        /// Dirección de contacto
        /// </summary>
        public string DireccionContacto { get; set; }
        /// <summary>
        /// Código postal de contacto
        /// </summary>
        public string CodigoPostalContacto { get; set; }
        /// <summary>
        /// Emails
        /// </summary>
        public List<Email> Emails { get; set; }
        /// <summary>
        /// Telefonos
        /// </summary>
        public List<string> Telefonos { get; set; }
        /// <summary>
        /// Moviles
        /// </summary>
        public List<string> Moviles { get; set; }
    }
}
