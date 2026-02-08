using AutoMapper;
using Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WpfApp.ViewModels;

namespace WpfApp;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<PatientModel, PatientItemViewModel>().ReverseMap();
        CreateMap<ServiceSettingsModel, SettingsItemViewModel>().ReverseMap();        
    }
}
