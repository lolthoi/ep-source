using Kloon.EmployeePerformance.DataAccess;
using Kloon.EmployeePerformance.DataAccess.Domain;
using Kloon.EmployeePerformance.Logic.Services.Base;
using Kloon.EmployeePerformance.Models.Common;
using Kloon.EmployeePerformance.Models.Position;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kloon.EmployeePerformance.Logic.Services
{
    public interface IPositionService
    {
        ResultModel<List<PositionModel>> GetAll();
        ResultModel<List<PositionModel>> GetPositionsForTemplate();
    }
    public class PositionService : IPositionService
    {
        private readonly IAuthenLogicService<PositionService> _logicService;
        private readonly IUnitOfWork<EmployeePerformanceContext> _dbContext;

        private readonly IEntityRepository<Position> _positionRespo;
        public PositionService(IAuthenLogicService<PositionService> logicService)
        {
            _logicService = logicService;
            _dbContext = logicService.DbContext;

            _positionRespo = _dbContext.GetRepository<Position>();
        }
        public ResultModel<List<PositionModel>> GetAll()
        {
            var result = _logicService
                .Start()
                .ThenAuthorize(Roles.ADMINISTRATOR, Roles.USER)
                .ThenValidate(current =>
                {
                    return null;
                })
               .ThenImplement(current =>
               {
                    return _logicService.Cache.Position.GetValues()
                            .OrderBy(t => t.Id)
                            .Select(t => new PositionModel { 
                                Id = t.Id,
                                Name = t.Name
                            })
                            .ToList();
               });
            return result;
        }

        public  ResultModel<List<PositionModel>> GetPositionsForTemplate()
        {
            var result = _logicService
                .Start()
                .ThenAuthorize(Roles.ADMINISTRATOR, Roles.USER)
                .ThenValidate(current => null)
                .ThenImplement(current =>
                {
                    var data = _logicService.Cache.Position
                            .GetValues()
                            .Where(x => x.Name != "CEO" && !x.Name.Contains("Intern"))
                            .Select(t => new PositionModel
                            {
                                Id = t.Id,
                                Name = t.Name
                            })
                            .OrderBy(x => x.Name)
                            .ToList();
                    return data;
                });
            return result;
        }
    }
}
