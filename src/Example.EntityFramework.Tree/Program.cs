using System.Text.Json;
using Example.EntityFramework.Tree.Data;
using Example.EntityFramework.Tree.Data.SqlServer;
using Example.EntityFramework.Tree.Options;
using FluentValidation.AspNetCore;
using kr.bbon.AspNetCore.Extensions.DependencyInjection;
using kr.bbon.AspNetCore.Filters;
using Microsoft.EntityFrameworkCore;
using Example.EntityFramework.Tree.Extensions.DependencyInjection;
using kr.bbon.Core.Models;
using Microsoft.AspNetCore.Mvc;
using Example.EntityFramework.Tree.Infrastructure;

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
    })
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            PathString path = context.HttpContext.Request.Path;
            string method = context.HttpContext.Request.Method;
            string displayName = context.ActionDescriptor.DisplayName ?? string.Empty;

            var errors = context.ModelState.Values
                .SelectMany(x => x.Errors)
                .Select(error => JsonSerializer.Deserialize<ErrorModel>(error.ErrorMessage));

            var responseStatusCode = StatusCodes.Status400BadRequest;
            var responseModel = kr.bbon.AspNetCore.Models.ApiResponseModelFactory.Create(responseStatusCode, "Payload is invalid", errors);

            responseModel.Path = path.ToString();
            responseModel.Method = method;
            responseModel.Instance = displayName;

            context.HttpContext.Response.StatusCode = responseStatusCode;

            return new ObjectResult(responseModel);
        };
    });

builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidators();

builder.Services.AddTransient<IValidatorInterceptor, CustomValidatorInterceptor>();

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
