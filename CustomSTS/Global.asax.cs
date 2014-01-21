using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Security;
using System.Web.SessionState;

namespace CustomSTS
{
    public class Global : System.Web.HttpApplication
    {

        protected void Application_Start(object sender, EventArgs e)
        {
            System.Threading.Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-US");
            System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");

            // Set some more sane defaults for thread creation and outgoing connecting 
            int logicalProcessors = System.Environment.ProcessorCount;
            ThreadPool.SetMaxThreads(512 * logicalProcessors, 512 * logicalProcessors);
            ThreadPool.SetMinThreads(100 * logicalProcessors, 100 * logicalProcessors);
            System.Net.ServicePointManager.DefaultConnectionLimit = int.MaxValue;

            // Disable CRL checking
            System.Net.ServicePointManager.CheckCertificateRevocationList = false;
        }

        protected void Session_Start(object sender, EventArgs e)
        {

        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            // Disallow connections to the STS that are not comming from localhost
            if (ConfigurationManager.AppSettings["RestrictToLocalhost"] == "true"
                && !HttpContext.Current.Request.IsLocal
                && HttpContext.Current.Request.AppRelativeCurrentExecutionFilePath.ToLower().StartsWith("~/services")
            )    
            {
                Response.Status = "403 Forbidden";
                Response.End();
            }
        }

        protected void Application_AuthenticateRequest(object sender, EventArgs e)
        {

        }

        protected void Application_Error(object sender, EventArgs e)
        {

        }

        protected void Session_End(object sender, EventArgs e)
        {

        }

        protected void Application_End(object sender, EventArgs e)
        {

        }
    }
}