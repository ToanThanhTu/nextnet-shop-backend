using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// adds the database context to the dependency injection (DI) container
builder.Services.AddDbContext<ItemDb>(opt =>
  opt.UseInMemoryDatabase("ItemList")
);

// enables displaying database-related exceptions
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// Enables the API Explorer, provides metadata about the HTTP API
builder.Services.AddEndpointsApiExplorer();

// Adds the Swagger OpenAPI document generator
builder.Services.AddOpenApiDocument(config =>
{
  config.DocumentName = "ItemAPI";
  config.Title = "ItemAPI v1";
  config.Version = "v1";
});

var app = builder.Build();

// Enable Swagger middleware for testing in development
if (app.Environment.IsDevelopment())
{
  app.UseOpenApi();
  app.UseSwaggerUi(config =>
  {
    config.DocumentTitle = "ItemAPI";
    config.Path = "/swagger";
    config.DocumentPath = "/swagger/{documentName}/swagger.json";
    config.DocExpansion = "list";
  });
}

var items = app.MapGroup("/items");

items.MapGet("/", GetAllItems);
items.MapGet("/sold", GetSoldItems);
items.MapGet("/{id}", GetItem);
items.MapPost("/", CreateItem);
items.MapPut("/{id}", UpdateItem);
items.MapDelete("/{id}", DeleteItem);

app.Run();

// Using TypedResults to verify the return type is correct
// Advantages:
// - testability
// - automatically returning the response type metadata for OpenAPI to describe the endpoint
static async Task<IResult> GetAllItems(ItemDb db)
{
  return TypedResults.Ok(
    await db.Items.Select(x => new ItemDTO(x)).ToArrayAsync()
  );
}

static async Task<IResult> GetSoldItems(ItemDb db)
{
  return TypedResults.Ok(
    await db
      .Items.Where(i => i.IsSold)
      .Select(x => new ItemDTO(x))
      .ToListAsync()
  );
}

static async Task<IResult> GetItem(int id, ItemDb db)
{
  return await db.Items.FindAsync(id) is Item item
    ? TypedResults.Ok(new ItemDTO(item))
    : TypedResults.NotFound();
}

static async Task<IResult> CreateItem(ItemDTO itemDTO, ItemDb db)
{
  var item = new Item
  {
    Name = itemDTO.Name,
    Price = itemDTO.Price,
    IsSold = itemDTO.IsSold,
  };

  db.Items.Add(item);
  await db.SaveChangesAsync();

  itemDTO = new ItemDTO(item);

  return TypedResults.Created($"/items/{item.Id}", itemDTO);
}

static async Task<IResult> UpdateItem(int id, ItemDTO itemDTO, ItemDb db)
{
  var item = await db.Items.FindAsync(id);

  if (item is null)
    return TypedResults.NotFound();

  item.Name = itemDTO.Name;
  item.Price = itemDTO.Price;
  item.IsSold = itemDTO.IsSold;

  await db.SaveChangesAsync();

  return TypedResults.NoContent();
}

static async Task<IResult> DeleteItem(int id, ItemDb db)
{
  if (await db.Items.FindAsync(id) is Item item)
  {
    db.Items.Remove(item);
    await db.SaveChangesAsync();
    return TypedResults.NoContent();
  }

  return TypedResults.NotFound();
}
