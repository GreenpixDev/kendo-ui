using System.Net;
using System.Web.Mvc;

namespace Reports.Web.Auth
{
    /// <summary>Ошибка API по контракту модуля (п. 9.2 брифа): { code, message, details }, сообщение на русском.</summary>
    public static class ApiError
    {
        public static JsonResult Result(HttpStatusCode status, string code, string message, string[] details = null)
        {
            return new StatusJsonResult(status)
            {
                Data = new { code = code, message = message, details = details ?? new string[0] },
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        private sealed class StatusJsonResult : JsonResult
        {
            private readonly HttpStatusCode _status;

            public StatusJsonResult(HttpStatusCode status) { _status = status; }

            public override void ExecuteResult(ControllerContext context)
            {
                context.HttpContext.Response.StatusCode = (int)_status;
                context.HttpContext.Response.TrySkipIisCustomErrors = true;
                base.ExecuteResult(context);
            }
        }
    }
}
