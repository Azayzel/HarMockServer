using HarMockServer.Middleware;
using HarMockServer.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add YARP services
builder.Services.AddHttpForwarder();

// Add custom services
builder.Services.AddSingleton<IHarService, HarService>();
builder.Services.AddScoped<IMockResponseService, MockResponseService>();

// Configure CORS for development
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseCors();
}

app.UseHttpsRedirection();

// Add HAR mock middleware before routing
app.UseMiddleware<HarMockMiddleware>();

app.UseRouting();
app.UseAuthorization();
app.MapControllers();

// Health check endpoint
app.MapGet("/health", () => Results.Ok(new { Status = "Healthy", Timestamp = DateTime.UtcNow }));

// Info endpoint
app.MapGet("/", () => Results.Ok(new
{
    Service = "HAR Mock Server",
    Version = "1.0.0",
    Endpoints = new
    {
        Upload = "/api/har/upload",
        Environments = "/api/har/environments",
        Mock = "/mock/{environmentId}/{**path}",
        Health = "/health",
        Swagger = "/swagger"
    }
}));

app.Run();