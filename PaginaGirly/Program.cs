using Microsoft.EntityFrameworkCore;
using SubastaYa.Dominio.Repositorios;
using SubastaYa.Infraestructura;
using SubastaYa.Infraestructura.Repositorios;
using SubastaYa.Infraestructura.Workers;
using SubastaYa.Servicios;
using SubastaYa.Servicios.Abstracciones;
using SubastaYa.Servicios.Billetera.Commands;
using SubastaYa.Servicios.Billetera.Queries;
using SubastaYa.Servicios.Subastas.Commands;
using SubastaYa.Servicios.Subastas.Queries;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<SubastaYaDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// --- Proceso en segundo plano (cierre automático de subastas vencidas) ---
builder.Services.AddHostedService<ProcesosCierreSubastas>();

// --- Repository + Unit of Work ---
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// --- Mediator (implementación propia, sin librería) ---
builder.Services.AddScoped<IMediator, Mediator>();

// --- Handlers de Subastas ---
builder.Services.AddScoped<IRequestHandler<ListarSubastasQuery, List<SubastaResumenDto>>, ListarSubastasQueryHandler>();
builder.Services.AddScoped<IRequestHandler<ObtenerDetalleSubastaQuery, SubastaDetalleDto?>, ObtenerDetalleSubastaQueryHandler>();
builder.Services.AddScoped<IRequestHandler<CrearSubastaCommand, ResultadoCreacionDto>, CrearSubastaCommandHandler>();
builder.Services.AddScoped<IRequestHandler<RegistrarPujaCommand, ResultadoPujaDto>, RegistrarPujaCommandHandler>();

// --- Handlers de Billetera ---
builder.Services.AddScoped<IRequestHandler<ObtenerSaldoQuery, SaldoDto?>, ObtenerSaldoQueryHandler>();
builder.Services.AddScoped<IRequestHandler<ListarTransaccionesQuery, List<TransaccionDto>?>, ListarTransaccionesQueryHandler>();
builder.Services.AddScoped<IRequestHandler<DepositarCommand, DepositarResultadoDto>, DepositarCommandHandler>();

// --- Services que todavía NO están migrados a CQRS ---
// (los usan CategoriasController, UsuarioController y AuditoriaController)
builder.Services.AddScoped<CatalogoService>();
builder.Services.AddScoped<AuditoriaService>();
builder.Services.AddScoped<UsuarioService>();

var app = builder.Build();

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

app.UseDefaultFiles();
app.UseStaticFiles();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();