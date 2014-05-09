using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;
using System.Text;

using Seasar.Quill;

using NEXS.ERP.CM.Common;
using NEXS.ERP.CM.BL;

//************************************************************************
/// <summary>
/// 共通検索AJAXサービス
/// </summary>
//************************************************************************
[ServiceContract(Namespace = "")]
[AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
public class CMCommonService
{
    #region インジェクション用フィールド
    protected ICMCommonBL m_commonBL;
    #endregion

    #region コンストラクタ
    //************************************************************************
    /// <summary>
    /// コンストラクタ
    /// </summary>
    //************************************************************************
    public CMCommonService()
    {
        // インジェクション実行
        QuillInjector injector = QuillInjector.GetInstance();
        injector.Inject(this);
    }
    #endregion

    #region サービスメソッド
    //************************************************************************
    /// <summary>
    /// コード値から名称を取得する。
    /// </summary>
    /// <param name="argCode">コード値</param>
    /// <param name="argSelectId">共通検索ID</param>
    /// <param name="argSelectParam">共通検索パラメータ</param>
    /// <returns>コード値に対する名称</returns>
    //************************************************************************
    [OperationContract]
    [WebGet]
    public CodeName GetCodeName(string argCode, string argSelectId, string argSelectParam)
	{
        string name = "";

        List<object> paramList = new List<object>();
        paramList.Add(argCode);

        // 共通検索パラメータ作成
        if (!string.IsNullOrEmpty(argSelectParam))
        {
            foreach (string p0 in argSelectParam.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                string p = p0.TrimStart();
                if (p.Length < 2) continue;

                // 'から始まる場合はそのまま設定
                if (p[0] == '\'') paramList.Add(p.Substring(1));
                // "#"から始まる場合はUserInfoから設定
                else if (p[0] == '#')
                {
                    System.Reflection.PropertyInfo pi = CMInformationManager.UserInfo.GetType().GetProperty(p.Substring(1));
                    paramList.Add(pi.GetValue(CMInformationManager.UserInfo, null));
                }
                // Rowの値を取得
                //else paramList.Add(row[p]);
            }
        }

        // 検索実行
        DataTable result = m_commonBL.Select(argSelectId, paramList.ToArray());
        if (result != null && result.Rows.Count > 0)
            name = result.Rows[0][0].ToString();

        // 結果を返却
        return new CodeName() { Name = name };
    }
    #endregion

    //************************************************************************
    /// <summary>
    /// コード値に対する名称返却クラス
    /// </summary>
    //************************************************************************
    public class CodeName
    {
        /// <summary>コード値に対する名称</summary>
        public string Name { get; set; }
    }
}
