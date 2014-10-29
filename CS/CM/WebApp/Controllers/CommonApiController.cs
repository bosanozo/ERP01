using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;
using System.Web.Http;

using NEXS.ERP.CM.Common;
using NEXS.ERP.CM.BL;
using NEXS.ERP.CM.WEB;

namespace WebApp.Controllers
{
    //************************************************************************
    /// <summary>
    /// 共通 APIコントローラ
    /// </summary>
    //************************************************************************
    [Authorize]
    public class CommonApiController : BaseApiController
    {
        #region コンストラクタ
        //************************************************************************
        /// <summary>
        /// コンストラクタ
        /// </summary>
        //************************************************************************
        public CommonApiController()
        {
        }
        #endregion

        // GET: api/CommonApi
        public ResultData Get(string SelectId, string DbCodeCol, string DbNameCol, string CodeId, string CodeLen)
        {
            var query = HttpContext.Current.Request.QueryString;
            string Name = query["Name"];
            string Code = query["Code"];
            string Params = query["Params"];

            // 検索パラメータ取得
            List<CMSelectParam> param = CMSelectParam.CreateSelectParam(Name, Code, Params, DbCodeCol, DbNameCol, CodeId);

            // ファサードの呼び出し
            CMMessage message;
            DataTable table = CommonBL.SelectSub(SelectId, param, out message);

            // データ返却
            return ResultData.CreateResultData(table);
        }

        // GET: api/CommonApi
        public string Get(string argCode, string argSelectId, string argSelectParam)
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
            DataTable result = CommonBL.Select(argSelectId, paramList.ToArray());
            if (result != null && result.Rows.Count > 0)
                name = result.Rows[0][0].ToString();

            return name;
        }
    }
}
