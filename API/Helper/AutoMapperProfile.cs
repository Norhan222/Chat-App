
using API.Dtos;
using API.Entities;
using API.Extensions;
using AutoMapper;

namespace API.Helper
{
    public class AutoMapperProfile:Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<AppUser, MemberDto>()
                .ForMember(dest => dest.PhotoUrl, opt => opt.MapFrom(src =>
                src.Photos.FirstOrDefault(x => x.IsMain).Url))
                .ForMember(dest => dest.Age, opt => opt.MapFrom(src => src.DateOfBirth.CalculateAge()));
            CreateMap<Photo, PhotoDto>();
            CreateMap<MemberUpdateDto, AppUser>().ReverseMap();
            CreateMap<RegisterDto, AppUser>()
                .ForMember(dest=>dest.DateOfBirth, opt => opt.MapFrom(src => src.DateOfBirth.Date));
            CreateMap<Message, MessageDto>()
                .ForMember(dest => dest.SenderphotoUrl, opt => opt.MapFrom(src =>
                     src.Sender.Photos.FirstOrDefault(p => p.IsMain).Url))
                .ForMember(dest => dest.RecipientPhotoUrl, opt => opt.MapFrom(src =>
                     src.Recipient.Photos.FirstOrDefault(p => p.IsMain).Url));
                
        }
    }
}
