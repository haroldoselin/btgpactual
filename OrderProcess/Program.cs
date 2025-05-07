using MongoDB.Driver;
using OrderProcess.Infrastructure.Repositories;
using OrderProcess.Infrastructure.Services;
using static OrderProcess.Infrastructure.Services.RabbitMQConsumerService;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<RabbitMQSettings>(builder.Configuration.GetSection("RabbitMQ"));

builder.Services.AddSingleton<IMongoDatabase>(sp =>
{
    var mongoConnection = builder.Configuration["MongoDb:ConnectionString"];
    var mongoClient = new MongoClient(mongoConnection);
    var databaseName = builder.Configuration["MongoDb:DatabaseName"];
    return mongoClient.GetDatabase(databaseName);
});

builder.Services.AddSingleton<IPedidoRepository, PedidoRepository>();


builder.Services.AddHostedService<RabbitMQConsumerService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

//middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
