using System.Text;
using ApiAuth.Data.Repositories;
using ApiAuth.Filters;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Shared.Interfaces;
using Shared.Middlewares;
using Shared.Services;
using Shared.Settings;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.Configure<AppSettings>(builder.Configuration);

builder.Services.AddSingleton<IUserRepository, UserRepository>();

builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IEmailService, EmailService>();

SwaggerConfig();
AuthenticationConfig();

// Esse filtro não é necessário, pois o próprio framework com a biblioteca Microsoft.AspNetCore.Authentication.JwtBearer já faz a validação do token, expiração, roles, etc.
// Adicionamos ele apenas para enriquecer o contexto da requisição com informações do usuário.
builder.Services.AddControllers(options =>
{
    options.Filters.Add<AuthorizationFilter>();
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();


void SwaggerConfig()
{
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new() { Title = "Pfm.Api", Version = "v1" });
        c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Description = @"
                Bearer {token}",
            Name = "Authorization",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.ApiKey,
            Scheme = "Bearer"
        });
        c.AddSecurityRequirement(new OpenApiSecurityRequirement()
                    {
                        {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            },
                            Scheme = "oauth2",
                            Name = "Bearer",
                            In = ParameterLocation.Header,

                            },
                            new List<string>()
                        }
                    });
    });
}


void AuthenticationConfig()
{
    var appSettings = builder?.Services?.BuildServiceProvider()?.GetService<IOptions<AppSettings>>()?.Value ?? throw new ArgumentNullException(nameof(AppSettings));

    var jwtKey = appSettings.JwtConfig.Secret;
    var key = Encoding.ASCII.GetBytes(jwtKey);
    builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.RequireHttpsMetadata = appSettings.JwtConfig.RequireHttpsMetadata;
            options.SaveToken = appSettings.JwtConfig.SaveToken;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuerSigningKey = appSettings.JwtConfig.ValidateIssuerSigningKey,
                ValidateLifetime = appSettings.JwtConfig.ValidateLifetime,
                ValidateIssuer = appSettings.JwtConfig.ValidateIssuer,
                ValidIssuer = appSettings.JwtConfig.Issuer,
                ValidateAudience = appSettings.JwtConfig.ValidateAudience,
                ValidAudience = appSettings.JwtConfig.Audience,
                ClockSkew = TimeSpan.Zero
            };
        });
}