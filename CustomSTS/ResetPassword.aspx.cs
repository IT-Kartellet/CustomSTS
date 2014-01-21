using CustomSTS.Code;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.DirectoryServices.Protocols;
using System.Linq;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace CustomSTS
{
    public partial class ResetPassword : System.Web.UI.Page
    {
        private string userName;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(Request.QueryString["key"]))
            {
                throw new Exception("Invalid parameters.");
            }

            // Decode
            string resetKey;
            SafeCrypt sc = new SafeCrypt();
            string encodedResetKey = Request.QueryString["key"];
            try
            {
                resetKey = Encoding.UTF8.GetString(sc.Decode(encodedResetKey));
            }
            catch
            {
                throw new Exception("Invalid reset key (unable to decode).");
            }

            // Parse
            string[] ps = resetKey.Split(':');
            userName = ps[0];
            long keyLastReset = long.Parse(ps[1]);
            long keyExpires = long.Parse(ps[2]);

            // Verify validity
            long lastReset = AccountUtils.GetPwdLastSet(userName);
            if (lastReset != keyLastReset)
            {
                throw new Exception("Reset key has already been used.");
            }
            long unixNow = (long)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds;
            if (unixNow > keyExpires)
            {
                throw new Exception("Reset key has expired.");
            }
        }

        protected void SubmitButton_Click(object sender, EventArgs e)
        {
            if (PasswordTextBox.Text != RepeatPasswordTextBox.Text)
            {
                throw new Exception("Entered passwords do not match.");
            }

            Uri ldapUri = AccountUtils.GetLDAPUri();
            string baseDN = ldapUri.LocalPath.Substring(1);
            LdapConnection ldapConn = AccountUtils.GetAdminConnection(ldapUri);

            try
            {
                LDAPUtils.LDAPAccount.SetPassword(ldapConn, "CN=" + userName + "," + baseDN, PasswordTextBox.Text);
            }
            catch (LdapException)
            {
                throw new Exception("Unable to connect to LDAP");
            }
            StatusLabel.Text = "Password was successfully changed.";
            SubmitButton.Enabled = false;
        }
    }


}