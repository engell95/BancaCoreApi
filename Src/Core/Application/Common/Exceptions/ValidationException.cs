using System;
using System.Linq;
using System.Collections.Generic;
using FluentValidation.Results;

namespace BancaCore.Common.Exceptions
{
    /// <summary>
    /// Excepción personalizada que se lanza cuando una o más validaciones han fallado.
    /// </summary>
    public class ValidationException : Exception
    {
        /// <summary>
        /// Diccionario que contiene los errores de validación agrupados por propiedad.
        /// </summary>
        public IDictionary<string, string[]> Failures { get; }

        /// <summary>
        /// Crea una nueva instancia de la excepción ValidationException sin errores específicos.
        /// </summary>
        public ValidationException()
            : base("Se produjeron uno o más errores de validación.")
        {
            Failures = new Dictionary<string, string[]>();
        }

        /// <summary>
        /// Crea una nueva instancia de la excepción ValidationException con una lista de errores de validación.
        /// </summary>
        /// <param name="failures">Lista de errores de validación.</param>
        public ValidationException(List<ValidationFailure> failures)
            : this()
        {
            var propertyNames = failures
                .Select(e => e.PropertyName)
                .Distinct();

            foreach (var propertyName in propertyNames)
            {
                var propertyFailures = failures
                    .Where(e => e.PropertyName == propertyName)
                    .Select(e => e.ErrorMessage)
                    .ToArray();

                Failures.Add(propertyName == "" ? "mensaje" : propertyName, propertyFailures);
            }
        }

    }

}
