namespace Api.AppCore.SeveralFunctions
{
    /* --> † 29/07/2024 - Luiz Lenire. <-- */

    public sealed class GlobalEnum
    {
        #region --> Public properties. <--

        public enum ApiType
        {
            Stage = 1,
            External = 2,
            Mobile = 3,
            Work = 4
        }

        public enum ClientType
        {
            Development = 0
        }

        public enum AdmType
        {
            NewVersion = 1,
            CheckingConnections = 2
        }

        public enum ReferentialDataType
        {
            LegalPerson = 1,
            Bank = 2
        }

        #endregion --> Public properties. <--
    }
}
