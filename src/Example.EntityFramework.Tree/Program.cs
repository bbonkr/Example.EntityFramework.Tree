using System.Text.Json;
using Example.EntityFramework.Tree.Data;
using Example.EntityFramework.Tree.Data.SqlServer;
using Example.EntityFramework.Tree.Options;
using kr.bbon.AspNetCore.Extensions.DependencyInjection;
using kr.bbon.AspNetCore.Filters;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.ConfigureAppOptions();
builder.Services
    .AddOptions<TreeOptions>()
    .Configure<IConfiguration>((options, configuration) =>
    {
        configuration.GetSection(TreeOptions.Name).Bind(options);
    });

builder.Services.AddDbContext<AppDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("Default");
    options.UseSqlServer(connectionString, sqlServerOptions =>
    {
        var migrationAssembly = typeof(Placeholder).Assembly.GetName(false).Name;
        sqlServerOptions.MigrationsAssembly(migrationAssembly);
    });
});

builder.Services.AddOptions<JsonSerializerOptions>()
    .Configure(options =>
    {
        options.AllowTrailingCommas = true;
        options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    });

builder.Services.AddLogging();
builder.Services.AddControllers(options =>
{
    options.Filters.Add<ApiExceptionHandlerFilter>();
})
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.AllowTrailingCommas = true;
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    });

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddApiVersioningAndSwaggerGen();

builder.Services.AddAutoMapper(typeof(Program).GetType().Assembly);

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await dbContext.Database.MigrateAsync();
}

app.UseSwaggerUIWithApiVersioning();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
