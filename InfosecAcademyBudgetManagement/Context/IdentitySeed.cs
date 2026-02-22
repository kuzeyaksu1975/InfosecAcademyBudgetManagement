using InfosecAcademyBudgetManagement.Models;
using Microsoft.AspNetCore.Identity;

namespace InfosecAcademyBudgetManagement.Data
{
    public static class IdentitySeed
    {
        public static async Task SeedAsync(IServiceProvider services, IConfiguration configuration)
        {
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

            var roles = new[] { "Kullanıcı", "Müdür", "Yönetici" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            var adminEmail = configuration["AdminSeed:Email"] ?? "admin@local.test";
            var adminPassword = configuration["AdminSeed:Password"] ?? "Admin123!";
            var adminFirstName = configuration["AdminSeed:FirstName"] ?? "System";
            var adminLastName = configuration["AdminSeed:LastName"] ?? "Admin";

            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser is null)
            {
                // Email değişmişse mevcut yönetici hesabını seed ayarlarına göre güncelle.
                var managerUserIds = await userManager.GetUsersInRoleAsync("Yönetici");
                adminUser = managerUserIds.FirstOrDefault();
            }

            if (adminUser is null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true,
                    FirstName = adminFirstName,
                    LastName = adminLastName
                };

                var createResult = await userManager.CreateAsync(adminUser, adminPassword);
                if (!createResult.Succeeded)
                {
                    var errors = string.Join("; ", createResult.Errors.Select(e => e.Description));
                    throw new InvalidOperationException($"Seed admin kullanıcı oluşturulamadı: {errors}");
                }
            }
            else
            {
                adminUser.UserName = adminEmail;
                adminUser.Email = adminEmail;
                adminUser.EmailConfirmed = true;
                adminUser.FirstName = adminFirstName;
                adminUser.LastName = adminLastName;

                var updateResult = await userManager.UpdateAsync(adminUser);
                if (!updateResult.Succeeded)
                {
                    var errors = string.Join("; ", updateResult.Errors.Select(e => e.Description));
                    throw new InvalidOperationException($"Seed admin kullanıcı güncellenemedi: {errors}");
                }

                if (!await userManager.CheckPasswordAsync(adminUser, adminPassword))
                {
                    var resetToken = await userManager.GeneratePasswordResetTokenAsync(adminUser);
                    var resetResult = await userManager.ResetPasswordAsync(adminUser, resetToken, adminPassword);
                    if (!resetResult.Succeeded)
                    {
                        var errors = string.Join("; ", resetResult.Errors.Select(e => e.Description));
                        throw new InvalidOperationException($"Seed admin şifresi güncellenemedi: {errors}");
                    }
                }
            }

            if (!await userManager.IsInRoleAsync(adminUser, "Yönetici"))
            {
                await userManager.AddToRoleAsync(adminUser, "Yönetici");
            }
        }
    }
}
