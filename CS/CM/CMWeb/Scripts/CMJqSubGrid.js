// サブグリッド作成
function createSubGrid(gid, colNames, colModel, pagerId) {
    var grid = $("#" + gid);

    // グリッドデフォルト値設定
    $.jgrid.defaults = {
        altRows: true,
        loadonce: true,
        rownumbers: true,
        shrinkToFit: false,
        viewrecords: true,
        height: 'auto',
        loadError: function (xhr) { showServerError(xhr); }
    };

    // グリッド作成
    grid.jqGrid({
        colNames: colNames,
        colModel: colModel,
        datatype: 'json', // 自動検索
        pager: pagerId,
        width: 'auto',
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

// 検索ボタン
function onSelectClick(evt) {
    var grid = evt.data.grid;
    var postData = grid.getGridParam('postData');

    // Formのデータを結合
    getSendInputs(evt.data.form).serializeArray().forEach(function (data) {
        postData[data.name] = data.value;
    });

    // 検索
    grid.setGridParam({ datatype: 'json', postData: postData });
    grid.trigger('reloadGrid');
}
