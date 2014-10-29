﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

using NEXS.ERP.CM.WEB;

namespace WebApp.Controllers
{
    [Authorize]
    public class HomeController : BaseController
    {
        public ActionResult Index()
        {
            return View();
        }
    }
}
