using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Withyun.Core.Enums;
using Withyun.Infrastructure.Services;

namespace Withyun.Web.Components
{
    public class Recomment : ViewComponent
    {
        readonly RecommentService _recommentService;
        public Recomment(RecommentService recommentService)
        {
            _recommentService = recommentService;
        }

        public IViewComponentResult Invoke(RecommentCategory id)
        {
            var list = _recommentService.GetRecommentsByCategory(id);
            ViewBag.Title = id.ToString();
            ViewBag.Icon = _recommentService.GetIconByCategory(id);
            return View(list);
        }
    }
}
