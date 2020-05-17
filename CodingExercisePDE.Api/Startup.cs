using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using CodingExercisePDE.Entities;
using CodingExercisePDE.ServiceBusMessaging;
using CodingExercisePDE.Services;
using CodingExercisePDE.Services.HostedService;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Polly;
using Polly.Extensions.Http;

namespace CodingExercisePDE.Api
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            //test line

            services.AddControllers();

            // Register the Swagger generator, defining 1 or more Swagger documents
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "PDE API", Version = "v1" });
            });

            services.AddScoped(typeof(IRepository<RandomNumber>), sp =>
            {                
                var ctx = sp.GetService<PdeContext>();
                var rep = new Repository<RandomNumber>(ctx);
                return rep;
            });

            services.AddHttpClient();
            services.AddHttpClient("local", c =>
            {
                c.BaseAddress = new Uri("https://localhost:5001");
                c.DefaultRequestHeaders.Add("Accept", "application/json");
                c.DefaultRequestHeaders.Add("User-Agent", "HttpClientFactory-Sample");
            })
                .SetHandlerLifetime(TimeSpan.FromMinutes(5))
                .AddPolicyHandler(GetRetryPolicy())
                .AddPolicyHandler(GetCircuitBreakerPolicy())
                ;

            services.AddMemoryCache();
            services.AddSingleton<ICacheProvider, CacheProvider>(); //testing

            services.AddSingleton<IServiceBusSender, ServiceBusSender>();
            services.AddSingleton<IServiceBusConsumer, ServiceBusConsumerCreated>(sp =>
            {
                var ctx = new PdeContext(Configuration.GetConnectionString("SqlLietConnString"));
                IRepository<RandomNumber> repository = new Repository<RandomNumber>(ctx);
                IHttpClientFactory httpClientFactory = sp.GetService<IHttpClientFactory>();
                var logger = sp.GetRequiredService<ILogger<ServiceBusConsumerCreated>>();
                var obj = new ServiceBusConsumerCreated(Configuration, repository, httpClientFactory, logger);
                obj.RegisterOnMessageHandlerAndReceiveMessages();
                return obj;
            });

            services.AddHostedService<StandardNumbersHostedService>();

            services.AddDbContext<PdeContext>(options =>
            {
                //sql Lite                
                options.UseSqlite(Configuration.GetConnectionString("SqlLietConnString"));
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler(appBuilder =>
                {
                    appBuilder.Run(async context =>
                    {
                        context.Response.StatusCode = 500;
                        await context.Response.WriteAsync("An unexpected fault happened. Try again later.");
                    });
                });

            }

            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger();

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.),
            // specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "PDE API v1");
            });

            //app.UseHttpsRedirection();

            //resolve to init
            var bus = app.ApplicationServices.GetService<IServiceBusConsumer>();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

        static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.NotFound)
                .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
        }

        static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .CircuitBreakerAsync(3, TimeSpan.FromMinutes(1));
        }
    }
}
