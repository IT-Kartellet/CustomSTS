using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.SessionState;
using Microsoft.IdentityModel.Web;

namespace CustomSTS.SampleRP
{
    public class Global : System.Web.HttpApplication
    {

        protected void Application_Start(object sender, EventArgs e)
        {
            System.Net.ServicePointManager.ServerCertificateValidationCallback = (a, b, c, d) => true;
            FederatedAuthentication.ServiceConfigurationCreated += (s, fede) =>
            {
                fede.ServiceConfiguration.CertificateValidationMode = System.ServiceModel.Security.X509CertificateValidationMode.None;
            };
        }
    }
}