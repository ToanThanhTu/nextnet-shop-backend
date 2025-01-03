using Microsoft.EntityFrameworkCore;
using net_backend.Categories;
using net_backend.Data.Types;

namespace net_backend.Products;

public static class ProductsEndpoints
{
    public static void RegisterProductsEndpoints(this WebApplication app)
    {
        var products = app.MapGroup("/products");

        products.MapGet("/", GetAllProducts);
        products.MapGet("/sales", GetOnSaleProducts);
        products.MapGet("/bestsellers", GetBestSellers);
        products.MapGet("/categories/{categoryName}", GetProductsByCategoryName);
        products.MapGet("/subcategories/{subCategoryName}", GetProductsBySubCategoryName);
        products.MapGet("/{id}", GetProduct);
        products.MapGet("/{id}/image", GetProductImage);
        products.MapPost("/", CreateProduct);
        products.MapPut("/{id}", UpdateProduct);
        products.MapDelete("/{id}", DeleteProduct);

        // Using TypedResults to verify the return type is correct
        // Advantages:
        // - testability
        // - automatically returning the response type metadata for OpenAPI to describe the endpoint
        static async Task<IResult> GetAllProducts(AppDbContext db)
        {
            return TypedResults.Ok(await db.Products.Select(p => new ProductDTO
            {
                Id = p.Id,
                Title = p.Title,
                Description = p.Description,
                Price = p.Price,
                Sale = p.Sale,
                Stock = p.Stock
            }).ToArrayAsync());
        }

        static async Task<IResult> GetOnSaleProducts(AppDbContext db)
        {
            return TypedResults.Ok(
              await db.Products.Where(p => p.Sale > 0).ToListAsync()
            );
        }

        static async Task<IResult> GetBestSellers(AppDbContext db)
        {
            return TypedResults.Ok(
              await db.Products.OrderByDescending(p => p.Sold).Take(12).Select(p => new ProductDTO
              {
                  Id = p.Id,
                  Title = p.Title,
                  Description = p.Description,
                  Price = p.Price,
                  Sale = p.Sale,
                  Stock = p.Stock
              }).ToListAsync()
            );
        }

        static async Task<IResult> GetProductsByCategoryName(string categoryName, AppDbContext db)
        {
            //var category = await db.Categories.FirstOrDefaultAsync(c => c.Title == categoryName);

            //if (category is null)
            //    return TypedResults.NotFound();

            var products = await (from p in db.Products
                                  join sc in db.SubCategories on p.SubCategoryId equals sc.Id
                                  join c in db.Categories on sc.CategoryId equals c.Id
                                  where c.Name == categoryName
                                  select new ProductDTO
                                  {
                                      Id = p.Id,
                                      Title = p.Title,
                                      Description = p.Description,
                                      Price = p.Price,
                                      Sale = p.Sale,
                                      Stock = p.Stock
                                  }).ToListAsync();

            if (products.Count == 0)
                return TypedResults.NotFound();

            return TypedResults.Ok(products);
        }

        static async Task<IResult> GetProductsBySubCategoryName(string subCategoryName, AppDbContext db)
        {
            var products = await (from p in db.Products
                                  join sc in db.SubCategories on p.SubCategoryId equals sc.Id
                                  where sc.Name == subCategoryName
                                  select new ProductDTO
                                  {
                                      Id = p.Id,
                                      Title = p.Title,
                                      Description = p.Description,
                                      Price = p.Price,
                                      Sale = p.Sale,
                                      Stock = p.Stock
                                  }).ToListAsync();

            if (products.Count == 0)
                return TypedResults.NotFound();

            return TypedResults.Ok(products);
        }

        static async Task<IResult> GetProduct(int id, AppDbContext db)
        {
            return await db.Products.FindAsync(id) is Product product
              ? TypedResults.Ok(product)
              : TypedResults.NotFound();
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
