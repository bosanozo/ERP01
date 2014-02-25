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
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

using log4net;

using NEXS.ERP.CM.Common;

namespace NEXS.ERP.CM.DA
{
    //************************************************************************
    /// <summary>
    /// �f�[�^�A�N�Z�X�w�̊��N���X
    /// </summary>
    //************************************************************************
    public class CMBaseDA
    {
        #region ���K�[�t�B�[���h
        private ILog m_logger;
        #endregion

        #region �v���p�e�B
        /// <summary>
        /// ���K�[
        /// </summary>
        protected ILog Log
        {
            get { return m_logger; }
        }

        /// <summary>�R�l�N�V����</summary>
        public IDbConnection Connection { get; set; }

        /// <summary>�f�[�^�A�_�v�^</summary>
        protected IDbDataAdapter Adapter { get; set; }
        #endregion

        // �s���b�N�^�C���A�E�g�G���[NO
        private const int LOCK_TIMEOUT_ERR = 1222;

        // PKEY����ᔽ�G���[NO
        private const int PKEY_ERR = 2627;

        #region SQL��
        /// <summary>
        /// INSERT��
        /// </summary>
        private const string INSERT_SQL =
            "INSERT INTO {0} (" +
            "{1}" +
            "�쐬����," +
            "�쐬��ID," +
            "�쐬��IP," +
            "�쐬PG," +
            "�X�V����," +
            "�X�V��ID," +
            "�X�V��IP," +
            "�X�VPG" +
            ")VALUES(" +
            "{2}" +
            "@�X�V����," +
            "@�X�V��ID," +
            "@�X�V��IP," +
            "@�X�VPG," +
            "@�X�V����," +
            "@�X�V��ID," +
            "@�X�V��IP," +
            "@�X�VPG)";

        /// <summary>
        /// UPDATE��
        /// </summary>
        private const string UPDATE_SQL =
            "UPDATE {0} SET " +
            "{1}" +
            "�X�V���� = @�X�V����," +
            "�X�V��ID = @�X�V��ID," +
            "�X�V��IP = @�X�V��IP," +
            "�X�VPG = @�X�VPG " +
            "WHERE ";

        /// <summary>
        /// DELETE��
        /// </summary>
        private const string DELETE_SQL =
            "DELETE FROM {0} WHERE ";

        /// <summary>
        /// �r���`�F�b�N�pSELECT��
        /// </summary>
        private const string CONC_CHEK_SQL =
            "SET LOCK_TIMEOUT 10000 " +
            "SELECT �X�V����, �X�V��ID, �X�V��IP, �X�VPG, �r���p�o�[�W���� " +
              "FROM {0} WITH(ROWLOCK, UPDLOCK) WHERE ";

        /// <summary>
        /// ���݃`�F�b�N�pSELECT��
        /// </summary>
        private const string EXIST_CHEK_SQL =
            "SELECT TOP 1 COUNT(*) FROM {0} WHERE ROWNUM <= 1";

        /// <summary>
        /// �č��ؐ�INSERT��
        /// </summary>
        private const string INSERT_AUDITLOG_SQL =
            "INSERT INTO CMST�č��ؐ� (" +
            "�e�[�u����," +
            "�X�V�敪," +
            "�L�[," +
            "���e," +
            "�쐬����," +
            "�쐬��ID," +
            "�쐬��IP," +
            "�쐬PG," +
            "�X�V����," +
            "�X�V��ID," +
            "�X�V��IP," +
            "�X�VPG" +
            ")VALUES(" +
            "@�e�[�u����," +
            "@�X�V�敪," +
            "@�L�[," +
            "@���e," +
            "@�X�V����," +
            "@�X�V��ID," +
            "@�X�V��IP," +
            "@�X�VPG," +
            "@�X�V����," +
            "@�X�V��ID," +
            "@�X�V��IP," +
            "@�X�VPG)";
        #endregion

        #region SELECT���쐬�pSQL
        private const string TOROKU_COLS =
            "A.�쐬����," +
            "A.�쐬��ID," +
            "US1.���[�U�� �쐬�Җ�," +
            "A.�쐬��IP," +
            "A.�쐬PG," +
            "A.�X�V����," +
            "A.�X�V��ID," +
            "US2.���[�U�� �X�V�Җ�," +
            "A.�X�V��IP," +
            "A.�X�VPG";

        private const string TOROKU_JOIN =
            "LEFT JOIN CMSM���[�U US1 ON US1.���[�UID = A.�쐬��ID " +
            "LEFT JOIN CMSM���[�U US2 ON US2.���[�UID = A.�X�V��ID ";

        /// <summary>
        /// SELECT��
        /// </summary>
        private const string SELECT_SQL =
            "SELECT " +
            "'0' �폜," +
            "{0}" +
            TOROKU_COLS + "," +
            "A.�r���p�o�[�W����," +
            "A.ROWNUMBER " +
            "FROM (SELECT A.*, ROW_NUMBER() OVER (ORDER BY {1}) - 1 ROWNUMBER " +
            "FROM {2} A{3}) A " +
            TOROKU_JOIN +
            "{4}" +
            "WHERE ROWNUMBER <= @�ő匟������ " +
            "ORDER BY ROWNUMBER";

        /// <summary>
        /// CSV�o�͗pSELECT��
        /// </summary>
        private const string SELECT_CSV_SQL =
            "SELECT " +
            "{0}" +
            TOROKU_COLS +
            " FROM {2} A " +
            TOROKU_JOIN +
            "{4}{3}" +
            "ORDER BY {1}";

        /// <summary>
        /// �o�^���SELECT��
        /// </summary>
        private const string SELECT_EDIT_SQL =
            "SELECT " +
            "{0}" +
            TOROKU_COLS + "," +
            "A.�r���p�o�[�W����," +
            "0 ROWNUMBER " +
            "FROM {1} A " +
            TOROKU_JOIN +
            "{3}{2}";

        /// <summary>
        /// �K�p���ԃ`�F�b�NSELECT��
        /// </summary>
        private const string SELECT_SPAN_SQL =
            "SELECT NULL FROM {0} WHERE �K�p�I���� >= TO_CHAR(@1, 'YYYYMMDD') AND �K�p�J�n�� <= TO_CHAR(@1, 'YYYYMMDD')";
        #endregion
        
        #region �R���X�g���N�^
        //************************************************************************
        /// <summary>
        /// �R���X�g���N�^
        /// </summary>
        //************************************************************************
        public CMBaseDA()
        {
            // ���K�[���擾
            m_logger = LogManager.GetLogger(this.GetType());

            // �f�[�^�A�_�v�^��factory����쐬����
            DbProviderFactory factory = DbProviderFactories.GetFactory("System.Data.SqlClient");
            Adapter = factory.CreateDataAdapter();
        }
        #endregion

        #region public���\�b�h
        //************************************************************************
        /// <summary>
        /// �w�肳�ꂽ�e�[�u���ɍX�V�f�[�^��o�^����B
        /// </summary>
        /// <param name="argUpdateData">�X�V�f�[�^</param>
        /// <param name="argOperationTime">���쎞��</param>
        /// <param name="argCmdSettings">Command�ݒ�</param>
        /// <returns>�o�^�������R�[�h��</returns>
        //************************************************************************
        public int Update(DataSet argUpdateData, DateTime argOperationTime, CMCmdSettings argCmdSettings = null)
        {
            // �f�t�H���g�ݒ�
            if (argCmdSettings == null)
            {
                argCmdSettings = new CMCmdSettings();
                foreach (DataTable table in argUpdateData.Tables)
                    argCmdSettings.AddFomXml(table.TableName);
            }

            int cnt = 0;
            int tableCnt = argCmdSettings.CmdSettings.Count;

            // 1�e�[�u���̏ꍇ
            if (tableCnt == 1)
            {
                // �o�^���s
                cnt = UpdateTable(argCmdSettings[0], argUpdateData.Tables[argCmdSettings[0].Name], argOperationTime);
            }
            // �����e�[�u���̏ꍇ
            else
            {
                // Command�ݒ�̋t���ɍ폜�f�[�^��o�^
                for (int i = tableCnt - 1; i > 0; i--)
                {
                    DataTable table = argUpdateData.Tables[argCmdSettings[i].Name].GetChanges(DataRowState.Deleted);
                    if (table != null && table.Rows.Count > 0)
                        cnt += UpdateTable(argCmdSettings[i], table, argOperationTime);
                }

                // �ŏ��̃e�[�u���̃f�[�^��o�^
                cnt += UpdateTable(argCmdSettings[0], argUpdateData.Tables[argCmdSettings[0].Name], argOperationTime);

                // Command�ݒ�̏��ɐV�K�A�C���f�[�^��o�^
                for (int i = 1; i < tableCnt; i++)
                {
                    DataTable table = argUpdateData.Tables[argCmdSettings[i].Name].GetChanges(DataRowState.Added | DataRowState.Modified);
                    if (table != null && table.Rows.Count > 0)
                        cnt += UpdateTable(argCmdSettings[i], table, argOperationTime);
                }
            }

            return cnt;
        }

        //************************************************************************
        /// <summary>
        /// �w�肳�ꂽ�e�[�u���ɍX�V�f�[�^���A�b�v���[�h����B
        /// </summary>
        /// <param name="argUpdateData">�X�V�f�[�^</param>
        /// <param name="argOperationTime">���쎞��</param>
        /// <param name="argCmdSettings">Command�ݒ�</param>
        /// <returns>�o�^�������R�[�h��</returns>
        //************************************************************************
        public int Upload(DataSet argUpdateData, DateTime argOperationTime, CMCmdSettings argCmdSettings = null)
        {
            // �f�t�H���g�ݒ�
            if (argCmdSettings == null)
            {
                argCmdSettings = new CMCmdSettings();
                foreach (DataTable table in argUpdateData.Tables)
                    argCmdSettings.AddFomXml(table.TableName);
            }

            int cnt = 0;

            // Command�ݒ�̏��ɐV�K�A�C���f�[�^��o�^
            for (int i = 0; i < argCmdSettings.CmdSettings.Count; i++)
            {
                DataTable table = argUpdateData.Tables[argCmdSettings[i].Name];
                if (table != null && table.Rows.Count > 0)
                    cnt += UploadTable(argCmdSettings[i], table, argOperationTime);
            }

            return cnt;
        }
        #endregion

        #region protected���\�b�h
        #region SQL�쐬���\�b�h
        //************************************************************************
        /// <summary>
        /// ������ʂɉ�����SELECT�����쐬����B�쓮�\�̃V�m�j����A�Ƃ���B
        /// </summary>
        /// <param name="argCols">������</param>
        /// <param name="argTableName">�����e�[�u����</param>
        /// <param name="argWhere">WHERE��</param>
        /// <param name="argJoin">JOIN��</param>
        /// <param name="argOrder">���я�</param>
        /// <param name="argSelectType">�������</param>
        /// <returns>SELECT��</returns>
        //************************************************************************
        protected string CreateSelectSql(string argCols, string argTableName,
            string argWhere, string argJoin, string argOrder, CMSelectType argSelectType)
        {
            //string tableName = Escape(argTableName);
            string tableName = argTableName;

            // �o�^��ʂ̏ꍇ
            if (argSelectType == CMSelectType.Edit)
                return string.Format(SELECT_EDIT_SQL, argCols, tableName, argWhere, argJoin);
            // �ꗗ����, CSV�o�͂̏ꍇ
            else
            {
                return string.Format(argSelectType == CMSelectType.List ? SELECT_SQL : SELECT_CSV_SQL,
                    argCols, argOrder, tableName, argWhere, argJoin);
            }
        }

        //************************************************************************
        /// <summary>
        /// StringBuilder�Ɍ���������ǉ�����B
        /// </summary>
        /// <param name="where">����������ǉ�����StringBuilder</param>
        /// <param name="argParam">��������</param>
        //************************************************************************
        protected void AddWhere(StringBuilder where, List<CMSelectParam> argParam)
        {
            // �󔒂ŏI����ĂȂ���΁A�󔒒ǉ�
            if (where.Length > 0 && where[where.Length - 1] != ' ')
                where.Append(" ");

            // �ǉ��̏���
            foreach (var param in argParam)
            {
                if (string.IsNullOrEmpty(param.condtion)) continue;
                where.Append(where.Length > 0 ? "AND " : " WHERE ");
                if (!string.IsNullOrEmpty(param.name))
                {
                    // �e�[�u���̎w�肪�Ȃ��ꍇ�́AA������
                    if (!param.name.Contains('.')) where.Append("A.");
                    where.AppendFormat("{0} ", Escape(param.name));
                }
                where.Append(param.condtion).Append(" ");
            }
        }

        //************************************************************************
        /// <summary>
        /// �e�[�u�����ڗ�DataTable����INSERT��, UPDATE�����쐬����B
        /// </summary>
        /// <param name="argCmdSetting">Command�ݒ�</param>
        /// <param name="argInsertSql">INSERT��</param>
        /// <param name="argUpdateSql">UPDATE��</param>
        //************************************************************************
        protected void CreateInsertUpdateSql(CMCmdSetting argCmdSetting,
            out string argInsertSql, out string argUpdateSql)
        {
            StringBuilder ins1 = new StringBuilder();
            StringBuilder ins2 = new StringBuilder();
            StringBuilder upd = new StringBuilder();

            // �e�[�u�����ڗ�Ń��[�v
            foreach (var row in argCmdSetting.ColumnParams)
            {
                string valueFmt;

                // �L�[���ڂ�NULL�͐ݒ肳���Ȃ�
                if (row.IsKey)
                {
                    if (row.DbType == CMDbType.���z || row.DbType == CMDbType.���� || row.DbType == CMDbType.����)
                        valueFmt = "ISNULL(@{0}, 0),";
                    // ���t�^�͑Ή��Ȃ�
                    else if (row.DbType == CMDbType.���� || row.DbType == CMDbType.���t)
                        valueFmt = "@{0},";
                    else valueFmt = "ISNULL(@{0}, ' '),";
                }
                else valueFmt = "@{0},";

                //string colName = Escape(row.Name);
                string colName = row.Name;

                // INSERT���쐬
                ins1.Append(colName).Append(",");
                ins2.AppendFormat(valueFmt, row.Name);

                // �]�����ڂ̏ꍇ
                if (!row.IsKey)
                    // UPDATE���쐬
                    upd.Append(colName).Append(" = ").AppendFormat(valueFmt, row.Name);
            }

            //string tname = Escape(argCmdSetting.Name);
            string tname = argCmdSetting.Name;
            argInsertSql = string.Format(INSERT_SQL, tname, ins1, ins2);
            argUpdateSql = string.Format(UPDATE_SQL, tname, upd);
        }

        //************************************************************************
        /// <summary>
        /// "�["������ꍇ�A"�ň͂��B
        /// </summary>
        /// <param name="arg">������</param>
        /// <returns>�ϊ��㕶����</returns>
        //************************************************************************
        protected string Escape(string arg)
        {
            if (arg.Contains("�[")) return '"' + arg + '"';
            else return arg;
        }
        #endregion

        //************************************************************************
        /// <summary>
        /// �ڑ��Ɋ֘A�t����ꂽCommand�I�u�W�F�N�g���쐬����B
        /// </summary>
        /// <param name="argCommandText">Command�ɐݒ肷��SQL��</param>
        /// <returns>�ڑ��Ɋ֘A�t����ꂽCommand�I�u�W�F�N�g</returns>
        //************************************************************************
        protected IDbCommand CreateCommand(string argCommandText)
        {
            IDbCommand cmd = Connection.CreateCommand();
            cmd.CommandText = argCommandText;
            return cmd;
        }

        //************************************************************************
        /// <summary>
        /// �ڑ��Ɋ֘A�t����ꂽCommand�̃p�����[�^�I�u�W�F�N�g���쐬����B
        /// </summary>
        /// <param name="argParameterName">�p�����[�^��</param>
        /// <param name="argValue">�l</param>
        /// <returns>�ڑ��Ɋ֘A�t����ꂽCommand�̃p�����[�^�I�u�W�F�N�g</returns>
        //************************************************************************
        protected IDbDataParameter CreateCmdParam(string argParameterName, object argValue)
        {
            return new SqlParameter(argParameterName, argValue);
        }

        //************************************************************************
        /// <summary>
        /// �ڑ��Ɋ֘A�t����ꂽCommand�̃p�����[�^�I�u�W�F�N�g���쐬����B
        /// </summary>
        /// <param name="argParameterName">�p�����[�^��</param>
        /// <param name="argDbType">SqlDbType</param>
        /// <returns>�ڑ��Ɋ֘A�t����ꂽCommand�̃p�����[�^�I�u�W�F�N�g</returns>
        //************************************************************************
        protected IDbDataParameter CreateCmdParam(string argParameterName, SqlDbType argDbType)
        {
            return new SqlParameter(argParameterName, argDbType);
        }

        //************************************************************************
        /// <summary>
        /// Command�Ɍ����p�����[�^��ݒ肷��B
        /// </summary>
        /// <param name="argCmd">IDbCommand</param>
        /// <param name="argParam">��������</param>
        //************************************************************************
        protected void SetParameter(IDbCommand argCmd, List<CMSelectParam> argParam)
        {
            Regex regex = new Regex("@\\S+");

            foreach (var param in argParam)
            {
                // �v���[�X�t�H���_�����擾
                MatchCollection mc = regex.Matches(param.condtion);
                if (mc.Count == 0) continue;
 
                if (param.paramFrom != null && param.paramTo != null)
                {
                    argCmd.Parameters.Add(CreateCmdParam(mc[0].Value, param.paramFrom));
                    argCmd.Parameters.Add(CreateCmdParam(mc[1].Value, param.paramTo));
                }
                else
                {
                    if (param.paramFrom != null) argCmd.Parameters.Add(CreateCmdParam(mc[0].Value, param.paramFrom));
                    if (param.paramTo != null) argCmd.Parameters.Add(CreateCmdParam(mc[0].Value, param.paramTo));
                }
            }
        }

        #region �f�[�^�o�^���\�b�h
        //************************************************************************
        /// <summary>
        /// �f�[�^�x�[�X�X�V�p�����[�^��ݒ肷��
        /// </summary>
        /// <param name="argDataRow">�p�����[�^�ݒ�Ώۂ�DataRow</param>
        /// <param name="argUpdateTime">�f�[�^�x�[�X�ɋL�^����X�V����</param>
        //************************************************************************
        protected void SetUpdateParameter(DataRow argDataRow, DateTime argUpdateTime)
        {
            argDataRow["�X�V����"] = argUpdateTime;
            argDataRow["�X�V��ID"] = CMInformationManager.UserInfo.Id;
            argDataRow["�X�V��IP"] = CMInformationManager.ClientInfo.MachineName;
            argDataRow["�X�VPG"] = CMInformationManager.ClientInfo.FormId;
        }

        //************************************************************************
        /// <summary>
        /// �w�肳�ꂽ�e�[�u���ɍX�V�f�[�^��o�^����B
        /// </summary>
        /// <param name="argCmdSetting">Command�ݒ�</param>
        /// <param name="argDataTable">�X�V�f�[�^���i�[����DataTable</param>
        /// <param name="argSysdate">�f�[�^�x�[�X�ɋL�^����X�V����</param>
        /// <param name="argInsertSql">INSERT��</param>
        /// <param name="argUpdateSql">UPDATE��</param>
        /// <returns>�o�^�������R�[�h��</returns>
        //************************************************************************
        protected int UpdateTable(CMCmdSetting argCmdSetting,
            DataTable argDataTable, DateTime argSysdate,
            string argInsertSql = null, string argUpdateSql = null)
        {
            // �e�[�u�������擾
            //string tname = Escape(argCmdSetting.Name);
            string tname = argCmdSetting.Name;

            // ��L�[�̌����������擾
            string keyCond = argCmdSetting.GetKeyCondition();

            // INSERT��, UPDATE���̎����ݒ�
            if (argInsertSql == null || argUpdateSql == null)
            {
                // INSERT��, UPDATE�����쐬
                string insertSql;
                string updateSql;
                CreateInsertUpdateSql(argCmdSetting, out insertSql, out updateSql);
                // null�̏ꍇ�͍쐬�������̂�ݒ�
                if (argInsertSql == null) argInsertSql = insertSql;
                if (argUpdateSql == null) argUpdateSql = updateSql + keyCond;
            }

            // INSERT, UPDATE, DELETE�R�}���h�̍쐬
            IDbCommand insertCommand = CreateCommand(argInsertSql);
            IDbCommand updateCommand = CreateCommand(argUpdateSql);
            IDbCommand deleteCommand =
                CreateCommand(string.Format(DELETE_SQL, tname) + keyCond);
            // �r���`�F�b�N�pSELECT�R�}���h�̍쐬
            IDbCommand concCheckCommand =
                CreateCommand(string.Format(CONC_CHEK_SQL, tname) + keyCond);

            // INSERT, UPDATE, DELETE���̐ݒ�
            Adapter.InsertCommand = insertCommand;
            Adapter.UpdateCommand = updateCommand;
            Adapter.DeleteCommand = deleteCommand;

            // INSERT, UPDATE, DELETE, �r���`�F�b�N�pSELECT�R�}���h�̃p�����[�^��ݒ�
            AddCommandParameter(insertCommand, updateCommand, deleteCommand,
                concCheckCommand, argCmdSetting);

            // �R�l�N�V���������I�[�v������t���O
            bool isClosed = Connection.State == ConnectionState.Closed;

            try
            {
                if (isClosed) Connection.Open();

                // �f�[�^�̍X�V���[�v
                foreach (DataRow row in argDataTable.Rows)
                {
                    // �f�[�^�x�[�X�X�V�p�����[�^�̐ݒ�
                    if (row.RowState == DataRowState.Added || row.RowState == DataRowState.Modified)
                        SetUpdateParameter(row, argSysdate);

                    // �X�V�A�폜�f�[�^�̏ꍇ�A�r���`�F�b�N�����{
                    if (row.RowState == DataRowState.Modified || row.RowState == DataRowState.Deleted)
                        DoConcCheck(concCheckCommand, row, argCmdSetting);
                }
            }
            finally
            {
                if (isClosed) Connection.Close();
            }

            // �f�[�^�̓o�^�����s
            int cnt = DoUpdate(argDataTable);

            // �č��ؐՏo��
            WriteAuditLog(argCmdSetting, argDataTable, argSysdate);

            return cnt;
        }

        //************************************************************************
        /// <summary>
        /// �w�肳�ꂽ�e�[�u���ɁA�w�肳�ꂽ�f�[�^���A�b�v���[�h����B
        /// </summary>
        /// <param name="argCmdSetting">Command�ݒ�</param>
        /// <param name="argDataTable">�X�V�f�[�^���i�[����DataTable</param>
        /// <param name="argUpdateTime">�f�[�^�x�[�X�ɋL�^����X�V����</param>
        /// <param name="argInsertSql">INSERT��</param>
        /// <param name="argUpdateSql">UPDATE��</param>
        /// <returns>�o�^�������R�[�h��</returns>
        //************************************************************************
        protected int UploadTable(CMCmdSetting argCmdSetting,
            DataTable argDataTable,  DateTime argUpdateTime,
            string argInsertSql = null, string argUpdateSql = null)
        {
            // �X�V�p�̗��DataTable�ɒǉ�
            argDataTable.Columns.Add("�X�V����", typeof(DateTime));
            argDataTable.Columns.Add("�X�V��ID");
            argDataTable.Columns.Add("�X�V��IP");
            argDataTable.Columns.Add("�X�VPG");

            // �e�[�u�������擾
            //string tname = Escape(argCmdSetting.Name);
            string tname = argCmdSetting.Name;

            // ��L�[�̌����������擾
            string keyCond = argCmdSetting.GetKeyCondition();

            // INSERT��, UPDATE���̎����ݒ�
            if (argInsertSql == null || argUpdateSql == null)
            {
                // INSERT��, UPDATE�����쐬
                string insertSql;
                string updateSql;
                CreateInsertUpdateSql(argCmdSetting, out insertSql, out updateSql);
                // null�̏ꍇ�͍쐬�������̂�ݒ�
                if (argInsertSql == null) argInsertSql = insertSql;
                if (argUpdateSql == null) argUpdateSql = updateSql + keyCond;
            }
           
            // INSERT, UPDATE�R�}���h�̍쐬
            IDbCommand insertCommand = CreateCommand(argInsertSql);
            IDbCommand updateCommand = CreateCommand(argUpdateSql);

            // ���݃`�F�b�N�pSELECT���̍쐬
            IDbCommand existCheckCommand = 
                CreateCommand(string.Format(CONC_CHEK_SQL, tname) + keyCond);

            // INSERT, UPDATE���̐ݒ�
            Adapter.InsertCommand = insertCommand;
            Adapter.UpdateCommand = updateCommand;

            // INSERT, UPDATE, ���݃`�F�b�N�pSELECT�R�}���h�̃p�����[�^��ݒ�
            AddCommandParameter(insertCommand, updateCommand, null, existCheckCommand, argCmdSetting);

            // �R�l�N�V���������I�[�v������t���O
            bool isClosed = Connection.State == ConnectionState.Closed;

            try
            {
                if (isClosed) Connection.Open();

                // �f�[�^�̍X�V���[�v
                foreach (DataRow row in argDataTable.Rows)
                {
                    // �f�[�^�x�[�X�X�V�p�����[�^�̐ݒ�
                    SetUpdateParameter(row, argUpdateTime);
                    // ���݂��邩�`�F�b�N
                    DoUploadCheck(existCheckCommand, row, argCmdSetting);
                }
            }
            finally
            {
                if (isClosed) Connection.Close();
            }

            // �f�[�^�̓o�^�����s
            int cnt = 0;
            try
            {
                DataTable table = argDataTable.Copy();
                table.TableName = "Table";
                DataSet ds = new DataSet();
                ds.Tables.Add(table);
                cnt = Adapter.Update(ds);
            }
            catch (SqlException ex)
            {
                // ���o�^�s���擾
                DataRow[] rows = argDataTable.Select(null, null, DataViewRowState.ModifiedCurrent | DataViewRowState.Added);
                // �s�ԍ����擾
                int rowNumber = argDataTable.Rows.IndexOf(rows[0]) + 1;
                // ���b�Z�[�W�R�[�h��ݒ�
                string msgCode = ex.Number == PKEY_ERR ? "WV001" : "EV002";
                // ���b�Z�[�W�ݒ�
                string message = ex.Number == PKEY_ERR ? CMMessageManager.GetMessage(msgCode) : ex.Message;
                // ���b�Z�[�W�쐬
                CMMessage msgData = new CMMessage(msgCode, new CMRowField(rowNumber), message);
                // ��O����
                throw new CMException(msgData, ex);
            }
            return cnt;
        }
        #endregion
        #endregion

        #region private���\�b�h
        //************************************************************************
        /// <summary>
        /// �r���`�F�b�N�����s����B
        /// </summary>
        /// <param name="argConcCheckCommand">�r���`�F�b�N�p�R�}���h</param>
        /// <param name="argRow">�r���`�F�b�N�Ώۂ�DataRow</param>
        /// <param name="argCmdSetting">Command�ݒ�</param>
        //************************************************************************
        private void DoConcCheck(IDbCommand argConcCheckCommand,
            DataRow argRow, CMCmdSetting argCmdSetting)
        {
            // �r���`�F�b�N�p�R�}���h�Ƀp�����[�^�l��ݒ�
            foreach (var row in argCmdSetting.ColumnParams)
            {
                if (row.IsKey)
                {
                    string name = !string.IsNullOrEmpty(row.SourceColumn) ? row.SourceColumn : row.Name;
                    ((IDbDataParameter)argConcCheckCommand.Parameters[row.Name]).Value = argRow[name, DataRowVersion.Original];
                }
            }

            // �������s
            try
            {
                using (IDataReader reader = argConcCheckCommand.ExecuteReader())
                {
                    // ���R�[�h����̏ꍇ
                    if (reader.Read())
                    {
                        long rowversion = BitConverter.ToInt64((byte[])reader.GetValue(4), 0);

                        // �f�[�^�X�V�`�F�b�N
                        if (BitConverter.ToInt64((byte[])argRow["�r���p�o�[�W����", DataRowVersion.Original], 0) != rowversion)
                        {
                            DateTime updateTime = reader.GetDateTime(0);
                            string userId = reader.GetString(1);
                            string hostname = reader.GetString(2);
                            string progId = reader.GetString(3);

                            // �f�[�^���X�V����Ă����ꍇ
                            // ���b�Z�[�W�R�[�h�̐ݒ�
                            string msgCode = argRow.RowState == DataRowState.Modified ? "WV002" : "WV004";
                            CMMessage message;
                            int rowNumber = CMUtil.GetRowNumber(argRow);
                            // �s�ԍ�����̏ꍇ
                            if (rowNumber >= 0) message = new CMMessage(msgCode, new CMRowField(rowNumber),
                                    userId, updateTime, progId, hostname);
                            // �s�ԍ��Ȃ��̏ꍇ
                            else message = new CMMessage(msgCode, userId, updateTime, progId, hostname);
                            // ��O����
                            throw new CMException(message);
                        }
                    }
                    // ���R�[�h�Ȃ��̏ꍇ
                    else
                    {
                        // ���b�Z�[�W�R�[�h�̐ݒ�
                        string msgCode = argRow.RowState == DataRowState.Modified ? "WV003" : "WV005";
                        CMMessage message;
                        int rowNumber = CMUtil.GetRowNumber(argRow);
                        // �s�ԍ�����̏ꍇ
                        if (rowNumber >= 0) message = new CMMessage(msgCode, new CMRowField(rowNumber));
                        // �s�ԍ��Ȃ��̏ꍇ
                        else message = new CMMessage(msgCode);
                        // ��O����
                        throw new CMException(message);
                    }
                }
            }
            catch (SqlException ex)
            {
                // ���\�[�X�r�W�[�ȊO�͂��̂܂�throw
                if (ex.Number != LOCK_TIMEOUT_ERR) throw ex;

                // ���b�Z�[�W�R�[�h�̐ݒ�
                string msgCode = argRow.RowState == DataRowState.Modified ? "WV006" : "WV007";
                CMMessage message;
                int rowNumber = CMUtil.GetRowNumber(argRow);
                // �s�ԍ�����̏ꍇ
                if (rowNumber >= 0) message = new CMMessage(msgCode, new CMRowField(rowNumber));
                // �s�ԍ��Ȃ��̏ꍇ
                else message = new CMMessage(msgCode);
                // ��O����
                throw new CMException(message);
            }
        }

        //************************************************************************
        /// <summary>
        /// ���݃`�F�b�N�����s����B
        /// </summary>
        /// <param name="argExistCheckCommand">���݃`�F�b�N�p�R�}���h</param>
        /// <param name="argRow">���݃`�F�b�N�Ώۂ�DataRow</param>
        /// <param name="argCmdSetting">Command�ݒ�</param>
        //************************************************************************
        private void DoUploadCheck(IDbCommand argExistCheckCommand,
            DataRow argRow, CMCmdSetting argCmdSetting)
        {
            // ���݃`�F�b�N�p�R�}���h�Ƀp�����[�^�l��ݒ�
            foreach (var row in argCmdSetting.ColumnParams)
            {
                if (row.IsKey)
                {
                    string name = !string.IsNullOrEmpty(row.SourceColumn) ? row.SourceColumn : row.Name;
                    ((IDbDataParameter)argExistCheckCommand.Parameters[row.Name]).Value = argRow[name];
                }
            }

            // �������s
            using (IDataReader reader = argExistCheckCommand.ExecuteReader())
            {
                // ���R�[�h����̏ꍇ
                if (reader.Read())
                {
                    // �V�K��ʏ�ɕύX
                    argRow.AcceptChanges();
                    // �X�V�ɕύX
                    argRow.SetModified();
                }
            }
        }

        //************************************************************************
        /// <summary>
        /// �p�����[�^�ϐ���ݒ肷��B
        /// </summary>
        /// <param name="argInsertCommand">INSERT�R�}���h</param>
        /// <param name="argUpdateCommand">UPDATE�R�}���h</param>
        /// <param name="argDeleteCommand">DELETE�R�}���h</param>
        /// <param name="argConcCheckCommand">�r���`�F�b�N�p�R�}���h</param>
        /// <param name="argCmdSetting">���ڐݒ�</param>
        //************************************************************************
        private void AddCommandParameter(IDbCommand argInsertCommand, IDbCommand argUpdateCommand,
            IDbCommand argDeleteCommand, IDbCommand argConcCheckCommand, CMCmdSetting argCmdSetting)
        {
            string[] updateCols = { "�X�V����", "�X�V��ID", "�X�V��IP", "�X�VPG" };
            SqlDbType[] updateTypes = { SqlDbType.DateTime, SqlDbType.VarChar, SqlDbType.VarChar, SqlDbType.VarChar };

            IDbCommand[] keyCmds = { argInsertCommand, argUpdateCommand, argDeleteCommand, argConcCheckCommand };
            IDbCommand[] apdCmds = { argInsertCommand, argUpdateCommand };

            foreach (var row in argCmdSetting.ColumnParams)
            {
                // sourceColumn���ݒ肳��Ă����ꍇ�͎g�p����
                string sc = !string.IsNullOrEmpty(row.SourceColumn) ? row.SourceColumn : row.Name;

                // �L�[���ڂ̏ꍇ
                if (row.IsKey)
                {
                    foreach (var cmd in keyCmds)
                    {
                        if (cmd == null) continue;

                        // �p�����[�^��ǉ�
                        IDbDataParameter cmdParam = CreateCmdParam(row.Name, row.GetDbType());
                        cmdParam.SourceColumn = sc;
                        cmd.Parameters.Add(cmdParam);
                    }
                }
                // �]�����ڂ̏ꍇ
                else
                {
                    foreach (var cmd in apdCmds)
                    {
                        // �p�����[�^��ǉ�
                        IDbDataParameter cmdParam = CreateCmdParam(row.Name, row.GetDbType());
                        cmdParam.SourceColumn = sc;
                        cmd.Parameters.Add(cmdParam);
                    }
                }
            }

            // �X�V���p�����[�^
            for (int i = 0; i < updateCols.Length; i++)
            {
                foreach (var cmd in apdCmds)
                {
                    // �p�����[�^��ǉ�
                    IDbDataParameter cmdParam = CreateCmdParam(updateCols[i], updateTypes[i]);
                    cmdParam.SourceColumn = updateCols[i];
                    cmd.Parameters.Add(cmdParam);
                }
            }
        }

        //************************************************************************
        /// <summary>
        /// �f�[�^�̓o�^�����s����B
        /// </summary>
        /// <param name="argDataTable">�X�V�f�[�^���i�[����DataTable</param>
        /// <returns>�o�^�������R�[�h��</returns>
        //************************************************************************
        private int DoUpdate(DataTable argDataTable)
        {
            DataRowState[] stats = new DataRowState[]
            { 
                DataRowState.Deleted, DataRowState.Modified, DataRowState.Added
            };

            DataTable updateTable = null;
            int cnt = 0;
            try
            {
                // Delete, Update, Insert���s
                foreach (DataRowState sts in stats)
                {
                    updateTable = argDataTable.GetChanges(sts);
                    if (updateTable != null)
                    {
                        updateTable.TableName = "Table";
                        DataSet ds = new DataSet();
                        ds.Tables.Add(updateTable);
                        cnt += Adapter.Update(ds);
                    }
                }
            }
            catch (SqlException ex)
            {
                DataTable table = updateTable.GetChanges();
                // ���b�Z�[�W�R�[�h��ݒ�
                string msgCode = ex.Number == PKEY_ERR ? "WV001" : "EV002";

                CMMessage message;
                int rowNumber = CMUtil.GetRowNumber(table.Rows[0]);
                // �s�ԍ�����̏ꍇ
                if (rowNumber >= 0) message = new CMMessage(msgCode, new CMRowField(rowNumber), ex.Message);
                // �s�ԍ��Ȃ��̏ꍇ
                else message = new CMMessage(msgCode, ex.Message);
                // ��O����
                Log.Error(message.ToString(), ex);
                throw new CMException(message);
            }

            return cnt;
        }
        #endregion

        #region �č��ؐ�
        //************************************************************************
        /// <summary>
        /// �č��ؐՂ��L�^����B
        /// </summary>
        /// <param name="argCmdSetting">Command�ݒ�</param>
        /// <param name="argDataTable">�X�V�f�[�^���i�[����DataTable</param>
        /// <param name="argUpdateTime">�f�[�^�x�[�X�ɋL�^����X�V����</param>
        //************************************************************************
        protected void WriteAuditLog(CMCmdSetting argCmdSetting, 
            DataTable argDataTable, DateTime argUpdateTime)
        {
            // �o��OFF�̏ꍇ�͏o�͂��Ȃ�
            if (!Properties.Settings.Default.WriteAuditLog) return;

            // �}�X�^�ȊO�͑Ώ�
            if (!argCmdSetting.Name.StartsWith("CMSM")) return;

            // �R�l�N�V���������I�[�v������t���O
            bool isClosed = Connection.State == ConnectionState.Closed;

            try
            {
                // �R�l�N�V�������J��
                if (isClosed) Connection.Open();
                // INSERT���̐ݒ�
                IDbCommand cmd = CreateCommand(INSERT_AUDITLOG_SQL);
                // �p�����[�^�̐ݒ�
                cmd.Parameters.Add(CreateCmdParam("�e�[�u����", argCmdSetting.Name));
                cmd.Parameters.Add(CreateCmdParam("�X�V�敪", SqlDbType.Char));
                cmd.Parameters.Add(CreateCmdParam("�L�[", SqlDbType.NVarChar));
                cmd.Parameters.Add(CreateCmdParam("���e", SqlDbType.NVarChar));
                cmd.Parameters.Add(CreateCmdParam("�X�V����", argUpdateTime));
                cmd.Parameters.Add(CreateCmdParam("�X�V��ID", CMInformationManager.UserInfo.Id));
                cmd.Parameters.Add(CreateCmdParam("�X�V��IP", CMInformationManager.ClientInfo.MachineName));
                cmd.Parameters.Add(CreateCmdParam("�X�VPG", CMInformationManager.ClientInfo.FormId));

                StringBuilder key = new StringBuilder();
                StringBuilder content = new StringBuilder();

                // �o�^���[�v
                foreach (DataRow row in argDataTable.Rows)
                {
                    // �X�V�敪�̐ݒ�
                    string updType;
                    switch (row.RowState)
                    {
                        case DataRowState.Added:
                            updType = "C";
                            break;
                        case DataRowState.Modified:
                            updType = "U";
                            break;
                        case DataRowState.Deleted:
                            updType = "D";
                            break;
                        default:
                            continue;
                    }
                    ((IDbDataParameter)cmd.Parameters["�X�V�敪"]).Value = updType;

                    // DataRowVersion�̔���
                    DataRowVersion ver = row.RowState == DataRowState.Deleted ?
                        DataRowVersion.Original : DataRowVersion.Default;

                    // �X�V��̂ݏo�̓t���O�ݒ�
                    bool onlyModCol =
                        row.RowState == DataRowState.Modified && row.HasVersion(DataRowVersion.Original);

                    key.Length = 0;
                    content.Length = 0;

                    // �e�[�u�����ڗ�Ń��[�v
                    foreach (var csRow in argCmdSetting.ColumnParams)
                    {
                        // �񖼂��擾
                        string srcCol = csRow.SourceColumn != null ? csRow.SourceColumn : csRow.Name;

                        // �t�H�[�}�b�g��ݒ�
                        string format = csRow.DbType == CMDbType.���t ?
                            "{0}:{1:yyyy/MM/dd}" : "{0}:{1}";

                        // �L�[����
                        if (csRow.IsKey)
                        {
                            if (key.Length > 0) key.Append(",");
                            key.AppendFormat(format, csRow.Name, row[srcCol, ver]);
                        }
                        // �]������
                        else
                        {
                            // �X�V��o�̓t���O��True�̂Ƃ��́A���ƒl���قȂ�Ƃ��̂ݏo��
                            if (onlyModCol && row[srcCol].ToString() != row[srcCol, DataRowVersion.Original].ToString()
                                || !onlyModCol)
                            {
                                if (content.Length > 0) content.Append(",");
                                content.AppendFormat(format, csRow.Name, row[srcCol, ver]);
                            }
                        }
                    }

                    // �L�[�̐ݒ�
                    ((IDbDataParameter)cmd.Parameters["�L�["]).Value = key.ToString();
                    // ���e�̐ݒ�
                    ((IDbDataParameter)cmd.Parameters["���e"]).Value = content.ToString();

                    // INSERT���s
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                Log.Error("�č��ؐՏo�̓G���[", ex);
            }
            finally
            {
                // �R�l�N�V���������
                if (isClosed) Connection.Close();
            }
        }
        #endregion
    }
}