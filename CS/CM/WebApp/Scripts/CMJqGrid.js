// 初期化 グリッド用
function commonGridInit() {
    // 共通の初期化
    commonInit();

    // グリッドデフォルト値設定
    $.jgrid.defaults = {
        altRows: true,
        datatype: 'local',
        loadonce: true,
        //multiselect: true,
        rownumbers: true,
        shrinkToFit: false,
        viewrecords: true,
        //width: 950,
        width: $("#GridPanel").width(),
        height: 'auto',
        loadError: function (xhr) { showServerError(xhr); }
    };

    // グリッド拡張
    $.jgrid.extend({
        // refresh
        refreshGrid : function () {
            this.trigger("reloadGrid");
            this.setButtonState(true);
        },
        // ボタン状態設定functionを設定する
        setButtonStateFunc: function (func) {
            this[0].p.setButtonState = func;
        },
        // ボタン状態設定
        setButtonState: function (flg) {
            if (this[0].p.setButtonState) {
                this[0].p.setButtonState(flg);
            }
        },
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
    var val = sts ? "trash" : "refresh";
    var func = sts ? "deleteRow" : "cancelEdit";

    return '<button class="SelectButton btn btn-default" onclick="' + func + '(' + "'" +
        opts.gid + "'," + opts.rowId + ')"><span class="glyphicon glyphicon-' + val + '"></span></button>';
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
        url: editurl,
        gridComplete: function () {
            $(this).showErrorMessage();
            // 削除行のCSSクラスを設定
            for (var i = 0; i < delRows.length; i++)
                $(this).setRowData(delRows[i], false, 'deleted');
        },
        // 行選択
        onSelectRow: function (id) {
            // 編集行のデータを復元
            $(this).rejectEditRow();

            // ボタンの操作可否を設定
            var flg = $(this).getGridParam('selrow') == null;
            $(this).setButtonState(flg);
        },
        // 行追加
        //afterInsertRow : function (id) {},
        // 行編集
        ondblClickRow: function (id) {
            // 状態が非表示の場合、編集不可
            if ($(this).getColProp('状態').hidden) return;

            // 削除行は編集不可
            var sts = $(this).getCell(id, '状態');
            if (sts.match(/^削除/)) return;

            // 編集前のデータを退避
            $(this).saveEditRow(id);

            // 編集開始
            $(this).editRow(id, true, null, null, null, null, function (rowid, response) {
                $(this).commitEditRow();
                if (response.responseText.length > 0) {
                    var retId = parseInt(JSON.parse(response.responseText));
                    if (sts == "") {
                        $(this).setCell(retId, '状態', '2');
                        //$(this).setFrozenColumns();
                        $(this).refreshGrid();
                    }
                }
            });
        },
    });

    // 列固定
    grid.setFrozenColumns();

    if (pagerId) {
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
                    $(this).refreshGrid();

                    return [true];
                }
            },
            // 削除ボタン
            {
                jqModal: false, width: 350, closeOnEscape: true, afterComplete:
                function (response, postdata) {
                    $(this).refreshGrid();
                }
            },
            { modal: true, multipleSearch: true, closeOnEscape: true },
            { jqModal: false, navkeys: [true, 38, 40], closeOnEscape: true }
        );
    }

    return grid;
}

// 詳細ダイアログ作成
function createDetailDialog(dlgId, rules, grid) {
    var dlg = $("#" + dlgId);
    var form = $("form", "#" + dlgId);

    // 標準操作追加
    addCommonEvent(form);

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
            else form.find(":input").val("");

            // エラー消去
            validator.resetForm();

            // open後処理があれば実行
            if (dlg.oper != 'view' && dlg.afterOpen) dlg.afterOpen();
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
        //width: Math.min($("#GridPanel").width(), 800)
});

    return dlg;
}

// 選択ボタン追加
function addSelectButton(code, data) {
    var btnId = 'Btn' + code.attr('id');
    code.after('<span class="left-addon"><i class="glyphicon glyphicon-search"/><input id="' + btnId + '" class="SelectButton btn btn-default" type="button"/></span>');
    data.codeId = code.attr('id');

    // イベントハンドラ設定
    $("#" + btnId).click(data, ShowSelectSub);
}

var delRows = [];

// 行削除ボタン
function deleteRow(gid, id) {
    var grid = $("#" + gid);

    // 編集行を戻す
    grid.rejectEditRow();

    $.ajax({
        url: grid.getGridParam('editurl'),
        dataType: 'json',
        type: 'DELETE',
        data: { oper: 'del', id: id }
    }).done(function (data) {
        if (data.error) {
            showError(data.messages[0].message);
        } else {
            // 状態を削除に変更
            grid.setCell(id, '状態', '3');
            // 削除行IDを記憶
            delRows[delRows.length] = id;
            grid.refreshGrid();
        }
    }).fail(function (xhr) {
        showServerError(xhr);
    });
}

// 取消ボタン
function cancelEdit(gid, id) {
    var grid = $("#" + gid);

    // 編集行を戻す
    grid.rejectEditRow();

    $.ajax({
        url: grid.getGridParam('editurl'),
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

            // 削除行IDを削除
            for (var i = 0; i < delRows.length; i++) {
                if (delRows[i] == id) {
                    delRows.splice(i, 1);
                    break;
                }
            }

            // エラー表示消去
            grid.resetErrorMessage(id);

            grid.refreshGrid();
        }
    }).fail(function (xhr) {
        showServerError(xhr);
    });
}

// 検索ボタン
function onSelectClick(evt) {
    var grid = evt.data.grid;
    var postData = grid.getGridParam('postData');

    // データクリア
    $.each(postData, function (key) { delete postData[key] });

    // Formのデータを結合
    $.each(getSendInputs(evt.data.form).serializeArray(), function (id, data) {
        if (data.value && data.value.length > 0) postData[data.name] = data.value;
    });

    // 検索
    grid.setGridParam({ datatype: 'json', postData: postData });
    grid.setErrorMessage(null);
    grid.refreshGrid();
}

// 詳細画面表示ボタン共通処理
function showDetailDialog(data, oper) {
    var grid = data.grid;
    var dlg = data.editDlg;

    // 編集行を戻す
    grid.rejectEditRow();

    var id;
    var title;
    var method;

    // 新規の場合
    if (oper == 'new') {
        title = '新規';
        id = '_empty';
        method = 'PUT';
    }
    // 修正の場合
    else if (oper == 'edit') {
        title = '修正';
        id = grid.getGridParam('selrow');
        method = 'POST';

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

        dlg.inputs = dlg.find("input:not([readonly]), textarea:not([readonly])");
        dlg.buttons = dlg.find("input[type=button], button");
        dlg.selects = dlg.find("select:not([disabled]), :checkbox:not([disabled])");
    }

    // oper保存
    dlg.oper = oper;

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
                    url: grid.getGridParam('editurl'),
                    dataType: 'json',
                    type: method,
                    data: 'oper=' + oper + '&id=' + id + '&' + getSendInputs(form).serialize()
                }).done(function (data) {
                    if (data.error) {
                        showError(data.messages[0].message);
                    } else {
                        grid.destroyFrozenColumns();

                        // 新規の場合
                        if (oper == 'new') {
                            var retId = parseInt(JSON.parse(data));
                            // グリッドに行追加
                            grid.FormToGrid(retId, form, 'add');
                            grid.setCell(retId, '状態', '1');
                        }
                        // 修正の場合
                        else if (oper == 'edit') {
                            if (data.length > 0) {
                                var retId = parseInt(JSON.parse(data));
                                // グリッドにデータ設定
                                grid.FormToGrid(retId, form);
                                if (sts == "") grid.setCell(retId, '状態', '2');
                            }
                        }

                        grid.setFrozenColumns(); // 0行のときリフレッシュされないので追加
                        grid.refreshGrid(); /// ここが問題

                        dlg.dialog("close");
                    }
                }).fail(function (xhr) {
                    showServerError(xhr);
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

// 登録ボタン
function onCommitClick(evt) {
    var grid = evt.data.grid;

    // 編集行を戻す
    grid.rejectEditRow();

    if (!confirm("登録しますか？")) return;

    $.ajax({
        url: grid.getGridParam('editurl'),
        dataType: 'json',
        type: 'POST',
        data: { oper: 'commit' }
    }).done(function (data) {
        if (data.error) {
            grid.setErrorMessage(data.messages);
            showError(getMessageText(data, grid));
        } else {
            showInfo('登録しました。');

            // 再検索
            grid.setGridParam({ datatype: 'json' });
            grid.setErrorMessage(null);
        }

        grid.refreshGrid();

    }).fail(function (xhr) {
        showServerError(xhr);
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
                            url: grid.getGridParam('editurl'),
                            type: 'DELETE',
                            data: { oper: 'del', id: id }
                        }).done(function (data) {
                            if (data.error) {
                                showError(data.messages[0].message);
                            } else {
                                // 状態を削除に変更
                                for (var i = 0; i < ids.length; i++) {
                                    grid.setCell(ids[i], '状態', '3');
                                }
                                grid.refreshGrid();
                            }
                        }).fail(function (xhr) {
                            showServerError(xhr);
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

// 参照モードにする
function setViewMode(grid) {
    grid.destroyFrozenColumns();
    grid.hideCol(['状態', '操作']);
    grid.setFrozenColumns();
}