using GhostInTheShell.Modules.ShellInfra;
using GhostInTheShell.Servers.Shell;
using GhostInTheShell.Servers.Shell.Services;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Prism.Events;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<HttpClient>();
builder.Services.AddSingleton<IEventAggregator, EventAggregator>();
builder.Services.AddSingleton<IShellModelFactory, ShellModelLocalFactory>();
builder.Services.AddSingleton<IShellMaterialFactory, ShellMaterialLocalFactory>();
builder.Services.AddSingleton<ICharacterLocalService, CharacterLocalService>();

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

bool isModelFactoryReady = await app.Services
    .GetRequiredService<IShellModelFactory>()
    .InitializeAsync("Kaori");

if (!isModelFactoryReady)
{
    app.Logger.Log(LogLevel.Error, $"FailInitialize: {nameof(ShellModelLocalFactory)}, Enter to Exit.");
    Console.ReadLine();
}

Console.WriteLine("Initialized: ModelFactory.");

var charLocalSvc = app.Services.GetRequiredService<ICharacterLocalService>();
bool isCharSvcReady = await charLocalSvc.InitializeAsync("Kaori");

if (!isCharSvcReady)
{
    string msg = $"FailInitialize {nameof(CharacterLocalService)}";

    app.Logger.LogError(msg);
    Console.WriteLine(msg);

    Console.Write("Enter to exit.");
    Console.ReadLine();
}
else
{
    Console.WriteLine("CharacterService is Ready.");

    //if(app.Environment.IsDevelopment())
    {
        app.MapGrpcReflectionService();
    }

    // Configure the HTTP request pipeline.
    app.MapGrpcService<CharacterServerService>();
    app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");
    app.Run();
}