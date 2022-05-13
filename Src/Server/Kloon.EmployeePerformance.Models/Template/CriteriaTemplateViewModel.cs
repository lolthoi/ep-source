using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kloon.EmployeePerformance.Models.Template
{
    public class EvaluationTemplateViewModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public int PositionId { get; set; }
        public List<CriteriaTemplateViewModel> CriteriaTemplateViewModels { get; set; }
    }
    public class CriteriaTemplateViewModel
    {
        public Guid Id { get; set; }
        public Guid? TemplateId { get; set; }
        public Guid? TypeId { get; set; }
        public Guid CriteriaStoreId { get; set; }
        public Guid? CriteriaTypeStoreId { get; set; }
        public string Description { get; set; }
        public int OrderNo { get; set; }
        public string Name { get; set; }
    }
}
