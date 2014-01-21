using System;
using System.IdentityModel.Tokens;
using Microsoft.IdentityModel.Tokens;
using Microsoft.IdentityModel.Claims;
using System.Xml;
using System.Web.Security;

namespace CustomSTS.Code
{
    public class CustomUserNameSecurityTokenHandler : UserNameSecurityTokenHandler
    {
        public override bool CanValidateToken
        {
            get
            {
                return true;
            }
        }

        public override ClaimsIdentityCollection ValidateToken(System.IdentityModel.Tokens.SecurityToken token)
        {
            //Debug.Log("ValidateToken Start");
            UserNameSecurityToken userNameToken = token as UserNameSecurityToken;
            if (userNameToken == null)
                throw new ArgumentException("The security token is not a valid username token.");

            if (Membership.ValidateUser(userNameToken.UserName, userNameToken.Password))
            {
                //Debug.Log("ValidateToken OK");
                IClaimsIdentity identity = new ClaimsIdentity();
                identity.Claims.Add(new Claim(ClaimTypes.Name, userNameToken.UserName));
                return new ClaimsIdentityCollection(new IClaimsIdentity[] { identity });
            }

            //Debug.Log("ValidateToken FAIL");
            throw new SecurityTokenValidationException("Username/password is incorrect in STS.");
        }
    }
}