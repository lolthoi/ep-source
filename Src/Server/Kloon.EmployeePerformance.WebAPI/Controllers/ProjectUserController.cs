using Kloon.EmployeePerformance.Logic.Services;
using Kloon.EmployeePerformance.Models.Project;
using Kloon.EmployeePerformance.Models.User;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Kloon.EmployeePerformance.WebAPI.Controllers
{
    [Route("api/Project/{projectId:int}/[controller]")]
    [ApiController]
    public class ProjectUserController : ControllerBase
    {
        private readonly IProjectUserService _projectUserService;
        public ProjectUserController(IProjectUserService projectUserService)
        {
            _projectUserService = projectUserService;
        }

        [HttpGet]
        public ActionResult<List<ProjectUserModel>> GetAll(int projectId, [FromQuery] string searchText = "")
        {
            var result = _projectUserService.GetAll(projectId, searchText);
            return result.ToResponse();
        }

        [HttpGet("{projectUserId:Guid}")]
        public ActionResult<ProjectUserModel> GetById(int projectId, Guid projectUserId)
        {
            var result = _projectUserService.GetById(projectId, projectUserId);
            return result.ToResponse();
        }

        [HttpGet("/api/Project/ProjectUser/GetTopFiveUserNotInProject")]
        public ActionResult<List<UserModel>> GetTopFiveUserNotInProject(int projectId, string searchText = "")
        {
            var result = _projectUserService.GetTopFiveUserNotInProject(projectId, searchText);
            return result.ToResponse();
        }

        #region POST

        [HttpPost("{userId:int}")]
        public ActionResult<ProjectUserModel> Create(int projectId, int userId)
        {
            var result = _projectUserService.Create(projectId, userId);
            return result.ToResponse();
        }

        #endregion

        #region PUT

        [HttpPut]
        public ActionResult<ProjectUserModel> Update(int projectId, Guid projectUserId, int projectRoleId)
        {
            var result = _projectUserService.Update(projectId, projectUserId, projectRoleId);
            return result.ToResponse();
        }

        #endregion

        #region DELETE

        [HttpDelete("{projectUserId}")]
        public ActionResult<bool> Delete(int projectId, Guid projectUserId)
        {
            var result = _projectUserService.Delete(projectId ,projectUserId);
            return result.ToResponse();
        }

        #endregion
    }
}
