using Models;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Hercules.ED.ImportadorWebCV.Models
{
    public class Preimport
    {
        /// <summary>
        /// Array de apartados del CV
        /// </summary>
        public List<Subseccion> secciones { get; set; }

        public Preimport(List<Subseccion> subsecciones)
        {
            this.secciones = subsecciones;
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
        public List<Entity.Property> propiedades { get; set; }

        public SubseccionItem(int id, string idBBDD, List<Entity.Property> propiedades, bool isBlocked = false)
        {
            this.id = id;
            this.idBBDD = idBBDD;
            this.isBlocked = isBlocked;
            this.propiedades = propiedades;
        }

        public SubseccionItem(int id, string idBBDD)
        {
            this.id = id;
            this.idBBDD = idBBDD;
            this.isBlocked = true;
            this.propiedades = new List<Entity.Property>();
        }

        public SubseccionItem()
        {
        }
    }
}
