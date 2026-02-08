using AutoMapper;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Core.Common;
using Core.Contracts.Services;
using Core.Models;
using Cronos;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using System.IO;
using System.ServiceProcess;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace WpfApp.ViewModels
{
    public class MainWindowViewModel : ObservableObject
    {
        private readonly ISettingsService settingsService;
        private readonly IPatientsService patientService;
        private readonly IMapper mapper;
        private readonly ILogger<MainWindowViewModel> logger;
        private ServiceController serviceController;        
        private DispatcherTimer statusTimer;

        public MainWindowViewModel(ISettingsService settingsService, 
                            IPatientsService patientService, 
                            IMapper mapper,
                            ILogger<MainWindowViewModel> logger)
        {
            this.settingsService = settingsService;
            this.patientService = patientService;
            this.mapper = mapper;
            this.logger = logger;

            RefreshPatientsCommand = new AsyncRelayCommand(OnRefreshPatientsCommand);
            SaveSettingsCommand = new AsyncRelayCommand(OnSaveSettingsCommand);
            StartCommand = new AsyncRelayCommand(OnStartCommand);
            StopCommand = new AsyncRelayCommand(OnStopCommand);
            PauseToggleCommand = new AsyncRelayCommand(OnPauseToggleCommand);
            SelectImportFolderCommand = new AsyncRelayCommand(OnSelectImportFolderCommand);
            SelectExportFolderCommand = new AsyncRelayCommand(OnSelectExportFolderCommand);

            Initializer();            
        }
      
        public SettingsItemViewModel Settings { get; set; } = new SettingsItemViewModel();
        public IReadOnlyList<PatientItemViewModel> Patients { get; set; } = new List<PatientItemViewModel>();        
        public IAsyncRelayCommand StartCommand { get; }
        public IAsyncRelayCommand StopCommand { get; }
        public IAsyncRelayCommand PauseToggleCommand { get; }
        public IAsyncRelayCommand SaveSettingsCommand { get; }
        public IAsyncRelayCommand RefreshPatientsCommand { get; }
        public IAsyncRelayCommand SelectImportFolderCommand { get; }
        public IAsyncRelayCommand SelectExportFolderCommand { get; }
        public const string PauseText = "Pause", ContinueText = "Continue", ErrorStatus = "Not Installed", Paused = "Paused";

        public string PauseButtonText => Settings.IsPaused ? ContinueText : PauseText;        
        public string StatusText { get; set; }


        private async void Initializer()
        {
            await LoadSettingsAsync();
            await LoadPatientsAsync();

            // Polling timer to refresh Service Status every 2 seconds
            statusTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(2) };
            statusTimer.Tick += (s, e) => RefreshServiceStatus();
            statusTimer.Start();

            try
            {
                serviceController = new ServiceController(Constants.ServiceName);                                
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Failed to initialize ServiceController for {Constants.ServiceName}. Ensure the service is installed.");
            }
        }

        private async Task LoadSettingsAsync()
        {
            try
            {                
                var res = await settingsService.GetAsync();
                if (res.Success)
                {
                    this.Settings = mapper.Map<SettingsItemViewModel>(res.Result);
                }                    
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to LoadSettingsAsync()");                
            }
        }

        private async Task LoadPatientsAsync()
        {
            var res = await patientService.GetAllAsync();

            if (res.Success)
            {
                this.Patients = res.Result
                                        .Select(p => mapper.Map<PatientItemViewModel>(p))
                                        .ToList();
            }
            else
            {
                MessageBox.Show($"Failed to load patients: {res.Error?.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task OnRefreshPatientsCommand()
        {
            await LoadPatientsAsync();
        }

        private Task OnStartCommand()
        {
            try
            {
                if (serviceController.Status == ServiceControllerStatus.Stopped)
                {
                    serviceController.Start();
                }
            }
            catch (Exception ex) 
            { 
                logger.LogError(ex, "Failed to start service.");

                MessageBox.Show($"Service '{Constants.ServiceName}' is not available. Please ensure it is installed.", "Service Not Found", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return Task.CompletedTask;
        }

        private Task OnStopCommand()
        {
            try
            {
                if (serviceController.Status == ServiceControllerStatus.Running ||
                    serviceController.Status == ServiceControllerStatus.Paused)
                {
                    serviceController.Stop();
                }
            }
            catch (Exception ex) 
            { 
                logger.LogError(ex, "Failed to stop service.");
                MessageBox.Show($"Service '{Constants.ServiceName}' is not available. Please ensure it is installed.", "Service Not Found", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return Task.CompletedTask;
        }

        private async Task OnPauseToggleCommand()
        {
            // Logic Pause: Update the DB flag
            Settings.IsPaused = !Settings.IsPaused;
            var model = mapper.Map<ServiceSettingsModel>(Settings);
            var res = await settingsService.UpdateAsync(model);

            this.OnPropertyChanged(nameof(PauseButtonText));

            logger.LogInformation($"Service logic set to: {(Settings.IsPaused ? "PAUSED" : "RUNNING")}");
        }

        private async Task OnSaveSettingsCommand()
        {
            logger.LogInformation("SaveSettings();");
            var isValid = this.ValidateSettings();
            if (isValid)
            {
                var model = mapper.Map<ServiceSettingsModel>(Settings);
                var res = await settingsService.UpdateAsync(model);
                if (res.Success)
                {
                    MessageBox.Show("Settings saved successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show($"Failed to save settings: {res.Error?.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private Task OnSelectImportFolderCommand()
        {
            var dialog = new OpenFolderDialog
            {
                Title = "Select Import Folder",
            };

            if (dialog.ShowDialog() == true)
            {
                // Update the property via the Action
                this.Settings.ImportFolder = dialog.FolderName;
            }

            return Task.CompletedTask;
        }

        private Task OnSelectExportFolderCommand()
        {            
            var dialog = new OpenFolderDialog
            {
                Title = "Select Export Folder",                
            };

            if (dialog.ShowDialog() == true)
            {
                // Update the property via the Action
               this.Settings.ExportFolder = dialog.FolderName;
            }

            return Task.CompletedTask;
        }

        private void RefreshServiceStatus()
        {
            try
            {
                if (Settings.IsPaused)
                {
                    StatusText = Paused;
                }               
                else
                {
                    serviceController.Refresh();
                    StatusText = serviceController.Status.ToString();
                }
            }
            catch (Exception ex)
            {
                StatusText = ErrorStatus;
                logger.LogError(ex, ErrorStatus);                
            }
        }

        private bool ValidateSettings()
        {
            if(!IsCronValid(Settings.ImportSchedule))
            {
                MessageBox.Show("Invalid Cron expression for Import Schedule.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (!IsPathValid(Settings.ImportFolder))
            {
                MessageBox.Show("Invalid Import folder path.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (!IsCronValid(Settings.ExportSchedule))
            {
                MessageBox.Show("Invalid Cron expression for Export Schedule.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (!IsPathValid(Settings.ExportFolder))
            {
                MessageBox.Show("Invalid Export folder path.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }

        public bool IsCronValid(string expression)
        {
            if (string.IsNullOrWhiteSpace(expression)) 
                return false;

            try
            {
                // Try to parse the string. If it fails, it's invalid.
                CronExpression.Parse(expression);
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error reading Cron expression");                
            }

            return false;
        }

        public bool IsPathValid(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) 
                return false;

            // Check if it exists or if the drive is valid
            return Directory.Exists(path);
        }
    }
}
