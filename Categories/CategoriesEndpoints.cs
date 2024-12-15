using Microsoft.EntityFrameworkCore;

public static class CategoriesEndpoints
{
  public static void RegisterCategoriesEndpoints(this WebApplication app)
  {
    var categories = app.MapGroup("/categories");

    categories.MapGet("/", GetAllCategories);
    categories.MapGet("/{id}", GetCategory);
    categories.MapPost("/", CreateCategory);
    categories.MapPut("/{id}", UpdateCategory);
    categories.MapDelete("/{id}", DeleteCategory);

    // Using TypedResults to verify the return type is correct
    // Advantages:
    // - testability
    // - automatically returning the response type metadata for OpenAPI to describe the endpoint
    static async Task<IResult> GetAllCategories(AppDbContext db)
    {
      return TypedResults.Ok(await db.Categories.ToArrayAsync());
    }

    static async Task<IResult> GetCategory(int id, AppDbContext db)
    {
      return await db.Categories.FindAsync(id) is Category category
        ? TypedResults.Ok(category)
        : TypedResults.NotFound();
    }

    static async Task<IResult> CreateCategory(
      Category category,
      AppDbContext db
    )
    {
      db.Categories.Add(category);
      await db.SaveChangesAsync();

      return TypedResults.Created($"/categories/{category.Id}", category);
    }

    static async Task<IResult> UpdateCategory(
      int id,
      Category inputCategory,
      AppDbContext db
    )
    {
      var category = await db.Categories.FindAsync(id);

      if (category is null)
        return TypedResults.NotFound();

      category.Title = inputCategory.Title;
      category.Description = inputCategory.Description;

      await db.SaveChangesAsync();

      return TypedResults.NoContent();
    }

    static async Task<IResult> DeleteCategory(int id, AppDbContext db)
    {
      if (await db.Categories.FindAsync(id) is Category category)
      {
        db.Categories.Remove(category);
        await db.SaveChangesAsync();
        return TypedResults.NoContent();
      }

      return TypedResults.NotFound();
    }
  }
}
