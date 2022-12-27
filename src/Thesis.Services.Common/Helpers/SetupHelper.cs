using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Thesis.Services.Common.Options;

namespace Thesis.Services.Common.Helpers;

/// <summary>
/// Помощник для конфигурации сервиса
/// </summary>
public static class SetupHelper
{
    /// <summary>
    /// Конфигурация сервиса
    /// </summary>
    /// <param name="builder">Билдер</param>
    /// <returns>Сконфигурированное приложение</returns>
    public static WebApplication BuildWebApplication(this WebApplicationBuilder builder)
    {
        var jwtOptions = builder.Configuration.GetSection(nameof(JwtOptions)).Get<JwtOptions>();
        
        builder.Services.AddOptions<JwtOptions>()
            .Bind(builder.Configuration.GetSection(nameof(JwtOptions)));
        
        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = jwtOptions?.GetSymmetricSecurityKey(),
                    ValidateIssuer = true,
                    ValidIssuer = jwtOptions?.Issuer,
                    ValidateAudience = true,
                    ValidAudience = $"*.{jwtOptions?.Issuer}",
                    ValidateLifetime = true
                };
            });

        builder.Services.AddControllers();
        builder.Services.AddSingleton<JwtReader>();

        var appBasePath = AppContext.BaseDirectory;
        var appName = System.Reflection.Assembly.GetEntryAssembly()?.GetName().Name;
        var appVersion = System.Reflection.Assembly.GetEntryAssembly()?.GetName().Version?.ToString();
        
        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = appName,
                Version = appVersion
            });
            c.UseAllOfToExtendReferenceSchemas();
            var xmlFiles = Directory.GetFiles(AppContext.BaseDirectory, "*.xml", SearchOption.TopDirectoryOnly).ToList();
            xmlFiles.ForEach(xmlFile => c.IncludeXmlComments(xmlFile));
        });
        
        var app = builder.Build();
        var logger = app.Logger;

        logger.LogInformation("Starting {AppName}...", appName);

        if (app.Environment.IsDevelopment())
        {
            logger.LogInformation($"Use development exception page");
            app.UseDeveloperExceptionPage();
        }

        logger.LogInformation($"Use Swagger.");
        app.UseSwagger();

        if (app.Environment.IsDevelopment())
        {
            logger.LogInformation($"Use Swagger UI.");
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", appName);
                c.RoutePrefix = "swagger";
            });
        }

        app.UseRouting();

        logger.LogInformation($"Use JWT authorization.");
        app.UseAuthentication();
        app.UseAuthorization();

        logger.LogInformation($"Use controllers.");
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });

        return app;
    }
}