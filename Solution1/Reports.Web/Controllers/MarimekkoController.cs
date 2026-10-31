using System.Web.Mvc;
using Reports.Web.Auth;

namespace Reports.Web.Controllers
{
    [ReportUserAuthorize]
    public class MarimekkoController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.UserName = CurrentUserResolver.Current(HttpContext).DisplayName;
            return View();
        }
    }
}
