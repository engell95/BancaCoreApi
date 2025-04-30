using BancaCore.Application.Transacciones.Commands.RealizarDeposito;
using BancaCore.Common.Interfaces;
using BancaCore.Domain.Entities;
using MediatR;
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
        private readonly IMediator _mediator;

        public CreateCuentaBancariaCommandHandler(IBancaDbContext context, IMediator mediator)
        {
            _context = context;
            _mediator = mediator;
        }

        public async Task<string> Handle(CreateCuentaBancariaCommand request, CancellationToken cancellationToken)
        {
            // Crear la cuenta bancaria con saldo inicial en cero
            var cuenta = new CuentaBancaria
            {
                NumeroCuenta = request.NumeroCuenta,
                ClienteId = request.ClienteId,
                Saldo = 0, // Inicializar en cero, el depósito actualizará el saldo
            };

            _context.CuentasBancarias.Add(cuenta);
            await _context.SaveChangesAsync(cancellationToken);

            // Si hay un saldo inicial, realizar un depósito utilizando el comando existente
            if (request.SaldoInicial > 0)
            {
                var depositoCommand = new RealizarDepositoCommand
                {
                    NumeroCuenta = cuenta.NumeroCuenta,
                    Monto = request.SaldoInicial,
                    Descripcion = "Depósito inicial"
                };

                await _mediator.Send(depositoCommand, cancellationToken);
            }

            return cuenta.NumeroCuenta;
        }
    }
}