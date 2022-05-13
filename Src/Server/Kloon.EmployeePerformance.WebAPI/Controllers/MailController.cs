using Kloon.EmployeePerformance.Logic.Caches;
using Kloon.EmployeePerformance.Logic.Common;
using Kloon.EmployeePerformance.Logic.Services;
using Kloon.EmployeePerformance.Logic.Services.Base;
using Kloon.EmployeePerformance.Models.Common;
using Kloon.EmployeePerformance.Models.QuarterEvaluation;
using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Xml;

namespace Kloon.EmployeePerformance.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MailController : ControllerBase
    {
        private readonly IMailService _mailService;
        private readonly IQuarterEvaluationService _evaluationService;
        private readonly CacheProvider _cache;
        private readonly IHttpContextAccessor _httpContext;
        private static Object lockObject = new Object();

        public MailController(IMailService mailService, IQuarterEvaluationService evaluationService, CacheProvider cacheProvider, IHttpContextAccessor httpContextAccessor)
        {
            _mailService = mailService;
            _evaluationService = evaluationService;
            _cache = cacheProvider;
            _httpContext = httpContextAccessor;
        }

        [HttpGet("year/{year:int}/quarter/{quarter:int}")]
        public async Task<ActionResult<bool>> SendInformEvaluate(int year, int quarter)
        {
            lock (lockObject)
            {
                try
                {
                    var result = _evaluationService.GenerateEvaluation(year, quarter);
                    if (result.Error != null)
                    {
                        throw new Exception(result.Error.Message);
                    }
                    //_mailService.SendMailInformEvaluation(year, quarter);
                    return true;
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message, ex.InnerException);
                }
            }
        }

        [HttpDelete]
        public ActionResult<bool> Delete()
        {
            return _evaluationService.DeleteAllEvaluation().ToResponse();
        }
    }
}
