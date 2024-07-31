using Api.AppCore.SeveralFunctions;

namespace AppCore.Models
{
    /* --> † 29/07/2024 - Luiz Lenire. <-- */

    public sealed class Profile
    {
        #region --> Public properties. <--

        public GlobalEnum.ApiType apiType { get; set; }

        public GlobalEnum.ClientType clientType { get; set; }

        public string Url { get; set; }

        public bool Active { get; set; }

        #endregion --> Public properties. <--
    }
}
