using Api.AppCore.Models;
using Api.AppCore.SeveralFunctions;
using AppCore.Models;
using Newtonsoft.Json;
using System.Text;

namespace Api.AppCore.Controllers
{
    /* --> † 29/07/2024 - Luiz Lenire. <-- */

    public sealed class HangFireJob : Common
    {
        #region --> Public static methods. <--

        public static void Adm(List<Profile> listProfile, string appVersion)
        {
            foreach (Profile item in listProfile) CheckingConnections(item);

            #region --> Sub-methods. <--        

            void CheckingConnections(Profile profile)
            {
                string returnTestAvailability = TestAvailability(profile.Url + "/swagger/index.html");

                if (returnTestAvailability == default)
                {
                    ServiceResponse serviceResponse = GetTokenAsync(profile);

                    if (!serviceResponse.success) returnTestAvailability = "Realizado o teste de autenticação da API {" + profile.Url + "} e não foi possível obter sucesso, segue mais detalhes: {" + serviceResponse.message + "}.";
                }
                else returnTestAvailability = "Realizado o teste de disponibilidade da API {" + profile.Url + "} e não foi possível obter sucesso, segue mais detalhes: {" + returnTestAvailability + "}.";

                if (returnTestAvailability != default)
                {
                    ServiceResponse serviceResponse = GetTokenAsync(GlobalAtributtes.connectGateway);

                    if (serviceResponse.success)
                    {
                        Process("/email/send",
                                new StringContent(JsonConvert.SerializeObject(new
                                {
                                    Subject = "Monitoramento " + appVersion,
                                    Body = returnTestAvailability,
                                    Recipient = "luizlenire@outlook.com"
                                }), Encoding.UTF8, "application/json"),
                                Signature.POST);
                    }
                }
            }

            #endregion --> Sub-methods. <--
        }

        public static void AdmNewVersion(string appVersion)
        {
            ServiceResponse serviceResponse = GetTokenAsync(GlobalAtributtes.connectGateway);

            if (serviceResponse.success)
            {
                Process("/email/send",
                        new StringContent(JsonConvert.SerializeObject(new
                        {
                            Subject = "Nova versão " + appVersion,
                            Body = "Olá, detectada uma nova subida de versão do " + appVersion + ".",
                            Recipient = "luizlenire@outlook.com"
                        }), Encoding.UTF8, "application/json"),
                        Signature.POST);
            }
        }

        public static void AdmMonitoringScheduler(string appVersion)
        {
            string returnTestAvailability = default;

            ServiceResponse serviceResponse = GetAsync(GlobalAtributtes.connectScheduler + "/administrator/get-last-datetime-execution");

            if (!serviceResponse.success) returnTestAvailability = "Realizado o teste de disponibilidade do Connect Scheduler {" + GlobalAtributtes.connectScheduler + "} e não foi possível obter sucesso, segue mais detalhes: {" + serviceResponse.message + "}.";
            else
            {
                DateTime lastDateTimeExecution = serviceResponse.obj;

                int Limite = 1;
                int TotalHours = (int)(Tools.GetDateTimeNow().Subtract(lastDateTimeExecution)).TotalHours;

                if (TotalHours >= Limite)
                {
                    returnTestAvailability = "Realizado o teste de operabilidade do Connect Scheduler {" + GlobalAtributtes.connectScheduler + "} e foi detectado que o mesmo está inoperante a mais de ";

                    if (TotalHours > 1) returnTestAvailability += TotalHours + " horas, ";
                    else returnTestAvailability += TotalHours + " hora, ";

                    returnTestAvailability += "a ultima data de operação registrada é de " + lastDateTimeExecution.ToString("dd/MM/yyyy HH:mm") + ".";
                }
            }

            if (returnTestAvailability != default)
            {
                serviceResponse = GetTokenAsync(GlobalAtributtes.connectGateway);

                if (serviceResponse.success)
                {
                    Process("/email/send",
                            new StringContent(JsonConvert.SerializeObject(new
                            {
                                Subject = "(CRITICO) Monitoramento " + appVersion,
                                Body = returnTestAvailability,
                                Recipient = "luizlenire@outlook.com"
                            }), Encoding.UTF8, "application/json"),
                            Signature.POST);
                }
            }
        }

        public static void ReferentialData(Profile profile, GlobalEnum.ReferentialDataType referentialDataType)
        {
            if (GetTokenAsync(profile).success)
            {
                if (referentialDataType == GlobalEnum.ReferentialDataType.LegalPerson)
                {
                    Process(@"/referentialdata/legal-person/process-by-default-list", default, Signature.POST);
                    Process(@"/referentialdata/legal-person/process-by-opening-date", default, Signature.POST);
                }
                else if (referentialDataType == GlobalEnum.ReferentialDataType.Bank) Process(@"/referentialdata/bank/transfer-to-siss", default, Signature.GET);
            }
        }

        #endregion --> Public static methods. <--
    }
}
