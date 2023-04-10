using GhostInTheShell.Modules.InfraStructure;
using GhostInTheShell.Modules.ShellInfra;
using GhostInTheShell.Servers.Shell;
using GhostInTheShell.Servers.Shell.Services;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Prism.Events;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<HttpClient>();
builder.Services.AddSingleton<IEventAggregator, EventAggregator>();
builder.Services.AddScoped<IShellModelFactory, ShellModelLocalFactory>();
builder.Services.AddSingleton<IShellMaterialFactory, ShellMaterialLocalFactory>();
builder.Services.AddSingleton<IFuminoCharacterLocalService, FuminoCharacterLocalService>(prov =>
{
    ILogger<FuminoCharacterLocalService> logger = prov.GetRequiredService<ILogger<FuminoCharacterLocalService>>();
    IConfiguration config = prov.GetRequiredService<IConfiguration>();

    IShellModelFactory modelFac = prov.GetRequiredService<IShellModelFactory>();
    modelFac.InitializeAsync(ShellNames.Fumino).Await();

    IShellMaterialFactory materialFac = prov.GetRequiredService<IShellMaterialFactory>();

    FuminoCharacterLocalService fuminoCharSvc = new FuminoCharacterLocalService(logger, config, modelFac, materialFac);
    return fuminoCharSvc;
});
builder.Services.AddSingleton<IKaoriCharacterLocalService, KaoriCharacterLocalService>(prov =>
{
    ILogger<KaoriCharacterLocalService> logger = prov.GetRequiredService<ILogger<KaoriCharacterLocalService>>();
    IConfiguration config = prov.GetRequiredService<IConfiguration>();

    IShellModelFactory modelFac = prov.GetRequiredService<IShellModelFactory>();
    modelFac.InitializeAsync(ShellNames.Kaori).Await();

    IShellMaterialFactory materialFac = prov.GetRequiredService<IShellMaterialFactory>();

    KaoriCharacterLocalService kaoriCharSvc = new KaoriCharacterLocalService(logger, config, modelFac, materialFac);
    return kaoriCharSvc;
});

//if(builder.Environment.IsDevelopment())
{
    builder.Services.AddGrpcReflection();
}

builder.Services.AddGrpc();

if(!builder.Environment.IsDevelopment())
{
    builder.WebHost.ConfigureKestrel((context, options) =>
    {
        int port = context.Configuration.GetValue<int>("CustomKestrelSettings:Port", 444);
        options.ListenAnyIP(port, listenOptions =>
        {
            var isTlsEnabled = context.Configuration.GetValue<bool>("CustomKestrelSettings:TLS-Enabled", true);
            Console.WriteLine($"Enabling connection encryption({port}): {isTlsEnabled}");

            if (isTlsEnabled)
            {
                string? certPath = context.Configuration.GetValue<string>("CustomKestrelSettings:CertPath");
                string? certPass = context.Configuration.GetValue<string>("CustomKestrelSettings:CertPass");

                if (String.IsNullOrEmpty(certPath))
                    throw new NullReferenceException($"NullRef: {nameof(certPath)}");
                if (String.IsNullOrEmpty(certPass))
                    throw new NullReferenceException($"NullRef: {nameof(certPass)}");

                listenOptions.UseHttps(certPath, certPass);
            }


            listenOptions.Protocols = HttpProtocols.Http2;
        });
    });
}


var app = builder.Build();

bool isKaoriCharSvcReady = await app.Services.GetRequiredService<IKaoriCharacterLocalService>().InitializeAsync();
bool isFuminoCharSvcReady = await app.Services.GetRequiredService<IFuminoCharacterLocalService>().InitializeAsync();


if (!isKaoriCharSvcReady)
{
    app.Logger.Log(LogLevel.Error, $"FailInitialize: {nameof(IKaoriCharacterLocalService)}, Enter to Exit.");
    Console.ReadLine();
}
else if (!isFuminoCharSvcReady)
{
    app.Logger.Log(LogLevel.Error, $"FailInitialize: {nameof(IFuminoCharacterLocalService)}, Enter to Exit.");
    Console.ReadLine();
}

Console.WriteLine("Initialized: CharacterLocalServices.");
Console.WriteLine("CharacterServices is Ready.");

//if(app.Environment.IsDevelopment())
{
    app.MapGrpcReflectionService();
}

// Configure the HTTP request pipeline.
app.MapGrpcService<CharacterServerService>();
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");
app.Run();