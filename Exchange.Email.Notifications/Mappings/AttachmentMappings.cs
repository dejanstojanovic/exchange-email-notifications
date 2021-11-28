using AutoMapper;
using Exchange.Email.Notifications.Models;
using Microsoft.Exchange.WebServices.Data;

namespace Exchange.Email.Notifications.Mappings
{
    public class AttachmentMappings : Profile
    {
        public AttachmentMappings()
        {
            CreateMap<FileAttachment, EmailMessageAttachmentModel>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Filename, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Size, opt => opt.MapFrom(src => src.Size));

        }
    }
}
