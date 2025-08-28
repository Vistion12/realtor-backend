using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using PropertyStore.Application.Services;
using PropertyStore.DataAccess;
using PropertyStore.DataAccess.Repository;
using System.Text;

Console.OutputEncoding = Encoding.UTF8;
Console.InputEncoding = Encoding.UTF8;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<PropertyStoreDBContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString(nameof(PropertyStoreDBContext)));
});

builder.Services.AddScoped<IPropertiesService, PropertiesService>();
builder.Services.AddScoped<IPropertiesRepository, PropertiesRepository>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Добавляем поддержку статических файлов
var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "uploads");
if (!Directory.Exists(uploadsPath))
    Directory.CreateDirectory(uploadsPath);

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(builder.Environment.ContentRootPath, "uploads")),
    RequestPath = "/uploads",
    ServeUnknownFileTypes = true // Для поддержки различных форматов изображений
});

app.UseExceptionHandler("/error");
app.UseStatusCodePagesWithReExecute("/error/{0}");

app.UseHttpsRedirection();
app.UseAuthorization();

app.MapControllers();

app.UseCors(x =>
{
    x.AllowAnyHeader();
    x.AllowAnyMethod(); 
    x.AllowAnyOrigin(); 
});

app.Run();