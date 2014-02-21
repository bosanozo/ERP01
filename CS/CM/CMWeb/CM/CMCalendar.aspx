<%@ Page Language="C#" AutoEventWireup="true" CodeFile="CMCalendar.aspx.cs" Inherits="CMCalender" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>カレンダー</title>
    <link href="~/Styles/SsDefaultStyle.css" type="text/css" rel="stylesheet" />
    <base target="_self" />
</head>
<body id="Body1" runat="server">
    <form id="Form1" runat="server">
    <table cellspacing="0" cellpadding="0" align="center" width="230">
        <tr>
            <td width="50">
                <asp:LinkButton ID="LbPrev" runat="server" onclick="LbPrev_Click">前月</asp:LinkButton>
            </td>
            <td>
                <asp:DropDownList ID="DdlYear" runat="server"
                    AutoPostBack="True" onselectedindexchanged="DdlSelectedIndexChanged" />
                    年
            </td>
            <td>
                <asp:DropDownList ID="DdlMonth" runat="server" oninit="DdlMonth_Init" 
                    AutoPostBack="True" onselectedindexchanged="DdlSelectedIndexChanged" />
                    月
            </td>
            <td align="right">
                <asp:LinkButton ID="LbNext" runat="server" onclick="LbNext_Click">次月</asp:LinkButton>
            </td>
        </tr>
        <tr>
            <td align="center" colspan="4">
                <asp:Calendar ID="Calendar1" runat="server"  
                    DayNameFormat="Shortest" Height="200px" Width="200px" 
                    onselectionchanged="Calendar1_SelectionChanged" ShowTitle="False">
                    <NextPrevStyle Font-Bold="true" />
                    <SelectedDayStyle BackColor="#00AAAA" Font-Bold="True" />
                </asp:Calendar>
            </td>
        </tr>
    </table>
    </form>
</body>
</html>
