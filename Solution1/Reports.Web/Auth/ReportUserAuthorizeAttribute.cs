using System.Net;
using System.Web.Mvc;

namespace Reports.Web.Auth
{
    /// <summary>
    /// Проверка входа и роли модуля (п. 5.1 брифа).
    /// Страница: нет входа - перенаправление на вход АИС, нет роли - 403 и «Нет доступа».
    /// API (<see cref="Api"/> = true): 401 / 403 с телом { code, message, details } без перенаправления.
    /// </summary>
    public sealed class ReportUserAuthorizeAttribute : FilterAttribute, IAuthorizationFilter
    {
        public bool Api { get; set; }
        public bool SpecialistOnly { get; set; }

        public void OnAuthorization(AuthorizationContext filterContext)
        {
            var user = CurrentUserResolver.Resolve(filterContext.HttpContext);
            filterContext.HttpContext.Items[CurrentUserResolver.ItemKey] = user;

            if (!user.IsAuthenticated)
            {
                filterContext.Result = Api
                    ? ApiError.Result(HttpStatusCode.Unauthorized, "UNAUTHORIZED", "Требуется вход в систему.")
                    : (ActionResult)new RedirectResult(CurrentUserResolver.LoginUrl);
            }
            else if (!user.IsUser || (SpecialistOnly && !user.IsSpecialist))
            {
                if (Api)
                {
                    filterContext.Result = ApiError.Result(HttpStatusCode.Forbidden, "FORBIDDEN", "Нет доступа.");
                }
                else
                {
                    filterContext.HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                    filterContext.Result = new ContentResult
                    {
                        Content = "<h2>Нет доступа</h2><p>Для вашей учётной записи не назначена роль модуля отчётов.</p>",
                        ContentType = "text/html; charset=utf-8"
                    };
                }
            }
        }
    }
}
