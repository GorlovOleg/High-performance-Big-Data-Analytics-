using System.IO;
using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.SpaServices.AngularCli;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

// **************
// Authentication

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
//using Microsoft.Extensions.Logging.EventLog;
//--- services
using i4C.Services;
using i4C.DAL;
using i4C.BAL;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.OpenApi.Models;
using System.Collections.Generic;
//--- 3. Enabling CORS Globally 

namespace i4C
{
    public class Startup
    {
        //--- Logger 
        private readonly ILogger _logger;

        public static HttpClient WebApiClient = new HttpClient();
        
        public string connectionString;
        public string connectionString1;
        public string connectionString2;
        public string connectionString3;
        public string connectionString4;

        public string url_base1;
        public string url_base2;

        readonly string MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

        public Startup(IConfiguration configuration)
        {

            Configuration = configuration;

            var builder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json");

            Configuration = builder.Build();

            connectionString = Configuration["ConnectionStrings:dbConnection"];
            connectionString1 = Configuration["ConnectionStrings:DefaultConnection1"];
            connectionString2 = Configuration["ConnectionStrings:DefaultConnection2"];
            connectionString3 = Configuration["ConnectionStrings:DefaultConnection3"];
            connectionString4 = Configuration["ConnectionStrings:DefaultConnection4"];

            url_base1 = Configuration["url:URL_BASE1"];
            url_base2 = Configuration["url:URL_BASE2"];

            WebApiClient.BaseAddress = new Uri("https://localhost:44365/api");
            WebApiClient.DefaultRequestHeaders
                .Accept
                .Add
                (new MediaTypeWithQualityHeaderValue("application/json"));
            WebApiClient.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json; charset=utf-8");

        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            //--- logger
            #region LoggerFactory
            var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder
                    .AddFilter("Microsoft", LogLevel.Warning)
                    .AddFilter("System", LogLevel.Warning)
                    .AddFilter("LoggingConsoleApp.Program", LogLevel.Debug)
                    .AddConsole()
                    .AddEventLog();
            });

            ILogger logger = loggerFactory.CreateLogger<Program>();
            #endregion

            //--- Setting up CORS GET
            services.AddCors(options =>
            {
                options.AddPolicy(MyAllowSpecificOrigins,
                builder =>
                {
                    builder.WithOrigins(
                    	                "https://localhost:44375/api/test",
                                        "https://localhost:44365",
                                        "https://localhost:44365/api/DoctorInfo/GetDoctorInfoByUserId",
                                        "http://localhost:65062",
                                        "http://localhost:65062/api/DoctorInfo/GetDoctorInfoByUserId"
                                        )
                                .AllowAnyHeader()
                                .AllowAnyMethod();
                });
            });

            services.AddCors(c =>
            {
                c.AddPolicy("AllowOrigin", options => options.AllowAnyOrigin());
            });

            //--- 3. Enabling CORS Globally 
            services.AddCors(o => o.AddPolicy("AppPolicy", builder =>
            {
                builder.AllowAnyOrigin()
                       .AllowAnyMethod()
                       .AllowAnyHeader();
            }));

            //services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_3_0);

            //--- Core 3
            services.AddMvc()
                .AddNewtonsoftJson();

            services.AddControllers();

            //---3,4 
            services.AddSingleton(provider => Configuration);
            services.AddSingleton<IGreeter, Greeter>();

            services.AddScoped<IGreetingService, GreetingService>();

            //---4.8 
            //services.AddScoped<IReportRepositoryInMemory, ReportRepositoryInMemory>();
//---

            services.AddDbContext<AFCDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("dbConnection")));

            services.AddDbContext<C3_DASHBOARDDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection1")));

            //--- Authentication
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection2")));

            services.AddDbContext<CASADbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection2")));

            services.AddDbContext<SAMSDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection3")));

            services.AddDbContext<CEREBRUMDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection4")));

            //--- vary short term
            services.AddScoped(p => new SAMSDbContext(p.GetService<DbContextOptions<SAMSDbContext>>()));

            //--- long term service
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            //services.AddSwaggerDocumentation();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });
                //c.SwaggerDoc("v2", new OpenApiInfo { Title = "My API", Version = "v2" });
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "AvaalAuth"
                });
                c.AddSecurityRequirement(new OpenApiSecurityRequirement()
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            },
                            Scheme = "oauth2",
                            Name = "Bearer",
                            In = ParameterLocation.Header,

                        },
                        new List<string>()
                    }
                });
            });

            //--- every each authenction type is added in here
            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            services.AddMvc();

            // In production, the Angular files will be served from this directory
            services.AddSpaStaticFiles(configuration =>
            {
                configuration.RootPath = "ClientApp/dist";
            });
        }



        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseCors(builder => builder.AllowAnyOrigin());

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseSpaStaticFiles();

            //--- Core 3
            app.UseRouting();
            //app.UseCors("default");

            // Shows UseCors with CorsPolicyBuilder.
            app.UseCors(builder =>
                builder.WithOrigins( 
                                     "http://localhost:65062",
                                     "http://localhost:65062/api/DoctorInfo/GetDoctorInfoByUserId"
                                    ));


            //--- 3. Enabling CORS Globally 
            app.UseCors(options => options.AllowAnyOrigin());

            app.UseCors(MyAllowSpecificOrigins);

            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger();

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.),
            // specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v2/swagger.json", "My API V2 ");
            });




            app.UseAuthentication();
            app.UseAuthorization();
            //app.UseCors();

            app.UseEndpoints(endpoints =>
            {
                //---endpoints.MapHub<ChatHub>("/chat");
                endpoints.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
                endpoints.MapDefaultControllerRoute();
            });

            app.UseSpa(spa =>
            {

                spa.Options.SourcePath = "ClientApp";

                if (env.IsDevelopment())
                {
                    spa.Options.StartupTimeout = new TimeSpan(0, 0, 1500);
                    spa.UseAngularCliServer(npmScript: "start");
...