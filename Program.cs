using System.Data;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using MonitoringDokumenGS.Data;
using MonitoringDokumenGS.Interfaces;
using MonitoringDokumenGS.Services;
using MonitoringDokumenGS.Services.Infrastructure;
using MonitoringDokumenGS.Services.Master;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("fixed", builder =>
    {
        builder.Window = TimeSpan.FromMinutes(1);
        builder.PermitLimit = 5;
        builder.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        builder.QueueLimit = 2;
    });
});

// DI registrations (Service Layer)
builder.Services.AddScoped<IAuth, AuthService>();
builder.Services.AddScoped<IUser, UserService>();
builder.Services.AddScoped<IFile, FileService>();
builder.Services.AddScoped<IAuditLog, AuditLogService>();
builder.Services.AddScoped<IApprovalStatus, ApprovalStatusService>();
builder.Services.AddScoped<IAttachmentTypes, AttachmentTypeService>();
builder.Services.AddScoped<IContractStatus, ContractStatusService>();
builder.Services.AddScoped<IInvoiceProgressStatuses, InvoiceProgressStatusService>();
builder.Services.AddScoped<IVendorCategory, VendorCategoryService>();
builder.Services.AddScoped<IVendor, VendorService>();

builder.Services.AddAuthorization();

builder.Services.AddDbContext<ApplicationDBContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IDbConnection>(sp =>
    new SqlConnection(builder.Configuration.GetConnectionString("DefaultConnection")));

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddAuthentication(options =>
    {
        // Default to cookie authentication for the web UI. API controllers can explicitly require JWT.
        options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    })
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Index";
        options.AccessDeniedPath = "/Auth/Index";
        options.Cookie.Name = "mdgs_auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        //options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
    })
    .AddJwtBearer(options =>
    {
        var key = builder.Configuration["Jwt:Key"];
        if (string.IsNullOrWhiteSpace(key))
            throw new InvalidOperationException("Configuration 'Jwt:Key' is missing. Please set 'Jwt:Key' in appsettings.json or as an environment variable (Jwt__Key).");

        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(
                System.Text.Encoding.UTF8.GetBytes(key))
        };
    });

// File Storage Options
builder.Services.Configure<FileStorageOptions>(
    builder.Configuration.GetSection("FileStorage"));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

// Swagger aktif (boleh prod, tapi biasanya dibatasi)
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// Route API
app.MapControllers();

// Route default (non-area, optional)
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auth}/{action=Index}/{id?}");

// Using Rate Limiter
app.UseRateLimiter();

app.Run();
