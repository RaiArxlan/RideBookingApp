using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using IdentityService.Database;
using IdentityService.Model;
using Identity;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

ConfigureServices(builder.Services, builder.Configuration);

var app = builder.Build();

ConfigureMiddleware(app);

await app.RunAsync();

void ConfigureServices(IServiceCollection services, IConfiguration configuration)
{
    // Configure Database
    var isDocker = Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true";
    var connectionString = isDocker
        ? configuration.GetConnectionString("DockerConnectionString")
        : configuration.GetConnectionString("DefaultConnection");

    services.AddDbContext<IdentityDbContext>(options =>
        options.UseSqlServer(connectionString)
    );

    // Configure Identity
    services.AddIdentity<ApplicationUser, IdentityRole>()
        .AddEntityFrameworkStores<IdentityDbContext>()
        .AddDefaultTokenProviders();

    services.AddJwtAuthentication(configuration);

    services.AddControllers();
    services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(opt =>
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
