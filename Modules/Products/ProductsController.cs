using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using net_backend.Common.Auth;
using net_backend.Products.Application.Commands;
using net_backend.Products.Application.Queries;
using net_backend.Products.Contracts;
using net_backend.Products.Domain;

namespace net_backend.Products;

[ApiController]
[Route("products")]
public class ProductsController(
    ListProductsHandler listHandler,
    ListOnSaleProductsHandler listOnSaleHandler,
    ListBestsellersHandler listBestsellersHandler,
    GetTopDealsHandler topDealsHandler,
    SearchProductsHandler searchHandler,
    GetProductByIdHandler getByIdHandler,
    GetProductBySlugHandler getBySlugHandler,
    GetSimilarProductsHandler similarHandler,
    GetPersonalRecommendationsHandler personalRecommendationsHandler,
    CreateProductHandler createHandler,
    UpdateProductHandler updateHandler,
    DeleteProductHandler deleteHandler,
    IProductRepository repository) : ControllerBase
{
    [HttpGet("all")]
    public async Task<ActionResult<ProductListPageDto>> List(
        [FromQuery] ProductListQuery query, CancellationToken cancellationToken)
        => Ok(await listHandler.ExecuteAsync(query, cancellationToken));

    [HttpGet("sales")]
    public async Task<ActionResult<ProductListPageDto>> ListOnSale(
        [FromQuery] ProductListQuery query, CancellationToken cancellationToken)
        => Ok(await listOnSaleHandler.ExecuteAsync(query, cancellationToken));

    [HttpGet("bestsellers")]
    public async Task<ActionResult<ProductListPageDto>> ListBestsellers(
        [FromQuery] ProductListQuery query, CancellationToken cancellationToken)
        => Ok(await listBestsellersHandler.ExecuteAsync(query, cancellationToken));

    [HttpGet("top-deals")]
    public async Task<ActionResult<List<ProductDto>>> TopDeals(CancellationToken cancellationToken)
        => Ok(await topDealsHandler.ExecuteAsync(cancellationToken));

    [HttpGet("search")]
    public async Task<ActionResult<List<ProductDto>>> Search(
        [FromQuery] string query, CancellationToken cancellationToken)
        => Ok(await searchHandler.ExecuteAsync(query, cancellationToken));

    [HttpGet("recommendations/{productId:int}")]
    public async Task<ActionResult<List<ProductDto>>> Similar(
        int productId, CancellationToken cancellationToken)
        => Ok(await similarHandler.ExecuteAsync(productId, cancellationToken));

    /// <summary>
    /// Personalised recommendations for the authenticated user. The user id
    /// comes from the JWT claim — never a path parameter.
    /// </summary>
    [HttpGet("personal-recommendations")]
    [Authorize]
    public async Task<ActionResult<List<ProductDto>>> PersonalRecommendations(
        CancellationToken cancellationToken)
    {
        var userId = User.GetRequiredUserId();
        return Ok(await personalRecommendationsHandler.ExecuteAsync(userId, cancellationToken));
    }

    [HttpGet("id/{id:int}")]
    public async Task<ActionResult<ProductDto>> GetById(
        int id, CancellationToken cancellationToken)
        => Ok(await getByIdHandler.ExecuteAsync(id, cancellationToken));

    [HttpGet("slug/{slug}")]
    public async Task<ActionResult<ProductWithHierarchyDto>> GetBySlug(
        string slug, CancellationToken cancellationToken)
        => Ok(await getBySlugHandler.ExecuteAsync(slug, cancellationToken));

    [HttpGet("{id:int}/image")]
    public async Task<IActionResult> GetImage(int id, CancellationToken cancellationToken)
    {
        var bytes = await repository.GetImageAsync(id, cancellationToken);
        if (bytes is null) return NotFound();
        return File(bytes, "image/jpeg");
    }

    [HttpPost("")]
    [Authorize(Policy = "Admin")]
    public async Task<ActionResult<ProductDto>> Create(
        [FromBody] CreateProductRequest request,
        CancellationToken cancellationToken)
    {
        var created = await createHandler.ExecuteAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("id/{id:int}")]
    [Authorize(Policy = "Admin")]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateProductRequest request,
        CancellationToken cancellationToken)
    {
        await updateHandler.ExecuteAsync(id, request, cancellationToken);
        return NoContent();
    }

    [HttpDelete("id/{id:int}")]
    [Authorize(Policy = "Admin")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await deleteHandler.ExecuteAsync(id, cancellationToken);
        return NoContent();
    }
}
