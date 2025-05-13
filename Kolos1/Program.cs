using Kolos1.Services;

var builder = WebApplication.CreateBuilder();
builder.Services.AddControllers();
builder.Services.AddScoped<IDeliveryService, DeliveryService>();
var app = builder.Build();
app.UseAuthorization();
app.MapControllers();
app.Run();