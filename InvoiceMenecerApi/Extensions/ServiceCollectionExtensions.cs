using FluentValidation;
using FluentValidation.AspNetCore;
using InvoiceMenecerApi.Config;
using InvoiceMenecerApi.Mapping;
using InvoiceMenecerApi.Models;
using InvoiceMenecerApi.Services;
using InvoiceMenecerApi.Services.Interfaces;
using InvoiceMenecerApi.Validators;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Reflection;
using System.Text;

namespace InvoiceMenecerApi.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSwagger(
        this IServiceCollection services)
    {
        services.AddControllers();
        services.AddOpenApi();
        services.AddSwaggerGen(
                 options =>
                 {
                     options.SwaggerDoc("v1",
                     new OpenApiInfo
                     {
                         Version = "v1",
                         Title = "Invoice Manager API",
                         Description = "API for managing customers and invoices",
                         Contact = new OpenApiContact
                         {
                             Name = "InvoiceTeam",
                             Email = "support@invoiceteam.com"
                         },
                         License = new OpenApiLicense
                         {
                             Name = "MIT License",
                             Url = new Uri("https://opensource.org/licenses/mit")
                         }
                     });

                     var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                     var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                     if (File.Exists(xmlPath))
                     {
                         options.IncludeXmlComments(xmlPath);
                     }

                     // JWT options for Swaggeer
                     options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                     {
                         Description = """
                JWT Suthorization header using the Bearer scheme. 
                Example: Authorization: Bearer {token}
                """,
                         Name = "Authorization",
                         In = ParameterLocation.Header,
                         Type = SecuritySchemeType.ApiKey,
                         Scheme = "Bearer"
                     });

                     options.AddSecurityRequirement(
                         new OpenApiSecurityRequirement
                         {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id="Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                         });
                 });

        return services;
    }

    public static IServiceCollection AddTaskFlowDbContext(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnectionString");
        services.AddDbContext<InvoiceMenecerDBContext>(
            options => options.UseSqlServer(connectionString)
            );
        return services;
    }

    public static IServiceCollection AddIdentityAndDb(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<JwtConfig>(configuration.GetSection(JwtConfig.SectionName));
        services.AddIdentity<ApplicationUser, IdentityRole>(
    options =>
    {
        options.Password.RequireDigit = false;
        options.Password.RequireLowercase = false;
        options.Password.RequireUppercase = false;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequiredLength = 6;

        options.User.RequireUniqueEmail = true;
        options.SignIn.RequireConfirmedEmail = false;
    }

)
    .AddEntityFrameworkStores<InvoiceMenecerDBContext>()
    .AddDefaultTokenProviders();
        return services;
    }

    public static IServiceCollection AddJwtAuthenticationAndAuthorization(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var jwtConfig = new JwtConfig();
        configuration.GetSection("JwtSettings")
                                .Bind(jwtConfig);


        services.AddAuthentication(
            options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }
            )
            .AddJwtBearer(
                options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = jwtConfig.Issuer,
                        ValidAudience = jwtConfig.Audience,
                        IssuerSigningKey =
                            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtConfig.SecretKey!)),
                        ClockSkew = TimeSpan.Zero
                    };
                }
            );

        services.AddAuthorization(
            options =>
            {
                options
                .AddPolicy(
                    "AdminOnly",
                    policy =>
                        policy.RequireRole("Admin"));
                options
                .AddPolicy(
                    "AdminOrManager",
                    policy =>
                        policy.RequireRole("Admin", "Manager"));
                options
                .AddPolicy(
                    "UserOrAbove",
                    policy =>
                        policy.RequireRole("Admin", "Manager", "User"));
            }
            );

        return services;
    }

    public static IServiceCollection AddCorsPolicy(
        this IServiceCollection services)
    {
        services.AddCors(
    options =>
    {
        options.AddDefaultPolicy(
            policy =>
            {
                policy.WithOrigins(
                    "http://localhost:3000",
                    "http://127.0.0.1:3000"
                    )
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
            }
            );
    }
    );
        return services;
    }

    public static IServiceCollection AddFluentValidation(
        this IServiceCollection services)
    {

        services.AddValidatorsFromAssemblyContaining<RegisterValidator>();
        services.AddFluentValidationAutoValidation();
        return services;
    }

    public static IServiceCollection AddAutoMapperAndOtherDI(
       this IServiceCollection services)
    {
        services.AddAutoMapper(typeof(MappingProfile));

        // Services
        services.AddScoped<IInvoiceService, InvoiceService>();
        services.AddScoped<ICustomerService, CustomerService>();
        services.AddScoped<IInvoiceRowService, InvoiceRowService>();
        services.AddScoped<IAuthService, AuthService>();
        return services;
    }
}
