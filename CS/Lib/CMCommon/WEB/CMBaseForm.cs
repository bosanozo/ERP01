/*******************************************************************************
 * �y���ʕ��i�z
 * 
 * �쐬��: ���i�e�N�m���W�[�^�c�� �]
 * ���ŗ���:
 * 2014.1.30, �V�K�쐬
 ******************************************************************************/
using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.SessionState;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;

using log4net;
using Seasar.Quill;

using DocumentFormat.OpenXml;
using SpreadsheetLight;

using NEXS.ERP.CM.Common;
using NEXS.ERP.CM.BL;
using NEXS.ERP.CM.DA;

namespace NEXS.ERP.CM.WEB
{
    //************************************************************************
    /// <summary>
    /// ��ʂ̊��N���X
    /// </summary>
    //************************************************************************
    public class CMBaseForm : Page
    {
        #region ���K�[�t�B�[���h
        private ILog m_logger;
        #endregion

        #region �C���W�F�N�V�����p�t�B�[���h
        protected ICMCommonBL m_commonBL;
        #endregion

        #region �v���p�e�B
        /// <summary>
        /// ���K�[
        /// </summary>
        protected ILog Log
        {
            get { return m_logger; }
        }

        /// <summary>
        /// ���ʏ����t�@�T�[�h
        /// </summary>
        protected ICMCommonBL CommonBL
        {
            get { return m_commonBL; }
        }        
        #endregion

        #region �R���X�g���N�^
        //************************************************************************
        /// <summary>
        /// �R���X�g���N�^
        /// </summary>
        //************************************************************************
        public CMBaseForm()
        {
            // ���K�[���擾
            m_logger = LogManager.GetLogger(this.GetType());

            // �C���W�F�N�V�������s
            QuillInjector injector = QuillInjector.GetInstance();
            injector.Inject(this);
        }
        #endregion

        #region protected���\�b�h
        #region ���b�Z�[�W�֘A
        //************************************************************************
        /// <summary>
        /// ���b�Z�[�W���N���A����B
        /// </summary>
        //************************************************************************
        /*
        protected void ClearMessage()
        {
            ShowMessage("I", "");
        }*/

        //************************************************************************
        /// <summary>
        /// ����������O�����b�Z�[�W�\������B
        /// </summary>
        /// <param name="argException">����������O</param>
        //************************************************************************
        protected void ShowError(Exception argException)
        {
            // CMException�̏ꍇ
            if (argException is CMException)
            {
                CMException ex = (CMException)argException;

                // ���O�̏o��
                if (ex.CMMessage != null && ex.CMMessage.MessageCd != null &&
                    ex.CMMessage.MessageCd.Length > 0)
                {
                    if (ex.CMMessage.MessageCd[0] == 'E')
                        Log.Error(ex.CMMessage.ToString(), argException);
                }

                // ���b�Z�[�W�\��
                ShowMessage(ex.CMMessage);
            }
            // ���̑��̏ꍇ
            else
            {
                string msgCd = "EV001";

                if (argException is FileNotFoundException)
                    msgCd = "W";
                else if (argException is IOException)
                    msgCd = "EV003";

                // ���O�̏o��
                if (msgCd[0] == 'E')
                    Log.Error(argException.Message, argException);

                // ���b�Z�[�W�\��
                ShowMessage(msgCd, argException.Message);
            }
        }

        //************************************************************************
        /// <summary>
        /// �w�肳�ꂽ���b�Z�[�W�R�[�h�̃��b�Z�[�W��\������B
        /// </summary>
        /// <param name="argMessage">���b�Z�[�W</param>
        /// <returns>�_�C�A���O���U���g</returns>
        //************************************************************************
        protected void ShowMessage(CMMessage argMessage)
        {
            ShowMessage(argMessage.MessageCd, argMessage.Params);
        }

        //************************************************************************
        /// <summary>
        /// �w�肳�ꂽ���b�Z�[�W�R�[�h�̃��b�Z�[�W��\������B
        /// </summary>
        /// <param name="argCode">���b�Z�[�W�R�[�h</param>
        /// <param name="argParams">�p�����[�^</param>
        //************************************************************************
        protected void ShowMessage(string argCode, params object[] argParams)
        {
            if ((dynamic)Master != null)
                ((dynamic)Master).ShowMessage(argCode, CMMessageManager.GetMessage(argCode, argParams));
        }
        #endregion

        #region UI����֘A
        //************************************************************************
        /// <summary>
        /// ��ʂ����
        /// </summary>
        /// <param name="argStatus">����:True, ���s:False</param>
        //************************************************************************
        protected void Close(bool argStatus)
        {
            string script =
                "<script language=JavaScript>" +
                "window.onLoad = window.returnValue = {0};" +
                "window.close()<" +
                "/" + "script>";

            // �X�N���v�g�o�^
            ClientScript.RegisterClientScriptBlock(GetType(),
                "Close", string.Format(script, argStatus.ToString().ToLower()));
        }

        //************************************************************************
        /// <summary>
        /// �p�l����ǂݎ���p�ɂ���B
        /// </summary>
        /// <param name="argPanel">�ǂݎ���p�ɂ���p�l��</param>
        //************************************************************************
        protected void ProtectPanel(Panel argPanel)
        {
            foreach (Control c in argPanel.Controls)
            {
                if (c is TextBox)
                {
                    TextBox t = (TextBox)c;
                    ProtectTextBox(t);
                }
                else if (c is DropDownList)
                {
                    DropDownList d = (DropDownList)c;
                    d.Enabled = false;
                    d.BackColor = Color.FromName("#CCCCFF");
                    //if (d.Visible) d.Visible = false;
                }
                else if (c is HtmlInputButton)
                {
                    HtmlInputButton b = (HtmlInputButton)c;
                    b.Disabled = true;
                }
            }
        }

        //************************************************************************
        /// <summary>
        /// �e�L�X�g�{�b�N�X��ǂݎ���p�ɂ���B
        /// </summary>
        /// <param name="argTextBox">�ǂݎ���p�ɂ���e�L�X�g�{�b�N�X</param>
        //************************************************************************
        protected void ProtectTextBox(TextBox argTextBox)
        {
            if (argTextBox.ReadOnly) return;

            //argTextBox.BorderStyle = BorderStyle.None;
            argTextBox.BackColor = Color.FromName("#CCCCFF"); //Color.Transparent;
            argTextBox.ReadOnly = true;
            argTextBox.TabIndex = -1;
        }

        //************************************************************************
        /// <summary>
        /// �R���g���[���ɒl���ݒ肳��Ă��邩�Ԃ��B
        /// </summary>
        /// <param name="arg">�R���g���[��</param>
        /// <returns>True:�ݒ肠��, False:�ݒ�Ȃ�</returns>
        //************************************************************************
        protected bool IsSetValue(WebControl arg)
        {
            return arg is TextBox && ((TextBox)arg).Text.Trim().Length > 0 ||
                arg is DropDownList && ((DropDownList)arg).SelectedValue.Trim().Length > 0;
        }

        //************************************************************************
        /// <summary>
        /// �R���g���[���ɓ��͂��ꂽ�������Ԃ��B
        /// </summary>
        /// <param name="argControl">�R���g���[��</param>
        /// <returns>���͂��ꂽ������</returns>
        //************************************************************************
        protected virtual object GetValue(WebControl argControl)
        {
            if (argControl is TextBox)
            {
                if (argControl.CssClass == "DateInput")
                    return Convert.ToDateTime(((TextBox)argControl).Text);
                else
                    return ((TextBox)argControl).Text;
            }

            if (argControl is DropDownList)
                return ((DropDownList)argControl).SelectedValue;

            return "";
        }
        #endregion

        #region ���ʃt�@�T�[�h�Ăяo��
        //************************************************************************
        /// <summary>
        /// ���엚�����o�͂���B
        /// </summary>
        /// <returns>True:�G���[����AFalse:�G���[�Ȃ�</returns>
        //************************************************************************
        protected bool WriteOperationLog()
        {
            try
            {
                // ���샍�O�L�^
                CommonBL.WriteOperationLog(((dynamic)Master).Title);
            }
            catch (Exception ex)
            {
                ShowError(ex);
                return true;
            }

            return false;
        }

        //************************************************************************
        /// <summary>
        /// �R�[�h�l���疼�̂��擾����B
        /// </summary>
        /// <param name="argSelectId">�������</param>
        /// <param name="argNotFound">True:���̎擾���s, False:���̎擾����</param>
        /// <param name="argTextBox">���̕\���e�L�X�g�{�b�N�X</param>
        /// <param name="argParams">�p�����[�^</param>
        /// <returns>����</returns>
        //************************************************************************
        protected string GetCodeName(string argSelectId, out bool argNotFound,
            TextBox argTextBox, params object[] argParams)
        {
            string name = null;
            argNotFound = true;

            try
            {
                // ����
                DataTable result = CommonBL.Select(argSelectId, argParams);

                argNotFound = result == null || result.Rows.Count == 0;

                name = argNotFound ? "�R�[�h�G���[" : result.Rows[0][0].ToString();
                string cssClass = argNotFound ? "transp warning" : "1 transp";

                // ���x���̐ݒ�
                argTextBox.Text = name;
                argTextBox.CssClass = cssClass;
            }
            catch (Exception ex)
            {
                ShowError(ex);
            }

            return name;
        }

        //************************************************************************
        /// <summary>
        /// �h���b�v�_�E�����X�g�ɃA�C�e����ݒ肷��B
        /// </summary>
        /// <param name="argSelectId">�������</param>
        /// <param name="argDDList">�h���b�v�_�E�����X�g</param>
        /// <param name="argParams">�p�����[�^</param>
        //************************************************************************
        protected void SetDropDownItems(string argSelectId, DropDownList argDDList,
            params object[] argParams)
        {
            try
            {
                // ����
                DataTable result = CommonBL.Select(argSelectId, argParams);

                // �������ʂ�ݒ�
                argDDList.DataSource = result;
            }
            catch (Exception ex)
            {
                ShowError(ex);
            }
        }
        #endregion

        #region �h���b�v�_�E�����X�g�֘A
        //************************************************************************
        /// <summary>
        /// �h���b�v�_�E�����X�g�ɃA�C�e����ݒ肷��B
        /// �h���b�v�_�E�����X�g�̍ŏ��Ɏw��Ȃ���}������B
        /// </summary>
        /// <param name="argSelectId">�������</param>
        /// <param name="argDDList">�h���b�v�_�E�����X�g</param>
        /// <param name="argParams">�p�����[�^</param>
        //************************************************************************
        protected void SetDropDownItemsList(string argSelectId, DropDownList argDDList,
            params object[] argParams)
        {
            SetDropDownItems(argSelectId, argDDList, argParams);
            InsertTopItem(argDDList, "�w��Ȃ�");
        }

        //************************************************************************
        /// <summary>
        /// �h���b�v�_�E�����X�g�ɃA�C�e����ݒ肷��B
        /// �h���b�v�_�E�����X�g�̍ŏ��Ɏw��Ȃ���}������B
        /// </summary>
        /// <param name="argSelectId">�������</param>
        /// <param name="argDDList">�h���b�v�_�E�����X�g</param>
        /// <param name="argParams">�p�����[�^</param>
        //************************************************************************
        protected void SetDropDownItemsEntry(string argSelectId, DropDownList argDDList,
            params object[] argParams)
        {
            SetDropDownItems(argSelectId, argDDList, argParams);
            InsertTopItem(argDDList, "");
        }

        //************************************************************************
        /// <summary>
        /// �h���b�v�_�E�����X�g�̍ŏ��ɃA�C�e����}������B
        /// </summary>
        /// <param name="argDDList">�h���b�v�_�E�����X�g</param>
        /// <param name="argTopText">�A�C�e���\����</param>
        //************************************************************************
        protected void InsertTopItem(DropDownList argDDList, string argTopText)
        {
            DataTable table = (DataTable)argDDList.DataSource;
            DataRow row = table.NewRow();
            row[argDDList.DataTextField] = argTopText;
            table.Rows.InsertAt(row, 0);
        }

        //************************************************************************
        /// <summary>
        /// �h���b�v�_�E�����X�g�Ɏ��ԃA�C�e����ݒ肷��B
        /// </summary>
        /// <param name="argDDList">�h���b�v�_�E�����X�g</param>
        //************************************************************************
        protected void SetHourItems(DropDownList argDDList)
        {
            argDDList.Items.Add("");
            for (int i = 0; i < 24; i++) argDDList.Items.Add(i.ToString("00"));
        }

        //************************************************************************
        /// <summary>
        /// �h���b�v�_�E�����X�g�ɕ��A�C�e����ݒ肷��B
        /// </summary>
        /// <param name="argDDList">�h���b�v�_�E�����X�g</param>
        //************************************************************************
        protected void SetMinuteItems(DropDownList argDDList)
        {
            argDDList.Items.Add("");
            for (int i = 0; i < 12; i++) argDDList.Items.Add((i * 5).ToString("00"));
        }

        //************************************************************************
        /// <summary>
        /// �h���b�v�_�E�����X�g�̃A�C�e���̖��̂��擾����B
        /// </summary>
        /// <param name="argDDList">�h���b�v�_�E�����X�g</param>
        /// <returns>����</returns>
        //************************************************************************
        protected string GetItemName(DropDownList argDDList)
        {
            string text = argDDList.SelectedItem.Text;
            int idx = text.IndexOf(' ');
            return idx >= 0 ? text.Substring(idx) : "";
        }
        #endregion

        #region EXCEL����
        //************************************************************************
        /// <summary>
        /// �w���Xml�t�@�C������f�[�^�e�[�u�����쐬����B
        /// </summary>
        /// <param name="argName">Xml�t�@�C����</param>
        /// <returns>�f�[�^�e�[�u��</returns>
        //************************************************************************
        protected DataTable CreateDataTableFromXml(string argName)
        {
            // �f�[�^�Z�b�g�Ƀt�@�C����ǂݍ���
            CM����DataSet ds = new CM����DataSet();
            ds.ReadXml(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Model", argName + ".xml"));

            // �f�[�^�e�[�u���쐬
            DataTable table = new DataTable(ds.���ڈꗗ[0].���ڈꗗID);

            // DataColumn�ǉ�
            foreach (var row in ds.����)
            {
                // DataColumn�쐬
                DataColumn dcol = new DataColumn(row.���ږ�);
                // �^
                CMDbType dbType = (CMDbType)Enum.Parse(typeof(CMDbType), row.���ڌ^);
                switch (dbType)
                {
                    case CMDbType.�t���O:
                        dcol.DataType = typeof(bool);
                        break;
                    case CMDbType.���z:
                    case CMDbType.���l:
                        dcol.DataType = row.������ > 0 ? typeof(decimal) : typeof(long);
                        break;
                    case CMDbType.���t:
                    case CMDbType.����:
                        dcol.DataType = typeof(DateTime);
                        break;
                }
                // �K�{����
                if (row.�K�{) dcol.AllowDBNull = false;

                table.Columns.Add(dcol);
            }

            return table;
        }

        //******************************************************************************
        /// <summary>
        /// �w��Stream����DataSet�Ƀf�[�^����荞�ށB
        /// </summary>
        /// <param name="argInputStream">����Stream</param>
        /// <returns>�f�[�^����荞��DataSet</returns>
        /// <remarks>�f�[�^����荞��DataTable�̃X�L�[�}�̓G���e�B�e�B��`XML�t�@�C����萶������B
        /// �V�[�g����XML�t�@�C�����ɂȂ�B</remarks>
        //******************************************************************************
        protected DataSet ImportExcel(Stream argInputStream)
        {
            // EXCEL�������쐬
            SLDocument xslDoc = new SLDocument(argInputStream);

            // �f�[�^�Z�b�g�Ƀf�[�^����荞��
            DataSet ds = new DataSet();

            // �V�[�g�Ń��[�v
            foreach (string sheet in xslDoc.GetSheetNames())
            {
                // �V�[�g��I��
                xslDoc.SelectWorksheet(sheet);

                // �f�[�^�e�[�u���쐬
                DataTable table = CreateDataTableFromXml(sheet);

                var sheetStat = xslDoc.GetWorksheetStatistics();

                // �P�s���ǂݍ��݁A�擪�s�̓^�C�g���Ƃ��ēǂݔ�΂�
                for (int rowIdx = sheetStat.StartRowIndex + 1; rowIdx <= sheetStat.EndRowIndex; rowIdx++)
                {
                    DataRow newRow = table.NewRow();
                    for (int colIdx = 0; colIdx < table.Columns.Count; colIdx++)
                    {
                        int col = colIdx + sheetStat.StartColumnIndex;

                        // �^�ɉ����Ēl���擾����
                        switch (table.Columns[colIdx].DataType.Name)
                        {
                            case "bool":
                                newRow[colIdx] = xslDoc.GetCellValueAsBoolean(rowIdx, col);
                                break;

                            case "decimal":
                                newRow[colIdx] = xslDoc.GetCellValueAsDecimal(rowIdx, col);
                                break;

                            case "long":
                                newRow[colIdx] = xslDoc.GetCellValueAsInt64(rowIdx, col);
                                break;

                            case "DateTime":
                                newRow[colIdx] = xslDoc.GetCellValueAsDateTime(rowIdx, col);
                                break;

                            default:
                                newRow[colIdx] = xslDoc.GetCellValueAsString(rowIdx, col);
                                break;
                        }
                    }
                    table.Rows.Add(newRow);
                }

                // �f�[�^�e�[�u����ǉ�
                ds.Tables.Add(table);
            }

            return ds;
        }
        #endregion

        #region EXCEL�o��
        //******************************************************************************
        /// <summary>
        /// �w�肳�ꂽ�f�[�^�Z�b�g�̓��e��EXCEL�t�@�C���ɏo�͂���B
        /// </summary>
        /// <param name="argDataSet">�f�[�^�Z�b�g</param>
        /// <param name="argPath">�t�@�C���o�̓t���p�X</param>
        /// <returns>true:�o�͂���, false:�L�����Z������</returns>
        /// <remarks>�e���v���[�g�t�@�C��������ꍇ�́A�e���v���[�g�t�@�C���̏����ɏ]����
        /// �f�[�^���o�͂���B�o�͊J�n�ʒu�̓Z����"�J�n"�ƋL�q���邱�ƂŎw��\(�����Ă���)�B
        /// �f�[�^���o�͂���V�[�g�̓f�[�^�e�[�u�����ƃV�[�g������v������̂��g�p����B
        /// �e���v���[�g�t�@�C���������ꍇ�́A�f�t�H���g�̌`���Ńf�[�^���o�͂���B</remarks>
        //******************************************************************************
        protected bool ExportExcel(DataSet argDataSet, string argPath)
        {
            SLDocument xslDoc = CreateExcel(argDataSet);

            // �u�b�N��ۑ�
            xslDoc.SaveAs(argPath);

            return true;
        }

        protected SLDocument CreateExcel(DataSet argDataSet)
        {
            SLDocument xslDoc;

            // �e���v���[�g�t�@�C�����쐬
            string template = Path.Combine(Request.PhysicalApplicationPath,
                "Template", argDataSet.Tables[0].TableName + ".xlsx");

            if (File.Exists(template))
            {
                // �e���v���[�g��ǂݍ���
                xslDoc = new SLDocument(template);

                foreach (string sheet in xslDoc.GetSheetNames())
                {
                    DataTable table = argDataSet.Tables[sheet];
                    if (table == null) continue;

                    // �V�[�g��I��
                    xslDoc.SelectWorksheet(sheet);

                    var sheetStat = xslDoc.GetWorksheetStatistics();
                    int startRow = sheetStat.StartRowIndex + 1;
                    int startCol = sheetStat.StartColumnIndex;

                    // �J�n�ʒu������
                    var cells = xslDoc.GetCells().Where(c => c.Value.DataType == 
                        DocumentFormat.OpenXml.Spreadsheet.CellValues.SharedString);
                    foreach (var cell in cells)
                    {
                        if (xslDoc.GetCellValueAsRstType(cell.Key.RowIndex, cell.Key.ColumnIndex).GetText() == "�J�n")
                        {
                            startRow = cell.Key.RowIndex;
                            startCol = cell.Key.ColumnIndex;
                            break;
                        }
                    }

                    // �X�^�C����ݒ�
                    for (int i = 0; i < table.Columns.Count; i++)
                        xslDoc.SetCellStyle(startRow + 1, i + startCol,
                            table.Rows.Count + startRow - 1, i + startCol,
                            xslDoc.GetCellStyle(startRow, i + startCol));

                    // �f�[�^�̏o��
                    xslDoc.ImportDataTable(startRow, startCol, table, false);
                }
            }
            else
            {
                // Book���쐬
                xslDoc = new SLDocument();

                foreach (DataTable table in argDataSet.Tables)
                {
                    // �V�[�g��ǉ�
                    if (xslDoc.GetCurrentWorksheetName() == SLDocument.DefaultFirstSheetName)
                        xslDoc.RenameWorksheet(SLDocument.DefaultFirstSheetName, table.TableName);
                    else
                        xslDoc.AddWorksheet(table.TableName);

                    // �X�^�C����ݒ�
                    for (int i = 0; i < table.Columns.Count; i++)
                    {
                        // DateTime�̏ꍇ
                        if (table.Columns[i].DataType == typeof(DateTime))
                        {
                            xslDoc.SetColumnWidth(i + 1, 11);
                            var style = xslDoc.CreateStyle();
                            style.FormatCode = "yyyy/m/d";

                            xslDoc.SetCellStyle(2, i + 1,
                                table.Rows.Count + 2, i + 1, style);
                        }
                    }

                    // �f�[�^�̏o��
                    xslDoc.ImportDataTable(1, 1, table, true);
                }
            }

            // �u�b�N��ԋp
            return xslDoc;
        }
        #endregion

        #region CSV�o��
        //************************************************************************
        /// <summary>
        /// DataTable�ɕێ�����Ă���f�[�^��CSV�t�@�C���ɏo�͂���B
        /// </summary>
        /// <param name="argTable">�o�͂���f�[�^���ێ�����Ă���DataTable</param>
        /// <param name="argPath">�t�@�C���o�̓t���p�X</param>
        /// <param name="argAppend">true:�ǉ���������, false:�V�K�쐬</param>
        /// <param name="argOutputHeader">�w�b�_�o�̓t���O</param>
        /// <param name="argDuplicateNull">�d���f�[�^NULL�t���O�Ftrue�̏ꍇ��NULL�l��O�̍s�̃f�[�^�ŕ�������B</param>
        //************************************************************************
        protected void ExportCsv(DataTable argTable, string argPath, bool argAppend = false,
            bool argOutputHeader = true, bool argDuplicateNull = false)
        {
            // �f�[�^�Ȃ��͏������Ȃ�
            if (argTable.Rows.Count == 0) return;

            StringBuilder builder = new StringBuilder();
            DataColumnCollection colmuns = argTable.Columns;

            // �t�@�C���o��
            using (StreamWriter writer = new StreamWriter(argPath, argAppend, Encoding.Default))
            {
                // �V�K�쐬���w�b�_�o�͂���̏ꍇ�A�w�b�_���o�͂���
                if (!argAppend && argOutputHeader)
                {
                    // 1���
                    builder.Append(colmuns[0].Caption);
                    // �񖈂̃��[�v
                    for (int i = 1; i < colmuns.Count; i++)
                        builder.Append(',').Append(colmuns[i].Caption);

                    // ��s���o��
                    writer.WriteLine(builder);
                    // �N���A
                    builder.Length = 0;
                }

                // �d���f�[�^�����̎w�肠��̏ꍇ
                if (argDuplicateNull)
                {
                    // �d���f�[�^�L���p
                    DataRow preRow = argTable.NewRow();

                    // �s���̃��[�v
                    foreach (DataRow row in argTable.Rows)
                    {
                        // 1���
                        if (row[0] != DBNull.Value) preRow[0] = row[0];
                        builder.Append(preRow[0].ToString());

                        // �񖈂̃��[�v
                        for (int i = 1; i < colmuns.Count; i++)
                        {
                            // �d�����Ă��Ȃ���Βl���L��
                            if (row[i] != DBNull.Value) preRow[i] = row[i];
                            builder.Append(',').Append(preRow[i].ToString());
                        }

                        // ��s���o��
                        writer.WriteLine(builder);
                        // �N���A
                        builder.Length = 0;
                    }
                }
                // �d���f�[�^�����̎w��Ȃ��̏ꍇ
                else
                {
                    // �s���̃��[�v
                    foreach (DataRow row in argTable.Rows)
                    {
                        // 1���
                        builder.Append(row[0].ToString());
                        // �񖈂̃��[�v
                        for (int i = 1; i < colmuns.Count; i++)
                            builder.Append(',').Append(row[i].ToString());

                        // ��s���o��
                        writer.WriteLine(builder);
                        // �N���A
                        builder.Length = 0;
                    }
                }
            }
        }

        //************************************************************************
        /// <summary>
        /// Excel�t�@�C����ʂ�Windows�ɊJ�����\�b�h
        /// </summary>
        /// <param name="argUrl">Excel�t�@�C����URL</param>
        //************************************************************************
        protected void OpenExcel(string argUrl)
        {
            // �Ԃ�javascript���쐬
            StringBuilder sb = new StringBuilder();
            sb.Append("<script language='javascript'>");
            sb.Append("function openExcel(){");
            // �����̃T�C�Y���擾
            sb.Append("var xMax = screen.Width, yMax = screen.Height;");
            sb.Append("var xOffset = (xMax - 800)/2, yOffset = (yMax - 600)/4;");
            sb.Append("window.open('").Append(argUrl);
            // �o�͑����̏�Ԃ�ݒ�
            sb.Append("',null,'menubar=yes,toolbar=no,location=no,width=800,height=600,screenX='+xOffset+',screenY='+yOffset+',top='+yOffset+',left='+xOffset+',resizable=yes,status=yes,scrollbars=yes,center=yes');}");
            sb.Append("window.onLoad=openExcel();");
            sb.Append("</script>");

            if (!ClientScript.IsClientScriptBlockRegistered("openExcelScript"))
                ClientScript.RegisterClientScriptBlock(GetType(), "openExcelScript", sb.ToString());
        }
        #endregion
        #endregion
    }
}
