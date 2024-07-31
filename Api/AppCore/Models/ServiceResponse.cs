namespace Api.AppCore.Models
{
    /* --> † 29/07/2024 - Luiz Lenire. <-- */

    public sealed class ServiceResponse
    {
        #region --> Public properties. <--

        public dynamic obj { get; set; }

        public bool success { get; set; }

        public string message { get; set; }

        #endregion --> Public properties. <--
    }
}
