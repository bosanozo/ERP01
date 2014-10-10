<%@ Page Language="C#" MasterPageFile="~/CM2.master" AutoEventWireup="true" CodeFile="XM010F01.aspx.cs" Inherits="CM2_XM010F01" %>
<%@ MasterType  virtualPath="~/CM2.master"%>
<%@ Import Namespace="NEXS.ERP.CM.Common"%>
<%@ Import Namespace="NEXS.ERP.CM.Helper"%>

<asp:Content ID="Content1" ContentPlaceHolderID="Content1" Runat="Server">
    <% var dataSet = CM項目DataSet.ReadFormXml(FORM_XML); %>

    <!-- jQueryスクリプト -->
    <script type="text/javascript">
        // ボタンの操作状態設定
        function setButtonState(flg)
        {
            $("#BtnEdit").attr('disabled', flg);
            $("#BtnDel").attr('disabled', flg);
            $("#BtnView").attr('disabled', flg);
        }

        // 詳細画面表示用検索パラメータ取得
        function getDetailSearchParam(rowData) {
            if (rowData)
                return escape('項目一覧ID') + '=' + escape(rowData.項目一覧ID) + '&VER=' + rowData.VER;
            else
                return '項目一覧ID= &VER=0';
        }

        // jquery
        $(document).ready(function () {
            // グリッド列名
            var grid1ColNames = [
                <%= JqGridHelper.GetColNames(dataSet) %>
            ];

            // グリッド列設定
            var grid1ColModel = [
                <%= JqGridHelper.GetColModel(dataSet, CommonBL) %>
            ];

            // ValidationRule
            var rules = {
                <%= JqGridHelper.GetValidationRules(dataSet) %>
            };

            // 初期化
            commonGridInit();

            var form = $("#Form1");

            // 検索条件部
            $("#PanelCondition").accordion({ collapsible: true, heightStyle: 'content' });

            // 標準操作追加
            addCommonEvent(form);

            // グリッド作成
            var grid1 = createGrid('Grid1', grid1ColNames, grid1ColModel, 'XM010F01.aspx', 'Grid1_Pager');
            // ボタンの操作状態設定Func
            grid1.setButtonStateFunc(setButtonState);

            // 詳細ダイアログ作成
            var editDlg1 = createDetailDialog('DlgDetail', rules, grid1);

            // ボタンイベント登録
            $("#BtnSelect").click({ grid: grid1, form: form }, onSelectClick);
            $("#BtnClear").click(false, ClearCondition);
            $("#BtnCsvExport").click({ form: form }, onCsvExportClick);
            $("#BtnAdd").click({ grid: grid1, editDlg: editDlg1 }, onAddClick2);
            $("#BtnEdit").click({ grid: grid1, editDlg: editDlg1 }, onEditClick2);
            //$("#BtnDel").click({ grid: grid1 }, onDelClick);
            $("#BtnView").click({ grid: grid1, editDlg: editDlg1 }, onViewClick2);
            $("#BtnCommit").click({ grid: grid1 }, onCommitClick);
        });
    </script>

    <!-- フォーム -->
    <form id="Form1" class="form-horizontal">
        <!-- 条件部 -->
        <div id="PanelCondition">
            <span>検索条件</span>
            <div>
            <%= JqGridHelper.CreateForm(CM項目DataSet.ReadFormXml("XMFS項目一覧"), CommonBL, true) %>
            </div>
        </div>
	    <!-- 隠し項目 -->
        <input id="EntryForm" type="hidden" value="XM010F02.aspx" />
    </form>

    <!-- 機能ボタン -->
    <div class="FuncPanel row">
    <%= JqGridHelper.CreateFuncButton(1, "検索", "条件クリア", "ＣＳＶ出力") %>
    </div>

    <!-- 明細部 -->
    <div id="GridPanel" class="row">
        <table id="Grid1"></table>
        <div id="Grid1_Pager"></div>
    </div>

    <!-- 機能ボタン２ -->
    <div class="FuncPanel row">
    <%= JqGridHelper.CreateFuncButton("新規", "修正", "参照", "登録") %>
    </div>

    <!-- 詳細ダイアログ -->
    <div id="DlgDetail" >
        <form>
            <table class="FormTable">
            <%= JqGridHelper.CreateFormFixed(dataSet, CommonBL) %>
            </table>
        </form>
    </div>

</asp:Content>
