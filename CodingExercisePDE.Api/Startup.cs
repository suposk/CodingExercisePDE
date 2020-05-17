using System;
using System.Collections.Generic;
using System.Linq;
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

            services.AddHostedService<StandardNumbersHostedService>(sp => 
            {
                var ctx = new PdeContext(Configuration.GetConnectionString("SqlLietConnString"));
                IRepository<RandomNumber> repo = new Repository<RandomNumber>(ctx);
                var logger = sp.GetRequiredService<ILogger<StandardNumbersHostedService>>();
                return new StandardNumbersHostedService(repo, logger);
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
    }
}
