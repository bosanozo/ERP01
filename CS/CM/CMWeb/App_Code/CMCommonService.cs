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
    // HTTP GET を使用するために [WebGet] 属性を追加します (既定の ResponseFormat は WebMessageFormat.Json)。
	// XML を返す操作を作成するには、
	//     [WebGet(ResponseFormat=WebMessageFormat.Xml)] を追加し、
	//     操作本文に次の行を含めます。
	//         WebOperationContext.Current.OutgoingResponse.ContentType = "text/xml";
	[OperationContract]
    [WebGet]
    public string GetCodeName(string argSelectId, string argCode)
	{
        string name = "";

        // 検索実行
        DataTable result = m_commonBL.Select(argSelectId, argCode);
        if (result != null && result.Rows.Count > 0)
            name = result.Rows[0][0].ToString();
        return name;
        //return new CodeName() { Code = argCode, Name = name };
    }
    #endregion

    /*
    public class CodeName
    {
        public string Code { get; set; }
        public string Name { get; set; }
    }*/
}
