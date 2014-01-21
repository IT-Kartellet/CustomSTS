using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens;
using System.Configuration;

namespace CustomSTS.SampleRP
{
    public class FederatedIssuerNameRegistry : IssuerNameRegistry
    {
        public override string GetIssuerName(System.IdentityModel.Tokens.SecurityToken securityToken)
        {
            X509SecurityToken x509Token = securityToken as X509SecurityToken;
            if (x509Token != null && x509Token.Certificate != null)
            {
                if (string.IsNullOrEmpty(WsFederationThumbprint) || x509Token.Certificate.Thumbprint == WsFederationThumbprint)
                    return WsFederationIssuerName;
            }

            throw new SecurityTokenException("Untrusted issuer.");
        }

        public string WsFederationThumbprint
        {
            get
            {
                return ConfigurationManager.AppSettings["wsFederationThumbprint"];
            }
        }

        public string WsFederationIssuerName
        {
            get
            {
                return ConfigurationManager.AppSettings["wsFederationIssuerName"];
            }
        }
    }
}