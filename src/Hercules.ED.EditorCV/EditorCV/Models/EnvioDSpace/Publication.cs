using System.Collections.Generic;

namespace EditorCV.Models.EnvioDSpace
{
    public class Publication
    {
        public Publication(string idRecursoDspace, string tituloRecurso)
        {
            this.idRecursoDspace = idRecursoDspace;
            this.tituloRecurso = tituloRecurso;
            this.autores = new List<string>();
        }
        public Publication()
        {
            this.autores = new List<string>();
        }

        public string idRecursoDspace { get; set; }
        public string tituloRecurso { get; set; }

        public string proyect { get; set; }
        public string anioPublicacion { get; set; }
        public string issn { get; set; }
        public string isbn { get; set; }
        public string handle { get; set; }
        public string descripcion { get; set; }
        public string pagIni { get; set; }
        public string pagFin { get; set; }
        public string editorial { get; set; }
        public string isOpenAccess { get; set; }
        public string tipo { get; set; }

        public List<string> autores { get; set; }
    }
}
