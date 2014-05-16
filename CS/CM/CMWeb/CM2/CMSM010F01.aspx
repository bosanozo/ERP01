<%@ Page Language="C#" MasterPageFile="~/CM2.master" AutoEventWireup="true" CodeFile="CMSM010F01.aspx.cs" Inherits="CM2_CMSM010F01" %>
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

        // jquery
        $(document).ready(function () {
            // グリッド列名
            var grid1ColNames = [
                '状態', '操作',
                <%= GetColNames("CMSM組織") %>
            ];

            // グリッド列設定
            var grid1ColModel = [
                { name: '状態', align: 'center', frozen: true, formatter: statusFormatter, width: 40 },
                { name: '操作', align: 'center', frozen: true, formatter: actionFormatter, width: 50 },
                <%= GetColModel("CMSM組織") %>
            ];

            // ValidationRule
            var rules = {
                <%= GetValidationRules("CMSM組織") %>
            };

            // 初期化
            commonInit();

            // 検索条件部
            $("#PanelCondition").accordion({ collapsible: true, heightStyle: 'content' });

            // コード検索
            $(".CodeInput[changeParam]").change(GetCodeValue);

            // 選択ボタン
            $(".SelectButton").click(ShowSelectSub);

            // 日付
            $(".DateInput").datepicker({ dateFormat: 'yy/mm/dd' });

            // グリッド作成
            var grid1 = createGrid('Grid1', grid1ColNames, grid1ColModel, 'CMSM010F01.aspx', 'Grid1_Pager');

            // 詳細ダイアログ作成
            var editDlg1 = createDetailDialog('DlgDetail', rules, 'Grid1');

            // ボタンイベント登録
            $("#BtnSelect").click({ grid: grid1, form: $("#Form1") }, onSelectClick);
            $("#BtnCsvExport").click({ form: $("#Form1") }, onCsvExportClick);
            $("#BtnAdd").click({ grid: grid1, editDlg: editDlg1 }, onAddClick);
            $("#BtnEdit").click({ grid: grid1, editDlg: editDlg1 }, onEditClick);
            //$("#BtnDel").click({ grid: grid1 }, onDelClick);
            $("#BtnView").click({ grid: grid1, editDlg: editDlg1 }, onViewClick);
            $("#BtnCommit").click({ grid: grid1 }, onCommitClick);
        });
    </script>

    <!-- スクリプト -->
	<script type="text/javascript">
	    // 選択行のキーを取得する
	    function GetKeys(argRow) {
	        return argRow.cells[1].innerHTML;
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

    <!-- フォーム -->
    <form id="Form1" runat="server">
        <!-- 条件部 -->
        <asp:Panel id="PanelCondition" Runat="server">
            <span>検索条件</span>
            <table width="100%" cellspacing="2">
                <%= CreateForm("CMSM組織検索条件", true) %>
            </table>
        </asp:Panel>
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
	    <!-- 隠し項目 -->
        <input id="KaishaCd" type="hidden" runat="server" />
        <input id="SoshikiLayer" type="hidden" runat="server" />
        <input id="Selected" type="hidden" runat="server" />
    </form>

    <!-- 詳細ダイアログ -->
    <div id="DlgDetail" >
        <form>
            <table>
                <%= CreateForm("CMSM組織") %>
            </table>
        </form>
    </div>

</asp:Content>
