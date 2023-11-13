using FluentValidation;
using MediatR;
using Publisher.Core.Behaviors;
using Publisher.Service.FluentValidation;
using Publisher.Service.MediatR;
using Shared.Kernel.Absract;
using Shared.Kernel.EventStore.Persistence;

var builder = WebApplication.CreateBuilder(args);

IConfiguration configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.Development.json", optional: true)
    .Build();

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(IMediatR).Assembly));
builder.Services.AddValidatorsFromAssemblyContaining<IBaseValidator>();

builder.Services.AddScoped(typeof(IEventStoreReadRepository<>), typeof(EventStoreReadRepository<>))
                .AddScoped(typeof(IEventStoreWriteRepository<>), typeof(EventStoreWriteRepository<>))
                .AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));



var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();
app.Run();