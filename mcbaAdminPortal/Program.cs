using Microsoft.AspNetCore.Mvc.ViewFeatures;   // <-- add this


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

// HttpClient for API calls
builder.Services.AddHttpClient();

// Cookie-based TempData (for success/error messages)
builder.Services.AddSingleton<ITempDataProvider, CookieTempDataProvider>();

// Session to store JWT
builder.Services.AddSession(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();      // safe even if portal is on http
app.UseStaticFiles();
app.UseRouting();

app.UseSession();               // <-- IMPORTANT: enable session BEFORE auth
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auth}/{action=Login}/{id?}");

app.Run();
