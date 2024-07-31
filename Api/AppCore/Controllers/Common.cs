using Api.AppCore.Models;
using Api.AppCore.SeveralFunctions;
using AppCore.Models;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http.Headers;
using System.Text;

namespace Api.AppCore.Controllers
{
    /* --> † 29/07/2024 - Luiz Lenire. <-- */

    public abstract class Common
    {
        #region --> Private static properties. <--

        private static string Address { get; set; }

        protected static string Token { get; set; }

        protected enum Signature
        {
            GET = 1,
            POST = 2,
            PUT = 3,
            DELETE = 4
        }

        #endregion --> Private static properties. <--

        #region --> Protected static methods. <--

        protected static string TestAvailability(string url)
        {
            Tools.LastDateTimeExecution = Tools.GetDateTimeNow();

            using HttpClient httpClient = new();
            HttpResponseMessage httpResponseMessage = httpClient.GetAsync(url).Result;

            if (httpResponseMessage.StatusCode == HttpStatusCode.OK) return default;
            else return httpResponseMessage.StatusCode.ToString();
        }

        protected static ServiceResponse GetTokenAsync(string url) => GetTokenAsync(new Profile { Url = url, Active = true });

        protected static ServiceResponse GetTokenAsync(Profile profile)
        {
            ServiceResponse serviceResponse = new();

            if (profile == default || !profile.Active)
            {
                serviceResponse.message = "Não foi possivel gerar o token de acesso.";
                return serviceResponse;
            }

            Tools.LastDateTimeExecution = Tools.GetDateTimeNow();
            Login login = new();

            if (GlobalAtributtes.LogActive)
            {
                using HttpClient httpClient = new();
                HttpResponseMessage httpResponseMessage = httpClient.PostAsync(profile.Url + "/authentication/token/generate",
                                                                               new StringContent(JsonConvert.SerializeObject(new
                                                                               {
                                                                                   login.Username,
                                                                                   login.Password
                                                                               }),
                                                                               Encoding.UTF8,
                                                                               "application/json")).Result;



                if (httpResponseMessage.StatusCode == HttpStatusCode.OK)
                {
                    serviceResponse = JsonConvert.DeserializeObject<ServiceResponse>(httpResponseMessage.Content.ReadAsStringAsync().Result);

                    if (serviceResponse != default &&
                        serviceResponse.success)
                    {
                        Address = profile.Url;
                        Token = Token = serviceResponse.obj;
                    }
                }
                else serviceResponse.message = httpResponseMessage.Content.ReadAsStringAsync().Result;
            }
            else
            {
                try
                {
                    using HttpClient httpClient = new();
                    HttpResponseMessage httpResponseMessage = httpClient.PostAsync(profile.Url + "/authentication/token/generate",
                                                                                   new StringContent(JsonConvert.SerializeObject(new
                                                                                   {
                                                                                       login.Username,
                                                                                       login.Password
                                                                                   }),
                                                                                   Encoding.UTF8,
                                                                                   "application/json")).Result;



                    if (httpResponseMessage.StatusCode == HttpStatusCode.OK)
                    {
                        serviceResponse = JsonConvert.DeserializeObject<ServiceResponse>(httpResponseMessage.Content.ReadAsStringAsync().Result);

                        if (serviceResponse != default &&
                            serviceResponse.success)
                        {
                            Address = profile.Url;
                            Token = Token = serviceResponse.obj;
                        }
                    }
                }
                catch { }
            }

            return serviceResponse;
        }

        protected static ServiceResponse GetAsync(string url)
        {
            ServiceResponse serviceResponse = new();

            using HttpClient httpClient = new();
            HttpResponseMessage httpResponseMessage = httpClient.GetAsync(url).Result;

            if (httpResponseMessage.StatusCode == HttpStatusCode.OK) serviceResponse = JsonConvert.DeserializeObject<ServiceResponse>(httpResponseMessage.Content.ReadAsStringAsync().Result);
            else serviceResponse.message = httpResponseMessage.Content.ReadAsStringAsync().Result;

            return serviceResponse;
        }

        protected static Task Process(string endPoint, dynamic obj, Signature signature)
        {
            Tools.LastDateTimeExecution = Tools.GetDateTimeNow();

            using HttpClient httpClient = new();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Token);

            HttpResponseMessage httpResponseMessage = default;

            if (GlobalAtributtes.LogActive) httpResponseMessage = SendRequest();
            else try { httpResponseMessage = SendRequest(); } catch { }

            if (httpResponseMessage.StatusCode == HttpStatusCode.OK)
            {
                ServiceResponse serviceResponse = JsonConvert.DeserializeObject<ServiceResponse>(httpResponseMessage.Content.ReadAsStringAsync().Result);
                if (serviceResponse != default) Console.WriteLine(serviceResponse.message);
            }
            else Console.WriteLine(httpResponseMessage.Content.ReadAsStringAsync().Result);

            return Task.CompletedTask;

            #region --> Internal methods. <--

            HttpResponseMessage SendRequest()
            {
                if (signature == Signature.GET) return httpClient.GetAsync(Address + endPoint).Result;
                else if (signature == Signature.POST) return httpClient.PostAsync(Address + endPoint, obj).Result;
                else if (signature == Signature.PUT) return httpClient.PutAsync(Address + endPoint, obj).Result;
                else if (signature == Signature.DELETE) return httpClient.DeleteAsync(Address + endPoint, obj).Result;
                else return default;
            }

            #endregion --> Internal methods. <--
        }

        #endregion --> Protected static methods. <--
    }
}
