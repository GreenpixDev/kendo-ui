using System;
using System.Collections.Generic;
using Reports.Web.Auth;
using Reports.Web.Models;

namespace Reports.Web.Services
{
    /// <summary>
    /// Данные отчётов с диаграммой Маримекко. Реальная реализация выполняет в БД
    /// <c>GROUP BY column, segment</c> с <c>SUM(value)</c> по источнику формы (AsNoTracking, только чтение);
    /// пока подключена демонстрационная <see cref="DemoReportDataService"/>.
    /// </summary>
    public interface IReportDataService
    {
        /// <summary>Формы, доступные пользователю.</summary>
        IList<ReportInfo> GetReports(ICurrentUser user);

        /// <summary>Плоские строки отчёта; null - формы нет или нет доступа (404). Ошибка фильтра - <see cref="ReportFilterException"/>.</summary>
        IList<MarimekkoRow> GetMarimekko(Guid reportId, string filter, ICurrentUser user);
    }
}
