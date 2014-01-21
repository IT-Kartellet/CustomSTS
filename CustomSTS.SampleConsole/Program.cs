using System.ServiceModel;
using Microsoft.IdentityModel.Protocols.WSTrust.Bindings;
using Microsoft.IdentityModel.Protocols.WSTrust;
using System.IdentityModel.Tokens;
using System;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.Xml;
using System.IO;
using System.IO.Compression;
using Microsoft.IdentityModel.Protocols.WSFederation;
using System.ServiceModel.Security.Tokens;
using System.Globalization;
using Microsoft.IdentityModel.Tokens.Saml2;

namespace CustomSTS.SampleConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            SignInWithTokenFromOtherSTS("tlb013", "4");

        }

        private static void SignInWithTokenFromOtherSTS(string UserName, string Password)
        {

            const string OtherSTSAddress = "https://sts.it-kartellet.dk/customsts/services/trust/2005/UserName.svc/Sts";
            const string YourStsAddress = "http://adfs2.it-kartellet.dk/adfs/services/trust";

            EndpointAddress endpointAddress = new EndpointAddress(OtherSTSAddress);
            UserNameWSTrustBinding binding = new UserNameWSTrustBinding(SecurityMode.TransportWithMessageCredential);

            WSTrustChannelFactory factory = new WSTrustChannelFactory(binding, endpointAddress);
            factory.Credentials.UserName.UserName = UserName;
            factory.Credentials.UserName.Password = Password;
            factory.TrustVersion = System.ServiceModel.Security.TrustVersion.WSTrustFeb2005;

            RequestSecurityToken rst = new RequestSecurityToken(WSTrustFeb2005Constants.RequestTypes.Issue, WSTrustFeb2005Constants.KeyTypes.Bearer);
            rst.AppliesTo = new EndpointAddress(YourStsAddress);
            rst.TokenType = "http://docs.oasis-open.org/wss/oasis-wss-saml-token-profile-1.1#SAMLV2.0";

            RequestSecurityTokenResponse rstr;
            WSTrustChannel channel = (WSTrustChannel)factory.CreateChannel();
            SecurityToken token = channel.Issue(rst, out rstr);
            Console.WriteLine(token);

            var lifeTime = rstr.Lifetime;
            var appliesTo = rstr.AppliesTo.Uri;

            var test = token as GenericXmlSecurityToken;
            Console.WriteLine(test);
            Console.WriteLine(rstr.RequestedSecurityToken.SecurityTokenXml.InnerXml);
            var str = Convert.ToBase64String(Encoding.UTF8.GetBytes(rstr.RequestedSecurityToken.SecurityTokenXml.InnerXml));
            Console.WriteLine(test.TokenXml.OuterXml.Length);
            Console.WriteLine(CompressString(test.TokenXml.OuterXml).Length);

            Console.ReadKey();
        }

        public static SecurityToken ToSamlSecurityToken(GenericXmlSecurityToken token)
        {
            var handlers = SecurityTokenHandlerCollection.CreateDefaultSecurityTokenHandlerCollection();
            var reader = new XmlTextReader(new StringReader(token.TokenXml.OuterXml));

            return handlers.ReadToken(reader);
        }

        /// <summary>
        /// Compresses the string.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns></returns>
        public static string CompressString(string text)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(text);
            var memoryStream = new MemoryStream();
            using (var gZipStream = new GZipStream(memoryStream, CompressionMode.Compress, true))
            {
                gZipStream.Write(buffer, 0, buffer.Length);
            }

            memoryStream.Position = 0;

            var compressedData = new byte[memoryStream.Length];
            memoryStream.Read(compressedData, 0, compressedData.Length);

            var gZipBuffer = new byte[compressedData.Length + 4];
            Buffer.BlockCopy(compressedData, 0, gZipBuffer, 4, compressedData.Length);
            Buffer.BlockCopy(BitConverter.GetBytes(buffer.Length), 0, gZipBuffer, 0, 4);
            return Convert.ToBase64String(gZipBuffer);
        }

        /// <summary>
        /// Decompresses the string.
        /// </summary>
        /// <param name="compressedText">The compressed text.</param>
        /// <returns></returns>
        public static string DecompressString(string compressedText)
        {
            byte[] gZipBuffer = Convert.FromBase64String(compressedText);
            using (var memoryStream = new MemoryStream())
            {
                int dataLength = BitConverter.ToInt32(gZipBuffer, 0);
                memoryStream.Write(gZipBuffer, 4, gZipBuffer.Length - 4);

                var buffer = new byte[dataLength];

                memoryStream.Position = 0;
                using (var gZipStream = new GZipStream(memoryStream, CompressionMode.Decompress))
                {
                    gZipStream.Read(buffer, 0, buffer.Length);
                }

                return Encoding.UTF8.GetString(buffer);
            }
        }

      

    }


}
