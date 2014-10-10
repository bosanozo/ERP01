<%@ Page Language="C#" MasterPageFile="~/CM2.master" AutoEventWireup="true" CodeFile="CMSM010F02.aspx.cs" Inherits="CM2_CMSM010F02" %>
<%@ MasterType  virtualPath="~/CM2.master"%>
<%@ Import Namespace="NEXS.ERP.CM.Common"%>
<%@ Import Namespace="NEXS.ERP.CM.Helper"%>

<asp:Content ID="Content1" ContentPlaceHolderID="Content1" Runat="Server">
    <% var dataSet = CM項目DataSet.ReadFormXml("CMSM組織"); %>

    <!-- jQueryスクリプト -->
    <script type="text/javascript">
        // jquery
        $(document).ready(function () {
            // ValidationRule
            var rules = {
                <%= JqGridHelper.GetValidationRules(dataSet) %>
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
            $("#BtnClose").click(false, window.parent.colseDetailDialog);

            // 初期処理
            initDetail(form);
        });
    </script>

    <!-- フォーム -->
    <form id="Form1" class="form-horizontal">
        <!-- 条件部 -->
        <div id="PanelCondition">
        <%= JqGridHelper.CreateForm(dataSet, CommonBL) %>
        </div>
    </form>

    <!-- 機能ボタン -->
    <div class="EntryFuncPanel row">
    <%= JqGridHelper.CreateFuncButton(1, "登録", "閉じる") %>
    </div>

</asp:Content>
