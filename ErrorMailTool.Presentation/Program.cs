using ErrorMailTool.BLL.Services;
using ErrorMailTool.DAL.Data;
using ErrorMailTool.DAL.Repositories;
using ErrorMailTool.DAL.Scanners;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

var backupPath = builder.Configuration["ErrorMail:BackupPath"] ?? @"D:\ErrorMailBackup";
var connectionString = builder.Configuration.GetConnectionString("ErrorMailDb") ??
    "Server=localhost;Database=ErrorMailTool;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True";

builder.Services.AddDbContext<ErrorMailDbContext>(options => options.UseSqlServer(connectionString));
builder.Services.AddSingleton<IErrorMailFileScanner>(_ => new FileSystemErrorMailScanner(backupPath));
builder.Services.AddScoped<IErrorMailRepository, EntityFrameworkErrorMailRepository>();
builder.Services.AddScoped<IErrorMailSyncService, ErrorMailSyncService>();
builder.Services.AddScoped<IErrorMailService>(provider =>
    new ErrorMailService(
        provider.GetRequiredService<IErrorMailRepository>(),
        provider.GetRequiredService<IErrorMailSyncService>(),
        backupPath));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
