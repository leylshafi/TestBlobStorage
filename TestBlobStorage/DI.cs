﻿using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using TestBlobStorage.Data;
using TestBlobStorage.Models;
using TestBlobStorage.Services.JwtService;
using TestBlobStorage.Services;
using TestBlobStorage.Utilities;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Text;

namespace TestBlobStorage
{
    public static class DI
    {
        public static IServiceCollection AddSwagger(this IServiceCollection services)
        {
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(setup =>
            {
                setup.SwaggerDoc("v1",
                    new OpenApiInfo
                    {
                        Title = "My Api - V1",
                        Version = "v1",
                    }
                );

                setup.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Jwt Authorization header using the Bearer scheme/ \r\r\r\n Enter 'Bearer' [space] and then token in the text input below. \r\n\r\n Example : \"Bearer f3c04qc08mh3n878\""
                });

                setup.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id ="Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });
            });
            return services;
        }

        public static IServiceCollection AddAuthenticationAndAuthorization(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<IJwtService, JwtService>();

            var jwtConfig = new JwtConfig();
            configuration.GetSection("JWT").Bind(jwtConfig);

            services.AddSingleton(jwtConfig);


            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, setup =>
            {
                setup.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidAudience = jwtConfig.Audience,
                    ValidIssuer = jwtConfig.Issuer,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtConfig.Secret)),
                };
            });

            services.AddAuthorization();

            return services;
        }


        public static IServiceCollection AddStorageManaganer(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddTransient<IStorageManager, BlobStorageManager>();
            services.Configure<BlobStorageOptions>(configuration.GetSection("BlobStorage"));
            return services;
        }

    }
}
