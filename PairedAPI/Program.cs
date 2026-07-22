using Microsoft.Extensions.DependencyInjection;
using PairedAPI.Data;
using PairedAPI.Services;

var builder = WebApplication.CreateBuilder(args);

// Add controllers
builder.Services.AddControllers();

// Register Database + Services
builder.Services.AddSingleton<Database>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<RequestService>();
builder.Services.AddScoped<SessionService>();
builder.Services.AddScoped<FeedbackService>();
builder.Services.AddScoped<MessageService>();
// register connection string value
builder.Services.AddSingleton(sp =>
    sp.GetRequiredService<IConfiguration>().GetConnectionString("DefaultConnection") 
    ?? throw new InvalidOperationException("Missing connection string"));

// services can now resolve string
builder.Services.AddScoped<RequestService>();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
