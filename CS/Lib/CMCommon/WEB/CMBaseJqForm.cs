using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.SessionState;
using System.Web.Script.Serialization;
using System.Web.UI;
using System.Web.UI.WebControls;

using NEXS.ERP.CM.Common;
using NEXS.ERP.CM.BL;
using NEXS.ERP.CM.DA;

namespace NEXS.ERP.CM.WEB
{
    //************************************************************************
    /// <summary>
    /// Jquery��ʂ̊��N���X
    /// </summary>
    //************************************************************************
    public class CMBaseJqForm : CMBaseForm
    {
        private Dictionary<string, CM����DataSet> m_formDsDic = new Dictionary<string, CM����DataSet>();

        #region �R���X�g���N�^
        //************************************************************************
        /// <summary>
        /// �R���X�g���N�^
        /// </summary>
        //************************************************************************
        public CMBaseJqForm()
        {
        }
        #endregion

        #region protected���\�b�h
        #region jqGrid
        //************************************************************************
        /// <summary>
        /// �w���Xml�t�@�C������jqGrid�̗񖼔z����쐬����B
        /// </summary>
        /// <param name="argName">Xml�t�@�C����</param>
        /// <returns>jqGrid�̗񖼔z��</returns>
        //************************************************************************
        protected string GetColNames(string argName)
        {
            // �f�[�^�Z�b�g���擾
            CM����DataSet ds = GetFormDataSet(argName);

            // StringBuilder�쐬
            StringBuilder sb = new StringBuilder();

            // DataColumn�ǉ�
            foreach (var row in ds.����)
                sb.AppendFormat("'{0}', ", string.IsNullOrEmpty(row.���x��) ? row.���ږ� : row.���x��);

            string[] updateCols = 
                {
                    "�쐬����", "�쐬��ID", "�쐬�Җ�", "�쐬��IP", "�쐬PG",
                    "�X�V����", "�X�V��ID", "�X�V�Җ�", "�X�V��IP", "�X�VPG"
                };

            sb.AppendLine();
            foreach (var col in updateCols)
                sb.AppendFormat("'{0}', ", col);
            sb.Length -= 2;

            return sb.ToString();
        }

        //************************************************************************
        /// <summary>
        /// �w���Xml�t�@�C������jqGrid�̗�ݒ���쐬����B
        /// </summary>
        /// <param name="argName">Xml�t�@�C����</param>
        /// <returns>jqGrid�̗�ݒ�</returns>
        //************************************************************************
        protected string GetColModel(string argName)
        {
            // �f�[�^�Z�b�g���擾
            CM����DataSet ds = GetFormDataSet(argName);

            // StringBuilder�쐬
            StringBuilder sb = new StringBuilder();

            // DataColumn�ǉ�
            foreach (var row in ds.����)
            {
                string cssClass;
                int maxLen;
                int width;
                GetColParams(row, out cssClass, out maxLen, out width);

                // ���ږ�
                sb.AppendFormat("{{ name: '{0}', width: {1}, ", row.���ږ�, width);

                // �^
                CMDbType dbType = (CMDbType)Enum.Parse(typeof(CMDbType), row.���ڌ^);

                // align
                if (dbType == CMDbType.���z || dbType == CMDbType.���l)
                    sb.Append("align: 'right', ");
                else if (dbType == CMDbType.�t���O || dbType == CMDbType.���t || dbType == CMDbType.����)
                    sb.Append("align: 'center', ");

                // �L�[
                if (row.��L�[) sb.Append("frozen: true, ");

                // ��\����
                if (!string.IsNullOrEmpty(row.��\��) && row.��\��.Contains('G'))
                    sb.Append("hidden: true, ");

                // �ҏW
                if (row.���͐��� != "�s��")
                {
                    sb.Append("editable: true, editrules: { ");

                    // �K�{����
                    if (row.���͐��� == "�K�{") sb.Append("required: true, ");
                    sb.Append("}, ");

                    switch (dbType)
                    {
                        case CMDbType.�敪:
                            sb.Append("edittype: 'select', formatter:'select', editoptions: { value:'");
                            int sbLen = sb.Length;
                            foreach (DataRow kbnRow in CommonBL.SelectKbn(row.��l����CD).Rows)
                            {
                                if (sb.Length > sbLen) sb.Append(";");
                                sb.AppendFormat("{0}:{1}", kbnRow["��lCD"], kbnRow["�\����"]);
                            }
                            sb.Append("'}");
                            break;
                        case CMDbType.�t���O:
                            sb.Append("edittype: 'checkbox', editoptions: { value:'true:false' } ");
                            break;
                        case CMDbType.���t:
                            sb.AppendLine();
                            sb.AppendLine("editoptions: { size: 12, maxlength: 10, " +
                                "dataInit: function (el) { $(el).datepicker({ dateFormat: 'yy/mm/dd' }); $(el).addClass('DateInput'); }}");
                            break;
                        default:
                            sb.AppendFormat("editoptions: {{ size: {0}, maxlength: {1}, " +
                                "dataInit: function (el) {{ $(el).addClass('{2}'); ", Math.Min(40, maxLen), maxLen, cssClass);
                            // ���ʌ���
                            if (!string.IsNullOrEmpty(row.���ʌ���ID))
                            {
                                sb.AppendFormat("$(el).change({{ selectId: '{0}', selectParam: \"{1}\", selectOut: '{2}' }}, GetCodeValue); ",
                                    row.���ʌ���ID, row.���ʌ����p�����[�^, row.���ʌ������ʏo�͍���);
                            }
                            // �I���{�^��
                            if (row.�I���{�^��)
                            {
                                sb.AppendFormat("addSelectButton($(el), {{ nameId: '{0}', selectId: '{1}', dbCodeCol: '{2}', dbNameCol: '{3}'}});",
                                    row.���ʌ������ʏo�͍���, row.���ʌ���ID2, row.�R�[�h�l��, row.���̗�);
                            }
                            sb.Append("} }");
                            break;
                    }
                }

                sb.AppendLine("},");
            }

            string[] updateCols = 
                {
                    "�쐬����", "�쐬��ID", "�쐬�Җ�", "�쐬��IP", "�쐬PG",
                    "�X�V����", "�X�V��ID", "�X�V�Җ�", "�X�V��IP", "�X�VPG"
                };

            foreach (var col in updateCols)
            {
                sb.Append("{ ");
                sb.AppendFormat("name:'{0}'", col);
                if (col.EndsWith("����")) sb.Append(", align: 'center', formatter : 'date', formatoptions:{ newformat: 'Y/m/d H:i:s' }, width: 110");
                else sb.Append(", width: 70");
                sb.AppendLine(" },");
            }
            sb.Length -= 3;

            return sb.ToString();
        }
        #endregion

        #region �t�H�[��
        //************************************************************************
        /// <summary>
        /// ���͗��̗v�f���쐬����B
        /// </summary>
        /// <param name="col">�v�f�ɐݒ肷��id</param>
        /// <param name="cssClass">�v�f��class</param>
        /// <param name="maxLen">�ő咷</param>
        /// <param name="row">����Row</param>
        /// <param name="selectForm">���������t���O</param>
        /// <returns>���͗��̗v�f</returns>
        //************************************************************************
        protected string CreateInput(string col, string cssClass, int maxLen, CM����DataSet.����Row row, bool selectForm)
        {
            StringBuilder sb = new StringBuilder();

            // class=\"ui-widget-content ui-corner-all\" 

            // ���ڌ^
            switch ((CMDbType)Enum.Parse(typeof(CMDbType), row.���ڌ^))
            {
                case CMDbType.�敪:
                    sb.AppendFormat("<select id=\"Ddl{0}\" name=\"{0}\"", col);
                    if (row.��L�[) sb.Append(" key=\"true\"");
                    sb.Append(">");
                    // option
                    if (selectForm) sb.Append("<option value=\"\"></option>");
                    foreach (DataRow kbnRow in CommonBL.SelectKbn(row.��l����CD).Rows)
                        sb.AppendFormat("<option value=\"{0}\">{1}</option>", kbnRow["��lCD"], kbnRow["�\����"]);
                    sb.Append("</select>");
                    break;

                case CMDbType.�t���O:
                    sb.AppendFormat("<input id=\"Chk{0}\" name=\"{0}\" type=\"checkbox\" value=\"true\" />", col);
                    break;

                default:
                    string format = maxLen < 50 ?
                        "<input id=\"Txt{0}\" name=\"{0}\" class=\"{1}\" type=\"text\"" :
                        "<textarea id=\"Txa{0}\" name=\"{0}\" class=\"{1}\"";
                    sb.AppendFormat(format, col, cssClass);
                    if (row.��L�[) sb.Append(" key=\"true\"");
                    if (row.���͐��� == "�s��")
                    {
                        sb.Append(" readonly=\"readonly\"/>");
                        break;
                    }
                    else
                    {
                        // ���ʌ���
                        if (!string.IsNullOrEmpty(row.���ʌ���ID))
                        {
                            sb.AppendFormat(" changeParam =\"{{ selectId: '{0}', selectParam: &quot;{1}&quot;, selectOut: '{2}_{3}' }}\"",
                                row.���ʌ���ID, row.���ʌ����p�����[�^, col, row.���ʌ������ʏo�͍���);
                        }
                    }

                    if (maxLen < 50)
                    {
                        sb.AppendFormat(" maxlength=\"{0}\" size=\"{0}\"", maxLen);
                        sb.Append("/>");
                    }
                    else sb.AppendFormat(" maxlength=\"{0}\" cols=\"50\" rows=\"{1}\"></textarea>", maxLen, Math.Min(maxLen / 50, 3));
                    break;
            }

            // �I���{�^��
            if (row.�I���{�^��)
            {
                sb.AppendLine();
                sb.AppendFormat("<input id=\"Btn{0}\" class=\"SelectButton\" type=\"button\" value=\"...\"", col);
                sb.AppendFormat(" clickParam = \"{{ codeId: 'Txt{0}', nameId: '{0}_{1}', selectId: '{2}', dbCodeCol: '{3}', dbNameCol: '{4}' }}\" />",
                    col, row.���ʌ������ʏo�͍���, row.���ʌ���ID2, row.�R�[�h�l��, row.���̗�
                );
            }

            // ���ʌ������ʏo�͍���
            if (!string.IsNullOrEmpty(row.���ʌ������ʏo�͍���))
            {
                sb.AppendFormat("<input id=\"{0}_{1}\" name=\"{1}\" class=\"TextInput\" type=\"text\" readonly=\"readonly\" size=\"{2}\"/>",
                    col, row.���ʌ������ʏo�͍���, 30);
            }

            return sb.ToString();
        }

        //************************************************************************
        /// <summary>
        /// �w���Xml�t�@�C������t�H�[�����쐬����B
        /// </summary>
        /// <param name="argName">Xml�t�@�C����</param>
        /// <param name="selectForm">���������t���O</param>
        /// <returns>�t�H�[��</returns>
        //************************************************************************
        protected string CreateForm(string argName, bool selectForm = false)
        {
            // �f�[�^�Z�b�g���擾
            CM����DataSet ds = GetFormDataSet(argName);

            // StringBuilder�쐬
            StringBuilder sb = new StringBuilder();

            int colCnt = 0;

            // ���͗��쐬���[�v
            foreach (var row in ds.����)
            {
                // ��\����͖���
                if (!string.IsNullOrEmpty(row.��\��) && row.��\��.Contains('F')) continue;

                if (colCnt == 0) sb.Append("<tr>");

                string cssClass;
                int maxLen;
                int width;

                string col = GetColParams(row, out cssClass, out maxLen, out width);

                // ���ږ�
                sb.AppendFormat("<td class=\"ItemName\">{0}</td><td class=\"ItemPanel\">", col);

                // ���͗�
                if (row.FromTo)
                {
                    sb.Append(CreateInput(row.���ږ� + "From", cssClass, maxLen, row, selectForm));
                    sb.Append(" �` ");
                    sb.Append(CreateInput(row.���ږ� + "To", cssClass, maxLen, row, selectForm));
                }
                else sb.Append(CreateInput(row.���ږ�, cssClass, maxLen, row, selectForm));

                sb.Append("</td>");

                // ���s����
                if (colCnt == 1 || row.���s)
                {
                    sb.AppendLine("</tr>");
                    colCnt = 0;
                }
                else
                {
                    sb.AppendLine();
                    colCnt++;
                }
            }

            return sb.ToString();
        }

#if ASP_Form
    //************************************************************************
    /// <summary>
    /// ���͗��̗v�f���쐬����B
    /// </summary>
    /// <param name="cell">�v�f��ǉ�����e�[�u���̃Z��</param>
    /// <param name="col">�v�f�ɐݒ肷��id</param>
    /// <param name="cssClass">�v�f��class</param>
    /// <param name="maxLen">�ő咷</param>
    /// <param name="row">����Row</param>
    /// <param name="selectForm">���������t���O</param>
    /// <returns>���͗��̗v�f</returns>
    //************************************************************************
    protected void CreateInput(TableCell cell, string col, string cssClass, int maxLen, CM����DataSet.����Row row, bool selectForm)
    {
        // ���ڌ^
        switch ((CMDbType)Enum.Parse(typeof(CMDbType), row.���ڌ^))
        {
            case CMDbType.�敪:
                DropDownList ddl = new DropDownList();
                ddl.ID = "Ddl" + col;
                if (row.Key) ddl.Attributes["key"] = "true";
                // option
                DataTable kbnTable = CommonBL.SelectKbn(row.��l����CD);
                if (selectForm) kbnTable.Rows.InsertAt(kbnTable.NewRow(), 0);
                ddl.DataSource = kbnTable;
                ddl.DataTextField = "�\����";
                ddl.DataValueField = "��lCD";
                ddl.DataBind();
                cell.Controls.Add(ddl);
                break;

            case CMDbType.�t���O:
                CheckBox chk = new CheckBox();
                chk.ID = "Chk" + col;
                cell.Controls.Add(chk);
                break;

            default:
                TextBox txt = new TextBox();
                txt.ID = "Txt" + col;
                txt.CssClass = cssClass;
                if (row.Key) txt.Attributes["key"] = "true";
                if (row.���͐��� == "�s��")
                {
                    txt.ReadOnly = true;;
                    cell.Controls.Add(txt);
                    break;
                }
                else
                {
                    // ���ʌ���
                    if (!string.IsNullOrEmpty(row.���ʌ���ID))
                    {
                        txt.Attributes["changeParam"] =
                            string.Format("{{ selectId: '{0}', selectParam: \"{1}\", selectOut: '{2}_{3}' }}",
                            row.���ʌ���ID, row.���ʌ����p�����[�^, col, row.���ʌ������ʏo�͍���);
                    }
                }

                txt.MaxLength = maxLen;
                txt.Attributes["size"] = maxLen.ToString();
                cell.Controls.Add(txt);
                break;
        }

        // �I���{�^��
        if (row.�I���{�^��)
        {
            var btn = new System.Web.UI.HtmlControls.HtmlButton();
            btn.Attributes["type"] = "button";
            btn.ID = "Btn" + col;
            btn.Attributes["class"] = "SelectButton";
            btn.InnerText = "...";
            btn.Attributes["clickParam"] =
            string.Format("{{ codeId: 'Txt{0}', nameId: '{0}_{1}', selectId: '{2}', dbCodeCol: '{3}', dbNameCol: '{4}' }}",
                col, row.���ʌ������ʏo�͍���, row.���ʌ���ID2, row.�R�[�h�l��, row.���̗�
            );
            cell.Controls.Add(btn);
        }

        // ���ʌ������ʏo�͍���
        if (!string.IsNullOrEmpty(row.���ʌ������ʏo�͍���))
        {
            TextBox txt = new TextBox();
            txt.ID = col + "_" + row.���ʌ������ʏo�͍���;
            txt.CssClass = "TextInput";
            txt.ReadOnly = true;
            txt.Attributes["size"] = "30";
            cell.Controls.Add(txt);
        }
    }

    //************************************************************************
    /// <summary>
    /// �w���Xml�t�@�C������t�H�[�����쐬����B
    /// </summary>
    /// <param name="table">HTML table</param>
    /// <param name="argName">Xml�t�@�C����</param>
    /// <param name="selectForm">���������t���O</param>
    /// <returns>�t�H�[��</returns>
    //************************************************************************
    protected void CreateForm(Table table, string argName, bool selectForm = false)
    {
        // �f�[�^�Z�b�g���擾
        CM����DataSet ds = GetFormDataSet(argName);

        // StringBuilder�쐬
        StringBuilder sb = new StringBuilder();

        int colCnt = 0;
        TableRow tableRow = null;

        // ���͗��쐬���[�v
        foreach (var row in ds.����)
        {
            if (row.��\�� == "F") continue;

            if (colCnt == 0)
            {
                tableRow = new TableRow();
                table.Rows.Add(tableRow);
            }

            string cssClass;
            int maxLen;
            int width;

            string col = GetColParams(row, out cssClass, out maxLen, out width);

            // ���ږ�
            TableCell labelCell = new TableCell();
            labelCell.CssClass = "ItemName";
            labelCell.Text = col;
            tableRow.Cells.Add(labelCell);

            TableCell inputCell = new TableCell();
            inputCell.CssClass = "ItemPanel";
            tableRow.Cells.Add(inputCell);

            // ���͗�
            if (row.FromTo)
            {
                CreateInput(inputCell, row.���ږ� + "From", cssClass, maxLen, row, selectForm);
                Label t = new Label();
                t.Text= " �` ";
                inputCell.Controls.Add(t);
                CreateInput(inputCell, row.���ږ� + "To", cssClass, maxLen, row, selectForm);
            }
            else CreateInput(inputCell, row.���ږ�, cssClass, maxLen, row, selectForm);

            // ���s����
            if (colCnt == 1) colCnt = 0;
            else colCnt++;
        }
    }
#endif

        //************************************************************************
        /// <summary>
        /// �w���Xml�t�@�C������ڍ׃t�H�[����ValidationRule���쐬����B
        /// </summary>
        /// <param name="argName">Xml�t�@�C����</param>
        /// <returns>�ڍ׃t�H�[����ValidationRule</returns>
        //************************************************************************
        protected string GetValidationRules(string argName)
        {
            // �f�[�^�Z�b�g���擾
            CM����DataSet ds = GetFormDataSet(argName);

            // StringBuilder�쐬
            StringBuilder sb = new StringBuilder();

            // DataColumn�ǉ�
            foreach (var row in ds.����)
            {
                // ��\����͖���
                if (!string.IsNullOrEmpty(row.��\��) && row.��\��.Contains('F')) continue;

                StringBuilder rule = new StringBuilder();

                // �K�{����
                if (row.���͐��� == "�K�{") rule.Append("required: true, ");

                if (rule.Length > 0)
                    sb.Append(row.���ږ�).Append(": { ").Append(rule).AppendLine("},");
            }

            return sb.ToString();
        }
        #endregion

        //************************************************************************
        /// <summary>
        /// ���ڃp�����[�^��Ԃ��B
        /// </summary>
        /// <param name="argRow">����Row</param>
        /// <param name="argCssClass">CSS�N���X</param>
        /// <param name="argMaxLen">�ő咷</param>
        /// <returns>���ږ�</returns>
        //************************************************************************
        private string GetColParams(CM����DataSet.����Row argRow, out string argCssClass, out int argMaxLen, out int argWidth)
        {
            string name = string.IsNullOrEmpty(argRow.���x��) ? argRow.���ږ� : argRow.���x��;

            // �^
            CMDbType dbType = (CMDbType)Enum.Parse(typeof(CMDbType), argRow.���ڌ^);

            argCssClass = null;
            argMaxLen = argRow.����;
            argWidth = 0;

            switch (dbType)
            {
                case CMDbType.�敪:
                    break;
                case CMDbType.�t���O:
                    break;
                case CMDbType.�R�[�h:
                case CMDbType.�R�[�h_��:
                    argCssClass = "CodeInput";
                    argWidth = argMaxLen * 8 + 4;
                    break;
                case CMDbType.������:
                    argCssClass = "TextInput";
                    argWidth = Math.Min(argMaxLen * 16 + 4, 120);
                    break;
                case CMDbType.���z:
                case CMDbType.���l:
                    argCssClass = "NumberInput";
                    argMaxLen += (argMaxLen - 1) / 3 + argRow.������ > 0 ? argRow.������ + 1 : 0;
                    argWidth = argMaxLen * 8 + 4;
                    break;
                case CMDbType.���t:
                    argCssClass = "DateInput";
                    argMaxLen = 10;
                    argWidth = 70;
                    break;
                case CMDbType.����:
                    argCssClass = "DateInput";
                    argMaxLen = 19;
                    argWidth = 110;
                    break;
            }

            argWidth = Math.Max(argWidth, System.Windows.Forms.TextRenderer.MeasureText(name, new System.Drawing.Font("MS UI Gothic", 9)).Width);

            return name;
        }

        //************************************************************************
        /// <summary>
        /// �t�H�[���f�[�^�Z�b�g���擾����B
        /// </summary>
        /// <param name="argName">Xml�t�@�C����</param>
        /// <returns>�t�H�[���f�[�^�Z�b�g</returns>
        //************************************************************************
        protected CM����DataSet GetFormDataSet(string argName)
        {
            if (!m_formDsDic.ContainsKey(argName))
            {
                // �f�[�^�Z�b�g�Ƀt�@�C����ǂݍ���
                CM����DataSet ds = new CM����DataSet();
                ds.ReadXml(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "View", argName + ".xml"));

                string entName = ds.���ڈꗗ.First().�G���e�B�e�BID;
                if (!string.IsNullOrEmpty(entName))
                {
                    CM����DataSet entDs = new CM����DataSet();
                    entDs.ReadXml(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Model", entName + ".xml"));

                    // �}�[�W���鍀�ږ�
                    string[] colNames =
                    {
                        "��L�[", "���ڌ^", "����", "������", "��l����CD", "�f�t�H���g", "���ʌ���ID", "���ʌ����p�����[�^"
                    };

                    // �t�H�[���ɃG���e�B�e�B�̏����}�[�W
                    foreach (var row in ds.����)
                    {
                        DataRow[] entRows = entDs.����.Select("���ږ�='" + row.���ږ� + "'");
                        if (entRows.Length == 0) continue;

                        CM����DataSet.����Row entRow = (CM����DataSet.����Row)entRows[0];

                        foreach (string col in colNames)
                            if (row[col] == DBNull.Value) row[col] = entRow[col];

                        if (row["���͐���"] == DBNull.Value)
                        {
                            if (entRow.�K�{ == true) row.���͐��� = "�K�{";
                            else if (!string.IsNullOrEmpty(entRow.���͐���)) row.���͐��� = entRow.���͐���;
                        }
                    }
                }

                // �ҏW�����f�[�^�Z�b�g���L��
                m_formDsDic[argName] = ds;
            }

            return m_formDsDic[argName];
        }

        #region ���N�G�X�g���s
        //************************************************************************
        /// <summary>
        /// �u���E�U����̃��N�G�X�g�����s����B
        /// </summary>
        /// <param name="argFacade"></param>
        /// <param name="argForm"></param>
        //************************************************************************
        protected void DoRequest(ICMBaseBL argFacade, NameValueCollection argForm = null)
        {
            if (argForm == null) argForm = Request.Form;

            dynamic result = null;

            // �t�@�T�[�h�̌Ăяo���p�ϐ�
            DateTime operationTime;
            CMMessage message;

            try
            {
                // �����̏ꍇ
                if (Request.QueryString["_search"] != null)
                {
                    // �����p�����[�^�擾
                    List<CMSelectParam> param = CreateSelectParam();

                    CMSelectType selType = Request.QueryString["_search"] == "edit" ? CMSelectType.Edit : CMSelectType.List;

                    // �t�@�T�[�h�̌Ăяo��
                    DataSet ds = argFacade.Select(param, selType, out operationTime, out message);

                    // �ԋp���b�Z�[�W�̕\��
                    if (message != null) ShowMessage(message);

                    DataTable table = ds.Tables[0];
                    
                    // �ԋp�f�[�^�N���X�쐬
                    if (selType == CMSelectType.Edit)
                    {
                        ResultDataSet resultDs = new ResultDataSet();

                        // �ŏ��̍s�̃f�[�^��ݒ�
                        if (table.Rows.Count > 0)
                        {
                            DataRow row = table.Rows[0];

                            foreach (DataColumn dcol in table.Columns)
                                resultDs.firstRow.Add(dcol.ColumnName, row[dcol.ColumnName]);
                        }

                        // DataTable��ݒ�
                        foreach (DataTable dt in ds.Tables)
                        {
                            ResultData rd = new ResultData();
                            rd.records = dt.Rows.Count;
                            resultDs.tables.Add(dt.TableName, rd);

                            foreach (DataRow row in dt.Rows)
                                rd.rows.Add(new ResultRecord { id = Convert.ToInt32(row["ROWNUMBER"]), cell = row.ItemArray });

#if ResultTable
                            ResultTable rt = new ResultTable();
                            rt.records = dt.Rows.Count;
                            resultDs.tables.Add(dt.TableName, rt);

                            foreach (DataRow row in dt.Rows)
                            {
                                Dictionary<string, object> record = new Dictionary<string, object>();
                                rt.rows.Add(record);

                                foreach (DataColumn dcol in dt.Columns)
                                {
                                    string name = dcol.ColumnName;
                                    if (name == "ROWNUMBER") record.Add("id", Convert.ToInt32(row[name]));
                                    else if (name == "�폜") record.Add("���", row[name]);
                                    else record.Add(name, row[name]);
                                }
                            }
#endif
                        }

                        // �V�K�̏ꍇ
                        string mode = Request.QueryString["_mode"];
                        if (mode == "new")
                        {
                            // �S�ĐV�K�s�ɂ���
                            foreach (DataTable dt in ds.Tables)
                                foreach (DataRow row in dt.Rows) row.SetAdded();
                        }

                        // �e�̓N���A
                        if (mode == "new") table.Rows.Clear();

                        result = resultDs;
                    }
                    // �ꗗ����
                    else
                    {
                        result = new ResultData();
                        foreach (DataRow row in table.Rows)
                            result.rows.Add(new ResultRecord { id = Convert.ToInt32(row["ROWNUMBER"]), cell = row.ItemArray });
                    }

                    // �������ʂ�ۑ�
                    Session[Request.Path + "_DataSet"] = ds;
                }
                // �ҏW����̏ꍇ
                else if (argForm["oper"] != null)
                {
                    // �������ʂ��擾
                    DataSet ds = (DataSet)Session[Request.Path + "_DataSet"];

                    // �ҏW�Ώۂ�DataTable�擾
                    DataTable table = Request.QueryString["TableName"] != null ?
                        (DataTable)ds.Tables[Request.QueryString["TableName"]] : (DataTable)ds.Tables[0];

                    // �ҏW�Ώۂ�DataRow�擾
                    string id = argForm["id"];
                    DataRow row = string.IsNullOrEmpty(id) || id == "_empty" ?
                        null : table.Select("ROWNUMBER=" + id).First();

                    string oper = argForm["oper"];

                    switch (oper)
                    {
                        case "add":
                        case "new":
                            // todo:����������table���N���A����

                            row = table.NewRow();

                            // �V�K��id���擾
                            int retId = table.Rows.Count > 0 ? Convert.ToInt32(table.AsEnumerable().Max(tr => tr["ROWNUMBER"])) + 1 : 0;
                            row["ROWNUMBER"] = retId;

                            // �p�����[�^��ݒ�
                            foreach (string key in argForm.Keys)
                            {
                                if (!table.Columns.Contains(key)) continue;

                                row[key] = GetDataColumnVal(table.Columns[key], argForm[key]);
                            }

                            table.Rows.Add(row);

                            // id��ԋp
                            if (oper == "add") Response.Write(retId.ToString());
                            else result = new ResultStatus { id = retId };
                            break;

                        case "edit":
                            foreach (string key in argForm.Keys)
                            {
                                if (!table.Columns.Contains(key)) continue;

                                string txtVal = argForm[key];
                                object value = GetDataColumnVal(table.Columns[key], txtVal);

                                if (value == DBNull.Value)
                                {
                                    if (row[key] != DBNull.Value) row[key] = value;
                                    continue;
                                }

                                // �^�ɉ����āA�l���r���ADataTable�ɒl��ݒ肷��
                                switch (table.Columns[key].DataType.Name)
                                {
                                    case "bool":
                                    case "Boolean":
                                        if (row[key] == DBNull.Value) row[key] = value;
                                        else if (row[key].ToString() != value.ToString())
                                            row[key] = value;
                                        // �ł��ĂȂ�
                                        break;

                                    case "DateTime":
                                        if (row[key] == DBNull.Value) row[key] = value;
                                        else if (((DateTime)row[key]) != ((DateTime)value))
                                            row[key] = value;
                                        break;

                                    default:
                                        if (row[key].ToString() != txtVal)
                                            row[key] = value;
                                        break;
                                }
                            }

                            // �ύX����̏ꍇ�Aid��ԋp
                            Response.Write(row.RowState == DataRowState.Modified ? id : "");
                            break;

                        case "del":
                            result = new ResultStatus();
                            foreach (string ids in id.Split(','))
                            {
                                DataRow delRow = table.Select("ROWNUMBER=" + ids).First();
                                delRow["�폜"] = "1";
                            }
                            break;

                        case "cancel":
                            row.RejectChanges();

                            // �ԋp�f�[�^�N���X�쐬
                            result = new Dictionary<string, object>();

                            if (row.RowState == DataRowState.Detached) row.Delete();
                            else
                            {
                                foreach (DataColumn dcol in table.Columns)
                                {
                                    if (dcol.ColumnName == "�쐬����") break;
                                    result.Add(dcol.ColumnName, row[dcol.ColumnName, DataRowVersion.Original]);
                                }
                            }
                            break;

                        case "commit":
                            result = new ResultStatus();
                            DataSet updateDs = ds.GetChanges();

                            if (updateDs != null)
                            {
                                foreach (DataTable dt in updateDs.Tables)
                                {
                                    // �폜�s���m��
                                    foreach (var delRow in dt.Select("�폜 = '1'")) delRow.Delete();
                                }

                                // �t�@�T�[�h�̌Ăяo��
                                argFacade.Update(updateDs, out operationTime);
                            }
                            else
                            {
                                result.error = true;
                                // �G���[���b�Z�[�W��ݒ�
                                result.messages.Add(new ResultMessage
                                {
                                    messageCd = "WV106",
                                    message = CMMessageManager.GetMessage("WV106"),
                                });
                            }
                            break;

                        case "csvexp":
                            // �����p�����[�^�擾
                            List<CMSelectParam> param = CreateSelectParam();

                            // �t�@�T�[�h�̌Ăяo��
                            DataSet expDs = argFacade.Select(param, CMSelectType.Csv, out operationTime, out message);

                            // �w�b�_�ݒ�
                            Response.AppendHeader("Content-type", "application/octet-stream; charset=UTF-8");
                            Response.AppendHeader("Content-Disposition", "Attachment; filename=" +
                                ((dynamic)Master).Title + ".xlsx");

                            // Excel�t�@�C���쐬
                            var xslDoc = CreateExcel(expDs);
                            xslDoc.SaveAs(Response.OutputStream);
                            break;
                    }
               }
            }
            catch (CMException ex)
            {
                Response.StatusCode = 200;

                result = new ResultStatus { error = true };
                // �G���[���b�Z�[�W��ݒ�
                result.messages.Add(new ResultMessage
                {
                    messageCd = ex.CMMessage.MessageCd,
                    message = ex.CMMessage.ToString(),
                    rowField = ex.CMMessage.RowField
                });
            }
            catch (System.Data.SqlClient.SqlException ex)
            {
                // �f�[�^�x�[�X�G���[
                Response.StatusCode = 500;

                result = new ResultStatus { error = true };
                // �G���[���b�Z�[�W��ݒ�
                result.messages.Add(new ResultMessage
                {
                    messageCd = "EV002",
                    message = CMMessageManager.GetMessage("EV002", ex.Message)
                });
            }
            catch (Exception ex)
            {
                Response.StatusCode = 500;
                Response.Write(ex.ToString());
            }

            // ���ʂ�JSON�ŕԋp
            if (result != null)
            {
                var serializer = new JavaScriptSerializer();
                Response.ContentType = "text/javascript";
                Response.Write(serializer.Serialize(result));
            }
            Response.End();
        }

        //************************************************************************
        /// <summary>
        /// DataColumn�ɑΉ������^�̒l���擾����B
        /// </summary>
        /// <param name="dcol">DataColumn</param>
        /// <param name="value">�l�̕�����</param>
        /// <returns>DataColumn�ɑΉ������^�̒l</returns>
        //************************************************************************
        protected object GetDataColumnVal(DataColumn dcol, string value)
        {
            if (value.Length == 0) return DBNull.Value;

            object result;

            // �^�ɉ����āA�l���r���ADataTable�ɒl��ݒ肷��
            switch (dcol.DataType.Name)
            {
                case "bool":
                case "Boolean":
                    // �ł��ĂȂ�
                    result = value == "true";  //Convert.ToBoolean(value);
                    break;

                case "decimal":
                    result = Convert.ToDecimal(value);
                    break;

                case "int32":
                case "Byte":
                    result = Convert.ToInt32(value);
                    break;

                case "DateTime":
                    result = Convert.ToDateTime(value);
                    break;

                default:
                    result = value;
                    break;
            }

            return result;
        }

        //************************************************************************
        /// <summary>
        /// �����p�����[�^���쐬����B
        /// </summary>
        /// <returns>�����p�����[�^</returns>
        //************************************************************************
        protected List<CMSelectParam> CreateSelectParam()
        {
            List<CMSelectParam> param = new List<CMSelectParam>();

            foreach (string key in Request.QueryString)
            {
                // �ȉ��͖���
                if (key.StartsWith("_") || key.StartsWith("ctl00$") || key.EndsWith("To") ||
                    System.Text.RegularExpressions.Regex.IsMatch(key, "(nd|rows|page|sidx|sord|oper)")) continue;

                // From�̏ꍇ
                if (key.EndsWith("From"))
                {
                    // From�Ȃ����̎擾
                    string colName = key.Substring(0, key.IndexOf("From"));
                    string toName = colName + "To";

                    bool isSetFrom = !string.IsNullOrEmpty(Request.QueryString[key]);
                    bool isSetTo = !string.IsNullOrEmpty(Request.QueryString[toName]);

                    // FromTo
                    if (isSetFrom && isSetTo)
                    {
                        param.Add(new CMSelectParam(colName,
                            string.Format("BETWEEN @{0} AND @{1}", key, toName),
                            Request.QueryString[key], Request.QueryString[toName]));
                    }
                    // From or To
                    else if (isSetFrom || isSetTo)
                    {
                        string op = isSetFrom ? ">= @" + key : "<= @" + toName;

                        param.Add(new CMSelectParam(colName, op, isSetFrom ? Request.QueryString[key] : Request.QueryString[toName]));
                    }
                }
                // �P�ꍀ�ڂ̏ꍇ
                else
                {
                    // �ݒ肠��̏ꍇ
                    if (!string.IsNullOrEmpty(Request.QueryString[key]))
                    {
                        string op = "= @";
                        string value = Request.QueryString[key];

                        // LIKE�����̏ꍇ
                        if (key.EndsWith("��"))
                        {
                            op = "LIKE @";
                            value = "%" + value + "%";
                        }

                        param.Add(new CMSelectParam(key, op + key, value));
                    }
                }
            }

            return param;
        }
        #endregion
        #endregion
    }
}
