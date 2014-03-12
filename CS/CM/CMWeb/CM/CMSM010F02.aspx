<%@ Page Title="" Language="C#" MasterPageFile="~/CM.master" AutoEventWireup="true" CodeFile="CMSM010F02.aspx.cs" Inherits="CM_CMSM010F02" %>
<%@ MasterType  virtualPath="~/CM.master"%>

<asp:Content ID="Content1" ContentPlaceHolderID="Content1" Runat="Server">
    <!-- スクリプト -->
    <script type="text/javascript" src="<%=ResolveUrl("~") %>Scripts/jquery-1.11.0.min.js"></script>
    <script type="text/javascript">
        // jQuery
        $(document).ready(function () {
            $("#上位組織CD").change(function () {
                $.getJSON("<%=ResolveUrl("~") %>CMCommonService.svc/GetCodeName",
                    //{ arg: [ "CN組織名", $(this).val()] },
                    { argCodeId: $(this).attr("id"), argNameId: "上位組織名",
                      argSelectId: "CN組織名", argCode: $(this).val() },
                    SetCodeName);
            });
        });

        // サーバから取得した名称を設定する
        function SetCodeName(json)
        {
            var codeId = "#" + json.d.CodeId;
            var nameId = "#" + json.d.NameId;
            var name = json.d.Name;

            // 名称を設定
            $(nameId).val(name);

            // 背景色設定
            var color = name.length > 0 ? "" : "Pink";
            $(codeId).css("background-color", color);

            // エラー表示＆フォーカス
            if (name.length == 0) {
                $(nameId).val("データなし");
                $(codeId).focus();
            }
        }
    </script>
	<script type="text/javascript">
// 確認ボタン押下時の入力値チェック
function CheckInputEntry(argMode)
{
	// 新規の場合、キー項目チェック
    if (argMode == "Insert") {
        var inputs = new Array(Form1.組織CD);
        var names = new Array("組織CD");

        // 必須入力チェック
        for (i = 0; i < inputs.length; i++) {
            if (CheckNull(inputs[i], names[i])) return false;
        }
        // コード値チェック
        for (i = 0; i < inputs.length; i++) {
            if (CheckCode(inputs[i], names[i])) return false;
        }
    }

	// 従属項目チェック

	// 必須入力チェック
    if (CheckNull(Form1.組織名, "組織名")) return false;
	
	// 入力形式チェック
    if (CheckName(Form1.組織名, "組織名")) return false;
}
	</script>
    <!-- キー項目部 -->
    <asp:Panel id="PanelKeyItems" Runat="server">
	    <table cellspacing="2" width="100%">
            <tr>
                <td class="ItemName" width="120">組織コード</td>
                <td class="ItemPanel">
                    <asp:TextBox ID="組織CD" CssClass="CodeInput" runat="server" MaxLength="4" Width="40" Text='<%# InputRow["組織CD"] %>' />
                </td>
            </tr>
	    </table>
    </asp:Panel>
    <!-- 従属項目部 -->
    <asp:panel id="PanelSubItems" Runat="server">
	    <table cellspacing="2" width="100%">
            <tr>
                <td class="ItemName" width="120">組織名</td>
                <td class="ItemPanel">
                    <asp:TextBox ID="組織名" CssClass="TextInput" runat="server" MaxLength="40" Width="200" Text='<%# InputRow["組織名"] %>' />
                </td>
            </tr>
            <tr>
                <td class="ItemName">組織階層区分</td>
                <td class="ItemPanel">
                    <asp:DropDownList ID="組織階層区分" DataTextField="表示名" DataValueField="基準値CD" runat="server" Text='<%# InputRow["組織階層区分"] %>' />
                </td>
            </tr>
            <tr>
                <td class="ItemName">上位組織コード</td>
                <td class="ItemPanel">
                    <asp:TextBox ID="上位組織CD" CssClass="CodeInput" runat="server" MaxLength="4" Width="40" Text='<%# InputRow["上位組織CD"] %>' />
                    <input id="B上位組織CD" class="SelectButton" runat="server" type="button" value="..."
                        onclick="ShowSelectJyouiSoshikiCd(this, 上位組織CD, 上位組織名, 組織階層区分)" />
					<asp:TextBox ID="上位組織名" CssClass="ReadOnly" ReadOnly="true" runat="server" TabIndex="-1" Text='<%# InputRow["上位組織名"] %>' />
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
	<!-- 隠し項目 -->
    <input id="KaishaCd" type="hidden" runat="server" />
    <input id="SoshikiLayer" type="hidden" runat="server" />
</asp:Content>

