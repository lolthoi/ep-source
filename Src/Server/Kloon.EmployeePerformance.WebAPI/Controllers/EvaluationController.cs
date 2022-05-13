using Kloon.EmployeePerformance.Logic.Services;
using Kloon.EmployeePerformance.Models.Criteria;
using Kloon.EmployeePerformance.Models.QuarterEvaluation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Kloon.EmployeePerformance.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EvaluationController : ControllerBase
    {
        private readonly IUserQuarterEvaluationService _userQuarterEvaluationService;

        public EvaluationController(IUserQuarterEvaluationService userQuarterEvaluationService)
        {
            _userQuarterEvaluationService = userQuarterEvaluationService;
        }

        [HttpGet("GetAllByPerson")]
        public ActionResult<List<QuarterEvaluationModel>> GetAllByPerson(int startYear = 0, int endYear = 0, int projectId = 0)
        {
            var result = _userQuarterEvaluationService.GetAllByPerson(startYear, endYear, projectId);
            return result.ToResponse();
        }

        [HttpGet("leader/{id}")]
        public ActionResult<List<LeaderEvaluationResultModel>> GetAll(int id)
        {
            return _userQuarterEvaluationService.LeaderEvaluation(id).ToResponse();
        }

        [HttpGet]
        public ActionResult<List<CriteriaModelQuarter>> GetQuarter(int id, Guid quarterId)
        {
            return _userQuarterEvaluationService.EvaluationInfo(id, quarterId).ToResponse();
        }

        [HttpPost]
        public ActionResult<bool> CreateQuarter(Guid quarterId, [FromBody] List<QuarterPoint> model)
        {
            return _userQuarterEvaluationService.CreateLeaderEvaluation(quarterId, model).ToResponse();
        }
    }
}
