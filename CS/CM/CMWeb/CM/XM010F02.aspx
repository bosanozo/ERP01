<%@ Page Title="" Language="C#" MasterPageFile="~/CM.master" AutoEventWireup="true" CodeFile="XM010F02.aspx.cs" Inherits="CM_XM010F02" %>
<%@ MasterType  virtualPath="~/CM.master"%>

<asp:Content ID="Content1" ContentPlaceHolderID="Content1" Runat="Server">
    <!-- スクリプト -->
	<script type="text/javascript">
// 確認ボタン押下時の入力値チェック
function CheckInputEntry(argMode)
{
	// 新規の場合、キー項目チェック
    if (argMode == "Insert") {
        var inputs = new Array(Form1.オブジェクト名);
        var names = new Array("オブジェクト名");

        // 必須入力チェック
        for (i = 0; i < inputs.length; i++) {
            if (CheckNull(inputs[i], names[i])) return false;
        }
        // コード値チェック
        //for (i = 0; i < inputs.length; i++) {
        //    if (CheckCode(inputs[i], names[i])) return false;
        //}
    }

	// 従属項目チェック

	// 必須入力チェック
    //if (CheckNull(Form1.組織名, "組織名")) return false;
	
	// 入力形式チェック
    //if (CheckName(Form1.組織名, "組織名")) return false;
}
	</script>
    <!-- キー項目部 -->
    <asp:Panel id="PanelKeyItems" Runat="server">
	    <table cellspacing="2" width="100%">
            <tr>
                <td class="ItemName" width="120">オブジェクト名</td>
                <td class="ItemPanel" width="300">
                    <asp:TextBox ID="オブジェクト名" CssClass="TextInput" runat="server" MaxLength="32" Width="200" Text='<%# InputRow["オブジェクト名"] %>' />
                </td>
                <td class="ItemName" width="120">VER</td>
                <td class="ItemPanel">
                    <asp:TextBox ID="VER" CssClass="CodeInput" runat="server" MaxLength="3" Width="30" Text='<%# InputRow["VER"] %>' />
                </td>
            </tr>
	    </table>
    </asp:Panel>
    <!-- 従属項目部 -->
    <asp:panel id="PanelSubItems" Runat="server">
	    <table cellspacing="2" width="100%">
            <tr>
                <td class="ItemName" width="120">コメント</td>
                <td class="ItemPanel" colspan="3">
                    <asp:TextBox ID="コメント" CssClass="TextInput" runat="server" MaxLength="4000" Width="600" Text='<%# InputRow["コメント"] %>' />
                </td>
            </tr>
            <tr>
                <td class="ItemName" width="120">サブシステム</td>
                <td class="ItemPanel">
                    <asp:TextBox ID="サブシステム" CssClass="CodeInput" runat="server" MaxLength="4" Width="40" Text='<%# InputRow2["サブシステムID"] %>' />
                </td>
                <td class="ItemName" width="120">エンティティ種別</td>
                <td class="ItemPanel">
                    <asp:DropDownList ID="エンティティ種別" DataTextField="表示名" DataValueField="基準値CD" runat="server" Text='<%# InputRow2["エンティティ種別"] %>' />
                </td>
            </tr>
	    </table>
    </asp:panel>
	<!-- 登録・更新情報表示部 -->
    <asp:panel id="PanelUpdateInfo" Runat="server" width="100%">
		<table cellspacing="1" cellpadding="0" align="right">
			<tr>
				<td>
					<asp:label id="LabelAddInfo" Runat="server" Text="<%# GetAddInfo() %>" />
                </td>
			</tr>
			<tr>
				<td>
					<asp:label id="LabelUpdateInfo" Runat="server" Text="<%# GetUpdateInfo() %>" />
			    </td>
			</tr>
		</table>
	</asp:panel>
    <!-- 明細部 -->
    <asp:GridView ID="GridView1" runat="server" AllowPaging="True" AutoGenerateColumns="False"
        PageSize="20" Width="100%">
        <Columns>
            <asp:TemplateField HeaderText="選択">
                <ItemTemplate>
                    <input id="Checkbox" type="checkbox" onclick="SetSelectedIndex()" />
                </ItemTemplate>
                <ItemStyle HorizontalAlign="Center" Width="40px" />
            </asp:TemplateField>
            <asp:BoundField DataField="項目NO" HeaderText="NO" HtmlEncode="False" >
                <ItemStyle Width="40px" />
            </asp:BoundField>
            <asp:BoundField DataField="VER" HeaderText="VER" HtmlEncode="False" >
                <ItemStyle Width="40px" />
            </asp:BoundField>
            <asp:BoundField DataField="項目名" HeaderText="項目名" HtmlEncode="False" />
            <asp:TemplateField HeaderText="説明" ItemStyle-Width="300">
                <ItemTemplate>
                    <asp:TextBox ID="TextBox1" runat="server" Text="<%# Bind('説明') %>" ></asp:TextBox>
                </ItemTemplate>
            </asp:TemplateField>
            <asp:BoundField DataField="項目型" HeaderText="項目型" HtmlEncode="False" />
            <asp:BoundField DataField="長さ" HeaderText="長さ" HtmlEncode="False" >
                <ItemStyle Width="40px" />
            </asp:BoundField>
            <asp:BoundField DataField="小数桁" HeaderText="小数桁" HtmlEncode="False" >
                <ItemStyle Width="40px" />
            </asp:BoundField>
            <asp:BoundField DataField="必須" HeaderText="必須" HtmlEncode="False" >
                <ItemStyle Width="40px" />
            </asp:BoundField>
            <asp:BoundField DataField="主キー" HeaderText="主キー" HtmlEncode="False" >
                <ItemStyle Width="40px" />
            </asp:BoundField>
            <asp:BoundField DataField="デフォルト" HeaderText="デフォルト" HtmlEncode="False" >
                <ItemStyle Width="100px" />
            </asp:BoundField>
            <asp:BoundField DataField="更新日時" HeaderText="更新日時" 
                DataFormatString="{0:yyyy/MM/dd HH:mm:ss}" />
            <asp:BoundField DataField="更新者名" HeaderText="更新者名" HtmlEncode="False" />
        </Columns>
        <HeaderStyle BackColor="#003580" ForeColor="White" Height="18px" />
        <AlternatingRowStyle BackColor="#CCCCFF" />
        <PagerSettings Mode="NumericFirstLast" />
        <PagerStyle BackColor="Control" />
    </asp:GridView>
	<!-- 機能ボタン部 -->
    <asp:panel id="PanelFunction" CssClass="EntryFuncPanel" Runat="server">
        <table cellspacing="0" cellpadding="0" width="100%">
			<tr>
				<td>
					<asp:button id="BtnCommit" runat="server" CssClass="FuncButton" Text="登録" 
                        onclick="BtnCommit_Click" />
                 </td>
				<td>
					<asp:button id="BtnCancel" runat="server" CssClass="FuncButton" Text="終了" 
                        onclick="BtnCancel_Click" />
                 </td>
			</tr>
		</table>
	</asp:panel>
</asp:Content>

