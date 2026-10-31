using System;
using System.Collections.Generic;
using System.Linq;
using Reports.Web.Auth;
using Reports.Web.Models;

namespace Reports.Web.Services
{
    /// <summary>Демонстрационные данные в памяти: контрольный набор приёмки (раздел 10 тест-кейсов) и два отчёта по источникам из приложения Б.</summary>
    public sealed class DemoReportDataService : IReportDataService
    {
        public static readonly Guid ControlSetId = new Guid("00000000-0000-0000-0000-0000000000a1");
        public static readonly Guid ExecutionId = new Guid("00000000-0000-0000-0000-0000000000a2");
        public static readonly Guid FinancingId = new Guid("00000000-0000-0000-0000-0000000000a3");

        private static readonly ReportInfo[] Reports =
        {
            new ReportInfo { Id = ControlSetId, Name = "Контрольный набор (столбцы 500 / 300 / 150 / 50)",
                ColumnTitle = "Госпрограмма", SegmentTitle = "ГРБС", Unit = "млн руб." },
            new ReportInfo { Id = ExecutionId, Name = "Исполнение расходных обязательств в разрезе ГП",
                ColumnTitle = "Госпрограмма", SegmentTitle = "ГРБС", Unit = "млн руб." },
            new ReportInfo { Id = FinancingId, Name = "Структура финансирования госпрограмм",
                ColumnTitle = "Госпрограмма", SegmentTitle = "Источник финансирования", Unit = "млн руб." }
        };

        private static readonly Dictionary<Guid, MarimekkoRow[]> Data = new Dictionary<Guid, MarimekkoRow[]>
        {
            { ControlSetId, ControlSet() },
            { ExecutionId, Execution() },
            { FinancingId, Financing() }
        };

        public IList<ReportInfo> GetReports(ICurrentUser user)
        {
            return Reports.ToList();
        }

        public IList<MarimekkoRow> GetMarimekko(Guid reportId, string filter, ICurrentUser user)
        {
            MarimekkoRow[] rows;
            if (!Data.TryGetValue(reportId, out rows)) { return null; }

            ReportFilter parsed = ReportFilter.Parse(filter);
            return rows.Where(r => parsed.Matches(r.Column, r.Segment)).ToList();
        }

        private static MarimekkoRow Row(string column, string segment, decimal value)
        {
            return new MarimekkoRow { Column = column, Segment = segment, Value = value };
        }

        // Итоги столбцов 500 / 300 / 150 / 50, три сегмента; доли внутри столбца: 50/30/20, 30/40/30, 50/30/20, 20/50/30.
        private static MarimekkoRow[] ControlSet()
        {
            return new[]
            {
                Row("ГП 11", "ГРБС 001", 250m), Row("ГП 11", "ГРБС 002", 150m), Row("ГП 11", "ГРБС 003", 100m),
                Row("ГП 12", "ГРБС 001", 90m),  Row("ГП 12", "ГРБС 002", 120m), Row("ГП 12", "ГРБС 003", 90m),
                Row("ГП 13", "ГРБС 001", 75m),  Row("ГП 13", "ГРБС 002", 45m),  Row("ГП 13", "ГРБС 003", 30m),
                Row("ГП 14", "ГРБС 001", 10m),  Row("ГП 14", "ГРБС 002", 25m),  Row("ГП 14", "ГРБС 003", 15m)
            };
        }

        private static MarimekkoRow[] Execution()
        {
            return new[]
            {
                Row("ГП 11 Развитие образования", "Минобразования", 4820.5m),
                Row("ГП 11 Развитие образования", "Минфин", 310.0m),
                Row("ГП 11 Развитие образования", "Минкультуры", 95.2m),
                Row("ГП 12 Развитие здравоохранения", "Минздрав", 3975.3m),
                Row("ГП 12 Развитие здравоохранения", "Минфин", 420.8m),
                Row("ГП 13 Социальная поддержка граждан", "Минсоцзащиты", 2760.0m),
                Row("ГП 13 Социальная поддержка граждан", "Минфин", 180.4m),
                Row("ГП 14 Развитие транспортной системы", "Минтранс", 2210.7m),
                Row("ГП 14 Развитие транспортной системы", "Минфин", 90.1m),
                Row("ГП 15 Охрана окружающей среды", "Минприроды", 640.9m),
                Row("ГП 15 Охрана окружающей среды", "Минфин", 35.0m),
                Row("ГП 16 Развитие культуры", "Минкультуры", 480.6m),
                Row("ГП 16 Развитие культуры", "Минобразования", 55.3m)
            };
        }

        private static MarimekkoRow[] Financing()
        {
            return new[]
            {
                Row("ГП 11 Развитие образования", "Федеральный бюджет", 1200.0m),
                Row("ГП 11 Развитие образования", "Региональный бюджет", 3900.0m),
                Row("ГП 11 Развитие образования", "Внебюджетные источники", 125.7m),
                Row("ГП 12 Развитие здравоохранения", "Федеральный бюджет", 1650.0m),
                Row("ГП 12 Развитие здравоохранения", "Региональный бюджет", 2400.0m),
                Row("ГП 12 Развитие здравоохранения", "Внебюджетные источники", 346.1m),
                Row("ГП 13 Социальная поддержка граждан", "Федеральный бюджет", 1400.0m),
                Row("ГП 13 Социальная поддержка граждан", "Региональный бюджет", 1540.4m),
                Row("ГП 14 Развитие транспортной системы", "Федеральный бюджет", 900.0m),
                Row("ГП 14 Развитие транспортной системы", "Региональный бюджет", 1100.8m),
                Row("ГП 14 Развитие транспортной системы", "Внебюджетные источники", 300.0m),
                Row("ГП 15 Охрана окружающей среды", "Региональный бюджет", 520.9m),
                Row("ГП 15 Охрана окружающей среды", "Внебюджетные источники", 155.0m)
            };
        }
    }
}
