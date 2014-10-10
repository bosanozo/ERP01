using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Web;

using NEXS.ERP.CM.Common;
using NEXS.ERP.CM.BL;

namespace NEXS.ERP.CM.Helper
{
    //************************************************************************
    /// <summary>
    /// JqGridの設定を生成するヘルパー
    /// </summary>
    //************************************************************************
    public static class JqGridHelper
    {
        /// <summary>
        /// 更新情報列名
        /// </summary>
        private static string[] updateCols =
        {
            "作成日時", "作成者ID", "作成者名", "作成者IP", "作成PG",
            "更新日時", "更新者ID", "更新者名", "更新者IP", "更新PG"
        };

        //************************************************************************
        /// <summary>
        /// 項目パラメータ
        /// </summary>
        //************************************************************************
        private class ColParam
        {
            /// <summary>項目名</summary>
            public string Name { get; set; }
            /// <summary>ID</summary>
            public string Id { get; set; }
            /// <summary>CSSクラス</summary>
            public string CssClass { get; set; }
            /// <summary>最大長</summary>
            public int MaxLen { get; set; }
            /// <summary>項目幅計算値</summary>
            public int Width { get; set; }
        }

        #region publicメソッド
        //************************************************************************
        /// <summary>
        /// メニュー
        /// </summary>
        /// <returns>メニュー</returns>
        //************************************************************************
        public static IHtmlString CreateMenu()
        {
            ICMCommonBL argCommonBL = new CMCommonBL();

            // インジェクション実行
            var injector = Seasar.Quill.QuillInjector.GetInstance();
            injector.Inject(argCommonBL);

            // タグリスト作成
            var taglist = new List<TagBuilder>();

            CMUserInfo uinfo = CMInformationManager.UserInfo;

            // メニューレベル１の検索
            DataTable table = argCommonBL.Select("CMSMメニューレベル1", uinfo.SoshikiCd, uinfo.Id);

            foreach (DataRow row in table.Rows)
            {
                // 操作不可のメニューは表示しない
                if (row["許否フラグ"].ToString() == "False") continue;

                // メニューレベル１作成
                var dropdown = new TagBuilder("li", "dropdown");
                var dropdownTgl = new TagBuilder 
                {
                    TagName = "a",
                    Text = row["画面名"] + " ",
                    CssClass = { "dropdown-toggle" },
                    Attributes = { { "href", "#" }, { "data-toggle", "dropdown" } }
                };
                
                dropdownTgl.Children.Add(new TagBuilder("span", "caret"));
                dropdown.Children.Add(dropdownTgl);

                var dropdownMenu = new TagBuilder("ul", "dropdown-menu");

                // メニューレベル２の検索
                DataTable tableLv2 = argCommonBL.Select("CMSMメニューレベル2", uinfo.SoshikiCd, uinfo.Id, row["メニューID"]);

                foreach (DataRow row2 in tableLv2.Rows)
                {
                    if (row2["許否フラグ"].ToString() == "False") continue;

                    // メニューレベル２作成
                    var menu = new TagBuilder("li");
                    menu.Children.Add(new TagBuilder
                        {
                            TagName = "a",
                            Text = row2["画面名"].ToString(),
                            Attributes = { { "href", row2["URL"].ToString().Replace("~", "") } }
                        });
                    dropdownMenu.Children.Add(menu);
                }

                dropdown.Children.Add(dropdownMenu);
                taglist.Add(dropdown);
            }

            // 文字列化
            var sb = new StringBuilder();
            taglist.ForEach(r => sb.AppendLine(r.ToString()));

            return new HtmlString(sb.ToString());
        }

        //************************************************************************
        /// <summary>
        /// 指定のXmlファイルからjqGridの列名配列を作成する。
        /// </summary>
        /// <param name="argDataSet">CM項目DataSet</param>
        /// <returns>jqGridの列名配列</returns>
        //************************************************************************
        public static IHtmlString GetColNames(CM項目DataSet argDataSet)
        {
            // StringBuilder作成
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("'状態', '操作',");

            // DataColumn追加
            foreach (var row in argDataSet.項目)
                sb.AppendFormat("'{0}', ", string.IsNullOrEmpty(row.ラベル) ? row.項目名 : row.ラベル);

            sb.AppendLine();
            foreach (var col in updateCols)
                sb.AppendFormat("'{0}', ", col);
            sb.Length -= 2;

            return new HtmlString(sb.ToString());
        }

        //************************************************************************
        /// <summary>
        /// 指定のXmlファイルからjqGridの列設定を作成する。
        /// </summary>
        /// <param name="argDataSet">CM項目DataSet</param>
        /// <param name="argCommonBL">共通処理ファサード</param>
        /// <returns>jqGridの列設定</returns>
        //************************************************************************
        public static IHtmlString GetColModel(CM項目DataSet argDataSet, ICMCommonBL argCommonBL)
        {
            // StringBuilder作成
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("{ name: '状態', align: 'center', frozen: true, formatter: statusFormatter, width: 40 },");
            sb.AppendLine("{ name: '操作', align: 'center', frozen: true, formatter: actionFormatter, width: 50 },");

            // DataColumn追加
            foreach (var row in argDataSet.項目)
            {
                ColParam colParam = GetColParams(row);

                // 項目名
                sb.AppendFormat("{{ name: '{0}', width: {1}, ", row.項目名, colParam.Width);

                // 型
                CMDbType dbType = (CMDbType)Enum.Parse(typeof(CMDbType), row.項目型);

                // align
                if (dbType == CMDbType.金額 || dbType == CMDbType.数値)
                    sb.Append("align: 'right', ");
                else if (dbType == CMDbType.フラグ || dbType == CMDbType.日付 || dbType == CMDbType.日時)
                    sb.Append("align: 'center', ");

                // キー
                if (row.主キー) sb.Append("frozen: true, ");

                // 非表示列
                if (!string.IsNullOrEmpty(row.非表示) && row.非表示.Contains('G'))
                    sb.Append("hidden: true, ");

                // 編集
                if (row.入力制限 != "不可")
                {
                    sb.Append("editable: true, editrules: { ");

                    // 必須入力
                    if (row.入力制限 == "必須") sb.Append("required: true, ");
                    sb.Append("}, ");

                    switch (dbType)
                    {
                        case CMDbType.区分:
                            sb.Append("edittype: 'select', formatter:'select', editoptions: { value:'");
                            int sbLen = sb.Length;
                            foreach (DataRow kbnRow in argCommonBL.SelectKbn(row.基準値分類CD).Rows)
                            {
                                if (sb.Length > sbLen) sb.Append(";");
                                sb.AppendFormat("{0}:{1}", kbnRow["基準値CD"], kbnRow["表示名"]);
                            }
                            sb.Append("'}");
                            break;
                        case CMDbType.フラグ:
                            sb.Append("edittype: 'checkbox', editoptions: { value:'true:false' } ");
                            break;
                        case CMDbType.日付:
                            sb.AppendLine();
                            sb.AppendLine("editoptions: { size: 12, maxlength: 10, " +
                                "dataInit: function (el) { $(el).datepicker({ dateFormat: 'yy/mm/dd' }); $(el).addClass('DateInput'); }}");
                            break;
                        default:
                            sb.AppendFormat("editoptions: {{ size: {0}, maxlength: {1}, " +
                                "dataInit: function (el) {{ $(el).addClass('{2}'); ", Math.Min(40, colParam.MaxLen), colParam.MaxLen, colParam.CssClass);
                            // 共通検索
                            if (!string.IsNullOrEmpty(row.共通検索ID))
                            {
                                sb.AppendFormat("$(el).change({{ selectId: '{0}', selectParam: \"{1}\", selectOut: '{2}' }}, GetCodeValue); ",
                                    row.共通検索ID, row.共通検索パラメータ, row.共通検索結果出力項目);
                            }
                            // 選択ボタン
                            if (row.選択ボタン)
                            {
                                sb.AppendFormat("addSelectButton($(el), {{ nameId: '{0}', selectId: '{1}', dbCodeCol: '{2}', dbNameCol: '{3}'}});",
                                    row.共通検索結果出力項目, row.共通検索ID2, row.コード値列名, row.名称列名);
                            }
                            sb.Append("} }");
                            break;
                    }
                }

                sb.AppendLine("},");
            }

            foreach (var col in updateCols)
            {
                sb.Append("{ ");
                sb.AppendFormat("name:'{0}'", col);
                if (col.EndsWith("日時")) sb.Append(", align: 'center', formatter : 'date', formatoptions:{ newformat: 'Y/m/d H:i:s' }, width: 110");
                else sb.Append(", width: 70");
                sb.AppendLine(" },");
            }
            sb.Length -= 3;

            return new HtmlString(sb.ToString());
        }

        //************************************************************************
        /// <summary>
        /// 指定のXmlファイルから詳細フォームのValidationRuleを作成する。
        /// </summary>
        /// <param name="argDataSet">CM項目DataSet</param>
        /// <returns>詳細フォームのValidationRule</returns>
        //************************************************************************
        public static IHtmlString GetValidationRules(CM項目DataSet argDataSet)
        {
            // StringBuilder作成
            StringBuilder sb = new StringBuilder();

            // DataColumn追加
            foreach (var row in argDataSet.項目)
            {
                // 非表示列は無視
                if (!string.IsNullOrEmpty(row.非表示) && row.非表示.Contains('F')) continue;

                StringBuilder rule = new StringBuilder();

                // 必須入力
                if (row.入力制限 == "必須") rule.Append("required: true, ");

                if (rule.Length > 0)
                    sb.Append(row.項目名).Append(": { ").Append(rule).AppendLine("},");
            }

            return new HtmlString(sb.ToString());
        }

        //************************************************************************
        /// <summary>
        /// 機能ボタンのタグを作成する。
        /// </summary>
        /// <param name="argNames">ボタン名</param>
        /// <returns>機能ボタンのタグ</returns>
        //************************************************************************
        public static IHtmlString CreateFuncButton(params string[] argNames)
        {
            return CreateFuncButton(null, 0, argNames);
        }

        //************************************************************************
        /// <summary>
        /// 機能ボタンのタグを作成する。
        /// </summary>
        /// <param name="argNames">ボタン名</param>
        /// <param name="argPrimaryPos">primaryボタンの位置(最初は1)</param>
        /// <returns>機能ボタンのタグ</returns>
        //************************************************************************
        public static IHtmlString CreateFuncButton(int argPrimaryPos, params string[] argNames)
        {
            return CreateFuncButton(null, argPrimaryPos, argNames);
        }

        //************************************************************************
        /// <summary>
        /// 機能ボタンのタグを作成する。
        /// </summary>
        /// <param name="argId">idはBtn + argId + xxxになる</param>
        /// <param name="argPrimaryPos">primaryボタンの位置(最初は1)</param>
        /// <param name="argNames">ボタン名</param>
        /// <returns>機能ボタンのタグ</returns>
        //************************************************************************
        public static IHtmlString CreateFuncButton(string argId, int argPrimaryPos, params string[] argNames)
        {
            // タグリスト作成
            var taglist = new List<TagBuilder>();

            int pos = 1;
            foreach(var n in argNames)
            {
                var btn = new TagBuilder("button", "FuncButton btn btn-sm");
                btn.Id = "Btn" + argId;
                btn.CssClass.Add(pos == argPrimaryPos ? "btn-primary" : "btn-default");

                var icon = new TagBuilder("sapn", "glyphicon");
                btn.Children.Add(icon);
                btn.Children.Add(new TagBuilder { TagName = "sapn", Text = n });

                switch (n)
                {
                    case "検索":
                        btn.Id += "Select";
                        icon.CssClass.Add("glyphicon-search");
                        break;
                    case "条件クリア":
                        btn.Id += "Clear";
                        icon.CssClass.Add("glyphicon-refresh");
                        break;
                    case "ＣＳＶ出力":
                        btn.Id += "CsvExport";
                        icon.CssClass.Add("glyphicon-export");
                        break;
                    case "XML出力":
                        btn.Id += "Xml";
                        icon.CssClass.Add("glyphicon-export");
                        break;
                    case "新規":
                        btn.Id += "Add";
                        icon.CssClass.Add("glyphicon-plus");
                        break;
                    case "修正":
                        btn.Id += "Edit";
                        btn.Attributes.Add("disabled", "disabled");
                        icon.CssClass.Add("glyphicon-edit");
                        break;
                    case "削除":
                        btn.Id += "Del";
                        btn.Attributes.Add("disabled", "disabled");
                        icon.CssClass.Add("glyphicon-trash");
                        break;
                    case "参照":
                        btn.Id += "View";
                        btn.Attributes.Add("disabled", "disabled");
                        icon.CssClass.Add("glyphicon-list-alt");
                        break;
                    case "登録":
                        btn.Id += "Commit";
                        icon.CssClass.Add("glyphicon-save");
                        break;
                    case "閉じる":
                        btn.Id += "Close";
                        icon.CssClass.Add("glyphicon-remove");
                        break;
                }

                taglist.Add(btn);
                pos++;
            }

            // 文字列化
            var sb = new StringBuilder();
            taglist.ForEach(r => sb.AppendLine(r.ToString()));

            return new HtmlString(sb.ToString());
        }

        //************************************************************************
        /// <summary>
        /// 指定のXmlファイルからフォームを作成する。
        /// </summary>
        /// <param name="argDataSet">CM項目DataSet</param>
        /// <param name="argCommonBL">共通処理ファサード</param>
        /// <param name="selectForm">検索条件フラグ</param>
        /// <returns>フォーム</returns>
        //************************************************************************
        public static IHtmlString CreateForm(CM項目DataSet argDataSet, ICMCommonBL argCommonBL,
            bool selectForm = false)
        {
            // タグリスト作成
            var taglist = new List<TagBuilder>();
            
            int colCnt = 0;
            TagBuilder formRow = null;
            var items = argDataSet.項目.Where(i => 
                string.IsNullOrEmpty(i.非表示) || !i.非表示.Contains('F'));

            // 入力欄作成ループ(非表示列は無視)
            foreach (var row in items)
            {
                // 改行フラグ
                bool nl = colCnt == 0 && (row.改行 || row == items.Last());

                if (colCnt == 0) formRow = new TagBuilder("div", "row");

                // ブロック作成
                var formCol = new TagBuilder("div", string.Format("col-sm-{0}", nl ? 12 : 6));
                var formGroup = new TagBuilder("div", "form-group");

                ColParam colParam = GetColParams(row);

                // 項目名
                var label = new TagBuilder
                {
                    TagName = "label",
                    Text = colParam.Name,
                    CssClass = { "control-label", string.Format("col-sm-{0}", nl ? 2 : 4) },
                    Attributes = { { "for", colParam.Id + row.項目名 + (row.FromTo ? "From" : null) } }
                };
                //if (colParam.CssClass == "TextInput") label.CssClass.Add("hidden-sm hidden-xs");
                formGroup.Children.Add(label);

                // 入力欄
                var inputGroup = new TagBuilder("div", "input-group");
                if (row.FromTo)
                {
                    inputGroup.Children.AddRange(CreateInput("From", colParam, row, argCommonBL, selectForm));
                    inputGroup.Children.Add(new TagBuilder { TagName = "sapn", Text = " ～ " });
                    inputGroup.Children.AddRange(CreateInput("To", colParam, row, argCommonBL, selectForm));
                }
                else inputGroup.Children.AddRange(CreateInput(null, colParam, row, argCommonBL, selectForm));

                // ブロック追加
                formGroup.Children.Add(inputGroup);
                formCol.Children.Add(formGroup);
                formRow.Children.Add(formCol);

                // 改行判定
                if (colCnt == 1 || nl)
                {
                    colCnt = 0;
                    taglist.Add(formRow);
                }
                else colCnt++;
            }

            // 文字列化
            var sb = new StringBuilder();
            taglist.ForEach(r => sb.AppendLine(r.ToString()));

            return new HtmlString(sb.ToString());
        }

        //************************************************************************
        /// <summary>
        /// 指定のXmlファイルからフォームを作成する。
        /// </summary>
        /// <param name="argDataSet">CM項目DataSet</param>
        /// <param name="argCommonBL">共通処理ファサード</param>
        /// <param name="selectForm">検索条件フラグ</param>
        /// <returns>フォーム</returns>
        //************************************************************************
        public static IHtmlString CreateFormFixed(CM項目DataSet argDataSet, ICMCommonBL argCommonBL,
            bool selectForm = false)
        {
            // タグリスト作成
            var taglist = new List<TagBuilder>();

            int colCnt = 0;
            TagBuilder formRow = null;
            var items = argDataSet.項目.Where(i =>
                string.IsNullOrEmpty(i.非表示) || !i.非表示.Contains('F'));

            // 入力欄作成ループ(非表示列は無視)
            foreach (var row in items)
            {
                // 改行フラグ
                bool nl = colCnt == 0 && (row.改行 || row == items.Last());

                if (colCnt == 0) formRow = new TagBuilder("tr");

                ColParam colParam = GetColParams(row);

                // 項目名
                var labelTd = new TagBuilder("td");
                labelTd.Children.Add(new TagBuilder
                {
                    TagName = "label",
                    Text = colParam.Name,
                    CssClass = { "control-label" },
                    Attributes = { { "for", colParam.Id + row.項目名 + (row.FromTo ? "From" : null) } }
                });
                formRow.Children.Add(labelTd);

                // 入力欄
                var inputGroup = new TagBuilder("td");
                if (row.FromTo)
                {
                    inputGroup.Children.AddRange(CreateInput("From", colParam, row, argCommonBL, selectForm));
                    inputGroup.Children.Add(new TagBuilder { TagName = "sapn", Text = " ～ " });
                    inputGroup.Children.AddRange(CreateInput("To", colParam, row, argCommonBL, selectForm));
                }
                else inputGroup.Children.AddRange(CreateInput(null, colParam, row, argCommonBL, selectForm));

                formRow.Children.Add(inputGroup);

                // 改行判定
                if (colCnt == 1 || nl)
                {
                    colCnt = 0;
                    taglist.Add(formRow);
                }
                else colCnt++;
            }

            // 文字列化
            var sb = new StringBuilder();
            taglist.ForEach(r => sb.AppendLine(r.ToString()));

            return new HtmlString(sb.ToString());
        }
        #endregion

        #region privateメソッド
        //************************************************************************
        /// <summary>
        /// 項目パラメータを返す。
        /// </summary>
        /// <param name="argRow">項目Row</param>
        /// <returns>項目パラメータ</returns>
        //************************************************************************
        private static ColParam GetColParams(CM項目DataSet.項目Row argRow)
        {
            // 返却値生成
            var colParam = new ColParam
            {
                Name = string.IsNullOrEmpty(argRow.ラベル) ? argRow.項目名 : argRow.ラベル,
                MaxLen = argRow.長さ,
                Width = 0
            };

            // 型
            CMDbType dbType = (CMDbType)Enum.Parse(typeof(CMDbType), argRow.項目型);

            switch (dbType)
            {
                case CMDbType.区分:
                    colParam.Id = "Ddl";
                    break;
                case CMDbType.フラグ:
                    colParam.Id = "Chk";
                    break;
                case CMDbType.コード:
                case CMDbType.コード_可変:
                    colParam.Id = "Cd";
                    colParam.CssClass = "CodeInput";
                    colParam.Width = colParam.MaxLen * 8 + 4;
                    break;
                case CMDbType.文字列:
                    colParam.Id = "Txt";
                    colParam.CssClass = "TextInput";
                    colParam.Width = Math.Min(colParam.MaxLen * 16 + 4, 120);
                    break;
                case CMDbType.金額:
                case CMDbType.数値:
                    colParam.Id = "Num";
                    colParam.CssClass = "NumberInput";
                    colParam.MaxLen += (colParam.MaxLen - 1) / 3 + argRow.小数桁 > 0 ? argRow.小数桁 + 1 : 0;
                    colParam.Width = colParam.MaxLen * 8 + 4;
                    break;
                case CMDbType.日付:
                    colParam.Id = "Ymd";
                    colParam.CssClass = "DateInput";
                    colParam.MaxLen = 10;
                    colParam.Width = 70;
                    break;
                case CMDbType.日時:
                    colParam.Id = "Dtm";
                    colParam.CssClass = "DateInput";
                    colParam.MaxLen = 19;
                    colParam.Width = 110;
                    break;
            }

            // ピクセル値計算
            colParam.Width = Math.Max(colParam.Width, System.Windows.Forms.TextRenderer.MeasureText(colParam.Name, new System.Drawing.Font("MS UI Gothic", 9)).Width);

            return colParam;
        }

        //************************************************************************
        /// <summary>
        /// 入力欄の要素を作成する。
        /// </summary>
        /// <param name="argFromTo">FromTo</param>
        /// <param name="colParam">項目パラメータ</param>
        /// <param name="argCommonBL">共通処理ファサード</param>
        /// <param name="row">項目Row</param>
        /// <param name="selectForm">検索条件フラグ</param>
        /// <returns>入力欄の要素</returns>
        //************************************************************************
        private static List<TagBuilder> CreateInput(string argFromTo, ColParam colParam,
            CM項目DataSet.項目Row row, ICMCommonBL argCommonBL, bool selectForm)
        {
            var taglist = new List<TagBuilder>();

            // name作成
            string name = row.項目名 + argFromTo;

            // 入力要素を作成
            var input = new TagBuilder
            {
                Id = colParam.Id + name,
                Name = name,
                CssClass = { "form-control-custom" }
            };

            if (row.主キー) input.Attributes.Add("key", "true");

            // 項目型
            switch ((CMDbType)Enum.Parse(typeof(CMDbType), row.項目型))
            {
                case CMDbType.区分:
                    input.TagName = "select";
                    // optionを追加
                    if (selectForm) input.Children.Add(new TagBuilder { TagName = "option", Value = "" });
                    foreach (DataRow kbnRow in argCommonBL.SelectKbn(row.基準値分類CD).Rows)
                        input.Children.Add(new TagBuilder { TagName = "option", Value = kbnRow["基準値CD"],
                                Text = kbnRow["表示名"].ToString() });
                    break;

                case CMDbType.フラグ:
                    input.TagName = "input";
                    input.Type = "checkbox";
                    input.Value = "true";
                    break;

                default:
                    if (colParam.MaxLen < 50)
                    {
                        input.TagName = "input";
                        input.Type = "text";
                        input.Attributes.Add("maxlength", colParam.MaxLen);
                        input.Attributes.Add("size", colParam.MaxLen);
                    }
                    else
                    {
                        input.TagName = "textarea";
                        input.Attributes.Add("maxlength", colParam.MaxLen);
                        input.Attributes.Add("cols", 50);
                        input.Attributes.Add("rows", Math.Min(colParam.MaxLen / 50, 3));
                    }

                    input.CssClass.Add(colParam.CssClass);

                    if (colParam.CssClass == "TextInput") input.Attributes.Add("placeholder", colParam.Name);

                    if (row.入力制限 == "不可") input.Attributes.Add("readonly", "readonly"); 
                    else
                    {
                        // 共通検索
                        if (!string.IsNullOrEmpty(row.共通検索ID))
                            input.Attributes.Add("changeParam", string.Format(
                                "{{ selectId: '{0}', selectParam: &quot;{1}&quot;, selectOut: '{2}_{3}' }}",
                                row.共通検索ID, row.共通検索パラメータ, name, row.共通検索結果出力項目));                            
                    }
                    break;
            }

            taglist.Add(input);

            // 選択ボタン
            if (row.選択ボタン)
            {
                // ボタン要素を作成
                var btn = new TagBuilder
                {
                    TagName = "button",
                    Id = "Btn" + name,
                    Name = name,
                    Type = "button",
                    CssClass = { "SelectButton", "btn", "btn-default" },
                    Attributes = {  
                        { "clickParam", string.Format(
                            "{{ codeId: '{0}{1}', nameId: '{1}_{2}', selectId: '{3}', dbCodeCol: '{4}', dbNameCol: '{5}' }}",
                            colParam.Id, name, row.共通検索結果出力項目, row.共通検索ID2, row.コード値列名, row.名称列名) }
                    }
                };

                // 子要素を追加
                btn.Children.Add(new TagBuilder { TagName = "span", CssClass = { "glyphicon", "glyphicon-search" } });

                taglist.Add(btn);
            }

            // 共通検索結果出力項目
            if (!string.IsNullOrEmpty(row.共通検索結果出力項目))
            {
                // テキスト表示要素を作成
                taglist.Add(new TagBuilder
                {
                    TagName = "input",
                    Id = name + "_" + row.共通検索結果出力項目,
                    Name = row.共通検索結果出力項目,
                    Type = "text",
                    CssClass = { "TextInput", "form-control-custom" },
                    Attributes = {  
                        { "readonly", "readonly" },
                        { "size", 30 }
                    }
                });
            }

            return taglist;
        }
        #endregion
    }
}
