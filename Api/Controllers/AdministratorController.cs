using Api.AppCore.Models;
using Api.AppCore.SeveralFunctions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Api.Controllers
{
    /* --> † 29/07/2024 - Luiz Lenire. <-- */

    [ApiController]
    [Route("[controller]")]
    [AllowAnonymous]
    public sealed class AdministratorController
    {
        #region --> Private properties. <--      

        private Stopwatch stopwatch { get; set; }

        #endregion --> Private properties. <--

        #region --> Constructors. <--

        public AdministratorController()
        {
            stopwatch = new();
            stopwatch.Start();
        }

        #endregion --> Constructors. <--    

        #region --> Public methods. <--     

        [HttpGet]
        [Route("get-last-datetime-execution")]
        public ServiceResponse GetLastDateTimeExecution()
        {
            ServiceResponse serviceResponse = new();

            try
            {
                serviceResponse.obj = Tools.LastDateTimeExecution;
                serviceResponse.success = true;
                serviceResponse.message = "Data/hora da ultima execução obtida com sucesso.";
            }
            catch (Exception ex)
            {
                serviceResponse.obj = ex.Message;
                serviceResponse.message = "Não foi possível obter a data/hora da ultima execução.";
            }
            finally { serviceResponse.message += Tools.GlobalFinally(serviceResponse, stopwatch); }

            return serviceResponse;
        }

        #endregion --> Public methods. <--
    }
}
