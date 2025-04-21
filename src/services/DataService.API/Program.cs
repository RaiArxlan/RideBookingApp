var builder = WebApplication.CreateBuilder(args);

// Configure services
ConfigureServices(builder.Services);

var app = builder.Build();

// Configure middleware
ConfigureMiddleware(app);

await app.RunAsync();

void ConfigureServices(IServiceCollection services)
{
    // Add services to the container
    services.AddControllers();
    services.AddEndpointsApiExplorer();
    services.AddSwaggerGen();
}

void ConfigureMiddleware(WebApplication app)
{
    // Configure the HTTP request pipeline
    if (app.Environment.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
        app.UseSwagger();
        app.UseSwaggerUI();
    }
    else
    {
        app.UseExceptionHandler("/error");
        app.UseHttpsRedirection();
        app.UseHsts();
    }

    app.UseAuthorization();

    app.MapControllers();
    app.MapGet("/", () => "Data API is running!");
}
