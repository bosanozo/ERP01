/*******************************************************************************
 * �y���ʕ��i�z
 *
 * �쐬��: ���i�e�N�m���W�[�^�c�� �]
 * ���ŗ���:
 * 2014.1.30, �V�K�쐬
 ******************************************************************************/
using System;
using System.Collections.Generic;
using System.Text;

namespace NEXS.ERP.CM.Common
{
    //************************************************************************
    /// <summary>
    /// ���ʌ����Ăяo���p�����f�[�^
    /// </summary>
    //************************************************************************
    [Serializable]
    public class CMCommonSelectArgs
    {
        #region �v���p�e�B
        /// <summary>����ID</summary>
        public string SelectId { get; set; }

        /// <summary>�p�����[�^</summary>
        public object[] Params { get; set; }
        #endregion

        #region �R���X�g���N�^
        //************************************************************************
        /// <summary>
        /// �R���X�g���N�^
        /// </summary>
        /// <param name="argSelectId">����ID</param>
        /// <param name="argParams">�p�����[�^</param>
        //************************************************************************
        public CMCommonSelectArgs(string argSelectId, params object[] argParams)
        {
            SelectId = argSelectId;
            Params = argParams;
        }
        #endregion
    }
}
