using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.JSInterop;
using Eton;
using Eton.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddSingleton<SupabaseService>();
builder.Services.AddSingleton<AuthStateService>();
builder.Services.AddSingleton<SpaceRepository>();
builder.Services.AddSingleton<SpaceStateService>();
builder.Services.AddSingleton<MarkdownRenderer>();
builder.Services.AddSingleton<NoteRepository>();
builder.Services.AddSingleton<CollectionRepository>();
builder.Services.AddSingleton<CollectionItemRepository>();
builder.Services.AddSingleton<ReviewRepository>();
builder.Services.AddSingleton<ExpenseRepository>();
builder.Services.AddSingleton(sp => new RottaRichiesta((IJSInProcessRuntime)sp.GetRequiredService<IJSRuntime>()));

await builder.Build().RunAsync();
