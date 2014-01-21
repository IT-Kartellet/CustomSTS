<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ResetPassword.aspx.cs" Inherits="CustomSTS.ResetPassword" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
        <p>Please enter your new password:</p>
        <asp:TextBox runat="server" ID="PasswordTextBox" Type="Password" /><br />
        <p>Please repeat your new password:</p>
        <asp:TextBox runat="server" ID="RepeatPasswordTextBox" Type="Password" /><br />
        <asp:Button runat="server" ID="SubmitButton" OnClick="SubmitButton_Click" Text="Change password" /><br />
        <asp:Label runat="server" ID="StatusLabel" />
    </div>
    </form>
</body>
</html>
