namespace EMS.Application.Mappers;

using AutoMapper;
using EMS.Application.DTOs.Attendance;
using EMS.Application.DTOs.Auth;
using EMS.Application.DTOs.Course;
using EMS.Application.DTOs.Grade;
using EMS.Application.DTOs.Student;
using EMS.Domain.Entities;

/// <summary>
/// AutoMapper profile for all entity-to-DTO and DTO-to-entity mappings
/// This file centralizes all mapping configurations to keep them organized
/// </summary>
public class MappingProfile : Profile
{
    public MappingProfile()
    {
        #region Student Mappings

     

        #endregion

        #region User/Auth Mappings

        // User ? UserProfileResponse
        CreateMap<User, UserProfileResponse>()
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.GetFullName()))
            //.ForMember(dest => dest.DepartmentName, opt => opt.MapFrom(src => src.Department != null ? src.Department.Name : null))
            //.ForMember(dest => dest.AccountStatus, opt => opt.MapFrom(src => src.AccountStatus.ToString()))
            .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role.ToString()));

        #endregion

        #region Pagination Mappings

        // Map List<T> to list responses
        CreateMap(typeof(List<>), typeof(List<>)).ConvertUsing(typeof(ListConverter<,>));

        #endregion
    }

    /// <summary>
    /// Custom type converter for generic list mapping
    /// </summary>
    private class ListConverter<TSource, TDestination> : ITypeConverter<List<TSource>, List<TDestination>>
    {
        public List<TDestination> Convert(List<TSource> source, List<TDestination> destination, ResolutionContext context)
        {
            return source.Select(item => context.Mapper.Map<TDestination>(item)).ToList();
        }
    }
}
