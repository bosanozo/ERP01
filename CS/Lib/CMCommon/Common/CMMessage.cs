/*******************************************************************************
 * �y���ʕ��i�z
 *
 * �쐬��: ���i�e�N�m���W�[�^�c�� �]
 * ���ŗ���:
 * 2014.1.30, �V�K�쐬
 ******************************************************************************/
using System;

namespace NEXS.ERP.CM.Common
{
    //************************************************************************
    /// <summary>
    /// ���b�Z�[�W�f�[�^
    /// </summary>
    //************************************************************************
    [Serializable]
    public class CMMessage
    {
        #region �v���p�e�B
        /// <summary>
        /// ���b�Z�[�W�R�[�h
        /// </summary>
        public string MessageCd { get; set; }

        /// <summary>
        /// �f�[�^�e�[�u�����A�s�ԍ��A�t�B�[���h�f�[�^
        /// </summary>
        public CMRowField RowField { get; set; }

        /// <summary>
        /// �p�����[�^
        /// </summary>
        public object[] Params { get; set; }
        #endregion

        #region �R���X�g���N�^
        //************************************************************************
        /// <summary>
        /// �R���X�g���N�^
        /// ���b�Z�[�W�f�[�^�𐶐�����B
        /// </summary>
        /// <param name="argMsgCode">���b�Z�[�W�R�[�h</param>
        /// <param name="argParams">�p�����[�^</param>
        //************************************************************************
        public CMMessage(string argMsgCode, params object[] argParams)
        {
            MessageCd = argMsgCode;
            Params = argParams;
        }

        //************************************************************************
        /// <summary>
        /// �R���X�g���N�^�i�s�ԍ��w��j
        /// ���b�Z�[�W�f�[�^�𐶐�����B
        /// </summary>
        /// <param name="argMsgCode">���b�Z�[�W�R�[�h</param>
        /// <param name="argRowField">�s�ԍ�</param>
        /// <param name="argParams">�p�����[�^</param>
        //************************************************************************
        public CMMessage(string argMsgCode, CMRowField argRowField,
            params object[] argParams) : this(argMsgCode, argParams)
        {
            RowField = argRowField;
        }
        #endregion

        //************************************************************************
        /// <summary>
        /// ���b�Z�[�W�������Ԃ��B
        /// </summary>
        //************************************************************************
        public override string ToString()
        {
            return CMMessageManager.GetMessage(MessageCd, Params);
        }
    }
}
