using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using DevTrends.MvcDonutCaching;
using Domain.Services;
using NLogUtility;
using Withyun.ViewModels;


namespace Withyun.Controllers
{
    public class HomeController : Controller
    {
        //[OutputCache(Duration = 1*60*60, Location = OutputCacheLocation.Any)]
        [DonutOutputCache(Duration = 1 * 60 * 60, Location = OutputCacheLocation.Any)]
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            return View();
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Message(MessageViewModel model)
        {
            if (!string.IsNullOrWhiteSpace(model.Message))
            {
                Logger.Email("Message:{0},Contacts:{1}", model.Message, model.Contacts);
            }
            return Json(new OperationResult(true));
        }
    }
}