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
using System.Linq;
using System.Text;

using Seasar.Quill.Attrs;

namespace NEXS.ERP.CM.DA
{
    //************************************************************************
    /// <summary>
    /// ���[�U�[��񌟍��f�[�^�A�N�Z�X�w
    /// </summary>
    //************************************************************************
    [Implementation]
    public class CMUserInfoDA : CMBaseDA
    {
        #region SQL��
        /// <summary>
        /// SELECT��
        /// </summary>
        private const string SELECT_SQL =
            "SELECT " +
            "U.���[�UID ID," +
            "U.���[�U�� NAME," +
            "U.�p�X���[�h PASSWD," +
            "'' ROLE," +
            "U.�g�DCD," +
            "S1.�g�D��," +
            "S1.�g�D�K�w�敪 " +
            "FROM CMSM���[�U U " +
            "JOIN CMSM�g�D S1 ON S1.�g�DCD = U.�g�DCD " +
            "WHERE U.���[�UID = @ID";

        /// <summary>
        /// ���[��SELECT��
        /// </summary>
        private const string SELECT_ROLE_SQL =
            "SELECT " +
            "���[��ID ROLE " +
            "FROM CMSM���[�U���[�� " +
            "WHERE ���[�UID = @ID " +
            "ORDER BY ���[��ID";
        #endregion

        #region �R���X�g���N�^
        //************************************************************************
        /// <summary>
        /// �R���X�g���N�^
        /// </summary>
        //************************************************************************
        public CMUserInfoDA()
        {
        }
        #endregion

        #region �f�[�^�A�N�Z�X���\�b�h
        //************************************************************************
        /// <summary>
        /// ���[�U�[�����擾����B
        /// </summary>
        /// <param name="argUserId">���[�U�[ID</param>
        /// <returns>���[�U�[���DataRow</returns>
        //************************************************************************
        public DataRow FindById(string argUserId)
        {
            // SelectCommand�̐ݒ�
            Adapter.SelectCommand = CreateCommand(SELECT_SQL);
            // �p�����[�^�̐ݒ�
            Adapter.SelectCommand.Parameters.Add(CreateCmdParam("ID", argUserId));

            // �f�[�^�Z�b�g�̍쐬
            DataSet ds = new DataSet();
            // �f�[�^�̎擾
            int cnt = Adapter.Fill(ds);

            // �������ʂȂ�
            if (cnt == 0) return null;

            // ���[��������
            Adapter.SelectCommand.CommandText = SELECT_ROLE_SQL;
            // �f�[�^�e�[�u���̍쐬
            // �f�[�^�Z�b�g�̍쐬
            DataSet roleDs = new DataSet();
            // �f�[�^�̎擾
            Adapter.Fill(roleDs);

            // ���[����,��؂�Ō���
            StringBuilder sb = new StringBuilder();
            foreach (DataRow row in roleDs.Tables[0].Rows)
            {
                if (sb.Length > 0) sb.Append(',');
                sb.Append(row["ROLE"].ToString());
            }
            ds.Tables[0].Rows[0]["ROLE"] = sb.ToString();

            // �������ʂ̕ԋp
            return ds.Tables[0].Rows[0];
        }
        #endregion
    }
}
