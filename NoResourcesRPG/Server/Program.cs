using Mapster;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NoResourcesRPG.Client.Helpers;
using NoResourcesRPG.Server;
using NoResourcesRPG.Server.Database;
using NoResourcesRPG.Server.Database.Models;
using NoResourcesRPG.Server.Hubs;
using NoResourcesRPG.Server.Mappers;
using NoResourcesRPG.Server.Services;
using System;
using System.Reflection;
using System.Text.Encodings.Web;
using System.Text.Json.Serialization;
using System.Text.Unicode;

namespace NoResourcesRPG.Server;

public class Program
{
    public static void Main(string[] args)
    {

        var builder = WebApplication.CreateBuilder(args);

#if DEBUG
        string tempPath = Path.GetTempPath();
        string dbFileName = Path.Combine(tempPath, AppDomain.CurrentDomain.FriendlyName, "noResDb_test.db");
        Directory.CreateDirectory(Path.GetDirectoryName(dbFileName)!);

        //if (File.Exists(dbFileName)) File.Delete(dbFileName);

#endif

        //db
        builder.Services.AddDbContext<NoResDbContext>(options =>
        options.UseSqlite($"DataSource={dbFileName}", o => o.MigrationsHistoryTable(NoResDbContext.DefaultMigrationsTable, NoResDbContext.DefaultSchema)));

        //identity
        builder.Services.AddIdentity<UserEntity, UserRoleEntity>(options =>
        {
            builder.Configuration.GetSection("Identity").Bind(options);
        })
            .AddRoles<UserRoleEntity>() // Add roles BEFORE RoleManager & UserManager
            .AddEntityFrameworkStores<NoResDbContext>() // Must come before UserStore
            .AddSignInManager<SignInManager<UserEntity>>()
            .AddUserManager<UserManager<UserEntity>>() // Depends on EntityFrameworkStores
            .AddRoleManager<RoleManager<UserRoleEntity>>() // Depends on Roles
            .AddUserStore<UserStore<UserEntity, UserRoleEntity, NoResDbContext, string>>() // Needs EF Stores
            .AddDefaultTokenProviders() // Requires User Manager
        ;
        //authentication
        builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.RequireHttpsMetadata = false;
            options.SaveToken = true;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                RequireSignedTokens = true,
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = builder.Configuration["Jwt:Issuer"],
                ValidAudience = builder.Configuration["Jwt:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(Token.KeyBytes),
            };
            // Important: allow JWT in SignalR requests
            options.Events = new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    var accessToken = context.Request.Query["access_token"];

                    // If the request is for the hub endpoint, use the token from query
                    var path = context.HttpContext.Request.Path;
                    if (!string.IsNullOrEmpty(accessToken) &&
                        path.StartsWithSegments("/gamehub"))
                    {
                        context.Token = accessToken;
                    }

                    return Task.CompletedTask;
                }
            };
        });

        builder.Services.AddAuthorization();
        // Add SignalR and world state service
        builder.Services.AddSignalR();
        builder.Services.AddMapster();
        builder.Services.AddServerServices();
        builder.Services.AddControllersWithViews();
        builder.Services.AddRazorPages();
        builder.Services.AddControllers()
                .AddJsonOptions(opt =>
                {
                    opt.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
                    opt.JsonSerializerOptions.Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Cyrillic);
                    opt.JsonSerializerOptions.NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals;
                    opt.JsonSerializerOptions.UnknownTypeHandling = JsonUnknownTypeHandling.JsonElement;
                    opt.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault | JsonIgnoreCondition.WhenWritingNull;
                    opt.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
                    opt.JsonSerializerOptions.WriteIndented = false;
                });

        builder.Services.AddResponseCompression(options =>
        {
            options.EnableForHttps = true;
        });
        var app = builder.Build();

        using (IServiceScope scope = app.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<NoResDbContext>();
            dbContext.Database.Migrate();  // Apply migrations automatically

            //var projectService = scope.ServiceProvider.GetRequiredService<NoResDbContext>();
            //projectService.LoadFromDatabaseAsync(dbContext).GetAwaiter().GetResult(); // Load data from database
        }

        if (app.Environment.IsDevelopment())
        {
            app.UseWebAssemblyDebugging();
        }
        else
        {
            app.UseExceptionHandler("/Error");
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseBlazorFrameworkFiles();
        app.UseStaticFiles();

        app.UseRouting();

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapRazorPages();
        app.MapControllers();
        app.MapFallbackToFile("index.html");

        // Map your game hub
        app.MapHub<GameHub>("/gamehub");

        app.Run();
    }
}