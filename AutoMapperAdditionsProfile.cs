using System.Linq;
using AutoMapper;
using E_CommerceSystem.DTOs;
using E_CommerceSystem.Models; // entities + your DTO container classes

namespace E_CommerceSystem.Mapping.Profiles
{
    public class AutoMapperAdditionsProfile : Profile
    {
        public AutoMapperAdditionsProfile()
        {
            // ===================== PRODUCTS =====================
            // Product -> E_CommerceSystem.Models.PagingDtos.ProductListDto (nested record)
            CreateMap<Product, E_CommerceSystem.Models.PagingDtos.ProductListDto>()
                .ForCtorParam("Id", o => o.MapFrom(p => p.PID))
                .ForCtorParam("ProductName", o => o.MapFrom(p => p.ProductName))
                .ForCtorParam("Price", o => o.MapFrom(p => p.Price))
                // Your Product has no ImageUrl: map null to satisfy the ctor
                .ForCtorParam("ImageUrl", o => o.MapFrom(p => p.ImageUrl))
                .ForCtorParam("Category", o => o.MapFrom(p => p.Category!.Name))
                .ForCtorParam("Supplier", o => o.MapFrom(p => p.Supplier!.Name));

            // ProductDTO <-> Product (your create/update DTO)
            CreateMap<ProductDTO, Product>();  // DTO -> Entity
            CreateMap<Product, ProductDTO>();  // Entity -> DTO

            // ===================== CATEGORIES =====================
            CreateMap<Category, CategoryDtos.CategoryDto>()
                .ConstructUsing(c => new CategoryDtos.CategoryDto(
                    c.CategoryId, c.Name, c.Description));

            CreateMap<CategoryDtos.CreateCategoryDto, Category>();
            CreateMap<CategoryDtos.UpdateCategoryDto, Category>();

            // ===================== SUPPLIERS =====================
            CreateMap<Supplier, SupplierDtos.SupplierDto>()
                .ConstructUsing(s => new SupplierDtos.SupplierDto(
                    s.SupplierId, s.Name, s.ContactEmail, s.Phone));

            CreateMap<SupplierDtos.CreateSupplierDto, Supplier>();
            CreateMap<SupplierDtos.UpdateSupplierDto, Supplier>();

            // ===================== REVIEWS =====================
            CreateMap<ReviewDTO, Review>()
                .ForMember(e => e.ReviewID, o => o.Ignore())
                .ForMember(e => e.ReviewDate, o => o.Ignore()) // set in service: DateTime.UtcNow
                .ForMember(e => e.UID, o => o.Ignore()) // set from auth 
                .ForMember(e => e.PID, o => o.Ignore()); // set from route/body
            CreateMap<Review, ReviewDTO>();

            // ===================== ORDERS (your minimal DTOs) =====================
            // Note: your entity is OrderProducts (plural). If it doesn't have a Product nav yet,
            // map ProductName as empty/null for now to keep build green.
            CreateMap<OrderProducts, OrderItemDTO>()
     .ForMember(d => d.ProductName, o => o.MapFrom(s => s.product!.ProductName))
     .ForMember(d => d.Quantity, o => o.MapFrom(s => s.Quantity));


            CreateMap<Order, OrdersOutputOTD>()
     .ForMember(d => d.ProductName, o => o.MapFrom(s =>
         s.OrderProducts.Select(op => op.product!.ProductName).FirstOrDefault()))
     .ForMember(d => d.Quantity, o => o.MapFrom(s =>
         s.OrderProducts.Select(op => op.Quantity).FirstOrDefault()));

            // ===================== USERS =====================
            // Input DTO -> Entity
            // UserDTO mapping (already exists)
            CreateMap<UserDTO, User>()
    .ForMember(u => u.Role, o => o.MapFrom(s => Enum.Parse<User.UserRole>(s.Role, true)))
    .ForMember(u => u.Password, o => o.Ignore())
    .ForMember(u => u.CreatedAt, o => o.Ignore());

            CreateMap<RegisterUserDTO, User>()
                .ForMember(u => u.Password, o => o.Ignore())   // hash in service
                .ForMember(u => u.CreatedAt, o => o.MapFrom(src => DateTime.UtcNow));
            // User -> LoginResponseDTO
            CreateMap<User, LoginResponseDTO>()
                .ForMember(dest => dest.Message, opt => opt.MapFrom(src => "Login successful"))
                .ForMember(dest => dest.AccessToken, opt => opt.Ignore())   // Day-2
                .ForMember(dest => dest.RefreshToken, opt => opt.Ignore()); // Day-2

            CreateMap<UpdateOrderDTO, Order>();
            CreateMap<UpdateProductDTO, Product>();

            // ===================== ORDER PRODUCTS =====================
            CreateMap<RegisterUserDTO, User>();            // Password will be set manually (hashed)
            CreateMap<User, UserDTO>();
            CreateMap<User, LoginResponseDTO>()
                .ForMember(d => d.Token, o => o.Ignore()); // set later after issuing JWT

            // ===================== TOP RATED PRODUCTS =====================
            CreateMap<Product, TopRatedProductDto>()
             .ForMember(d => d.ProductId, m => m.MapFrom(s => s.PID))
             .ForMember(d => d.ProductName, m => m.MapFrom(s => s.ProductName))
             .ForMember(d => d.AvgRating, m => m.MapFrom(s => s.Reviews.Any() ? s.Reviews.Average(r => r.Rating) : 0))
             .ForMember(d => d.ReviewsCount, m => m.MapFrom(s => s.Reviews.Count));

           

        }
    }
}
