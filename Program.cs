var builder = WebApplication.CreateBuilder(args);

// Add controller support
builder.Services.AddControllers();

// Register HTTP Client
builder.Services.AddHttpClient();

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Map controllers
app.MapControllers();

app.Run();