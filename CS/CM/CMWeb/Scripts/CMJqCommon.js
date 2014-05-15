// 初期化
function commonInit() {
    // グリッドデフォルト値設定
    $.jgrid.defaults = {
        altRows: true,
        datatype: 'local',
        loadonce: true,
        //multiselect: true,
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

    // グリッド拡張
    $.jgrid.extend({
        // エラーメッセージを設定する
        setErrorMessage: function (msgs) {
            this[0].p.errmsgs = msgs;
        },
        // エラーメッセージを表示する
        showErrorMessage: function () {
            var grid = this;

            if (this[0].p.errmsgs) {
                // エラーの表示
                this[0].p.errmsgs.forEach(function (msg) {
                    if (msg.rowField && msg.rowField.RowNumber >= 0) {
                        var field = msg.rowField.FieldName ? msg.rowField.FieldName : '状態';
                        grid.setCell(msg.rowField.RowNumber, field, '', 'error', { title: msg.message });
                    }
                });
            }
        },
        // 指定行のエラーメッセージをリセットする
        resetErrorMessage: function (id) {
            var errmsgs = this[0].p.errmsgs;
            if (errmsgs) {
                for (var i = 0; i < errmsgs.length; i++) {
                    if (errmsgs[i].rowField && errmsgs[i].rowField.RowNumber == id) {
                        errmsgs.splice(i, 1);
                        break;
                    }
                }
            }
        },
        // 編集行データ退避
        saveEditRow: function (id) {
            this[0].p.editRowId = id;
            this[0].p.editRowData = this.getRowData(id);
        },
        // 編集行データ復元
        rejectEditRow: function () {
            var id = this[0].p.editRowId;

            if (id) {
                this.setRowData(id, this[0].p.editRowData);
                this.restoreRow(id);
                this[0].p.editRowId = null;
            }
        },
        // 編集行確定
        commitEditRow: function () {
            this[0].p.editRowId = null;
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
    var ret;
    if (cellval == "0") return "";
    else if (cellval == "1") ret = "新規";
    else if (cellval == "2") ret = "修正";
    else if (cellval == "3") ret = "削除";
    else ret = cellval;

    return ret;
}

// 操作列のカスタムフォーマッター
function actionFormatter(cellval, opts, data) {
    var sts = data.状態 == "0" || data.状態 == undefined || data.状態 == "";
    var val = sts ? "削除" : "取消";
    var func = sts ? "deleteRow" : "cancelEdit";

    return '<input type="button" value="' + val +'" onclick="' + func + '(' + "'" + opts.gid + "'," + opts.rowId + ')"/>';
}

// グリッド作成
function createGrid(gid, colNames, colModel, editurl, pagerId) {
    var grid = $("#" + gid);

    // グリッド作成
    grid.jqGrid({
        colNames: colNames,
        colModel: colModel,
        editurl: editurl,
        pager: pagerId,
        //url: 'CMSM010F01.aspx',
        gridComplete: function () {
            $(this).showErrorMessage();
        },
        // 行選択
        onSelectRow: function (id) {
            // 編集行のデータを復元
            $(this).rejectEditRow();

            // ボタンの操作可否を設定
            var flg = $(this).getGridParam('selrow') == null;
            setButtonState(flg);
        },
        // 行追加
        //afterInsertRow : function (id) {},
        // 行編集
        ondblClickRow: function (id) {
            // 削除行は編集不可
            var sts = $(this).getCell(id, '状態');
            if (sts.match(/^削除/)) return;

            // 編集前のデータを退避
            $(this).saveEditRow(id);

            // 編集開始
            $(this).editRow(id, true, null, null, null, null, function (rowid, response) {
                $(this).commitEditRow();
                if (response.responseText.length > 0) {
                    var retId = parseInt(response.responseText);
                    if (sts == "") {
                        $(this).setCell(retId, '状態', '2');
                        //$(this).setFrozenColumns();
                        $(this).trigger("reloadGrid");
                        setButtonState(true);
                    }
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

// 選択ボタン追加
function addSelectButton(code, data) {
    var btnId = 'Btn' + code.attr('id');
    code.after('<input id="' + btnId + '" class="SelectButton" type="button" value="..."/>');
    data.codeId = code.attr('id');
    // イベントハンドラ設定
    $("#" + btnId).click(data, ShowSelectSub);
}

// コード検索 グリッド
function GetCodeValueGrid(evt) {
    var code = $(this);

    // Webサービス呼び出し
    $.getJSON("../CMCommonService.svc/GetCodeName", {
        argCode: code.val(),
        argSelectId: evt.data.selectId,
        argSelectParam: evt.data.selectParam,
    }).done(function (json) {
        var row = code.parents('tr');
        var grid = row.parents('table');
        var nameVal = json.d.Name;

        // 背景色設定
        var color = nameVal.length > 0 ? "" : "Pink";
        grid.setCell(row.attr('id'), code.attr('name'), '', 'error'); // リロードすると消える

        // エラー表示＆フォーカス
        if (nameVal.length == 0) {
            nameVal = "データなし";
            code.focus();
        }

        // 名称を設定
        grid.setCell(row.attr('id'), evt.data.selectOut, nameVal);

    }).fail(function (xhr) {
        showError(xhr.responseText);
    });
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
function ShowSelectSub(evt) {
    var buttonId = $(this).attr('id');
    var grid = !!evt.data;
    var p = grid ? evt.data : eval('(' + $(this).attr('clickParam') + ')');

    var param = 'SelectId=' + escape(p.selectId) +
        "&DbCodeCol=" + escape(p.dbCodeCol) +
        "&DbNameCol=" + escape(p.dbNameCol) +
        "&CodeId=" + escape($("#" + p.codeId).attr('name')) +
        "&CodeLen=" + $("#" + p.codeId).attr('maxLength');

    $('<div id="SubDialog"><iframe width="100%" height="100%" frameborder="0" src="../CM2/CMSubForm.aspx?' + param + '"></iframe></div>').dialog({
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

    if (rowData) {
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
            if (selrow) {
                grid.GridToForm(selrow, form);
            }
            else form.find(":text").val("");

            // エラー消去
            validator.resetForm();
        },
        close: function () {
            // 読み取り専用を戻す
            if (dlg.inputs)
                dlg.inputs.each(function () {
                    $(this).removeAttr('readonly');
                });

            if (dlg.buttons)
                dlg.buttons.each(function () {
                    $(this).css('display', 'inline');
                });

            if (dlg.selects)
                dlg.selects.each(function () {
                    $(this).removeAttr('disabled');
                });

            dlg.inputs = null;
            dlg.buttons = null;
            dlg.selects = null;

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
    grid.setErrorMessage(null);
    grid.trigger('reloadGrid');
    setButtonState(true);
}

// CSV出力ボタン
function onCsvExportClick(evt) {
    location.href = '?oper=csvexp&' + evt.data.form.serialize();
}

// 行削除ボタン
function deleteRow(gid, id) {
    var grid = $("#" + gid);

    // 編集行を戻す
    grid.rejectEditRow();

    $.ajax({
        dataType: 'json',
        type: 'POST',
        data: { oper: 'del', id: id }
    }).done(function (data) {
        if (data.error) {
            showError(data.messages[0].message);
        } else {
            // 状態を削除に変更
            grid.setCell(id, '状態', '3');
            grid.trigger("reloadGrid");
            setButtonState(true);
        }
    }).fail(function (xhr) {
        showError(xhr.responseText);
    });
}

// 取消ボタン
function cancelEdit(gid, id) {
    var grid = $("#" + gid);

    // 編集行を戻す
    grid.rejectEditRow();

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

            // エラー表示消去
            grid.resetErrorMessage(id);

            grid.trigger("reloadGrid");
            setButtonState(true);
        }
    }).fail(function (xhr) {
        showError(xhr.responseText);
    });
}

// 詳細画面表示ボタン共通処理
function showDetailDialog(data, oper) {
    var grid = data.grid;
    var dlg = data.editDlg;

    // 編集行を戻す
    grid.rejectEditRow();

    var id;
    var title;
    var dataType;

    // 新規の場合
    if (oper == 'new') {
        title = '新規';
        id = '_empty';
        dataType = 'json';
    }
    // 修正の場合
    else if (oper == 'edit') {
        title = '修正';
        id = grid.getGridParam('selrow');
        dataType = 'html';

        // 削除行は編集不可
        var sts = grid.getCell(id, '状態');
        if (sts.match(/^削除/)) {
            showAlert('削除行は修正できません。');
            return;
        }

        dlg.inputs = dlg.find("input[key=true]");
        dlg.selects = dlg.find("select[key=true]");
    }
    // 参照の場合
    else if (oper == 'view') {
        title = '参照';

        dlg.inputs = dlg.find("input[readonly!=readonly]");
        dlg.buttons = dlg.find("input[type=button]");
        dlg.selects = dlg.find("select[disabled!=disabled]");
    }

    // オプション設定
    dlg.dialog('option', 'title', title);
    if (oper != 'view') {
        dlg.dialog('option', 'buttons', {
            '登録': function () {
                // 操作対象のフォーム要素を取得
                var form = dlg.find("form");

                // 入力値検証
                if (!form.valid()) return;

                // 送信
                $.ajax({
                    dataType: dataType,
                    type: 'POST',
                    data: 'oper=' + oper + '&id=' + id + '&' + form.serialize()
                }).done(function (data) {
                    if (data.error) {
                        showError(data.messages[0].message);
                    } else {
                        // 新規の場合
                        if (oper == 'new') {
                            // グリッドに行追加
                            grid.FormToGrid(data.id, form, 'add');
                            grid.setCell(data.id, '状態', '1');
                        }
                        // 修正の場合
                        else if (oper == 'edit') {
                            if (data.length > 0) {
                                var retId = parseInt(data);
                                // グリッドにデータ設定
                                grid.FormToGrid(retId, form);
                                if (sts == "") grid.setCell(retId, '状態', '2');
                            }
                        }

                        grid.trigger("reloadGrid");
                        setButtonState(true);

                        dlg.dialog("close");
                    }
                }).fail(function (xhr) {
                    showError(xhr.responseText);
                });
            },
            '閉じる': function () { $(this).dialog("close"); }
        });
    }
    else dlg.dialog('option', 'buttons', { '閉じる': function () { $(this).dialog("close"); } });

    // 修正、参照の場合
    if (oper != 'new') {
        // 読み取り専用にする
        dlg.inputs.each(function () {
            $(this).attr('readonly', true);
        });

        if (dlg.buttons)
            dlg.buttons.each(function () {
                $(this).css('display', 'none');
            });

        dlg.selects.each(function () {
            $(this).attr('disabled', 'disabled');
        });
    }

    // ダイアログ表示
    dlg.dialog('open');
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

// 登録ボタン
function onCommitClick(evt) {
    var grid = evt.data.grid;

    // 編集行を戻す
    grid.rejectEditRow();

    if (!confirm("登録しますか？")) return;

    $.ajax({
        dataType: 'json',
        type: 'POST',
        data: { oper: 'commit' }
    }).done(function (data) {
        if (data.error) {
            var errmsg = "";
            data.messages.forEach(function (msg) {
                errmsg += grid.getInd(msg.rowField.RowNumber) + '行目 ' + msg.rowField.FieldName + ' : ' + msg.message;
                //msg.rowField.DataTableName
            });

            grid.setErrorMessage(data.messages);
            showError(errmsg);
        } else {
            showInfo('登録しました。');

            // 再検索
            grid.setGridParam({ datatype: 'json' });
            grid.setErrorMessage(null);
        }

        grid.trigger('reloadGrid');
        setButtonState(true);

    }).fail(function (xhr) {
        showError(xhr.responseText);
    });
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

// 編集グリッドクラス
function EditGrid(conf) {
    //conf.grid
    //conf.editDlg
}

// 編集グリッド prototype
EditGrid.prototype = {
    constructor: EditGrid,
}
