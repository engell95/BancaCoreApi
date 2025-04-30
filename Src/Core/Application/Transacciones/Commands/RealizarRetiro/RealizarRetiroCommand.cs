using BancaCore.Common.Enumerable;
using BancaCore.Common.Exceptions;
using BancaCore.Common.Interfaces;
using BancaCore.Domain.Entities;
using FluentValidation.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BancaCore.Application.Transacciones.Commands.RealizarRetiro
{
    /// <summary>
    /// Comando para realizar un retiro de una cuenta bancaria
    /// </summary>
    public class RealizarRetiroCommand : IRequest<int>
    {
        /// <summary>
        /// Número de cuenta bancaria de donde se realizará el retiro
        /// </summary>
        public string NumeroCuenta { get; set; }

        /// <summary>
        /// Monto a retirar (debe ser positivo)
        /// </summary>
        public decimal Monto { get; set; }

        /// <summary>
        /// Descripción opcional de la transacción
        /// </summary>
        public string Descripcion { get; set; }
    }

    /// <summary>
    /// Manejador para el comando de realizar retiro
    /// </summary>
    public class RealizarRetiroCommandHandler : IRequestHandler<RealizarRetiroCommand, int>
    {
        private readonly IBancaDbContext _context;

        public RealizarRetiroCommandHandler(IBancaDbContext context)
        {
            _context = context;
        }

        public async Task<int> Handle(RealizarRetiroCommand request, CancellationToken cancellationToken)
        {
            // Obtener la cuenta bancaria por número de cuenta
            var cuenta = await _context.CuentasBancarias
                .FirstOrDefaultAsync(c => c.NumeroCuenta == request.NumeroCuenta, cancellationToken);

            // Validar que haya saldo suficiente
            if (cuenta.Saldo < request.Monto)
            {
                var failures = new List<ValidationFailure>
                {
                    new ValidationFailure("Monto", $"El saldo de la cuenta ({cuenta.Saldo:C}) es insuficiente para realizar este retiro de {request.Monto:C}.")
                };
                throw new ValidationException(failures);
            }

            // Actualizar el saldo de la cuenta
            cuenta.Saldo -= request.Monto;

            // Crear la transacción
            var transaccion = new Transaccion
            {
                CuentaId = cuenta.Id,
                TipoTransaccion = (int)EnumTipoTransaccion.Retiro,
                Descripcion = string.IsNullOrEmpty(request.Descripcion) 
                    ? "Retiro de cuenta" 
                    : request.Descripcion,
                Monto = -request.Monto, // Monto negativo para retiros
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