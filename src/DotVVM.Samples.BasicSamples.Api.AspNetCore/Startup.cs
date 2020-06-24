﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotVVM.Core.Common;
using DotVVM.Framework.Api.Swashbuckle.AspNetCore;
using DotVVM.Framework.Api.Swashbuckle.AspNetCore.Filters;
using DotVVM.Samples.BasicSamples.Api.Common.DataStore;
using DotVVM.Samples.BasicSamples.Api.Common.Model;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Swashbuckle.AspNetCore.Swagger;

namespace DotVVM.Samples.BasicSamples.Api.AspNetCore
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            services.AddMvc().AddJsonOptions(options =>
            {
                options.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Utc;
            }); 

            services.AddDotVVM<DotvvmServiceConfigurator>();

            services.Configure<DotvvmApiOptions>(opt => opt.AddKnownType(typeof(Company<string>)));

            string CustomSchemaId(Type modelType)
            {
                if (!modelType.IsConstructedGenericType) return modelType.Name.Replace("[]", "Array");

                var generics = modelType.GetGenericArguments()
                    .Select(genericArg => CustomSchemaId(genericArg))
                    .Aggregate((previous, current) => previous + current);

                return $"{modelType.Name.Split('`').First()}[{generics}]";
            }

            services.AddSwaggerGen(options => {
                options.SwaggerDoc("v1", new OpenApiInfo() { Title = "DotVVM Test API", Version = "v1" });
                options.CustomSchemaIds(type => CustomSchemaId(type));

                options.EnableDotvvmIntegration();
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            app.UseCors(p => {
                p.AllowAnyOrigin();
                p.AllowAnyMethod();
                p.AllowAnyHeader();
            });

            app.UseDeveloperExceptionPage();
            app.UseSwagger(options => {
                options.PreSerializeFilters.Add((swaggerDoc, httpReq) => {
                    swaggerDoc.Servers = new List<OpenApiServer>()
                    {
                        new OpenApiServer { Url = "localhost:5001" }
                    };
                });
                options.SerializeAsV2 = true;
            });
            app.UseSwaggerUI(c => {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Northwind API");
            });

            app.UseMvc();

            app.UseDotVVM<DotvvmStartup>(env.ContentRootPath);

            SeedDatabase();
        }

        private static void SeedDatabase()
        {
            var database = new Database();
            database.SeedData();
            Database.Instance = database;
        }
    }
}
