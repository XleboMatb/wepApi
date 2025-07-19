using Microsoft.EntityFrameworkCore;
using wepApi.Models;
using Npgsql.EntityFrameworkCore.PostgreSQL;
using Npgsql;
using Microsoft.AspNetCore.Mvc;



var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
// Add services to the container.
builder.Services.AddEntityFrameworkNpgsql().AddDbContext<FilesDbContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"), o=>o.MaxBatchSize(100000)));
builder.Services.Configure<IISServerOptions>(options =>
{
    options.MaxRequestBodySize = 100 * 1024 * 1024; // 100MB
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<FilesDbContext>();
        context.Database.Migrate(); // Автоматически применяет миграции
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while migrating the database.");
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
