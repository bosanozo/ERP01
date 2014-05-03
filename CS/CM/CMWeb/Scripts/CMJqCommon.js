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
function onSelectClick(formId, gid) {
    var grid = $("#" + gid);
    var postData = grid.getGridParam('postData');

    // Formのデータを結合
    $("#" + formId).serializeArray().forEach(function (data) {
        postData[data.name] = data.value;
    });

    // 検索
    grid.setGridParam({ datatype: 'json', postData: postData });
    grid.trigger('reloadGrid');
    setButtonState(true);
}

// CSV出力ボタン
function onCsvExportClick(formId) {
    location.href = '?oper=csvexp&' + $("#" + formId).serialize();
}

// 取消ボタン
function cancelEdit(gid, id) {
    var grid = $("#" + gid);

    $.ajax({
        //url: grid.getGridParam('url'),
        data: { oper: 'cancel', id: id },
        dataType: 'json'
    }).done(function (data) {
        // 状態を初期化
        var sts = grid.getCell(id, '状態');
        if (sts.match(/^新規/)) grid.delRowData(id);
        else {
            if (grid.setRowData(id, data))
                grid.setCell(id, '状態', '0');
        }
        grid.trigger("reloadGrid");
        setButtonState(true);
    }).fail(function (xhr) {
        showError(xhr.responseText);
    });
}

// 新規ボタン
function onAddClick(gid, dlgId) {
    var grid = $("#" + gid);
    var dlg = $("#" + dlgId);

    // オプション設定
    dlg.dialog('option', 'title', '新規');
    dlg.dialog('option', 'buttons', {
        '登録': function () {
            // 操作対象のフォーム要素を取得
            var form = $("form", "#" + dlgId);

            // 入力値検証
            if (form.valid()) {
                // 送信
                $.ajax({
                    type: 'POST',
                    data: 'oper=add&id=_empty&' + form.serialize()
                }).done(function () {
                    // グリッドに行追加
                    var id = grid.getGridParam('records');
                    grid.FormToGrid(id, $("form", "#" + dlgId), 'add');
                    grid.setCell(id, '状態', '1');
                    grid.trigger("reloadGrid");
                    setButtonState(true);
                    dlg.dialog("close");
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
function onEditClick(gid, dlgId) {
    var grid = $("#" + gid);
    var dlg = $("#" + dlgId);

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
            var form = $("form", "#" + dlgId);

            // 入力値検証
            if (form.valid()) {
                // 送信
                $.ajax({
                    type: 'POST',
                    data: 'oper=edit&id=' + id + '&' + form.serialize()
                }).done(function () {
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
function onViewClick(gid, dlgId) {
    var grid = $("#" + gid);
    var dlg = $("#" + dlgId);

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
function onDelClick(gid) {
    var grid = $("#" + gid);
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
                            //url: grid.getGridParam('editurl'),
                            data: { oper: 'del', id: id }
                        }).done(function (msg) {
                            // 状態を削除に変更
                            for (var i = 0; i < ids.length; i++) {
                                grid.setCell(ids[i], '状態', '3');
                            }
                            grid.trigger("reloadGrid");
                            setButtonState(true);
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
function onCommitClick(gid) {
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
            var grid = $("#" + gid);
            grid.setGridParam({ datatype: 'json' });
            grid.trigger('reloadGrid');
            setButtonState(true);
        }
    }).fail(function (xhr) {
        showError(xhr.responseText);
    });
}

