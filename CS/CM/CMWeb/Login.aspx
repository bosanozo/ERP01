<%@ Page Language="C#" MasterPageFile="~/CM.master" AutoEventWireup="true" CodeFile="Login.aspx.cs" Inherits="Login" Title="" %>
<%@ MasterType  virtualPath="~/CM.master"%>
<asp:Content ID="Content1" ContentPlaceHolderID="Content1" Runat="Server">
    <asp:Panel ID="PanelMain" runat="server" Width="100%">
    <center>
        <table>
            <tr>
                <td style="text-align: right; width: 80px;">
                    <strong>
                        <asp:Label ID="LabelID" runat="server" Text="ID"></asp:Label></strong></td>
                <td style="width: 10px">
                </td>
                <td style="text-align: left;">
                    <asp:TextBox ID="TextBoxUserId" runat="server" MaxLength="8" Width="80px" 
                        CssClass="CodeInput"></asp:TextBox></td>
            </tr>
            <tr>
                <td colspan="2"/>
                <td style="text-align: left;">
                    <asp:RequiredFieldValidator ID="UserIdRequired" runat="server" 
                        ControlToValidate="TextBoxUserId" CssClass="failureNotification" 
                        ErrorMessage="IDを入力してください。"></asp:RequiredFieldValidator>
                </td>
            </tr>
            <tr>
                <td style="text-align: right">
                    <strong>
                        <asp:Label ID="LabelPassword" runat="server" Text="パスワード"></asp:Label></strong></td>
                <td>
                </td>
                <td style="text-align: left;">
                    <asp:TextBox ID="TextBoxPassword" runat="server" MaxLength="15" 
                        TextMode="Password" Width="130px" CssClass="PasswordInput"></asp:TextBox></td>
            </tr>
            <tr>
                <td colspan="2"/>
                <td>
                    <asp:RequiredFieldValidator ID="PasswordRequired" runat="server" 
                        ControlToValidate="TextBoxPassword" CssClass="failureNotification" 
                        ErrorMessage="パスワードを入力してください。"></asp:RequiredFieldValidator>
                </td>
            </tr>
            <tr>
                <td align="center" colspan="3" style="height: 27px">
                    <asp:Button ID="ButtonLogin" CssClass="FuncButton" runat="server" OnClick="ButtonLogin_Click" Text="ログイン" /></td>
            </tr>
        </table>
    </center>
    </asp:Panel>
</asp:Content>

