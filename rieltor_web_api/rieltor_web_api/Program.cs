using AgencyStore.Core.Abstractions;
using AgencyStore.Core.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using PropertyStore.Application.Services;
using PropertyStore.DataAccess;
using PropertyStore.DataAccess.Repository;
using PropertyStore.DataAccess.Seed;
using System.Text;

Console.OutputEncoding = Encoding.UTF8;
Console.InputEncoding = Encoding.UTF8;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.Configure<JwtSettings>(
    builder.Configuration.GetSection("JwtSettings"));


builder.Services.AddDbContext<PropertyStoreDBContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString(nameof(PropertyStoreDBContext)));
});


builder.Services.Configure<TelegramBotSettings>(
    builder.Configuration.GetSection("TelegramBot"));


builder.Services.AddHttpClient<ITelegramService, TelegramService>();

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();

builder.Services.AddScoped<IPropertiesRepository, PropertiesRepository>();
builder.Services.AddScoped<IClientsRepository, ClientsRepository>();
builder.Services.AddScoped<IRequestsRepository, RequestsRepository>();

builder.Services.AddScoped<IPropertiesService, PropertiesService>();
builder.Services.AddScoped<IClientsService, ClientsService>();
builder.Services.AddScoped<IRequestsService, RequestsService>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
            ValidAudience = builder.Configuration["JwtSettings:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:Secret"]))
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    try
    {
        var context = scope.ServiceProvider.GetRequiredService<PropertyStoreDBContext>();

        
        context.Database.Migrate();

        
        UserSeeder.Seed(context);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error during seeding: {ex.Message}");
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "uploads");
if (!Directory.Exists(uploadsPath))
    Directory.CreateDirectory(uploadsPath);

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(builder.Environment.ContentRootPath, "uploads")),
    RequestPath = "/uploads",
    ServeUnknownFileTypes = true
});

app.UseExceptionHandler("/error");
app.UseStatusCodePagesWithReExecute("/error/{0}");

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.UseCors(x =>
{
    x.AllowAnyHeader();
    x.AllowAnyMethod();
    x.AllowAnyOrigin();
});

app.Run();