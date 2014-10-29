// 詳細画面初期処理
function initDetail(form, grids, url) {
    // 画面の状態設定
    var mode = getQueryString('_mode');
    var inputs, buttons, selects;

    switch (mode) {
        case 'edit':
            inputs = form.find("input[key=true]");
            selects = form.find("select[key=true]");
            break;

        case 'view':
            inputs = form.find("input:not([readonly]), textarea:not([readonly])");
            buttons = form.find("input[type=button], button");
            selects = form.find("select:not([disabled]), :checkbox:not([disabled])");
            $("#BtnCommit").attr('disabled', 'disabled');

            // グリッドを参照モードに設定
            if (grids) {
                for (var tname in grids) {
                    setViewMode(grids[tname]);
                }
            }
            break;
    }

    // 読み取り専用にする
    if (inputs) inputs.each(function () {
        $(this).attr('readonly', true);
    });

    if (buttons) buttons.each(function () {
        $(this).css('display', 'none');
    });

    if (selects) selects.each(function () {
        $(this).attr('disabled', 'disabled');
    });

    // 詳細画面データ検索
    selectDetail(form, grids, url + location.search);
}

// 詳細画面データ検索
function selectDetail(form, grids, url) {
    // 初期検索
    $.ajax({
        async: false,
        dataType: 'json',
        data: '_search=edit',
        url: url
    }).done(function (data) {
        if (data.error) {
            showError(data.messages[0].message);
        } else {
            // フォームに検索結果を設定
            for (var key in data.firstRow) {
                var i = form.find(':input[name=' + key + ']');
                if (i) i.val(data.firstRow[key]);
            }

            // グリッドに検索結果を設定
            if (grids) {
                for (var tname in grids) {
                    var grid = grids[tname];
                    grid.setGridParam({ datatype: 'json' });
                    grid[0].addJSONData(data.tables[tname]);
                    grid.setGridParam({ datatype: 'local' });

                    // 状態クリア
                    grid.setErrorMessage(null);
                    grid.refreshGrid();
                }
            }
        }
    }).fail(function (xhr) {
        showServerError(xhr);
    });
}

// 登録ボタン2
function onCommitClick2(evt) {
    // 入力値検証
    var form = evt.data.form;
    if (!form.valid()) return;

    var mode = getQueryString('_mode');
    var oper = form.posted && mode == 'new' ? 'edit' : mode;
    var id = !form.posted && mode == 'new' ? '_empty' : '0';
    var method = oper == 'edit' ? 'POST' : 'PUT';

    // 送信
    $.ajax({
        async: false,
        dataType: 'json',
        type: method,
        data: 'oper=' + oper + '&id=' + id + '&' + form.find('*:not([name^=_])').serialize(),
        url: evt.data.url
    }).done(function (data) {
        if (data.error) {
            showError(data.messages[0].message);
        } else {
            // 送信済フラグ設定
            form.posted = true;
        }
    }).fail(function (xhr) {
        showError(xhr.responseText);
    });

    // 登録
    $.ajax({
        async: false,
        dataType: 'json',
        type: 'POST',
        data: { oper: 'commit' },
        url: evt.data.url
    }).done(function (data) {
        if (data.error) {
            var errmsg = "";
            data.messages.forEach(function (msg) {
                if (msg.rowField && msg.rowField.FieldName) errmsg += msg.rowField.FieldName + ' : ';
                errmsg += msg.message;
            });

            showError(errmsg);
        } else {
            showInfo('登録しました。');
            // 送信済フラグOFF
            form.posted = false;
        }

    }).fail(function (xhr) {
        showServerError(xhr);
    });

    // 再検索url
    var url = evt.data.url + '?_mode=' + mode + '&' + form.find(":input[key=true]").serialize();

    // 詳細画面データ再検索
    selectDetail(form, evt.data.grids, url);
}
