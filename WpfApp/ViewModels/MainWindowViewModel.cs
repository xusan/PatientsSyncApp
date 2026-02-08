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

            RefreshPatientsCommand = new RelayCommand(OnRefreshPatientsCommand);
            SaveSettingsCommand = new RelayCommand(OnSaveSettingsCommand);
            StartCommand = new RelayCommand(OnStartCommand);
            StopCommand = new RelayCommand(OnStopCommand);
            PauseToggleCommand = new RelayCommand(OnPauseToggleCommand);
            SelectImportFolderCommand = new RelayCommand(OnSelectImportFolderCommand);
            SelectExportFolderCommand = new RelayCommand(OnSelectExportFolderCommand);

            Initializer();            
        }
      
        public ServiceSettingsViewModel Settings { get; set; } = new ServiceSettingsViewModel();
        public IReadOnlyList<PatientViewModel> Patients { get; set; } = new List<PatientViewModel>();        
        public ICommand StartCommand { get; }
        public ICommand StopCommand { get; }
        public ICommand PauseToggleCommand { get; }
        public ICommand SaveSettingsCommand { get; }
        public ICommand RefreshPatientsCommand { get; }
        public ICommand SelectImportFolderCommand { get; }
        public ICommand SelectExportFolderCommand { get; }
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
                    this.Settings = mapper.Map<ServiceSettingsViewModel>(res.Result);
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
                                        .Select(p => mapper.Map<PatientViewModel>(p))
                                        .ToList();
            }
            else
            {
                MessageBox.Show($"Failed to load patients: {res.Error?.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void OnRefreshPatientsCommand()
        {
            await LoadPatientsAsync();
        }

        private void OnStartCommand()
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
        }

        private void OnStopCommand()
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
        }

        private async void OnPauseToggleCommand()
        {
            // Logic Pause: Update the DB flag
            Settings.IsPaused = !Settings.IsPaused;
            var model = mapper.Map<ServiceSettingsModel>(Settings);
            var res = await settingsService.UpdateAsync(model);

            this.OnPropertyChanged(nameof(PauseButtonText));

            logger.LogInformation($"Service logic set to: {(Settings.IsPaused ? "PAUSED" : "RUNNING")}");
        }

        private async void OnSaveSettingsCommand()
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

        private void OnSelectImportFolderCommand()
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
        }

        private void OnSelectExportFolderCommand()
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
