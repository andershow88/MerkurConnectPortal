using Microsoft.EntityFrameworkCore;
using MerkurConnectPortal.Application.Interfaces;
using MerkurConnectPortal.Application.Services;
using MerkurConnectPortal.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

// --- Services ---
builder.Services.AddControllersWithViews();

// Datenbankkontext (SQLite für MVP, per Konfiguration auf SQL Server umstellbar)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Data Source=merkurconnect.db";

if (connectionString.Contains(".db") || connectionString.Contains("Data Source="))
{
    builder.Services.AddDbContext<ApplicationDbContext>(opt =>
        opt.UseSqlite(connectionString));
}
else
{
    builder.Services.AddDbContext<ApplicationDbContext>(opt =>
        opt.UseSqlServer(connectionString));
}

builder.Services.AddScoped<IApplicationDbContext>(sp =>
    sp.GetRequiredService<ApplicationDbContext>());

// Application Services
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IObjektService, ObjektService>();
builder.Services.AddScoped<IDokumentService, DokumentService>();
builder.Services.AddScoped<INachrichtService, NachrichtService>();
builder.Services.AddScoped<IAuthService, AuthService>();

// Cookie-Authentifizierung
builder.Services.AddAuthentication("MerkurCookieAuth")
    .AddCookie("MerkurCookieAuth", options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/Login";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
        options.Cookie.Name = "MerkurConnect.Auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
    });

builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// --- Datenbankinitialisierung und Seeding ---
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await DataSeeder.SeedAsync(db);
}

// --- Middleware ---
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Dashboard}/{action=Index}/{id?}");

app.Run();
