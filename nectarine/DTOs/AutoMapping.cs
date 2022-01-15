using AutoMapper;
using nectarineAPI.DTOs.Generic;
using nectarineData.Models;

namespace nectarineAPI.DTOs
{
    public class AutoMapping : Profile
    {
        public AutoMapping()
        {
            CreateMap<ApplicationUser, UserDTO>().ReverseMap();
        }
    }
}
