using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Microsoft.IdentityModel.Claims;

namespace CustomSTS.SampleRP
{
    public partial class Default : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            GridViewClaims.DataSource = (this.User.Identity as IClaimsIdentity)
                .Claims
                .Select(c =>
                    new {
                        ClaimType = c.ClaimType,
                        ClaimValue = c.Value,
                        ClaimValueType = c.ValueType.Substring(c.ValueType.IndexOf("#") + 1),
                        ClaimSubject = c.Subject != null && !string.IsNullOrEmpty(c.Subject.Name) ? c.Subject.Name : "[none]",
                        ClaimIssuer = c.Issuer ?? "[none]"
                    }
                );
            GridViewClaims.DataBind();
        }
    }
}