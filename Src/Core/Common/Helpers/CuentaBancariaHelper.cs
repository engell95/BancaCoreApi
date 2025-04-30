using System;
using System.Globalization;

namespace BancaCore.Common.Enumerable
{
    public static class CuentaBancariaHelper
    {
        /// <summary>
        /// Genera un número de cuenta único
        /// </summary>
        /// <returns>Número de cuenta generado</returns>
        public static string GenerarNumeroCuenta()
        {
            // Base: Fecha + Hora precisa (incluye milisegundos)
            string timestamp = DateTime.Now.ToString("yyyyMMdd-HHmmss-fff", CultureInfo.InvariantCulture);

            // Parte adicional con un GUID acortado para asegurar unicidad (6 caracteres)
            string guidSegment = Guid.NewGuid().ToString("N")[..6]; // toma los primeros 6 caracteres

            string numeroCuenta = $"{timestamp}-{guidSegment}";

            // Asegurarse que no exceda los 50 caracteres
            if (numeroCuenta.Length > 50)
                numeroCuenta = numeroCuenta[..50];

            return numeroCuenta;
        }
    }
}
