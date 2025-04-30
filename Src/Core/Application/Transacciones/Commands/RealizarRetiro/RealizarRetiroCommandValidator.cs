using BancaCore.Common.Interfaces;
using FluentValidation;
using System.Linq;

namespace BancaCore.Application.Transacciones.Commands.RealizarRetiro
{

    public class RealizarRetiroCommandValidator : AbstractValidator<RealizarRetiroCommand>
    {
        private readonly IBancaDbContext _context;

        public RealizarRetiroCommandValidator(IBancaDbContext context)
        {
            _context = context;

            RuleFor(v => v.NumeroCuenta)
                .NotEmpty().WithMessage("El número de cuenta bancaria es requerido.")
                .MaximumLength(50).WithMessage("El número de cuenta no puede exceder los 50 caracteres.")
                .Must(CuentaExiste).WithMessage("La cuenta bancaria especificada no existe.");

            RuleFor(v => v.Monto)
                .GreaterThan(0)
                .WithMessage("El monto del retiro debe ser mayor que 0.");

            RuleFor(v => v.Descripcion)
                .MaximumLength(200)
                .WithMessage("La descripción no puede exceder los 200 caracteres.");

            RuleFor(v => v)
               .Must(TieneSaldoSuficiente)
               .WithMessage("El saldo actual de la cuenta es insuficiente para realizar el retiro.");

        }

        private bool CuentaExiste(string numeroCuenta)
        {
            return _context.CuentasBancarias.Any(x => x.NumeroCuenta == numeroCuenta);
        }

        private bool TieneSaldoSuficiente(RealizarRetiroCommand command)
        {
            var cuenta = _context.CuentasBancarias
                .FirstOrDefault(c => c.NumeroCuenta == command.NumeroCuenta);

            if (cuenta == null)
                return true;

            return cuenta.Saldo >= command.Monto;
        }
    }
}