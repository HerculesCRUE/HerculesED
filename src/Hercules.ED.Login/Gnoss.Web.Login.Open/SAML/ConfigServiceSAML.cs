using Microsoft.Extensions.Configuration;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Gnoss.Web.Login.Open.SAML
{
    public class ConfigServiceSAML
    {
        private string urlServiceInDomain { get; set; }
        private string saml2_IdPMetadata { get; set; }
        private string saml2_Issuer { get; set;}
        private string saml2_SignatureAlgorithm { get; set; }
        private string saml2_CertificateValidationMode { get; set; }
        private string saml2_RevocationMode { get; set; }

        private IConfiguration _configuration { get; set; }

        public ConfigServiceSAML(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GetUrlServiceInDomain()
        {
            if (string.IsNullOrEmpty(urlServiceInDomain))
            {
                string connectionString = string.Empty;
                IDictionary environmentVariables = Environment.GetEnvironmentVariables();
                if (environmentVariables.Contains("urlServiceInDomain"))
                {
                    connectionString = environmentVariables["urlServiceInDomain"] as string;
                }
                else
                {
                    connectionString = _configuration["urlServiceInDomain"];
                }

                urlServiceInDomain = connectionString;
            }
            return urlServiceInDomain;
        }

        public string GetSaml2_IdPMetadata()
        {
            if (string.IsNullOrEmpty(saml2_IdPMetadata))
            {
                string connectionString = string.Empty;
                IDictionary environmentVariables = Environment.GetEnvironmentVariables();
                if (environmentVariables.Contains("Saml2_IdPMetadata"))
                {
                    connectionString = environmentVariables["Saml2_IdPMetadata"] as string;
                }
                else
                {
                    connectionString = _configuration["Saml2:IdPMetadata"];
                }

                saml2_IdPMetadata = connectionString;
            }
            return saml2_IdPMetadata;
        }

        public string GetSaml2_Issuer()
        {
            if (string.IsNullOrEmpty(saml2_Issuer))
            {
                string connectionString = string.Empty;
                IDictionary environmentVariables = Environment.GetEnvironmentVariables();
                if (environmentVariables.Contains("Saml2_Issuer"))
                {
                    connectionString = environmentVariables["Saml2_Issuer"] as string;
                }
                else
                {
                    connectionString = _configuration["Saml2:Issuer"];
                }

                saml2_Issuer = connectionString;
            }
            return saml2_Issuer;
        }


        public string GetSaml2_SignatureAlgorithm()
        {
            if (string.IsNullOrEmpty(saml2_SignatureAlgorithm))
            {
                string connectionString = string.Empty;
                IDictionary environmentVariables = Environment.GetEnvironmentVariables();
                if (environmentVariables.Contains("Saml2_SignatureAlgorithm"))
                {
                    connectionString = environmentVariables["Saml2_SignatureAlgorithm"] as string;
                }
                else
                {
                    connectionString = _configuration["Saml2:IdPMetadata"];
                }

                saml2_SignatureAlgorithm = connectionString;
            }
            return saml2_SignatureAlgorithm;
        }


        public string GetSaml2_CertificateValidationMode()
        {
            if (string.IsNullOrEmpty(saml2_CertificateValidationMode))
            {
                string connectionString = string.Empty;
                IDictionary environmentVariables = Environment.GetEnvironmentVariables();
                if (environmentVariables.Contains("Saml2_CertificateValidationMode"))
                {
                    connectionString = environmentVariables["Saml2_CertificateValidationMode"] as string;
                }
                else
                {
                    connectionString = _configuration["Saml2:CertificateValidationMode"];
                }

                saml2_CertificateValidationMode = connectionString;
            }
            return saml2_CertificateValidationMode;
        }


        public string GetSaml2_RevocationMode()
        {
            if (string.IsNullOrEmpty(saml2_RevocationMode))
            {
                string connectionString = string.Empty;
                IDictionary environmentVariables = Environment.GetEnvironmentVariables();
                if (environmentVariables.Contains("Saml2_RevocationMode"))
                {
                    connectionString = environmentVariables["Saml2_RevocationMode"] as string;
                }
                else
                {
                    connectionString = _configuration["Saml2:RevocationMode"];
                }

                saml2_RevocationMode = connectionString;
            }
            return saml2_RevocationMode;
        }


    }
}
