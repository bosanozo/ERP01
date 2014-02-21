using System;
using System.Web;

namespace NEXS.ERP.CM.Common
{
    //************************************************************************
    /// <summary>
    /// �N���C�A���g����T�[�o�ɑ��M���ꂽ����ێ�����N���X�ł��B
    /// </summary>
    //************************************************************************
    [Serializable]
    public class CMClientInfo
    {
        //************************************************************************
        /// <summary>
        /// �N���C�A���g�̃}�V�����B
        /// </summary>
        //************************************************************************
        public string MachineName
        {
            get
            {
                return HttpContext.Current.Request.UserHostAddress;
            }
        }

        //************************************************************************
        /// <summary>
        /// ����̃T�[�o�Ăяo�����N�G�X�gID�B
        /// </summary>
        //************************************************************************
        public string FormId
        {
            get
            {
                return System.IO.Path.GetFileNameWithoutExtension(HttpContext.Current.Request.Path);
            }
        }
    }
}
