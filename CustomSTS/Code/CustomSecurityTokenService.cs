using Microsoft.IdentityModel.SecurityTokenService;
using Microsoft.IdentityModel.Configuration;
using System.ServiceModel;
using System;
using Microsoft.IdentityModel.Claims;
using Microsoft.IdentityModel.Protocols.WSTrust;
using System.Configuration;
using System.Xml;
using System.Web.Configuration;

namespace CustomSTS.Code
{
    public class CustomSecurityTokenService : SecurityTokenService
    {
        public CustomSecurityTokenService(SecurityTokenServiceConfiguration configuration) : base(configuration)
        {
        }

        void ValidateAppliesTo(EndpointAddress appliesTo)
        {
            if (appliesTo == null)
            {
                throw new ArgumentNullException("appliesTo");
            }
        }

        protected override Scope GetScope(IClaimsPrincipal principal, RequestSecurityToken request)
        {
            ValidateAppliesTo(request.AppliesTo);

            Scope scope = new Scope(request.AppliesTo.Uri.OriginalString, SecurityTokenServiceConfiguration.SigningCredentials);
            scope.TokenEncryptionRequired = false;

            return scope;
        }

        protected override IClaimsIdentity GetOutputClaimsIdentity(IClaimsPrincipal principal, RequestSecurityToken request, Scope scope)
        {
            if (null == principal)
            {
                throw new ArgumentNullException("principal");
            }

            ClaimsIdentity outputIdentity = new ClaimsIdentity();

            // Provide the same information as Active Directory - this makes it possible to reuse the same claim transformation rules
            outputIdentity.Claims.Add(new Claim(ClaimTypes.Name, principal.Identity.Name));
            outputIdentity.Claims.Add(new Claim(ClaimTypes.WindowsAccountName, principal.Identity.Name));

            // Needed to make ADFS 1.x work with ADFS claims transformations, if this is not present the authentication section will not be generated in the SAML 1.x message : http://weblogs.asp.net/cibrax/archive/2010/08/27/some-wif-interop-gotchas.aspx
            Claim nameIdentifier = new Claim(ClaimTypes.NameIdentifier, principal.Identity.Name); 
            nameIdentifier.Properties["http://schemas.xmlsoap.org/ws/2005/05/identity/claimproperties/format"] = "http://schemas.xmlsoap.org/claims/UPN";
            outputIdentity.Claims.Add(nameIdentifier);

            // We add this to make the response more standard, but I don't know if this is actually need to make ADFS 2.0 transform to a valid SAML 1.x message
            outputIdentity.Claims.Add(new Claim(ClaimTypes.AuthenticationMethod, AuthenticationMethods.Password));
            outputIdentity.Claims.Add(new Claim(ClaimTypes.AuthenticationInstant, XmlConvert.ToString(DateTime.UtcNow, "yyyy-MM-ddTHH:mm:ss.fffZ"), ClaimValueTypes.Datetime));

            // Pass through any existing claims (here: passed from the custom token handler)
            foreach (var claim in principal.Identities[0].Claims)
            {
                outputIdentity.Claims.Add(new Claim(claim.ClaimType, claim.Value));
            }

            return outputIdentity;
        }
    }
}