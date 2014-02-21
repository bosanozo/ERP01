using Seasar.Quill.Attrs;

using NEXS.ERP.CM.Common;

namespace NEXS.ERP.CM.BL
{
    //************************************************************************
    /// <summary>
    /// 認証関連の機能を提供するファサードのインターフェイスです。
    /// </summary>
    //************************************************************************
    [Implementation(typeof(CMAuthenticationBL))]
    public interface ICMAuthenticationBL
    {
        //************************************************************************
        /// <summary>
        /// ユーザIDとパスワードによる認証を実行します。
        /// このメソッドは、ローカル呼び出しでユーザIDとパスワードの検証だけを行う、
        /// Webアプリケーション等用のメソッドです。
        /// </summary>
        /// <param name="userId">ユーザID。</param>
        /// <param name="password">パスワード。</param>
        /// <returns>ユーザが認証できた場合は true 。</returns>
        //************************************************************************
        bool Authenticate(string userId, string password);

        //************************************************************************
        /// <summary>
        /// ユーザIDで指定されたユーザのユーザ情報を取得します。
        /// このメソッドは、主にサーバ側で認証済みのユーザ情報を取得するために
        /// 使用されます。
        /// </summary>
        /// <param name="userId">ユーザID。</param>
        /// <returns>ユーザ情報。ユーザ情報が取得できなかった場合は null参照。</returns>
        //************************************************************************
        CMUserInfo GetUserInfo(string userId);
    }
}
