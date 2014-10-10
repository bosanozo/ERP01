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
        //************************************************************************
        /// <summary>
        /// �t�H�[���f�[�^�Z�b�g���擾����B
        /// </summary>
        /// <param name="argName">Xml�t�@�C����</param>
        /// <returns>�t�H�[���f�[�^�Z�b�g</returns>
        //************************************************************************
        protected CM����DataSet GetFormDataSet(string argName)
        {
            // �ҏW�����f�[�^�Z�b�g���L��
            if (!m_formDsDic.ContainsKey(argName))
                m_formDsDic[argName] = CM����DataSet.ReadFormXml(argName);

            return m_formDsDic[argName];
        }

        #region ���N�G�X�g���s
        //************************************************************************
        /// <summary>
        /// ���������s����B
        /// </summary>
        /// <param name="argFacade">���������s����t�@�T�[�h</param>
        /// <param name="argParam">�����p�����[�^</argParam>
        /// <returns>��������DataSet</returns>
        //************************************************************************
        protected void DoSearch(ICMBaseBL argFacade, List<CMSelectParam> argParam = null)
        {
            // �����p�����[�^�擾
            if (argParam == null) argParam = CreateSelectParam(Request.QueryString);

            dynamic result = null;
            DataSet ds = null;

            try
            {
                string search = Request.QueryString["_search"];
                CMSelectType selType = search == "edit" ? CMSelectType.Edit : 
                    search == "csvexp" ? CMSelectType.Csv : CMSelectType.List;

                // �t�@�T�[�h�̌Ăяo���p�ϐ�
                DateTime operationTime;
                CMMessage message;

                // �t�@�T�[�h�̌Ăяo��
                ds = argFacade.Select(argParam, selType, out operationTime, out message);

                // �ԋp���b�Z�[�W�̕\��
                if (message != null) ShowMessage(message);

                DataTable table = ds.Tables[0];
                    
                // �ԋp�f�[�^�N���X�쐬
                switch (selType)
                {
                    // �ꗗ����
                    case CMSelectType.List:
                        result = new ResultData();
                        foreach (DataRow row in table.Rows)
                            result.rows.Add(new ResultRecord { id = Convert.ToInt32(row["ROWNUMBER"]), cell = row.ItemArray });
                        break;

                    case CMSelectType.Edit:
                        result = CreateResultDataSet(ds);
                        break;

                    case CMSelectType.Csv:
                        // �w�b�_�ݒ�
                        Response.AppendHeader("Content-type", "application/octet-stream; charset=UTF-8");
                        Response.AppendHeader("Content-Disposition", "Attachment; filename=" +
                            ((dynamic)Master).Title + ".xlsx");

                        // Excel�t�@�C���쐬
                        var xslDoc = CreateExcel(ds);
                        xslDoc.SaveAs(Response.OutputStream);
                        break;
                }

                // �������ʂ�ۑ�
                if (selType != CMSelectType.Csv)
                    Session[Request.Path + "_DataSet"] = ds;
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
        /// ��������s����B
        /// </summary>
        /// <param name="argFacade">��������s����t�@�T�[�h</param>
        /// <param name="argDataSet">����Ώۂ�DataSet</param>
        /// <param name="argForm">Form</param>
        //************************************************************************
        protected void DoOperation(ICMBaseBL argFacade, DataSet argDataSet, NameValueCollection argForm = null)
        {
            if (argForm == null) argForm = Request.Form;

            dynamic result = null;

            try
            {
                // �ҏW�Ώۂ�DataTable�擾
                DataTable table = Request.QueryString["TableName"] != null ?
                    (DataTable)argDataSet.Tables[Request.QueryString["TableName"]] : (DataTable)argDataSet.Tables[0];

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
                        DataSet updateDs = argDataSet.GetChanges();

                        if (updateDs != null)
                        {
                            foreach (DataTable dt in updateDs.Tables)
                            {
                                // �폜�s���m��
                                foreach (var delRow in dt.Select("�폜 = '1'")) delRow.Delete();
                            }

                            // �t�@�T�[�h�̌Ăяo���p�ϐ�
                            DateTime operationTime;

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
        /// DataSet����ResultDataSet���쐬����B
        /// </summary>
        /// <param name="ds">DataSet</param>
        /// <returns>ResultDataSet</returns>
        //************************************************************************
        private ResultDataSet CreateResultDataSet(DataSet ds)
        {
            ResultDataSet resultDs = new ResultDataSet();

            DataTable table = ds.Tables[0];

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

            return resultDs;
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
        /// <param name="argQuery">QueryString</param>
        /// <param name="argName">Xml�t�@�C����</param>
        /// <returns>�����p�����[�^</returns>
        //************************************************************************
        protected List<CMSelectParam> CreateSelectParam(NameValueCollection argQuery, string argName = null)
        {
            // �f�[�^�Z�b�g���擾
            CM����DataSet ds = argName != null ? ds = GetFormDataSet(argName) : null;
            
            List<CMSelectParam> param = new List<CMSelectParam>();

            foreach (string key in argQuery)
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

                    bool isSetFrom = !string.IsNullOrEmpty(argQuery[key]);
                    bool isSetTo = !string.IsNullOrEmpty(argQuery[toName]);

                    // FromTo
                    if (isSetFrom && isSetTo)
                    {
                        param.Add(new CMSelectParam(colName,
                            string.Format("BETWEEN @{0} AND @{1}", key, toName),
                            argQuery[key], argQuery[toName]));
                    }
                    // From or To
                    else if (isSetFrom || isSetTo)
                    {
                        string op = isSetFrom ? ">= @" + key : "<= @" + toName;

                        param.Add(new CMSelectParam(colName, op, isSetFrom ? argQuery[key] : argQuery[toName]));
                    }
                }
                // �P�ꍀ�ڂ̏ꍇ
                else
                {
                    // �ݒ肠��̏ꍇ
                    if (string.IsNullOrEmpty(argQuery[key])) continue;

                    string op = "= @";
                    string value = argQuery[key];

                    if (ds != null)
                    {
                        // LIKE�����̏ꍇ
                        var irows = ds.����.Where(item => item.���ږ� == key);
                        if (irows.Count() > 0 && !string.IsNullOrEmpty(irows.First().��v����))
                        {
                            if (irows.First().��v���� != "�w��Ȃ�") op = "LIKE @";

                            switch (irows.First().��v����)
                            {
                                case "�O��":
                                    value = value + "%";
                                    break;
                                case "����":
                                    value = "%" + value + "%";
                                    break;
                                case "���":
                                    value = "%" + value;
                                    break;
                            }
                        }
                    }

                    param.Add(new CMSelectParam(key, op + key, value));
                }
            }

            return param;
        }
        #endregion
        #endregion
    }
}
