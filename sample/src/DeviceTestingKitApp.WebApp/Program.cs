using DeviceTestingKitApp.WebApp.Components;
using DeviceTestingKitApp.Features;
using DeviceTestingKitApp.ViewModels;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Register view models from the library project (same as MAUI app)
builder.Services.AddTransient<MainViewModel>();
builder.Services.AddTransient<CounterViewModel>();

// Register Blazor-specific semantic announcer
builder.Services.AddTransient<ISemanticAnnouncer, BlazorSemanticAnnouncer>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
