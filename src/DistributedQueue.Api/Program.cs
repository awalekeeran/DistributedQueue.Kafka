using DistributedQueue.Core.Services;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Distributed Queue API",
        Version = "v1",
        Description = "An in-memory distributed queue system similar to Kafka"
    });
});

// Add CORS for future web UI
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Register application services as singletons (in-memory state)
builder.Services.AddSingleton<ITopicManager, TopicManager>();
builder.Services.AddSingleton<IProducerManager, ProducerManager>();
builder.Services.AddSingleton<IConsumerGroupManager, ConsumerGroupManager>();
builder.Services.AddSingleton<IConsumerManager, ConsumerManager>();
builder.Services.AddSingleton<IMessageBroker, MessageBroker>();

var app = builder.Build();

// Configure the HTTP request pipeline
// Always enable Swagger for easy API access
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Distributed Queue API v1");
    c.RoutePrefix = "swagger";
});

// Don't redirect to HTTPS in container (using HTTP on port 8080)
// app.UseHttpsRedirection();

app.UseCors();
app.UseAuthorization();
app.MapControllers();

// Add a simple root endpoint
app.MapGet("/", () => Results.Redirect("/swagger"));

app.Run();
