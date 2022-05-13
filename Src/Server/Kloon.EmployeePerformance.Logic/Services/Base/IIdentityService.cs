using Kloon.EmployeePerformance.Models.User;

namespace Kloon.EmployeePerformance.Logic.Services.Base
{
    public interface IIdentityService
    {
        LoginUserModel GetCurrentUser();
    }
}
