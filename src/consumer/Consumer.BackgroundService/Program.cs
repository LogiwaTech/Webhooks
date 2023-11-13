using Consumer.BackgroundService.Consumers;
using ProtoBuf.Meta;
using Shared.Kernel.Absract;
using Shared.Kernel.EventStore.Persistence;
using Shared.Kernel.EventStore.Subscriptions;

var builder = WebApplication.CreateBuilder(args);

IConfiguration configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.Development.json", optional: true)
    .Build();

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHostedService<BookCreatedConsumer>();
builder.Services.AddScoped(typeof(IEventStoreReadRepository<>), typeof(EventStoreReadRepository<>));
builder.Services.AddScoped(typeof(IEventStoreWriteRepository<>), typeof(EventStoreWriteRepository<>));
builder.Services.AddScoped(typeof(IEventStoreSubscriber<>), typeof(EventStoreSubscriber<>));


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();



app.Run();

