namespace Reports.Web.Models
{
    /// <summary>Плоская строка для диаграммы Маримекко: результат GROUP BY column, segment с SUM(value).</summary>
    public class MarimekkoRow
    {
        public string Column { get; set; }    // измерение для столбцов
        public string Segment { get; set; }   // измерение для сегментов
        public decimal Value { get; set; }    // мера (> 0; нулевые и отрицательные пропускает клиент)
    }
}
