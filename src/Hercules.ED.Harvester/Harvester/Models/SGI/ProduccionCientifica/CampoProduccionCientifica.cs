using System.Collections.Generic;

namespace OAI_PMH.Models.SGI.ProduccionCientifica
{
    public class CampoProduccionCientifica : SGI_Base
    {
        public string CodigoCVN { get; set; }
        public List<string> Valores { get; set; }
    }
}
