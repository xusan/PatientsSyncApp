using AutoMapper;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Core;
using Core.Contracts;
using Core.Models;
using EfDataStorage.Entities;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
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
        private readonly Lazy<ISettingsRepository> settingsRepo;
        private readonly Lazy<IPatientRepository> patientRepo;
        private readonly Lazy<IMapper> mapper;
        private readonly Lazy<ILogger<MainViewModel>> logger;
        private ServiceController serviceController;
        private bool isServiceAvaiable;
        private DispatcherTimer statusTimer;

        public MainViewModel(Lazy<ISettingsRepository> settingsRepo, 
                            Lazy<IPatientRepository> patientRepo, 
                            Lazy<IMapper> mapper,
                            Lazy<ILogger<MainViewModel>> logger)
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

            Initializer();            
        }

        public ServiceSettingsViewModel Settings { get; set; } = new ServiceSettingsViewModel();
        public IReadOnlyList<PatientModel> Patients { get; set; } = new List<PatientModel>();
        public string StatusText { get; set;  }        
        public ICommand StartCommand { get; }
        public ICommand StopCommand { get; }
        public ICommand PauseToggleCommand { get; }
        public ICommand SaveSettingsCommand { get; }
        public ICommand RefreshPatientsCommand { get; }

        private void Initializer()
        {
            LoadSettings();
            LoadPatients();

            // Polling timer to refresh Service Status every 2 seconds
            statusTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(2) };
            statusTimer.Tick += (s, e) => RefreshServiceStatus();
            statusTimer.Start();

            try
            {
                serviceController = new ServiceController(Constants.ServiceName);
                isServiceAvaiable = true;
            }
            catch (Exception ex)
            {
                logger.Value.LogError(ex, $"Failed to initialize ServiceController for {Constants.ServiceName}. Ensure the service is installed.");
            }
        }

        private async void LoadSettings()
        {
            try
            {
                logger.Value.LogInformation("LoadSettings()");
                var res = await settingsRepo.Value.GetAsync();
                this.Settings = mapper.Value.Map<ServiceSettingsViewModel>(res.Result);
            }
            catch (Exception ex)
            {
                logger.Value.LogError(ex, "Failed to LoadSettings()");                
            }
        }

        private async void LoadPatients()
        {
            try
            {
                logger.Value.LogInformation("LoadPatients()");
                var res = await patientRepo.Value.GetAllAsync();
                this.Patients = res.Result;
            }
            catch (Exception ex)
            {
                logger.Value.LogError(ex, "Failed to LoadPatients()");
            }
        }

        private void OnRefreshPatientsCommand()
        {
            LoadPatients();
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
                logger.Value.LogError(ex, "Failed to start service. Ensure App is Admin."); 
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
                logger.Value.LogError(ex, "Failed to stop service."); 
            }
        }

        private async void OnPauseToggleCommand()
        {
            // Logic Pause: Update the DB flag
            Settings.IsPaused = !Settings.IsPaused;
            OnSaveSettingsCommand();

            logger.Value.LogInformation($"Service logic set to: {(Settings.IsPaused ? "PAUSED" : "RUNNING")}");
        }

        private async void OnSaveSettingsCommand()
        {
            logger.Value.LogInformation("SaveSettings()");
            var model = mapper.Value.Map<ServiceSettingsModel>(Settings);
            await settingsRepo.Value.UpdateAsync(model);            
        }

        private void RefreshServiceStatus()
        {
            try
            {
                if (isServiceAvaiable)
                {
                    serviceController.Refresh();
                    StatusText = serviceController.Status.ToString();
                }
            }
            catch (Exception ex)
            {
                StatusText = "Error reading status";
                logger.Value.LogError(ex, "Error reading status");                
            }
        }
    }
}
