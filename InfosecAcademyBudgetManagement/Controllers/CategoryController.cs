using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InfosecAcademyBudgetManagement.Data;
using InfosecAcademyBudgetManagement.Models;

namespace InfosecAcademyBudgetManagement.Controllers
{
    [Authorize]
    public class CategoryController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CategoryController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var items = await _context.Categories
                .Where(c => !c.IsDeleted)
                .OrderBy(c => c.Name)
                .ToListAsync();

            return View(items);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name")] Category category)
        {
            if (!ModelState.IsValid)
            {
                return View(category);
            }

            var now = DateTime.UtcNow;
            category.CreatedAt = now;
            category.UpdatedAt = now;

            _context.Add(category);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id is null)
            {
                return NotFound();
            }

            var item = await _context.Categories
                .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);

            if (item is null)
            {
                return NotFound();
            }

            return View(item);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id is null)
            {
                return NotFound();
            }

            var item = await _context.Categories
                .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);

            if (item is null)
            {
                return NotFound();
            }

            return View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name")] Category input)
        {
            if (id != input.Id)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(input);
            }

            var item = await _context.Categories
                .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);

            if (item is null)
            {
                return NotFound();
            }

            item.Name = input.Name;
            item.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id is null)
            {
                return NotFound();
            }

            var item = await _context.Categories
                .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);

            if (item is null)
            {
                return NotFound();
            }

            var hasCosts = await _context.CostItems
                .AnyAsync(c => c.CategoryId == id && !c.IsDeleted);

            if (hasCosts)
            {
                TempData["DeleteError"] = "Bu kategoriye bağlı masraflar var. Önce masrafları taşıyın veya silin.";
                return RedirectToAction(nameof(Index));
            }

            return View(item);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var item = await _context.Categories
                .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);

            if (item is null)
            {
                return RedirectToAction(nameof(Index));
            }

            var hasCosts = await _context.CostItems
                .AnyAsync(c => c.CategoryId == id && !c.IsDeleted);

            if (hasCosts)
            {
                TempData["DeleteError"] = "Bu kategoriye bağlı masraflar var. Önce masrafları taşıyın veya silin.";
                return RedirectToAction(nameof(Index));
            }

            var now = DateTime.UtcNow;
            item.IsDeleted = true;
            item.DeletedAt = now;
            item.UpdatedAt = now;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}



