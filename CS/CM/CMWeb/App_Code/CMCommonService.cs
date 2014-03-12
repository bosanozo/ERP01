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
    /// <param name="argCodeId">コード値要素ID属性</param>
    /// <param name="argNameId">名称要素ID属性</param>
    /// <param name="argSelectId">共通検索ID</param>
    /// <param name="argCode">コード値</param>
    /// <returns>コード値に対する名称</returns>
    //************************************************************************
    [OperationContract]
    [WebGet]
    public CodeName GetCodeName(string argCodeId, string argNameId, string argSelectId, string argCode)
	{
        string name = "";

        // 検索実行
        DataTable result = m_commonBL.Select(argSelectId, argCode);
        if (result != null && result.Rows.Count > 0)
            name = result.Rows[0][0].ToString();

        // 結果を返却
        return new CodeName() { CodeId = argCodeId, NameId = argNameId, Name = name };
    }
    #endregion

    //************************************************************************
    /// <summary>
    /// コード値に対する名称返却クラス
    /// </summary>
    //************************************************************************
    public class CodeName
    {
        /// <summary>コード値要素ID属性</summary>
        public string CodeId { get; set; }
        /// <summary>名称要素ID属性</summary>
        public string NameId { get; set; }
        /// <summary>コード値に対する名称</summary>
        public string Name { get; set; }
    }
}
