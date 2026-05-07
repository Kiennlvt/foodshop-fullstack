using AutoMapper;
using FoodShop.API.DTOs.Cart;
using FoodShop.API.DTOs.Category;
using FoodShop.API.DTOs.Order;
using FoodShop.API.DTOs.Product;
using FoodShop.API.Entities;

namespace FoodShop.API.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Product
        CreateMap<Product, ProductDto>()
            .ForMember(d => d.CategoryName, o => o.MapFrom(s => s.Category.Name));
        CreateMap<CreateProductDto, Product>();
        CreateMap<UpdateProductDto, Product>()
            .ForAllMembers(o => o.Condition((src, dest, srcMember) => srcMember != null));

        // Category
        CreateMap<Category, CategoryDto>()
            .ForMember(d => d.ProductCount, o => o.MapFrom(s => s.Products.Count));
        CreateMap<CreateCategoryDto, Category>();
        CreateMap<UpdateCategoryDto, Category>();

        // Cart
        CreateMap<Cart, CartDto>()
            .ForMember(d => d.Items, o => o.MapFrom(s => s.CartItems));
        CreateMap<CartItem, CartItemDto>()
            .ForMember(d => d.ProductName,  o => o.MapFrom(s => s.Product.Name))
            .ForMember(d => d.ProductImage, o => o.MapFrom(s => s.Product.ImageUrl))
            .ForMember(d => d.UnitPrice,    o => o.MapFrom(s => s.Product.Price))
            .ForMember(d => d.AvailableStock, o => o.MapFrom(s => s.Product.StockQuantity));

        // Order
        CreateMap<Order, OrderDto>()
            .ForMember(d => d.Username, o => o.MapFrom(s => s.User.Username))
            .ForMember(d => d.Status,   o => o.MapFrom(s => s.Status.ToString()))
            .ForMember(d => d.Items,    o => o.MapFrom(s => s.OrderDetails));
        CreateMap<OrderDetail, OrderDetailDto>()
            .ForMember(d => d.ProductName,  o => o.MapFrom(s => s.Product.Name))
            .ForMember(d => d.ProductImage, o => o.MapFrom(s => s.Product.ImageUrl));
    }
}
