using System.Collections.Generic;
using GitHubAPI.ROs.Codes.Models;


namespace GitHubAPI.ROs.Codes.Controllers
{

    public class ROGitHubController : ROCodeLogic
    {

        public ROGitHubController(string baseUri, string bareer) : base(baseUri, bareer)
        {
            // baseUri = "https://api.github.com";
            // _bareer = new Guid("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");
            this.baseUri = baseUri;
            this.bareer = bareer;
            headers.Add("User-Agent", "http://developer.github.com/v3/#user-agent-required");
            headers.Add("Accept", "application/vnd.github.mercy-preview+json");
        }

    }
}
