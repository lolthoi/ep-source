using Kloon.EmployeePerformance.DataAccess;
using Kloon.EmployeePerformance.DataAccess.Domain;
using Kloon.EmployeePerformance.Logic.Caches.Data;
using Kloon.EmployeePerformance.Logic.Common;
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
    public interface IProjectService
    {
        ResultModel<List<ProjectModel>> GetAll(string searchText = "");
        ResultModel<List<ProjectModel>> GetAllProjectOfUser();
        ResultModel<List<ProjectModel>> GetProjectOfLeader();
        ResultModel<ProjectModel> GetById(int projectId);
        ResultModel<ProjectModel> Create(ProjectModel projectModel);
        ResultModel<ProjectModel> Update(ProjectModel projectModel);
        ResultModel<bool> Delete(int id);
        ResultModel<List<ProjectModel>> GetTeamForReport();
    }
    public class ProjectService : IProjectService
    {
        private readonly IAuthenLogicService<ProjectService> _logicService;
        private readonly IUnitOfWork<EmployeePerformanceContext> _dbContext;

        private readonly IEntityRepository<Project> _projects;
        private readonly IEntityRepository<ProjectUser> _projectUsers;
        private readonly IEntityRepository<TSActivityGroup> _activityGroupRepository;
        private readonly IEntityRepository<TSActivity> _activityRepository;
        private readonly IEntityRepository<ActivityGroupUser> _activityGroupUserRepository;

        public ProjectService(
            IAuthenLogicService<ProjectService> logicService
        )
        {
            _logicService = logicService;
            _dbContext = logicService.DbContext;

            _projects = _dbContext.GetRepository<Project>();
            _projectUsers = _dbContext.GetRepository<ProjectUser>();
            _activityGroupRepository = _dbContext.GetRepository<TSActivityGroup>();
            _activityRepository = _dbContext.GetRepository<TSActivity>();
            _activityGroupUserRepository = _dbContext.GetRepository<ActivityGroupUser>();
        }

        public ResultModel<List<ProjectModel>> GetAll(string searchText = "")
        {
            var result = _logicService
                .Start()
                .ThenAuthorize(Roles.ADMINISTRATOR, Roles.USER)
                .ThenValidate(currentUser => null)
                .ThenImplement(currentUser =>
                {
                    IEnumerable<ProjectMD> query = null;
                    if (currentUser.Role == Roles.USER)
                    {
                        query = _logicService.Cache.Users.GetProjects(currentUser.Id).ToList();

                        List<ProjectModel> data = new List<ProjectModel>();
                        foreach (var item in query)
                        {
                            var project = _logicService.Cache.Projects.Get(item.Id);

                            if (project != null)
                            {
                                ProjectModel projectModel = new ProjectModel()
                                {
                                    Id = item.Id,
                                    Name = project.Name,
                                    Description = project.Description,
                                    Status = (ProjectStatusEnum)project.Status,
                                    StartDate = project.StartDate,
                                    EndDate = project.EndDate
                                };
                                data.Add(projectModel);
                            }

                        }
                        return data;
                    }
                    else
                    {
                        query = _logicService.Cache.Projects.GetValues();

                        if (!string.IsNullOrWhiteSpace(searchText))
                        {
                            searchText = searchText.Trim();
                            query = query.Where(x => x.Name.Contains(searchText, StringComparison.OrdinalIgnoreCase));
                        }

                        var record = query
                            .OrderBy(x => x.Name)
                            .Where(x => x.DeletedBy == null && x.DeletedDate == null)
                            .Select(t => new ProjectModel
                            {
                                Id = t.Id,
                                Name = t.Name,
                                Description = t.Description,
                                Status = (ProjectStatusEnum)t.Status,
                                StartDate = t.StartDate,
                                EndDate = t.EndDate
                            })
                            .ToList();
                        return record;
                    }


                });
            return result;
        }

        public ResultModel<List<ProjectModel>> GetAllProjectOfUser()
        {
            var result = _logicService
                .Start()
                .ThenAuthorize(Roles.ADMINISTRATOR, Roles.USER)
                .ThenValidate(currentUser => null)
                .ThenImplement(currentUser =>
                {
                    List<ProjectModel> projectModels = new List<ProjectModel>();
                    List<ProjectUser> projectUsers = _projectUsers.Query(x => x.UserId == currentUser.Id).ToList();
                    if (projectUsers.Count > 0)
                    {
                        List<int> projectIds = projectUsers.Select(x => x.ProjectId).ToList();
                        List<Project> projects = _projects.Query(x => projectIds.Contains(x.Id)).ToList();

                        foreach (var item in projectUsers)
                        {
                            Project project = projects.Where(x => x.Id == item.ProjectId).FirstOrDefault();
                            if (project != null)
                            {
                                ProjectModel projectModel = new ProjectModel()
                                {
                                    Id = project.Id,
                                    Name = project.Name
                                };
                                projectModels.Add(projectModel);
                            }
                        }
                    }
                    return projectModels.OrderBy(x => x.Name).ToList();
                });
            return result;
        }

        public ResultModel<List<ProjectModel>> GetProjectOfLeader()
        {
            var result = _logicService
                .Start()
                .ThenAuthorize(Roles.ADMINISTRATOR, Roles.USER)
                .ThenValidate(currentUser => null)
                .ThenImplement(currentUser =>
                {
                    List<ProjectMD> projects = new List<ProjectMD>();
                    if (currentUser.Role == Roles.USER)
                    {
                        projects = _logicService.Cache._userProjects.Get(currentUser.Id).Where(x => x.ProjectRoleId == (int)ProjectRoles.PM).ToList();
                        projects.ForEach(x =>
                        {
                            var project = _logicService.Cache.Projects.Get(x.Id);
                            x.Name = project.Name;
                        });
                    }
                    else
                    {
                        projects = _logicService.Cache.Projects.GetValues().ToList();
                    }

                    return projects.Select(x => new ProjectModel { Id = x.Id, Name = x.Name }).OrderBy(x => x.Name).ToList();
                });
            return result;
        }

        public ResultModel<ProjectModel> GetById(int projectId)
        {
            Project project = null;
            var result = _logicService
                .Start()
                .ThenAuthorize(Roles.ADMINISTRATOR, Roles.USER)
                .ThenValidate(current =>
                {
                    if (current.Role == Roles.USER)
                    {
                        var users = _logicService.Cache.Projects.GetUsers(projectId);
                        var userInProject = users.Where(x => x.Id == current.Id).FirstOrDefault();
                        if (userInProject == null)
                        {
                            return new ErrorModel(ErrorType.NOT_AUTHORIZED, "You do not have access to this project");
                        }
                    }

                    project = _projects.Query(x => x.Id == projectId && x.DeletedBy == null && x.DeletedDate == null).FirstOrDefault();
                    if (project == null)
                    {
                        return new ErrorModel(ErrorType.NOT_EXIST, "Project not found");
                    }
                    return null;
                })
                .ThenImplement(current =>
                {
                    var projectVM = new ProjectModel()
                    {
                        Id = project.Id,
                        Description = project.Description,
                        Name = project.Name,
                        Status = (ProjectStatusEnum)project.Status,
                        StartDate = project.StartDate,
                        EndDate = project.EndDate
                    };
                    return projectVM;
                });
            return result;
        }

        public ResultModel<ProjectModel> Create(ProjectModel projectModel)
        {
            var now = DateTime.Now;
            var result = _logicService
               .Start()
               .ThenAuthorize(Roles.ADMINISTRATOR)
               .ThenValidate(current =>
               {
                   var error = ValidateProject(projectModel);
                   if (error != null)
                   {
                       return error;
                   }
                   var hasSameProjectName = _logicService.Cache
                        .Projects
                        .GetValues()
                        .Where(x => x.DeletedDate == null && x.DeletedBy == null)
                        .Any(x => x.Name.Equals(projectModel.Name, StringComparison.OrdinalIgnoreCase));
                   if (hasSameProjectName)
                   {
                       return new ErrorModel(ErrorType.DUPLICATED, "The Project Name already exists");
                   }
                   var existedActivityGroup = _logicService.Cache.TSActivityGroups.GetValues().Where(x => !x.DeletedDate.HasValue).Any(x => x.Name.Equals(projectModel.Name.Trim(), StringComparison.OrdinalIgnoreCase));
                   if (existedActivityGroup)
                       return new ErrorModel(ErrorType.DUPLICATED, "Activity Group Name already exists");
                   return null;
               })
               .ThenImplement(current =>
               {
                   try
                   {
                       _dbContext.BeginTransaction();
                       //Create project
                       var project = new Project()
                       {
                           Name = projectModel.Name.Trim(),
                           Status = (int)projectModel.Status,
                           Description = projectModel.Description,
                           StartDate = projectModel.StartDate,
                           EndDate = projectModel.EndDate,
                           CreatedBy = current.Id,
                           CreatedDate = now
                       };
                       _projects.Add(project);
                       _dbContext.Save();
                       projectModel.Id = project.Id;
                       _logicService.Cache.Projects.Clear();
                       //Create Activity Group for timesheet at the same time
                       TSActivityGroup activityGroup = new()
                       {
                           ProjectId = project.Id,
                           Name = project.Name,
                           Description = project.Description,
                           CreatedBy = current.Id,
                           CreatedDate = DateTime.UtcNow,
                       };
                       _activityGroupRepository.Add(activityGroup);
                       _dbContext.Save();
                       _logicService.Cache.TSActivityGroups.Clear();
                       _dbContext.CommitTransaction();
                       return projectModel;
                   }
                   catch (Exception e)
                   {
                       _dbContext.RollbackTransaction();
                       throw new Exception(e.Message);
                   }
               });
            return result;
        }

        public ResultModel<bool> Delete(int id)
        {
            var now = DateTime.Now;
            Project project = null;
            TSActivityGroup activityGroup = new();
            var result = _logicService
                .Start()
                .ThenAuthorize(Roles.ADMINISTRATOR)
                .ThenValidate(currentUser =>
                {
                    project = _projects
                        .Query(x => x.Id == id && x.DeletedBy == null && x.DeletedDate == null)
                        .FirstOrDefault();
                    if (project == null)
                    {
                        return new ErrorModel(ErrorType.NOT_EXIST, "Project not found");
                    }

                    return null;
                })
                .ThenImplement(currentUser =>
                {
                    try
                    {
                        _dbContext.BeginTransaction();
                        //Delete Project and ProjectUsers
                        List<ProjectUser> projectUsers = _projectUsers.Query(x => x.ProjectId == id && x.DeletedBy == null && x.DeletedDate == null).ToList();
                        foreach (var item in projectUsers)
                        {
                            item.DeletedBy = currentUser.Id;
                            item.DeletedDate = now;
                        }
                        project.DeletedBy = currentUser.Id;
                        project.DeletedDate = now;

                        //Delete ActivityGroup created when creating project and all of its activities and access users at the same time
                        activityGroup = _activityGroupRepository.Query(x => !x.DeletedDate.HasValue && x.ProjectId == id).FirstOrDefault();
                        if (activityGroup != null)
                        {
                            List<TSActivity> listActivityDomain = _activityRepository.Query(x => !x.DeletedDate.HasValue && x.TSActivityGroupId == activityGroup.Id).ToList();
                            List<ActivityGroupUser> listActivityGroupUserDomain = _activityGroupUserRepository.Query(x => !x.DeletedDate.HasValue && x.TSActivityGroupId == activityGroup.Id).ToList();
                            if (listActivityDomain.Count > 0)
                            {
                                foreach (var item in listActivityDomain)
                                {
                                    item.DeletedBy = currentUser.Id;
                                    item.DeletedDate = DateTime.UtcNow;
                                }
                            }
                            if (listActivityGroupUserDomain.Count > 0)
                            {
                                foreach (var item in listActivityGroupUserDomain)
                                {
                                    item.DeletedBy = currentUser.Id;
                                    item.DeletedDate = DateTime.UtcNow;
                                }
                            }
                            activityGroup.DeletedBy = currentUser.Id;
                            activityGroup.DeletedDate = DateTime.UtcNow;
                        }

                        _dbContext.Save();
                        _dbContext.CommitTransaction();
                        _logicService.Cache.Projects.Clear();
                        _logicService.Cache.TSActivityGroups.Clear();
                        return true;
                    }
                    catch (Exception e)
                    {
                        _dbContext.RollbackTransaction();
                        throw new Exception(e.Message);
                    }
                });
            return result;
        }

        public ResultModel<List<ProjectModel>> GetTeamForReport()
        {
            var result = _logicService
                .Start()
                .ThenAuthorize(Roles.ADMINISTRATOR, Roles.USER)
                .ThenValidate(currentUser =>
                {
                    return null;
                })
                .ThenImplement(currentUser =>
                {
                    var allProject = _logicService.Cache.Projects.GetValues().Where(x => !x.DeletedDate.HasValue).ToList();
                    List<ProjectModel> listResult = new();
                    //Check Role
                    if (currentUser.Role == Roles.USER)
                    {
                        var tSAcitivityGroupIds = _activityGroupUserRepository.Query(x => x.UserId == currentUser.Id && x.DeletedDate == null && x.DeletedBy == null)
                            .Select(x => x.TSActivityGroupId).ToList();
                        if (tSAcitivityGroupIds.Count > 0)
                        {
                            var listCommonGroup = _logicService.Cache.TSActivityGroups.GetValues().Where(x => tSAcitivityGroupIds.Contains(x.Id) && x.ProjectId == null).Select(x => x.ProjectId).ToList();
                            if (listCommonGroup.Count > 0)
                            {
                                listResult = allProject.OrderBy(x => x.Name)
                                .Select(t => new ProjectModel
                                {
                                    Id = t.Id,
                                    Name = t.Name,
                                    Description = t.Description,
                                    Status = (ProjectStatusEnum)t.Status,
                                    StartDate = t.StartDate,
                                    EndDate = t.EndDate
                                }).ToList();
                            }
                            else
                            {
                                var listRealProject = _logicService.Cache.TSActivityGroups.GetValues().Where(x => tSAcitivityGroupIds.Contains(x.Id) && x.ProjectId != null).Select(x => x.ProjectId).ToList();
                                listResult = allProject.Where(x => listRealProject.Contains(x.Id)).OrderBy(x => x.Name)
                                .Select(t => new ProjectModel
                                {
                                    Id = t.Id,
                                    Name = t.Name,
                                    Description = t.Description,
                                    Status = (ProjectStatusEnum)t.Status,
                                    StartDate = t.StartDate,
                                    EndDate = t.EndDate
                                }).ToList();
                            }
                        }
                    }
                    else
                    {
                        listResult = allProject.OrderBy(x => x.Name)
                            .Select(t => new ProjectModel
                            {
                                Id = t.Id,
                                Name = t.Name,
                                Description = t.Description,
                                Status = (ProjectStatusEnum)t.Status,
                                StartDate = t.StartDate,
                                EndDate = t.EndDate
                            })
                            .ToList();
                    }
                    return listResult;
                });
            return result;
        }
        public ResultModel<ProjectModel> Update(ProjectModel projectModel)
        {
            var now = DateTime.Now;
            Project project = null;
            TSActivityGroup activityGroup = new();
            var result = _logicService
               .Start()
               .ThenAuthorize(Roles.ADMINISTRATOR)
               .ThenValidate(current =>
               {
                   var error = ValidateProject(projectModel);
                   if (error != null)
                   {
                       return error;
                   }
                   var hasSameProjectName = _logicService.Cache
                        .Projects
                        .GetValues()
                         .Where(x => x.DeletedDate == null && x.DeletedBy == null)
                        .Any(x => x.Id != projectModel.Id && x.Name.Equals(projectModel.Name, StringComparison.OrdinalIgnoreCase));
                   if (hasSameProjectName)
                   {
                       return new ErrorModel(ErrorType.DUPLICATED, "The Project Name already exists");
                   }
                   project = _projects.Query(x => x.Id == projectModel.Id).FirstOrDefault();
                   if (project == null)
                   {
                       return new ErrorModel(ErrorType.NOT_EXIST, "Project not found");
                   }
                   if (project.DeletedBy != null && project.DeletedDate != null)
                   {
                       return new ErrorModel(ErrorType.NOT_EXIST, "Project not found");
                   }
                   var existedActivityGroup = _logicService.Cache.TSActivityGroups.GetValues().Where(x => !x.DeletedDate.HasValue && x.ProjectId != projectModel.Id)
                       .Any(x => x.Name.Equals(projectModel.Name.Trim(), StringComparison.OrdinalIgnoreCase));
                   if (existedActivityGroup)
                       return new ErrorModel(ErrorType.DUPLICATED, "Activity Group Name existed");
                   return null;
               })
               .ThenImplement(current =>
               {
                   try
                   {
                       _dbContext.BeginTransaction();
                       //Update project
                       project.Name = projectModel.Name.Trim();
                       project.Status = (int)projectModel.Status;
                       project.Description = projectModel.Description;
                       project.StartDate = projectModel.StartDate;
                       project.EndDate = projectModel.EndDate;
                       project.ModifiedBy = current.Id;
                       project.ModifiedDate = now;

                       //Update activity group created when creating project
                       activityGroup = _activityGroupRepository.Query(x => !x.DeletedDate.HasValue).Where(x => x.ProjectId == projectModel.Id).FirstOrDefault();
                       if (activityGroup != null)
                       {
                           activityGroup.Name = projectModel.Name.Trim();
                           activityGroup.Description = projectModel.Description;
                           activityGroup.ModifiedDate = DateTime.UtcNow;
                           activityGroup.ModifiedBy = current.Id;
                       }

                       _dbContext.Save();
                       _dbContext.CommitTransaction();
                       _logicService.Cache.Projects.Clear();
                       _logicService.Cache.TSActivityGroups.Clear();
                       return projectModel;
                   }
                   catch (Exception e)
                   {
                       _dbContext.RollbackTransaction();
                       throw new Exception(e.Message);
                   }
               });
            return result;
        }
        private ErrorModel ValidateProject(ProjectModel projectModel)
        {
            if (projectModel == null)
            {
                return new ErrorModel(ErrorType.BAD_REQUEST, "Please fill in the required fields");
            }
            if (string.IsNullOrEmpty(projectModel.Name))
            {
                return new ErrorModel(ErrorType.BAD_REQUEST, "Name is required");
            }
            if (projectModel.Name.Length > 50)
            {
                return new ErrorModel(ErrorType.BAD_REQUEST, "Max length of Project name is 50");
            }
            if ((!string.IsNullOrEmpty(projectModel.Description) && projectModel.Description.Length > 500))
            {
                return new ErrorModel(ErrorType.BAD_REQUEST, "Max length of Description is 500");
            }

            if (projectModel.EndDate != null)
            {
                if (projectModel.StartDate > projectModel.EndDate)
                {
                    return new ErrorModel(ErrorType.BAD_REQUEST, "The date in the field Start Date must be less than the date in field End Date");
                }
            }

            //if (projectModel.Status <= 0 || projectModel.Status >= 4)
            //{
            //    return new ErrorModel(ErrorType.BAD_REQUEST, "Status must be selected");
            //}

            return null;
        }
    }
}
