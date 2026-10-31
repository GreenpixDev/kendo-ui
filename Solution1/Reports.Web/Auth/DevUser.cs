using System;

namespace Reports.Web.Auth
{
    /// <summary>Тестовый пользователь заглушки авторизации.</summary>
    public sealed class DevUser : ICurrentUser
    {
        public static readonly DevUser Anonymous = new DevUser(null, null, false, false);
        public static readonly DevUser Specialist = new DevUser(
            new Guid("11111111-1111-1111-1111-111111111111"), "Тестовый специалист", true, true);
        public static readonly DevUser User = new DevUser(
            new Guid("22222222-2222-2222-2222-222222222222"), "Тестовый пользователь", false, true);
        public static readonly DevUser NoRole = new DevUser(
            new Guid("33333333-3333-3333-3333-333333333333"), "Пользователь без роли", false, false);

        private DevUser(Guid? id, string name, bool specialist, bool user)
        {
            UserId = id;
            DisplayName = name;
            IsSpecialist = specialist;
            IsUser = user;
        }

        public bool IsAuthenticated { get { return UserId.HasValue; } }
        public Guid? UserId { get; private set; }
        public string DisplayName { get; private set; }
        public bool IsSpecialist { get; private set; }
        public bool IsUser { get; private set; }
    }
}
