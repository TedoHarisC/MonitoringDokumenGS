using System.Data;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using MonitoringDokumenGS.Data;
using MonitoringDokumenGS.Interfaces;
using MonitoringDokumenGS.Services;
using MonitoringDokumenGS.Services.Infrastructure;
using MonitoringDokumenGS.Services.Master;
using MonitoringDokumenGS.Services.Transaction;
using MonitoringDokumenGS.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;


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

// Register IHttpContextAccessor
builder.Services.AddHttpContextAccessor();

// DI registrations (Service Layer)
builder.Services.AddScoped<IAuth, AuthService>();
builder.Services.AddScoped<IUser, UserService>();
builder.Services.AddScoped<IFile, FileService>();
builder.Services.AddScoped<IAuditLog, AuditLogService>();
builder.Services.AddScoped<INotifications, NotificationService>();
builder.Services.AddScoped<IApprovalStatus, ApprovalStatusService>();
builder.Services.AddScoped<IAttachmentTypes, AttachmentTypeService>();
builder.Services.AddScoped<IContractStatus, ContractStatusService>();
builder.Services.AddScoped<IInvoiceProgressStatuses, InvoiceProgressStatusService>();
builder.Services.AddScoped<IInvoice, InvoiceService>();
builder.Services.AddScoped<IContract, ContractService>();
builder.Services.AddScoped<IAttachment, AttachmentService>();
builder.Services.AddScoped<IVendorCategory, VendorCategoryService>();
builder.Services.AddScoped<IVendor, VendorService>();
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<IBudget, BudgetService>();
builder.Services.AddScoped<IDashboard, DashboardService>();

// Configure Email Options from appsettings.json
builder.Services.Configure<MonitoringDokumenGS.Models.EmailOptions>(
    builder.Configuration.GetSection("Email"));
builder.Services.AddScoped<IEmailService, SmtpEmailService>();

builder.Services.AddAuthorization();

builder.Services.AddDbContext<ApplicationDBContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IDbConnection>(sp =>
    new SqlConnection(builder.Configuration.GetConnectionString("DefaultConnection")));

// Swagger
builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();

builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Masukkan token JWT: Bearer {token}"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Scheme = "bearer",
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// builder.Services.AddAuthentication(options =>
//     {
//         // Default to cookie authentication for the web UI. API controllers can explicitly require JWT.
//         options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
//         options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
//         options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
//     })
//     .AddCookie(options =>
//     {
//         options.LoginPath = "/Auth/Index";
//         options.AccessDeniedPath = "/Auth/Index";
//         options.Cookie.Name = "mdgs_auth";
//         options.Cookie.HttpOnly = true;
//         options.Cookie.SameSite = SameSiteMode.Lax;
//         //options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
//         options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
//         options.ExpireTimeSpan = TimeSpan.FromHours(8);
//     })
//     .AddJwtBearer(options =>
//     {
//         var key = builder.Configuration["Jwt:Key"];
//         if (string.IsNullOrWhiteSpace(key))
//             throw new InvalidOperationException("Configuration 'Jwt:Key' is missing. Please set 'Jwt:Key' in appsettings.json or as an environment variable (Jwt__Key).");

//         options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
//         {
//             ValidateIssuer = true,
//             ValidateAudience = true,
//             ValidateLifetime = true,
//             ValidateIssuerSigningKey = true,
//             ValidIssuer = builder.Configuration["Jwt:Issuer"],
//             ValidAudience = builder.Configuration["Jwt:Audience"],
//             IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(
//                 System.Text.Encoding.UTF8.GetBytes(key))
//         };
//     });

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = "Smart";
    options.DefaultChallengeScheme = "Smart";
})
.AddPolicyScheme("Smart", "JWT or Cookie", options =>
{
    options.ForwardDefaultSelector = context =>
    {
        // API → JWT
        if (context.Request.Path.StartsWithSegments("/api"))
            return JwtBearerDefaults.AuthenticationScheme;

        // Web → Cookie
        return CookieAuthenticationDefaults.AuthenticationScheme;
    };
})
.AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
{
    options.LoginPath = "/Auth/Index";
    options.AccessDeniedPath = "/Auth/Index";
    options.Cookie.Name = "mdgs_auth";
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
})
.AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
{
    var key = builder.Configuration["Jwt:Key"];

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(key!))
    };
});


// File Storage Options
builder.Services.Configure<FileStorageOptions>(
    builder.Configuration.GetSection("FileStorage"));

// Email Options
builder.Services.Configure<EmailOptions>(
    builder.Configuration.GetSection("Email"));

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
