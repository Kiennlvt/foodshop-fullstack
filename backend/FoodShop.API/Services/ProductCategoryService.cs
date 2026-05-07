using AutoMapper;
using FoodShop.API.DTOs.Product;
using FoodShop.API.DTOs.Category;
using FoodShop.API.Entities;
using FoodShop.API.Interfaces;
using FoodShop.API.Interfaces.Repositories;

namespace FoodShop.API.Services;

// ── Product Service ───────────────────────────────────────────────────────────
public class ProductService : IProductService
{
    private readonly IProductRepository _repo;
    private readonly ICategoryRepository _catRepo;
    private readonly IMapper _mapper;

    public ProductService(IProductRepository repo, ICategoryRepository catRepo, IMapper mapper)
    {
        _repo    = repo;
        _catRepo = catRepo;
        _mapper  = mapper;
    }

    public async Task<PagedResult<ProductDto>> GetProductsAsync(ProductQueryParams queryParams)
    {
        var paged = await _repo.GetPagedAsync(queryParams);
        return new PagedResult<ProductDto>
        {
            Items      = _mapper.Map<List<ProductDto>>(paged.Items),
            TotalCount = paged.TotalCount,
            Page       = paged.Page,
            PageSize   = paged.PageSize
        };
    }

    public async Task<ProductDto?> GetProductByIdAsync(int id)
    {
        var product = await _repo.GetWithCategoryAsync(id);
        return product is null ? null : _mapper.Map<ProductDto>(product);
    }

    public async Task<ProductDto> CreateProductAsync(CreateProductDto dto)
    {
        if (!await _catRepo.ExistsAsync(dto.CategoryId))
            throw new KeyNotFoundException($"Category {dto.CategoryId} not found.");

        var product = _mapper.Map<Product>(dto);
        await _repo.AddAsync(product);
        var created = await _repo.GetWithCategoryAsync(product.Id);
        return _mapper.Map<ProductDto>(created!);
    }

    public async Task<ProductDto?> UpdateProductAsync(int id, UpdateProductDto dto)
    {
        var product = await _repo.GetWithCategoryAsync(id);
        if (product is null) return null;

        if (dto.CategoryId.HasValue && !await _catRepo.ExistsAsync(dto.CategoryId.Value))
            throw new KeyNotFoundException($"Category {dto.CategoryId.Value} not found.");

        _mapper.Map(dto, product);
        await _repo.UpdateAsync(product);

        var updated = await _repo.GetWithCategoryAsync(id);
        return _mapper.Map<ProductDto>(updated!);
    }

    public async Task<bool> DeleteProductAsync(int id)
    {
        var product = await _repo.GetByIdAsync(id);
        if (product is null) return false;

        product.IsActive = false; // Soft delete
        await _repo.UpdateAsync(product);
        return true;
    }
}

// ── Category Service ─────────────────────────────────────────────────────────
public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _repo;
    private readonly IMapper _mapper;

    public CategoryService(ICategoryRepository repo, IMapper mapper)
    {
        _repo   = repo;
        _mapper = mapper;
    }

    public async Task<IEnumerable<CategoryDto>> GetCategoriesAsync()
    {
        var cats = await _repo.GetAllWithCountAsync();
        return _mapper.Map<IEnumerable<CategoryDto>>(cats);
    }

    public async Task<CategoryDto?> GetCategoryByIdAsync(int id)
    {
        var cat = await _repo.GetByIdAsync(id);
        return cat is null ? null : _mapper.Map<CategoryDto>(cat);
    }

    public async Task<CategoryDto> CreateCategoryAsync(CreateCategoryDto dto)
    {
        if (await _repo.NameExistsAsync(dto.Name))
            throw new InvalidOperationException($"Category '{dto.Name}' already exists.");

        var cat = _mapper.Map<Category>(dto);
        await _repo.AddAsync(cat);
        return _mapper.Map<CategoryDto>(cat);
    }

    public async Task<CategoryDto?> UpdateCategoryAsync(int id, UpdateCategoryDto dto)
    {
        var cat = await _repo.GetByIdAsync(id);
        if (cat is null) return null;

        _mapper.Map(dto, cat);
        await _repo.UpdateAsync(cat);
        return _mapper.Map<CategoryDto>(cat);
    }

    public async Task<bool> DeleteCategoryAsync(int id)
    {
        var cat = await _repo.GetByIdAsync(id);
        if (cat is null) return false;
        await _repo.DeleteAsync(cat);
        return true;
    }
}
