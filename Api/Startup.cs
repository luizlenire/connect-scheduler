using Api.AppCore.Controllers;
using Api.AppCore.Models;
using Api.AppCore.SeveralFunctions;
using Hangfire;
using Hangfire.MemoryStorage;
using HangfireBasicAuthenticationFilter;
using System.Diagnostics;
using System.Linq.Expressions;

namespace Api
{
    /* --> † 11/08/2020 - Luiz Lenire. <-- */

    public sealed class Startup
    {
        #region --> Private atributtes. <--       

        private const string ApplicationName = "Connect Scheduler";

        private const string Version = "1.0";

        private static string _DashboardTitle
        {
            get
            {
                string returnDashboardTitle = default;

                if (Debugger.IsAttached) returnDashboardTitle = "1 - Debugger | ";
                else if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development") returnDashboardTitle = "2 - Development | ";
                else if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Production") returnDashboardTitle = "3 - Production | ";

                return returnDashboardTitle + ApplicationName + " " + Version + " | Rise Date: " + Tools.GetDateTimeNow().ToString("dd/MM/yyyy HH:mm:ss");
            }
        }

        #endregion --> Private atributtes. <--

        #region --> Constructors. <--

        public Startup()
        {
            IConfiguration iconfiguration = new ConfigurationBuilder().AddJsonFile($"appsettings" + (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")) ? "." + Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") : default) + ".json", true, true).Build();

            GlobalAtributtes.listProfile = [];

            foreach (GlobalEnum.ClientType itemClientType in Enum.GetValues(typeof(GlobalEnum.ClientType)))
            {
                if (itemClientType == GlobalEnum.ClientType.Development)
                {
                    GlobalAtributtes.connectGateway = iconfiguration["ConnectGatewayDevelopment"];
                    GlobalAtributtes.connectScheduler = iconfiguration["ConnectScheduler"];
                }
                else if (GlobalAtributtes.connectGateway == default)
                {
                    GlobalAtributtes.connectGateway = iconfiguration["ConnectGateway"];
                    GlobalAtributtes.connectScheduler = iconfiguration["ConnectSchedulerDevelopment"];
                }

                foreach (GlobalEnum.ApiType itemApiType in Enum.GetValues(typeof(GlobalEnum.ApiType)))
                {
                    string url;

                    if (itemApiType != GlobalEnum.ApiType.Stage) url = iconfiguration["Connect" + itemApiType.ToString() + itemClientType.ToString()];
                    else url = iconfiguration[itemApiType.ToString() + itemClientType.ToString()];

                    GlobalAtributtes.listProfile.Add(new()
                    {
                        apiType = itemApiType,
                        clientType = itemClientType,
                        Url = url,
                        Active = url != default
                    });
                }
            }
        }

        #endregion --> Constructors. <--

        #region --> Public methods. <--

        public void ConfigureServices(IServiceCollection iServiceCollection)
        {
            iServiceCollection.AddControllers();

            Hangfire();

            #region --> Sub-methods. <--

            void Hangfire()
            {
                iServiceCollection.AddHangfire(x => x.UseSimpleAssemblyNameTypeSerializer().UseDefaultTypeSerializer().UseMemoryStorage());

                GlobalJobFilters.Filters.Add(new AutomaticRetryAttribute
                {
                    Attempts = 0,
                    DelaysInSeconds = [0],
                    OnAttemptsExceeded = AttemptsExceededAction.Delete
                });

                iServiceCollection.AddHangfireServer();
            }

            #endregion --> Sub-methods. <--
        }

        public void Configure(IApplicationBuilder iApplicationBuilder, IWebHostEnvironment iWebHostEnvironment, IRecurringJobManager irecurringJobManager)
        {
            if (iWebHostEnvironment.IsDevelopment()) iApplicationBuilder.UseDeveloperExceptionPage();

            iApplicationBuilder.UseRouting();
            iApplicationBuilder.UseEndpoints(x => x.MapControllers());

            Hangfire();

            #region --> Sub-methods. <--

            void Hangfire()
            {
                Login login = new();

                iApplicationBuilder.UseHangfireDashboard("/hangfire", new()
                {
                    PrefixPath = Debugger.IsAttached ? default : "/connect-scheduler",
                    Authorization = new[]
                    {
                        new HangfireCustomBasicAuthenticationFilter
                        {
                            User = login.Username,
                            Pass = login.Password
                        }
                    },
                    DashboardTitle = _DashboardTitle,
                    DarkModeEnabled = true
                });

                /* --> † 30/04/2024 - Luiz Lenire
                 * Avisando sobre novas versões.
                 */
                BackgroundJob.Enqueue(() => HangFireJob.AdmNewVersion(_DashboardTitle));

                if (Debugger.IsAttached)
                {
                    /* --> † 13/01/2022 - Luiz Lenire. <--
                     * Abaixo, exemplos de como funciona os quatros tipos de tarefas que podem ir deste imediata, intervalada, recorrente e continuada. 
                     * Seguir o padrão do projeto em cada perfil desta aplicação, os exemplos abaixo são apenas educativo, apenas para entender como a 
                     * aplicação foi concebiba.
                     */

                    // --> Job único
                    string jobId = BackgroundJob.Enqueue(() => Console.WriteLine("Job Fire-and-forget!"));

                    // --> Job intervalado
                    BackgroundJob.Schedule(() => Console.WriteLine("Job Delayed: 2 minutos após o início da aplicação"), TimeSpan.FromMinutes(2));

                    // --> Job recorrente.
                    RecurringJob.AddOrUpdate("Meu job recorrente", () => Console.WriteLine((new Random().Next(1, 200) % 2 == 0) ? "Job recorrente gerou um número par" : "Job recorrente gerou um número ímpar"), Cron.Minutely, TimeZoneInfo.Local);

                    // --> Job continuação de um inicial.
                    jobId = BackgroundJob.Enqueue(() => Console.WriteLine("Job fire-and-forget pai!")); BackgroundJob.ContinueJobWith(jobId, () => Console.WriteLine($"Job continuation! (Job pai: {jobId})"));

                    return;
                }

                //Monitoramento do ConnectScheduler | Hangfire.
                irecurringJobManager.AddOrUpdate("AdmMonitoringScheduler", () => HangFireJob.AdmMonitoringScheduler(_DashboardTitle), Cron.MinuteInterval(30), TimeZoneInfo.Utc);

                foreach (GlobalEnum.ClientType clientType in GlobalAtributtes.listProfile
                                                                             .Where(x => x.Active)
                                                                             .Select(x => x.clientType)
                                                                             .Distinct()
                                                                             .ToList())
                {
                    string recurringJobId = default;
                    Expression<Action> expressionAction = default;
                    string cronExpression = default;
                    TimeZoneInfo timeZoreInfo = TimeZoneInfo.Utc;
                    string queue = clientType.ToString().ToLower();
                    string serverName = clientType.ToString().ToLower();

                    ConfigurationHangfireQueuesWorkers(iApplicationBuilder, [queue], 2, serverName);

                    if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
                    {
                        #region --> Adm. <--

                        if (clientType == GlobalEnum.ClientType.Development)
                        {
                            recurringJobId = clientType.ToString() + " | Adm";
                            expressionAction = () => HangFireJob.Adm(GlobalAtributtes.listProfile
                                                                                     .Where(x => x.Active &&
                                                                                                 x.clientType == clientType)
                                                                                     .ToList(), default);
                            cronExpression = Cron.MinuteInterval(30);

                            irecurringJobManager.AddOrUpdate(recurringJobId, expressionAction, cronExpression, timeZoreInfo, queue);
                        }

                        #endregion --> Adm. <--                         
                    }
                    else if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Production")
                    {
                        #region --> Adm. <--

                        recurringJobId = clientType.ToString() + " | Adm";
                        expressionAction = () => HangFireJob.Adm(GlobalAtributtes.listProfile
                                                                                 .Where(x => x.Active &&
                                                                                             x.clientType == clientType)
                                                                                 .ToList(), default);
                        cronExpression = Cron.Hourly(30);

                        irecurringJobManager.AddOrUpdate(recurringJobId, expressionAction, cronExpression, timeZoreInfo, queue);

                        #endregion --> Adm. <--                                             
                    }
                }
            }

            #endregion --> Sub-methods. <--
        }

        private void ConfigurationHangfireQueuesWorkers(IApplicationBuilder iApplicationBuilder, string[] queues, int workerCount, string serverName)
        {
            BackgroundJobServerOptions hangfireQueueOptions = new BackgroundJobServerOptions
            {
                Queues = queues,
                WorkerCount = workerCount,
                ServerName = serverName,
            };

            iApplicationBuilder.UseHangfireServer(hangfireQueueOptions);
        }

        #endregion --> Public methods. <--
    }
}
