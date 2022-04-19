using System.Collections.Generic;

namespace OAI_PMH.Models.SGI.ProduccionCientifica
{
    public class ProduccionCientifica : SGI_Base
    {
        public string IdRef { get; set; }
        public string EpigrafeCVN { get; set; }
        public string Estado { get; set; }
        public List<CampoProduccionCientifica> Campos { get; set; }
        public List<Autor> Autores { get; set; }
        public List<IndiceImpacto> IndicesImpacto { get; set; }
        public List<float> Proyectos { get; set; }
        public List<Acreditacion> Acreditaciones { get; set; }
    }
}
