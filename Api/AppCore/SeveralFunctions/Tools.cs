using Newtonsoft.Json;
using System.Diagnostics;

namespace Api.AppCore.SeveralFunctions
{
    /* --> † 29/07/2024 - Luiz Lenire. <-- */

    public sealed class Tools
    {
        #region --> Public static methods. <--  

        public static DateTime GetDateTimeNow() => DateTime.UtcNow.AddHours(-3);

        public static DateTime LastDateTimeExecution { get; set; }

        public static string GlobalFinally(dynamic serviceResponse, Stopwatch stopwatch)
        {
            try
            {
                stopwatch.Stop();
                return " | Em " + GetDateTimeNow().ToString("dd/MM/yyyy hh:mm:ss") + " trafegou " + Tools.GetSize(serviceResponse.obj) + " em " + Tools.GetTime(stopwatch.Elapsed) + ".";
            }
            catch { return " | Em dd/MM/yyyy hh:mm:ss trafegou 0B em 00h:00m:00s:001ms."; }
            finally { GC.Collect(); }
        }

        #endregion --> Public static methods. <--

        #region --> Private static methods. <--

        private static string GetSize(dynamic obj)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            double len = default;
            int order = 0;

            try
            {
                len = JsonConvert.SerializeObject(obj).Length;

                while (len >= 1024 &&
                       order < sizes.Length - 1)
                {
                    order++;
                    len /= 1024;
                }
            }
            catch { }

            return string.Format("{0:0.##} {1}", len, sizes[order]);
        }

        private static string GetTime(TimeSpan timeSpan) => string.Format("{0:D2}h:{1:D2}m:{2:D2}s:{3:D3}ms", timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds, timeSpan.Milliseconds);

        #endregion --> Private static methods. <--
    }
}
