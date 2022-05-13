using Kloon.EmployeePerformance.Logic.Services;
using Kloon.EmployeePerformance.Models.Template;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Kloon.EmployeePerformance.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EvaluationTemplateController : Controller
    {
        private readonly IEvaluationTemplateService _evaluationTemplate;
        public EvaluationTemplateController(IEvaluationTemplateService evaluationTemplate)
        {
            _evaluationTemplate = evaluationTemplate;
        }
        [HttpGet]
        public ActionResult<List<EvaluationTemplateViewModel>> GetAll()
        {
            return _evaluationTemplate.GetAll().ToResponse();
        }
        [HttpGet("{id:Guid}")]
        public ActionResult<EvaluationTemplateViewModel> GetById(Guid id)
        {
            return _evaluationTemplate.GetById(id).ToResponse();
        }
        [HttpPost]
        public ActionResult<EvaluationTemplateViewModel> Create(EvaluationTemplateViewModel evaluationTemplateModel)
        {
            return _evaluationTemplate.Create(evaluationTemplateModel).ToResponse();
        }
        [HttpPut]
        public ActionResult<EvaluationTemplateViewModel> Update(EvaluationTemplateViewModel evaluationTemplateModel)
        {
            return _evaluationTemplate.Edit(evaluationTemplateModel).ToResponse();
        }
        [HttpDelete("{id:Guid}")]
        public ActionResult<bool> Delete(Guid id)
        {
            return _evaluationTemplate.Delete(id).ToResponse();
        }

        [HttpPost("{id:Guid}/reorder")]
        public ActionResult<bool> ReOrder(Guid id, List<CriteriaTemplateViewModel> models)
        {
            return _evaluationTemplate.ReOrder(id, models).ToResponse();
        }
    }
}
