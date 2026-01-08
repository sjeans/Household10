using AutoMapper;
using Household.Shared.Dtos;
using Household.Shared.Models;

namespace Household.Application.Common.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<AddressInfoDto, AddressInfo>().ReverseMap();

        CreateMap<ContactNumberDto, ContactNumber>().ReverseMap();

        CreateMap<TVShowDto, TvShow>()
            .ForMember(dest => dest.Rating, conf => conf.MapFrom(src => src.Rating))
            .ForMember(dest => dest.Seasons, conf => conf.MapFrom(src => src.Season))
            .ForMember(dest => dest.StreamingServiceId, conf => conf.MapFrom(src => src.StreamingId))
            .ForMember(dest => dest.Time, conf => conf.MapFrom(src => Convert.ToDateTime(src.Time).ToShortTimeString()))
            .ReverseMap();

        CreateMap<TvShow, TVShowDto>()
            .ForMember(dest => dest.Rating, conf => conf.MapFrom(src => src.Rating))
            .ForMember(dest => dest.Season, conf => conf.MapFrom(src => src.Seasons))
            .ForMember(dest => dest.StreamingId, conf => conf.MapFrom(src => src.StreamingService.Id))
            .ForMember(dest => dest.StreamingName, conf => conf.MapFrom(src => src.StreamingService.Name))
            .ForMember(dest => dest.StreamingSubscription, conf => conf.MapFrom(src => src.StreamingService.Description))
            .ForMember(dest => dest.Time, conf => conf.MapFrom(src => Convert.ToDateTime(src.Time).ToShortTimeString()))
            .ForMember(dest => dest.StartDate, conf => conf.MapFrom(src => Convert.ToDateTime(src.Time).ToString()))
            .ReverseMap();

        CreateMap<StreamingServiceDto, StreamingService>().ReverseMap();

        CreateMap<MovieInfoDto, MovieInfo>().ReverseMap();

        CreateMap<TvShowInformation, TVShowInformationDto>()
            .ForMember(dest => dest.Rating, conf => conf.MapFrom(src => src.Rating!.Average))
            .ForMember(dest => dest.Season, conf => conf.MapFrom(src => src.Episodes!.FirstOrDefault(x => x.AirStamp!.Value.CompareTo(DateTime.Today) >= 0)!.Season))
            .ForMember(dest => dest.NumberEpisodes, conf => conf.MapFrom(src => src.Episodes!.Count))
            //.ForMember(dest => dest.StartDate, conf => conf.MapFrom(src => src.Episodes.FirstOrDefault(x => x.airstamp.Value.CompareTo(DateTime.Today) >= 0).airstamp))
            .ReverseMap();
    }
}
