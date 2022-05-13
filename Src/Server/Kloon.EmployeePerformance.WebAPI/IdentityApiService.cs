using Kloon.EmployeePerformance.Logic.Caches;
using Kloon.EmployeePerformance.Logic.Services.Base;
using Kloon.EmployeePerformance.Models.Common;
using Kloon.EmployeePerformance.Models.User;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Kloon.EmployeePerformance.WebAPI
{
    public class IdentityApiService : IIdentityService
    {
        public IHttpContextAccessor _contextAccessor;
        private CacheProvider _cache;

        public IdentityApiService(CacheProvider cache, IHttpContextAccessor httpContextAccessor)
        {
            _cache = cache;
            _contextAccessor = httpContextAccessor;
        }

        public LoginUserModel GetCurrentUser()
        {
            var userClaims = _contextAccessor.HttpContext.User.Claims;
            var tokenRoleName = userClaims.Where(t => t.Type == ClaimTypes.Role).Select(t => t.Value).SingleOrDefault();
            var userIdString = userClaims.Where(c => c.Type == ClaimTypes.Sid)
             .Select(c => c.Value).SingleOrDefault();

            var isValidIdString = Int32.TryParse(userIdString, out int userId);

            var rs = new LoginUserModel
            {
                User = null
            };

            var user = _cache.Users.Get(userId);

            if (user == null || !user.Status)
                return rs;

            var lstProject = _cache.Users.GetProjects(user.Id);

            var loginUser = new LoginUserModel.LoginUser
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Role = (Roles)Enum.Parse(typeof(Roles), tokenRoleName.ToString().ToUpper()),
                ProjectRole = lstProject?.Select(t => new ProjectRoleModel
                {
                    ProjectId = t.Id,
                    ProjectRoleId = (ProjectRoles)t.ProjectRoleId
                }).ToList()
            };

            rs.User = loginUser;
            return rs;
        }
    }
}
