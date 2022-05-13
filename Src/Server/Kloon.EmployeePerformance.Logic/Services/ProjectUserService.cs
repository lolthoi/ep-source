using Kloon.EmployeePerformance.DataAccess;
using Kloon.EmployeePerformance.DataAccess.Domain;
using Kloon.EmployeePerformance.Logic.Services.Base;
using Kloon.EmployeePerformance.Models.Common;
using Kloon.EmployeePerformance.Models.Project;
using Kloon.EmployeePerformance.Models.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kloon.EmployeePerformance.Logic.Services
{
    public interface IProjectUserService
    {
        ResultModel<List<ProjectUserModel>> GetAll(int projectId, string searchText = "");
        ResultModel<ProjectUserModel> GetById(int projectId, Guid projectUserId);
        ResultModel<ProjectUserModel> Create(int projectId, int userId);
        ResultModel<ProjectUserModel> Update(int projectId, Guid projectUserId, int projectRoleId);
        ResultModel<bool> Delete(int projectId, Guid projectUserId);
        ResultModel<List<UserModel>> GetTopFiveUserNotInProject(int projectId, string searchText = "");
    }
    public class ProjectUserService : IProjectUserService
    {
        private readonly IAuthenLogicService<ProjectService> _logicService;
        private readonly IUnitOfWork<EmployeePerformanceContext> _dbContext;

        private readonly IEntityRepository<User> _users;
        private readonly IEntityRepository<ProjectUser> _projectUsers;
        private readonly IEntityRepository<Project> _projects;

        public ProjectUserService(
            IAuthenLogicService<ProjectService> logicService
        )
        {
            _logicService = logicService;
            _dbContext = logicService.DbContext;

            _users = _dbContext.GetRepository<User>();
            _projectUsers = _dbContext.GetRepository<ProjectUser>();
            _projects = _dbContext.GetRepository<Project>();
        }
        public ResultModel<ProjectUserModel> Create(int projectId, int userId)
        {
            DateTime now = DateTime.Now;
            var result = _logicService
                .Start()
                .ThenAuthorize(Roles.ADMINISTRATOR)
                .ThenValidate(currentUser =>
                {
                    var error = ValidateProjectMember(userId, projectId);
                    if (error != null)
                    {
                        return error;
                    }

                    var projectMember = _projectUsers.Query(x => x.UserId == userId && x.ProjectId == projectId && x.DeletedBy == null && x.DeletedDate == null)
                        .FirstOrDefault();
                    if (projectMember != null)
                    {
                        return new ErrorModel(ErrorType.DUPLICATED, "User already exists in project");
                    }

                    return null;
                })
                .ThenImplement(currentUser =>
                {
                    var projectMenber = new ProjectUser()
                    {
                        UserId = userId,
                        ProjectId = projectId,
                        ProjectRoleId = (int)ProjectRoles.MEMBER,
                        CreatedBy = currentUser.Id,
                        CreatedDate = now
                    };
                    _projectUsers.Add(projectMenber);

                    int result = _dbContext.Save();
                    _logicService.Cache.Projects.Clear();
                    _logicService.Cache.Users.Clear();
                    return new ProjectUserModel()
                    {
                        Id = projectMenber.Id,
                        UserId = userId,
                        ProjectId = projectId,
                        ProjectRoleId = ProjectRoles.MEMBER
                    };
                });
            return result;
        }

        public ResultModel<bool> Delete(int projectId, Guid projectUserId)
        {
            var now = DateTime.Now;
            ProjectUser projectUser = null;
            var result = _logicService
                .Start()
                .ThenAuthorize(Roles.ADMINISTRATOR)
                .ThenValidate(currentUser =>
                {
                    projectUser = _projectUsers
                        .Query(x => x.Id == projectUserId && x.DeletedBy == null && x.DeletedDate == null)
                        .FirstOrDefault();
                    if (projectUser == null)
                    {
                        return new ErrorModel(ErrorType.NOT_EXIST, "Project Member not found");
                    }

                    if (projectUser.UserId == currentUser.Id)
                    {
                        return new ErrorModel(ErrorType.BAD_REQUEST, "You must not delete yourself in the project");
                    }
                    return null;
                })
                .ThenImplement(currentUser =>
                {
                    projectUser.DeletedBy = currentUser.Id;
                    projectUser.DeletedDate = now;

                    int result = _dbContext.Save();

                    _logicService.Cache.Projects.Clear();
                    _logicService.Cache.Users.Clear();
                    return true;
                });
            return result;
        }

        public ResultModel<List<ProjectUserModel>> GetAll(int projectId, string searchText = "")
        {
            var result = _logicService
                .Start()
                .ThenAuthorize(Roles.ADMINISTRATOR, Roles.USER)
                .ThenAuthorizeProject(projectId, ProjectRoles.PM, ProjectRoles.MEMBER, ProjectRoles.QA)
                .ThenValidate(currentUser =>
                {
                    var project = _projects.Query(x => x.DeletedBy == null && x.DeletedDate == null && x.Id == projectId);
                    if (project == null)
                    {
                        return new ErrorModel(ErrorType.NOT_EXIST, "Project not found");
                    }
                    return null;
                })
                .ThenImplement(currentUser =>
                {
                    var projects = _logicService.Cache.Projects.GetUsers(projectId).OrderBy(x => x.Id);

                    List<ProjectUserModel> projectUserModels = new List<ProjectUserModel>();
                    foreach (var item in projects)
                    {
                        var user = _logicService.Cache.Users.Get(item.Id);
                        if (user != null && user.Status)
                        {
                            ProjectUserModel projectUserModel = new ProjectUserModel()
                            {
                                Id = item.ProjectUserId.Value,
                                Email = user.Email,
                                FirstName = user.FirstName,
                                LastName = user.LastName,
                                ProjectId = projectId,
                                ProjectRoleId = (ProjectRoles)item.ProjectRoleId,
                                UserId = user.Id
                            };
                            projectUserModels.Add(projectUserModel);
                        }
                    }
                    return projectUserModels;

                });
            return result;
        }

        public ResultModel<ProjectUserModel> GetById(int projectId, Guid projectUserId)
        {
            ProjectUser projectUser = null;
            Project project = null;
            User user = null;
            var result = _logicService
                .Start()
                .ThenAuthorize(Roles.ADMINISTRATOR, Roles.USER)
                .ThenAuthorizeProject(projectId, ProjectRoles.PM, ProjectRoles.MEMBER, ProjectRoles.QA)
                .ThenValidate(currentUser =>
                {
                    projectUser = _projectUsers.Query(x => x.Id == projectUserId && x.DeletedBy == null && x.DeletedDate == null).FirstOrDefault();

                    if (projectUser == null)
                    {
                        return new ErrorModel(ErrorType.NOT_EXIST, "Project User not found");
                    }

                    project = _projects.Query(x => x.DeletedBy == null && x.DeletedDate == null && x.Id == projectId).FirstOrDefault();
                    if (project == null)
                    {
                        return new ErrorModel(ErrorType.NOT_EXIST, "Project not found");
                    }

                    user = _users.Query(x => x.DeletedBy == null && x.DeletedDate == null && x.Id == projectUser.UserId).FirstOrDefault();
                    if (user == null)
                    {
                        return new ErrorModel(ErrorType.NOT_EXIST, "User not found");
                    }
                    return null;
                })
                .ThenImplement(currentUser =>
                {
                    return new ProjectUserModel
                    {
                        Id = projectUser.Id,
                        ProjectId = projectUser.ProjectId,
                        ProjectRoleId = (ProjectRoles)projectUser.ProjectRoleId,
                        Email = user.Email,
                        UserId = user.Id,
                        ProjectName = project.Name
                    };
                });
            return result;
        }

        public ResultModel<List<UserModel>> GetTopFiveUserNotInProject(int projectId, string searchText = "")
        {
            var result = _logicService
                .Start()
                .ThenAuthorize(Roles.ADMINISTRATOR, Roles.USER)
                .ThenValidate(currentUser => null)
                .ThenImplement(currentUser =>
                {

                    var users = _logicService.Cache
                        .Projects
                        .GetUsers(projectId);

                    List<int> userIds = new List<int>();
                    if (users.Count > 0)
                    {
                        userIds = users.Select(x => x.Id).ToList();
                    }

                    var query = _logicService.Cache.Users.GetValues()
                        .Where(x => x.DeletedBy == null && x.Status && x.DeletedDate == null && !userIds.Contains(x.Id)).
                        AsEnumerable();

                    if (!string.IsNullOrWhiteSpace(searchText))
                    {
                        query = query.Where(t => t.FullName.Contains(searchText, StringComparison.OrdinalIgnoreCase)
                                                || t.Email.Contains(searchText, StringComparison.OrdinalIgnoreCase));
                    }
                    var record = query
                        .Select(x => new UserModel()
                        {
                            Id = x.Id,
                            Email = x.Email,
                            FirstName = x.FirstName,
                            LastName = x.LastName
                        })
                        .OrderBy(x => x.FirstName)
                        .ToList();
                    return record;

                });
            return result;
        }

        public ResultModel<ProjectUserModel> Update(int projectId, Guid projectUserId, int projectRoleId)
        {
            var now = DateTime.Now;
            ProjectUser projectUser = null;
            var result = _logicService
                .Start()
                .ThenAuthorize(Roles.ADMINISTRATOR)
                .ThenValidate(currentUser =>
                {
                    projectUser = _projectUsers
                        .Query(x => x.Id == projectUserId && x.DeletedBy == null && x.DeletedDate == null)
                        .FirstOrDefault();
                    if (projectUser == null)
                    {
                        return new ErrorModel(ErrorType.NOT_EXIST, "Project Member not found");
                    }
                    var error = ValidateProjectMember(projectUser.UserId, projectUser.ProjectId);
                    if (error != null)
                    {
                        return error;
                    }
                    return null;
                })
                .ThenImplement(currentUser =>
                {
                    projectUser.ProjectRoleId = projectRoleId;
                    projectUser.ModifiedBy = currentUser.Id;
                    projectUser.ModifiedDate = now;

                    int result = _dbContext.Save();

                    _logicService.Cache.Projects.Clear();
                    _logicService.Cache.Users.Clear();
                    return new ProjectUserModel()
                    {
                        Id = projectUser.Id,
                        UserId = projectUser.UserId,
                        ProjectId = projectUser.ProjectId,
                        ProjectRoleId = (ProjectRoles)projectRoleId
                    };
                });
            return result;
        }

        private ErrorModel ValidateProjectMember(int userId, int projectId)
        {
            var user = _users.Query(x => x.Id == userId && x.DeletedBy == null && x.DeletedDate == null).FirstOrDefault();
            if (user == null)
            {
                return new ErrorModel(ErrorType.NOT_EXIST, "Please choose the user to add to the Project.");
            }
            var project = _projects.Query(x => x.Id == projectId && x.DeletedBy == null && x.DeletedDate == null).FirstOrDefault();
            if (project == null)
            {
                return new ErrorModel(ErrorType.NOT_EXIST, "Project not found");
            }
            return null;
        }
    }
}
