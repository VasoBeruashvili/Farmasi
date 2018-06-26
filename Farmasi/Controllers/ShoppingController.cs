using Farmasi.Repositories;
using Farmasi.Utils;
using Farmasi.ViewModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Farmasi.Controllers
{
    [ValidateUserFilter]
    public class ShoppingController : BaseController
    {
        public ActionResult Cart()
        {
            List<ProductViewModel> products = ShoppingRepository.GetCartProducts();
            products.ForEach(p =>
            {
                if(p.Image != null)
                {
                    p.ImageBase64 = Convert.ToBase64String(p.Image);
                    p.Image = null;
                }
            });
            ViewBag.products = products;

            return View();
        }

        public JsonResult AddToCart(int productId, int quantity)
        {
            return Json(ShoppingRepository.AddToCart(productId, quantity));
        }

        public JsonResult RemoveFromCart(int productId, int quantity)
        {
            return Json(ShoppingRepository.RemoveFromCart(productId, quantity));
        }
        
        public JsonResult PlaceOrder()
        {
            return Json(ShoppingRepository.PlaceOrder());
        }
    }
}