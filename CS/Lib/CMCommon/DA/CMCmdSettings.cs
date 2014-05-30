/*******************************************************************************
 * 【共通部品】
 *
 * 作成者: 日進テクノロジー／田中 望
 * 改版履歴:
 * 2014.1.30, 新規作成
 ******************************************************************************/
using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;

using NEXS.ERP.CM.Common;

namespace NEXS.ERP.CM.DA
{
    //************************************************************************
    /// <summary>
    /// SqlCommand設定コレクションクラス
    /// </summary>
    //************************************************************************
    public class CMCmdSettings
    {
        #region プロパティ
        [Category("共通部品")]
        [Description("SqlCommand設定のコレクション")]
        public List<CMCmdSetting> CmdSettings { get; set; }
        #endregion

        #region インデクサ
        /// <summary>指定IndexのSqlCommand設定を返します。</summary>
        public CMCmdSetting this[int argIndex]
        {
            get
            {
                return CmdSettings[argIndex];
            }
        }

        /// <summary>指定のテーブル名のSqlCommand設定を返します。</summary>
        public CMCmdSetting this[string argName]
        {
            get
            {
                var result = from row in CmdSettings
                             where row.Name == argName
                             select row;
                return result.First();
            }
        }
        #endregion

        #region コンストラクタ
        //************************************************************************
        /// <summary>
        /// コンストラクタ
        /// </summary>
        //************************************************************************
        public CMCmdSettings()
        {
            CmdSettings = new List<CMCmdSetting>();
        }

        //************************************************************************
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="argFname">読み込むXMLファイル名(拡張子なし)</param>
        //************************************************************************
        public CMCmdSettings(string argFname) : this()
        {
            AddFomXml(argFname);
        }
        #endregion

        //************************************************************************
        /// <summary>
        /// SqlCommand設定をXMLファイルから追加する。
        /// </summary>
        /// <param name="argFnames">読み込むXMLファイル名(拡張子なし)</param>
        //************************************************************************
        public void AddFomXml(params string[] argFnames)
        {
            foreach (string fname in argFnames)
            {
                // データセットにファイルを読み込み
                CM項目DataSet ds = new CM項目DataSet();
                ds.ReadXml(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Model", fname + ".xml"));

                CMCmdSetting cmdSetting = new CMCmdSetting();

                // テーブル名を設定
                cmdSetting.Name = ds.項目一覧[0].項目一覧ID;

                // パラメータ設定
                List<CMCmdParam> paramList = new List<CMCmdParam>();
                foreach (var row in ds.項目)
                {
                    // 更新対象外は無視
                    if (row.入力制限 == "不可") continue;

                    CMCmdParam cmdParam = new CMCmdParam();
                    cmdParam.Name = row.項目名;
                    cmdParam.DbType = (CMDbType)Enum.Parse(typeof(CMDbType), row.項目型);
                    cmdParam.IsKey = row.主キー;
                    cmdParam.IsDecimal = row.小数桁 > 0;

                    paramList.Add(cmdParam);
                }
                cmdSetting.ColumnParams = paramList.ToArray();

                // 設定を追加
                CmdSettings.Add(cmdSetting);
            }
        }
    }
}
