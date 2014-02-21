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
using System.Web;

using NEXS.ERP.CM.Common;
using NEXS.ERP.CM.DA;

using Seasar.Quill.Attrs;
using NEXS.ERP.CM.WEB;

namespace NEXS.ERP.CM.BL
{
    //************************************************************************
    /// <summary>
    /// ���ʏ����t�@�T�[�h�w
    /// </summary>
    //************************************************************************
    public class CMCommonBL : CMBaseBL, ICMCommonBL
    {
        #region �R���X�g���N�^
        //************************************************************************
        /// <summary>
        /// �R���X�g���N�^
        /// </summary>
        //************************************************************************
        public CMCommonBL() 
        {
        }
        #endregion

        #region �t�@�T�[�h���\�b�h
        //************************************************************************
        /// <summary>
        /// ���ݎ������擾����B
        /// </summary>
        /// <returns>���ݎ���</returns>
        //************************************************************************
        public DateTime GetSysdate()
        {
            CommonDA.Connection = Connection;
            return CommonDA.GetSysdate();
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
            // �������s
            CommonDA.Connection = Connection;
            DataTable result = CommonDA.Select(argSelectId, argParams);

            return result;
        }

        //************************************************************************
        /// <summary>
        /// ���ʌ����Ăяo���p�����Ɏw�肳�ꂽ����ID�̌��������s����B
        /// ���ʌ����Ăяo���p�����͕����w��\�Ƃ��A�������ʂ�DataSet�Ɋi�[����B
        /// </summary>
        /// <param name="args">���ʌ����Ăяo���p����</param>
        /// <returns>��������</returns>
        //************************************************************************
        public DataSet Select(params CMCommonSelectArgs[] args)
        {
            CommonDA.Connection = Connection;

            DataSet dataSet = new DataSet();
            // �������[�v
            foreach (var arg in args)
            {
                // �������s
                DataTable table = CommonDA.Select(arg.SelectId, arg.Params);
                // ����ID��ݒ�
                string tableName = arg.SelectId;
                int idx = 1;
                while (dataSet.Tables.IndexOf(tableName) >= 0) tableName = arg.SelectId + idx++;
                table.TableName = tableName;
                // DataSet�ɒǉ�
                dataSet.Tables.Add(table);
            }

            return dataSet;
        }

        //************************************************************************
        /// <summary>
        /// ���샍�O���L�^����B
        /// </summary>
        /// <param name="argFormName">��ʖ�</param>
        /// <returns>���ݎ���</returns>
        //************************************************************************
        public DateTime WriteOperationLog(string argFormName)
        {
            CommonDA.Connection = Connection;

            // ���샍�O�L�^
            CommonDA.WriteOperationLog(argFormName);

            // ���ݎ����ԋp
            return DateTime.Now;
        }

        //************************************************************************
        /// <summary>
        /// �ėp��l����敪�l���̂��擾����B
        /// </summary>
        /// <param name="argKbnList">��l����CD�̃��X�g</param>
        /// <returns>�敪�l���̂�DataTable</returns>
        //************************************************************************
        //[Aspect(typeof(CMWebInterceptor))]
        public virtual DataTable SelectKbn(params string[] argKbnList)
        {
            CommonDA.Connection = Connection;
            return CommonDA.SelectKbn(argKbnList);
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
            CommonDA.Connection = Connection;
            return CommonDA.GetRangeCanUpdate(argFormId, argIsRange);
        }

        //************************************************************************
        /// <summary>
        /// �w�肳�ꂽ����ID�̌������w�肳�ꂽ�����Ŏ��s����B
        /// </summary>
        /// <param name="argSelectId">����ID</param>
        /// <param name="argParam">��������</param>
        /// <param name="argMessage">���ʃ��b�Z�[�W</param>
        /// <returns>��������</returns>
        //************************************************************************
        public DataTable SelectSub(string argSelectId, List<CMSelectParam> argParam,
            out CMMessage argMessage)
        {
            // �������s
            bool isOver;
            CommonDA.Connection = Connection;
            DataTable result = CommonDA.SelectSub(argSelectId, argParam, out isOver);

            argMessage = null;
            // �������ʂȂ�
            if (result.Rows.Count == 0) argMessage = new CMMessage("IV001");
            // �ő匟�������I�[�o�[
            else if (isOver) argMessage = new CMMessage("IV002");

            return result;
        }

        //************************************************************************
        /// <summary>
        /// �X�V�҂��w�肳�ꂽ�����Ō�������B
        /// </summary>
        /// <param name="argParam">��������</param>
        /// <param name="argTables">�e�[�u�����̔z��</param>
        /// <param name="argMessage">���ʃ��b�Z�[�W</param>
        /// <returns>��������</returns>
        //************************************************************************
        public DataTable SelectUpdSub(List<CMSelectParam> argParam, string[] argTables,
            out CMMessage argMessage)
        {
            // �������s
            bool isOver;
            CommonDA.Connection = Connection;
            DataTable result = CommonDA.SelectUpdSub(argParam, argTables, out isOver);

            argMessage = null;
            // �������ʂȂ�
            if (result.Rows.Count == 0) argMessage = new CMMessage("IV001");
            // �ő匟�������I�[�o�[
            else if (isOver) argMessage = new CMMessage("IV002");

            return result;
        }
        #endregion
    }
}
