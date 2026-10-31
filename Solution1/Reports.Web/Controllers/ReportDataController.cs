using System;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using Reports.Web.Auth;
using Reports.Web.Services;

namespace Reports.Web.Controllers
{
    /// <summary>JSON-данные отчётов. Контроллер тонкий: вся логика - в <see cref="IReportDataService"/>.</summary>
    [ReportUserAuthorize(Api = true)]
    public class ReportDataController : Controller
    {
        private readonly IReportDataService _data;

        public ReportDataController() : this(new DemoReportDataService()) { }

        public ReportDataController(IReportDataService data)
        {
            _data = data;
        }

        private ICurrentUser CurrentUser
        {
            get { return CurrentUserResolver.Current(HttpContext); }
        }

        // JSON отдаётся объектом, а не массивом; свойства в camelCase - с ними работает marimekko.js
        // (JavaScriptSerializer в MVC 5 сохраняет регистр C#, поэтому проекция в анонимные объекты обязательна).
        [HttpGet]
        [OutputCache(NoStore = true, Duration = 0)]
        public JsonResult List()
        {
            var reports = _data.GetReports(CurrentUser).Select(r => new
            {
                id = r.Id,
                name = r.Name,
                columnTitle = r.ColumnTitle,
                segmentTitle = r.SegmentTitle,
                unit = r.Unit
            }).ToList();
            return Json(new { reports = reports }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [OutputCache(NoStore = true, Duration = 0)]
        public JsonResult Marimekko(Guid? reportId, string filter)
        {
            try
            {
                // фильтр принимается только по полям формы (белый список), Request.QueryString в сервис не передаётся
                var rows = reportId.HasValue ? _data.GetMarimekko(reportId.Value, filter, CurrentUser) : null;
                if (rows == null)
                {
                    return ApiError.Result(HttpStatusCode.NotFound, "NOT_FOUND", "Форма отчёта не найдена.");
                }
                var result = rows.Select(r => new { column = r.Column, segment = r.Segment, value = r.Value }).ToList();
                return Json(new { rows = result }, JsonRequestBehavior.AllowGet);
            }
            catch (ReportFilterException ex)
            {
                return ApiError.Result((HttpStatusCode)422, "VALIDATION_ERROR", ex.Message, new[] { ex.Message });
            }
        }
    }
}
