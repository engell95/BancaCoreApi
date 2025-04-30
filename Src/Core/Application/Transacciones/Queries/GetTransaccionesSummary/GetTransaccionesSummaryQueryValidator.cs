using BancaCore.Common.Interfaces;
using FluentValidation;
using System.Linq;

namespace BancaCore.Application.Transacciones.Queries.GetTransaccionesSummary
{
    public class GetTransaccionesSummaryQueryValidator : AbstractValidator<GetTransaccionesSummaryQuery>
    {
        private readonly IBancaDbContext _context;

        public GetTransaccionesSummaryQueryValidator(IBancaDbContext context)
        {
            _context = context;

            RuleFor(v => v.NumeroCuenta)
                .NotEmpty().WithMessage("El número de cuenta es requerido.")
                .Must(ExisteCuenta).WithMessage("La cuenta especificada no existe.");
        }

        private bool ExisteCuenta(string numeroCuenta)
        {
            return _context.CuentasBancarias
                .Any(c => c.NumeroCuenta == numeroCuenta);
        }
    }
}