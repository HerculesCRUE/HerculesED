using System.Collections.Generic;

namespace OpenAireConnect.ROs.OpenAire.Controllers
{
    public class ROOpenAireController : ROOpenAireLogic
    {
        public ROOpenAireController(string baseUri) : base(baseUri)
        {
            this.baseUri = baseUri;

        }
    }
}
