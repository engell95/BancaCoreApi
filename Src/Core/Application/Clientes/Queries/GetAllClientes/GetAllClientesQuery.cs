using System.Collections.Generic;
using MediatR;

namespace BancaCore.Application.Clientes.Queries.GetAllClientes
{
    public class GetAllClientesQuery : IRequest<List<ClienteDto>>
    {
    }
}