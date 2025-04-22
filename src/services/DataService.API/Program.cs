using Identity;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Configure services
ConfigureServices(builder.Services, builder.Configuration);

var app = builder.Build();

// Configure middleware
ConfigureMiddleware(app);

await app.RunAsync();

void ConfigureServices(IServiceCollection services, IConfiguration configuration)
{
    // Add services to the container
    services.AddControllers();

    services.AddJwtAuthentication(configuration);

    services.AddControllers();
    services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(opt =>
    {
        opt.SwaggerDoc("v1", new OpenApiInfo { Title = "Data Service", Version = "v1" });
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
    }); services.AddAuthorization();
}

void ConfigureMiddleware(WebApplication app)
{
    // Configure the HTTP request pipeline
    if (app.Environment.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
        app.UseSwagger();
        app.UseSwaggerUI();

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

    // Add authentication and authorization middleware
    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();
    app.MapGet("/", () => "Data API is running!");
}