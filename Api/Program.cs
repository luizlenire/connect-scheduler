namespace Api
{
    /* --> † 29/07/2024 - Luiz Lenire. <--*/

    public sealed class Program
    {
        #region --> Public static methods. <--

        public static void Main(string[] args) => CreateHostBuilder(args).Build().Run();

        public static IHostBuilder CreateHostBuilder(string[] args) => Host.CreateDefaultBuilder(args).ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); });

        #endregion --> Public static methods. <--    
    }
}
