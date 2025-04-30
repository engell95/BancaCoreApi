using FluentValidation;
using System;

namespace BancaCore.Application.Clientes.Commands.UpdateCliente
{
    public class UpdateClienteCommandValidator : AbstractValidator<UpdateClienteCommand>
    {
        public UpdateClienteCommandValidator()
        {
            RuleFor(v => v.Id)
                .NotEmpty().WithMessage("El ID del cliente es requerido.");

            RuleFor(v => v.Nombre)
                .NotEmpty().WithMessage("El nombre es requerido.")
                .MaximumLength(100).WithMessage("El nombre no puede exceder los 100 caracteres.");

            RuleFor(v => v.FechaNacimiento)
                .NotEmpty().WithMessage("La fecha de nacimiento es requerida.")
                .Must(BeValidBirthDate).WithMessage("La fecha de nacimiento debe ser en el pasado.")
                .Must(BeAtLeast18YearsOld).WithMessage("El cliente debe tener al menos 18 años.");

            RuleFor(v => v.Sexo)
                .NotEmpty().WithMessage("El sexo es requerido.")
                .Must(BeValidGender).WithMessage("El sexo debe ser 'M' o 'F'.");

            RuleFor(v => v.Ingresos)
                .NotEmpty().WithMessage("Los ingresos son requeridos.")
                .GreaterThan(0).WithMessage("Los ingresos deben ser mayores que cero.");
        }

        private bool BeValidBirthDate(DateOnly date)
        {
            return date < DateOnly.FromDateTime(DateTime.Now);
        }

        private bool BeAtLeast18YearsOld(DateOnly date)
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            var age = today.Year - date.Year;
            
            if (date > today.AddYears(-age))
                age--;

            return age >= 18;
        }

        private bool BeValidGender(string gender)
        {
            return gender == "M" || gender == "F";
        }
    }
}