using Bootstrap;
using Microsoft.EntityFrameworkCore;
using WorkerService;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddWindowsService(options =>
{
    options.ServiceName = "PatientsSyncService";
});

builder.Services.AddApplicationInfrastructure(builder.Configuration, builder.Environment);

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
