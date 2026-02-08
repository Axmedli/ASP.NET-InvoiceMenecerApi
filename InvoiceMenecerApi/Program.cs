using ASP_NET_08.Mapping;
using FluentValidation;
using FluentValidation.AspNetCore;
using InvoiceMenecerApi.Services;
using InvoiceMenecerApi.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen(
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
                Name = "TaskFlow Team",
                Email = "support@taskflow.com"
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

    }
);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnectionString");
builder.Services.AddDbContext<InvoiceMenecerDBContext>(
    options => options.UseSqlServer(connectionString)   
    );

builder.Services.AddAutoMapper(typeof(MappingProfile));

builder.Services.AddScoped<IInvoiceService, InvoiceService>();
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<IInvoiceRowService, InvoiceRowService>();

builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(
        options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "TaskFlow API v1");
            options.RoutePrefix = string.Empty;
            options.EnableFilter();
            options.EnableTryItOutByDefault();
            options.DisplayRequestDuration();
        }
        );
    app.MapOpenApi();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
