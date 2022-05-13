using Kloon.EmployeePerformance.DataAccess;
using Kloon.EmployeePerformance.DataAccess.Domain;
using Kloon.EmployeePerformance.Logic.Common;
using Kloon.EmployeePerformance.Logic.Services.Base;
using Kloon.EmployeePerformance.Models.Authentication;
using Kloon.EmployeePerformance.Models.Common;
using Kloon.EmployeePerformance.Models.User;
using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace Kloon.EmployeePerformance.Logic.Services
{
    public interface IAuthenticationService
    {
        ResultModel<UserAuthModel> Login(string email, string password);

        ResultModel<ForgotPasswordModel> ForgotPassword(ForgotPasswordModel model);
        ResultModel<bool> CheckCodeForResetPassword(string code);
        ResultModel<ResetPassword> ResetPasswordWithCode(ResetPassword resetPassword);

        ResultModel<UserAuthModel> ExternalLoginGoogle(string email);
        
    }
    public class AuthenticationService : IAuthenticationService
    {
        private readonly IUnitOfWork<EmployeePerformanceContext> _dbContext;
        private readonly ILogicService<AuthenticationService> _logicService;
        private readonly IEntityRepository<User> _users;


        public AuthenticationService(ILogicService<AuthenticationService> logicService)
        {
            _logicService = logicService;
            _dbContext = logicService.DbContext;
            _users = _dbContext.GetRepository<User>();
        }

        public ResultModel<UserAuthModel> Login(string email, string password)
        {
            UserAuthModel user = null;

            var result = _logicService
                .Start()
                .ThenValidate(() =>
                {
                    //TODO: CHECK VALID EMAIL

                    var data = _users
                        .Query(t => t.Email.Equals(email.Trim().ToLower()) && !t.DeletedBy.HasValue && !t.DeletedDate.HasValue && t.Status != false)
                        .Select(t => new
                        {
                            User = new UserAuthModel
                            {
                                Id = t.Id,
                                Email = t.Email.Trim().ToLower(),
                                FirstName = t.FirstName,
                                LastName = t.LastName,
                                PositionId = t.PositionId,
                                PasswordHash = t.PasswordHash,
                                PasswordSalt = t.PasswordSalt,
                                Role = (Roles)t.RoleId
                            }
                        })
                        .FirstOrDefault();

                    if (data == null)
                    {
                        return new ErrorModel(ErrorType.NOT_EXIST, "Your account could not be found");
                    }

                    var encryptedPassword = Utils.EncryptedPassword(password, data.User.PasswordSalt);
                    if (data.User.PasswordHash != encryptedPassword)
                    {
                        return new ErrorModel(ErrorType.NOT_EXIST, "The user name or password is incorrect");
                    }

                    user = data.User;
                    return null;
                })
                .ThenImplement(() =>
                {
                    return user;
                });

            return result;
        }

        public ResultModel<ForgotPasswordModel> ForgotPassword(ForgotPasswordModel model)
        {
            User user = null;
            DateTime currentNow = DateTime.Now;
            var result = _logicService
                .Start()
                .ThenValidate(() =>
                {
                    if (model == null)
                    {
                        return new ErrorModel(ErrorType.BAD_REQUEST, "PLease fill the email field");
                    }
                    if (string.IsNullOrEmpty(model.Email))
                    {
                        return new ErrorModel(ErrorType.BAD_REQUEST, "Email is required");
                    }
                    if (model.Email.Length > 75)
                    {
                        return new ErrorModel(ErrorType.BAD_REQUEST, "Max length of email is 75");
                    }

                    string validEmail = "^\\s*[A-Za-z0-9-.\\+]+(\\.[_A-Za-z0-9-]+)*@kloon.vn\\s*$";
                    if (Regex.IsMatch(model.Email, validEmail, RegexOptions.IgnoreCase) == false)
                    {
                        return new ErrorModel(ErrorType.BAD_REQUEST, "Email format wrong");
                    }

                    user = _users
                        .Query(x => x.DeletedBy == null &&
                                x.DeletedDate == null &&
                                x.Email == model.Email &&
                                x.Status == true
                                )
                        .FirstOrDefault();
                    if (user == null)
                    {
                        return new ErrorModel(ErrorType.NOT_EXIST, "The entered email does not exist");
                    }
 
                    if (user.ModifiedDate != null && user.ModifiedBy == user.Id && !string.IsNullOrWhiteSpace(user.PasswordResetCode))
                    {
                        var expireTime = user.ModifiedDate.Value.AddMinutes(10);
                        int data = DateTime.Compare(expireTime, currentNow);
                        if (data > 0)
                        {
                            return new ErrorModel(ErrorType.BAD_REQUEST, "Sending an email for a forgotten password can take more than 10 minutes.");
                        }
                    }
                    return null;
                })
                .ThenImplement(() =>
                {
                    string codeResetPassword = user.Id + "_Guid-" + Guid.NewGuid().ToString();
                    user.PasswordResetCode = codeResetPassword;
                    user.ModifiedDate = currentNow;
                    user.ModifiedBy = user.Id;
                    _users.Edit(user);
                    _dbContext.Save();

                    ForgotPasswordModel forgotPasswordModel = new ForgotPasswordModel()
                    {
                        Code = codeResetPassword,
                        Email = user.Email,
                        Url = model.Url,
                        IsSuccess = true
                    };
                    return forgotPasswordModel;
                });
            return result;
        }

        public ResultModel<bool> CheckCodeForResetPassword(string code)
        {
            var result = _logicService
              .Start()
              .ThenValidate(() =>
              {
                  return null;
              })
              .ThenImplement(() =>
              {
                  if (string.IsNullOrEmpty(code))
                  {
                      return false;
                  }
                  string userId = code.Split('_')[0];
                  if (int.TryParse(userId, out int id))
                  {
                      var user = _users.Query(x => x.Id == id).FirstOrDefault();
                      if (user == null)
                      {
                          return false;
                      }
                      if (user.PasswordResetCode == null || user.PasswordResetCode != code || user.ModifiedDate == null)
                      {
                          return false;
                      }

                      var current = DateTime.UtcNow;
                      var expiredAdd = user.ModifiedDate.Value.AddMinutes(10);
                      int data = DateTime.Compare(expiredAdd, current);
                      if (data > 0)
                      {
                          return true;
                      }
                      return false;
                  }
                  else
                  {
                      return false;
                  }
              });
            return result;
        }

        public ResultModel<ResetPassword> ResetPasswordWithCode(ResetPassword resetPassword)
        {
            User user = null;
            var result = _logicService
                .Start().
                ThenValidate(() =>
                {
                    if (resetPassword ==  null)
                    {
                        return new ErrorModel(ErrorType.BAD_REQUEST, "Please fill in the required fields");
                    }
                    if (string.IsNullOrEmpty(resetPassword.Code))
                    {
                        return new ErrorModel(ErrorType.BAD_REQUEST, "Code is Invalid");
                    }
                    if (string.IsNullOrEmpty(resetPassword.NewPassword))
                    {
                        return new ErrorModel(ErrorType.BAD_REQUEST, "Password is required.");
                    }

                    if (resetPassword.NewPassword.Length < 6)
                    {
                        return new ErrorModel(ErrorType.BAD_REQUEST, "Min length of Password is 6");
                    }
                    if (resetPassword.NewPassword.Length > 100)
                    {
                        return new ErrorModel(ErrorType.BAD_REQUEST, "Max length of Password is 100");
                    }

                    if (string.IsNullOrEmpty(resetPassword.ConfirmPassword))
                    {
                        return new ErrorModel(ErrorType.BAD_REQUEST, "Confirm Password is required");
                    }

                    if (resetPassword.ConfirmPassword != resetPassword.NewPassword)
                    {
                        return new ErrorModel(ErrorType.BAD_REQUEST, "The Password and Confirmation password do not match");
                    }
                    string userId = resetPassword.Code.Split('_')[0];

                    if (int.TryParse(userId, out int id))
                    {
                        user = _users.Query(x => x.Id == id && x.DeletedBy == null && x.DeletedDate == null).FirstOrDefault();
                        if (user == null)
                        {
                            return new ErrorModel(ErrorType.NOT_EXIST, "Can not identity User");
                        }

                        if (!user.Status)
                        {
                            return new ErrorModel(ErrorType.CONFLICTED, "Can not identity User");
                        }
                        if (user.PasswordResetCode == null)
                        {
                            return new ErrorModel(ErrorType.NOT_EXIST, "Can not identity User");
                        }

                        if (user.PasswordResetCode != resetPassword.Code)
                        {
                            return new ErrorModel(ErrorType.NOT_EXIST, "Can not identity User");
                        }

                        var current = DateTime.UtcNow;
                        var expiredAdd = user.ModifiedDate.Value.AddMinutes(10);
                        int data = DateTime.Compare(current, expiredAdd);
                        if (data > 0)
                        {
                            return new ErrorModel(ErrorType.NOT_EXIST, "Can not identity User");
                        }
                    }
                    else
                    {
                        return new ErrorModel(ErrorType.BAD_REQUEST, "The reset code is not valid");
                    }
                    return null;
                })
                .ThenImplement(() =>
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

        public ResultModel<UserAuthModel> ExternalLoginGoogle(string email)
        {
            UserAuthModel user = null;

            var result = _logicService
                .Start()
                .ThenValidate(() =>
                {
                    //CHECK VALID GOOGLE TOKENID


                    //TODO: CHECK VALID EMAIL
                    var data = _users
                        .Query(t => t.Email.Equals(email.Trim().ToLower()) && !t.DeletedBy.HasValue && !t.DeletedDate.HasValue && t.Status != false)
                        .Select(t => new
                        {
                            User = new UserAuthModel
                            {
                                Id = t.Id,
                                Email = t.Email.Trim().ToLower(),
                                FirstName = t.FirstName,
                                LastName = t.LastName,
                                PositionId = t.PositionId,
                                PasswordHash = t.PasswordHash,
                                PasswordSalt = t.PasswordSalt,
                                Role = (Roles)t.RoleId
                            }
                        })
                        .FirstOrDefault();

                    if (data == null)
                    {
                        return new ErrorModel(ErrorType.NOT_EXIST, "YOUR GOOGLE ACCOUNT CANT NOT BE FOUND IN SYSTEM. PLEASE CONTACT COMPANY ADMINISTRATOR!");
                    }

                    user = data.User;
                    return null;
                })
                .ThenImplement(() =>
                {
                    return user;
                });

            return result;
        }
    }
}
