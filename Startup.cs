using System;
using System.Linq;
using System.Net;
using System.Text;
using Backend.Helper.Authentication;
using BackEnd.DTO.Email;
using BackEnd.Helper.Authentication;
using BackEnd.Helper.Email;
using BackEnd.Helper.RefreshToken;
using BackEnd.Services;
using BackEnd.Services.Cache;
using BackEnd.Services.ExamQuestions;
using examedu.Services;
using examedu.Services.Account;
using examedu.Services.Classes;
using ExamEdu.DB;
using ExamEdu.DTO;
using ExamEdu.Helper.UploadDownloadFiles;
using ExamEdu.Hubs;
using ExamEdu.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using StackExchange.Redis.Extensions.Core.Configuration;
using StackExchange.Redis.Extensions.Newtonsoft;

namespace ExamEdu
{
    public class Startup
    {
        private readonly IWebHostEnvironment _env;
        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Configuration = configuration;
            _env = env;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // DB config
            string connectionString = Configuration.GetConnectionString("Postgre");
            services.AddDbContext<DataContext>(opt => opt.UseNpgsql(connectionString));
            services.AddScoped<DataContext, DataContext>();

            // Setting JSON convert to camelCase in Object properties
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };

            // Add Email service
            var emailConfig = Configuration.GetSection("Email").Get<EmailConfig>();
            services.AddScoped<IEmailHelper>(sp => new EmailHelper(emailConfig, _env));

            // Add validate JWT token middleware
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ClockSkew = TimeSpan.Zero, // Disable default 5 mins of Microsoft
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(Configuration["Jwt:Key"]))
                };
            });

            // Add IJwtGenerator to use in all project
            services.AddSingleton<IJwtGenerator>(new JwtGenerator(Configuration["Jwt:Key"]));

            // Add refresh token service
            services.AddSingleton<IRefreshToken, RefreshToken>();

            // Config username and password Mega using
            services.AddSingleton<IMegaHelper>(provider =>
            {
                string username = Configuration["Email:MailAddress"];
                string password = Configuration["Email:MegaPassword"];
                return new MegaHelper(username, password);
            });

            // Map data from Model to DTO and back
            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
            services.AddScoped<IAccountService, AccountService>();
            services.AddScoped<IStudentService, StudentService>();
            services.AddScoped<IExamService, ExamService>();
            services.AddScoped<IModuleService, ModuleService>();
            services.AddScoped<IQuestionService, QuestionService>();
            services.AddScoped<ILevelService, LevelService>();
            services.AddScoped<ITeacherService, TeacherService>();
            services.AddScoped<IMarkService, MarkService>();
            services.AddScoped<IStudentAnswerService, StudentAnswerService>();
            services.AddScoped<IExamQuestionsService, ExamQuestionsService>();
            services.AddScoped<IClassModuleService, ClassModuleService>();
            services.AddScoped<IClassService, ClassService>();
            services.AddScoped<ICacheProvider,CacheProvider>();
            services.AddScoped<IAcademicDepartmentService, AcademicDepartmentService>();

            services.AddSignalR();

            //Config redis for using signalR
            services.AddStackExchangeRedisExtensions<NewtonsoftSerializer>(Configuration.GetSection("Redis").Get<RedisConfiguration>());

            services.AddControllers().AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
            }).ConfigureApiBehaviorOptions(opt => // Custome Model State response object
            {
                opt.InvalidModelStateResponseFactory = actionContext =>
                    new BadRequestObjectResult(new ResponseDTO
                    {
                        Status = 400,
                        Message = "Input value is(are) invalid",
                        Errors = actionContext.ModelState.Where(x => x.Value.Errors.Count() != 0)
                                .ToDictionary(
                                    kvp => kvp.Key,
                                    kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                                )
                    });
            });
            ;
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "ExamEdu", Version = "v1" });

                var jwtSecurityScheme = new OpenApiSecurityScheme
                {
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    Name = "JWT Authentication",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                    Description = "Put **_ONLY_** your JWT Bearer token (Access Token) on textbox below!",

                    Reference = new OpenApiReference
                    {
                        Id = JwtBearerDefaults.AuthenticationScheme,
                        Type = ReferenceType.SecurityScheme
                    }
                };
                c.AddSecurityDefinition(jwtSecurityScheme.Reference.Id, jwtSecurityScheme);
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    { jwtSecurityScheme, Array.Empty<string>() }
                });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            //     if (env.IsDevelopment())
            //     {
            app.UseDeveloperExceptionPage();
            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "ExamEdu v1"));
            // }

            // ReFormat exception message
            app.UseExceptionHandler(e => e.Run(async context =>
            {
                var exception = context.Features.Get<IExceptionHandlerPathFeature>().Error;
                await context.Response.WriteAsJsonAsync(new ResponseDTO(500, exception.Message));
            }));
            app.UseHttpsRedirection();

            //Get front-end url from appsettings.json
            var frontEndDevUrl = Configuration["FrontEndDevUrl"];

            // Get front-end url from appsettings.json
            var frontEndUrl = Configuration["FrontEndUrl"];

            //CORS config for Front-end url
            app.UseCors(options => options.WithOrigins(frontEndDevUrl,frontEndUrl)
                                        .AllowAnyMethod()
                                        .AllowAnyHeader()
                                        .AllowCredentials());

            // ReFormat error message
            app.Use(async (context, next) =>
            {
                await next();

                if (context.Response.StatusCode == (int)HttpStatusCode.Unauthorized)
                {
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsync(new ResponseDTO(401).ToString());
                }

                if (context.Response.StatusCode == (int)HttpStatusCode.Forbidden)
                {
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsync(new ResponseDTO(403).ToString());
                }
            });
            app.UseRouting();

            app.UseAuthentication();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHub<NotifyHub>("/hubs/notification");
            });
        }
    }
}
