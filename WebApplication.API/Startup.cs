using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using OpenTelemetry.Trace.Configuration;
using Serilog;
using System;

namespace WebApplication.API
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public static readonly ILoggerFactory MyLoggerFactory
                    = LoggerFactory.Create(builder =>
                    {
                        builder
                            .AddFilter((category, level) =>
                                category == DbLoggerCategory.Database.Command.Name
                                && level == LogLevel.Information)
                            .AddConsole();
                    });

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {


            services.AddDbContextPool<ApplicationDbContext>(options => options
                    .UseLoggerFactory(MyLoggerFactory)
                    .UseSqlServer("Server=.;Database=Logging;Trusted_Connection=True;"));


            services.AddOpenTelemetry(builder =>
            {
                builder.UseZipkin(o =>
                {
                    o.Endpoint = new Uri("http://localhost:9411/api/v2/spans");
                    o.ServiceName = typeof(Startup).Assembly.GetName().Name;
                });

                builder.AddRequestCollector()
                       .AddDependencyCollector();
            });

            //   var metrics = AppMetrics.CreateDefaultBuilder()
            //    .Report.ToInfluxDb(options =>
            //    {
            //        options.InfluxDb.BaseUri = new Uri("http://127.0.0.1:8086");
            //        options.InfluxDb.Database = "my-metrics";
            //        options.InfluxDb.CreateDataBaseIfNotExists = true;
            //    })
            //    .Build();

            //     services.AddMetrics(metrics);
            //     services.AddMetricsTrackingMiddleware();
            //     services.AddMetricsReportingHostedService();
            //     services.AddHoneycomb(Configuration);


            services.AddControllers(opts =>
            {
                opts.Filters.Add<SerilogLoggingActionFilter>();
            });//.AddMetrics();
            // Register the Swagger generator, defining 1 or more Swagger documents
            var serviceCollection = services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseApiExceptionHandler();

            app.UseStaticFiles();

            app.UseSerilogRequestLogging();

            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger();

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.),
            // specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
            });

            //app.UseMetricsAllMiddleware();
            //app.UseHoneycomb();

            app.UseRouting();


            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            app.UseWelcomePage("/swagger");
        }
    }
}
