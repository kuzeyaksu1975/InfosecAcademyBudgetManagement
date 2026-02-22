using InfosecAcademyBudgetManagement.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InfosecAcademyBudgetManagement.Controllers
{
    [Authorize(Roles = "Yönetici")]
    public class AdminUsersController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminUsersController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users.AsNoTracking().OrderBy(u => u.Email).ToListAsync();
            var list = new List<AdminUserListItemViewModel>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                list.Add(new AdminUserListItemViewModel
                {
                    Id = user.Id,
                    Email = user.Email ?? "-",
                    FullName = $"{user.FirstName} {user.LastName}".Trim(),
                    Role = roles.FirstOrDefault() ?? "Kullanıcı",
                    TwoFactorEnabled = user.TwoFactorEnabled
                });
            }

            return View(list);
        }

        public IActionResult Create()
        {
            return View(new AdminCreateUserViewModel());
        }

        public async Task<IActionResult> Edit(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user is null)
            {
                TempData["DeleteError"] = "Kullanıcı bulunamadı.";
                return RedirectToAction(nameof(Index));
            }

            var roles = await _userManager.GetRolesAsync(user);
            var model = new AdminEditUserViewModel
            {
                Id = user.Id,
                Email = user.Email ?? string.Empty,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Department = user.Department,
                Role = roles.FirstOrDefault() ?? "User",
                TwoFactorEnabled = user.TwoFactorEnabled
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AdminCreateUserViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var normalizedRole = model.Role switch
            {
                "Yönetici" => "Yönetici",
                "Müdür" => "Müdür",
                _ => "Kullanıcı"
            };
            var existing = await _userManager.FindByEmailAsync(model.Email);
            if (existing is not null)
            {
                ModelState.AddModelError(nameof(model.Email), "Bu e-posta ile kayıtlı kullanıcı mevcut.");
                return View(model);
            }

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                EmailConfirmed = true,
                FirstName = model.FirstName,
                LastName = model.LastName,
                Department = model.Department
            };

            var createResult = await _userManager.CreateAsync(user, model.Password);
            if (!createResult.Succeeded)
            {
                foreach (var error in createResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }

                return View(model);
            }

            await _userManager.AddToRoleAsync(user, normalizedRole);
            TempData["SuccessMessage"] = "Kullanıcı oluşturuldu.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(AdminEditUserViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.FindByIdAsync(model.Id);
            if (user is null)
            {
                TempData["DeleteError"] = "Kullanıcı bulunamadı.";
                return RedirectToAction(nameof(Index));
            }

            var existingWithEmail = await _userManager.FindByEmailAsync(model.Email);
            if (existingWithEmail is not null && existingWithEmail.Id != user.Id)
            {
                ModelState.AddModelError(nameof(model.Email), "Bu e-posta başka bir kullanıcı tarafından kullanılıyor.");
                return View(model);
            }

            var requestedRole = model.Role switch
            {
                "Yönetici" => "Yönetici",
                "Müdür" => "Müdür",
                _ => "Kullanıcı"
            };
            var currentUserId = _userManager.GetUserId(User);
            if (currentUserId == user.Id && requestedRole != "Yönetici")
            {
                ModelState.AddModelError(nameof(model.Role), "Kendi hesabınızın rolünü Yönetici dışına düşüremezsiniz.");
                return View(model);
            }

            user.Email = model.Email;
            user.UserName = model.Email;
            user.NormalizedEmail = _userManager.NormalizeEmail(model.Email);
            user.NormalizedUserName = _userManager.NormalizeName(model.Email);
            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.Department = model.Department;

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                foreach (var error in updateResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }

                return View(model);
            }

            var userRoles = await _userManager.GetRolesAsync(user);
            if (!userRoles.Contains(requestedRole))
            {
                if (userRoles.Count > 0)
                {
                    await _userManager.RemoveFromRolesAsync(user, userRoles);
                }

                await _userManager.AddToRoleAsync(user, requestedRole);
            }

            if (user.TwoFactorEnabled != model.TwoFactorEnabled)
            {
                await _userManager.SetTwoFactorEnabledAsync(user, model.TwoFactorEnabled);
            }

            TempData["SuccessMessage"] = "Kullanıcı bilgileri güncellendi.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            var currentUserId = _userManager.GetUserId(User);
            if (currentUserId == id)
            {
                TempData["DeleteError"] = "Kendi hesabınızı silemezsiniz.";
                return RedirectToAction(nameof(Index));
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user is null)
            {
                TempData["DeleteError"] = "Kullanıcı bulunamadı.";
                return RedirectToAction(nameof(Index));
            }

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                TempData["DeleteError"] = string.Join("; ", result.Errors.Select(e => e.Description));
                return RedirectToAction(nameof(Index));
            }

            TempData["SuccessMessage"] = "Kullanıcı silindi.";
            return RedirectToAction(nameof(Index));
        }
    }
}
