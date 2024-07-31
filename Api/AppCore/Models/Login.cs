namespace Api.AppCore.Models
{
    /* --> † 29/07/2024 - Luiz Lenire. <-- */

    public sealed class Login
    {
        #region --> Public properties. <--

        public string Username { get; set; }

        public string Password { get; set; }

        #endregion --> Public properties. <--

        #region --> Constructors. <--

        public Login() { Username = "usrcscheduler"; Password = "pswcscheduler"; }

        #endregion --> Constructors. <--
    }
}
