using System;

namespace Reports.Web.Auth
{
    /// <summary>
    /// Пользователь АИС ГП (п. 5.1 брифа). Остальной код зависит только от этой абстракции;
    /// при подключении к реальной системе заменяется адаптер в <see cref="CurrentUserResolver"/>.
    /// </summary>
    public interface ICurrentUser
    {
        bool IsAuthenticated { get; }
        Guid? UserId { get; }
        string DisplayName { get; }
        bool IsSpecialist { get; }   // роль ReportSpecialist
        bool IsUser { get; }         // роль ReportUser или ReportSpecialist
    }
}
