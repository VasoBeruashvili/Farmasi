using Farmasi.Repositories;
using Farmasi.Utils;
using Farmasi.ViewModels;
using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace Farmasi.Controllers
{
    [ValidateUserFilter]
    public class HomeController : BaseController
    {
        public ActionResult Index()
        {
            ViewBag.Title = string.Format("{0} - {1}", Resources.Translator.Farmasi, Resources.Translator.Main);
            ViewBag.NewlyAddedProducts = ProductRepository.GetNewlyAddedProducts();

            return View();
        }

        public JsonResult GetRegisteredStaffs(DateTime fromDate, DateTime toDate)
        {
            return Json(GeneralRepository.GetRegisteredStaffs(fromDate, toDate), JsonRequestBehavior.AllowGet);
        }
    }
}