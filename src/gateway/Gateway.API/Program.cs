using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

ConfigureServices(builder.Services, builder.Configuration);

var app = builder.Build();

ConfigureMiddleware(app);

await app.RunAsync();

void ConfigureServices(IServiceCollection services, IConfiguration configuration)
{
    // Add YARP services
    services.AddReverseProxy()
        .LoadFromConfig(configuration.GetSection("ReverseProxy"));

    // Add rate limiting
    services.AddRateLimiter(options =>
    {
        options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
            RateLimitPartition.GetFixedWindowLimiter("fixed", _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 100,
                Window = TimeSpan.FromMinutes(1),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 2
            }));
    });
}

void ConfigureMiddleware(WebApplication app)
{
    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
    }
    // Enable HSTS
    app.UseHsts();

    // Enable HTTPS redirection
    app.UseHttpsRedirection();

    // Enable Authorization
    app.UseAuthorization();

    // Use YARP reverse proxy
    app.MapReverseProxy();

    // Use rate limiting
    app.UseRateLimiter();

    app.MapGet("/", () => "Gateway API is running!");
}
