using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kloon.EmployeePerformance.Models.QuarterEvaluation
{
    public class DataSourcePersonalEvaluateModel
    {
        public bool IsAvaibleEvaluate { get; set; }

        public List<ProjectDataSourceModel> ProjectSource { get; set; }
        public List<QuarterDataSourceModel> QuarterSource { get; set; }
        public List<YearDataSourceModel> YearSource { get; set; }
        public List<QuarterEvaluationModel> DataSource { get; set; }
    }

    public class YearDataSourceModel
    {
        public int Id { get; set; }
        public string Value { get; set; }
    }

    public class QuarterDataSourceModel
    {
        public int Id { get; set; }
        public int YearId { get; set; }
        public string Value { get; set; }
    }

    public class ProjectDataSourceModel
    {
        public int Id { get; set; }
        public int YearId { get; set; }
        public int QuarterId { get; set; }
        public string Value { get; set; }
        public int ProjectId { get; set; }

    }
}
