using System;
using System.Web;
using Microsoft.IdentityModel.Protocols.WSFederation.Metadata;
using CustomSTS.Code;
using System.Web.Configuration;
using Microsoft.IdentityModel.Protocols.WSIdentity;
using System.Collections.Generic;
using System.IO;
using System.IdentityModel.Tokens;
using System.Text;
using System.Xml;
using Microsoft.IdentityModel.Protocols.WSFederation;
using Microsoft.IdentityModel.Claims;
using System.ServiceModel.Description;
using System.ServiceModel;
using System.ServiceModel.Channels;
using Microsoft.IdentityModel.SecurityTokenService;
using System.Security.Cryptography.X509Certificates;


namespace CustomSTS.FederationMetadata._2007_06
{
    public class FederationMetadata : IHttpHandler
    {
        public static readonly Uri HttpPost = new Uri("urn:oasis:names:tc:SAML:2.0:bindings:HTTP-POST");
        public static readonly Uri HttpRedirect = new Uri("urn:oasis:names:tc:SAML:2.0:bindings:HTTP-Redirect");
        public static readonly Uri Saml20Protocol = new Uri("urn:oasis:names:tc:SAML:2.0:protocol");
        public static readonly Uri RootUrl = new Uri(WebConfigurationManager.AppSettings["BaseAddress"]);

        public void ProcessRequest(HttpContext context)
        {
            string endpointId = UriHelper.ResolveServerUrl("~/", RootUrl);
            EntityDescriptor metadata = new EntityDescriptor();
            metadata.EntityId = new EntityId(CustomSecurityTokenServiceConfiguration.CustomSTSID);

            // Define the signing key
            X509Certificate2 cert = CertificateUtil.GetCertificateFromStore(WebConfigurationManager.AppSettings["CertificateSubjectName"]);
            metadata.SigningCredentials = new X509SigningCredentials(cert, "http://www.w3.org/2000/09/xmldsig#rsa-sha1", "http://www.w3.org/2000/09/xmldsig#sha1");

            // Create role descriptor for security token service
            SecurityTokenServiceDescriptor stsRole = new SecurityTokenServiceDescriptor();
            stsRole.ProtocolsSupported.Add(new Uri(WSFederationMetadataConstants.Namespace));
            metadata.RoleDescriptors.Add(stsRole);

            // Add a contact name
            ContactPerson person = new ContactPerson(ContactType.Administrative);
            person.GivenName = "CustomSTS";
            stsRole.Contacts.Add(person);

            // Include key identifier for signing key in metadata
            SecurityKeyIdentifierClause clause = new X509RawDataKeyIdentifierClause(cert);
            SecurityKeyIdentifier ski = new SecurityKeyIdentifier(clause);
            KeyDescriptor signingKey = new KeyDescriptor(ski);
            signingKey.Use = KeyType.Signing;
            stsRole.Keys.Add(signingKey);

            // Add endpoints
            string activeSTSUrl = UriHelper.ResolveServerUrl("~/", RootUrl);
            string activeMetadata = UriHelper.ResolveServerUrl("~/FederationMetadata/2007-06/FederationMetadata.ashx", RootUrl);
            EndpointAddress endpointAddress = new EndpointAddress(new Uri(activeSTSUrl), null, null, GetMetadataReader(activeMetadata), null);
            stsRole.SecurityTokenServiceEndpoints.Add(endpointAddress);
            stsRole.PassiveRequestorEndpoints.Add(endpointAddress);

            // spsso, ipsso bindings
            ServiceProviderSingleSignOnDescriptor spsso = new ServiceProviderSingleSignOnDescriptor();
            spsso.ProtocolsSupported.Add(Saml20Protocol);
            spsso.Keys.Add(signingKey);
            spsso.SingleLogoutServices.Add(new ProtocolEndpoint(HttpPost, new Uri(activeSTSUrl)));
            spsso.SingleLogoutServices.Add(new ProtocolEndpoint(HttpRedirect, new Uri(activeSTSUrl)));
            spsso.AssertionConsumerService.Add(0, new IndexedProtocolEndpoint(0, HttpPost, new Uri(activeSTSUrl)));
            spsso.AssertionConsumerService.Add(1, new IndexedProtocolEndpoint(1, HttpRedirect, new Uri(activeSTSUrl)));
            metadata.RoleDescriptors.Add(spsso);
            IdentityProviderSingleSignOnDescriptor ipsso = new IdentityProviderSingleSignOnDescriptor();
            ipsso.ProtocolsSupported.Add(Saml20Protocol);
            ipsso.Keys.Add(signingKey);
            ipsso.SingleSignOnServices.Add(new ProtocolEndpoint(HttpPost, new Uri(activeSTSUrl)));
            ipsso.SingleSignOnServices.Add(new ProtocolEndpoint(HttpRedirect, new Uri(activeSTSUrl)));
            ipsso.SingleLogoutServices.Add(new ProtocolEndpoint(HttpPost, new Uri(activeSTSUrl)));
            ipsso.SingleLogoutServices.Add(new ProtocolEndpoint(HttpRedirect, new Uri(activeSTSUrl)));
            metadata.RoleDescriptors.Add(ipsso);

            stsRole.ClaimTypesOffered.Add(new DisplayClaim(ClaimTypes.Name));
            stsRole.ClaimTypesOffered.Add(new DisplayClaim(ClaimTypes.NameIdentifier));
            stsRole.ClaimTypesOffered.Add(new DisplayClaim(ClaimTypes.Role));
            stsRole.ClaimTypesOffered.Add(new DisplayClaim(ClaimTypes.WindowsAccountName));
            stsRole.ClaimTypesOffered.Add(new DisplayClaim(ClaimTypes.PrimarySid));
            stsRole.ClaimTypesOffered.Add(new DisplayClaim(ClaimTypes.GroupSid));
            stsRole.ClaimTypesOffered.Add(new DisplayClaim(ClaimTypes.PrimaryGroupSid));
            stsRole.ClaimTypesOffered.Add(new DisplayClaim(ClaimTypes.DenyOnlyPrimaryGroupSid));
            stsRole.ClaimTypesOffered.Add(new DisplayClaim(ClaimTypes.DenyOnlyPrimarySid));
            stsRole.ClaimTypesOffered.Add(new DisplayClaim(ClaimTypes.DenyOnlySid));
            stsRole.ClaimTypesOffered.Add(new DisplayClaim(ClaimTypes.Upn));
            stsRole.ClaimTypesOffered.Add(new DisplayClaim(ClaimTypes.Email));
            stsRole.ClaimTypesOffered.Add(new DisplayClaim(ClaimTypes.AuthenticationInstant));
            stsRole.ClaimTypesOffered.Add(new DisplayClaim(ClaimTypes.AuthenticationMethod));

            MetadataSerializer serializer = new MetadataSerializer();
            serializer.WriteMetadata(context.Response.OutputStream, metadata);
            context.Response.ContentType = "text/xml";
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Create a reader to provide simulated Metadata endpoint configuration element
        /// </summary>
        /// <param name="activeSTSUrl">The active endpoint URL.</param>
        private static XmlDictionaryReader GetMetadataReader(string activeSTSUrl)
        {
            MetadataSet metadata = new MetadataSet();
            MetadataReference mexReferece = new MetadataReference(new EndpointAddress(activeSTSUrl + "/mex"), AddressingVersion.WSAddressing10);
            MetadataSection refSection = new MetadataSection(MetadataSection.MetadataExchangeDialect, null, mexReferece);
            metadata.MetadataSections.Add(refSection);

            byte[] metadataSectionBytes;
            StringBuilder stringBuilder = new StringBuilder();
            using (StringWriter stringWriter = new StringWriter(stringBuilder))
            {
                using (XmlTextWriter textWriter = new XmlTextWriter(stringWriter))
                {
                    metadata.WriteTo(textWriter);
                    textWriter.Flush();
                    stringWriter.Flush();
                    metadataSectionBytes = stringWriter.Encoding.GetBytes(stringBuilder.ToString());
                }
            }

            return XmlDictionaryReader.CreateTextReader(metadataSectionBytes, XmlDictionaryReaderQuotas.Max);
        }

    }
}
