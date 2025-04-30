using System;

namespace BancaCore.Common.Exceptions
{
    /// <summary>
    /// Excepción personalizada que se lanza cuando no se encuentra una entidad.
    /// </summary>
    public class NotFoundException : Exception
    {
        /// <summary>
        /// Crea una nueva instancia de la excepción NotFoundException con un mensaje personalizado en español.
        /// </summary>
        /// <param name="name">Nombre de la entidad.</param>
        /// <param name="key">Clave o valor buscado.</param>
        public NotFoundException(string name, object key)
            : base($"La entidad '{name}' con el valor '{key}' no fue encontrada.")
        {
        }
    }
}
