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
    public class CMSM010BL : CMBaseBL, ICMBaseBL
    {
        #region �C���W�F�N�V�����p�t�B�[���h
        protected CMSM010DA m_dataAccess;
        #endregion

        #region �R���X�g���N�^
        //************************************************************************
        /// <summary>
        /// �R���X�g���N�^
        /// </summary>
        //************************************************************************
        public CMSM010BL()
        {
        }
        #endregion

        #region �t�@�T�[�h���\�b�h
        //************************************************************************
        /// <summary>
        /// �g�D�}�X�^����������B
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

            // �������s
            bool isOver;
            m_dataAccess.Connection = Connection;
            DataSet result = m_dataAccess.SelectFromXml(argParam, argSelectType,
                maxRow, out isOver, "CMSM�g�D");

            argMessage = null;
            // �������ʂȂ�
            if (result.Tables[0].Rows.Count == 0) argMessage = new CMMessage("IV001");
            // �ő匟�������I�[�o�[
            else if (isOver) argMessage = new CMMessage("IV002");

            return result;
        }

        //************************************************************************
        /// <summary>
        /// �g�D�}�X�^�Ƀf�[�^��o�^����B
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

            //throw new Exception("aaa");

            return cnt;
        }

        //************************************************************************
        /// <summary>
        /// �g�D�}�X�^�Ƀf�[�^���A�b�v���[�h����B
        /// </summary>
        /// <param name="argUpdateData">�X�V�f�[�^</param>
        /// <param name="argOperationTime">���쎞��</param>
        /// <returns>�o�^�������R�[�h��</returns>
        //************************************************************************
        [CMTransactionAttribute(Timeout = 100)]
        public virtual int Upload(DataSet argUpdateData, out DateTime argOperationTime)
        {
            // ���쎞����ݒ�
            CommonDA.Connection = Connection;
            argOperationTime = CommonDA.GetSysdate();

            // ���͒l�`�F�b�N
            CommonDA.ExistCheckFomXml(argUpdateData.Tables[0]);

            // �f�[�^�A�N�Z�X�w�ɃR�l�N�V�����ݒ�
            m_dataAccess.Connection = Connection;

            // �o�^���s
            int cnt = m_dataAccess.Upload(argUpdateData, argOperationTime);

            return cnt;
        }
        #endregion
    }
}
