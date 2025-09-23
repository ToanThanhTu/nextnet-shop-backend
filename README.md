# Next Net Shop Backend

The backend API for Next Net Shop, built with .NET 9 Minimal API, Entity Framework Core, and PostgreSQL.

## Technology Stack

- **Framework**: .NET 9 Minimal API
- **ORM**: Entity Framework Core 9
- **Database**: PostgreSQL
- **Authentication**: JWT Bearer tokens with BCrypt password hashing
- **Documentation**: NSwag (Swagger/OpenAPI)
- **Deployment**: Fly.io with Docker
- **Development Tools**: CSharpier for code formatting

## Project Structure

```
net-backend/
├── Data/                  # Data models and DTOs
│   └── Types/            # Entity models and DTOs
│       ├── User.cs               # User entity
│       ├── Product.cs            # Product entity
│       ├── Category.cs           # Category entity
│       ├── CartItem.cs           # Cart item entity
│       ├── Order.cs              # Order entity
│       └── *DTO.cs              # Data Transfer Objects
├── Products/             # Product management endpoints
│   └── ProductsEndpoints.cs
├── Users/                # User authentication and management
│   ├── UsersEndpoints.cs
│   └── JwtTokenHelper.cs
├── Cart/                 # Shopping cart functionality
│   └── CartEndpoints.cs
├── Orders/               # Order processing
│   └── OrderEndpoints.cs
├── Categories/           # Category management
│   ├── CategoriesEndpoints.cs
│   └── SubCategoriesEndpoints.cs
├── ConfigureServices.cs  # Service configuration and DI
├── ConfigureApp.cs       # Application pipeline configuration
├── Program.cs            # Application entry point
└── appsettings.json      # Configuration settings
```

## Key Features

- 🔐 JWT-based authentication with secure password hashing
- 📦 Complete product management system
- 🛒 Shopping cart functionality
- 📋 Order processing and management
- 🏷️ Category and subcategory organization
- 📚 Automatic API documentation with Swagger
- 🐘 PostgreSQL database with Entity Framework
- 🔧 Modular endpoint organization
- 🏥 Health checks and monitoring
- 🐳 Docker containerization

## Development Commands

```bash
# Restore dependencies
dotnet restore

# Start development server
dotnet run

# Build the application
dotnet build

# Format code with CSharpier
dotnet csharpier .

# Run in watch mode (auto-restart on changes)
dotnet watch run
```

## Database Management

### Entity Framework Commands
```bash
# Add new migration
dotnet ef migrations add AddNewFeature

# Update database
dotnet ef database update

# Remove last migration
dotnet ef migrations remove

# Generate SQL script for deployment
dotnet ef migrations script

# Drop database (development only)
dotnet ef database drop
```

### Database Schema

#### Core Entities
- **Users**: User accounts, authentication, and profiles
- **Products**: Product catalog with descriptions, prices, and inventory
- **Categories**: Product categorization system
- **SubCategories**: Product sub-categorization
- **CartItems**: Shopping cart items linked to users
- **Orders**: Customer order records
- **OrderItems**: Individual items within orders

## API Endpoints

### Authentication (`/api/users`)
```
POST   /api/users/register     # User registration
POST   /api/users/login        # User login
POST   /api/users/logout       # User logout (clears JWT)
GET    /api/users/profile      # Get user profile (authenticated)
PUT    /api/users/profile      # Update user profile (authenticated)
```

### Products (`/api/products`)
```
GET    /api/products           # Get all products with pagination
GET    /api/products/{id}      # Get product by ID
POST   /api/products           # Create new product (admin)
PUT    /api/products/{id}      # Update product (admin)
DELETE /api/products/{id}      # Delete product (admin)
GET    /api/products/search    # Search products by query
```

### Categories (`/api/categories`)
```
GET    /api/categories                    # Get all categories
GET    /api/categories/{id}               # Get category by ID
GET    /api/categories/{id}/subcategories # Get subcategories
GET    /api/categories/{id}/products      # Get products in category
POST   /api/categories                    # Create category (admin)
PUT    /api/categories/{id}               # Update category (admin)
DELETE /api/categories/{id}               # Delete category (admin)
```

### Cart (`/api/cart`)
```
GET    /api/cart              # Get user's cart items
POST   /api/cart/items        # Add item to cart
PUT    /api/cart/items/{id}   # Update cart item quantity
DELETE /api/cart/items/{id}   # Remove item from cart
DELETE /api/cart             # Clear entire cart
```

### Orders (`/api/orders`)
```
GET    /api/orders            # Get user's order history
GET    /api/orders/{id}       # Get order details
POST   /api/orders            # Create new order from cart
PUT    /api/orders/{id}/status # Update order status (admin)
```

## Configuration

### Environment Variables

#### Development (appsettings.Development.json)
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=nextnetshop;User Id=postgres;Password=yourpassword;"
  },
  "JwtSettings": {
    "SecretKey": "your-very-long-secret-key-here",
    "Issuer": "NextNetShop",
    "Audience": "NextNetShopUsers",
    "ExpirationInMinutes": 60
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

#### Production (Environment Variables)
```bash
DATABASE_URL=postgresql://user:pass@host:port/dbname
JWT_SECRET_KEY=your-production-secret-key
JWT_ISSUER=NextNetShop
JWT_AUDIENCE=NextNetShopUsers
ASPNETCORE_ENVIRONMENT=Production
```

## Authentication & Security

### JWT Implementation
- **Token Generation**: Uses `JwtTokenHelper.cs` for creating and validating tokens
- **Password Security**: BCrypt for password hashing with salt
- **Token Expiration**: Configurable expiration time
- **Authorization**: Endpoint-level authorization with `[Authorize]` attributes

### Security Best Practices
- Passwords are hashed using BCrypt with automatic salt generation
- JWT tokens include user ID and role claims
- CORS configured for frontend domain only
- Input validation on all endpoints
- SQL injection protection through Entity Framework parameterization

## Data Transfer Objects (DTOs)

### Request DTOs
- `UserRegistrationDTO`: User registration data
- `UserLoginDTO`: Login credentials
- `ProductCreateDTO`: Product creation data
- `CartItemCreateDTO`: Cart item data

### Response DTOs
- `UserDTO`: User profile information (excluding sensitive data)
- `ProductDTO`: Product information for API responses
- `CategoryDTO`: Category information
- `OrderDTO`: Order information with items

## Error Handling

The API implements consistent error handling:
- **400 Bad Request**: Invalid input data
- **401 Unauthorized**: Missing or invalid authentication
- **403 Forbidden**: Insufficient permissions
- **404 Not Found**: Resource not found
- **409 Conflict**: Resource conflicts (e.g., duplicate email)
- **500 Internal Server Error**: Server errors

## Performance Considerations

- **Database Indexing**: Proper indexes on frequently queried fields
- **Lazy Loading**: Entity Framework configured for optimal loading
- **Pagination**: Large result sets are paginated
- **Caching**: HTTP response caching for static data
- **Connection Pooling**: Database connection pooling enabled

## Testing

### Unit Testing (when implemented)
```bash
# Run all tests
dotnet test

# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage"
```

### API Testing
- Use the Swagger UI at `http://localhost:8080/swagger` for interactive testing
- HTTP file provided: `net-backend.http` for VS Code REST Client
- Postman collection can be generated from Swagger documentation

## Deployment

### Docker Build
```bash
# Build Docker image
docker build -t nextnet-backend .

# Run container
docker run -p 8080:8080 nextnet-backend
```

### Fly.io Deployment
```bash
# Deploy to Fly.io
fly deploy

# View logs
fly logs

# SSH into container
fly ssh console
```

## Development Guidelines

### Code Style
- Follow C# naming conventions (PascalCase for public members)
- Use CSharpier for code formatting
- Include XML documentation for public APIs
- Use meaningful variable and method names

### Adding New Endpoints
1. Create endpoint class in appropriate feature folder
2. Define route pattern and HTTP methods
3. Create required DTOs in `Data/Types/`
4. Implement business logic with proper error handling
5. Add authentication attributes if required
6. Update this documentation

### Database Changes
1. Modify entity models in `Data/Types/`
2. Create migration: `dotnet ef migrations add <MigrationName>`
3. Review generated migration for correctness
4. Update database: `dotnet ef database update`
5. Update related DTOs and endpoints

## Troubleshooting

### Common Issues
1. **Database Connection**: Check connection string in appsettings
2. **JWT Errors**: Verify secret key configuration and token format
3. **CORS Issues**: Ensure frontend URL is included in CORS policy
4. **Migration Errors**: Check Entity Framework model configuration

### Debugging Tips
- Use `dotnet watch run` for automatic restarts during development
- Check logs in console output for detailed error information
- Use Swagger UI for testing endpoints
- Verify database connectivity with `dotnet ef database update`

## Contributing

1. Follow the coding standards in `../CLAUDE.md`
2. Write unit tests for new functionality
3. Update this documentation for new features
4. Use meaningful commit messages
5. Test endpoints thoroughly before committing