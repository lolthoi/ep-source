using Kloon.EmployeePerformance.DataAccess.Domain;
using Kloon.EmployeePerformance.Logic.Services;
using Kloon.EmployeePerformance.Models.Common;
using Kloon.EmployeePerformance.Models.QuarterEvaluation;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Kloon.EmployeePerformance.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserQuarterEvaluationController : ControllerBase
    {
        private readonly IUserQuarterEvaluationService _userQuarterEvaluationService;
        public UserQuarterEvaluationController(IUserQuarterEvaluationService userQuarterEvaluationService)
        {
            _userQuarterEvaluationService = userQuarterEvaluationService;
        }

        [HttpGet("{id:Guid}")]
        public ActionResult<UserQuarterEvaluationModel> GetById(Guid id)
        {
            return _userQuarterEvaluationService.GetByQuarterEvaluationId(id).ToResponse();
        }
        //[HttpPost]
        //public ActionResult<List<UserQuarterEvaluationModel>> Create(List<UserQuarterEvaluationModel> model)
        //{
        //    return _userQuarterEvaluationService.Create(model).ToResponse();
        //}

        [HttpPost]
        public ActionResult<UserQuarterEvaluationModel> Create(UserQuarterEvaluationModel model)
        {
            return _userQuarterEvaluationService.Create(model).ToResponse();
        }

        [HttpPut]
        public ActionResult<UserQuarterEvaluationModel> Update(UserQuarterEvaluationModel userQuarterEvaluationModel)
        {
            return _userQuarterEvaluationService.Update(userQuarterEvaluationModel).ToResponse();
        }

        [HttpGet("user/{userId:int}")]
        public ActionResult<DataSourcePersonalEvaluateModel> GetUserQuarterEvaluationsAvaiable(int userId)
        {
            return _userQuarterEvaluationService.GetAvaiableQuarterEvaluations(userId).ToResponse();

        }
    }
}
