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
            if (argParam == null) argParam = CMSelectParam.CreateSelectParam(Request.QueryString);

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
                        result = ResultData.CreateResultData(table);
                        break;

                    case CMSelectType.Edit:
                        result = ResultDataSet.CreateResultDataSet(ds);
                        break;

                    case CMSelectType.Csv:
                        // �w�b�_�ݒ�
                        Response.AppendHeader("Content-type", "application/octet-stream; charset=UTF-8");
                        Response.AppendHeader("Content-Disposition", "Attachment; filename=" +
                            ((dynamic)Master).Title + ".xlsx");

                        // Excel�t�@�C���쐬
                        var xslDoc = ExcelUtil.CreateExcel(ds);
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
                    rowField = new RowField(ex.CMMessage.RowField)
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

                            row[key] = CMUtil.GetDataColumnVal(table.Columns[key], argForm[key]);
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
                            object value = CMUtil.GetDataColumnVal(table.Columns[key], txtVal);

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
                    rowField = new RowField(ex.CMMessage.RowField)
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
        #endregion
        #endregion
    }
}
