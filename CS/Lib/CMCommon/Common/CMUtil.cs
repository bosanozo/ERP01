/*******************************************************************************
 * �y���ʕ��i�z
 *
 * �쐬��: ���i�e�N�m���W�[�^�c�� �]
 * ���ŗ���:
 * 2014.1.30, �V�K�쐬
 ******************************************************************************/
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Text.RegularExpressions;

namespace NEXS.ERP.CM.Common
{
    //************************************************************************
    /// <summary>
    /// ���[�e�B���e�B�N���X
    /// </summary>
    //************************************************************************
    public static class CMUtil
    {
        //************************************************************************
        /// <summary>
        /// DataRow����s�ԍ����擾����B
        /// </summary>
        /// <param name="argDataRow">DataRow</param>
        /// <returns>�s�ԍ�(�s�ԍ��Ȃ��̏ꍇ��0��Ԃ��B)</returns>
        //************************************************************************
        public static int GetRowNumber(DataRow argDataRow)
        {
            int rowNumber = 0;
            // �s�ԍ����擾
            if (argDataRow.Table.Columns.Contains("ROWNUMBER"))
            {
                DataRowVersion version =
                    argDataRow.RowState == DataRowState.Deleted ? DataRowVersion.Original : DataRowVersion.Current;
                rowNumber = Convert.ToInt32(argDataRow["ROWNUMBER", version]);
            }

            return rowNumber;
        }

        //************************************************************************
        /// <summary>
        /// ���l�E�����E�L������ȏ�g�p���Ă��邩�`�F�b�N����B
        /// </summary>
        /// <param name="argPassword">�p�X���[�h</param>
        /// <returns>True:����菭�Ȃ�</returns>
        //************************************************************************
        public static bool CheckPassword(string argPassword)
        {
            int cnt = 0;
            if (Regex.IsMatch(argPassword, "[0-9]")) cnt++;
            if (Regex.IsMatch(argPassword, "[a-zA-Z]")) cnt++;
            if (Regex.IsMatch(argPassword, "[^a-zA-Z0-9]")) cnt++;

            return cnt < 2;
        }
    }
}
