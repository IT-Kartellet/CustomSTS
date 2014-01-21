using Microsoft.IdentityModel.Configuration;
using System.Web;
using System.Web.Configuration;
using Microsoft.IdentityModel.SecurityTokenService;
using System;

namespace CustomSTS.Code
{

    public class CustomSecurityTokenServiceConfiguration : SecurityTokenServiceConfiguration
    {
        public static readonly object syncRoot = new object();
        public static string CustomSTSID = WebConfigurationManager.AppSettings["IssuerName"] ?? "CustomSTS";
        public static string CustomSTSKey = CustomSTSID + ".key";

        public static CustomSecurityTokenServiceConfiguration Current
        {
            get
            {
                HttpApplicationState httpAppState = HttpContext.Current.Application;
                CustomSecurityTokenServiceConfiguration customConfiguration = httpAppState.Get(CustomSTSKey) as CustomSecurityTokenServiceConfiguration;
                
                if (customConfiguration == null)
                {
                    lock (syncRoot)
                    {
                        customConfiguration = httpAppState.Get(CustomSTSKey) as CustomSecurityTokenServiceConfiguration;

                        if (customConfiguration == null)
                        {
                            customConfiguration = new CustomSecurityTokenServiceConfiguration();
                            httpAppState.Add(CustomSTSKey, customConfiguration);
                        }
                    }
                }

                return customConfiguration;
            }
        }

        public CustomSecurityTokenServiceConfiguration() : base(CustomSTSID, new X509SigningCredentials(CertificateUtil.GetCertificateFromStore(WebConfigurationManager.AppSettings["CertificateSubjectName"]), "http://www.w3.org/2001/04/xmldsig-more#rsa-sha256", "http://www.w3.org/2001/04/xmlenc#sha256"))
        {
            this.SecurityTokenService = typeof(CustomSecurityTokenService);
            this.DefaultTokenLifetime = new TimeSpan(0, 8, 0, 0); // TODO: Read this from web.config
        }
    }
}