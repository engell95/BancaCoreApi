using BancaCore.Common.Enumerable;
using BancaCore.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BancaCore.Application.Transacciones.Queries.GetTransaccionesSummary
{
    /// <summary>
    /// Query para obtener el resumen de transacciones de una cuenta bancaria
    /// </summary>
    public class GetTransaccionesSummaryQuery : IRequest<TransaccionesSummaryDto>
    {
        /// <summary>
        /// Número de cuenta bancaria para la cual se solicita el resumen de transacciones
        /// </summary>
        public string NumeroCuenta { get; set; }
    }

    /// <summary>
    /// Manejador para la query de obtener el resumen de transacciones
    /// </summary>
    public class GetTransaccionesSummaryQueryHandler : IRequestHandler<GetTransaccionesSummaryQuery, TransaccionesSummaryDto>
    {
        private readonly IBancaDbContext _context;

        public GetTransaccionesSummaryQueryHandler(IBancaDbContext context)
        {
            _context = context;
        }

        public async Task<TransaccionesSummaryDto> Handle(GetTransaccionesSummaryQuery request, CancellationToken cancellationToken)
        {
            // Obtener la cuenta bancaria por número de cuenta
            var cuenta = await _context.CuentasBancarias
                .Include(c => c.Transacciones)
                .FirstOrDefaultAsync(c => c.NumeroCuenta == request.NumeroCuenta, cancellationToken);

            // Crear el DTO de respuesta
            var response = new TransaccionesSummaryDto
            {
                NumeroCuenta = cuenta.NumeroCuenta,
                SaldoFinal = cuenta.Saldo,
                Transacciones = cuenta.Transacciones
                    .OrderBy(t => t.FechaTransaccion)
                    .Select(t => new TransaccionDetailDto
                    {
                        Id = t.Id,
                        TipoTransaccion = ((EnumTipoTransaccion)t.TipoTransaccion).ToString(),
                        Monto = t.Monto,
                        SaldoPosterior = t.SaldoPosterior,
                        Descripcion = t.Descripcion,
                        FechaTransaccion = t.FechaTransaccion
                    })
                    .ToList()
            };

            return response;
        }
    }
}