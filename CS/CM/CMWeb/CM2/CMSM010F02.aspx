<%@ Page Language="C#" MasterPageFile="~/CM2.master" AutoEventWireup="true" CodeFile="CMSM010F02.aspx.cs" Inherits="CM2_CMSM010F02" %>
<%@ MasterType  virtualPath="~/CM2.master"%>

<asp:Content ID="Content1" ContentPlaceHolderID="Content1" Runat="Server">
    <!-- jQueryスクリプト -->
    <script type="text/javascript">
        // jquery
        $(document).ready(function () {
            // ValidationRule
            var rules = {
                <%= GetValidationRules("CMSM組織") %>
            };

            // 初期化
            commonInit();

            var form = $("#Form1");

            // validator作成
            var validator = form.validate({
                errorClass: 'ui-state-error',
                rules: rules
            });

            // 標準操作追加
            addCommonEvent(form);

            // ボタンイベント登録
            $("#BtnCommit").click({ form: form, validator: validator }, onCommitClick2);

            // 初期処理
            initDetail(form);
        });
    </script>

    <!-- フォーム -->
    <form id="Form1" runat="server">
        <!-- 条件部 -->
        <asp:Panel id="Panel1" Runat="server"/>
        <asp:Panel id="PanelCondition" Runat="server">
            <table width="100%" cellspacing="2">
                <%= CreateForm("CMSM組織") %>
            </table>
        </asp:Panel>
    </form>

    <!-- 機能ボタン -->
    <div class="EntryFuncPanel">
		<table cellspacing="0" cellpadding="0" width="100%">
			<tr>
				<td>
                    <input id="BtnCommit" class="FuncButton" type="button" value="登録"/>
                </td>
				<td>
                    <input id="BtnClose" class="FuncButton" type="button" value="閉じる" onclick="window.parent.colseDetailDialog()"/>
                </td>
			</tr>
		</table>
    </div>

</asp:Content>
