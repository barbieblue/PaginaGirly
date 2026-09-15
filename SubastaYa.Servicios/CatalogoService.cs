//using Microsoft.EntityFrameworkCore;
//using SubastaYa.Dominio;
//using SubastaYa.Infraestructura;

//namespace SubastaYa.Servicios
//{
//    public class CatalogoService
//    {
//        private readonly SubastaYaDbContext _context;

//        public CatalogoService(SubastaYaDbContext context)
//        {
//            _context = context;
//        }

//        public async Task<object> ObtenerSubastasAsync(string estado = null, string categoria = null)
//        {
//            var query = _context.Subastas.Include(s => s.Categoria).AsQueryable();

//            if (!string.IsNullOrEmpty(estado))
//            {
//                query = query.Where(s => s.Estado == estado.ToUpper());
//            }

//            if (!string.IsNullOrEmpty(categoria))
//            {
//                query = query.Where(s => s.Categoria.Nombre.Contains(categoria));
//            }

//            var subastas = await query
//                .Select(s => new
//                {
//                    s.Id,
//                    s.Titulo,
//                    Categoria = s.Categoria.Nombre,
//                    s.Fecha_Fin,
//                    s.Estado,
//                    s.Url_Imagen,
//                    CantidadOfertas = _context.Pujas.Count(p => p.Subasta_Id == s.Id),
//                    OfertaMasAlta = _context.Pujas.Where(p => p.Subasta_Id == s.Id).Max(p => (decimal?)p.Monto) ?? s.Precio_Base
//                })
//                .ToListAsync();

//            return subastas;
//        }

//        // ⬇️ ACÁ ARRANCA LO NUEVO — todo esto va DENTRO de la clase CatalogoService,
//        // al mismo nivel que ObtenerSubastasAsync (no adentro de ese método).

//        // DTO con los datos que el vendedor completa en el formulario de publicación.
//        public class NuevaSubastaDto
//        {
//            public int VendedorId { get; set; }
//            public int CategoriaId { get; set; }
//            public string Titulo { get; set; }
//            public string Descripcion { get; set; }
//            public string UrlImagen { get; set; }
//            public decimal PrecioBase { get; set; }
//            public decimal IncrementoMinimo { get; set; }
//            public DateTime FechaInicio { get; set; }
//            public DateTime FechaFin { get; set; }
//        }

//        // Resultado simple para que el Controller sepa qué código HTTP devolver.
//        public class ResultadoCreacion
//        {
//            public bool Exito { get; set; }
//            public string? MensajeError { get; set; }
//            public int? SubastaId { get; set; }
//        }

        
//        public async Task<ResultadoCreacion> CrearSubastaAsync(NuevaSubastaDto dto)
//        {
//            // --- Validaciones que pide la consigna ---
//            // "La fecha de finalización debe ser posterior a la de inicio"
//            if (dto.FechaFin <= dto.FechaInicio)
//            {
//                return new ResultadoCreacion { Exito = false, MensajeError = "La fecha de fin debe ser posterior a la de inicio." };
//            }
//            // "El incremento mínimo y el precio base deben ser valores positivos coherentes"
//            if (dto.PrecioBase <= 0 || dto.IncrementoMinimo <= 0)
//            {
//                return new ResultadoCreacion { Exito = false, MensajeError = "El precio base y el incremento mínimo deben ser mayores a cero." };
//            }

//            // Confirmamos que el vendedor y la categoría realmente existan antes de crear la subasta.
//            var vendedorExiste = await _context.Usuarios.AnyAsync(u => u.Id == dto.VendedorId);
//            if (!vendedorExiste)
//            {
//                return new ResultadoCreacion { Exito = false, MensajeError = "El vendedor indicado no existe." };
//            }

//            var categoriaExiste = await _context.Categorias.AnyAsync(c => c.Id == dto.CategoriaId);
//            if (!categoriaExiste)
//            {
//                return new ResultadoCreacion { Exito = false, MensajeError = "La categoría indicada no existe." };
//            }
//            // El estado inicial depende de si ya arrancó o todavía no, según su fecha de inicio.
//            var estadoInicial = dto.FechaInicio <= DateTime.UtcNow ? "ACTIVA" : "PROGRAMADA";

//            var nuevaSubasta = new Subasta
//            {
//                Vendedor_Id = dto.VendedorId,
//                Categoria_Id = dto.CategoriaId,
//                Titulo = dto.Titulo,
//                Descripcion = dto.Descripcion,
//                Url_Imagen = dto.UrlImagen,
//                Precio_Base = dto.PrecioBase,
//                Incremento_Minimo = dto.IncrementoMinimo,
//                Fecha_Inicio = dto.FechaInicio,
//                Fecha_Fin = dto.FechaFin,
//                Estado = estadoInicial
//            };

//            _context.Subastas.Add(nuevaSubasta);
//            await _context.SaveChangesAsync();

//            return new ResultadoCreacion { Exito = true, SubastaId = nuevaSubasta.Id };
//        }
//        // ⬆️ ACÁ TERMINA LO NUEVO
//    }
//}