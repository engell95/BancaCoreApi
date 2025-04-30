using BancaCore.Application.Common.Enumerable;
using BancaCore.Common.Interfaces;
using BancaCore.Domain.Entities;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BancaCore.Application.CuentasBancarias.Commands.CreateCuentaBancaria
{
    public class CreateCuentaBancariaCommand : IRequest<string>
    {
        public string NumeroCuenta { get; set; }
        public int ClienteId { get; set; }
        public decimal SaldoInicial { get; set; }
    }

    /// <summary>
    /// Manejador del comando CreateCuentaBancariaCommand 
    /// Implementa la lógica de negocio para crear una nueva cuenta bancaria
    /// </summary>
    public class CreateCuentaBancariaCommandHandler : IRequestHandler<CreateCuentaBancariaCommand, string>
    {
        private readonly IBancaDbContext _context;

        public CreateCuentaBancariaCommandHandler(IBancaDbContext context)
        {
            _context = context;
        }

        public async Task<string> Handle(CreateCuentaBancariaCommand request, CancellationToken cancellationToken)
        {
            var cuenta = new CuentaBancaria
            {
                NumeroCuenta = request.NumeroCuenta,
                ClienteId = request.ClienteId,
                Saldo = request.SaldoInicial,
            };

            _context.CuentasBancarias.Add(cuenta);
            await _context.SaveChangesAsync(cancellationToken);

            var transaccion = new Transaccion
            {
                CuentaId = cuenta.Id,
                TipoTransaccion = (int)EnumTipoTransaccion.Deposito,
                Descripcion = "Depósito inicial",
                Monto =  request.SaldoInicial,
                SaldoPosterior = request.SaldoInicial,
                FechaTransaccion = DateTime.Now
            };

            _context.Transacciones.Add(transaccion);
            await _context.SaveChangesAsync(cancellationToken);

            return cuenta.NumeroCuenta;
        }
    }
}
