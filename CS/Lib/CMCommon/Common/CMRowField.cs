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
    /// �f�[�^�e�[�u�����A�s�ԍ��A�t�B�[���h�f�[�^
    /// </summary>
    //************************************************************************
    [Serializable]
    public class CMRowField
    {
        #region �v���p�e�B
        /// <summary>
        /// �f�[�^�e�[�u����
        /// </summary>
        public string DataTableName { get; set; }

        /// <summary>
        /// �s�ԍ�
        /// </summary>
        public int RowNumber { get; set; }

        /// <summary>
        /// �t�B�[���h��
        /// </summary>
        public string FieldName { get; set; }
        #endregion

        #region �R���X�g���N�^
        //************************************************************************
        /// <summary>
        /// �R���X�g���N�^
        /// </summary>
        /// <param name="argRowNumber">�s�ԍ�</param>
        //************************************************************************
        public CMRowField(int argRowNumber) : this(argRowNumber, null) {}

        //************************************************************************
        /// <summary>
        /// �R���X�g���N�^
        /// </summary>
        /// <param name="argRowNumber">�s�ԍ�</param>
        /// <param name="argFieldName">�t�B�[���h��</param>
        //************************************************************************
        public CMRowField(int argRowNumber, string argFieldName)
            : this (null, argRowNumber, argFieldName) {}

        //************************************************************************
        /// <summary>
        /// �R���X�g���N�^
        /// </summary>
        /// <param name="argDataTableName">�f�[�^�e�[�u����</param>
        /// <param name="argRowNumber">�s�ԍ�</param>
        //************************************************************************
        public CMRowField(string argDataTableName, int argRowNumber)
            : this (argDataTableName, argRowNumber, null) {}

        //************************************************************************
        /// <summary>
        /// �R���X�g���N�^
        /// </summary>
        /// <param name="argDataTableName">�f�[�^�e�[�u����</param>
        /// <param name="argRowNumber">�s�ԍ�</param>
        /// <param name="argFieldName">�t�B�[���h��</param>
        //************************************************************************
        public CMRowField(string argDataTableName, int argRowNumber, string argFieldName)
        {            
            DataTableName = argDataTableName;
            RowNumber = argRowNumber;
            FieldName = argFieldName;
        }
        #endregion
    }
}
