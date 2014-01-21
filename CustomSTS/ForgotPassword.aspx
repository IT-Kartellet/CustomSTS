<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ForgotPassword.aspx.cs" Inherits="CustomSTS.ForgotPassword" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
        <p>Please enter your username:</p>
        <asp:TextBox runat="server" ID="UsernameTextBox" /><br />
        <asp:Button runat="server" ID="SubmitButton" OnClick="SubmitButton_Click" Text="Reset password" /><br />
        <asp:Label runat="server" ID="StatusLabel" />
    </div>
    </form>
</body>
</html>
