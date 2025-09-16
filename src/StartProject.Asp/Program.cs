// Copyright (c) 2025, Phoenix Contact GmbH & Co. KG
// Licensed under the Apache License, Version 2.0

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moryx.Launcher;
using Moryx.Model;
using Moryx.Runtime.Kernel;
using Moryx.Runtime.Modules;
using Moryx.Tools;
using NLog.Web;
using NLog;
using StartProject.Asp;
using System.Globalization;
using System.Text.Json.Serialization;

try
{
    // Initialize Logging
    LogManager.Setup().LoadConfigurationFromFile("Config/nlog.config");
    AppDomainBuilder.LoadAssemblies();

    var builder = WebApplication.CreateBuilder();
    builder.WebHost.UseStaticWebAssets();

    builder.Logging.ClearProviders();
    builder.Host.UseNLog();

    var services = builder.Services;

    // Setup MORYX
    services.AddMoryxKernel();
    services.AddMoryxModels();
    services.AddMoryxModules();

    #region Startup ConfigureServices
    services.AddSingleton<IShellNavigator, ShellNavigator>();
    services.AddSingleton<IAuthorizationPolicyProvider, ExamplePolicyProvider>();

    services.AddLocalization();
    services.Configure<RequestLocalizationOptions>(options =>
    {
        var supportedCultures = new[]
        {
                    new CultureInfo("de-DE"),
                    new CultureInfo("en-US"),
                    new CultureInfo("it-it"),
                    new CultureInfo("zh-Hans"),
                    new CultureInfo("pl-PL")
        };
        options.SupportedCultures = supportedCultures;
        options.SupportedUICultures = supportedCultures;
    });

    services.AddCors(options =>
    {
        options.AddPolicy("CorsPolicy", builder => builder
        .WithOrigins("http://localhost:4200") // Angular app url for testing purposes
        .AllowAnyMethod()
        .AllowAnyHeader()
        .AllowCredentials());
    });

    services.AddRazorPages();

    services.AddControllers()
        .AddJsonOptions(jo => jo.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

    services.AddSwaggerGen(c =>
    {
        c.CustomOperationIds(api => ((ControllerActionDescriptor)api.ActionDescriptor).MethodInfo.Name);
        c.CustomSchemaIds(type => type.ToString());
    });
    #endregion

    var app = builder.Build();
    var env = app.Environment;

    #region Startup Configure App
    if (env.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();

        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseRequestLocalization();

    app.UseStaticFiles();

    app.UseHttpsRedirection();

    app.UseRouting();

    if (env.IsDevelopment())
    {
        app.UseCors("CorsPolicy");
    }

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers().WithMetadata(new AllowAnonymousAttribute());
    app.MapRazorPages();

    #endregion

    app.Services.UseMoryxConfigurations("Config");

    var moduleManager = app.Services.GetRequiredService<IModuleManager>();
    moduleManager.StartModules();

    app.Run();

    moduleManager.StopModules();
}
finally
{
    // Ensure to flush and stop internal timers/threads before application-exit (Avoid segmentation fault on Linux)
    NLog.LogManager.Shutdown();
}
