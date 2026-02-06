using AutoMapper;
using Core.Models;
using EfDataStorage.Entities;

namespace Services;

public class MappingProfile : Profile
{
    public MappingProfile()
    {        
        CreateMap<PatientEntity, PatientModel>().ReverseMap();
        CreateMap<ServiceSettingsEntity, ServiceSettingsModel>().ReverseMap();
    }
}

