using System;
using System.Collections.Generic;

namespace EditorCV.Models.Clases
{
    public class DoctoradosBBDD
    {
        string programaDoctorado { get; set; }
        DateTime fechaTitulacion { get; set; }
        string entidadTituluacion { get; set; }
        string ciudadEntidadTit { get; set; }
        string paisEntiadadTit { get; set; }
        string cAutonEntidadTit { get; set; }
        string entidadTitDEA { get; set; }
        DateTime obtencionDEA { get; set; }
        string tituloTesis { get; set; }
        string calificacionObtenida { get; set; }
        string firmaDirector { get; set; }
        string nombreDirector { get; set; }
        string primApeDirector { get; set; }
        string segunApeDirector { get; set; }
        List<CodirectorTesis> codirectorTesis { get; set; }
        string doctoradoEuropeo { get; set; }
        string mencionCalidad { get; set; }
        string premioExtraordinarioDoctor { get; set; }
        string tituloHomologado { get; set; }

        
    }
}
