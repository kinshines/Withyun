using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Withyun.Core.Dtos;
using Withyun.Infrastructure.Utility;
using Withyun.Web.Models;

namespace Withyun.Controllers
{
    public class HomeController : Controller
    {
        [ResponseCache(Duration = 1*60*60, Location = ResponseCacheLocation.Any)]
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