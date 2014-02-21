<%@ Page Language="C#" MasterPageFile="~/CM.master" AutoEventWireup="true" CodeFile="CMSM010F01.aspx.cs" Inherits="CM_CMSM010F01" %>
<%@ MasterType  virtualPath="~/CM.master"%>

<asp:Content ID="Content1" ContentPlaceHolderID="Content1" Runat="Server">
    <!-- スクリプト -->
	<script type="text/javascript">
	    // 選択行のキーを取得する
	    function GetKeys(argRow) {
	        return argRow.cells[1].innerText;
	    }

	    // 検索ボタン押下時の入力値チェック
	    function CheckInputList() {
	        if (CheckFromTo(Form1.組織CDFrom, Form1.組織CDTo, "組織コード")) return false;
	        if (CheckName(Form1.組織名, "組織名")) return false;
	        if (CheckFromTo(Form1.会社CDFrom, Form1.会社CDTo, "会社コード")) return false;
	        if (CheckDateFromTo(Form1.更新日時From, Form1.更新日時To, "更新日時")) return false;

	        //if (CheckCode(Form1.wTxb_RyohanGrpCd, "量販グループコード")) return false;
	        //if (CheckAN(Form1.wTxb_UpdateId, "更新者")) return false;
	    }
	</script>
    <!-- 条件部 -->
    <asp:Panel id="PanelCondition" Runat="server">
        <table width="100%" cellspacing="2">
            <tr>
                <td class="ItemName">組織コード</td>
                <td class="ItemPanel">
                    <asp:TextBox ID="組織CDFrom" CssClass="CodeInput" runat="server" MaxLength="4" Width="40" />
                    ～
                    <asp:TextBox ID="組織CDTo" CssClass="CodeInput" runat="server" MaxLength="4" Width="40" />
                </td>
                <td class="ItemName">組織名</td>
                <td class="ItemPanel">
                    <asp:TextBox ID="組織名" CssClass="TextInput" runat="server" MaxLength="40" Width="200" />
                </td>
            </tr>
            <tr>
                <td class="ItemName">組織階層区分</td>
                <td class="ItemPanel" colspan="3">
                    <asp:DropDownList ID="組織階層区分" runat="server" DataTextField="表示名" DataValueField="基準値CD" />
                </td>
            </tr>
            <tr>
                <td class="ItemName">会社コード</td>
                <td class="ItemPanel">
                    <asp:TextBox ID="会社CDFrom" CssClass="CodeInput" runat="server" MaxLength="4" Width="40" />
                    <input id="B会社CDFrom" class="SelectButton" runat="server" type="button" value="..."
                        onclick="ShowSelectKaishaCd(this, 会社CDFrom, null)" /> ～
                    <asp:TextBox ID="会社CDTo" CssClass="CodeInput" runat="server" MaxLength="4" Width="40" />
                    <input id="B会社CDTo" class="SelectButton" runat="server" type="button" value="..."
                        onclick="ShowSelectKaishaCd(this, 会社CDTo, null)" />
                </td>
                <td class="ItemName">更新年月日</td>
                <td class="ItemPanel">
                    <asp:TextBox ID="更新日時From" CssClass="DateInput" runat="server" MaxLength="10" Width="80" />
                    <input id="B更新日時From" class="SelectButton" onclick="ShowCalendar(this, 更新日時From)"
				        type="button" value="..." /> ～
                    <asp:TextBox ID="更新日時To" CssClass="DateInput" runat="server" MaxLength="10" Width="80" />
                    <input id="B更新日時To" class="SelectButton" onclick="ShowCalendar(this, 更新日時To)"
					    type="button" value="..." />
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
            <asp:BoundField DataField="組織CD" HeaderText="組織コード"
                HtmlEncode="False" >
            <ItemStyle Width="80px" />
            </asp:BoundField>
            <asp:BoundField DataField="組織名" HeaderText="組織名" HtmlEncode="False" />
            <asp:BoundField DataField="上位組織CD" HeaderText="上位組織コード"
                HtmlEncode="False" >
            <ItemStyle Width="90px" />
            </asp:BoundField>
            <asp:BoundField DataField="上位組織名" HeaderText="上位組織名" HtmlEncode="False" />
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
    <input id="KaishaCd" type="hidden" runat="server" />
    <input id="SoshikiLayer" type="hidden" runat="server" />
    <input id="Selected" type="hidden" runat="server" />
    <input id="EntryForm" type="hidden" value="CMSM010F02" />
</asp:Content>

