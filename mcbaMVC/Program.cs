using mcbaMVC.Data;
using mcbaMVC.Services;            // BillPayProcessor
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ---------------- DB ----------------
builder.Services.AddDbContext<MCBAContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("MCBAConnection")));

// -------------- MVC / Session --------------
builder.Services.AddControllersWithViews();

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(opts =>
{
    opts.IdleTimeout = TimeSpan.FromMinutes(20);
    opts.Cookie.HttpOnly = true;
    opts.Cookie.IsEssential = true;
});

builder.Services.AddHttpContextAccessor();

// -------------- Background worker --------------
builder.Services.AddHostedService<BillPayProcessor>();

var app = builder.Build();

// -------------- Pipeline --------------
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();       // must be before Session/Auth
app.UseSession();       // Session BEFORE Authorization so guards can read it
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Login}/{action=Index}/{id?}")
    .WithStaticAssets();

// -------------- Seed data --------------
using (var scope = app.Services.CreateScope())
{
    var sp = scope.ServiceProvider;
    var db = sp.GetRequiredService<MCBAContext>();

    // If you want automatic migrations on startup (optional):
    // db.Database.Migrate();

    using var http = new HttpClient();
    var seeder = new DataSeeder(db, http);

    await seeder.SeedCustomersAsync();  // JSON feed
    await seeder.SeedPayeesAsync();     // Rob/Bob/Hob for BillPay dropdown
}

app.Run();
