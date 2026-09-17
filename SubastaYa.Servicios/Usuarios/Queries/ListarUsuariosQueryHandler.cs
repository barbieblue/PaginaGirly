using SubastaYa.Dominio.Repositorios;
using SubastaYa.Servicios.Abstracciones;

namespace SubastaYa.Servicios.Usuarios.Queries
{
    public class ListarUsuariosQueryHandler : IRequestHandler<ListarUsuariosQuery, List<UsuarioDto>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public ListarUsuariosQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<UsuarioDto>> Handle(ListarUsuariosQuery request)
        {
            var usuarios = await _unitOfWork.Usuarios.GetAllAsync();

            return usuarios
                .Select(u => new UsuarioDto
                {
                    Id = u.Id,
                    Nombre = u.Nombre,
                    Email = u.Email
                })
                .ToList();
        }
    }
}