using System;
using System.Web;

namespace NEXS.ERP.CM.Common
{
    //************************************************************************
    /// <summary>
    /// クライアントからサーバに送信された情報を保持するクラスです。
    /// </summary>
    //************************************************************************
    [Serializable]
    public class CMClientInfo
    {
        //************************************************************************
        /// <summary>
        /// クライアントのマシン名。
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
        /// 今回のサーバ呼び出しリクエストID。
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
