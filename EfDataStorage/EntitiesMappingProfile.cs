using AutoMapper;
using Core.Models;
using EfDataStorage.Entities;

namespace EfDataStorage;

public class EntitiesMappingProfile : Profile
{
    public EntitiesMappingProfile()
    {        
        CreateMap<PatientEntity, PatientModel>().ReverseMap();
        CreateMap<ServiceSettingsEntity, ServiceSettingsModel>().ReverseMap();
    }
}

