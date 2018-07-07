using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using BookMeMobi2.Entities;
using BookMeMobi2.Hangfire;
using BookMeMobi2.Helpers.Fliters;
using BookMeMobi2.Options;
using BookMeMobi2.Services;
using FluentValidation.AspNetCore;
using Google.Cloud.Diagnostics.AspNetCore;
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.PlatformAbstractions;
using Microsoft.IdentityModel.Tokens;
using Npgsql;
using Swashbuckle.AspNetCore.Swagger;

namespace BookMeMobi2
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
            services.AddOptions();
            services.Configure<JWTSettings>(Configuration.GetSection("JWTSettings"));
            services.Configure<GoogleCloudStorageSettings>(Configuration.GetSection("GoogleCloudStorage"));
            services.Configure<StackdriveSettings>(Configuration.GetSection("Stackdrive"));
            services.Configure<SMTPSettings>(Configuration.GetSection("SMTP"));
            services.Configure<SendGridSettings>(Configuration.GetSection("SendGrid"));
            services.Configure<AWSS3Settings>(Configuration.GetSection("AWSS3"));
            services.AddCors();

            //services.AddDbContext<ApplicationDbContext>(o => o.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"),
            //    b => b.MigrationsAssembly("BookMeMobi2")));


            services.AddDbContext<ApplicationDbContext>(o => o.UseMySQL(Configuration.GetConnectionString("ConnectionString"),
                b => b.MigrationsAssembly("BookMeMobi2")));

            services.AddIdentity<User, IdentityRole>(o =>
            {
                o.Password.RequireDigit = true;
                o.Password.RequiredLength = 6;
                o.Password.RequireUppercase = true;
                o.SignIn.RequireConfirmedEmail = true;

                o.Tokens.EmailConfirmationTokenProvider = TokenOptions.DefaultEmailProvider;
            }).AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultTokenProviders();

            var secret = Encoding.ASCII.GetBytes(Configuration["JWTSettings:Secret"]);

            services.AddAuthentication(o =>
            {
                o.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                o.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(o =>
            {
                o.RequireHttpsMetadata = false;
                o.SaveToken = true;
                o.TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(secret),
                    ValidateIssuer = false,
                    ValidateAudience = false
                };
            });

            services.AddAutoMapper();



            // services.AddHangfire(config => config.UsePostgreSqlStorage(Configuration.GetConnectionString("HangFireDb")));

            services.AddMvc(o => o.Filters.Add(typeof(ApiExceptionAttributeImpl)))
              .AddFluentValidation(o => o.RegisterValidatorsFromAssemblyContaining<Startup>())
              .AddJsonOptions(opt => opt.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore);

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info()
                {
                    Version = "v1",
                    Title = "BookMeMobi API"
                });

                c.IncludeXmlComments(GetXmlCommentsPath());

                c.AddSecurityDefinition("Bearer", new ApiKeyScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme",
                    Name = "Authorization",
                    In = "header",
                    Type = "apiKey"
                });

                c.AddSecurityRequirement(new Dictionary<string, IEnumerable<string>>
                {
                    {"Bearer", Enumerable.Empty<string>() }
                });
            });

            services.AddTransient<IBookService, BookService>();
            services.AddTransient<IUserService, UserService>();
            services.AddTransient<ITokenService, TokenService>();
            services.AddTransient<IMailService, SendGridMailService>();
            services.AddTransient<ITagService, TagServcie>();
            // services.AddTransient<IStorageService, GoogleStorageService>();
            services.AddTransient<IStorageService, AWSS3StorageService>();
            services.AddTransient<IPropertyMappingService, PropertyMappingService>();
            services.AddScoped<ValidateModelAttribute>();
            services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
            services.AddScoped<ReccuringDbJobs>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory, IServiceProvider serviceProvider)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseDeveloperExceptionPage();
            loggerFactory.AddDebug();


            app.UseCors(o => o.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin().AllowCredentials());
            app.UseStaticFiles();

            GlobalConfiguration.Configuration.UseActivator(new HangfireActivator(serviceProvider));

            // app.UseHangfireDashboard("/hangfire", new DashboardOptions()
            // {
                // Authorization = new[] { new CustomAuthorizeFilter() }
            // });
            // app.UseHangfireDashboard();
            // app.UseHangfireServer();
            //RecurringJob.AddOrUpdate<ReccuringDbJobs>(x => x.DeleteSoftDeletedBooksOlderThan30DaysAsync(), Cron.Daily(3));

            app.UseAuthentication();

            app.UseMvc();

            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "BookMeMobi API"));
        }

        private string GetXmlCommentsPath()
        {
            var app = AppContext.BaseDirectory;
            return System.IO.Path.Combine(app, "BookMeMobi2.xml");
        }
    }
}
