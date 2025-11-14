using DBContext.BMWindows.Entities;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// MVC + Razor (nếu cần Razor Pages thêm AddRazorPages())
builder.Services.AddControllersWithViews();

// DbContext SQL Server
builder.Services.AddDbContext<BMWindowDBContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("BMWindowDB"));
    if (builder.Environment.IsDevelopment())
    {
        options.EnableDetailedErrors();
        options.EnableSensitiveDataLogging();
    }
});

// Services
builder.Services.AddScoped<Service.BMWindows.Executes.Category.CategoryMany>();
builder.Services.AddScoped<Service.BMWindows.Executes.Category.CategoryCommand>();
builder.Services.AddScoped<Service.BMWindows.Executes.Category.CategoryOne>(); 
//builder.Services.AddScoped<Service.BMWindows.Executes.AppItem.AppItemMany>();
//builder.Services.AddScoped<Service.BMWindows.Executes.AppItem.AppItemCommand>();
//builder.Services.AddScoped<Service.BMWindows.Executes.AppItem.AppItemOne>();

var app = builder.Build();

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

// 1) Startup probe: log whether DB is reachable
using (var scope = app.Services.CreateScope())
{
    var ctx = scope.ServiceProvider.GetRequiredService<BMWindowDBContext>();
    try
    {
        var canConnect = await ctx.Database.CanConnectAsync();
        app.Logger.LogInformation("DB CanConnect: {CanConnect}", canConnect);
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "DB connection failed at startup.");
    }
}

// 2) Dev-only debug endpoints
if (app.Environment.IsDevelopment())
{
    app.MapGet("/debug/db-connection", async (BMWindowDBContext ctx) =>
    {
        try
        {
            var canConnect = await ctx.Database.CanConnectAsync();
            var cnn = ctx.Database.GetDbConnection();
            return Results.Ok(new
            {
                canConnect,
                dataSource = cnn.DataSource,
                database = cnn.Database,
                state = cnn.State.ToString()
            });
        }
        catch (Exception ex)
        {
            return Results.Problem(ex.ToString());
        }
    });

    app.MapGet("/debug/category-count", async (BMWindowDBContext ctx) =>
    {
        try
        {
            var count = await ctx.Categories.CountAsync();
            return Results.Ok(new { count });
        }
        catch (Exception ex)
        {
            return Results.Problem(ex.ToString());
        }
    });
}

app.Run();