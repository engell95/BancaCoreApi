using AutoMapper;
using AutoMapper.QueryableExtensions;
using BancaCore.Common.Exceptions;
using BancaCore.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace BancaCore.Application.Clientes.Queries.GetClienteById
{
    /// <summary>
    /// Manejador de la consulta GetClienteByIdQuery 
    /// Implementa la lógica para obtener el detalle de un clientes de la base de datos
    /// </summary>
    public class GetClienteByIdQueryHandler : IRequestHandler<GetClienteByIdQuery, ClienteDto>
    {
        private readonly IBancaDbContext _context;
        private readonly IMapper _mapper;

        public GetClienteByIdQueryHandler(IBancaDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<ClienteDto> Handle(GetClienteByIdQuery request, CancellationToken cancellationToken)
        {
            var cliente = await _context.Clientes
                .AsNoTracking()
                .ProjectTo<ClienteDto>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

            if (cliente == null)
            {
                throw new NotFoundException(nameof(Clientes), request.Id);
            }

            return cliente;
        }
    }
}