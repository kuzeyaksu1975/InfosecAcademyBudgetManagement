namespace InfosecAcademyBudgetManagement.Models
{
    public class DashboardViewModel
    {
        public List<string> MonthLabels { get; set; } = [];
        public List<decimal> MonthTotals { get; set; } = [];
        public List<CategoryShare> CategoryShares { get; set; } = [];
        public decimal TotalSpend { get; set; }
        public string TopCategoryName { get; set; } = "-";
        public decimal TopCategoryTotal { get; set; }
        public string TopDateLabel { get; set; } = "-";
        public decimal TopDateTotal { get; set; }
        public decimal YearPlannedBudget { get; set; }
        public decimal YearActualSpend { get; set; }
        public decimal YearRemainingBudget { get; set; }
        public bool HasYearPlan { get; set; }
    }

    public class CategoryShare
    {
        public string Name { get; set; } = "Uncategorized";
        public decimal Total { get; set; }
    }
}

