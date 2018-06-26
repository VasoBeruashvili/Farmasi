using System.Web.Mvc;

namespace Farmasi.Utils
{
    public class ValidateUserFilter : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (filterContext.HttpContext.Session["currentUser"] == null)
            {
                if (!filterContext.HttpContext.Request.IsAjaxRequest())
                {
                    filterContext.Result = new RedirectResult("/account/login");
                }
            }
        }
    }
}