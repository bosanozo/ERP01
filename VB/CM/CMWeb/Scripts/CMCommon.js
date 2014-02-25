// 検索子画面表示
function ShowSelectSub(argButton, argCd, argNm, argParam) {
    argParam += "&Code=" + escape(argCd.id) + "&CodeLen=" + argCd.maxLength;
    var result = window.showModalDialog("../CM/CMSubForm.aspx?" + argParam, "", "help:no");

    if (result != null) {
        argCd.value = result[0];
        if (argNm != null) {
            argNm.innerText = result[1];
            //argNm.className = "1 transp";
        }
        FocusNext(argButton);
    }
}

// 組織検索画面表示
function ShowSelectSoshikiCd(argButton, argCd, argNm) {
    var param = "SelectId=" + escape("CS組織") +
        "&DbCodeCol=" + escape("組織CD") +
        "&DbNameCol=" + escape("組織名");

    ShowSelectSub(argButton, argCd, argNm, param);
}

// 上位組織検索画面表示
function ShowSelectJyouiSoshikiCd(argButton, argCd, argNm, argSoshikiKaisoKbn) {
    var param = "SelectId=" + escape("CS上位組織") +
        "&DbCodeCol=" + escape("組織CD") +
        "&DbNameCol=" + escape("組織名") +
        "&Params=" + argSoshikiKaisoKbn.value;
        //"&Params=" + escape("#SoshikiKaisoKbn #KaishaCd");

    ShowSelectSub(argButton, argCd, argNm, param);
}

// カレンダー表示
function ShowCalendar(argButton, argText) {
    var result = window.showModalDialog("../CM/CMCalendar.aspx?value=" + argText.value, "", "dialogheight:240px;dialogwidth:230px;help:no");
    if (result != null) {
        argText.value = result;
        FocusNext(argButton);
    }
}

// 次項目フォーカス移動
function FocusNext(argObj) {
    var i;
    var nextObj;
    var found = false;

    for (i = 0; i < Form1.elements.length; i++) {
        nextObj = Form1.elements[i];
        if (nextObj == argObj) found = true;

        if (found && nextObj.type != "button" && nextObj.type != "reset" &&
			nextObj.type != "submit" && nextObj.type != "hidden" &&
			(nextObj.type == "text" && nextObj.readOnly == false)) break;
    }

    nextObj.focus();
}

// 現在日付取得
function GetCurDate() {
    date = new Date();
    y = date.getFullYear();
    m = date.getMonth() + 1;
    d = date.getDate();

    if (m < 10) m = "0" + m;
    if (d < 10) d = "0" + d;

    return y + "/" + m + "/" + d;
}

var wait_opacity;

// 処理中メッセージを表示する
function ShowWaitMessage() {
    var shim = document.getElementById("shim");
    if (shim != null) {
        shim.style.position = "absolute";
        shim.style.top = "0px"
        shim.style.left = "0px"
        shim.style.width = "1024px";
        shim.style.height = "100%";
        shim.style.display = "block";
        shim.style.filter = "alpha(opacity=0)";
        shim.zindex = 9;
    }

    wait.innerHTML = "処理中です・・・　　結果が出るまでお待ちください。";
    wait.className = "pleasewait";
    wait.style.display = "block";
    wait.style.filter = "alpha(opacity=0)";
    wait.zindex = 10;
    wait_opacity = 50;
    setTimeout('AddOpacity()', 300);
}

// だんだん透明度を下げる
function AddOpacity() {
    if (wait_opacity < 100) {
        wait.style.filter = "alpha(opacity=" + wait_opacity + ")";
        wait_opacity += 5;
        setTimeout('AddOpacity()', 30);
    }
}

//クリアボタン押下
function ClearCondition() {
    // クリア確認
    if (!confirm("検索条件をクリアします。よろしいですか？")) return false;

    // クリアループ
    var i;
    for (i = 0; i < Form1.length; i++) {
        var element = Form1[i];

        switch (element.type) {
            case "text":
                // 会社ＣＤの場合
                if ((element.id == "会社ＣＤFrom" || element.id == "会社ＣＤTo") &&
					Form1.SoshikiLayer.value != "1") element.value = Form1.KaishaCd.value;
                else element.value = "";
                break;
            case "select-one":
                element.selectedIndex = 0;
                break;
        }
    }
}

// 選択された行番号
var rowIdx = -1;

// 選択された行番号を設定する
function SetSelectedIndex() {
    var sheet = GridView1;
    var checkCnt = 0;
    var firstIdx = -1;

    for (i = 1; i < sheet.rows.length; i++) {
        if (sheet.rows[i].cells[0].children.Checkbox.checked) {
            if (firstIdx == -1) firstIdx = i;
            checkCnt++;
        }
    }

    var disable = checkCnt != 1;

    // 選択された行番号を設定
    if (disable) rowIdx = -1;
    else rowIdx = firstIdx;

    // 機能ボタンの状態を変更
    if (Form1.BtnDetail != null) Form1.BtnDetail.disabled = disable;

    // 編集不可の場合は、修正、削除ボタンの状態を変更しない
    if (Form1.BtnInsert != null && Form1.BtnInsert.disabled == true) return;

    if (Form1.BtnUpdate != null) Form1.BtnUpdate.disabled = disable;
    if (Form1.BtnDelete != null) Form1.BtnDelete.disabled = disable;
}

// 登録画面を表示する
function OpenEntryForm(argMode) {
    var sheet = GridView1;
    var keys = "";

    if (sheet != null && rowIdx >= 0) keys = GetKeys(sheet.rows[rowIdx]);

    var result = window.showModalDialog(Form1.EntryForm.value + ".aspx?mode=" +
		argMode + "&keys=" + keys, "", "dialogheight:724px;dialogwidth:1024px;help:no");

    if (result != null) return result;
    else return false;
}

// 必須入力チェック
function CheckNull(argInput, argName) {
    if (argInput.value.length > 0) return false;

    alert(argName + "を入力してください。");
    argInput.focus();
    return true;
}

// 時分入力チェック
function CheckTime(argHour, argMin, argName) {
    if (argHour.value.length > 0 && argMin.value.length > 0) return false;

    if (argHour.value.length > 0) {
        alert(argName + "（分）を入力してください。");
        argMin.focus();
        return true;
    }

    if (argMin.value.length > 0) {
        alert(argName + "（時）を入力してください。");
        argHour.focus();
        return true;
    }

    return false;
}

// バイト数チェック
function CheckLength(argInput, argName) {
    var len = argInput.value.length;
    if (len == 0) return false;

    var bytes = 0;
    for (i = 0; i < len; i++) {
        var c = argInput.value.charCodeAt(i);
        // 半角の場合
        if (c < 256 || (c >= 0xff61 && c < 0xffa0)) bytes++;
        // 半角以外の場合
        else if (c < 0xffff) bytes += 2;
        else {
            alert(argName + "に使用できない文字があります。'" + argInput.value.charAt(i) + "'");
            argInput.focus();
            return true;
        }
    }

    var maxLen = argInput.maxLength;
    if (bytes > maxLen) {
        alert(argName + "は" + maxLen + "バイト以内で入力してください。\n(現在 " + bytes + "バイト)");
        argInput.focus();
        return true;
    }
}

// 名称チェック
function CheckName(argInput, argName) {
    var len = argInput.value.length;
    if (len == 0) return false;

    var bytes = 0;
    for (i = 0; i < len; i++) {
        var c = argInput.value.charCodeAt(i);
        // 半角の場合
        if (c < 256 || (c >= 0xff61 && c < 0xffa0)) {
            alert(argName + "は全角文字を入力してください。");
            argInput.focus();
            return true;
        }
        // 範囲外の場合
        else if (c >= 0xffff) {
            alert(argName + "に使用できない文字があります。'" + argInput.value.charAt(i) + "'");
            argInput.focus();
            return true;
        }
    }
}

// 数値チェック
function CheckNumber(argInput, argName) {
    var len = argInput.value.length;

    if (len == 0) return false;

    if (!argInput.value.match(/^\d+$/)) {
        alert(argName + "は半角数値を入力してください。");
        argInput.focus();
        return true;
    }
}

// コード値チェック
function CheckCode(argInput, argName) {
    var len = argInput.value.length;

    if (len == 0) return false;

    // 数値チェック
    if (CheckNumber(argInput, argName)) return true;

    // 長さチェック
    if (len != argInput.maxLength) {
        alert(argName + "は" + argInput.maxLength + "桁で入力してください。");
        argInput.focus();
        return true;
    }
}

// 半角英数チェック
function CheckAN(argInput, argName) {
    var len = argInput.value.length;

    if (len == 0) return false;

    // 半角英数チェック
    if (!argInput.value.match(/^\w+$/)) {
        alert(argName + "は半角英数字を入力してください。");
        argInput.focus();
        return true;
    }
}

// 日付形式チェック
function CheckDate(argInput, argName) {
    var len = argInput.value.length;

    if (len == 0) return false;

    var sts = false;
    var msg;

    // フォーマットチェック
    if (!argInput.value.match(/^\d{4}\/\d{2}\/\d{2}$/)) {
        msg = "日付が正しくありません。\nyyyy/mm/dd形式で指定してください。";
        sts = true;
    } else {
        var year = argInput.value.substr(0, 4) - 0;
        var month = argInput.value.substr(5, 2) - 1;
        var day = argInput.value.substr(8, 4) - 0;

        if (year == 0) {
            msg = "年に0000を入力することはできません。";
            sts = true;
        }
        else if (month < 0 || month > 11) {
            msg = "月は01～12の数字を入力してください。";
            sts = true;
        }
        else if (day < 1 || day > 31) {
            msg = "日は01～31の数字を入力してください。"
            sts = true;
        } else {
            var d = new Date(year, month, day);

            if (d.getFullYear() != year || d.getMonth() != month || d.getDate() != day) {
                msg = "日付は存在しません。"
                sts = true;
            }
        }
    }

    if (sts) {
        alert(argName + "の" + msg);
        argInput.focus();
    }

    return sts;
}

// From-Toチェック
function CheckFromTo(argInputFrom, argInputTo, argName) {
    if (CheckCode(argInputFrom, argName)) return true;
    if (CheckCode(argInputTo, argName)) return true;

    if (argInputFrom.value.length > 0 && argInputTo.value.length > 0 &&
		argInputFrom.value > argInputTo.value) {
        alert(argName + "(From)は、" + argName + "(To)より前の値を入力してください。");
        argInputFrom.focus();
        return true;
    }
}

// From-Toチェック(半角英数)
function CheckFromToAN(argInputFrom, argInputTo, argName) {
    if (CheckAN(argInputFrom, argName)) return true;
    if (CheckAN(argInputTo, argName)) return true;

    if (argInputFrom.value.length > 0 && argInputTo.value.length > 0 &&
		argInputFrom.value > argInputTo.value) {
        alert(argName + "(From)は、" + argName + "(To)より前の値を入力してください。");
        argInputFrom.focus();
        return true;
    }
}

// 日付From-Toチェック
function CheckDateFromTo(argInputFrom, argInputTo, argName) {
    if (CheckDate(argInputFrom, argName)) return true;
    if (CheckDate(argInputTo, argName)) return true;

    if (argInputFrom.value.length > 0 && argInputTo.value.length > 0 &&
		argInputFrom.value > argInputTo.value) {
        alert(argName + "(From)は、" + argName + "(To)より前の日付を入力してください。");
        argInputFrom.focus();
        return true;
    }
}

// Trim
String.prototype.Trim =
function () {
    return this.replace(/^\s+|\s+$/g, "");
}
