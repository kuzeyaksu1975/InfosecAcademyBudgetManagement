using Cost;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.IO;
using InfosecAcademyBudgetManagement.Data;
using InfosecAcademyBudgetManagement.Models;

namespace InfosecAcademyBudgetManagement.Controllers
{
    [Authorize]
    public class CostController : Controller
    {
        private const long MaxPdfFileSizeBytes = 10 * 1024 * 1024; // 10 MB
        private readonly ApplicationDbContext _context;
        private readonly string _documentStorageRoot;

        public CostController(ApplicationDbContext context)
        {
            _context = context;
            _documentStorageRoot = Path.Combine(Directory.GetCurrentDirectory(), "App_Data", "uploads", "expenses");
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

        [HttpGet]
        public async Task<IActionResult> DocumentViewer(int id)
        {
            var item = await _context.CostItems
                .AsNoTracking()
                .Include(c => c.Category)
                .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted && !string.IsNullOrWhiteSpace(c.DocumentPath));

            if (item is null)
            {
                return NotFound();
            }

            var (fullPath, exists) = ResolveDocumentPhysicalPath(item.DocumentPath!);
            if (!exists)
            {
                return NotFound();
            }

            var model = new CostDocumentViewerViewModel
            {
                CostId = item.Id,
                CostName = item.Name ?? "-",
                CategoryName = item.Category?.Name ?? "-",
                CostDate = item.Date,
                DownloadFileName = string.IsNullOrWhiteSpace(item.DocumentName) ? "document.pdf" : Path.GetFileName(item.DocumentName)
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> StreamDocument(int id)
        {
            var item = await _context.CostItems
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted && !string.IsNullOrWhiteSpace(c.DocumentPath));

            if (item is null)
            {
                return NotFound();
            }

            var (fullPath, exists) = ResolveDocumentPhysicalPath(item.DocumentPath!);
            if (!exists)
            {
                return NotFound();
            }

            var contentType = string.IsNullOrWhiteSpace(item.DocumentContentType)
                ? "application/pdf"
                : item.DocumentContentType;

            var stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            return File(stream, contentType, enableRangeProcessing: true);
        }

        [HttpGet]
        public async Task<IActionResult> DownloadDocument(int id)
        {
            var item = await _context.CostItems
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);

            if (item is null || string.IsNullOrWhiteSpace(item.DocumentPath))
            {
                return NotFound();
            }

            var (fullPath, exists) = ResolveDocumentPhysicalPath(item.DocumentPath);
            if (!exists)
            {
                return NotFound();
            }

            var downloadName = string.IsNullOrWhiteSpace(item.DocumentName) ? "document.pdf" : Path.GetFileName(item.DocumentName);
            var contentType = string.IsNullOrWhiteSpace(item.DocumentContentType)
                ? "application/pdf"
                : item.DocumentContentType;

            var stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            return File(stream, contentType, downloadName, enableRangeProcessing: true);
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

            if (document.Length > MaxPdfFileSizeBytes)
            {
                throw new InvalidOperationException("PDF file is too large. Maximum allowed size is 10 MB.");
            }

            var extension = Path.GetExtension(document.FileName);
            if (!string.Equals(extension, ".pdf", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Only PDF files are allowed.");
            }

            if (!string.Equals(document.ContentType, "application/pdf", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Only PDF files are allowed.");
            }

            if (!await HasPdfSignatureAsync(document))
            {
                throw new InvalidOperationException("Invalid PDF file format.");
            }

            var now = DateTime.UtcNow;
            var year = now.ToString("yyyy");
            var month = now.ToString("MM");
            var uploadRoot = Path.Combine(_documentStorageRoot, year, month);
            Directory.CreateDirectory(uploadRoot);

            var fileName = $"{Guid.NewGuid():N}.pdf";
            var fullPath = Path.Combine(uploadRoot, fileName);

            await using var stream = new FileStream(fullPath, FileMode.CreateNew);
            await document.CopyToAsync(stream);

            var relativePath = Path.Combine(year, month, fileName);
            return (relativePath, Path.GetFileName(document.FileName), document.ContentType, document.Length);
        }

        private static async Task<bool> HasPdfSignatureAsync(IFormFile document)
        {
            var expected = new byte[] { 0x25, 0x50, 0x44, 0x46, 0x2D }; // %PDF-
            await using var stream = document.OpenReadStream();
            var signature = new byte[expected.Length];
            var read = await stream.ReadAsync(signature.AsMemory(0, signature.Length));
            if (read < expected.Length)
            {
                return false;
            }

            for (var i = 0; i < expected.Length; i++)
            {
                if (signature[i] != expected[i])
                {
                    return false;
                }
            }

            return true;
        }

        private (string FullPath, bool Exists) ResolveDocumentPhysicalPath(string storedPath)
        {
            // New storage format: yyyy/MM/{guid}.pdf under App_Data/uploads/expenses
            var normalizedStoredPath = storedPath.Replace('/', Path.DirectorySeparatorChar);
            var candidate = Path.GetFullPath(Path.Combine(_documentStorageRoot, normalizedStoredPath));
            var normalizedRoot = Path.GetFullPath(_documentStorageRoot);
            if (candidate.StartsWith(normalizedRoot, StringComparison.OrdinalIgnoreCase) && System.IO.File.Exists(candidate))
            {
                return (candidate, true);
            }

            // Backward compatibility for older records saved under /uploads/expenses in wwwroot.
            if (storedPath.StartsWith("/uploads/expenses/", StringComparison.OrdinalIgnoreCase))
            {
                var legacyRelative = storedPath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
                var legacyRoot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                var legacyPath = Path.GetFullPath(Path.Combine(legacyRoot, legacyRelative));
                var normalizedLegacyRoot = Path.GetFullPath(legacyRoot);
                if (legacyPath.StartsWith(normalizedLegacyRoot, StringComparison.OrdinalIgnoreCase) && System.IO.File.Exists(legacyPath))
                {
                    return (legacyPath, true);
                }
            }

            return (string.Empty, false);
        }
    }
}



