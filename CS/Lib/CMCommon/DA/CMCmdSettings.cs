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

namespace NEXS.ERP.CM.DA
{
    //************************************************************************
    /// <summary>
    /// SqlCommand設定コレクションクラス
    /// </summary>
    //************************************************************************
    public class CMCmdSettings : Component
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
                DataSet ds = new DataSet();
                ds.ReadXml(Path.Combine(HttpContext.Current.Request.PhysicalApplicationPath, "Model", fname + ".xml"));

                CMCmdSetting cmdSetting = new CMCmdSetting();

                // テーブル名を設定
                cmdSetting.Name = ds.Tables["エンティティ"].Rows[0]["テーブル名"].ToString();

                bool hasNotUpdate = ds.Tables["項目"].Columns.Contains("更新対象外");

                // パラメータ設定
                List<CMCmdParam> paramList = new List<CMCmdParam>();
                foreach (DataRow row in ds.Tables["項目"].Rows)
                {
                    // 更新対象外は無視
                    if (hasNotUpdate && row["更新対象外"].ToString() == "True") continue;

                    CMCmdParam cmdParam = new CMCmdParam();
                    cmdParam.Name = row["項目名"].ToString();
                    cmdParam.DbType = (CMDbType)Enum.Parse(typeof(CMDbType), row["項目型"].ToString());
                    cmdParam.IsKey = row["Key"].ToString() == "True";
                    if (row.Table.Columns.Contains("SourceColumn"))
                        cmdParam.SourceColumn = row["SourceColumn"].ToString();

                    paramList.Add(cmdParam);
                }
                cmdSetting.ColumnParams = paramList.ToArray();

                // 設定を追加
                CmdSettings.Add(cmdSetting);
            }
        }
    }
}
