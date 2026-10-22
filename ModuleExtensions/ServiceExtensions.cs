

using System.Text;
using _24dex_backend_comercial;
using Hangfire;
using _24dex_backend_comercial.Features.PrinterHub;
using Dls.Erp.Transversal.Logging;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;

using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OpenTelemetry.Metrics;
using OpenTelemetry.Logs;

public static class ServiceExtensions
{
    public static void ConfigureServices(this IServiceCollection services, IConfiguration configuration)
    {

        services.AddOutputCache(
            options =>
            {
                // Configuración global del caché (opcional)
                options.DefaultExpirationTimeSpan = TimeSpan.FromMinutes(60); // Expiración por defecto
                                                                              // options.SizeLimit = 100; // Límite de tamaño del caché (en MB)


                // options.AddPolicy("VaryByAuthorization", builder =>
                // {
                //     builder.Expire(TimeSpan.FromMinutes(10)) // Expiración de 10 minutos
                //             .SetVaryByHeader("Authorization"); // Aquí incluyes el heade
                // });


                // options.AddPolicy("JWT_Aware_Cache", builder =>
                // {
                //     builder.Expire(TimeSpan.FromMinutes(10))
                //             .SetVaryByQuery("*")
                //             .AddPolicy<JwtCachePolicy>(); // 👈 Política personalizada
                // });

            });

        services.Configure<JwtSettings>(configuration.GetSection("Jwt"));
        services.Configure<InvoiceSettings>(configuration.GetSection("Invoice"));
        services.Configure<SneakerSettings>(configuration.GetSection("Sneaker"));
        services.Configure<CustomerPortalSettings>(configuration.GetSection("CustomerPortal"));
        services.Configure<InvoicePrintSettings>(configuration.GetSection("InvoicePrint"));
        services.Configure<FileStorageSettings>(configuration.GetSection("FileStorage"));
        services.Configure<EncryptionSettings>(configuration.GetSection("Encryption"));
        services.Configure<PickupMobileSettings>(configuration.GetSection("PickupMobile"));

        var fileStorageProvider = configuration["FileStorage:Provider"];
        if (string.Equals(fileStorageProvider, "AzureBlob", StringComparison.OrdinalIgnoreCase))
        {
            services.AddScoped<IFileStorageService, AzureBlobStorageService>();
        }
        else
        {
            services.AddScoped<IFileStorageService, LocalStorageService>();
        }

        // Add services to the container.
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "My API", Version = "v1" });
            // Personalizar schemaId para evitar conflictos
            c.CustomSchemaIds(type => type.FullName!.Replace("+", "."));

            // Boton "Authorize" para pegar el JWT (sin esto, Swagger UI no sabe que existe auth Bearer)
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "JWT Authorization header usando el esquema Bearer. Ejemplo: \"Bearer {token}\"",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer"
            });
            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });


            // Definir tags manualmente: api.GroupName siempre es null porque ningún endpoint
            // llama a WithGroupName(...) (MapGroup no lo setea solo). Agrupamos por el segundo
            // segmento de la ruta (api/{modulo}/...) en su lugar.
            c.TagActionsBy(api =>
            {
                var segments = api.RelativePath?.Split('/', StringSplitOptions.RemoveEmptyEntries);
                var tag = segments is { Length: > 1 } ? segments[1] : "General";
                // refresh-token vive en /api/refresh-token, sin el prefijo /api/login/... que comparten
                // el resto de los endpoints de autenticación, así que lo unimos a mano.
                if (tag == "refresh-token") tag = "login";
                return new[] { tag };
            });
            c.DocInclusionPredicate((name, api) => true); // Incluir todos los endpoints

            // Configurar Swagger para reconocer los grupos de endpoints
            // c.DocInclusionPredicate((docName, apiDesc) =>
            // {
            //     if (docName == "v1")
            //         return true;

            //     var groupName = apiDesc.GroupName;
            //     return groupName != null && groupName.Equals(docName, StringComparison.OrdinalIgnoreCase);
            // });

            // c.TagActionsBy(apiDesc =>
            // {
            //     return new[] { apiDesc.GroupName ?? "default" };
            // });
        });

        services.AddDbContext<RecepcionDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        // Reutiliza la misma Azure SQL Database de la app (sin BD ni worker separado).
        services.AddHangfire(config => config
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UseSqlServerStorage(configuration.GetConnectionString("DefaultConnection")));
        services.AddHangfireServer();

        services.AddCors(options =>
        {
            options.AddPolicy("AllowAngularApp",
                builder => builder
                    .AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader());
        });

        services.AddControllers()
            .AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            });

        services.AddProblemDetails()
                        .AddExceptionHandler<GlobalExceptionHandler>(); ;


        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = configuration["Jwt:Issuer"],
                ValidAudience = configuration["Jwt:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!))
            };

            // El navegador no puede enviar un header Authorization al navegar directo a una URL.
            // Para /hangfire (y solo esa ruta) se acepta el JWT como query string "access_token",
            // igual que hace SignalR para WebSockets. No afecta la autenticacion del resto de la API.
            options.Events = new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    if (context.Request.Path.StartsWithSegments("/hangfire") &&
                        context.Request.Query.TryGetValue("access_token", out var token))
                    {
                        context.Token = token;
                    }

                    return Task.CompletedTask;
                }
            };
        });

        services.AddAuthorization(options =>
        {
            foreach (var permissionCode in PermissionCodes.All)
            {
                options.AddPolicy(permissionCode, policy => policy.RequireClaim("permissions", permissionCode));
            }

            options.AddPolicy("CustomerPortalAccess", policy => policy.RequireClaim("userType", "Customer"));

            foreach (var permissionCode in CustomerPermissionCodes.All)
            {
                options.AddPolicy(RequireCustomerPermissionExtensions.PolicyPrefix + permissionCode, policy => policy
                    .RequireClaim("userType", "Customer")
                    .RequireClaim("customerPermissions", permissionCode));
            }
        });
        services.AddScoped<JwtService>();
        services.AddScoped(typeof(IAppLogger<>), typeof(LoggerAdapter<>));
        // builder.Services.AddWatchDog(builder.Configuration);
        // builder.Services.AddOpenTelemetry().UseAzureMonitor();
        services.AddHealthCheck(configuration);
        services.AddSignalR();
        services.AddSingleton<PrinterRegistry>();


        // Configurar OpenTelemetry
        // services.AddOpenTelemetry()
        //    .WithTracing(tracerProviderBuilder =>
        //     {
        //         tracerProviderBuilder
        //             .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("DEXTER24webapi"))
        //             .AddAspNetCoreInstrumentation()
        //             .AddHttpClientInstrumentation()
        //             .AddOtlpExporter(options =>
        //             {
        //                 options.Endpoint = new Uri("http://127.0.0.1:4317"); // Cambia esto a la URL de tu OTEL Collector
        //             });
        //     })


        //     .WithMetrics(meterProviderBuilder =>
        //     {
        //         meterProviderBuilder
        //             .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("DEXTER24webapiMetric"))
        //             .AddAspNetCoreInstrumentation()
        //             .AddHttpClientInstrumentation()
        //             .AddRuntimeInstrumentation()
        //             .AddOtlpExporter(options =>
        //             {
        //                 options.Endpoint = new Uri("http://127.0.0.1:4317"); // Cambia esto a la URL de tu OTEL Collector
        //             });
        //     });


        // services.AddLogging(loggingBuilder =>
        // {
        //     loggingBuilder.ClearProviders();
        //     loggingBuilder.AddConsole(); // Para log de consola
        //     loggingBuilder.AddOpenTelemetry(options =>
        //     {
        //         options.SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("DEXTER24webapi"));
        //         options.AddOtlpExporter(otlpOptions =>
        //         {
        //             otlpOptions.Endpoint = new Uri("http://localhost:4317"); // Dirección del OTEL Collector para logs
        //         });
        //     });
        // });


    }
}
