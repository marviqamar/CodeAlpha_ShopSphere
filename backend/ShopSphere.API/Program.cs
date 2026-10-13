using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ShopSphere.Application.Interfaces;
using ShopSphere.Application.Settings;
using ShopSphere.Domain.Entities;
using ShopSphere.Infrastructure.Persistence;
using System.Text;
using ShopSphere.API.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Bind JwtSettings from appsettings.json into the JwtSettings class
var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>()!;
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));

// Register Identity (user/role management + password hashing)
builder.Services.AddIdentity<User, IdentityRole<int>>(options =>
{
    options.Password.RequiredLength = 8;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireDigit = true;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

// Register JWT authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidAudience = jwtSettings.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret))
    };
});

builder.Services.AddScoped<IAuthService, ShopSphere.Infrastructure.Services.AuthService>();
builder.Services.AddScoped<ICategoryService, ShopSphere.Infrastructure.Services.CategoryService>();
builder.Services.AddScoped<IProductService, ShopSphere.Infrastructure.Services.ProductService>();
builder.Services.AddScoped<ICartService, ShopSphere.Infrastructure.Services.CartService>();
builder.Services.AddScoped<IOrderService, ShopSphere.Infrastructure.Services.OrderService>();
builder.Services.AddScoped<IReviewService, ShopSphere.Infrastructure.Services.ReviewService>();
builder.Services.AddScoped<IWishlistService, ShopSphere.Infrastructure.Services.WishlistService>();
builder.Services.AddScoped<ICouponService, ShopSphere.Infrastructure.Services.CouponService>();
builder.Services.AddScoped<IDashboardService, ShopSphere.Infrastructure.Services.DashboardService>();
builder.Services.AddScoped<IFileUploadService, ShopSphere.Infrastructure.Services.FileUploadService>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularApp", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddAuthorization();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Seeding the admin and test user account
using (var scope = app.Services.CreateScope())
{
    await ShopSphere.Infrastructure.Persistence.Seed.DbSeeder.SeedAsync(scope.ServiceProvider);
}

//Adding Exception Handling
using (var scope = app.Services.CreateScope())
{
    await ShopSphere.Infrastructure.Persistence.Seed.DbSeeder.SeedAsync(scope.ServiceProvider);
}

app.UseMiddleware<ExceptionHandlingMiddleware>();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseCors("AllowAngularApp");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
