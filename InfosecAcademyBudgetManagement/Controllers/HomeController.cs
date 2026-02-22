using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InfosecAcademyBudgetManagement.Data;
using InfosecAcademyBudgetManagement.Models;

namespace InfosecAcademyBudgetManagement.Controllers;

[Authorize]
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ApplicationDbContext _context;

    public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var now = DateTime.UtcNow;
        var start = new DateTime(now.Year, now.Month, 1).AddMonths(-11);

        var costs = await _context.CostItems
            .Include(c => c.Category)
            .Where(c => !c.IsDeleted && c.Date >= start)
            .AsNoTracking()
            .ToListAsync();

        var monthLabels = new List<string>();
        var monthTotals = new List<decimal>();
        for (var i = 0; i < 12; i++)
        {
            var month = start.AddMonths(i);
            monthLabels.Add(month.ToString("MM.yyyy"));
            var total = costs
                .Where(c => c.Date.Year == month.Year && c.Date.Month == month.Month)
                .Sum(c => c.Amount);
            monthTotals.Add(total);
        }

        var categoryGroups = costs
            .Where(c => c.Category != null)
            .GroupBy(c => c.Category!.Name ?? "Uncategorized")
            .Select(g => new CategoryShare
            {
                Name = g.Key,
                Total = g.Sum(x => x.Amount)
            })
            .OrderByDescending(x => x.Total)
            .ToList();

        var totalSpend = costs.Sum(x => x.Amount);
        var topCategory = categoryGroups.FirstOrDefault();
        var topDate = costs
            .GroupBy(c => c.Date.Date)
            .Select(g => new { Date = g.Key, Total = g.Sum(x => x.Amount) })
            .OrderByDescending(x => x.Total)
            .FirstOrDefault();

        var model = new DashboardViewModel
        {
            MonthLabels = monthLabels,
            MonthTotals = monthTotals,
            CategoryShares = categoryGroups,
            TotalSpend = totalSpend,
            TopCategoryName = topCategory?.Name ?? "-",
            TopCategoryTotal = topCategory?.Total ?? 0m,
            TopDateLabel = topDate is null ? "-" : topDate.Date.ToString("dd.MM.yyyy"),
            TopDateTotal = topDate?.Total ?? 0m
        };

        var currentYear = now.Year;
        var plan = await _context.BudgetPlans
            .Include(b => b.Lines)
            .FirstOrDefaultAsync(b => b.Year == currentYear && !b.IsDeleted);

        var plannedYearTotal = plan?.Lines.Where(l => !l.IsDeleted).Sum(l => l.PlannedAmount) ?? 0m;
        var actualYearTotal = await _context.CostItems
            .Where(c => !c.IsDeleted && c.Date.Year == currentYear)
            .SumAsync(c => c.Amount);

        model.YearPlannedBudget = plannedYearTotal;
        model.YearActualSpend = actualYearTotal;
        model.YearRemainingBudget = plannedYearTotal - actualYearTotal;
        model.HasYearPlan = plan is not null;

        return View(model);
    }

    [AllowAnonymous]
    public IActionResult Privacy()
    {
        return View();
    }

    [AllowAnonymous]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}



