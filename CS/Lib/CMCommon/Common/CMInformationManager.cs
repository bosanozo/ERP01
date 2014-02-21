using System;
using System.Collections;
using System.Web;

namespace NEXS.ERP.CM.Common
{
    //************************************************************************
    /// <summary>
    /// �A�v���P�[�V�������A���[�U��񓙂ւ̃A�N�Z�X��񋟂��܂��B
    /// </summary>
    //************************************************************************
    public static class CMInformationManager
    {
        private static CMClientInfo m_clientInfo;

        //************************************************************************
        /// <summary>
        /// ���[�U���
        /// </summary>
        //************************************************************************
        public static CMUserInfo UserInfo
        {
            get
            {
                if (HttpContext.Current.Session["UserInfo"] == null)
                {
                    var uinfo = new CMUserInfo();
                    uinfo.Id = "TEST01";
                    uinfo.Name = "�e�X�g�O�P";
                    uinfo.SoshikiCd = "0001";
                    uinfo.SoshikiName = "�g�D0001";
                    uinfo.SoshikiKaisoKbn = CMSoshikiKaiso.ALL;
                    return uinfo;
                }

                return HttpContext.Current.Session["UserInfo"] as CMUserInfo;
            }
            set
            {
                HttpContext.Current.Session["UserInfo"] = value;
            }
        }

        //************************************************************************
        /// <summary>
        /// �N���C�A���g���
        /// </summary>
        //************************************************************************
        public static CMClientInfo ClientInfo
        {
            get
            {
                if (m_clientInfo == null)
                    m_clientInfo = new CMClientInfo();

                return m_clientInfo;
            }
        }
    }
}
