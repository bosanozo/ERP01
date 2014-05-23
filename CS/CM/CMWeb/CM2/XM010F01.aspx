<%@ Page Language="C#" MasterPageFile="~/CM2.master" AutoEventWireup="true" CodeFile="XM010F01.aspx.cs" Inherits="CM2_XM010F01" %>
<%@ MasterType  virtualPath="~/CM2.master"%>

<asp:Content ID="Content1" ContentPlaceHolderID="Content1" Runat="Server">
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
            return '項目一覧ID=' + rowData.項目一覧ID + '&VER=' + rowData.VER;
        }

        // jquery
        $(document).ready(function () {
            // グリッド列名
            var grid1ColNames = [
                '状態', '操作',
                <%= GetColNames(FORM_XML) %>
            ];

            // グリッド列設定
            var grid1ColModel = [
                { name: '状態', align: 'center', frozen: true, formatter: statusFormatter, width: 40 },
                { name: '操作', align: 'center', frozen: true, formatter: actionFormatter, width: 50 },
                <%= GetColModel(FORM_XML) %>
            ];

            // ValidationRule
            var rules = {
                <%= GetValidationRules(FORM_XML) %>
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
            var editDlg1 = createDetailDialog('DlgDetail', rules, 'Grid1');

            // ボタンイベント登録
            $("#BtnSelect").click({ grid: grid1, form: form }, onSelectClick);
            $("#BtnCsvExport").click({ form: form }, onCsvExportClick);
            $("#BtnAdd").click({ grid: grid1, editDlg: editDlg1 }, onAddClick2);
            $("#BtnEdit").click({ grid: grid1, editDlg: editDlg1 }, onEditClick2);
            //$("#BtnDel").click({ grid: grid1 }, onDelClick);
            $("#BtnView").click({ grid: grid1, editDlg: editDlg1 }, onViewClick2);
            $("#BtnCommit").click({ grid: grid1 }, onCommitClick);
        });
    </script>

    <!-- フォーム -->
    <form id="Form1" runat="server">
        <!-- 条件部 -->
        <asp:Panel id="Panel1" Runat="server"/>
        <asp:Panel id="PanelCondition" Runat="server">
            <span>検索条件</span>
            <table width="100%" cellspacing="2">
                <%= CreateForm("XMFS項目一覧", true) %>
            </table>
        </asp:Panel>
	    <!-- 隠し項目 -->
        <input id="EntryForm" type="hidden" value="XM010F02.aspx" />
    </form>

    <!-- 機能ボタン -->
    <div class="FuncPanel">
		<table cellspacing="0" cellpadding="0" width="100%">
			<tr>
				<td>
                    <input id="BtnSelect" class="FuncButton" type="button" value="検索"/>
                </td>
				<td>
                    <input id="BtnClear" class="FuncButton" type="button" value="条件クリア" onclick="ClearCondition()"/>
                    </td>
				<td>
                    <input id="BtnCsvExport" class="FuncButton" type="button" value="ＣＳＶ出力" />
                </td>
			</tr>
		</table>
    </div>
    <!-- 明細部 -->
    <table id="Grid1"></table>
    <div id="Grid1_Pager"></div>

    <!-- 機能ボタン２ -->
    <div class="FuncPanel">
		<table cellspacing="0" cellpadding="0" width="100%">
			<tr>
				<td>
                    <input id="BtnAdd" class="FuncButton" type="button" value="新規"/>
                </td>
				<td>
                    <input id="BtnEdit" class="FuncButton" type="button" value="修正" disabled/>
                </td>
                <!--
				<td>
                    <input id="BtnDel" class="FuncButton" type="button" value="削除" disabled/>
                </td>
                -->
				<td>
                    <input id="BtnView" class="FuncButton" type="button" value="参照" disabled/>
                </td>
				<td>
                    <input id="BtnCommit" class="FuncButton" type="button" value="登録"/>
                </td>
			</tr>
		</table>
    </div>

    <!-- 詳細ダイアログ -->
    <div id="DlgDetail" >
        <form>
            <table cellspacing="2">
                <%= CreateForm(FORM_XML) %>
            </table>
        </form>
    </div>

</asp:Content>
