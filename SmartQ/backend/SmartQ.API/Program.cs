using Microsoft.EntityFrameworkCore;
using SmartQ.API.Hubs;
using SmartQ.API.Services;
using SmartQ.Application.Interfaces;
using SmartQ.Infrastructure;
using SmartQ.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSignalR();
builder.Services.AddScoped<IQueueNotificationService, QueueNotificationService>();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddCors(options =>
{
    options.AddPolicy("SmartQCors", policy =>
        policy.WithOrigins(
                "http://localhost:4200",
                "https://localhost:4200")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials());
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<SmartQDbContext>();
    db.Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("SmartQCors");

// Avoid HTTP→HTTPS redirect in dev (breaks CORS from Angular on :4200 → :5105)
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseAuthorization();
app.MapControllers();
app.MapHub<QueueHub>(QueueHub.Route);

app.Run();
