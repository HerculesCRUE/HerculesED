using System;

namespace EditorCV.Models
{
    public enum FieldTypes
    {
        text,
        number,
        textarea,
        datetime,
        time,
        date,
        select,
        orderedlist,
        list,
        table,
        texteditor,
        option,
        button,
        optgroup,
        checkbox,
        search,
    }

    public enum TypeSelect
    {
        @default,
        fieldset,
        popup,
        collapse,
        collapseItem,
        tabs,
        tab,
    }
}
