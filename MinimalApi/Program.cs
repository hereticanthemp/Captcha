using Captcha;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCaptcha();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
    {
        var forecast = Enumerable.Range(1, 5).Select(index =>
                new WeatherForecast
                (
                    DateTime.Now.AddDays(index),
                    Random.Shared.Next(-20, 55),
                    summaries[Random.Shared.Next(summaries.Length)]
                ))
            .ToArray();
        return forecast;
    })
    .WithName("GetWeatherForecast");

app.MapGet("/Captcha", async () =>
    {
        using (var scope = app.Services.CreateScope())
        {
            var captcha = scope.ServiceProvider.GetRequiredService<ICaptchaFactory>();
            return await captcha.CreateAsync(new CaptchaOption
            {
                CharCount = 4,
                FontSizePt = 20,
                Type = CaptchaTypes.Numeric | CaptchaTypes.UpperCase | CaptchaTypes.Symbols
            });
        }
    })
    .WithName("GetCaptcha");

app.MapPost("/Captcha", async (CaptchaOption opt) =>
    {
        using var scope = app.Services.CreateScope();
        var captcha = scope.ServiceProvider.GetRequiredService<ICaptchaFactory>();
        return await captcha.CreateAsync(opt);
    })
    .WithName("Captcha");

app.Run();

internal record WeatherForecast(DateTime Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}