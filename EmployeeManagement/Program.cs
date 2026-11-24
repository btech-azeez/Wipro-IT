using EmployeeManagement.Data;
using EmployeeManagement.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using EmployeeManagement.Models;
using System.Security.Cryptography;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ── Database ─────────────────────────────────────────────────────────────────
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ── Session ──────────────────────────────────────────────────────────────────
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(opt =>
{
    opt.IdleTimeout        = TimeSpan.FromMinutes(builder.Configuration.GetValue<int>("Session:TimeoutInMinutes", 30));
    opt.Cookie.HttpOnly    = true;
    opt.Cookie.IsEssential = true;
    opt.Cookie.Name        = ".Wipro.Session";
});

// ── Authentication: Cookie (UI) + JWT (API) ───────────────────────────────────
var jwtKey = builder.Configuration["Jwt:SecretKey"];
if (string.IsNullOrWhiteSpace(jwtKey))
{
    if (!builder.Environment.IsDevelopment())
        throw new InvalidOperationException("Configure Jwt__SecretKey with a secret of at least 32 characters.");

    jwtKey = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
    builder.Configuration["Jwt:SecretKey"] = jwtKey;
}

if (Encoding.UTF8.GetByteCount(jwtKey) < 32)
    throw new InvalidOperationException("Jwt__SecretKey must contain at least 32 bytes.");

builder.Services.AddAuthentication(opt =>
{
    opt.DefaultScheme          = CookieAuthenticationDefaults.AuthenticationScheme;
    opt.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
.AddCookie(opt =>
{
    opt.LoginPath         = "/Account/Login";
    opt.LogoutPath        = "/Account/Logout";
    opt.AccessDeniedPath  = "/Account/AccessDenied";
    opt.ExpireTimeSpan    = TimeSpan.FromHours(8);
    opt.SlidingExpiration = true;
    opt.Cookie.Name       = ".Wipro.Auth";
    opt.Cookie.HttpOnly   = true;
    opt.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
})
.AddJwtBearer(opt =>
{
    opt.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        ValidateIssuer           = true,
        ValidIssuer              = builder.Configuration["Jwt:Issuer"] ?? "WiproEMS",
        ValidateAudience         = true,
        ValidAudience            = builder.Configuration["Jwt:Audience"] ?? "WiproEMS",
        ValidateLifetime         = true,
        ClockSkew                = TimeSpan.Zero
    };
});

// ── RBAC Authorization Policies ───────────────────────────────────────────────
builder.Services.AddAuthorization(opt =>
{
    opt.AddPolicy("CEOOnly",           p => p.RequireRole("CEO"));
    opt.AddPolicy("HROnly",            p => p.RequireRole("HR"));
    opt.AddPolicy("ManagerOrAbove",    p => p.RequireRole("CEO", "HR", "Manager"));
    opt.AddPolicy("HROrCEO",           p => p.RequireRole("CEO", "HR"));
    opt.AddPolicy("AllStaff",          p => p.RequireRole("CEO", "HR", "Manager", "Employee"));
});

// ── Application Services ──────────────────────────────────────────────────────
builder.Services.AddScoped<IJwtService,      JwtService>();
builder.Services.AddScoped<IAuthService,     AuthService>();
builder.Services.AddScoped<IEmployeeService, EmployeeService>();
builder.Services.AddSignalR();

builder.Services.AddRazorPages(opt =>
{
    // require login for every page by default
    opt.Conventions.AuthorizeFolder("/");
    opt.Conventions.AllowAnonymousToFolder("/Account");
    opt.Conventions.AllowAnonymousToPage("/Error");
});

builder.Services.AddAntiforgery(opt =>
{
    opt.HeaderName  = "X-CSRF-TOKEN";
    opt.Cookie.Name = ".Wipro.XSRF";
});

var app = builder.Build();

// ── Database initialise ───────────────────────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
    await EmployeeDataSeeder.SeedAsync(db);

    var bootstrapUsername = builder.Configuration["BootstrapAdmin:Username"];
    var bootstrapFullName = builder.Configuration["BootstrapAdmin:FullName"];
    var bootstrapEmail = builder.Configuration["BootstrapAdmin:Email"];
    var bootstrapPassword = builder.Configuration["BootstrapAdmin:Password"];
    var bootstrapValues = new[] { bootstrapUsername, bootstrapFullName, bootstrapEmail, bootstrapPassword };

    if (bootstrapValues.Any(value => !string.IsNullOrWhiteSpace(value)) &&
        bootstrapValues.Any(string.IsNullOrWhiteSpace))
        throw new InvalidOperationException("Configure all BootstrapAdmin values to create the initial administrator.");

    if (bootstrapValues.All(value => !string.IsNullOrWhiteSpace(value)) && !await db.Users.AnyAsync())
    {
        if (bootstrapPassword!.Length < 12)
            throw new InvalidOperationException("BootstrapAdmin__Password must be at least 12 characters.");

        db.Users.Add(new User
        {
            Username = bootstrapUsername!,
            FullName = bootstrapFullName!,
            Email = bootstrapEmail!,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(bootstrapPassword),
            Role = WiproRoles.CEO,
            Department = "Executive",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();
    }
}

if (!app.Environment.IsDevelopment())
    app.UseExceptionHandler("/Error");

app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();
app.MapRazorPages();
app.MapHub<EmployeeNotificationHub>("/hubs/employee-notifications");

app.Run();
