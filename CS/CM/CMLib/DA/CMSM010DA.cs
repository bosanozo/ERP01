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

using Seasar.Quill.Attrs;

using NEXS.ERP.CM.Common;

namespace NEXS.ERP.CM.DA
{
    //************************************************************************
    /// <summary>
    /// �f�[�^�A�N�Z�X�w
    /// </summary>
    //************************************************************************
    [Implementation]
    public class CMSM010DA : CMBaseDA
    {
        #region SQL��
        /// <summary>
        /// �����A�o�^�Ώۂ̃e�[�u����
        /// </summary>
        private const string TABLE_NAME = "CMSM�g�D";

        /// <summary>
        /// ������
        /// </summary>
        private const string SELECT_COLS =
            "A.�g�DCD," +
            "A.�g�D��," +
            "A.�g�D�K�w�敪," +
            "H1.��l�� �g�D�K�w�敪��," +
            "A.��ʑg�DCD," +
            "S2.�g�D�� ��ʑg�D��,";

        /// <summary>
        /// ���̎擾�p�O������
        /// </summary>
        private const string LEFT_JOIN =
            "LEFT JOIN CMSM�g�D S2 ON S2.�g�DCD = A.��ʑg�DCD " +
            "LEFT JOIN CMSM�ėp��l H1 ON H1.����CD = 'M001' AND H1.��lCD = A.�g�D�K�w�敪 ";

        /// <summary>
        /// �������̕��я�
        /// </summary>
        private const string ORDER = "�g�DCD";
        #endregion

        #region �R���X�g���N�^
        //************************************************************************
        /// <summary>
        /// �R���X�g���N�^
        /// </summary>
        //************************************************************************
        public CMSM010DA()
        {
        }
        #endregion

        #region �f�[�^�A�N�Z�X���\�b�h
        //************************************************************************
        /// <summary>
        /// �g�D�}�X�^����������B
        /// </summary>
        /// <param name="argParam">��������</param>
        /// <param name="argSelectType">�������</param>
        /// <param name="argMaxRow">�ő匟������</param>
        /// <param name="argIsOver">�ő匟�������I�[�o�[�t���O</param>
        /// <returns>��������</returns>
        //************************************************************************
        public DataSet Select(List<CMSelectParam> argParam, CMSelectType argSelectType,
            int argMaxRow, out bool argIsOver)
        {
            /*
            // �g�D�K�w���S�ЂłȂ���΁A��Ђ̏�����ǉ�
            CMUserInfo uinfo = CMInformationManager.UserInfo;
            if (uinfo.SoshikiKaisoKbn != CMSoshikiKaiso.ALL)
                argParam.Add(new CMSelectParam("���CD", "= :���CD", uinfo.KaishaCd));*/

            // WHERE��쐬
            StringBuilder where = new StringBuilder();
            AddWhere(where, argParam);

            // SELECT���̐ݒ�
            IDbCommand cmd = CreateCommand(
                CreateSelectSql(SELECT_COLS, TABLE_NAME, where.ToString(), LEFT_JOIN, ORDER, argSelectType));
            Adapter.SelectCommand = cmd;

            // �p�����[�^�̐ݒ�
            SetParameter(cmd, argParam);
            // �ꗗ�����̏ꍇ
            if (argSelectType == CMSelectType.List)
                cmd.Parameters.Add(CreateCmdParam("�ő匟������", argMaxRow));

            // �f�[�^�Z�b�g�̍쐬
            DataSet ds = new DataSet();
            // �f�[�^�̎擾
            int cnt = Adapter.Fill(ds);
            // �e�[�u������ݒ�
            ds.Tables[0].TableName = TABLE_NAME;

            // �ꗗ�����ōő匟�������I�[�o�[�̏ꍇ�A�ŏI�s���폜
            if (argSelectType == CMSelectType.List && cnt >= argMaxRow)
            {
                argIsOver = true;
                ds.Tables[0].Rows.RemoveAt(cnt - 1);
            }
            else argIsOver = false;

            // �������ʂ̕ԋp
            return ds;
        }
        #endregion
    }
}
