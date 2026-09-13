using SubastaYa.Servicios.Abstracciones;

namespace SubastaYa.Servicios.Subastas.Commands
{
    public class RegistrarPujaCommand : IRequest<ResultadoPujaDto>
    {
        public int SubastaId { get; set; }
        public int UsuarioId { get; set; }
        public decimal Monto { get; set; }
    }
}
