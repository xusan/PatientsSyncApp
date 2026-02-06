using Core.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private readonly ISettingsRepository settingsRepository;
        private readonly IPatientRepository patientRepository;

        public MainViewModel(ISettingsRepository settingsRepository, IPatientRepository patientRepository)
        {
            this.settingsRepository = settingsRepository;
            this.patientRepository = patientRepository;
        }
    }
}
