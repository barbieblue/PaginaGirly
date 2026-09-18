using Microsoft.EntityFrameworkCore;
using SubastaYa.Dominio.Repositorios;
using SubastaYa.Infraestructura;
using SubastaYa.Infraestructura.Repositorios;
using SubastaYa.Infraestructura.Workers;
using SubastaYa.Servicios;
using SubastaYa.Servicios.Abstracciones;
using SubastaYa.Servicios.Auditoria.Queries;
using SubastaYa.Servicios.Billetera.Commands;
using SubastaYa.Servicios.Billetera.Queries;
using SubastaYa.Servicios.Categorias.Queries;
using SubastaYa.Servicios.Subastas.Commands;
using SubastaYa.Servicios.Subastas.Queries;
using SubastaYa.Servicios.Usuarios.Queries;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<SubastaYaDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// --- Proceso en segundo plano (cierre automático de subastas vencidas) ---
builder.Services.AddHostedService<ProcesosCierreSubastas>();

// --- Mediator (implementación propia, sin librería) ---
builder.Services.AddScoped<IMediator, Mediator>();

// --- Handlers de Categorías ---
builder.Services.AddScoped<IRequestHandler<ListarCategoriasQuery, List<CategoriaDto>>, ListarCategoriasQueryHandler>();


// --- Handlers de Subastas ---
builder.Services.AddScoped<IRequestHandler<ListarSubastasQuery, List<SubastaResumenDto>>, ListarSubastasQueryHandler>();
builder.Services.AddScoped<IRequestHandler<ObtenerDetalleSubastaQuery, SubastaDetalleDto?>, ObtenerDetalleSubastaQueryHandler>();
builder.Services.AddScoped<IRequestHandler<CrearSubastaCommand, ResultadoCreacionDto>, CrearSubastaCommandHandler>();
builder.Services.AddScoped<IRequestHandler<RegistrarPujaCommand, ResultadoPujaDto>, RegistrarPujaCommandHandler>();

// --- Handlers de Billetera ---
builder.Services.AddScoped<IRequestHandler<ObtenerSaldoQuery, SaldoDto?>, ObtenerSaldoQueryHandler>();
builder.Services.AddScoped<IRequestHandler<ListarTransaccionesQuery, List<TransaccionDto>?>, ListarTransaccionesQueryHandler>();
builder.Services.AddScoped<IRequestHandler<DepositarCommand, DepositarResultadoDto>, DepositarCommandHandler>();

// -- Handlers de Usuario ---
builder.Services.AddScoped<IRequestHandler<ListarUsuariosQuery, List<UsuarioDto>>, ListarUsuariosQueryHandler>();

// -- Handlers de Auditoria ---
builder.Services.AddScoped<IRequestHandler<ListarAuditoriaQuery, List<AuditoriaLogDto>>, ListarAuditoriaQueryHandler>();

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

// En Desarrollo, le pedimos al navegador que nunca guarde en caché los
// archivos estáticos (CSS/JS/HTML de wwwroot): así cada cambio se ve al
// instante con un F5 normal, sin tener que abrir DevTools y tildar
// "Disable cache" a mano cada vez.
app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        if (app.Environment.IsDevelopment())
        {
            ctx.Context.Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
            ctx.Context.Response.Headers["Pragma"] = "no-cache";
            ctx.Context.Response.Headers["Expires"] = "0";
        }
    }
});

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();