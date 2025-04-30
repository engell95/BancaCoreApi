using AutoMapper;
using AutoMapper.QueryableExtensions;
using BancaCore.Common.Exceptions;
using BancaCore.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace BancaCore.Application.CuentasBancarias.Queries.GetCuentaBancariaById
{
    /// <summary>
    /// Manejador de la consulta GetCuentaBancariaByIdQuery 
    /// Implementa la lógica para obtener el detalle de una cuenta con sus transacciones de la base de datos
    /// </summary>
    public class GetCuentaBancariaByIdQueryHandler : IRequestHandler<GetCuentaBancariaByIdQuery, CuentaBancariaDto>
    {
        private readonly IBancaDbContext _context;
        private readonly IMapper _mapper;

        public GetCuentaBancariaByIdQueryHandler(IBancaDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<CuentaBancariaDto> Handle(GetCuentaBancariaByIdQuery request, CancellationToken cancellationToken)
        {
            var cuenta = await _context.CuentasBancarias
           .Include(c => c.Cliente)
           .Include(c => c.Transacciones)
           .AsNoTracking()
           .FirstOrDefaultAsync(c => c.NumeroCuenta == request.NumeroCuenta, cancellationToken);

            if (cuenta == null)
            {
                throw new NotFoundException(nameof(CuentasBancarias), request.NumeroCuenta);
            }

            return _mapper.Map<CuentaBancariaDto>(cuenta);
        }
    }
}
