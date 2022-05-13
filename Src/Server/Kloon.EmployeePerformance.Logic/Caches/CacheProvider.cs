using Kloon.EmployeePerformance.DataAccess;
using Kloon.EmployeePerformance.DataAccess.Domain;
using Kloon.EmployeePerformance.Logic.Caches.Data;
using Kloon.EmployeePerformance.Models.WorkSpaceSetting;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kloon.EmployeePerformance.Logic.Caches
{
    public class CacheProvider
    {
        private readonly IServiceProvider _serviceProvider;

        #region Users

        public UserAllCache Users { get; private set; }
        public DataKeyCache<int, List<ProjectMD>> _userProjects = new DataKeyCache<int, List<ProjectMD>>();

        #endregion

        #region Projects

        public ProjectAllCache Projects { get; private set; }
        public DataKeyCache<int, List<UserMD>> _projectUser = new DataKeyCache<int, List<UserMD>>();

        #endregion

        #region Criterias Store

        public CriteriaTypeStoreAllCache CriteriaTypeStores { get; private set; }

        private DataKeyCache<Guid, List<CriteriaStoreMD>> _criteriaStores = new DataKeyCache<Guid, List<CriteriaStoreMD>>();

        public CriteriaStoreAllCache CriteriaStores { get; private set; } = new CriteriaStoreAllCache();

        #endregion

        #region Criterias

        public QuarterCriteriaTemplateAllCache QuarterCriteriaTemplates { get; private set; }

        private DataKeyCache<Guid, List<CriteriaTypeMD>> _criteriaTypes = new DataKeyCache<Guid, List<CriteriaTypeMD>>();

        public CriteriaTypeAllCache CriteriaTypes { get; private set; }

        public CriteriaAllCache Criterias { get; private set; } = new CriteriaAllCache();

        private DataKeyCache<Guid, List<CriteriaMD>> _criterias = new DataKeyCache<Guid, List<CriteriaMD>>();

        #endregion

        #region TimeSheet

        public TSActivityGroupAllCache TSActivityGroups { get; private set; }
        private DataKeyCache<Guid, List<ActivityGroupUserMD>> _activityGroupUser = new DataKeyCache<Guid, List<ActivityGroupUserMD>>();
        private DataKeyCache<Guid, List<TSActivityMD>> _activity = new DataKeyCache<Guid, List<TSActivityMD>>();
        public TSActivityAllCache TSActivities { get; private set; }

        #endregion

        #region Other

        public PositionAllCache Position { get; private set; } = new PositionAllCache();

        public AppSettingAllCache AppSetting { get; private set; } = new AppSettingAllCache();

        public WorkSpaceSettingAllCache WorkSpaceSetting { get; private set; }
        private DataKeyCache<string, WorkSpaceSettingMD> _workSpaceSetting = new DataKeyCache<string, WorkSpaceSettingMD>();

        #endregion

        #region Constructor and private function

        public CacheProvider(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;

            Users = new UserAllCache(this);
            Projects = new ProjectAllCache(this);
            CriteriaTypeStores = new CriteriaTypeStoreAllCache(this);

            QuarterCriteriaTemplates = new QuarterCriteriaTemplateAllCache(this);
            CriteriaTypes = new CriteriaTypeAllCache(this);
            TSActivityGroups = new TSActivityGroupAllCache(this);
            TSActivities = new TSActivityAllCache(this);

            WorkSpaceSetting = new WorkSpaceSettingAllCache(this);

            RegisterData();
        }

        private T UseDbContext<T>(Func<IUnitOfWork<EmployeePerformanceContext>, T> func)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetService<IUnitOfWork<EmployeePerformanceContext>>();
                return func.Invoke(dbContext);
            }
        }

        private void RegisterData()
        {
            Users.OnNeedResource += Users_OnNeedResource;
            _userProjects.OnNeedValueIfKeyNotFound += _userProject_OnNeedValueIfKeyNotFound;

            CriteriaTypeStores.OnNeedResource += CriteriaTypeStores_OnNeedResource;
            _criteriaStores.OnNeedValueIfKeyNotFound += _criteriaStores_OnNeedValueIfKeyNotFound;

            CriteriaStores.OnNeedResource += CriteriaStores_OnNeedResource;

            Projects.OnNeedResource += Projects_OnNeedResource;
            _projectUser.OnNeedValueIfKeyNotFound += _projectUser_OnNeedValueIfKeyNotFound;

            Position.OnNeedResource += Position_OnNeedResource;

            AppSetting.OnNeedResource += AppSetting_OnNeedResource;

            WorkSpaceSetting.OnNeedResource += WorkSpaceSetting_OnNeedResource;
            _workSpaceSetting.OnNeedValueIfKeyNotFound += _workSpaceSetting_OnNeedValueIfKeyNotFound;

            QuarterCriteriaTemplates.OnNeedResource += QuarterCriteriaTemplates_OnNeedResource;
            _criteriaTypes.OnNeedValueIfKeyNotFound += _criteriaTypes_OnNeedValueIfKeyNotFound;

            CriteriaTypes.OnNeedResource += CriteriaTypes_OnNeedResource;
            _criterias.OnNeedValueIfKeyNotFound += _criterias_OnNeedValueIfKeyNotFound;

            Criterias.OnNeedResource += Criterias_OnNeedResource;

            //TimeSheet Cache
            TSActivityGroups.OnNeedResource += TSActivityGroup_OnNeedResource;
            _activityGroupUser.OnNeedValueIfKeyNotFound += ActivityGroupUser_OnNeedResource;
            _activity.OnNeedValueIfKeyNotFound += Activity_OnNeedResource;

            TSActivities.OnNeedResource += TSActivity_OnNeedResource;
        }



        #endregion

        #region Events

        #region Users

        private Dictionary<int, UserMD> Users_OnNeedResource(object sender, params object[] paramArrs)
        {
            return UseDbContext(dbContext =>
            {
                var result = dbContext.GetRepository<User>()
                    .Query()
                    .Where(t => !t.DeletedBy.HasValue && !t.DeletedDate.HasValue)
                    .ToDictionary(t => t.Id, t => new UserMD
                    {
                        Id = t.Id,
                        FirstName = t.FirstName,
                        LastName = t.LastName,
                        Email = t.Email,
                        DoB = t.DoB,
                        PhoneNo = t.PhoneNo,
                        PositionId = t.PositionId,
                        RoleId = t.RoleId,
                        Sex = t.Sex.Value,
                        Status = t.Status,
                        DeletedBy = t.DeletedBy,
                        DeletedDate = t.DeletedDate
                    });
                return result;
            });
        }

        private List<ProjectMD> _userProject_OnNeedValueIfKeyNotFound(object sender, params object[] paramArrs)
        {
            return UseDbContext(dbContext =>
            {
                var userId = (int)paramArrs.First();

                var result = dbContext.GetRepository<ProjectUser>()
                    .Query(x => x.UserId == userId)
                    .Where(t => !t.DeletedBy.HasValue && !t.DeletedDate.HasValue)
                    .Select(t => new ProjectMD
                    {
                        Id = t.ProjectId,
                        ProjectRoleId = t.ProjectRoleId
                    })
                    .ToList();

                return result.ToList();
            });
        }

        #endregion

        #region Project
        private Dictionary<int, ProjectMD> Projects_OnNeedResource(object sender, params object[] paramArrs)
        {
            return UseDbContext(dbContext =>
            {
                var result = dbContext.GetRepository<Project>()
                    .Query()
                    .Where(t => !t.DeletedBy.HasValue && !t.DeletedDate.HasValue)
                    .ToDictionary(t => t.Id, t => new ProjectMD
                    {
                        Id = t.Id,
                        Name = t.Name,
                        Description = t.Description,
                        Status = t.Status,
                        StartDate = t.StartDate,
                        EndDate = t.EndDate,
                        DeletedBy = t.DeletedBy,
                        DeletedDate = t.DeletedDate
                    });
                return result;
            });
        }

        private List<UserMD> _projectUser_OnNeedValueIfKeyNotFound(object sender, params object[] paramArrs)
        {
            return UseDbContext(dbContext =>
            {
                var projectId = (int)paramArrs.First();

                var result = dbContext.GetRepository<ProjectUser>()
                    .Query(x => x.ProjectId == projectId)
                    .Where(x => x.DeletedBy == null && x.DeletedDate == null)
                    .Join(dbContext.GetRepository<User>().Query(x => x.Status && !x.DeletedDate.HasValue), t1 => t1.UserId, t2 => t2.Id, (t1, t2) => new UserMD
                    {
                        ProjectUserId = t1.Id,
                        Id = t2.Id,
                        ProjectRoleId = t1.ProjectRoleId,
                        Email = t2.Email,
                        FirstName = t2.FirstName,
                        LastName = t2.LastName,
                        Status = t2.Status,
                        PositionId = t2.PositionId,
                        RoleId = t2.RoleId,
                        ProjectId = t1.ProjectId
                    })
                    .ToList();

                return result;
            });
        }
        #endregion

        #region Criteria Store

        private Dictionary<Guid, CriteriaTypeStoreMD> CriteriaTypeStores_OnNeedResource(object sender, params object[] paramArrs)
        {
            return UseDbContext(dbContext =>
            {
                var result = dbContext.GetRepository<CriteriaTypeStore>()
                    .Query()
                    .ToDictionary(t => t.Id, t => new CriteriaTypeStoreMD
                    {
                        Id = t.Id,
                        Description = t.Description,
                        Name = t.Name,
                        OrderNo = t.OrderNo,
                        DeletedBy = t.DeletedBy,
                        DeletedDate = t.DeletedDate
                    });
                return result;
            });
        }

        private List<CriteriaStoreMD> _criteriaStores_OnNeedValueIfKeyNotFound(object sender, params object[] paramArrs)
        {
            return UseDbContext(dbContext =>
            {
                var criteriaTypeId = (Guid)paramArrs.First();
                var result = dbContext.GetRepository<CriteriaStore>()
                    .Query(t => t.CriteriaTypeId == criteriaTypeId && t.DeletedBy != null && t.DeletedDate != null)
                    .Select(t => new CriteriaStoreMD
                    {
                        Id = t.Id,
                        Name = t.Name,
                        Description = t.Description,
                        CriteriaTypeId = t.CriteriaTypeId,
                        OrderNo = t.OrderNo,
                        DeletedBy = t.DeletedBy,
                        DeletedDate = t.DeletedDate
                    }).ToList();
                return result;
            });
        }

        private Dictionary<Guid, CriteriaStoreMD> CriteriaStores_OnNeedResource(object sender, params object[] paramArrs)
        {
            return UseDbContext(dbContext =>
            {
                var result = dbContext.GetRepository<CriteriaStore>()
                    .Query()
                    .ToDictionary(t => t.Id, t => new CriteriaStoreMD
                    {
                        Id = t.Id,
                        CriteriaTypeId = t.CriteriaTypeId,
                        Description = t.Description,
                        Name = t.Name,
                        OrderNo = t.OrderNo,
                        DeletedBy = t.DeletedBy,
                        DeletedDate = t.DeletedDate
                    });
                return result;
            });
        }

        #endregion

        #region Others

        private Dictionary<int, PositionMD> Position_OnNeedResource(object sender, params object[] paramArrs)
        {
            return UseDbContext(dbContext =>
            {
                var result = dbContext.GetRepository<Position>()
                    .Query()
                    .ToDictionary(t => t.Id, t => new PositionMD
                    {
                        Id = t.Id,
                        Name = t.Name,
                        DeletedBy = t.DeletedBy,
                        DeletedDate = t.DeletedDate
                    });
                return result;
            });
        }

        private Dictionary<string, AppSettingMD> AppSetting_OnNeedResource(object sender, params object[] paramArrs)
        {
            return UseDbContext(dbContext =>
            {
                var data = dbContext.GetRepository<AppSetting>()
                  .Query()
                  .Select(t => new AppSettingMD
                  {
                      Id = t.Id,
                      Value = t.Value,
                      Status = t.Status
                  })
                  .ToList();

                var result = data
                .GroupBy(t => t.Id)
                .ToDictionary(t => t.Key, t => t.First());

                return result;
            });
        }

        private Dictionary<string, WorkSpaceSettingMD> WorkSpaceSetting_OnNeedResource(object sender, params object[] paramArrs)
        {
            return UseDbContext(dbContext =>
            {
                List<string> ids = new List<string>()
                {
                    WorkSpaceSettingCommon.ISLOCKTIMESHEET,
                    WorkSpaceSettingCommon.LOCKAFTER,
                    WorkSpaceSettingCommon.LOCKVALUEBYDATE
                };
                var data = dbContext.GetRepository<AppSetting>()
                  .Query(x => ids.Contains(x.Id))
                  .Select(t => new WorkSpaceSettingMD
                  {
                      Id = t.Id,
                      Value = t.Value,
                      Status = t.Status
                  })
                  .ToList();

                var result = data
                .GroupBy(t => t.Id)
                .ToDictionary(t => t.Key, t => t.First());

                return result;
            });
        }

        private WorkSpaceSettingMD _workSpaceSetting_OnNeedValueIfKeyNotFound(object sender, params object[] paramArrs)
        {
            var appSettingId = (string)paramArrs.First();
            return UseDbContext(dbContext =>
            {
                var result = dbContext.GetRepository<AppSetting>()
                    .Query(t => t.Id == appSettingId)
                    .Select(t => new WorkSpaceSettingMD
                    {
                        Id = t.Id,
                        Value = t.Value,
                        Status = t.Status
                    }).First();
                return result;
            });
        }

        #endregion

        #region Criteria

        private Dictionary<Guid, QuarterCriteriaTemplateMD> QuarterCriteriaTemplates_OnNeedResource(object sender, params object[] paramArrs)
        {
            return UseDbContext(dbContext =>
            {
                var result = dbContext.GetRepository<QuarterCriteriaTemplate>()
                    .Query()
                    .ToDictionary(t => t.Id, t => new QuarterCriteriaTemplateMD
                    {
                        Id = t.Id,
                        Name = t.Name,
                        Year = t.Year,
                        Quarter = t.Quarter,
                        PositionId = t.PositionId,
                        DeletedBy = t.DeletedBy,
                        DeletedDate = t.DeletedDate
                    });
                return result;
            });
        }

        private List<CriteriaTypeMD> _criteriaTypes_OnNeedValueIfKeyNotFound(object sender, params object[] paramArrs)
        {
            return UseDbContext(dbContext =>
            {
                var quarterCriteriaTemplateId = (Guid)paramArrs.First();
                var result = dbContext.GetRepository<CriteriaType>()
                    .Query(t => t.QuarterCriteriaTemplateId == quarterCriteriaTemplateId && t.DeletedBy != null && t.DeletedDate != null)
                    .Select(t => new CriteriaTypeMD
                    {
                        Id = t.Id,
                        QuarterCriteriaTemplateId = t.QuarterCriteriaTemplateId,
                        Name = t.Name,
                        Description = t.Description,
                        OrderNo = t.OrderNo,
                        DeletedBy = t.DeletedBy,
                        DeletedDate = t.DeletedDate
                    }).ToList();
                return result;
            });
        }

        private Dictionary<Guid, CriteriaTypeMD> CriteriaTypes_OnNeedResource(object sender, params object[] paramArrs)
        {
            return UseDbContext(dbContext =>
            {
                var result = dbContext.GetRepository<CriteriaType>()
                    .Query()
                    .ToDictionary(t => t.Id, t => new CriteriaTypeMD
                    {
                        Id = t.Id,
                        QuarterCriteriaTemplateId = t.QuarterCriteriaTemplateId,
                        Description = t.Description,
                        Name = t.Name,
                        OrderNo = t.OrderNo,
                        DeletedBy = t.DeletedBy,
                        DeletedDate = t.DeletedDate
                    });
                return result;
            });
        }


        private List<CriteriaMD> _criterias_OnNeedValueIfKeyNotFound(object sender, params object[] paramArrs)
        {
            return UseDbContext(dbContext =>
            {
                var criteriaTypeId = (Guid)paramArrs.First();
                var result = dbContext.GetRepository<Criteria>()
                    .Query(t => t.CriteriaTypeId == criteriaTypeId && t.DeletedBy != null && t.DeletedDate != null)
                    .Select(t => new CriteriaMD
                    {
                        Id = t.Id,
                        Name = t.Name,
                        Description = t.Description,
                        CriteriaTypeId = t.CriteriaTypeId,
                        OrderNo = t.OrderNo,
                        DeletedBy = t.DeletedBy,
                        DeletedDate = t.DeletedDate
                    }).ToList();
                return result;
            });
        }
        private Dictionary<Guid, CriteriaMD> Criterias_OnNeedResource(object sender, params object[] paramArrs)
        {
            return UseDbContext(dbContext =>
            {
                var result = dbContext.GetRepository<Criteria>()
                    .Query()
                    .ToDictionary(t => t.Id, t => new CriteriaMD
                    {
                        Id = t.Id,
                        CriteriaTypeId = t.CriteriaTypeId,
                        Description = t.Description,
                        Name = t.Name,
                        OrderNo = t.OrderNo,
                        DeletedBy = t.DeletedBy,
                        DeletedDate = t.DeletedDate
                    });
                return result;
            });
        }


        #endregion

        #region TimeSheet

        public Dictionary<Guid, TSActivityGroupMD> TSActivityGroup_OnNeedResource(object sender, params object[] paramArrs)
        {
            return UseDbContext(dbContext =>
            {
                var result = dbContext.GetRepository<TSActivityGroup>()
                  .Query()
                  .ToDictionary(t => t.Id, t => new TSActivityGroupMD
                  {
                      Id = t.Id,
                      ProjectId = t.ProjectId,
                      Name = t.Name,
                      DeletedBy = t.DeletedBy,
                      DeletedDate = t.DeletedDate,
                      Description = t.Description
                  });
                return result;
            });
        }

        private List<ActivityGroupUserMD> ActivityGroupUser_OnNeedResource(object sender, params object[] paramArrs)
        {
            return UseDbContext(dbContext =>
            {
                var result = dbContext.GetRepository<ActivityGroupUser>()
                  .Query()
                  .Select(t => new ActivityGroupUserMD
                  {
                      Id = t.Id,
                      Role = t.Role,
                      TSAcitivityGroupId = t.TSActivityGroupId,
                      UserId = t.UserId,
                      DeletedBy = t.DeletedBy,
                      DeletedDate = t.DeletedDate
                  })
                  .ToList();
                return result;
            });
        }

        private List<TSActivityMD> Activity_OnNeedResource(object sender, params object[] paramArrs)
        {
            return UseDbContext(dbContext =>
            {
                var tsActivityGroupId = (Guid)paramArrs.First();

                var result = dbContext.GetRepository<TSActivity>()
                  .Query(t => t.TSActivityGroupId == tsActivityGroupId)
                  .Select(t => new TSActivityMD
                  {
                      Id = t.Id,
                      Description = t.Description,
                      Name = t.Name,
                      TSActivityGroupId = t.TSActivityGroupId,
                      DeletedBy = t.DeletedBy,
                      DeletedDate = t.DeletedDate
                  })
                  .ToList();
                return result;
            });
        }

        public Dictionary<Guid, TSActivityMD> TSActivity_OnNeedResource(object sender, params object[] paramArrs)
        {
            return UseDbContext(dbContext =>
            {
                var result = dbContext.GetRepository<TSActivity>()
                  .Query()
                  .ToDictionary(t => t.Id, t => new TSActivityMD
                  {
                      Id = t.Id,
                      Description = t.Description,
                      Name = t.Name,
                      TSActivityGroupId = t.TSActivityGroupId,
                      DeletedBy = t.DeletedBy,
                      DeletedDate = t.DeletedDate
                  });
                return result;
            });
        }

        #endregion

        #region Cache Extension

        public class UserAllCache : DataAllCache<int, UserMD>
        {
            private readonly CacheProvider _provider;
            public UserAllCache(CacheProvider provider)
            {
                _provider = provider;
            }

            public List<ProjectMD> GetProjects(int userId)
            {
                return _provider._userProjects.Get(userId);
            }

            public ProjectMD GetProject(int userId)
            {
                var projects = _provider._userProjects.Get(userId);
                if (projects == null || projects.Count == 0)
                {
                    return null;
                }
                return projects.FirstOrDefault();
            }

            public override bool Remove(int userId)
            {
                base.Remove(userId);
                _provider._userProjects.Remove(userId);
                return true;
            }
            public override void Clear()
            {
                base.Clear();

                _provider._userProjects.Clear();
                _provider._projectUser.Clear();
            }
        }

        public class ProjectAllCache : DataAllCache<int, ProjectMD>
        {
            private readonly CacheProvider _provider;
            public ProjectAllCache(CacheProvider provider)
            {
                _provider = provider;
            }
            public List<UserMD> GetUsers(int projectId)
            {
                return _provider._projectUser.Get(projectId);
            }

            public override bool Remove(int userId)
            {
                base.Remove(userId);
                _provider._projectUser.Remove(userId);
                return true;
            }
            public override void Clear()
            {
                base.Clear();

                _provider._projectUser.Clear();
            }
        }
        public class PositionAllCache : DataAllCache<int, PositionMD>
        {

        }

        public class AppSettingAllCache : DataAllCache<string, AppSettingMD>
        {

        }

        public class WorkSpaceSettingAllCache : DataAllCache<string, WorkSpaceSettingMD>
        {
            private readonly CacheProvider _provider;

            public WorkSpaceSettingAllCache(CacheProvider provider)
            {
                _provider = provider;
            }
            public bool GetIsLockTimeSheet()
            {
                return bool.Parse(_provider._workSpaceSetting.Get(WorkSpaceSettingCommon.ISLOCKTIMESHEET).Value);
            }
            public TimeSpan GetLockAfter()
            {
                return new TimeSpan(long.Parse(_provider._workSpaceSetting.Get(WorkSpaceSettingCommon.LOCKAFTER).Value));
            }

            public int GetLockValueByDate()
            {
                return int.Parse(_provider._workSpaceSetting.Get(WorkSpaceSettingCommon.LOCKVALUEBYDATE).Value);
            }
            public override void Clear()
            {
                base.Clear();
                _provider._workSpaceSetting.Clear();
            }
        }

        public class CriteriaTypeStoreAllCache : DataAllCache<Guid, CriteriaTypeStoreMD>
        {
            private readonly CacheProvider _provider;
            public CriteriaTypeStoreAllCache(CacheProvider provider)
            {
                _provider = provider;
            }

            public List<CriteriaStoreMD> GetCriterias(Guid criteriaTypeId)
            {
                var criterias = _provider._criteriaStores.Get(criteriaTypeId);
                return criterias;
            }

            public override bool Remove(Guid criteriaTypeId)
            {
                base.Remove(criteriaTypeId);
                _provider._criteriaStores.Remove(criteriaTypeId);

                return true;
            }

            public override void Clear()
            {
                base.Clear();
                _provider._criteriaStores.Clear();
            }
        }

        public class CriteriaStoreAllCache : DataAllCache<Guid, CriteriaStoreMD>
        {
        }

        public class QuarterCriteriaTemplateAllCache : DataAllCache<Guid, QuarterCriteriaTemplateMD>
        {
            private readonly CacheProvider _provider;
            public QuarterCriteriaTemplateAllCache(CacheProvider provider)
            {
                _provider = provider;
            }

            public List<CriteriaTypeMD> GetCriteriaTypes(Guid quarterCriteriaTemplateId)
            {
                var criteriaTypes = _provider._criteriaTypes.Get(quarterCriteriaTemplateId);
                return criteriaTypes;
            }

            public override bool Remove(Guid quarterCriteriaTemplateId)
            {
                base.Remove(quarterCriteriaTemplateId);
                _provider._criteriaTypes.Remove(quarterCriteriaTemplateId);

                return true;
            }

            public override void Clear()
            {
                base.Clear();
                _provider._criteriaTypes.Clear();
            }
        }
        public class CriteriaTypeAllCache : DataAllCache<Guid, CriteriaTypeMD>
        {
            private readonly CacheProvider _provider;
            public CriteriaTypeAllCache(CacheProvider provider)
            {
                _provider = provider;
            }

            public List<CriteriaMD> GetCriterias(Guid criteriaTypeId)
            {
                var criterias = _provider._criterias.Get(criteriaTypeId);
                return criterias;
            }

            public override bool Remove(Guid criteriaTypeId)
            {
                base.Remove(criteriaTypeId);
                _provider._criterias.Remove(criteriaTypeId);

                return true;
            }

            public override void Clear()
            {
                base.Clear();
                _provider._criterias.Clear();
            }
        }

        public class CriteriaAllCache : DataAllCache<Guid, CriteriaMD>
        {

        }

        public class TSActivityGroupAllCache : DataAllCache<Guid, TSActivityGroupMD>
        {
            private readonly CacheProvider _provider;

            public TSActivityGroupAllCache(CacheProvider provider)
            {
                _provider = provider;
            }

            public List<ActivityGroupUserMD> GetActivityGroupUsers(Guid activityGroupId)
            {
                var activityGroupUsers = _provider._activityGroupUser.Get(activityGroupId);
                return activityGroupUsers;
            }

            public List<TSActivityMD> GetActivity(Guid activityGroupId)
            {
                var activities = _provider._activity.Get(activityGroupId);
                return activities;
            }

            public override bool Remove(Guid id)
            {
                base.Remove(id);
                _provider._activityGroupUser.Remove(id);
                _provider._activity.Remove(id);
                return true;
            }
            public override void Clear()
            {
                base.Clear();
                _provider._activityGroupUser.Clear();
                _provider._activity.Clear();
            }
        }

        public class TSActivityAllCache : DataAllCache<Guid, TSActivityMD>
        {
            private readonly CacheProvider _provider;

            public TSActivityAllCache(CacheProvider provider)
            {
                _provider = provider;
            }

            public override bool Remove(Guid id)
            {
                base.Remove(id);
                return true;
            }
            public override void Clear()
            {
                base.Clear();
            }
        }

        #endregion

        #endregion
    }
}

