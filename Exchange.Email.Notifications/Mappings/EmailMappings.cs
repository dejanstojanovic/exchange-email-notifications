using AutoMapper;
using Exchange.Email.Notifications.Models;
using Microsoft.Exchange.WebServices.Data;
using System.Linq;

namespace Exchange.Email.Notifications.Mappings
{
    public class EmailMappings:Profile
    {
        public EmailMappings()
        {
            CreateMap<EmailMessage, EmailMessageModel>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.UniqueId))
                .ForMember(dest => dest.Attachments, opt => opt.MapFrom(src => src.Attachments.AsEnumerable()))
                .ForMember(dest => dest.From, opt => opt.MapFrom(src => src.From.Address));

            CreateMap<EmailMessage, NewEmailMessageModel>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.UniqueId))
                .ForMember(dest => dest.Attachments, opt => opt.MapFrom(src => src.Attachments.AsEnumerable()))
                .ForMember(dest => dest.From, opt => opt.MapFrom(src => src.From.Address));

        }
    }
}
