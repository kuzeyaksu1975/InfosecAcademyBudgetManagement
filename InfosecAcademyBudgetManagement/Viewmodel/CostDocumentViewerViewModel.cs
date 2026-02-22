namespace InfosecAcademyBudgetManagement.Models
{
    public class CostDocumentViewerViewModel
    {
        public int CostId { get; set; }
        public string CostName { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public DateTime CostDate { get; set; }
        public string DownloadFileName { get; set; } = "document.pdf";
    }
}
