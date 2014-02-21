/*******************************************************************************
 * �y���ʕ��i�z
 *
 * �쐬��: ���i�e�N�m���W�[�^�c�� �]
 * ���ŗ���:
 * 2014.1.30, �V�K�쐬
 ******************************************************************************/
using System;
using System.Data;
using System.IO;

namespace NEXS.ERP.CM.Common
{
    //************************************************************************
    /// <summary>
    /// ���b�Z�[�W�\�[�X
    /// </summary>
    //************************************************************************
    public class CMMessageManager
    {
        private static CMMessageDataSet.MessageDataTable s_messageTable;

        /// <summary>
        /// ���b�Z�[�W�t�@�C��
        /// </summary>
        public static string MessageFileDir { get; set; }

        //************************************************************************
        /// <summary>
        /// ���b�Z�[�W��`�������Ԃ��B
        /// </summary>
        /// <param name="argMessageCode">���b�Z�[�W�R�[�h</param>
        /// <param name="argParams">�p�����[�^</param>
        /// <returns>���b�Z�[�W</returns>
        //************************************************************************
        public static string GetMessage(string argMessageCode, params object[] argParams)
        {
            // ���b�Z�[�W��`�̓ǂݍ���
            if (s_messageTable == null)
            {
                s_messageTable = new CMMessageDataSet.MessageDataTable();

                string[] files = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory +
                    Path.DirectorySeparatorChar + MessageFileDir, "Message*.xml");
                foreach (string file in files) s_messageTable.ReadXml(file);
            }

            // ���b�Z�[�W��`�̎擾
            DataRow[] rows = s_messageTable.Select("Code = '" + argMessageCode + "'");
            if (rows.Length == 0) throw new Exception("Message.xml��\"" + argMessageCode + "\"���o�^����Ă��܂���B");
            return string.Format(rows[0]["Format"].ToString(), argParams);
        }
    }
}
    