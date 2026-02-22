using System.Globalization;
using InfosecAcademyBudgetManagement.Data;
using InfosecAcademyBudgetManagement.Helpers;
using InfosecAcademyBudgetManagement.Interface;
using InfosecAcademyBudgetManagement.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
AppContext.SetSwitch("Npgsql.DisableDateTimeInfinityConversions", true);

var builder = WebApplication.CreateBuilder(args);

var cultureInfo = new CultureInfo("tr-TR");
CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services
    .AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequiredLength = 6;
        options.Lockout.MaxFailedAccessAttempts = 5;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
});

builder.Services.AddDataProtection();
builder.Services.AddScoped<IEmailPasswordProtector, EmailPasswordProtector>();
builder.Services.AddScoped<IEmailServiceHelper, EmailServiceHelper>();

builder.Services.AddAuthorization();
builder.Services.AddControllersWithViews();
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    options.DefaultRequestCulture = new Microsoft.AspNetCore.Localization.RequestCulture(cultureInfo);
    options.SupportedCultures = [cultureInfo];
    options.SupportedUICultures = [cultureInfo];
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseRequestLocalization();
app.UseAuthentication();
app.UseAuthorization();

using (var scope = app.Services.CreateScope())
{
    await IdentitySeed.SeedAsync(scope.ServiceProvider, app.Configuration);
}

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
