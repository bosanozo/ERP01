using System;
using System.Collections;
using System.Web;

namespace NEXS.ERP.CM.Common
{
    //************************************************************************
    /// <summary>
    /// アプリケーション情報、ユーザ情報等へのアクセスを提供します。
    /// </summary>
    //************************************************************************
    public static class CMInformationManager
    {
        private static CMClientInfo m_clientInfo;

        //************************************************************************
        /// <summary>
        /// ユーザ情報
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
                    uinfo.Name = "テスト０１";
                    uinfo.SoshikiCd = "0001";
                    uinfo.SoshikiName = "組織0001";
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
        /// クライアント情報
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
