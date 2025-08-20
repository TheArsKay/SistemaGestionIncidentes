var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddSession();

// HttpClient para consumir la API
builder.Services.AddHttpClient("Api", client =>
{
    // "Services:URL" debe estar en appsettings.json del WebApp (ej: "https://localhost:7162/api/")
    client.BaseAddress = new Uri(builder.Configuration["Services:URL"]);
});

var app = builder.Build();

// rest of your existing setup...
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
