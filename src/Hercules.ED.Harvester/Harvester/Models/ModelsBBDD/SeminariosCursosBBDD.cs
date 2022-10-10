using System;

namespace EditorCV.Models.Clases
{
    public class SeminariosCursosBBDD
    {
        string tituloCurso { get; set; }
        string objetivosCurso { get; set; }
        string entidadOrganizadora { get; set; }
        DateTime fechaInicio { get; set; }
        DateTime fechaFin { get; set; }
        int duracionHoras { get; set; }
        string ciudadEntidad { get; set; }
        string paisEntidad { get; set; }
        string cAutonEntidad { get; set; }
        int duracionAnyos { get; set; }
        int duracionMeses { get; set; }
        int duracionDias { get; set; }
        string objetivoEstancia { get; set; }
        string programaFin { get; set; }
        string perfilDestinatario { get; set; }
        string tareasContrastables { get; set; }
    }
}
