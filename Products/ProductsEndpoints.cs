using Microsoft.EntityFrameworkCore;

public static class ProductsEndpoints
{
  public static void RegisterProductsEndpoints(this WebApplication app)
  {
    var products = app.MapGroup("/products");

    products.MapGet("/", GetAllProducts);
    products.MapGet("/sold", GetOnSaleProducts);
    products.MapGet("/{id}", GetProduct);
    products.MapPost("/", CreateProduct);
    products.MapPut("/{id}", UpdateProduct);
    products.MapDelete("/{id}", DeleteProduct);

    // Using TypedResults to verify the return type is correct
    // Advantages:
    // - testability
    // - automatically returning the response type metadata for OpenAPI to describe the endpoint
    static async Task<IResult> GetAllProducts(AppDbContext db)
    {
      return TypedResults.Ok(await db.Products.ToArrayAsync());
    }

    static async Task<IResult> GetOnSaleProducts(AppDbContext db)
    {
      return TypedResults.Ok(
        await db.Products.Where(p => p.Sale > 0).ToListAsync()
      );
    }

    static async Task<IResult> GetProduct(int id, AppDbContext db)
    {
      return await db.Products.FindAsync(id) is Product product
        ? TypedResults.Ok(product)
        : TypedResults.NotFound();
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
