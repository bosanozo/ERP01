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
    /// �o�^��ʂ̊��N���X
    /// </summary>
    //************************************************************************
    public class CMBaseEntryForm : CMBaseForm
    {
        #region �v���p�e�B
        /// <summary>
        /// ���샂�[�h
        /// </summary>
        public string OpeMode { get; set; }

        /// <summary>
        /// ���̓f�[�^��ێ�����DataRow
        /// </summary>
        public DataRow InputRow { get; set; }
        #endregion

        #region �R���X�g���N�^
        //************************************************************************
        /// <summary>
        /// �R���X�g���N�^
        /// </summary>
        //************************************************************************
        public CMBaseEntryForm()
        {
        }
        #endregion

        #region protected���\�b�h
        //************************************************************************
        /// <summary>
        /// ���샂�[�h��ݒ肵�A���샂�[�h�ɉ�����ʂ̏�Ԃ�ύX����B
        /// </summary>
        /// <param name="argPanelKeyItems">�L�[���ڃp�l��</param>
        /// <param name="argPanelSubItems">�]�����ڃp�l��</param>
        /// <param name="argPanelUpdateInfo">�X�V���p�l��</param>
        /// <param name="argPanelFunction">�@�\�{�^���p�l��</param>
        /// <param name="argButtonClose">����{�^��</param>
        /// <param name="argButtonConfirm">�m�F�{�^��</param>
        /// <param name="argButtonCancel">�L�����Z���{�^��</param>
        /// <returns>�T�u��ʖ�</returns>
        //************************************************************************
        protected string SetOpeMode(Panel argPanelKeyItems, Panel argPanelSubItems,
            Panel argPanelUpdateInfo, Panel argPanelFunction,
            HtmlInputButton argButtonClose, Button argButtonConfirm, Button argButtonCancel)
        {
            // ���샂�[�h���擾
            OpeMode = Request.Params["mode"];

            string subName;
            //argButtonClose.Visible = false;

            // ���샂�[�h�ɉ������ݒ�
            switch (OpeMode)
            {
                case "Insert":
                    subName = "�V�K";
                    argPanelUpdateInfo.Visible = false;
                    argButtonConfirm.Attributes.Add("onclick",
                        string.Format("return confirm('{0}') && CheckInputEntry('{1}')",
                            CMMessageManager.GetMessage("QV001"), OpeMode));
                    break;
                case "Update":
                    subName = "�C��";
                    ProtectPanel(argPanelKeyItems);
                    argButtonConfirm.Attributes.Add("onclick",
                        string.Format("return confirm('{0}') && CheckInputEntry('{1}')",
                            CMMessageManager.GetMessage("QV001"), OpeMode));
                    break;
                case "Delete":
                    subName = "�폜�m�F";
                    ProtectPanel(argPanelKeyItems);
                    ProtectPanel(argPanelSubItems);
                    argButtonConfirm.Text = "�폜���s";
                    argButtonCancel.Text = "�L�����Z��";
                    argButtonConfirm.Attributes.Add("onclick",
                        string.Format("return confirm('{0}')", CMMessageManager.GetMessage("QV002")));
                    break;
                default:
                    subName = "�Q��";
                    //argButtonClose.Visible = true;
                    ProtectPanel(argPanelKeyItems);
                    ProtectPanel(argPanelSubItems);
                    argButtonConfirm.Enabled = false;
                    break;
            }

            return subName;
        }

        //************************************************************************
        /// <summary>
        /// ��ʕ\������
        /// </summary>
        /// <param name="argBody">body�^�O</param>
        /// <param name="argFacade">�g�p�t�@�T�[�h</param>
        //************************************************************************
        protected void OnPageOnLoad(HtmlGenericControl argBody,
            ICMBaseBL argFacade)
        {
            // �L�[���擾
            string paramKey = Request.Params["keys"];

            // �����\���̏ꍇ
            if (paramKey != null)
            {
                // �L�����Z���{�^���̖߂�l��������
                Session["cancelRet"] = false;

                // �p�����[�^�쐬
                List<CMSelectParam> param = CreateSelectParam(paramKey);

                try
                {
                    // �t�@�T�[�h�̌Ăяo��
                    DateTime operationTime;
                    CMMessage message;
                    DataSet result = argFacade.Select(param, CMSelectType.Edit, out operationTime, out message);

                    DataTable table = result.Tables[0];

                    bool found = table.Rows.Count > 0;
                    // �V�K�܂��͌������ʂ���̏ꍇ
                    if (OpeMode == "Insert" || found)
                    {
                        // �V�K�Ō������ʂȂ��̏ꍇ
                        if (!found)
                        {
                            // �f�t�H���g�̍s���쐬
                            DataRow newRow = table.NewRow();
                            // �V�K�s�Ƀf�t�H���g�l��ݒ肷��
                            SetDefaultValue(newRow);
                            // �V�K�s��ǉ�
                            table.Rows.Add(newRow);
                            // �X�V���m��
                            table.AcceptChanges();
                        }

                        // �������ʂ��擾
                        InputRow = table.Rows[0];

                        // �f�[�^�o�C���h���s
                        DataBind();

                        // �Z�b�V�����Ɍ������ʂ�ێ�
                        Session["inputRow"] = InputRow;

                        // ���엚�����o��
                        WriteOperationLog();
                    }
                    // �������ʂȂ��̏ꍇ
                    else
                    {
                        argBody.Attributes.Add("onload",
                            "alert('" + CMMessageManager.GetMessage("IV001") +
                            "'); window.returnValue = false; window.close()");
                    }
                }
                catch (Exception ex)
                {
                    ShowError(ex);
                    return;
                }
            }
            // �m�F��ʁA�߂�����ʂ̏ꍇ
            else
            {
                // �ҏW���ʂ��擾
                InputRow = (DataRow)Session["inputRow"];

                // ���ʃ��b�Z�[�W��\��
                string mes = (string)Session["retMessage"];
                if (mes != null && mes.Length > 0)
                {
                    ((dynamic)Master).ShowMessage("I", mes);
                    Session.Remove("retMessage");
                }

                // �f�[�^�o�C���h���s
                DataBind();
            }
        }
        
        //************************************************************************
        /// <summary>
        /// �o�^�{�^������������
        /// </summary>
        /// <param name="argBody">body�^�O</param>
        /// <param name="argFacade">�g�p�t�@�T�[�h</param>
        //************************************************************************
        protected void OnCommitClick(HtmlGenericControl argBody, ICMBaseBL argFacade)
        {
            // �Z�b�V��������f�[�^���擾
            InputRow = (DataRow) Session["inputRow"];
            // �o�^DataTable
            DataTable inputTable = InputRow.Table;

            // �V�K�A�C���̏ꍇ
            if (OpeMode == "Insert" || OpeMode == "Update")
            {
                // �f�[�^���X�V����Ă��Ȃ���΁A�A���[�g�\��
                if (!IsModified())
                {
                    ShowMessage("WV106");
                    return;
                }

                // ���̓f�[�^��ݒ�
                bool hasError = SetInputRow();

                // �Z�b�V�����ɕҏW���ʂ�ێ�
                Session["inputRow"] = InputRow;

                // �G���[���Ȃ���Γo�^���s
                if (hasError) return;

                // �V�K�m�F�̏ꍇ
                if (OpeMode == "Insert")
                {
                    DataSet ds = InputRow.Table.DataSet.Clone();
                    inputTable = ds.Tables[0];
                    DataRow row = inputTable.NewRow();
                    // �f�[�^�R�s�[
                    for (int i = 0; i < inputTable.Columns.Count; i++) row[i] = InputRow[i];
                    // �V�K�s�ǉ�
                    inputTable.Rows.Add(row);
                }
            }
            // �폜�m�F�̏ꍇ
            else
            {
                DataSet ds = InputRow.Table.DataSet.Copy();
                inputTable = ds.Tables[0];
                inputTable.Rows[0].Delete();
            }

            try
            {
                // �t�@�T�[�h�̌Ăяo��
                DateTime operationTime;
                argFacade.Update(inputTable.DataSet, out operationTime);

                // �V�K�A�C���̏ꍇ
                if (OpeMode == "Insert" || OpeMode == "Update")
                {
                    // �ύX���m��
                    InputRow.AcceptChanges();
                    // �Z�b�V�����ɕҏW���ʂ�ێ�
                    Session["inputRow"] = InputRow;
                    Session["retMessage"] = CMMessageManager.GetMessage("IV003");
                    Session["cancelRet"] = true;
                    // �V�K��ʂփ��_�C���N�g
                    //Response.Redirect(Request.Path + "?mode=" + OpeMode);
                }
                // �폜�m�F�̏ꍇ�A��ʂ����
                else Close(true);
            }
            catch (Exception ex)
            {
                ShowError(ex);
            }
        }

        //************************************************************************
        /// <summary>
        /// �L�����Z���{�^������������
        /// </summary>
        /// <param name="argBody">body�^�O</param>
        //************************************************************************
        protected void OnCancelClick(HtmlGenericControl argBody)
        {
            // �Z�b�V��������f�[�^���擾
            InputRow = (DataRow)Session["inputRow"];
            bool retVal = (bool)Session["cancelRet"];

            // �V�K�A�C���̏ꍇ
            if (OpeMode == "Insert" || OpeMode == "Update")
            {
                string msgcd = IsModified() ? "QV005" : "QV006";

                // �m�F��ʂ�\��
                argBody.Attributes.Add("onload",
                    string.Format("if (confirm('{0}')) {{window.returnValue = {1}; window.close()}}",
                        CMMessageManager.GetMessage(msgcd).Replace("\r\n", "\\n"), retVal.ToString().ToLower()));
            }
            else Close(retVal);
        }

        //************************************************************************
        /// <summary>
        /// �p�l���̃f�[�^���ύX����Ă��邩�`�F�b�N����B
        /// </summary>
        /// <param name="argPanel">�p�l��</param>
        /// <returns>True:�ύX����, False:�ύX�Ȃ�</returns>
        //************************************************************************
        protected bool IsPanelModified(Panel argPanel)
        {
            foreach (Control c in argPanel.Controls)
            {
                WebControl wc = c as WebControl;

                // �e�L�X�g�ƃh���b�v�_�E�����Ώ�
                if (!(wc is DropDownList) && !(wc is TextBox)) continue;

                // �l���r
                if (InputRow[wc.ID, DataRowVersion.Original].ToString() != GetValue(wc).ToString())
                    return true;
            }

            return false;
        }

        //************************************************************************
        /// <summary>
        /// �p�l���ɐݒ肳�ꂽ�l��InputRow�ɐݒ肷��B
        /// </summary>
        /// <param name="argPanel">�p�l��</param>
        //************************************************************************
        protected void SetPanelInputRow(Panel argPanel)
        {
            foreach (Control c in argPanel.Controls)
            {
                WebControl wc = c as WebControl;

                // �e�L�X�g�ƃh���b�v�_�E�����Ώ�
                if (!(wc is DropDownList) && !(wc is TextBox)) continue;

                // �l��ݒ�
                InputRow[wc.ID] = GetValue(wc);
            }
        }

        //************************************************************************
        /// <summary>
        /// InputRow���Ŏw���̒l���ύX����Ă��邩�`�F�b�N���A�ύX����Ă���ꍇ�A
        /// ���ږ����x���̕����F���I�����W�ɕύX����B
        /// </summary>
        /// <param name="argColname">��</param>
        /// <param name="argLabel">���ږ����x��</param>
        //************************************************************************
        protected void CheckSetModColor(string argColname, Label argLabel)
        {
            if (InputRow[argColname].ToString() != InputRow[argColname, DataRowVersion.Original].ToString())
                argLabel.Attributes.Add("class", "transp head2");
        }

        //************************************************************************
        /// <summary>
        /// �o�^�����̕�������擾����B
        /// </summary>
        /// <returns>�o�^�����̕�����</returns>
        //************************************************************************
        protected string GetAddInfo()
        {
            if (InputRow == null) return "";
            
            return string.Format("{0}�F{1:yyyy/MM/dd HH:mm:ss}&nbsp;</TD><TD>{2}",
                "�쐬����", InputRow["�쐬����"], InputRow["�쐬�Җ�"]);
        }

        //************************************************************************
        /// <summary>
        /// �X�V�����̕�������擾����B
        /// </summary>
        /// <returns>�X�V�����̕�����</returns>
        //************************************************************************
        protected string GetUpdateInfo()
        {
            if (InputRow == null) return "";
            
            return string.Format("{0}�F{1:yyyy/MM/dd HH:mm:ss}&nbsp;</TD><TD>{2}",
                "�X�V����", InputRow["�X�V����"], InputRow["�X�V�Җ�"]);
        }

        //************************************************************************
        /// <summary>
        /// �w���̎����������擾����B
        /// </summary>
        /// <param name="argCol">��</param>
        /// <returns>��������������</returns>
        //************************************************************************
        protected string GetHour(string argCol)
        {
            string s = InputRow[argCol].ToString();
            return s.Length < 2 ? "" : s.Substring(0, 2);  
        }

        //************************************************************************
        /// <summary>
        /// �w���̕��������擾����B
        /// </summary>
        /// <param name="argCol">��</param>
        /// <returns>������������</returns>
        //************************************************************************
        protected string GetMinute(string argCol)
        {
            string s = InputRow[argCol].ToString();
            return s.Length < 4 ? "" : s.Substring(2, 2);
        }

        //************************************************************************
        /// <summary>
        /// ������������擾����B
        /// </summary>
        /// <param name="argCol">��</param>
        /// <returns>������������</returns>
        //************************************************************************
        protected string GetTimeStr(string argCol)
        {
            // �V�K�A�C���̏ꍇ�A�����A���̓h���b�v�_�E�����X�g�ɕ\������
            if (OpeMode == "Insert" || OpeMode == "Update") return "�F";

            string s = InputRow[argCol].ToString();
            return s.Length < 4 ? "" : s.Substring(0, 2) + "�F" + s.Substring(2, 2);
        }
        #endregion

        #region �T�u�N���X�ŏ㏑�����郁�\�b�h
        //************************************************************************
        /// <summary>
        /// �L�[�f�[�^�����񂩂猟���p�����[�^���쐬����B
        /// </summary>
        /// <param name="argKey">�L�[�f�[�^������</param>
        /// <returns>�����p�����[�^</returns>
        //************************************************************************
        protected virtual List<CMSelectParam> CreateSelectParam(string argKey)
        {
            return new List<CMSelectParam>();
        }
        
        //************************************************************************
        /// <summary>
        /// �V�K�s�Ƀf�t�H���g�l��ݒ肷��B
        /// </summary>
        /// <param name="argRow">�f�t�H���g�l��ݒ肷��DataRow</param>
        //************************************************************************
        protected virtual void SetDefaultValue(DataRow argRow)
        {
        }

        //************************************************************************
        /// <summary>
        /// �f�[�^���ύX����Ă��邩�`�F�b�N����B
        /// </summary>
        /// <returns>True:�ύX����, False:�ύX�Ȃ�</returns>
        //************************************************************************
        protected virtual bool IsModified()
        {
            return false;
        }

        //************************************************************************
        /// <summary>
        /// InputRow�ɓ��̓f�[�^��ݒ肷��B
        /// </summary>
        /// <returns>True:�G���[����, False:�G���[�Ȃ�</returns>
        //************************************************************************
        protected virtual bool SetInputRow()
        {
            return false;
        }
        #endregion
    }
}
