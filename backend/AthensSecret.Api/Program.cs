using AthensSecret.Api.Data;
using AthensSecret.Api.Middleware;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
var connectionString = Environment.GetEnvironmentVariable("DATABASE_URL") 
    ?? builder.Configuration.GetConnectionString("DefaultConnection");

// Convert PostgreSQL URL to connection string if needed
if (connectionString?.StartsWith("postgresql://") == true)
{
    var uri = new Uri(connectionString);
    var port = uri.Port == -1 ? 5432 : uri.Port; // Default PostgreSQL port if not specified
    connectionString = $"Host={uri.Host};Port={port};Database={uri.AbsolutePath.Trim('/')};Username={uri.UserInfo.Split(':')[0]};Password={uri.UserInfo.Split(':')[1]};SslMode=Require;TrustServerCertificate=true";
}

builder.Services.AddDbContext<ApiDbContext>(options =>
    options.UseNpgsql(connectionString));

// Configure admin security
builder.Services.Configure<AdminSecurityOptions>(builder.Configuration.GetSection(AdminSecurityOptions.SectionName));

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Keep PascalCase property names in JSON responses
        options.JsonSerializerOptions.PropertyNamingPolicy = null;
    });

// Add rate limiting
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("ApiPolicy", opt =>
    {
        opt.PermitLimit = 10; // 10 requests
        opt.Window = TimeSpan.FromMinutes(1); // per minute
        opt.QueueProcessingOrder = System.Threading.RateLimiting.QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = 2;
    });
});

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowedOrigins", policy =>
    {
        if (builder.Environment.IsDevelopment())
        {
            // Development: Allow common dev origins
            policy.WithOrigins("http://localhost:3000", "http://localhost:7182", "http://localhost:8080","http://localhost:5182" )
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .AllowCredentials();
        }
        else
        {
            // Production: Restrict to deployed frontend origin
            policy.WithOrigins("https://athens-secret-api.onrender.com")
                  .WithMethods("GET", "POST")
                  .WithHeaders("Content-Type", "Authorization");
        }
    });
});

// Add security headers
builder.Services.AddAntiforgery();
builder.Services.AddHsts(options =>
{
    options.Preload = true;
    options.IncludeSubDomains = true;
    options.MaxAge = TimeSpan.FromDays(365);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Security middleware
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
    app.UseHsts();
}

// Add security headers
app.Use(async (context, next) =>
{
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    context.Response.Headers["X-Frame-Options"] = "DENY";
    context.Response.Headers["X-XSS-Protection"] = "1; mode=block";
    context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
    
    if (!app.Environment.IsDevelopment())
    {
        context.Response.Headers["Content-Security-Policy"] = 
            "default-src 'self'; script-src 'self' 'unsafe-inline'; style-src 'self' 'unsafe-inline' https://cdnjs.cloudflare.com; font-src 'self' https://cdnjs.cloudflare.com; img-src 'self' data:";
    }
    
    await next();
});

// Use CORS policy
app.UseCors("AllowedOrigins");

// Enable admin security middleware
app.UseAdminSecurity();

// Enable rate limiting
app.UseRateLimiter();

// Enable static files and default files
app.UseDefaultFiles();
app.UseStaticFiles();

app.MapControllers();

app.Run();
