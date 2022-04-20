using System.Collections.Generic;

namespace EditorCV.Models.EnvioPRC
{
    public class ProduccionCientifica
    {
        public string idRef { get; set; }
        public string epigrafeCVN { get; set; }
        public string estado { get; set; }
        public List<CampoProduccionCientifica> campos { get; set; }
        public List<Autor> autores { get; set; }
        public List<IndiceImpacto> indicesImpacto { get; set; }
        public List<float> proyectos { get; set; }
        public List<Acreditacion> acreditaciones { get; set; }
    }
}
