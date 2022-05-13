using Kloon.EmployeePerformance.Logic.Services;
using Kloon.EmployeePerformance.Models.Common;
using Kloon.EmployeePerformance.Models.User;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Kloon.EmployeePerformance.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        public UserController(IUserService userService)
        {
            _userService = userService;
        }
        [HttpGet]
        public ActionResult<List<UserModel>> GetAll([FromQuery] string searchText)
        {
            return _userService.GetAll(searchText).ToResponse();
        }

        [HttpPost("UsersForTimeSheetReport")]
        public ActionResult<List<UserModel>> GetUsersForTimeSheetReport([FromBody] List<int> projectIds)
        {
            return _userService.GetUsersForTimeSheetReport(projectIds).ToResponse();
        }
        [HttpGet("{id:int}")]
        public ActionResult<UserModel> GetById(int id)
        {
            return _userService.GetById(id).ToResponse();
        }
        [HttpPost]
        public ActionResult<UserModel> Create(UserModel userModel)
        {
            return _userService.Create(userModel).ToResponse();
        }
        [HttpPut]
        public ActionResult<UserModel> Update(UserModel userModel)
        {
            return _userService.Update(userModel).ToResponse();
        }
        [HttpDelete("{id:int}")]
        public ActionResult<bool> Delete(int id)
        {
            return _userService.Delete(id).ToResponse();
        }

        [HttpPost("ChangedPassword")]
        public async Task<ResetPassword> ChangedPassword(ResetPassword resetPasswordModel)
        {
            var result = _userService.ChangedPassword(resetPasswordModel);

            return result.ToResponse();
        }
    }
}
