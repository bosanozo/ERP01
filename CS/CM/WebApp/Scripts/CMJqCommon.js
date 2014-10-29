// 初期化
function commonInit() {
    // Date型取得設定
    $.ajaxSetup({
        converters: {
            "text json": function (text) {
                return JSON.parse(text, reviveDate);
            }
        },
        // WebAPI認証
        headers: {
            'Authorization': 'Bearer ' + sessionStorage.getItem("accessToken")
        }
    });
}

// Date型 デシリアライズ
function reviveDate(key, value) {
    // ISO 8601
    if (value == null ||
        value.constructor !== String ||
        value.search(/^\d{4}-\d{2}-\d{2}/g) === -1)
        return value;
    return new Date(value);

    /* ASP.NET
    if (value == null ||
        value.constructor !== String)
        return value;
    var m = /^\/Date\((\d+)(.+)?\)\/$/g.exec(value);
    if (!m) return value;
    return new Date(parseInt(m[1]));*/
}

// 標準操作追加
function addCommonEvent(form) {
    // コード検索
    form.find(".CodeInput[changeParam]").change(GetCodeValue);

    // 選択ボタン
    form.find(".SelectButton").click(ShowSelectSub);

    // 日付
    form.find(".DateInput").datepicker({ dateFormat: 'yy/mm/dd' });
}

// コード検索
function GetCodeValue(evt) {
    var code = $(this);
    var p = !!evt.data ? evt.data : eval('(' + $(this).attr('changeParam') + ')');

    // 名称設定
    var setName = function (nodata, nameVal) {
        // フォーカス
        if (nodata) code.focus();

        // グリッドの場合
        if (!!evt.data) {
            var row = code.parents('tr');
            var grid = row.parents('table');

            // 名称を設定
            if (nameVal.length == 0) nameVal = " ";
            grid.setCell(row.attr('id'), p.selectOut, nameVal);

            // エラー設定
            if (nodata) code.parent().addClass('error');  // リロードすると消える
            else code.parent().removeClass('error');
        }
            // フォームの場合
        else {
            // 名称を設定
            $("#" + p.selectOut).val(nameVal);

            // エラー設定
            if (nodata) code.addClass('ui-state-error');
            else code.removeClass('ui-state-error');
        }

    };

    if (code.val().length == 0)
    {
        setName(false, "");
        return;
    }

    // Webサービス呼び出し
    $.getJSON("/Api/CommonApi", {
        argCode: code.val(),
        argSelectId: p.selectId,
        argSelectParam: p.selectParam,
    }).done(function (json) {
        var nodata = json.length == 0;
        var nameVal = nodata ? "データなし" : json;

        setName(nodata, nameVal);
    }).fail(function (xhr) {
        showServerError(xhr);
    });
}

// 検索子画面表示
function ShowSelectSub(evt) {
    var buttonId = $(this).attr('id');
    var grid = !!evt.data;
    var p = grid ? evt.data : eval('(' + $(this).attr('clickParam') + ')');

    var param = 'SelectId=' + escape(p.selectId) +
        "&DbCodeCol=" + escape(p.dbCodeCol) +
        "&DbNameCol=" + escape(p.dbNameCol) +
        "&CodeId=" + escape($("#" + p.codeId).attr('name')) +
        "&CodeLen=" + $("#" + p.codeId).attr('maxLength');

    $('<div id="SubDialog"><iframe width="100%" height="100%" frameborder="0" src="/SubForm?' + param + '"></iframe></div>').dialog({
        title: 'タイトル',
        modal: true,
        width: 430,
        height: 400,
        open: function () {
            $(this).attr({ buttonId: buttonId, codeId: p.codeId, nameId: p.nameId, grid: grid });
        },
        close: function () {
            $(this).dialog('destroy');
        }
    });
}

// インラインフレームのダイアログ側から呼んで
// ダイアログを閉じる
function colseSubDialog(rowData) {
    var dlg = $("#SubDialog");
    dlg.dialog('close');

    if (rowData.code) {
        var code = $("#" + dlg.attr('codeId'));
        code.val(rowData.code);

        // Gridの場合
        if (dlg.attr('grid') == 'true') {
            var row = code.parents('tr');
            var grid = row.parents('table');

            // 名称を設定
            grid.setCell(row.attr('id'), dlg.attr('nameId'), rowData.name);
        }
        else {
            var name = $("#" + dlg.attr('nameId'));
            if (name) name.val(rowData.name);

            // 次項目へフォーカス移動
            var button = $("#" + dlg.attr('buttonId'));
            var next = button.next('input[readonly!=readonly], select, textarea');
            if (next.length > 0) next.focus();
            else button.focus();
        }
    }
}

// QueryStringの値を取得する
function getQueryString(name) {
    if ($(location).attr('search').length > 0) {
        var arr = $(location).attr('search').substr(1).split('&');
        for (var i = 0; i < arr.length; i++) {
            var p = arr[i].split('=');
            if (p[0] == name) return p[1];
        }
    }

    return null;
}

// メッセージ取得
function getMessageText(data, grid) {
    var msgText = "";
    data.messages.forEach(function (msg) {
        if (msg.rowField) {
            if (msg.rowField.RowNumber && grid) msgText += grid.getInd(msg.rowField.RowNumber) + '行目 ';
            if (msg.rowField.FieldName) msgText += msg.rowField.FieldName + ' : ';
        }
        //msg.rowField.DataTableName
        msgText += msg.message;
    });

    return msgText;
}

// サーバ側のエラーをダイアログ表示する
function showServerError(xhr) {
    if (xhr.responseJSON) {
        var data = xhr.responseJSON;
        if (data.error) showError(getMessageText(data));
        else if (data.exceptionMessage) showError(data.exceptionMessage);
        else if (data.message) showError(data.message);
}
    else showError(xhr.responseText);
}

// Infoダイアログ表示
function showInfo(msg) {
    $('<div><span class="glyphicon glyphicon-info-sign" style="font-size: 26px; color: blue;"/>' +
        '<span class="dialog-message">' + msg + '</span></div>').dialog({
        title: 'メッセージ',
        buttons: { '閉じる': function () { $(this).dialog('close'); } }
    });
}

// 警告ダイアログ表示
function showAlert(msg) {
    $('<div><span class="glyphicon glyphicon-warning-sign" style="font-size: 26px; color: yellow;"/>' +
        '<span class="dialog-message">' + msg + '</span></div>').dialog({
        title: '警告',
        buttons: { '閉じる': function () { $(this).dialog('close'); } }
    });
}

// エラーダイアログ表示
function showError(msg) {
    $('<div><span class="glyphicon glyphicon-remove-sign" style="font-size: 26px; color: red;"/>' +
        '<span class="dialog-message">' + msg + '</span></div>').dialog({            
        title: 'エラー',
        buttons: { '閉じる': function () { $(this).dialog('close'); } }
    });
}

// 入力項目取得
function getSendInputs(form) {
    return form.find('*:not([readonly]):not([name^=_])').filter(function() { return $(this).val() != ""; });
}

// CSV出力ボタン
function onCsvExportClick(evt) {
    var url = evt.data.url + '?_search=csvexp';
    var param = getSendInputs(evt.data.form).serialize();
    if (param.length > 0) url += '&' + param;

    location.href = url;
}

// 詳細画面表示ボタン共通処理２
function showDetailDialog2(data, oper, title) {
    var grid = data.grid;
    var selrow = grid.getGridParam('selrow');
    var rowData = selrow ? grid.getRowData(selrow) : null;
    var param = '_mode=' + oper + '&' + getDetailSearchParam(rowData);

    $('<div id="DetailDialog"><iframe width="100%" height="100%" frameborder="0" src="' + $("#EntryForm").val() + '?' + param + '"></iframe></div>').dialog({
        title: title,
        modal: true,
        //width: 1024,
        //height: 700,
        width: Math.min($(window).width(), 1024),
        height: Math.min($(window).height(), 600),
        close: function () {
            $(this).dialog('destroy');
        }
    });
}

// インラインフレームのダイアログ側から呼んで
// ダイアログを閉じる
function colseDetailDialog(rowData) {
    var dlg = $("#DetailDialog");
    dlg.dialog('close');
}

// 新規ボタン
function onAddClick(evt) {
    showDetailDialog(evt.data, 'new');
}

// 修正ボタン
function onEditClick(evt) {
    showDetailDialog(evt.data, 'edit');
}

// 参照ボタン
function onViewClick(evt) {
    showDetailDialog(evt.data, 'view');
}

// 新規ボタン
function onAddClick2(evt) {
    showDetailDialog2(evt.data, 'new', $(this).val());
}

// 修正ボタン
function onEditClick2(evt) {
    showDetailDialog2(evt.data, 'edit', $(this).val());
}

// 参照ボタン
function onViewClick2(evt) {
    showDetailDialog2(evt.data, 'view', $(this).val());
}

/*
// 編集グリッドクラス
function EditGrid(conf) {
    //conf.grid
    //conf.editDlg
}

// 編集グリッド prototype
EditGrid.prototype = {
    constructor: EditGrid,
}*/
