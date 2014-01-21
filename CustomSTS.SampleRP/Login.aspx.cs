using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Collections.Specialized;

namespace CustomSTS.SampleRP
{
    public partial class Login : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
           
        }

        protected void SubmitButton_Click(object sender, EventArgs e)
        {
            try
            {
                SignInWithTokenFromOtherSTS( UsernameTextBox.Text, PasswordTextBox.Text );
            }
            catch 
            {     
                try
                {
                    SignIn( UsernameTextBox.Text, PasswordTextBox.Text );
                }
                catch ( AuthenticationFailedException ex )
                {
                    HandleError( ex.Message );
                }
            }       
        }

        private void SignInWithTokenFromOtherSTS(string UserName, string Password)
        {
        }
        private void SignIn(string UserName, string Password)
        {
        }

        private void HandleError(string ex)
        {

        }
        
    }

    public class AuthenticationFailedException : Exception
    {

    }
}