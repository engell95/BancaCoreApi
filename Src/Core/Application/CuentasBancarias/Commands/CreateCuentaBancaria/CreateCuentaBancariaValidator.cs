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

            RuleFor(v => v.NumeroCuenta)
            .NotEmpty().WithMessage("El número de cuenta es requerido.")
            .MaximumLength(50).WithMessage("El número de cuenta no puede exceder los 50 caracteres.")
            .Must(ExisteCuenta).WithMessage("El número de cuenta ya existe.");

            RuleFor(v => v.ClienteId)
                .GreaterThan(1).WithMessage("El ID del cliente debe ser mayor a 1.")
                .Must(ExisteCliente).WithMessage("El cliente no existe en nuestra base de datos.");

            RuleFor(v => v.SaldoInicial)
                .GreaterThanOrEqualTo(0).WithMessage("El saldo inicial no puede ser negativo.");
        }

        private bool ExisteCliente(int ClienteId)
        {

            if (_context.Clientes.Any(x => x.Id == ClienteId))
            {
                return true;
            }
            return false;
        }

        private bool ExisteCuenta(string NumeroCuenta)
        {

            if (!_context.CuentasBancarias.Any(x => x.NumeroCuenta == NumeroCuenta))
            {
                return true;
            }
            return false;
        }

    }
}
