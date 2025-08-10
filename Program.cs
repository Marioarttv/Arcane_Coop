using Arcane_Coop.Components;
using Arcane_Coop.Hubs;
using Arcane_Coop.Data;
using Arcane_Coop.Services;
using Microsoft.EntityFrameworkCore;
using Plk.Blazor.DragDrop;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Add SignalR
builder.Services.AddSignalR();

// Add Blazor DragDrop
builder.Services.AddBlazorDragDrop();

// Add Entity Framework
builder.Services.AddDbContext<GameDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") ?? 
                     "Data Source=arcane_coop.db"));

// Add Visual Novel Service
builder.Services.AddScoped<IVisualNovelService, VisualNovelService>();
// Add Act 1 Story Engine
builder.Services.AddSingleton<Arcane_Coop.Services.IAct1StoryEngine, Arcane_Coop.Services.Act1StoryEngine>();

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

// Map SignalR hubs
app.MapHub<GameHub>("/gamehub");

app.Run();
