using EfDataStorage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Services;
using System.Windows;
using WpfApp.ViewModels;

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
            .ConfigureServices((context, services) =>
            {
                // Register your DbContext (e.g., from EfDataStorage/DependencyInjection.cs)
                services.AddPersistence(context.Configuration, context.HostingEnvironment);

                // Register your application services (e.g., from Services/DependencyInjection.cs)
                services.AddServices();

                // Register the ViewModel
                services.AddSingleton<MainViewModel>();

                // Register your Windows (WPF needs this to inject them)
                services.AddSingleton<MainWindow>();
            })
            .Build();
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        await _host.StartAsync();
     
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
}

