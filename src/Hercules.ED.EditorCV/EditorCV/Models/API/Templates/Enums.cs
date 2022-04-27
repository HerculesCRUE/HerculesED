using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EditorCV.Models.API.Templates
{
    //Contiene las enumerciones disponibles

    /// <summary>
    /// Enumeración con los tipos de presentación de las secciones de las pestañas
    /// </summary>
    public enum TabSectionPresentationType
    {
        //Lista de items
        listitems,
        //Un unico item
        item
    }

    /// <summary>
    /// Enumeración con los tipos disponibles en los listados de items
    /// </summary>
    public enum DataTypeListItem
    {
        //Texto
        text,
        //Número
        number,
        //Fecha
        date,
        //Booleano
        boolean,

    }

    /// <summary>
    /// Enumeración con los tipos disponibles en los datos de edición
    /// </summary>
    public enum DataTypeEdit
    {
        //Texto
        text,
        //Textarea
        textarea,
        //Fecha
        date,
        //Número
        number,
        //Combo de selección
        selectCombo,
        //Entidad auxiliar
        auxEntity,
        //Entidad principal
        entity,
        //Listado de autores
        auxEntityAuthorList,
        //Tesauro
        thesaurus,
        //Imagen
        image,
        //Entidad con autocompletar
        entityautocomplete,
        //Booleano
        boolean,
        //Autorización de proyecto
        projectauthorization

    }

}