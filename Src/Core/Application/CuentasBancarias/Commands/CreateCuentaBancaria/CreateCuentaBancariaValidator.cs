using BancaCore.Common.Interfaces;
using FluentValidation;
using System.Linq;

namespace BancaCore.Application.CuentasBancarias.Commands.CreateCuentaBancaria
{
    public class CreateCuentaBancariaValidator : AbstractValidator<CreateCuentaBancariaCommand>
    {
        private readonly IBancaDbContext _context;
        public CreateCuentaBancariaValidator(IBancaDbContext context)
        {
            _context = context;

            RuleFor(v => v.ClienteId)
                .GreaterThan(1).WithMessage("El ID del cliente debe ser mayor a 1.")
                .Must(ClienteExiste).WithMessage("El cliente especificado no existe.");

            RuleFor(v => v.SaldoInicial)
                .GreaterThanOrEqualTo(0).WithMessage("El saldo inicial no puede ser negativo.");
        }

        private bool ClienteExiste(int ClienteId)
        {
            return _context.Clientes.Any(x => x.Id == ClienteId);
        }

    }
}
