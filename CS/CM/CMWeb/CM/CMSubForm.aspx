<%@ Page Language="C#" AutoEventWireup="true" CodeFile="CMSubForm.aspx.cs" Inherits="CMSubForm" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
    <link href="~/Styles/SsDefaultStyle.css" type="text/css" rel="stylesheet" />
    <base target="_self" />
</head>
<body>
    <script type="text/javascript" src="<%=ResolveUrl("~") %>Scripts/CMCommon.js"></script>
	<script type="text/javascript">
	    // 検索ボタン押下時の入力値チェック
	    function CheckInputList() {
	        if (CheckName(Form1.Name, "<%# GetNameLabel() %>")) return false;
	        if (CheckCode(Form1.Code, "<%# GetCodeLabel() %>")) return false;
	    }

        // 選択ボタン押下
	    function SelectRow(argRow) {
	        var code = argRow.cells[1].innerText;
	        var name = argRow.cells[2].innerText;
	        window.returnValue = new Array(code, name);
	        window.close();
	    }
	</script>
    <form id="Form1" runat="server">
    <div class="main">
    <!-- 条件部 -->
    <asp:Panel id="PanelCondition" Runat="server">
        <table width="100%" cellspacing="2" >
            <tr>
                <td class="ItemName"><%# GetNameLabel() %></td>
                <td class="ItemPanel">
                    <asp:TextBox ID="Name" runat="server" CssClass="TextInput" Width="300" />
                </td>
            </tr>
            <tr>
                <td class="ItemName"><%# GetCodeLabel() %></td>
                <td class="ItemPanel">
                    <asp:TextBox ID="Code" CssClass="CodeInput" runat="server" />
                </td>
            </tr>
        </table>
    </asp:Panel>
    <!-- 機能ボタン -->
    <div class="FuncPanel">
		<table cellspacing="0" cellpadding="0" width="100%">
			<tr>
				<td>
                    <asp:Button ID="BtnSelect" CssClass="FuncButton" runat="server" Text="検索" 
                        CommandName="Select" oncommand="Select_Command" />
                </td>
				<td>
                    <input id="BtnClose" class="FuncButton" onclick=" window.close()" type="button" value="閉じる"/>
                 </td>
			</tr>
		</table>
    </div>
    <!-- 明細部 -->
    <asp:GridView ID="GridView1" runat="server" AllowPaging="True" AutoGenerateColumns="False"
        PageSize="20" OnPageIndexChanging="GridView1_PageIndexChanging"
        Width="100%">
        <Columns>
            <asp:TemplateField HeaderText="選択">
                <ItemTemplate>
                    <input id="SelectButton" type="button" onclick="SelectRow(this.parentNode.parentNode)" value="選択" />
                </ItemTemplate>
                <ItemStyle HorizontalAlign="Center" Width="40px" />
            </asp:TemplateField>
            <asp:BoundField HtmlEncode="False" >
                <ItemStyle Width="80px" />
            </asp:BoundField>
            <asp:BoundField HtmlEncode="False" />
        </Columns>
        <HeaderStyle BackColor="#003580" ForeColor="White" Height="18px" />
        <AlternatingRowStyle BackColor="#CCCCFF" />
        <PagerSettings Mode="NumericFirstLast" />
        <PagerStyle BackColor="Control" />
    </asp:GridView>    
    </div>
    </form>
</body>
</html>
