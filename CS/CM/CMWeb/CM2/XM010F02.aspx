<%@ Page Language="C#" MasterPageFile="~/CM2.master" AutoEventWireup="true" CodeFile="XM010F02.aspx.cs" Inherits="CM2_XM010F02" %>
<%@ MasterType  virtualPath="~/CM2.master"%>

<asp:Content ID="Content1" ContentPlaceHolderID="Content1" Runat="Server">
    <!-- jQueryスクリプト -->
    <script type="text/javascript">
        // グリッド1ボタンの操作状態設定
        function setGrid1ButtonState(flg) {
            $("#BtnGrid1Edit").attr('disabled', flg);
            $("#BtnGrid1Del").attr('disabled', flg);
            $("#BtnGrid1View").attr('disabled', flg);
        }

        // グリッド2ボタンの操作状態設定
        function setGrid2ButtonState(flg) {
            $("#BtnGrid2Edit").attr('disabled', flg);
            $("#BtnGrid2Del").attr('disabled', flg);
            $("#BtnGrid2View").attr('disabled', flg);
        }

        // jquery
        $(document).ready(function () {
            // グリッド1列名
            var grid1ColNames = [
                '状態', '操作',
                <%= GetColNames(GRID1_XML) %>
            ];

            // グリッド1列設定
            var grid1ColModel = [
                { name: '状態', align: 'center', frozen: true, formatter: statusFormatter, width: 40 },
                { name: '操作', align: 'center', frozen: true, formatter: actionFormatter, width: 50 },
                <%= GetColModel(GRID1_XML) %>
            ];

            // グリッド2列名
            var grid2ColNames = [
                '状態', '操作',
                <%= GetColNames(GRID2_XML) %>
            ];

            // グリッド2列設定
            var grid2ColModel = [
                { name: '状態', align: 'center', frozen: true, formatter: statusFormatter, width: 40 },
                { name: '操作', align: 'center', frozen: true, formatter: actionFormatter, width: 50 },
                <%= GetColModel(GRID2_XML) %>
            ];

            // ValidationRule
            var rules = {
                <%= GetValidationRules(FORM_XML) %>
            };

            var grid1Rules = {
                <%= GetValidationRules(GRID1_XML) %>
            };

            var grid2Rules = {
                <%= GetValidationRules(GRID2_XML) %>
            };

            // 初期化
            commonGridInit();

            var form = $("#Form1");

            // validator作成
            var validator = form.validate({
                errorClass: 'ui-state-error',
                rules: rules
            });

            // 標準操作追加
            addCommonEvent(form);

            // グリッド作成
            var grid1 = createGrid('Grid1', grid1ColNames, grid1ColModel, 'XM010F02.aspx?TableName=XMEM結合テーブル');
            var grid2 = createGrid('Grid2', grid2ColNames, grid2ColModel, 'XM010F02.aspx?TableName=XMEM項目');

            // ボタンの操作状態設定Func
            grid1.setButtonStateFunc(setGrid1ButtonState);
            grid2.setButtonStateFunc(setGrid2ButtonState);

            // 詳細ダイアログ作成
            var editDlg1 = createDetailDialog('DlgGrid1Detail', grid1Rules, grid1);
            var editDlg2 = createDetailDialog('DlgGrid2Detail', grid2Rules, grid2);

            // グリッド１ ボタンイベント登録
            $("#BtnGrid1Add").click({ grid: grid1, editDlg: editDlg1 }, onAddClick);
            $("#BtnGrid1Edit").click({ grid: grid1, editDlg: editDlg1 }, onEditClick);
            $("#BtnGrid1View").click({ grid: grid1, editDlg: editDlg1 }, onViewClick);

            // グリッド２ ボタンイベント登録
            $("#BtnGrid2Add").click({ grid: grid2, editDlg: editDlg2 }, onAddClick);
            $("#BtnGrid2Edit").click({ grid: grid2, editDlg: editDlg2 }, onEditClick);
            $("#BtnGrid2View").click({ grid: grid2, editDlg: editDlg2 }, onViewClick);

            var grids = { XMEM結合テーブル: grid1, XMEM項目: grid2 };

            // ボタンイベント登録
            $("#BtnCommit").click({ form: form, validator: validator, grids: grids }, onCommitClick2);

            // 初期処理
            initDetail(form, grids);
        });
    </script>

    <!-- フォーム -->
    <form id="Form1" runat="server">
        <!-- 条件部 -->
        <asp:Panel id="Panel1" Runat="server"/>
        <asp:Panel id="PanelCondition" Runat="server">
            <table width="100%" cellspacing="2">
                <%= CreateForm(FORM_XML) %>
            </table>
        </asp:Panel>
    </form>

    <!-- グリッド１ -->
    <table id="Grid1"></table>
    <!-- グリッド１ 操作ボタン -->
    <div class="FuncPanel">
		<table cellspacing="0" cellpadding="0" width="100%">
			<tr>
				<td>
                    <input id="BtnGrid1Add" class="FuncButton" type="button" value="新規"/>
                </td>
				<td>
                    <input id="BtnGrid1Edit" class="FuncButton" type="button" value="修正" disabled/>
                </td>
				<td>
                    <input id="BtnGrid1View" class="FuncButton" type="button" value="参照" disabled/>
                </td>
			</tr>
		</table>
    </div>

    <!-- グリッド１ 詳細ダイアログ -->
    <div id="DlgGrid1Detail" >
        <form>
            <table cellspacing="2">
                <%= CreateForm(GRID1_XML) %>
            </table>
        </form>
    </div>

    <!-- グリッド２ -->
    <table id="Grid2"></table>
    <!-- グリッド２ 操作ボタン -->
    <div class="FuncPanel">
		<table cellspacing="0" cellpadding="0" width="100%">
			<tr>
				<td>
                    <input id="BtnGrid2Add" class="FuncButton" type="button" value="新規"/>
                </td>
				<td>
                    <input id="BtnGrid2Edit" class="FuncButton" type="button" value="修正" disabled/>
                </td>
				<td>
                    <input id="BtnGrid2View" class="FuncButton" type="button" value="参照" disabled/>
                </td>
			</tr>
		</table>
    </div>

    <!-- グリッド２ 詳細ダイアログ -->
    <div id="DlgGrid2Detail" >
        <form>
            <table cellspacing="2">
                <%= CreateForm(GRID2_XML) %>
            </table>
        </form>
    </div>

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
