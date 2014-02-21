/*******************************************************************************
 * 【共通部品】
 *
 * 作成者: 日進テクノロジー／田中 望
 * 改版履歴:
 * 2014.1.30, 新規作成
 ******************************************************************************/
using System;
using System.Data;
using System.IO;

namespace NEXS.ERP.CM.Common
{
    //************************************************************************
    /// <summary>
    /// メッセージソース
    /// </summary>
    //************************************************************************
    public class CMMessageManager
    {
        private static CMMessageDataSet.MessageDataTable s_messageTable;

        /// <summary>
        /// メッセージファイル
        /// </summary>
        public static string MessageFileDir { get; set; }

        //************************************************************************
        /// <summary>
        /// メッセージ定義文字列を返す。
        /// </summary>
        /// <param name="argMessageCode">メッセージコード</param>
        /// <param name="argParams">パラメータ</param>
        /// <returns>メッセージ</returns>
        //************************************************************************
        public static string GetMessage(string argMessageCode, params object[] argParams)
        {
            // メッセージ定義の読み込み
            if (s_messageTable == null)
            {
                s_messageTable = new CMMessageDataSet.MessageDataTable();

                string[] files = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory +
                    Path.DirectorySeparatorChar + MessageFileDir, "Message*.xml");
                foreach (string file in files) s_messageTable.ReadXml(file);
            }

            // メッセージ定義の取得
            DataRow[] rows = s_messageTable.Select("Code = '" + argMessageCode + "'");
            if (rows.Length == 0) throw new Exception("Message.xmlに\"" + argMessageCode + "\"が登録されていません。");
            return string.Format(rows[0]["Format"].ToString(), argParams);
        }
    }
}
    