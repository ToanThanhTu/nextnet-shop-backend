using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using net_backend.Data.Types;

namespace net_backend.Products;

public static class ProductsEndpoints
{
    public static void RegisterProductsEndpoints(this WebApplication app)
    {
        var products = app.MapGroup("/products");

        products.MapGet("/all/", GetProducts);
        products.MapGet("/top-deals", GetTopDeals);
        products.MapGet("/sales/", GetOnSaleProducts);
        products.MapGet("/bestsellers/", GetBestSellers);
        products.MapGet("/search", GetProductsBySearch);
        products.MapGet("/recommendations/{productId}", GetProductsRecommendations);
        products.MapGet("/personal-recommendations/{userId}", GetPersonalRecommendations);
        products.MapGet("/id/{id}", GetProductById);
        products.MapGet("/slug/{slug}", GetProductBySlug);
        products.MapGet("/{id}/image", GetProductImage);
        products.MapPost("/", CreateProduct);
        products.MapPut("/id/{id}", UpdateProduct);
        products.MapDelete("/id/{id}", DeleteProduct);

        // Using TypedResults to verify the return type is correct
        // Advantages:
        // - testability
        // - automatically returning the response type metadata for OpenAPI to describe the endpoint
        static async Task<IResult> GetProducts(
            [FromQuery] string? category,
            [FromQuery] string? subcategory,
            [FromQuery] decimal? priceMin,
            [FromQuery] decimal? priceMax,
            [FromQuery] string? sortBy,
            [FromQuery] int? limit,
            [FromQuery] int? page,
            AppDbContext db)
        {
            if (db is null) return TypedResults.Problem("Database context is unavailable");

            try
            {
                // Validate pagination parameters
                limit = Math.Clamp(limit.GetValueOrDefault(12), 1, 100);
                page = Math.Max(page.GetValueOrDefault(1), 1);

                var query = db.Products
                    .Join(db.SubCategories,
                        product => product.SubCategoryId,
                        subCategory => subCategory.Id,
                        (product, subCategory) => new { product, subCategory })
                    .Join(db.Categories,
                        ps => ps.subCategory.CategoryId,
                        category => category.Id,
                        (ps, category) => new
                        {
                            ps.product,
                            ps.subCategory,
                            category
                        })
                    .AsNoTracking();

                // Filter by SubCategory if exists, otherwise filter by Category
                if (!string.IsNullOrEmpty(subcategory))
                {
                    query = query.Where(p => p.subCategory.Title.ToLower() == subcategory.ToLower());
                }
                else if (!string.IsNullOrEmpty(category))
                {
                    query = query.Where(p => p.category.Title.ToLower() == category.ToLower());
                }

                // Filter by Price
                if (priceMin.HasValue)
                {
                    query = query.Where(p => p.product.SalePrice >= priceMin.Value);
                }

                if (priceMax.HasValue)
                {
                    query = query.Where(p => p.product.SalePrice <= priceMax.Value);
                }

                // Sorting
                query = sortBy?.ToLower() switch
                {
                    "priceLowHigh" => query.OrderBy(p => p.product.SalePrice),
                    "priceHighLow" => query.OrderByDescending(p => p.product.SalePrice),
                    _ => query.OrderBy(p => p.product.Id)
                };

                var totalItems = await query.CountAsync();

                // Pagination
                var itemsToSkip = (page.Value - 1) * limit.Value;

                var products = await query
                    .Skip(itemsToSkip)
                    .Take(limit.Value)
                    .Select(p => new ProductDTO
                    {
                        Id = p.product.Id,
                        Title = p.product.Title,
                        Slug = p.product.Slug,
                        Description = p.product.Description,
                        Price = p.product.Price,
                        Sale = p.product.Sale,
                        SalePrice = p.product.SalePrice,
                        Stock = p.product.Stock
                    })
                    .ToArrayAsync();

                return TypedResults.Ok(new
                {
                    products,
                    totalItems
                });
            }
            catch (Exception ex)
            {
                return TypedResults.Problem($"An error occurred: {ex.Message}");
            }
        }

        static async Task<IResult> GetTopDeals(AppDbContext db)
        {
            var query = db.Products
                .Where(p => p.Sale > 0)
                .OrderByDescending(p => p.Sale)
                .Take(4);

            var products = await query.Select(p => new ProductDTO
            {
                Id = p.Id,
                Title = p.Title,
                Slug = p.Slug,
                Description = p.Description,
                Price = p.Price,
                Sale = p.Sale,
                SalePrice = p.SalePrice,
                Stock = p.Stock
            }).ToArrayAsync();

            return TypedResults.Ok(products);
        }

        static async Task<IResult> GetOnSaleProducts(
            [FromQuery] decimal? priceMin,
            [FromQuery] decimal? priceMax,
            [FromQuery] string? sortBy,
            [FromQuery] int? limit,
            [FromQuery] int? page,
            AppDbContext db)
        {
            var query = db.Products.Where(p => p.Sale > 0);

            // Filter by Price
            if (priceMin.HasValue)
            {
                query = query.Where(p => p.SalePrice >= priceMin.Value);
            }

            if (priceMax.HasValue)
            {
                query = query.Where(p => p.SalePrice <= priceMax.Value);
            }

            // Sorting
            query = sortBy switch
            {
                "priceLowHigh" => query.OrderBy(p => p.SalePrice),
                "priceHighLow" => query.OrderByDescending(p => p.SalePrice),
                _ => query.OrderBy(p => p.Id)
            };

            var totalItems = await query.CountAsync();

            // Pagination
            int itemsToSkip = (page.GetValueOrDefault(1) - 1) * limit.GetValueOrDefault(12);
            query = query.Skip(itemsToSkip).Take(limit.GetValueOrDefault(12));

            var products = await query.Select(p => new ProductDTO
            {
                Id = p.Id,
                Title = p.Title,
                Slug = p.Slug,
                Description = p.Description,
                Price = p.Price,
                Sale = p.Sale,
                SalePrice = p.SalePrice,
                Stock = p.Stock
            }).ToArrayAsync();
            return TypedResults.Ok(new
            {
                products,
                totalItems
            });
        }

        static async Task<IResult> GetBestSellers(
            [FromQuery] decimal? priceMin,
            [FromQuery] decimal? priceMax,
            [FromQuery] string? sortBy,
            [FromQuery] int? limit,
            [FromQuery] int? page,
            AppDbContext db)
        {
            var query = db.Products.Where(p => p.Sold > 20);

            // Filter by Price
            if (priceMin.HasValue)
            {
                query = query.Where(p => p.SalePrice >= priceMin.Value);
            }

            if (priceMax.HasValue)
            {
                query = query.Where(p => p.SalePrice <= priceMax.Value);
            }

            // Sorting
            query = sortBy switch
            {
                "priceLowHigh" => query.OrderBy(p => p.SalePrice),
                "priceHighLow" => query.OrderByDescending(p => p.SalePrice),
                _ => query.OrderByDescending(p => p.Sold)
            };

            var totalItems = await query.CountAsync();

            // Pagination
            int itemsToSkip = (page.GetValueOrDefault(1) - 1) * limit.GetValueOrDefault(12);
            query = query.Skip(itemsToSkip).Take(limit.GetValueOrDefault(12));

            var products = await query.Select(p => new ProductDTO
            {
                Id = p.Id,
                Title = p.Title,
                Slug = p.Slug,
                Description = p.Description,
                Price = p.Price,
                Sale = p.Sale,
                SalePrice = p.SalePrice,
                Stock = p.Stock
            }).ToArrayAsync();
            return TypedResults.Ok(new
            {
                products,
                totalItems
            });
        }

        static async Task<IResult> GetProductsBySearch(
            [FromQuery] string? search,
            AppDbContext db)
        {
            var query = db.Products.AsQueryable();

            if (!string.IsNullOrEmpty(search))
                query = query.Where(p => p.Title.Contains(search) || (p.Description != null && p.Description.Contains(search)));

            var products = await query.Select(p => new ProductDTO
            {
                Id = p.Id,
                Title = p.Title,
                Slug = p.Slug,
                Description = p.Description,
                Price = p.Price,
                Sale = p.Sale,
                SalePrice = p.SalePrice,
                Stock = p.Stock
            }).ToArrayAsync();

            if (products.Length == 0)
                return TypedResults.NotFound();

            return TypedResults.Ok(products);
        }

        static async Task<IResult> GetProductsRecommendations(int productId, AppDbContext db)
        {
            var originalProduct = await db.Products
                .Where(p => p.Id == productId)
                .Select(p => new
                {
                    p.SubCategoryId
                }).FirstOrDefaultAsync();

            if (originalProduct == null)
            {
                return TypedResults.NotFound($"Product with Id {productId} not found.");
            }

            var recommendations = await db.Products
                .Where(p => p.SubCategoryId == originalProduct.SubCategoryId && p.Id != productId)
                .Select(p => new ProductDTO
                {
                    Id = p.Id,
                    Title = p.Title,
                    Slug = p.Slug,
                    Price = p.Price,
                    Sale = p.Sale,
                    SalePrice = p.SalePrice
                })
                .ToArrayAsync();

            return TypedResults.Ok(recommendations);
        }

        static async Task<IResult> GetPersonalRecommendations(int userId, AppDbContext db)
        {
            var userOrders = await db.Orders
                .Where(o => o.UserId == userId)
                .Include(o => o.OrderItems!)
                    .ThenInclude(oi => oi.Product)
                .ToListAsync();

            // if user does not have order history, return top deals
            if (userOrders == null || userOrders.Count == 0)
            {
                return TypedResults.Ok(GetTopDeals);
            }

            var subCategoryIds = userOrders
                .SelectMany(o => o.OrderItems!)
                .Select(oi => oi.Product!.SubCategoryId)
                .Distinct()
                .ToList();

            var personalRecommendedProducts = await db.Products
                .Where(p => subCategoryIds.Contains(p.SubCategoryId))
                .Select(p => new ProductDTO
                {
                    Id = p.Id,
                    Title = p.Title,
                    Slug = p.Slug,
                    Description = p.Description,
                    Price = p.Price,
                    Sale = p.Sale,
                    SalePrice = p.SalePrice,
                    Stock = p.Stock
                })
                .ToListAsync();

            return TypedResults.Ok(personalRecommendedProducts);
        }

        static async Task<IResult> GetProductById(int id, AppDbContext db)
        {
            var product = await db.Products.Where(p => p.Id == id).Select(p => new ProductDTO
            {
                Id = p.Id,
                Title = p.Title,
                Slug = p.Slug,
                Description = p.Description,
                Price = p.Price,
                Sale = p.Sale,
                SalePrice = p.SalePrice,
                Stock = p.Stock
            }).FirstOrDefaultAsync();

            if (product is null)
                return TypedResults.NotFound();

            return TypedResults.Ok(product);
        }

        static async Task<IResult> GetProductBySlug(string slug, AppDbContext db)
        {
            var productData = await db.Products
                .Where(p => p.Slug == slug)
                .Select(p => new
                {
                    Product = new ProductDTO
                    {
                        Id = p.Id,
                        Title = p.Title,
                        Slug = p.Slug,
                        Description = p.Description,
                        Price = p.Price,
                        Sale = p.Sale,
                        SalePrice = p.SalePrice,
                        Stock = p.Stock,
                        SubCategoryId = p.SubCategoryId
                    },
                    SubCategory = new
                    {
                        p.SubCategory!.Title,
                        p.SubCategory.Slug
                    },
                    Category = new
                    {
                        p.SubCategory!.Category!.Title,
                        p.SubCategory.Category.Slug
                    }
                }).FirstOrDefaultAsync();

            if (productData is null)
                return TypedResults.NotFound();

            return TypedResults.Ok(new
            {
                product = productData.Product,
                subCategory = productData.SubCategory,
                category = productData.Category
            });
        }

        static async Task<IResult> GetProductImage(int id, AppDbContext db)
        {
            var product = await db.Products.FindAsync(id);
            if (product is null)
                return TypedResults.NotFound();
            if (product.Image is null)
                return TypedResults.NotFound();
            return TypedResults.File(product.Image, "image/jpeg");
        }

        static async Task<IResult> CreateProduct(Product product, AppDbContext db)
        {
            db.Products.Add(product);
            await db.SaveChangesAsync();

            return TypedResults.Created($"/products/{product.Id}", product);
        }

        static async Task<IResult> UpdateProduct(
          int id,
          Product inputProduct,
          AppDbContext db
        )
        {
            var product = await db.Products.FindAsync(id);

            if (product is null)
                return TypedResults.NotFound();

            product.Title = inputProduct.Title;
            product.Description = inputProduct.Description;
            product.Price = inputProduct.Price;
            product.Sale = inputProduct.Sale;
            product.Stock = inputProduct.Stock;

            await db.SaveChangesAsync();

            return TypedResults.NoContent();
        }

        static async Task<IResult> DeleteProduct(int id, AppDbContext db)
        {
            if (await db.Products.FindAsync(id) is Product product)
            {
                db.Products.Remove(product);
                await db.SaveChangesAsync();
                return TypedResults.NoContent();
            }

            return TypedResults.NotFound();
        }
    }
}
