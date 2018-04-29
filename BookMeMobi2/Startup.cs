using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using BookMeMobi2.Entities;
using BookMeMobi2.Helpers.Fliters;
using BookMeMobi2.Options;
using BookMeMobi2.Services;
using FluentValidation.AspNetCore;
using Google.Cloud.Diagnostics.AspNetCore;
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

            services.AddCors();

            //services.AddDbContext<ApplicationDbContext>(o => o.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"),
            //    b => b.MigrationsAssembly("BookMeMobi2")));


            services.AddDbContext<ApplicationDbContext>(o => o.UseMySql(Configuration.GetConnectionString("ConnectionString"),
                b => b.MigrationsAssembly("BookMeMobi2")));

            services.AddIdentity<User, IdentityRole>(o =>
            {
                o.Password.RequireDigit = true;
                o.Password.RequiredLength = 6;
                o.Password.RequireUppercase = true;
                o.Password.RequireUppercase = true;
                o.SignIn.RequireConfirmedEmail = true;
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

            services.AddMvc(o => o.Filters.Add(typeof(ApiExceptionAttributeImpl))).AddFluentValidation(o => o.RegisterValidatorsFromAssemblyContaining<Startup>());

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info()
                {
                    Version = "v1",
                    Title = "BookMeMobi API"
                });
                c.IncludeXmlComments(GetXmlCommentsPath());
            });

            services.AddTransient<IBookService, BookService>();
            services.AddTransient<IUserService, UserService>();
            services.AddTransient<ITokenService, TokenService>();
            services.AddTransient<IMailService, MailService>();
            services.AddTransient<IStorageService, GoogleStorageService>();
            services.AddTransient<IPropertyMappingService, PropertyMappingService>();
            services.AddScoped<ValidateModelAttribute>();
            services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseDeveloperExceptionPage();
            loggerFactory.AddDebug();
            loggerFactory.AddGoogle(Configuration["Stackdriver:ProjectId"]);
            
            app.UseCors(o => o.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin().AllowCredentials());
            app.UseStaticFiles();
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
