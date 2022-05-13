using Kloon.EmployeePerformance.Logic.Caches;
using Kloon.EmployeePerformance.Logic.Services;
using Kloon.EmployeePerformance.Models.Position;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Kloon.EmployeePerformance.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PositionController : ControllerBase
    {
        private readonly IPositionService _positionService;

        public PositionController(IPositionService positionService)
        {
            _positionService = positionService;
        }

        [HttpGet]
        public ActionResult<List<PositionModel>> GetAll()
        {
            return _positionService.GetAll().ToResponse();
        }
        [HttpGet("PositionsForTemplate")]
        public ActionResult<List<PositionModel>> Get()
        {
            return _positionService.GetPositionsForTemplate().ToResponse();
        }
    }
}
