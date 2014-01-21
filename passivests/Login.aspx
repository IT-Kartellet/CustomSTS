<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Login.aspx.cs" Inherits="CustomSTS.Login" %>

<%@ Register assembly="Microsoft.IdentityModel, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35" namespace="Microsoft.IdentityModel.Web.Controls" tagprefix="wif" %>
<%@ Import Namespace="System.ComponentModel" %>
<%@ Import Namespace="System.Data" %>
<%@ Import Namespace="System.Drawing" %>
<%@ Import Namespace="System.Web" %>
<%@ Import Namespace="System.Web.SessionState" %>
<%@ Import Namespace="System.Web.UI" %>
<%@ Import Namespace="System.Web.UI.WebControls" %>
<%@ Import Namespace="System.Web.UI.HtmlControls" %>
<%@ Import Namespace="System.Web.Security" %>
<%@ Import Namespace="SwivelSecure.PINsafe" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
    <title>Credential Collection</title>
    <meta name="robots" content="noindex, nofollow" />
<style type="text/css">
<!--
body {
	margin: 0px;
	background-image: url('bg-main.png');
	background-repeat: repeat-x;
}
p { font-family: Arial, Helvetica, sans-serif; font-size: 12px; }
h3 { font-family: Arial, Helvetica, sans-serif; font-size: 16px; }
input { font-size: 12px; }

a:link {
	font-family: Arial, Helvetica, sans-serif;
	font-size: 12px;
	color: #0095da;
	text-decoration: underline;
}
a:visited {
	font-family: Arial, Helvetica, sans-serif;
	font-size: 12px;
	color: #0095da;
	text-decoration: underline;
}
a:hover {
	font-family: Arial, Helvetica, sans-serif;
	font-size: 12px;
	color: #0095da;
	text-decoration: underline;
}

-->
</style>
<script type="text/javascript" language="javascript">
<!--
    function init() {document.forms[0].Username.focus();}
-->
</script>

<script type="text/javascript" language="javascript">
    function showTuring()
    {
        var username = document.getElementById("Username");
        if (username && username.value) {
            var image = document.getElementById("imgTuring");
            if (image) {
                image.src = "SCImage.aspx?username=" + username.value + "&random=" + Math.round((new Date()).getTime() / 1000);
                image.style.display = "";
            }
        }
    }

    function checkEnter(e){ //e is event object passed from function invocation
        var characterCode; // literal character code will be stored in this variable
        if(e && e.which){ //if which property of event object is supported (NN4)
            e = e;
            characterCode = e.which; //character code is contained in NN4's which property
        }
        else{
            e = event;
            characterCode = e.keyCode; //character code is contained in IE's keyCode property
        }

        if(characterCode == 13){ //if generated character code is equal to ascii 13 (if enter key)
            //alert("3");
            document.getElementById("UsernamePasswordButton").onclick(); //submit the form
            return false;
        }
        else{
            return true;
        }
    }

    function TogglePasswordReset()
    {
        var table = document.getElementById("passwordResetTable");
        var button = document.getElementById("UsernamePasswordButton");
        if(table.style.display == "none")
        {
            table.style.display = "";
            button.disabled = true;
        }
        else
        {
            table.style.display = "none";
            button.disabled = false;
        }
    }
</script>
</head>

<script runat="server">
string m_errorStatus = null;
string m_message = "";
string m_callingIP = "";
string m_RemoteAddress = "";
string m_debugMessage = "";
bool m_debug = false;

private PINsafeClient client;

protected void Page_Load(object sender, EventArgs e)
{
	try
	{
		string internalIpPrefix = ConfigurationManager.AppSettings["internalIPPrefix"];
		string testersIpPrefix = ConfigurationManager.AppSettings["testersIPPrefix"];
		string callingIp = Request.UserHostAddress;
	 	m_callingIP = callingIp;
		m_RemoteAddress = Request.ServerVariables["HTTP_CLIENT_IP"];
		if(m_RemoteAddress == null || m_RemoteAddress == "") m_RemoteAddress = callingIp;
    
    // Redirect to NTLM authentication for internal users
		if(internalIpPrefix != null && m_RemoteAddress.Substring(0, internalIpPrefix.Length) == internalIpPrefix)
		{
			Context.Response.Redirect("auth/integrated/"+Context.Request.Url.Query);
		}
		
		// Application testers may be on a different subnet (IBM India), but still have domain access, so redirect
		// if client IP is in the testers range. Key MUST be removed from config in production
		if(testersIpPrefix != null && m_RemoteAddress.Substring(0, testersIpPrefix.Length) == testersIpPrefix)
		{
			Context.Response.Redirect("auth/integrated/"+Context.Request.Url.Query);
		}
	}
	catch(Exception){}

	PINsafeSettings settings = new PINsafeSettings();
	client = new PINsafeClient(settings);
}


override protected void OnInit(EventArgs e)
{
    this.PreRenderComplete +=new System.EventHandler(this.Page_OnPreRenderComplete);
    base.OnInit(e);
}

public void Page_OnPreRenderComplete(object sender, EventArgs e)
{
    Username.Attributes.Add("onblur","javascript: showTuring();");
    Password.Attributes.Add("onKeyPress","javascript: checkEnter(event);");
    OTC.Attributes.Add("onKeyPress","javascript: checkEnter(event);");


}

// We process any error that comes back from the account store. For active
// directory, we can extract the Win32 error code and generate a friendly
// message. Different processing may be useful for other account store types.
private void SetErrorStatus(Exception e)
{
   m_debugMessage = e.Message;
   m_errorStatus = e.Message;

   if(e.InnerException != null)
   {
	m_debugMessage += ";" + e.InnerException.Message;
   }


}

private void SignInButtonClick(object sender, EventArgs e)
{

   try
   {
	bool pinSafeLoginSuccessful = client.Login(Username.Text, "", OTC.Text);

	if(pinSafeLoginSuccessful)
	{
		FormsAuthentication.SetAuthCookie(Username.Text, false);
	     //   logonServer.LogonClient(creds);
	}
	else
	{
		m_debugMessage = "PINsafe failed.";
		m_errorStatus = "PINsafe failed.";
	}
   }
   catch(Exception ex)
   {
        //lblError.Text = ex.Message + "<br/>" + ex.Source + "<br/>" + ex.StackTrace;
   	SetErrorStatus(ex);
   }

}

private void PasswordResetButtonClick(object sender, EventArgs e)
{

}

</script>

<body onload="showTuring()">

<!--<pre><%=Context.Request.Url.GetLeftPart(UriPartial.Path)%>


<form id="CredentialCollection" method="post" runat="server" autocomplete="off">

<center>
<table cellspacing="0" cellpadding="0" border="0" width="980">
<tr><td align="left" valign="middle"><img src="log_custom.gif" border="0"/></h3>
<tr><td align="center" valign="middle"><!--<h3>Custom Portal</h3>-->

<% if(m_errorStatus != null && m_message == "")
   {
%>     <p style="color:red">There was an error processing your credentials: Invalid username, password or OTC.</p>
       <asp:Label ID="lblError" runat="server"/>
<% } %>
<p><%=m_message%></p>
<% if(m_debug) {%>
<p>Calling IP: <%=m_callingIP%></p>
<p>Remote IP: <%=m_RemoteAddress%></p>
<p>Debug Message: <%=m_debugMessage%></p>

<%
foreach (string x in Request.ServerVariables)
     {
        //Response.Write(x.ToString() + " : ");
        //Response.Write(Request.ServerVariables[x].ToString() + "<br/>");
     }
//Response.Write("Headers<br/><br/><br/>");
foreach (string key in Request.Headers.Keys)
     {
        //Response.Write(key.ToString() + " : ");
        //Response.Write(Request.Headers[key].ToString() + "<br/>");
     }

%>

<% } %>

    <%-- RP to STS local
    <wif:FederatedPassiveSignIn ID="FederatedPassiveSignIn1" 
            runat="server"
            DisplayRememberMe="false" 
            RequireHttps="false" 
            Realm="http://localhost:8091/Login.aspx"
            Issuer="http://localhost:8090/Default.aspx"
            VisibleWhenSignedIn="false">
    </wif:FederatedPassiveSignIn> --%>
    <%-- RP to STS over SSL 
    <wif:FederatedPassiveSignIn ID="FederatedPassiveSignIn1" 
            runat="server"
            DisplayRememberMe="false" 
            RequireHttps="false" 
            Realm="https://forward1.it-kartellet.dk/Login.aspx"
            Issuer="https://sts.it-kartellet.dk/customsts/Default.aspx"
            VisibleWhenSignedIn="false">
    </wif:FederatedPassiveSignIn>  --%>    

    <%-- RP to STS over SSL --%>
    <wif:FederatedPassiveSignIn ID="FederatedPassiveSignIn1" 
            runat="server"
            DisplayRememberMe="false" 
            RequireHttps="false" 
            Realm="https://forward1.it-kartellet.dk/Login.aspx"
            Issuer="https://adfs2.it-kartellet.dk/adfs/ls"
            VisibleWhenSignedIn="false">
    </wif:FederatedPassiveSignIn>

    <table cellspacing="1" cellpadding="0" style="border: 1px solid #DDDDDD"><tr><td>

<table cellspacing="0" cellpadding="5" border="0" >
<tr><td><p>Username</p></td><td><asp:TextBox runat="server" id="Username" width="150px" /></td></tr>
<tr><td><p>Password</p></td><td><asp:TextBox runat="server" id="Password" TextMode="Password" width="150px" /></td></tr>
<tr><td><p>One-time code</p></td><td><asp:TextBox runat="server" id="OTC" TextMode="Password" width="150px" /></td></tr>
<tr><td><input type="button" onclick="javascript:showTuring();" value="New OTC image" /></td>
<td align="right"><asp:Button runat="server" OnClick="SignInButtonClick" id="UsernamePasswordButton" Text="Sign in" /></td></tr>
<tr><td colspan="2"><img id="imgTuring" src="" alt="Turing image" style="display:none"/></td></tr>
<tr><td colspan="2"><a href="passwordreset.aspx">Forgot your password?</a></td></tr>
<tr><td colspan="2"><a href="https://pinsafe.maersk.com/PINsafe/reset.asp" target="_blank">Forgot your PIN code?</a></td></tr>
<tr><td colspan="2"><a href="https://pinsafe.maersk.com/PINsafe/changepin.asp" target="_blank">Change your PIN code?</a></td></tr>
<tr><td colspan="2"><a href="https://pinsafe.maersk.com/help" target="_blank">PIN code User Guide</a></td></tr>
<tr><td colspan="2"><a href="requestaccess.aspx">Don’t have access to our site? Request access here</a></td></tr>
</table>

</td></tr>
</table>
<br />
<table cellspacing="1" cellpadding="0" style="border: 1px solid #DDDDDD; display: none" id="passwordResetTable" width="320px"><tr><td>

<table cellspacing="0" cellpadding="5" border="0" >
<tr><td colspan="2"><p>TODO: Enter your email address and press submit.</p></td></tr>
<tr><td><p>Email</p></td><td><asp:TextBox runat="server" id="Email" width="150px" /></td></tr>
<tr><td></td><td align="right"><asp:Button runat="server" OnClick="PasswordResetButtonClick" id="PasswordResetButton" Text="Submit" /></td></tr>
</table>

</td></tr>
</table>
</center>
    <!--
    <p>
&nbsp;&nbsp;&nbsp;
    </p>
    <p>
        <asp:Label ID="UsernameLabel" runat="server" Text="Username" />
        <asp:TextBox ID="UsernameTextBox" runat="server" />
    </p>
    <p>
        <asp:Label ID="PasswordLabel" runat="server" Text="Password" />
        <asp:TextBox ID="PasswordTextBox" TextMode="password" runat="server" />
    </p>
    <p>
        <asp:Label ID="OTCLabel" runat="server" Text="OTC" />
        <asp:TextBox ID="OTCTextBox" runat="server" />
    </p>
        <asp:Button ID="SubmitButton" runat="server" Text="Submit" onclick="SubmitButton_Click" /> -->
    </form>
    </body>
</html>
