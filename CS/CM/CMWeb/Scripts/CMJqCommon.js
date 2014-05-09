// 初期化
function commonInit() {
    // グリッドデフォルト値設定
    $.jgrid.defaults = {
        altRows: true,
        datatype: 'local',
        loadonce: true,
        multiselect: true,
        rownumbers: true,
        shrinkToFit: false,
        viewrecords: true,
        width: 950,
        height: 'auto',
    };

    // Date型取得設定
    $.ajaxSetup({
        converters: {
            "text json": function (text) {
                return JSON.parse(text, reviveDate);
            }
        }
    });
}

// Date型 デシリアライズ
function reviveDate(key, value) {
    if (value == null ||
        value.constructor !== String)
        return value;
    var m = /^\/Date\((\d+)(.+)?\)\/$/g.exec(value);
    if (!m) return value;
    return new Date(parseInt(m[1]));
}

// 状態列のカスタムフォーマッター
function statusFormatter(cellval, opts) {
    if (cellval == "0") return "";

    var ret;
    if (cellval == "1") ret = "新規";
    else if (cellval == "2") ret = "修正";
    else if (cellval == "3") ret = "削除";

    return ret + '<input type="button" value="取消" onclick="cancelEdit(' + "'" + opts.gid + "'," + opts.rowId + ')"/>';
}

// グリッド作成
function createGrid(gid, colNames, colModel, editurl, pagerId) {
    var lastRowId;
    var grid = $("#" + gid);

    // グリッド作成
    grid.jqGrid({
        colNames: colNames,
        colModel: colModel,
        editurl: editurl,
        pager: pagerId,
        //url: 'CMSM010F01.aspx',
        // 行選択
        onSelectRow: function (id) {
            if (lastRowId) {
                $(this).restoreRow(lastRowId);
                lastRowId = null;
            }

            // ボタンの操作可否を設定
            var flg = $(this).getGridParam('selrow') == null;
            setButtonState(flg);
        },
        // 行追加
        //afterInsertRow : function (id) {},
        // 行編集
        ondblClickRow: function (id) {
            if (id !== lastRowId) {
                $(this).restoreRow(lastRowId);
                lastRowId = id;
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
    grid.setFrozenColumns();

    // 検索有効
    grid.navGrid('#' + pagerId, { edit: false, add: true, del: true, search: true, view: true },
        {},
        // 新規ボタン
        {
            jqModal: false, savekey: [true, 13], closeOnEscape: true, closeAfterAdd: true, afterSubmit:
            function (response, postdata) {
                // グリッドに行追加
                var id = parseInt(response.responseText);
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

    return grid;
}

// コード検索
function GetCodeValue() {
    var code = $(this);

    // Webサービス呼び出し
    $.getJSON("../CMCommonService.svc/GetCodeName", {
        argCode: code.val(),
        argSelectId: code.attr('selectId'),
        argSelectParam: code.attr('selectParam'),
    }).done(function (json) {
        var name = $("#" + code.attr('selectOut'));
        var nameVal = json.d.Name;

        // 名称を設定
        name.val(nameVal);

        // 背景色設定
        var color = nameVal.length > 0 ? "" : "Pink";
        code.css("background-color", color);

        // エラー表示＆フォーカス
        if (nameVal.length == 0) {
            name.val("データなし");
            code.focus();
        }
    }).fail(function (xhr) {
        showError(xhr.responseText);
    });
}

// 検索子画面表示
function ShowSelectSub() {
    var buttonId = $(this).attr('id');
    var codeId = $(this).attr('codeId');
    var nameId = $(this).attr('nameId');
    var param = 'SelectId=' + escape($(this).attr('selectId')) +
        "&DbCodeCol=" + escape($(this).attr('dbCodeCol')) +
        "&DbNameCol=" + escape($(this).attr('dbNameCol')) +
        "&CodeId=" + escape(codeId) +
        "&CodeLen=" + $("#" + codeId).attr('maxLength');

    $('<div id="SubDialog"><iframe width="100%" height="100%" frameborder="0" src="../CM2/CMSubForm.aspx?' + param + '"></iframe></div>').dialog({
        title: 'タイトル',
        modal: true,
        width: 430,
        height: 400,
        open: function () {
            $(this).attr({ buttonId: buttonId, codeId: codeId, nameId: nameId });
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

    if (rowData) {
        var code = $("#" + dlg.attr('codeId'));
        code.val(rowData.code);

        var name = $("#" + dlg.attr('nameId'));
        if (name) name.val(rowData.name);

        var button = $("#" + dlg.attr('buttonId'));
        var next = button.next('input[readonly!=readonly], select, textarea');
        if (next.length > 0) next.focus();
        else button.focus();
    }
}

// サブグリッド作成
function createSubGrid(gid, colNames, colModel, pagerId) {
    var grid = $("#" + gid);

    // グリッド作成
    grid.jqGrid({
        colNames: colNames,
        colModel: colModel,
        pager: pagerId,
        multiselect: false,
        //shrinkToFit: true,
        viewrecords: true,
        width: 390,
        // 行選択
        onSelectRow: function (id) {
            window.parent.colseSubDialog(grid.getRowData(id));
        },
    });

    // 検索有効
    grid.navGrid('#' + pagerId, { edit: false, add: false, del: false, search: true },
        {}, {}, {},
        { modal: true, multipleSearch: true, closeOnEscape: true }
    );

    return grid;
}

// 詳細ダイアログ作成
function createDetailDialog(dlgId, rules, gid) {
    var dlg = $("#" + dlgId);
    var form = $("form", "#" + dlgId);
    var grid = $("#" + gid);

    // validator作成
    var validator = form.validate({
        errorClass: 'ui-state-error',
        rules: rules
    });

    // 詳細ダイアログ作成
    dlg.dialog({
        autoOpen: false,
        modal: true,
        open: function () {
            // 選択行があればデータを表示
            var selrow = grid.getGridParam('selrow');
            if (selrow) grid.GridToForm(selrow, form);
            else form.find(":text").val("");

            // エラー消去
            validator.resetForm();
        },
        close: function () {
            // エラー表示消去
            form.find(":text").each(function () {
                $(this).hideBalloon();
            });
        },
        width: 'auto'
    });

    return dlg;
}

// Infoダイアログ表示
function showInfo(msg) {
    $('<div><table><tr><td><span class="ui-icon ui-icon-info"/></td><td>' +
        msg + '</td></tr></table></div>').dialog({
        title: 'メッセージ',
        buttons: { '閉じる': function () { $(this).dialog('close'); } }
    });
}

// 警告ダイアログ表示
function showAlert(msg) {
    $('<div><table><tr><td><span class="ui-icon ui-icon-alert"/></td><td>' +
        msg + '</td></tr></table></div>').dialog({
        title: '警告',
        buttons: { '閉じる': function () { $(this).dialog('close'); } }
    });
}

// エラーダイアログ表示
function showError(msg) {
    $('<div><table><tr><td><span class="ui-icon ui-icon-circle-close"/></td><td>' +
        msg + '</td></tr></table></div>').dialog({
        title: 'エラー',
        buttons: { '閉じる': function () { $(this).dialog('close'); } }
    });
}

// 検索ボタン
function onSelectClick(evt) {
    var grid = evt.data.grid;
    var postData = grid.getGridParam('postData');

    // Formのデータを結合
    evt.data.form.serializeArray().forEach(function (data) {
        postData[data.name] = data.value;
    });

    // 検索
    grid.setGridParam({ datatype: 'json', postData: postData });
    grid.trigger('reloadGrid');
    setButtonState(true);
}

// CSV出力ボタン
function onCsvExportClick(evt) {
    location.href = '?oper=csvexp&' + evt.data.form.serialize();
}

// 取消ボタン
function cancelEdit(gid, id) {
    var grid = $("#" + gid);

    $.ajax({
        dataType: 'json',
        type: 'POST',
        data: { oper: 'cancel', id: id },
    }).done(function (data) {
        if (data.error) {
            showError(data.messages[0].message);
        } else {
            // 状態を初期化
            var sts = grid.getCell(id, '状態');
            if (sts.match(/^新規/)) grid.delRowData(id);
            else {
                if (grid.setRowData(id, data))
                    grid.setCell(id, '状態', '0');
            }
            grid.trigger("reloadGrid");
            setButtonState(true);
        }
    }).fail(function (xhr) {
        showError(xhr.responseText);
    });
}

// 新規ボタン
function onAddClick(evt) {
    var grid = evt.data.grid;
    var dlg = evt.data.editDlg;

    // オプション設定
    dlg.dialog('option', 'title', '新規');
    dlg.dialog('option', 'buttons', {
        '登録': function () {
            // 操作対象のフォーム要素を取得
            var form = dlg.find("form");

            // 入力値検証
            if (form.valid()) {
                // 送信
                $.ajax({
                    dataType: 'json',
                    type: 'POST',
                    data: 'oper=new&id=_empty&' + form.serialize()
                }).done(function (data) {
                    if (data.error) {
                        showError(data.messages[0].message);
                    } else {
                        // グリッドに行追加
                        grid.FormToGrid(data.id, form, 'add');
                        grid.setCell(data.id, '状態', '1');
                        grid.trigger("reloadGrid");
                        setButtonState(true);
                        dlg.dialog("close");
                    }
                }).fail(function (xhr) {
                    showError(xhr.responseText);
                });
            }
        },
        '閉じる': function () {
            // thisは、ダイアログボックス
            $(this).dialog("close");
        }
    });

    // ダイアログ表示
    dlg.dialog('open');
}

// 修正ボタン
function onEditClick(evt) {
    var grid = evt.data.grid;
    var dlg = evt.data.editDlg;

    var id = grid.getGridParam('selrow');

    // 削除行は編集不可
    var sts = grid.getCell(id, '状態');
    if (sts.match(/^削除/)) {
        showAlert('削除行は修正できません。');
        return;
    }

    var inputs = dlg.find("input[key=true]");
    var selects = dlg.find("select[key=true]");

    // オプション設定
    dlg.dialog('option', 'title', '修正');
    dlg.dialog('option', 'buttons', {
        '登録': function () {
            // 操作対象のフォーム要素を取得
            var form = dlg.find("form");

            // 入力値検証
            if (form.valid()) {
                // 送信
                $.ajax({
                    //dataType: 'json',
                    type: 'POST',
                    data: 'oper=edit&id=' + id + '&' + form.serialize()
                }).done(function (data) {
                    if (data.error) {
                        showError(data.messages[0].message);
                    } else {
                        // グリッドにデータ設定
                        grid.FormToGrid(id, form);
                        if (sts == "") grid.setCell(id, '状態', '2');
                        grid.trigger("reloadGrid");
                        setButtonState(true);

                        // 読み取り専用を戻す
                        inputs.each(function () {
                            $(this).removeAttr('readonly');
                        });

                        selects.each(function () {
                            $(this).removeAttr('disabled');
                        });

                        dlg.dialog("close");
                    }
                }).fail(function (xhr) {
                    showError(xhr.responseText);
                });
            }
        },
        '閉じる': function () {
            // 読み取り専用を戻す
            inputs.each(function () {
                $(this).removeAttr('readonly');
            });

            selects.each(function () {
                $(this).removeAttr('disabled');
            });

            $(this).dialog("close");
        }
    });

    // 読み取り専用にする
    inputs.each(function () {
        $(this).attr('readonly', true);
    });

    selects.each(function () {
        $(this).attr('disabled', 'disabled');
    });

    // ダイアログ表示
    dlg.dialog('open');
}

// 参照ボタン
function onViewClick(evt) {
    var grid = evt.data.grid;
    var dlg = evt.data.editDlg;

    var inputs = dlg.find("input[readonly!=readonly]");
    var selects = dlg.find("select[disabled!=disabled]");

    // オプション設定
    dlg.dialog('option', 'title', '参照');
    dlg.dialog('option', 'buttons', {
        '閉じる': function () {
            // 読み取り専用を戻す
            inputs.each(function () {
                $(this).removeAttr('readonly');
            });

            selects.each(function () {
                $(this).removeAttr('disabled');
            });

            $(this).dialog("close");
        }
    });

    // 読み取り専用にする
    inputs.each(function () {
        $(this).attr('readonly', true);
    });

    selects.each(function () {
        $(this).attr('disabled', 'disabled');
    });

    // ダイアログ表示
    dlg.dialog('open');
}

// 削除ボタン
function onDelClick(evt) {
    var grid = evt.data.grid;
    var selarrrow = grid.getGridParam('selarrrow');
    var errmsg = null;

    if (selarrrow.length > 0) {
        var id = "";

        // POSTデータ作成
        for (var i = 0; i < selarrrow.length; i++) {
            var sts = grid.getCell(selarrrow[i], '状態');
            if (sts.match(/^新規/)) {
                errmsg = "新規行は削除できません。";
                id = "";
                break;
            }

            if (sts.match(/^削除/)) continue;

            if (id.length > 0) id += ",";
            id += selarrrow[i];
        }

        if (id.length > 0) {
            // 削除確認ダイアログ表示
            var ids = id.split(",");
            $('<div>選択したレコードを削除しますか？ (' + ids.length + '件)</div>').dialog({
                title: '削除',
                buttons: {
                    '削除': function () {
                        $.ajax({
                            type: 'POST',
                            data: { oper: 'del', id: id }
                        }).done(function (data) {
                            if (data.error) {
                                showError(data.messages[0].message);
                            } else {
                                // 状態を削除に変更
                                for (var i = 0; i < ids.length; i++) {
                                    grid.setCell(ids[i], '状態', '3');
                                }
                                grid.trigger("reloadGrid");
                                setButtonState(true);
                            }
                        }).fail(function (xhr) {
                            showError(xhr.responseText);
                        });

                        $(this).dialog('close');
                    },
                    'キャンセル': function () { $(this).dialog('close'); }
                }
            });
        }
        else if (!errmsg) errmsg = "削除可能な行がありません。";
    }
        // 選択行なし
    else errmsg = "行を選択して下さい。";

    // エラーダイアログ表示
    if (errmsg) showAlert(errmsg);
}

// 登録ボタン
function onCommitClick(evt) {
    if (!confirm("登録しますか？")) return;

    $.ajax({
        dataType: 'json',
        type: 'POST',
        data: { oper: 'commit' }
    }).done(function (data) {
        if (data.error) {
            showError(data.messages[0].message);
        } else {
            showInfo('登録しました。');

            // 再検索
            var grid = evt.data.grid;
            grid.setGridParam({ datatype: 'json' });
            grid.trigger('reloadGrid');
            setButtonState(true);
        }
    }).fail(function (xhr) {
        showError(xhr.responseText);
    });
}

// 編集グリッドクラス
function EditGrid(conf) {
    //conf.grid
    //conf.editDlg
}

// 編集グリッド prototype
EditGrid.prototype = {
    constructor: EditGrid,
}
