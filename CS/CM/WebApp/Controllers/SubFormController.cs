using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;

using NEXS.ERP.CM.WEB;

namespace WebApp.Controllers
{
    //************************************************************************
    /// <summary>
    /// 検索サブ画面 コントローラ
    /// </summary>
    //************************************************************************
    [Authorize]
    public class SubFormController : BaseController
    {
        // GET: SubForm
        public ActionResult Index(string CodeId, string CodeLen)
        {
            // 検索コード名
            var codeName = Regex.Replace(CodeId, "(From|To)", "");

            ViewBag.CodeLabel = codeName.Replace("CD", "コード");
            ViewBag.NameLabel = Regex.Replace(codeName, "(CD|ID)", "名");

            return View();
        }
    }
}