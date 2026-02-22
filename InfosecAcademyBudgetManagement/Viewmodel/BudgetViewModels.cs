using System.ComponentModel.DataAnnotations;

namespace InfosecAcademyBudgetManagement.Models
{
    public class BudgetPlanEditViewModel
    {
        public int PlanId { get; set; }
        public int Year { get; set; }
        public List<Category> Categories { get; set; } = [];
        public List<BudgetLineInput> Lines { get; set; } = [];
    }

    public class BudgetLineInput
    {
        public int CategoryId { get; set; }
        public int Month { get; set; }
        [Range(0, double.MaxValue)]
        public decimal PlannedAmount { get; set; }
    }

    public class BudgetReportViewModel
    {
        public int Year { get; set; }
        public List<string> MonthLabels { get; set; } = [];
        public List<decimal> PlannedMonthlyTotals { get; set; } = [];
        public List<decimal> ActualMonthlyTotals { get; set; } = [];
        public List<BudgetCategoryReportRow> CategoryRows { get; set; } = [];
        public List<CategoryMonthlySeries> CategoryMonthlySeries { get; set; } = [];
    }

    public class BudgetCategoryReportRow
    {
        public string Name { get; set; } = "-";
        public decimal PlannedTotal { get; set; }
        public decimal ActualTotal { get; set; }
        public decimal Variance => ActualTotal - PlannedTotal;
        public decimal VariancePercent => PlannedTotal == 0 ? 0 : (Variance / PlannedTotal) * 100;
    }

    public class CategoryMonthlySeries
    {
        public string Name { get; set; } = "-";
        public List<decimal> Planned { get; set; } = [];
        public List<decimal> Actual { get; set; } = [];
    }
}

