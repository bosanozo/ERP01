/*******************************************************************************
 * �y���ʕ��i�z
 * 
 * �쐬��: ���i�e�N�m���W�[�^�c�� �]
 * ���ŗ���:
 * 2014.1.30, �V�K�쐬
 ******************************************************************************/
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Reflection;
using System.Web;
using System.Web.SessionState;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;

using NEXS.ERP.CM.Common;
using NEXS.ERP.CM.BL;

namespace NEXS.ERP.CM.WEB
{
    //************************************************************************
    /// <summary>
    /// �ꗗ��ʂ̊��N���X
    /// </summary>
    //************************************************************************
    public class CMBaseListForm : CMBaseForm
    {
        #region �R���X�g���N�^
        //************************************************************************
        /// <summary>
        /// �R���X�g���N�^
        /// </summary>
        //************************************************************************
        public CMBaseListForm()
        {
        }
        #endregion

        #region protected���\�b�h
        //************************************************************************
        /// <summary>
        /// ���������s����B
        /// </summary>
        /// <param name="argFacade">�g�p�t�@�T�[�h</param>
        /// <param name="argParam">���������p�����[�^</param>
        /// <param name="argGrid">�ꗗ�\���p�O���b�h</param>
        /// <param name="argPage">�y�[�W</param>
        /// <returns>True:�G���[����, False:�G���[�Ȃ�</returns>
        //************************************************************************
        protected bool DoSelect(ICMBaseBL argFacade, List<CMSelectParam> argParam, GridView argGrid, int argPage = 0)
        {
            try
            {
                // �t�@�T�[�h�̌Ăяo��
                DateTime operationTime;
                CMMessage message;
                DataSet result = argFacade.Select(argParam, CMSelectType.List, out operationTime, out message);

                // �ԋp���b�Z�[�W�̕\��
                if (message != null) ShowMessage(message);
                
                // DataSource�ݒ�
                argGrid.DataSource = result.Tables[0];
                // �y�[�W�Z�b�g
                argGrid.PageIndex = argPage;
                // �o�C���h
                argGrid.DataBind();
            }
            catch (Exception ex)
            {
                // DataSource�N���A
                argGrid.DataSource = null;
                argGrid.DataBind();

                ShowError(ex);

                return true;
            }

            return false;
        }

        //************************************************************************
        /// <summary>
        /// CSV�o�͂����s����B
        /// </summary>
        /// <param name="argFacade">�g�p�t�@�T�[�h</param>
        /// <param name="argParam">���������p�����[�^</param>
        /// <param name="argUrl">�Q��URL</param>
        /// <returns>True:�G���[����, False:�G���[�Ȃ�</returns>
        //************************************************************************
        protected bool DoCsvOut(ICMBaseBL argFacade, List<CMSelectParam> argParam, out string argUrl)
        {
            argUrl = null;

            try
            {
                // �t�@�T�[�h�̌Ăяo��
                DateTime operationTime;
                CMMessage message;
                DataSet result = argFacade.Select(argParam, CMSelectType.Csv, out operationTime, out message);

                // �ԋp���b�Z�[�W�̕\��
                if (message != null) ShowMessage(message);

                DataTable table = result.Tables[0];

                bool found = table.Rows.Count > 0;
                // �������ʂ���̏ꍇ
                if (found)
                {
                    // CSV�t�@�C�����쐬
                    //string fname = string.Format("{0}_{1}_{2}.csv",
                    string fname = string.Format("{0}_{1}_{2}.xlsx",
                        table.TableName, DateTime.Now.ToString("yyyyMMddHHmmss"), CMInformationManager.UserInfo.Id);
                    // CSV�t�@�C���o��
                    //ExportCsv(table, System.IO.Path.Combine(Request.PhysicalApplicationPath, "Csv", fname));
                    ExportExcel(result, System.IO.Path.Combine(Request.PhysicalApplicationPath, "Csv", fname));
                    // ��ʕ\��
                    argUrl = Request.ApplicationPath + "/Csv/" + Uri.EscapeUriString(fname);
                }
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
        /// �@�\�{�^���ɃX�N���v�g��o�^����B
        /// </summary>
        /// <param name="argButtonSelect">�����{�^��</param>
        /// <param name="argButtonCsvOut">CSV�o�̓{�^��</param>
        //************************************************************************
        protected void AddFuncOnclick(Button argButtonSelect, Button argButtonCsvOut)
        {
            argButtonSelect.Attributes.Add("onclick","return CheckInputList()");
            argButtonCsvOut.Attributes.Add("onclick","ShowWaitMessage(); return CheckInputList()");
        }

        //************************************************************************
        /// <summary>
        /// �@�\�{�^���ɃX�N���v�g��o�^����B
        /// </summary>
        /// <param name="argButtonSelect">�����{�^��</param>
        /// <param name="argButtonCsvOut">CSV�o�̓{�^��</param>
        /// <param name="argButtonInsert">�V�K�{�^��</param>
        /// <param name="argButtonUpdate">�C���{�^��</param>
        /// <param name="argButtonDelete">�폜�{�^��</param>
        //************************************************************************
        protected void AddFuncOnclick(Button argButtonSelect, Button argButtonCsvOut,
            Button argButtonInsert, Button argButtonUpdate, Button argButtonDelete)
        {
            AddFuncOnclick(argButtonSelect, argButtonCsvOut);
            argButtonInsert.Attributes.Add("onclick","return OpenEntryForm('Insert')");
            argButtonUpdate.Attributes.Add("onclick","return OpenEntryForm('Update')");
            argButtonDelete.Attributes.Add("onclick","return OpenEntryForm('Delete')");
        }

        //************************************************************************
        /// <summary>
        /// �����p�����[�^��ǉ�����B
        /// </summary>
        /// <param name="param">�����p�����[�^</param>
        /// <param name="wc">WebControl</param>
        /// <param name="argPanel">���������p�l��</param>
        //************************************************************************
        private void AddSelectParam(List<CMSelectParam> param, WebControl wc, Panel argPanel)
        {
            // �e�L�X�g�ƃh���b�v�_�E�����Ώ�
            if (!(wc is DropDownList) && !(wc is TextBox))
            {
                foreach (Control c in wc.Controls)
                {
                    if (c is WebControl) AddSelectParam(param, (WebControl)c, argPanel);
                }

                return;
            }

            // To�͖���
            if (wc.ID.EndsWith("To")) return;

            // From�̏ꍇ
            if (wc.ID.EndsWith("From"))
            {
                // From�Ȃ����̎擾
                string colName = wc.ID.Substring(0, wc.ID.IndexOf("From"));

                WebControl toCnt = (WebControl)argPanel.FindControl(colName + "To");
                bool isSetFrom = IsSetValue(wc);
                bool isSetTo = IsSetValue(toCnt);

                // FromTo
                if (isSetFrom && isSetTo)
                {
                    param.Add(new CMSelectParam(colName.Substring(3),
                        string.Format("BETWEEN @{0} AND @{1}", wc.ID, toCnt.ID),
                        GetValue(wc), GetValue(toCnt)));
                }
                // From or To
                else if (isSetFrom || isSetTo)
                {
                    string op = isSetFrom ? ">= @" : "<= @";
                    WebControl condCnt = isSetFrom ? wc : toCnt;

                    param.Add(new CMSelectParam(colName.Substring(3), op + condCnt.ID, GetValue(condCnt)));
                }
            }
            // �P�ꍀ�ڂ̏ꍇ
            else
            {
                // �ݒ肠��̏ꍇ
                if (IsSetValue(wc))
                {
                    string op = "= @";
                    object value = GetValue(wc);

                    // LIKE�����̏ꍇ
                    if (wc is TextBox && wc.ID.EndsWith("��"))
                    {
                        op = "LIKE @";
                        value = "%" + value + "%";
                    }

                    param.Add(new CMSelectParam(wc.ID.Substring(3), op + wc.ID, value));
                }
            }
        }

        //************************************************************************
        /// <summary>
        /// �����p�����[�^���쐬����B
        /// </summary>
        /// <param name="argPanel">���������p�l��</param>
        /// <returns>�����p�����[�^</returns>
        //************************************************************************
        protected List<CMSelectParam> CreateSelectParam(Panel argPanel)
        {
            List<CMSelectParam> param = new List<CMSelectParam>();

            foreach (Control c in argPanel.Controls)
            {
                if (c is WebControl) AddSelectParam(param, (WebControl)c, argPanel);
            }

            return param;
        }
        #endregion
    }
}
