using Microsoft.EntityFrameworkCore;      // necesario para AddDbContext y UseSqlServer
using SubastaYa.Infraestructura;          // necesario para poder usar la clase SubastaYaDbContext

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Registramos el DbContext como servicio disponible para toda la app.
// Le decimos que use SQL Server, y le pasamos la cadena de conexión
// que definimos en appsettings.json bajo la clave "DefaultConnection".
builder.Services.AddDbContext<SubastaYaDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();  // a partir de acá ya no se pueden registrar más servicios

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();       // genera el JSON de OpenAPI
    app.UseSwaggerUI();     // genera la página visual de Swagger que ya viste antes
}

app.UseHttpsRedirection();  // redirige requests HTTP a HTTPS

app.UseAuthorization();     // middleware de autorización (todavía no lo configuramos, pero viene default)

app.MapControllers();       // conecta las rutas de tus Controllers con el sistema de routing

app.Run();                  // arranca el servidor y se queda escuchando requests