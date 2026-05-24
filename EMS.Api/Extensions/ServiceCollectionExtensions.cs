namespace EMS.Api.Extensions;

using EMS.Application.DTOs.Auth;
using EMS.Application.Mappers;
using EMS.Application.Services.Organisation;
using EMS.Application.Validators.Auth;
using FluentValidation;
using Microsoft.OpenApi.Models;

/// <summary>
/// API-level service collection extensions
/// Registers validators, AutoMapper, Swagger, and CORS
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Add API services
    /// </summary>
    public static IServiceCollection AddApiServices(this IServiceCollection services)
    {
        // FluentValidation
        AddFluentValidation(services);

        // AutoMapper
        AddAutoMapper(services);

        // Swagger/OpenAPI
        AddSwagger(services);

        // CORS
        AddCors(services);

        // Organisation
        AddOrganisation(services);

        // Controllers
        services.AddControllers();

        return services;
    }

    /// <summary>
    /// Register FluentValidation validators
    /// </summary>
    private static void AddFluentValidation(IServiceCollection services)
    {
        
        services.AddScoped<IValidator<RegisterRequest>, RegisterValidator>();
        services.AddScoped<IValidator<LoginRequest>, LoginValidator>();
    }

    /// <summary>
    /// Register AutoMapper
    /// </summary>
    private static void AddAutoMapper(IServiceCollection services)
    {
        services.AddAutoMapper(typeof(MappingProfile));
    }

    /// <summary>
    /// Register Swagger/OpenAPI documentation
    /// </summary>
    private static void AddSwagger(IServiceCollection services)
    {
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "EMS API",
                Version = "v1",
                Description = "Educational Management System REST API",
                Contact = new OpenApiContact
                {
                    Name = "EMS Support",
                    Email = "support@ems.com"
                },
                License = new OpenApiLicense
                {
                    Name = "MIT",
                    Url = new Uri("https://opensource.org/licenses/MIT")
                }
            });

            // Add bearer token authentication
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "JWT Authorization header using the Bearer scheme"
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
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
                    new string[] { }
                }
            });

            // XML documentation
            var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            if (File.Exists(xmlPath))
                options.IncludeXmlComments(xmlPath);
        });
    }

    /// <summary>
    /// Register CORS
    /// </summary>
    private static void AddCors(IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddPolicy("AllowAll", builder =>
                builder.AllowAnyOrigin()
                       .AllowAnyMethod()
                       .AllowAnyHeader());

            options.AddPolicy("AllowSpecific", builder =>
                builder.WithOrigins("http://localhost:3000", "http://localhost:4200")
                       .AllowAnyMethod()
                       .AllowAnyHeader()
                       .AllowCredentials());
        });
    }

    private static void AddOrganisation(IServiceCollection services)
    {
        // Organisation related services can be registered here in the future
        services.AddScoped<EMS.Application.Interfaces.Repositories.Organisation.IOrganisationRepository, EMS.Infrastructure.Repositories.Organisation.OrganisationRepository>();
        services.AddScoped<OrganisationService>();

    }
}
