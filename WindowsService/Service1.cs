using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WindowsService
{
    public partial class Service1 : ServiceBase
    {
        private IServiceProvider _serviceProvider;
        private CancellationTokenSource _cts;
        private Task _syncTask;
        private bool _isPaused = false;

        public Service1()
        {
            InitializeComponent();

            this.CanPauseAndContinue = true; // Enables the 'Pause' button in services.msc
            this.ServiceName = "PatientSyncService";
        }

        protected override void OnStart(string[] args)
        {
            // 1. Setup Configuration (read appsettings.json)
            var configuration = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            // 2. Build the Service Collection
            var services = new ServiceCollection();

            // 3. Use your existing Bootstrap extension!
            // Note: For 'environment', we can pass a dummy or mock for a Windows Service
            services.AddApplicationInfrastructure(configuration, new Microsoft.Extensions.Hosting.Internal.HostingEnvironment());

            _serviceProvider = services.BuildServiceProvider();

            // 4. Start the Background Loop
            _cts = new CancellationTokenSource();
            _syncTask = Task.Run(() => RunSyncLoop(_cts.Token), _cts.Token);
        }

        private async Task RunSyncLoop(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                if (!_isPaused)
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var syncService = scope.ServiceProvider.GetRequiredService<ISyncService>();
                        var settingsRepo = scope.ServiceProvider.GetRequiredService<ISettingsRepository>();

                        var settings = await settingsRepo.GetSettingsAsync();

                        // Logic using Cronos to check if it's time to run
                        // await syncService.ProcessInboundFiles(settings.InboxFolder);
                    }
                }

                // Check every minute
                await Task.Delay(TimeSpan.FromMinutes(1), token);
            }
        }

        protected override void OnPause() { _isPaused = true; }
        protected override void OnContinue() { _isPaused = false; }

        protected override void OnStop()
        {
            _cts.Cancel();
            _syncTask.Wait();
        }
    }
}
