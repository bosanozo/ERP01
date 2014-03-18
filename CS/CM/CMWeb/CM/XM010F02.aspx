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
    <asp:GridView ID="GridView1" runat="server" AllowPaging="True" 
        AutoGenerateColumns="False" Width="100%" onrowcancelingedit="GridView1_RowCancelingEdit" 
        onrowediting="GridView1_RowEditing" onrowupdating="GridView1_RowUpdating">
        <Columns>
            <asp:TemplateField HeaderText="選択">
                <ItemTemplate>
                    <input id="Checkbox" type="checkbox" onclick="SetSelectedIndex()" />
                </ItemTemplate>
                <ItemStyle HorizontalAlign="Center" Width="40px" />
            </asp:TemplateField>
            <asp:CommandField ShowEditButton="True" ButtonType="Button">
            </asp:CommandField>
            <asp:TemplateField HeaderText="NO">
                <ItemTemplate>
                    <asp:Label ID="NO_I" runat="server" Text='<%# Bind("項目NO") %>'></asp:Label>
                </ItemTemplate>
                <EditItemTemplate>
                    <asp:TextBox ID="NO_E" runat="server" Text='<%# Bind("項目NO") %>' Width="40px"></asp:TextBox>
                </EditItemTemplate>
                <ItemStyle Width="40px" />
            </asp:TemplateField>
            <asp:TemplateField HeaderText="VER">
                <ItemTemplate>
                    <asp:Label ID="VER_I" runat="server" Text='<%# Bind("VER") %>'></asp:Label>
                </ItemTemplate>
                <EditItemTemplate>
                    <asp:Label ID="VER_E" runat="server" Text='<%# Bind("VER") %>'></asp:Label>
                </EditItemTemplate>
                <ItemStyle Width="40px" />
            </asp:TemplateField>
            <asp:TemplateField HeaderText="項目名">
                <ItemTemplate>
                    <asp:Label ID="項目名_I" runat="server" Text='<%# Bind("項目名") %>'></asp:Label>
                </ItemTemplate>
                <EditItemTemplate>
                    <asp:TextBox ID="項目名_E" runat="server" Text='<%# Bind("項目名") %>'></asp:TextBox>
                </EditItemTemplate>
            </asp:TemplateField>
            <asp:TemplateField HeaderText="説明">
                <ItemTemplate>
                    <asp:Label ID="説明_I" runat="server" Text='<%# Bind("説明") %>'></asp:Label>
                </ItemTemplate>
                <EditItemTemplate>
                    <asp:TextBox ID="説明_E" runat="server" Text='<%# Bind("説明") %>' TextMode="MultiLine" Rows="5" Columns="50"></asp:TextBox>
                </EditItemTemplate>
                <ItemStyle Width="300px"/>
            </asp:TemplateField>
            <asp:TemplateField HeaderText="項目型">
                <ItemTemplate>
                    <asp:TextBox ID="項目型_I" runat="server" Text='<%# Bind("項目型") %>'></asp:TextBox>
                </ItemTemplate>
                <EditItemTemplate>
                    <asp:TextBox ID="項目型_E" runat="server" Text='<%# Bind("項目型") %>' Width="40px"></asp:TextBox>
                </EditItemTemplate>
            </asp:TemplateField>
            <asp:TemplateField HeaderText="長さ">
                <ItemTemplate>
                    <asp:Label ID="長さ_I" runat="server" Text='<%# Bind("長さ") %>'></asp:Label>
                </ItemTemplate>
                <EditItemTemplate>
                    <asp:TextBox ID="長さ_E" runat="server" Text='<%# Bind("長さ") %>' Width="40px"></asp:TextBox>
                </EditItemTemplate>
                <ItemStyle Width="40px" />
            </asp:TemplateField>
            <asp:TemplateField HeaderText="小数桁">
                <ItemTemplate>
                    <asp:Label ID="小数桁_I" runat="server" Text='<%# Bind("小数桁") %>'></asp:Label>
                </ItemTemplate>
                <EditItemTemplate>
                    <asp:TextBox ID="小数桁_E" runat="server" Text='<%# Bind("小数桁") %>' Width="40px"></asp:TextBox>
                </EditItemTemplate>
                <ItemStyle Width="40px" />
            </asp:TemplateField>
            <asp:BoundField DataField="必須" HeaderText="必須" HtmlEncode="False" >
                <ItemStyle Width="40px" />
            </asp:BoundField>
            <asp:BoundField DataField="主キー" HeaderText="主キー" HtmlEncode="False" >
            <ItemStyle Width="40px" />
            </asp:BoundField>
            <asp:TemplateField HeaderText="デフォルト">
                <ItemTemplate>
                    <asp:Label ID="デフォルト_I" runat="server" Text='<%# Bind("デフォルト") %>'></asp:Label>
                </ItemTemplate>
                <EditItemTemplate>
                    <asp:TextBox ID="デフォルト_E" runat="server" Text='<%# Bind("デフォルト") %>' Width="100px"></asp:TextBox>
                </EditItemTemplate>
                <ItemStyle Width="100px" />
            </asp:TemplateField>
            <asp:TemplateField HeaderText="更新日時">
                <ItemTemplate>
                    <asp:Label ID="更新日時_I" runat="server" 
                        Text='<%# Bind("更新日時", "{0:yyyy/MM/dd HH:mm:ss}") %>'></asp:Label>
                </ItemTemplate>
                <EditItemTemplate>
                    <asp:Label ID="更新日時_E" runat="server" Text='<%# Bind("更新日時") %>'></asp:Label>
                </EditItemTemplate>
            </asp:TemplateField>
            <asp:TemplateField HeaderText="更新者名">
                <ItemTemplate>
                    <asp:Label ID="更新者名_I" runat="server" Text='<%# Bind("更新者名") %>'></asp:Label>
                </ItemTemplate>
                <EditItemTemplate>
                    <asp:Label ID="更新者名_E" runat="server" Text='<%# Bind("更新者名") %>'></asp:Label>
                </EditItemTemplate>
            </asp:TemplateField>
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

