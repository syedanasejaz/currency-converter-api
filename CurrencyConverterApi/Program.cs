using CurrencyConverterApi.Services.Implementation;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Register IMemoryCache as a service
builder.Services.AddMemoryCache(); // Adds IMemoryCache to DI container

// Register HttpClient for dependency injection
builder.Services.AddHttpClient<CurrencyService>();

// Register the CurrencyService as a singleton
builder.Services.AddSingleton<CurrencyService>();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthorization();

app.MapControllers();

app.Run();
