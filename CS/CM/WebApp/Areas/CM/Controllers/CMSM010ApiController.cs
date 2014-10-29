using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;

using NEXS.ERP.CM.Common;
using NEXS.ERP.CM.BL;
using NEXS.ERP.CM.WEB;

namespace WebApp.Areas.CM.Controllers
{
    //************************************************************************
    /// <summary>
    /// 組織マスタメンテ APIコントローラ
    /// </summary>
    //************************************************************************
    [Authorize]
    public class CMSM010ApiController : BaseApiController
    {
        #region BLインジェクション用フィールド
        protected CMSM010BL m_facade;
        #endregion

        #region コンストラクタ
        //************************************************************************
        /// <summary>
        /// コンストラクタ
        /// </summary>
        //************************************************************************
        public CMSM010ApiController()
        {
        }
        #endregion

        // GET: api/CMSM010Api
        public object Get(string _search) // , int rows, int page, string sord
        {
            // 検索種別取得
            CMSelectType selType = GetSelectType(_search);
            
            // 検索パラメータ作成
            var argParam = CMSelectParam.CreateSelectParam(
                HttpContext.Current.Request.QueryString,
                selType == CMSelectType.Edit ? null : "CMSM組織検索条件");

            // 検索実行
            var result = DoSearch(m_facade, selType, argParam);

            return result;
        }

        // POST: api/CMSM010Api
        public object Post([FromBody]PostParam argParam)
        {
            // 編集、キャンセル、登録実行
            return DoEdit(m_facade, argParam, HttpContext.Current.Request.Form);
        }

        // 行追加
        // PUT: api/CMSM010Api
        public string Put()
        {
            // 行追加実行
            return DoAdd(HttpContext.Current.Request.Form);
        }

        // 行削除
        // DELETE: api/CMSM010Api
        public ResultStatus Delete([FromBody]PostParam argParam)
        {
            // 行削除実行
            return DoDelete(argParam);
        }
    }
}
