using AutoMapper;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using NLog.Web;
using Tasin.Website.Authorizations;
using Tasin.Website.BackgroundServices;
using Tasin.Website.Common.AutoMapper;
using Tasin.Website.Common.CommonModels;
using Tasin.Website.Common.Enums;
using Tasin.Website.Common.Services;
using Tasin.Website.Common.Util;
using Tasin.Website.DAL.Interfaces;
using Tasin.Website.DAL.Repository;
using Tasin.Website.DAL.Services;
using Tasin.Website.DAL.Services.WebInterfaces;
using Tasin.Website.DAL.Services.WebServices;
using Tasin.Website.Domains.DBContexts;
using System.Reflection;
using System.Text;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

var logger = NLogBuilder.ConfigureNLog("nlog.config").GetCurrentClassLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);
    var tokenSecretKey = Utils.DecodePassword(builder.Configuration.GetSection("JWT:SecretKey").Value, EEncodeType.SHA_256);

    ConfigurationManager configuration = builder.Configuration;

    // Add services to the container.
    builder.Services.AddAutoMapper(typeof(Program));

    // Configure JSON options for enum handling - allow both string and numeric values
    builder.Services.Configure<Microsoft.AspNetCore.Mvc.JsonOptions>(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
        options.JsonSerializerOptions.NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowReadingFromString;
    });

    var mappingConfig = new MapperConfiguration(mc =>
    {
        mc.AddProfile(new DomainToDTOMappingProfile());
    });
    IMapper mapper = mappingConfig.CreateMapper();

    builder.Services.AddSingleton(mapper);

    //builder.Services.AddSignalR();


    builder.Services.AddCors();

    builder.Services.AddControllers();
    builder.Services.AddControllersWithViews();

    // Service Background
    //var on_off_BackgroundService = builder.Configuration.GetSection("BackgroundService").Value;
    //if (on_off_BackgroundService == "true")
    //{
    //    builder.Services.AddHostedService<ExampleBackground>();
    //}


    builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

    var connectionString = configuration.GetConnectionString("TasinDB");
    var CookieName_ = configuration.GetSection("Path:AuthCookie_").Value;
    builder.Services.AddDbContext<SampleDBContext>(options => options.UseNpgsql(connectionString));
    builder.Services.AddDbContext<SampleReadOnlyDBContext>(options => options.UseNpgsql(connectionString));

    var appConfig = builder.Configuration.GetSection("Path").Get<ApplicationConfiguration>();
    builder.Services.AddSingleton<IApplicationConfiguration, ApplicationConfiguration>(e => appConfig);
    #region SETTING AUTHEN & AUTHOR
    builder.Services.AddSingleton<IAuthorizationPolicyProvider, C3FunctionAuthorizationPolicyProvider>();
    builder.Services.AddSingleton<IAuthorizationHandler, C3FunctionAuthorizationHandler>();
    builder.Services.AddAuthentication(cfg =>
    {
        cfg.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        cfg.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        cfg.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;

    })
            .AddCookie(
                options =>
                {
                    options.Cookie.Name = CookieName_;
                    options.LoginPath = "/Account/Login";
                    options.LogoutPath = "/Account/Logout";
                    // cookie settings
                    options.Cookie.HttpOnly = true;
                    //options.Cookie.MaxAge = TimeSpan.FromSeconds(10);
                    //options.Cookie.Expiration = TimeSpan.FromSeconds(10);
                    options.Cookie.SameSite = SameSiteMode.Lax;
                    options.ExpireTimeSpan = TimeSpan.FromDays(1);
                })
            .AddJwtBearer(x =>
            {
                x.RequireHttpsMetadata = false;
                x.SaveToken = true;
                x.Events = new JwtBearerEvents()
                {
                    OnChallenge = context =>
                    {
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        context.Response.ContentType = "application/json; charset=utf-8";
                        var result = JsonConvert.SerializeObject(
                            new
                            {
                                statusCode = 401,
                                message = "Token mismatch",
                                receivedRequest = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"),
                                sendResponse = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")
                            }
                        );
                        return context.Response.WriteAsync(result);
                    }
                };
                x.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(tokenSecretKey)),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    // set clockskew to zero so tokens expire exactly at token expiration time (instead of 5 minutes later)
                    ClockSkew = TimeSpan.FromSeconds(5)
                };
            });
    #endregion
    builder.Services.AddSession(options =>
    {
        options.IdleTimeout = TimeSpan.FromMinutes(100);
    });
    builder.Services.AddRazorPages();
    //builder.Services.AddSignalR(hubOptions =>
    //{
    //    hubOptions.EnableDetailedErrors = true;
    //    hubOptions.ClientTimeoutInterval = TimeSpan.FromHours(24);
    //    hubOptions.KeepAliveInterval = TimeSpan.FromHours(8);
    //});

    // Configure Swagger
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
        {
            Title = "Tasin API",
            Version = "v1",
            Description = "API for Tasin Website",
            Contact = new Microsoft.OpenApi.Models.OpenApiContact
            {
                Name = "Support Team",
                Email = "support@example.com"
            }
        });

        // Add JWT Authentication to Swagger
        c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
        {
            Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
            Name = "Authorization",
            In = Microsoft.OpenApi.Models.ParameterLocation.Header,
            Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
            Scheme = "Bearer"
        });

        c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
        {
            {
                new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                {
                    Reference = new Microsoft.OpenApi.Models.OpenApiReference
                    {
                        Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                Array.Empty<string>()
            }
        });

        // Include XML comments if available
        var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
        if (File.Exists(xmlPath))
        {
            c.IncludeXmlComments(xmlPath);
        }
    });
    #region NLog: Setup NLog for Dependency injection
    logger.Debug("Running...");
    builder.Logging.ClearProviders();
    builder.Logging.AddDebug();
    builder.Logging.AddConsole();
    builder.Logging.SetMinimumLevel(LogLevel.Trace);
    builder.Host.UseNLog();
    #endregion

    #region ConfigurationRepositoryAndUnitOfWorkSettings
    // Register DbContext
    //builder.Services.AddDbContext<SampleDBContext>(options =>
    //    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
    //builder.Services.AddDbContext<SampleReadOnlyDBContext>(options =>
    //    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

    // REGISTER SERVICES HERE
    //builder.Services.AddSingleton<IChatHub, ChatHub>();
    builder.Services.AddScoped<ICurrentUserContext, CurrentUserContext>();
    builder.Services.AddScoped<IUserService, UserService>();
    builder.Services.AddScoped<IRoleService, RoleService>();
    builder.Services.AddScoped<ICommonService, CommonService>();
    builder.Services.AddScoped<ICustomerService, CustomerService>();
    builder.Services.AddScoped<IVendorService, VendorService>();
    builder.Services.AddScoped<IUnitService, UnitService>();
    builder.Services.AddScoped<ICategoryService, CategoryService>();
    builder.Services.AddScoped<IProcessingTypeService, ProcessingTypeService>();
    builder.Services.AddScoped<IMaterialService, MaterialService>();
    builder.Services.AddScoped<ISpecialProductTaxRateService, SpecialProductTaxRateService>();
    builder.Services.AddScoped<IProductService, ProductService>();
    builder.Services.AddScoped<IPurchaseOrderService, PurchaseOrderService>();


    builder.Services.AddScoped<IUserRepository, UserRepository>();
    builder.Services.AddScoped<IRoleRepository, RoleRepository>();
    builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
    builder.Services.AddScoped<IVendorRepository, VendorRepository>();
    builder.Services.AddScoped<IUnitRepository, UnitRepository>();
    builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
    builder.Services.AddScoped<IProcessingTypeRepository, ProcessingTypeRepository>();
    builder.Services.AddScoped<IMaterialRepository, MaterialRepository>();
    builder.Services.AddScoped<ISpecialProductTaxRateRepository, SpecialProductTaxRateRepository>();
    builder.Services.AddScoped<IProductRepository, ProductRepository>();
    builder.Services.AddScoped<IPurchaseOrderRepository, PurchaseOrderRepository>();
    builder.Services.AddScoped<IPurchaseOrderItemRepository, PurchaseOrderItemRepository>();


    #endregion

    builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
    builder.Services.AddTransient<EmailService>();

    // builder.Services.AddHttpClient<TelegramService>();
    // builder.Services.AddHostedService<BotHostedBackgroundService>();
    // builder.Services.AddHostedService<TelegramNotiUrnInfoBackgroundService>();



    // REGISTER MIDDLEWARE HERE
    var app = builder.Build();
    var environment = app.Environment;
    var config = builder.Configuration;
    config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
          .AddJsonFile($"appsettings.{environment.EnvironmentName}.json", optional: true);

    config.AddEnvironmentVariables().Build();
    if (environment.IsDevelopment())
    {
        var appAssembly = Assembly.Load(new AssemblyName(environment.ApplicationName));
        if (appAssembly != null)
            config.AddUserSecrets(appAssembly, optional: true);
    }

    // Configure Swagger UI for all environments
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Tasin API v1");
        c.RoutePrefix = "swagger";
        c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None);
        c.DefaultModelsExpandDepth(-1); // Hide schemas section
        c.DisplayRequestDuration();
    });
    // Configure the HTTP request pipeline.
    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Home/Error");
        // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
        app.UseHsts();
    }

    app.UseHttpsRedirection();
    app.UseStaticFiles();

    // Allow Swagger without authorization
    app.MapSwagger();

    app.UseCors(x => x
         .SetIsOriginAllowed(origin => true)
         .AllowAnyMethod()
         .AllowAnyHeader()
         .AllowCredentials());

    app.UseRouting();

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");
    //app.MapHub<ChatHub>("/chatHub");
    app.MapRazorPages();
    // Start the Telegram service
    //var telegramService = app.Services.GetRequiredService<TelegramService>();
    //telegramService.Start();
    app.Run();

}
catch (Exception ex)
{
    logger.Error(ex, "Error in init");
    throw;
}
finally
{
    NLog.LogManager.Shutdown();
}
