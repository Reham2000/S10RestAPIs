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


builder.Services.Configure<Jwt>(builder.Configuration.GetSection("JwtSettings"));
var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<Jwt>();
//builder.Services.Configure<Jwt>(jwtSettings);
builder.Services.AddSingleton(jwtSettings);


// jwt settings
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidAudience = jwtSettings.Audience,
        // key
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secretkey))
    };
    // logout
    options.Events = new JwtBearerEvents
    {
        OnTokenValidated = async context =>
        {
            var revocedToken = context.HttpContext.RequestServices
            .GetRequiredService<IRevokedTokenRepository>();
            var jti = context.Principal.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;
            if (jti is not null && await revocedToken.IsTokenRevokedAsync(jti))
                context.Fail("The Token Has been Revoced!");
        }
    };
});



// add policy 

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminPolicy",policy => policy.Requirements.Add(
        new CustomAuthorizationRequirement(new List<string> {"Admin" })));
    options.AddPolicy("UserPolicy", policy => policy.Requirements.Add(
        new CustomAuthorizationRequirement(new List<string> { "User" })));
    options.AddPolicy("ManagerPolicy", policy => policy.Requirements.Add(
        new CustomAuthorizationRequirement(new List<string> { "Manager" })));
    options.AddPolicy("AdminManagerPolicy", policy => policy.Requirements.Add(
        new CustomAuthorizationRequirement(new List<string> { "Admin", "Magager" })));
    options.AddPolicy("AllPolicy", policy => policy.Requirements.Add(
        new CustomAuthorizationRequirement(new List<string> { "Admin","User","Magager" })));
});



builder.Services.AddHttpContextAccessor();




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
