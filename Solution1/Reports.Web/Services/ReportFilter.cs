using System;
using System.Collections.Generic;
using System.Linq;

namespace Reports.Web.Services
{
    /// <summary>Ошибка в фильтре отчёта (HTTP 422).</summary>
    public sealed class ReportFilterException : Exception
    {
        public ReportFilterException(string message) : base(message) { }
    }

    /// <summary>
    /// Фильтр отчёта вида <c>segment=ГРБС 001|ГРБС 002;column=ГП 11</c>.
    /// Принимаются только поля из белого списка формы; строка запроса целиком в сервис не передаётся.
    /// </summary>
    public sealed class ReportFilter
    {
        public static readonly string[] AllowedFields = { "column", "segment" };

        private readonly Dictionary<string, HashSet<string>> _values;

        private ReportFilter(Dictionary<string, HashSet<string>> values) { _values = values; }

        public static ReportFilter Parse(string filter)
        {
            var values = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);
            if (!string.IsNullOrWhiteSpace(filter))
            {
                foreach (string part in filter.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    int eq = part.IndexOf('=');
                    string field = (eq < 0 ? part : part.Substring(0, eq)).Trim();
                    if (eq < 0 || !AllowedFields.Contains(field, StringComparer.OrdinalIgnoreCase))
                    {
                        throw new ReportFilterException("Недопустимое поле фильтра: «" + field + "».");
                    }
                    var set = new HashSet<string>(
                        part.Substring(eq + 1).Split('|').Select(v => v.Trim()).Where(v => v.Length > 0),
                        StringComparer.OrdinalIgnoreCase);
                    HashSet<string> existing;
                    if (values.TryGetValue(field, out existing)) { existing.UnionWith(set); } else { values[field] = set; }
                }
            }
            return new ReportFilter(values);
        }

        public bool Matches(string column, string segment)
        {
            return Allows("column", column) && Allows("segment", segment);
        }

        private bool Allows(string field, string value)
        {
            HashSet<string> set;
            return !_values.TryGetValue(field, out set) || set.Contains(value);
        }
    }
}
