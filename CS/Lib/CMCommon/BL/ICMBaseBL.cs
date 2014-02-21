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

using NEXS.ERP.CM.Common;

namespace NEXS.ERP.CM.BL
{
    //************************************************************************
    /// <summary>
    /// �t�@�T�[�h�C���^�[�t�F�[�X
    /// </summary>
    //************************************************************************
    public interface ICMBaseBL
    {
        //************************************************************************
        /// <summary>
        /// ��������B
        /// </summary>
        /// <param name="argParam">��������</param>
        /// <param name="argSelectType">�������</param>
        /// <param name="argOperationTime">���쎞��</param>
        /// <param name="argMessage">���ʃ��b�Z�[�W</param>
        /// <returns>��������</returns>
        //************************************************************************
        DataSet Select(List<CMSelectParam> argParam, CMSelectType argSelectType,
            out DateTime argOperationTime, out CMMessage argMessage);

        //************************************************************************
        /// <summary>
        /// �f�[�^��o�^����B
        /// </summary>
        /// <param name="argUpdateData">�X�V�f�[�^</param>
        /// <param name="argOperationTime">���쎞��</param>
        /// <returns>�o�^�������R�[�h��</returns>
        //************************************************************************
        int Update(DataSet argUpdateData, out DateTime argOperationTime);
    }
}
