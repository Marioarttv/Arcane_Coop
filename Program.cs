using Arcane_Coop.Components;
using Arcane_Coop.Hubs;
using Arcane_Coop.Services;
using KristofferStrube.Blazor.MediaCaptureStreams;
using Plk.Blazor.DragDrop;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddMediaDevicesService();
// Add SignalR
builder.Services.AddSignalR();

// Add Blazor DragDrop
builder.Services.AddBlazorDragDrop();

// Add Visual Novel Service
builder.Services.AddScoped<IVisualNovelService, VisualNovelService>();
// Add Act 1 Story Engine
builder.Services.AddSingleton<IAct1StoryEngine, Act1StoryEngine>();
// Add Audio Manager Service
builder.Services.AddScoped<IAudioManager, AudioManager>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Map SignalR hubs
app.MapHub<GameHub>("/gamehub");

app.Run();
