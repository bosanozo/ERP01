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

using NEXS.ERP.CM.Common;

namespace NEXS.ERP.CM.BL
{
    //************************************************************************
    /// <summary>
    /// ���ʏ����t�@�T�[�h�w�̃C���^�t�F�[�X
    /// </summary>
    //************************************************************************
    [Implementation(typeof(CMCommonBL))]
    public interface ICMCommonBL
    {
        //************************************************************************
        /// <summary>
        /// ���ݎ������擾����B
        /// </summary>
        /// <returns>���ݎ���</returns>
        //************************************************************************
        DateTime GetSysdate();

        //************************************************************************
        /// <summary>
        /// �w�肳�ꂽ����ID�̌������w�肳�ꂽ�����Ŏ��s����B
        /// </summary>
        /// <param name="argSelectId">����ID</param>
        /// <param name="argParams">�p�����[�^</param>
        /// <returns>��������</returns>
        //************************************************************************
        DataTable Select(string argSelectId, params object[] argParams);

        //************************************************************************
        /// <summary>
        /// ���ʌ����Ăяo���p�����Ɏw�肳�ꂽ����ID�̌��������s����B
        /// ���ʌ����Ăяo���p�����͕����w��\�Ƃ��A�������ʂ�DataSet�Ɋi�[����B
        /// </summary>
        /// <param name="args">���ʌ����Ăяo���p����</param>
        /// <returns>��������</returns>
        //************************************************************************
        DataSet Select(params CMCommonSelectArgs[] args);

        //************************************************************************
        /// <summary>
        /// ���샍�O���L�^����B
        /// </summary>
        /// <param name="argFormName">��ʖ�</param>
        /// <returns>���ݎ���</returns>
        //************************************************************************
        DateTime WriteOperationLog(string argFormName);

        //************************************************************************
        /// <summary>
        /// �ėp��l����敪�l���̂��擾����B
        /// </summary>
        /// <param name="argKbnList">��l����CD�̃��X�g</param>
        /// <returns>�敪�l���̂�DataTable</returns>
        //************************************************************************
        DataTable SelectKbn(params string[] argKbnList);

        //************************************************************************
        /// <summary>
        /// �Q�Ɣ͈�, �X�V������������B
        /// </summary>
        /// <param name="argFormId">��ʂh�c</param>
        /// <param name="argIsRange">True:�Q�Ɣ͈�, False:�X�V����</param>
        /// <returns>True:��ЁA�X�V��, False:���_�A�X�V�s��</returns>
        //************************************************************************
        bool GetRangeCanUpdate(string argFormId, bool argIsRange);

        //************************************************************************
        /// <summary>
        /// �w�肳�ꂽ����ID�̌������w�肳�ꂽ�����Ŏ��s����B
        /// </summary>
        /// <param name="argSelectId">����ID</param>
        /// <param name="argParam">��������</param>
        /// <param name="argMessage">���ʃ��b�Z�[�W</param>
        /// <returns>��������</returns>
        //************************************************************************
        DataTable SelectSub(string argSelectId, List<CMSelectParam> argParam,
            out CMMessage argMessage);

        //************************************************************************
        /// <summary>
        /// �X�V�҂��w�肳�ꂽ�����Ō�������B
        /// </summary>
        /// <param name="argParam">��������</param>
        /// <param name="argTables">�e�[�u�����̔z��</param>
        /// <param name="argMessage">���ʃ��b�Z�[�W</param>
        /// <returns>��������</returns>
        //************************************************************************
        DataTable SelectUpdSub(List<CMSelectParam> argParam, string[] argTables,
            out CMMessage argMessage);
    }
}
