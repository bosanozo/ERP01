/*******************************************************************************
 * �yERP�V�X�e���z
 *
 * �쐬��: ���i�e�N�m���W�[�^�c�� �]
 * ���ŗ���:
 * 2014.1.30, �V�K�쐬
 ******************************************************************************/
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

using NEXS.ERP.CM.Common;
using NEXS.ERP.CM.DA;

using Seasar.Quill.Attrs;

namespace NEXS.ERP.CM.BL
{
    //************************************************************************
    /// <summary>
    /// �t�@�T�[�h�w
    /// </summary>
    //************************************************************************
    [Implementation]
    public class XM010BL : CMBaseBL, ICMBaseBL
    {
        #region �C���W�F�N�V�����p�t�B�[���h
        protected XM010DA m_dataAccess;
        #endregion

        // �ŐV�o�[�W������������
        private const string VER_COND =
            "= (SELECT MAX(VER) FROM {0} WHERE VER <= @VER " +
            "AND ���ڈꗗID = A.���ڈꗗID{1})";

        #region �R���X�g���N�^
        //************************************************************************
        /// <summary>
        /// �R���X�g���N�^
        /// </summary>
        //************************************************************************
        public XM010BL()
        {
        }
        #endregion

        #region �t�@�T�[�h���\�b�h
        //************************************************************************
        /// <summary>
        /// �G���e�B�e�B��`����������B
        /// </summary>
        /// <param name="argParam">��������</param>
        /// <param name="argSelectType">�������</param>
        /// <param name="argOperationTime">���쎞��</param>
        /// <param name="argMessage">���ʃ��b�Z�[�W</param>
        /// <returns>��������</returns>
        //************************************************************************      
        public DataSet Select(List<CMSelectParam> argParam, CMSelectType argSelectType,
            out DateTime argOperationTime, out CMMessage argMessage)
        {
            // ���쎞����ݒ�
            argOperationTime = DateTime.Now;

            // �ő匟�������擾
            CommonDA.Connection = Connection;
            int maxRow = CommonDA.GetMaxRow();

            string[] fnames = { "XMEM���ڈꗗ" };

            // �o�^��ʂ̏ꍇ
            if (argSelectType == CMSelectType.Edit)
            {
                // �����e�[�u��
                fnames = new string[] { "XMEM���ڈꗗ", "XMEM�����e�[�u��", "XMEM����" };

                // VER�̏�����ύX
                argParam[1].tableName = fnames[0];

                // �q�e�[�u����VER�̏�����ǉ�
                for (int i = 1; i <= 2; i++)
                {
                    CMSelectParam param2 = new CMSelectParam("VER",
                        string.Format(VER_COND,
                        i == 1 ? "XM�����e�[�u��" : "XM����",
                        i == 1 ? " AND �e�[�u���� = A.�e�[�u����" : " AND ���ږ� = A.���ږ�"),
                        argParam[1].paramFrom);
                    param2.tableName = fnames[i];
                    argParam.Add(param2);
                }
            }�@
            else
            {
                // �ŐVVER�̂ݕ\��
                var p1 = argParam.Where(p => p.name == "�ŐV�ł̂�");
                if (p1.Count() > 0)
                {
                    var param = p1.First();
                    if (param.paramFrom.ToString() == "true")
                    {
                        param.name = "VER";
                        param.condtion = "= (SELECT MAX(VER) FROM XM���ڈꗗ WHERE ���ڈꗗID = A.���ڈꗗID)";
                        param.paramFrom = null;
                    }
                }
            }

            // �������s
            bool isOver;
            m_dataAccess.Connection = Connection;
            DataSet result = m_dataAccess.SelectFromXml(argParam, argSelectType,
                maxRow, out isOver, fnames);

            argMessage = null;
            // �������ʂȂ�
            if (result.Tables[0].Rows.Count == 0) argMessage = new CMMessage("IV001");
            // �ő匟�������I�[�o�[
            else if (isOver) argMessage = new CMMessage("IV002");

            return result;
        }

        //************************************************************************
        /// <summary>
        /// �G���e�B�e�B��`�Ƀf�[�^��o�^����B
        /// </summary>
        /// <param name="argUpdateData">�X�V�f�[�^</param>
        /// <param name="argOperationTime">���쎞��</param>
        /// <returns>�o�^�������R�[�h��</returns>
        //************************************************************************
        [CMTransactionAttribute(Timeout=100)]
        public virtual int Update(DataSet argUpdateData, out DateTime argOperationTime)
        {
            // ���쎞����ݒ�
            CommonDA.Connection = Connection;
            argOperationTime = CommonDA.GetSysdate();

            CMUserInfo uinfo = CMInformationManager.UserInfo;

            // ���͒l�`�F�b�N
            CommonDA.ExistCheckFomXml(argUpdateData.Tables[0]);

            // �f�[�^�A�N�Z�X�w�ɃR�l�N�V�����ݒ�
            m_dataAccess.Connection = Connection;

            // �o�^���s
            int cnt = m_dataAccess.Update(argUpdateData, argOperationTime);

            return cnt;
        }

        //************************************************************************
        /// <summary>
        /// �q�e�[�u���̍폜�f�[�^��ǉ�����B
        /// </summary>
        /// <param name="argDataSet">�폜�f�[�^�ǉ��Ώ�DataSet</param>
        //************************************************************************
        public void AddChildDelRow(DataSet argDataSet)
        {
            foreach (DataRow prow in argDataSet.Tables[0].Rows)
            {
                if (prow["�폜"].ToString() == "1")
                {
                    // ���������ݒ�
                    List<CMSelectParam> paramList = new List<CMSelectParam>();
                    paramList.Add(new CMSelectParam("���ڈꗗID", "= @���ڈꗗID", prow["���ڈꗗID"]));
                    paramList.Add(new CMSelectParam("VER", "= @VER", prow["VER"]));

                    m_dataAccess.Connection = Connection;

                    // �������s
                    bool isOver;
                    DataSet result = m_dataAccess.SelectFromXml(paramList, CMSelectType.Edit,
                        0, out isOver, "XMEM�����e�[�u��", "XMEM����");

                    // �폜�s����荞��
                    foreach (DataTable dt in result.Tables)
                    {
                        if (!argDataSet.Tables.Contains(dt.TableName)) argDataSet.Tables.Add(dt.Clone());

                        foreach (DataRow row in dt.Rows)
                        {
                            row["�폜"] = "1";
                            argDataSet.Tables[dt.TableName].ImportRow(row);
                        }
                    }
                }
            }
        }

        //************************************************************************
        /// <summary>
        /// �폜�s�̑SVER�̍폜�f�[�^��ǉ�����B
        /// </summary>
        /// <param name="argDataSet">�폜�f�[�^�ǉ��Ώ�DataSet</param>
        //************************************************************************
        public void AddDelRow(DataSet argDataSet)
        {
            m_dataAccess.Connection = Connection;

            foreach (DataTable dt in argDataSet.Tables)
            {
                foreach (var delRow in dt.Select("�폜 = '1'"))
                {
                    // ���������ݒ�
                    List<CMSelectParam> paramList = new List<CMSelectParam>();
                    paramList.Add(new CMSelectParam("���ڈꗗID", "= @���ڈꗗID", delRow["���ڈꗗID"]));
                    paramList.Add(new CMSelectParam("VER", "!= @VER", delRow["VER"]));
                    if (dt.TableName == "XMEM�����e�[�u��" || dt.TableName == "XMEM����")
                    {
                        string key2 = dt.TableName == "XMEM�����e�[�u��" ? "�e�[�u����" : "���ږ�";
                        paramList.Add(new CMSelectParam(key2, "= @" + key2, delRow[key2]));
                    }

                    // �������s
                    bool isOver;
                    DataSet result = m_dataAccess.SelectFromXml(paramList, CMSelectType.Edit,
                        0, out isOver, dt.TableName);

                    // �폜�s����荞��
                    foreach (DataRow row in result.Tables[0].Rows)
                    {
                        row["�폜"] = "1";
                        dt.ImportRow(row);
                    }
                }
            }
        }
        #endregion
    }
}
