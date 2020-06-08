using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using AutoMapper;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using NLog;
using Swashbuckle.AspNetCore.Swagger;
using Web.Api.Core;
using Web.Api.Extensions;
using Web.Api.Infrastructure;
using Web.Api.Infrastructure.Helpers;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json;
using Polly.Extensions.Http;
using Polly.Timeout;

namespace Web.Api
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            // Don't try and load nlog config during integ tests.
            var nLogConfigPath = string.Concat(Directory.GetCurrentDirectory(), "/nlog.config");
            if (File.Exists(nLogConfigPath)) { LogManager.LoadConfiguration(string.Concat(Directory.GetCurrentDirectory(), "/nlog.config"));}
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            services.AddCors();
            /*services.AddMetrics(options =>
        {
            options.WithGlobalTags((globalTags, info) =>
            {
                globalTags.Add("app", info.EntryAssemblyName);
                globalTags.Add("env", "stage");
            });
        });*/
            //services.AddMetricsMiddleware(options => options.IgnoredHttpStatusCodes = new[] { 404 });
            //services.AddMvc(options => options.AddMetricsResourceFilter());
            services.AddHttpContextAccessor();
            services.AddAutoMapper();

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1)
                             .AddFluentValidation(fv => fv.RegisterValidatorsFromAssemblyContaining<Startup>())
                             .AddJsonOptions(options => {
                                options.SerializerSettings.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());
                                options.SerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;
                              });

            // Register the Swagger generator, defining 1 or more Swagger documents
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info { Title = "AspNetCoreApiStarter", Version = "v1" });
            });

var retryPolicy = HttpPolicyExtensions
    .HandleTransientHttpError()
    .Or<TimeoutRejectedException>() // thrown by Polly's TimeoutPolicy if the inner call times out
    .WaitAndRetryAsync(6, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2,
                                                                    retryAttempt)));

            services.AddHttpClient("GitHub", client =>
            {
                client.BaseAddress = new Uri("https://api.github.com/");
                client.DefaultRequestHeaders.Add("Accept", "application/vnd.github.v3+json");
            })
            .AddPolicyHandler(retryPolicy);

            // Now register our services with Autofac container.
            var builder = new ContainerBuilder();

            builder.RegisterModule(new CoreModule());
            builder.RegisterModule(new InfrastructureModule());

            // Presenters
            builder.RegisterAssemblyTypes(Assembly.GetExecutingAssembly()).Where(t => t.Name.EndsWith("Presenter")).SingleInstance();

            builder.Populate(services);
            var container = builder.Build();
            // Create the IServiceProvider based on the container.
            return new AutofacServiceProvider(container);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            /*app.UseExceptionHandler(
                builder =>
                {
                    builder.Run(
                        async context =>
                        {
                            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                            context.Response.Headers.Add("Access-Control-Allow-Origin", "*");

                            var error = context.Features.Get<IExceptionHandlerFeature>();
                            if (error != null)
                            {
                                context.Response.AddApplicationError(error.Error.Message);
                                await context.Response.WriteAsync(error.Error.Message).ConfigureAwait(false);
                            }
                        });
                });*/

            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger();
            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.), 
            // specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "AspNetCoreApiStarter V1");
                c.RoutePrefix = "swagger";
            });

            // Enable middleware to serve generated Swagger as a JSON endpoint.
            /*app.UseCors(builder => builder.WithOrigins("http://localhost:4200")
                                     .AllowAnyMethod()
                                     .AllowAnyHeader()
                                     .AllowCredentials());*/

            //app.UseAuthentication();
            //app.UseMetrics();
            app.UseMvc();
        }
    }
}
