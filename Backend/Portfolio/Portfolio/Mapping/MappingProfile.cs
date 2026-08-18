using AutoMapper;
using Portfolio.Models;
using Portfolio.DTOs;
namespace Portfolio.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // --- User / Profile ---
            CreateMap<User, ProfileResponseDto>()
                .ForMember(dest => dest.HasResume, opt => opt.MapFrom(src => src.ResumeUrl != null));

            CreateMap<UpdateProfileRequestDto, User>()
                // Only map fields this DTO is actually allowed to change —
                // prevents accidentally overwriting things like PasswordHash or Id.
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Email, opt => opt.Ignore())
                .ForMember(dest => dest.PasswordHash, opt => opt.Ignore());

            // --- Education ---
            CreateMap<Education, EducationDto>();
            CreateMap<UpsertEducationDto, Education>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.UserId, opt => opt.Ignore());

            // --- Project ---
            CreateMap<Project, ProjectDto>();
            CreateMap<ProjectImage, ProjectImageDto>();
            CreateMap<UpsertProjectDto, Project>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.UserId, opt => opt.Ignore());

            // --- Technology ---
            CreateMap<Technology, TechnologyDto>();

            // --- Work Experience ---
            CreateMap<WorkExperience, WorkExperienceDto>();
            CreateMap<UpsertWorkExperienceDto, WorkExperience>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.UserId, opt => opt.Ignore());
        }
    }
}
