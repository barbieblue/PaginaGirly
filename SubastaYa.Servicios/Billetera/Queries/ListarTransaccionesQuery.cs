using SubastaYa.Servicios.Abstracciones;

namespace SubastaYa.Servicios.Billetera.Queries
{
    // Nullable, mismo criterio que ObtenerSaldoQuery: el usuario podría no
    // tener billetera todavía.
    public class ListarTransaccionesQuery : IRequest<List<TransaccionDto>?>
    {
        public int UsuarioId { get; set; }
    }
}