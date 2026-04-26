using Microsoft.EntityFrameworkCore;
using net_backend.Data.Types;

namespace net_backend.Categories;

public static class SubCategoriesEndpoints
{
    public static void RegisterSubCategoriesEndpoints(this WebApplication app)
    {
        var subCategories = app.MapGroup("/subcategories");

        subCategories.MapGet("/", GetAllSubCategories);
        subCategories.MapGet("/{id}", GetSubCategory);
        subCategories.MapGet("/{id}/image", GetSubCategoryImage);
        subCategories.MapPost("/", CreateSubCategory).RequireAuthorization("Admin");
        subCategories.MapPut("/{id}", UpdateSubCategory).RequireAuthorization("Admin");
        subCategories.MapDelete("/{id}", DeleteSubCategory).RequireAuthorization("Admin");

        // Using TypedResults to verify the return type is correct
        // Advantages:
        // - testability
        // - automatically returning the response type metadata for OpenAPI to describe the endpoint
        static async Task<IResult> GetAllSubCategories(AppDbContext db)
        {
            return TypedResults.Ok(await db.SubCategories.Select(sc => new SubCategoryDTO
            {
                Id = sc.Id,
                Title = sc.Title,
                Slug = sc.Slug,
                Description = sc.Description,
            }).ToArrayAsync());
        }

        static async Task<IResult> GetSubCategory(int id, AppDbContext db)
        {
            return await db.SubCategories.FindAsync(id) is SubCategory subCategory
              ? TypedResults.Ok(subCategory)
              : TypedResults.NotFound();
        }

        static async Task<IResult> GetSubCategoryImage(int id, AppDbContext db)
        {
            var subCategory = await db.SubCategories.FindAsync(id);
            if (subCategory is null)
                return TypedResults.NotFound();
            if (subCategory.Image is null)
                return TypedResults.NotFound();
            return TypedResults.File(subCategory.Image, "image/jpeg");
        }

        static async Task<IResult> CreateSubCategory(SubCategory subCategory, AppDbContext db)
        {
            db.SubCategories.Add(subCategory);
            await db.SaveChangesAsync();

            var dto = new SubCategoryDTO
            {
                Id = subCategory.Id,
                Title = subCategory.Title,
                Slug = subCategory.Slug,
                Description = subCategory.Description,
            };
            return TypedResults.Created($"/subcategories/{subCategory.Id}", dto);
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
