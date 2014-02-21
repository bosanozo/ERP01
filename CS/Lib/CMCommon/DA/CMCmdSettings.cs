/*******************************************************************************
 * �y���ʕ��i�z
 *
 * �쐬��: ���i�e�N�m���W�[�^�c�� �]
 * ���ŗ���:
 * 2014.1.30, �V�K�쐬
 ******************************************************************************/
using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;

namespace NEXS.ERP.CM.DA
{
    //************************************************************************
    /// <summary>
    /// SqlCommand�ݒ�R���N�V�����N���X
    /// </summary>
    //************************************************************************
    public class CMCmdSettings : Component
    {
        #region �v���p�e�B
        [Category("���ʕ��i")]
        [Description("SqlCommand�ݒ�̃R���N�V����")]
        public List<CMCmdSetting> CmdSettings { get; set; }
        #endregion

        #region �C���f�N�T
        /// <summary>�w��Index��SqlCommand�ݒ��Ԃ��܂��B</summary>
        public CMCmdSetting this[int argIndex]
        {
            get
            {
                return CmdSettings[argIndex];
            }
        }

        /// <summary>�w��̃e�[�u������SqlCommand�ݒ��Ԃ��܂��B</summary>
        public CMCmdSetting this[string argName]
        {
            get
            {
                var result = from row in CmdSettings
                             where row.Name == argName
                             select row;
                return result.First();
            }
        }
        #endregion

        #region �R���X�g���N�^
        //************************************************************************
        /// <summary>
        /// �R���X�g���N�^
        /// </summary>
        //************************************************************************
        public CMCmdSettings()
        {
            CmdSettings = new List<CMCmdSetting>();
        }

        //************************************************************************
        /// <summary>
        /// �R���X�g���N�^
        /// </summary>
        /// <param name="argFname">�ǂݍ���XML�t�@�C����(�g���q�Ȃ�)</param>
        //************************************************************************
        public CMCmdSettings(string argFname) : this()
        {
            AddFomXml(argFname);
        }
        #endregion

        //************************************************************************
        /// <summary>
        /// SqlCommand�ݒ��XML�t�@�C������ǉ�����B
        /// </summary>
        /// <param name="argFnames">�ǂݍ���XML�t�@�C����(�g���q�Ȃ�)</param>
        //************************************************************************
        public void AddFomXml(params string[] argFnames)
        {
            foreach (string fname in argFnames)
            {
                // �f�[�^�Z�b�g�Ƀt�@�C����ǂݍ���
                DataSet ds = new DataSet();
                ds.ReadXml(Path.Combine(HttpContext.Current.Request.PhysicalApplicationPath, "Model", fname + ".xml"));

                CMCmdSetting cmdSetting = new CMCmdSetting();

                // �e�[�u������ݒ�
                cmdSetting.Name = ds.Tables["�G���e�B�e�B"].Rows[0]["�e�[�u����"].ToString();

                bool hasNotUpdate = ds.Tables["����"].Columns.Contains("�X�V�ΏۊO");

                // �p�����[�^�ݒ�
                List<CMCmdParam> paramList = new List<CMCmdParam>();
                foreach (DataRow row in ds.Tables["����"].Rows)
                {
                    // �X�V�ΏۊO�͖���
                    if (hasNotUpdate && row["�X�V�ΏۊO"].ToString() == "True") continue;

                    CMCmdParam cmdParam = new CMCmdParam();
                    cmdParam.Name = row["���ږ�"].ToString();
                    cmdParam.DbType = (CMDbType)Enum.Parse(typeof(CMDbType), row["���ڌ^"].ToString());
                    cmdParam.IsKey = row["Key"].ToString() == "True";
                    if (row.Table.Columns.Contains("SourceColumn"))
                        cmdParam.SourceColumn = row["SourceColumn"].ToString();

                    paramList.Add(cmdParam);
                }
                cmdSetting.ColumnParams = paramList.ToArray();

                // �ݒ��ǉ�
                CmdSettings.Add(cmdSetting);
            }
        }
    }
}
