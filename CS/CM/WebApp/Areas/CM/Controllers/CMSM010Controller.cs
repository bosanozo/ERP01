using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using NEXS.ERP.CM.WEB;

namespace WebApp.Areas.CM.Controllers
{
    //************************************************************************
    /// <summary>
    /// 組織マスタメンテ コントローラ
    /// </summary>
    //************************************************************************
    [Authorize]
    public class CMSM010Controller : BaseController
    {
        // GET: CM/CMSM010
        public ActionResult Index()
        {
            ViewBag.CommonBL = CommonBL;

            return View();
        }

        public ActionResult Detail()
        {
            ViewBag.CommonBL = CommonBL;

            return View();
        }
    }
}