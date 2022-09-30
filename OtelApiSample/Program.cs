// Define some important constants and the activity source
using System.Reflection;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

var serviceName = "ArquitetoMovel.Samples.OtelApi";
//var serviceVersion = "1.0.0";

// Switch between Zipkin/Jaeger/OTLP by setting UseExporter in appsettings.json.
var tracingExporter = "Zipkin";

// OpenTelemetry
var assemblyVersion = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "unknown";

var builder = WebApplication.CreateBuilder(args);

var resourceBuilder = tracingExporter switch
{
    "jaeger" => ResourceBuilder.CreateDefault().AddService(builder.Configuration.GetValue<string>("Jaeger:ServiceName"), serviceVersion: assemblyVersion, serviceInstanceId: Environment.MachineName),
    "zipkin" => ResourceBuilder.CreateDefault().AddService(builder.Configuration.GetValue<string>("Zipkin:ServiceName"), serviceVersion: assemblyVersion, serviceInstanceId: Environment.MachineName),
    "otlp" => ResourceBuilder.CreateDefault().AddService(builder.Configuration.GetValue<string>("Otlp:ServiceName"), serviceVersion: assemblyVersion, serviceInstanceId: Environment.MachineName),
    _ => ResourceBuilder.CreateDefault().AddService(serviceName, serviceVersion: assemblyVersion, serviceInstanceId: Environment.MachineName),
};



// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure important OpenTelemetry settings, the console exporter, and automatic instrumentation
builder.Services.AddOpenTelemetryTracing(b =>
{
    b
    .SetResourceBuilder(resourceBuilder)
    .SetSampler(new AlwaysOnSampler())
    .AddHttpClientInstrumentation()
    .AddAspNetCoreInstrumentation()
    .AddSource(serviceName)
    .AddZipkinExporter(b =>
        {
            b.Endpoint = new Uri("http://localhost:9411/api/v2/spans");
        })
    .AddConsoleExporter();
    
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run();

