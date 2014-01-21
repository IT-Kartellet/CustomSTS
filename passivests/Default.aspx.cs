using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.Security;
using Microsoft.IdentityModel.Protocols.WSFederation;
using Microsoft.IdentityModel.SecurityTokenService;
using Microsoft.IdentityModel.Web;
using System.Threading;
using CustomSTS.Code;


// TODO: Handle signout : http://netpl.blogspot.dk/2010/12/wif-ws-federation-and-single-sign-out.html

namespace CustomSTS
{
    public partial class Default : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
          /*  Response.Write("Hello, " + Server.HtmlEncode(User.Identity.Name));

            FormsIdentity id = (FormsIdentity)User.Identity;
            FormsAuthenticationTicket ticket = id.Ticket;

            Response.Write("<p/>TicketName: " + ticket.Name);
            Response.Write("<br/>Cookie Path: " + ticket.CookiePath);
            Response.Write("<br/>Ticket Expiration: " +
                            ticket.Expiration.ToString());
            Response.Write("<br/>Expired: " + ticket.Expired.ToString());
            Response.Write("<br/>Persistent: " + ticket.IsPersistent.ToString());
            Response.Write("<br/>IssueDate: " + ticket.IssueDate.ToString());
            Response.Write("<br/>UserData: " + ticket.UserData);
            Response.Write("<br/>Version: " + ticket.Version.ToString());
          */
        }

        protected void Page_PreRender(object sender, EventArgs e)
        {
            string action = Request.QueryString[WSFederationConstants.Parameters.Action];

            try
            {
                if (action == WSFederationConstants.Actions.SignIn)
                {
                    // Process signin request.
                    SignInRequestMessage requestMessage = (SignInRequestMessage)WSFederationMessage.CreateFromUri(Request.Url);
                    if (User != null && User.Identity != null && User.Identity.IsAuthenticated)
                    {
                        SecurityTokenService sts = new CustomSecurityTokenService(CustomSecurityTokenServiceConfiguration.Current);
                        SignInResponseMessage responseMessage = FederatedPassiveSecurityTokenServiceOperations.ProcessSignInRequest(requestMessage, User, sts);
                        FederatedPassiveSecurityTokenServiceOperations.ProcessSignInResponse(responseMessage, Response);
                    }
                    else
                    {
                        throw new UnauthorizedAccessException();
                    }
                }
                else if (action == WSFederationConstants.Actions.SignOut)
                {
                    // Process signout request.
                    SignOutRequestMessage requestMessage = (SignOutRequestMessage)WSFederationMessage.CreateFromUri(Request.Url);
                    FederatedPassiveSecurityTokenServiceOperations.ProcessSignOutRequest(requestMessage, User, requestMessage.Reply, Response);
                }
            }
            catch (ThreadAbortException) { }
            catch (Exception exception)
            {
                throw new Exception("An unexpected error occurred when processing the request. See inner exception for details.", exception);
            }
        }
    }



}