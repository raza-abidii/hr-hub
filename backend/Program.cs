using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using EMSSolution.DataAccess;
using EMSSolution.LoggingService;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using System;
using System.Configuration;
using System.Data.Entity.Core.Metadata.Edm;
using System.Net;

var builder = WebApplication.CreateBuilder(args);

// Add CORS policy for frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5175", "http://localhost:8080", "http://localhost:3000")
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

// Add services to the container.
// Configure as API-only (no MVC Views)
builder.Services.AddControllers();
builder.Services.AddRazorPages();

builder.Services.AddSession();
builder.Services.AddDbContext<ApplicationDBContext>(
    options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

//builder.Services.AddScoped<ApplicationDBContext>(provider =>
//{
//    var httpContext = provider.GetService<IHttpContextAccessor>()?.HttpContext;
//    var connString = httpContext?.Session.GetString("ConnString");

//    if (string.IsNullOrEmpty(connString))
//        connString = builder.Configuration.GetConnectionString("DefaultConnection");

//    var optionsBuilder = new DbContextOptionsBuilder<ApplicationDBContext>();
//    optionsBuilder.UseSqlServer(connString);

//    return new ApplicationDBContext(optionsBuilder.Options);
//});

builder.Services.AddHttpClient();
builder.Services.AddSingleton<WebClient>();

builder.Services.AddScoped<IUserActivityLogger, UserActivityLogger>();
builder.Services.AddHttpContextAccessor();

builder.Services.AddAuthorization();
//builder.Services.AddAuthentication("Cookies")
//    .AddCookie("Cookies", options =>
//    {
//        options.LoginPath = "/Account/Login";
//    });

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(15);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/api/error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseCors("AllowFrontend");

app.UseRouting();

app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

