using System;
using System.Configuration;
using System.Web;

namespace Reports.Web.Auth
{
    /// <summary>
    /// Определяет текущего пользователя. Единственное место, которое знает, откуда он берётся:
    /// в тестовом контуре (appSettings "DevAuth:Enabled" = true) - из cookie заглушки /Dev/Login,
    /// иначе - адаптер к сессии АИС ГП (пока не реализован, пользователь не определён).
    /// </summary>
    public static class CurrentUserResolver
    {
        public const string ItemKey = "currentUser";
        public const string DevCookie = "dev_user";

        public static bool DevAuthEnabled
        {
            get
            {
                bool enabled;
                return bool.TryParse(ConfigurationManager.AppSettings["DevAuth:Enabled"], out enabled) && enabled;
            }
        }

        public static string LoginUrl
        {
            get { return ConfigurationManager.AppSettings["AisLoginUrl"] ?? "~/Dev/Login"; }
        }

        public static ICurrentUser Resolve(HttpContextBase context)
        {
            if (!DevAuthEnabled) { return DevUser.Anonymous; }   // TODO: адаптер к сессии АИС ГП

            HttpCookie cookie = context.Request.Cookies[DevCookie];
            string key = cookie == null ? null : cookie.Value;
            switch (key)
            {
                case "specialist": return DevUser.Specialist;
                case "user": return DevUser.User;
                case "norole": return DevUser.NoRole;
                default: return DevUser.Anonymous;
            }
        }

        public static ICurrentUser Current(HttpContextBase context)
        {
            return (ICurrentUser)context.Items[ItemKey] ?? Resolve(context);
        }
    }
}
