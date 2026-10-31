using System.Web;
using System.Web.Mvc;
using Reports.Web.Auth;

namespace Reports.Web.Controllers
{
    /// <summary>
    /// Заглушка входа АИС ГП для тестового контура (п. 5.1 брифа). Включается настройкой
    /// appSettings "DevAuth:Enabled"; при выключенной настройке страниц не существует (404).
    /// </summary>
    public class DevController : Controller
    {
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (!CurrentUserResolver.DevAuthEnabled)
            {
                filterContext.Result = new HttpNotFoundResult();
            }
            base.OnActionExecuting(filterContext);
        }

        // GET /Dev/Login            - выбор пользователя
        // GET /Dev/Login?as=user    - войти (specialist | user | norole), as=none - выйти
        public ActionResult Login(string @as)
        {
            if (@as == null)
            {
                ViewBag.Current = CurrentUserResolver.Resolve(HttpContext).DisplayName;
                return View();
            }

            var cookie = new HttpCookie(CurrentUserResolver.DevCookie, @as) { HttpOnly = true };
            if (@as == "none") { cookie.Expires = System.DateTime.UtcNow.AddDays(-1); }
            Response.Cookies.Add(cookie);
            return RedirectToAction("Login");
        }
    }
}
