using BugStore.Application.Features.Products.CreateProduct;
using BugStore.Application.Features.Products.GetProducts;
using BugStore.Application.Features.Products.GetProductById;
using BugStore.Domain.Entities;
using BugStore.Application.Features.Products.ProductSearch;
using BugStore.Domain.Common;

namespace BugStore.Application.Mappings.Products;

public static class ProductMappings
{
    public static CreateProductResponse ToCreateResponse(this Product product)
    {
        return new CreateProductResponse
        {
            Id = product.Id,
            Title = product.Title,
            Description = product.Description,
            Slug = product.Slug,
            Price = product.Price,
            CreatedAt = DateTime.UtcNow
        };
    }
    public static GetProductsResponse ToGetResponse(this Product product)
    {
        return new GetProductsResponse
        {
            Id = product.Id,
            Title = product.Title,
            Description = product.Description,
            Slug = product.Slug,
            Price = product.Price
        };
    }

    public static ProductSearchResponse ToProductSearchResponse(this Product product)
    {
        return new ProductSearchResponse
        {
            Id = product.Id,
            Title = product.Title,
            Description = product.Description,
            Slug = product.Slug,
            Price = product.Price
        };
    }

    public static GetProductByIdResponse ToGetByIdResponse(this Product product)
    {
        return new GetProductByIdResponse
        {
            Id = product.Id,
            Title = product.Title,
            Description = product.Description,
            Slug = product.Slug,
            Price = product.Price
        };
    }

    public static List<GetProductsResponse> ToGetResponseList(this IEnumerable<Product> products)
    {
        return products.Select(p => p.ToGetResponse()).ToList();
    }

    public static PagedResponse<GetProductsResponse> ToGetPagedResponse(this PaginatedList<Product> products)
    {
        return new PagedResponse<GetProductsResponse>
        (
             products.Items.Select(p => p.ToGetResponse()).ToList(),
             products.TotalCount,
             products.PageSize,
             products.CurrentPage
        );
    }
}

