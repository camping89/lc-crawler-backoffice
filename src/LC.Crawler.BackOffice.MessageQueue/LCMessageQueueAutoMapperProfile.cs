using AutoMapper;

namespace LC.Crawler.BackOffice.MessageQueue;

public class LCMessageQueueAutoMapperProfile : Profile
{
    public LCMessageQueueAutoMapperProfile()
    {
        
    }

    // private void MapFeed()
    // {
    //     CreateMap<CrawlPayload, Feed>()
    //         .ForMember(dest => dest.Content, opt => opt.MapFrom(src => src.Content))
    //         .ForMember(dest => dest.PostedAt, opt => opt.MapFrom(src => src.CreatedAt))
    //         .ForMember(dest => dest.Like, opt => opt.MapFrom(src => src.LikeCount))
    //         .ForMember(dest => dest.Comment, opt => opt.MapFrom(src => src.CommentCount))
    //         .ForMember(dest => dest.Share, opt => opt.MapFrom(src => src.ShareCount))
    //         .ForMember(dest => dest.AuthorUid, opt => opt.MapFrom(src => src.CreateFuid))
    //         .ForMember(dest => dest.AuthorName, opt => opt.MapFrom(src => src.CreatedBy))
    //         .ForMember(dest => dest.Link, opt => opt.MapFrom(src => src.Url))
    //         .ForMember(dest => dest.AvatarUrl, opt => opt.MapFrom(src => src.AvatarUrl))
    //         .ForMember(dest => dest.ImageUrls, opt => opt.MapFrom(src => src.Images));
    // }
    //
}