using Microsoft.EntityFrameworkCore;
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
app.UseHttpsRedirection();
app.UseAuthorization();

app.MapControllers();

app.Run();
