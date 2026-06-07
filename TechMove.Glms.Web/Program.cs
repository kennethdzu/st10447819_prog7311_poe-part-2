using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TechMove.Glms.Web.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddSession();

// Configure HttpClient to call the API
builder.Services.AddHttpClient("ApiClient", client =>
{
    // The API URL will be configured via environment variable in Docker, fallback to localhost
    var apiUrl = builder.Configuration["ApiBaseUrl"] ?? "http://localhost:5269/";
    client.BaseAddress = new System.Uri(apiUrl);
});

builder.Services.AddHttpClient<ICurrencyConversionStrategy, LiveApiConversionStrategy>();

// Register ContractFactory so MVC controller can activate Contracts
builder.Services.AddScoped<IContractFactory, ContractFactory>();

// PDF validator remains in frontend for pre-upload checks
builder.Services.AddScoped<FileValidationService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
