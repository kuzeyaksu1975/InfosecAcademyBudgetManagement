using Cost;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.IO;
using InfosecAcademyBudgetManagement.Data;

namespace InfosecAcademyBudgetManagement.Controllers
{
    [Authorize]
    public class CostController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CostController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var items = await _context.CostItems
                .Include(c => c.Category)
                .Where(c => !c.IsDeleted)
                .OrderByDescending(c => c.Date)
                .ToListAsync();

            return View(items);
        }

        public IActionResult Create()
        {
            ViewData["Categories"] = new SelectList(
                _context.Categories.Where(c => !c.IsDeleted).OrderBy(c => c.Name),
                "Id",
                "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Amount,Date,CategoryId,Description")] CostItem costItem, IFormFile? document)
        {
            if (!ModelState.IsValid)
            {
                ViewData["Categories"] = new SelectList(
                    _context.Categories.Where(c => !c.IsDeleted).OrderBy(c => c.Name),
                    "Id",
                    "Name",
                    costItem.CategoryId);
                return View(costItem);
            }

            var now = DateTime.UtcNow;
            if (costItem.Date == default)
            {
                costItem.Date = now;
            }

            costItem.CreatedAt = now;
            costItem.UpdatedAt = now;

            if (document is not null)
            {
                try
                {
                    var docInfo = await SaveDocumentAsync(document);
                    costItem.DocumentPath = docInfo.Path;
                    costItem.DocumentName = docInfo.OriginalName;
                    costItem.DocumentContentType = docInfo.ContentType;
                    costItem.DocumentSize = docInfo.Size;
                }
                catch (InvalidOperationException ex)
                {
                    ModelState.AddModelError("Document", ex.Message);
                    ViewData["Categories"] = new SelectList(
                        _context.Categories.Where(c => !c.IsDeleted).OrderBy(c => c.Name),
                        "Id",
                        "Name",
                        costItem.CategoryId);
                    return View(costItem);
                }
            }

            _context.Add(costItem);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id is null)
            {
                return NotFound();
            }

            var item = await _context.CostItems
                .Include(c => c.Category)
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

            var item = await _context.CostItems
                .Include(c => c.Category)
                .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);

            if (item is null)
            {
                return NotFound();
            }

            ViewData["Categories"] = new SelectList(
                _context.Categories.Where(c => !c.IsDeleted).OrderBy(c => c.Name),
                "Id",
                "Name",
                item.CategoryId);
            return View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Amount,Date,CategoryId,Description")] CostItem input, IFormFile? document)
        {
            if (id != input.Id)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                ViewData["Categories"] = new SelectList(
                    _context.Categories.Where(c => !c.IsDeleted).OrderBy(c => c.Name),
                    "Id",
                    "Name",
                    input.CategoryId);
                return View(input);
            }

            var item = await _context.CostItems
                .Include(c => c.Category)
                .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);

            if (item is null)
            {
                return NotFound();
            }

            item.Name = input.Name;
            item.Amount = input.Amount;
            item.CategoryId = input.CategoryId;
            item.Date = input.Date == default ? DateTime.UtcNow : input.Date;
            item.Description = input.Description;
            item.UpdatedAt = DateTime.UtcNow;

            if (document is not null)
            {
                try
                {
                    var docInfo = await SaveDocumentAsync(document);
                    item.DocumentPath = docInfo.Path;
                    item.DocumentName = docInfo.OriginalName;
                    item.DocumentContentType = docInfo.ContentType;
                    item.DocumentSize = docInfo.Size;
                }
                catch (InvalidOperationException ex)
                {
                    ModelState.AddModelError("Document", ex.Message);
                    ViewData["Categories"] = new SelectList(
                        _context.Categories.Where(c => !c.IsDeleted).OrderBy(c => c.Name),
                        "Id",
                        "Name",
                        input.CategoryId);
                    return View(input);
                }
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id is null)
            {
                return NotFound();
            }

            var item = await _context.CostItems
                .Include(c => c.Category)
                .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);

            if (item is null)
            {
                return NotFound();
            }

            return View(item);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var item = await _context.CostItems
                .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);

            if (item is null)
            {
                return RedirectToAction(nameof(Index));
            }

            var now = DateTime.UtcNow;
            item.IsDeleted = true;
            item.DeletedAt = now;
            item.UpdatedAt = now;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        private async Task<(string Path, string OriginalName, string ContentType, long Size)> SaveDocumentAsync(IFormFile document)
        {
            if (document.Length == 0)
            {
                throw new InvalidOperationException("Empty document.");
            }

            if (!string.Equals(document.ContentType, "application/pdf", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Only PDF files are allowed.");
            }

            var now = DateTime.UtcNow;
            var uploadRoot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "expenses", now.ToString("yyyy"), now.ToString("MM"));
            Directory.CreateDirectory(uploadRoot);

            var fileName = $"{Guid.NewGuid():N}.pdf";
            var fullPath = Path.Combine(uploadRoot, fileName);

            await using var stream = new FileStream(fullPath, FileMode.Create);
            await document.CopyToAsync(stream);

            var relativePath = $"/uploads/expenses/{now:yyyy}/{now:MM}/{fileName}";
            return (relativePath, document.FileName, document.ContentType, document.Length);
        }
    }
}



