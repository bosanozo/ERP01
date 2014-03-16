<%@ Page Language="C#" MasterPageFile="~/CM.master" AutoEventWireup="true" CodeFile="XM010F01.aspx.cs" Inherits="CM_XM010F01" %>
<%@ MasterType  virtualPath="~/CM.master"%>

<asp:Content ID="Content1" ContentPlaceHolderID="Content1" Runat="Server">
    <!-- スクリプト -->
	<script type="text/javascript">
	    // 選択行のキーを取得する
	    function GetKeys(argRow) {
	        return encodeURI(argRow.cells[2].innerHTML) + "," + argRow.cells[3].innerHTML;
	    }

	    // 検索ボタン押下時の入力値チェック
	    function CheckInputList() {
	        if (CheckDateFromTo(Form1.更新日時From, Form1.更新日時To, "更新日時")) return false;
	    }
	</script>
    <!-- 条件部 -->
    <asp:Panel id="PanelCondition" Runat="server">
        <table width="100%" cellspacing="2">
            <tr>
                <td class="ItemName">オブジェクト型</td>
                <td class="ItemPanel">
                    <asp:DropDownList ID="オブジェクト型" runat="server" DataTextField="表示名" DataValueField="基準値CD" />
                </td>
                <td class="ItemName">オブジェクト名</td>
                <td class="ItemPanel">
                    <asp:TextBox ID="オブジェクト名" CssClass="TextInput" runat="server" MaxLength="32" Width="200" />
                </td>
            </tr>
            <tr>
                <td class="ItemName">更新年月日</td>
                <td class="ItemPanel">
                    <asp:TextBox ID="更新日時From" CssClass="DateInput" runat="server" MaxLength="10" Width="80" />
                    <input id="B更新日時From" class="SelectButton" onclick="ShowCalendar(this, 更新日時From)"
				        type="button" value="..." /> ～
                    <asp:TextBox ID="更新日時To" CssClass="DateInput" runat="server" MaxLength="10" Width="80" />
                    <input id="B更新日時To" class="SelectButton" onclick="ShowCalendar(this, 更新日時To)"
					    type="button" value="..." />
                </td>
                <td class="ItemName">表示範囲</td>
                <td class="ItemPanel">
                    <asp:RadioButton ID="最新" runat="server" Checked="True" Text="最新のみ表示" 
                        GroupName="表示範囲" />
                    <asp:RadioButton ID="全て" runat="server" Text="全て表示" GroupName="表示範囲" />
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
                    <input id="BtnClear" class="FuncButton" onclick="ClearCondition()" type="button" value="条件クリア"/>
                 </td>
				<td>
                    <asp:Button ID="BtnCsvOut" CssClass="FuncButton" runat="server" Text="ＣＳＶ出力" 
                        CommandName="CsvOut" oncommand="Select_Command" />
                </td>
				<td>
                    <asp:button id="BtnInsert" CssClass="FuncButton" Text="新規" Runat="server" 
                        CommandName="Insert" oncommand="OpenEntryForm_Command" />
                </td>
				<td>
                    <asp:button id="BtnUpdate" CssClass="FuncButton" Text="修正" Runat="server" 
                        Enabled="False" CommandName="Update" oncommand="OpenEntryForm_Command"/>
                </td>
				<td>
                    <asp:button id="BtnDelete" CssClass="FuncButton" Text="削除" Runat="server" 
                        Enabled="False" CommandName="Delete" oncommand="OpenEntryForm_Command" />
                </td>
				<td>
                    <input id="BtnDetail" class="FuncButton" onclick="OpenEntryForm('Detail')" type="button" value="参照" disabled="disabled" />
                </td>
			</tr>
		</table>
    </div>
    <!-- 明細部 -->
    <asp:GridView ID="GridView1" runat="server" AllowPaging="True" AutoGenerateColumns="False"
        PageSize="20" OnRowDataBound="GridView1_RowDataBound" OnPageIndexChanging="GridView1_PageIndexChanging"
        Width="100%">
        <Columns>
            <asp:TemplateField HeaderText="選択">
                <ItemTemplate>
                    <input id="Checkbox" type="checkbox" onclick="SetSelectedIndex()" />
                </ItemTemplate>
                <ItemStyle HorizontalAlign="Center" Width="40px" />
            </asp:TemplateField>
            <asp:BoundField DataField="オブジェクト型名" HeaderText="オブジェクト型" HtmlEncode="False" >
                <ItemStyle Width="80px" />
            </asp:BoundField>
            <asp:BoundField DataField="オブジェクト名" HeaderText="オブジェクト名" HtmlEncode="False" />
            <asp:BoundField DataField="VER" HeaderText="VER" HtmlEncode="False" >
                <ItemStyle Width="20px" />
            </asp:BoundField>
            <asp:BoundField DataField="コメント" HeaderText="コメント" HtmlEncode="True" >
                <ItemStyle Width="400px" />
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
	<!-- 隠し項目 -->
    <input id="Selected" type="hidden" runat="server" />
    <input id="EntryForm" type="hidden" value="XM010F02" />
</asp:Content>

