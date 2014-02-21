using System;

namespace NEXS.ERP.CM.Common
{
    //************************************************************************
    /// <summary>
    /// ユーザ情報を格納するためのクラス
    /// </summary>
    //************************************************************************
    [Serializable]
    public class CMUserInfo
    {
        /// <summary>ユーザＩＤ</summary>
        public string Id { get; set; }

        /// <summary>ユーザ名</summary>
        public string Name { get; set; }

        /// <summary>ロール</summary>
        public string[] Roles { get; set; }

        /// <summary>組織コード</summary>
        public string SoshikiCd { get; set; }

        /// <summary>組織名</summary>
        public string SoshikiName { get; set; }

        /// <summary>組織階層区分</summary>
        public string SoshikiKaisoKbn { get; set; }
    }
}
