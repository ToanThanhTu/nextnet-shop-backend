using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using net_backend.Modules.SubCategories.Application.Commands;
using net_backend.Modules.SubCategories.Application.Queries;
using net_backend.Modules.SubCategories.Contracts;
using net_backend.Modules.SubCategories.Domain;

namespace net_backend.Modules.SubCategories;

[ApiController]
[Route("subcategories")]
public class SubCategoriesController(
    ListSubCategoriesHandler listHandler,
    GetSubCategoryByIdHandler getByIdHandler,
    CreateSubCategoryHandler createHandler,
    UpdateSubCategoryHandler updateHandler,
    DeleteSubCategoryHandler deleteHandler,
    ISubCategoryRepository repository) : ControllerBase
{
    [HttpGet("")]
    public async Task<ActionResult<List<SubCategoryDto>>> List(CancellationToken cancellationToken)
        => Ok(await listHandler.ExecuteAsync(cancellationToken));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<SubCategoryDto>> GetById(int id, CancellationToken cancellationToken)
        => Ok(await getByIdHandler.ExecuteAsync(id, cancellationToken));

    [HttpGet("{id:int}/image")]
    public async Task<IActionResult> GetImage(int id, CancellationToken cancellationToken)
    {
        var image = await repository.GetImageAsync(id, cancellationToken);
        if (image is null) return NotFound();
        return File(image, "image/jpeg");
    }

    [HttpPost("")]
    [Authorize(Policy = "Admin")]
    public async Task<ActionResult<SubCategoryDto>> Create(
        [FromBody] CreateSubCategoryRequest request,
        CancellationToken cancellationToken)
    {
        var created = await createHandler.ExecuteAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = "Admin")]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateSubCategoryRequest request,
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
