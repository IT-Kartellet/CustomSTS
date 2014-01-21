using System;
using System.Collections.Generic;
using System.Configuration;
using System.DirectoryServices.Protocols;
using System.Linq;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace CustomSTS.Code
{
    public class AccountUtils
    {

        private static string GetSHA1(string input)
        {
            // Create a new instance of SHA-1
            SHA1 sha1 = SHA1.Create();

            // Convert the input string to a byte array
            byte[] hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(input));

            // Create a new StringBuilder to create the output string
            StringBuilder sb = new StringBuilder();

            // Loop for each byte
            for (int i = 0; i < hash.Length; i++)
            {
                sb.Append(hash[i].ToString());
            }

            // Return the hexadecimal string
            return sb.ToString();
        }

        // Get the LDAP connection string from Web.config and return it as an Uri object
        public static Uri GetLDAPUri()
        {
            Uri ldapUri = null;
            string connString = null;

            if (0 < ConfigurationManager.ConnectionStrings.Count)
            {
                connString = ConfigurationManager.ConnectionStrings["LDAPConnectionString"].ConnectionString;
                if (null != connString)
                    ldapUri = new Uri(connString);
            }
            else
            {
                throw new Exception("No connection strings found in Web.config.");
            }

            return ldapUri;
        }

        // Open and return an admin connection to LDAP
        public static LdapConnection GetAdminConnection(Uri ldapUri)
        {
            LdapConnection conn = null;
            string adminUsername = ConfigurationManager.AppSettings["adamAdminUser"];
            string adminPassword = ConfigurationManager.AppSettings["adamAdminPassword"];
            string baseDN = ldapUri.LocalPath.Substring(1);
            conn = LDAPUtils.LDAPAccount.LdapConnectBind(ldapUri, "CN=" + adminUsername + "," + baseDN, adminPassword);
            return conn;
        }

        // Convert a string to a byte array regardless of encoding
        private static byte[] GetBytes(string str)
        {
            byte[] bytes = new byte[str.Length * sizeof(char)];
            System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
            return bytes;
        }

        // Convert a byte array to a string regardless of encoding
        private static string GetString(byte[] bytes)
        {
            char[] chars = new char[bytes.Length / sizeof(char)];
            System.Buffer.BlockCopy(bytes, 0, chars, 0, bytes.Length);
            return new string(chars);
        }

        // Fetch the "password last changed" value for a specific LDAP user
        public static long GetPwdLastSet(String username)
        {
            string sResult = getUserAttribute(username, "pwdLastSet");

            // Convert from ANSI string to UNIX long
            long ticks = long.Parse(sResult);
            TimeSpan ts = (new DateTime(1601, 01, 02).AddTicks(ticks) - new DateTime(1970, 1, 1, 0, 0, 0));

            return (long)ts.TotalSeconds;
        }

        public static string GetEmail(String username)
        {
            return getUserAttribute(username, "mail");
        }

        private static string getUserAttribute(string username, string attributeName)
        {
            // Get the LDAP connection URI and the base DN
            Uri ldapUri = GetLDAPUri();
            string baseDN = ldapUri.LocalPath.Substring(1);

            // Get an administrator LDAP connection
            LdapConnection conn = GetAdminConnection(ldapUri);

            // Create a search request for the specified user
            SearchRequest request = new SearchRequest(baseDN, "(name=" + username + ")", System.DirectoryServices.Protocols.SearchScope.Subtree, attributeName);

            // Execute the search request
            SearchResponse response = (SearchResponse)conn.SendRequest(request);

            // Fail if the search outputs more than one result (this shouldn't happen)
            if (1 < response.Entries.Count)
            {
                throw new Exception("Multiple results. Try again.");
            }

            // Get the attribute from the search response and close the connection
            SearchResultEntry entry = response.Entries[0];
            DirectoryAttribute attr = entry.Attributes[attributeName];
            conn.Dispose();

            return (string)attr[0];
        }

        public static string GetResetKey(string username)
        {
            // Get the PwdLastSet value for the user
            long pwdLastSet = GetPwdLastSet(username);

            // Compute expire UNIX timestamp based on expire value in Web.config
            long expireValue = Convert.ToInt64(ConfigurationManager.AppSettings["PasswordResetExpire"]);
            TimeSpan ts = (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0));
            long expireTime = (long)ts.TotalSeconds + expireValue;

            // Get a byte array consisting of username + password last changed value + reset link expire time
            return String.Format("{0}:{1}:{2}", username, pwdLastSet, expireTime); //TODO: What is username contains :
        }

        public static string Encode64(string input)
        {
            byte[] toEncodeAsBytes = GetBytes(input);
            string output = Convert.ToBase64String(toEncodeAsBytes);
            return output;
        }

        public static string Decode64(string input)
        {
            byte[] encodedDataAsBytes = Convert.FromBase64String(input);
            string output = GetString(encodedDataAsBytes);
            return output;
        }
    }
}