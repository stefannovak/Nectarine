using AutoMapper;
using NectarineAPI.DTOs.Generic;
using NectarineData.Models;

namespace NectarineAPI.DTOs
{
    public class AutoMapping : Profile
    {
        public AutoMapping()
        {
            CreateMap<ApplicationUser, UserDTO>().ReverseMap();
            CreateMap<Order, OrderDTO>().ReverseMap();
            CreateMap<Product, ProductDTO>().ReverseMap();
            CreateMap<UserAddress, UserAddressDTO>().ReverseMap();
            CreateMap<Rating, RatingDTO>().ReverseMap();
        }
    }
}
