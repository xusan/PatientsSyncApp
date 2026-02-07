using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskApp.Domain.Patients;
using TaskApp.Domain.Settings;

namespace TaskApp.AppServices.Dto
{
    internal class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Patient, PatientModel>().ReverseMap();
            CreateMap<ServiceSetting, ServiceSettingsModel>().ReverseMap();
        }
    }
}
