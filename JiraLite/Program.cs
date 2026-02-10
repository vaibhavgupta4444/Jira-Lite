using System.Text;
using JiraLite.Application.Interfaces;
using JiraLite.Application.Services;
using JiraLite.Infrastructure.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Database configuration
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        ServerVersion.AutoDetect(
            builder.Configuration.GetConnectionString("DefaultConnection")
        )
    );
});

// Register services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IIssueService, IssueService>();
builder.Services.AddScoped<IWorkflowService, WorkflowService>();

// Add session support
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(2);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// JWT Authentication Configuration
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"];

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey!))
    };
});

// CORS configuration for frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddControllersWithViews();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "JiraLite API",
        Version = "v1",
        Description = "JiraLite Issue Tracking System API",
        Contact = new OpenApiContact
        {
            Name = "JiraLite",
            Email = "support@jiralite.com"
        }
    });

    // Add JWT Authentication to Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token in the text input below. Example: 'Bearer 12345abcdef'",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
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



var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "JiraLite API V1");
        c.RoutePrefix = "swagger"; // Access Swagger at /swagger
    });
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Enable Session
app.UseSession();

// Enable CORS
app.UseCors("AllowAll");

// Enable Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    
    if (!context.WorkflowTransitions.Any())
    {
        var systemUserId = Guid.Empty;
        var now = DateTime.UtcNow;
        
        var defaultTransitions = new[]
        {
            new JiraLite.Models.WorkflowTransition
            {
                FromStatus = JiraLite.Domain.Enums.IssueStatus.Open,
                ToStatus = JiraLite.Domain.Enums.IssueStatus.InProgress,
                IsActive = true,
                Created = now,
                Updated = now,
                CreatedBy = systemUserId,
                UpdatedBy = systemUserId
            },
            new JiraLite.Models.WorkflowTransition
            {
                FromStatus = JiraLite.Domain.Enums.IssueStatus.InProgress,
                ToStatus = JiraLite.Domain.Enums.IssueStatus.Closed,
                IsActive = true,
                Created = now,
                Updated = now,
                CreatedBy = systemUserId,
                UpdatedBy = systemUserId
            },
            new JiraLite.Models.WorkflowTransition
            {
                FromStatus = JiraLite.Domain.Enums.IssueStatus.Closed,
                ToStatus = JiraLite.Domain.Enums.IssueStatus.Reopened,
                IsActive = true,
                Created = now,
                Updated = now,
                CreatedBy = systemUserId,
                UpdatedBy = systemUserId
            },
            new JiraLite.Models.WorkflowTransition
            {
                FromStatus = JiraLite.Domain.Enums.IssueStatus.Reopened,
                ToStatus = JiraLite.Domain.Enums.IssueStatus.InProgress,
                IsActive = true,
                Created = now,
                Updated = now,
                CreatedBy = systemUserId,
                UpdatedBy = systemUserId
            }
        };
        
        context.WorkflowTransitions.AddRange(defaultTransitions);
        context.SaveChanges();
    }
}

app.Run();
