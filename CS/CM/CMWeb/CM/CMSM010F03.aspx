<%@ Page Language="C#" MasterPageFile="~/CM.master" AutoEventWireup="true" CodeFile="CMSM010F03.aspx.cs" Inherits="CM_CMSM010F03" %>
<%@ MasterType  virtualPath="~/CM.master"%>

<asp:Content ID="Content1" ContentPlaceHolderID="Content1" Runat="Server">
    <!-- スクリプト -->
	<script type="text/javascript">
	</script>
    <!-- 条件部 -->
    <asp:Panel id="PanelCondition" Runat="server">
        <table width="100%" cellspacing="2">
            <tr>
                <td class="ItemName" width="120">ファイル名</td>
                <td class="ItemPanel">
                    <asp:FileUpload ID="FileUpload1" runat="server" Width="400" />
                </td>
            </tr>
        </table>
    </asp:Panel>
    <!-- 機能ボタン -->
    <div class="FuncPanel">
		<table cellspacing="0" cellpadding="0" width="100%">
			<tr>
				<td>
                    <asp:Button ID="BtnExcelInput" CssClass="FuncButton" runat="server" 
                        Text="EXCEL入力" onclick="BtnExcelInput_Click"/>
                </td>
				<td>
                    <asp:button id="BtnUpdate" CssClass="FuncButton" Runat="server" 
                        Text="登録" onclick="BtnUpdate_Click"  />
                </td>
			</tr>
		</table>
    </div>
    <!-- 明細部 -->
    <asp:GridView ID="GridView1" runat="server" AutoGenerateColumns="False" Width="100%">
        <Columns>
            <asp:BoundField DataField="組織CD" HeaderText="組織コード"
                HtmlEncode="False" >
            <ItemStyle Width="80px" />
            </asp:BoundField>
            <asp:BoundField DataField="組織名" HeaderText="組織名" HtmlEncode="False" />
            <asp:BoundField DataField="組織階層区分" HeaderText="組織階層区分" HtmlEncode="False" ItemStyle-Width="80"/>
            <asp:BoundField DataField="組織階層区分名" HeaderText="組織階層区分名" HtmlEncode="False" />
            <asp:BoundField DataField="上位組織CD" HeaderText="上位組織コード"
                HtmlEncode="False" >
            <ItemStyle Width="90px" />
            </asp:BoundField>
            <asp:BoundField DataField="上位組織名" HeaderText="上位組織名" HtmlEncode="False" />
        </Columns>
        <HeaderStyle BackColor="#003580" ForeColor="White" Height="18px" />
        <AlternatingRowStyle BackColor="#CCCCFF" />
        <PagerSettings Mode="NumericFirstLast" />
        <PagerStyle BackColor="Control" />
    </asp:GridView>
	<!-- 隠し項目 -->
    <input id="KaishaCd" type="hidden" runat="server" />
    <input id="SoshikiLayer" type="hidden" runat="server" />
</asp:Content>

