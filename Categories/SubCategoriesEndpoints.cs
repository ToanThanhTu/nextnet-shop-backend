using Microsoft.EntityFrameworkCore;

public static class SubCategoriesEndpoints
{
  public static void RegisterSubCategoriesEndpoints(this WebApplication app)
  {
    var subCategories = app.MapGroup("/subcategories");

    subCategories.MapGet("/", GetAllSubCategories);
    subCategories.MapGet("/{id}", GetSubCategory);
    subCategories.MapPost("/", CreateSubCategory);
    subCategories.MapPut("/{id}", UpdateSubCategory);
    subCategories.MapDelete("/{id}", DeleteSubCategory);

    // Using TypedResults to verify the return type is correct
    // Advantages:
    // - testability
    // - automatically returning the response type metadata for OpenAPI to describe the endpoint
    static async Task<IResult> GetAllSubCategories(AppDbContext db)
    {
      return TypedResults.Ok(await db.SubCategories.ToArrayAsync());
    }

    static async Task<IResult> GetSubCategory(int id, AppDbContext db)
    {
      return await db.SubCategories.FindAsync(id) is SubCategory subCategory
        ? TypedResults.Ok(subCategory)
        : TypedResults.NotFound();
    }

    static async Task<IResult> CreateSubCategory(
      SubCategory subCategory,
      AppDbContext db
    )
    {
      db.SubCategories.Add(subCategory);
      await db.SaveChangesAsync();

      return TypedResults.Created(
        $"/subcategories/{subCategory.Id}",
        subCategory
      );
    }

    static async Task<IResult> UpdateSubCategory(
      int id,
      SubCategory inputSubCategory,
      AppDbContext db
    )
    {
      var subCategory = await db.SubCategories.FindAsync(id);

      if (subCategory is null)
        return TypedResults.NotFound();

      subCategory.Title = inputSubCategory.Title;
      subCategory.Description = inputSubCategory.Description;
      subCategory.CategoryId = inputSubCategory.CategoryId;

      await db.SaveChangesAsync();

      return TypedResults.NoContent();
    }

    static async Task<IResult> DeleteSubCategory(int id, AppDbContext db)
    {
      if (await db.SubCategories.FindAsync(id) is SubCategory subCategory)
      {
        db.SubCategories.Remove(subCategory);
        await db.SaveChangesAsync();
        return TypedResults.NoContent();
      }

      return TypedResults.NotFound();
    }
  }
}
