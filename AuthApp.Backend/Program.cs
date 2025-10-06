using AuthApp.Backend.AuthApp.Backend.Domain;
using AuthApp.Backend.AuthApp.Backend.EFCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ============================================
// ✅ Add services
// ============================================
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ✅ Database + Identity setup
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);

builder.Services.AddIdentityApiEndpoints<ApplicationUser>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

//password configuration
builder.Services.Configure<IdentityOptions>(options =>
{
    options.Password.RequireDigit = true;             // Require at least one digit
    options.Password.RequireLowercase = true;         // Require at least one lowercase
    options.Password.RequireUppercase = true;         // Require at least one uppercase
    options.Password.RequireNonAlphanumeric = true;  // Require at least one special character
    options.Password.RequiredLength = 8;             // Minimum 8 characters
    options.Password.RequiredUniqueChars = 1;        // At least 1 unique character
    options.User.RequireUniqueEmail = true;
});

// ✅ Add CORS Policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy
            .WithOrigins("http://localhost:4200") // 👈 Angular dev server
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials(); // use if frontend sends auth cookies/tokens
    });
});

// Add Authentication with JWT
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    var jwtSettings = builder.Configuration.GetSection("Jwt");
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtSettings["Key"]!)
        )
    };
});
var app = builder.Build();

// ============================================
// ✅ Middleware pipeline
// ============================================

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// ✅ Use CORS before Authentication/Authorization
app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();

// ✅ Map endpoints
app.MapControllers();
app.MapGroup("/api").MapIdentityApi<ApplicationUser>();

app.Run();
