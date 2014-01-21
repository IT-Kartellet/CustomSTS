using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.Security;
using System.Collections.Specialized;
using System.Text;
using CustomSTS.Utils;
using System.ServiceModel;
using Microsoft.IdentityModel.Protocols.WSTrust.Bindings;
using Microsoft.IdentityModel.Protocols.WSTrust;
using System.IdentityModel.Tokens;

namespace CustomSTS
{

    public class AuthenticationFailedException : Exception
    {

    }

    public partial class Login : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            HttpCookie cookie = Request.Cookies.Get("CustomSTSSession");
            SignInWithCookie(cookie);
        }

        //protected void Login1_Authenticate(object sender, AuthenticateEventArgs e)
        //{
        //    if (Membership.ValidateUser(Login1.UserName, Login1.Password))
        //    {
        //        Login1.Visible = true;
        //        Session["user"] = User.Identity.Name;
        //        FormsAuthentication.RedirectFromLoginPage(Login1.UserName, true);
        //    }
        //    else
        //    {
        //        Response.Write("Invalid Login");
        //    }
        //}

        protected void SubmitButton_Click(object sender, EventArgs e)
        {
            try
            {
                SignInWithTokenFromOtherSTS(UsernameTextBox.Text, PasswordTextBox.Text);

                HttpCookie cookie = new HttpCookie("CustomSTSSession");
                SaveCredentialsToCookie(cookie, UsernameTextBox.Text, PasswordTextBox.Text, DateTime.Now.AddHours(1));
            }
            catch
            {
                try
                {
                    //SignIn( UsernameTextBox.Text, PasswordTextBox.Text );
                }
                catch (AuthenticationFailedException ex)
                {
                    HandleError(ex.Message);
                }
            }
        }

        private void SignInWithTokenFromOtherSTS(string UserName, string Password)
        {
            const string OtherSTSAddress = "https://sts.it-kartellet.dk/customsts/services/trust/2005/UserName.svc/Sts";
            const string YourStsAddress = "http://adfs2.it-kartellet.dk/adfs/services/trust";

            EndpointAddress endpointAddress = new EndpointAddress(OtherSTSAddress);
            UserNameWSTrustBinding binding = new UserNameWSTrustBinding(SecurityMode.TransportWithMessageCredential);

            WSTrustChannelFactory factory = new WSTrustChannelFactory(binding, endpointAddress);
            factory.Credentials.UserName.UserName = UserName;
            factory.Credentials.UserName.Password = Password;
            factory.TrustVersion = System.ServiceModel.Security.TrustVersion.WSTrustFeb2005;

            WSTrustChannel channel = (WSTrustChannel)factory.CreateChannel();

            RequestSecurityToken rst = new RequestSecurityToken(WSTrustFeb2005Constants.RequestTypes.Issue, WSTrustFeb2005Constants.KeyTypes.Bearer);
            rst.AppliesTo = new EndpointAddress(YourStsAddress);

            SecurityToken token = channel.Issue(rst);
            SignIn(token);
        }

        private void SignInWithCookie(HttpCookie cookie)
        {
            if (cookie != null)
            {
                try
                {
                    byte[] decryptedBytes = MachineKey.Decode(cookie.Value, MachineKeyProtection.All);
                    string value = Encoding.UTF8.GetString(decryptedBytes);

                    // IndexOf throws an exception if second argument is -1(as in char not found from former search)
                    int timestamp_idx = value.IndexOf(':');
                    int username_idx = value.IndexOf(':', timestamp_idx + 1);

                    if (username_idx > 0)
                    {
                        string str = value.Substring(0, timestamp_idx);
                        DateTime timestamp = new DateTime(1970, 1, 1, 0, 0, 0).AddSeconds(long.Parse(str));
                        string username = value.Substring(timestamp_idx + 1, username_idx - timestamp_idx - 1);
                        string password = value.Substring(username_idx + 1);

                        if (timestamp > DateTime.Now)
                        {
                            SignIn(username, password);
                        }
                        else
                        {
                            throw new Exception("Timestamp expired");
                        }
                    }
                    else
                    {
                        throw new Exception("Missing password field");
                    }
                }
                catch (Exception ex)
                {
                    // Delete cookie if decryption fails
                    HttpCookie c = new HttpCookie("CustomSTSSession");
                    c.Expires = new DateTime(1970, 1, 1, 0, 0, 1); // Set to 1
                    Response.Cookies.Add(c);
                    throw ex;
                }
            }
        }

        private void SaveCredentialsToCookie(HttpCookie cookie, string username, string password, DateTime expires)
        {
            TimeSpan _UnixTimeSpan = (expires - new DateTime(1970, 1, 1, 0, 0, 0));
            string timestamp = ((long)_UnixTimeSpan.TotalSeconds).ToString();

            byte[] plaintextBytes = Encoding.UTF8.GetBytes(timestamp + ":" + username + ":" + password);
            cookie.Value = MachineKey.Encode(plaintextBytes, MachineKeyProtection.All);
            Response.SetCookie(cookie);
        }

        // Test functions
        private void SignIn(string username, string password)
        {
            Response.Write("Go session: " + username + "-" + password);
        }
        private void SignIn(SecurityToken token)
        {
            Response.Write("Go session: with token");
        }
        private void HandleError(String msg)
        {
            Response.Write("Error: " + msg);
        }

    }
}