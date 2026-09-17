using SubastaYa.Dominio.Repositorios;
using SubastaYa.Servicios.Abstracciones;

namespace SubastaYa.Servicios.Categorias.Queries
{
    public class ListarCategoriasQueryHandler : IRequestHandler<ListarCategoriasQuery, List<CategoriaDto>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public ListarCategoriasQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<CategoriaDto>> Handle(ListarCategoriasQuery request)
        {
            var categorias = await _unitOfWork.Categorias.GetAllAsync();

            return categorias
                .Select(c => new CategoriaDto
                {
                    Id = c.Id,
                    Nombre = c.Nombre,
                    UrlIcono = c.Url_Icono
                })
                .ToList();
        }
    }
}
