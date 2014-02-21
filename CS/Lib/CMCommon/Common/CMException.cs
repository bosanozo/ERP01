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
using System.Runtime.Serialization;
using System.Text;

namespace NEXS.ERP.CM.Common
{
    //************************************************************************
    /// <summary>
    /// 業務アプリケーション例外クラス
    /// </summary>
    //************************************************************************
    [Serializable]
    public class CMException : Exception
    {
        #region プロパティ
        /// <summary>
        /// メッセージ
        /// </summary>
        public CMMessage CMMessage
        {
            get { return (CMMessage)Data["CMMessage"]; }
            set { Data.Add("CMMessage", value); }
        }
        #endregion

        #region コンストラクタ
        // 親クラスから引き継いだコンストラクタ
        public CMException() { }
        public CMException(string argMessage) : base(argMessage) { }
        public CMException(string argMessage, Exception argInnerException)
            : base(argMessage, argInnerException) { }
        public CMException(SerializationInfo argSerializationInfo, StreamingContext argStreamingContext)
            : base(argSerializationInfo, argStreamingContext) { }

        //************************************************************************
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="argCMMessage">メッセージデータ</param>
        //************************************************************************
        public CMException(CMMessage argCMMessage)
        {
            CMMessage = argCMMessage;
        }

        //************************************************************************
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="argCMMessage">メッセージデータ</param>
        /// <param name="argInnerException">例外発生の元となった例外</param>
        //************************************************************************
        public CMException(CMMessage argCMMessage, Exception argInnerException)
            : base(argInnerException.Message, argInnerException)
        {
            CMMessage = argCMMessage;
        }
        #endregion

        //************************************************************************
        /// <summary>
        /// メッセージ文字列を返す。
        /// </summary>
        //************************************************************************
        public override string ToString()
        {
            if (CMMessage != null)
            {
                // 全メッセージ文字列を連結
                StringBuilder builder = new StringBuilder(CMMessage.ToString());

                // メッセージがエラー以外の場合は簡略化する
                if (CMMessage.MessageCd.Length >= 1 && CMMessage.MessageCd[0] != 'E')
                {
                    if (InnerException != null)
                        builder.AppendLine().Append(InnerException.GetType().FullName)
                            .Append(": ").Append(InnerException.Message);
                }
                else builder.AppendLine().Append(base.ToString());

                return builder.ToString();
            }
            else return base.ToString();
        }
    }
}
