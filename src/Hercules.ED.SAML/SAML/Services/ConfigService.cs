using Microsoft.Extensions.Configuration;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hercules_SAML.Services
{
    public class ConfigService
    {
        private string _communityURL { get; set; }
        private string _urlServicioLoginGnoss { get; set; }
        private string value { get; set; }
        private IConfiguration _configuration { get; set; }

        public ConfigService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GetCommunityURL()
        {
            if (string.IsNullOrEmpty(_communityURL))
            {
                string CommunityURL = string.Empty;
                IDictionary environmentVariables = Environment.GetEnvironmentVariables();
                if (environmentVariables.Contains("CommunityURL"))
                {
                    CommunityURL = environmentVariables["CommunityURL"] as string;
                }
                else
                {
                    CommunityURL = _configuration["CommunityURL"];
                }
                _communityURL = CommunityURL;
            }
            return _communityURL;
        }

        public string GetUrlServicioLoginGnoss()
        {
            if (string.IsNullOrEmpty(_urlServicioLoginGnoss))
            {
                string UrlServicioLoginGnoss = string.Empty;
                IDictionary environmentVariables = Environment.GetEnvironmentVariables();
                if (environmentVariables.Contains("UrlServicioLoginGnoss"))
                {
                    UrlServicioLoginGnoss = environmentVariables["UrlServicioLoginGnoss"] as string;
                }
                else
                {
                    UrlServicioLoginGnoss = _configuration["UrlServicioLoginGnoss"];
                }
                _urlServicioLoginGnoss = UrlServicioLoginGnoss;
            }
            return _urlServicioLoginGnoss;
        }
    }
}
