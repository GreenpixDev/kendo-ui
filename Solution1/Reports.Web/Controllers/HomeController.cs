using System.Web.Mvc;

namespace Reports.Web.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return RedirectToAction("Index", "Marimekko");
        }
    }
}
