using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
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
using Microsoft.Extensions.Logging;
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
using Polly;
using Prometheus;
using Polly.Contrib.Simmy;
using Polly.Contrib.Simmy.Outcomes;

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
            // Geração de uma mensagem simulado erro HTTP do tipo 500
            // (Internal Server Error)
            var resultInternalServerError = new HttpResponseMessage(
                HttpStatusCode.InternalServerError)
            {
                Content = new StringContent(
                    "Erro gerado em simulação de caos com Simmy...")
            };

            // Criação da Chaos Policy com uma probabilidade
            // de 60% de erro
            var chaosPolicy = MonkeyPolicy
                .InjectResultAsync<HttpResponseMessage>(with =>
                    with.Result(resultInternalServerError)
                        .InjectionRate(0.6)
                        .Enabled(true)
                );

            // Add framework services.
            services.AddCors();
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

            /*var retryPolicy = HttpPolicyExtensions
                .HandleTransientHttpError()
                .OrResult(msg => msg.StatusCode == HttpStatusCode.Forbidden)
                .Or<TimeoutRejectedException>() // thrown by Polly's TimeoutPolicy if the inner call times out
                .WaitAndRetryAsync(5, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));*/
            var retryPolicy = Policy
                .HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
                .RetryAsync(3, onRetry: (message, retryCount) =>
                {
                    //Console.Out.WriteLine($"Content: {message.Result.Content.ReadAsStringAsync().Result}");
                    //Console.Out.WriteLine($"ReasonPhrase: {message.Result.ReasonPhrase}");
                    //string msg = $"Retentativa: {retryCount}";
                    //Console.Out.WriteLineAsync(msg);
                });

            var timeoutPolicy = Policy.TimeoutAsync<HttpResponseMessage>(5); 

            var policyWrap = Policy.WrapAsync(retryPolicy, chaosPolicy);

            services.AddHttpClient("GitHub", client =>
            {
                client.BaseAddress = new Uri("https://api.github.com/");
                client.DefaultRequestHeaders.Add("Accept", "application/vnd.github.v3+json");
            }).AddPolicyHandler(policyWrap).AddPolicyHandler(timeoutPolicy);

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
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            app.UseProblemDetailsExceptionHandler(loggerFactory);
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
            // Custom Metrics to count requests for each endpoint and the method
var counter = Metrics.CreateCounter("api_path_counter", "Counts requests to the API endpoints", new CounterConfiguration
{
LabelNames = new[] { "method", "endpoint" }
});
app.Use((context, next) =>
{
counter.WithLabels(context.Request.Method, context.Request.Path).Inc();
return next();
});
            app.UseMetricServer();
            app.UseMvc();
        }
    }
}
