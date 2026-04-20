using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using vizsgaController.Model;
using vizsgaController.Persistence;


namespace vizsgaController
{
    public partial class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Adjust logging so we only see warnings/errors from the framework (no DB commands)
            // but keep our custom request logs (Information).
            builder.Logging.ClearProviders();
            builder.Logging.AddConsole();
            builder.Logging.AddFilter("Microsoft", LogLevel.Warning);
            builder.Logging.AddFilter("System", LogLevel.Warning);
            builder.Logging.AddFilter("Microsoft.Hosting.Lifetime", LogLevel.Information);

            builder.WebHost.UseUrls("http://0.0.0.0:3070");
            builder.Services.AddDbContextPool<NewsDbContext>(options => { options.UseNpgsql(builder.Configuration.GetConnectionString("ForumDb")); });
            // Add services to the container.
            builder.Services.AddTransient<IUserModel, UserModel>();
            builder.Services.AddTransient<INewsModel, NewsModel>();
            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(options =>
            {
                options.LoginPath = "/api/User/login";
                options.LogoutPath = "/api/User/logout";
                options.Cookie.HttpOnly = true;
                options.Cookie.SameSite = SameSiteMode.Lax;
                options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
                options.ExpireTimeSpan = TimeSpan.FromDays(7);
                options.SlidingExpiration = true;

                options.Events = new CookieAuthenticationEvents
                {
                    OnRedirectToLogin = ctx => { ctx.Response.StatusCode = 401; return Task.CompletedTask; },
                    OnRedirectToAccessDenied = ctx => { ctx.Response.StatusCode = 403; return Task.CompletedTask; }
                };
            });

            var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
                                 ?? new[] { "http://localhost:5500", "https://fcsab.ddns.net" };
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("FrontendPolicy", policy =>
                {
                    policy
                        .WithOrigins(allowedOrigins)
                        .AllowCredentials()   // required for cookies to work cross-origin
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                });
            });

            var app = builder.Build();

            // Custom middleware to log requests and IP addresses in a clear, readable way
            app.Use(async (context, next) =>
            {
                var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
                var ip = context.Connection.RemoteIpAddress?.ToString() ?? "Unknown IP";
                var method = context.Request.Method;
                var path = context.Request.Path;
                var queryStr = context.Request.QueryString.ToString();

                // Enable buffering so we can read the body and it can still be read by the endpoints later
                context.Request.EnableBuffering();
                var bodyAsText = "";
                if (context.Request.Body.CanRead)
                {
                    using var reader = new StreamReader(context.Request.Body, System.Text.Encoding.UTF8, false, 1024, true);
                    bodyAsText = await reader.ReadToEndAsync();
                    context.Request.Body.Position = 0; // Reset the stream position for the next middleware
                }

                logger.LogInformation("=========================================");
                if (!string.IsNullOrWhiteSpace(bodyAsText))
                {
                    logger.LogInformation("Request Body:");
                    logger.LogInformation("{Body}", bodyAsText);
                }
                logger.LogInformation("=========================================");

                await next();
            });

            // Configure the HTTP request pipeline.
            app.UseSwagger();
            app.UseSwaggerUI();

            // Serve static files from uploads folder
            var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "uploads");
            Directory.CreateDirectory(uploadsPath);
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(uploadsPath),
                RequestPath = "/uploads"
            });

            app.UseCors("FrontendPolicy");

            app.UseAuthentication();
            app.UseAuthorization();


            app.MapControllers();

            app.Run();

        }
    }
    public partial class Program { }
}
