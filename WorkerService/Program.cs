using Bootstrap;
using Core.Common;
using WorkerService;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddWindowsService(options =>
{
    options.ServiceName = Constants.ServiceName;
});

builder.Services.AddApplicationInfrastructure(builder.Configuration, builder.Environment);

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
