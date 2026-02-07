using Bootstrap;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using System.Windows;
using WpfApp.ViewModels;
using EfDataStorage;
using Microsoft.EntityFrameworkCore;
using TaskApp.Infrastructures;
using TaskApp.AppServices;

namespace WpfApp;
/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private readonly IHost _host;

    public App()
    {
        _host = Host.CreateDefaultBuilder()
            .ConfigureLogging((context, logging) =>
            {                
                logging.ClearProviders();
                logging.AddDebug();
                logging.SetMinimumLevel(LogLevel.Information);
            })
            .ConfigureServices((context, services) =>
            {
                //services.AddApplicationInfrastructure(context.Configuration, context.HostingEnvironment);
                services.RegisterInfrastructures(context.Configuration);
                services.RegisterAppServices();


                services.AddSingleton<MainViewModel>();
                services.AddSingleton<MainWindow>();
            })
            .Build();
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        await _host.StartAsync();

        await InitializeDatabaseAsync(); 

        var mainWindow = _host.Services.GetRequiredService<MainWindow>();
        mainWindow.Show();

        base.OnStartup(e);
    }

    protected override async void OnExit(ExitEventArgs e)
    {        
        using (_host)
        {
            await _host.StopAsync();
        }
        base.OnExit(e);
    }

    private async Task InitializeDatabaseAsync()
    {
        using (var scope = _host.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<TaskApp.Infrastructures.AppDbContext>();
            await db.Database.MigrateAsync();
        }        
    }
}

