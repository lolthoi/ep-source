using Kloon.EmployeePerformance.Logic.Services;
using Kloon.EmployeePerformance.Models.Project;
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
    public class ProjectController : ControllerBase
    {
        private readonly IProjectService _projectService;
        public ProjectController(IProjectService projectService)
        {
            _projectService = projectService;
        }

        [HttpGet]
        public ActionResult<List<ProjectModel>> GetAll([FromQuery] string searchText)
        {
            var result = _projectService.GetAll(searchText);
            return result.ToResponse();
        }

        [HttpGet("{projectId:int}")]
        public ActionResult<ProjectModel> GetById(int projectId)
        {
            var result = _projectService.GetById(projectId);
            return result.ToResponse();
        }

        [HttpGet("GetAllProjectOfUser")]
        public ActionResult<List<ProjectModel>> GetAllProjectOfUser()
        {
            return _projectService.GetAllProjectOfUser().ToResponse();
        }
        [HttpGet("Leader")]
        public ActionResult<List<ProjectModel>> GetProjectOfLeader()
        {
            return _projectService.GetProjectOfLeader().ToResponse();
        }

        [HttpGet("GetTeamForReport")]
        public ActionResult<List<ProjectModel>> GetTeamForReport()
        {
            return _projectService.GetTeamForReport().ToResponse();
        }

        #region POST

        /// <summary>
        /// Create a new project
        /// </summary>
        /// <remarks>
        /// Return the newly created project
        /// </remarks>
        /// <param name="model">Project object that needs to be created</param>
        /// <returns>Return the newly created project</returns>
        /// <response code="200">Successful operation</response>
        /// <response code="204">Successful operation, no content is returned</response>
        /// <response code="400">Invalid Id supplied</response>
        /// <response code="401">User is unauthorized</response>
        /// <response code="403">User is not authenticated</response>
        /// <response code="404">Request is inaccessible</response>
        /// <response code="409">Data is conflicted</response>
        /// <response code="500">Server side error</response>
        [HttpPost]
        public ActionResult<ProjectModel> Create(ProjectModel model)
        {
            var result = _projectService.Create(model);
            return result.ToResponse();
        }

        #endregion

        #region PUT

        /// <summary>
        /// Update an existing project
        /// </summary>
        /// <remarks>
        /// Return the updated project
        /// </remarks>
        /// <param name="model">Project object that needs to be created</param>
        /// <returns>Return the updated project</returns>
        /// <response code="200">Successful operation</response>
        /// <response code="204">Successful operation, no content is returned</response>
        /// <response code="400">Invalid Id supplied</response>
        /// <response code="401">User is unauthorized</response>
        /// <response code="403">User is not authenticated</response>
        /// <response code="404">Request is inaccessible</response>
        /// <response code="409">Data is conflicted</response>
        /// <response code="500">Server side error</response>
        [HttpPut]
        public ActionResult<ProjectModel> Update(ProjectModel model)
        {
            var result = _projectService.Update(model);
            return result.ToResponse();
        }

        #endregion

        #region DELETE

        /// <summary>
        /// Delete an existing project
        /// </summary>
        /// <remarks>
        /// Return whether the deleting is successed or failed
        /// </remarks>
        /// <param name="projectId">Project id that need to be deleted</param>
        /// <returns>Return whether the deleting is successed or failed</returns>
        /// <response code="200">Successful operation</response>
        /// <response code="204">Successful operation, no content is returned</response>
        /// <response code="400">Invalid Id supplied</response>
        /// <response code="401">User is unauthorized</response>
        /// <response code="403">User is not authenticated</response>
        /// <response code="404">Request is inaccessible</response>
        /// <response code="409">Data is conflicted</response>
        /// <response code="500">Server side error</response>
        [HttpDelete("{projectId:int}")]
        public ActionResult<bool> Delete(int projectId)
        {
            var result = _projectService.Delete(projectId);
            return result.ToResponse();
        }

        #endregion
    }
}
