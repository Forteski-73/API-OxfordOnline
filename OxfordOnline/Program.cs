using System.ServiceModel;
using Microsoft.EntityFrameworkCore;
using OxfordOnline.Data;
using Oxfordonline.Integration;

var builder = WebApplication.CreateBuilder(args);

//builder.AddServiceDefaults();

// Configurar Entity Framework
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(connectionString, new MySqlServerVersion(new Version(10, 5, 9))));

// Adicionar WSIntegratorServices como um serviço Singleton
/*builder.Services.AddSingleton<WSIntegratorServices>(provider =>
{
    var binding = new BasicHttpBinding();
    var endpoint = new EndpointAddress("http://ax201203:8201/DynamicsAx/Services/WSIntegratorServices");
    return new WSIntegratorServices(binding, endpoint);
});*/

builder.Services.AddSingleton<Oxfordonline.Integration.ProductServicesClient>(provider =>
{
    var binding = new BasicHttpBinding();
    var endpoint = new EndpointAddress("http://ax201203:8201/DynamicsAx/Services/WSIntegratorServices");
    return new Oxfordonline.Integration.ProductServicesClient(binding, endpoint);
});

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var environment = builder.Environment.EnvironmentName;
if (environment == "Production")  // Ambiente de Produção
{
    builder.WebHost.UseUrls("http://0.0.0.0:80");
}

var app = builder.Build();

//app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment()) // ambiente de desenvolvimento
//{
    app.UseSwagger();
    app.UseSwaggerUI();
//}

app.UseHttpsRedirection();

app.Use(async (context, next) =>
{
    var tokenConfigurado = builder.Configuration["AuthToken"];
    var tokenEnviado = context.Request.Headers["Authorization"].FirstOrDefault();

    if (string.IsNullOrEmpty(tokenEnviado) || tokenEnviado != $"Bearer {tokenConfigurado}")
    {
        context.Response.StatusCode = 401;
        await context.Response.WriteAsync("Token inválido ou ausente.");
        return;
    }

    await next();
});

app.UseAuthorization();

app.MapControllers();

app.Run();
