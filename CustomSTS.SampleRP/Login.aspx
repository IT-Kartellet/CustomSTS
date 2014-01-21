<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Login.aspx.cs" Inherits="CustomSTS.SampleRP.Login" %>

<%@ Register assembly="Microsoft.IdentityModel, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35" namespace="Microsoft.IdentityModel.Web.Controls" tagprefix="wif" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
    
    </div>
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

    <p>
&nbsp;&nbsp;&nbsp;
    </p>
        <asp:Label ID="UsernameLabel" runat="server" Text="Username"></asp:Label>
        <asp:TextBox ID="UsernameTextBox" runat="server"></asp:TextBox>
    <p>
        <asp:Label ID="PasswordLabel" runat="server" Text="Password"></asp:Label>
        <asp:TextBox ID="PasswordTextBox" TextMode="password" runat="server"></asp:TextBox>
    </p>
    <p>
        <asp:Label ID="OTCLabel" runat="server" Text="OTC"></asp:Label>
        <asp:TextBox ID="OTC" runat="server"></asp:TextBox>
    </p>
        <asp:Button ID="SubmitButton" runat="server" Text="Submit" 
        onclick="SubmitButton_Click" />
    </form>
    </body>
</html>
