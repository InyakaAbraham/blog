using System.Text;
using Blog.Features;
using Blog.Models;
using Blog.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Filters;

var builder = WebApplication.CreateBuilder(args);
var config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", false, true)
    .AddJsonFile("appsettings.local.json", false, true)
    .AddEnvironmentVariables()
    .Build();
var appSettings = config.GetSection("AppSettings").Get<AppSettings>();

builder.Services.AddSingleton(appSettings);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddDbContextFactory<DataContext>(options => { options.UseNpgsql(appSettings.PostgresDsn); });
builder.Services.AddScoped<IBlogService, BlogService>();

builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
    {
        Description = "Kindly enter token issued by ditadev (\"bearer {token}\")",
        In = ParameterLocation.Header,
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey
    });
    options.OperationFilter<SecurityRequirementsOperationFilter>();
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8
                .GetBytes(appSettings.JwtSecret)),
            ValidateIssuer = false,
            ValidateAudience = false
        };
    });

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dataContext = scope.ServiceProvider.GetRequiredService<DataContext>();
    dataContext.Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();