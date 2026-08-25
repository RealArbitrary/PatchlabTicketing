using Microsoft.Extensions.FileProviders;
using PatchlabTicketing.Api.Data;
using PatchlabTicketing.Api.Json;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new UtcDateTimeConverter());
    });
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddScoped<TicketRepository>();
builder.Services.AddScoped<ErrorLogRepository>();
builder.Services.AddScoped<TicketFeedbackRepository>();
builder.Services.AddScoped<TicketCommentRepository>();
builder.Services.AddScoped<TicketPhotoRepository>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactDev", policy =>
        policy.WithOrigins("http://localhost:5173", "http://localhost:3000")
              .AllowAnyMethod()
              .AllowAnyHeader());
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

// Serves ticket photo files saved by PatchlabWhatsAppBot. Scoped narrowly to
// just this folder (not the bot's whole working directory, which also holds
// config.json/secrets) via a dedicated PhysicalFileProvider, so nothing
// outside TicketPhotos is reachable through this route.
var ticketPhotosRoot = Path.GetFullPath(Path.Combine(
    app.Environment.ContentRootPath,
    app.Configuration["TicketPhotosRootPath"] ?? "TicketPhotos"));
Directory.CreateDirectory(ticketPhotosRoot);
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(ticketPhotosRoot),
    RequestPath = "/photos",
});

app.UseCors("AllowReactDev");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
