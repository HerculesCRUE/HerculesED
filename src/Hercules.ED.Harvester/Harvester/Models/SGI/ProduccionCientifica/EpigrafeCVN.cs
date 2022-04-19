using System.Collections.Generic;

namespace OAI_PMH.Models.SGI.ProduccionCientifica
{
    public class EpigrafeCVN : SGI_Base
    {
        public string Codigo { get; set; }
        public List<string> Campos { get; set; }
    }
}
