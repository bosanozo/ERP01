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
            var grids = { XMEM結合テーブル: grid1, XMEM項目: grid2 };

            // ボタンの操作状態設定Func
            grid1.setButtonStateFunc(setGrid1ButtonState);
            grid2.setButtonStateFunc(setGrid2ButtonState);

            // 詳細ダイアログ作成
            var editDlg1 = createDetailDialog('DlgGrid1Detail', grid1Rules, grid1);
            var editDlg2 = createDetailDialog('DlgGrid2Detail', grid2Rules, grid2);

            // 画面の状態設定
            var mode = getQueryString('_mode');

            // 修正の時はチェックボックス追加
            if (mode == 'edit') {
                $("#TxtVER").after('<input id="ChkVerUp" name="VerUp" type="checkbox"/><label for="ChkVerUp">新版</label>');
                $("#ChkVerUp").click(function () {
                    var add = $(this).prop('checked') ? 1 : -1;
                    $("#TxtVER").val(Number($("#TxtVER").val()) + add);
                });
            }

            if (mode != 'view') {
                // グリッド１ ボタンイベント登録
                $("#BtnGrid1Add").click({ grid: grid1, editDlg: editDlg1 }, onAddClick);
                $("#BtnGrid1Edit").click({ grid: grid1, editDlg: editDlg1 }, onEditClick);

                // グリッド２ ボタンイベント登録
                $("#BtnGrid2Add").click({ grid: grid2, editDlg: editDlg2 }, onAddClick);
                $("#BtnGrid2Edit").click({ grid: grid2, editDlg: editDlg2 }, onEditClick);

                // ボタンイベント登録
                $("#BtnCommit").click({ form: form, validator: validator, grids: grids }, onCommitClick2);

                // 入力制限制御
                editDlg2.afterOpen = setReadOnly;

                // 入力制限制御
                $("#Ddl項目型").change(setReadOnly);
                $("#Chk選択ボタン").click(setReadOnly);

            } else {
                // ボタン非表示
                $("#BtnGrid1Add").css('display', 'none');
                $("#BtnGrid1Edit").css('display', 'none');
                $("#BtnGrid2Add").css('display', 'none');
                $("#BtnGrid2Edit").css('display', 'none');
            }

            // 参照ボタン
            $("#BtnGrid1View").click({ grid: grid1, editDlg: editDlg1 }, onViewClick);
            $("#BtnGrid2View").click({ grid: grid2, editDlg: editDlg2 }, onViewClick);

            // XML出力ボタン
            $("#BtnXml").click(function () { location.href = '?oper=xml'; });

            // 初期処理
            initDetail(form, grids);

            // VERの設定
            if (mode == 'new') $("#TxtVER").val('1');
        });

        // 入力制限
        function setReadOnly() {
            var type = $("#Ddl項目型").val();
            var isCode = type == '1' || type == '2';

            $("#Txt共通検索ID, #Txa共通検索パラメータ, #Txt共通検索結果出力項目").attr('readonly', !isCode);
            $("#Chk選択ボタン").attr('disabled', !isCode);

            var addBtn = isCode && $("#Chk選択ボタン").prop('checked');
            $("#Txt共通検索ID2, #Txtコード値列名, #Txt名称列名").attr('readonly', !addBtn);

            $("#Txt長さ").attr('readonly', type == '7');
            $("#Txt小数桁").attr('readonly', type != '5' && type != '6');
            $("#Txt基準値分類CD").attr('readonly', type != '3');
        }

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
                    <input id="BtnXml" class="FuncButton" type="button" value="XML出力"/>
                </td>
				<td>
                    <input id="BtnClose" class="FuncButton" type="button" value="閉じる" onclick="window.parent.colseDetailDialog()"/>
                </td>
			</tr>
		</table>
    </div>

</asp:Content>
