using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Blocktrust.CredentialBuilder.Client;
using Blocktrust.CredentialBuilder.Client.Services;
using MudBlazor.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddSingleton<IConnectionService, ConnectionService>();
builder.Services.AddSingleton<IAgentService, AgentService>();
builder.Services.AddSingleton<IStorageService, StorageService>();
builder.Services.AddSingleton<IDidService, DidService>();
builder.Services.AddSingleton<ICredentialIssuingService, CredentialIssuingService>();
builder.Services.AddBlazoredLocalStorageAsSingleton();
builder.Services.AddMudServices();

await builder.Build().RunAsync();