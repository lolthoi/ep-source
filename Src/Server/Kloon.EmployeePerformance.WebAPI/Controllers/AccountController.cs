using Google.Apis.Auth;
using Kloon.EmployeePerformance.Logic.Caches;
using Kloon.EmployeePerformance.Logic.Services;
using Kloon.EmployeePerformance.Models.Authentication;
using Kloon.EmployeePerformance.Models.Common;
using Kloon.EmployeePerformance.Models.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace Kloon.EmployeePerformance.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IAuthenticationService _authenticationService;
        private readonly CacheProvider _cache;
        private readonly IMailService _mailService;

        public AccountController(IConfiguration configuration, IAuthenticationService authenticationService, CacheProvider cacheProvider, IMailService mailService)
        {
            _authenticationService = authenticationService;
            _configuration = configuration;
            _cache = cacheProvider;
            _mailService = mailService;
        }


        /// <summary>
        /// Login with an account
        /// </summary>
        /// <remarks>
        /// Return the status whether login is successed of failed
        /// </remarks>
        /// <param name="login">Information of the account</param>
        /// <returns>Return the status whether login is successed of failed</returns>
        /// <response code="200">Successful operation</response>
        /// <response code="204">Successful operation, no content is returned</response>
        /// <response code="400">Invalid Id supplied</response>
        /// <response code="401">User is unauthorized</response>
        /// <response code="403">User is not authenticated</response>
        /// <response code="404">Request is inaccessible</response>
        /// <response code="409">Data is conflicted</response>
        /// <response code="500">Server side error</response>
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginModel login)
        {
            if (!String.IsNullOrEmpty(login.Email))
            {
                login.Email = login.Email.Trim().ToLower();
            }

            var result = _authenticationService.Login(login.Email, login.Password);
            if (result.Error != null)
            {
                throw new LogicException(new ErrorModel(ErrorType.BAD_REQUEST, result.Error.Message));
            }

            var user = result;

            var roles =(int) user.Data.Role;
            var claims = new List<Claim>();
            claims.Add(new Claim(ClaimTypes.Sid, user.Data.Id.ToString()));
            claims.Add(new Claim(ClaimTypes.Email, login.Email));
            claims.Add(new Claim(ClaimTypes.Role, roles.ToString()));

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSecurityKey"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expiry = DateTime.Now.AddDays(Convert.ToInt32(_configuration["JwtExpiryInDays"]));

            var token = new JwtSecurityToken(
                _configuration["JwtIssuer"],
                _configuration["JwtAudience"],
                claims,
                expires: expiry,
                signingCredentials: creds
            );

            var projectRole = _cache.Users.GetProjects(user.Data.Id);

            var results = new LoginResult
            {
                Id = user.Data.Id,
                IsSuccessful = true,
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                FirstName = user.Data.FirstName,
                LastName = user.Data.LastName,
                AppRole = user.Data.Role,
                Email = user.Data.Email,
                ProjectRoles = projectRole?.Select(t => new ProjectRoleModel
                { 
                    ProjectId = t.Id,
                    ProjectRoleId = (ProjectRoles)t.ProjectRoleId
                }).ToList()
            };
            return Ok(results);
        }

        /// <summary>
        /// Forgot Password with an email exits
        /// </summary>
        /// <remarks>
        /// Return the status whether login is successed of failed
        /// </remarks>
        /// <param name="forgotPasswordModel">Information of the forgotPassword</param>
        /// <returns>Return the status whether Forgot Password is successed of failed</returns>
        /// <response code="200">Successful operation</response>
        /// <response code="204">Successful operation, no content is returned</response>
        /// <response code="400">Invalid Id supplied</response>
        /// <response code="401">User is unauthorized</response>
        /// <response code="403">User is not authenticated</response>
        /// <response code="404">Request is inaccessible</response>
        /// <response code="409">Data is conflicted</response>
        /// <response code="500">Server side error</response>
        [HttpPost("ForgotPassword")]
        [AllowAnonymous]
        public async Task<ForgotPasswordModel> ForgotPassword(ForgotPasswordModel forgotPasswordModel)
        {
            var forgotPassword = _authenticationService.ForgotPassword(forgotPasswordModel);
            if (forgotPassword.Error != null)
            {
                throw new LogicException(new ErrorModel(forgotPassword.Error.Type, forgotPassword.Error.Message));
            }

            var code = forgotPassword.Data.Code;
            var callBack = string.Format("{0}/reset-password?code={1}", forgotPassword.Data.Url, code);

            var mailRequest = new MailRequest()
            {
                ToEmails = new List<string> { forgotPassword.Data.Email },
                Subject = "Reset Password",
                Body = $"Please reset your password <a href='{HtmlEncoder.Default.Encode(callBack)}'> click Here.</a>."
            };

            await _mailService.SendMailSMTP(mailRequest);

            return forgotPassword.ToResponse();
        }

        /// <summary>
        /// Check code exits for reset password
        /// </summary>
        /// <remarks>
        /// Return the status code is successed of failed
        /// </remarks>
        /// <param name="code">Information of Code</param>
        /// <returns>Return the status whether code is successed of failed</returns>
        /// <response code="200">Successful operation</response>
        /// <response code="400">Invalid Id supplied</response>
        /// <response code="404">Request is inaccessible</response>
        /// <response code="409">Code not found</response>
        /// <response code="500">Server side error</response>
        [HttpGet("CodeForResetPassword")]
        [AllowAnonymous]
        public async Task<bool> CheckCodeForResetPassword(string code)
        {
            var data = _authenticationService.CheckCodeForResetPassword(code);
            return data.ToResponse();
        }

        /// <summary>
        /// Reset Password with an code exits
        /// </summary>
        /// <remarks>
        /// Return the status whether login is successed of failed
        /// </remarks>
        /// <param name="resetPasswordModel">Information of Reset Password</param>
        /// <returns>Return the status whether reset password is successed of failed</returns>
        /// <response code="200">Successful operation</response>
        /// <response code="204">Successful operation, no content is returned</response>
        /// <response code="400">Invalid Id supplied</response>
        /// <response code="401">User is unauthorized</response>
        /// <response code="403">User is not authenticated</response>
        /// <response code="404">Request is inaccessible</response>
        /// <response code="409">Data is conflicted</response>
        /// <response code="500">Server side error</response>
        [HttpPost("ResetPassword")]
        [AllowAnonymous]
        public async Task<ResetPassword> ResetPasswordWithCode(ResetPassword resetPasswordModel)
        {
            var result = _authenticationService.ResetPasswordWithCode(resetPasswordModel);

            return result.ToResponse();
        }

        #region GOOGLE LOGIN
        [HttpPost("LoginExternalCallBack")]
        [AllowAnonymous]
        public async Task<IActionResult> ExternalLoginCallBack(GoogleLoginModel model)
        {
            try
            {
                //CHECK VALID
                GoogleJsonWebSignature.ValidationSettings settings = new GoogleJsonWebSignature.ValidationSettings();
                settings.Audience = new List<string>() { _configuration["GoogleClientID"] };
                GoogleJsonWebSignature.Payload payload = await GoogleJsonWebSignature.ValidateAsync(model.GoogleTokenId, settings);

                //CHECK PAYLOAD

                var result = _authenticationService.ExternalLoginGoogle(payload.Email);

                if (result.Error != null)
                {
                    throw new LogicException(new ErrorModel(ErrorType.BAD_REQUEST, result.Error.Message));
                }

                var user = result;

                var roles = (int)user.Data.Role;
                var claims = new List<Claim>();
                claims.Add(new Claim(ClaimTypes.Sid, user.Data.Id.ToString()));
                claims.Add(new Claim(ClaimTypes.Email, payload.Email));
                claims.Add(new Claim(ClaimTypes.Role, roles.ToString()));

                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSecurityKey"]));
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
                var expiry = DateTime.Now.AddDays(Convert.ToInt32(_configuration["JwtExpiryInDays"]));

                var token = new JwtSecurityToken(
                    _configuration["JwtIssuer"],
                    _configuration["JwtAudience"],
                    claims,
                    expires: expiry,
                    signingCredentials: creds
                );

                var projectRole = _cache.Users.GetProjects(user.Data.Id);

                var results = new LoginResult
                {
                    Id = user.Data.Id,
                    IsSuccessful = true,
                    Token = new JwtSecurityTokenHandler().WriteToken(token),
                    FirstName = user.Data.FirstName,
                    LastName = user.Data.LastName,
                    AppRole = user.Data.Role,
                    Email = user.Data.Email,
                    ProjectRoles = projectRole?.Select(t => new ProjectRoleModel
                    {
                        ProjectId = t.Id,
                        ProjectRoleId = (ProjectRoles)t.ProjectRoleId
                    }).ToList()
                };
                return Ok(results);
            }
            catch (Exception ex)
            {
                throw new LogicException(new ErrorModel(ErrorType.BAD_REQUEST, "Your account could not be found"));
            }
        }

        #endregion

    }
}
