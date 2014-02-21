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
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web;

using Seasar.Quill.Attrs;

using NEXS.ERP.CM.Common;

namespace NEXS.ERP.CM.DA
{
    //************************************************************************
    /// <summary>
    /// ���ʏ����f�[�^�A�N�Z�X�w
    /// </summary>
    //************************************************************************
    [Implementation]
    public class CMCommonDA : CMBaseDA
    {
        private static CMCommonSelectDataSet.SelectStatementDataTable s_selectStatementTable;

        #region SQL��
        /// <summary>
        /// ���샍�OINSERT��
        /// </summary>
        private const string INSERT_OPLOG_SQL =
            "INSERT INTO CMST�V�X�e�����p�� " +
            "VALUES(" +
            "CURRENT_TIMESTAMP," +
            "@���ID," +
            "@��ʖ�," +
            "@���[�UID," +
            "@�[��ID," +
            "@AP�T�[�o)";

        /// <summary>
        /// �敪�l����SELECT��
        /// </summary>
        private const string SELECT_KBN_SQL =
            "SELECT '' �\����, ����CD, ���ޖ�, ��lCD, ��l�� " +
            "FROM CMSM�ėp��l WHERE ����CD IN ({0}) " +
            "ORDER BY ����CD, ��lCD";

        /// <summary>
        /// �Q�Ɣ͈�, �X�V����ELECT��
        /// </summary>
        private const string SELECT_RANGE_CANUPDATE_SQL =
            // ��ʑg�D�ċA����
            "WITH SL (�g�DCD, ��ʑg�DCD, �g�D�K�w�敪) AS " +
            "(SELECT �g�DCD, ��ʑg�DCD, �g�D�K�w�敪 " +
              "FROM CMSM�g�D " +
             "WHERE �g�DCD = @�g�DCD " +
            "UNION ALL " +
            "SELECT A.�g�DCD, A.��ʑg�DCD, A.�g�D�K�w�敪 " +
              "FROM CMSM�g�D A " +
              "JOIN SL ON SL.��ʑg�DCD = A.�g�DCD AND SL.�g�D�K�w�敪 != '1')" +
            // ���[�U�ɕt�^���ꂽ���������ID��������v������́A�g�D�K�w���߂����̂�D�悵�Ď擾
            "SELECT DISTINCT ���[��ID, " +
                    "FIRST_VALUE(���ۃt���O) OVER (PARTITION BY ���[��ID " +
                    "ORDER BY LEN(���ID) DESC, �g�D�K�w�敪 DESC) ���ۃt���O " +
              "FROM {0} A " +
              "JOIN SL ON SL.�g�DCD = A.�g�DCD " +
             "WHERE ���[��ID IN ({1}) " +
               "AND @���ID LIKE ���ID + '%' " +
             "ORDER BY ���[��ID";

        /// <summary>
        /// �X�V��SELECT��
        /// </summary>
        private const string SELECT_UPD_SQL =
            "SELECT B.�X�V��ID, A.\"���[�U��\" �X�V�Җ� " +
            "FROM \"CMSM���[�U\" A " +
            "JOIN CMSM�g�D S ON S.�g�DCD = A.�g�DCD " +
            "JOIN ({0}) B ON A.\"���[�UID\" = B.�X�V��ID " +
            "{1}" +
            "ORDER BY B.�X�V��ID";
        #endregion

        #region �R���X�g���N�^
        //************************************************************************
        /// <summary>
        /// �R���X�g���N�^
        /// </summary>
        //************************************************************************
        public CMCommonDA()
        {
            // ���ʌ����ݒ�t�@�C���ǂݍ���
            if (s_selectStatementTable == null)
            {
                s_selectStatementTable = new CMCommonSelectDataSet.SelectStatementDataTable();
                s_selectStatementTable.ReadXml(AppDomain.CurrentDomain.BaseDirectory + "/CommonSelect.xml");
            }
        }
        #endregion

        #region �f�[�^�A�N�Z�X���\�b�h
        //************************************************************************
        /// <summary>
        /// ���ݎ������擾����B
        /// </summary>
        /// <returns>���ݎ���</returns>
        //************************************************************************
        public DateTime GetSysdate()
        {
            // SelectCommand�̐ݒ�
            Adapter.SelectCommand = CreateCommand("SELECT CURRENT_TIMESTAMP");
            // �f�[�^�Z�b�g�̍쐬
            DataSet ds = new DataSet();
            // �f�[�^�̎擾
            int cnt = Adapter.Fill(ds);

            return cnt > 0 ? (DateTime)ds.Tables[0].Rows[0][0] : DateTime.Now;
        }

        //************************************************************************
        /// <summary>
        /// �w�肳�ꂽ����ID�̌������w�肳�ꂽ�����Ŏ��s����B
        /// </summary>
        /// <param name="argSelectId">����ID</param>
        /// <param name="argParams">�p�����[�^</param>
        /// <returns>��������</returns>
        //************************************************************************
        public DataTable Select(string argSelectId, params object[] argParams)
        {
            // SELECT���̐ݒ�
            DataRow[] rows = s_selectStatementTable.Select("SelectId = '" + argSelectId + "'");
            if (rows.Length == 0) throw new Exception("CommonSelect.xml��\"" + argSelectId + "\"���o�^����Ă��܂���B");
            string statement = rows[0]["Statement"].ToString();

            IDbCommand selectCommand = CreateCommand(statement);
            Adapter.SelectCommand = selectCommand;
            // �p�����[�^�̐ݒ�
            for (int i = 0; i < argParams.Length; i++)
            {
                /*
                // DateTime�^�̏ꍇ
                if (argParams[i] is DateTime)
                    selectCommand.Parameters.Add((i + 1).ToString(), SqlDbType.Date,
                        argParams[i], ParameterDirection.Input);
                // DateTime�^�ȊO�̏ꍇ
                else*/
                selectCommand.Parameters.Add(CreateCmdParam((i + 1).ToString(), argParams[i]));
            }
            // �f�[�^�Z�b�g�̍쐬
            DataSet ds = new DataSet();
            // �f�[�^�̎擾
            Adapter.Fill(ds);
            // �������ʂ̕ԋp
            return ds.Tables[0];
        }

        //************************************************************************
        /// <summary>
        /// �ő匟��������Ԃ��B
        /// </summary>
        /// <param name="argId">���ID</param>
        /// <returns>�ő匟������</returns>
        //************************************************************************
        public int GetMaxRow(string argId = null)
        {
            if (argId == null) argId = CMInformationManager.ClientInfo.FormId;

            DataTable result = Select("CMSM�ėp��l", "V001", argId);
            if (result.Rows.Count > 0 && result.Rows[0]["��l�P"] != DBNull.Value)
                return Convert.ToInt32(result.Rows[0]["��l�P"]);
            else return 1000;
        }

        //************************************************************************
        /// <summary>
        /// ���샍�O���L�^����B
        /// </summary>
        /// <param name="argFormName">��ʖ�</param>
        //************************************************************************
        public void WriteOperationLog(string argFormName)
        {
            // �R�l�N�V���������I�[�v������t���O
            bool isClosed = Connection.State == ConnectionState.Closed;

            try
            {
                // �R�l�N�V�������J��
                if (isClosed) Connection.Open();
                // INSERT���̐ݒ�
                IDbCommand cmd = CreateCommand(INSERT_OPLOG_SQL);
                // �p�����[�^�̐ݒ�
                cmd.Parameters.Add(CreateCmdParam("���ID", CMInformationManager.ClientInfo.FormId));
                cmd.Parameters.Add(CreateCmdParam("��ʖ�", argFormName));
                cmd.Parameters.Add(CreateCmdParam("���[�UID", CMInformationManager.UserInfo.Id));
                cmd.Parameters.Add(CreateCmdParam("�[��ID", CMInformationManager.ClientInfo.MachineName));
                cmd.Parameters.Add(CreateCmdParam("�`�o�T�[�o", Environment.MachineName));
                // INSERT���s
                cmd.ExecuteNonQuery();
            }
            finally
            {
                if (isClosed)
                {
                    // �R�l�N�V������j������
                    Connection.Close();
                }
            }
        }

        //************************************************************************
        /// <summary>
        /// �ėp��l����敪�l���̂��擾����B
        /// </summary>
        /// <param name="argKbnList">��l����CD�̃��X�g</param>
        /// <returns>�敪�l���̂�DataTable</returns>
        //************************************************************************
        public DataTable SelectKbn(params string[] argKbnList)
        {
            // IN�̒��̏������쐬
            StringBuilder sb = new StringBuilder();
            for (int i = 1; i <= argKbnList.Length; i++)
            {
                if (i > 1) sb.Append(",");
                sb.AppendFormat("@{0}", i);
            }

            // IDbCommand�쐬
            IDbCommand cmd = CreateCommand(string.Format(SELECT_KBN_SQL, sb));
            Adapter.SelectCommand = cmd;

            // �p�����[�^��ݒ�
            foreach (string val in argKbnList) cmd.Parameters.Add(CreateCmdParam("1", val));

            // �f�[�^�Z�b�g�̍쐬
            DataSet ds = new DataSet();
            // �f�[�^�̎擾
            int cnt = Adapter.Fill(ds);
            // �\�����̐ݒ�
            ds.Tables[0].Columns["�\����"].Expression = "[��lCD] + ' ' + [��l��]";

            return ds.Tables[0];
        }

        //************************************************************************
        /// <summary>
        /// �Q�Ɣ͈�, �X�V������������B
        /// </summary>
        /// <param name="argFormId">��ʂh�c</param>
        /// <param name="argIsRange">True:�Q�Ɣ͈�, False:�X�V����</param>
        /// <returns>True:��ЁA�X�V��, False:���_�A�X�V�s��</returns>
        //************************************************************************
        public bool GetRangeCanUpdate(string argFormId, bool argIsRange)
        {
            // ���[���̏����쐬
            string[] roles = CMInformationManager.UserInfo.Roles;
            StringBuilder builder = new StringBuilder();

            if (roles != null && roles.Length > 0)
            {
                builder.Append("'" + roles[0] + "'");
                for (int i = 1; i < roles.Length; i++)
                {
                    builder.AppendFormat(",'{0}'", roles[i]);
                }
            }
            else return false;

            // SELECT���̐ݒ�
            IDbCommand cmd = CreateCommand(
                string.Format(SELECT_RANGE_CANUPDATE_SQL, argIsRange ? "CMSM�Q�Ɣ͈�" : "CMSM�X�V����",
                builder.ToString()));
            // �p�����[�^�̐ݒ�
            cmd.Parameters.Add(CreateCmdParam("�g�DCD", CMInformationManager.UserInfo.SoshikiCd));
            cmd.Parameters.Add(CreateCmdParam("���ID", argFormId));

            // �R�}���h�ݒ�
            Adapter.SelectCommand = cmd;

            // �f�[�^�Z�b�g�̍쐬
            DataSet ds = new DataSet();
            // �f�[�^�̎擾
            Adapter.Fill(ds);

            return ds.Tables[0].Select("���ۃt���O = True").Count() > 0;
        }

        //************************************************************************
        /// <summary>
        /// �ėp��l�}�X�^����������B
        /// </summary>
        /// <param name="argSelectId">����ID</param>
        /// <param name="argParam">��������</param>
        /// <param name="argIsOver">�ő匟�������I�[�o�[�t���O</param>
        /// <returns>��������</returns>
        //************************************************************************
        public DataTable SelectSub(string argSelectId, List<CMSelectParam> argParam, out bool argIsOver)
        {
            // SELECT���̎擾
            DataRow[] rows = s_selectStatementTable.Select("SelectId = '" + argSelectId + "'");
            if (rows.Length == 0) throw new Exception("CommonSelect.xml��\"" + argSelectId + "\"���o�^����Ă��܂���B");
            
            // SELECT���쐬
            StringBuilder selectSql = new StringBuilder("SELECT TOP 1001 * FROM (");
            selectSql.Append(rows[0]["Statement"].ToString());

            // WHERE��ǉ�
            AddWhere(selectSql, argParam);

            // �i���ݏ����ǉ�
            selectSql.Append(") A ORDER BY ROWNUMBER");

            // SELECT���̐ݒ�
            IDbCommand cmd = CreateCommand(selectSql.ToString());
            Adapter.SelectCommand = cmd;

            // �p�����[�^�̐ݒ�
            int pCnt = 1;
            foreach (var param in argParam)
            {
                if (param.paramFrom != null)
                {
                    // �p�����[�^���̎擾
                    string name;
                    if (string.IsNullOrEmpty(param.condtion))
                    {
                        name = pCnt.ToString();
                        pCnt++;
                    }
                    else name = param.condtion.Substring(param.condtion.IndexOf("@") + 1);

                    /*
                    if (param.paramFrom is DateTime)
                        cmd.Parameters.Add(name, SqlDbType.Date, param.paramFrom, ParameterDirection.Input);
                    else*/
                    cmd.Parameters.Add(CreateCmdParam(name, param.paramFrom));
                }
            }

            // �f�[�^�Z�b�g�̍쐬
            DataSet ds = new DataSet();
            // �f�[�^�̎擾
            int cnt = Adapter.Fill(ds);

            // �ő匟�������I�[�o�[�̏ꍇ�A�ŏI�s���폜
            if (cnt > 1000)
            {
                argIsOver = true;
                ds.Tables[0].Rows.RemoveAt(cnt - 1);
            }
            else argIsOver = false;

            // �������ʂ̕ԋp
            return ds.Tables[0];
        }

        //************************************************************************
        /// <summary>
        /// �X�V�҂��w�肳�ꂽ�����Ō�������B
        /// </summary>
        /// <param name="argParam">��������</param>
        /// <param name="argTables">�e�[�u�����̔z��</param>
        /// <param name="argIsOver">�ő匟�������I�[�o�[�t���O</param>
        /// <returns>��������</returns>
        //************************************************************************
        public DataTable SelectUpdSub(List<CMSelectParam> argParam, string[] argTables,
             out bool argIsOver)
        {
            // SQL���̍쐬
            StringBuilder union = new StringBuilder();
            // ���₢���킹�̍쐬
            if (argTables != null && argTables.Length > 0)
            {
                union.Append("SELECT DISTINCT �X�V��ID FROM ").Append(argTables[0]);

                for (int i = 1; i < argTables.Length; i++)
                    union.Append(" UNION SELECT �X�V��ID FROM ").Append(argTables[i]);
            }

            // �g�D�K�w���S�ЂłȂ���΁A��Ђ̏�����ǉ�
            CMUserInfo uinfo = CMInformationManager.UserInfo;
            if (uinfo.SoshikiKaisoKbn != CMSoshikiKaiso.ALL)
                argParam.Add(new CMSelectParam("S.�g�DCD", "= @�g�DCD", uinfo.SoshikiCd));

            // WHERE��쐬
            StringBuilder where = new StringBuilder();
            AddWhere(where, argParam);

            // SELECT���̐ݒ�
            IDbCommand cmd = CreateCommand(string.Format(SELECT_UPD_SQL, union, where));
            Adapter.SelectCommand = cmd;

            // �p�����[�^�̐ݒ�
            SetParameter(cmd, argParam);

            // �f�[�^�Z�b�g�̍쐬
            DataSet ds = new DataSet();
            // �f�[�^�̎擾
            int cnt = Adapter.Fill(ds);

            // �ő匟�������I�[�o�[�̏ꍇ�A�ŏI�s���폜
            if (cnt > 1000)
            {
                argIsOver = true;
                ds.Tables[0].Rows.RemoveAt(cnt - 1);
            }
            else argIsOver = false;

            // �������ʂ̕ԋp
            return ds.Tables[0];
        }

        //************************************************************************
        /// <summary>
        /// ���ʌ������g�p���ăf�[�^�̑��݃`�F�b�N���s���B
        /// ���݂��Ȃ������ꍇ�́ACMException��throw����B
        /// </summary>
        /// <param name="argSelectId">����ID</param>
        /// <param name="argTableName">���݃`�F�b�N�Ώۂ̃e�[�u����</param>
        /// <param name="argRow">���݃`�F�b�N�Ώۃf�[�^���܂�DataRow</param>
        /// <param name="argColumnName">���݃`�F�b�N�Ώۃf�[�^��DataRow���̗�</param>
        /// <param name="argParams">���ʌ������i�ɓn���p�����[�^(�w��Ȃ��̏ꍇ��argColumnName��
        ///�w�肵����̃f�[�^���p�����[�^�Ɏg�p����)</param>
        //************************************************************************
        public void ExistCheck(string argSelectId,
            string argTableName, DataRow argRow, string argColumnName, params object[] argParams)
        {
            int cnt;
            // argParams�̎w�肪�Ȃ������ꍇ
            if (argParams.Length == 0)
                cnt = Select(argSelectId, argRow[argColumnName]).Rows.Count;
            // argParams�̎w�肪������
            else cnt = Select(argSelectId, argParams).Rows.Count;
            // ���݃`�F�b�N
            if (cnt == 0)
            {
                // ���b�Z�[�W�̍쐬
                CMMessage message = new CMMessage("WV107",
                    new CMRowField(CMUtil.GetRowNumber(argRow), argColumnName),
                    argTableName);
                // ��O�𔭐�
                throw new CMException(message);
            }
        }

        //************************************************************************
        /// <summary>
        /// XML�t�@�C���̐ݒ肩��f�[�^�̑��݃`�F�b�N���s���B
        /// </summary>
        /// <param name="argTable">���݃`�F�b�N�Ώۂ�DataTable</param>
        /// <param name="argFname">�ǂݍ���XML�t�@�C����(�g���q�Ȃ�)</param>
        //************************************************************************
        public void ExistCheckFomXml(DataTable argTable, string argFname = null)
        {
            // �f�t�H���g�ݒ�
            if (argFname == null) argFname = argTable.TableName;

            // �f�[�^�Z�b�g�Ƀt�@�C����ǂݍ���
            DataSet ds = new DataSet();
            ds.ReadXml(Path.Combine(HttpContext.Current.Request.PhysicalApplicationPath, "Model", argFname + ".xml"));

            // ���͒l�`�F�b�N���[�v
            foreach (DataRow row in argTable.Rows)
            {
                // �폜�f�[�^�̓`�F�b�N���Ȃ�
                if (row.RowState == DataRowState.Deleted) continue;

                // ���݃`�F�b�N���ڃ��[�v
                foreach (DataRow irow in ds.Tables["����"].Select("Len(���݃`�F�b�N�e�[�u����) > 0"))
                {
                    // �L�[���ڂ͐V�K�̂݃`�F�b�N
                    if (irow["Key"].ToString() == "True" && row.RowState != DataRowState.Added) continue;

                    List<object> checkParams = new List<object>();
                    string paramText;

                    // ���ʌ����p�����[�^�擾
                    if (irow.Table.Columns.Contains("���ʌ����p�����[�^") &&
                        (paramText = irow["���ʌ����p�����[�^"].ToString()).Length > 0)
                    {
                        foreach (string p0 in paramText.Split(new char[] {','}, StringSplitOptions.RemoveEmptyEntries))
                        {
                            string p = p0.TrimStart();
                            if (p.Length < 2) continue;

                            // '����n�܂�ꍇ�͂��̂܂ܐݒ�
                            if (p[0] == '\'') checkParams.Add(p.Substring(1));
                            // "#"����n�܂�ꍇ��UserInfo����ݒ�
                            else if (p[0] == '#')
                            {
                                PropertyInfo pi = CMInformationManager.UserInfo.GetType().GetProperty(p.Substring(1));
                                checkParams.Add(pi.GetValue(CMInformationManager.UserInfo, null));
                            }
                            // Row�̒l���擾
                            else checkParams.Add(row[p]);
                        }
                    }

                    // ���݃`�F�b�N
                    ExistCheck(irow["���ʌ���ID"].ToString(), irow["���݃`�F�b�N�e�[�u����"].ToString(),
                        row, irow["���ږ�"].ToString(), checkParams.ToArray());
                }
            }
        }
        #endregion
    }
}
