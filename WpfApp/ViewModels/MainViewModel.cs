using AutoMapper;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Core;
using Core.Contracts;
using Core.Models;
using Cronos;
using EfDataStorage.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace WpfApp.ViewModels
{
    public class MainViewModel : ObservableObject
    {
        private readonly ISettingsRepository settingsRepo;
        private readonly IPatientRepository patientRepo;
        private readonly IMapper mapper;
        private readonly ILogger<MainViewModel> logger;
        private ServiceController serviceController;
        private bool isServiceAvaiable;
        private DispatcherTimer statusTimer;

        public MainViewModel(ISettingsRepository settingsRepo, 
                            IPatientRepository patientRepo, 
                            IMapper mapper,
                            ILogger<MainViewModel> logger)
        {
            this.settingsRepo = settingsRepo;
            this.patientRepo = patientRepo;
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
        public IReadOnlyList<PatientModel> Patients { get; set; } = new List<PatientModel>();        
        public ICommand StartCommand { get; }
        public ICommand StopCommand { get; }
        public ICommand PauseToggleCommand { get; }
        public ICommand SaveSettingsCommand { get; }
        public ICommand RefreshPatientsCommand { get; }
        public ICommand SelectImportFolderCommand { get; }
        public ICommand SelectExportFolderCommand { get; }
        public const string PauseText = "Pause", ContinueText = "Continue", ErrorReadingStatus = "Error reading status", Paused = "Paused";

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
                // Accessing any property (like DisplayName) forces the controller 
                // to check if the service actually exists in Windows.
                var name = serviceController.DisplayName;
                isServiceAvaiable = true;
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
                var res = await settingsRepo.GetAsync();
                this.Settings = mapper.Map<ServiceSettingsViewModel>(res.Result);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to LoadSettingsAsync()");                
            }
        }

        private async Task LoadPatientsAsync()
        {
            try
            {                
                var res = await patientRepo.GetAllAsync();
                this.Patients = res.Result;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to LoadPatientsAsync()");
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
                if (!isServiceAvaiable)
                {
                    MessageBox.Show($"Service '{Constants.ServiceName}' is not available. Please ensure it is installed.", "Service Not Found", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else if (serviceController.Status == ServiceControllerStatus.Stopped)
                {
                    serviceController.Start();
                }
            }
            catch (Exception ex) 
            { 
                logger.LogError(ex, "Failed to start service."); 
            }
        }

        private void OnStopCommand()
        {
            try
            {
                if (!isServiceAvaiable)
                {
                    MessageBox.Show($"Service '{Constants.ServiceName}' is not available. Please ensure it is installed.", "Service Not Found", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else if (serviceController.Status == ServiceControllerStatus.Running ||
                    serviceController.Status == ServiceControllerStatus.Paused)
                {
                    serviceController.Stop();
                }
            }
            catch (Exception ex) 
            { 
                logger.LogError(ex, "Failed to stop service."); 
            }
        }

        private async void OnPauseToggleCommand()
        {
            // Logic Pause: Update the DB flag
            Settings.IsPaused = !Settings.IsPaused;
            var model = mapper.Map<ServiceSettingsModel>(Settings);
            var res = await settingsRepo.UpdateAsync(model);

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
                var res = await settingsRepo.UpdateAsync(model);
                if (res.Success)
                {
                    MessageBox.Show("Settings saved successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show($"Failed to save settings: {res.Msg}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
                else if (isServiceAvaiable)
                {
                    serviceController.Refresh();
                    StatusText = serviceController.Status.ToString();
                }
                else
                {
                    StatusText = ErrorReadingStatus;
                }
            }
            catch (Exception ex)
            {
                StatusText = ErrorReadingStatus;
                logger.LogError(ex, ErrorReadingStatus);                
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
