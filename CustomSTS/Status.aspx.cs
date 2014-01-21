using Microsoft.IdentityModel.Protocols.WSTrust;
using Microsoft.IdentityModel.Protocols.WSTrust.Bindings;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Management;
using System.Net;
using System.Net.Security;
using System.ServiceModel;
using System.Threading;
using System.Web;
using System.Web.Configuration;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml;

namespace CustomSTS
{
    public partial class Status : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Request.QueryString["adfs"] != null)
            {
                IgnoreSSLTrust();
                Response.StatusCode = 200;
                using (WebClient client = new WebClient())
                {
                    var xmldocument = client.DownloadString("https://localhost/FederationMetadata/2007-06/FederationMetadata.xml");
                    if (!IsValidXml(xmldocument))
                    {
                        throw new Exception("Not getting a valid XML file from ADFS: https://localhost/FederationMetadata/2007-06/FederationMetadata.xml");
                    }
                }
            }

            if (Request.QueryString["sts"] != null)
            {
                IgnoreSSLTrust();
                SecurityToken testToken = null;
                Response.StatusCode = 200;

                testToken = GetTokenFromSTS(WebConfigurationManager.AppSettings["CheckSTSUserName"], WebConfigurationManager.AppSettings["CheckSTSPassword"]);
                if (null == testToken)
                {
                    Response.StatusCode = 500;
                }
            }

            Response.Write(DateTime.Now.ToString() + "<br />\n");
            Response.Write(String.Format("Hostname: {0}<br />\n", Environment.MachineName));

            int logicalProcessors = System.Environment.ProcessorCount;
            int maxworkerThreads;
            int maxportThreads;
            ThreadPool.GetMaxThreads(out maxworkerThreads, out maxportThreads);
            int minworkerThreads;
            int minportThreads;
            ThreadPool.GetMinThreads(out minworkerThreads, out minportThreads);
            int conLimit = System.Net.ServicePointManager.DefaultConnectionLimit;
            int availworkerThreads;
            int availportThreads;
            ThreadPool.GetAvailableThreads(out availworkerThreads, out availportThreads);

            Response.Write(String.Format("Number of logical Processors: {0}<br />\n", logicalProcessors));
            Response.Write(String.Format("MaxThreads(worker threads): {0}<br />\n", maxworkerThreads));
            Response.Write(String.Format("MaxThreads(port threads): {0}<br />\n", maxportThreads));
            Response.Write(String.Format("MinThreads(worker threads): {0}<br />\n", minworkerThreads));
            Response.Write(String.Format("MinThreads(port threads): {0}<br />\n", minportThreads));
            Response.Write(String.Format("AvailableThreads(worker threads): {0}<br />\n", availworkerThreads));
            Response.Write(String.Format("AvailableThreads(port threads): {0}<br />\n", availportThreads));
            Response.Write(String.Format("DefaultConnectionLimit: {0}<br />\n", conLimit));

            /*
            try
            {
                ManagementObjectSearcher searcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_ComputerSystem");
                
                UInt64 TotalPhysicalMemory = 0;
                foreach (ManagementObject queryObj in searcher.Get())
                {
                    TotalPhysicalMemory = (UInt64)queryObj["TotalPhysicalMemory"];
                }
                Response.Write(String.Format("Total Physical Memory: {0:0.00} GB<br />\n", TotalPhysicalMemory / 1024.0 / 1024.0 / 1024.0));

                searcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_PerfFormattedData_Counters_ProcessorInformation WHERE NOT Name='0,_Total'");
                foreach (ManagementObject queryObj in searcher.Get())
                {
                    Response.Write(String.Format("CPU {0}: {1}<br />\n", queryObj["Name"].ToString() == "_Total" ? "ALL" : queryObj["Name"], queryObj["PercentProcessorTime"]));
                }
            }
            catch (ManagementException)
            {
            }
            */
            
        }

        private static void IgnoreSSLTrust()
        {
            // Change SSL checks so that all checks pass
            ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(delegate { return true; });
        }

        public bool IsValidXml(string xmldata)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xmldata);
            return true;
        }

        private SecurityToken GetTokenFromSTS(string UserName, string Password)
        {
            EndpointAddress endpointAddress = new EndpointAddress(WebConfigurationManager.AppSettings["CheckSTSAddress"]);
            UserNameWSTrustBinding binding = new UserNameWSTrustBinding(SecurityMode.TransportWithMessageCredential);

            WSTrustChannelFactory factory = new WSTrustChannelFactory(binding, endpointAddress);
            factory.Credentials.UserName.UserName = UserName;
            factory.Credentials.UserName.Password = Password;
            factory.TrustVersion = System.ServiceModel.Security.TrustVersion.WSTrustFeb2005;
            // Don't check CRL lists for our self-signed certificate
            factory.Credentials.ServiceCertificate.Authentication.RevocationMode = System.Security.Cryptography.X509Certificates.X509RevocationMode.NoCheck;

            WSTrustChannel channel = (WSTrustChannel)factory.CreateChannel();

            RequestSecurityToken rst = new RequestSecurityToken(WSTrustFeb2005Constants.RequestTypes.Issue, WSTrustFeb2005Constants.KeyTypes.Bearer);
            rst.AppliesTo = new EndpointAddress(WebConfigurationManager.AppSettings["CheckSTSAddress"]);

            SecurityToken token = channel.Issue(rst);
            return token;
        }
    }
}