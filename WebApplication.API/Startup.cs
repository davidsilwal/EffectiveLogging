using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System;
using System.Threading.Tasks;

namespace WebApplication.API
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


            //services.AddDbContextPool<ApplicationDbContext>(options => options
            //        .UseLoggerFactory(MyLoggerFactory)
            //        .UseInMemoryDatabase("testdb"));

            services.AddHealthChecks()
                .AddCheck("self", () => HealthCheckResult.Healthy())
                .AddUrlGroup(new Uri("https://www.facebook.com/"), "Facebook", tags: new[] { "ready" })
                .AddSqlServer("Server=(localdb)\\mssqllocaldb;Database=MyLogging;Trusted_Connection=True;",
                                                        healthQuery: "SELECT 1;",
                                                        name: "sql",
                                                        failureStatus: HealthStatus.Degraded)
                .AddDbContextCheck<ApplicationDbContext>()
                .AddSeqPublisher(s => {
                    s.Endpoint = "http://localhost:5341";
                    s.ApiKey = "WQ1jNI1GgJNMXpJOPAsz";
                });
            
            services.AddHealthChecksUI();


            services.AddDbContextPool<ApplicationDbContext>((provider, options) => {
                var logger = provider.GetRequiredService<ILoggerFactory>();

                options
                    .UseLoggerFactory(logger)
                    .EnableDetailedErrors()
                    .EnableSensitiveDataLogging()
                    .UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=MyLogging;Trusted_Connection=True;",
                     sql => sql.MigrationsAssembly(typeof(Startup).Assembly.FullName));

            });

            //services.AddOpenTelemetry(builder => {
            //    builder.UseZipkin(o => {
            //        o.Endpoint = new Uri("http://localhost:9411/api/v2/spans");
            //        o.ServiceName = typeof(Startup).Assembly.GetName().Name;
            //    });

            //    builder.AddRequestCollector()
            //           .AddDependencyCollector();
            //});

            //var metrics = AppMetrics.CreateDefaultBuilder()
            // .Report.ToInfluxDb(options => {
            //     options.InfluxDb.BaseUri = new Uri("http://127.0.0.1:8086");
            //     options.InfluxDb.Database = "my-metrics";
            //     options.InfluxDb.CreateDataBaseIfNotExists = true;
            // })
            // .Build();

            //services.AddMetrics(metrics);
            //services.AddMetricsTrackingMiddleware();
            //services.AddMetricsReportingHostedService();
            //    services.AddHoneycomb(Configuration);

            services.AddControllers(opts => {
                opts.Filters.Add<SerilogLoggingActionFilter>();
            });

            // Register the Swagger generator, defining 1 or more Swagger documents
            var serviceCollection = services.AddSwaggerGen(c => {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {

            app.UseDeveloperExceptionPage();

            //    app.UseApiExceptionHandler();

            app.UseStaticFiles();

            //  app.UseSerilogRequestLogging();

            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger();

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.),
            // specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c => {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
            });

            //      app.UseMetricsAllMiddleware();
            //app.UseHoneycomb();

            app.UseRouting();

            app.UseHealthChecks("/health", new HealthCheckOptions() {
                Predicate = _ => true,
                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
            });

            app.UseHealthChecksUI(o => o.UIPath = "/healthchecks-ui");


            app.UseEndpoints(endpoints => endpoints.MapControllers());

            app.Run(ctx => { ctx.Response.Redirect("/swagger"); return Task.CompletedTask; });
        }
    }
}
