using FoodShop.API.Data;
using FoodShop.API.DTOs.Product;
using FoodShop.API.Entities;
using FoodShop.API.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FoodShop.API.Repositories;

public class ProductRepository : GenericRepository<Product>, IProductRepository
{
    public ProductRepository(AppDbContext context) : base(context) { }

    public async Task<PagedResult<Product>> GetPagedAsync(ProductQueryParams p)
    {
        // Base query – only active products, Include category to avoid N+1
        var query = _context.Products
            .Include(x => x.Category)
            .Where(x => x.IsActive)
            .AsQueryable();

        // Filter by category
        if (p.CategoryId.HasValue)
            query = query.Where(x => x.CategoryId == p.CategoryId.Value);

        // Search by name (indexed)
        if (!string.IsNullOrWhiteSpace(p.Search))
            query = query.Where(x => x.Name.Contains(p.Search));

        // Price range
        if (p.MinPrice.HasValue)
            query = query.Where(x => x.Price >= p.MinPrice.Value);
        if (p.MaxPrice.HasValue)
            query = query.Where(x => x.Price <= p.MaxPrice.Value);

        // Sorting
        query = p.SortBy switch
        {
            "price_asc"  => query.OrderBy(x => x.Price),
            "price_desc" => query.OrderByDescending(x => x.Price),
            "name"       => query.OrderBy(x => x.Name),
            _            => query.OrderByDescending(x => x.CreatedAt)
        };

        var totalCount = await query.CountAsync();

        var items = await query
            .Skip((p.Page - 1) * p.PageSize)
            .Take(p.PageSize)
            .ToListAsync();

        return new PagedResult<Product>
        {
            Items = items,
            TotalCount = totalCount,
            Page = p.Page,
            PageSize = p.PageSize
        };
    }

    public async Task<Product?> GetWithCategoryAsync(int id)
        => await _context.Products
            .Include(x => x.Category)
            .FirstOrDefaultAsync(x => x.Id == id && x.IsActive);
}
