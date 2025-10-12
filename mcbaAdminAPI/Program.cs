using mcbaMVC.Data;
using Microsoft.EntityFrameworkCore;
using mcbaAdminAPI.Repositories;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// Adding EF Core
builder.Services.AddDbContext<MCBAContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("MCBAConnection")));

// Adding CORS (so Admin Portal can access API)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowPortal",
        policy => policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

// Adding JWT Authentication
var jwtSettings = builder.Configuration.GetSection("Jwt"); // reads from appsettings.json

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSettings["Key"]!))
        };
    });

// Registering repositories
builder.Services.AddScoped<IPayeeRepository, PayeeRepository>();
builder.Services.AddScoped<IBillPayRepository, BillPayRepository>();

// Adding Swagger + JWT Authorize support
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "MCBA Admin API",
        Version = "v1",
        Description = "Secure API for managing payees and bill payments in the MCBA Admin Portal"
    });

    // Adding JWT Bearer token support
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter 'Bearer {your JWT token}' below."
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

var app = builder.Build();

// Enable Swagger UI
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "MCBA Admin API v1");
    c.RoutePrefix = string.Empty; // Open Swagger UI at root URL
});

app.UseHttpsRedirection();

// Adding Authentication + Authorization middleware
app.UseCors("AllowPortal");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
