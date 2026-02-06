using EfDataStorage;
using Services;
using WorkerService;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddWindowsService(options =>
{
    options.ServiceName = "PatientsSyncService";
});

// Register your DbContext (e.g., from EfDataStorage/DependencyInjection.cs)
builder.Services.AddPersistence(builder.Configuration, builder.Environment);

// Register your application services (e.g., from Services/DependencyInjection.cs)
builder.Services.AddServices();

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
