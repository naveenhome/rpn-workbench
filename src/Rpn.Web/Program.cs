using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Rpn.Core;
using Rpn.Data;
using Rpn.Web.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddDefaultIdentity<IdentityUser>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequiredLength = 8;
    })
    .AddEntityFrameworkStores<AppDbContext>();

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

builder.Services.AddSingleton<OperatorRegistry>();
builder.Services.AddSingleton<Evaluator>();
builder.Services.AddScoped<HistoryRepository>();
builder.Services.AddScoped<HistoryService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    context.Database.EnsureCreated();

    // Two known accounts, in development only, so the history views have more
    // than one owner to look at. A course exercise that needs two signed-in
    // users should not begin with ten pairs registering twenty accounts by
    // hand through the Identity UI.
    if (app.Environment.IsDevelopment())
    {
        var users = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
        foreach (var email in new[] { "alice@example.com", "bob@example.com" })
        {
            if (await users.FindByEmailAsync(email) is not null) continue;
            var user = new IdentityUser { UserName = email, Email = email, EmailConfirmed = true };
            var result = await users.CreateAsync(user, "Workshop123!");
            if (!result.Succeeded)
            {
                throw new InvalidOperationException(
                    $"could not seed {email}: "
                    + string.Join("; ", result.Errors.Select(e => e.Description)));
            }
        }
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();

/// <summary>
/// Exposed so the integration tests can host this application in memory.
/// Top-level statements compile to an internal Program, which
/// WebApplicationFactory&lt;Program&gt; cannot reach.
/// </summary>
public partial class Program { }
