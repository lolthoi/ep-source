using Kloon.EmployeePerformance.DataAccess;
using Kloon.EmployeePerformance.DataAccess.Domain;
using Kloon.EmployeePerformance.Logic.Caches.Data;
using Kloon.EmployeePerformance.Logic.Common;
using Kloon.EmployeePerformance.Logic.Services.Base;
using Kloon.EmployeePerformance.Models.Common;
using Kloon.EmployeePerformance.Models.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Kloon.EmployeePerformance.Logic.Services
{
    public interface IUserService
    {
        ResultModel<List<UserModel>> GetAll(string searchText = "");
        ResultModel<UserModel> GetById(int userId);
        ResultModel<UserModel> Create(UserModel userModel);
        ResultModel<UserModel> Update(UserModel userModel);
        ResultModel<bool> Delete(int userId);
        ResultModel<ResetPassword> ChangedPassword(ResetPassword resetPassword);
        ResultModel<List<UserModel>> GetUsersForTimeSheetReport(List<int> projectIds);
    }
    public class UserService : IUserService
    {
        private readonly IAuthenLogicService<UserService> _logicService;
        private readonly IUnitOfWork<EmployeePerformanceContext> _dbContext;
        private readonly IEntityRepository<User> _users;
        private readonly IEntityRepository<ProjectUser> _projectUsers;
        private readonly IEntityRepository<TSActivityGroup> _activityGroups;
        private readonly IEntityRepository<ActivityGroupUser> _activityGroupUsers;
        public UserService(IAuthenLogicService<UserService> logicService)
        {
            _logicService = logicService;
            _dbContext = logicService.DbContext;

            _projectUsers = _dbContext.GetRepository<ProjectUser>();
            _users = _dbContext.GetRepository<User>();

            _activityGroups = _dbContext.GetRepository<TSActivityGroup>();
            _activityGroupUsers = _dbContext.GetRepository<ActivityGroupUser>();
        }

        public ResultModel<UserModel> Create(UserModel userModel)
        {
            var now = DateTime.UtcNow;
            var result = _logicService
                .Start()
                .ThenAuthorize(Roles.ADMINISTRATOR)
                .ThenValidate(current =>
                {
                    var error = ValidateUser(userModel);
                    if (error != null)
                    {
                        return error;
                    };
                    var registedEmail = _logicService.Cache.Users.GetValues().Any(x => x.Email.ToLower().Trim().Equals(userModel.Email.ToLower().Trim()));
                    if (registedEmail)
                    {
                        return new ErrorModel(ErrorType.DUPLICATED, "INVALID_MODEL_DUPLICATED_EMAIL");
                    }
                    return null;
                })
               .ThenImplement(current =>
               {
                   var salt = Guid.NewGuid().ToString();

                   var user = new User()
                   {
                       Email = userModel.Email.ToLower().Trim(),
                       FirstName = userModel.FirstName.Trim(),
                       LastName = userModel.LastName.Trim(),
                       PositionId = userModel.PositionId,
                       Sex = (int)userModel.Sex,
                       DoB = userModel.DoB,
                       PhoneNo = userModel.PhoneNo,
                       RoleId = userModel.RoleId,
                       CreatedBy = current.Id,
                       CreatedDate = now,
                       PasswordHash = Utils.EncryptedPassword("123456", salt),
                       PasswordSalt = salt,
                       Status = userModel.Status
                   };
                   _users.Add(user);
                   int result = _dbContext.Save();

                   userModel.Id = user.Id;
                   _logicService.Cache.Users.Clear();

                   return userModel;
               });
            return result;
        }

        public ResultModel<bool> Delete(int userId)
        {
            var now = DateTime.Now;
            User user = null;
            var result = _logicService
                .Start()
                .ThenAuthorize(Roles.ADMINISTRATOR)
                .ThenValidate(currentUser =>
                {
                    if (currentUser.Id == userId)
                    {
                        return new ErrorModel(ErrorType.CONFLICTED, "Cannot delete yourself");
                    }
                    user = _users
                        .Query(x => x.Id == userId && x.DeletedBy == null && x.DeletedDate == null)
                        .FirstOrDefault();
                    if (user == null)
                    {
                        return new ErrorModel(ErrorType.NOT_EXIST, "User not found");
                    }
                    return null;
                })
                .ThenImplement(currentUser =>
                {
                    List<ProjectUser> projectUsers = _projectUsers.Query(x => x.UserId == userId && x.DeletedBy == null && x.DeletedDate == null)
                        .ToList();

                    foreach (var item in projectUsers)
                    {
                        item.DeletedBy = currentUser.Id;
                        item.DeletedDate = now;
                    }

                    user.DeletedBy = currentUser.Id;
                    user.DeletedDate = now;

                    int result = _dbContext.Save();

                    _logicService.Cache.Users.Clear();
                    return true;
                });
            return result;
        }

        public ResultModel<List<UserModel>> GetAll(string searchText)
        {
            var result = _logicService
                .Start()
                .ThenAuthorize(Roles.ADMINISTRATOR, Roles.USER)
                .ThenValidate(current => null)
                .ThenImplement(current =>
                {

                    var query = _logicService.Cache.Users.GetValues().AsEnumerable();
                    if (!string.IsNullOrWhiteSpace(searchText))
                    {
                        query = query.Where(t => t.LastName.Contains(searchText, StringComparison.OrdinalIgnoreCase)
                                                || t.FirstName.Contains(searchText, StringComparison.OrdinalIgnoreCase)
                                                || t.Email.Contains(searchText, StringComparison.OrdinalIgnoreCase));
                    }

                    var record = query.OrderBy(x => x.FirstName)
                                      .Select(t => new UserModel
                                      {
                                          Id = t.Id,
                                          Email = t.Email,
                                          FirstName = t.FirstName,
                                          LastName = t.LastName,
                                          PositionId = t.PositionId,
                                          Sex = (SexEnum)t.Sex,
                                          DoB = t.DoB,
                                          PhoneNo = t.PhoneNo,
                                          RoleId = t.RoleId,
                                          Status = t.Status
                                      }).ToList();
                    record = current.Role == Roles.USER ? record.Where(t => t.RoleId != (int)Roles.ADMINISTRATOR && t.Id == current.Id).ToList() : record;

                    return record;

                });
            return result;
        }

        public ResultModel<UserModel> GetById(int userId)
        {
            UserMD userMD = null;
            var result = _logicService
                        .Start()
                        .ThenAuthorize(Roles.ADMINISTRATOR, Roles.USER)
                        .ThenValidate(current =>
                        {
                            userMD = _logicService.Cache.Users.Get(userId);
                            if (userMD == null)
                                return new ErrorModel(ErrorType.NOT_EXIST, "User not found");
                            return null;
                        })
                        .ThenImplement(current =>
                        {
                            var user = new UserModel()
                            {
                                Id = userMD.Id,
                                Email = userMD.Email,
                                FirstName = userMD.FirstName,
                                LastName = userMD.LastName,
                                PositionId = userMD.PositionId,
                                Sex = (SexEnum)userMD.Sex,
                                DoB = userMD.DoB,
                                PhoneNo = userMD.PhoneNo,
                                RoleId = userMD.RoleId,
                                Status = userMD.Status
                            };
                            return user;
                        });
            return result;
        }

        public ResultModel<UserModel> Update(UserModel userModel)
        {
            var now = DateTime.UtcNow;
            User user = null;
            var result = _logicService
                .Start()
                .ThenAuthorize(Roles.ADMINISTRATOR)
                .ThenValidate(current =>
                {
                    var error = ValidateUser(userModel);
                    if (error != null)
                    {
                        return error;
                    }
                    var registedEmail = _logicService.Cache.Users.GetValues().Any(x => x.Id != userModel.Id && x.Email.ToLower().Trim().Equals(userModel.Email.ToLower().Trim()));
                    if (registedEmail)
                    {
                        return new ErrorModel(ErrorType.DUPLICATED, "INVALID_MODEL_DUPLICATED_EMAIL");
                    }
                    user = _users.Query(x => x.Id == userModel.Id).FirstOrDefault();
                    if (user == null)
                    {
                        return new ErrorModel(ErrorType.NOT_EXIST, "User with Id = " + userModel.Id + " not found");
                    }
                    if (user.DeletedBy != null && user.DeletedDate != null)
                    {
                        return new ErrorModel(ErrorType.NOT_EXIST, "User with Id = " + userModel.Id + " has been deleted");
                    }
                    var activeLeft = _users.Query(x => !x.DeletedDate.HasValue && x.Status && x.RoleId == (int)Roles.ADMINISTRATOR).Count();
                    if (activeLeft < 2 && user.Status && !userModel.Status && user.RoleId == (int)Roles.ADMINISTRATOR)
                    {
                        return new ErrorModel(ErrorType.BAD_REQUEST, "DEACTIVED_LAST_ADMIN");
                    }
                    return null;
                })
                .ThenImplement(current =>
                {
                    user.Email = userModel.Email.ToLower().Trim();
                    user.FirstName = userModel.FirstName.Trim();
                    user.LastName = userModel.LastName.Trim();
                    user.PositionId = userModel.PositionId;
                    user.Sex = (int)userModel.Sex;
                    user.DoB = userModel.DoB;
                    user.PhoneNo = userModel.PhoneNo;
                    user.RoleId = userModel.RoleId;
                    user.ModifiedBy = current.Id;
                    user.ModifiedDate = now;
                    user.Status = userModel.Status;
                    int result = _dbContext.Save();
                    _logicService.Cache.Users.Clear();
                    return userModel;
                });
            return result;
        }
        public ResultModel<ResetPassword> ChangedPassword(ResetPassword resetPassword)
        {
            User user = null;
            var result = _logicService
                .Start()
                .ThenAuthorize(Roles.ADMINISTRATOR, Roles.USER)
                .ThenValidate(currentUser =>
                {
                    if (resetPassword == null)
                    {
                        return new ErrorModel(ErrorType.BAD_REQUEST, "Please fill in the required fields");
                    }

                    if (string.IsNullOrEmpty(resetPassword.Password))
                    {
                        return new ErrorModel(ErrorType.BAD_REQUEST, "The Old Password field is required.");
                    }

                    if (resetPassword.Password.Length < 6)
                    {
                        return new ErrorModel(ErrorType.BAD_REQUEST, "The Old Password field must be at least 6 characters");
                    }
                    if (resetPassword.Password.Length > 50)
                    {
                        return new ErrorModel(ErrorType.BAD_REQUEST, "The Old Password field must be between 6 and 50 characters");
                    }

                    user = _users.Query(x => x.Id == currentUser.Id).FirstOrDefault();

                    if (user == null)
                    {
                        return new ErrorModel(ErrorType.NOT_EXIST, "Your account could not be found");
                    }

                    var encryptedPassword = Utils.EncryptedPassword(resetPassword.Password, user.PasswordSalt);
                    if (user.PasswordHash != encryptedPassword)
                    {
                        return new ErrorModel(ErrorType.NOT_EXIST, "The old password is incorrect");
                    }

                    if (string.IsNullOrEmpty(resetPassword.NewPassword))
                    {
                        return new ErrorModel(ErrorType.BAD_REQUEST, "The New Password field is required.");
                    }

                    if (resetPassword.NewPassword.Length < 6)
                    {
                        return new ErrorModel(ErrorType.BAD_REQUEST, "The New Password field must be at least 6 characters");
                    }
                    if (resetPassword.NewPassword.Length > 50)
                    {
                        return new ErrorModel(ErrorType.BAD_REQUEST, "The New Password field must be between 6 and 50 characters");
                    }

                    if (string.IsNullOrEmpty(resetPassword.ConfirmPassword))
                    {
                        return new ErrorModel(ErrorType.BAD_REQUEST, "The Confirm Password field is required.");
                    }

                    if (resetPassword.ConfirmPassword != resetPassword.NewPassword)
                    {
                        return new ErrorModel(ErrorType.BAD_REQUEST, "The New Password and Confirm Password do not match");
                    }
                    return null;
                })
                .ThenImplement(currentUser =>
                {
                    var salt = Guid.NewGuid().ToString();
                    user.PasswordHash = Utils.EncryptedPassword(resetPassword.NewPassword, salt);
                    user.PasswordResetCode = null;
                    user.PasswordSalt = salt;
                    user.ModifiedBy = user.Id;
                    user.ModifiedDate = DateTime.Now;
                    _dbContext.Save();
                    return new ResetPassword()
                    {
                        Status = true
                    };
                });
            return result;
        }

        public ResultModel<List<UserModel>> GetUsersForTimeSheetReport(List<int> projectIds)
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
                    List<UserModel> listResult = new();
                    var allUser = _logicService.Cache.Users.GetValues().Where(x => !x.DeletedDate.HasValue).ToList();
                    //Check parmas
                    if (projectIds != null && projectIds.Count > 0)
                    {
                        List<UserMD> listUser = new();
                        foreach (var item in projectIds)
                        {
                            var users = _logicService.Cache.Projects.GetUsers(item).ToList();
                            listUser.AddRange(users);
                        }
                        var listUserId = listUser.Select(x => x.Id).Distinct().ToList();
                        allUser = allUser.Where(x => listUserId.Contains(x.Id)).ToList();
                    }

                    //Check Role
                    if (currentUser.Role == Roles.USER)
                    {
                        var tsActityGroupIds = _activityGroupUsers.Query(x => x.UserId == currentUser.Id && x.DeletedDate == null && x.DeletedBy == null).Select(t => t.TSActivityGroupId).ToList();
                        if (tsActityGroupIds.Count > 0)
                        {
                            var realProject = _activityGroups.Query(x => tsActityGroupIds.Contains(x.Id) && x.DeletedDate == null && x.DeletedBy == null && x.ProjectId != null)
                                .Select(t => t.ProjectId).ToList();

                            if (realProject.Count > 0)
                            {
                                var userIds = _projectUsers.Query(x => realProject.Contains(x.ProjectId) && x.DeletedBy == null && x.DeletedDate == null).Select(x => x.UserId).ToList();
                                if (userIds.Count > 0)
                                {
                                    listResult = allUser.Where(x => userIds.Contains(x.Id)).OrderBy(x => x.FirstName)
                                       .Select(t => new UserModel
                                       {
                                           Id = t.Id,
                                           Email = t.Email,
                                           FirstName = t.FirstName,
                                           LastName = t.LastName,
                                           PositionId = t.PositionId,
                                           Sex = (SexEnum)t.Sex,
                                           DoB = t.DoB,
                                           PhoneNo = t.PhoneNo,
                                           RoleId = t.RoleId,
                                           Status = t.Status
                                       }).ToList();
                                }
                            }
                            else
                            {
                                listResult = allUser.OrderBy(x => x.FirstName)
                                   .Select(t => new UserModel
                                   {
                                       Id = t.Id,
                                       Email = t.Email,
                                       FirstName = t.FirstName,
                                       LastName = t.LastName,
                                       PositionId = t.PositionId,
                                       Sex = (SexEnum)t.Sex,
                                       DoB = t.DoB,
                                       PhoneNo = t.PhoneNo,
                                       RoleId = t.RoleId,
                                       Status = t.Status
                                   }).ToList();
                            }
                        }
                    }
                    else
                    {
                        listResult = allUser.OrderBy(x => x.FirstName)
                           .Select(t => new UserModel
                           {
                               Id = t.Id,
                               Email = t.Email,
                               FirstName = t.FirstName,
                               LastName = t.LastName,
                               PositionId = t.PositionId,
                               Sex = (SexEnum)t.Sex,
                               DoB = t.DoB,
                               PhoneNo = t.PhoneNo,
                               RoleId = t.RoleId,
                               Status = t.Status
                           }).ToList();
                    }
                    return listResult;
                });
            return result;
        }
        private ErrorModel ValidateUser(UserModel userModel)
        {
            #region FirstName
            if (string.IsNullOrEmpty(userModel.FirstName))
            {
                return new ErrorModel(ErrorType.BAD_REQUEST, "INVALID_MODEL_FIRST_NAME_NULL");
            }
            if (userModel.FirstName.Length > 50)
            {
                return new ErrorModel(ErrorType.BAD_REQUEST, "INVALID_MODEL_FIRST_NAME_MAX_LENGTH");
            }
            #endregion 

            #region LastName
            if (string.IsNullOrEmpty(userModel.LastName))
            {
                return new ErrorModel(ErrorType.BAD_REQUEST, "INVALID_MODEL_LAST_NAME_NULL");
            }
            if (userModel.LastName.Length > 50)
            {
                return new ErrorModel(ErrorType.BAD_REQUEST, "INVALID_MODEL_LAST_NAME_MAX_LENGTH");
            }
            #endregion

            #region Email
            if (string.IsNullOrEmpty(userModel.Email))
            {
                return new ErrorModel(ErrorType.BAD_REQUEST, "INVALID_MODEL_EMAIL_NULL");
            }
            if (userModel.Email.Length > 75)
            {
                return new ErrorModel(ErrorType.BAD_REQUEST, "INVALID_MODEL_EMAIL_MAX_LENGTH");
            }

            string validEmail = "^\\s*[A-Za-z0-9-.\\+]+(\\.[_A-Za-z0-9-]+)*@kloon.vn\\s*$";
            if (Regex.IsMatch(userModel.Email, validEmail, RegexOptions.IgnoreCase) == false)
            {
                return new ErrorModel(ErrorType.BAD_REQUEST, "INVALID_MODEL_EMAIL_FORMAT_WRONG");
            }

            #endregion

            #region Phone
            if (userModel.PhoneNo != null && userModel.PhoneNo.Length > 10)
            {
                return new ErrorModel(ErrorType.BAD_REQUEST, "INVALID_MODEL_PHONE_MAX_LENGTH");
            }
            #endregion

            #region Selectable Attribute

            var position = _logicService.Cache.Position.GetValues();

            if (!position.Any(x => x.Id == userModel.PositionId))
            {
                return new ErrorModel(ErrorType.BAD_REQUEST, "INVALID_MODEL_POSITION_NULL");
            }
            if (!Enum.IsDefined(typeof(SexEnum), userModel.Sex))
            {
                return new ErrorModel(ErrorType.BAD_REQUEST, "INVALID_MODEL_SEX_NULL");
            }
            if (!Enum.IsDefined(typeof(Roles), userModel.RoleId))
            {
                return new ErrorModel(ErrorType.BAD_REQUEST, "INVALID_MODEL_ROLE_NULL");
            }
            #endregion

            return null;
        }
    }
}

