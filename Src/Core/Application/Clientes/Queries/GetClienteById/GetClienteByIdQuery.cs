using MediatR;

namespace BancaCore.Application.Clientes.Queries.GetClienteById
{
    public class GetClienteByIdQuery : IRequest<ClienteDto>
    {
        public int Id { get; set; }
    }
}