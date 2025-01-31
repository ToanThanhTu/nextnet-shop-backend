using Microsoft.EntityFrameworkCore;
using net_backend.Data.Types;

namespace net_backend.Users;

public static class UsersEndpoints
{
    public static void RegisterUsersEndpoints(this WebApplication app)
    {
        var users = app.MapGroup("/users");

        users.MapGet("/", GetUsers);
        users.MapGet("/id/{id}", GetUserById);
        users.MapPost("/register", RegisterUser);
        users.MapPost("/admin/create", CreateAdmin);
        users.MapPost("/login", Login);

        static async Task<IResult> GetUsers(AppDbContext db)
        {
            var users = await db.Users.Select(u => new UserDTO
            {
                Id = u.Id,
                Name = u.Name,
                Email = u.Email,
                Role = u.Role
            }).ToArrayAsync();

            return TypedResults.Ok(users);
        }

        static async Task<IResult> GetUserById(int id, AppDbContext db)
        {
            var user = await db.Users
                .Where(u => u.Id == id)
                .Select(u => new UserDTO
                {
                    Id = u.Id,
                    Name = u.Name,
                    Email = u.Email,
                    Role = u.Role
                }).FirstOrDefaultAsync();

            if (user is null)
                return TypedResults.NotFound();

            return TypedResults.Ok(user);
        }

        static async Task<IResult> RegisterUser(UserRegistration userRegistration, AppDbContext db)
        {
            if (await db.Users.AnyAsync(u => u.Email == userRegistration.Email))
                return TypedResults.BadRequest("Email is already used, please use a different email or log in with your email");

            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(userRegistration.Password);

            var user = new User
            {
                Name = userRegistration.Name,
                Email = userRegistration.Email,
                PasswordHash = hashedPassword,
                Role = "User"
            };

            db.Users.Add(user);
            await db.SaveChangesAsync();

            return TypedResults.Created(
              $"/users/{user.Id}",
              user
            );
        }

        static async Task<IResult> CreateAdmin(UserRegistration userRegistration, AppDbContext db)
        {
            if (await db.Users.AnyAsync(u => u.Email == userRegistration.Email))
                return TypedResults.BadRequest("Email is already used, please use a different email or log in with your email");

            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(userRegistration.Password);

            var user = new User
            {
                Name = userRegistration.Name,
                Email = userRegistration.Email,
                PasswordHash = hashedPassword,
                Role = "Admin"
            };

            db.Users.Add(user);
            await db.SaveChangesAsync();

            return TypedResults.Created(
              $"/users/{user.Id}",
              user
            );
        }

        static async Task<IResult> Login(UserLogin userLogin, AppDbContext db)
        {
            var user = await db.Users
                .Where(u => u.Email == userLogin.Email)
                .Include(u => u.CartItems!)
                    .ThenInclude(ci => ci.Product)
                .Include(u => u.Orders)
                .FirstOrDefaultAsync();

            if (user is null)
                return TypedResults.NotFound("User not found");

            if (!BCrypt.Net.BCrypt.Verify(userLogin.Password, user.PasswordHash))
                return TypedResults.Unauthorized();

            var userToken = JwtTokenHelper.GenerateToken(user); // Generate JWT
            var userDto = new UserDTO
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                Role = user.Role,
                CartItems = user.CartItems?.Select(ci => new CartItemDTO
                {
                    Id = ci.Id,
                    UserId = ci.UserId,
                    ProductId = ci.ProductId,
                    Quantity = ci.Quantity,
                    Product = ci.Product
                }).ToList() ?? [],
                Orders = user.Orders?.Select(o => new OrderDTO
                {
                    Id = o.Id,
                    OrderDate = o.OrderDate,
                    TotalPrice = o.TotalPrice,
                    Status = o.Status,
                    OrderItems = o.OrderItems?
                        .Select(oi => new OrderItemDTO
                        {
                            Id = oi.Id,
                            ProductId = oi.ProductId,
                            Quantity = oi.Quantity,
                            Price = oi.Price,
                            Product = oi.Product ?? null,
                        }).ToList() ?? []
                }).ToList() ?? []
            };

            return TypedResults.Ok(new
            {
                userDto,
                userToken,
            });
        }
    }
}
