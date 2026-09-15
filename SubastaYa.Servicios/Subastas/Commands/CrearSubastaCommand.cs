using SubastaYa.Servicios.Abstracciones;

namespace SubastaYa.Servicios.Subastas.Commands
{
    // El Command transporta los datos del formulario de publicación
    // (Módulo 2). No tiene lógica: eso vive en el Handler.
    public class CrearSubastaCommand : IRequest<ResultadoCreacionDto>
    {
        public int VendedorId { get; set; }
        public int CategoriaId { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string? UrlImagen { get; set; }
        public decimal PrecioBase { get; set; }
        public decimal IncrementoMinimo { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
    }
}
