using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using OxfordOnline.Data;
using OxfordOnline.Models.Dto;
using OxfordOnline.Repositories;
using OxfordOnline.Repositories.Interfaces;
using OxfordOnline.Resources;
using OxfordOnline.Services;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// === Banco de dados (MySQL) ===
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(connectionString, new MySqlServerVersion(new Version(10, 5, 9))));

// === Autenticação JWT ===
builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)
            )
        };
    });

// === Controllers e Versionamento ===
//builder.Services.AddControllers();
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
});
builder.Services.AddVersionedApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});

// === Swagger ===
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    // Segurança JWT
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = EndPointsMessages.TokenFormatInvalid,
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
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
            Array.Empty<string>()
        }
    });
});

// === Injeção de dependência ===
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<ProductService>();

builder.Services.AddScoped<IImageRepository, ImageRepository>();
builder.Services.AddScoped<ImageService>();

builder.Services.AddScoped<IFtpService, FtpService>();
builder.Services.AddScoped<FtpService>();

builder.Services.AddScoped<IInventRepository, InventRepository>();
builder.Services.AddScoped<InventService>();

builder.Services.AddScoped<IOxfordRepository, OxfordRepository>();
builder.Services.AddScoped<OxfordService>();

builder.Services.AddScoped<ITaxInformationRepository, TaxInformationRepository>();
builder.Services.AddScoped<TaxInformationService>();

builder.Services.AddScoped<IFtpServiceOxServer, FtpServiceOxServer>();
builder.Services.AddScoped<FtpServiceOxServer>();

builder.Services.AddScoped<IInventDimRepository, InventDimRepository>();
builder.Services.AddScoped<InventDimService>();

builder.Services.AddScoped<IFtpRepository, FtpRepository>();
builder.Services.AddScoped<FtpService>();

builder.Services.Configure<FtpSettings>(builder.Configuration.GetSection("Ftp"));

builder.Services.AddScoped<IInventoryRepository, InventoryRepository>();
builder.Services.AddScoped<InventoryService>();

// === Produção ===
if (builder.Environment.IsProduction())
{
    builder.WebHost.UseUrls("http://0.0.0.0:5000");
}

// === Adicionar CORS ===
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactLocalhost", policy =>
    {
        policy.WithOrigins("http://localhost:5173") // endereço do frontend
              .AllowAnyHeader() // permite Authorization, Content-Type, etc.
              .AllowAnyMethod() // GET, POST, PUT, DELETE, OPTIONS
              .AllowCredentials(); // se precisar enviar cookies
    });
});
// === Adicionar CORS ===

var app = builder.Build();

// === Versões da API no Swagger ===
var apiVersionDescriptionProvider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();

app.UsePathBase("/API");

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    foreach (var desc in apiVersionDescriptionProvider.ApiVersionDescriptions)
    {
        c.SwaggerEndpoint($"/API/swagger/{desc.GroupName}/swagger.json", $"Oxford API {desc.GroupName.ToUpperInvariant()}");
    }

    c.RoutePrefix = "swagger";
});

// Redirecionamento para Swagger UI
app.Use(async (context, next) =>
{
    if (context.Request.Path == "/")
    {
        context.Response.Redirect("/API/swagger/index.html");
        return;
    }
    await next();
});

// === Usar CORS antes da autenticação ===
app.UseCors("AllowReactLocalhost");
// === Usar CORS antes da autenticação ===

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();