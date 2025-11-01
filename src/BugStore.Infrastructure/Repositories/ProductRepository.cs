using BugStore.Domain.Common;
using BugStore.Domain.Contracts.IRepositories;
using BugStore.Domain.Entities;
using BugStore.Infrastructure.Data;
using BugStore.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace BugStore.Infrastructure.Repositories; 

public class ProductRepository : BaseRepository<Product>, IProductRepository
{
    public ProductRepository(AppDbContext context) 
        : base(context) { }

    public Task<Product> CreateAsync(Product product)
    {
        Add(product); 
        return Task.FromResult(product);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
       var product = await GetByCondition(x=>x.Id==id, true)
            .FirstOrDefaultAsync();

        if(product == null) 
            return false;

        Delete(product);
        return true;
    }

    public async Task<bool> ExistsAsync(Guid id)
        => await _context.Products.AnyAsync(p => p.Id == id);

    public async Task<PaginatedList<Product>> GetAllAsync(RequestParameters parameters)
    {
        var query = GetAll(false); 

        var count = query.Count();

        var items = await query
            .Skip((parameters.PageNumber - 1) * parameters.PageSize)
            .Take(parameters.PageSize).ToListAsync();

        return PaginatedList<Product>.ToPagedList(items, count, parameters.PageNumber, parameters.PageSize);
    }

    public Task<Product?> GetByIdAsync(Guid id)
        => GetByCondition(p => p.Id == id, false)
            .FirstOrDefaultAsync();

    public async Task<IEnumerable<Product>> GetByIdsAsync(IEnumerable<Guid> ids)
        => await GetByCondition(p => ids.Contains(p.Id), false)
            .ToListAsync();

    public async Task<PaginatedList<Product>> SearchAsync(ProductSearchParameters parameters, CancellationToken cancellationToken = default)
    {
        var query = GetAll(false)
            .WhereIf(!string.IsNullOrWhiteSpace(parameters.Title), x => EF.Functions.Like(x.Title, $"%{parameters.Title}%"))
            .WhereIf(!string.IsNullOrWhiteSpace(parameters.Description), x => EF.Functions.Like(x.Description, $"%{parameters.Description}%"))
            .WhereIf(!string.IsNullOrWhiteSpace(parameters.Slug), x => EF.Functions.Like(x.Slug, $"%{parameters.Slug}%"))
            .WhereIf(parameters.Price.HasValue, x => x.Price== parameters.Price); 


        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((parameters.PageNumber - 1) * parameters.PageSize)
            .Take(parameters.PageSize).ToListAsync(cancellationToken);

        return PaginatedList<Product>.ToPagedList(items, totalCount, parameters.PageNumber, parameters.PageSize);
    }

    public Task<Product> UpdateAsync(Product product)
    {
       Update(product);
         return  Task.FromResult(product);
    }
}
