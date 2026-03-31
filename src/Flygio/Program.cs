using Flygio.Components;
using Flygio.Data;
using Flygio.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddDbContext<FlygioDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddHealthChecks()
    .AddDbContextCheck<FlygioDbContext>();

// Amadeus API
builder.Services.Configure<AmadeusSettings>(builder.Configuration.GetSection(AmadeusSettings.SectionName));
builder.Services.AddMemoryCache();
builder.Services.AddHttpClient<AmadeusTokenService>();
builder.Services.AddSingleton<AmadeusTokenService>();
builder.Services.AddHttpClient<AmadeusFlightSearchService>();
builder.Services.AddScoped<AmadeusFlightSearchService>();

var app = builder.Build();

// Auto-migrate in production
if (!app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<FlygioDbContext>();
    await db.Database.MigrateAsync();

    app.UseExceptionHandler("/Error", createScopeForErrors: true);
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseAntiforgery();

app.MapHealthChecks("/healthz");
app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
