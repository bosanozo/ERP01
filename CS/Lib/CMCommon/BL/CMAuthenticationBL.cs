/*******************************************************************************
 * 【共通部品】
 *
 * 作成者: 日進テクノロジー／田中 望
 * 改版履歴:
 * 2014.1.30, 新規作成
 ******************************************************************************/
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Web;

using NEXS.ERP.CM.Common;
using NEXS.ERP.CM.DA;

namespace NEXS.ERP.CM.BL
{
    //************************************************************************
    /// <summary>
    /// 認証管理ファサード
    /// </summary>
    //************************************************************************
    public class CMAuthenticationBL : CMBaseBL, ICMAuthenticationBL
    {
        private const string ID = "ID";
        private const string NAME = "NAME";
        private const string PASSWD = "PASSWD";
        private const string ROLE = "ROLE";

        #region インジェクション用フィールド
        protected CMUserInfoDA m_dataAccess;
        #endregion

        #region コンストラクタ
        //************************************************************************
        /// <summary>
        /// コンストラクタ
        /// </summary>
        //************************************************************************
        public CMAuthenticationBL() 
        {
        }
        #endregion

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
        public bool Authenticate(string userId, string password)
        {
            DataRow row = GetUserData(userId, password);

            // ユーザＩＤとパスワードから社員情報を取得（取得できれば true）
            if (row != null)
            {
                CMInformationManager.UserInfo = CreateUserInfo(row);
            }

            return row != null;
        }

        //************************************************************************
        /// <summary>
        /// ユーザIDで指定されたユーザのユーザ情報を取得します。
        /// このメソッドは、主にサーバ側で認証済みのユーザ情報を取得するために
        /// 使用されます。
        /// </summary>
        /// <param name="userId">ユーザID。</param>
        /// <returns>ユーザ情報。ユーザ情報が取得できなかった場合は null参照。</returns>
        //************************************************************************
        public CMUserInfo GetUserInfo(string userId)
        {
            // ユーザＩＤから社員情報を取得
            return CreateUserInfo(GetUserData(userId));
        }

        //************************************************************************
        /// <summary>
        /// DataRowから<see cref="CMUserInfo"/> を作成します。
        /// null を渡すと、例外にはならずに null参照を返します。
        /// このメソッドは、このファサードの各メソッドから使用される内部メソッドです。
        /// </summary>
        /// <param name="userData">ユーザの情報を格納しているDataRow。</param>
        /// <returns>ユーザ情報を保持する <see cref="CMUserInfo"/>。
        /// <paramref name="userData"/> が null の場合は null参照。</returns>
        //************************************************************************
        private CMUserInfo CreateUserInfo(DataRow userData)
        {
            if (userData == null) return null;

            CMUserInfo userInfo = new CMUserInfo();
            // ユーザ情報の設定
            userInfo.Id = userData[ID].ToString();
            userInfo.Name = userData[NAME].ToString();
            // ロールの設定
            userInfo.Roles = userData[ROLE].ToString().Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            userInfo.SoshikiCd = userData["組織CD"].ToString();
            userInfo.SoshikiName = userData["組織名"].ToString();
            userInfo.SoshikiKaisoKbn = userData["組織階層区分"].ToString();

            return userInfo;
        }

        //************************************************************************
        /// <summary>
        /// ユーザIDとパスワードで指定されたユーザの情報を格納しているDataRowを取得します。
        /// このメソッドは、このファサードの各メソッドから使用される内部メソッドです。
        /// </summary>
        /// <param name="userId">ユーザID。</param>
        /// <param name="password">パスワード。</param>
        /// <returns>指定されたユーザの情報を格納しているDataRow。
        /// 対象のユーザ情報が取得できなかった場合は null参照。</returns>
        //************************************************************************
        private DataRow GetUserData(string userId, string password)
        {
            // ユーザのパスワードチェックをプロジェクトでカスタマイズする場合は、
            // 主にこのメソッドをカスタマイズします（パスワードを直接チェックする場合のみ）。

            // ユーザＩＤから社員情報を取得
            DataRow userData = GetUserData(userId);
            if (userData == null) return null;

            // パスワードのチェック（パスワードは常に大文字小文字を区別してチェック）
            if (password != userData[PASSWD].ToString()) return null;

            return userData;
        }

        //************************************************************************
        /// <summary>
        /// ユーザIDで指定されたユーザの情報を格納しているDataRowを取得します。
        /// このメソッドは、このファサードの各メソッドから使用される内部メソッドです。
        /// </summary>
        /// <param name="userId">ユーザID。</param>
        /// <returns>指定されたユーザの情報を格納しているDataRow。
        /// 対象のユーザ情報が取得できなかった場合は null参照。</returns>
        //************************************************************************
        private DataRow GetUserData(string userId)
        {
            // データアクセス層作成
            m_dataAccess.Connection = Connection;
            // ユーザー情報取得
            return m_dataAccess.FindById(userId);
        }
    }
}
