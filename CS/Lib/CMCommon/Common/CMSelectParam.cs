/*******************************************************************************
 * �y���ʕ��i�z
 *
 * �쐬��: ���i�e�N�m���W�[�^�c�� �]
 * ���ŗ���:
 * 2014.1.30, �V�K�쐬
 ******************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NEXS.ERP.CM.Common
{
    //************************************************************************
    /// <summary>
    /// ���������N���X
    /// </summary>
    //************************************************************************
    [Serializable]
    public class CMSelectParam
    {
        /// <summary>���ږ�</summary>
        public string name;
        /// <summary>��������SQL</summary>
        public string condtion;
        /// <summary>�v���[�X�t�H���_�ɐݒ肷��From�l</summary>
        public object paramFrom;
        /// <summary>�v���[�X�t�H���_�ɐݒ肷��To�l</summary>
        public object paramTo;
        /// <summary>���Ӎ��ږ�(leftcol = @name)</summary>
        public string leftCol;
        /// <summary>����������ǉ�����e�[�u����</summary>
        /// <remarks>���w��̏ꍇ�͑S�e�[�u���̌����ɏ�����ǉ�����B</remarks>
        public string tableName;

        //************************************************************************
        /// <summary>
        /// �R���X�g���N�^
        /// </summary>
        /// <param name="argName">���ږ�</param>
        /// <param name="argCondtion">��������SQL</param>
        /// <param name="argValue">�v���[�X�t�H���_�ɐݒ肷��l</param>
        //************************************************************************
        public CMSelectParam(string argName, string argCondtion, object argValue)
            : this(argName, argCondtion, argValue, null) { }

        //************************************************************************
        /// <summary>
        /// �R���X�g���N�^
        /// </summary>
        /// <param name="argName">���ږ�</param>
        /// <param name="argCondtion">��������SQL</param>
        /// <param name="argFrom">�v���[�X�t�H���_�ɐݒ肷��From�l</param>
        /// <param name="argTo">�v���[�X�t�H���_�ɐݒ肷��To�l</param>
        //************************************************************************
        public CMSelectParam(string argName, string argCondtion, object argFrom, object argTo)
            : this(null, argName, argCondtion, argFrom, argTo) { }

        //************************************************************************
        /// <summary>
        /// �R���X�g���N�^
        /// </summary>
        /// <param name="argLeftCol">���Ӎ��ږ�</param>
        /// <param name="argRightCol">�E�ύ��ږ�</param>
        /// <param name="argCondtion">��������SQL</param>
        /// <param name="argFrom">�v���[�X�t�H���_�ɐݒ肷��From�l</param>
        /// <param name="argTo">�v���[�X�t�H���_�ɐݒ肷��To�l</param>
        //************************************************************************
        public CMSelectParam(string argLeftCol, string argRightCol,
            string argCondtion, object argFrom, object argTo)
        {
            leftCol = argLeftCol;
            name = argRightCol;
            condtion = argCondtion;
            paramFrom = argFrom;
            paramTo = argTo;
        }
    }
}
