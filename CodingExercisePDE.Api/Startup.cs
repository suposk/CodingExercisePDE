using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using CodingExercisePDE.Entities;
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
            services.AddControllers();

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
                .AddPolicyHandler(GetCircuitBreakerPolicy());

            services.AddMemoryCache();
            services.AddSingleton<ICacheProvider, CacheProvider>(); //testing

            services.AddHostedService<StandardNumbersHostedService>(sp => 
            {
                var ctx = new PdeContext(Configuration.GetConnectionString("SqlLietConnString"));
                IRepository<RandomNumber> repo = new Repository<RandomNumber>(ctx);
                var logger = sp.GetRequiredService<ILogger<StandardNumbersHostedService>>();
                var httpClientFactory = sp.GetService<IHttpClientFactory>();
                return new StandardNumbersHostedService(httpClientFactory, repo, logger);
            });

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

            //app.UseHttpsRedirection();

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
                .WaitAndRetryAsync(10, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
        }

        static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .CircuitBreakerAsync(10, TimeSpan.FromMinutes(5));
        }
    }
}
