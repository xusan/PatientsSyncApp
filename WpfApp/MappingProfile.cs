using AutoMapper;
using Core.Models;
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
