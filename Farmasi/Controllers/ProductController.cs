using Farmasi.Repositories;
using Farmasi.Utils;
using Farmasi.ViewModels;
using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace Farmasi.Controllers
{
    [ValidateUserFilter]
    public class ProductController : BaseController
    {
        public ActionResult Catalog(int id)
        {
            ViewBag.Title = string.Format("{0} - {1}", Resources.Translator.Farmasi, Resources.Translator.Catalog);
            List<ProductViewModel> products = ProductRepository.GetProducts(id);
            products.ForEach(p =>
            {
                p.ImageBase64 = p.Image == null ? string.Empty : Convert.ToBase64String(p.Image);
                p.Image = null;
            });
            ViewBag.products = products;

            return View();
        }

        public ActionResult Details(int id)
        {
            ViewBag.Title = string.Format("{0} - {1}", Resources.Translator.Farmasi, Resources.Translator.ProductDetails);
            ViewBag.product = ProductRepository.GetProductDetails(id);

            return View();
        }

        public JsonResult GetProductOuts(DateTime fromDate, DateTime toDate, int? cid)
        {
            return Json(ProductRepository.GetProductOuts(fromDate, toDate, cid), JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetContragentRelation(DateTime fromDate, DateTime toDate)
        {
            return Json(ProductRepository.GetContragentRelation(fromDate, toDate), JsonRequestBehavior.AllowGet);
        }

        public JsonResult SearchCatalogProducts(string code)
        {
            List<CatalogProductViewModel> products = ProductRepository.SearchCatalogProducts(code);
            products.ForEach(p =>
            {
                p.ImageBase64 = p.Image == null ? string.Empty : Convert.ToBase64String(p.Image);
                p.Image = null;
            });
            return Json(products, JsonRequestBehavior.AllowGet);
        }
    }
}