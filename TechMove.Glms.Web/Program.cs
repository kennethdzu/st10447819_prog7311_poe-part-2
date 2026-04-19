using Microsoft.EntityFrameworkCore;
using TechMove.Glms.Web.Data;
using TechMove.Glms.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// Setup MVC
builder.Services.AddControllersWithViews();

// Setup database
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Setup currency API
builder.Services.AddHttpClient<ICurrencyConversionStrategy, LiveApiConversionStrategy>();

// Register services
// Contract factory
builder.Services.AddScoped<IContractFactory, ContractFactory>();

// Workflow rules
builder.Services.AddScoped<IContractWorkflowService, ContractWorkflowService>();

// PDF validator
builder.Services.AddScoped<FileValidationService>();

var app = builder.Build();

// Setup HTTP pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
