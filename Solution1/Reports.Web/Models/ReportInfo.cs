using System;

namespace Reports.Web.Models
{
    /// <summary>Описание формы отчёта с диаграммой Маримекко: какие поля сопоставлены столбцам, сегментам и мере.</summary>
    public class ReportInfo
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string ColumnTitle { get; set; }    // что означает столбец (например, «Госпрограмма»)
        public string SegmentTitle { get; set; }   // что означает сегмент (например, «ГРБС»)
        public string Unit { get; set; }           // единица меры (например, «млн руб.»)
    }
}
