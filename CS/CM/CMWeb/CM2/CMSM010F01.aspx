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

        $(document).ready(function () {
            // グリッド列名
            var grid1ColNames = [
                '状態',
                <%= GetColNames("CMSM組織") %>
            ];

            // グリッド列設定
            var grid1ColModel = [
                { name: '状態', frozen: true, formatter: statusFormatter, width: 70 },
                <%= GetColModel("CMSM組織") %>
            ];

            // 初期化
            commonInit();

            // 検索条件部
            $("#PanelCondition").accordion({ collapsible: true, heightStyle: 'content' });

            // 日時
            $("#更新日時From,#更新日時To").datepicker({ dateFormat: 'yy/mm/dd' });

            // グリッド要素取得
            var grid1 = $("#Grid1");
            var grid1LastRowId;

            // グリッド作成
            grid1.jqGrid({
                colNames: grid1ColNames,
                colModel: grid1ColModel,
                editurl: 'CMSM010F01.aspx',
                pager: 'Grid1_Pager',
                //url: 'CMSM010F01.aspx',
                // 行選択
                onSelectRow: function (id) {
                    if (grid1LastRowId) {
                        $(this).restoreRow(grid1LastRowId);
                        grid1LastRowId = null;
                    }

                    // ボタンの操作可否を設定
                    var flg = $(this).getGridParam('selrow') == null;
                    setButtonState(flg);
                },
                // 行追加
                /*
                afterInsertRow : function (id) {
                },*/
                // 行編集
                ondblClickRow: function (id) {
                    if (id !== grid1LastRowId) {
                        $(this).restoreRow(grid1LastRowId);
                        grid1LastRowId = id;
                    }

                    // 削除行は編集不可
                    var sts = $(this).getCell(id, '状態');
                    if (sts.match(/^削除/)) return;

                    $(this).editRow(id, true, null, null, null, null, function (rowid, result) {
                        if (sts == "") {
                            $(this).setCell(rowid, '状態', '2');
                            //$(this).setFrozenColumns();
                            $(this).trigger("reloadGrid");
                            setButtonState(true);
                        }
                    });
                },
            });

            // 列固定
            grid1.setFrozenColumns();

            // 検索有効
            grid1.navGrid('#Grid1_Pager', { edit: false, add: true, del: true, search: true, view: true },
                {},
                // 新規ボタン
                {
                    jqModal: false, savekey: [true, 13], closeOnEscape: true, closeAfterAdd: true, afterSubmit:
                    function (response, postdata) {
                        // グリッドに行追加
                        var id = $(this).getGridParam('records');
                        $(this).addRowData(id, postdata);
                        $(this).setCell(id, '状態', '1');
                        $(this).trigger("reloadGrid");
                        setButtonState(true);

                        return [true];
                    }
                },
                // 削除ボタン
                {
                    jqModal: false, width: 350, closeOnEscape: true, afterComplete:
                    function (response, postdata) {
                        $(this).trigger("reloadGrid"); setButtonState(true);
                    }
                },
                { modal: true, multipleSearch: true, closeOnEscape: true },
                { jqModal: false, navkeys: [true, 38, 40], closeOnEscape: true }
            );

            // validator作成
            var validator = $("#FormDetail").validate({
                errorClass: 'ui-state-error',
                rules: {
                    <%= GetValidationRules("CMSM組織") %>
                }
            });

            // 詳細ダイアログ作成
            $("#DlgDetail").dialog({
                autoOpen: false,
                modal: true,
                open: function () {
                    // 操作対象のフォーム要素を取得
                    var form = $("form", "#" + this.id);

                    // 選択行があればデータを表示
                    var selrow = grid1.getGridParam('selrow');
                    if (selrow) grid1.GridToForm(selrow, form);
                    else form.find(":text").val("");

                    // エラー消去
                    validator.resetForm();
                },
                close: function () {
                    // 操作対象のフォーム要素を取得
                    var form = $("form", "#" + this.id);

                    // エラー表示消去
                    form.find(":text").each(function () {
                        $(this).hideBalloon();
                    });
                },
                width: 'auto'
            });
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
                        <asp:TextBox ID="更新日時From" CssClass="DateInput" runat="server" MaxLength="10" Width="80" /> ～
                        <asp:TextBox ID="更新日時To" CssClass="DateInput" runat="server" MaxLength="10" Width="80" />
                    </td>
                </tr>
            </table>
            </td>
        </asp:Panel>
        <!-- 機能ボタン -->
        <div class="FuncPanel">
		    <table cellspacing="0" cellpadding="0" width="100%">
			    <tr>
				    <td>
                        <input id="BtnSelect" class="FuncButton" type="button" value="検索" onclick="onSelectClick('Form1', 'Grid1')" />
                    </td>
				    <td>
                        <input id="BtnClear" class="FuncButton" type="button" value="条件クリア" onclick="ClearCondition()"/>
                     </td>
				    <td>
                        <input id="BtnCsvExport" class="FuncButton" type="button" value="ＣＳＶ出力" onclick="onCsvExportClick('Form1')" />
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
                        <input id="BtnAdd" class="FuncButton" type="button" value="新規" onclick="onAddClick('Grid1', 'DlgDetail')"/>
                    </td>
				    <td>
                        <input id="BtnEdit" class="FuncButton" type="button" value="修正" disabled onclick="onEditClick('Grid1', 'DlgDetail')"/>
                    </td>
				    <td>
                        <input id="BtnDel" class="FuncButton" type="button" value="削除" disabled onclick="onDelClick('Grid1')"/>
                    </td>
				    <td>
                        <input id="BtnView" class="FuncButton" type="button" value="参照" disabled onclick="onViewClick('Grid1', 'DlgDetail')"/>
                    </td>
				    <td>
                        <input id="BtnCommit" class="FuncButton" type="button" value="登録" onclick="onCommitClick('Grid1')"/>
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
        <form id="FormDetail">
            <table>
                <%= CreateForm("CMSM組織") %>
            </table>
        </form>
    </div>

</asp:Content>
