using CustomSTS.Code;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.DirectoryServices.Protocols;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace CustomSTS
{
    public partial class ForgotPassword : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            
        }

        protected void SubmitButton_Click(object sender, EventArgs e)
        {
            String username = null;
            if (String.Empty == UsernameTextBox.Text)
            {
                throw new Exception("No username entered.");
            }
            else
            {
                username = UsernameTextBox.Text;
            }

            // Get the base URL to the ResetPassword.aspx file
            Uri baseURI = new Uri(ConfigurationManager.AppSettings["BaseAddress"]);

            // Get reset key for user and encode it
            SafeCrypt sc = new SafeCrypt();
            string resetKey = AccountUtils.GetResetKey(username);
            string encodedResetKey = sc.Encode(Encoding.UTF8.GetBytes(resetKey));

            // Compose URL and send E-mail
            string resetLink = "ResetPassword.aspx?key=" + HttpUtility.UrlEncode(encodedResetKey);
            SendPasswordResetMail(baseURI + resetLink, username);

            // Update page (success)
            StatusLabel.Text = "Password reset mail has been sent.";
            SubmitButton.Enabled = false;
        }

        private void SendPasswordResetMail(string resetURL, string username)
        {
            StringBuilder sb = new StringBuilder();
            string msgBodyFile = HttpRuntime.AppDomainAppPath + "App_Data/ResetMailBody.txt";

            using (StreamReader reader = new StreamReader(msgBodyFile))
            {
                sb.Append(reader.ReadToEnd());
            }

            // Send the message
            System.Net.Mail.MailMessage message = new System.Net.Mail.MailMessage();
            message.To.Add(AccountUtils.GetEmail(username));
            message.From = new System.Net.Mail.MailAddress(ConfigurationManager.AppSettings["PasswordResetEmailFrom"]);
            message.Subject = ConfigurationManager.AppSettings["PasswordResetEmailSubject"];
            message.Body = String.Format(sb.ToString(), resetURL);
            System.Net.Mail.SmtpClient smtp = new System.Net.Mail.SmtpClient(ConfigurationManager.AppSettings["smtpServer"]);
            smtp.Send(message);
        }

    }
}