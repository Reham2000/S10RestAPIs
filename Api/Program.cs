using Core.Authorization;
using Core.Interfaces;
using Core.Middelware;
using Core.Services;
using Domain.DTos;
using Infrastructure.Data;
using Infrastructure.Implements;
using Infrastructure.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// add Db connection
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("con")));

// add identity
builder.Services.AddIdentity<Domain.Models.User, IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped(typeof(IGenericReposatory<>), typeof(GenericRepository<>));
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITokenService, TokenService>();


builder.Services.AddScoped<IProductService, ProductService>();


//builder.Services.Configure<Jwt>(builder.Configuration.GetSection("JwtSettings"));
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
builder.Services.Configure<Jwt>(jwtSettings);
builder.Services.AddSingleton(jwtSettings);

// add policy 

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminPolicy",policy => policy.Requirements.Add(
        new CustomAuthorizationRequirement(new List<string> {"Admin" })));
    options.AddPolicy("UserPolicy", policy => policy.Requirements.Add(
        new CustomAuthorizationRequirement(new List<string> { "User" })));
    options.AddPolicy("ManagerPolicy", policy => policy.Requirements.Add(
        new CustomAuthorizationRequirement(new List<string> { "Manager" })));
    options.AddPolicy("AllPolicy", policy => policy.Requirements.Add(
        new CustomAuthorizationRequirement(new List<string> { "Admin","User","Magager" })));
});








var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

using(var scope = app.Services.CreateScope())
{
    var service = scope.ServiceProvider;
    await DbInitalizer.SeedRolesAsync(service);
}



app.UseMiddleware<TokenRevocation>();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
