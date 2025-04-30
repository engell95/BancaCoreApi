using System;

namespace BancaCore.Common.Exceptions
{
    public class NotFoundException : Exception
    {
        public NotFoundException(string name, object key)
            : base($"Entity {name} value {key} was not found.")
        {
        }
    }
}
