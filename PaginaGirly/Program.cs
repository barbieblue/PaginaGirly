using Microsoft.EntityFrameworkCore;      // necesario para AddDbContext y UseSqlServer
using SubastaYa.Infraestructura;          // para usar la clase SubastaYaDbContext
using SubastaYa.Servicios;                // para usar PujaService y CatalogoService
using SubastaYa.Dominio.Repositorios;
using SubastaYa.Infraestructura.Repositorios;
using SubastaYa.Infraestructura.Workers;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHostedService<ProcesosCierreSubastas>();

// Registra el DbContext, usando la cadena de conexión del appsettings.json
builder.Services.AddDbContext<SubastaYaDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Registra los servicios de la capa Application (casos de uso) para que
// el Controller pueda "pedirlos" por inyección de dependencias en su constructor.
builder.Services.AddScoped<PujaService>();
builder.Services.AddScoped<CatalogoService>();
builder.Services.AddScoped<BilleteraService>();

var app = builder.Build();  // a partir de acá ya no se pueden registrar más servicios



// Al arrancar, si la base está vacía, la llenamos con los datos de prueba del TP.

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<SubastaYaDbContext>();
    SeedData.Inicializar(context);
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();