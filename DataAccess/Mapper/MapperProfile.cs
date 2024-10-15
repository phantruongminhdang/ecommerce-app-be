using AutoMapper;
using DataAccess.Commons;
using Domain.Entities;
using Domain.ViewModels.Cart;
using Domain.ViewModels.Order;
using Domain.ViewModels.Product;
using Domain.ViewModels.User;

namespace DataAccess.Mapper
{
    public class MapperProfile : Profile
    {
        public MapperProfile()
        {
            CreateMap(typeof(Pagination<>), typeof(Pagination<>));
            CreateMap<UserResponseDTO, ApplicationUser>().ReverseMap();
            CreateMap<Product, ProductDTO>()
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category.Name)).ReverseMap();
            CreateMap<Product, ProductRequestDTO>().ReverseMap();
            CreateMap<OrderItem, OrderItemRequestDTO>().ReverseMap();
            CreateMap<OrderItem, OrderItemResponseDTO>()
                 .ForMember(dest => dest.ProductDTO, opt => opt.MapFrom(src => src.Product)); ;
            CreateMap<Order, OrderRequestDTO>().ReverseMap();
            CreateMap<Order, OrderResponseDTO>()
                .ForMember(dest => dest.UserResponseDTO, opt => opt.MapFrom(src => src.Customer.ApplicationUser))
                .ForMember(dest => dest.OrderItemResponseDTOs, opt => opt.MapFrom(src => src.OrderItems));
            CreateMap<CartItem, CartItemRequestDTO>().ReverseMap();
            CreateMap<CartItem, CartItemResponseDTO>()
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product.Name))
                .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Product.Price))
                .ForMember(dest => dest.Code, opt => opt.MapFrom(src => src.Product.Code))
                .ForMember(dest => dest.imageUrl, opt => opt.MapFrom(src => src.Product.imageUrl));
            CreateMap<Cart, CartRequestDTO>().ReverseMap();
            CreateMap<Cart, CartResponseDTO>()
                .ForMember(dest => dest.CartItemResponseDTOs, opt => opt.MapFrom(src => src.CartItems));
        }
    }
}
