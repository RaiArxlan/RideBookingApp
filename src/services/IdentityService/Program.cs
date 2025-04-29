using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using IdentityService.Database;
using IdentityService.Model;
using Identity;
using Microsoft.OpenApi.Models;
using SharedKernel.Interfaces;
using SharedKernel.Services;

var builder = WebApplication.CreateBuilder(args);

ConfigureServices(builder.Services, builder.Configuration);

var app = builder.Build();

ConfigureMiddleware(app);

await app.RunAsync();

void ConfigureServices(IServiceCollection services, IConfiguration configuration)
{
    var isDocker = Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true";
    var connectionString = isDocker
        ? configuration.GetConnectionString("DockerConnectionString")
        : configuration.GetConnectionString("DefaultConnection");

    services.AddDbContext<IdentityDbContext>(options =>
        options.UseNpgsql(connectionString)
    );

    // Configure Identity
    services.AddIdentity<ApplicationUser, IdentityRole>()
        .AddEntityFrameworkStores<IdentityDbContext>()
        .AddDefaultTokenProviders();

    services.AddJwtAuthentication(configuration);

    services.AddControllers();
    services.AddEndpointsApiExplorer();
    services.AddSwaggerGen(opt =>
    {
        opt.SwaggerDoc("v1", new OpenApiInfo { Title = "Identity Service", Version = "v1" });
        opt.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            In = ParameterLocation.Header,
            Description = "Please enter token",
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            BearerFormat = "JWT",
            Scheme = "bearer"
        });
        opt.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type=ReferenceType.SecurityScheme,
                        Id="Bearer"
                    }
                },
                Array.Empty<string>()
            }
        });
    });

    services.AddAuthorization();

    services.AddSingleton<IRabbitMqPublisher, RabbitMqPublisher>();
}

void ConfigureMiddleware(WebApplication app)
{
    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
        app.UseSwagger();
        app.UseSwaggerUI();

        // Apply database migrations in development
        using (var scope = app.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();

            // Check if there are any pending migrations
            if (context.Database.GetPendingMigrations().Any())
            {
                // Apply migrations
                context.Database.Migrate();
            }

            // Register defaul user with admin role using UserManager
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            var user = new ApplicationUser()
            {
                Email = "raiarslan4671@gmail.com",
                UserName = "raiarslan4671@gmail.com",
                FullName = "Arslan Rai",
                Address = "Lahore"
            };

            // Check if the user already exists
            var existingUser = userManager.FindByEmailAsync(user.Email).Result;
            if (existingUser == null)
            {
                if (userManager.CreateAsync(user, "AVeryStrongPassword@1").Result.Succeeded)
                {
                    // User created successfully
                    existingUser = userManager.FindByEmailAsync(user.Email).Result;
                }
                else
                {
                    return;
                }
            }

            if (!roleManager.RoleExistsAsync("Admin").Result)
            {
                var role = new IdentityRole("Admin");
                roleManager.CreateAsync(role).Wait();
            }

            if (!userManager.IsInRoleAsync(existingUser!, "Admin").Result)
            {
                userManager.AddToRoleAsync(existingUser!, "Admin").Wait();
            }
        }

        app.UseCors(builder =>
        {
            builder.AllowAnyOrigin()
                   .AllowAnyMethod()
                   .AllowAnyHeader();
        });
    }
    else
    {
        app.UseExceptionHandler("/error");
        app.UseHttpsRedirection();
        app.UseHsts();
    }

    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();
    app.MapGet("/", () => "Identity Service is running!");
}
