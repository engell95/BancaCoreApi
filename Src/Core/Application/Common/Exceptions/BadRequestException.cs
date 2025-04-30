using System;

namespace BancaCore.Common.Exceptions
{
    /// <summary>
    /// Excepción personalizada que se lanza cuando ocurre un error de solicitud incorrecta (Bad Request).
    /// </summary>
    public class BadRequestException : Exception
    {
        /// <summary>
        /// Crea una nueva instancia de la excepción BadRequestException con un mensaje personalizado.
        /// </summary>
        /// <param name="message">Mensaje que describe el error.</param>
        public BadRequestException(string message)
            : base($"Solicitud incorrecta: {message}")
        {
        }
    }
}
