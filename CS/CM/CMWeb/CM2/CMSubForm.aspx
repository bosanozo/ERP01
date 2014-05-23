<%@ Page Language="C#" AutoEventWireup="true" CodeFile="CMSubForm.aspx.cs" Inherits="CM2_CMSubForm" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
    <base target="_self" />

    <!--jQuery UI スタイルシート -->
    <link href="~/Content/themes/base/jquery-ui.css" rel="stylesheet" />
    <!-- jqGrid スタイルシート -->
    <link href="~/Content/jquery.jqGrid/ui.jqgrid.css" rel="stylesheet" />
    <!-- ローカルスタイルシート -->
    <link href="~/Styles/SsDefaultStyle.css" type="text/css" rel="stylesheet" />
    <link href="~/Styles/jqStyle.css" rel="stylesheet" />
    <!-- jQuery -->
    <!--<script src="../Scripts/jquery-2.1.0.intellisense.js"></script>-->
    <script src="http://ajax.googleapis.com/ajax/libs/jquery/1/jquery.min.js"></script>
    <!-- jQuery UI -->
    <script src="../Scripts/jquery-ui-1.10.4.js"></script>
    <!--<script src="http://ajax.googleapis.com/ajax/libs/jqueryui/1/jquery-ui.min.js"></script>-->
    <!-- jQuery UI datepicker 日本語 -->
    <script src="http://ajax.googleapis.com/ajax/libs/jqueryui/1/i18n/jquery.ui.datepicker-ja.min.js"></script>
    <!-- jqGrid -->
    <script src="../Scripts/jquery.jqGrid.src.js"></script>
    <!-- jqGrid 日本語メッセージ -->
    <script src="../Scripts/i18n/grid.locale-ja.js"></script>
    <!-- バルーンチップ -->
    <script src="../Scripts/jquery.balloon.js"></script>
    <!-- jQuery Validation -->
    <script src="../Scripts/jquery.validate-vsdoc.js"></script>
    <!-- jQuery Validation 日本語メッセージ -->
    <script src="../Scripts/localization/messages_ja.js"></script>
    <!-- jQuery Validation 日本語 -->
    <script src="../Scripts/jquery.validate.japlugin.js"></script>
    <!-- jQuery Validation 日付 -->
    <script src="../Scripts/jquery.ui.datepicker.validation.js"></script>
    <!-- jQuery ローカル -->
    <script src="../Scripts/CMJqCommon.js"></script>
    <script src="../Scripts/CMJqSubGrid.js"></script>

    <!-- jQueryスクリプト -->
    <script type="text/javascript">
        // jquery
        $(document).ready(function () {
            // グリッド列名
            var grid1ColNames = [
                '<%# GetCodeLabel() %>', '<%# GetNameLabel() %>'
            ];

            // グリッド列設定
            var grid1ColModel = [
                { name: 'code', width: 100 },
                { name: 'name', width: 250 }
            ];

            // 初期化
            commonInit();

            // グリッド作成
            var grid1 = createSubGrid('Grid1', grid1ColNames, grid1ColModel, 'Grid1_Pager');

            // ボタンイベント登録
            $("#BtnSelect").click({ grid: grid1, form: $("#Form1") }, onSelectClick);
        });
    </script>

    <script type="text/javascript">
	    // 検索ボタン押下時の入力値チェック
	    function CheckInputList() {
	        if (CheckName(Form1.Name, "<%# GetNameLabel() %>")) return false;
	        if (CheckCode(Form1.Code, "<%# GetCodeLabel() %>")) return false;
	    }
	</script>
</head>
<body>
    <!-- 条件部 -->
    <form id="Form1" runat="server">
        <asp:Panel id="PanelCondition" Runat="server">
            <table width="100%" cellspacing="2" >
                <tr>
                    <td class="ItemName"><%# GetNameLabel() %></td>
                    <td class="ItemPanel">
                        <asp:TextBox ID="Name" runat="server" CssClass="TextInput" Width="300" />
                    </td>
                </tr>
                <tr>
                    <td class="ItemName"><%# GetCodeLabel() %></td>
                    <td class="ItemPanel">
                        <asp:TextBox ID="Code" CssClass="CodeInput" runat="server" />
                    </td>
                </tr>
            </table>
        </asp:Panel>
    </form>
    <!-- 機能ボタン -->
    <div class="FuncPanel">
		<table cellspacing="0" cellpadding="0" width="100%">
			<tr>
				<td>
                    <input id="BtnSelect" class="FuncButton" type="button" value="検索" />
                </td>
				<td>
                    <input id="BtnClose" class="FuncButton" type="button" value="閉じる" onclick="window.parent.colseSubDialog()"/>
                </td>
			</tr>
		</table>
    </div>
    <!-- 明細部 -->
    <table id="Grid1"></table>
    <div id="Grid1_Pager"></div>
</body>
</html>
