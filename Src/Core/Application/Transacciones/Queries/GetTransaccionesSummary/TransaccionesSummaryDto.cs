using BancaCore.Common.Mappings;
using BancaCore.Domain.Entities;
using System;
using System.Collections.Generic;

namespace BancaCore.Application.Transacciones.Queries.GetTransaccionesSummary
{
    /// <summary>
    /// DTO para el resumen de transacciones de una cuenta bancaria
    /// </summary>
    public class TransaccionesSummaryDto
    {
        /// <summary>
        /// Número de cuenta bancaria
        /// </summary>
        public string NumeroCuenta { get; set; }
        
        /// <summary>
        /// Saldo final de la cuenta después de todas las transacciones
        /// </summary>
        public decimal SaldoFinal { get; set; }
        
        /// <summary>
        /// Lista de todas las transacciones realizadas en la cuenta
        /// </summary>
        public List<TransaccionDetailDto> Transacciones { get; set; } = new List<TransaccionDetailDto>();
    }

    /// <summary>
    /// DTO para los detalles de una transacción
    /// </summary>
    public class TransaccionDetailDto : IMapFrom<Transaccion>
    {
        /// <summary>
        /// Identificador único de la transacción
        /// </summary>
        public int Id { get; set; }
        
        /// <summary>
        /// Tipo de transacción (Depósito o Retiro)
        /// </summary>
        public string TipoTransaccion { get; set; }
        
        /// <summary>
        /// Monto de la transacción
        /// </summary>
        public decimal Monto { get; set; }
        
        /// <summary>
        /// Saldo de la cuenta después de esta transacción
        /// </summary>
        public decimal SaldoPosterior { get; set; }
        
        /// <summary>
        /// Descripción de la transacción
        /// </summary>
        public string Descripcion { get; set; }
        
        /// <summary>
        /// Fecha y hora en que se realizó la transacción
        /// </summary>
        public DateTime FechaTransaccion { get; set; }
    }
}