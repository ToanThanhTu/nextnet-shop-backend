using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using net_backend.Modules.Categories.Application.Commands;
using net_backend.Modules.Categories.Application.Queries;
using net_backend.Modules.Categories.Contracts;
using net_backend.Modules.Categories.Domain;

namespace net_backend.Modules.Categories;

/// <summary>
/// HTTP adapter for the Category aggregate. Each action method is a thin
/// shell: validate input via [ApiController] + DataAnnotations, dispatch to
/// the matching handler, return the result. Authorization policies are
/// declared per-method via [Authorize].
///
/// Routes are explicitly lowercase to preserve the existing public API
/// (the frontend hits /categories, not /Categories).
/// </summary>
[ApiController]
[Route("categories")]
public class CategoriesController(
    ListCategoriesHandler listHandler,
    GetCategoryByIdHandler getByIdHandler,
    CreateCategoryHandler createHandler,
    UpdateCategoryHandler updateHandler,
    DeleteCategoryHandler deleteHandler,
    ICategoryRepository repository) : ControllerBase
{
    [HttpGet("")]
    public async Task<ActionResult<List<CategoryDto>>> List(CancellationToken cancellationToken)
    {
        var categories = await listHandler.ExecuteAsync(cancellationToken);
        return Ok(categories);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<CategoryDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var category = await getByIdHandler.ExecuteAsync(id, cancellationToken);
        return Ok(category);
    }

    [HttpGet("{id:int}/image")]
    public async Task<IActionResult> GetImage(int id, CancellationToken cancellationToken)
    {
        var image = await repository.GetImageAsync(id, cancellationToken);
        if (image is null) return NotFound();
        return File(image, "image/jpeg");
    }

    [HttpPost("")]
    [Authorize(Policy = "Admin")]
    public async Task<ActionResult<CategoryDto>> Create(
        [FromBody] CreateCategoryRequest request,
        CancellationToken cancellationToken)
    {
        var created = await createHandler.ExecuteAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = "Admin")]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateCategoryRequest request,
        CancellationToken cancellationToken)
    {
        await updateHandler.ExecuteAsync(id, request, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = "Admin")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await deleteHandler.ExecuteAsync(id, cancellationToken);
        return NoContent();
    }
}
