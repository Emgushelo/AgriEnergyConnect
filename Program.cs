using AgriEnergyConnect.Data;
using AgriEnergyConnect.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Diagnostics.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<User>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = true;
    options.Password.RequiredLength = 6;

    // Lockout settings
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

builder.Services.AddControllersWithViews();

// Configure session
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Add logging
builder.Services.AddLogging();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

// Seed database with sample data
try
{
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        var context = services.GetRequiredService<ApplicationDbContext>();
        var userManager = services.GetRequiredService<UserManager<User>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var logger = services.GetRequiredService<ILogger<Program>>();

        // Ensure database is created
        await context.Database.EnsureCreatedAsync();

        // Create roles
        string[] roles = { "Employee", "Farmer" };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        // Create default employee
        var defaultEmployee = new User
        {
            UserName = "employee@agrienergy.com",
            Email = "employee@agrienergy.com",
            FirstName = "John",
            LastName = "Smith",
            Role = "Employee"
        };

        if (await userManager.FindByEmailAsync(defaultEmployee.Email) == null)
        {
            var result = await userManager.CreateAsync(defaultEmployee, "Employee123!");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(defaultEmployee, "Employee");
                logger.LogInformation("Default employee account created successfully.");

                // Create sample farmers and products
                await CreateSampleData(context, userManager);
            }
        }
    }
}
catch (Exception ex)
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "An error occurred while seeding the database.");
}

app.Run();

async Task CreateSampleData(ApplicationDbContext context, UserManager<User> userManager)
{
    // Create sample farmers
    var farmers = new[]
    {
        new { FirstName = "Mike", LastName = "Johnson", FarmName = "Green Valley Farm", Email = "mike@greenvalley.com" },
        new { FirstName = "Sarah", LastName = "Williams", FarmName = "Sunrise Organics", Email = "sarah@sunrise.com" },
        new { FirstName = "David", LastName = "Brown", FarmName = "Riverbend Acres", Email = "david@riverbend.com" }
    };

    foreach (var farmerData in farmers)
    {
        var farmerUser = new User
        {
            UserName = farmerData.Email,
            Email = farmerData.Email,
            FirstName = farmerData.FirstName,
            LastName = farmerData.LastName,
            Role = "Farmer"
        };

        if (await userManager.FindByEmailAsync(farmerUser.Email) == null)
        {
            var result = await userManager.CreateAsync(farmerUser, "Farmer123!");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(farmerUser, "Farmer");

                var farmer = new Farmer
                {
                    UserId = farmerUser.Id,
                    FarmName = farmerData.FarmName,
                    Email = farmerData.Email,
                    PhoneNumber = "+1-555-0100",
                    Address = "123 Farm Road, Agricultural District"
                };

                context.Farmers.Add(farmer);
                await context.SaveChangesAsync();

                // Create sample products for this farmer
                var products = new[]
                {
                    new Product { Name = "Organic Tomatoes", Category = "Vegetables", ProductionDate = DateTime.Now.AddDays(-10), Price = 2.50m, Quantity = 100, Description = "Fresh organic tomatoes" },
                    new Product { Name = "Sweet Corn", Category = "Grains", ProductionDate = DateTime.Now.AddDays(-5), Price = 1.80m, Quantity = 200, Description = "Sweet yellow corn" },
                    new Product { Name = "Carrots", Category = "Vegetables", ProductionDate = DateTime.Now.AddDays(-7), Price = 1.20m, Quantity = 150, Description = "Fresh carrots" }
                };

                foreach (var product in products)
                {
                    product.FarmerId = farmer.FarmerId;
                    context.Products.Add(product);
                }

                await context.SaveChangesAsync();
            }
        }
    }
}