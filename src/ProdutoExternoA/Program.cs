using Microsoft.FeatureManagement;
using MongoDB.Driver;
using ProdutoExternoA.Infrastructure;
using ProdutoExternoA.Service;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IMongoClient>(sp =>
    new MongoClient(builder.Configuration.GetConnectionString("MongoDb") ?? "mongodb://localhost:27017"));

builder.Services.AddSingleton<IMongoDatabase>(sp =>
    sp.GetRequiredService<IMongoClient>().GetDatabase("DesafioTecnico"));

builder.Services.AddSingleton<IRabbitMqPublisher, RabbitMqPublisher>();

builder.Services.AddFeatureManagement();

builder.Services.AddScoped<IPedidoService, PedidoService>();
builder.Services.AddScoped<ICalculadoraImpostoFactory, CalculadoraImpostoFactory>();

builder.Services.AddScoped<IPedidoRepository, PedidoRepository>();

builder.Services.AddTransient<MongoDbInitializer>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    try
    {
        var initializer = services.GetRequiredService<MongoDbInitializer>();
        await initializer.InicializarAsync();
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Ocorreu um erro na migração do banco de dados.");
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();

app.MapControllers();

app.Run();