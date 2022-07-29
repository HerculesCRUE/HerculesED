using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace EditorCV.Models.PreimportModels
{
    public class Preimport
    {
        /// <summary>
        /// Array de apartados del CV
        /// </summary>
        public List<Subseccion> secciones { get; set; }
        /// <summary>
        /// Archivo XML
        /// </summary>
        public string cvn_xml { get; set; }
        /// <summary>
        /// Objeto Preimport
        /// </summary>
        public string cvn_preimportar { get; set; }

        public Preimport(List<Subseccion> subsecciones, string cvn_xml, string cvn_preimportar)
        {
            this.secciones = subsecciones;
            this.cvn_xml = cvn_xml;
            this.cvn_xml = cvn_preimportar;
        }

        public Preimport()
        {
            this.secciones = new List<Subseccion>();
        }
    }

    public class Subseccion
    {
        /// <summary>
        /// ID de la sección
        /// </summary>
        public string id { get; set; }

        /// <summary>
        /// Subapartados del apartado
        /// </summary>
        public List<SubseccionItem> subsecciones { get; set; }

        public Subseccion()
        {
        }

        public Subseccion(string id, List<SubseccionItem> subsecciones)
        {
            this.id = id;
            this.subsecciones = subsecciones == null ? new List<SubseccionItem>() : subsecciones;
        }

        public Subseccion(string id)
        {
            this.id = id;
            this.subsecciones = new List<SubseccionItem>();
        }
    }

    public class SubseccionItem
    {
        /// <summary>
        /// Valor numerico del orden de lectura
        /// </summary>
        public int id { get; set; }
        /// <summary>
        /// GUID del recurso
        /// </summary>
        public string guid { get; set; }

        /// <summary>
        /// Identificador en BBDD si existe.
        /// </summary>
        public string idBBDD { get; set; }

        /// <summary>
        /// Indica si el objeto esta bloqueado o se puede editar.
        /// </summary>
        public bool isBlocked { get; set; }

        /// <summary>
        /// Bloques pertenecientes al subapartado
        /// </summary>
        public List<EntityPreimport.Property> propiedades { get; set; }
        /// <summary>
        /// Bloques pertenecientes al subapartado
        /// </summary>
        public List<EntityPreimport.Property> propiedadesCV { get; set; }

        //public SubseccionItem(int id, string idBBDD, List<EntityPreimport.Property> propiedades, List<EntityPreimport.Property> propiedadesCV, bool isBlocked = false)
        //{
        //    this.id = id;
        //    this.idBBDD = idBBDD;
        //    this.isBlocked = isBlocked;
        //    this.propiedades = propiedades;
        //    this.propiedadesCV = propiedadesCV;
        //}
        
        //public SubseccionItem(int id, string idBBDD, List<EntityPreimport.Property> propiedades, bool isBlocked = false)
        //{
        //    this.id = id;
        //    this.idBBDD = idBBDD;
        //    this.isBlocked = isBlocked;
        //    this.propiedades = propiedades;
        //    this.propiedadesCV = new List<EntityPreimport.Property>();
        //}

        //public SubseccionItem(int id, string idBBDD)
        //{
        //    this.id = id;
        //    this.idBBDD = idBBDD;
        //    this.isBlocked = true;
        //    this.propiedades = new List<EntityPreimport.Property>();
        //    this.propiedadesCV = new List<EntityPreimport.Property>();
        //}

        //public SubseccionItem()
        //{
        //}
    }
}
