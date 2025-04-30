using BancaCore.Application.Common.Enumerable;
using BancaCore.Common.Interfaces;
using BancaCore.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BancaCore.Application.Transacciones.Commands.RealizarDeposito
{
    /// <summary>
    /// Comando para realizar un depósito en una cuenta bancaria
    /// </summary>
    public class RealizarDepositoCommand : IRequest<int>
    {
        /// <summary>
        /// Número de cuenta bancaria donde se realizará el depósito
        /// </summary>
        public string NumeroCuenta { get; set; }

        /// <summary>
        /// Monto a depositar (debe ser positivo)
        /// </summary>
        public decimal Monto { get; set; }

        /// <summary>
        /// Descripción opcional de la transacción
        /// </summary>
        public string Descripcion { get; set; }
    }

    /// <summary>
    /// Manejador para el comando de realizar depósito
    /// </summary>
    public class RealizarDepositoCommandHandler : IRequestHandler<RealizarDepositoCommand, int>
    {
        private readonly IBancaDbContext _context;

        public RealizarDepositoCommandHandler(IBancaDbContext context)
        {
            _context = context;
        }

        public async Task<int> Handle(RealizarDepositoCommand request, CancellationToken cancellationToken)
        {
            // Obtener la cuenta bancaria por número de cuenta
            var cuenta = await _context.CuentasBancarias
                .FirstOrDefaultAsync(c => c.NumeroCuenta == request.NumeroCuenta, cancellationToken);

            // Actualizar el saldo de la cuenta
            cuenta.Saldo += request.Monto;

            // Crear la transacción
            var transaccion = new Transaccion
            {
                CuentaId = cuenta.Id,
                TipoTransaccion = (int)EnumTipoTransaccion.Deposito,
                Descripcion = string.IsNullOrEmpty(request.Descripcion) 
                    ? "Depósito en cuenta" 
                    : request.Descripcion,
                Monto = request.Monto,
                SaldoPosterior = cuenta.Saldo,
                FechaTransaccion = DateTime.Now
            };

            // Guardar la transacción
            _context.Transacciones.Add(transaccion);
            await _context.SaveChangesAsync(cancellationToken);

            return transaccion.Id;
        }
    }
}