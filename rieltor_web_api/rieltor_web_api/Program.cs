using AgencyStore.Core.Abstractions;
using AgencyStore.Core.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
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

builder.Services.Configure<RouteOptions>(options => {
    options.LowercaseUrls = true;
    options.LowercaseQueryStrings = true;
});
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

builder.Services.AddScoped<IClientDocumentRepository, ClientDocumentRepository>();

builder.Services.AddScoped<IClientAccountService>(provider =>
{
    var clientsRepository = provider.GetRequiredService<IClientsRepository>();
    var userRepository = provider.GetRequiredService<IUserRepository>();
    var telegramService = provider.GetRequiredService<ITelegramService>();
    var logger = provider.GetRequiredService<ILogger<ClientAccountService>>();

    return new ClientAccountService(clientsRepository, userRepository, telegramService, logger);
});

builder.Services.AddScoped<IPropertiesService, PropertiesService>();
builder.Services.AddScoped<IClientsService, ClientsService>();
builder.Services.AddScoped<IRequestsService, RequestsService>();

builder.Services.AddScoped<IDealPipelineRepository, DealPipelineRepository>();
builder.Services.AddScoped<IDealStageRepository, DealStageRepository>();
builder.Services.AddScoped<IDealRepository, DealRepository>();
builder.Services.AddScoped<IDealHistoryRepository, DealHistoryRepository>();

builder.Services.AddScoped<IDealPipelineService, DealPipelineService>();
builder.Services.AddScoped<IDealStageService, DealStageService>();
builder.Services.AddScoped<IDealService, DealService>();
builder.Services.AddScoped<IDealHistoryService, DealHistoryService>();

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

        
        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                Console.WriteLine($" Authentication failed: {context.Exception.Message}");
                return Task.CompletedTask;
            },
            OnForbidden = context =>
            {
                Console.WriteLine($" Forbidden: {context.HttpContext.Request.Path}");
                return Task.CompletedTask;
            },
            OnChallenge = context =>
            {
                Console.WriteLine($" Challenge: {context.Error} - {context.ErrorDescription}");
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseCors(x =>
{
    x.AllowAnyHeader();
    x.AllowAnyMethod();
    x.WithOrigins("http://localhost:3000")
     .AllowCredentials();
});


app.Use(async (context, next) =>
{
    Console.WriteLine($"IN: {context.Request.Method} {context.Request.Path}");
    Console.WriteLine($"Query: {context.Request.QueryString}");
    await next();
    Console.WriteLine($"OUT: {context.Request.Method} {context.Request.Path} - {context.Response.StatusCode}");
});

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseExceptionHandler("/error");
app.UseStatusCodePagesWithReExecute("/error/{0}");



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
    ServeUnknownFileTypes = false
});


app.UseHttpsRedirection();


using (var scope = app.Services.CreateScope())
{
    try
    {
        var context = scope.ServiceProvider.GetRequiredService<PropertyStoreDBContext>();
        context.Database.Migrate();
        UserSeeder.Seed(context);

        // 
        var testPassword = "123456";
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(testPassword);
        Console.WriteLine($" Хеш пароля '{testPassword}': {passwordHash}");
        //  

    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error during seeding: {ex.Message}");
    }
}


app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});

app.Run();