using AutoMapper;
using AutoMapper.QueryableExtensions;
using BancaCore.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BancaCore.Application.Clientes.Queries.GetAllClientes
{
    /// <summary>
    /// Manejador de la consulta GetAllClientesQuery 
    /// Implementa la lógica para obtener todos los clientes de la base de datos
    /// </summary>
    public class GetAllClientesQueryHandler : IRequestHandler<GetAllClientesQuery, List<ClienteDto>>
    {
        private readonly IBancaDbContext _context;
        private readonly IMapper _mapper;

        public GetAllClientesQueryHandler(IBancaDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<List<ClienteDto>> Handle(GetAllClientesQuery request, CancellationToken cancellationToken)
        {

            var clientes = await _context.Clientes
           .OrderByDescending(x => x.Id)
           .ProjectTo<ClienteDto>(_mapper.ConfigurationProvider)
           .ToListAsync(cancellationToken);

            return clientes;
        }
    }
}