using AppCore.Models;

namespace Api.AppCore.SeveralFunctions
{
    /* --> † 29/07/2024 - Luiz Lenire. <-- */

    public sealed class GlobalAtributtes
    {
        #region --> Public static properties. <--         

        public const bool LogActive = default;

        public static string connectScheduler { get; set; }

        public static string connectGateway { get; set; }

        public static List<Profile> listProfile { get; set; }

        #endregion --> Private static properties. <-- 
    }
}
