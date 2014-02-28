/*******************************************************************************
 * �y���ʕ��i�z
 *
 * �쐬��: ���i�e�N�m���W�[�^�c�� �]
 * ���ŗ���:
 * 2014.1.30, �V�K�쐬
 ******************************************************************************/
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;

using System.Data.SqlClient;

using NEXS.ERP.CM.Common;

namespace NEXS.ERP.CM.DA
{
    //************************************************************************
    /// <summary>
    /// DB���ڎ��
    /// </summary>
    //************************************************************************
    public enum CMDbType
    {
        �R�[�h, �R�[�h_��, �敪, ������, ���z, ����, ����, �t���O, ���t, ����
    }

    //************************************************************************
    /// <summary>
    /// Command�p�����[�^
    /// </summary>
    //************************************************************************
    public class CMCmdParam
    {
        /// <summary>�p�����[�^��</summary>
        [Category("���ʕ��i")]
        [Description("�p�����[�^��")]
        public string Name { get; set; }

        /// <summary>DataTable�̗�(�p�����[�^���ƈقȂ�ꍇ�Ɏw��)</summary>
        [Category("���ʕ��i")]
        [Description("DataTable�̗�(�p�����[�^���ƈقȂ�ꍇ�Ɏw��)")]
        [DefaultValue(null)]
        public string SourceColumn { get; set; }

        /// <summary>DB���ڂ̌^</summary>
        [Category("���ʕ��i")]
        [Description("DB���ڂ̌^")]
        [DefaultValue(CMDbType.�R�[�h)]
        public CMDbType DbType { get; set; }

        /// <summary>�L�[���ڃt���O</summary>
        [Category("���ʕ��i")]
        [Description("�L�[���ڃt���O")]
        [DefaultValue(false)]
        public bool IsKey { get; set; }

        #region �R���X�g���N�^
        //************************************************************************
        /// <summary>
        /// �R���X�g���N�^
        /// </summary>
        //************************************************************************
        public CMCmdParam()
        {
            DbType = CMDbType.�R�[�h;
        }
        #endregion

        //************************************************************************
        /// <summary>
        /// SqlDbType��Ԃ��B
        /// </summary>
        /// <returns>SqlDbType</returns>
        //************************************************************************
        public SqlDbType GetDbType()
        {
            switch (DbType)
            {
                case CMDbType.�R�[�h_��:
                    return SqlDbType.VarChar;
                case CMDbType.������:
                    return SqlDbType.NVarChar;
                case CMDbType.���z:
                    return SqlDbType.Money;
                case CMDbType.����:
                    return SqlDbType.Int;
                case CMDbType.����:
                    return SqlDbType.Decimal;
                case CMDbType.�t���O:
                    return SqlDbType.Bit;
                case CMDbType.���t:
                    return SqlDbType.Date;
                case CMDbType.����:
                    return SqlDbType.DateTime;
                default:
                    return SqlDbType.Char;
            }
        }
    }

    //************************************************************************
    /// <summary>
    /// SqlCommand�ݒ�
    /// </summary>
    //************************************************************************
    public class CMCmdSetting
    {
        /// <summary>�e�[�u����</summary>
        [Category("���ʕ��i")]
        [Description("�e�[�u����")]
        public string Name { get; set; }

        /// <summary>SqlCommand�p�����[�^�z��</summary>
        [Category("���ʕ��i")]
        [Description("SqlCommand�p�����[�^�z��")]
        public CMCmdParam[] ColumnParams { get; set; }

        //************************************************************************
        /// <summary>
        /// ��L�[�̌���������Ԃ��B
        /// </summary>
        /// <returns>��L�[�̌�������</returns>
        //************************************************************************
        public string GetKeyCondition()
        {
            StringBuilder builder = new StringBuilder();
            // �e�[�u�����ڗ�Ń��[�v
            foreach (var row in ColumnParams)
            {
                if (row.IsKey)
                {
                    if (builder.Length > 0) builder.Append(" AND ");
                    builder.AppendFormat("{0} = @{0}", row.Name);
                }
            }

            return builder.ToString();
        }    
    }
}
